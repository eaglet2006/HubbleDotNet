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

/* If you want to convert pdf to text, you have to install Adobe PDF iFilter 9 or later  
 * 64bit download url  http://www.adobe.com/support/downloads/thankyou.jsp?ftpID=4025&fileID=3941
 * 
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hubble.Tools
{
    /// <summary>
    /// This class is used to Convert file to text.
    /// Support all of the office file type such as *.doc, *.docx, *.ppt, *.xls etc. and pdf file.
    /// </summary>
    public class TextParse
    {
        string _FilePath;

        /// <summary>
        /// Constractor
        /// </summary>
        /// <param name="filePath">file path of document file</param>
        public TextParse(string filePath)
        {
            _FilePath = filePath;

            if (!File.Exists(_FilePath))
            {
                throw new IOException(string.Format("{0} does not exist", _FilePath));
            }
        }

        /// <summary>
        /// Convert document file to string
        /// </summary>
        /// <returns></returns>
        public string ConvertToString()
        {
            FilterLibrary.FilterCode filter = new FilterLibrary.FilterCode();

            StringBuilder sb = new StringBuilder();

            filter.GetTextFromDocument(_FilePath, ref sb);

            return sb.ToString();
        }

        /// <summary>
        /// Convert to a text file
        /// </summary>
        /// <param name="filePath">specified output text file path</param>
        public void ConvertToFile(string filePath)
        {
            ConvertToFile(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// Convert to a text file
        /// </summary>
        /// <param name="filePath">specified output text file path</param>
        /// <param name="encoding">specified encoding</param>
        public void ConvertToFile(string filePath, Encoding encoding)
        {
            FilterLibrary.FilterCode filter = new FilterLibrary.FilterCode();

            StringBuilder sb = new StringBuilder();

            filter.ConvertToFile(_FilePath, filePath, encoding);
        }
    }
}
