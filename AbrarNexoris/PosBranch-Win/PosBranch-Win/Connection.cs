
using ModelClass;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class Connection : Form
    {

        public Connection()
        {
            InitializeComponent();
        }

        private void Connection_Load(object sender, EventArgs e)
        {
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataBase db = new DataBase();
            DataBase.Status = comboBox1.SelectedItem.ToString();
            BaseRepostitory ab = new BaseRepostitory();
        }
    }
}
