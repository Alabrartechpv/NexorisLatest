using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.SettingsRepo;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Transaction
{
    public partial class FrmStockAdjustment : Form
    {
        StockAdjustmentDetails StockAdjDetails = new StockAdjustmentDetails();
        Dropdowns dp = new Dropdowns();
        bool CheckExists;
        StockAdjMaster stockadjsmaster = new StockAdjMaster();
        StockAdjPriceDetails stockadjsdetails = new StockAdjPriceDetails();
        StockAdjustmentRepository stockrepos = new StockAdjustmentRepository();

        // DataTable to hold the grid data
        private DataTable stockAdjustmentTable;

        // Column state persistence
        private const string GRID_LAYOUT_FILE = "StockAdjustmentGridLayout.xml";
        private string GridLayoutPath => Path.Combine(Application.StartupPath, GRID_LAYOUT_FILE);

        public FrmStockAdjustment()
        {
            InitializeComponent();
        }

        public void ReturnTable()
        {
            // Create DataTable for UltraGrid
            stockAdjustmentTable = new DataTable();
            stockAdjustmentTable.Columns.Add("NO", typeof(int));
            stockAdjustmentTable.Columns.Add("BarCode", typeof(string));
            stockAdjustmentTable.Columns.Add("ItemNo", typeof(int));
            stockAdjustmentTable.Columns.Add("Description", typeof(string));
            stockAdjustmentTable.Columns.Add("UOM", typeof(string));
            stockAdjustmentTable.Columns.Add("Qty On Hand", typeof(int));
            stockAdjustmentTable.Columns.Add("Adjustment Qty", typeof(int)); // Amount to add/subtract
            stockAdjustmentTable.Columns.Add("New Balance", typeof(int));
            stockAdjustmentTable.Columns.Add("Qty Difference", typeof(int));
            stockAdjustmentTable.Columns.Add("Status", typeof(string));

            // Set the DataTable as the grid's data source
            ultraGrid1.DataSource = stockAdjustmentTable;
        }

        private void StyleGrid()
        {
            // Configure grid appearance and behavior
            ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;

            // Enhanced modern styling
            ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.None;

            // Set alternating row appearance for a professional look
            Infragistics.Win.Appearance altRowAppearance = new Infragistics.Win.Appearance();
            altRowAppearance.BackColor = Color.FromArgb(240, 249, 255);
            altRowAppearance.BorderColor = Color.FromArgb(236, 240, 241);
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance = altRowAppearance;

            // Configure column properties
            foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                col.CellAppearance.TextHAlign = HAlign.Left;
                col.Header.Appearance.TextHAlign = HAlign.Center;
                col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                col.Header.Appearance.BackColor = Color.FromArgb(52, 152, 219);
                col.Header.Appearance.BackColor2 = Color.FromArgb(41, 128, 185);
                col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                col.Header.Appearance.ForeColor = Color.White;
                col.Header.Appearance.BorderColor = Color.FromArgb(44, 62, 80);
            }

            // Set specific column properties
            UltraGridColumn noCol = ultraGrid1.DisplayLayout.Bands[0].Columns["NO"];
            if (noCol != null)
            {
                noCol.Width = 50;
                noCol.CellAppearance.TextHAlign = HAlign.Center;
            }

            UltraGridColumn barcodeCol = ultraGrid1.DisplayLayout.Bands[0].Columns["BarCode"];
            if (barcodeCol != null)
            {
                barcodeCol.Width = 110;
            }

            UltraGridColumn itemNoCol = ultraGrid1.DisplayLayout.Bands[0].Columns["ItemNo"];
            if (itemNoCol != null)
            {
                itemNoCol.Width = 70;
                itemNoCol.CellAppearance.TextHAlign = HAlign.Center;
                itemNoCol.Hidden = true; // Hide to reduce clutter
            }

            UltraGridColumn descCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Description"];
            if (descCol != null)
            {
                descCol.Width = 200;
            }

            UltraGridColumn uomCol = ultraGrid1.DisplayLayout.Bands[0].Columns["UOM"];
            if (uomCol != null)
            {
                uomCol.Width = 80;
                uomCol.CellAppearance.TextHAlign = HAlign.Center;
            }

            UltraGridColumn qtyOnHandCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Qty On Hand"];
            if (qtyOnHandCol != null)
            {
                qtyOnHandCol.Width = 110;
                qtyOnHandCol.CellAppearance.TextHAlign = HAlign.Right;
                qtyOnHandCol.Header.Caption = "Current Stock";
            }

            UltraGridColumn adjQtyCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Adjustment Qty"];
            if (adjQtyCol != null)
            {
                adjQtyCol.Width = 100;
                adjQtyCol.CellAppearance.TextHAlign = HAlign.Right;
                adjQtyCol.CellAppearance.BackColor = Color.FromArgb(240, 248, 255);
                adjQtyCol.Header.Caption = "Adjustment Qty";
                // The header color is now set in the foreach loop above to ensure consistency
            }

            UltraGridColumn newBalCol = ultraGrid1.DisplayLayout.Bands[0].Columns["New Balance"];
            if (newBalCol != null)
            {
                newBalCol.Width = 110;
                newBalCol.CellAppearance.TextHAlign = HAlign.Right;
            }

            UltraGridColumn qtyDiffCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Qty Difference"];
            if (qtyDiffCol != null)
            {
                qtyDiffCol.Width = 120;
                qtyDiffCol.CellAppearance.TextHAlign = HAlign.Right;
                qtyDiffCol.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                qtyDiffCol.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                qtyDiffCol.CellAppearance.BackColor = Color.FromArgb(235, 245, 251);
            }

            UltraGridColumn statusCol = ultraGrid1.DisplayLayout.Bands[0].Columns["Status"];
            if (statusCol != null)
            {
                statusCol.Width = 120;
                statusCol.CellAppearance.TextHAlign = HAlign.Center;
            }

            // Remove the button column for delete functionality - Delete key will be used instead

            // Configure cell activation and editing behavior
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGrid1.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 32;
            ultraGrid1.DisplayLayout.Override.MinRowHeight = 32;

            // Set row selectors (row numbering)
            ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 30;
            ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(52, 152, 219);
            ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;

            // Configure auto-fit behavior
            ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            // Set fonts for the entire grid
            ultraGrid1.Font = new Font("Segoe UI", 9F);

            // Make the current cell stand out more
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = Color.FromArgb(215, 230, 245);
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = Color.Black;
            ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.FontData.Bold = DefaultableBoolean.True;
        }

        private void FrmStockAdjustment_Load(object sender, EventArgs e)
        {
            try
            {
                this.ReturnTable();

                // Load saved grid layout if exists FIRST
                LoadGridLayout();

                // Apply styling AFTER loading layout to ensure styles are not overwritten
                StyleGrid();

                // Register UltraGrid event handlers
                ultraGrid1.AfterCellUpdate += UltraGrid1_AfterCellUpdate;
                ultraGrid1.ClickCell += UltraGrid1_ClickCell;
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;

                // Add double click event handler for UOM column
                ultraGrid1.DoubleClickCell += UltraGrid1_DoubleClickCell;

                // Ensure the btn_ItemLoad click event is connected
                if (btn_ItemLoad != null)
                {
                    // Remove any existing event handlers to avoid duplicates
                    btn_ItemLoad.Click -= btn_ItemLoad_Click;
                    // Add our event handler
                    btn_ItemLoad.Click += btn_ItemLoad_Click;
                }

                KeyPreview = true;

                // Ensure KeyDown event is connected
                this.KeyDown -= FrmStockAdjustment_KeyDown;
                this.KeyDown += FrmStockAdjustment_KeyDown;

                ultraRadioButton1.Checked = true;

                // Generate adjustment number
                int AdjNo = stockrepos.GenerateAdjustNo();
                txt_Adjno.Text = AdjNo.ToString();



                // In the constructor or FrmStockAdjustment_Load, ensure this event is connected:
                txtb_barcode.KeyDown += txtb_barcode_KeyDown;

                // Setup picture box click events
                btnSave.Click += UltraPictureBox6_Click; // Save (F8)
                ultraPictureBox7.Click += UltraPictureBox7_Click; // Update
                ultraPictureBox5.Click += UltraPictureBox5_Click; // Clear (F1)
                pbxExit.Click += PbxExit_Click; // Close (F4)

                // Hide the side panel — Save/Clear/Exit are handled via the ribbon
                ultraPanel6.Visible = false;

                // Register Activated event so barcode textbox always gets focus
                this.Activated += FrmStockAdjustment_Activated;

                btnSave.Visible = true;
                ultraPictureBox7.Visible = false;

                // Set initial focus to barcode field
                barcodeFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during form load: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Activated event: return focus to barcode textbox whenever the form is activated
        private void FrmStockAdjustment_Activated(object sender, EventArgs e)
        {
            barcodeFocus();
        }

        // Add method to load grid layout
        private void LoadGridLayout()
        {
            try
            {
                if (File.Exists(GridLayoutPath))
                {
                    ultraGrid1.DisplayLayout.LoadFromXml(GridLayoutPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading grid layout: {ex.Message}");
                // Silently fail - don't disrupt the user if layout can't be loaded
            }
        }

        // Add method to save grid layout
        private void SaveGridLayout()
        {
            try
            {
                ultraGrid1.DisplayLayout.SaveAsXml(GridLayoutPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving grid layout: {ex.Message}");
                // Silently fail - don't disrupt the user if layout can't be saved
            }
        }

        // Override form closing to save grid layout
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveGridLayout();
            base.OnFormClosing(e);
        }

        private void FrmStockAdjustment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                try
                {
                    string Params1 = "FrmStockAdjustment";
                    frmdialForItemMaster itemDialog = new frmdialForItemMaster(Params1);
                    itemDialog.Owner = this; // Set owner for communication
                    itemDialog.ShowDialog();

                    // Return focus to barcode textbox after dialog closes
                    barcodeFocus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening item dialog: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Dial_Reason_Click(object sender, EventArgs e)
        {
            frmReasonDialog reasonDialog = new frmReasonDialog();
            reasonDialog.ShowDialog();
        }

        private void btn_Dial_Categ_Click(object sender, EventArgs e)
        {
            string Params = "FrmStockAdjustment";
            frmCategoryDialog category = new frmCategoryDialog(Params);
            category.ShowDialog();
        }

        private void btn_ItemLoad_Click(object sender, EventArgs e)
        {
            try
            {
                // Log that we've entered the click event handler
                System.Diagnostics.Debug.WriteLine("btn_ItemLoad_Click event triggered");

                string Params1 = "FrmStockAdjustment";
                frmdialForItemMaster itemDialog = new frmdialForItemMaster(Params1);

                // Set the owner so the dialog can communicate back to this form
                itemDialog.Owner = this;

                // Show the dialog
                itemDialog.ShowDialog();

                // Log after dialog is closed
                System.Diagnostics.Debug.WriteLine("Item dialog closed");

                // Return focus to barcode textbox after dialog closes
                barcodeFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening item dialog: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in btn_ItemLoad_Click: {ex.Message}");
            }
        }

        // Handle cell editing logic (replaces dgv_stockadjustment_CellEndEdit)
        private void UltraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            try
            {
                // Handle cell edit logic for both radio button modes
                if (e.Cell.Column.Key == "Adjustment Qty")
                {
                    UltraGridRow row = e.Cell.Row;

                    // Get cell values with validation
                    var adjQtyCell = row.Cells["Adjustment Qty"];
                    var qtyOnHandCell = row.Cells["Qty On Hand"];
                    var qtyDifferenceCell = row.Cells["Qty Difference"];
                    var newBalanceCell = row.Cells["New Balance"];

                    if (adjQtyCell != null && adjQtyCell.Value != null &&
                        qtyOnHandCell != null && qtyOnHandCell.Value != null)
                    {
                        int adjQty = Convert.ToInt32(adjQtyCell.Value);
                        int sysQty = Convert.ToInt32(qtyOnHandCell.Value);

                        // Calculate New Balance = Current Stock + Adjustment Qty
                        int newBalance = sysQty + adjQty;
                        newBalanceCell.Value = newBalance;

                        // Qty Difference is the same as Adjustment Qty (for display purposes)
                        int difference = adjQty;
                        qtyDifferenceCell.Value = difference;

                        // Enhanced visual feedback based on difference
                        if (difference < 0)
                        {
                            // Stock decrease (negative adjustment)
                            qtyDifferenceCell.Appearance.ForeColor = Color.White;
                            qtyDifferenceCell.Appearance.BackColor = Color.FromArgb(231, 76, 60); // Red background
                            qtyDifferenceCell.Appearance.BackColor2 = Color.FromArgb(192, 57, 43); // Gradient effect
                            qtyDifferenceCell.Appearance.BackGradientStyle = GradientStyle.Vertical;
                            qtyDifferenceCell.Appearance.FontData.Bold = DefaultableBoolean.True;

                            adjQtyCell.Appearance.ForeColor = Color.FromArgb(192, 57, 43); // Dark red text
                            adjQtyCell.Appearance.BackColor = Color.FromArgb(255, 235, 235); // Light red background
                            adjQtyCell.Appearance.FontData.Bold = DefaultableBoolean.True;

                            // Update status column
                            row.Cells["Status"].Value = "Stock OUT";
                        }
                        else if (difference > 0)
                        {
                            // Stock increase (positive adjustment)
                            qtyDifferenceCell.Appearance.ForeColor = Color.White;
                            qtyDifferenceCell.Appearance.BackColor = Color.FromArgb(46, 204, 113); // Green background
                            qtyDifferenceCell.Appearance.BackColor2 = Color.FromArgb(39, 174, 96); // Gradient effect
                            qtyDifferenceCell.Appearance.BackGradientStyle = GradientStyle.Vertical;
                            qtyDifferenceCell.Appearance.FontData.Bold = DefaultableBoolean.True;

                            adjQtyCell.Appearance.ForeColor = Color.FromArgb(39, 174, 96); // Dark green text
                            adjQtyCell.Appearance.BackColor = Color.FromArgb(235, 255, 235); // Light green background
                            adjQtyCell.Appearance.FontData.Bold = DefaultableBoolean.True;

                            // Update status column
                            row.Cells["Status"].Value = "Stock IN";
                        }
                        else
                        {
                            // No change
                            qtyDifferenceCell.Appearance.ForeColor = Color.FromArgb(52, 73, 94);
                            qtyDifferenceCell.Appearance.BackColor = Color.FromArgb(245, 245, 245);
                            qtyDifferenceCell.Appearance.BackGradientStyle = GradientStyle.None;
                            qtyDifferenceCell.Appearance.ResetFontData();

                            adjQtyCell.Appearance.ForeColor = Color.FromArgb(52, 73, 94);
                            adjQtyCell.Appearance.BackColor = Color.FromArgb(248, 248, 248);
                            adjQtyCell.Appearance.ResetFontData();

                            // Update status column
                            row.Cells["Status"].Value = "No Change";
                        }

                        // Highlight the New Balance cell with a subtle color
                        newBalanceCell.Appearance.BackColor = Color.FromArgb(245, 245, 245);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing cell edit: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Handle the delete button click (replaces dgv_stockadjustment_CellContentClick)
        private void UltraGrid1_ClickCell(object sender, ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Column.Key == "DeleteButton")
                {
                    // Ask for confirmation
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this item?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Get the DataTable and remove the row
                        DataTable dt = (DataTable)ultraGrid1.DataSource;
                        dt.Rows.RemoveAt(e.Cell.Row.Index);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting row: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Handle keyboard events for the grid
        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // Handle Enter key to commit cell changes immediately
                if (e.KeyCode == Keys.Enter)
                {
                    if (ultraGrid1.ActiveRow == null) return;

                    // If in edit mode, exit it first
                    if (ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
                    {
                        ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                    }

                    int rowIndex = ultraGrid1.ActiveRow.Index;

                    // Move to next row
                    if (rowIndex + 1 < ultraGrid1.Rows.Count)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex + 1];
                        // Ensure Adjustment Qty is the active cell and entered into edit mode
                        if (ultraGrid1.ActiveRow.Cells.Exists("Adjustment Qty"))
                        {
                            ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Adjustment Qty"];
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        }
                    }

                    e.Handled = true;
                    return;
                }

                // Handle Up Arrow navigation
                if (e.KeyCode == Keys.Up)
                {
                    if (ultraGrid1.ActiveRow == null) return;

                    int rowIndex = ultraGrid1.ActiveRow.Index;
                    if (rowIndex > 0)
                    {
                        // Exit edit mode on current cell
                        if (ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        }

                        // Move to previous row
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex - 1];

                        // Force edit mode on Adjustment Qty
                        if (ultraGrid1.ActiveRow.Cells.Exists("Adjustment Qty"))
                        {
                            ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Adjustment Qty"];
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        }

                        e.Handled = true;
                    }
                    return;
                }

                // Handle Down Arrow navigation
                if (e.KeyCode == Keys.Down)
                {
                    if (ultraGrid1.ActiveRow == null) return;

                    int rowIndex = ultraGrid1.ActiveRow.Index;
                    if (rowIndex + 1 < ultraGrid1.Rows.Count)
                    {
                        // Exit edit mode on current cell
                        if (ultraGrid1.ActiveCell != null && ultraGrid1.ActiveCell.IsInEditMode)
                        {
                            ultraGrid1.PerformAction(UltraGridAction.ExitEditMode);
                        }

                        // Move to next row
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[rowIndex + 1];

                        // Force edit mode on Adjustment Qty
                        if (ultraGrid1.ActiveRow.Cells.Exists("Adjustment Qty"))
                        {
                            ultraGrid1.ActiveCell = ultraGrid1.ActiveRow.Cells["Adjustment Qty"];
                            ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                        }

                        e.Handled = true;
                    }
                    return;
                }

                // Handle F8 to save
                if (e.KeyCode == Keys.F8)
                {
                    btnSave_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    return;
                }

                // Handle delete key press
                if (e.KeyCode == Keys.Delete && ultraGrid1.ActiveRow != null)
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this item?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        DataTable dt = (DataTable)ultraGrid1.DataSource;
                        dt.Rows.RemoveAt(ultraGrid1.ActiveRow.Index);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling keyboard input: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ultraRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (ultraRadioButton1.Checked)
            {
                int rowIndex = ultraGrid1.ActiveRow?.Index ?? -1;

                if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
                {
                    FillGridWithValue(ultraRadioButton1.Text, rowIndex);
                }

                // Update all cells to reflect the new radio button selection
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Adjustment Qty"].Value != null && row.Cells["Qty On Hand"].Value != null)
                    {
                        int adjQty = Convert.ToInt32(row.Cells["Adjustment Qty"].Value);
                        int sysQty = Convert.ToInt32(row.Cells["Qty On Hand"].Value);

                        // Calculate New Balance = Current Stock + Adjustment Qty
                        int newBalance = sysQty + adjQty;
                        row.Cells["New Balance"].Value = newBalance;

                        // Qty Difference is the same as Adjustment Qty
                        int difference = adjQty;
                        row.Cells["Qty Difference"].Value = difference;

                        // Apply color formatting based on difference
                        if (difference < 0)
                        {
                            row.Cells["Qty Difference"].Appearance.ForeColor = Color.Red;
                            row.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                            row.Cells["Adjustment Qty"].Appearance.ForeColor = Color.Red;
                            row.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                        else if (difference > 0)
                        {
                            row.Cells["Qty Difference"].Appearance.ForeColor = Color.Green;
                            row.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                            row.Cells["Adjustment Qty"].Appearance.ForeColor = Color.Green;
                            row.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                        else
                        {
                            row.Cells["Qty Difference"].Appearance.ForeColor = SystemColors.WindowText;
                            row.Cells["Qty Difference"].Appearance.ResetFontData();
                            row.Cells["Adjustment Qty"].Appearance.ForeColor = SystemColors.WindowText;
                            row.Cells["Adjustment Qty"].Appearance.ResetFontData();
                        }
                    }
                }

            }
        }

        private void ultraRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (ultraRadioButton2.Checked)
            {
                int rowIndex = ultraGrid1.ActiveRow?.Index ?? -1;

                if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
                {
                    FillGridWithValue(ultraRadioButton2.Text, rowIndex);
                }

                // Update all cells to reflect the new radio button selection
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["Adjustment Qty"].Value != null && row.Cells["Qty On Hand"].Value != null)
                    {
                        int adjQty = Convert.ToInt32(row.Cells["Adjustment Qty"].Value);
                        int sysQty = Convert.ToInt32(row.Cells["Qty On Hand"].Value);

                        // Calculate New Balance = Current Stock + Adjustment Qty
                        int newBalance = sysQty + adjQty;
                        row.Cells["New Balance"].Value = newBalance;

                        // Qty Difference is the same as Adjustment Qty
                        int difference = adjQty;
                        row.Cells["Qty Difference"].Value = difference;

                        // Apply color formatting based on difference
                        if (difference < 0)
                        {
                            row.Cells["Qty Difference"].Appearance.ForeColor = Color.Red;
                            row.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                            row.Cells["Adjustment Qty"].Appearance.ForeColor = Color.Red;
                            row.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                        else if (difference > 0)
                        {
                            row.Cells["Qty Difference"].Appearance.ForeColor = Color.Green;
                            row.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                            row.Cells["Adjustment Qty"].Appearance.ForeColor = Color.Green;
                            row.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                        else
                        {
                            row.Cells["Qty Difference"].Appearance.ForeColor = SystemColors.WindowText;
                            row.Cells["Qty Difference"].Appearance.ResetFontData();
                            row.Cells["Adjustment Qty"].Appearance.ForeColor = SystemColors.WindowText;
                            row.Cells["Adjustment Qty"].Appearance.ResetFontData();
                        }
                    }
                }

            }
        }

        // Helper method to apply color formatting based on difference
        private void ApplyColorFormatting(UltraGridRow row, int difference)
        {
            if (difference < 0)
            {
                row.Cells["Qty Difference"].Appearance.ForeColor = Color.Red;
                row.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                row.Cells["Adjustment Qty"].Appearance.ForeColor = Color.Red;
                row.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }
            else if (difference > 0)
            {
                row.Cells["Qty Difference"].Appearance.ForeColor = Color.Green;
                row.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;
                row.Cells["Adjustment Qty"].Appearance.ForeColor = Color.Green;
                row.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }
            else
            {
                row.Cells["Qty Difference"].Appearance.ForeColor = SystemColors.WindowText;
                row.Cells["Qty Difference"].Appearance.ResetFontData();
                row.Cells["Adjustment Qty"].Appearance.ForeColor = SystemColors.WindowText;
                row.Cells["Adjustment Qty"].Appearance.ResetFontData();
            }
        }

        private void FillGridWithValue(string optionLabel, int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < ultraGrid1.Rows.Count)
            {
                // Set the status cell value
                ultraGrid1.Rows[rowIndex].Cells["Status"].Value = optionLabel;
            }
        }

        private void txtb_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string input = txtb_barcode.Text.Trim();
                if (string.IsNullOrEmpty(input)) return;

                // Special handling for quantity change (*5, *-5)
                if (input.StartsWith("*") && input.Length > 1)
                {
                    string quantityText = input.Substring(1);
                    if (int.TryParse(quantityText, out int quantity))
                    {
                        // If we have an active row, update its Adjustment Qty
                        if (ultraGrid1.ActiveRow != null)
                        {
                            ultraGrid1.ActiveRow.Cells["Adjustment Qty"].Value = quantity;

                            // Update calculations directly without creating a new event
                            int rowIndex = ultraGrid1.ActiveRow.Index;
                            var row = ultraGrid1.Rows[rowIndex];

                            // Get cell values with validation
                            var adjQtyCell = row.Cells["Adjustment Qty"];
                            var qtyOnHandCell = row.Cells["Qty On Hand"];
                            var qtyDifferenceCell = row.Cells["Qty Difference"];
                            var newBalanceCell = row.Cells["New Balance"];

                            if (adjQtyCell != null && adjQtyCell.Value != null &&
                                qtyOnHandCell != null && qtyOnHandCell.Value != null)
                            {
                                int adjQty = Convert.ToInt32(adjQtyCell.Value);
                                int sysQty = Convert.ToInt32(qtyOnHandCell.Value);

                                // Calculate New Balance = Current Stock + Adjustment Qty
                                int newBalance = sysQty + adjQty;
                                newBalanceCell.Value = newBalance;

                                // Qty Difference is the same as Adjustment Qty
                                int difference = adjQty;
                                qtyDifferenceCell.Value = difference;

                                // Apply color formatting based on difference
                                ApplyColorFormatting(row, difference);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select a row first before changing quantity.",
                                "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        barcodeFocus();
                        return;
                    }
                }

                // Special handling for unit dialog ('u')
                if (input.ToLower() == "u")
                {
                    if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Cells["ItemNo"].Value != null)
                    {
                        int itemId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["ItemNo"].Value);
                        int rowIndex = ultraGrid1.ActiveRow.Index;
                        frmUnitDialog unitDialog = new frmUnitDialog("FrmStockAdjustment", itemId);
                        if (unitDialog.ShowDialog() == DialogResult.OK && unitDialog.Tag != null)
                        {
                            ultraGrid1.Rows[rowIndex].Cells["UOM"].Value = unitDialog.Tag.ToString();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a row first before changing unit.",
                            "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    barcodeFocus();
                    return;
                }

                // Check for duplicate
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["BarCode"].Value?.ToString() == input)
                    {
                        MessageBox.Show("Item already added.");
                        barcodeFocus();
                        return;
                    }
                }

                // Use the same lookup logic as frmdialForItemMaster dialog
                DataBase.Operations = "BARCODEPURCHASE";
                ItemDDlGrid itemDDLG = dp.itemDDlGrid(input, null);

                if (itemDDLG == null || itemDDLG.List == null || !itemDDLG.List.Any())
                {
                    MessageBox.Show("Item not found with barcode: " + input, "Item Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    barcodeFocus();
                    return;
                }

                // Add the first item found to the grid
                // If multiple items have the same barcode, take the first one
                if (itemDDLG.List.Count() >= 1)
                {
                    var item = itemDDLG.List.First();
                    AddItemToGrid(
                        item.ItemId.ToString(),
                        item.BarCode,
                        item.Description,
                        item.Unit,
                        item.Stock.ToString(),
                        0 // Default adjustment quantity (0 = no change)
                    );
                }

                barcodeFocus();
            }
        }

        public void CheckBarcode(string barcode)
        {
            if (ultraGrid1.Rows.Count > 0)
            {
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells["BarCode"].Value.ToString() == barcode)
                    {
                        CheckExists = true;
                        MessageBox.Show("Item already selected");
                        this.barcodeFocus();
                    }
                }
            }
        }

        private void barcodeFocus()
        {
            try
            {
                this.ActiveControl = txtb_barcode;
                txtb_barcode.Clear();
                txtb_barcode.Text = "";
                txtb_barcode.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting barcode focus: " + ex.Message);
                // Non-critical error, don't show message box
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                txtb_reason.Clear();
                // Use Text = string.Empty instead of Clear() for UltraFormattedTextEditor
                txteditor_remark.Text = string.Empty;
                txtb_category.Clear();
                txtb_barcode.Clear();

                // Generate new adjustment number
                int AdjNo = stockrepos.GenerateAdjustNo();
                txt_Adjno.Text = AdjNo.ToString();

                // Clear ultralblId if it exists
                if (ultralblId != null)
                    ultralblId.Text = "0";

                // Reset ledger id
                if (ultlbl_ledgerid != null)
                    ultlbl_ledgerid.Text = "0";

                // Reset category id
                if (ultlbl_catid != null)
                    ultlbl_catid.Text = "0";

                // Reset date picker to current date
                dateTimePicker1.Value = DateTime.Now;

                // Reset button states for a new entry
                btnSave.Visible = true;
                btnSave.Enabled = true;
                ultraPictureBox7.Visible = false;
                ultraPictureBox7.Enabled = true;
                _isUpdateMode = false; // Reset to save mode

                // Clear the DataTable
                if (stockAdjustmentTable != null)
                {
                    stockAdjustmentTable.Clear();
                }

                // Set focus to barcode field
                barcodeFocus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clearing form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirmation dialog
                if (MessageBox.Show("Do you want to save this stock adjustment?", "Confirm Save",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // 1. Validate inputs
                if (string.IsNullOrWhiteSpace(txtb_reason.Text))
                {
                    MessageBox.Show("Please select a reason for the adjustment.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtb_reason.Text.Trim().Equals(DefaultLedgers.BEGINSTOCK, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The primary Stock In Hand ledger cannot be selected as the adjustment reason.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to adjust.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }



                // 2. Prepare master record data
                stockadjsmaster.LedgerName = txtb_reason.Text;

                int ledgerId = 0;
                if (ultlbl_ledgerid != null && int.TryParse(ultlbl_ledgerid.Text, out int lid))
                {
                    ledgerId = lid;
                }
                stockadjsmaster.LedgerId = ledgerId;

                stockadjsmaster.Comments = txteditor_remark.Text;

                int stockAdjNo = 0;
                if (int.TryParse(txt_Adjno.Text, out int adjNo))
                {
                    stockAdjNo = adjNo;
                }
                stockadjsmaster.StockAdjustmentNo = stockAdjNo;

                stockadjsmaster.StockAdjustmentDate = dateTimePicker1.Value;

                int categoryId = 0;
                if (ultlbl_catid != null && int.TryParse(ultlbl_catid.Text, out int catId))
                {
                    categoryId = catId;
                }
                stockadjsmaster.CategoryId = categoryId;

                // 3. Show processing indicator
                Cursor.Current = Cursors.WaitCursor;

                // 4. Prepare data for repository
                DataGridView tempGridView = CreateDataGridForRepository();
                PopulateRepositoryGrid(tempGridView);

                // 5. Save data through repository
                stockadjsdetails.LedgerId = ledgerId;

                System.Diagnostics.Debug.WriteLine($"Calling saveStock with MasterId: {stockadjsmaster.Id}, LedgerId: {stockadjsmaster.LedgerId}");
                string result = stockrepos.saveStock(stockadjsmaster, stockadjsdetails, tempGridView);
                System.Diagnostics.Debug.WriteLine($"SaveStock result: {result}");

                // 6. Handle results
                if (result == "success")
                {
                    SaveStockAdjustmentActivityLog("SAVE", stockadjsmaster);
                    frmSuccesMsg success = new frmSuccesMsg();
                    success.FormClosed += (s, args) =>
                    {
                        // Call clear method when success message is closed
                        btnClear_Click(this, EventArgs.Empty);
                    };
                    success.ShowDialog();
                }
                else
                {
                    string errorDetails = result.StartsWith("Failed:") ? result : $"Failed to save stock adjustment: {result}";
                    MessageBox.Show(errorDetails, "Save Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving stock adjustment: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void SaveStockAdjustmentActivityLog(string activityType, StockAdjMaster stockAdjustment)
        {
            try
            {
                using (var repo = new TransactionActivityLogRepository())
                {
                    repo.SaveStockAdjustmentActivity(
                        stockAdjustment.StockAdjustmentNo,
                        Convert.ToString(stockAdjustment.StockAdjustmentNo),
                        stockAdjustment.LedgerName,
                        "PhysicalStock",
                        0,
                        activityType,
                        $"Stock Adjustment No: {stockAdjustment.StockAdjustmentNo}, Reason: {stockAdjustment.LedgerName}, Remarks: {stockAdjustment.Comments}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stock adjustment activity log save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a DataGridView with columns required by the repository
        /// </summary>
        private DataGridView CreateDataGridForRepository()
        {
            DataGridView dgv = new DataGridView();

            // Add columns to match the repository expectations
            dgv.Columns.Add("No", "No");
            dgv.Columns.Add("ItemNo", "ItemNo");
            dgv.Columns.Add("BarCode", "BarCode");
            dgv.Columns.Add("Description", "Description");
            dgv.Columns.Add("UOM", "UOM");
            dgv.Columns.Add("Qty On Hand", "Qty On Hand");
            dgv.Columns.Add("Adjustment Qty", "Adjustment Qty"); // Amount to add/subtract
            dgv.Columns.Add("New Balance", "New Balance");

            return dgv;
        }

        /// <summary>
        /// Populates the repository grid with data from the UltraGrid
        /// </summary>
        private void PopulateRepositoryGrid(DataGridView repositoryGrid)
        {
            System.Diagnostics.Debug.WriteLine($"PopulateRepositoryGrid: Processing {ultraGrid1.Rows.Count} rows");

            foreach (UltraGridRow row in ultraGrid1.Rows)
            {
                try
                {
                    // Skip empty or invalid rows
                    if (row.Cells["NO"].Value == null || row.Cells["ItemNo"].Value == null ||
                        string.IsNullOrWhiteSpace(row.Cells["NO"].Value.ToString()) ||
                        string.IsNullOrWhiteSpace(row.Cells["ItemNo"].Value.ToString()))
                    {
                        System.Diagnostics.Debug.WriteLine($"  Skipping row - NO or ItemNo is null/empty");
                        continue;
                    }

                    int rowIndex = repositoryGrid.Rows.Add();

                    // Map UltraGrid values to DataGridView cells
                    repositoryGrid.Rows[rowIndex].Cells["No"].Value = row.Cells["NO"].Value;
                    repositoryGrid.Rows[rowIndex].Cells["ItemNo"].Value = row.Cells["ItemNo"].Value;
                    repositoryGrid.Rows[rowIndex].Cells["BarCode"].Value = row.Cells["BarCode"].Value;
                    repositoryGrid.Rows[rowIndex].Cells["Description"].Value = row.Cells["Description"].Value;
                    repositoryGrid.Rows[rowIndex].Cells["UOM"].Value = row.Cells["UOM"].Value;
                    repositoryGrid.Rows[rowIndex].Cells["Qty On Hand"].Value = row.Cells["Qty On Hand"].Value ?? 0;
                    repositoryGrid.Rows[rowIndex].Cells["Adjustment Qty"].Value = row.Cells["Adjustment Qty"].Value ?? 0;
                    repositoryGrid.Rows[rowIndex].Cells["New Balance"].Value = row.Cells["New Balance"].Value ?? 0;

                    System.Diagnostics.Debug.WriteLine($"  Added row {rowIndex}: ItemNo={repositoryGrid.Rows[rowIndex].Cells["ItemNo"].Value}, " +
                        $"BarCode={repositoryGrid.Rows[rowIndex].Cells["BarCode"].Value}, " +
                        $"QtyOnHand={repositoryGrid.Rows[rowIndex].Cells["Qty On Hand"].Value}, " +
                        $"AdjustmentQty={repositoryGrid.Rows[rowIndex].Cells["Adjustment Qty"].Value}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  ERROR adding row: {ex.Message}");
                    MessageBox.Show($"Error preparing grid data: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }

            System.Diagnostics.Debug.WriteLine($"PopulateRepositoryGrid: Total rows added to DataGridView: {repositoryGrid.Rows.Count}");
        }

        private void docBtn_Click(object sender, EventArgs e)
        {
            frmDocDialog docdialo = new frmDocDialog();
            if (docdialo.ShowDialog() == DialogResult.OK)
            {
                btnSave.Visible = false;
                ultraPictureBox7.Visible = true;
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Validate inputs
                if (string.IsNullOrWhiteSpace(txtb_reason.Text))
                {
                    MessageBox.Show("Please select a reason for the adjustment.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtb_reason.Text.Trim().Equals(DefaultLedgers.BEGINSTOCK, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The primary Stock In Hand ledger cannot be selected as the adjustment reason.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to adjust.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }



                // 2. Prepare master record data
                stockadjsmaster.LedgerName = txtb_reason.Text;
                stockadjsmaster.LedgerId = Convert.ToInt32(ultlbl_ledgerid.Text);
                stockadjsmaster.Comments = txteditor_remark.Text;
                stockadjsmaster.StockAdjustmentNo = Convert.ToInt32(txt_Adjno.Text);
                stockadjsmaster.StockAdjustmentDate = dateTimePicker1.Value;
                stockadjsmaster.Id = Convert.ToInt32(ultralblId.Text);
                stockadjsmaster.CategoryId = (ultlbl_catid != null && !string.IsNullOrWhiteSpace(ultlbl_catid.Text))
                    ? Convert.ToInt32(ultlbl_catid.Text) : 0;

                // 3. Show processing indicator
                Cursor.Current = Cursors.WaitCursor;

                // 4. Prepare data for repository
                DataGridView tempGridView = CreateDataGridForRepository();
                PopulateRepositoryGrid(tempGridView);

                // 5. Save data through repository
                stockadjsdetails.LedgerId = Convert.ToInt32(ultlbl_ledgerid.Text);
                stockadjsdetails.StockAdjustmentMasterId = Convert.ToInt32(ultralblId.Text);

                System.Diagnostics.Debug.WriteLine($"Calling updateStock with MasterId: {stockadjsmaster.Id}, LedgerId: {stockadjsmaster.LedgerId}");
                string result = stockrepos.updateStock(stockadjsmaster, stockadjsdetails, tempGridView);
                System.Diagnostics.Debug.WriteLine($"UpdateStock result: {result}");

                // 6. Handle results
                if (result == "success")
                {
                    SaveStockAdjustmentActivityLog("UPDATE", stockadjsmaster);
                    frmSuccesMsg success = new frmSuccesMsg();
                    success.FormClosed += (s, args) =>
                    {
                        // Call clear method when success message is closed
                        btnClear_Click(this, EventArgs.Empty);
                    };
                    success.ShowDialog();
                }
                else
                {
                    string errorDetails = result.StartsWith("Failed:") ? result : $"Failed to update stock adjustment: {result}";
                    MessageBox.Show(errorDetails, "Update Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating stock adjustment: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // Add a new method to handle the DataGridView row addition
        private void AfterRowAdded()
        {
            try
            {
                // Remove call to update status bar
                // UpdateStatusBar();

                // Optional: Automatically scroll to the last row
                if (ultraGrid1.Rows.Count > 0)
                {
                    int lastRowIndex = ultraGrid1.Rows.Count - 1;
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[lastRowIndex];

                    // Ensure the "Adjustment Qty" column exists before setting the current cell
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Adjustment Qty"))
                    {
                        ultraGrid1.ActiveCell = ultraGrid1.Rows[lastRowIndex].Cells["Adjustment Qty"];
                        ultraGrid1.PerformAction(UltraGridAction.EnterEditMode);
                    }

                    // Scroll to make the last row visible
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in AfterRowAdded: " + ex.Message);
                // Don't show a message box for this non-critical error
            }
        }

        // Override the form KeyDown to provide additional shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // F1 to clear the form (previously F5)
                if (keyData == Keys.F1)
                {
                    btnClear_Click(this, EventArgs.Empty);
                    return true;
                }

                // F4 to close the form
                if (keyData == Keys.F4)
                {
                    btnClose_Click(this, EventArgs.Empty);
                    return true;
                }

                // F7 to open frmdialForItemMaster
                if (keyData == Keys.F7)
                {
                    string Params1 = "FrmStockAdjustment";
                    frmdialForItemMaster itemDialog = new frmdialForItemMaster(Params1);
                    itemDialog.Owner = this; // Set owner for communication
                    itemDialog.ShowDialog();
                    return true;
                }

                // F8 to save
                if (keyData == Keys.F8)
                {
                    btnSave_Click(this, EventArgs.Empty);
                    return true;
                }

                // Ctrl+S to save (keeping this for backward compatibility)
                if (keyData == (Keys.Control | Keys.S))
                {
                    btnSave_Click(this, EventArgs.Empty);
                    return true;
                }

                // Ctrl+U to update (keeping this for backward compatibility)
                if (keyData == (Keys.Control | Keys.U))
                {
                    btn_update_Click(this, EventArgs.Empty);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error processing keyboard command: " + ex.Message);
                // Continue with normal processing if there's an error
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Picture box click events
        private void UltraPictureBox6_Click(object sender, EventArgs e)
        {
            // Save - F8
            btnSave_Click(this, EventArgs.Empty);
        }

        private void UltraPictureBox7_Click(object sender, EventArgs e)
        {
            // Update
            {
                btn_update_Click(this, EventArgs.Empty);
            }
        }

        private void UltraPictureBox5_Click(object sender, EventArgs e)
        {
            // Clear - F1
            btnClear_Click(this, EventArgs.Empty);
        }

        private void PbxExit_Click(object sender, EventArgs e)
        {
            // Close - F4
            btnClose_Click(this, EventArgs.Empty);
        }

        // Public method to add items to the UltraGrid from dialog classes
        public int AddItemToGrid(string itemId, string barcode, string description, string uom, string qtyOnHand, int adjQty = 0)
        {
            try
            {
                // Check if this barcode already exists in the grid
                foreach (UltraGridRow existingRow in ultraGrid1.Rows)
                {
                    if (existingRow.Cells["BarCode"].Value.ToString() == barcode)
                    {
                        MessageBox.Show("Item already selected");
                        this.barcodeFocus();
                        return -1;
                    }
                }

                // Add a new row to the DataTable
                DataRow newRow = stockAdjustmentTable.NewRow();

                newRow["NO"] = stockAdjustmentTable.Rows.Count + 1;
                newRow["ItemNo"] = itemId;
                newRow["BarCode"] = barcode;
                newRow["Description"] = description;
                newRow["UOM"] = uom;
                newRow["Qty On Hand"] = qtyOnHand;
                newRow["Adjustment Qty"] = adjQty; // Amount to add/subtract

                int adjAmount = adjQty;
                int sysQty = Convert.ToInt32(qtyOnHand);

                // Calculate New Balance = Current Stock + Adjustment Qty
                int newBalance = sysQty + adjAmount;
                int difference = adjAmount; // Qty Difference is the same as Adjustment Qty

                // Set New Balance and Qty Difference
                newRow["New Balance"] = newBalance;
                newRow["Qty Difference"] = difference;

                // Set status based on which radio button is selected
                if (ultraRadioButton1.Checked)
                {
                    newRow["Status"] = "Adjustment IN/OUT";
                }
                else if (ultraRadioButton2.Checked)
                {
                    newRow["Status"] = "Actual Qty";
                }



                // Add the new row to the DataTable
                stockAdjustmentTable.Rows.Add(newRow);

                // Get the index of the newly added row
                int lastRowIndex = ultraGrid1.Rows.Count - 1;

                // Apply color formatting
                UltraGridRow newGridRow = ultraGrid1.Rows[lastRowIndex];

                // Apply formatting based on difference value
                if (difference < 0)
                {
                    newGridRow.Cells["Qty Difference"].Appearance.ForeColor = Color.White;
                    newGridRow.Cells["Qty Difference"].Appearance.BackColor = Color.FromArgb(231, 76, 60);
                    newGridRow.Cells["Qty Difference"].Appearance.BackColor2 = Color.FromArgb(192, 57, 43);
                    newGridRow.Cells["Qty Difference"].Appearance.BackGradientStyle = GradientStyle.Vertical;
                    newGridRow.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;

                    newGridRow.Cells["Adjustment Qty"].Appearance.ForeColor = Color.FromArgb(192, 57, 43);
                    newGridRow.Cells["Adjustment Qty"].Appearance.BackColor = Color.FromArgb(255, 235, 235);
                    newGridRow.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;

                    newGridRow.Cells["Status"].Value = "Stock OUT";
                }
                else if (difference > 0)
                {
                    newGridRow.Cells["Qty Difference"].Appearance.ForeColor = Color.White;
                    newGridRow.Cells["Qty Difference"].Appearance.BackColor = Color.FromArgb(46, 204, 113);
                    newGridRow.Cells["Qty Difference"].Appearance.BackColor2 = Color.FromArgb(39, 174, 96);
                    newGridRow.Cells["Qty Difference"].Appearance.BackGradientStyle = GradientStyle.Vertical;
                    newGridRow.Cells["Qty Difference"].Appearance.FontData.Bold = DefaultableBoolean.True;

                    newGridRow.Cells["Adjustment Qty"].Appearance.ForeColor = Color.FromArgb(39, 174, 96);
                    newGridRow.Cells["Adjustment Qty"].Appearance.BackColor = Color.FromArgb(235, 255, 235);
                    newGridRow.Cells["Adjustment Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;

                    newGridRow.Cells["Status"].Value = "Stock IN";
                }
                else
                {
                    newGridRow.Cells["Qty Difference"].Appearance.ForeColor = Color.FromArgb(52, 73, 94);
                    newGridRow.Cells["Qty Difference"].Appearance.BackColor = Color.FromArgb(245, 245, 245);
                    newGridRow.Cells["Qty Difference"].Appearance.BackGradientStyle = GradientStyle.None;
                    newGridRow.Cells["Qty Difference"].Appearance.ResetFontData();

                    newGridRow.Cells["Adjustment Qty"].Appearance.ForeColor = Color.FromArgb(52, 73, 94);
                    newGridRow.Cells["Adjustment Qty"].Appearance.BackColor = Color.FromArgb(248, 248, 248);
                    newGridRow.Cells["Adjustment Qty"].Appearance.ResetFontData();

                    newGridRow.Cells["Status"].Value = "No Change";
                }

                // Handle after-add tasks
                AfterRowAdded();

                // Focus the barcode field for the next entry
                barcodeFocus();

                // Return the index of the newly added row
                return lastRowIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item to grid: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        public void ClearGrid()
        {
            if (stockAdjustmentTable != null)
            {
                stockAdjustmentTable.Clear();
            }
        }

        // Track whether we're in save mode (new) or update mode (edit existing)
        private bool _isUpdateMode = false;

        // Public method called by Home.cs universal ribbon Save
        // NOTE: btnSave is inside ultraPanel6 which is hidden (ribbon replaces it),
        // so we cannot rely on btnSave.Visible. Use _isUpdateMode flag instead.
        public void Save()
        {
            if (_isUpdateMode)
            {
                btn_update_Click(this, EventArgs.Empty);
            }
            else
            {
                btnSave_Click(this, EventArgs.Empty);
            }
        }

        public void SetUpdateMode()
        {
            btnSave.Visible = false;
            ultraPictureBox7.Visible = true;
            _isUpdateMode = true;  // Tell ribbon Save to call update instead
        }



        public void SetRemarkForLastRow(string remark)
        {
            try
            {
                // Remark column removed, this method is no longer needed but kept for compatibility
                // if (ultraGrid1.Rows.Count > 0)
                // {
                //     int lastRowIndex = ultraGrid1.Rows.Count - 1;
                //     ultraGrid1.Rows[lastRowIndex].Cells["Remark"].Value = remark;
                // }
            }
            catch (Exception ex)
            {
                // Ignore errors
            }
        }

        // Update double click handler to use the new method
        private void UltraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            // Check if the double-clicked cell is in the UOM column
            if (e.Cell.Column.Key == "UOM")
            {
                int rowIndex = e.Cell.Row.Index;
                if (e.Cell.Row.Cells["ItemNo"].Value != null)
                {
                    int itemId = Convert.ToInt32(e.Cell.Row.Cells["ItemNo"].Value);
                    frmUnitDialog unitDialog = new frmUnitDialog("FrmStockAdjustment", itemId);
                    if (unitDialog.ShowDialog() == DialogResult.OK && unitDialog.Tag != null)
                    {
                        ultraGrid1.Rows[rowIndex].Cells["UOM"].Value = unitDialog.Tag.ToString();
                    }
                }
            }
        }
    }
}

