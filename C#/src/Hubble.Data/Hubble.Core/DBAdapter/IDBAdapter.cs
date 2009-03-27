using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.DBAdapter
{
    public interface IDBAdapter
    {
        Data.Table Table { get; set; }

        void Create();

        void Insert(List<Data.Document> docs);
    }
}
