using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace CleverDb.Models
{
    public class CleverQuery
    {
        public IEnumerable<CleverCondition> ClassConditions { get; set; } = new List<CleverCondition>();
        public IEnumerable<CleverCondition> AttributesConditions { get; set; } = new List<CleverCondition>();



        public CleverQuery(dynamic json)
        {
            var convert = JsonConvert.SerializeObject(json);
            dynamic obj = Json.Decode<dynamic>(convert);

            var a = json.First;
            var b = json.First.First;
            foreach (var query in obj)
            {
                string entityType = new List<string>(query.Keys).First<string>();
                string fieldName =  new List<string>(query[entityType].Keys).First();
                string operationType = new List<string>(query[entityType][fieldName].Keys).First();
                dynamic value = query[entityType][fieldName][operationType];

                CleverCondition cd = new CleverCondition(fieldName, operationType, value);

                if (entityType == "class")
                {
                    ((List<CleverCondition>) ClassConditions).Add(cd);
                    
                } else if (entityType == "attribute")
                {
                    ((List<CleverCondition>)AttributesConditions).Add(cd);
                }
                else
                {
                    throw new Exception("Could not parse query");
                }
            }
        }
    }
}
