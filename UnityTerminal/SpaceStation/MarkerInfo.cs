using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ReacTiVisionHal.Model
{
    public class MarkerInfo : INotifyPropertyChanged
    {
        #region Id
        private int _id;

        /// <summary>
        /// From Hardware
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                Set(ref _id, value);
            }
        }
        #endregion

        #region X
        private double _x;

        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                Set(ref _x, value);
            }
        }
        #endregion

        #region Y
        private double _y;

        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                Set(ref _y, value);
            }
        }
        #endregion

        #region Angle
        private double _angle;

        /// <summary>
        /// Sets and gets the Angle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                Set(ref _angle, value);
            }
        }
        #endregion

        public long Timestamp { get; set; }

        public static event PropertyChangedEventHandler GlobalPropertyChanged;

        private static void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GlobalPropertyChanged?.Invoke(sender, e);
        }

        public MarkerInfo()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public void Copy(MarkerInfo other) => Update(other.Id, other.X, other.Y, other.Angle);

        public void Clear() => Update(Id, 0, 0, 0);

        public void Update(int id, double x, double y, double angle)
        {
            Id = id;
            X = x;
            Y = y;
            Angle = angle;
        }

        public bool IsSimilar(MarkerInfo other)
            => IsSimilar(other.X, other.Y, other.Angle);

        public bool IsSimilar(double x, double y, double angle)
        {
            if (Math.Abs(X - x) > 10)
                return false;

            if (Math.Abs(Y - y) > 10)
                return false;

            if (AngleDifference(Angle, angle) > 10)
                return false;

            return true;
        }

        private static double AngleDifference(double leftAngle, double rhtAngle)
        {
            var angleDifference = Math.Abs(leftAngle - rhtAngle);
            const double circleDegree = 360;
            if (angleDifference > circleDegree / 2)
                angleDifference = circleDegree - angleDifference;
            return angleDifference;
        }

        public override string ToString() => $@"id={Id}; x={X}; y={Y}; angle={Angle}";

        //public override bool Equals(object obj) 
        //    => Id == (obj as MarkerInfo)?.Id;

        //protected bool Equals(MarkerInfo other)
        //{
        //    return _id == other._id && _x.Equals(other._x) && _y.Equals(other._y) && _angle.Equals(other._angle);
        //}

        //public override int GetHashCode()
        //{
        //    return Id;
        //    unchecked
        //    {
        //        var hashCode = _id;
        //        hashCode = (hashCode*397) ^ _x.GetHashCode();
        //        hashCode = (hashCode*397) ^ _y.GetHashCode();
        //        hashCode = (hashCode*397) ^ _angle.GetHashCode();
        //        return hashCode;
        //    }
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            field = newValue;

            OnPropertyChanged(propertyName);
            return true;
        }
    }
}