using Business.Abstract;
using DataAccess.Model;
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
            where T:class
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

        public void GenerateQuery(EsQueryStructure q, BoolQueryDescriptor<T> block)
        {
            List<QueryContainer> AndContainer = new List<QueryContainer>();
            List<QueryContainer> OrContainer = new List<QueryContainer>();
            if (q.IsAnd)
            {
                if (q.QueryItems != null)
                {
                    foreach (EsQueryItem qitem in q.QueryItems)
                    {
                        switch (qitem.CompareOperand)
                        {
                            case EnumCompareOperands.Contains:
                                AndContainer.Add(_queryContainerDescriptor.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query("*" + qitem.Value.ToString() + "*")));
                                break;
                            case EnumCompareOperands.EndsWith:
                                AndContainer.Add(_queryContainerDescriptor.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query("*" + qitem.Value.ToString())));
                                break;
                            case EnumCompareOperands.Equal:
                                AndContainer.Add(_queryContainerDescriptor.Match(m => m.Field(qitem.FieldName.ToString()).Query(qitem.Value.ToString())));
                                break;
                            case EnumCompareOperands.GreaterOrEqual:
                                AndContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).GreaterThanOrEquals((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.GreaterThen:
                                AndContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).GreaterThan((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.LessOrEqual:
                                AndContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).LessThanOrEquals((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.LessThen:
                                AndContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).LessThan((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.NotEqual:
                                block.MustNot(_queryContainerDescriptor.Match(r => r.Field(qitem.FieldName.ToString()).Query(qitem.Value.ToString())));// bakılacak buraya 
                                break;
                            case EnumCompareOperands.StartsWith:
                                AndContainer.Add(_queryContainerDescriptor.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query(qitem.Value.ToString() + "*")));
                                break;
                        }
                        block.Must(AndContainer.ToArray());
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
                block.Must(AndContainer.ToArray());
            }
            else
            {
                if (q.QueryItems != null)
                {
                    foreach (EsQueryItem qitem in q.QueryItems)
                    {
                        switch (qitem.CompareOperand)
                        {
                            case EnumCompareOperands.Contains:
                                OrContainer.Add(_queryContainerDescriptor.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query("*" + qitem.Value.ToString() + "*")));
                                break;
                            case EnumCompareOperands.EndsWith:
                                OrContainer.Add(_queryContainerDescriptor.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query("*" + qitem.Value.ToString())));
                                break;
                            case EnumCompareOperands.Equal:
                                OrContainer.Add(_queryContainerDescriptor.Match(m => m.Field(qitem.FieldName.ToString()).Query(qitem.Value.ToString())));
                                break;
                            case EnumCompareOperands.GreaterOrEqual:
                                OrContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).GreaterThanOrEquals((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.GreaterThen:
                                OrContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).GreaterThan((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.LessOrEqual:
                                OrContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).LessThanOrEquals((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.LessThen:
                                OrContainer.Add(_queryContainerDescriptor.Range(r => r.Field(qitem.FieldName.ToString()).LessThan((double?)qitem.Value)));
                                break;
                            case EnumCompareOperands.NotEqual:
                                OrContainer.Add(_queryContainerDescriptor.Match(r => r.Field(qitem.FieldName.ToString()).Query(qitem.Value.ToString())));// bakılacak buraya
                                break;
                            case EnumCompareOperands.StartsWith:
                                OrContainer.Add(_queryContainerDescriptor.QueryString(q => q.Fields(f => f.Field(qitem.FieldName.ToString())).Query(qitem.Value.ToString() + "*")));
                                break;
                        }


                    }
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
                block.Should(OrContainer.ToArray());
            }
        }

        public List<T> Search(BoolQueryDescriptor<T> block,string IxName)
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
