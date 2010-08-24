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

namespace Hubble.Core.Entity
{
    public struct WordInfo : IComparable<WordInfo>
    {
        [Flags]
        public enum Flag
        {
            None = 0,
            Or = 0x00000001,
        }

        public string Word;
        public int Position;
        public int Rank;
        public Flag Flags;

        public WordInfo(string word, int position) 
            : this(word, position, 0, Flag.None)
        {
        }

        public WordInfo(string word, int position, int rank)
            : this(word, position, rank, Flag.None)
        {
        }

        public WordInfo(string word, int position, int rank, Flag flags)
        {
            Word = word;
            Position = position;
            Rank = rank <= 0 ? 1 : rank;
            Flags = flags;
        }

        public void SetWord(string word)
        {
            this.Word = word;
        }

        public int GetEndPositon()
        {
            return this.Position + this.Word.Length;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Word, Position, Rank) ;
        }

        #region IComparable<WordInfo> Members

        public int CompareTo(WordInfo other)
        {
            return this.Position.CompareTo(other.Position);
        }

        #endregion
    }
}
