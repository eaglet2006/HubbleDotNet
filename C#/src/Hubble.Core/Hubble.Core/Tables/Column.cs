using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Tables
{
    public class Column
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

        public Column(string name)
        {
            Debug.Assert(name != null);

            _Name = name;
        }

        #endregion
    }
}
