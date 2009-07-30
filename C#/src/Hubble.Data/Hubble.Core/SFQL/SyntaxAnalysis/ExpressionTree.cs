using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.SFQL.SyntaxAnalysis
{
    public interface IExpression
    {
        string SqlText { get; }
        bool NeedReverse { get; }
        bool NeedTokenize { get; }
    }

    public class Expression : IExpression
    {
        //For parse
        public int FieldTab = -1;
        public int PayloadLength = 0;
        public Data.DataType DataType;
        public int[] ComparisionData = null;


        //For Syntax
        public List<LexicalAnalysis.Lexical.Token> Left; //Left of Comparison Operators
        public LexicalAnalysis.Lexical.Token Operator; //Comparison Operators
        public List<LexicalAnalysis.Lexical.Token> Right; //Right of Comparison Operators

        private bool? _NeedTokenize = null;
        private string _SqlText = null;

        public Expression()
        {
            Left = new List<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token>();
            Right = new List<Hubble.Core.SFQL.LexicalAnalysis.Lexical.Token>();
        }

        public override string ToString()
        {
            return SqlText;
        }

        #region IExpression Members

        public string SqlText
        {
            get
            {
                if (_SqlText == null)
                {
                    StringBuilder str = new StringBuilder();

                    foreach(LexicalAnalysis.Lexical.Token token in Left)
                    {
                        if (token.SyntaxType == SyntaxType.String)
                        {
                            str.AppendFormat("'{0}' ", token.Text.Replace("'", "''"));
                        }
                        else
                        {
                            str.AppendFormat("{0} ", token.Text);
                        }
                    }

                    switch (Operator.SyntaxType)
                    {
                        case SyntaxType.NotEqual:
                            str.Append("<>");
                            break;
                        case SyntaxType.Equal:
                            str.Append("=");
                            break;
                        case SyntaxType.Lessthan:
                            str.Append("<");
                            break;
                        case SyntaxType.LessthanEqual:
                            str.Append("<=");
                            break;
                        case SyntaxType.Largethan:
                            str.Append(">");
                            break;
                        case SyntaxType.LargethanEqual:
                            str.Append(">=");
                            break;
                        default:
                            str.Append(Operator.SyntaxType.ToString());
                            break;
                    }

                    str.Append(" ");

                    foreach (LexicalAnalysis.Lexical.Token token in Right)
                    {
                        if (token.SyntaxType == SyntaxType.String)
                        {
                            str.AppendFormat("'{0}' ", token.Text.Replace("'", "''"));
                        }
                        else
                        {
                            str.AppendFormat("{0} ", token.Text);
                        }
                    }

                    _SqlText = str.ToString();
                }

                return _SqlText;
            }
        }

        public bool NeedReverse
        {
            get { return false; }
        }

        public bool NeedTokenize
        {
            get
            {
                if (_NeedTokenize != null)
                {
                    return _NeedTokenize.Value;
                }

                _NeedTokenize = ExpressionTree.ExpressionNeedTokenize(this);
                return _NeedTokenize.Value;
            }
        }

        #endregion

        #region IExpression Members



        #endregion
    }

    public class ExpressionTree : IExpression
    {
        public ExpressionTree Parent = null;
        public ExpressionTree AndChild = null;
        public ExpressionTree OrChild = null;
        public IExpression Expression = null;

        static public bool ExpressionNeedTokenize(Expression expression)
        {
            switch (expression.Operator.SyntaxType)
            {
                case SyntaxType.LIKE:
                case SyntaxType.MATCH:
                case SyntaxType.CONTAINS:
                case SyntaxType.CONTAINS1:
                case SyntaxType.CONTAINS2:
                case SyntaxType.CONTAINS3:
                    return true;
                default:
                    return false;
            }
        }

        private void MakeNeedTokenize(ExpressionTree tree)
        {
            if (_NeedTokenize != null)
            {
                return;
            }

            if (this.Expression.NeedTokenize)
            {
                _NeedTokenize = true;
                return;
            }

            if (tree.AndChild != null)
            {
                if (tree.AndChild.Expression.NeedTokenize)
                {
                    _NeedTokenize = true;
                    return;
                }
            }

            if (tree.OrChild != null)
            {
                if (tree.OrChild.Expression.NeedTokenize)
                {
                    _NeedTokenize = true;
                    return;
                }
            }

            _NeedTokenize = false;
        }

        public ExpressionTree(ExpressionTree parent)
        {
            Parent = parent;
        }

        private bool? _NeedTokenize = null;

        public override string ToString()
        {
            return SqlText;
        }

        #region IExpression Members

        public string SqlText
        {
            get
            {

                StringBuilder str = new StringBuilder();

                str.Append("(");

                str.Append(this.Expression.SqlText);

                ExpressionTree next = this.AndChild;

                while (next != null)
                {
                    str.AppendFormat(" AND {0} ", next.Expression.SqlText);

                    next = next.AndChild;
                }

                next = this.OrChild;

                while (next != null)
                {
                    str.AppendFormat(" OR {0} ", next.Expression.SqlText);

                    next = next.OrChild;
                }

                str.Append(")");

                return str.ToString();

            }
        }

        public bool NeedReverse
        {
            get { return true; }
        }

        public bool NeedTokenize
        {
            get
            {
                if (_NeedTokenize != null)
                {
                    return _NeedTokenize.Value;
                }

                MakeNeedTokenize(this);

                return _NeedTokenize.Value;
            }
        }

        #endregion
    }
}
