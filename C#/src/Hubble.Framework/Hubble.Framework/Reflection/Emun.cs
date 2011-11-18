using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Framework.Reflection
{
    public class Emun
    {
        /// <summary>
        /// Get enum value as int from string.
        /// If the enum is flags enum, use '|' to split every elements.
        /// For example:
        /// Insert|Update|Delete
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="text">string need to be converted. It is case insensitive</param>
        /// <returns>enum value as int</returns>
        public static int FromString(Type enumType, string text)
        {
            int last = 0;
            foreach (string str in text.Split(new char[] { '|' }))
            {
                int t = (int)Enum.Parse(enumType, str.Trim());
                last |= t;
            }

            return last;
        }
    }
}
