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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("QueryAnalyzer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Hubble Star Pty")]
#if HubblePro
[assembly: AssemblyProduct("HubblePro 2012 Beta 2 Edition")]
[assembly: AssemblyCopyright("Copyright © Hubble Star Pty 2011-2012")]
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
[assembly: Guid("a9547725-a7e9-4c8a-b76e-be9c76d17203")]

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
[assembly: AssemblyVersion("1.2.7.8")]
[assembly: AssemblyFileVersion("1.2.7.8")]

/*****************************************************************************************
 * QueryAnalyzer modification records
 * ***************************************************************************************
 * 
 * 1.1.6.7 Modified by Bo Xiao
 * Change the display of unit of QueryCacheTimeout from ms to s. It is a mistake.
 * 
 * 1.1.9.3 
 * Modified Rebuild table feature
 * 
 * 1.1.9.4
 * Change System.Data.DataSet, DataTable to Hubble.Framework.Data.DataSet, DataTable
 * 
 * 1.2.3.3
 * MinimumCapacity can cause windows form control alloc a lot of memory. Set it to zeor before 
 * dataset transfer to windows from control. 
 * 1.2.4.1
 * Can create mirror table at append only mode
 * 1.2.7.7
 * Fix the right click issue.
 * Expand treeview after truncate table.
 * 1.2.7.8
 * Modify Bigtable setting, can enable/disable server, enable/disable tablet
 *****************************************************************************************/
