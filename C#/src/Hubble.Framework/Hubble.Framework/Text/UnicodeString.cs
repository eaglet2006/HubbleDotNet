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

namespace Hubble.Framework.Text
{
    public class UnicodeString
    {
        public static int Comparer(string str1, string str2)
        {
            if (str1 == null && str2 == null)
            {
                return 0;
            }
            else if (str1 == null)
            {
                return -1;
            }
            else if (str2 == null)
            {
                return 1;
            }

            for (int i = 0; i < str1.Length; i++)
            {
                if (i >= str2.Length)
                {
                    return 1;
                }

                if (str1[i] > str2[i])
                {
                    return 1;
                }
                else if (str1[i] < str2[i])
                {
                    return -1;
                }
            }

            if (str1.Length < str2.Length)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
