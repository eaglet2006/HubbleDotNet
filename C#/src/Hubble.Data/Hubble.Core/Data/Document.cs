using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Data
{
    [Serializable]
    public class FieldValue
    {
        #region Private

        private string _FieldName;
        private string _Value;
        private DataType _Type;

        #endregion

        #region Public properties

        public string FieldName
        {
            get
            {
                return _FieldName;
            }
        }

        public string Value
        {
            get
            {
                return _Value;
            }
        }

        public DataType Type
        {
            get
            {
                return _Type;
            }
        }

        #endregion

        #region Constructor

        public FieldValue(string name, string value, DataType type)
        {
            Debug.Assert(name != null);

            _FieldName = name;
            _Value = value;
            _Type = type;
        }

        #endregion
    }

    [Serializable]
    public class Document
    {
        List<FieldValue> _FieldValues = new List<FieldValue>();

        private long _DocId;

        public long DocId
        {
            get
            {
                return _DocId;
            }

            set
            {
                _DocId = value;
            }
        }

        public List<FieldValue> FieldValues
        {
            get
            {
                return _FieldValues;
            }
        }


        public void Add(string fieldName, string value, DataType type)
        {
            _FieldValues.Add(new FieldValue(fieldName, value, type));
        }


    }
}
