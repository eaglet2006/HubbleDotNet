using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Data
{
    [Serializable]
    public class Field
    {
        public enum Index
        {
            None = 0,
            Tokenized = 1,
            Untokenized = 2,
        }

        #region Private field

        string _Name;
        DataType _DataType;
        int _DataLength = 0;
        bool _Store = true;
        Index _IndexType = Index.None;
        int _TabIndex = 0;
        string _AnalyzerName = null;
        Analysis.IAnalyzer _Analyzer;

        #endregion

        #region Public properties

        /// <summary>
        /// Field name
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        /// <summary>
        /// Field data type
        /// </summary>
        public DataType DataType
        {
            get
            {
                return _DataType;
            }

            set
            {
                _DataType = value;
            }
        }

        /// <summary>
        /// Length of this field
        /// Valid when DataType is String and Data only. 
        /// </summary>
        public int DataLength
        {
            get
            {
                return _DataLength;
            }

            set
            {
                _DataLength = value;
            }
        }

        /// <summary>
        /// Need store
        /// </summary>
        public bool Store
        {
            get
            {
                return _Store;
            }

            set
            {
                _Store = value;
            }
        }

        /// <summary>
        /// Index type
        /// </summary>
        public Index IndexType
        {
            get
            {
                return _IndexType;
            }

            set
            {
                _IndexType = value;
            }
        }

        /// <summary>
        /// Full name of analyzer class
        /// </summary>
        public string AnalyzerName
        {
            get
            {
                return _AnalyzerName;
            }

            set
            {
                _AnalyzerName = value;
            }
        }

        /// <summary>
        /// The index in Payload index
        /// </summary>
        public int TabIndex
        {
            get
            {
                return _TabIndex;
            }

            set
            {
                _TabIndex = value;
            }
        }

        #endregion

        public Analysis.IAnalyzer GetAnalyzer()
        {
            if (string.IsNullOrEmpty(AnalyzerName))
            {
                _Analyzer = new Analysis.SimpleAnalyzer();
            }
            else
            {
                _Analyzer = (Analysis.IAnalyzer)Hubble.Framework.Reflection.Instance.CreateInstance(AnalyzerName);
            }

            return _Analyzer;
        }

        #region Constructor

        public Field()
        {
        }

        public Field(string name, DataType dataType) :
            this(name, dataType, 0, true, Index.None)
        {
        }


        public Field(string name, DataType dataType, bool store) :
            this(name, dataType, 0, store, Index.None)
        {
        }

        public Field(string name, DataType dataType, Index indexType) :
            this(name, dataType, 0, true, indexType)
        {
        }

        public Field(string name, DataType dataType, bool store, Index indexType) :
            this(name, dataType, 0, store, indexType)
        {
        }


        public Field(string name, DataType dataType, int dataLength, bool store, Index indexType)
        {
            _Name = name;
            _DataType = dataType;
            _DataLength = dataLength;
            _Store = store;

            switch (indexType)
            {
                case Index.None:
                    break;
                case Index.Tokenized:
                    _IndexType = DataType == DataType.String ? Index.Tokenized : Index.None;
                    break;
                case Index.Untokenized:
                    _IndexType = DataType == DataType.Data ? Index.None : Index.Tokenized;
                    break;

            }
        }
        #endregion
    }
}
