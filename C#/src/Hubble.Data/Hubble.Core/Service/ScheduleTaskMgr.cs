using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using TaskManage;

namespace Hubble.Core.Service
{
    class TaskComparableByNextTime : IComparer<Task>
    {
        #region IComparer<Task> Members

        public int Compare(Task x, Task y)
        {
            return x.NextTime.CompareTo(y.NextTime);
        }

        #endregion
    }

    class Task : IComparable<Task>
    {
        internal int SchemaId
        {
            get
            {
                return Schema.SchemaId;
            }
        }

        DateTime _NextTime;

        internal DateTime NextTime
        {
            get
            {
                return _NextTime;
            }
        }

        internal void GetNextTime()
        {
            _NextTime = Schema.NextTime(DateTime.Now);
        }

        internal TaskManage.Schema Schema;

        Thread _Thread = null;

        internal Task(Schema schema)
        {
            this.Schema = schema;
        }


        void DoTask()
        {
            Global.Report.WriteAppLog(string.Format("Task scheduler:{0} id = {1} starting",
                        Schema.Name, SchemaId));

            try
            {
                System.Data.SqlClient.SqlConnectionStringBuilder sqlConnBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                sqlConnBuilder.DataSource = "127.0.0.1";
                sqlConnBuilder.UserID = Schema.UserName.Trim();
                sqlConnBuilder.Password = Hubble.Framework.Security.DesEncryption.Decrypt(new byte[] { 0x14, 0x0A, 0x0C, 0x0E, 0x0A, 0x11, 0x42, 0x58 }, Schema.Password); 
                sqlConnBuilder.InitialCatalog = Schema.Database.Trim();

                using (Hubble.SQLClient.HubbleConnection conn = new Hubble.SQLClient.HubbleConnection(sqlConnBuilder.ConnectionString))
                {
                    conn.Open();
                    Hubble.SQLClient.HubbleCommand cmd = new Hubble.SQLClient.HubbleCommand(Schema.Sql, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Global.Report.WriteErrorLog(string.Format("Run task scheduler:{0} id = {1} fail, task hasn't finished",
                    Schema.Name, SchemaId), e);
            }

            lock (this)
            {
                _Thread = null;
            }

            Global.Report.WriteAppLog(string.Format("Task scheduler:{0} id = {1} finished",
                        Schema.Name, SchemaId));
        }

        internal void Start()
        {
            lock (this)
            {
                if (_Thread != null)
                {
                    Global.Report.WriteErrorLog(string.Format("Start task scheduler:{0} id = {1} fail, task hasn't finished",
                        Schema.Name, SchemaId));
                }
                else
                {
                    _Thread = new Thread(DoTask);
                    _Thread.IsBackground = true;
                    _Thread.Start();
                }
            }
        }

        internal void Abort()
        {
            lock (this)
            {
                try
                {
                    _Thread.Abort();
                }
                catch (Exception e)
                {
                    Global.Report.WriteErrorLog(string.Format("Abort task scheduler:{0} id = {1} fail, task hasn't finished",
                        Schema.Name, SchemaId), e);
                }
            }
        }

        #region IComparable<Task> Members

        public int CompareTo(Task other)
        {
            return SchemaId.CompareTo(other.SchemaId);
        }

        #endregion
    }

    class ScheduleTaskMgr
    {
        static ScheduleTaskMgr _ScheduleTask = new ScheduleTaskMgr();

        internal static ScheduleTaskMgr ScheduleMgr
        {
            get
            {
                return _ScheduleTask;
            }
        }

        string _Path;
        Dictionary<int, Task> _TaskDict = new Dictionary<int, Task>();
        int _MaxSchemaId = -1;
        object _LockObj = new object();
        object _LockSchedule = new object();
        List<Task> _TaskScheduleQueue = new List<Task>();
        Thread _Thread = null;

        internal void Init(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            _Path = Hubble.Framework.IO.Path.AppendDivision(path, '\\');

            foreach (string file in System.IO.Directory.GetFiles(path, "*.xml"))
            {
                string schemaIdStr = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileName(file));

                int schemaId;
                if (int.TryParse(schemaIdStr, out schemaId))
                {
                    try
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                        {
                            Task task = new Task(
                                Hubble.Framework.Serialization.XmlSerialization<Schema>.Deserialize(fs));

                            if (_TaskDict.ContainsKey(task.SchemaId))
                            {
                                Global.Report.WriteErrorLog(string.Format("Load schema xml fail. Repeated shcemaId={0}", schemaId));
                            }
                            else
                            {
                                _TaskDict.Add(task.SchemaId, task);
                                if (task.SchemaId > _MaxSchemaId)
                                {
                                    _MaxSchemaId = task.SchemaId;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Global.Report.WriteErrorLog(string.Format("Load schema xml fail. shcemaId={0}", schemaId), e);
                    }

                }
            }

            RecalculateSchedule();
        }

        private void ScheduleTask()
        {
            while (true)
            {
                lock (_LockSchedule)
                {
                    try
                    {
                        if (_TaskScheduleQueue.Count <= 0)
                        {
                            break;
                        }

                        bool needRecalculate = false;

                        foreach (Task task in _TaskScheduleQueue)
                        {
                            if (DateTime.Now > _TaskScheduleQueue[0].NextTime)
                            {
                                _TaskScheduleQueue[0].Start();
                                _TaskScheduleQueue[0].GetNextTime();
                                needRecalculate = true;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (needRecalculate)
                        {
                            RecalculateSchedule();
                        }
                    }
                    catch(Exception e)
                    {
                        Global.Report.WriteErrorLog("ScheduleTask fail", e);
                    }
                }

                Thread.Sleep(1000);
            }

            Global.Report.WriteAppLog("Task schedule thread stoped");
        }

        private void RecalculateSchedule()
        {
            lock (_LockSchedule)
            {
                _TaskScheduleQueue = GetEnableTaskList();

                if (_TaskScheduleQueue.Count <= 0)
                {
                    _Thread = null;
                    
                    return;
                }

                _TaskScheduleQueue.Sort(new TaskComparableByNextTime());

                if (_Thread == null)
                {
                    _Thread = new Thread(ScheduleTask);
                    _Thread.IsBackground = true;
                    _Thread.Start();

                    Global.Report.WriteAppLog("Task schedule thread started");
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Task recalculate schedule");

                foreach (Task task in _TaskScheduleQueue)
                {
                    sb.AppendLine(string.Format(string.Format("Task scheduler: {0}, id={1}, next time:{2}",
                        task.Schema.Name, task.SchemaId, task.NextTime.ToString("yyyy-MM-dd HH:mm:ss"))));
                }

                Global.Report.WriteAppLog(sb.ToString());

            }
        }

        internal void Add(Schema schema)
        {
            lock (_LockObj)
            {
                _MaxSchemaId++;
                schema.SchemaId = _MaxSchemaId;

                using (System.IO.FileStream fs = new System.IO.FileStream(_Path + schema.SchemaId.ToString() + ".xml",
                     System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    Hubble.Framework.Serialization.XmlSerialization<Schema>.Serialize(schema, Encoding.UTF8, fs);
                }

                Task task = new Task(schema);
                _TaskDict.Add(schema.SchemaId, task);

                if (schema.State == SchemaState.Enable)
                {
                    task.GetNextTime();
                    Global.Report.WriteAppLog(string.Format("Add new task scheduler: {0}, id={1}, next time:{2}",
                        schema.Name, task.SchemaId, task.NextTime.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {
                    Global.Report.WriteAppLog(string.Format("Add new task scheduler: {0}, id={1}, disabled",
                        schema.Name, task.SchemaId));
                }

                RecalculateSchedule();
            }
        }

        internal void Remove(int schemaId)
        {
            lock (_LockObj)
            {
                Task task = GetTask(schemaId);

                Global.Report.WriteAppLog(string.Format("Remove task scheduler: {0}, Id = {1}",
                    task.Schema.Name, schemaId));

                System.IO.File.Delete(_Path + schemaId.ToString() + ".xml");

                lock (_LockSchedule)
                {
                    task.Abort();
                    _TaskDict.Remove(schemaId);
                    RecalculateSchedule();
                }
            }
        }

        internal Task GetTask(int schemaId)
        {
            lock (_LockObj)
            {
                Task task;
                if (_TaskDict.TryGetValue(schemaId, out task))
                {
                    return task;
                }
                else
                {
                    throw new Data.DataException(string.Format("Schema Id = {0} does not exist",
                        schemaId));
                }
            }
        }

        internal List<Task> GetEnableTaskList()
        {
            lock (_LockObj)
            {
                List<Task> taskList = new List<Task>();

                foreach (Task task in _TaskDict.Values)
                {
                    if (task.Schema.State == SchemaState.Enable)
                    {
                        taskList.Add(task);
                    }
                }

                return taskList;
            }
        }

        internal List<Task> GetTaskList()
        {
            lock (_LockObj)
            {
                List<Task> taskList = new List<Task>();

                foreach (Task task in _TaskDict.Values)
                {
                    taskList.Add(task);
                }

                return taskList;
            }
        }
    }
}
