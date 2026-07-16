using Infragistics.Win.UltraWinGrid;
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

namespace PosBranch_Win.DialogBox
{
    public partial class FrmSReturnDialog : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Dropdowns dp = new Dropdowns();
        public FrmSReturnDialog()
        {
            InitializeComponent();
             KeyPreview=true;
        }

        private void FrmSReturnDialog_Load(object sender, EventArgs e)
        {
            ItemDDlGrid item = dp.itemDDlGrid();
            dgvSReturnDial.DataSource = item.List;
        }

        private void FrmSReturnDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
