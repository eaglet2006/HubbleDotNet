using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{

    /// <summary>
    ///Schema info
    /// </summary>
    [Serializable]
    public class SchemaInfo
    {
        private DateTime _RunOnceTime;
        private Frequency _Frequency;
        private DayFrequency _DayFrequency;
        private AvailableTime _AvailableTime;

        /// <summary>
        /// one time
        /// </summary>
        public DateTime RunOnceTime
        {
            get
            {
                return _RunOnceTime;
            }
            set
            {
                _RunOnceTime = value;
            }
        }

        /// <summary>
        /// frequency
        /// </summary>
        public Frequency Frequency
        {
            get
            {
                return _Frequency;
            }
            set
            {
                _Frequency = value;
            }
        }

        /// <summary>
        /// Daily frequency
        /// </summary>
        public DayFrequency DayFrequency
        {
            get
            {
                return _DayFrequency;
            }
            set
            {
                _DayFrequency = value;
            }
        }

        /// <summary>
        /// duration
        /// </summary>
        public AvailableTime RunTime
        {
            get
            {
                return _AvailableTime;
            }
            set
            {
                _AvailableTime = value;
            }
        }

        public SchemaInfo()
        {
        }
    }

}
