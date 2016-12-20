using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverDb.Models
{
    public enum CleverObjectAttributeTypes
    {
        String,
        DateTime,
        Double
    }
    public class CleverObjectAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StringValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public Double? DoubleValue { get; set; }
        public String Type { get; set; }

        public int CleverObjectId { get; set; }
        public CleverObjectAttributeTypes EnumType { get; set; }

        public CleverObjectAttribute(CleverObjectAttributeTypes type)
        {
            switch (type)
            {
                case CleverObjectAttributeTypes.String:
                    Type = "String";
                    break;
                case CleverObjectAttributeTypes.DateTime:
                    Type = "DateTime";
                    break;
                case CleverObjectAttributeTypes.Double:
                    Type = "Double";
                    break;
                default:
                    break;
            }
            EnumType = type;
            StringValue = null;
            DateTimeValue = null;
            DoubleValue = null;
        }

        public CleverObjectAttribute(String type)
        {
            Type = type;
            switch (type)
            {
                case "String":
                    Type = "String";
                    EnumType = CleverObjectAttributeTypes.String;
                    break;
                case "DateTime":
                    Type = "DateTime";
                    EnumType = CleverObjectAttributeTypes.DateTime;
                    break;
                case "Double":
                    Type = "Double";
                    EnumType = CleverObjectAttributeTypes.Double;
                    break;
                default:
                    break;
            }
            StringValue = null;
            DateTimeValue = null;
            DoubleValue = null;
        }

    }
}
