using Business.Abstract;
using Entities.Model;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class EsSearch<T> : IEsSearch<T>
            where T : class
    {
        private SearchDescriptor<T> searchDescriptor = new SearchDescriptor<T>();
        private QueryContainerDescriptor<T> _queryContainerDescriptor = new QueryContainerDescriptor<T>();
        private IConfiguration _configuration;
        public EsSearch(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ElasticClient Connect()
        {
            var settings = new ConnectionSettings(new Uri(_configuration.GetSection("ElasticSearchOptions").GetSection("Host").Value));
            var client = new ElasticClient(settings);
            return client;
        }
        public QueryContainerDescriptor<T> QueryBlock(EsQueryItem qitem)
        {
            QueryContainerDescriptor<T> result = new QueryContainerDescriptor<T>();
            switch (qitem.CompareOperand)
            {
                case EnumCompareOperands.Contains:
                    result.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query("*" + qitem.Value.ToString() + "*"));
                    break;
                case EnumCompareOperands.EndsWith:
                    result.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query("*" + qitem.Value.ToString()));
                    break;
                case EnumCompareOperands.NotEqual:
                    result.Match(r => r.Field(qitem.FieldName.ToString()).Query(qitem.Value.ToString()));// bakılacak buraya 
                    break;
                case EnumCompareOperands.Equal:
                    result.Match(m => m.Field(qitem.FieldName.ToString()).Query(qitem.Value.ToString()));
                    break;
                case EnumCompareOperands.GreaterOrEqual:
                    result.Range(r => r.Field(qitem.FieldName.ToString()).GreaterThanOrEquals((double?)qitem.Value));
                    break;
                case EnumCompareOperands.GreaterThen:
                    result.Range(r => r.Field(qitem.FieldName.ToString()).GreaterThan((double?)qitem.Value));
                    break;
                case EnumCompareOperands.LessOrEqual:
                    result.Range(r => r.Field(qitem.FieldName.ToString()).LessThanOrEquals((double?)qitem.Value));
                    break;
                case EnumCompareOperands.LessThen:
                    result.Range(r => r.Field(qitem.FieldName.ToString()).LessThan((double?)qitem.Value));
                    break;
                case EnumCompareOperands.StartsWith:
                    result.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query(qitem.Value.ToString() + "*"));
                    break;
            }
            return result;

        }
        public void GenerateQuery(EsQueryStructure q, BoolQueryDescriptor<T> block)
        {
            List<QueryContainer> AndContainer = new List<QueryContainer>();
            List<QueryContainer> OrContainer = new List<QueryContainer>();
            List<QueryContainer> MustNotContainerForAnd = new List<QueryContainer>();
            List<QueryContainer> MustNotContainerForOr = new List<QueryContainer>();
            if (q.IsAnd)
            {
                if (q.QueryItems != null)
                {
                    foreach (EsQueryItem qitem in q.QueryItems)
                    {
                        if (qitem.CompareOperand != EnumCompareOperands.NotEqual)
                        {
                            AndContainer.Add(QueryBlock(qitem));
                        }
                        else
                        {
                            MustNotContainerForAnd.Add(QueryBlock(qitem));
                        }
                    }
                }
                if (q.QueryStructures != null)
                {
                    foreach (EsQueryStructure qsubstructure in q.QueryStructures)
                    {
                        BoolQueryDescriptor<T> subBlock = new BoolQueryDescriptor<T>();
                        GenerateQuery(qsubstructure, subBlock);
                        AndContainer.Add(_queryContainerDescriptor.Bool(s => subBlock));
                        //block.Must(_queryContainerDescriptor.Bool(s => subBlock));
                    }
                }
                AndContainer.Add(_queryContainerDescriptor.Bool(b=>b.MustNot(MustNotContainerForAnd.ToArray())));
                block.Must(AndContainer.ToArray());
            }
            else
            {
                foreach (EsQueryItem qitem in q.QueryItems)
                {
                    if (qitem.CompareOperand != EnumCompareOperands.NotEqual)
                    {
                        OrContainer.Add(QueryBlock(qitem));
                    }
                    else
                    {
                        MustNotContainerForOr.Add(QueryBlock(qitem));
                    }         
                    if (q.QueryStructures != null)
                    {
                        foreach (EsQueryStructure qsubstructure in q.QueryStructures)
                        {
                            BoolQueryDescriptor<T> subBlock = new BoolQueryDescriptor<T>();
                            GenerateQuery(qsubstructure, subBlock);
                            OrContainer.Add(_queryContainerDescriptor.Bool(s => subBlock));
                        }
                    }
                   
                }
                OrContainer.Add(_queryContainerDescriptor.Bool(b => b.MustNot(MustNotContainerForOr.ToArray())));
                block.Should(OrContainer.ToArray());
            }
        }
        public List<T> Search(BoolQueryDescriptor<T> block, string IxName)
        {
            searchDescriptor.Query(q => q.Bool(b => block));
            searchDescriptor.Index(IxName);
            searchDescriptor.From(0);
            searchDescriptor.Size(1000);
            var client = Connect();
            var response = client.Search<T>(searchDescriptor);
            using (MemoryStream mStream = new MemoryStream())
            {
                client.RequestResponseSerializer.Serialize(searchDescriptor, mStream);
                string rawQueryText = Encoding.ASCII.GetString(mStream.ToArray());
            }
            List<T> responseList = new List<T>();
            responseList.AddRange(response.Documents);
            return responseList;
        }
    }
}
