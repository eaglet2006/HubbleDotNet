

namespace TaskManage
{
    public enum TaskState
    {
        Disable = 0,
        Enable = 1
    }

    public enum DaysOfWeek
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Day = 9
    }
    
    /// <summary>
    /// Schema type
    /// </summary>
    public enum SchemaType
    {
        /// <summary>
        /// One time only
        /// </summary>
        RunOnce = 1,

        /// <summary>
        /// Repeating
        /// </summary>
        RunRepeat = 9
    }

    public enum SchemaState
    {
        Disable = 0,
        Enable  = 1
    }

    /// <summary>
    /// Frequency type
    /// </summary>
    public enum FrequencyType
    {
        Day = 1,
        Week = 7,
        Month = 30
    }

    /// <summary>
    /// Time unit
    /// </summary>
    public enum TimeUnit
    {
        Second = 1,
        Minute = 60,
        Hour = 3600
    }
}