using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SpaceStation.Core;

namespace SpaceStation
{
    /// <summary>
    /// Proxy of Remote Service
    /// </summary>
    public abstract class IntelligentCommunication
    {
        public virtual Task Establish(string homelandAddress, int port, string target)
        {
            ClassName = target;

            Station.Terminal.ServerIp = homelandAddress;
            Station.Terminal.Port = port;
            return Station.Terminal.LaunchAsync();
        }

        public void Shutdown() => Station.Close();

        /// <summary>
        /// Should assign nameof(yourClass)
        /// </summary>
        public string ClassName { get; protected set; }

        /// <summary>
        /// copy:
        /// 
        /// protected override MethodInfo GetCurrentMethodInfo([CallerMemberName]string name = "")
        /// {
        ///     Debug.Assert(!string.IsNullOrEmpty(name));
        ///     return GetType().GetMethod(name);
        /// }
        /// </summary>
        protected abstract MethodInfo GetCurrentMethodInfo(string name);

        public Station Station { get; protected set; }

        public ITerminal Terminal => Station.Terminal;

        #region obsolete
        /*
        public EventHandler<Tuple<string, object>> InvokeCompleted;

        /// <summary>
        /// it will blcok UI, obsolete
        /// </summary>
        /// <example>
        /// public void CallSample1(float x, float y, float z)
        /// {
        ///     Invoke(nameof(CallSample1), true, x, y, z);
        /// }
        /// </example>
        private void Invoke(string method, bool isVoid, IReadOnlyDictionary<string, object> parameters)
        {
            var invokeTask = InvokeAsync(method, isVoid, parameters);
            
            invokeTask.ContinueWith(t1 =>
            {
                if (t1.Status == TaskStatus.RanToCompletion)
                {
                    InvokeCompleted?.Invoke(this, new Tuple<string, object>(method, t1.Result));
                }
                else if (t1.IsCanceled)
                    Debug.WriteLine("Task cancelled");
                else if (t1.IsFaulted)
                    Debug.WriteLine("Error: " + t1.Exception?.Message);
            });

            invokeTask.Start();
        }

        /// <summary>
        /// it will block UI, obsolete
        /// </summary>
        /// <example>
        /// public void CallSample3(float x, float y, float z)
        /// {
        ///     IntelligentInvoke(x, y, z);
        /// }
        /// </example>
        private void IntelligentInvoke(params object[] parameters)
        {
            var methodInfo = GetMethodInfo();
            var paramsDictionary = GetTargetMethodInfo(methodInfo, parameters);

            Invoke(methodInfo.Name, methodInfo.ReturnType == typeof(void), paramsDictionary);
        }
        */
        #endregion

        #region Utility

        private static Dictionary<string, object> GetTargetMethodInfo(MethodInfo methodInfo, IList<object> parameters)
        {
            var paramsInfos = methodInfo.GetParameters();

            Debug.Assert(parameters.Count == paramsInfos.Length);
            Debug.Assert(paramsInfos.All(p => !p.IsOut));

            var paramsDictionary = new Dictionary<string, object>();

            for (int i = 0; i < parameters.Count; i++)
            {
                var pInfo = paramsInfos[i];
                paramsDictionary.Add(pInfo.Name, parameters[i]);
            }

            return paramsDictionary;
        }

        private bool IsVoidMethod(MethodInfo methodInfo)
            => methodInfo.ReturnType == typeof (void) || methodInfo.ReturnType == typeof (Task);

        private const string AsyncSuffix = "Async";

        protected string RemoveAsync(string method) => method.Remove(method.Length - AsyncSuffix.Length);

        protected string GetContractMethodName(string method)
            => method.EndsWith(AsyncSuffix) ? RemoveAsync(method) : method;


        [MethodImpl(MethodImplOptions.NoInlining)]
        static MethodInfo GetMethodInfo()
        {
            const int levelOfStack = 2;
            var st = new StackTrace(new StackFrame(levelOfStack));
            return st.GetFrame(0).GetMethod() as MethodInfo;
        }

        #endregion

        protected void RegisterService<TService>(TService service) =>
            Station.RegisterService(service);

        protected Task<object> InvokeAsync(string method, bool isVoid, IDictionary<string, object> parameters)
        {
            Debug.Assert(!string.IsNullOrEmpty(ClassName));
            Debug.Assert(!string.IsNullOrEmpty(method));

            return Station.InvokeRemoteAsync(ClassName, method, isVoid, parameters);
        }

        /// <summary>
        /// Use to aysnc (await) Method
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <example>
        /// </example>
        protected Task<object> IntelligentInvokeAsync(MethodInfo methodInfo,  params object[] parameters)
        {
            //var methodInfo2 = GetMethodInfo();
            var paramsDictionary = GetTargetMethodInfo(methodInfo, parameters);
            return InvokeAsync(
                method: GetContractMethodName(methodInfo.Name), 
                isVoid: IsVoidMethod(methodInfo), 
                parameters: paramsDictionary);
        }

        protected async Task<T> InvokeAsync<T>(MethodInfo methodInfo, params object[] parameters)
        {
            var paramsDictionary = GetTargetMethodInfo(methodInfo, parameters);
            var result = await InvokeAsync(
                method: GetContractMethodName(methodInfo.Name),
                isVoid: IsVoidMethod(methodInfo),
                parameters: paramsDictionary);
            
            return Station.EnhanceChangeType<T>(result);
        }

        /// <summary>
        /// use to non-async (no await) method,
        /// place callback to last of parameters if needed
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected void Invoke(params object[] parameters)
        {
            if(!Terminal.IsConnected)
                return;

            var methodInfo = GetMethodInfo();

            var continuationAction = parameters.LastOrDefault() as Action<Task<object>>;

            var methodParameters = (continuationAction == null)
                ? parameters
                : parameters.Take(parameters.Length - 1);
                
            var paramsDictionary = GetTargetMethodInfo(methodInfo, methodParameters.ToList());

            var invoker = InvokeAsync(
                method: GetContractMethodName(methodInfo.Name),
                isVoid: IsVoidMethod(methodInfo),
                parameters: paramsDictionary);

            if (continuationAction != null)
                invoker.ContinueWith(continuationAction);
        }
    }
}