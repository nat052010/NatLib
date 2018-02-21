using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NatLib.EventsArgs;

namespace NatLib.DB
{
    /// <summary>
    /// nat20161117 object for manipulating ms sql server database
    /// </summary>
    public class MsSqlServer : IDisposable
    {
        private bool _disposed;

        #region Properties
        public SqlConnectionStringBuilder ConString { get; set; }
        public SqlConnection SqlConnection { get; set; }
        public SqlTransaction SqlTransaction { get; set; }
        #endregion

        #region Constructor
        public MsSqlServer(SqlConnectionStringBuilder conString = null)
        {
            ConString = conString;
            SqlExecCommand(Script());
        }
        #endregion

        #region Methods

        public virtual string Script()
        {
            var script = new List<string>
            {
                "IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'fCommaValToTable')",
                "BEGIN",
                "EXEC('",
                "CREATE FUNCTION dbo.fCommaValToTable(@idRandomTables VARCHAR(MAX)=NULL)",
                "RETURNS @tbl TABLE(ID INT PRIMARY KEY IDENTITY(1, 1), Val VARCHAR(50))",
                "AS",
                "BEGIN",
                "-------------NAT 20160323------------	",
                "DECLARE @index INT, @prevIndex INT = 1, @counter INT = 0, @val VARCHAR(50);",
                "IF @idRandomTables IS NOT NULL",
                "BEGIN",
                "SET @index = CHARINDEX('','', @idRandomTables, 1)",
                "WHILE @index != 0",
                "BEGIN",
                "SET @val = REPLACE(SUBSTRING(@idRandomTables, @prevIndex, @index - @prevIndex), '','', '''');",
                "INSERT INTO @tbl(Val) VALUES (RTRIM(LTRIM(@val)));",
                "SET @prevIndex = @index;",
                "SET @index = CHARINDEX('','', @idRandomTables, @prevIndex + 1);",
                "IF @index = 0",
                "BEGIN",
                "SET @val = RIGHT(@idRandomTables, LEN(@idRandomTables) - @prevIndex);",
                "INSERT INTO @tbl(Val) VALUES (@val);",
                "END",
                "SET @counter = @counter + 1;",
                "END",
                "IF @counter = 0 AND @index = 0",
                "INSERT INTO @tbl(Val) VALUES (@idRandomTables);",
                "END",
                "ELSE",
                "INSERT INTO @tbl (Val) VALUES (NULL);",
                "RETURN;",
                "END",
                "');",
                "END",
                "",
                "IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'fCommaValDefinition')",
                "BEGIN",
                "EXEC('",
                "CREATE FUNCTION dbo.fCommaValDefinition (@tableName VARCHAR(50), @columns VARCHAR(max), @values VARCHAR(max))",
                "RETURNS @tbl TABLE(id INT, name VARCHAR(200), value VARCHAR(MAX), system_type_id INT, isnumber BIT)",
                "AS",
                "BEGIN",
                "--nat20161223",
                "INSERT INTO @tbl",
                "( id ,",
                "name ,",
                "value ,",
                "system_type_id, ",
                "isnumber",
                ")",
                "SELECT b.ID AS id, a.name, c.Val AS value, a.system_type_id, CASE WHEN d.name IS NULL THEN 0 ELSE 1 END AS isnumber FROM ",
                "(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(@tableName)) a",
                "INNER JOIN fCommaValToTable(@columns) b ON a.name = RTRIM(LTRIM(b.val))",
                "LEFT JOIN dbo.fCommaValToTable(@values) c ON b.ID = c.ID",
                "LEFT JOIN (SELECT name, system_type_id FROM sys.types ",
                "WHERE name IN (''bigint'', ''bit'', ''decimal'', ''int'', ''money'', ''numeric'', ''smallint'', ''smallmoney'', ''tinyint'', ''float'', ''real'' )",
                ") d ON a.system_type_id = d.system_type_id;",
                "RETURN;",
                "END",
                "');",
                "END",
                "",
                "IF NOT EXISTS(SELECT* FROM sys.objects WHERE name = 'fSemiColonValToTable')",
                "BEGIN",
                "EXEC('",
                "CREATE FUNCTION dbo.fSemiColonValToTable(@idRandomTables VARCHAR(MAX)=NULL)",
                "RETURNS @tbl TABLE(ID INT PRIMARY KEY IDENTITY(1, 1), Val VARCHAR(MAX))",
                "AS",
                "BEGIN",
                "-------------NAT 20170407------------",
                "DECLARE @index INT, @prevIndex INT = 1, @counter INT = 0, @val VARCHAR(50);",
                "IF @idRandomTables IS NOT NULL",
                "BEGIN",
                "SET @index = CHARINDEX('';'', @idRandomTables, 1)",
                "WHILE @index != 0",
                "BEGIN",
                "SET @val = REPLACE(SUBSTRING(@idRandomTables, @prevIndex, @index - @prevIndex), '';'', '''');",
                "INSERT INTO @tbl(Val) VALUES(RTRIM(LTRIM(@val)));",
                "SET @prevIndex = @index;",
                "SET @index = CHARINDEX('';'', @idRandomTables, @prevIndex + 1)",
                "IF @index = 0",
                "BEGIN",
                "SET @val = RIGHT(@idRandomTables, LEN(@idRandomTables) - @prevIndex);",
                "INSERT INTO @tbl(Val) VALUES(@val);",
                "END",
                "SET  @counter = @counter + 1;",
                "END",
                "IF @counter = 0 AND @index = 0",
                "INSERT INTO @tbl(Val) VALUES(@idRandomTables);",
                "END",
                "ELSE",
                "INSERT INTO @tbl(Val) VALUES(NULL);",
                "DELETE FROM @tbl WHERE Val IS NULL OR RTRIM(LTRIM(Val)) = '''';",
                "RETURN;",
                "END",
                "');",
                "END",
                "",
                "IF NOT EXISTS(SELECT * FROM sys.objects WHERE name = 'pInsertMultipleRows')",
                "BEGIN",
                "EXEC('",
                "CREATE PROCEDURE dbo.pInsertMultipleRows (@table VARCHAR(50), @columns VARCHAR(max), @values VARCHAR(max))",
                "AS",
                "BEGIN",
                "-------------NAT 20170407------------",
                "DECLARE @tbl TABLE(ID INT, Val VARCHAR(MAX));",
                "DECLARE @scripts VARCHAR(MAX), @toInsert VARCHAR(MAX);",
                "DECLARE @max INT, @counter INT = 1;",
                "DECLARE @holder VARCHAR(MAX);",
                "DECLARE @result TABLE(ID INT);",
                "DECLARE @view VARCHAR(50) = ''v'' + SUBSTRING(@table, 2, LEN(@table) - 1);",
                "SET XACT_ABORT ON;",
                "BEGIN TRANSACTION;",
                "INSERT INTO @tbl",
                "(ID, Val )",
                "SELECT ID, Val FROM dbo.fSemiColonValToTable(@values);",
                "SET @scripts = ''DECLARE @result TABLE(ID INT)'' + CHAR(13) + CHAR(13);",
                "SELECT @max = MAX(ID) FROM @tbl;",
                "WHILE @counter <= @max",
                "BEGIN",
                "SELECT @holder = Val FROM @tbl WHERE ID = @counter;",
                "SET @toInsert = ''INSERT INTO '' + @table + CHAR(13) + ''('' + @columns + '')'' + CHAR(13) + ",
                "''VALUES('' + @holder + '');'' + CHAR(13) +",
                "''INSERT INTO @result(ID) VALUES(SCOPE_IDENTITY());'' + CHAR(13)",
                "SET @scripts = @scripts + @toInsert;",
                "SET @counter = @counter + 1;",
                "END",
                "SET @scripts = @scripts + CHAR(13) + ''SELECT a.* FROM '' + @view + '' a'' + CHAR(13) +",
                "''INNER JOIN @result b ON a.ID = b.ID'';",
                "EXEC (@scripts);",
                "COMMIT TRANSACTION;",
                "SET XACT_ABORT OFF;",
                "END",
                "');",
                "END"
            };

            return string.Join(Environment.NewLine, script);

        }
        public void Initialize(SqlConnectionStringBuilder conString = null)
        {
            try
            {
                SqlConnection = new SqlConnection();
                
                
                ConString = ConnectionBuilder(conString);

                SqlConnection.ConnectionString = ConString.ConnectionString;
                SqlConnection.Open();
                SqlTransaction = SqlConnection.BeginTransaction();
                Initialized?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                (ex.Message + " - MsSqlServer Constructor").Log();
                InitializeFailed?.Invoke(this, new ExceptionEventsArgs { Exception = ex });
            }
        }

        private SqlConnectionStringBuilder ConnectionBuilder(SqlConnectionStringBuilder conString = null)
        {
            SqlConnectionStringBuilder connectionBuilder;
            Func<string, string> config = ConfigurationManager.AppSettings.Get;
            if (conString == null)
            {
                connectionBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = config("Server"),
                    UserID = config("UserID"),
                    Password = config("Password"),
                    InitialCatalog = config("Database")
                };
            }
            else
                connectionBuilder = conString;

            return connectionBuilder;
        }
        public virtual SqlConnection Connection(SqlConnectionStringBuilder conString = null)
        {

            try
            {
                var con = new SqlConnection();
                ConString = ConString ?? ConnectionBuilder(conString);
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

        public DataSet SqlExecCommand(string command, bool withPrimary = true)
        {
            var dataSet = new DataSet();
            using (var con = Connection())
            {
                var com = con.CreateCommand();
                com.CommandType = CommandType.Text;
                com.CommandText = command;
                var adapter = new SqlDataAdapter
                {
                    SelectCommand = com
                };

                if (withPrimary) adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                adapter.Fill(dataSet);
                
                //if (hasSchema) adapter.FillSchema(dataSet, SchemaType.Source);
            }
            return dataSet;
        }

        public DataSet SqlExecCommand(string command, Dictionary<string, object> param, bool withPrimary = true)
        {
            var dataSet = new DataSet();
            using (var con = Connection())
            {
                var com = con.CreateCommand();
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = command;
                foreach (var item in param)
                    com.Parameters.AddWithValue(item.Key.SqlParamName(), item.Value);

                var adapter = new SqlDataAdapter
                {
                    SelectCommand = com
                };

                if (withPrimary) adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

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

        protected virtual string CheckValues(string val, bool isnumber)
        {
            val = val.ToSqlCharacter();
            var result = val;

            if (string.IsNullOrEmpty(val)) return "NULL";
            if (!isnumber)
                result = "'" + val + "'";
            else
            {
                var lower = val.ToLower();
                if (lower == "true") result = "1";
                if (lower == "false") result = "0";
            }

            return result;
        }

        public virtual string GenerateSQLSyntax(Dml dml, Dictionary<string, string> qDictionary, string table, string where, Func<string, string, string, string> query)
        {
            try
            {
                var param = qDictionary.Where(r => r.Key.ToLower() != "table" && r.Key.ToLower() != "where");
                var columns = param.Select(r => r.Key).Fuse();
                var value = param.Select(r => r.Value.ToSqlCharacter()).Fuse(",");

                var rows = SqlExecCommand($"SELECT * FROM dbo.fCommaValDefinition ('{table}', '{columns}', '{value}')").Tables[0].Rows;
                var valList = new List<string>();

                if (table == null)
                    throw new Exception("Invalid Table");

                foreach (DataRow row in rows)
                {
                    var val = CheckValues(row["value"].ToString(), (bool)row["isnumber"]);
                    switch (dml)
                    {
                        case Dml.Add:
                            valList.Add(val);
                            break;
                        case Dml.Update:
                            columns = string.IsNullOrEmpty(where) ? "" : "WHERE " + where;
                            valList.Add(row["name"] + "=" + val);
                            break;
                    }
                }

                var values = valList.Fuse(", ");

                return query(table, columns, values);
            }
            catch (Exception ex)
            {

                (ex.Message + " - GenerateSQLSyntax").Log();
                throw;
            }
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
                if (SqlConnection?.State == ConnectionState.Open) SqlConnection.Close();
                SqlConnection?.Dispose();
                SqlTransaction?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MsSqlServer()
        {
            Dispose(false);
        }
        #endregion

    }
}