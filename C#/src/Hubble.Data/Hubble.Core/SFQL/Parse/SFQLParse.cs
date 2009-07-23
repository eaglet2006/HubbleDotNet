using System;
using System.Collections.Generic;
using System.Text;

using Hubble.Core.SFQL.LexicalAnalysis;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.SFQL.Parse
{
    public class SFQLParse
    {
        List<TSFQLSentence> _SFQLSentenceList;
        TSFQLSentence _SFQLSentence;

        private void InputLexicalToken(Lexical.Token token)
        {
            if (token.SyntaxType == SyntaxType.Space)
            {
                return;
            }

            if (_SFQLSentence == null)
            {
                _SFQLSentence = new TSFQLSentence();
            }

            DFAResult result = _SFQLSentence.Input((int)token.SyntaxType, token);

            switch (result)
            {
                case DFAResult.Quit:
                case DFAResult.ElseQuit:
                    _SFQLSentenceList.Add(_SFQLSentence);
                    _SFQLSentence = null;
                    break;
            }

        }

        private void SyntaxAnalyse(string text)
        {
            Lexical lexical = new Lexical(text);

            DFAResult dfaResult;

            for (int i = 0; i < text.Length; i++)
            {
                dfaResult = lexical.Input(text[i], i);

                switch (dfaResult)
                {
                    case DFAResult.Continue:
                        continue;
                    case DFAResult.Quit:
                        InputLexicalToken(lexical.OutputToken);
                        break;
                    case DFAResult.ElseQuit:
                        InputLexicalToken(lexical.OutputToken);
                        i--;
                        break;
                }

            }


            dfaResult = lexical.Input(0, text.Length);

            switch (dfaResult)
            {
                case DFAResult.Continue:
                    throw new Hubble.Core.SFQL.LexicalAnalysis.LexicalException("Lexical abort at the end of sql");
                case DFAResult.Quit:
                    InputLexicalToken(lexical.OutputToken);
                    break;
                case DFAResult.ElseQuit:
                    InputLexicalToken(lexical.OutputToken);
                    break;
            }

            InputLexicalToken(new Lexical.Token());

        }

        private System.Data.DataSet ExecuteTSFQLSentence(TSFQLSentence sentence)
        {
            ParseOptimize pOptimize = new ParseOptimize();
            sentence = pOptimize.Optimize(sentence);
            return null;
        }


        public void ExecuteNonQuery(string sql)
        {
            Query(sql);
        }

        public System.Data.DataSet Query(string sql)
        {
            _SFQLSentenceList = new List<TSFQLSentence>();

            System.Data.DataSet result = new System.Data.DataSet();

            SyntaxAnalyse(sql);

            foreach (TSFQLSentence sentence in _SFQLSentenceList)
            {
                System.Data.DataSet ds = ExecuteTSFQLSentence(sentence);

                if (ds != null)
                {
                    foreach (System.Data.DataTable table in ds.Tables)
                    {
                        result.Tables.Add(table);
                    }
                }
            }

            return result;
        }
    }
}
