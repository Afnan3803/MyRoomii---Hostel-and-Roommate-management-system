using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace MyRoomii
{
    // ============================================
    // Simple SQL Helper Class
    // ============================================
    
    public static class DbHelper
    {
        // Default connection string
        public static string DefaultConnectionString { get; set; } = 
            "Server=localhost,1433;Database=myRoomii;User Id=sa;Password=Test123!@#;TrustServerCertificate=True;";

        /// <summary>
        /// Creates a SQL Server connection
        /// </summary>
        public static SqlConnection CreateConnection(string? connectionString = null)
        {
            return new SqlConnection(connectionString ?? DefaultConnectionString);
        }

        /// <summary>
        /// Executes a stored procedure and returns a SqlDataReader
        /// </summary>
        public static SqlDataReader ExecuteStoredProcedure(SqlConnection connection, string procedureName, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes a stored procedure that doesn't return data
        /// </summary>
        public static int ExecuteNonQuery(SqlConnection connection, string procedureName, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a simple SELECT query and returns a SqlDataReader
        /// </summary>
        public static SqlDataReader ExecuteQuery(SqlConnection connection, string sqlQuery, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(sqlQuery, connection);

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteReader();
        }
    }
}
