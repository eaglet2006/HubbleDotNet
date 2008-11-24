using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Tables
{
    public class Row
    {
        #region Private
        
        private List<Column> _Columns = new List<Column>();

        #endregion

        #region Public properties

        public Column this[int index]
        {
            get
            {
                return _Columns[index];
            }
        }

        public Column this[string name]
        {
            get
            {
                Debug.Assert(name != null);

                foreach (Column field in _Columns)
                {
                    if (field.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return field;
                    }
                }

                throw new ArgumentException(string.Format("There is not a field named {0}", name));
            }
        }

        #endregion
    }
}
