using CleverDb;
using CleverDb.Infrastructure;
using CleverDb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace CleverApi.Controllers
{
    public class CleverDbController : ApiController
    {
        [HttpPost]
        [Route("api/cleverdb/insert")]
        public HttpResponseMessage Insert(dynamic json)
        {
            if (json == null)
            {
                return Request.CreateResponse<dynamic>(HttpStatusCode.BadRequest,
                    new
                    {
                        Exception = "InvalidObjectFormatException",
                        Message = "Json object you have provided has invalid format"
                    });
            }
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            try
            {
                var inserted = db.Insert(json);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(inserted.ToString(), Encoding.UTF8, "application/json")
                };
            }
            catch (Exception exp)
            {
                return Request.CreateResponse<dynamic>(HttpStatusCode.BadRequest,
                    new
                    {
                        Exception = exp.GetType().Name.ToString(),
                        Message = exp.Message
                    });
                //throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, exp.Message));
            }

        }

        [HttpGet]
        [Route("api/cleverdb/get/{id}")]
        public HttpResponseMessage Get(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);

            try
            {
                var result = db.FindById(id);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(result.ToString(), Encoding.UTF8, "application/json")
                };
            }
            catch (Exception exp)
            {
                return Request.CreateResponse<dynamic>(HttpStatusCode.InternalServerError,
                  new
                  {
                      Exception = exp.GetType().Name.ToString(),
                      Message = exp.Message
                  });
            }
        }

        [HttpGet]
        [Route("api/cleverdb/subtree/{id}")]
        public object GetSubtree(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            try
            {
                return Json(db.GetSubTreeForTheNode(id));
            }
            catch (Exception exp)
            {
                return Request.CreateResponse<dynamic>(HttpStatusCode.InternalServerError,
                new
                {
                    Exception = exp.GetType().Name.ToString(),
                    Message = exp.Message
                });
            }
        }

        [HttpPost]
        [Route("api/cleverdb/find")]
        public HttpResponseMessage FindObject(dynamic json)
        {
            if (json == null)
            {
                return Request.CreateResponse<dynamic>(HttpStatusCode.BadRequest,
                  new
                  {
                      Exception = "InvalidObjectFormatException",
                      Message = "Json object you have provided has invalid format"
                  });
            }
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            try
            {
                CleverQuery cq = new CleverQuery(json);
                var result = db.Find(cq);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(CleverObjectService.GetJsonFromCleverObjectArray(result), Encoding.UTF8, "application/json")
                };
            }
            catch (Exception exp)
            {
                return Request.CreateResponse<dynamic>(HttpStatusCode.BadRequest,
                new
                {
                    Exception = exp.GetType().Name.ToString(),
                    Message = exp.Message
                });
            }
        }
    }
}
