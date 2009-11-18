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
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.SFQL.LexicalAnalysis
{
    public enum LexicalFunction
    {
        None = 0,
        OutputIdentifier = 1,
        DoSpace = 2,
        OutputSpace = 3,
        OutputNumeric = 4,
        OutputComment = 5,
        OutputArithmeticOpr = 6,
        DoubleQuotation = 7,
        OutputString = 8,
        OutputComparisonOpr = 9,
        OutputBracketSymbol = 10,
    }

    class LexicalState : DFAState<int, LexicalFunction>
    {
        public LexicalState(int id, bool isQuit, LexicalFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == LexicalFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public LexicalState(int id, bool isQuit, LexicalFunction function):
            this(id, isQuit, function, null)
        {

        }

        public LexicalState(int id, bool isQuit) :
            this(id, isQuit, LexicalFunction.None, null)
        {

        }

        public LexicalState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, LexicalFunction.None, nextStateIdDict)
        {

        }

        public LexicalState(int id) :
            this(id, false, LexicalFunction.None, null)
        {

        }

        public LexicalState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, LexicalFunction.None, nextStateIdDict)
        {

        }

        private void GetTextElse(DFA<int, LexicalFunction> dfa)
        {
            int endIndex = dfa.CurrentToken;

            Lexical lexical = (Lexical)dfa;

            lexical.OutputToken.Row = lexical.Row;
            lexical.OutputToken.Col = lexical.beginIndex - lexical.LastLineStart;
            lexical.OutputToken.Text = lexical.InputText.Substring(lexical.beginIndex, endIndex - lexical.beginIndex);

            lexical.beginIndex = endIndex;
        }

        private void GetText(DFA<int, LexicalFunction> dfa)
        {
            int endIndex = dfa.CurrentToken;

            Lexical lexical = (Lexical)dfa;
            
            lexical.OutputToken.Row = lexical.Row;
            lexical.OutputToken.Col = lexical.beginIndex - lexical.LastLineStart;

            if (endIndex == lexical.InputText.Length)
            {
                lexical.OutputToken.Text = lexical.InputText.Substring(lexical.beginIndex,
                    endIndex - lexical.beginIndex);
            }
            else
            {
                lexical.OutputToken.Text = lexical.InputText.Substring(lexical.beginIndex,
                    endIndex - lexical.beginIndex + 1);
            }

            lexical.beginIndex = endIndex + 1;
        }

        private void GetString(DFA<int, LexicalFunction> dfa)
        {
            int endIndex = dfa.CurrentToken;

            Lexical lexical = (Lexical)dfa;

            lexical.OutputToken.Row = lexical.Row;
            lexical.OutputToken.Col = lexical.beginIndex - lexical.LastLineStart;
            lexical.OutputToken.Text = lexical.InputText.Substring(lexical.beginIndex + 1, endIndex - lexical.beginIndex - 2);

            if (lexical.DoubleQuotationCount > 0)
            {
                lexical.OutputToken.Text = lexical.OutputToken.Text.Replace("''", "'");
                lexical.DoubleQuotationCount = 0;
            }

            lexical.beginIndex = endIndex;
        }

        #region Dothings
        public override void DoThings(int action, DFA<int, LexicalFunction> dfa)
        {
            Lexical lexical = (Lexical)dfa;

            switch (Func)
            {
                case LexicalFunction.OutputIdentifier:
                    lexical.OutputToken = new Lexical.Token();
                    GetTextElse(dfa);
                    if (!Lexical.KeywordsTbl.TryGetValue(lexical.OutputToken.Text.ToUpper(),
                        out lexical.OutputToken.SyntaxType))
                    {
                        lexical.OutputToken.SyntaxType = SyntaxType.Identifer;
                    }

                    break;
                case LexicalFunction.DoSpace:
                    DoSpace(action, dfa);
                    break;
                case LexicalFunction.OutputSpace:
                    lexical.OutputToken = new Lexical.Token();
                    GetTextElse(dfa);
                    lexical.OutputToken.SyntaxType = SyntaxType.Space;
                    break;
                case LexicalFunction.OutputNumeric:
                    lexical.OutputToken = new Lexical.Token();
                    GetTextElse(dfa);
                    lexical.OutputToken.SyntaxType = SyntaxType.Numeric;
                    break;
                case LexicalFunction.OutputComment:
                    lexical.OutputToken = new Lexical.Token();
                    GetText(dfa);
                    if (lexical.OutputToken.Text[0] == '-')
                    {
                        lexical.Row++;
                        lexical.LastLineStart = lexical.CurrentToken + 1;
                    }

                    lexical.OutputToken.SyntaxType = SyntaxType.Comment;
                    break;
                case LexicalFunction.OutputArithmeticOpr:
                    lexical.OutputToken = new Lexical.Token();
                    GetTextElse(dfa);
                    lexical.OutputToken.SyntaxType = Lexical.ArithmeticTbl[lexical.OutputToken.Text];

                    break;
                case LexicalFunction.DoubleQuotation:
                    lexical.DoubleQuotationCount++;
                    break;
                case LexicalFunction.OutputString:
                    lexical.OutputToken = new Lexical.Token();
                    GetString(dfa);
                    lexical.OutputToken.SyntaxType = SyntaxType.String;
                    break;
                case LexicalFunction.OutputComparisonOpr:
                    lexical.OutputToken = new Lexical.Token();
                    GetTextElse(dfa);
                    lexical.OutputToken.SyntaxType = Lexical.ComparisonOprTbl[lexical.OutputToken.Text];
                    break;
                case LexicalFunction.OutputBracketSymbol:
                    lexical.OutputToken = new Lexical.Token();
                    GetText(dfa);
                    lexical.OutputToken.SyntaxType = Lexical.BracketSymbolTbl[lexical.OutputToken.Text];
                    break;
            }
        }

        private void DoSpace(int action, DFA<int, LexicalFunction> dfa)
        {
            if (action == '\n')
            {
                Lexical lexical = (Lexical)dfa;
                lexical.Row++;
                lexical.LastLineStart = lexical.CurrentToken + 1;
            }

        }

        #endregion
    }

    public class Lexical:DFA<int, LexicalFunction>
    {
        public class Token
        {
            public string Text;
            public SyntaxAnalysis.SyntaxType SyntaxType;
            public int Row;
            public int Col;
        }

        public int beginIndex = 0;

        public string InputText = null;
        public Token OutputToken = null;

        //Row and LastLineStart using for Row and Col
        public int Row = 0;
        public int LastLineStart = 0;

        //For string
        public int DoubleQuotationCount = 0;

        private static DFAState<int, LexicalFunction> s0 = AddState(new LexicalState(0)); //Start state;

        public static Dictionary<string, SyntaxType> ArithmeticTbl = new Dictionary<string, SyntaxType>();
        public static Dictionary<string, SyntaxType> ComparisonOprTbl = new Dictionary<string, SyntaxType>();
        public static Dictionary<string, SyntaxType> BracketSymbolTbl = new Dictionary<string, SyntaxType>();
        public static Dictionary<string, SyntaxType> KeywordsTbl = new Dictionary<string, SyntaxType>();
        

        private static void InitIdentifierStates()
        {
            KeywordsTbl.Add("OR", SyntaxType.OR);
            KeywordsTbl.Add("AND", SyntaxType.AND);
            KeywordsTbl.Add("NOT", SyntaxType.NOT);

            for(int i = (int)SyntaxType.BEGIN_KEYWORD + 1; i < (int)SyntaxType.END_KEYWORD; i++)
            {
                KeywordsTbl.Add(((SyntaxType)i).ToString(), (SyntaxType)i);
            }

            DFAState<int, LexicalFunction> s1 = AddState(new LexicalState(1)); //Identifier begin state;
            DFAState<int, LexicalFunction> s2 = AddState(new LexicalState(2, true, LexicalFunction.OutputIdentifier)); //Identifier quit state;

            //s0 [_a-zA-Z] s1
            s0.AddNextState('_', s1.Id);
            s0.AddNextState('a', 'z', s1.Id);
            s0.AddNextState('A', 'Z', s1.Id);

            //s1 [_a-zA-Z0-9] s1
            s1.AddNextState('_', s1.Id);
            s1.AddNextState('a', 'z', s1.Id);
            s1.AddNextState('A', 'Z', s1.Id);
            s1.AddNextState('0', '9', s1.Id);

            //s1 ^[_a-zA-Z0-9] s2
            s1.AddElseState(s2.Id);
        }

        private static void InitSpaceStates()
        {
            DFAState<int, LexicalFunction> s3 = AddState(new LexicalState(3, false, LexicalFunction.DoSpace)); //Space begin state;
            DFAState<int, LexicalFunction> s4 = AddState(new LexicalState(4, true, LexicalFunction.OutputSpace)); //Space quit state;

            //s0 [ \t\r\n] s3
            s0.AddNextState(new int[]{' ', '\t', '\r', '\n'}, s3.Id);

            //s3 [ \t\r\n] s3
            s3.AddNextState(new int[] { ' ', '\t', '\r', '\n' }, s3.Id);

            //s3 ^[ \t\r\n] s4
            s3.AddElseState(s4.Id);
        }

        private static void InitNumericStates()
        {
            DFAState<int, LexicalFunction> s5 = AddState(new LexicalState(5, false)); //Numeric begin state;
            DFAState<int, LexicalFunction> s6 = AddState(new LexicalState(6, false)); //Number dot state;
            DFAState<int, LexicalFunction> s7 = AddState(new LexicalState(7, true, LexicalFunction.OutputNumeric)); //Number quit state;

            //s0 [0-9] s5
            s0.AddNextState('0', '9', s5.Id);

            //s5 [0-9] s5
            s5.AddNextState('0', '9', s5.Id);
            //s5 [\.] s6
            s5.AddNextState('.', s6.Id);

            //s5 else s7 (integer)
            s5.AddElseState(s7.Id);

            //s6 [0-9] s6
            s6.AddNextState('0', '9', s6.Id);

            //s6 else s7 (float)
            s6.AddElseState(s7.Id);

        }

        private static void InitCommentAndArithmeticStates()
        {
            ArithmeticTbl.Add("+", SyntaxType.Plus);
            ArithmeticTbl.Add("-", SyntaxType.Subtract);
            ArithmeticTbl.Add("*", SyntaxType.Multiply);
            ArithmeticTbl.Add("/", SyntaxType.Divide);

            DFAState<int, LexicalFunction> s8 = AddState(new LexicalState(8, false)); //Comment / state;
            DFAState<int, LexicalFunction> s9 = AddState(new LexicalState(9, false)); //Comment * start state;
            DFAState<int, LexicalFunction> s10 = AddState(new LexicalState(10, false)); //Comment * end state;
            DFAState<int, LexicalFunction> s11 = AddState(new LexicalState(11, false)); //Comment - first state;
            DFAState<int, LexicalFunction> s12 = AddState(new LexicalState(12, false)); //Comment - second state;
            DFAState<int, LexicalFunction> s13 = AddState(new LexicalState(13, true, LexicalFunction.OutputComment)); //Comment quit state;

            DFAState<int, LexicalFunction> s14 = AddState(new LexicalState(14, false)); //Arithmetic begin state;
            DFAState<int, LexicalFunction> s15 = AddState(new LexicalState(15, true, LexicalFunction.OutputArithmeticOpr)); //Arithmetic quit state;

            //s0 [\/] s8
            s0.AddNextState('/', s8.Id);

            //s8 [\*] s9
            s8.AddNextState('*', s9.Id);
            s8.AddElseState(s15.Id);

            //s9 [\*] s10
            s9.AddNextState('*', s10.Id);
            //s9 else s9
            s9.AddElseState(s9.Id);

            //s10 [\/] s13
            s10.AddNextState('/', s13.Id);
            //s10 else s9
            s10.AddElseState(s9.Id);

            //s0 [-] s11
            s0.AddNextState('-', s11.Id);

            //s11 [-] s12
            s11.AddNextState('-', s12.Id);
            s11.AddElseState(s15.Id);

            //s12 [\n] s13
            s12.AddNextState(new int[] {0, '\n'}, s13.Id);
            s12.AddElseState(s12.Id);

            //s0 [\+\*] s14
            s0.AddNextState(new int[] { '+', '*' }, s14.Id);
            s14.AddElseState(s15.Id);

        }


        private static void InitStringStates()
        {
            DFAState<int, LexicalFunction> s16 = AddState(new LexicalState(16, false)); // \' start state;
            DFAState<int, LexicalFunction> s17 = AddState(new LexicalState(17, false)); // \' second start state;
            DFAState<int, LexicalFunction> s18 = AddState(new LexicalState(18, false, LexicalFunction.DoubleQuotation)); // \' third state;
            DFAState<int, LexicalFunction> s19 = AddState(new LexicalState(19, true, LexicalFunction.OutputString)); //String quit state;

            //s0 [\'] s16
            s0.AddNextState('\'', s16.Id);

            //s16 [\'] s17
            s16.AddNextState('\'', s17.Id);
            s16.AddElseState(s16.Id);

            //s17 [\'] s18
            s17.AddNextState('\'', s18.Id);
            s17.AddElseState(s19.Id);

            //s18 [\'] s17
            s18.AddNextState('\'', s17.Id);
            s18.AddElseState(s16.Id);

        }

        private static void InitComparisonOprStates()
        {
            ComparisonOprTbl.Add("<", SyntaxType.Lessthan);
            ComparisonOprTbl.Add("<=", SyntaxType.LessthanEqual);
            ComparisonOprTbl.Add(">", SyntaxType.Largethan);
            ComparisonOprTbl.Add(">=", SyntaxType.LargethanEqual);
            ComparisonOprTbl.Add("<>", SyntaxType.NotEqual);
            ComparisonOprTbl.Add("!=", SyntaxType.NotEqual);
            ComparisonOprTbl.Add("=", SyntaxType.Equal);

            DFAState<int, LexicalFunction> s20 = AddState(new LexicalState(20, false)); // < start state;
            DFAState<int, LexicalFunction> s21 = AddState(new LexicalState(21, false)); // <= <> >= != state;
            DFAState<int, LexicalFunction> s22 = AddState(new LexicalState(22, false)); // ! state;
            DFAState<int, LexicalFunction> s23 = AddState(new LexicalState(23, false)); // > state;
            DFAState<int, LexicalFunction> s24 = AddState(new LexicalState(24, true, LexicalFunction.OutputComparisonOpr)); // quit state;

            //s0 [<] s20
            s0.AddNextState('<', s20.Id);

            //s20 [=<] s21
            s20.AddNextState(new int[] { '=', '>' }, s21.Id);
            s20.AddElseState(s24.Id);

            s21.AddElseState(s24.Id);

            //s0 [!] s22
            s0.AddNextState('!', s22.Id);

            //s22 [=] s21
            s22.AddNextState('=', s21.Id);

            s0.AddNextState('>', s23.Id);

            s23.AddNextState('=', s21.Id);
            s23.AddElseState(s24.Id);

            s0.AddNextState('=', s21.Id);

        }

        private static void InitBracketSymbolStates()
        {
            BracketSymbolTbl.Add("(", SyntaxType.LBracket);
            BracketSymbolTbl.Add(")", SyntaxType.RBracket);
            BracketSymbolTbl.Add("[", SyntaxType.LSquareBracket);
            BracketSymbolTbl.Add("]", SyntaxType.RSquareBracket);
            BracketSymbolTbl.Add("^", SyntaxType.Up);
            BracketSymbolTbl.Add(",", SyntaxType.Comma);
            BracketSymbolTbl.Add(".", SyntaxType.Dot);
            BracketSymbolTbl.Add(";", SyntaxType.Semicolon);
            BracketSymbolTbl.Add(":", SyntaxType.Colon);
            BracketSymbolTbl.Add("@", SyntaxType.At);


            DFAState<int, LexicalFunction> s25 = AddState(new LexicalState(25, true, LexicalFunction.OutputBracketSymbol)); // quit state;

            s0.AddNextState(new int[] { '(', ')', '[', ']', '^', ',', '.', ';', ':', '@' }, s25.Id);
        }

        private static void InitDFAStates()
        {
            InitIdentifierStates();
            InitSpaceStates();
            InitNumericStates();
            InitCommentAndArithmeticStates();
            InitStringStates();
            InitComparisonOprStates();
            InitBracketSymbolStates();
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

        public Lexical(string inputText)
        {
            InputText = inputText;
        }

        new public DFAResult Input(int action, int token)
        {
            try
            {
                return base.Input(action, token);
            }
            catch (DFAException dfaEx)
            {
                throw new LexicalException(dfaEx.Message, dfaEx, Row, beginIndex - LastLineStart);
            }
        }
    }
}
