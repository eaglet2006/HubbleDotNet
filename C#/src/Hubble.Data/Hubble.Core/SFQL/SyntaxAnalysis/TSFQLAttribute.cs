using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public enum TSFQLAttrFunction
    {
        None = 0,
        Name = 1,
        Parameter = 2,
    }


    class TSFQLAttrState : SyntaxState<TSFQLAttrFunction>
    {
        public TSFQLAttrState(int id, bool isQuit, TSFQLAttrFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == TSFQLAttrFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public TSFQLAttrState(int id, bool isQuit, TSFQLAttrFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public TSFQLAttrState(int id, bool isQuit) :
            this(id, isQuit, TSFQLAttrFunction.None, null)
        {

        }

        public TSFQLAttrState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, TSFQLAttrFunction.None, nextStateIdDict)
        {

        }

        public TSFQLAttrState(int id) :
            this(id, false, TSFQLAttrFunction.None, null)
        {

        }

        public TSFQLAttrState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, TSFQLAttrFunction.None, nextStateIdDict)
        {
            
        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, TSFQLAttrFunction> dfa)
        {
            TSFQLAttribute sfqlAttr = dfa as TSFQLAttribute;

            switch (Func)
            {
                case TSFQLAttrFunction.Name:
                    sfqlAttr.Name = dfa.CurrentToken.Text;
                    break;
                case TSFQLAttrFunction.Parameter:
                    sfqlAttr.Parameters.Add(dfa.CurrentToken.Text);
                    break;
            }
        }
    }

    public class TSFQLAttribute : Syntax<TSFQLAttrFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<TSFQLAttrFunction> s0 = AddSyntaxState(new TSFQLAttrState(0)); //Start state;
        private static SyntaxState<TSFQLAttrFunction> squit = AddSyntaxState(new TSFQLAttrState(30, true)); //quit state;

        /*****************************************************
         * s0 -- [ -- s1
         * s1 -- Identitier -- s2
         * s2 -- ] -- squit
         * s2 -- ( -- s3
         * s3 -- String, Numeric -- s4
         * s4 -- ) -- s5
         * s4 -- , -- s3
         * s5 -- ] -- squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<TSFQLAttrFunction> s1 = AddSyntaxState(new TSFQLAttrState(1)); //Attribute start state;
            SyntaxState<TSFQLAttrFunction> s2 = AddSyntaxState(new TSFQLAttrState(2, false, TSFQLAttrFunction.Name)); 
            SyntaxState<TSFQLAttrFunction> s3 = AddSyntaxState(new TSFQLAttrState(3)); 
            SyntaxState<TSFQLAttrFunction> s4 = AddSyntaxState(new TSFQLAttrState(4, false, TSFQLAttrFunction.Parameter));
            SyntaxState<TSFQLAttrFunction> s5 = AddSyntaxState(new TSFQLAttrState(5)); 

            s0.AddNextState((int)SyntaxType.LSquareBracket, s1.Id);

            s1.AddNextState((int)SyntaxType.Identifer, s2.Id);
            s2.AddNextState((int)SyntaxType.RSquareBracket, squit.Id);
            s2.AddNextState((int)SyntaxType.LBracket, s3.Id);
            s3.AddNextState(new int[]{(int)SyntaxType.String, (int)SyntaxType.Numeric}, s4.Id);
            s4.AddNextState((int)SyntaxType.RBracket, s5.Id);
            s4.AddNextState((int)SyntaxType.Comma, s3.Id);
            s5.AddNextState((int)SyntaxType.RSquareBracket, squit.Id);
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

        public List<string> Parameters = new List<string>();

        #endregion


    }
}
