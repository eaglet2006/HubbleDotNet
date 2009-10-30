using System;
using System.Collections.Generic;
using System.Text;
using Hubble.SQLClient;

namespace Hubble.Core.StoredProcedure
{
    public interface IStoredProc
    {
        List<string> Parameters {get;}

        QueryResult Result {get;}

        string Name { get; }

        void Run();
    }
}
