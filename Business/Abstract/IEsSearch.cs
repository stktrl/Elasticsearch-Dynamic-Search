using DataAccess.Model;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IEsSearch<T> where T : class
    {
        public ElasticClient Connect();
        public void GenerateQuery(EsQueryStructure q, BoolQueryDescriptor<T> block);
        public List<T> Search(BoolQueryDescriptor<T> block,string IxName);
    }
}
