using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Model
{
    public enum EnumCompareOperands
    {
        Equal = 0,
        GreaterThen = 1,
        LessThen = 2,
        GreaterOrEqual = 3,
        LessOrEqual = 4,
        NotEqual = 5,
        StartsWith = 6,
        EndsWith = 7,
        Contains = 8
    }
}
