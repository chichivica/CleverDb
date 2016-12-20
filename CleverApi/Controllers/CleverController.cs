using CleverDb;
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
            var q = db.Insert(json);
            return Json(q);
        }

        [HttpGet]
        [Route("customers/{customerId}/orders")]
        public void SomAction(int customerId)
        {
            int q = 2;

        }
    }
}
