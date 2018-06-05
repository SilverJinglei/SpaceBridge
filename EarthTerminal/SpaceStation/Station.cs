using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceStation.Core;

namespace SpaceStation
{
    public sealed class Station
    {
        public ITerminal Terminal { get; }

        readonly Dictionary<string, ThreadNotify<ResultMetadata>> _results;

        public Station(ITerminal terminal)
        {
            Terminal = terminal;
            _results = new Dictionary<string, ThreadNotify<ResultMetadata>>();

            Terminal.Recieved +=
#if NETFX3_5
                (s, e) => OnRecieved(s, e.Item);
#else
                OnRecieved;
#endif
        }

        public void Close() => Terminal.Close();

        private void OnRecieved(object sender, Tuple<IntPtr, string> e)
        {
            var message = e.Item2;
            var metadata = Metadata.Parse(message);

            switch (metadata.Type)
            {
                case MetadataType.Result:
                    RecieveResult(message);
                    break;
                case MetadataType.Operation:
                    RecieveOperation(message, handle: e.Item1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RecieveResult(string message)
        {
            var result = ResultMetadata.Parse(message);

            _results[result.Token].Tag = result;
            _results[result.Token].Notify();
        }

        #region Invoke Local Service

        //private readonly object _syncLock = new object();
        private readonly Dictionary<string, Type> _servicesRegistry = new Dictionary<string, Type>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        // Refer to mvvmlight => how to register a serivce
        public void RegisterService<TService>(Func<TService> factory = null)
        {
            if (factory == null)
                factory = Activator.CreateInstance<TService>; // new TService

            RegisterService(factory());
        }

        /// <summary>
        /// Make Station as a Service that can be invoked by other connected peer
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="service"></param>
        public void RegisterService<TService>(TService service)
        {
            var t = typeof (TService);

            if (t.IsGenericType)
                throw new ArgumentException("Generic class not support");

            if (service == null)
            {
                if (t.IsInterface || t.IsAbstract)
                    throw new ArgumentException("An interface or abstract cannot be registered alone.");

                service = Activator.CreateInstance<TService>(); // new TService
            }

            _servicesRegistry.Add(t.Name, t);
            _instances.Add(t, service);
        }

        public static T EnhanceChangeType<T>(object obj)
        {
            if (obj == null)
                return default(T);

            if (obj is JToken)
                return JsonConvert.DeserializeObject<T>(obj.ToString());

            return (T) obj;
        }

        public static object EnhanceChangeType(object value, Type conversionType)
        {
            if (conversionType.IsEnum)
                return Enum.Parse(conversionType, value.ToString());

            if (conversionType == typeof (IList))
                return value as IList;

            if (value is JArray)
                return JsonConvert.DeserializeObject(value.ToString(), conversionType);

            return Convert.ChangeType(value, conversionType);
        }

        private static List<object> GetParamenters(IDictionary<string, object> rawParams, IList<ParameterInfo> paramInfos)
        {
            var ps = new List<object>();
            for (var i = 0; i < rawParams.Count; i++)
            {
                var p = rawParams.ElementAt(i);
                var info = paramInfos[i];

                Debug.Assert(p.Key == info.Name);

                ps.Add(EnhanceChangeType(p.Value, info.ParameterType));
            }
            return ps;
        }

        private object InvokeLocal(OperationMetadata operation)
        {
            var typeSvc = _servicesRegistry[operation.Class];

            var paramsMetadata = operation.Parameters;

            Func<MethodInfo, bool> isMethodSignatureCorrect = m => m.Name == operation.Name && m.GetParameters().Length == paramsMetadata.Count;
            var methods = typeSvc.GetMethods().Where(isMethodSignatureCorrect).ToList();

            if (methods.Count == 0)
                throw new Exception($"Not find {operation.Name} Method");

            if (methods.Count > 1)
                throw new Exception($"Duplicated {operation.Name} Methods");

            var targetMethod = methods.First();

            var parameters = GetParamenters(paramsMetadata, targetMethod.GetParameters()).ToArray();
            if (targetMethod.IsStatic)
                return targetMethod.Invoke(null, parameters);

            var obj = _instances[typeSvc];
            return targetMethod.Invoke(obj, parameters);
        }

        private void RecieveOperation(string message, IntPtr handle)
        {
            var method = OperationMetadata.Parse(message);

            try
            {
                if (method.IsOneWay)
                {
                    InvokeLocal(method);
                    return;
                }

                var result = new ResultMetadata
                {
                    Token = method.Token, Value = InvokeLocal(method)
                };

                Terminal.SendTo(result.ToString(), handle);
            }
            catch (Exception exception)
            {
                Action sendException = () => Terminal.SendTo(new ResultMetadata
                {
                    Token = method.Token, IsException = true, Value = exception.Message
                }.ToString(), handle);

                sendException();

                throw;
            }
        }

        #endregion

        #region Invoke Remote Service

        /// <summary>
        /// obsolete
        /// </summary>
        private object InvokeRemote(string @class, string method, bool isVoid, IDictionary<string, object> parameters)
        {
            var mc = new OperationMetadata
            {
                Type = MetadataType.Operation, Class = @class, Name = method, IsOneWay = isVoid, Parameters = parameters
            };

            var result = InvokeRemoteAsync(mc).GetAwaiter().GetResult();

            if (result.IsException)
                throw new Exception(result.Value.ToString());

            return result.Value;
        }

        public async Task<object> InvokeRemoteAsync(string @class, string method, bool isVoid, IDictionary<string, object> parameters)
        {
            Debug.Assert(!string.IsNullOrEmpty(@class));

            var mc = new OperationMetadata
            {
                Type = MetadataType.Operation, Class = @class, Name = method, IsOneWay = isVoid, Parameters = parameters
            };

            var result = await InvokeRemoteAsync(mc);

            if (result == null)
                return null; //Task.CompletedTask;

            if (result.IsException)
                throw new Exception(result.Value.ToString());

            return result.Value;
        }

        private async Task<ResultMetadata> InvokeRemoteAsync(OperationMetadata operation)
        {
            Debug.Assert(!operation.IsValid());

            // Prepare
            var token = Stopwatch.GetTimestamp().ToString(); // DateTime.Now.ToLongTimeString() + DateTime.Now.Millisecond;
            operation.Token = token;

            var jsonString = operation.ToString();

#if NETFX3_5
            Debug.Warning($"Start send {jsonString}, thread = {Thread.CurrentThread.ManagedThreadId}");
#endif

            await Terminal.Boradcast(jsonString).ConfigureAwait(false);

            if (operation.IsOneWay)
                return ResultMetadata.Empty;


            var resultEvent = new ThreadNotify<ResultMetadata>(null);
            _results.Add(token, resultEvent);

            resultEvent.WaitNotification();
            _results.Remove(token);

            return resultEvent.Tag;
        }

        #endregion
    }
}