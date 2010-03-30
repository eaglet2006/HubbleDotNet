using System;
using System.Collections.Generic;
using System.Text;

namespace QueryAnalyzer.CreateTable
{
    interface IBefore
    {
        void Do(QueryAnalyzer.FormCreateTable frmCreateTable);
    }
}
