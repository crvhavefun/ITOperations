using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ITOperations
{
    public partial class ShopDBFreeSpaceReport : Form
    {
        private DataSet ds;
        
        public ShopDBFreeSpaceReport()
        {
            InitializeComponent();
        }

        private void ShopDBFreeSpaceReport_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = ds.Tables[0];
        }

        public void setDataSet(DataSet ds)
        {
            this.ds = ds;
        }
    }
}
