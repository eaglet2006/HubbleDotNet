using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Framework
{
    public class TypeConverter
    {
        public static object FromString(Type type, string value)
        {
            return System.ComponentModel.TypeDescriptor.GetConverter(type).ConvertFrom(value);
        }
    }
}
