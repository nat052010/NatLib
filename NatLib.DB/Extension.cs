using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using NatLib;

namespace NatLib.DB
{
    public static class Extension
    {
        public static string SqlParamName(this string param)
        {
            return param.IndexOf('@') != -1 ? param : "@" + param;
        }

        public static string ToSqlCharacter(this string value)
        {
            var result = value?.Replace("'", "''");

            return result;
        }

        public static string ToCleanSql(this string sql)
        {
            if (sql == null) return null;

            var length = sql.IndexOf(';') == -1 ? sql.Length : sql.IndexOf(';');
            return sql.Substring(0, length);
        }

        public static string MSSQLServerDataType(this string type)
        {
            var result = "VARCHAR(MAX)";
            switch (type)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                    result = "INT";
                    break;
                case "Decimal":
                case "Double":
                    result = "DECIMAL (18, 9)";
                    break;
                case "Boolean":
                    result = "BIT DEFAULT(1)";
                    break;
                case "DateTime":
                    result = "DATETIME";
                    break;

            }

            return result;
        }

        public static string SqLiteDataType(this string type)
        {
            var result = "TEXT";
            switch (type)
            {
                case "Int16":
                case "Int32":
                case "Int64":                
                    result = "INTEGER";
                    break;
                case "Decimal":
                case "Double":
                    result = "NUMERIC";
                    break;
                case "Boolean":
                    result = "BOOLEAN";
                    break;
/*                case "DateTime":
                    result = "DATETIME";
                    break;*/
            }

            return result;
        }


        public static string MSSQLCreateTableScript(this DataTable dt, string name)
        {
            var columns = (from DataColumn col in dt.Columns select col.ColumnName + " " + col.DataType.Name.MSSQLServerDataType()).ToList();

            return $"CREATE TABLE {name} (" + string.Join("," + Environment.NewLine, columns) + ");";

        }

        public static string SqlLiteCreateTableScript(this DataTable dt, string name)
        {
            var columns = new List<string>();
            var keys = dt.PrimaryKey;
            var key = keys.Any() ? keys[0].ColumnName : "";
            foreach (DataColumn col in dt.Columns)
            {
                var dType = col.DataType.Name;
                var colName = col.ColumnName;
                var sqLiteDataType = dType.SqLiteDataType();
                //sqLiteDataType = sqLiteDataType == "BOOLEAN" ? "NUMERIC" : sqLiteDataType;
                columns.Add(colName + " " + sqLiteDataType + (colName == key ? " PRIMARY KEY" : ""));
            }

            return $"CREATE TABLE IF NOT EXISTS {name} (" + string.Join("," + Environment.NewLine, columns) + ");";

        }

        public static string SqlLiteInsert(this DataRow dRow, string tblName)
        {
            var columns = (from DataColumn col in dRow.Table.Columns select col.ColumnName).ToList();
            var values = new List<string>();
            foreach (DataColumn col in dRow.Table.Columns)
            {
                var val = Convert.ToString(dRow[col.ColumnName]).ToSqlCharacter();
                var type = col.DataType.Name.SqLiteDataType();

                if (dRow[col.ColumnName] == DBNull.Value)
                    val = "NULL";
                else if (type == "BOOLEAN")
                    val = val.ToLower() == "true" ? "1" : "0";
                else if (type != "INTEGER" && type != "NUMERIC")
                    val = "'" + val + "'";

                values.Add(val);
            }


            return $"INSERT INTO {tblName} ({string.Join(", ", columns.Select(r => "[" + r + "]"))}) {Environment.NewLine} " +
                   $"VALUES({string.Join(", ", values)});";
        }


        public static string SqlLiteMultipleInsert(this DataTable dt, string tblName)
        {
            var result = (from DataRow row in dt.Rows select row.SqlLiteInsert(tblName)).ToList();
            return string.Join("; " + Environment.NewLine, result);
        }

        public static string SqlLiteGenerateScript(this DataTable dt, string tblName)
        {
            var script = new List<string> {dt.SqlLiteCreateTableScript(tblName), dt.SqlLiteMultipleInsert(tblName)};
            return string.Join(Environment.NewLine + Environment.NewLine, script);
        }

        public static List<string> MSSQLColumnList(this DataTable dt, List<string> removeCol = null, string pre = "", string suf = "")
        {
            var list = (from DataColumn col in dt.Columns select pre + col.ColumnName + suf).ToList();

            if (removeCol != null)
                foreach (var rem in removeCol)
                    list.Remove(rem);

            return list;
        }

        public static DataTable SyncSQLServerDataTable( this DataTable dt, 
            List<string> removeCol, 
            Action<List<string>, SqlConnection, DataTable> todo = null, 
            SqlConnectionStringBuilder conString = null, 
            List<string> output = null)
        {
            var dtName = dt.TableName;
            var tempName = $"##{dtName}_{DateTime.Now.Ticks}";
            var dtResult = new DataTable();

            if (output == null)
                output = new List<string> {"ID"};

            output = output.Select(r => "INSERTED." + r).ToList();

            using (var sql = new MsSqlServer(conString))
            {
                using (var con = sql.Connection())
                {
                    try
                    {
                        var columnList = dt.MSSQLColumnList(removeCol);
                        var columnsforUpdate = columnList.Select(r => r + " = b." + r);
                        var columnsforInsert = columnList.Select(r => r);
                        var columnsforInsert2 = columnList.Select(r => "b." + r);
                        var newLine = Environment.NewLine;

                        var script = new List<string>
                        {
                            $"IF OBJECT_ID('tempdb..{tempName}') IS NOT NULL",
                            $"DROP TABLE {tempName}",
                            dt.MSSQLCreateTableScript(tempName)
                        };

                        var com = con.CreateCommand();
                        com.CommandType = CommandType.Text;
                        com.CommandText = string.Join(newLine, script);
                        com.ExecuteNonQuery();

                        //bulkcopy here..
                        using (var bulkCopy = new SqlBulkCopy(con))
                        {
                            bulkCopy.DestinationTableName = tempName;
                            bulkCopy.WriteToServer(dt);
                        }

                        var script2 = new List<string>
                        {
                            $"UPDATE {dtName} SET",
                            string.Join(", ", columnsforUpdate),
                            $"FROM {dtName} a INNER JOIN {tempName} b ON a.ID = b.ID;",
                            newLine,
                            $"INSERT INTO {dtName} (",
                            string.Join(", ", columnsforInsert) + ")",
                            newLine,
                            "OUTPUT " + string.Join(", ", output),
                            newLine,
                            "SELECT " + string.Join(", ", columnsforInsert2),
                            $"FROM {dtName} a RIGHT JOIN {tempName} b ON a.ID = b.ID",
                            "WHERE a.ID IS NULL;",
                            newLine,
                            $"IF OBJECT_ID('tempdb..{tempName}') IS NOT NULL",
                            $"DROP TABLE {tempName}"
                        };

                        if (todo == null)
                        {
                            var com2 = con.CreateCommand();
                            com2.CommandType = CommandType.Text;
                            com2.CommandText = string.Join(newLine, script2);
                            var adapter = new SqlDataAdapter(com2);
                            adapter.Fill(dtResult);
                            //com2.ExecuteNonQuery();
                        }
                        else
                            todo(script2, con, dtResult);
                    }
                    catch (Exception ex)
                    {
                        (ex.Message + " - SyncDataTable").Log();
                    }
                }

            }
            return dtResult;
        }

    }
}
