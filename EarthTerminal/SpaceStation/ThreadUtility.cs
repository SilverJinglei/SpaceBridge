using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceStation
{
    internal static class TplExtensions
    {
        public static void Forget(this Task task) { }
    }

    public class ObjectPool<T>
    {
        public ConcurrentBag<T> Objects { get; }
        private readonly Func<T> _objectGenerator;

        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null) throw new ArgumentNullException(nameof(objectGenerator));
            Objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
        }

        public T GetObject()
        {
            T item;
            return Objects.TryTake(out item) ? item : _objectGenerator();
        }

        public void PutObject(T item)
        {
            Objects.Add(item);
        }
    }

    public class ThreadNotify<T>
    {
        public T Tag { get; set; }

        private readonly ManualResetEventSlim _event;

        private static readonly ObjectPool<ManualResetEventSlim> _eventPool;

        static ThreadNotify()
        {
            _eventPool = new ObjectPool<ManualResetEventSlim>(() => new ManualResetEventSlim());

            // prepare stock
            _eventPool.PutObject(new ManualResetEventSlim());
            _eventPool.PutObject(new ManualResetEventSlim());
        }

        public ThreadNotify(T t)
        {
            Tag = t;
            _event = _eventPool.GetObject();
        }

        static readonly int _timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

        public void WaitNotification()
        {
#if NETFX3_5
            AsyncTools.WhereAmI("Wait");
#endif

            _event.Wait(_timeout);

#if NETFX3_5
            AsyncTools.WhereAmI("Get Notify");
#endif

            _event.Reset();
            _eventPool.PutObject(_event);
        }

        public void Notify() => _event.Set();
    }
}