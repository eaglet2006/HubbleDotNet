using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Core.Analysis;
using Hubble.Core.Data;
using Hubble.Framework.Serialization;
using PanGu;

namespace Hubble.Analyzer
{
    public class PanGuAnalyzer : IAnalyzer, INamedExternalReference
    {
        static PanGu.Setting.PanGuSettings _SqlClientSetting;

        private void LoadSqlClientSetting(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read))
                    {
                        _SqlClientSetting = XmlSerialization<PanGu.Setting.PanGuSettings>.Deserialize(fs);
                    }
                }
                catch
                {
                    _SqlClientSetting = new PanGu.Setting.PanGuSettings();
                }
            }
            else
            {
                _SqlClientSetting = new PanGu.Setting.PanGuSettings();
            }

        }

        #region IAnalyzer Members

        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            PanGu.Segment segment = new Segment();
            foreach (PanGu.WordInfo wi in segment.DoSegment(text))
            {
                yield return new Hubble.Core.Entity.WordInfo(wi.Word, wi.Position, wi.Rank);
            }
        }

        public IEnumerable<Hubble.Core.Entity.WordInfo> TokenizeForSqlClient(string text)
        {
            PanGu.Segment segment = new Segment();
            foreach (PanGu.WordInfo wi in segment.DoSegment(text, _SqlClientSetting.MatchOptions, _SqlClientSetting.Parameters))
            {
                yield return new Hubble.Core.Entity.WordInfo(wi.Word, wi.Position, wi.Rank);
            }
        }

        #endregion

        #region IAnalyzer Members

        public void Init()
        {
            LoadSqlClientSetting(PanGu.Framework.Path.GetAssemblyPath() + "PanGuSqlClient.xml");
            PanGu.Segment.Init();
        }

        #endregion

        #region INamedExternalReference Members

        public string Name
        {
            get 
            {
                return "PanGuSegment";
            }
        }

        #endregion
    }
}
