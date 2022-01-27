using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Model
{
    public class EsQueryStructure
    {
        public EsQueryStructure()
        {
            QueryItems = new List<EsQueryItem>();
            QueryStructures = new List<EsQueryStructure>();
        }
        public bool IsAnd { get; set; }
        public List<EsQueryItem> QueryItems { get; set; }
        public List<EsQueryStructure> QueryStructures { get; set; }
    }
}
