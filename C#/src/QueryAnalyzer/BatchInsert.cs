using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QueryAnalyzer
{
    class BatchInsert
    {
        public delegate void GetTotalRecordsDelegate(int current);

        public int GetTotalRecords(string fileName, GetTotalRecordsDelegate getTotalRecordsDelegate)
        {
            int totalRecords = 0;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string lastLine = null;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (line.IndexOf("insert", 0, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            if (lastLine == null)
                            {
                                totalRecords++;
                                getTotalRecordsDelegate(totalRecords);
                            }
                            else if (lastLine.EndsWith(";"))
                            {
                                totalRecords++;
                                getTotalRecordsDelegate(totalRecords);
                            }
                        }

                        lastLine = line;
                    }
                }

                return totalRecords;
            }
        }

        public void BatchImport(string fileName, DbAccess db, int count, 
            GetTotalRecordsDelegate getTotalRecordsDelegate, System.Diagnostics.Stopwatch sw)
        {
            int totalRecords = 0;

            StringBuilder sb = new StringBuilder();

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string lastLine = null;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (line.IndexOf("insert", 0, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            if (lastLine == null)
                            {
                                totalRecords++;
                                getTotalRecordsDelegate(totalRecords);
                            }
                            else if (lastLine.EndsWith(";"))
                            {
                                if (totalRecords > 0)
                                {
                                    if (totalRecords % 1000 == 0 || totalRecords == count)
                                    {
                                        sw.Start();
                                        db.Excute(sb.ToString());
                                        sw.Stop();
                                        sb = new StringBuilder();
                                    }
                                }

                                if (totalRecords == count)
                                {
                                    getTotalRecordsDelegate(totalRecords);
                                    return;
                                }

                                totalRecords++;


                                getTotalRecordsDelegate(totalRecords);

                            }
                        }

                        sb.AppendLine(line);

                        lastLine = line;
                    }

                    if (sb.Length > 0)
                    {
                        sw.Start();
                        db.Excute(sb.ToString());
                        sw.Stop();
                        getTotalRecordsDelegate(totalRecords);
                    }
                }
            }
        }
    }
}
