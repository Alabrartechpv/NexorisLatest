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
            if(e.KeyCode == Keys.Enter)
            {
                int index = 0;
                if (DgvItem.Rows.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    int currentRowIndex = DgvItem.CurrentRow.Index;

                    frmPurchase =(FrmPurchase) Application.OpenForms["FrmPurchase"];
                    
                    if(index>=0 && index<DgvItem.Rows.Count)
                    {
                        int count;
                      
                        count = frmPurchase.dgvItem.Rows.Add();
                        frmPurchase.dgvItem.Rows[count].Cells["SlNo"].Value = frmPurchase.dgvItem.Rows.Count;
                        frmPurchase.dgvItem.Rows[count].Cells["ItemId"].Value = DgvItem.Rows[currentRowIndex].Cells["ItemId"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["BarCode"].Value = DgvItem.Rows[currentRowIndex].Cells["BarCode"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["Description"].Value = DgvItem.Rows[currentRowIndex].Cells["Description"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["Cost"].Value = DgvItem.Rows[currentRowIndex].Cells["Cost"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["UnitId"].Value = DgvItem.Rows[currentRowIndex].Cells["UnitId"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["Unit"].Value = DgvItem.Rows[currentRowIndex].Cells["Unit"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["Qty"].Value = 1;
                        frmPurchase.dgvItem.Rows[count].Cells["UnitPrize"].Value = DgvItem.Rows[currentRowIndex].Cells["RetailPrice"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["Packing"].Value = DgvItem.Rows[currentRowIndex].Cells["Packing"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["MarginPer"].Value = DgvItem.Rows[currentRowIndex].Cells["MarginPer"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["MarginAmt"].Value = DgvItem.Rows[currentRowIndex].Cells["MarginAmt"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["TaxPer"].Value = DgvItem.Rows[currentRowIndex].Cells["TaxPer"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["TaxAmt"].Value = DgvItem.Rows[currentRowIndex].Cells["TaxAmt"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["RetailPrice"].Value = DgvItem.Rows[currentRowIndex].Cells["RetailPrice"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["WholeSalePrice"].Value = DgvItem.Rows[currentRowIndex].Cells["WholeSalePrice"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["CreditPrice"].Value = DgvItem.Rows[currentRowIndex].Cells["CreditPrice"].Value;
                        frmPurchase.dgvItem.Rows[count].Cells["CardPrice"].Value = DgvItem.Rows[currentRowIndex].Cells["CardPrice"].Value;
                    }
                }
            }
            

        }
    }
}
