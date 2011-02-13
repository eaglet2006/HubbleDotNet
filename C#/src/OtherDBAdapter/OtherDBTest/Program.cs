using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MySqlDBAdapter;

namespace OtherDBTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=192.168.1.4;Database=test;Uid=root;Pwd=sa;";
            string sql = "select * from News where docid >0 order by docid limit 5000";
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();

            for (int i = 0; i < 30; i++)
            {
                sql = string.Format("select * from News where docid > {0} order by docid limit 10000", i * 10000);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                using (MysqlDataProvider sqlData = new MysqlDataProvider())
                {
                    sqlData.Connect(connectionString);
                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds);
                    sw.Reset();
                    sw.Start();
                    sqlData.QuerySql(sql);
                }

                sw.Stop();

                Console.WriteLine(sw.ElapsedMilliseconds);
            }

            sw1.Stop();

            Console.WriteLine(sw1.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
