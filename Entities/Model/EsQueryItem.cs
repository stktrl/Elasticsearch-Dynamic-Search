using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Model
{
    public class EsQueryItem
    {
        public string FieldName { get; set; }
        public EnumCompareOperands CompareOperand { get; set; }
        public object Value { get; set; }
    }
}
