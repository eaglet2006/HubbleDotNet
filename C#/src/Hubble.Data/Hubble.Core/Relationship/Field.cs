using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Relationship
{
    public class Field
    {
        #region Private
        
        private List<Document> _Columns = new List<Document>();

        #endregion

        #region Public properties

        public Document this[int index]
        {
            get
            {
                return _Columns[index];
            }
        }

        public Document this[string name]
        {
            get
            {
                Debug.Assert(name != null);

                foreach (Document field in _Columns)
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
