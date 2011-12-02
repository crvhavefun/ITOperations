using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace ITOperations
{
    public partial class DailySales : Form
    {
        private String sql, startDate, endDate;
        public DailySales()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now.AddDays(-1);
            this.sql = @"
                select	substring(a.dw_storecode,3,4) shop, '@txdate' txdate, 
		CAST(SUM(a.amt1) AS NUMERIC(10,2)), CAST(SUM(a.amt2) AS NUMERIC(10,2)), 
		CAST(SUM(a.amt3) AS NUMERIC(10,2)), CAST(SUM(a.amt4) AS NUMERIC(10,2)), 
		CAST(SUM(b.txcount) AS NUMERIC(10,0)),
		CAST(round((SUM(a.amt3)+SUM(a.amt4))/SUM(b.txcount),2)  AS NUMERIC(10,2))
from (
		select dw_storecode, SUM(amt1) amt1, SUM(amt2) amt2, SUM(amt3) amt3, SUM(amt4) amt4
		from (
		                select	i.dw_storecode, SUM(i.dw_amt) amt1, SUM(0) amt2, SUM(0) amt3, SUM(0) amt4
		                from	dw_itemdsd i, dw_itemmas m
		                where	i.dw_plu = m.dw_plu and i.dw_bu = m.dw_bu and m.dw_bu in (@bu) and
								i.dw_txdate between '@sdate' and '@edate' and m.dw_group6 = '0'
		                group by i.dw_storecode
		                union all
		                select	i.dw_storecode, SUM(0) amt1, SUM(i.dw_amt) amt2, SUM(0) amt3, SUM(0) amt4
		                from	dw_itemdsd i, dw_itemmas m
		                where	i.dw_plu = m.dw_plu and i.dw_bu = m.dw_bu and m.dw_bu in (@bu) and
								i.dw_txdate between '@sdate' and '@edate' and m.dw_group6 = '1'
		                group by i.dw_storecode
		                union all
		                select	i.dw_storecode, SUM(0) amt1, SUM(0) amt2, SUM(i.dw_amt) amt3, SUM(0) amt4
		                from	dw_itemdsd i, dw_itemmas m
		                where	i.dw_plu = m.dw_plu and i.dw_bu = m.dw_bu and m.dw_bu in (@bu) and
								i.dw_txdate = '@edate' and m.dw_group6 = '0'
		                group by i.dw_storecode
		                union all
		                select	i.dw_storecode, SUM(0) amt1, SUM(0) amt2, SUM(0) amt3, SUM(i.dw_amt) amt4
		                from	dw_itemdsd i, dw_itemmas m
		                where	i.dw_plu = m.dw_plu and i.dw_bu = m.dw_bu and m.dw_bu in (@bu) and
								i.dw_txdate = '@edate' and m.dw_group6 = '1'
		                group by i.dw_storecode
	                ) z group by dw_storecode
                ) a left outer join (
	                select	dw_storecode, COUNT(dw_docno) txcount
	                from	dw_transsalestotal
	                where	dw_txdate = '@edate' and dw_bu in (@bu)
	                group by dw_storecode
                ) b on a.dw_storecode = b.dw_storecode 
				group by a.dw_storecode having sum(amt3) > 0 or SUM(amt4) > 0
				order by 1";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Calculate date of yesterday
            DateTime dateTime = dateTimePicker1.Value;
            dateTime = dateTime.AddDays(-(dateTime.Day - 1));

            //Define date format
            this.startDate = dateTime.ToString("yyyy-MM-dd");
            this.endDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

            //Open folder browser to get the path
            FolderBrowserDialog saveFolderDialog = new FolderBrowserDialog();
            saveFolderDialog.SelectedPath = Application.StartupPath;
            if (saveFolderDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Make a connection to get BU information
                    SqlConnection connection = new SqlConnection(Properties.Settings.Default.crrhkitop);
                    connection.Open();
                    String busql = "select bu_filename, bu_parameter from DailySalesConfig order by 1 desc";
                    SqlCommand command = new SqlCommand(busql, connection);

                    SqlDataReader reader = command.ExecuteReader();
                    progressBar1.Value += 5;
                    while (reader.Read())
                    {
                        //Config sql connection and bind parameters to sql
                        DailySalesGetData getData = new DailySalesGetData(Properties.Settings.Default.crrhkdw, this.sql);
                        getData.SetParameter("@sdate", this.startDate);
                        getData.SetParameter("@edate", this.endDate);
                        getData.SetParameter("@txdate", dateTimePicker1.Value.ToString("yyyyMMdd"));
                        getData.SetParameter("@bu", reader[1].ToString());

                        //Get data reader and output csv file
                        String fileName = reader[0].ToString() + "_sales_" + this.endDate;
                        DailySalesOutputCSV.Output(getData.GetReader(), saveFolderDialog.SelectedPath, fileName);

                        progressBar1.Value += 15;
                        getData.Close(); //Close get data connection
                    }
                    connection.Close();
                    progressBar1.Value = progressBar1.Maximum;

                    MessageBox.Show("完成.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Dispose(); //Close window
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
