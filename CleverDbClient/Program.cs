using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleverDb;
using System.Web.Script.Serialization;
using System.Web.Helpers;
using CleverDb.Infrastructure;
using CleverDb.Models;

namespace CleverDbClient
{
    class Program
    {

        static void Main(string[] args)
        {
            string connectionString = "Data Source = (local);Initial Catalog = CleverDb;Integrated Security = True;MultipleActiveResultSets = True";

            CleverDbContext db = new CleverDbContext(connectionString);
            
            var obj = new {
                name = "Насос",
                parentId = 95,
                attributes =  new {
                    марка = "Bosh",
                    мощность = 95.3,
                    ДатаУстановки = DateTime.Now
                }
            };
            string stringified = Json.Encode(obj);
            var clever = CleverObjectService.GetCleverObjectFromJson(stringified);

            //insert operation
            CleverObject inserted = db.Insert(clever);
            Console.WriteLine(inserted.Id);

            //get operation
            CleverObject found = db.FindById(inserted.Id);

            //get subtree operation
            string json = db.GetSubTreeForTheNode(166);

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
