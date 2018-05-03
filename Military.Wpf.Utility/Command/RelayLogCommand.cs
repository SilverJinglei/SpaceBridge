using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace Military.Wpf.Utility.Command
{
    public class RelayLogCommand : RelayCommand
    {
        private readonly LogInfo _info = new LogInfo();

        public RelayLogCommand(Action execute, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string member = "") : base(execute)
        {
            _info.Init(execute.Method, file, lineNumber, member);
        }

        public RelayLogCommand(Action execute, Func<bool> canExecute, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string member = "") : base(execute, canExecute)
        {
            _info.Init(execute.Method, file, lineNumber, member);
        }

        public override void Execute(object parameter)
        {
            LogHelper.Log(LogLevel.Info, _info);
            base.Execute(parameter);
        }
    }

    public class RelayLogCommand<T> : RelayCommand<T>
    {
        private readonly LogInfo _info = new LogInfo();

        public RelayLogCommand(Action<T> execute, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string member = "") : base(execute)
        {
            _info.Init(execute.Method, file, lineNumber, member);
        }

        public RelayLogCommand(Action<T> execute, Func<T, bool> canExecute, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string member = "") : base(execute, canExecute)
        {
            _info.Init(execute.Method, file, lineNumber, member);
        }

        public override void Execute(object parameter)
        {
            LogHelper.Log(LogLevel.Info, _info);
            base.Execute(parameter);
        }
    }
}