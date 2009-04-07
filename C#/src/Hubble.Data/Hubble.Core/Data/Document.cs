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

        #endregion

        #region Constructor

        public FieldValue(string name, string value)
        {
            Debug.Assert(name != null);

            _FieldName = name;
            _Value = value;
        }

        #endregion
    }

    [Serializable]
    public class Document
    {
        List<FieldValue> _FieldValues = new List<FieldValue>();

        public List<FieldValue> FieldValues
        {
            get
            {
                return _FieldValues;
            }
        }


        public void Add(string fieldName, string value)
        {
            _FieldValues.Add(new FieldValue(fieldName, value));
        }


    }
}
