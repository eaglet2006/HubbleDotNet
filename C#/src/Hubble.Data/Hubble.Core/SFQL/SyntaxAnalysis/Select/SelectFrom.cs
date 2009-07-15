using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.Select
{
    public enum SelectFromStateFunction
    {
        None = 0,
        Name = 1,
        Alias= 2,
    }

    class SelectFromState : SyntaxState<SelectFromStateFunction>
    {
        public SelectFromState(int id, bool isQuit, SelectFromStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == SelectFromStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public SelectFromState(int id, bool isQuit, SelectFromStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public SelectFromState(int id, bool isQuit) :
            this(id, isQuit, SelectFromStateFunction.None, null)
        {

        }

        public SelectFromState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, SelectFromStateFunction.None, nextStateIdDict)
        {

        }

        public SelectFromState(int id) :
            this(id, false, SelectFromStateFunction.None, null)
        {

        }

        public SelectFromState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, SelectFromStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, SelectFromStateFunction> dfa)
        {
            SelectFrom selectFrom = dfa as SelectFrom;

            switch (Func)
            {
                case SelectFromStateFunction.Name:
                    selectFrom.Name = dfa.CurrentToken.Text;
                    selectFrom.Alias = selectFrom.Name;
                    break;
                case SelectFromStateFunction.Alias:
                    selectFrom.Alias = dfa.CurrentToken.Text;
                    break;
            }
        }
    }

    public class SelectFrom : Syntax<SelectFromStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<SelectFromStateFunction> s0 = AddSyntaxState(new SelectFromState(0)); //Start state;
        private static SyntaxState<SelectFromStateFunction> squit = AddSyntaxState(new SelectFromState(30, true)); //quit state;

        /*****************************************************
         * s0 -- From , -- s1
         * s1 -- Identitier -- s2
         * s2 -- AS -- s3
         * s2 -- Identitier -- s4
         * s2 -- , Where, Eof, Order, Group -- squit
         * s3 -- Identitier -- s4
         * s4 -- , Where, Eof, Order, Group -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<SelectFromStateFunction> s1 = AddSyntaxState(new SelectFromState(1)); //Attribute start state;
            SyntaxState<SelectFromStateFunction> s2 = AddSyntaxState(new SelectFromState(2, false, SelectFromStateFunction.Name));
            SyntaxState<SelectFromStateFunction> s3 = AddSyntaxState(new SelectFromState(3));
            SyntaxState<SelectFromStateFunction> s4 = AddSyntaxState(new SelectFromState(4, false, SelectFromStateFunction.Alias));

            s0.AddNextState(new int[] { (int)SyntaxType.FROM, (int)SyntaxType.Comma }, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

            s2.AddNextState((int)SyntaxType.AS, s3.Id);
            s2.AddNextState((int)SyntaxType.Identifer, s4.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.Comma, 
                (int)SyntaxType.WHERE, (int)SyntaxType.ORDER, (int)SyntaxType.GROUP }, squit.Id);

            s3.AddNextState((int)SyntaxType.Identifer, s4.Id);

            s4.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.Comma, 
                (int)SyntaxType.WHERE, (int)SyntaxType.ORDER, (int)SyntaxType.GROUP }, squit.Id);
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

        public string Name;

        public string Alias;

        #endregion

    }
}


