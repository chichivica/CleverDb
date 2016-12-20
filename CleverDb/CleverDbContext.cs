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
                insert into [CleverDb].[dbo].[CleverObjects](Name, ParentId)
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

            queryString = @" buildPathForObject @objId";
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

        public string GetSubTreeForTheNode(int id)
        {
            string queryString = "exec [dbo].[getSubTreeForTheNode] @objectId";
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
            return CleverObjectService.CreateJsonFromDynamicObj(result);
        }

        void BuildTheTreeRecurs (dynamic obj, List<dynamic> source)
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

            queryString += " EXECUTE insertCleverObjectAttributes @TableVariable = @DataTable";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                //command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
            }
        }
    }
}
