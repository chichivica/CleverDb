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
        public virtual string GetSqlCondition(string tableName)
        {
            DateTime dtResult = new DateTime();
            bool parseResult = DateTime.TryParse(Value.ToString(), out dtResult);

            string result = $" {tableName}.Name = '{FieldName}' and ";
            if (Value.GetType() == typeof(int)
                    || Value.GetType() == typeof(decimal)
                    || Value.GetType() == typeof(float))
            {
                result += $" {tableName}.DoubleValue {SqlOperator} {Value} ";
            }
            else
            if (parseResult)
            {
                result += $" {tableName}.DateTimeValue {SqlOperator} '{Value}' ";
            }
            else
            {
                result += $" {tableName}.StringValue {SqlOperator} '{Value}' ";
            }

            return result;
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
