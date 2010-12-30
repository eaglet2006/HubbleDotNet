using System;
using System.Collections.Generic;
using System.Text;
using TaskManage;

namespace Hubble.Core.Service
{
    class Task : IComparable<Task>
    {
        internal int SchemaId
        {
            get
            {
                return Schema.SchemaId;
            }
        }

        internal TaskManage.Schema Schema;

        internal Task(Schema schema)
        {
            this.Schema = schema;
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

                _TaskDict.Add(schema.SchemaId, new Task(schema));
            }
        }

        internal void Remove(int schemaId)
        {
            lock (_LockObj)
            {
                System.IO.File.Delete(_Path + schemaId.ToString() + ".xml");
                _TaskDict.Remove(schemaId);
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
