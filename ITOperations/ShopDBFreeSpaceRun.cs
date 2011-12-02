using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ITOperations
{
    class ShopDBFreeSpaceRun
    {
        private Dictionary<String, Shop> shopList;
        private String key, connectionString;
        private Shop shop;
        private const String sizeSql = @"
            select 3800-sum(MB) MB from (
	            select sum(bytes/1024/1024) MB from dba_data_files where tablespace_name = 'USERS'
	            union all
	            select -sum(bytes/1024/1024) MB from dba_free_space  where tablespace_name = 'USERS')";
        private DataSet ds;
        private DataTable dt;
        private DataRow dr;
        
        //variables for threading 
        private ShopDBFreeSpace form;
        private Boolean state = true;

        public ShopDBFreeSpaceRun(ShopDBFreeSpace form)
        {
            this.form = form;
        }

        public void stop()
        {
            state = false;
            updateGUI("Stop");
        }

        public void run()
        {
            while (state)
            {
                updateGUI("Start.");

                this.shopList = new Dictionary<String, Shop>();
                try
                {
                    updateGUI("Getting shop ip list.");

                    SqlConnection connection = new SqlConnection(Properties.Settings.Default.crrhkitop);
                    connection.Open();
                    String shop_sql = @"
                        select store_code, store_ip, store_loginid, store_loginpw
                        from ShopIPAddress where store_active = 'T' order by store_code";
                    SqlCommand command = new SqlCommand(shop_sql, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        shopList.Add(reader[0].ToString(), new Shop(reader[1].ToString(), reader[2].ToString(), reader[3].ToString()));
                    }
                    reader.Close();
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    updateGUI(ex.Message);
                    stop();
                }

                ds = new DataSet();
                dt = new DataTable();
                dt.Columns.Add(new DataColumn("Shop", Type.GetType("System.String")));
                dt.Columns.Add(new DataColumn("Size", Type.GetType("System.String")));
                
                foreach (KeyValuePair<String, Shop> kvp in shopList)
                {
                    key = kvp.Key;
                    shop = kvp.Value;
                    dr = dt.NewRow();
                    dr["Shop"] = key;

                    updateGUI("Checking " + key);

                    connectionString = "Provider=MSDAORA.1;Data Source=@ip;Persist Security Info=True;User ID=@id;Password=@pw;Unicode=True";
                    connectionString = connectionString.Replace("@ip", shop.ip + "/xe");
                    connectionString = connectionString.Replace("@id", shop.id);
                    connectionString = connectionString.Replace("@pw", shop.pw);

                    try
                    {
                        OleDbConnection con = new OleDbConnection(connectionString);
                        con.Open();
                        OleDbCommand command = new OleDbCommand(sizeSql, con);
                        OleDbDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                            dr["Size"] = reader[0].ToString() + " MB";
                    }
                    catch (OleDbException ex)
                    {
                        updateGUI(key + " " +ex.Message);
                        dr["Size"] = ex.Message;
                    }
                    dt.Rows.Add(dr);
                }
                if (dt.Rows.Count > 0)
                {
                    ds.Tables.Add(dt);
                    using (ShopDBFreeSpaceReport sdbfsr = new ShopDBFreeSpaceReport())
                    {
                        sdbfsr.setDataSet(ds);
                        sdbfsr.ShowDialog();
                    }
                }
                state = false;
            }
        }

        private void updateGUI(String message)
        {
            if (state)
                form.Invoke(new ShopDBFreeSpace.InvokeFunction(form.updateLog), new object[] { message });
        }

        private class Shop
        {
            public string ip, id, pw;
            public Shop(String ip, String id, String pw)
            {
                this.ip = ip; this.id = id; this.pw = pw;
            }
        }
    }
}
