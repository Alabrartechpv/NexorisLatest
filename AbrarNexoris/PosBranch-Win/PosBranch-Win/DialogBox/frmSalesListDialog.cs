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
    public partial class frmSalesListDialog : Form
    {
        SalesRepository Sales = new SalesRepository();
        frmSalesInvoice invoice = new frmSalesInvoice();
        public frmSalesListDialog()
        {
            InitializeComponent();
        }

        private void frmSalesListDialog_Load(object sender, EventArgs e)
        {
            GetHoldBillGrid hold = Sales.GetBill();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = hold.List;

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (ultraGrid1.Rows.Count > 0)
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

            if (sale.ListSales.Count() > 0)
            {
                foreach (SalesMaster sm in sale.ListSales)
                {
                    // invoice.GetMyBill(sm);
                    invoice.txtNetTotal.Text = sm.NetAmount.ToString();
                    invoice.txtSubtotal.Text = sm.SubTotal.ToString();
                    invoice.txtCustomer.Text = sm.CustomerName.ToString();
                    invoice.lblBillNo.Text = sm.BillNo.ToString();

                }
                invoice.dgvItems.DataSource = sale.ListSDetails;
            }
            invoice.txtBarcode.Focus();
            this.Close();
        }
    }
}
