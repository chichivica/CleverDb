using CleverDb;
using CleverDb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CleverApi.Controllers
{
    public class CleverDbController : ApiController
    {
        [HttpPost]
        [Route("api/cleverdb/insert")]
        public object Insert(dynamic json)
        {
            if (json == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "provide correct json object"));
            }
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            return Json(db.Insert(json));
        }

        [HttpGet]
        [Route("customers/{customerId}/orders")]
        public void SomAction(int customerId)
        {
            int q = 2;

        }

        [HttpGet]
        [Route("api/cleverdb/get/{id}")]
        public object Get(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            return Json(db.FindById(id));
        }

        [HttpGet]
        [Route("api/cleverdb/subtree/{id}")]
        public object GetSubtree(int id)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            CleverDbContext db = new CleverDbContext(connectionString);
            var q = db.GetSubTreeForTheNode(id);
            return Json(q);
        }

        [HttpPost]
        [Route("api/cleverdb/find")]
        public object FindObject(dynamic json)
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
                return Json(result);
            }
            catch (Exception exp)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, exp.ToString()));
            }
        }
    }
}
