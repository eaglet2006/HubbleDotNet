using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.Update
{

    public enum UpdateTableNameStateFunction
    {
        None = 0,
        Name = 1,
    }

    class UpdateTableNameState : SyntaxState<UpdateTableNameStateFunction>
    {

        public UpdateTableNameState(int id, bool isQuit, UpdateTableNameStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == UpdateTableNameStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public UpdateTableNameState(int id, bool isQuit, UpdateTableNameStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public UpdateTableNameState(int id, bool isQuit) :
            this(id, isQuit, UpdateTableNameStateFunction.None, null)
        {

        }

        public UpdateTableNameState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, UpdateTableNameStateFunction.None, nextStateIdDict)
        {

        }

        public UpdateTableNameState(int id) :
            this(id, false, UpdateTableNameStateFunction.None, null)
        {

        }

        public UpdateTableNameState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, UpdateTableNameStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, UpdateTableNameStateFunction> dfa)
        {
            UpdateTableName deleteFrom = dfa as UpdateTableName;

            switch (Func)
            {
                case UpdateTableNameStateFunction.Name:
                    deleteFrom.Name = dfa.CurrentToken.Text;
                    break;
            }
        }

    }

    public class UpdateTableName : Syntax<UpdateTableNameStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<UpdateTableNameStateFunction> s0 = AddSyntaxState(new UpdateTableNameState(0)); //Start state;
        private static SyntaxState<UpdateTableNameStateFunction> squit = AddSyntaxState(new UpdateTableNameState(30, true)); //quit state;

        /*****************************************************
         * s0 -- Update , -- s1
         * s1 -- Identitier -- s2
         * s2 -- Set -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<UpdateTableNameStateFunction> s1 = AddSyntaxState(new UpdateTableNameState(1));
            SyntaxState<UpdateTableNameStateFunction> s2 = AddSyntaxState(new UpdateTableNameState(2, false, UpdateTableNameStateFunction.Name));

            s0.AddNextState((int)SyntaxType.UPDATE, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

            s2.AddNextState((int)SyntaxType.SET, squit.Id);
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
