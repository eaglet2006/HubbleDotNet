using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{
    /// <summary>
    /// 频率－每月
    /// </summary>
    [Serializable]
    public class EveryMonth
    {
        private int _RunInterval;
        private int _Option;
        private int _WhichDay;
        private int _WhichWeek;
        private DaysOfWeek _DayOfWeek;

        /// <summary>
        /// 执行间隔
        /// 单位：月
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
        /// 选项
        /// 1代表每几个月第几天选项
        /// 2代表在第几个星期几－每几个月选项
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
        /// 第几天（每几月）
        /// </summary>
        public int WhichDay
        {
            get
            {
                return _WhichDay;
            }
            set
            {
                _WhichDay = value;
            }
        }

        /// <summary>
        /// which week
        /// </summary>
        public int WhichWeek
        {
            get
            {
                return _WhichWeek;
            }
            set
            {
                _WhichWeek = value;
            }
        }

        /// <summary>
        /// day of week
        /// </summary>
        public DaysOfWeek DayOfWeek
        {
            get
            {
                return _DayOfWeek;
            }
            set
            {
                _DayOfWeek = value;
            }
        }

        public EveryMonth()
        {
            _DayOfWeek = DaysOfWeek.Sunday;
        }
    }
  
}
