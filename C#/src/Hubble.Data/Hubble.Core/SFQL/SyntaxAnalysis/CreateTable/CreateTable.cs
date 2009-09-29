using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis.CreateTable
{
    public enum CreateTableFunction
    {
        None = 0,
        TableName = 1,
        Field = 2,
    }

    class CreateTableState : SyntaxState<CreateTableFunction>
    {
        public CreateTableState(int id, bool isQuit, CreateTableFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == CreateTableFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public CreateTableState(int id, bool isQuit, CreateTableFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public CreateTableState(int id, bool isQuit) :
            this(id, isQuit, CreateTableFunction.None, null)
        {

        }

        public CreateTableState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, CreateTableFunction.None, nextStateIdDict)
        {

        }

        public CreateTableState(int id) :
            this(id, false, CreateTableFunction.None, null)
        {

        }

        public CreateTableState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, CreateTableFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, CreateTableFunction> dfa)
        {
            switch (Func)
            {
                case CreateTableFunction.TableName:
                    ((CreateTable)dfa).TableName = dfa.CurrentToken.Text;
                    break;
                case CreateTableFunction.Field:
                    ((CreateTable)dfa).CurrentSyntax = new CreateTableField();
                    break;
            }
        }
    }



    class CreateTable : SyntaxContainer<CreateTableFunction>, SyntaxAnalysis.ITokenInput, SyntaxAnalysis.ISyntaxEntity
    {
        private static SyntaxState<CreateTableFunction> s0 = AddSyntaxState(new CreateTableState(0)); //Start state;
        private static SyntaxState<CreateTableFunction> squit = AddSyntaxState(new CreateTableState(30, true)); //quit state;

        /******************************************************
         * s0 --Table-- s1
         * s1 --Identifer--s2
         * s2 --(--s3
         * s3 --)--s4
         * s3 --,-- s3
         * s4 --Eof ;--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<CreateTableFunction> s1 = AddSyntaxState(new CreateTableState(1)); 
            SyntaxState<CreateTableFunction> s2 = AddSyntaxState(new CreateTableState(2, false, CreateTableFunction.TableName));
            SyntaxState<CreateTableFunction> s3 = AddSyntaxState(new CreateTableState(3, false, CreateTableFunction.Field));
            SyntaxState<CreateTableFunction> s4 = AddSyntaxState(new CreateTableState(4)); 

            s0.AddNextState((int)SyntaxType.TABLE, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);

            s2.AddNextState((int)SyntaxType.LBracket, s3.Id);

            s3.AddNextState((int)SyntaxType.RBracket, s4.Id);
            s3.AddNextState((int)SyntaxType.Comma, s3.Id);

            s4.AddNextState((int)SyntaxType.Eof, squit.Id);
            s4.AddNextState((int)SyntaxType.Semicolon, squit.Id);
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
                if (obj is CreateTableField)
                {
                    CreateTableField field = obj as CreateTableField;

                    Fields.Add(field);
                }
            }
        }

        #region public Fields

        public List<CreateTableField> Fields = new List<CreateTableField>();
        public string TableName;

        #endregion

    }
}
