using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Query
{
    public interface IQuery
    {
        string FieldName { get; set;}
        string QueryString { get; set;}
        Index.InvertedIndex InvertedIndex { get; set;}
        Analysis.IAnalyzer Analyzer { get; set;}

        List<Entities.WordInfo> GetQueryWords();
        List<Entities.WordInfo> GetNextHitWords(out long docId);
    }
}
