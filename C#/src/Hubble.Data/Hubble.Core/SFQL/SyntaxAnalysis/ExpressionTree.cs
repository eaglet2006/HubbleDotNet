using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public interface IExpression
    {
        bool NeedReverse { get; }
    }

    public class Expression : IExpression
    {
        public List<LexicalAnalysis.Lexical.Token> Left; //Left of Comparison Operators
        public LexicalAnalysis.Lexical.Token Operator; //Comparison Operators
        public List<LexicalAnalysis.Lexical.Token> Right; //Right of Comparison Operators

        public Expression()
        {
            Left = new List<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token>();
            Right = new List<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token>();
        }

        #region IExpression Members

        public bool NeedReverse
        {
            get { return false; }
        }

        #endregion
    }

    public class ExpressionTree : IExpression
    {
        public ExpressionTree Parent = null;
        public ExpressionTree AndChild = null;
        public ExpressionTree OrChild = null;
        public IExpression Expression = null;

        public ExpressionTree(ExpressionTree parent)
        {
            Parent = parent;
        }

        #region IExpression Members

        public bool NeedReverse
        {
            get { return true; }
        }

        #endregion
    }
}
