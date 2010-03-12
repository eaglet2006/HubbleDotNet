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
using Hubble.Core.Analysis;
using Hubble.Core.Data;
using Hubble.Framework.Serialization;
using PanGu;

namespace Hubble.Analyzer
{
    public class PanGuAnalyzer : IAnalyzer, INamedExternalReference
    {
        static PanGu.Setting.PanGuSettings _SqlClientSetting;

        ICollection<WordInfo> _Tokenes = null;

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

        public int Count
        {
            get
            {
                if (_Tokenes == null)
                {
                    throw new Exception("Tokenes is null. Count property should be called after Tokenize");
                }
                else
                {
                    return _Tokenes.Count;
                }
            }
        }

        public IEnumerable<Hubble.Core.Entity.WordInfo> Tokenize(string text)
        {
            PanGu.Segment segment = new Segment();
            _Tokenes = segment.DoSegment(text);

            foreach (PanGu.WordInfo wi in _Tokenes)
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
