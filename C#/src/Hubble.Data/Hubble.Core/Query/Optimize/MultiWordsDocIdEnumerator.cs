using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hubble.Core.Query.Optimize
{
    public class MultiWordsDocIdEnumerator
    {
        /// <summary>
        /// Doc id entity for specify word
        /// </summary>
        class WordDocIdEntity : IComparable<WordDocIdEntity>
        {
            internal Entity.OriginalDocumentPositionList DocList;

            internal WordIndexForQuery WordIndex; //WordIndex for this word

            internal int Index; //index of _WordIndexes

            internal WordDocIdEntity(int index, WordIndexForQuery wordIndex)
            {
                this.Index = index;
                this.WordIndex = wordIndex;
            }

            #region IComparable<WordDocIdEntity> Members

            public int CompareTo(WordDocIdEntity other)
            {
                if (this.DocList.DocumentId < other.DocList.DocumentId)
                {
                    return -1;
                }
                else if (this.DocList.DocumentId > other.DocList.DocumentId)
                {
                    //if docid = -1, sort to tail of the list
                    if (other.DocList.DocumentId < 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 0;
                }
            }

            #endregion
        }

        public WordIndexForQuery[] WordIndexes;
        public int SelectedCount;
        public int TotalDocIdCount;
        public int[] SelectedIndexes;

        WordDocIdEntity[] _WordDocEntities; //this is a priority queue
        int _WordsCount;


        public MultiWordsDocIdEnumerator(WordIndexForQuery[] wordIndexes)
        {
            WordIndexes = wordIndexes;

            Array.Sort(WordIndexes);

            _WordDocEntities = new WordDocIdEntity[WordIndexes.Length]; 

            for (int index = 0; index < WordIndexes.Length; index++)
            {
                _WordDocEntities[index] = new WordDocIdEntity(index, WordIndexes[index]);

                _WordDocEntities[index].WordIndex.WordIndex.GetNextOriginal(ref _WordDocEntities[index].DocList);
            }

            Array.Sort(_WordDocEntities);

            _WordsCount = _WordDocEntities.Length;
            SelectedIndexes = new int[_WordsCount];
            TotalDocIdCount = 0;
        }

        public void GetNextOriginal(ref Entity.OriginalDocumentPositionList odpl)
        {
            int fstDocId = _WordDocEntities[0].DocList.DocumentId;

            if (fstDocId < 0)
            {
                odpl.DocumentId = fstDocId;
                return;
            }

            //Find same docid 
            SelectedIndexes[0] = _WordDocEntities[0].Index;
            SelectedCount = 1;
            TotalDocIdCount++;

            for (int i = 1; i < _WordDocEntities.Length; i++)
            {
                if (fstDocId == _WordDocEntities[i].DocList.DocumentId)
                {
                    SelectedIndexes[i] = _WordDocEntities[i].Index;
                    SelectedCount++;
                }
                else
                {
                    break;
                }
            }

            odpl = _WordDocEntities[0].DocList;

            for (int i = 0; i < SelectedCount; i++)
            {
                //Get the next docid from first entity of the word
                _WordDocEntities[0].WordIndex.WordIndex.GetNextOriginal(ref _WordDocEntities[0].DocList);

                //Re sort the priority queue
                int cur = 0;
                int nextOne = cur + 1;

                while (nextOne < _WordsCount)
                {
                    if (_WordDocEntities[nextOne].DocList.DocumentId < 0)
                    {
                        break;
                    }

                    if (_WordDocEntities[cur].DocList.DocumentId > _WordDocEntities[nextOne].DocList.DocumentId)
                    {
                        //Swap when cur docid > next docid
                        WordDocIdEntity temp = _WordDocEntities[cur];

                        _WordDocEntities[cur] = _WordDocEntities[nextOne];

                        _WordDocEntities[nextOne] = temp;
                    }
                    else if (_WordDocEntities[cur].DocList.DocumentId < 0)
                    {
                        if (_WordDocEntities[cur].DocList.DocumentId < _WordDocEntities[nextOne].DocList.DocumentId)
                        {
                            //Swap when the docid of this word is empty from full text index
                            WordDocIdEntity temp = _WordDocEntities[cur];

                            _WordDocEntities[cur] = _WordDocEntities[nextOne];

                            _WordDocEntities[nextOne] = temp;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    cur++;
                    nextOne++;
                }
            }
        }
    }
}
