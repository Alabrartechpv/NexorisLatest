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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
// Explicit namespace to avoid Nullable ambiguity
using SystemNullable = System.Nullable;

namespace PosBranch_Win.DialogBox
{
    public partial class frmDocDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        StockAdjMasterDialog stockadjsmasterDialog = new StockAdjMasterDialog();
        StockAdjPriceDetails stockadjsdetails = new StockAdjPriceDetails();
        StockAdjustmentRepository stockrepos = new StockAdjustmentRepository();
        FrmStockAdjustment stockk = new FrmStockAdjustment();
        BaseRepostitory con = new BaseRepostitory();

        private DataTable fullDataTable = null;

        // Add a status label for debugging
        private Label lblStatus;

        // Add a class-level variable to track the original order
        private bool isOriginalOrder = true;

        // Add a class-level variable to store the column order comboBox
        // (comboBox2 already exists in designer, but ensure logic is present)
        // Add a class-level variable to store the column chooser form
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        // Add this dictionary as a class-level field to store column widths
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        // Tooltip for drag-and-drop feedback
        // Track mouse position and column for drag
        private Point startPoint;
        private Infragistics.Win.UltraWinGrid.UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;

        // FIXED: Add a list to store hidden column keys for persistence
        // Initialize with new list to avoid null reference
        private static List<string> persistentHiddenColumns = new List<string>();

        public frmDocDialog()
        {
            InitializeComponent();

            // Initialize status label
            InitializeStatusLabel();

            // Set up the grid style
            SetupUltraGridStyle();

            // Set up the panel styles
            SetupUltraPanelStyle();

            // Initialize the search filter comboBox
            InitializeSearchFilterComboBox();

            // --- BEGIN: Merge from frmdialForItemMaster ---
            // Initialize the column order comboBox
            InitializeColumnOrderComboBox();
            // Set up the column chooser right-click menu
            SetupColumnChooserMenu();
            // Register events to preserve column widths
            ultraGrid1.AfterRowsDeleted += UltraGrid1_AfterRowsDeleted;
            ultraGrid1.AfterSortChange += UltraGrid1_AfterSortChange;
            ultraGrid1.AfterColPosChanged += UltraGrid1_AfterColPosChanged;
            // Add handler for form resize to preserve column widths
            this.SizeChanged += FrmDocDialog_SizeChanged;
            ultraGrid1.Resize += UltraGrid1_Resize;
            // Initialize tooltip
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 500;
            this.toolTip.ShowAlways = true;
            // Wire up all navigation, sort, OK/Close, and textbox click events
            WirePanelAndPictureBoxClicks();
            // --- END: Merge from frmdialForItemMaster ---

            // Connect key events for grid
            ultraGrid1.KeyPress += ultraGrid1_KeyPress;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;

            // FIXED: Add form closing event to save column state
            this.FormClosing += FrmDocDialog_FormClosing;

            // --- MISSING FEATURES: Add from frmCustomerDialog ---
            // Setup panel hover effects
            SetupPanelHoverEffects();

            // Add event handlers for better UX
            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, e) => PositionColumnChooserAtBottomRight();
        }

        private void InitializeStatusLabel()
        {
            // Create a status label at the bottom of the form
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.AutoSize = false;
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.BackColor = Color.LightYellow;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Text = "Ready";
            this.Controls.Add(lblStatus);
        }

        // Update status without interrupting the user
        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            // Add key handling for navigation
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = false; // Let default grid navigation happen
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // Handle as if key press
                KeyPressEventArgs keyPress = new KeyPressEventArgs((char)13);
                ultgrid_docNo_KeyPress(sender, keyPress);
                e.Handled = true;
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ultgrid_docNo_KeyPress(sender, e);
            }
        }

        // Load the selected stock adjustment master record and all its details
        private void ultgrid_docNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (sender is Infragistics.Win.UltraWinGrid.UltraGrid grid &&
                    grid.ActiveRow != null)
                {
                    // Get the FrmStockAdjustment form instance
                    stockk = (FrmStockAdjustment)Application.OpenForms["FrmStockAdjustment"];

                    if (stockk == null)
                    {
                        MessageBox.Show("Stock Adjustment form is not open.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Get master record data from the selected grid row
                    UltraGridCell Id = grid.ActiveRow.Cells["Id"];
                    UltraGridCell StockAdjustmentNo = grid.ActiveRow.Cells["StockAdjustmentNo"];
                    UltraGridCell StockAdjustmentDate = grid.ActiveRow.Cells["StockAdjustmentDate"];
                    UltraGridCell Comments = grid.ActiveRow.Cells["Comments"];
                    UltraGridCell LedgerName = grid.ActiveRow.Cells["LedgerName"];
                    UltraGridCell LedgerID = grid.ActiveRow.Cells["LedgerId"];
                    UltraGridCell VoucherId = grid.ActiveRow.Cells["VoucherId"];
                    UltraGridCell CategoryId = grid.ActiveRow.Cells["CategoryId"];

                    // Clear the existing grid in the stock adjustment form
                    stockk.ClearGrid();

                    // Load the full master and detail data using Dropdowns
                    int masterId = Convert.ToInt32(Id.Value);
                    StockGrid skGrid = drop.getStockAdjustmentById(masterId);

                    if (skGrid == null)
                    {
                        MessageBox.Show("Error loading stock adjustment details.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Populate master data into the form
                    stockk.txt_Adjno.Text = StockAdjustmentNo.Value?.ToString() ?? "";
                    stockk.dateTimePicker1.Value = StockAdjustmentDate.Value != null
                        ? Convert.ToDateTime(StockAdjustmentDate.Value)
                        : DateTime.Now;
                    stockk.txtb_reason.Text = LedgerName.Value?.ToString() ?? "";
                    stockk.txteditor_remark.Text = Comments.Value?.ToString() ?? "";
                    stockk.ultlbl_ledgerid.Text = LedgerID.Value?.ToString() ?? "0";
                    stockk.ultravoucherId.Text = VoucherId.Value?.ToString() ?? "0";
                    stockk.ultralblId.Text = Id.Value?.ToString() ?? "0";

                    // Populate category ID and name
                    int categoryIdValue = 0;
                    if (CategoryId != null && CategoryId.Value != null && CategoryId.Value != DBNull.Value)
                    {
                        categoryIdValue = Convert.ToInt32(CategoryId.Value);
                    }
                    else if (skGrid.ListMaster != null && skGrid.ListMaster.Any())
                    {
                        var masterRecord = skGrid.ListMaster.FirstOrDefault();
                        if (masterRecord != null && masterRecord.CategoryId > 0)
                        {
                            categoryIdValue = masterRecord.CategoryId;
                        }
                    }

                    if (categoryIdValue > 0 && stockk.ultlbl_catid != null)
                    {
                        stockk.ultlbl_catid.Text = categoryIdValue.ToString();

                        try
                        {
                            DataBase.Operations = "Category";
                            var categoryDDl = drop.getCategoryDDl("");
                            var categoryItem = categoryDDl?.List?.FirstOrDefault(c => c.Id == categoryIdValue);
                            if (categoryItem != null && !string.IsNullOrEmpty(categoryItem.CategoryName))
                            {
                                stockk.txtb_category.Text = categoryItem.CategoryName;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading category name: {ex.Message}");
                        }
                    }
                    else if (stockk.ultlbl_catid != null)
                    {
                        stockk.ultlbl_catid.Text = "0";
                        stockk.txtb_category.Text = "";
                    }

                    // Populate detail data into the grid
                    if (skGrid.ListDetails != null && skGrid.ListDetails.Any())
                    {
                        foreach (StockAdjPriceDetails detail in skGrid.ListDetails)
                        {
                            // Calculate Adjustment Qty from PhysicalStock and SystemStock
                            // Adjustment Qty = PhysicalStock - SystemStock (the amount that was adjusted)
                            int adjustmentQty = (int)(detail.PhysicalStock - detail.SystemStock);

                            // Add each detail item to the grid
                            // Note: StockAdjPriceDetails uses Description and UOM, not ItemName and UnitName
                            int newRowIdx = stockk.AddItemToGrid(
                                detail.ItemId.ToString(),
                                detail.BarCode ?? "",
                                detail.Description ?? "",
                                detail.UOM ?? "",
                                detail.SystemStock.ToString(),
                                adjustmentQty  // Pass the calculated adjustment amount
                            );
                        }
                    }

                    // Set DialogResult to OK so parent form knows we loaded data
                    this.DialogResult = DialogResult.OK;

                    // Close the dialog
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock adjustment: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupUltraPanelStyle()
        {
            // Set appearance for the main panel
            ultPanelPurchaseDisplay.Appearance.BackColor = Color.White;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = Color.FromArgb(200, 230, 250);
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

            // Style the top panel
            ultraPanel2.Appearance.BackColor = Color.FromArgb(46, 93, 144);
            ultraPanel2.Appearance.BackColor2 = Color.FromArgb(0, 0, 0, 0);

            // Style the navigation panels (up/down)
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel7);

            // Style the action panels
            StyleIconPanel(ultraPanel4);
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);

            // Ensure they match exact colors
            ultraPanel3.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel3.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel3.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel3.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            ultraPanel7.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel7.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel7.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel7.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Define consistent colors for all panels
            Color lightBlue = Color.FromArgb(127, 219, 255); // Light blue
            Color darkBlue = Color.FromArgb(0, 116, 217);    // Darker blue
            Color borderBlue = Color.FromArgb(0, 150, 220);  // Border blue
            Color borderBase = Color.FromArgb(0, 100, 180);  // Border base color

            // Create a gradient from light to dark blue with exact specified colors
            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            // Set highly rounded border style
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;

            // Add exact specified border color
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            // Ensure icons inside have transparent background
            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                {
                    Infragistics.Win.UltraWinEditors.UltraPictureBox pic = (Infragistics.Win.UltraWinEditors.UltraPictureBox)control;
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                }
                else if (control is Label)
                {
                    ((Label)control).ForeColor = Color.White;
                    ((Label)control).Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
                    ((Label)control).BackColor = Color.Transparent;
                }
            }

            // Add hover effect with consistent colors
            panel.ClientArea.MouseEnter += (sender, e) => {
                panel.Appearance.BackColor = Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(30, 140, 230);
            };

            panel.ClientArea.MouseLeave += (sender, e) => {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };

            // Set cursor to hand to indicate clickable
            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                // Reset everything first to ensure clean slate
                ultraGrid1.DisplayLayout.Reset();

                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;

                // Enable column moving and dragging
                ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;

                // Important: This setting ensures we get only row selection on click, not automatic action
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // Hide the group-by area (gray bar)
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;

                // Define colors
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

                // Set light blue border color for cells
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

                // Configure row height - increase to match the image (about 26-30 pixels)
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;

                // Add header styling - blue headers
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Configure row selector appearance with blue
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

                // Set all cells to have white background (no alternate row coloring)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;

                // Configure selected row appearance with light blue highlight - UPDATED to match FrmPurchaseDisplayDialog.cs
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215); // Bright blue from the image
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White; // White text for better contrast

                // Configure active row appearance - make it same as selected row
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Set font size for all cells to match the image (standard text size)
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    // Configure track appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    // Configure thumb appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Configure cell appearance to increase vertical content alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;

                // FIXED: Change horizontal alignment to left for better readability
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;

                // Connect click events for double-clicking
                ultraGrid1.DoubleClickCell += new Infragistics.Win.UltraWinGrid.DoubleClickCellEventHandler(ultraGrid1_DoubleClickCell);

                // In SetupUltraGridStyle method - add these lines to disable filter indicators
                ultraGrid1.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;

                // FIXED: Enable horizontal scrolling explicitly
                ultraGrid1.DisplayLayout.ViewStyleBand = Infragistics.Win.UltraWinGrid.ViewStyleBand.OutlookGroupBy;

                // FIXED: Set default column width to prevent tight columns
                ultraGrid1.DisplayLayout.Override.DefaultColWidth = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            // Clear any existing items
            comboBox1.Items.Clear();

            // Add search filter options based on MASTER grid columns only
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Adjustment No");
            comboBox1.Items.Add("Date");
            comboBox1.Items.Add("Ledger/Reason");
            comboBox1.Items.Add("Comments");
            comboBox1.Items.Add("Status");

            // Select "Select all" by default
            comboBox1.SelectedIndex = 0;

            // Add event handler
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Handle comboBox1 selection change
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-apply the current filter with the new search column selection
            string searchText = textBoxsearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchText) && searchText != "Search items...")
            {
                ApplyFilter(searchText);
            }
        }

        private void ApplyFilter(string searchText)
        {
            try
            {
                if (fullDataTable == null)
                {
                    UpdateStatus("No data available to filter.");
                    return;
                }

                // Create a view of the data
                DataView dv = fullDataTable.DefaultView;

                // If search text is provided, apply filter
                if (!string.IsNullOrEmpty(searchText) && searchText != "Search documents..." && searchText != "Search...")
                {
                    // Get the selected search filter option
                    string filterOption = comboBox1.SelectedItem?.ToString() ?? "Select all";

                    // Escape single quotes in search text
                    string escapedSearchText = searchText.Replace("'", "''");

                    // Build a filter based on selected option and data columns
                    string filter = "";

                    // Build filter based on available columns and selected filter
                    switch (filterOption)
                    {
                        case "Adjustment No":
                            if (fullDataTable.Columns.Contains("StockAdjustmentNo"))
                                filter = $"CONVERT(StockAdjustmentNo, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "Date":
                            if (fullDataTable.Columns.Contains("StockAdjustmentDate"))
                                filter = $"CONVERT(StockAdjustmentDate, 'System.String') LIKE '%{escapedSearchText}%'";
                            break;
                        case "Ledger/Reason":
                            if (fullDataTable.Columns.Contains("LedgerName"))
                                filter = $"LedgerName LIKE '%{escapedSearchText}%'";
                            break;
                        case "Comments":
                            if (fullDataTable.Columns.Contains("Comments"))
                                filter = $"Comments LIKE '%{escapedSearchText}%'";
                            break;
                        case "Status":
                            if (fullDataTable.Columns.Contains("Status"))
                                filter = $"Status LIKE '%{escapedSearchText}%'";
                            break;
                        case "Select all":
                        default:
                            // Create a filter for each string column
                            List<string> filterParts = new List<string>();
                            foreach (DataColumn col in fullDataTable.Columns)
                            {
                                // Skip the OriginalRowOrder column
                                if (col.ColumnName == "OriginalRowOrder") continue;

                                if (col.DataType == typeof(string) || col.DataType == typeof(DateTime))
                                {
                                    filterParts.Add($"CONVERT({col.ColumnName}, 'System.String') LIKE '%{escapedSearchText}%'");
                                }
                            }
                            if (filterParts.Count > 0)
                            {
                                filter = string.Join(" OR ", filterParts);
                            }
                            break;
                    }

                    // Apply filter if valid
                    if (!string.IsNullOrEmpty(filter))
                    {
                        dv.RowFilter = filter;
                        UpdateStatus($"Filter applied - Found {dv.Count} matching rows");
                    }
                }
                else
                {
                    // Clear filter
                    dv.RowFilter = string.Empty;
                    UpdateStatus("Showing all records");
                }

                // Select first row after filtering
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                // Update record count label
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error applying filter: {ex.Message}");
            }
        }

        // Double-click handler for grid cells
        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            // Process the same action as Enter key press
            KeyPressEventArgs args = new KeyPressEventArgs((char)13);
            ultgrid_docNo_KeyPress(ultraGrid1, args);
        }

        // Override to process Enter key properly
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    KeyPressEventArgs args = new KeyPressEventArgs((char)13);
                    ultgrid_docNo_KeyPress(ultraGrid1, args);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void frmDocDialog_Load(object sender, EventArgs e)
        {
            try
            {
                // Update status
                UpdateStatus("Loading documents...");

                // Initialize label1 with record count info if it exists
                if (label1 != null)
                {
                    label1.Text = "Loading records...";
                    label1.AutoSize = true;
                    label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    label1.ForeColor = Color.FromArgb(0, 70, 170); // Dark blue color
                }

                // Load document data
                StockGrid stokGrid = new StockGrid();
                stokGrid = drop.getAllDocNo();

                // FIXED: Disable AutoFitStyle to prevent columns from being resized automatically
                this.ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;

                // Store data in a DataTable for filtering capability
                if (stokGrid.ListMaster != null)
                {
                    fullDataTable = ToDataTable(stokGrid.ListMaster);

                    // Preserve original row order for sorting functionality
                    PreserveOriginalRowOrder(fullDataTable);

                    ultraGrid1.DataSource = fullDataTable;

                    // FIXED: Initialize column widths after data is loaded
                    InitializeColumnWidths();

                    // FIXED: Restore hidden column state
                    RestoreHiddenColumns();

                    // FIXED: Ensure horizontal scrolling is enabled
                    EnsureHorizontalScrolling();

                    // Initialize saved column widths
                    InitializeSavedColumnWidths();

                    // Select first row if available
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    }

                    // Update record count label
                    UpdateRecordCountLabel();

                    // Update status
                    UpdateStatus($"Loaded {fullDataTable?.Rows.Count ?? 0} documents");
                }
                else
                {
                    UpdateStatus("No documents found");
                    if (label1 != null)
                    {
                        label1.Text = "No records found";
                    }
                }

                // Set search textbox placeholder with improved handling
                if (textBoxsearch != null)
                {
                    textBoxsearch.Text = "Search documents...";
                    textBoxsearch.ForeColor = Color.Gray;
                    textBoxsearch.GotFocus += TextBoxsearch_GotFocus;
                    textBoxsearch.LostFocus += TextBoxsearch_LostFocus;
                    textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
                    textBoxsearch.KeyDown += TextBoxsearch_KeyDown;
                }

                // Ensure focus is on the search textbox for better UX
                this.BeginInvoke(new Action(() =>
                {
                    if (textBoxsearch != null)
                    {
                        textBoxsearch.Focus();
                        textBoxsearch.Select();
                    }
                }));
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading data: {ex.Message}");
                MessageBox.Show($"Error loading document data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to convert list to DataTable for filtering
        private DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            if (items == null) return new DataTable();

            var table = new DataTable();
            var props = typeof(T).GetProperties();

            // Create columns
            foreach (var prop in props)
            {
                table.Columns.Add(prop.Name, SystemNullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Add rows
            foreach (var item in items)
            {
                var row = table.NewRow();
                foreach (var prop in props)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }

        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            // Apply filter when search text changes
            string searchText = textBoxsearch.Text;
            if (searchText == "Search documents..." || searchText == "Search...") searchText = string.Empty;
            ApplyFilter(searchText);
        }

        // Placeholder text handlers
        private void TextBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search documents..." || textBoxsearch.Text == "Search...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void TextBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search documents...";
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        // Keyboard navigation from search box
        private void TextBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Down)
                {
                    // Move focus to the grid and select the first row if available
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.Focus();
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    // If there are rows in the grid, move focus to the grid and select the last row
                    if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.Focus();
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    // If there's only one row in the filtered results, select it
                    if (ultraGrid1.Rows.Count == 1)
                    {
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                        // Trigger the selection action
                        KeyPressEventArgs keyPress = new KeyPressEventArgs((char)13);
                        ultgrid_docNo_KeyPress(ultraGrid1, keyPress);
                    }
                    // Otherwise, move focus to the grid
                    else if (ultraGrid1.Rows.Count > 0)
                    {
                        ultraGrid1.Focus();
                        ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                        ultraGrid1.Selected.Rows.Clear();
                        ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error handling keyboard navigation: {ex.Message}");
            }
        }

        // --- BEGIN: Merge from frmdialForItemMaster ---
        // Column order combo box initialization
        private void InitializeColumnOrderComboBox()
        {
            // Clear any existing items
            comboBox2.Items.Clear();
            // Add column options for reordering (master columns only)
            comboBox2.Items.Add("Adjustment No");
            comboBox2.Items.Add("Date");
            comboBox2.Items.Add("Ledger/Reason");
            comboBox2.Items.Add("Comments");
            comboBox2.Items.Add("Status");
            // Select first by default
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the selected column option
                string selectedColumn = comboBox2.SelectedItem?.ToString() ?? "Doc Number";

                // Reorder columns based on selection
                ReorderColumns(selectedColumn);

                UpdateStatus($"Column order changed: {selectedColumn} is now first");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error changing column order: {ex.Message}");
            }
        }

        // Reorder columns based on selected option
        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            // Store current column widths
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                {
                    columnWidths[col.Key] = col.Width;
                }
            }

            // Suspend layout to prevent flickering
            ultraGrid1.SuspendLayout();

            // Define the columns to show in the specified order
            List<string> columnsToShow = new List<string>();

            // Map display names to column keys
            string columnKey = GetColumnKeyFromDisplayName(selectedColumn);

            // Add visible columns to the list
            foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden && !columnsToShow.Contains(col.Key))
                {
                    columnsToShow.Add(col.Key);
                }
            }

            // Move selected column to the front if it exists
            if (!string.IsNullOrEmpty(columnKey) && columnsToShow.Contains(columnKey))
            {
                columnsToShow.Remove(columnKey);
                columnsToShow.Insert(0, columnKey);
            }

            // Set the order
            for (int i = 0; i < columnsToShow.Count; i++)
            {
                string colKey = columnsToShow[i];
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                {
                    ultraGrid1.DisplayLayout.Bands[0].Columns[colKey].Header.VisiblePosition = i;

                    // Preserve width if we have it stored
                    if (columnWidths.ContainsKey(colKey))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns[colKey].Width = columnWidths[colKey];
                    }
                }
            }

            // Resume layout
            ultraGrid1.ResumeLayout();
            ultraGrid1.Refresh();
        }

        // Helper method to convert display name to column key
        private string GetColumnKeyFromDisplayName(string displayName)
        {
            switch (displayName)
            {
                case "Adjustment No": return "StockAdjustmentNo";
                case "Date": return "StockAdjustmentDate";
                case "Ledger/Reason": return "LedgerName";
                case "Comments": return "Comments";
                case "Status": return "Status";
                default: return "";
            }
        }
        // Set up the column chooser right-click menu
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += ColumnChooserMenuItem_Click;
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            // Set up direct header drag and drop
            SetupDirectHeaderDragDrop();
        }
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingColumn = false;
            columnToMove = null;
            startPoint = new Point(e.X, e.Y);
            if (e.Y < 40)
            {
                int xPos = 0;
                if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                {
                    xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                }
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        if (e.X >= xPos && e.X < xPos + col.Width)
                        {
                            columnToMove = col;
                            isDraggingColumn = true;
                            break;
                        }
                        xPos += col.Width;
                    }
                }
            }
        }
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
            {
                int deltaX = Math.Abs(e.X - startPoint.X);
                int deltaY = Math.Abs(e.Y - startPoint.Y);
                if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                {
                    bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);
                    if (isDraggingDown && (e.Y - startPoint.Y > 50))
                    {
                        HideColumn(columnToMove);
                        columnToMove = null;
                        isDraggingColumn = false;
                        ultraGrid1.Cursor = Cursors.Default;
                    }
                }
            }
        }
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            ultraGrid1.Cursor = Cursors.Default;
            isDraggingColumn = false;
            columnToMove = null;
        }
        private void HideColumn(Infragistics.Win.UltraWinGrid.UltraGridColumn column)
        {
            if (column != null && !column.Hidden)
            {
                string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? column.Header.Caption : column.Key;
                savedColumnWidths[column.Key] = column.Width;
                ultraGrid1.SuspendLayout();
                column.Hidden = true;

                // FIXED: Add to persistent hidden columns list
                if (!persistentHiddenColumns.Contains(column.Key))
                {
                    persistentHiddenColumns.Add(column.Key);
                }

                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }
                }
                ultraGrid1.ResumeLayout();
                if (columnChooserForm == null || columnChooserForm.IsDisposed)
                {
                    CreateColumnChooserForm();
                }
                PopulateColumnChooserListBox(); // Always update the chooser
            }
        }
        private void ColumnChooserMenuItem_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                PopulateColumnChooserListBox();
                columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }
            CreateColumnChooserForm();
            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
            this.LocationChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.SizeChanged += (s, evt) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, evt) => PositionColumnChooserAtBottomRight();
        }
        // --- END: Merge from frmdialForItemMaster ---

        // --- BEGIN: Merge from frmdialForItemMaster ---
        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();
            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };
            // Custom drawing for blue button style
            columnChooserListBox.DrawItem += (s, evt) => {
                if (evt.Index < 0) return;
                ColumnItem item = columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;
                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                Color bgColor = Color.FromArgb(33, 150, 243);
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 4, diameter = radius * 2;
                    Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                    path.AddArc(arcRect, 180, 90);
                    arcRect.X = rect.Right - diameter;
                    path.AddArc(arcRect, 270, 90);
                    arcRect.Y = rect.Bottom - diameter;
                    path.AddArc(arcRect, 0, 90);
                    arcRect.X = rect.Left;
                    path.AddArc(arcRect, 90, 90);
                    path.CloseFigure();
                    evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    evt.Graphics.FillPath(bgBrush, path);
                }
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, sf);
                }
                if ((evt.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    using (Pen focusPen = new Pen(Color.White, 1.5f))
                    {
                        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        Rectangle focusRect = rect;
                        focusRect.Inflate(-2, -2);
                        evt.Graphics.DrawRectangle(focusPen, focusRect);
                    }
                }
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserListBox.ScrollAlwaysVisible = false;
            PopulateColumnChooserListBox();
        }
        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    this.Right - columnChooserForm.Width - 20,
                    this.Bottom - columnChooserForm.Height - 20);
                columnChooserForm.TopMost = true;
                columnChooserForm.BringToFront();
            }
        }
        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserListBox != null)
            {
                columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                columnChooserListBox = null;
            }
        }
        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    columnChooserListBox.SelectedIndex = index;
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }
        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        // Keep only the new implementation of ColumnChooserListBox_DragDrop
        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Handle dropping a column from grid to chooser
                if (e.Data.GetDataPresent(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn)))
                {
                    var column = (Infragistics.Win.UltraWinGrid.UltraGridColumn)e.Data.GetData(typeof(Infragistics.Win.UltraWinGrid.UltraGridColumn));
                    if (column != null && !column.Hidden)
                    {
                        HideColumn(column);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error during column chooser drag-drop: {ex.Message}");
            }
        }
        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();

            // Define display names for MASTER columns only
            Dictionary<string, string> displayNames = new Dictionary<string, string>()
            {
                { "Id", "ID" },
                { "StockAdjustmentNo", "Adjustment No" },
                { "StockAdjustmentDate", "Date" },
                { "Comments", "Comments" },
                { "LedgerName", "Ledger/Reason" },
                { "LedgerId", "Ledger ID" },
                { "VoucherId", "Voucher ID" },
                { "Status", "Status" }
            };

            // Track columns we've already added to prevent duplicates
            HashSet<string> addedColumns = new HashSet<string>();

            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                // Add ALL hidden columns to the chooser
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (col.Hidden && !addedColumns.Contains(col.Key))
                    {
                        // Use display name if available, otherwise use column key or caption
                        string displayText;
                        if (displayNames.ContainsKey(col.Key))
                        {
                            displayText = displayNames[col.Key];
                        }
                        else if (!string.IsNullOrEmpty(col.Header.Caption))
                        {
                            displayText = col.Header.Caption;
                        }
                        else
                        {
                            displayText = col.Key;
                        }

                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                        addedColumns.Add(col.Key);
                    }
                }
            }

            // Log the number of items for debugging
            UpdateStatus($"Column chooser populated with {columnChooserListBox.Items.Count} hidden columns");
        }

        // Helper class for column chooser items
        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }
            public override string ToString() { return DisplayText; }
        }
        // --- END: Merge from frmdialForItemMaster ---

        // --- BEGIN: Merge from frmdialForItemMaster ---
        private void SetupDirectHeaderDragDrop()
        {
            ultraGrid1.AllowDrop = true;
            ultraGrid1.MouseDown += new MouseEventHandler(UltraGrid1_MouseDown);
            ultraGrid1.MouseMove += new MouseEventHandler(UltraGrid1_MouseMove);
            ultraGrid1.MouseUp += new MouseEventHandler(UltraGrid1_MouseUp);
            ultraGrid1.DragOver += new DragEventHandler(UltraGrid1_DragOver);
            ultraGrid1.DragDrop += new DragEventHandler(UltraGrid1_DragDrop);
        }
        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            // Accept ColumnItem drops from the column chooser
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Handle dropping a ColumnItem from the chooser onto the grid to restore a column
                if (e.Data.GetDataPresent(typeof(ColumnItem)))
                {
                    ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                    if (item != null && ultraGrid1.DisplayLayout.Bands.Count > 0 &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        // Get the column and unhide it
                        Infragistics.Win.UltraWinGrid.UltraGridColumn col =
                            ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                        col.Hidden = false;

                        // FIXED: Remove from persistent hidden columns list
                        persistentHiddenColumns.Remove(item.ColumnKey);

                        // Remove from the column chooser
                        for (int i = 0; i < columnChooserListBox.Items.Count; i++)
                        {
                            if (columnChooserListBox.Items[i] is ColumnItem listItem &&
                                listItem.ColumnKey == item.ColumnKey)
                            {
                                columnChooserListBox.Items.RemoveAt(i);
                                break;
                            }
                        }

                        // Restore the saved width if available
                        if (savedColumnWidths.ContainsKey(item.ColumnKey))
                        {
                            col.Width = savedColumnWidths[item.ColumnKey];
                        }

                        // Show feedback
                        this.toolTip.Show($"Column '{item.DisplayText}' restored",
                            ultraGrid1, ultraGrid1.PointToClient(Control.MousePosition), 2000);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error during drag-drop: {ex.Message}");
            }
        }
        private void UltraGrid1_AfterRowsDeleted(object sender, EventArgs e) { PreserveColumnWidths(); }
        private void UltraGrid1_AfterSortChange(object sender, Infragistics.Win.UltraWinGrid.BandEventArgs e) { PreserveColumnWidths(); }
        private void UltraGrid1_AfterColPosChanged(object sender, Infragistics.Win.UltraWinGrid.AfterColPosChangedEventArgs e) { PreserveColumnWidths(); }
        private void FrmDocDialog_SizeChanged(object sender, EventArgs e) { PreserveColumnWidths(); }
        private void UltraGrid1_Resize(object sender, EventArgs e) { PreserveColumnWidths(); }
        private void PreserveColumnWidths()
        {
            try
            {
                ultraGrid1.SuspendLayout();
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }
                    }
                }
                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error preserving column widths: {ex.Message}");
            }
        }

        private void InitializeSavedColumnWidths()
        {
            try
            {
                savedColumnWidths.Clear();
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!column.Hidden)
                        {
                            savedColumnWidths[column.Key] = column.Width;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing column widths: {ex.Message}");
            }
        }
        // --- END: Merge from frmdialForItemMaster ---

        // --- BEGIN: Merge from frmdialForItemMaster ---
        private void ultraPictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we have data to sort
                if (ultraGrid1.DataSource == null || ultraGrid1.Rows.Count == 0)
                {
                    UpdateStatus("No data to sort");
                    return;
                }

                // Toggle between original order and reverse order
                isOriginalOrder = !isOriginalOrder;

                // Update status message
                if (isOriginalOrder)
                {
                    UpdateStatus("Displaying documents in original order (first to last)");
                }
                else
                {
                    UpdateStatus("Displaying documents in reverse order (last to first)");
                }

                // Suspend layout while sorting
                ultraGrid1.SuspendLayout();

                // Get the current DataView
                if (ultraGrid1.DataSource is DataView dataView)
                {
                    if (fullDataTable != null && fullDataTable.Columns.Contains("OriginalRowOrder"))
                    {
                        dataView.Sort = isOriginalOrder ? "OriginalRowOrder ASC" : "OriginalRowOrder DESC";
                    }
                    else
                    {
                        if (isOriginalOrder)
                        {
                            dataView.Sort = "";  // Clear sort to restore original order
                        }
                        else
                        {
                            // Find the first visible column to sort by
                            string sortColumn = "";

                            if (ultraGrid1.DisplayLayout.Bands.Count > 0 && ultraGrid1.DisplayLayout.Bands[0].Columns.Count > 0)
                            {
                                // Find the first visible column
                                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                                {
                                    if (!col.Hidden)
                                    {
                                        sortColumn = col.Key;
                                        break;
                                    }
                                }

                                // If we found a column, sort by it in descending order
                                if (!string.IsNullOrEmpty(sortColumn))
                                {
                                    dataView.Sort = sortColumn + " DESC";
                                }
                            }
                        }
                    }

                    // Force the grid to refresh
                    ultraGrid1.Refresh();
                }

                // Resume layout
                ultraGrid1.ResumeLayout();

                // Update status
                UpdateStatus($"Sort order changed: {(isOriginalOrder ? "Original" : "Reverse")}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error during sort: {ex.Message}");
            }
        }
        // --- END: Merge from frmdialForItemMaster ---

        // --- BEGIN: Merge from frmdialForItemMaster ---
        // Navigation panel click events
        private void WirePanelAndPictureBoxClicks()
        {
            // Up navigation
            ultraPanel3.Click += (s, e) => MoveRowUp();
            ultraPictureBox5.Click += (s, e) => MoveRowUp();
            // Down navigation
            ultraPanel7.Click += (s, e) => MoveRowDown();
            ultraPictureBox6.Click += (s, e) => MoveRowDown();
            // Sort toggle
            ultraPictureBox4.Click += ultraPictureBox4_Click;
            ultraPanel9.Click += (s, e) => ultraPictureBox4_Click(s, e);
            // OK/Close
            ultraPanel5.Click += (s, e) => this.Close();
            label5.Click += (s, e) => this.Close();
            ultraPictureBox1.Click += (s, e) => this.Close();
            ultraPanel6.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            // textBox3: focus search box
            textBox3.Click += (s, e) => textBoxsearch.Focus();
        }
        private void MoveRowUp()
        {
            if (ultraGrid1.Rows.Count == 0) return;
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.Rows.Count - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                UpdateRecordCountLabel();
                return;
            }
            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex > 0)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }
        private void MoveRowDown()
        {
            if (ultraGrid1.Rows.Count == 0) return;
            if (ultraGrid1.ActiveRow == null)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                UpdateRecordCountLabel();
                return;
            }
            int currentIndex = ultraGrid1.ActiveRow.Index;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                var rowToActivate = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(rowToActivate);
                ultraGrid1.ActiveRow = rowToActivate;
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(rowToActivate);
            }
            UpdateRecordCountLabel();
        }
        // Call this in the constructor after InitializeComponent
        // WirePanelAndPictureBoxClicks();
        // --- END: Merge from frmdialForItemMaster ---

        // FIXED: Add method to set specific column widths
        private void InitializeColumnWidths()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

                ultraGrid1.SuspendLayout();

                // Define column widths for MASTER columns only
                Dictionary<string, int> columnWidths = new Dictionary<string, int>()
                {
                    { "Id", 60 },                      // ID column
                    { "StockAdjustmentNo", 120 },      // Adjustment number column
                    { "StockAdjustmentDate", 120 },    // Date column
                    { "Comments", 200 },               // Comments column (wider for visibility)
                    { "LedgerName", 150 },             // Ledger/Reason column (wider)
                    { "LedgerId", 80 },                // Ledger ID column
                    { "VoucherId", 80 },               // Voucher ID column
                    { "Status", 100 }                  // Status column
                };

                // Apply widths to columns
                foreach (var columnWidth in columnWidths)
                {
                    if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnWidth.Key))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns[columnWidth.Key].Width = columnWidth.Value;

                        // Store in saved widths dictionary for persistence
                        savedColumnWidths[columnWidth.Key] = columnWidth.Value;
                    }
                }

                // FIXED: Adjust header captions to match the image and be shorter where needed
                SetHeaderCaptions();

                // Ensure text alignment is appropriate for each column type
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    // Center numeric columns
                    if (col.DataType == typeof(int) || col.DataType == typeof(decimal) ||
                        col.DataType == typeof(double) || col.DataType == typeof(float))
                    {
                        col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                    }
                    // Center date columns
                    else if (col.DataType == typeof(DateTime))
                    {
                        col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                    }
                    // Left-align text columns
                    else
                    {
                        col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                    }
                }

                // FIXED: Set horizontal scrolling options to ensure proper scrolling
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;

                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting column widths: {ex.Message}");
            }
        }

        // FIXED: New method to set proper header captions
        private void SetHeaderCaptions()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            // Map of column keys to display captions for MASTER columns only
            Dictionary<string, string> headerCaptions = new Dictionary<string, string>()
            {
                { "Id", "ID" },
                { "StockAdjustmentNo", "Adjustment No" },
                { "StockAdjustmentDate", "Date" },
                { "Comments", "Comments" },
                { "LedgerName", "Ledger/Reason" },
                { "LedgerId", "Ledger ID" },
                { "VoucherId", "Voucher ID" },
                { "Status", "Status" }
            };

            // Apply the captions
            foreach (var caption in headerCaptions)
            {
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(caption.Key))
                {
                    ultraGrid1.DisplayLayout.Bands[0].Columns[caption.Key].Header.Caption = caption.Value;
                }
            }
        }

        // FIXED: Add method to ensure horizontal scrolling works properly
        private void EnsureHorizontalScrolling()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

                // Calculate total width of all columns
                int totalWidth = 0;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        totalWidth += col.Width;
                    }
                }

                // If total width is less than grid width, adjust the last column
                if (totalWidth < ultraGrid1.Width)
                {
                    // Find the last visible column
                    Infragistics.Win.UltraWinGrid.UltraGridColumn lastCol = null;
                    int lastVisiblePosition = -1;

                    foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && col.Header.VisiblePosition > lastVisiblePosition)
                        {
                            lastVisiblePosition = col.Header.VisiblePosition;
                            lastCol = col;
                        }
                    }

                    // Adjust the last column width if needed
                    if (lastCol != null)
                    {
                        int difference = ultraGrid1.Width - totalWidth - 25; // 25 pixels for scrollbar and margin
                        if (difference > 0)
                        {
                            lastCol.Width += difference;
                        }
                    }
                }

                // Set scroll properties
                // Ensure horizontal scrolling is enabled by setting scroll style
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up horizontal scrolling: {ex.Message}");
            }
        }

        // FIXED: Add form closing event handler to save column state
        private void FrmDocDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save column widths for next time
            SaveColumnState();
        }

        // FIXED: Add method to save column state
        private void SaveColumnState()
        {
            try
            {
                // Update hidden columns list before closing
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        // Save column width
                        if (!col.Hidden)
                        {
                            savedColumnWidths[col.Key] = col.Width;
                        }
                    }
                }

                // Note: persistentHiddenColumns is already updated when columns are hidden/shown
                UpdateStatus($"Saved column state: {persistentHiddenColumns.Count} hidden columns");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error saving column state: {ex.Message}");
            }
        }

        // FIXED: Add method to restore hidden columns
        private void RestoreHiddenColumns()
        {
            try
            {
                // FIXED: Add null check for persistentHiddenColumns
                if (persistentHiddenColumns == null)
                {
                    persistentHiddenColumns = new List<string>();
                    return;
                }

                // Check if there are any hidden columns to restore
                if (persistentHiddenColumns.Count == 0)
                {
                    UpdateStatus("No hidden columns to restore");
                    return;
                }

                if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null ||
                    ultraGrid1.DisplayLayout.Bands == null || ultraGrid1.DisplayLayout.Bands.Count == 0)
                {
                    UpdateStatus("Grid not fully initialized, cannot restore column state");
                    return;
                }

                ultraGrid1.SuspendLayout();

                // Hide columns that were previously hidden
                foreach (string columnKey in persistentHiddenColumns.ToList()) // Use ToList() to avoid collection modification issues
                {
                    if (!string.IsNullOrEmpty(columnKey) &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns != null &&
                        ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                    {
                        ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey].Hidden = true;
                    }
                }

                ultraGrid1.ResumeLayout();

                UpdateStatus($"Restored {persistentHiddenColumns.Count} hidden columns");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error restoring hidden columns: {ex.Message}");
                // Reset persistent columns if we encounter an error
                persistentHiddenColumns = new List<string>();
            }
        }

        // --- MISSING FEATURES from frmCustomerDialog ---

        // Panel Hover Effects for better UX
        private void SetupPanelHoverEffects()
        {
            try
            {
                SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
                SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
                SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
                SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
                SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error setting up panel hover effects: {ex.Message}");
            }
        }

        private void SetupPanelGroupHoverEffects(
            Infragistics.Win.Misc.UltraPanel panel,
            Label label,
            Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            if (panel == null) return;

            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            Action applyHoverEffect = () =>
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            Action removeHoverEffect = () =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };

            panel.MouseEnter += (s, e) => { applyHoverEffect(); };
            panel.MouseLeave += (s, e) => { removeHoverEffect(); };
            panel.ClientArea.MouseEnter += (s, e) => { applyHoverEffect(); };
            panel.ClientArea.MouseLeave += (s, e) => { removeHoverEffect(); };

            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => { applyHoverEffect(); pictureBox.Cursor = Cursors.Hand; };
                pictureBox.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }

            if (label != null)
            {
                label.MouseEnter += (s, e) => { applyHoverEffect(); label.Cursor = Cursors.Hand; };
                label.MouseLeave += (s, e) => { if (!IsMouseOverControl(panel)) removeHoverEffect(); };
            }
        }

        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        private bool IsMouseOverControl(Control control)
        {
            if (control == null) return false;
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }

        // Preserve original row order for DataTable
        private void PreserveOriginalRowOrder(DataTable table)
        {
            try
            {
                if (table == null) return;

                if (!table.Columns.Contains("OriginalRowOrder"))
                {
                    DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                    table.Columns.Add(orderColumn);
                    int rowIndex = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        row["OriginalRowOrder"] = rowIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error preserving row order: {ex.Message}");
            }
        }

        // Update record count label
        private void UpdateRecordCountLabel()
        {
            try
            {
                int currentDisplayCount = ultraGrid1.Rows?.Count ?? 0;
                int totalCount = fullDataTable?.Rows.Count ?? 0;

                if (label1 != null)
                {
                    label1.Text = $"Showing {currentDisplayCount} of {totalCount} records";
                    label1.AutoSize = true;
                    label1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular);
                    label1.ForeColor = Color.FromArgb(0, 70, 170);
                }

                if (textBox3 != null)
                {
                    textBox3.Text = currentDisplayCount.ToString();
                }

                UpdateStatus($"Showing {currentDisplayCount} of {totalCount} records");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error updating record count: {ex.Message}");
            }
        }
    }
}

