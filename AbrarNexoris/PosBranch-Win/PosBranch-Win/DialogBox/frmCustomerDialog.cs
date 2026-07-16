using Infragistics.Win.UltraWinGrid;
using ModelClass;
using PosBranch_Win.Transaction;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmCustomerDialog : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Dropdowns dp = new Dropdowns();
        public static string SetValueForText1 = "";
        public frmCustomerDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmCustomerDialog_Load(object sender, EventArgs e)
        {
            CustomerDDlGrid cs = dp.CustomerDDl();
            cs.List.ToString();
            //ultraGridItem.Visible = true;
            //DataBase.Operations = "GETITEM";
            //ItemDDlGrid item = dp.itemDDlGrid("", txtItemName.Text);
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
             ultraGrid1.DataSource = cs.List;
            // ultraGridItem.Focus();
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            CustomerDDl customer = new CustomerDDl();
            UltraGridCell aCell = this.ultraGrid1.ActiveRow.Cells["LedgerName"];
            UltraGridCell aCellLedgerId = this.ultraGrid1.ActiveRow.Cells["LedgerId"];

            string name =   aCell.Value.ToString();
            int LedgerId = Convert.ToInt32(aCellLedgerId.Value.ToString());
            customer.LedgerName = name;
            customer.LedgerID = LedgerId;
            frmSalesInvoice invoice = new frmSalesInvoice();
            invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];
            invoice.txtCustomer.Text = name;
            invoice.lblledger.Text = Convert.ToString(LedgerId);
            this.Close();
        }
    }
}
