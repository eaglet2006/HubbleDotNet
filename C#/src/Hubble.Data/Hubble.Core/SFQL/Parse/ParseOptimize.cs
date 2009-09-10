using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.SFQL.SyntaxAnalysis;

namespace Hubble.Core.SFQL.Parse
{
    class ParseOptimize
    {
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
                        if (firstTreeUnTokenize == tree)
                        {
                            firstTreeUnTokenize = firstTreeUnTokenize.AndChild;
                        }
                    }
                }

                next = next.AndChild;
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
                    break;
                default:
                    throw new SyntaxException(string.Format("Unknow sentence {0}", sentence.SentenceType));
            }

            return sentence;
        }
    }
}
