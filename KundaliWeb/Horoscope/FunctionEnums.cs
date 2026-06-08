using System;
using System.Collections.Generic;
using System.Text;

namespace GTS.MI.MPM.AF.InductionData
{
    internal enum RecordStatusEnum
    {
        NewRecord = 0,
        Sucessfull = 1,
        Failed = 2,
        DoNotReTry = 3,
        NoDataFound = 4
    }

    internal enum RecordType
    {
        Create = 0,
        Update = 1
    }

}
