using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace NatLib.DB
{
    public class SqLite : IDisposable
    {
        private bool _disposed = false;
        public SQLiteConnectionStringBuilder ConString { get; set; }
        public string Location { get; set; }
        public bool Vacuum { get; set; }
        public string Password { get; set; }
        //public string Folder { get; set; }

        public SqLite()
        {
            Vacuum = true;
            Password = ConfigurationManager.AppSettings.Get("SQLitePassword");
            //Folder = "Db";
            Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Db");
        }

        public int Version
        {
            get { return ConString?.Version ?? 0; }
            set
            {
                if (ConString == null)
                {
                    ConString = new SQLiteConnectionStringBuilder
                    {
                        DataSource = Path.Combine(Location, Guid.NewGuid().ToString() + ".db3")
                    };
                }
                ConString.Version = value;
            }
        }

        public string DataSource
        {
            get { return ConString?.DataSource; }
            set
            {
                if (ConString == null)
                {
                    ConString = new SQLiteConnectionStringBuilder
                    {
                        Version = 3
                    };
                }
                ConString.DataSource = Path.Combine(Location, value);
            }
        }

        public string Folder
        {
            get { return Folder; }
            set
            {
                Folder = value;
                Location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, value);
                //ConString.DataSource = Path.Combine(Location, value);
            }
        }


        public SQLiteConnection Connection(SQLiteConnectionStringBuilder conString = null, bool open = true)
        {
            try
            {
                var con = new SQLiteConnection();
                Func<string, string> config = ConfigurationManager.AppSettings.Get;
                if (!Directory.Exists(Location) && !string.IsNullOrEmpty(Location)) Directory.CreateDirectory(Location);

                if (conString == null)
                    ConString = new SQLiteConnectionStringBuilder
                    {
                        DataSource = Path.Combine(Location, (config("SQLiteDataSource") ?? Guid.NewGuid().ToString() + ".db3")),
                        Version = Convert.ToInt32(config("SQLiteVersion") ?? "3")
                    };
                else
                {
                    ConString = conString;
                }

                con.ConnectionString = ConString.ConnectionString;

                if (Password != null) con.SetPassword(Password);

                if (open) con.Open();
                return con;

            }
            catch (Exception ex)
            {
                ex.Message.Log();
                throw;
            }
        }

        #region Method for running sql command
        public DataSet SqlExecCommand(string command)
        {
            var dataSet = new DataSet();
            using (var con = Connection(ConString))
            {
                using (var com = con.CreateCommand())
                {
                    com.CommandType = CommandType.Text;

                    command = VacuumFiltered(command);

                    com.CommandText = command;
                    using (var adapter = new SQLiteDataAdapter
                    {
                        SelectCommand = com,
                        MissingSchemaAction = MissingSchemaAction.AddWithKey
                    })
                        adapter.Fill(dataSet);
                }
                //if (hasSchema) adapter.FillSchema(dataSet, SchemaType.Source);
            }
            return dataSet;
        }

        public DataSet SqlExecCommand(string command, Dictionary<string, object> param)
        {
            var dataSet = new DataSet();
            using (var con = Connection(ConString))
            {
                using (var com = con.CreateCommand())
                {
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandText = VacuumFiltered(command);
                    foreach (var item in param)
                        com.Parameters.Add(item.Key.SqlParamName(), (DbType)item.Value);

                    using (var adapter = new SQLiteDataAdapter
                    {
                        SelectCommand = com,
                        MissingSchemaAction = MissingSchemaAction.AddWithKey
                    })
                        adapter.Fill(dataSet);
                }

                //if (hasSchema) adapter.FillSchema(dataSet, SchemaType.Source);
            }
            return dataSet;
        }

        public void SqlNoRetCommand(string command)
        {
            var dataTable = new DataTable();
            using (var con = Connection(ConString))
            {
                using (var comm = con.CreateCommand())
                {
                    comm.CommandType = CommandType.Text;
                    comm.CommandText = VacuumFiltered(command);
                    comm.ExecuteNonQuery();
                }
            }
        }

        public object SqlScalarCommand(string command)
        {
            object result;
            using (var con = Connection(ConString))
            {
                using (var comm = con.CreateCommand())
                {
                    comm.CommandType = CommandType.Text;
                    comm.CommandText = VacuumFiltered(command);
                    result = comm.ExecuteScalar();
                }
            }
            return result;
        }
        #endregion

        #region Password Maintenance

        public void ChangePassword(string newPass)
        {
            using (var con = Connection(ConString))
            {
                con.ChangePassword(newPass);
                Password = newPass;
            }
        }

        public void RemovePassword()
        {
            using (var con = Connection(ConString)) 
            {
                con.ChangePassword("");
                Password = null;
            }
        }

        #endregion

        private string VacuumFiltered(string command)
        {
            if (Vacuum)
            {
                var len = command.Length;
                if (command.Substring(len - 2).IndexOf(';') == -1)
                    command = command + ";";

                command = command + Environment.NewLine + "VACUUM;";
            }
            return command;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {

            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqLite()
        {
            Dispose(false);
        }
    }
}
