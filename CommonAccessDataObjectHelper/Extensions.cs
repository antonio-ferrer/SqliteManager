using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CommonAccessDataObjectHelper
{
    public static class Extensions
    {
        public static DbCommand CreateCommand(this DbConnection connection, StringSqlCommand sqlCommand)
        {
            var command = connection.CreateCommand();
            command.CommandText = sqlCommand;
            if (sqlCommand.HasParameter)
            {
                foreach(var p in sqlCommand)
                {
                    var commandParameter = command.CreateParameter();
                    commandParameter.ParameterName = p.Name;
                    commandParameter.Value = p.Value;
                    if (p.IsOutput)
                        commandParameter.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(commandParameter);
                }
            }
            return command;
        }

        public static DbDataReader CreateReader(this DbConnection connection, StringSqlCommand sqlCommand)
        {
            return CreateCommand(connection, sqlCommand).ExecuteReader();
        }

        public static DataTable GetDataTable(this DbConnection connection, StringSqlCommand sqlCommand)
        {
            var table = new DataTable();
            using (var reader = CreateReader(connection, sqlCommand))
            {
                table.Load(reader);
            }
            return table;
        }

        public static T[] GetElements<T>(this DbConnection connection, StringSqlCommand sqlCommand, Func<IDataRecord, T> parse)
        {
            var elements = new List<T>();
            using(var reader = CreateReader(connection, sqlCommand))
            {
                if (reader.Read())
                {
                    do
                    {
                        elements.Add(parse.Invoke(reader));
                    }
                    while (reader.Read());
                }
            }
            return elements.ToArray();
        }

        public static void BindOutputParameters(this DbParameterCollection dbParameters, StringSqlCommand sqlCommand)
        {
            foreach(DbParameter p in dbParameters)
            {
                var sParameter = sqlCommand.GetParameter(p.ParameterName);
                if(sParameter?.IsOutput ?? false)
                    sParameter.Value = p.Value;
            }
        }
    }
}
