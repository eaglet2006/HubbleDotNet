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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hubble.Framework.Serialization
{
    public class BinSerialization
    {
        public static void SerializeBinary(object Obj, Stream s)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, Obj);
            s.Flush();
        }

        public static Stream SerializeBinary(object Obj)
        {
            MemoryStream s = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, Obj);
            s.Position = 0;
            return s;
        }

        public static void DeserializeBinary(Stream In, out object Obj)
        {
            In.Position = 0;
            IFormatter formatter = new BinaryFormatter();
            Obj = formatter.Deserialize(In);
        }

    }
}
