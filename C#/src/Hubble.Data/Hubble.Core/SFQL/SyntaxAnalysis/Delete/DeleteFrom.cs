using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.Delete
{
    public enum DeleteFromStateFunction
    {
        None = 0,
        Name = 1,
    }

    class DeleteFromState : SyntaxState<DeleteFromStateFunction>
    {
                
        public DeleteFromState(int id, bool isQuit, DeleteFromStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == DeleteFromStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public DeleteFromState(int id, bool isQuit, DeleteFromStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public DeleteFromState(int id, bool isQuit) :
            this(id, isQuit, DeleteFromStateFunction.None, null)
        {

        }

        public DeleteFromState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, DeleteFromStateFunction.None, nextStateIdDict)
        {

        }

        public DeleteFromState(int id) :
            this(id, false, DeleteFromStateFunction.None, null)
        {

        }

        public DeleteFromState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, DeleteFromStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, DeleteFromStateFunction> dfa)
        {
            DeleteFrom deleteFrom = dfa as DeleteFrom;

            switch (Func)
            {
                case DeleteFromStateFunction.Name:
                    deleteFrom.Name = dfa.CurrentToken.Text;
                    break;
            }
        }

    }

    public class DeleteFrom : Syntax<DeleteFromStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<DeleteFromStateFunction> s0 = AddSyntaxState(new DeleteFromState(0)); //Start state;
        private static SyntaxState<DeleteFromStateFunction> squit = AddSyntaxState(new DeleteFromState(30, true)); //quit state;

        /*****************************************************
         * s0 -- Delete , -- s1
         * s1 -- Identitier -- s3
         * s1 -- From -- s2
         * s2 -- Identitier -- s3
         * s3 -- Where, Eof -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<DeleteFromStateFunction> s1 = AddSyntaxState(new DeleteFromState(1)); 
            SyntaxState<DeleteFromStateFunction> s2 = AddSyntaxState(new DeleteFromState(2));
            SyntaxState<DeleteFromStateFunction> s3 = AddSyntaxState(new DeleteFromState(3, false, DeleteFromStateFunction.Name));

            s0.AddNextState((int)SyntaxType.DELETE, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s3.Id);
            s1.AddNextState((int)SyntaxType.FROM, s2.Id);

            s2.AddNextState((int)SyntaxType.Identifer, s3.Id);

            s3.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.WHERE}, squit.Id);
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

        #endregion

    }

}
