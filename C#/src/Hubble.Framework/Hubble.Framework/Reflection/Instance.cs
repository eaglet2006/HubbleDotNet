using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Reflection
{
    public class Instance
    {
        static public object CreateInstance(Type type)
        {
            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                object obj = asm.CreateInstance(type.FullName);

                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }

        static public object CreateInstance(Type type, string assemblyFile)
        {
            System.Reflection.Assembly asm;

            asm = System.Reflection.Assembly.LoadFrom(assemblyFile);

            return asm.CreateInstance(type.FullName);
        }
    }
}
