using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Relationship
{
    public class Document
    {
        #region Private

        private string _Name;

        #endregion

        #region Public properties

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        #endregion

        #region Constructor

        public Document(string name)
        {
            Debug.Assert(name != null);

            _Name = name;
        }

        #endregion
    }
}
