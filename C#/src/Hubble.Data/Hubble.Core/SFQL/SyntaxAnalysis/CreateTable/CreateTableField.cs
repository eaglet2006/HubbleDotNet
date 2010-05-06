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

namespace Hubble.Core.SFQL.SyntaxAnalysis.CreateTable
{
    public enum CreateTableFieldFunction
    {
        None = 0,
        Name = 1,
        Type = 2,
        Length = 3,
        Tokenized = 4,
        Analyzer = 5,
        NULL = 6,
        NOTNULL = 7,
        DEAFAULT = 8,
        PRIMARYKEY = 9,
        FASTEST = 10,
    }

    class CreateTableFieldState : SyntaxState<CreateTableFieldFunction>
    {
        public CreateTableFieldState(int id, bool isQuit, CreateTableFieldFunction function, IDictionary<int, int> nextStateIdDict)
        {
            m_Id = id;
            IsQuitState = isQuit;
            Func = function;

            NoFunction = Func == CreateTableFieldFunction.None;
            NextStateIdDict = nextStateIdDict;
        }

        public CreateTableFieldState(int id, bool isQuit, CreateTableFieldFunction function) :
            this(id, isQuit, function, null)
        {

        }

        public CreateTableFieldState(int id, bool isQuit) :
            this(id, isQuit, CreateTableFieldFunction.None, null)
        {

        }

        public CreateTableFieldState(int id, bool isQuit, IDictionary<int, int> nextStateIdDict) :
            this(id, isQuit, CreateTableFieldFunction.None, nextStateIdDict)
        {

        }

        public CreateTableFieldState(int id) :
            this(id, false, CreateTableFieldFunction.None, null)
        {

        }

        public CreateTableFieldState(int id, IDictionary<int, int> nextStateIdDict) :
            this(id, false, CreateTableFieldFunction.None, nextStateIdDict)
        {

        }

        private Data.DataType GetDataType(string strDataType)
        {
            string lowerDataType = strDataType.ToLower();

            switch (lowerDataType)
            {
                case "tinyint":
                    return Hubble.Core.Data.DataType.TinyInt;
                case "smallint":
                    return Hubble.Core.Data.DataType.SmallInt;
                case "int":
                    return Hubble.Core.Data.DataType.Int;
                case "bigint":
                    return Hubble.Core.Data.DataType.BigInt;
                case "float":
                    return Hubble.Core.Data.DataType.Float;
                case "date":
                    return Hubble.Core.Data.DataType.Date;
                case "smalldatetime":
                    return Hubble.Core.Data.DataType.SmallDateTime;
                case "datetime":
                    return Hubble.Core.Data.DataType.DateTime;
                case "varchar":
                    return Hubble.Core.Data.DataType.Varchar;
                case "nvarchar":
                    return Hubble.Core.Data.DataType.NVarchar;
                case "char":
                    return Hubble.Core.Data.DataType.Char;
                case "nchar":
                    return Hubble.Core.Data.DataType.NChar;
                case "data":
                    return Hubble.Core.Data.DataType.Data;
                default:
                    throw new SyntaxException(string.Format("Unknown data type:{0}", strDataType));
            }

        }

        private Data.Field.Index GetIndexType(SyntaxType syntaxType)
        {
            switch (syntaxType)
            {
                case SyntaxType.NONE:
                    return Hubble.Core.Data.Field.Index.None;
                case SyntaxType.TOKENIZED:
                    return Hubble.Core.Data.Field.Index.Tokenized;
                case SyntaxType.UNTOKENIZED:
                    return Hubble.Core.Data.Field.Index.Untokenized;
                default:
                    return Hubble.Core.Data.Field.Index.None;
            }
        }

        public override void DoThings(int action, Hubble.Framework.DataStructure.DFA<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token, CreateTableFieldFunction> dfa)
        {
            switch (Func)
            {
                case CreateTableFieldFunction.Name:
                    ((CreateTableField)dfa).FieldName = dfa.CurrentToken.Text;
                    break;
                case CreateTableFieldFunction.Type:
                    ((CreateTableField)dfa).DataType = GetDataType(dfa.CurrentToken.Text);
                    break;
                case CreateTableFieldFunction.Length:
                    if (dfa.CurrentToken.SyntaxType == SyntaxType.MAX)
                    {
                        ((CreateTableField)dfa).DataLength = -1;
                    }
                    else
                    {
                        ((CreateTableField)dfa).DataLength = int.Parse(dfa.CurrentToken.Text);
                    }
                    break;
                case CreateTableFieldFunction.Tokenized:
                    ((CreateTableField)dfa).IndexType = GetIndexType(dfa.CurrentToken.SyntaxType);
                    break;
                case CreateTableFieldFunction.FASTEST:
                    ((CreateTableField)dfa).IndexMode = Hubble.Core.Data.Field.IndexMode.Simple;
                    break;
                case CreateTableFieldFunction.Analyzer:
                    ((CreateTableField)dfa).AnalyzerName = dfa.CurrentToken.Text;
                    break;
                case CreateTableFieldFunction.DEAFAULT:
                    ((CreateTableField)dfa).Default = dfa.CurrentToken.Text;
                    break;
                case CreateTableFieldFunction.NULL:
                    ((CreateTableField)dfa).CanNull = true;
                    break;
                case CreateTableFieldFunction.NOTNULL:
                    ((CreateTableField)dfa).CanNull = false;
                    break;
                case CreateTableFieldFunction.PRIMARYKEY:
                    ((CreateTableField)dfa).PrimaryKey = true;
                    break;
            }
        }
    }


    class CreateTableField : Syntax<CreateTableFieldFunction>, Hubble.Core.SFQL.SyntaxAnalysis.ITokenInput
    {
        private static SyntaxState<CreateTableFieldFunction> s0 = AddSyntaxState(new CreateTableFieldState(0)); //Start state;
        private static SyntaxState<CreateTableFieldFunction> squit = AddSyntaxState(new CreateTableFieldState(30, true)); //quit state;

        /******************************************************
         * s0 --(,-- s1
         * s1 --Identifer, string--s2 //Field name
         * s2 --Identifer--s3 //Type
         * s3 --(--s4 //length
         * s3 --Tokenized UnTokenized None--s7
         * s3 --Analyzer--s8
         * s3 --NULL--s10 
         * s3 --NOT --s11
         * s3 --DEFAULT --s13
         * s3 --PRIMARY --s15
         * s3 --FASTEST --s17
         * s3 --,)--squit
         * 
         * s4 --Numeric MAX-- s5 //length
         * s5 --)-- s6 //length
         * s6 --Tokenized UnTokenized None--s7
         * s6 --Analyzer--s8
         * s6 --NULL--s10 
         * s6 --NOT --s11
         * s6 --DEFAULT --s13
         * s6 --PRIMARY --s15
         * s6 --FASTEST --s17
         * s6 --,)--squit

         * s7 --Analyzer--s8
         * s7 --NULL--s10 
         * s7 --NOT --s11
         * s7 --DEFAULT --s13
         * s7 --PRIMARY --s15
         * s7 --,)--squit

         * s8 --String-- s9
         * s9 --Tokenized UnTokenized None--s7
         * s9 --NULL--s10 
         * s9 --NOT --s11
         * s9 --DEFAULT --s13
         * s9 --PRIMARY --s15
         * s9 --FASTEST --s17
         * s9 --,)--squit
          
         * s10 --Tokenized UnTokenized None--s7
         * s10 --Analyzer--s8
         * s10 --DEFAULT --s13
         * s10 --PRIMARY --s15
         * s10 --FASTEST --s17
         * s10 --,)--squit
         
         * s11 --NULL--s12
         * s12 --Tokenized UnTokenized None--s7
         * s12 --Analyzer--s8
         * s12 --DEFAULT --s13
         * s12 --PRIMARY --s15
         * s12 --FASTEST --s17
         * s12 --,)--squit

         * s13 --String Numeric --s14
         * s14 --Tokenized UnTokenized None--s7
         * s14 --Analyzer--s8
         * s14 --NULL--s10 
         * s14 --NOT --s11
         * s14 --PRIMARY --s15
         * s14 --,)--squit
         * s14 --FASTEST --s17
         * 
         * s15 --KEY--s16
         * s16 --Tokenized UnTokenized None--s7
         * s16 --Analyzer--s8
         * s16 --NULL--s10 
         * s16 --NOT --s11
         * s16 --DEFAULT --s13
         * s16 --FASTEST --s17         
         * s16 --,)--squit
         * 
         * s17 --Tokenized UnTokenized None--s7
         * s17 --Analyzer--s8
         * s17 --NULL--s10 
         * s17 --NOT --s11
         * s17 --DEFAULT --s13
         * s17 --PRIMARY --s15
         * s17 --,)--squit
         * **************************************************/

        private static void InitDFAStates()
        {
            SyntaxState<CreateTableFieldFunction> s1 = AddSyntaxState(new CreateTableFieldState(1));
            SyntaxState<CreateTableFieldFunction> s2 = AddSyntaxState(new CreateTableFieldState(2, false, CreateTableFieldFunction.Name));
            SyntaxState<CreateTableFieldFunction> s3 = AddSyntaxState(new CreateTableFieldState(3, false, CreateTableFieldFunction.Type)); 
            SyntaxState<CreateTableFieldFunction> s4 = AddSyntaxState(new CreateTableFieldState(4));
            SyntaxState<CreateTableFieldFunction> s5 = AddSyntaxState(new CreateTableFieldState(5, false, CreateTableFieldFunction.Length));
            SyntaxState<CreateTableFieldFunction> s6 = AddSyntaxState(new CreateTableFieldState(6));
            SyntaxState<CreateTableFieldFunction> s7 = AddSyntaxState(new CreateTableFieldState(7, false, CreateTableFieldFunction.Tokenized));
            SyntaxState<CreateTableFieldFunction> s8 = AddSyntaxState(new CreateTableFieldState(8));
            SyntaxState<CreateTableFieldFunction> s9 = AddSyntaxState(new CreateTableFieldState(9, false, CreateTableFieldFunction.Analyzer));
            SyntaxState<CreateTableFieldFunction> s10 = AddSyntaxState(new CreateTableFieldState(10, false, CreateTableFieldFunction.NULL));
            SyntaxState<CreateTableFieldFunction> s11 = AddSyntaxState(new CreateTableFieldState(11));
            SyntaxState<CreateTableFieldFunction> s12 = AddSyntaxState(new CreateTableFieldState(12, false, CreateTableFieldFunction.NOTNULL));
            SyntaxState<CreateTableFieldFunction> s13 = AddSyntaxState(new CreateTableFieldState(13));
            SyntaxState<CreateTableFieldFunction> s14 = AddSyntaxState(new CreateTableFieldState(14, false, CreateTableFieldFunction.DEAFAULT));
            SyntaxState<CreateTableFieldFunction> s15 = AddSyntaxState(new CreateTableFieldState(15, false));
            SyntaxState<CreateTableFieldFunction> s16 = AddSyntaxState(new CreateTableFieldState(16, false, CreateTableFieldFunction.PRIMARYKEY));
            SyntaxState<CreateTableFieldFunction> s17 = AddSyntaxState(new CreateTableFieldState(17, false, CreateTableFieldFunction.FASTEST));

            s0.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.LBracket }, s1.Id);

            s1.AddNextState(new int[] {(int)SyntaxType.Identifer, (int)SyntaxType.String}, s2.Id);

            s2.AddNextState((int)SyntaxType.Identifer, s3.Id);

            s3.AddNextState((int)SyntaxType.LBracket, s4.Id);
            s3.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s3.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s3.AddNextState((int)SyntaxType.NULL, s10.Id);
            s3.AddNextState((int)SyntaxType.NOT, s11.Id);
            s3.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s3.AddNextState((int)SyntaxType.PRIMARY, s15.Id);
            s3.AddNextState((int)SyntaxType.FASTEST, s17.Id);

            s3.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);

            s4.AddNextState((int)SyntaxType.Numeric, s5.Id);
            s4.AddNextState((int)SyntaxType.MAX, s5.Id);

            s5.AddNextState((int)SyntaxType.RBracket, s6.Id);

            s6.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s6.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s6.AddNextState((int)SyntaxType.NULL, s10.Id);
            s6.AddNextState((int)SyntaxType.NOT, s11.Id);
            s6.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s6.AddNextState((int)SyntaxType.PRIMARY, s15.Id);
            s6.AddNextState((int)SyntaxType.FASTEST, s17.Id);
            s6.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);

            s7.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s7.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s7.AddNextState((int)SyntaxType.NULL, s10.Id);
            s7.AddNextState((int)SyntaxType.NOT, s11.Id);
            s7.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s7.AddNextState((int)SyntaxType.PRIMARY, s15.Id);

            s8.AddNextState((int)SyntaxType.String, s9.Id);

            s9.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s9.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s9.AddNextState((int)SyntaxType.NULL, s10.Id);
            s9.AddNextState((int)SyntaxType.NOT, s11.Id);
            s9.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s9.AddNextState((int)SyntaxType.PRIMARY, s15.Id);
            s9.AddNextState((int)SyntaxType.FASTEST, s17.Id);

            s10.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s10.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s10.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s10.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s10.AddNextState((int)SyntaxType.PRIMARY, s15.Id);
            s10.AddNextState((int)SyntaxType.FASTEST, s17.Id);

            s11.AddNextState((int)SyntaxType.NULL, s12.Id);

            s12.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s12.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s12.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s12.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s12.AddNextState((int)SyntaxType.PRIMARY, s15.Id);
            s12.AddNextState((int)SyntaxType.FASTEST, s17.Id);

            s13.AddNextState(new int[] {(int)SyntaxType.String, (int)SyntaxType.Numeric}, s14.Id);

            s14.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s14.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s14.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s14.AddNextState((int)SyntaxType.NULL, s10.Id);
            s14.AddNextState((int)SyntaxType.NOT, s11.Id);
            s14.AddNextState((int)SyntaxType.PRIMARY, s15.Id);
            s14.AddNextState((int)SyntaxType.FASTEST, s17.Id);

            s15.AddNextState((int)SyntaxType.KEY, s16.Id);

            s16.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s16.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s16.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s16.AddNextState((int)SyntaxType.NULL, s10.Id);
            s16.AddNextState((int)SyntaxType.NOT, s11.Id);
            s16.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s16.AddNextState((int)SyntaxType.FASTEST, s17.Id);

            s17.AddNextState(new int[] { (int)SyntaxType.Comma, (int)SyntaxType.RBracket }, squit.Id);
            s17.AddNextState((int)SyntaxType.ANALYZER, s8.Id);
            s17.AddNextState(new int[] { (int)SyntaxType.TOKENIZED, (int)SyntaxType.UNTOKENIZED, (int)SyntaxType.NONE }, s7.Id);
            s17.AddNextState((int)SyntaxType.NULL, s10.Id);
            s17.AddNextState((int)SyntaxType.NOT, s11.Id);
            s17.AddNextState((int)SyntaxType.DEFAULT, s13.Id);
            s17.AddNextState((int)SyntaxType.PRIMARY, s15.Id);

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
        public string FieldName;
        public Data.DataType DataType;
        public int DataLength;
        public Data.Field.Index IndexType = Hubble.Core.Data.Field.Index.None;
        public Data.Field.IndexMode IndexMode = Hubble.Core.Data.Field.IndexMode.Complex;
        public string AnalyzerName;
        public string Default = null;
        public bool CanNull = true;
        public bool PrimaryKey = false;

        #endregion

    }
}
