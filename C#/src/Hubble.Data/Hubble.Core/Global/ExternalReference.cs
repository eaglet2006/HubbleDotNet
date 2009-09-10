using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Hubble.Core.Global
{
    [Serializable]
    public abstract class ExternalReference
    {
        private string _AssemblyFile;

        /// <summary>
        /// Assembly file path
        /// </summary>
        public string AssemblyFile
        {
            get
            {
                return _AssemblyFile;
            }

            set
            {
                _AssemblyFile = value;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        protected abstract Type Interface { get; }

        internal void Load(Dictionary<string, Type> dictionary)
        {
            Assembly asm = Assembly.LoadFrom(AssemblyFile);

            foreach (Type type in asm.GetTypes())
            {
                if (type.GetInterface(Interface.FullName) != null)
                {
                    Data.INamedExternalReference refer = asm.CreateInstance(type.FullName) as
                        Data.INamedExternalReference;

                    if (refer == null)
                    {
                        Report.WriteErrorLog(string.Format("External reference {0} does not inherit from INamedExternalReference",
                            type.FullName));
                    }
                    else
                    {
                        string key = refer.Name.ToLower().Trim();

                        if (!dictionary.ContainsKey(key))
                        {
                            if (refer is Analysis.IAnalyzer)
                            {
                                Analysis.IAnalyzer analyzer = refer as Analysis.IAnalyzer;
                                analyzer.Init();
                            }

                            dictionary.Add(key, type);
                        }
                        else
                        {
                            Global.Report.WriteErrorLog(string.Format("Reduplicate name = {0} in External reference {1}!",
                                refer.Name, type.FullName));
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            ExternalReference other = obj as ExternalReference;

            if (other == null)
            {
                return false;
            }

            return other.AssemblyFile.Equals(this.AssemblyFile, 
                StringComparison.CurrentCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return _AssemblyFile.GetHashCode();
        }
    
    }
}
