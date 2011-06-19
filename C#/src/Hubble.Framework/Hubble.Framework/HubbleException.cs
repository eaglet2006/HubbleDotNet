using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.Net;

namespace Hubble.Framework
{
    public class HubbleException
    {
        public static string GetStackTrace(Exception e)
        {
            if (e is ServerException)
            {
                return (e as ServerException).InnerStackTrace;
            }
            else
            {
                return e.StackTrace;
            }
        }
    }
}
