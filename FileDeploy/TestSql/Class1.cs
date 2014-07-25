using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace TestSql
{
    public static class SqlHelper
    {



        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(commandParameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataSet Query(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddRange(commandParameters);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();

                        // Fill the DataSet using default values for DataTable names, etc
                        da.Fill(ds);

                        ds.Locale = new System.Globalization.CultureInfo("zh-cn");
                        // Return the dataset
                        return ds;
                    }
                }
            }
        }

        public static object GetSingle(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddRange(commandParameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static bool Exists(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            object o = GetSingle(connectionString, commandText, commandParameters);
            int cnt;
            if (o != null && int.TryParse(o.ToString(), out cnt) && cnt > 0)
                return true;
            return false;
        }
    }
}
