using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DatabaseMapper
{
    /// <summary>
    /// Maps stored procedure to any object, with no use of relection
    /// Field names of returned columns and property names of classes have to be identical.
    ///
    /// useage:
    /// long postID = 123;
    /// List<DbParameter> parameters = new List<DbParameter>(); 
    /// parameters.Add(new DbParameter("postID", System.Data.ParameterDirection.Input, postID));
    /// List<Category> categories = DbManager.ExecuteList<Category>("GetCategories", parameters);
    /// </summary>
    public class DatabaseMapper
    {
        private readonly string _connectionString;
        private SqlConnection Connection { get; set; }
        private SqlCommand Command { get; set; }
        public List<DbParameter> Parameters { get; private set; }

        public DatabaseMapper(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public enum ExecuteType
        {
            ExecuteReader,
            ExecuteNonQuery,
            ExecuteScalar
        };

        public class DbParameter
        {
            public string Name { get; set; }
            public ParameterDirection Direction { get; set; }
            public object Value { get; set; }

            public DbParameter(string parameterName, ParameterDirection parameterDirection, object parameterValue)
            {
                Name = parameterName;
                Direction = parameterDirection;
                Value = parameterValue;
            }
        }

        private void Open()
        {
            try
            {
                Connection = new SqlConnection(_connectionString);
                Connection.Open();
            }
            catch
            {
                Close();
            }
        }

        private void Close()
        {
            if (Connection != null)
            {
                Connection.Close();
            }
        }

        private object ExecuteProcedure(string procedureName, ExecuteType executeType, List<DbParameter> parameters)
        {
            object returnObject = null;

            if (Connection == null) Open();
            Command = new SqlCommand(procedureName, Connection) {CommandType = CommandType.StoredProcedure};

            if (parameters != null)
            {
                Command.Parameters.Clear();

                foreach (var parameter in parameters.Select(dbParameter => new SqlParameter
                {
                    ParameterName = "@" + dbParameter.Name,
                    Direction = dbParameter.Direction,
                    Value = dbParameter.Value
                }))
                {
                    Command.Parameters.Add(parameter);
                }
            }

            switch (executeType)
            {
                case ExecuteType.ExecuteReader:
                    returnObject = Command.ExecuteReader();
                    break;
                case ExecuteType.ExecuteNonQuery:
                    returnObject = Command.ExecuteNonQuery();
                    break;
                case ExecuteType.ExecuteScalar:
                    returnObject = Command.ExecuteScalar();
                    break;
            }

            return returnObject;
        }

        // updates output parameters from stored procedure
        private void UpdateOutParameters()
        {
            if (Command.Parameters.Count <= 0) return;
            Parameters = new List<DbParameter>();
            Parameters.Clear();

            for (var index = 0; index < Command.Parameters.Count; index++)
            {
                if (Command.Parameters[index].Direction == ParameterDirection.Output)
                {
                    Parameters.Add(new DbParameter(Command.Parameters[index].ParameterName,
                        ParameterDirection.Output,
                        Command.Parameters[index].Value));
                }
            }
        }

        public T ExecuteSingle<T>(string procedureName) where T : new()
        {
            return ExecuteSingle<T>(procedureName, null);
        }

        public T ExecuteSingle<T>(string procedureName, List<DbParameter> parameters) where T : new()
        {
            Open();
            var reader = (IDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);
            var currentObject = new T();

            if (reader.Read())
            {
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    var propertyInfo = typeof(T).GetProperty(reader.GetName(index));
                    propertyInfo.SetValue(currentObject, reader.GetValue(index), null);
                }
            }

            reader.Close();
            UpdateOutParameters();
            Close();

            return currentObject;
        }

        // executes list query stored procedure without parameters
        public List<T> ExecuteList<T>(string procedureName) where T : new()
        {
            return ExecuteList<T>(procedureName, null);
        }

        // executes list query stored procedure and maps result generic list of objects
        public List<T> ExecuteList<T>(string procedureName, List<DbParameter> parameters) where T : new()
        {
            var objects = new List<T>();

            Open();
            var reader = (IDataReader)ExecuteProcedure(procedureName, ExecuteType.ExecuteReader, parameters);

            while (reader.Read())
            {
                var currentObject = new T();

                for (var index = 0; index < reader.FieldCount; index++)
                {
                    if (reader.GetValue(index) == DBNull.Value) continue;
                    var propertyInfo = typeof(T).GetProperty(reader.GetName(index));
                    propertyInfo.SetValue(currentObject, reader.GetValue(index), null);
                }

                objects.Add(currentObject);
            }

            reader.Close();
            UpdateOutParameters();
            Close();

            return objects;
        }

        public int ExecuteNonQuery(string procedureName, List<DbParameter> parameters)
        {
            Open();

            var returnValue = (int)ExecuteProcedure(procedureName, ExecuteType.ExecuteNonQuery, parameters);

            UpdateOutParameters();
            Close();

            return returnValue;
        }
    }
}