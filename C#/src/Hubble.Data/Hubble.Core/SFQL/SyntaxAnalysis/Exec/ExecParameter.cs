using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.Exec
{

    public enum ExecParameterFunction
    {
        None = 0,
        Value =1,
    }

    class ExecParameterState : SyntaxState<ExecParameterFunction>
    {
        public ExecParameterState(int id, bool isQuit, ExecParameterFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == ExecParameterFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public ExecParameterState(int id, bool isQuit, ExecParameterFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public ExecParameterState(int id, bool isQuit) :
            this(id, isQuit, ExecParameterFunction.None, null)
        {

        }

        public ExecParameterState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, ExecParameterFunction.None, nextStateIdDict)
        {

        }

        public ExecParameterState(int id) :
            this(id, false, ExecParameterFunction.None, null)
        {

        }

        public ExecParameterState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, ExecParameterFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, ExecParameterFunction> dfa)
        {
            ExecParameter selectField = dfa as ExecParameter;

            switch (Func)
            {
                case ExecParameterFunction.Value:
                    selectField.Value = dfa.CurrentToken.Text;
                    break;
            }
        }
    }

    public class ExecParameter : Syntax<ExecParameterFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<ExecParameterFunction> s0 = AddSyntaxState(new ExecParameterState(0)); //Start state;
        private static SyntaxState<ExecParameterFunction> squit = AddSyntaxState(new ExecParameterState(30, true)); //quit state;

        /*****************************************************
         * s0 -- String Numeric -- s2
         * s0 -- , -- s1
         * s1 -- String Numeric --s2
         * s2 -- , Eof ;-- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<ExecParameterFunction> s1 = AddSyntaxState(new ExecParameterState(1)); //Attribute start state;
            SyntaxState<ExecParameterFunction> s2 = AddSyntaxState(new ExecParameterState(2, false, ExecParameterFunction.Value));

            s0.AddNextState(new int[] {(int)SyntaxType.String, (int)SyntaxType.Numeric}, s2.Id);
            s0.AddNextState((int)SyntaxType.Comma, s1.Id);

            s1.AddNextState(new int[] { (int)SyntaxType.String, (int)SyntaxType.Numeric }, s2.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.Semicolon, (int)SyntaxType.Comma }, squit.Id);
        }

        public static void Initialize()
        {
            lock (InitLockObj)
            {
                if (!Inited)
                {
                    InitDFAStates();
                    Inited = true;
                }
            }
        }

        protected override void Init()
        {
            Initialize();
        }

        #region public Fields

        public string Value;

        #endregion

    }

}
