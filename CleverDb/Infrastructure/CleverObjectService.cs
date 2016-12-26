using CleverDb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Web.Helpers;

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

        public static dynamic GetDynamicFromCleverObject(CleverObject obj)
        {
            StringBuilder result = new StringBuilder();
            dynamic expandObject = new ExpandoObject();
            result.AppendLine("{ id : " + obj.Id + ",");
            result.AppendLine(" name : \"" + obj.Name + "\",");
            result.AppendLine(" parentId : " + obj.ParentId+ ",");
            result.AppendLine(" attributes : {");
            int counter = 1;
            foreach (var attribute in obj.Attributes)
            {
                string value = "";
                switch (attribute.EnumType)
                {
                    case CleverObjectAttributeTypes.String:
                        result.AppendLine(String.Format(" \"{0}\" : \"{1}\"", attribute.Name, attribute.StringValue));
                        break;
                    case CleverObjectAttributeTypes.DateTime:
                        result.AppendLine(String.Format(" \"{0}\" : \"{1}\"", attribute.Name, attribute.DateTimeValue.ToString()));
                        break;
                    case CleverObjectAttributeTypes.Double:
                        result.AppendLine(String.Format(" \"{0}\" : {1}", attribute.Name, attribute.DoubleValue.ToString().Replace(',','.')));
                        break;
                }
                if (obj.Attributes.Count > counter)
                {
                    result.Append(",");
                }
                counter++;
            }
            result.Append("}}");
            var q = JsonConvert.DeserializeObject<dynamic>(result.ToString());
            return q;
        }
        public static IEnumerable<dynamic> GetDynamicFromCleverObject(IEnumerable<CleverObject> listOfObjects)
        {
            List<dynamic> result = new List<dynamic>();
            foreach (var item in listOfObjects)
            {
                result.Add(GetDynamicFromCleverObject(item));
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
