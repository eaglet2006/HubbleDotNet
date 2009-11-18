/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace HubbleTask
{
    class Program
    {
        static string _LogFilePath;

        static private void WriteErrorMessage(string msg)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Error message at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", null));
                sb.AppendLine(msg);

                Console.WriteLine(sb.ToString());
                Hubble.Framework.IO.File.WriteLine(_LogFilePath, sb.ToString());
            }
            catch
            {
            }
        }

        static void Main(string[] args)
        {
            _LogFilePath = Hubble.Framework.IO.Path.AppendDivision(
                Hubble.Framework.IO.Path.ProcessDirectory, '\\') + "hubblesvr.log";
            
            string taskPath ;

            if (args.Length < 1)
            {
                WriteErrorMessage("args.Length == 0 and set path to current dir");

                taskPath = Hubble.Framework.IO.Path.ProcessDirectory;
            }
            else
            {
                taskPath = args[0];
            }

            try
            {
                Hubble.Core.Service.HubbleTask hubbleTask = new Hubble.Core.Service.HubbleTask(taskPath);

                Console.ReadKey();
            }
            catch (Exception e)
            {
                WriteErrorMessage(e.Message + e.StackTrace);
            }
        }
    }
}
