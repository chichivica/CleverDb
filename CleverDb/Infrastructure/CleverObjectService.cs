using CleverDb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace CleverDb.Infrastructure
{
    public class CleverObjectService
    {
        public static CleverObject GetCleverObjectFromJson(string json)
        {
            CleverObject result = new CleverObject();
            dynamic decodedObject = Json.Decode(json);
            result.Name = decodedObject.name;
            result.ParentId = decodedObject.parentId;

            foreach (var attribute in decodedObject.attributes[0])
            {
                
                if (attribute.Value.GetType() == typeof(string))
                {
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.String)
                    {
                        Name = attribute.Key,
                        StringValue = attribute.Value
                    });
                }
                else
                if (attribute.Value.GetType() == typeof(Decimal) || attribute.Value.GetType() == typeof(Int32))
                {
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.Double)
                    {
                        Name = attribute.Key,
                        DoubleValue = (double)attribute.Value
                    });
                }
                else
                if (attribute.Value.GetType() == typeof(DateTime))
                {
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.DateTime)
                    {
                        Name = attribute.Key,
                        DateTimeValue = attribute.Value
                    });
                }

            }
            return result;
        }

        public static string CreateJsonFromDynamicObj (dynamic obj)
        {
            return Json.Encode(obj);
        }

    }
}
