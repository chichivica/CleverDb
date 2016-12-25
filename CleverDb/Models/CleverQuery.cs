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
        public IEnumerable<CleverCondition> ObjectConditions { get; set; } = new List<CObjectCondition>();
        public IEnumerable<CleverCondition> AttributesConditions { get; set; } = new List<CleverCondition>();

        public bool IsEmpty
        {
            get
            {
                return ObjectConditions.Count() == 0 && AttributesConditions.Count() == 0;
            }
        }



        public CleverQuery(dynamic json)
        {
            var convert = JsonConvert.SerializeObject(json);
            dynamic obj = Json.Decode<dynamic>(convert);

            var a = json.First;
            var b = json.First.First;
            foreach (var query in obj)
            {
                string entityType = new List<string>(query.Keys).First<string>();
                string fieldName = new List<string>(query[entityType].Keys).First();
                string operationType = new List<string>(query[entityType][fieldName].Keys).First();
                dynamic value = query[entityType][fieldName][operationType];

                if (entityType == "class")
                {
                    ((List<CObjectCondition>)ObjectConditions)
                        .Add(new CObjectCondition(fieldName, operationType, value));

                }
                else if (entityType == "attribute")
                {
                    ((List<CleverCondition>)AttributesConditions)
                        .Add(new CleverCondition(fieldName, operationType, value));
                }
                else
                {
                    throw new Exception("Could not parse query");
                }
            }
        }
    }
}
