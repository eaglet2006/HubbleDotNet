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

namespace Hubble.Framework.Reflection
{
    public class Instance
    {
        static public object CreateInstance(string typeName)
        {
            object obj = null;
            obj = System.Reflection.Assembly.GetCallingAssembly().CreateInstance(typeName);

            if (obj != null)
            {
                return obj;
            }

            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                obj = asm.CreateInstance(typeName);

                if (obj != null)
                {
                    return obj;
                }
            }

            return null;

        }

        static public object CreateInstance(Type type)
        {
            return type.Assembly.CreateInstance(type.FullName);
        }

        static public object CreateInstance(Type type, string assemblyFile)
        {
            System.Reflection.Assembly asm;

            asm = System.Reflection.Assembly.LoadFrom(assemblyFile);

            return asm.CreateInstance(type.FullName);
        }
    }
}
