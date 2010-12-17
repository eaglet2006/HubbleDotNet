using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManage
{

    /// <summary>
    /// 计划内容
    /// </summary>
    [Serializable]
    public class SchemaInfo
    {
        private DateTime _RunOnceTime;
        private Frequency _Frequency;
        private DayFrequency _DayFrequency;
        private AvailableTime _AvailableTime;

        /// <summary>
        /// 执行一次时间
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
        /// 频率
        /// 日期间隔
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
        /// 每日频率
        /// 时间间隔
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
        /// 持续时间
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
