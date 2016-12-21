using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleverDb.Infrastructure;
using System.ComponentModel;

namespace CleverDb.Models
{
    public enum ConditionTypes
    {
        [Description("=")]
        Equal,
        [Description("<")]
        Less,
        [Description(">")]
        Greater,
        [Description("like")]
        Contains
    }

    public class CleverCondition
    {
        public string FieldName { get; set; }
        public dynamic Value { get; set; }
        public ConditionTypes Type { get; set; } = ConditionTypes.Equal;
        public string SqlOperator
        {
            get
            {
                return Type.GetDescription<ConditionTypes>();
            }
        }
        public CleverCondition(string name, string type, dynamic value)
        {
            switch (type)
            {
                case "$eq":
                    Type = ConditionTypes.Equal;
                    break;
                case "$lt":
                    Type = ConditionTypes.Less;
                    break;
                case "$gt":
                    Type = ConditionTypes.Greater;
                    break;
                case "$in":
                    Type = ConditionTypes.Contains;
                    break;
                default:
                    break;
            }
            FieldName = name;
            Value = value;
        }
    }
}
