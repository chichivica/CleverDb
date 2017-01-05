using CleverDb.Infrastructure;
using CleverDb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

namespace CleverDb
{
    public class CleverDbContext
    {
        public string ConnectionString { get; set; }
        public CleverDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public CleverObject Insert(dynamic obj)
        {
            CleverObject result = CleverObjectService.GetCleverObjectFromDynamic(obj);
            return Insert(result);
        }

        public CleverObject Insert(CleverObject co)
        {
            string queryString = @"
                insert into [CleverObjects] (Name, ParentId)
                output INSERTED.ID
                values (@objectName, @parentId)";

            int insertedId;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection)
                {
                    CommandType = CommandType.Text
                };
                command.Parameters.AddWithValue("@objectName", co.Name);
                command.Parameters.AddWithValue("@parentId", (object)co.ParentId ?? DBNull.Value);
                try
                {
                    connection.Open();
                    insertedId = (int)command.ExecuteScalar();
                }
                finally
                {
                    connection.Close();
                }

            }
            co.Id = insertedId;
            InsertAttributes(co);

            queryString = @" CreatePathForObject @objId";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection)
                {
                    CommandType = CommandType.Text
                };
                command.Parameters.AddWithValue("@objId", insertedId);
                try
                {
                    connection.Open();
                    command.ExecuteScalar();
                }
                finally
                {
                    connection.Close();
                }

            }



            return co;
        }

        public CleverObject FindById(int id)
        {
            CleverObject result = null;

            string queryString = "select * from  [CleverObjects] where id = @objectId  ";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@objectId", id);

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = command;
                DataSet ds = new DataSet();

                try
                {
                    connection.Open();
                    dataAdapter.Fill(ds);
                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        result = ds.Tables[0].AsEnumerable().Select(dataRow =>
                        new CleverObject()
                        {
                            Id = dataRow.Field<int>("Id"),
                            Name = dataRow.Field<string>("Name"),
                            ParentId = dataRow.Field<int?>("ParentId")
                        }).First<CleverObject>();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            return PopulateAttributes(result);
        }

        public dynamic GetSubTreeForTheNode(int id)
        {
            string queryString = "exec [GetSubTreeForTheNode] @objectId";
            dynamic result = new ExpandoObject();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@objectId", id);

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = command;
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    dataAdapter.Fill(ds);

                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        List<dynamic> obj = ds.Tables[0].AsEnumerable().Select(dataRow =>
                        new
                        {
                            Id = dataRow.Field<int>("Id"),
                            Name = dataRow.Field<string>("Name"),
                            Depth = dataRow.Field<int>("Depth"),
                            ParentId = dataRow.Field<int?>("ParentId"),
                            Children = new List<dynamic>()
                        }).ToList<object>();

                        result = obj.Find(f => f.Id == id);

                        BuildTheTreeRecurs(result, obj);
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return result;
        }

        void BuildTheTreeRecurs(dynamic obj, List<dynamic> source)
        {
            obj.Children.AddRange(source.FindAll(f => f.ParentId == obj.Id));
            if (obj.Children.Count == 0)
            {
                return;
            }
            else
            {
                foreach (dynamic child in obj.Children)
                {
                    BuildTheTreeRecurs(child, source);
                }
            }
        }

        CleverObject PopulateAttributes(CleverObject co)
        {
            string queryString = "select * from  [CleverObjectAttributes] where CleverObjectId = @objectId";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@objectId", co.Id);

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = command;
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    dataAdapter.Fill(ds);
                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        co.Attributes = ds.Tables[0].AsEnumerable().Select(dataRow =>
                        new CleverObjectAttribute(dataRow.Field<string>("Type"))
                        {
                            Id = dataRow.Field<int>("Id"),
                            Name = dataRow.Field<string>("Name"),
                            StringValue = dataRow.Field<string>("StringValue"),
                            DoubleValue = dataRow.Field<double?>("DoubleValue"),
                            DateTimeValue = dataRow.Field<DateTime?>("DateTimeValue"),
                            CleverObjectId = dataRow.Field<int>("CleverObjectId")
                        }).ToList();
                    }

                }
                finally
                {
                    connection.Close();
                }

            }
            return co;
        }

        void InsertAttributes(CleverObject co)
        {
            string queryString = "Declare @DataTable as CleverAttributeType";
            foreach (var attribute in co.Attributes)
            {
                queryString += " insert into @DataTable(Name, StringValue, DateTimeValue, DoubleValue, Type, CleverObjectId) ";

                switch (attribute.EnumType)
                {
                    case CleverObjectAttributeTypes.String:
                        queryString += $@"values('{attribute.Name}', '{attribute.StringValue}', null, null, 'String', {co.Id})";
                        break;
                    case CleverObjectAttributeTypes.DateTime:
                        queryString += $@"values('{attribute.Name}',null, '{attribute.DateTimeValue.Value.ToString("yyyy-MM-dd hh:mm:ss")}', null, 'DateTime', {co.Id})";
                        break;
                    case CleverObjectAttributeTypes.Double:
                        queryString += $@"values('{attribute.Name}',null, null, {attribute.DoubleValue.ToString().Replace(',', '.')}, 'Double', {co.Id})";
                        break;
                    default:
                        break;
                }
            }

            queryString += " EXECUTE InsertCleverObjectAttributes @TableVariable = @DataTable";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
            }
        }

        public IEnumerable<CleverObject> Find(CleverQuery cq)
        {
            if (cq.IsEmpty) return null;

            string queryString = "select ob.Id as 'Id', ob.Name, ob.ParentId, at.Id as 'AttributeId', at.Name as 'AttributeName'," +
                " at.StringValue, at.DateTimeValue, at.DoubleValue, at.Type, at.CleverObjectId from [CleverObjects] ob " +
                " left join [CleverObjectAttributes] at on ob.Id = at.CleverObjectId " +
                " where ob.Id in ( " +
                " select distinct (obj.Id) from [CleverObjects] obj " +
                " left join [CleverObjectAttributes] atr on obj.Id = atr.CleverObjectId ";

            if (cq.ObjectConditions.ToList().Count > 0)
            {
                queryString += " where ";
                int counter = 1;
                foreach (var query in cq.ObjectConditions)
                {
                    queryString += query.GetSqlCondition("obj");
                    if (counter < cq.ObjectConditions.Count())
                        queryString += " and ";
                    counter++;
                }
            }
            if (cq.AttributesConditions.ToList().Count > 0)
            {
                if (cq.ObjectConditions.ToList().Count == 0)
                {
                    queryString += " where ";
                }
                else
                {
                    queryString += " and ";
                }
                int count = 1;
                foreach (var query in cq.AttributesConditions)
                {
                    queryString += query.GetSqlCondition("atr");
                    if (count < cq.AttributesConditions.Count())
                        queryString += " and ";
                    count++;
                }
            }
            queryString += ") order by ob.Id";

            List<int> appropriateObjects = new List<int>();
            Stack<CleverObject> stack = new Stack<CleverObject>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = command;
                DataSet ds = new DataSet();

                try
                {
                    connection.Open();
                    dataAdapter.Fill(ds);
                    if (ds.Tables[0].Rows.Count != 0)
                    {

                        foreach (var dataRow in ds.Tables[0].AsEnumerable())
                        {
                            int objectId = dataRow.Field<int>("Id");
                            string objectName = dataRow.Field<string>("Name");
                            int? objectPartentId = dataRow.Field<int?>("ParentId");
                            var attribute = new CleverObjectAttribute(dataRow.Field<string>("Type"))
                            {
                                Id = dataRow.Field<int>("AttributeId"),
                                Name = dataRow.Field<string>("AttributeName"),
                                StringValue = dataRow.Field<string>("StringValue"),
                                DoubleValue = dataRow.Field<double?>("DoubleValue"),
                                DateTimeValue = dataRow.Field<DateTime?>("DateTimeValue"),
                                CleverObjectId = dataRow.Field<int>("CleverObjectId")
                            };

                            if (stack.Count == 0)
                            {
                                stack.Push(new CleverObject()
                                {
                                    Id = objectId,
                                    ParentId = objectPartentId,
                                    Name = objectName
                                });
                            }
                            var stackedObject = stack.Peek();
                            if (stackedObject.Id != objectId)
                            {
                                stack.Push(new CleverObject()
                                {
                                    Id = objectId,
                                    ParentId = objectPartentId,
                                    Name = objectName
                                });
                                stackedObject = stack.Peek();
                            }
                            stackedObject.Attributes.Add(attribute);
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            return stack.AsEnumerable();
        }
    }
}
