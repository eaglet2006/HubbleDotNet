using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.DataStructure;

namespace Hubble.Core.Store
{
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
