using System;
using System.Collections.Generic;
using System.Text;

namespace TestFramework
{
    abstract class TestCaseBase
    {
        protected StringBuilder _Report = new StringBuilder();
        protected string _Name;

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

        public string Report
        {
            get
            {
                return _Report.ToString();
            }
        }

        public TestCaseBase()
        {
            _Name = this.GetType().Name;
        }

        abstract public void Test();

        protected void AssignEquals(object src, object dest, string description)
        {
            if (src.Equals(dest))
            {
                _Report.AppendFormat("{0}: {1} Equal OK!\r\n", _Name, description);
            }
            else
            {
                _Report.AppendFormat("{0}: {1} Equal Fail!\r\n", _Name, description);

                System.Diagnostics.Debugger.Break();
            }
        }

    }
}
