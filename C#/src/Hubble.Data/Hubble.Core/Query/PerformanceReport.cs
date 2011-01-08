using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hubble.Core.Query
{
    class PerformanceReport : IDisposable
    {
        string _ReportTitle = null;
        Stopwatch _SW = null;

        public PerformanceReport()
            :this(null)
        {
        }

        public PerformanceReport(string reportTitle)
        {
            if (Service.CurrentConnection.ConnectionInfo == null)
            {
                return;
            }

            if (!Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.PerformanceReport)
            {
                return;
            }

            _ReportTitle = reportTitle;
            _SW = new Stopwatch();
            _SW.Start();
        }

        ~PerformanceReport()
        {
            Dispose();
        }

        public void Stop(string title)
        {
            if (_SW != null)
            {
                _SW.Stop();

                Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.WritePerformanceReportText(
                    string.Format("PerformanceReport:{0} elapse: {1} ms, at : {2}", title, _SW.ElapsedMilliseconds,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")));
            }
        }

        public void Stop()
        {
            if (_SW != null)
            {
                _SW.Stop();

                Service.CurrentConnection.ConnectionInfo.CurrentCommandContent.WritePerformanceReportText(
                    string.Format("PerformanceReport:{0} elapse: {1} ms, at : {2}", _ReportTitle, _SW.ElapsedMilliseconds,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")));
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_SW != null)
            {
                if (_SW.IsRunning)
                {
                    _SW.Stop();
                }
            }

        }

        #endregion
    }
}
