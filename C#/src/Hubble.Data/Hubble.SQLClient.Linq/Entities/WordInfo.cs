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
    /// Word info for analyzer
    /// </summary>
    public class WordInfo
    {
        /// <summary>
        /// Word
        /// </summary>
        [Column("Word")]
        public string Word { get; set; }

        /// <summary>
        /// Position of the first character of the word in the input text
        /// </summary>
        [Column("Position")]
        public int? Position { get; set; }

        /// <summary>
        /// Rank for the word
        /// </summary>
        [Column("Rank")]
        public int? Rank { get; set; }

        public override string ToString()
        {
            return Word;
        }
    }
}
