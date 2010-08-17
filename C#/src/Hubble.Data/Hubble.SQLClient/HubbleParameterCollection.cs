using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Hubble.SQLClient
{
    public class HubbleParameterCollection : System.Data.Common.DbParameterCollection
    {
        private List<SqlParameter> _Parameters = null;

        public HubbleParameterCollection()
        {
            _Parameters = new List<System.Data.SqlClient.SqlParameter>();
        }

        public SqlParameter Add(SqlParameter value)
        {
            if (IndexOf(value) >= 0)
            {
                throw new System.Data.DataException(string.Format("Reduplicate parameter Name: {0}", value.ParameterName));
            }

            Add((object)value);
            return value;
        }

        public SqlParameter Add(string parameterName, object value)
        {
            return Add(new SqlParameter(parameterName, value));
        }
        public SqlParameter AddWithValue(string parameterName, object value)
        {
            return Add(new SqlParameter(parameterName, value));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType)
        {
            return Add(new SqlParameter(parameterName, sqlDbType));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size)
        {
            return Add(new SqlParameter(parameterName, sqlDbType, size));
        }

        public SqlParameter Add(string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
        {
            return Add(new SqlParameter(parameterName, sqlDbType, size, sourceColumn));
        }

        public override int Add(object value)
        {
            _Parameters.Add(value as SqlParameter);
            return _Parameters.Count - 1;
        }

        public override void AddRange(Array values)
        {
            _Parameters.AddRange(values as SqlParameter[]);
        }

        public override void Clear()
        {
            _Parameters.Clear();
        }

        public override bool Contains(string value)
        {
            return IndexOf(value) >= 0;
        }

        public override bool Contains(object value)
        {
            return IndexOf(value) >= 0;
        }

        public override void CopyTo(Array array, int index)
        {
            Array.Copy(_Parameters.ToArray(), 0, array, index, _Parameters.Count);
        }

        public override int Count
        {
            get
            {
                return _Parameters.Count;
            }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            foreach (SqlParameter para in _Parameters)
            {
                yield return para;
            }
        }

        protected override System.Data.Common.DbParameter GetParameter(string parameterName)
        {
            foreach (SqlParameter para in _Parameters)
            {
                if (para.ParameterName == parameterName)
                {
                    return para;
                }
            }

            return null;
        }

        protected override System.Data.Common.DbParameter GetParameter(int index)
        {
            return _Parameters[index];
        }

        public override int IndexOf(string parameterName)
        {
            for(int i = 0 ; i < _Parameters.Count ; i++)
            {
                if (_Parameters[i].ParameterName == parameterName)
                {
                    return i;
                }
            }

            return -1;
        }

        public override int IndexOf(object value)
        {
            for (int i = 0; i < _Parameters.Count; i++)
            {
                if (_Parameters[i].ParameterName == ((SqlParameter)value).ParameterName)
                {
                    return i;
                }
            }

            return -1;
        }

        public override void Insert(int index, object value)
        {
            _Parameters.Insert(index, value as SqlParameter);
        }

        public override bool IsFixedSize
        {
            get 
            { 
                return false; 
            }
        }

        public override bool IsReadOnly
        {
            get 
            { 
                return false; 
            }
        }

        public override bool IsSynchronized
        {
            get 
            { 
                return true; 
            }
        }

        public override void Remove(object value)
        {
            int index = IndexOf(value);

            if (index >= 0)
            {
                _Parameters.RemoveAt(index);
            }
        }

        public override void RemoveAt(string parameterName)
        {
            int index = IndexOf(parameterName);

            if (index >= 0)
            {
                _Parameters.RemoveAt(index);
            }
        }

        public override void RemoveAt(int index)
        {
            _Parameters.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, System.Data.Common.DbParameter value)
        {
            int index = IndexOf(parameterName);

            if (index >= 0)
            {
                _Parameters[index] = value as SqlParameter;
            }
            else
            {
                throw new System.ArgumentException();
            }
        }

        protected override void SetParameter(int index, System.Data.Common.DbParameter value)
        {
            _Parameters[index] = value as SqlParameter;
        }

        private object _SyncRoot = new object();

        public override object SyncRoot
        {
            get 
            {
                return _SyncRoot; 
            }
        }
    }
}
