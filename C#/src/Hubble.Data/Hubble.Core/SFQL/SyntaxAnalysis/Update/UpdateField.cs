using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.Update
{

    public enum UpdateFieldFunction
    {
        None = 0,
        Name = 1,
        Value = 2,
    }

    class UpdateFieldState : SyntaxState<UpdateFieldFunction>
    {
        public UpdateFieldState(int id, bool isQuit, UpdateFieldFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == UpdateFieldFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public UpdateFieldState(int id, bool isQuit, UpdateFieldFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public UpdateFieldState(int id, bool isQuit) :
            this(id, isQuit, UpdateFieldFunction.None, null)
        {

        }

        public UpdateFieldState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, UpdateFieldFunction.None, nextStateIdDict)
        {

        }

        public UpdateFieldState(int id) :
            this(id, false, UpdateFieldFunction.None, null)
        {

        }

        public UpdateFieldState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, UpdateFieldFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, UpdateFieldFunction> dfa)
        {
            UpdateField selectField = dfa as UpdateField;

            switch (Func)
            {
                case UpdateFieldFunction.Name:
                    selectField.Name = dfa.CurrentToken.Text;
                    break;
                case UpdateFieldFunction.Value:
                    selectField.Value = dfa.CurrentToken.Text;
                    break;
            }
        }
    }

    public class UpdateField : Syntax<UpdateFieldFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<UpdateFieldFunction> s0 = AddSyntaxState(new UpdateFieldState(0)); //Start state;
        private static SyntaxState<UpdateFieldFunction> squit = AddSyntaxState(new UpdateFieldState(30, true)); //quit state;

        /*****************************************************
         * s0 -- Set , -- s1
         * s1 -- Identitier -- s2
         * s2 -- = -- s3
         * s3 -- String, Numeric -- s4
         * s4 -- , Where Eof -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<UpdateFieldFunction> s1 = AddSyntaxState(new UpdateFieldState(1)); //Attribute start state;
            SyntaxState<UpdateFieldFunction> s2 = AddSyntaxState(new UpdateFieldState(2, false, UpdateFieldFunction.Name));
            SyntaxState<UpdateFieldFunction> s3 = AddSyntaxState(new UpdateFieldState(3));
            SyntaxState<UpdateFieldFunction> s4 = AddSyntaxState(new UpdateFieldState(4, false, UpdateFieldFunction.Value));

            s0.AddNextState(new int[] { (int)SyntaxType.SET, (int)SyntaxType.Comma }, s1.Id);

            s1.AddNextState(new int[] { (int)SyntaxType.Identifer}, s2.Id);

            s2.AddNextState((int)SyntaxType.Equal, s3.Id);

            s3.AddNextState(new int[] { (int)SyntaxType.Numeric, (int)SyntaxType.String }, s4.Id);

            s4.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.WHERE, (int)SyntaxType.Comma }, squit.Id);

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

        public string Value;

        #endregion

    }

}
