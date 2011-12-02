using System;
using System.Windows.Forms;

namespace ITOperations
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (DailySales ds = new DailySales())
            {
                this.Visible = false;
                ds.ShowDialog();
            }
            this.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (ShopDBFreeSpace sdbfs = new ShopDBFreeSpace())
            {
                this.Visible = false;
                sdbfs.ShowDialog();
            }
            this.Visible = true;
        }
    }
}
