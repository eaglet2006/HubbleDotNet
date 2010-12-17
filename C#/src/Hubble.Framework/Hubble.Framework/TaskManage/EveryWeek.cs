using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{

    /// <summary>
    /// 频率－每周
    /// </summary>
    [Serializable]
    public class EveryWeek
    {
        private int _RunInterval;
        private DayOfWeek[] _DaysOfWeek;

        /// <summary>
        /// 执行间隔
        /// 单位：周
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
        /// 星期几
        /// </summary>
        public DayOfWeek[] DaysOfWeek
        {
            get
            {
                return _DaysOfWeek;
            }
            set
            {
                _DaysOfWeek = value;
            }
        }

        public EveryWeek()
        {
        }
    }

}
