/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public enum WhereStateFunction
    {
        None = 0,
        Left = 1,
        Operator = 2,
        Right = 3,
        LBracket = 4,
        RBracket = 5,
        AndOr = 6,
        Quit = 7,
    }

    public enum LogicFunc
    {
        None = 0,
        And = 1,
        Or = 2,
    }

    class WhereState : SyntaxState<WhereStateFunction>
    {
        public WhereState(int id, bool isQuit, WhereStateFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == WhereStateFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public WhereState(int id, bool isQuit, WhereStateFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public WhereState(int id, bool isQuit) :
            this(id, isQuit, WhereStateFunction.None, null)
        {

        }

        public WhereState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, WhereStateFunction.None, nextStateIdDict)
        {

        }

        public WhereState(int id) :
            this(id, false, WhereStateFunction.None, null)
        {

        }

        public WhereState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, WhereStateFunction.None, nextStateIdDict)
        {

        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, WhereStateFunction> dfa)
        {
            Where where = dfa as Where;

            switch (Func)
            {
                case WhereStateFunction.LBracket:
                    where.LastLogicFunc = LogicFunc.None;

                    where.ExpressionTreeStack.Push(where.CurrentExpressionTree);
                    where.CurrentExpressionTree = new ExpressionTree(null);
                    break;
                case WhereStateFunction.RBracket:
                    where.LastLogicFunc = LogicFunc.None;

                    if (where.ExpressionTreeStack.Count <= 0)
                    {
                        throw new SyntaxException("Half-baked bracket", where,
                            new Hubble.Framework.DataStructure.DFAException("Half-baked bracket",
                                action, -1), dfa.CurrentToken);
                    }

                    ExpressionTree upTree = where.ExpressionTreeStack.Pop();

                    while (where.CurrentExpressionTree.Parent != null)
                    {
                        where.CurrentExpressionTree = where.CurrentExpressionTree.Parent;
                    }

                    upTree.Expression = where.CurrentExpressionTree;
                    where.CurrentExpressionTree = upTree;
                    break;
                case WhereStateFunction.Left:
                    if (where.CurrentExpressionTree.Expression == null)
                    {
                        where.CurrentExpressionTree.Expression = new Expression();
                    }

                    ((Expression)where.CurrentExpressionTree.Expression).Left.Add(dfa.CurrentToken);
                    break;
                case WhereStateFunction.Right:
                    ((Expression)where.CurrentExpressionTree.Expression).Right.Add(dfa.CurrentToken);
                    break;
                case WhereStateFunction.Operator:
                    ((Expression)where.CurrentExpressionTree.Expression).Operator = dfa.CurrentToken;
                    break;
                case WhereStateFunction.AndOr:
                    switch (dfa.CurrentToken.SyntaxType)
                    {
                        case SyntaxType.AND:
                            if (where.LastLogicFunc == LogicFunc.Or)
                            {
                                throw new SyntaxException("And Or can't in same short sentence without bracket. Eg. a > 0 and c < 10 or b > 0 must be a > 0 and c < 10 or (b > 0) ", 
                                    where, new Hubble.Framework.DataStructure.DFAException("Half-baked bracket",
                                        action, -1), dfa.CurrentToken);
                            }

                            where.LastLogicFunc = LogicFunc.And;
                            where.CurrentExpressionTree.AndChild = new ExpressionTree(where.CurrentExpressionTree);
                            where.CurrentExpressionTree = where.CurrentExpressionTree.AndChild;

                            break;

                        case SyntaxType.OR:
                            if (where.LastLogicFunc == LogicFunc.And)
                            {
                                throw new SyntaxException("And Or can't in same short sentence without bracket. Eg. a > 0 and c < 10 or b > 0 must be a > 0 and c < 10 or (b > 0) ",
                                    where, new Hubble.Framework.DataStructure.DFAException("Half-baked bracket",
                                        action, -1), dfa.CurrentToken);
                            }

                            where.LastLogicFunc = LogicFunc.Or;


                            where.CurrentExpressionTree.OrChild = new ExpressionTree(where.CurrentExpressionTree);
                            where.CurrentExpressionTree = where.CurrentExpressionTree.OrChild;
                            break;
                    }
                    break;
                case WhereStateFunction.Quit:
                    if (where.ExpressionTreeStack.Count > 0)
                    {
                        throw new SyntaxException("Half-baked bracket", where,
                            new Hubble.Framework.DataStructure.DFAException("Half-baked bracket",
                                action, -1), dfa.CurrentToken);
                    }
                    break;
            }
        }
    }

    public class Where : Syntax<WhereStateFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        public LogicFunc LastLogicFunc = LogicFunc.None;

        private static SyntaxState<WhereStateFunction> s0 = AddSyntaxState(new WhereState(0)); //Start state;
        private static SyntaxState<WhereStateFunction> squit = AddSyntaxState(new WhereState(30, true, WhereStateFunction.Quit)); //quit state;

        /*****************************************************
         * s0 -- where -- s1
         * s1 -- ( -- s5
         * s1 -- Identitier, Numeric, String -- s2
         * s2 -- Identitier, Arithmetic Operators, Numeric, String, [ , ] , ^ --s2
         * s2 -- Comparision operator -- s3
         * s3 -- Identitier, Numeric, String -- s4
         * s4 -- Order Group Eof -- squit
         * s4 -- Identitier, Arithmetic Operators, Numeric, String, [ , ] --s4
         * s4 -- ) --s6
         * s4 -- AND OR -- s7
         * s5 -- Identitier, Numeric, String -- s2
         * s5 -- ( -- s5
         * s5 -- ) -- s6
         * s6 -- AND OR -- s7
         * s6 --) --s6
         * s6 --Order Group Eof -- squit
         * s7 -- ( -- s5
         * s7 -- Identitier, Numeric, String -- s2
         * **************************************************/
        private static void InitDFAStates()
        {
            SyntaxState<WhereStateFunction> s1 = AddSyntaxState(new WhereState(1)); //start state;
            SyntaxState<WhereStateFunction> s2 = AddSyntaxState(new WhereState(2, false, WhereStateFunction.Left));
            SyntaxState<WhereStateFunction> s3 = AddSyntaxState(new WhereState(3, false, WhereStateFunction.Operator));
            SyntaxState<WhereStateFunction> s4 = AddSyntaxState(new WhereState(4, false, WhereStateFunction.Right));
            SyntaxState<WhereStateFunction> s5 = AddSyntaxState(new WhereState(5, false, WhereStateFunction.LBracket));
            SyntaxState<WhereStateFunction> s6 = AddSyntaxState(new WhereState(6, false, WhereStateFunction.RBracket));
            SyntaxState<WhereStateFunction> s7 = AddSyntaxState(new WhereState(7, false, WhereStateFunction.AndOr));


            s0.AddNextState((int)SyntaxType.WHERE, s1.Id);
            
            s1.AddNextState((int)SyntaxType.LBracket, s5.Id);
            s1.AddNextState(new int[] { (int)SyntaxType.Identifer, 
                (int)SyntaxType.Numeric, (int)SyntaxType.String }, s2.Id);

            s2.AddNextState(new int[] { (int)SyntaxType.Identifer, 
                (int)SyntaxType.Numeric, (int)SyntaxType.String, (int)SyntaxType.LSquareBracket, 
                (int)SyntaxType.RSquareBracket, (int)SyntaxType.Up }, s2.Id);
            s2.AddNextState((int)SyntaxType.Plus, (int)SyntaxType.Mod, s2.Id);
            s2.AddNextState((int)SyntaxType.NotEqual, (int)SyntaxType.LargethanEqual, s3.Id);
            s2.AddNextState(new int[] { (int)SyntaxType.LIKE, (int)SyntaxType.MATCH, (int)SyntaxType.CONTAINS,
                (int)SyntaxType.CONTAINS1, (int)SyntaxType.CONTAINS2, (int)SyntaxType.CONTAINS3} , s3.Id);

            s3.AddNextState(new int[] { (int)SyntaxType.Identifer, 
                (int)SyntaxType.Numeric, (int)SyntaxType.String }, s4.Id);

            s4.AddNextState(new int[] { (int)SyntaxType.GROUP, 
                (int)SyntaxType.ORDER, (int)SyntaxType.Eof }, squit.Id);
            s4.AddNextState(new int[] { (int)SyntaxType.Identifer, 
                (int)SyntaxType.Numeric, (int)SyntaxType.String, (int)SyntaxType.LSquareBracket, (int)SyntaxType.RSquareBracket }, s4.Id);
            s4.AddNextState((int)SyntaxType.Plus, (int)SyntaxType.Mod, s4.Id);
            s4.AddNextState(new int[] { (int)SyntaxType.AND, (int)SyntaxType.OR }, s7.Id);
            s4.AddNextState((int)SyntaxType.RBracket, s6.Id);

            s5.AddNextState(new int[] { (int)SyntaxType.Identifer, 
                (int)SyntaxType.Numeric, (int)SyntaxType.String }, s2.Id);
            s5.AddNextState((int)SyntaxType.LBracket, s5.Id);
            s5.AddNextState((int)SyntaxType.RBracket, s6.Id);

            s6.AddNextState(new int[] { (int)SyntaxType.AND, (int)SyntaxType.OR }, s7.Id);
            s6.AddNextState((int)SyntaxType.RBracket, s6.Id);
            s6.AddNextState(new int[] { (int)SyntaxType.GROUP, 
                (int)SyntaxType.ORDER, (int)SyntaxType.Eof }, squit.Id);

            s7.AddNextState((int)SyntaxType.LBracket, s5.Id);
            s7.AddNextState(new int[] { (int)SyntaxType.Identifer, 
                (int)SyntaxType.Numeric, (int)SyntaxType.String }, s2.Id);

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

        public Where()
        {
            CurrentExpressionTree = ExpressionTree;
        }

        public Stack<ExpressionTree> ExpressionTreeStack = new Stack<ExpressionTree>();
        public ExpressionTree CurrentExpressionTree;

        #region public Fields

        public ExpressionTree ExpressionTree = new ExpressionTree(null);

        #endregion

    }
}


