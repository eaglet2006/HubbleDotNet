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
using Hubble.Framework.DataStructure;
using Hubble.Core.Entity;

namespace Hubble.Core.Store
{
    public class WordStepDocIndex
    {
        public class IndexFileInfo
        {
            public List<Hubble.Core.Entity.DocumentPosition> StepDocIndex; //Step doc index
            public IndexFile.FilePosition IndexFilePostion; //Position of Index without step doc index 
            public int DocumentCount; //Count of the document records in this index

            public IndexFileInfo(List<Hubble.Core.Entity.DocumentPosition> stepDocIndex,
                IndexFile.FilePosition indexFilePostion, int documentCount)
            {
                this.StepDocIndex = stepDocIndex;
                this.IndexFilePostion = indexFilePostion;
                this.DocumentCount = documentCount;
            }
        }

        public List<IndexFileInfo> IndexFileInfoList = null;

        public WordFilePositionList WordFilePositionList = null;

        /// <summary>
        /// Sum of word count
        /// </summary>
        public long WordCountSum = 0;

        /// <summary>
        /// if doc count > max return count
        /// this field return rel doc count
        /// </summary>
        public int RelDocCount = 0;

        /// <summary>
        /// Index is Simple Mode
        /// </summary>
        public bool SimpleMode;

        public WordStepDocIndex(List<IndexFileInfo> indexFileInfoList,
            long wordCountSum, int relDocCount, bool simple)
        {
            IndexFileInfoList = indexFileInfoList;
            WordCountSum = wordCountSum;
            RelDocCount = relDocCount;
            SimpleMode = simple;
        }

   

    }

    /// <summary>
    /// This class is the result of documents list for one word
    /// </summary>
    public class WordDocumentsList : SuperList<Entity.DocumentPositionList>
    {
        /// <summary>
        /// Sum of word count
        /// </summary>
        public long WordCountSum = 0;

        /// <summary>
        /// if doc count > max return count
        /// this field return rel doc count
        /// </summary>
        public int RelDocCount = 0;

    }
}
