using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.Transaction;
using Repository;
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
    
    public partial class frmBillsDialog : Form
    {
        Int64 BillNoP = 0;
        Dropdowns drop = new Dropdowns();
        frmSalesReturn SalesReturn = new frmSalesReturn();
        frmSalesReturnItem returnList = new frmSalesReturnItem();
        SalesReturnRepository operations = new SalesReturnRepository();


        public frmBillsDialog()
        {
            InitializeComponent();
        }

        private void frmBillsDialog_Load(object sender, EventArgs e)
        {

            DataBase.Operations = "SalesInvoice";
            SalesDDlGrid grid = drop.SalesDDl();
            if(grid.List != null)
            {
                grid.List.ToString();
                DataGridTableStyle ts1 = new DataGridTableStyle();
                DataGridColumnStyle datagrid = new DataGridBoolColumn();
                //datagrid.Width = 400;
                this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ts1.GridColumnStyles.Add(datagrid);
                ultraGrid1.DataSource = grid.List;
            }
           
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            SalesReturn = (frmSalesReturn)Application.OpenForms["frmSalesReturn"];
            returnList = (frmSalesReturnItem)Application.OpenForms["frmSalesReturnItem"];

            UltraGridCell BillNo = this.ultraGrid1.ActiveRow.Cells["BillNo"];
            UltraGridCell BillDate = this.ultraGrid1.ActiveRow.Cells["BillDate"];
            UltraGridCell CustomerName = this.ultraGrid1.ActiveRow.Cells["CustomerName"];
            UltraGridCell LedgerID = this.ultraGrid1.ActiveRow.Cells["LedgerID"];
            UltraGridCell PaymodeId = this.ultraGrid1.ActiveRow.Cells["PaymodeId"];
            UltraGridCell NetAmount = this.ultraGrid1.ActiveRow.Cells["NetAmount"];
            UltraGridCell PaymodeName = this.ultraGrid1.ActiveRow.Cells["PaymodeName"];

            SalesReturn.cmbCustomer.Text = CustomerName.Value.ToString();
            SalesReturn.textBox1.Text = BillNo.Value.ToString();
            SalesReturn.lblNetAmount.Text = NetAmount.Value.ToString();
            SalesReturn.dtInvoiceDate.Value =Convert.ToDateTime(BillDate.Value);
            SalesReturn.TxtInvoiceAmnt.Text = NetAmount.Value.ToString();
            // UltraGridCell BrandName = this.ultraGrid1.ActiveRow.Cells["BrandName"];
            frmBillsDialog frmb = new frmBillsDialog();
            frmb.Close();

            Int64 BNo = Convert.ToInt64( BillNo.Value.ToString());
            frmSalesReturnItem retrun = new frmSalesReturnItem();

            SalesReturnDetailsGrid SRD = operations.GetByIdSRD(BNo);

                SRD.List.ToString();
                DataGridTableStyle ts1 = new DataGridTableStyle();
                DataGridColumnStyle datagrid = new DataGridBoolColumn();
                //datagrid.Width = 400;
                retrun.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ts1.GridColumnStyles.Add(datagrid);
                retrun.ultraGrid1.DataSource = SRD.List;
                retrun.ShowDialog();
        }
    }
}
