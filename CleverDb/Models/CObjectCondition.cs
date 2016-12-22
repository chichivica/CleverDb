using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverDb.Models
{
    public class CObjectCondition : CleverCondition
    {
        public CObjectCondition(string name, string type, dynamic value) : base(name, type, (object)value)
        {
        }

        public override string GetSqlCondition(string tableName)
        {
            string result = $" {tableName}.{FieldName} {SqlOperator} ";
            if (Value.GetType() == typeof(int)
                    || Value.GetType() == typeof(decimal)
                    || Value.GetType() == typeof(float))
            {
                result += $"{Value}";
            }
            else
            {
                result += $"'{Value}'";
            }
            return result;

        }
    }
}
