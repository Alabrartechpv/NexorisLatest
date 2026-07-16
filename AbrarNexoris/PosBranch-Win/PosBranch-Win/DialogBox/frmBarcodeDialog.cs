using Infragistics.Win.UltraWinGrid;
using ModelClass;
using PosBranch_Win.Transaction;
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
    public partial class frmBarcodeDialog : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Dropdowns dp = new Dropdowns();
        frmSalesInvoice invoice = new frmSalesInvoice();
        public frmBarcodeDialog()
        {
            InitializeComponent();
        }

        private void frmBarcodeDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETALL";
            ItemDDlGrid im = dp.itemDDlGrid();
            im.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = im.List;


        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {

            invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];

            CustomerDDl customer = new CustomerDDl();
            UltraGridCell ItemId =  this.ultraGrid1.ActiveRow.Cells["ItemId"];
            UltraGridCell BarCode = this.ultraGrid1.ActiveRow.Cells["BarCode"];
            UltraGridCell Description = this.ultraGrid1.ActiveRow.Cells["Description"];
            UltraGridCell Cost = this.ultraGrid1.ActiveRow.Cells["Cost"];
            UltraGridCell UnitId = this.ultraGrid1.ActiveRow.Cells["UnitId"];
            UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];
            UltraGridCell Marginper = this.ultraGrid1.ActiveRow.Cells["Marginper"];
            UltraGridCell MarginAmt = this.ultraGrid1.ActiveRow.Cells["MarginAmt"];
            UltraGridCell TaxPer = this.ultraGrid1.ActiveRow.Cells["TaxPer"];
            UltraGridCell TaxAmt = this.ultraGrid1.ActiveRow.Cells["TaxAmt"];
            UltraGridCell RetailPrice = this.ultraGrid1.ActiveRow.Cells["RetailPrice"];
            UltraGridCell WholeSalePrice = this.ultraGrid1.ActiveRow.Cells["WholeSalePrice"];
            UltraGridCell CreditPrice = this.ultraGrid1.ActiveRow.Cells["CreditPrice"];
            UltraGridCell CardPrice = this.ultraGrid1.ActiveRow.Cells["CardPrice"];
            UltraGridCell Unit = this.ultraGrid1.ActiveRow.Cells["Unit"];


            int rowIndex = 0;
            if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
            {
                
                    int count;
                    count = invoice.dgvItems.Rows.Add();
                    invoice.dgvItems.Rows[count].Cells["SlNo"].Value = invoice.dgvItems.Rows.Count;
                    invoice.dgvItems.Rows[count].Cells["ItemId"].Value = ItemId.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["BarCode"].Value = BarCode.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["ItemName"].Value = Description.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["Cost"].Value = Cost.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["UnitId"].Value = UnitId.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["Unit"].Value = Unit.Value.ToString();

                    invoice.dgvItems.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["Marginper"].Value = Marginper.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["MarginAmt"].Value = MarginAmt.Value.ToString();
                    invoice.dgvItems.Rows[count].Cells["Qty"].Value = 1;
                    invoice.dgvItems.Rows[count].Cells["Unit"].Value = Unit.Value.ToString();
                
                
                //invoice.dgvItems.Rows[count].Cells["TaxPer"].Value = TaxPer.Value.ToString();
                //  invoice.dgvItems.Rows[count].Cells["TaxAmt"].Value = TaxAmt.Value.ToString();
                if (invoice.cmpPrice.SelectedItem.ToString() == "RetailPrice")
                    {
                        invoice.dgvItems.Rows[count].Cells["Amount"].Value = RetailPrice.Value.ToString();
                    }
                    else if (invoice.cmpPrice.SelectedItem.ToString() == "WholeSalePrice")
                    {
                        invoice.dgvItems.Rows[count].Cells["Amount"].Value = WholeSalePrice.Value.ToString();

                    }
                    else if (invoice.cmpPrice.SelectedItem.ToString() == "CreditPrice")
                    {
                        invoice.dgvItems.Rows[count].Cells["Amount"].Value = CreditPrice.Value.ToString();

                    }
                    else if (invoice.cmpPrice.SelectedItem.ToString() == "CardPrice")
                    {
                        invoice.dgvItems.Rows[count].Cells["Amount"].Value = CardPrice.Value.ToString();

                    }
                invoice.dgvItems.Rows[count].Cells["UnitPrice"].Value = invoice.dgvItems.Rows[count].Cells["Amount"].Value;
                float sp = float.Parse(invoice.dgvItems.Rows[count].Cells["Amount"].Value.ToString());
                int qty = Convert.ToInt32( invoice.dgvItems.Rows[count].Cells["Qty"].Value.ToString());
                float Total = sp * qty;
                invoice.dgvItems.Rows[count].Cells["TotalAmount"].Value = Total;
                invoice.dgvItems.Rows[count].Cells["DiscPer"].Value = 0;
                invoice.dgvItems.Rows[count].Cells["DiscAmt"].Value = 0;
                invoice.dgvItems.Rows[invoice.dgvItems.RowCount - 1].Selected = true;
                CalculateTotal();
                this.Close();
            }
        }

        private void CalculateTotal()
        {
            invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];

            for (int i = 0; i < invoice.dgvItems.Rows.Count; i++)
            {
                float oldSub = float.Parse( invoice.txtSubtotal.Text);
                float OldNet = float.Parse(invoice.txtNetTotal.Text);
                 float  SubTotal = float.Parse(invoice.dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                SubTotal = oldSub + SubTotal;
                invoice.txtSubtotal.Text = SubTotal.ToString();
               float NetTotal = float.Parse(invoice.dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                NetTotal = NetTotal + OldNet;
                invoice.txtNetTotal.Text = SubTotal.ToString();

            }
        }

        
    }
}
