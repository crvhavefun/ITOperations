using System;
using System.Data.SqlClient;

namespace ITOperations
{
    class DailySalesGetData
    {
        private SqlConnection connection;
        private String sql;
        
        public DailySalesGetData(String connectionString, String sql)
        {
            this.connection = new SqlConnection(connectionString);
            this.connection.Open();
            this.sql = sql;
        }

        public void SetParameter(String parameter, String value)
        {
            this.sql = this.sql.Replace(parameter, value);
        }

        public SqlDataReader GetReader()
        {
            SqlCommand command = new SqlCommand(this.sql, this.connection);
            command.CommandTimeout = 3600;
            return command.ExecuteReader();
        }

        public String GetSql()
        {
            return this.sql;
        }

        public void Close()
        {
            this.connection.Close();
        }
    }
}
