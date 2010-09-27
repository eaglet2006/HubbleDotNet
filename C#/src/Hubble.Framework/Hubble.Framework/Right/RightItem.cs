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

namespace Hubble.Core.Right
{
    [Flags]
    public enum RightItem
    {
        ManageUser    = 0x00000001,
        ManageDB      = 0x00000002,
        WriteDatabase = 0x00000004,
        ExcuteStoreProcedure = 0x00000008,
        Optimize      = 0x00000010,
        CreateTable   = 0x00000020,
        DropTable     = 0x00000040,
        Select        = 0x00000080,
        Update        = 0x00000100,
        Delete        = 0x00000200,
    }
}
