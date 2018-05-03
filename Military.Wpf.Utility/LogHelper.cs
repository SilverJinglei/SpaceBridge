using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;

[assembly: log4net.Config.XmlConfigurator(Watch = true, ConfigFile = "log4net.config")]

namespace Military.Wpf.Utility
{
    public class LogInfo
    {
        public string ExecuteName;
        public string File;
        public int LineNumber;
        public string Member;

        public void Init(MethodInfo method, string file = "Logger", int lineNumber = 0,
            string member = "Info")
        {
            Debug.Assert(method.DeclaringType != null, "execute.Method.DeclaringType != null");

            ExecuteName = $"{method.DeclaringType.FullName}.{method.Name}";
            File = file;
            LineNumber = lineNumber;
            Member = member;
        }

        public override string ToString()
            => $@"Method={ExecuteName}, File={File}, LineNumber={LineNumber}, Member={Member}";
    }

    public enum LogLevel
    {
        Error,
        Info,
        Warn,
        Fatal
    }

    public class LogHelper
    {
        public static ILog GetTheLogger([CallerFilePath] string filename = "")
            => LogManager.GetLogger(filename);

        //public LogHelper()
        //{
        //    var rollingFileAppender = LogManager.GetRepository().GetAppenders().FirstOrDefault(a => a is RollingFileAppender) as RollingFileAppender;

        //    //rollingFileAppender.RollingStyle
        //}

        private static readonly Dictionary<LogLevel, Tuple<bool, MethodInfo>> LogLevelInfo;

        static LogHelper()
        {
            LogLevelInfo = new Dictionary<LogLevel, Tuple<bool, MethodInfo>>();

            var logger = LogManager.GetLogger(nameof(LogHelper));
            Type typeOfILog = typeof(ILog);

            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                var levelName = Enum.GetName(typeof(LogLevel), level);
                var isLevelEnabled = (bool)typeOfILog.GetProperty($@"Is{levelName}Enabled").GetValue(logger, null);
                var logMethodInfo = typeOfILog.GetMethod(levelName, new[] { typeof(object), typeof(Exception) });

                LogLevelInfo.Add(level, new Tuple<bool, MethodInfo>(isLevelEnabled, logMethodInfo));
            }
        }

        public static void Log(LogLevel level, object message, string filename = nameof(LogHelper), Exception exception = null)
        {
            var logger = LogManager.GetLogger(filename);

            var isLevelEnabled = LogLevelInfo[level].Item1;
            if (isLevelEnabled)
            {
                var logMethodInfo = LogLevelInfo[level].Item2;
                logMethodInfo.Invoke(logger, new object[] {message.ToString(), exception});
            }
        }

        public static void LogIntelligent(LogLevel level, object message, Exception exception = null, [CallerFilePath] string filename = "")
            => Log(level, message, filename, exception);
    }

    public class LogTraceListener : TraceListener
    {
        public static void Register() => Trace.Listeners.Add(new LogTraceListener());

        public override void Write(string message)
            => LogHelper.Log(LogLevel.Info, $@"[{DateTime.Now}] {message}", nameof(LogTraceListener));

        public override void WriteLine(string message)
            => LogHelper.Log(LogLevel.Info, $@"[{DateTime.Now}] {message}", nameof(LogTraceListener));
    }
}