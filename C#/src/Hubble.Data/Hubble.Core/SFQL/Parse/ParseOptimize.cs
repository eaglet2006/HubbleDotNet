using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.SFQL.SyntaxAnalysis;

namespace Hubble.Core.SFQL.Parse
{
    public class ParseOptimize
    {
        bool _ComplexTree = false;

        /// <summary>
        /// Complex tree like following:
        /// ((title match 'xxx' and price > 1) or (content match 'xxx')) and time > '2000-1-1'
        /// Folloing is not complex tree
        /// (title match 'xxx' or content match 'xxx') and time > '2000-1-1' and price > 1
        /// </summary>
        internal bool ComplexTree
        {
            get
            {
                return _ComplexTree;
            }
        }

        ExpressionTree _UntokenizedTreeOnRoot = null;

        internal ExpressionTree UntokenizedTreeOnRoot
        {
            get
            {
                return _UntokenizedTreeOnRoot;
            }
        }

        /// <summary>
        /// Is the tree has or child
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private bool HasOrChild(ExpressionTree tree)
        {
            if (tree.OrChild != null)
            {
                return true;
            }

            if (tree.AndChild != null)
            {
                return HasOrChild(tree.AndChild);
            }
            else
            {
                return false;
            }

        }

        private void OptimizeAndChild(ExpressionTree tree)
        {
            ExpressionTree firstTreeUnTokenize = tree;

            ExpressionTree next = tree;

            while (next != null)
            {
                if (next.Expression.NeedTokenize)
                {
                    if (next.Expression is ExpressionTree)
                    {
                        if (!_ComplexTree)
                        {
                            //Expression is Tree and has OR condition, like (a match 'xxx' or b match 'yyy') and ....
                            //Set it to ComplexTree.
                            _ComplexTree = HasOrChild(next.Expression as ExpressionTree); //
                        }

                        OptimizeExpressionTree(next.Expression as ExpressionTree);
                    }

                    if (!firstTreeUnTokenize.Expression.NeedTokenize)
                    {
                        //Swap expression
                        IExpression temp = firstTreeUnTokenize.Expression;
                        firstTreeUnTokenize.Expression = next.Expression;
                        next.Expression = temp;
                        firstTreeUnTokenize = firstTreeUnTokenize.AndChild;
                    }
                    else
                    {
                        firstTreeUnTokenize = firstTreeUnTokenize.AndChild; //Modification at 2011-12-1

                        //if (firstTreeUnTokenize == tree)
                        //{
                        //}
                    }
                }

                next = next.AndChild;
            }


            if (_UntokenizedTreeOnRoot == null)
            {
                _UntokenizedTreeOnRoot = firstTreeUnTokenize;
            }
            else
            {
                _UntokenizedTreeOnRoot = firstTreeUnTokenize;
                _ComplexTree = true;
            }
        }

        private void OptimizeExpressionTree(ExpressionTree tree)
        {
            if (tree == null)
            {
                return;
            }

            if (tree.OrChild != null)
            {
                OptimizeExpressionTree(tree.OrChild);
            }

            OptimizeAndChild(tree);
        }


        private SyntaxAnalysis.Where Optimize(SyntaxAnalysis.Where where)
        {
            OptimizeExpressionTree(where.ExpressionTree);

            return where;
        }

        public TSFQLSentence Optimize(TSFQLSentence sentence)
        {
            switch (sentence.SentenceType)
            {
                case SentenceType.SELECT:
                    SyntaxAnalysis.Select.Select select = sentence.SyntaxEntity as SyntaxAnalysis.Select.Select;

                    if (select.Where != null)
                    {
                        select.Where = Optimize(select.Where);
                    }

                    break;
                case SentenceType.DELETE:
                    SyntaxAnalysis.Delete.Delete delete = sentence.SyntaxEntity as SyntaxAnalysis.Delete.Delete;

                    if (delete.Where != null)
                    {
                        delete.Where = Optimize(delete.Where);
                    }

                    break;

                case SentenceType.UPDATE:
                    SyntaxAnalysis.Update.Update update = sentence.SyntaxEntity as SyntaxAnalysis.Update.Update;

                    if (update.Where != null)
                    {
                        update.Where = Optimize(update.Where);
                    }

                    break;
                case SentenceType.INSERT:
                case SentenceType.EXEC:
                case SentenceType.CREATETABLE:
                    break;
                default:
                    throw new SyntaxException(string.Format("Unknow sentence {0}", sentence.SentenceType));
            }

            sentence.ParseOptimize = this;
            return sentence;
        }
    }
}
