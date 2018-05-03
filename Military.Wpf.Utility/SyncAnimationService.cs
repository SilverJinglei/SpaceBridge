using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace Military.Wpf.Utility
{
    public class SyncAnimationService
    {
        private SyncAnimationService()
        {

        }

        public static SyncAnimationService GetInstance()
        {
            return new SyncAnimationService();
        }

        readonly List<Storyboard> _animationList = new List<Storyboard>();

        public void Register(Storyboard animation)
        {
            if (HasStarted)
            {

                // to sync
                animation.BeginTime = -_stopwatch.Elapsed;
                //Debug.WriteLine("{0} - {1}", animation.BeginTime, "HasStarted");

                //foreach (var item in _animationList)
                //    item.BeginTime = -_stopwatch.Elapsed;
            }
            else
            {
                _stopwatch.Start();
                //Debug.WriteLine("{0} - {1}", _stopwatch.Elapsed, "First Started");
            }

            _animationList.Add(animation);
        }

        public void Unregister(Storyboard animation)
        {
            //Debug.WriteLine("{0} - {1}", _stopwatch.Elapsed, "Unregister");

            animation.BeginTime = TimeSpan.Zero;
            _animationList.Remove(animation);

            if (_animationList.Count == 0)
                _stopwatch.Reset();
        }

        private bool HasStarted => _animationList.Count > 0;

        private readonly Stopwatch _stopwatch = new Stopwatch();
    }
}