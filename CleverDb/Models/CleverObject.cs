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
    }
}
