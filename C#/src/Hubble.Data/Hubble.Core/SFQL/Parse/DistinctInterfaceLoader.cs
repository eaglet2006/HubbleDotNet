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
using System.Linq;
using System.Text;

namespace Hubble.Core.SFQL.Parse
{
    /// <summary>
    /// this class is used to load interfaces for distinct
    /// </summary>
    class DistinctInterfaceLoader
    {
        static private Dictionary<string, Type> _sNameToType = new Dictionary<string, Type>();
        static private object _LockObj = new object();

        static internal Dictionary<string, Type> ExternalDistincts
        {
            get
            {
                lock (_LockObj)
                {
                    return _sNameToType;
                }
            }
        }

        static internal IDistinct GetDistinct(string name)
        {
            if (name.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                return new ParseDistinct();
            }

            lock (_LockObj)
            {
                Type type;

                if (_sNameToType.TryGetValue(name.ToLower(), out type))
                {
                    return Hubble.Framework.Reflection.Instance.CreateInstance(type) as IDistinct;
                }
            }
            
            return null;
        }
    }
}
