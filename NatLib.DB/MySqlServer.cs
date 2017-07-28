using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using NatLib.EventsArgs;

namespace NatLib.DB
{
    /// <summary>
    /// NAT20161117 object for manipulating mysql server database
    /// </summary>
    public class MySqlServer : IDisposable
    {
        private bool _disposed = false;
        #region Properties
        public MySqlConnectionStringBuilder ConString { get; set; }
        public MySqlConnection MySqlConnection { get; set; }
        public MySqlTransaction MySqlTransaction { get; set; }

        #endregion

        #region Constructor        
        public MySqlServer()
        {
            
        }
        #endregion

        #region Methods

        public void Initialize(MySqlConnectionStringBuilder conString = null)
        {
            try
            {
                MySqlConnection = new MySqlConnection();
                ConString = ConnectionBuilder(conString);
                MySqlConnection.ConnectionString = ConString.ConnectionString;
                MySqlConnection.Open();
                MySqlTransaction = MySqlConnection.BeginTransaction();
                Initialized?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                (ex.Message + " - MySqlServer Constructor").Log();
                InitializeFailed?.Invoke(this, new ExceptionEventsArgs { Exception = ex });
            }
        }

        private MySqlConnectionStringBuilder ConnectionBuilder(MySqlConnectionStringBuilder conString = null)
        {
            MySqlConnectionStringBuilder conBuilder;
            Func<string, string> config = ConfigurationManager.AppSettings.Get;
            if (conString == null)
                conBuilder = new MySqlConnectionStringBuilder()
                {
                    Server = config("Server"),
                    UserID = config("UserID"),
                    Password = config("Password"),
                    Database = config("Database"),
                    Port = Convert.ToUInt32(config("Port"))
                };
            else
                conBuilder = conString;

            return conBuilder;
        }

        protected virtual MySqlConnection Connection(MySqlConnectionStringBuilder conString = null)
        {
            try
            {
                var con = new MySqlConnection();
                ConString = ConnectionBuilder(conString);

                con.ConnectionString = ConString.ConnectionString;
                con.Open();
                return con;

            }
            catch (Exception ex)
            {
                ex.Message.Log();
                throw;
            }
        }

        public DataSet SqlExecCommand(string command)
        {
            var dataSet = new DataSet();
            using (var con = Connection())
            {
                var com = con.CreateCommand();
                com.CommandType = CommandType.Text;
                com.CommandText = command;
                var adapter = new MySqlDataAdapter
                {
                    SelectCommand = com,
                    MissingSchemaAction = MissingSchemaAction.AddWithKey
                };
                adapter.Fill(dataSet);
                //if (hasSchema) adapter.FillSchema(dataSet, SchemaType.Source);
            }
            return dataSet;
        }

        public DataSet SqlExecCommand(string command, Dictionary<string, object> param)
        {
            var dataSet = new DataSet();
            using (var con = Connection())
            {
                var com = con.CreateCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = command;
                foreach (var item in param)
                    com.Parameters.AddWithValue(item.Key.SqlParamName(), item.Value);

                var adapter = new MySqlDataAdapter
                {
                    SelectCommand = com,
                    MissingSchemaAction = MissingSchemaAction.AddWithKey
                };
                adapter.Fill(dataSet);
                //if (hasSchema) adapter.FillSchema(dataSet, SchemaType.Source);
            }
            return dataSet;
        }

        public void SqlNoRetCommand(string command)
        {
            var dataTable = new DataTable();
            using (var con = Connection())
            {
                var comm = con.CreateCommand();
                comm.CommandType = CommandType.Text;
                comm.CommandText = command;
                comm.ExecuteNonQuery();
            }
        }

        public object SqlScalarCommand(string command)
        {
            object result;
            using (var con = Connection())
            {
                var comm = con.CreateCommand();
                comm.CommandType = CommandType.Text;
                comm.CommandText = command;
                result = comm.ExecuteScalar();
            }
            return result;
        }

        #endregion

        #region Events

        public event EventHandler<ExceptionEventsArgs> InitializeFailed;
        public event EventHandler<EventArgs> Initialized;

        #endregion


        #region Dispose Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (MySqlConnection?.State == ConnectionState.Open) MySqlConnection.Close();
                MySqlConnection?.Dispose();
                MySqlTransaction?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MySqlServer()
        {
            Dispose(false);
        } 
        #endregion
    }
}