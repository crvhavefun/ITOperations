using System;
using System.Data.SqlClient;
using System.IO;

namespace ITOperations
{
    class DailySalesOutputCSV
    {
        public static void Output(SqlDataReader reader, String path, String fileName)
        {
            StreamWriter fp = new StreamWriter(path + "\\" + fileName + ".csv");
            String content = String.Empty;
            
            while (reader.Read())
            {
                content = string.Empty;
                content = reader[0].ToString() + "," + reader[1].ToString() + ",";
                content += reader[2].ToString() + "," + reader[3].ToString() + ",,,,";
                content += reader[4].ToString() + "," + reader[5].ToString() + ",";
                content += reader[6].ToString() + "," + reader[7].ToString().Replace("NULL", "0.00");
                fp.WriteLine(content);
            }
            fp.Close();
        }
    }
}
