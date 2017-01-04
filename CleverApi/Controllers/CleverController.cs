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
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "provide correct json object"));
            }
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            var inserted = db.Insert(json);
            return new HttpResponseMessage()
            {
                Content = new StringContent(inserted.ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        [Route("api/cleverdb/get/{id}")]
        public HttpResponseMessage Get(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            var result = db.FindById(id);

            return new HttpResponseMessage()
            {
                Content = new StringContent(result.ToString(), Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        [Route("api/cleverdb/subtree/{id}")]
        public object GetSubtree(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            return Json(db.GetSubTreeForTheNode(id));
        }

        [HttpPost]
        [Route("api/cleverdb/find")]
        public HttpResponseMessage FindObject(dynamic json)
        {
            if (json == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "provide correct json object"));
            }
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;

            CleverDbContext db = new CleverDbContext(connectionString);
            CleverQuery cq = new CleverQuery(json);
            try
            {
                var result = db.Find(cq);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(CleverObjectService.GetJsonFromCleverObjectArray(result), Encoding.UTF8, "application/json")
                };
            }
            catch (Exception exp)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, exp.ToString()));
            }
        }
    }
}
