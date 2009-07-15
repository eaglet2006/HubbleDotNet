using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;
using Hubble.Core.SFQL.LexicalAnalysis;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    abstract public class Syntax<Function> : DFA<LexicalAnalysis.Lexical.Token, Function> 
    {
        public static SyntaxState<Function> AddSyntaxState(SyntaxState<Function> state)
        {
            return (SyntaxState<Function>)AddState((DFAState<Lexical.Token, Function>)state);
        }

        new public virtual DFAResult Input(int action, LexicalAnalysis.Lexical.Token token)
        {
            try
            {
                return base.Input(action, token);
            }
            catch (DFAException dfaEx)
            {
                throw new SyntaxException(dfaEx.Message, this, dfaEx, token);
            }
        }
    }
}
