using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{

    /// <summary>
    /// Frequency
    /// </summary>
    [Serializable]
    public class Frequency
    {
        private FrequencyType _FrequencyType;
        private EveryDay _EveryDay;
        private EveryWeek _EveryWeek;
        private EveryMonth _EveryMonth;

        /// <summary>
        /// Frequency type
        /// </summary>
        public FrequencyType FrequencyType
        {
            get
            {
                return _FrequencyType;
            }
            set
            {
                _FrequencyType = value;
            }
        }

        /// <summary>
        /// Daily frequency
        /// </summary>
        public EveryDay EveryDay
        {
            get
            {
                return _EveryDay;
            }
            set
            {
                _EveryDay = value;
            }
        }

        /// <summary>
        /// Weekly
        /// </summary>
        public EveryWeek EveryWeek
        {
            get
            {
                return _EveryWeek;
            }
            set
            {
                _EveryWeek = value;
            }
        }

        /// <summary>
        /// Monthly
        /// </summary>
        public EveryMonth EveryMonth
        {
            get
            {
                return _EveryMonth;
            }
            set
            {
                _EveryMonth = value;
            }
        }

        public Frequency()
        {
            _FrequencyType = FrequencyType.Day;
        }
    }

}
