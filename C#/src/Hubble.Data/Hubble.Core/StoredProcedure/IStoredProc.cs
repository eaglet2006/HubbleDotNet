using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.StoredProcedure
{
    public interface IStoredProc
    {
        List<string> Parameters {get;}

        Hubble.Core.SFQL.Parse.QueryResult Result {get;}

        string Name { get; }

        void Run();
    }
}
