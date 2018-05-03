using System;
using System.Timers;
using System.Windows.Threading;

namespace Military.Wpf.Utility
{
    public class DelayOperation
    {
        public Action Action { get; set; }

        readonly DispatcherTimer _counter;

        protected DelayOperation()
        { }

        private DelayOperation(double delayDuration)
        {
            _counter = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(delayDuration)
            };
        }

        public static DelayOperation CreateDelayer(Action operation, double delayDuration = 500.0)
        {
            return new DelayOperation(delayDuration)
            {
                Action = operation
            };
        }

        void OnTick(object sender, EventArgs e)
        {
            Action?.Invoke();

            Cancel(); // run once
        }

        public void Cancel()
        {
            if (_counter.IsEnabled)
            {
                _counter.Stop(); // run once
                _counter.Tick -= OnTick;
            }
        }

        public void Begin()
        {
            Cancel();

            _counter.Start();
            _counter.Tick += OnTick;
        }

        public void Cleanup()
        {
            Cancel();
            Action = null;
        }
    }

    public class DelayOperationOnThread
    {
        public Action Action { get; set; }

        readonly Timer _counter;

        protected DelayOperationOnThread()
        { }

        private DelayOperationOnThread(double delayDuration)
        {
            _counter = new Timer()
            {
                Interval = delayDuration
            };
        }

        public static DelayOperationOnThread CreateDelayer(Action operation, double delayDuration = 500.0)
        {
            return new DelayOperationOnThread(delayDuration)
            {
                Action = operation
            };
        }

        void OnTick(object sender, EventArgs e)
        {
            Action?.Invoke();

            Cancel(); // run once
        }

        public void Cancel()
        {
            if (_counter.Enabled)
            {
                _counter.Stop(); // run once
                _counter.Elapsed -= OnTick;
            }
        }

        public void Begin()
        {
            Cancel();

            _counter.Start();
            _counter.Elapsed += OnTick;
        }

        public void Cleanup()
        {
            Cancel();
            Action = null;
        }
    }
}
