using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;


namespace TaskManage
{
    /// <summary>
    /// Daily
    /// </summary>
    [Serializable]
    public class DayFrequency
    {
        public int _RunOnceTime;
        private int _Option;
        private int _RunInterval;
        private TimeUnit _TimeUnit;
        public int _StartTime;
        public int _EndTime;

        /// <summary>
        /// Option
        /// 1 one time only
        /// 2 repeating
        /// </summary>
        public int Option
        {
            get
            {
                return _Option;
            }
            set
            {
                _Option = value;
            }
        }

        /// <summary>
        /// Time for one time only
        /// </summary>
        [XmlIgnoreAttribute]
        public TimeSpan RunOnceTime
        {
            get
            {
                int totalSeconds = _RunOnceTime;
                int hours = totalSeconds / 3600;
                int m = totalSeconds % 3600;
                int minutes = m / 60;
                int seconds = m % 60;
                TimeSpan ts = new TimeSpan(hours, minutes, seconds);
                return ts;
            }
            set
            {

                _RunOnceTime = (int)value.TotalSeconds;
            }
        }

        /// <summary>
        /// Interval
        /// in hours or minutes
        /// </summary>
        public int RunInterval
        {
            get
            {
                return _RunInterval;
            }
            set
            {
                _RunInterval = value;
            }
        }

        /// <summary>
        /// Interval
        /// </summary>
        public TimeUnit TimeUnit
        {
            get
            {
                return _TimeUnit;
            }
            set
            {
                _TimeUnit = value;
            }
        }

        /// <summary>
        /// Start time
        /// </summary>
        [XmlIgnoreAttribute]
        public TimeSpan StartTime
        {
            get
            {
                int totalSeconds = _StartTime;
                int hours = totalSeconds / 3600;
                int m = totalSeconds % 3600;
                int minutes = m / 60;
                int seconds = m % 60;
                TimeSpan ts = new TimeSpan(hours, minutes, seconds);
                return ts;
            }
            set
            {
                _StartTime = (int)value.TotalSeconds;
            }
        }

        /// <summary>
        /// End time
        /// </summary>
        [XmlIgnoreAttribute]
        public TimeSpan EndTime
        {
            get
            {
                int totalSeconds = _EndTime;
                int hours = totalSeconds / 3600;
                int m = totalSeconds % 3600;
                int minutes = m / 60;
                int seconds = m % 60;
                TimeSpan ts = new TimeSpan(hours, minutes, seconds);
                return ts;
            }
            set
            {
                _EndTime = (int)value.TotalSeconds;
            }
        }

        public DayFrequency()
        {
            _TimeUnit = TimeUnit.Hour;
        }

    }


}
