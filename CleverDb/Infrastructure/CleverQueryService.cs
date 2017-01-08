using CleverDb.Exceptions;
using CleverDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverDb.Infrastructure
{
    public class CleverQueryService
    {
        public static void CheckDynamicObjectConsistency(dynamic decodedQuery)
        {
            //throw new InvalidQueryFormatException;
            try
            {
                if (decodedQuery.GetType() != typeof(Object[]))
                {
                    throw new InvalidQueryFormatException
                    {
                        ExceptionDetails = "Query array has no elements or has bad format"
                    };
                }
                foreach (var query in decodedQuery)
                {


                    string entityType = new List<string>(query.Keys).First<string>();
                    string fieldName = new List<string>(query[entityType].Keys).First();
                    string operationType = new List<string>(query[entityType][fieldName].Keys).First();
                    dynamic value = query[entityType][fieldName][operationType];

                    if (entityType != "class" && entityType != "attribute")
                    {
                        throw new InvalidQueryFormatException
                        {
                            ExceptionDetails = "query type you have provided is invalid. You must query class or its attributes"
                        };
                    }

                    if (!CleverCondition.StringArrayOfOperators().Contains(operationType))
                    {
                        throw new InvalidQueryFormatException
                        {
                            ExceptionDetails = "operation type you have provided is not supported. Look up the documentation to see legal operations"
                        };
                    }
                }
            }
            catch (InvalidQueryFormatException)
            {
                throw;
            }
            catch (Exception exp)
            {
                throw new InvalidQueryFormatException
                {
                    ExceptionDetails = $"During query parse there was an error {exp.Message}"
                };
            }
        }
    }
}
