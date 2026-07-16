using Infragistics.Win.UltraWinGrid;
using ModelClass.TransactionModels;
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

namespace PosBranch_Win.Transaction
{
    public partial class frmSalesReturnItem : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        SalesRNItem sritem = new SalesRNItem();
        SalesReturnRepository operations = new SalesReturnRepository();
        frmSalesReturn SR = new frmSalesReturn();

        public frmSalesReturnItem()
        {
            InitializeComponent();
            KeyPreview = true;
        }
       
        private void SalesReturnItem_Load(object sender, EventArgs e)
        {
            this.DesingGrid();
            if (ultraGrid1.Rows.Count > 0)
            {
                UltraGridRow firstRow = ultraGrid1.Rows[0];
                firstRow.Activate(); // Activate the first row
                ultraGrid1.ActiveCell = firstRow.Cells[0]; // Select the first cell
                firstRow.Selected = true; // Optionally select the row
            }

        }
        public void DesingGrid()
        {
            ultraGrid1.DisplayLayout.Bands[0].Columns["CompanyId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["FinYearId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BranchID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BranchName"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SReturnNo"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SReturnDate"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["InvoiceNo"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Free"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["OriginalCost"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["OriginalSP"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxType"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SeriesID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Reason"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["KFCessAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CancelFlag"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["KFCessPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CessAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CessPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["MarginAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["MarginPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["DiscountAmount"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["DiscountPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["MRP"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["_Operation"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BaseUnit"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Packing"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Cost"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["DisPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["DisAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SalesPrice"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TotalSP"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["OriginalCost"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["OriginalSP"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["IsExpiry"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxType"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SeriesID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CounterId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BillNo"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["ItemId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["ItemName"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CounterId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CounterId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CounterId"].Hidden = true;


        }

        private void dgvSRNItem_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            SR = (frmSalesReturn)Application.OpenForms["frmSalesReturn"];

            

        }

        private void frmSalesReturnItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void dgvSRNItem_KeyUp(object sender, KeyEventArgs e)
        {
        
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            SR = (frmSalesReturn)Application.OpenForms["frmSalesReturn"];
            UltraGridCell ItemId = this.ultraGrid1.ActiveRow.Cells["ItemId"];
            UltraGridCell ItemName = this.ultraGrid1.ActiveRow.Cells["Description"];
            UltraGridCell Reason = this.ultraGrid1.ActiveRow.Cells["Reason"];
            UltraGridCell Barcode = this.ultraGrid1.ActiveRow.Cells["Barcode"];
            UltraGridCell Unit = this.ultraGrid1.ActiveRow.Cells["Unit"];
            UltraGridCell UnitPrice = this.ultraGrid1.ActiveRow.Cells["UnitPrice"];
            UltraGridCell Qty = this.ultraGrid1.ActiveRow.Cells["Qty"];
            UltraGridCell Amount = this.ultraGrid1.ActiveRow.Cells["Amount"];
            int rowIndex = 0;
            if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
            {
                int count;
                count = SR.dgvInvoice.Rows.Add();
                SR.dgvInvoice.Rows[count].Cells["SlNo"].Value = SR.dgvInvoice.Rows.Count;
                SR.dgvInvoice.Rows[count].Cells["ItemId"].Value = ItemId.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["ItemName"].Value = ItemName.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["Reason"].Value = "";//Reason.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["Barcode"].Value =Barcode.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["Unit"].Value = Unit.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["UnitPrice"].Value = UnitPrice.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["Qty"].Value = Qty.Value.ToString();
                SR.dgvInvoice.Rows[count].Cells["Amount"].Value = Amount.Value.ToString();


            }

        }
    }
}
