using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Transaction;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmPurchaseItemDialog : Form
    {
        Dropdowns drop = new Dropdowns();

        FrmPurchase frmPurchase = new FrmPurchase();
        public FrmPurchaseItemDialog()
        {
            InitializeComponent();
            KeyPreview = true;
        }

        private void FrmPurchaseItemDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void FrmPurchaseItemDialog_Load(object sender, EventArgs e)
        {

            //ItemDDlGrid item = drop.itemDDlGrid();
            //DgvItem.DataSource = item.List;
        }




        private void DgvItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index = 0;
                if (DgvItem.Rows.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    int currentRowIndex = DgvItem.CurrentRow.Index;

                    frmPurchase = (FrmPurchase)Application.OpenForms["FrmPurchase"];

                }
            }
        }
    }
}