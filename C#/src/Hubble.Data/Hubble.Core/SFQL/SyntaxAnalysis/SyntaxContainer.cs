using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;
using Hubble.Core.SFQL.LexicalAnalysis;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public abstract class SyntaxContainer<Function> : DFA<LexicalAnalysis.Lexical.Token, Function> 
    {
        List<ITokenInput> _SyntaxList = new List<ITokenInput>();

        public ITokenInput CurrentSyntax = null;

        public List<ITokenInput> SyntaxList
        {
            get
            {
                return _SyntaxList;
            }
        }

        new public virtual DFAResult Input(int action, LexicalAnalysis.Lexical.Token token)
        {
            try
            {
                DFAResult result;

                if (CurrentSyntax == null)
                {
                    result = base.Input(action, token);

                    switch (result)
                    {
                        case DFAResult.Quit:
                        case DFAResult.ElseQuit:
                            Finish();
                            return result;
                        default:
                            if (CurrentSyntax == null)
                            {
                                return result;
                            }
                            break;
                    }
                }

                result = CurrentSyntax.Input(action, token);

                switch (result)
                {
                    case DFAResult.Quit:
                        _SyntaxList.Add(CurrentSyntax);
                        CurrentSyntax = null;

                        return Input(action, token);

                    case DFAResult.ElseQuit:
                        _SyntaxList.Add(CurrentSyntax);
                        CurrentSyntax = null;

                        return Input(action, token);
                }

                return DFAResult.Continue;
            }
            catch (DFAException dfaEx)
            {
                throw new SyntaxException(dfaEx.Message, this, dfaEx, token);
            }
        }

        public static SyntaxState<Function> AddSyntaxState(SyntaxState<Function> state)
        {
            return (SyntaxState<Function>)AddState((DFAState<Lexical.Token, Function>)state);
        }

        abstract public void Finish();
    }
}
