using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Reflection
{
    public class Assembly
    {
        public static Version GetAssemblyVersion(System.Reflection.Assembly assembly)
        {
            return assembly.GetName().Version;
        }

        public static Version GetCallingAssemblyVersion()
        {
            return GetAssemblyVersion(System.Reflection.Assembly.GetCallingAssembly());
        }

        public static Version GetExecutingAssemblyVersion()
        {
            return GetAssemblyVersion(System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static int[] GetVersionValue(Version version)
        {
            return new int[] {version.Major, version.Minor, version.Build, version.Revision };
        }

    }
}
