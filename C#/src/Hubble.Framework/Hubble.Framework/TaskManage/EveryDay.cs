using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{
    /// <summary>
    /// Daily
    /// </summary>
    [Serializable]
    public class EveryDay
    {
        private int _RunInterval;

        /// <summary>
        /// Inteval
        /// In days
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

        public EveryDay()
        {
        }
    }

}
