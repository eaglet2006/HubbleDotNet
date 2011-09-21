using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Hubble.Core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Hubble Star Pty")]
#if HubblePro
[assembly: AssemblyProduct("HubblePro 2011 Beta 3 Edition")]
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
[assembly: Guid("6936932f-1a62-4b02-b274-ee985480ad82")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.1.6.9")]
[assembly: AssemblyFileVersion("1.1.6.9")]

/*****************************************************************************************
 * Hubble.Core modification records
 * ***************************************************************************************
 * 1.1.6.8 Bo Xiao
 * If sorted, don't sort again.
 * 1.1.6.9 Bo Xiao
 * Modify AndMergeForNot. Set the RelCount = Count;
 * When we execute sql with the condition looks like:
 * title match 'abc news' and title not contains 'new' order by score 
 * Wrong count returned.
 * 
 * 
 * 
 *****************************************************************************************/