using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;
using Hubble.Core.SFQL.LexicalAnalysis;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public abstract class SyntaxState<Function> : DFAState<Lexical.Token, Function>
    {
    }
}
