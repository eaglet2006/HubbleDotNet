using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{
    /// <summary>
    /// 持续时间
    /// </summary>
    [Serializable]
    public class AvailableTime
    {
        private DateTime _StartDate;
        private Boolean _IsInfinity;
        private DateTime _EndDate;

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                _StartDate = value;
            }
        }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return _EndDate;
            }
            set
            {
                _EndDate = value;
            }
        }

        /// <summary>
        /// Infinity
        /// </summary>
        public Boolean IsInfinity
        {
            get
            {
                return _IsInfinity;
            }
            set
            {
                _IsInfinity = value;
            }
        }

        public AvailableTime()
        {
        }
    }

}
