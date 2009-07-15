using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public enum TSFQLSentenceFunction
    {
        None = 0,
        Attribute = 1,
        Select = 2,

    }

    class TSFQLState : SyntaxState<TSFQLSentenceFunction>
    {
        public TSFQLState(int id, bool isQuit, TSFQLSentenceFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == TSFQLSentenceFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public TSFQLState(int id, bool isQuit, TSFQLSentenceFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public TSFQLState(int id, bool isQuit) :
            this(id, isQuit, TSFQLSentenceFunction.None, null)
        {

        }

        public TSFQLState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, TSFQLSentenceFunction.None, nextStateIdDict)
        {

        }

        public TSFQLState(int id) :
            this(id, false, TSFQLSentenceFunction.None, null)
        {

        }

        public TSFQLState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, TSFQLSentenceFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, TSFQLSentenceFunction> dfa)
        {
            switch (Func)
            {
                case TSFQLSentenceFunction.Attribute:
                    ((TSFQLSentence)dfa).CurrentSyntax = new TSFQLAttribute();
                    break;
                case TSFQLSentenceFunction.Select:
                    ((TSFQLSentence)dfa).CurrentSyntax = new Select.Select(); 
                    break;
            }
        }
    }

    /// <summary>
    /// This class explain one T-SFQL Sentence
    /// </summary>
    public class TSFQLSentence : SyntaxContainer<TSFQLSentenceFunction>
    {
        private static SyntaxState<TSFQLSentenceFunction> s0 = AddSyntaxState (new TSFQLState(0)); //Start state;
        private static SyntaxState<TSFQLSentenceFunction> squit = AddSyntaxState(new TSFQLState(30, true)); //quit state;

        /***********************************************************
         * s0 -- [ -- s1
         * s0 -- Select --s3
         * s1 -- ] -- s2
         * s2 -- [ -- s1
         * s2 -- EOF ; -- squit
         * s2 -- Select -- s3
         * s3 -- EOF ; -- squit
         * 
         * ********************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<TSFQLSentenceFunction> s1 = AddSyntaxState(new TSFQLState(1, false, TSFQLSentenceFunction.Attribute)); //Attribute start state;
            SyntaxState<TSFQLSentenceFunction> s2 = AddSyntaxState(new TSFQLState(2)); //Attribute end state;
            SyntaxState<TSFQLSentenceFunction> s3 = AddSyntaxState(new TSFQLState(3, false, TSFQLSentenceFunction.Select)); //select state;

            s0.AddNextState((int)SyntaxType.LSquareBracket, s1.Id);
            s0.AddNextState((int)SyntaxType.SELECT, s3.Id);

            s1.AddNextState((int)SyntaxType.RSquareBracket, s2.Id);
            
            s2.AddNextState((int)SyntaxType.LSquareBracket, s1.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.Eof, (int)SyntaxType.Semicolon}, squit.Id);
            s2.AddNextState((int)SyntaxType.SELECT, s3.Id);

            s3.AddNextState(new int[] { (int)SyntaxType.FROM, (int)SyntaxType.Eof, (int)SyntaxType.Semicolon }, squit.Id);

            //s1.AddNextState((int)SyntaxType.RSquareBracket, s2.Id);

            //s2.AddNextState((int)SyntaxType.LSquareBracket, s1.Id);
            //s2.AddNextState((int)SyntaxType.SELECT, s3.Id);

            //s3.AddElseState(squit.Id);
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
                if (obj is TSFQLAttribute)
                {
                    Attributes.Add(obj as TSFQLAttribute);
                }

                if (obj is ISyntaxEntity)
                {
                    SyntaxEntity = obj as ISyntaxEntity;
                }
            }
        }

        #region public Fields

        public List<TSFQLAttribute> Attributes = new List<TSFQLAttribute>();

        public ISyntaxEntity SyntaxEntity = null;

        #endregion
    }
}
