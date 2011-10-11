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
using System.Linq;
using System.Text;

namespace Hubble.SQLClient.Linq.Entities
{
    /// <summary>
    /// TF-IDF info
    /// </summary>
    public class TFIDF
    {
        /// <summary>
        /// Word
        /// </summary>
        [Column("Word", true)]
        public string Word { get; set; }

        /// <summary>
        /// Term frequency
        /// </summary>
        [Column("TF")]
        public double TF { get; set; }

        /// <summary>
        /// inverse document frequency
        /// </summary>
        [Column("IDF")]
        public double IDF { get; set; }

        /// <summary>
        /// number of documents where the term t appears 
        /// </summary>
        [Column("T_D")]
        public string T_D { get; set; }

        /// <summary>
        /// the total number of documents in the corpus
        /// </summary>
        [Column("TotalDoucments")]
        public int TotalDoucments { get; set; }

        /// <summary>
        /// TF * IDF
        /// </summary>
        [Column("TF_IDF")]
        public double TF_IDF { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            TFIDF dest = (TFIDF)obj;

            if (this.Word == null || dest.Word == null)
            {
                return (this.Word == null && dest.Word == null);
            }

            return Word.Equals(dest.Word, StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            if (Word == null)
            {
                return 0;
            }

            return Word.ToLower().GetHashCode();
        }

        public override string ToString()
        {
            return Word;
        }
    }
}
