using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hubble.Core.SFQL.LexicalAnalysis;
using Hubble.Core.SFQL.SyntaxAnalysis;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public class SyntaxAnalyse
    {
        List<TSFQLSentence> _SFQLSentenceList;
        TSFQLSentence _SFQLSentence;
        bool _ComplexExpression = false;

        #region public properties

        public bool ComplexExpression
        {
            get
            {
                return _ComplexExpression;
            }
        }

        public List<TSFQLSentence> SFQLSentenceList
        {
            get
            {
                return _SFQLSentenceList;
            }
        }

        #endregion

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="merged">
        /// if expression hasn't or child, output the merged expression. It means the expression can be 
        /// merged.
        /// Like (a match 'xxx' and b match 'yyy') can merged to a match 'xxx' and b match 'yyy'
        /// else
        /// output null.
        /// </param>
        internal void OptimizeReverse(SyntaxAnalysis.IExpression expression)
        {
            if (expression == null)
            {
                return;
            }

            if (expression is SyntaxAnalysis.ExpressionTree)
            {
                SyntaxAnalysis.ExpressionTree tree = expression as SyntaxAnalysis.ExpressionTree;

                if (tree.Expression is ExpressionTree)
                {
                    SyntaxAnalysis.ExpressionTree child = tree.Expression as SyntaxAnalysis.ExpressionTree;
                    
                    if (child.OrChild == null && child.AndChild == null)
                    {
                        tree.Expression = child.Expression;
                    }
                }

                OptimizeReverse(tree.AndChild);

                OptimizeReverse(tree.Expression);

                OptimizeReverse(tree.OrChild);
            }

            return;

            //if (expression
        }

        public SyntaxAnalyse(string text)
        {
            _SFQLSentenceList = new List<TSFQLSentence>();

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

    }
}
