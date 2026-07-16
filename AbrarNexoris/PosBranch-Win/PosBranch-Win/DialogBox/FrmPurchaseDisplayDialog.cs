using ModelClass;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Master;
using ModelClass.TransactionModels;
using PosBranch_Win.Master;
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
    public partial class FrmPurchaseDisplayDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        FrmPurchase frmPurchase = new FrmPurchase();
        PurchaseInvoiceRepository ObjPurchaseInvRepo = new PurchaseInvoiceRepository();
        public int Pid = 0;
        
        public FrmPurchaseDisplayDialog()
        {
            InitializeComponent();
            
        }

        private void FrmPurchaseDisplayDialog_Load(object sender, EventArgs e)
        {
            PurchaseGrid purchaseGrid = new PurchaseGrid();
            purchaseGrid = drop.getAllPurchaseMaster();
            purchaseGrid.ListPurchase.ToString();
            DataGridTableStyle ts = new DataGridTableStyle();
            DataGridColumnStyle cs = new DataGridBoolColumn();
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts.GridColumnStyles.Add(cs);
            ultraGrid1.DataSource = purchaseGrid.ListPurchase;
            this.HideUltraGridColumns();
            
        }

        

        

        public void HideUltraGridColumns()
        {
            ultraGrid1.DisplayLayout.Bands[0].Columns["CompanyId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["FinYearId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BranchId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BranchName"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["InvoiceNo"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["InvoiceDate"].Hidden = true;
            
            

            ultraGrid1.DisplayLayout.Bands[0].Columns["LedgerID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["PaymodeID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["PaymodeLedgerID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CreditPeriod"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SubTotal"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["SpDisPer"].Hidden = true;

            ultraGrid1.DisplayLayout.Bands[0].Columns["SpDsiAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BillDiscountPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BillDiscountAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Frieght"].Hidden = true;

            ultraGrid1.DisplayLayout.Bands[0].Columns["ExpenseAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["OtherExpAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CancelFlag"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["UserID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxType"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Remarks"].Hidden = true;

            ultraGrid1.DisplayLayout.Bands[0].Columns["RoundOff"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CessPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CessAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CalAfterTax"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CurrencyID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CurSymbol"].Hidden = true;

            ultraGrid1.DisplayLayout.Bands[0].Columns["SeriesID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["VoucherID"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["IsSyncd"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Paid"].Hidden = true;
           
            ultraGrid1.DisplayLayout.Bands[0].Columns["POrderMasterId"].Hidden = true;

            ultraGrid1.DisplayLayout.Bands[0].Columns["PayedAmount"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["BilledBy"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["_Operation"].Hidden = true;

        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            frmPurchase.btnSave.Visible = false;
            frmPurchase.btnUpdate.Visible = true;
            FrmPurchase f1 = new FrmPurchase();
            frmPurchase.dgvItem.Rows.Clear();
            PurchaseInvoiceGrid purchaseInvoiceGrid = new PurchaseInvoiceGrid();
            frmPurchase = (FrmPurchase)Application.OpenForms["FrmPurchase"];
            UltraGridCell purchaseno = this.ultraGrid1.ActiveRow.Cells["Pid"];
            int PurchaseNo = Convert.ToInt32(purchaseno.Value.ToString());
            purchaseInvoiceGrid = ObjPurchaseInvRepo.getPurchaseNumber(PurchaseNo);

            if(purchaseInvoiceGrid.Listpmaster.Count()>0)
            {

                foreach(PurchaseMaster pm in purchaseInvoiceGrid.Listpmaster)
                {
                    frmPurchase.txtPurchaseNo.Text = pm.PurchaseNo.ToString();
                    frmPurchase.dtpPurchaseDate.Value = Convert.ToDateTime(pm.PurchaseDate.ToShortDateString());
                    frmPurchase.CmboVendor.Text = pm.VendorName.ToString();
                    frmPurchase.CmboPayment.Text = pm.Paymode.ToString();
                    frmPurchase.txtInvoiceNo.Text = pm.InvoiceNo.ToString();
                    frmPurchase.DtpInoviceDate.Value = Convert.ToDateTime(pm.InvoiceDate.ToShortDateString());
                    frmPurchase.txtInvoiceAmt.Text = pm.GrandTotal.ToString();
                    frmPurchase.LblPid.Text =  pm.Pid.ToString();
                    frmPurchase.lblVoucherId.Text = pm.VoucherID.ToString();
                    
                }

                PurchaseDetails[] pd = purchaseInvoiceGrid.Listpdetails.ToArray();

                for(int i=0;i<pd.Length;i++)
                {
                    int count = frmPurchase.dgvItem.Rows.Add();
                    frmPurchase.dgvItem.Rows[count].Cells["SLNO"].Value = i+1;
                    frmPurchase.dgvItem.Rows[count].Cells["ItemID"].Value = pd[i].ItemID.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Barcode"].Value = pd[i].Barcode.ToString();    
                    frmPurchase.dgvItem.Rows[count].Cells["Description"].Value = pd[i].ItemName.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Qty"].Value = pd[i].Qty.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Unit"].Value = pd[i].Unit.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["UnitId"].Value = pd[i].UnitId.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Packing"].Value = pd[i].Packing.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Cost"].Value = Math.Round( Convert.ToDecimal(pd[i].Cost),3);
                    frmPurchase.dgvItem.Rows[count].Cells["RetailPrice"].Value = pd[i].RetailPrice.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["UnitPrize"].Value = pd[i].RetailPrice.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["WholeSalePrice"].Value = pd[i].WholeSalePrice.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["CreditPrice"].Value = pd[i].CreditPrice.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Free"].Value = pd[i].Free.ToString();
                    frmPurchase.dgvItem.Rows[count].Cells["Amount"].Value = float.Parse(pd[i].RetailPrice.ToString()) * float.Parse(pd[i].Qty.ToString());
                    frmPurchase.dgvItem.Rows[count].Cells["TotalAmount"].Value = float.Parse(pd[i].RetailPrice.ToString()) * float.Parse(pd[i].Qty.ToString());
                }


                
            }


            
        }

        private void Clear()
        {
            frmPurchase.txtOrderNo.Clear();
            frmPurchase.txtPurchaseNo.Clear();
            frmPurchase.txtInvoiceNo.Clear();
            frmPurchase.txtInvoiceAmt.Clear();
            frmPurchase.GrandToalAmt.Text = "";
            frmPurchase.lblSubTotalAmt.Text = "";
            frmPurchase.txtBarcode.Clear();
            frmPurchase.dgvItem.Rows.Clear();
            frmPurchase.LblPid.Text = "";

        }
    }
}
