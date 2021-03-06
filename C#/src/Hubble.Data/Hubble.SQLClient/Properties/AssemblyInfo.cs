﻿/*
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Hubble.SQLClient")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Hubble Star Pty")]
#if HubblePro
[assembly: AssemblyProduct("HubblePro 2012 Beta 1 Edition")]
[assembly: AssemblyCopyright("Copyright © Hubble Star Pty 2011")]
#else
[assembly: AssemblyProduct("HubbleDotNet Community Edition")]
[assembly: AssemblyCopyright("Copyright © eaglet 2009-2011")]
#endif
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("40ccb0cd-2dcb-472f-b8de-196bb124c052")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.2.4.0")]
[assembly: AssemblyFileVersion("1.2.4.0")]

/*****************************************************************************************
 * Hubble.SQLClient modification records
 * ***************************************************************************************
 * 1.1.9.6
 * Add connection pool for async connection. Add Min Pool Size parameter in connection string of Hubble can 
 * set the connection pool size. Default value is 1.
 * 1.1.9.7
 * Add a static function named Cancel for HubbleAsyncConnection class.
 * This function can be used to cancel the tcp connection.
 * 1.2.4.0
 * Fix a import bug of AsyncHubbleConnection that will hang up the caller at high loading environment.
 * Please replace hubble.sqlclient.dll at the bin folder of asp.net or any client side.
*****************************************************************************************/