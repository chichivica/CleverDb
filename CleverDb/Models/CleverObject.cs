using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverDb.Models
{
    public class CleverObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public List<CleverObjectAttribute> Attributes { get; set; }
        public CleverObject()
        {
            Attributes = new List<CleverObjectAttribute>();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            int counter = 1;
            result.AppendLine("{");
            result.AppendLine("\"id\" : " + Id + ",");
            result.AppendLine("\"name\" : \"" + Name + "\",");
            if (ParentId.HasValue)
            {
                result.AppendLine("\"parentId\" : " + ParentId + ",");
            }
            result.AppendLine("\"attributes\" :");
            result.AppendLine("{");
            foreach (var attribute in Attributes)
            {
                string value = "";
                switch (attribute.EnumType)
                {
                    case CleverObjectAttributeTypes.String:
                        result.Append(String.Format(" \"{0}\" : \"{1}\"", attribute.Name, attribute.StringValue));
                        break;
                    case CleverObjectAttributeTypes.DateTime:
                        result.Append(String.Format(" \"{0}\" : \"{1}\"", attribute.Name, attribute.DateTimeValue.ToString()));
                        break;
                    case CleverObjectAttributeTypes.Double:
                        result.Append(String.Format(" \"{0}\" : {1}", attribute.Name, attribute.DoubleValue.ToString().Replace(',', '.')));
                        break;
                }
                if (Attributes.Count > counter)
                {
                    result.AppendLine(",");
                }
                counter++;
            }
            result.AppendLine("} }");
            return result.ToString();
        }
    }
}
