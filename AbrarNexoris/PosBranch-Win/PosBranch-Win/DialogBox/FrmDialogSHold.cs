using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.Transaction;
using Repository.TransactionRepository;
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
    public partial class FrmDialogSHold : Form
    {
        SalesRepository Sales = new SalesRepository();
        frmSalesInvoice invoice = new frmSalesInvoice();
        public FrmDialogSHold()
        {
            InitializeComponent();
        }

        private void FrmDialogSHold_Load(object sender, EventArgs e)
        {
            GetHoldBillGrid hold = Sales.GetHolBill();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = hold.List;

            
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Down)
            {
                if(ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.Rows[0].Selected = true;
                    ultraGrid1.Focus();
                }
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];
            UltraGridCell BillNo = this.ultraGrid1.ActiveRow.Cells["BillNo"];
            Int64 Billno = Convert.ToInt64(BillNo.Value.ToString());          
            salesGrid sale = Sales.GetById(Billno);
           
            if(sale.ListSales.Count() > 0)
            {
                foreach(SalesMaster sm in sale.ListSales)
                {
                   // invoice.GetMyBill(sm);
                    invoice.txtNetTotal.Text = sm.NetAmount.ToString();
                    invoice.txtSubtotal.Text = sm.SubTotal.ToString();
                    invoice.txtCustomer.Text = sm.CustomerName.ToString();
                    invoice.lblBillNo.Text = sm.BillNo.ToString();

                }
                invoice.dgvItems.DataSource = sale.ListSDetails;
                //    for(int index=0;  sale.ListSDetails.Count() > index; index++)
                //    {
                //    foreach (SalesDetails sd in sale.ListSDetails)
                //    {
                //        int i;
                //        i = invoice.dgvItems.Rows.Add();
                //        invoice.dgvItems.Rows[i].Cells["SlNO"].Value = index + 1; //sd.SlNO.ToString();
                //        invoice.dgvItems.Rows[i].Cells["ItemId"].Value = sd.ItemId.ToString();
                //        invoice.dgvItems.Rows[i].Cells["BarCode"].Value = sd.Barcode.ToString();
                //        invoice.dgvItems.Rows[i].Cells["Description"].Value = sd.ItemName.ToString();
                //        invoice.dgvItems.Rows[i].Cells["Cost"].Value = sd.Cost.ToString();
                //        invoice.dgvItems.Rows[i].Cells["UnitId"].Value = sd.UnitId.ToString();
                //        invoice.dgvItems.Rows[i].Cells["Qty"].Value = sd.Qty;
                //        invoice.dgvItems.Rows[i].Cells["Unit"].Value = sd.Unit.ToString();
                //        invoice.dgvItems.Rows[i].Cells["UnitPrice"].Value = sd.UnitPrice.ToString(); 
                //        invoice.dgvItems.Rows[i].Cells["DiscPer"].Value = 0;
                //        invoice.dgvItems.Rows[i].Cells["DiscAmt"].Value = 0;
                //        //float qty1 = float.Parse(invoice.dgvItems.Rows[i].Cells["Qty"].Value.ToString());
                //        // float UnitPrice = float.Parse(invoice.dgvItems.Rows[i].Cells["UnitPrice"].Value.ToString());
                //        invoice.dgvItems.Rows[i].Cells["S/Price"].Value = sd.UnitPrice.ToString();//(qty1 * UnitPrice);
                //        invoice.dgvItems.Rows[i].Cells["AmountWithTax"].Value = sd.Amount.ToString();//(qty1 * UnitPrice);
                //    }
                  
                //}
            }
        }
    }
}
