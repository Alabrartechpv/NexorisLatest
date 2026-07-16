using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.Transaction;
using Repository;
using Repository.MasterRepositry;
using Repository.TransactionRepository;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmBarcodeDialog : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        Dropdowns dp = new Dropdowns();
        frmSalesInvoice invoice = new frmSalesInvoice();
        FrmStockAdjustment stokk = new FrmStockAdjustment();
        StockAdjustmentDetails StockAdjDetails = new StockAdjustmentDetails();
        StockAdjustmentRepository stockrepo = new StockAdjustmentRepository();
        ItemMasterRepository ItemREpo = new ItemMasterRepository();
        string formname;
        public frmBarcodeDialog(string FormNames)
        {
            InitializeComponent();
            formname = FormNames;
            textBox1.Focus();
        }

        private void frmBarcodeDialog_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            DataBase.Operations = "GETALL";
            ItemDDlGrid im = dp.itemDDlGrid();
            im.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = im.List;
            ultraGrid1.Rows[0].Selected = true;
            this.Design();
            textBox1.Focus();
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (formname == "frmSalesInvoice")
                {
                    invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];
                    if (invoice != null && ultraGrid1.ActiveRow != null)
                    {
                        UltraGridCell ItemId = this.ultraGrid1.ActiveRow.Cells["ItemId"];
                        UltraGridCell BarCode = this.ultraGrid1.ActiveRow.Cells["BarCode"];
                        UltraGridCell Description = this.ultraGrid1.ActiveRow.Cells["Description"];
                        UltraGridCell Cost = this.ultraGrid1.ActiveRow.Cells["Cost"];
                        UltraGridCell UnitId = this.ultraGrid1.ActiveRow.Cells["UnitId"];
                        UltraGridCell Unit = this.ultraGrid1.ActiveRow.Cells["Unit"];
                        UltraGridCell RetailPrice = this.ultraGrid1.ActiveRow.Cells["RetailPrice"];

                        decimal price = 0;
                        decimal.TryParse(RetailPrice.Value?.ToString(), out price);

                        // Use the AddItemToGrid method instead of directly accessing dgvItems
                        invoice.AddItemToGrid(
                            ItemId.Value.ToString(),       // itemId
                            Description.Value.ToString(),  // itemName
                            BarCode.Value.ToString(),      // barcode
                            Unit.Value.ToString(),         // unit
                            price,                         // unitPrice
                            1,                             // qty (default to 1)
                            price                          // amount (price * qty)
                        );

                        this.Close();
                    }
                }
                else if (e.KeyChar == (char)Keys.Escape)
                {
                    this.Close();
                }
                else if (formname == "FrmStockAdjustment")
                {
                    stokk = (FrmStockAdjustment)Application.OpenForms["FrmStockAdjustment"];

                    CustomerDDl customer = new CustomerDDl();
                    UltraGridCell ItemId = this.ultraGrid1.ActiveRow.Cells["ItemId"];
                    UltraGridCell Barcode = this.ultraGrid1.ActiveRow.Cells["BarCode"];
                    UltraGridCell Description = this.ultraGrid1.ActiveRow.Cells["Description"];
                    UltraGridCell Cost = this.ultraGrid1.ActiveRow.Cells["Cost"];
                    UltraGridCell UnitId = this.ultraGrid1.ActiveRow.Cells["UnitId"];
                    UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];
                    UltraGridCell Marginper = this.ultraGrid1.ActiveRow.Cells["Marginper"];
                    UltraGridCell Unit = this.ultraGrid1.ActiveRow.Cells["Unit"];
                    UltraGridCell Stock = this.ultraGrid1.ActiveRow.Cells["Stock"];

                    StockAdjDetails.Id = Convert.ToInt32(ItemId.Value.ToString());

                    //StockAdjustmentDetails getItem = stockrepo.GetByIdItem(StockAdjDetails.Id);


                    int rowIndex = 0;
                    if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
                    {
                        // Use the new AddItemToGrid method instead of directly accessing dgv_stockadjustment
                        stokk.AddItemToGrid(
                            ItemId.Value.ToString(),       // Item ID
                            Barcode.Value.ToString(),      // Barcode
                            Description.Value.ToString(),  // Description
                            Unit.Value.ToString(),         // UOM
                            Stock.Value.ToString(),        // Qty On Hand
                            1                              // Default Adj Qty to 1
                        );
                        
                        this.Close();
                    }
                    //}//
                }
            }

        }

        private void CalculateTotal()
        {
            invoice = (frmSalesInvoice)Application.OpenForms["frmSalesInvoice"];
            if (invoice != null)
            {
                // We no longer need to calculate totals here as the AddItemToGrid method
                // in frmSalesInvoice will handle this calculation

                // Let the main form know to recalculate
                invoice.CalculateTotal();
            }
        }

        public void Design()
        {
            ultraGrid1.DisplayLayout.Bands[0].Columns["ItemId"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["Packing"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["MarginPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["MarginAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxPer"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["TaxAmt"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["RetailPrice"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["WholeSalePrice"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CreditPrice"].Hidden = true;
            ultraGrid1.DisplayLayout.Bands[0].Columns["CardPrice"].Hidden = true;

            ultraGrid1.DisplayLayout.Bands[0].Columns["Description"].Width = 250;
            textBox1.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 3)
            {
                DataBase.Operations = "GETBYNAME";
                string output = "%" + textBox1.Text.Replace(" ", "%") + "%";
                ItemDDlGrid im = dp.itemDDlGrid(output);
                if (im.List != null)
                {
                    if (im.List.Count() > 0)
                    {
                        im.List.ToString();
                        DataGridTableStyle ts1 = new DataGridTableStyle();
                        DataGridColumnStyle datagrid = new DataGridBoolColumn();
                        //datagrid.Width = 400;
                        this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                        ts1.GridColumnStyles.Add(datagrid);
                        ultraGrid1.DataSource = im.List;
                        ultraGrid1.Rows[0].Selected = true;
                        this.Design();
                    }
                }

            }
            if (textBox1.Text.Length == 0)
            {
                DataBase.Operations = "GETALL";
                ItemDDlGrid im = dp.itemDDlGrid();
                if (im.List != null)
                {
                    if (im.List.Count() > 0)
                    {
                        im.List.ToString();
                        DataGridTableStyle ts1 = new DataGridTableStyle();
                        DataGridColumnStyle datagrid = new DataGridBoolColumn();
                        //datagrid.Width = 400;
                        this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                        ts1.GridColumnStyles.Add(datagrid);
                        ultraGrid1.DataSource = im.List;
                        ultraGrid1.Rows[0].Selected = true;
                        this.Design();
                    }
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ultraGrid1.Focus();
                ultraGrid1.Rows[0].Selected = true;

            }
        }
    }
}
