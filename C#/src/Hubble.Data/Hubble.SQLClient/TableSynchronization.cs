using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.SQLClient
{
    /// <summary>
    /// This class used to synchronize table from database
    /// </summary>
    public class TableSynchronization
    {
        /// <summary>
        /// Optimize option
        /// </summary>
        public enum OptimizeOption
        {
            Minimum = 1, //Only one .idx file of each field
            Middle = 2, //two .idx files of each field
        }

        HubbleConnection _Conn;
        string _TableName;
        int _Step;
        OptimizeOption _Option;

        /// <summary>
        /// Constractor
        /// </summary>
        /// <param name="conn">hubble connection</param>
        /// <param name="tableName">table name</param>
        /// <param name="step">number of record synchronized every time</param>
        /// <param name="option">optimize option</param>
        public TableSynchronization(HubbleConnection conn, string tableName, int step, 
            OptimizeOption option)
        {
            _Conn = conn;
            _TableName = tableName;
            _Step = step;
            _Option = option;
        }

        /// <summary>
        /// Do synchronize
        /// </summary>
        public void Synchronize()
        {
            HubbleCommand cmd = new HubbleCommand("exec SP_SynchronizeTable {0}, {1}, {2}", _Conn,
                _TableName, _Step, (int)_Option);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Stop synchronize
        /// </summary>
        public void Stop()
        {
            HubbleCommand cmd = new HubbleCommand("exec SP_StopSynchronizeTable {0}", _Conn, _TableName);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get synchronize progress
        /// </summary>
        /// <returns>progress</returns>
        /// <remarks>if synchronize finished, return 100</remarks>
        public double GetProgress()
        {
            HubbleCommand cmd = new HubbleCommand("exec SP_GetTableSyncProgress {0}", _Conn, _TableName);
            System.Data.DataSet ds = cmd.Query();
            double progress = double.Parse(ds.Tables[0].Rows[0][0].ToString());

            if (progress >= 100 || progress < 0)
            {
                return 100;
            }
            else
            {
                return progress;
            }
        }
    }
}
