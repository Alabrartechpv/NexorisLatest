using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.TransactionModels;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmSalesReturnItem : Form
    {
        private BaseRepostitory con = new BaseRepostitory();
        private SalesRNItem sritem = new SalesRNItem();
        private SalesReturnRepository operations = new SalesReturnRepository();
        private frmSalesReturn parentForm;

        public frmSalesReturnItem()
        {
            InitializeComponent();
            KeyPreview = true;
            // Try to get the parent form from open forms
            parentForm = Application.OpenForms["frmSalesReturn"] as frmSalesReturn;
        }

        public frmSalesReturnItem(frmSalesReturn parent)
        {
            InitializeComponent();
            KeyPreview = true;
            parentForm = parent;
        }

        private void SalesReturnItem_Load(object sender, EventArgs e)
        {
            try
            {
                this.DesignGrid();
                if (ultraGrid1.Rows.Count > 0)
                {
                    UltraGridRow firstRow = ultraGrid1.Rows[0];
                    firstRow.Activate();
                    ultraGrid1.ActiveCell = firstRow.Cells["Description"];
                    firstRow.Selected = true;
                }

                // Add a close button or label with instructions
                Label instructionLabel = new Label();
                instructionLabel.Text = "Press ESC to close when done selecting items";
                instructionLabel.AutoSize = true;
                instructionLabel.Location = new Point(10, 10);
                this.Controls.Add(instructionLabel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message);
            }
        }

        public void DesignGrid()
        {
            try
            {
                // Hide system columns
                string[] columnsToHide = new string[]
                {
                    "CompanyId", "FinYearId", "BranchID", "BranchName", "SReturnNo",
                    "SReturnDate", "InvoiceNo", "Free", "OriginalCost", "OriginalSP",
                    "TaxType", "SeriesID", "Reason", "KFCessAmt", "CancelFlag",
                    "KFCessPer", "CessAmt", "CessPer", "MarginAmt", "MarginPer",
                    "DiscountAmount", "DiscountPer", "MRP", "_Operation", "BaseUnit",
                    "Packing", "Cost", "DisPer", "DisAmt", "SalesPrice", "TaxPer",
                    "TaxAmt", "TotalSP", "IsExpiry", "CounterId", "BillNo",
                    "ItemId", "ItemName"
                };

                foreach (string columnName in columnsToHide)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnName))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns[columnName].Hidden = true;
                    }
                }

                // Configure visible columns
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Description"))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns["Description"];
                    column.Header.Caption = "Item Name";
                    column.Width = 200;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Barcode"))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns["Barcode"];
                    column.Header.Caption = "Barcode";
                    column.Width = 100;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Unit"))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns["Unit"];
                    column.Header.Caption = "Unit";
                    column.Width = 80;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("UnitPrice"))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns["UnitPrice"];
                    column.Header.Caption = "Unit Price";
                    column.Width = 100;
                    column.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    column.Format = "N2";
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Qty"))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns["Qty"];
                    column.Header.Caption = "Quantity";
                    column.Width = 80;
                    column.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Amount"))
                {
                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns["Amount"];
                    column.Header.Caption = "Amount";
                    column.Width = 100;
                    column.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    column.Format = "N2";
                }

                // Basic grid settings
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Change CellClickAction to RowSelect for full row selection
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Enable row selection with mouse click
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = SelectType.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = SystemColors.Highlight;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.HighlightText;

                // Add click event handler
                ultraGrid1.ClickCell += new ClickCellEventHandler(ultraGrid1_ClickCell);
                ultraGrid1.DoubleClickCell += new DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error configuring grid: " + ex.Message);
            }
        }

        private void frmSalesReturnItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    // Get reference to the parent form
                    if (parentForm == null)
                    {
                        MessageBox.Show("Sales Return form is not open.");
                        return;
                    }

                    UltraGridRow activeRow = ultraGrid1.ActiveRow;
                    if (activeRow == null) return;

                    // Get values from the active row
                    string itemId = activeRow.Cells["ItemId"].Value?.ToString() ?? "";
                    string itemName = activeRow.Cells["Description"].Value?.ToString() ?? "";
                    string barcode = activeRow.Cells["Barcode"].Value?.ToString() ?? "";
                    string unit = activeRow.Cells["Unit"].Value?.ToString() ?? "";
                    decimal unitPrice = decimal.Parse(activeRow.Cells["UnitPrice"].Value?.ToString() ?? "0");
                    int qty = int.Parse(activeRow.Cells["Qty"].Value?.ToString() ?? "1");
                    decimal amount = decimal.Parse(activeRow.Cells["Amount"].Value?.ToString() ?? "0");

                    // Add the item to the grid in the main form
                    parentForm.AddItemToGrid(itemId, itemName, barcode, unit, unitPrice, qty, amount);

                    // Clear the selection but don't close the form
                    ultraGrid1.Selected.Rows.Clear();
                }
                else if (e.KeyChar == (char)Keys.Escape)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message);
            }
        }

        private void ultraGrid1_ClickCell(object sender, ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null)
                {
                    // Select the entire row when any cell is clicked
                    e.Cell.Row.Selected = true;
                    ultraGrid1.ActiveRow = e.Cell.Row;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling cell click: " + ex.Message);
            }
        }

        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            try
            {
                // Get reference to the parent form
                if (parentForm == null)
                {
                    MessageBox.Show("Sales Return form is not open.");
                    return;
                }

                if (e.Cell == null || e.Cell.Row == null) return;

                UltraGridRow activeRow = e.Cell.Row;

                // Get values from the active row
                string itemId = activeRow.Cells["ItemId"].Value?.ToString() ?? "";
                string itemName = activeRow.Cells["Description"].Value?.ToString() ?? "";
                string barcode = activeRow.Cells["Barcode"].Value?.ToString() ?? "";
                string unit = activeRow.Cells["Unit"].Value?.ToString() ?? "";
                decimal unitPrice = 0;
                decimal.TryParse(activeRow.Cells["UnitPrice"].Value?.ToString(), out unitPrice);
                int qty = 1; // Default to 1
                int.TryParse(activeRow.Cells["Qty"].Value?.ToString(), out qty);
                decimal amount = unitPrice * qty;

                // Add the item to the grid in the main form
                parentForm.AddItemToGrid(itemId, itemName, barcode, unit, unitPrice, qty, amount);

                // Optionally close this form after item selection
                // this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message);
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {

        }
    }
}

