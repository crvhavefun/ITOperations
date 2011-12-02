using System;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Threading;

namespace ITOperations
{
    public partial class ShopDBFreeSpace : Form
    {
        
        private ShopDBFreeSpaceRun SDBFSRun;
        private Thread thread;

        public ShopDBFreeSpace()
        {
            InitializeComponent();
        }

        private void ShopDBFreeSpace_Load(object sender, EventArgs e)
        {
            this.SDBFSRun = new ShopDBFreeSpaceRun(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            thread = new Thread(new ThreadStart(SDBFSRun.run));
            thread.Start();
        }

        public delegate void InvokeFunction(String message);

        public void updateLog(String message)
        {
            textBox1.AppendText(message + "\n");
        }
    }
}
