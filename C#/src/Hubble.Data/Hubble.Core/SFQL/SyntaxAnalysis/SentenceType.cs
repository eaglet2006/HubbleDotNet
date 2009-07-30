using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public enum SentenceType
    {
        NONE   = 0,
        SELECT = 1,
        UPDATE = 2,
        DELETE = 3,
        INSERT = 4,
    }
}
