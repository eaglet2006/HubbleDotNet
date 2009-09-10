using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.Exec
{
    public enum ExecFunction
    {
        None = 0,
        Name = 1,
        Para = 2,
    }

    public class ExecState : SyntaxState<ExecFunction>
    {

        public ExecState(int id, bool isQuit, ExecFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == ExecFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public ExecState(int id, bool isQuit, ExecFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public ExecState(int id, bool isQuit) :
            this(id, isQuit, ExecFunction.None, null)
        {

        }

        public ExecState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, ExecFunction.None, nextStateIdDict)
        {

        }

        public ExecState(int id) :
            this(id, false, ExecFunction.None, null)
        {

        }

        public ExecState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, ExecFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, ExecFunction> dfa)
        {
            switch (Func)
            {
                case ExecFunction.Name:
                    ((Exec)dfa).StoredProcedureName = dfa.CurrentToken.Text;
                    break;
                case ExecFunction.Para:
                    ((Exec)dfa).CurrentSyntax = new ExecParameter();
                    break;
            }
        }

    }

    public class Exec : SyntaxContainer<ExecFunction>, SyntaxAnalysis.ITokenInput, SyntaxAnalysis.ISyntaxEntity
    {
        private static SyntaxState<ExecFunction> s0 = AddSyntaxState(new ExecState(0)); //Start state;
        private static SyntaxState<ExecFunction> squit = AddSyntaxState(new ExecState(30, true)); //quit state;

        /******************************************************
         * s0 --Exec-- s1
         * s1 --Identifer--s2
         * s2 --Eof ;--squit
         * s2 --String Numeric--s3
         * s3 --,--s3
         * s3 --Eof ;--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<ExecFunction> s1 = AddSyntaxState(new ExecState(1)); 
            SyntaxState<ExecFunction> s2 = AddSyntaxState(new ExecState(2, false, ExecFunction.Name));
            SyntaxState<ExecFunction> s3 = AddSyntaxState(new ExecState(3, false, ExecFunction.Para)); 

            s0.AddNextState((int)SyntaxType.EXEC, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

            s2.AddNextState(new int[]{ (int)SyntaxType.String, (int)SyntaxType.Numeric}, s3.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.Semicolon, (int)SyntaxType.Eof }, squit.Id);
            
            s3.AddNextState((int)SyntaxType.Comma, s3.Id);
            s3.AddNextState(new int[] { (int)SyntaxType.Semicolon, (int)SyntaxType.Eof}, squit.Id);
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

        override public void Finish()
        {
            foreach (object obj in SyntaxList)
            {
                if (obj is ExecParameter)
                {
                    ExecParameters.Add(obj as ExecParameter);
                }
            }
        }

        #region public Fields

        public int Begin = 0;
        public int End = -1;

        public List<ExecParameter> ExecParameters = new List<ExecParameter>();

        string _Name;

        public string StoredProcedureName
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }
        #endregion

    }

}
