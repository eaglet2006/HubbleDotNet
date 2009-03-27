using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Global
{
    public class Report : Hubble.Framework.IO.LogFile<Report>
    {
        public Report()
        {
            LogDir = Setting.Config.Directories.LogDirectory;
        }
    }
}
