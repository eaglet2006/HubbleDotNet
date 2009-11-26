using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Framework.Security
{
    public class EncryptString
    {
        public static string GetString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (byte b in bytes)
            {
                if (i++ == 0)
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                else
                {
                    sb.AppendFormat("-{0:X2}", b);
                }
            }

            return sb.ToString();
        }

        public static byte[] GetBytes(string str)
        {
            string[] hexValuesSplit = str.Split('-');

            byte[] bytes = new byte[hexValuesSplit.Length];

            int i = 0;
            foreach (String hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                bytes[i++] = (byte)Convert.ToInt32(hex.Trim(), 16);
            }

            return bytes;

        }
    }
}
