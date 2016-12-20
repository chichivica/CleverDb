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
        public static CleverObject GetCleverObjectFromDynamic(dynamic decodedObject)
        {

            CleverObject result = new CleverObject();
            result.Name = decodedObject.name;
            result.ParentId = decodedObject.parentId;

            foreach (var attribute in decodedObject.attributes)
            {

                if (attribute.Value.GetType() == typeof(string) || attribute.Value.Type.ToString() == "String")
                {
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.String)
                    {
                        Name = attribute.Name,
                        StringValue = attribute.Value
                    });
                }
                else
                if (attribute.Value.GetType() == typeof(Decimal) || attribute.Value.GetType() == typeof(Int32) || attribute.Value.Type.ToString() == "Float")
                {
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.Double)
                    {
                        Name = attribute.Name,
                        DoubleValue = (double)attribute.Value
                    });
                }
                else
                if (attribute.Value.GetType() == typeof(DateTime) || attribute.Value.Type.ToString() == "Date")
                {
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.DateTime)
                    {
                        Name = attribute.Name,
                        DateTimeValue = attribute.Value
                    });
                }

            }
            return result;
        }

        public static CleverObject GetCleverObjectFromJson(string json)
        {
            dynamic decodedObject = Json.Decode(json);
            return GetCleverObjectFromDynamic(decodedObject);
        }

        public static string CreateJsonFromDynamicObj (dynamic obj)
        {
            return Json.Encode(obj);
        }

    }
}
