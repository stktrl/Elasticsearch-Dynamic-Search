using Business.Abstract;
using DataAccess.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Search : ControllerBase
    {
        private IEsSearch<dynamic> _esSearch;
        private BoolQueryDescriptor<dynamic> Block = new BoolQueryDescriptor<dynamic>();

        public Search(IEsSearch<dynamic> esSearch)
        {
            _esSearch = esSearch;
        }
        /// <summary>
        /// QueryStructure yapısında gelen sorgu parametreleri ile ElasticSearch üzerinde arama işlemi yapılır.
        /// </summary>
        /// <param name="queryStructure"></param>
        /// <param name="IxName"></param>
        /// <returns></returns>
        /// <response code="200">Arama sonucunda parametreler ile eşleşen sonuçları listeler. </response>    
        /// <response code="400">İstek hatalı.</response>
        /// <response code="404">Liste boş.</response>
        [HttpGet]
        public IActionResult GetSearch(EsQueryStructure queryStructure,string IxName)
        {
            _esSearch.GenerateQuery(queryStructure,Block);
            var response = _esSearch.Search(Block,IxName);
            if (response.Count > 0)
            {
                return Ok(response);
            }else if (response.Count == 0)
            {
                return NotFound();
            }
            return BadRequest();
        }
    }  
}
