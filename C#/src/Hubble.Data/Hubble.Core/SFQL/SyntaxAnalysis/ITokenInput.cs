using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public interface ITokenInput
    {
        DFAResult Input(int action, LexicalAnalysis.Lexical.Token token);
    }
}
