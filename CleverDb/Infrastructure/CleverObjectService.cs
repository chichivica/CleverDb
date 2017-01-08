using CleverDb.Exceptions;
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
        public static void CheckDynamicObjectConsistency(dynamic decodedObject)
        {
            if (decodedObject.name == null)
            {
                throw new InvalidObjectFormatException()
                {
                    ExceptionDetails = "Object doesn't have name property and/or it is wrong named"
                };
            }
            if (((string)decodedObject.name).Length > 255)
            {
                throw new InvalidObjectFormatException()
                {
                    ExceptionDetails = $"Object {decodedObject.name} has name with length greater then 255 simbols"
                };
            }

            if (decodedObject.attributes == null)
            {
                throw new InvalidObjectFormatException()
                {
                    ExceptionDetails = "Object doesn't have attributes"
                };
            }

            foreach (var item in decodedObject.attributes)
            {

                if (item == null)
                {
                    throw new InvalidObjectFormatException()
                    {
                        ExceptionDetails = "Empty attribute array"
                    };
                }
                if (item.Name == null)
                {
                    throw new InvalidObjectFormatException()
                    {
                        ExceptionDetails = $"in Object {decodedObject.name} any attribute name is not presented"
                    };
                }
                if (string.IsNullOrEmpty(item.Name.ToString()) || string.IsNullOrWhiteSpace(item.Name.ToString()))
                {
                    throw new InvalidObjectFormatException()
                    {
                        ExceptionDetails = $"Attribute name cannot be empty neither white space"
                    };
                }

                if (((string)item.Name).Length > 255)
                {
                    throw new InvalidObjectFormatException()
                    {
                        ExceptionDetails = $"Object {decodedObject.name} has Attribute {item.Name} name length higher then 255 characters"
                    };
                }
                if (item.Value.GetType() == typeof(string) || item.Value.Type.ToString() == "String")
                {
                    if (((string)item.Value).Length > 3000)
                    {
                        throw new InvalidObjectFormatException()
                        {
                            ExceptionDetails = $"Object {decodedObject.name} has Attribute {item.Name} has value with length greater then 3000 characters"
                        };
                    }
                }
            }
        }

        public static CleverObject GetCleverObjectFromDynamic(dynamic decodedObject)
        {
            CheckDynamicObjectConsistency(decodedObject);

            CleverObject result = new CleverObject();
            result.Name = decodedObject.name;
            result.ParentId = decodedObject.parentId;

            foreach (var attribute in decodedObject.attributes)
            {

                if (attribute.Value.GetType() == typeof(string) || attribute.Value.Type.ToString() == "String")
                {
                    DateTime newDate = new DateTime();
                    bool parseResult = DateTime.TryParse(attribute.Value.ToString(), out newDate);
                    if (parseResult)
                    {
                        result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.DateTime)
                        {
                            Name = attribute.Name,
                            DateTimeValue = newDate
                        });
                    }
                    else
                    {
                        result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.String)
                        {
                            Name = attribute.Name,
                            StringValue = attribute.Value
                        });
                    }

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
                else
                {
                    Double parseResult;
                    Double.TryParse(attribute.Value.ToString(), out parseResult);
                    result.Attributes.Add(new CleverObjectAttribute(CleverObjectAttributeTypes.Double)
                    {
                        Name = attribute.Name,
                        DoubleValue = parseResult
                    });
                }

            }
            return result;
        }


        public static string GetJsonFromCleverObjectArray(IEnumerable<CleverObject> listOfObjects)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("[");
            var totalAmount = listOfObjects.Count();
            int counter = 1;
            foreach (var item in listOfObjects)
            {
                result.AppendLine(item.ToString());
                if (counter < totalAmount)
                {
                    result.AppendLine(",");
                }
                counter++;
            }
            result.AppendLine("]");
            return result.ToString();
        }

        public static CleverObject GetCleverObjectFromJson(string json)
        {
            dynamic decodedObject = Json.Decode(json);
            return GetCleverObjectFromDynamic(decodedObject);
        }

        public static string CreateJsonFromDynamicObj(dynamic obj)
        {
            return Json.Encode(obj);
        }

    }
}
