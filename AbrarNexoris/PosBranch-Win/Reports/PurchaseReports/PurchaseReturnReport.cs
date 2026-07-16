using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.PurchaseReports
{
    /// <summary>
    /// Professional Purchase Return Report Form with Hierarchical Master-Detail UltraGrid
    /// </summary>
    public partial class PurchaseReturnReport : Form
    {
        #region Private Fields
        private PurchaseReturnReportRepository reportRepository;
        private DataSet dsHierarchical;
        private bool isLoading = false;
        #endregion

        #region Constructor
        public PurchaseReturnReport()
        {
            InitializeComponent();
            InitializeForm();
        }
        #endregion

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new PurchaseReturnReportRepository();

                // Set form properties
                this.Text = "Purchase Return Report - Master Detail View";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                // Initialize date controls
                InitializeDateControls();

                // Initialize search controls
                InitializeSearchControls();

                // Initialize hierarchical DataSet
                InitializeHierarchicalDataSet();

                // Setup Hierarchical Grid
                SetupHierarchicalGrid();

                // Initialize panels
                InitializePanels();

                // Don't load data here - will load in Form_Load event after form is shown
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeDateControls()
        {
            // Set default date range (last 30 days)
            ultraDateTimeEditorFrom.Value = DateTime.Now.AddDays(-30);
            ultraDateTimeEditorTo.Value = DateTime.Now;

            // Set date format
            ultraDateTimeEditorFrom.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditorTo.FormatString = "dd/MM/yyyy";
        }

        private void InitializeSearchControls()
        {
            // Initialize preset date options
            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("Today", "Today");
            ultraComboPresetDates.Items.Add("Yesterday", "Yesterday");
            ultraComboPresetDates.Items.Add("This Week", "ThisWeek");
            ultraComboPresetDates.Items.Add("Last Week", "LastWeek");
            ultraComboPresetDates.Items.Add("This Month", "ThisMonth");
            ultraComboPresetDates.Items.Add("Last Month", "LastMonth");
            ultraComboPresetDates.Items.Add("This Quarter", "ThisQuarter");
            ultraComboPresetDates.Items.Add("Last Quarter", "LastQuarter");
            ultraComboPresetDates.Items.Add("This Year", "ThisYear");
            ultraComboPresetDates.Items.Add("Last Year", "LastYear");
            ultraComboPresetDates.Items.Add("Custom Range", "Custom");

            // Set default to "This Month"
            ultraComboPresetDates.Value = "ThisMonth";

            // Initialize numeric editors
            ultraNumericEditorAmountFrom.FormatString = "N2";
            ultraNumericEditorAmountTo.FormatString = "N2";
            ultraNumericEditorReturnNo.FormatString = "0";

            // Set placeholder text
            ultraTextEditorVendor.NullText = "Enter vendor name...";

            // Add tooltips for better UX
            InitializeTooltips();

            // Style buttons
            StyleButtons();

            // Add keyboard shortcuts
            SetupKeyboardShortcuts();
        }

        /// <summary>
        /// Initialize tooltips for better user experience
        /// </summary>
        private void InitializeTooltips()
        {
            try
            {
                System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
                toolTip.SetToolTip(ultraDateTimeEditorFrom, "Select start date for the report");
                toolTip.SetToolTip(ultraDateTimeEditorTo, "Select end date for the report");
                toolTip.SetToolTip(ultraComboPresetDates, "Quick date range selection");
                toolTip.SetToolTip(ultraNumericEditorReturnNo, "Enter specific return number to search");
                toolTip.SetToolTip(ultraTextEditorVendor, "Enter vendor name (partial match supported)");
                toolTip.SetToolTip(ultraNumericEditorAmountFrom, "Minimum amount filter");
                toolTip.SetToolTip(ultraNumericEditorAmountTo, "Maximum amount filter");
                toolTip.SetToolTip(btnSearch, "Search with current filters (F5)");
                toolTip.SetToolTip(btnClearFilters, "Clear all search filters (F6)");
                toolTip.SetToolTip(btnRefresh, "Refresh data (F5)");
                toolTip.SetToolTip(btnExport, "Export to Excel (Ctrl+E)");
                toolTip.SetToolTip(btnPrint, "Print report (Ctrl+P)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up tooltips: {ex.Message}");
            }
        }

        /// <summary>
        /// Style buttons for better appearance - Modern Material Design inspired
        /// </summary>
        private void StyleButtons()
        {
            try
            {
                // Modern Material Design color palette with enhanced styling

                // Style search button - Primary Blue
                btnSearch.UseAppStyling = false;
                btnSearch.UseOsThemes = DefaultableBoolean.False;
                btnSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);
                btnSearch.Appearance.BackColor2 = Color.FromArgb(33, 150, 243);
                btnSearch.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                btnSearch.Appearance.ForeColor = Color.White;
                btnSearch.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnSearch.Appearance.FontData.SizeInPoints = 10;
                btnSearch.Appearance.BorderColor = Color.FromArgb(21, 101, 192);

                // Style clear filters button - Orange Accent
                btnClearFilters.UseAppStyling = false;
                btnClearFilters.UseOsThemes = DefaultableBoolean.False;
                btnClearFilters.Appearance.BackColor = Color.FromArgb(245, 124, 0);
                btnClearFilters.Appearance.BackColor2 = Color.FromArgb(255, 152, 0);
                btnClearFilters.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                btnClearFilters.Appearance.ForeColor = Color.White;
                btnClearFilters.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnClearFilters.Appearance.FontData.SizeInPoints = 10;
                btnClearFilters.Appearance.BorderColor = Color.FromArgb(230, 81, 0);

                // Style refresh button - Green
                btnRefresh.UseAppStyling = false;
                btnRefresh.UseOsThemes = DefaultableBoolean.False;
                btnRefresh.Appearance.BackColor = Color.FromArgb(56, 142, 60);
                btnRefresh.Appearance.BackColor2 = Color.FromArgb(76, 175, 80);
                btnRefresh.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                btnRefresh.Appearance.ForeColor = Color.White;
                btnRefresh.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnRefresh.Appearance.FontData.SizeInPoints = 10;
                btnRefresh.Appearance.BorderColor = Color.FromArgb(46, 125, 50);

                // Style export button - Teal
                btnExport.UseAppStyling = false;
                btnExport.UseOsThemes = DefaultableBoolean.False;
                btnExport.Appearance.BackColor = Color.FromArgb(0, 121, 107);
                btnExport.Appearance.BackColor2 = Color.FromArgb(0, 150, 136);
                btnExport.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                btnExport.Appearance.ForeColor = Color.White;
                btnExport.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnExport.Appearance.FontData.SizeInPoints = 10;
                btnExport.Appearance.BorderColor = Color.FromArgb(0, 105, 92);

                // Style print button - Deep Purple
                btnPrint.UseAppStyling = false;
                btnPrint.UseOsThemes = DefaultableBoolean.False;
                btnPrint.Appearance.BackColor = Color.FromArgb(81, 45, 168);
                btnPrint.Appearance.BackColor2 = Color.FromArgb(103, 58, 183);
                btnPrint.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                btnPrint.Appearance.ForeColor = Color.White;
                btnPrint.Appearance.FontData.Bold = DefaultableBoolean.True;
                btnPrint.Appearance.FontData.SizeInPoints = 10;
                btnPrint.Appearance.BorderColor = Color.FromArgb(69, 39, 160);

                // Add hover effects for all buttons
                SetButtonHoverEffects();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error styling buttons: {ex.Message}");
            }
        }

        /// <summary>
        /// Set hover effects for buttons
        /// </summary>
        private void SetButtonHoverEffects()
        {
            try
            {
                // Search button hover
                btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
                btnSearch.HotTrackAppearance.ForeColor = Color.White;

                // Clear filters button hover
                btnClearFilters.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
                btnClearFilters.HotTrackAppearance.ForeColor = Color.White;

                // Refresh button hover
                btnRefresh.HotTrackAppearance.BackColor = Color.FromArgb(102, 187, 106);
                btnRefresh.HotTrackAppearance.ForeColor = Color.White;

                // Export button hover
                btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
                btnExport.HotTrackAppearance.ForeColor = Color.White;

                // Print button hover
                btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(126, 87, 194);
                btnPrint.HotTrackAppearance.ForeColor = Color.White;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting hover effects: {ex.Message}");
            }
        }

        /// <summary>
        /// Setup keyboard shortcuts
        /// </summary>
        private void SetupKeyboardShortcuts()
        {
            try
            {
                // Set up keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up keyboard shortcuts: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle keyboard shortcuts
        /// </summary>
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.E)
                {
                    btnExport_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    btnPrint_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F5)
                {
                    if (e.Control)
                        btnRefresh_Click(sender, e);
                    else
                        btnSearch_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F6)
                {
                    btnClearFilters_Click(sender, e);
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
                System.Diagnostics.Debug.WriteLine($"Error handling keyboard shortcut: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize hierarchical DataSet with master-detail relationship
        /// </summary>
        private void InitializeHierarchicalDataSet()
        {
            dsHierarchical = new DataSet("PurchaseReturnReport");

            // Create Master Table (Purchase Return Records)
            DataTable masterTable = new DataTable("PurchaseReturnMaster");
            masterTable.Columns.Add("PReturnNo", typeof(int));
            masterTable.Columns.Add("PReturnDate", typeof(DateTime));
            masterTable.Columns.Add("InvoiceNo", typeof(string));
            masterTable.Columns.Add("InvoiceDate", typeof(DateTime));
            masterTable.Columns.Add("VendorName", typeof(string));
            masterTable.Columns.Add("Paymode", typeof(string));
            masterTable.Columns.Add("SubTotal", typeof(decimal));
            masterTable.Columns.Add("GrandTotal", typeof(decimal));
            masterTable.PrimaryKey = new[] { masterTable.Columns["PReturnNo"] };

            // Create Detail Table (Return Items)
            DataTable detailTable = new DataTable("PurchaseReturnDetail");
            detailTable.Columns.Add("DetailID", typeof(int)); // Auto-increment unique ID
            detailTable.Columns.Add("PReturnNo", typeof(int));
            detailTable.Columns.Add("SlNo", typeof(int));
            detailTable.Columns.Add("ItemName", typeof(string));
            detailTable.Columns.Add("Unit", typeof(string));
            detailTable.Columns.Add("Packing", typeof(string));
            detailTable.Columns.Add("Qty", typeof(decimal));
            detailTable.Columns.Add("Cost", typeof(decimal));
            detailTable.Columns.Add("TaxPer", typeof(decimal));
            detailTable.Columns.Add("TaxAmt", typeof(decimal));
            detailTable.Columns.Add("Amount", typeof(decimal));
            detailTable.Columns.Add("Reason", typeof(string));

            // Set auto-increment for DetailID
            detailTable.Columns["DetailID"].AutoIncrement = true;
            detailTable.Columns["DetailID"].AutoIncrementSeed = 1;
            detailTable.Columns["DetailID"].AutoIncrementStep = 1;

            // Add tables to DataSet
            dsHierarchical.Tables.Add(masterTable);
            dsHierarchical.Tables.Add(detailTable);

            // Create relationship between master and detail
            DataRelation relation = new DataRelation(
                "MasterDetail",
                masterTable.Columns["PReturnNo"],
                detailTable.Columns["PReturnNo"],
                true
            );
            dsHierarchical.Relations.Add(relation);
        }

        /// <summary>
        /// Setup Hierarchical Grid for Master-Detail view
        /// </summary>
        private void SetupHierarchicalGrid()
        {
            try
            {
                // Reset grid layout
                ultraGridMaster.DisplayLayout.Reset();

                // Basic properties
                ultraGridMaster.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGridMaster.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGridMaster.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;

                // CRITICAL: Set ViewStyleBand to Vertical for hierarchical display
                ultraGridMaster.DisplayLayout.ViewStyleBand = ViewStyleBand.Vertical;

                // Enable row expansion
                ultraGridMaster.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
                ultraGridMaster.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGridMaster.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
                ultraGridMaster.DisplayLayout.Override.RowSelectorWidth = 40;
                ultraGridMaster.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGridMaster.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Style row selectors - Modern look
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

                // Allow row selection by clicking
                ultraGridMaster.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // Column interactions
                ultraGridMaster.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGridMaster.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

                // Appearance
                ultraGridMaster.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
                ultraGridMaster.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.GroupByBox.Hidden = true;

                // Row height
                ultraGridMaster.DisplayLayout.Override.MinRowHeight = 25;
                ultraGridMaster.DisplayLayout.Override.DefaultRowHeight = 25;

                // Modern selection colors - Material Design Blue
                ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
                ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

                // Modern header styling - Deep Blue-Grey gradient effect
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

                // Modern alternating row colors - Soft gradient
                ultraGridMaster.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGridMaster.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);

                // Modern hover effects - Light blue
                ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
                ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
                ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);

                // Add grid lines
                ultraGridMaster.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGridMaster.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

                // Event handlers
                ultraGridMaster.InitializeLayout += UltraGridMaster_InitializeLayout;
                ultraGridMaster.AfterRowExpanded += UltraGridMaster_AfterRowExpanded;
                ultraGridMaster.BeforeRowExpanded += UltraGridMaster_BeforeRowExpanded;
                ultraGridMaster.InitializeRow += UltraGridMaster_InitializeRow;

                System.Diagnostics.Debug.WriteLine("Hierarchical grid setup completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up hierarchical grid: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePanels()
        {
            // Setup master panel (contains hierarchical grid) - Modern clean white
            ultraPanelMaster.BackColor = Color.FromArgb(250, 251, 252);
            ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

            // Setup control panel - Modern gradient-like appearance
            ultraPanelControls.BackColor = Color.FromArgb(236, 240, 245);

            // Style summary caption labels - Modern bold headers
            StyleSummaryLabel(ultraLabelTotalReturnsCaption, Color.FromArgb(25, 118, 210), true);
            StyleSummaryLabel(ultraLabelSubTotalCaption, Color.FromArgb(56, 142, 60), true);
            StyleSummaryLabel(ultraLabelGrandTotalCaption, Color.FromArgb(123, 31, 162), true);

            // Style summary value labels - Large, bold, colorful
            StyleSummaryValueLabel(ultraLabelTotalReturnsValue, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(ultraLabelSubTotalValue, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(ultraLabelGrandTotalValue, Color.FromArgb(74, 20, 140), 16);
        }

        /// <summary>
        /// Style summary caption labels
        /// </summary>
        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 10;
            label.Appearance.TextHAlign = Infragistics.Win.HAlign.Left;
        }

        /// <summary>
        /// Style summary value labels with larger font
        /// </summary>
        private void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, float fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
            label.Appearance.TextHAlign = Infragistics.Win.HAlign.Left;
        }
        #endregion

        #region UltraGrid Layout Configuration
        /// <summary>
        /// Configure Hierarchical Grid layout
        /// </summary>
        private void UltraGridMaster_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Configure Master Band (Band 0)
                if (e.Layout.Bands.Count > 0)
                {
                    ConfigureMasterBandColumns(e.Layout.Bands[0]);
                }

                // Configure Detail Band (Band 1)
                if (e.Layout.Bands.Count > 1)
                {
                    ConfigureDetailBandColumns(e.Layout.Bands[1]);

                    // Set detail band caption
                    e.Layout.Bands[1].Header.Caption = "Return Details";
                    e.Layout.Bands[1].HeaderVisible = true;
                }

                // Enable AutoFit for all columns
                e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring grid layout: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configure columns for Master Band (Purchase Return Records)
        /// </summary>
        private void ConfigureMasterBandColumns(UltraGridBand masterBand)
        {
            // PReturnNo column
            if (masterBand.Columns["PReturnNo"] != null)
            {
                masterBand.Columns["PReturnNo"].Header.Caption = "Return No";
                masterBand.Columns["PReturnNo"].Width = 100;
                masterBand.Columns["PReturnNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["PReturnNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
            }

            // Format date columns
            if (masterBand.Columns["PReturnDate"] != null)
            {
                masterBand.Columns["PReturnDate"].Format = "dd/MM/yyyy";
                masterBand.Columns["PReturnDate"].Header.Caption = "Return Date";
                masterBand.Columns["PReturnDate"].Width = 100;
            }

            if (masterBand.Columns["InvoiceNo"] != null)
            {
                masterBand.Columns["InvoiceNo"].Header.Caption = "Invoice No";
                masterBand.Columns["InvoiceNo"].Width = 100;
            }

            if (masterBand.Columns["InvoiceDate"] != null)
            {
                masterBand.Columns["InvoiceDate"].Format = "dd/MM/yyyy";
                masterBand.Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                masterBand.Columns["InvoiceDate"].Width = 100;
            }

            if (masterBand.Columns["VendorName"] != null)
            {
                masterBand.Columns["VendorName"].Header.Caption = "Vendor";
                masterBand.Columns["VendorName"].Width = 200;
            }

            if (masterBand.Columns["Paymode"] != null)
            {
                masterBand.Columns["Paymode"].Header.Caption = "Payment Mode";
                masterBand.Columns["Paymode"].Width = 120;
                masterBand.Columns["Paymode"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            // Format currency columns with modern styling
            if (masterBand.Columns["SubTotal"] != null)
            {
                masterBand.Columns["SubTotal"].Format = "? #,##0.00";
                masterBand.Columns["SubTotal"].Header.Caption = "Sub Total";
                masterBand.Columns["SubTotal"].Width = 110;
                masterBand.Columns["SubTotal"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (masterBand.Columns["GrandTotal"] != null)
            {
                masterBand.Columns["GrandTotal"].Format = "? #,##0.00";
                masterBand.Columns["GrandTotal"].Header.Caption = "Grand Total";
                masterBand.Columns["GrandTotal"].Width = 120;
                masterBand.Columns["GrandTotal"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                masterBand.Columns["GrandTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["GrandTotal"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }
        }

        /// <summary>
        /// Configure columns for Detail Band (Return Items)
        /// </summary>
        private void ConfigureDetailBandColumns(UltraGridBand detailBand)
        {
            // Hide DetailID (auto-increment) and PReturnNo (foreign key)
            if (detailBand.Columns["DetailID"] != null)
                detailBand.Columns["DetailID"].Hidden = true;
            if (detailBand.Columns["PReturnNo"] != null)
                detailBand.Columns["PReturnNo"].Hidden = true;

            // Set column captions and widths with modern formatting
            if (detailBand.Columns["SlNo"] != null)
            {
                detailBand.Columns["SlNo"].Header.Caption = "S.No";
                detailBand.Columns["SlNo"].Width = 50;
                detailBand.Columns["SlNo"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["ItemName"] != null)
            {
                detailBand.Columns["ItemName"].Header.Caption = "Item Name";
                detailBand.Columns["ItemName"].Width = 220;
                detailBand.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (detailBand.Columns["Unit"] != null)
            {
                detailBand.Columns["Unit"].Header.Caption = "Unit";
                detailBand.Columns["Unit"].Width = 60;
                detailBand.Columns["Unit"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["Packing"] != null)
            {
                detailBand.Columns["Packing"].Header.Caption = "Packing";
                detailBand.Columns["Packing"].Width = 80;
            }

            if (detailBand.Columns["Qty"] != null)
            {
                detailBand.Columns["Qty"].Header.Caption = "Quantity";
                detailBand.Columns["Qty"].Format = "0.00";
                detailBand.Columns["Qty"].Width = 80;
                detailBand.Columns["Qty"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (detailBand.Columns["Cost"] != null)
            {
                detailBand.Columns["Cost"].Header.Caption = "Cost";
                detailBand.Columns["Cost"].Format = "? #,##0.00";
                detailBand.Columns["Cost"].Width = 90;
                detailBand.Columns["Cost"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (detailBand.Columns["TaxPer"] != null)
            {
                detailBand.Columns["TaxPer"].Header.Caption = "Tax %";
                detailBand.Columns["TaxPer"].Format = "0.00 %";
                detailBand.Columns["TaxPer"].Width = 70;
                detailBand.Columns["TaxPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["TaxAmt"] != null)
            {
                detailBand.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                detailBand.Columns["TaxAmt"].Format = "? #,##0.00";
                detailBand.Columns["TaxAmt"].Width = 100;
                detailBand.Columns["TaxAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
            }

            if (detailBand.Columns["Amount"] != null)
            {
                detailBand.Columns["Amount"].Header.Caption = "Amount";
                detailBand.Columns["Amount"].Format = "? #,##0.00";
                detailBand.Columns["Amount"].Width = 120;
                detailBand.Columns["Amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            if (detailBand.Columns["Reason"] != null)
            {
                detailBand.Columns["Reason"].Header.Caption = "Return Reason";
                detailBand.Columns["Reason"].Width = 200;
                detailBand.Columns["Reason"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);
            }

            // Configure summaries for detail band
            ConfigureDetailBandSummaries(detailBand);

            // Style detail band differently - Modern gradient look
            detailBand.Override.RowAppearance.BackColor = Color.FromArgb(252, 252, 255);
            detailBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 248, 252);

            // Detail band header styling - Modern accent color
            detailBand.Override.HeaderAppearance.BackColor = Color.FromArgb(41, 128, 185);
            detailBand.Override.HeaderAppearance.ForeColor = Color.White;
            detailBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
        }

        /// <summary>
        /// Configure summaries (totals) for detail band columns
        /// </summary>
        private void ConfigureDetailBandSummaries(UltraGridBand detailBand)
        {
            try
            {
                // Enable summary footer
                detailBand.SummaryFooterCaption = "Detail Totals:";

                // Add summaries for key columns
                if (detailBand.Columns["Qty"] != null)
                {
                    SummarySettings sumQty = detailBand.Summaries.Add("SumQty", SummaryType.Sum, detailBand.Columns["Qty"], SummaryPosition.UseSummaryPositionColumn);
                    sumQty.DisplayFormat = "Qty: {0:N2}";
                    sumQty.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumQty.Appearance.ForeColor = Color.FromArgb(44, 62, 80);
                    sumQty.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (detailBand.Columns["TaxAmt"] != null)
                {
                    SummarySettings sumTax = detailBand.Summaries.Add("SumTax", SummaryType.Sum, detailBand.Columns["TaxAmt"], SummaryPosition.UseSummaryPositionColumn);
                    sumTax.DisplayFormat = "? {0:N2}";
                    sumTax.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumTax.Appearance.ForeColor = Color.FromArgb(211, 84, 0);
                    sumTax.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (detailBand.Columns["Amount"] != null)
                {
                    SummarySettings sumAmount = detailBand.Summaries.Add("SumAmount", SummaryType.Sum, detailBand.Columns["Amount"], SummaryPosition.UseSummaryPositionColumn);
                    sumAmount.DisplayFormat = "? {0:N2}";
                    sumAmount.Appearance.BackColor = Color.FromArgb(52, 73, 94);
                    sumAmount.Appearance.ForeColor = Color.White;
                    sumAmount.Appearance.FontData.Bold = DefaultableBoolean.True;
                    sumAmount.Appearance.FontData.SizeInPoints = 10;
                }

                // Style the summary footer
                detailBand.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(236, 240, 241);
                detailBand.Override.SummaryFooterAppearance.ForeColor = Color.FromArgb(44, 62, 80);
                detailBand.Override.SummaryFooterAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Override.SummaryFooterAppearance.BorderColor = Color.FromArgb(52, 152, 219);

                System.Diagnostics.Debug.WriteLine($"Configured {detailBand.Summaries.Count} summaries for detail band");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring summaries: {ex.Message}");
            }
        }
        #endregion

        #region Data Loading Methods
        /// <summary>
        /// Load all purchase return data without filters
        /// </summary>
        private void LoadPurchaseReturnData()
        {
            try
            {
                isLoading = true;

                DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeEditorTo.Value);

                // STEP 1: Unbind grid completely
                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataMember = null;
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // STEP 2: Remove relation
                if (dsHierarchical.Relations.Count > 0)
                {
                    dsHierarchical.Relations.Clear();
                }

                // STEP 3: Clear tables
                dsHierarchical.Tables["PurchaseReturnDetail"].Clear();
                dsHierarchical.Tables["PurchaseReturnMaster"].Clear();

                // STEP 4: Load master data
                LoadMasterData(fromDate, toDate);

                // STEP 5: Load detail data
                LoadAllDetailData(fromDate, toDate);

                // STEP 6: Recreate relation
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                DataTable detailTable = dsHierarchical.Tables["PurchaseReturnDetail"];

                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["PReturnNo"],
                    detailTable.Columns["PReturnNo"],
                    true
                );
                dsHierarchical.Relations.Add(relation);

                // STEP 7: Accept changes
                dsHierarchical.AcceptChanges();

                // STEP 8: Rebind grid
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "PurchaseReturnMaster";

                // STEP 9: Force refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // STEP 10: Update totals
                UpdateGrandTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase return data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Load master data (Purchase Return Records)
        /// </summary>
        private void LoadMasterData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                int branchId = SessionContext.BranchId;
                List<PurchaseReturnReportMaster> masterData = reportRepository.GetPurchaseReturnRecords(fromDate, toDate, branchId);
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];

                foreach (var returnRecord in masterData)
                {
                    if (returnRecord.PReturnNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["PReturnNo"] = returnRecord.PReturnNo;
                        row["PReturnDate"] = returnRecord.PReturnDate;
                        row["InvoiceNo"] = returnRecord.InvoiceNo ?? "";
                        row["InvoiceDate"] = returnRecord.InvoiceDate;
                        row["VendorName"] = returnRecord.VendorName ?? "";
                        row["Paymode"] = returnRecord.Paymode ?? "";
                        row["SubTotal"] = returnRecord.SubTotal;
                        row["GrandTotal"] = returnRecord.GrandTotal;
                        masterTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading master data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load ALL detail data for the date range (optimization)
        /// </summary>
        private void LoadAllDetailData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                DataTable detailTable = dsHierarchical.Tables["PurchaseReturnDetail"];

                // Get all PReturnNo values from master
                var returnNumbers = dsHierarchical.Tables["PurchaseReturnMaster"].AsEnumerable()
                    .Select(r => r.Field<int>("PReturnNo"))
                    .ToList();

                // Load details for each return
                foreach (int pReturnNo in returnNumbers)
                {
                    PurchaseReturnReportData reportData = reportRepository.GetPurchaseReturnReportDetails(pReturnNo);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            // DetailID will auto-increment
                            row["PReturnNo"] = pReturnNo;
                            row["SlNo"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["Qty"] = detail.Qty;
                            row["Cost"] = detail.Cost;
                            row["TaxPer"] = detail.TaxPer;
                            row["TaxAmt"] = detail.TaxAmt;
                            row["Amount"] = detail.Amount;
                            row["Reason"] = detail.Reason ?? "";
                            detailTable.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading detail data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update grand totals for all loaded data and display in summary panel
        /// </summary>
        private void UpdateGrandTotals()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                decimal grandTotal = 0;
                decimal grandSubTotal = 0;
                int totalReturns = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    grandSubTotal += Convert.ToDecimal(row["SubTotal"]);
                    grandTotal += Convert.ToDecimal(row["GrandTotal"]);
                }

                // Update summary labels with calculated totals
                ultraLabelTotalReturnsValue.Text = totalReturns.ToString("N0");
                ultraLabelSubTotalValue.Text = $"? {grandSubTotal:N2}";
                ultraLabelGrandTotalValue.Text = $"? {grandTotal:N2}";

                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Grand Totals Updated - Returns: {totalReturns}, SubTotal: {grandSubTotal:N2}, Grand: {grandTotal:N2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating grand totals: {ex.Message}");
                // Reset to zeros on error
                ultraLabelTotalReturnsValue.Text = "0";
                ultraLabelSubTotalValue.Text = "? 0.00";
                ultraLabelGrandTotalValue.Text = "? 0.00";
            }
        }
        #endregion

        #region Grid Event Handlers
        /// <summary>
        /// Initialize row event - enhance row appearance
        /// </summary>
        private void UltraGridMaster_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                // Only style master band (Band 0)
                if (e.Row.Band.Index == 0)
                {
                    // Enhanced alternating rows with better colors
                    if (e.Row.Index % 2 == 0)
                    {
                        e.Row.Appearance.BackColor = Color.White;
                    }
                    else
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(246, 248, 252);
                    }

                    // Add subtle border to rows for better separation
                    e.Row.Appearance.BorderColor = Color.FromArgb(224, 224, 224);
                }
                else if (e.Row.Band.Index == 1) // Detail band styling
                {
                    // Enhanced alternating rows for detail band
                    if (e.Row.Index % 2 == 0)
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(252, 252, 255);
                    }
                    else
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(245, 247, 252);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeRow: {ex.Message}");
            }
        }

        /// <summary>
        /// Before row expanded event
        /// </summary>
        private void UltraGridMaster_BeforeRowExpanded(object sender, CancelableRowEventArgs e)
        {
            try
            {
                // Details are already loaded, so just update summary
                if (e.Row.Band.Index == 0 && e.Row.Cells["PReturnNo"] != null)
                {
                    var dataRowView = e.Row.ListObject as DataRowView;
                    if (dataRowView != null)
                    {
                        // Can add additional logic here if needed
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BeforeRowExpanded: {ex.Message}");
            }
        }

        /// <summary>
        /// After row expanded event
        /// </summary>
        private void UltraGridMaster_AfterRowExpanded(object sender, RowEventArgs e)
        {
            try
            {
                if (e.Row.Band.Index == 0 && e.Row.Cells["PReturnNo"] != null)
                {
                    int pReturnNo = Convert.ToInt32(e.Row.Cells["PReturnNo"].Value);
                    int detailCount = e.Row.ChildBands[0].Rows.Count;

                    System.Diagnostics.Debug.WriteLine($"Expanded Return No: {pReturnNo}, Detail Count: {detailCount}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AfterRowExpanded: {ex.Message}");
            }
        }
        #endregion

        #region Button Event Handlers
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                LoadPurchaseReturnData();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (dsHierarchical.Tables["PurchaseReturnMaster"].Rows.Count > 0)
                {
                    PrintPurchaseReturnReport();
                }
                else
                {
                    MessageBox.Show("No data available to print.", "Print",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Print the purchase return report
        /// </summary>
        private void PrintPurchaseReturnReport()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = "Purchase Return Report";
                printDocument.PrintPage += PrintDocument_PrintPage;

                printDialog.Document = printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Print document event handler
        /// </summary>
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                Font titleFont = new Font("Arial", 16, FontStyle.Bold);
                Font headerFont = new Font("Arial", 10, FontStyle.Bold);
                Font dataFont = new Font("Arial", 9);
                Font summaryFont = new Font("Arial", 10, FontStyle.Bold);

                float yPosition = 50;
                float leftMargin = 50;
                float rightMargin = e.MarginBounds.Right;

                // Print title
                e.Graphics.DrawString("PURCHASE RETURN REPORT - MASTER DETAIL VIEW", titleFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print date range
                string dateRange = $"From: {ultraDateTimeEditorFrom.Value:dd/MM/yyyy} To: {ultraDateTimeEditorTo.Value:dd/MM/yyyy}";
                e.Graphics.DrawString(dateRange, dataFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;

                // Print summary
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                decimal totalAmount = 0;
                int totalReturns = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    totalAmount += Convert.ToDecimal(row["GrandTotal"]);
                }

                e.Graphics.DrawString($"Total Returns: {totalReturns}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Grand Total: ? {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print master data headers
                string[] headers = { "Return No", "Date", "Vendor", "Payment", "Sub Total", "Grand Total" };
                float[] columnWidths = { 90, 90, 180, 100, 110, 110 };
                float xPosition = leftMargin;

                for (int i = 0; i < headers.Length; i++)
                {
                    e.Graphics.DrawString(headers[i], headerFont, Brushes.Black, xPosition, yPosition);
                    xPosition += columnWidths[i];
                }
                yPosition += 25;

                // Draw line under headers
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition - 5, rightMargin, yPosition - 5);
                yPosition += 10;

                // Print master data
                foreach (DataRow row in masterTable.Rows)
                {
                    if (yPosition > e.MarginBounds.Bottom - 100)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    xPosition = leftMargin;
                    string[] values = {
                        row["PReturnNo"].ToString(),
                        Convert.ToDateTime(row["PReturnDate"]).ToString("dd/MM/yyyy"),
                        row["VendorName"].ToString(),
                        row["Paymode"].ToString(),
                        Convert.ToDecimal(row["SubTotal"]).ToString("N2"),
                        Convert.ToDecimal(row["GrandTotal"]).ToString("N2")
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        e.Graphics.DrawString(values[i], dataFont, Brushes.Black, xPosition, yPosition);
                        xPosition += columnWidths[i];
                    }
                    yPosition += 20;
                }

                // Print totals at bottom
                yPosition = e.MarginBounds.Bottom - 50;
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPosition, rightMargin, yPosition);
                yPosition += 10;
                e.Graphics.DrawString($"GRAND TOTAL: ? {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
            }
            catch (Exception ex)
            {
                e.Graphics.DrawString($"Error printing: {ex.Message}", new Font("Arial", 10), Brushes.Red, 50, 50);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dsHierarchical.Tables["PurchaseReturnMaster"].Rows.Count > 0)
                {
                    ExportToExcel();
                }
                else
                {
                    MessageBox.Show("No data available to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // Validate date range
            if (!ValidateDateRange())
                return;

            // Validate amount range
            if (!ValidateAmountRange())
                return;

            // Show wait cursor during search
            this.Cursor = Cursors.WaitCursor;
            try
            {
                LoadPurchaseReturnDataWithFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Validate date range input
        /// </summary>
        private bool ValidateDateRange()
        {
            try
            {
                DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeEditorTo.Value);

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be greater than To date.", "Invalid Date Range",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraDateTimeEditorFrom.Focus();
                    return false;
                }

                if (fromDate > DateTime.Now)
                {
                    MessageBox.Show("From date cannot be in the future.", "Invalid Date",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraDateTimeEditorFrom.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid date format: {ex.Message}", "Date Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Validate amount range input
        /// </summary>
        private bool ValidateAmountRange()
        {
            try
            {
                if (ultraNumericEditorAmountFrom.Value != null && ultraNumericEditorAmountTo.Value != null)
                {
                    decimal fromAmount = Convert.ToDecimal(ultraNumericEditorAmountFrom.Value);
                    decimal toAmount = Convert.ToDecimal(ultraNumericEditorAmountTo.Value);

                    if (fromAmount > toAmount)
                    {
                        MessageBox.Show("From amount cannot be greater than To amount.", "Invalid Amount Range",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ultraNumericEditorAmountFrom.Focus();
                        return false;
                    }

                    if (fromAmount < 0 || toAmount < 0)
                    {
                        MessageBox.Show("Amount values cannot be negative.", "Invalid Amount",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid amount format: {ex.Message}", "Amount Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Clear filters and reload
        /// </summary>
        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // Clear all search filters
                ultraDateTimeEditorFrom.Value = DateTime.Now.AddDays(-30);
                ultraDateTimeEditorTo.Value = DateTime.Now;
                ultraNumericEditorAmountFrom.Value = null;
                ultraNumericEditorAmountTo.Value = null;
                ultraNumericEditorReturnNo.Value = null;

                // Clear vendor text field - set both Value and Text to ensure it's cleared
                ultraTextEditorVendor.Value = null;
                ultraTextEditorVendor.Text = string.Empty;

                ultraComboPresetDates.Value = "ThisMonth";

                // Force UI update
                Application.DoEvents();

                // Reload all data
                LoadPurchaseReturnData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing filters: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ultraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ultraComboPresetDates.Value == null) return;

                string preset = ultraComboPresetDates.Value.ToString();
                DateTime fromDate, toDate;

                switch (preset)
                {
                    case "Today":
                        fromDate = DateTime.Now.Date;
                        toDate = DateTime.Now.Date;
                        break;
                    case "Yesterday":
                        fromDate = DateTime.Now.AddDays(-1).Date;
                        toDate = DateTime.Now.AddDays(-1).Date;
                        break;
                    case "ThisWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date;
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7).Date;
                        toDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1).Date;
                        break;
                    case "ThisMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                        break;
                    case "ThisQuarter":
                        int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                        fromDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastQuarter":
                        int lastQuarter = (DateTime.Now.Month - 1) / 3;
                        if (lastQuarter == 0)
                        {
                            lastQuarter = 4;
                            fromDate = new DateTime(DateTime.Now.Year - 1, 10, 1);
                        }
                        else
                        {
                            fromDate = new DateTime(DateTime.Now.Year, (lastQuarter - 1) * 3 + 1, 1);
                        }
                        toDate = fromDate.AddMonths(3).AddDays(-1);
                        break;
                    case "ThisYear":
                        fromDate = new DateTime(DateTime.Now.Year, 1, 1);
                        toDate = DateTime.Now.Date;
                        break;
                    case "LastYear":
                        fromDate = new DateTime(DateTime.Now.Year - 1, 1, 1);
                        toDate = new DateTime(DateTime.Now.Year - 1, 12, 31);
                        break;
                    default:
                        return; // Custom range - don't change dates
                }

                ultraDateTimeEditorFrom.Value = fromDate;
                ultraDateTimeEditorTo.Value = toDate;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting preset dates: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load purchase return data with applied filters
        /// </summary>
        private void LoadPurchaseReturnDataWithFilters()
        {
            try
            {
                isLoading = true;
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeEditorTo.Value);

                // CRITICAL: Completely unbind and reset grid
                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataMember = null;
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Clear the DataSet completely and rebuild from scratch
                RebuildDataSetWithFilters(fromDate, toDate);

                // Rebind the grid with fresh data
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "PurchaseReturnMaster";

                // Force complete grid refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Update totals
                UpdateGrandTotals();

                // Show result count
                int resultCount = dsHierarchical.Tables["PurchaseReturnMaster"].Rows.Count;
                if (resultCount == 0)
                {
                    MessageBox.Show("No records found matching the search criteria.", "Search Results",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching purchase return data: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Rebuild the entire DataSet with filters applied
        /// </summary>
        private void RebuildDataSetWithFilters(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Step 1: Remove the relation temporarily
                if (dsHierarchical.Relations.Count > 0)
                {
                    dsHierarchical.Relations.Clear();
                }

                // Step 2: Clear both tables
                dsHierarchical.Tables["PurchaseReturnDetail"].Clear();
                dsHierarchical.Tables["PurchaseReturnMaster"].Clear();

                // Step 3: Load master data from database
                int branchId = SessionContext.BranchId;
                List<PurchaseReturnReportMaster> masterData = reportRepository.GetPurchaseReturnRecords(fromDate, toDate, branchId);

                // Step 4: Apply filters to master data BEFORE adding to DataTable
                var filteredMasterData = ApplyMasterFilters(masterData);

                // Step 5: Add filtered master data to DataTable
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                foreach (var returnRecord in filteredMasterData)
                {
                    if (returnRecord.PReturnNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["PReturnNo"] = returnRecord.PReturnNo;
                        row["PReturnDate"] = returnRecord.PReturnDate;
                        row["InvoiceNo"] = returnRecord.InvoiceNo ?? "";
                        row["InvoiceDate"] = returnRecord.InvoiceDate;
                        row["VendorName"] = returnRecord.VendorName ?? "";
                        row["Paymode"] = returnRecord.Paymode ?? "";
                        row["SubTotal"] = returnRecord.SubTotal;
                        row["GrandTotal"] = returnRecord.GrandTotal;
                        masterTable.Rows.Add(row);
                    }
                }

                // Step 6: Load details only for filtered returns
                if (masterTable.Rows.Count > 0)
                {
                    LoadDetailsForFilteredReturns();
                }

                // Step 7: Recreate the relation
                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["PReturnNo"],
                    dsHierarchical.Tables["PurchaseReturnDetail"].Columns["PReturnNo"],
                    true
                );
                dsHierarchical.Relations.Add(relation);

                // Step 8: Accept all changes
                dsHierarchical.AcceptChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error rebuilding dataset with filters: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Apply filters to master data list BEFORE adding to DataTable
        /// </summary>
        private List<PurchaseReturnReportMaster> ApplyMasterFilters(List<PurchaseReturnReportMaster> masterData)
        {
            var filtered = masterData.AsEnumerable();

            // Filter by Return Number
            if (ultraNumericEditorReturnNo.Value != null && Convert.ToInt32(ultraNumericEditorReturnNo.Value) > 0)
            {
                int returnNo = Convert.ToInt32(ultraNumericEditorReturnNo.Value);
                filtered = filtered.Where(x => x.PReturnNo == returnNo);
            }

            // Filter by Vendor Name (case-insensitive partial match)
            // Check both Value and Text to handle Infragistics control behavior
            string vendorText = ultraTextEditorVendor.Value?.ToString() ?? ultraTextEditorVendor.Text;
            if (!string.IsNullOrWhiteSpace(vendorText))
            {
                string vendorSearch = vendorText.Trim().ToLower();
                filtered = filtered.Where(x =>
                    !string.IsNullOrEmpty(x.VendorName) &&
                    x.VendorName.ToLower().Contains(vendorSearch));
            }

            // Filter by Amount Range
            if (ultraNumericEditorAmountFrom.Value != null && Convert.ToDecimal(ultraNumericEditorAmountFrom.Value) > 0)
            {
                decimal fromAmount = Convert.ToDecimal(ultraNumericEditorAmountFrom.Value);
                filtered = filtered.Where(x => x.GrandTotal >= (double)fromAmount);
            }

            if (ultraNumericEditorAmountTo.Value != null && Convert.ToDecimal(ultraNumericEditorAmountTo.Value) > 0)
            {
                decimal toAmount = Convert.ToDecimal(ultraNumericEditorAmountTo.Value);
                filtered = filtered.Where(x => x.GrandTotal <= (double)toAmount);
            }

            return filtered.ToList();
        }

        /// <summary>
        /// Load details only for returns that passed the filter
        /// </summary>
        private void LoadDetailsForFilteredReturns()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["PurchaseReturnMaster"];
                DataTable detailTable = dsHierarchical.Tables["PurchaseReturnDetail"];

                // Get all PReturnNo values from filtered master
                var returnNumbers = masterTable.AsEnumerable()
                    .Select(r => r.Field<int>("PReturnNo"))
                    .ToList();

                // Load details for each filtered return
                foreach (int pReturnNo in returnNumbers)
                {
                    PurchaseReturnReportData reportData = reportRepository.GetPurchaseReturnReportDetails(pReturnNo);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            row["PReturnNo"] = pReturnNo;
                            row["SlNo"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["Qty"] = detail.Qty;
                            row["Cost"] = detail.Cost;
                            row["TaxPer"] = detail.TaxPer;
                            row["TaxAmt"] = detail.TaxAmt;
                            row["Amount"] = detail.Amount;
                            row["Reason"] = detail.Reason ?? "";
                            detailTable.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading filtered details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Export data to Excel
        /// </summary>
        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|Excel Files (*.xls)|*.xls";
                saveDialog.FileName = $"PurchaseReturnReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create a new DataTable for export with flattened data
                    DataTable exportTable = CreateExportTable();

                    // Export to Excel using simple CSV approach
                    ExportToCSV(exportTable, saveDialog.FileName);

                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Create flattened table for export
        /// </summary>
        private DataTable CreateExportTable()
        {
            DataTable exportTable = new DataTable("PurchaseReturnReport");

            // Add columns
            exportTable.Columns.Add("PReturnNo", typeof(int));
            exportTable.Columns.Add("PReturnDate", typeof(DateTime));
            exportTable.Columns.Add("InvoiceNo", typeof(string));
            exportTable.Columns.Add("InvoiceDate", typeof(DateTime));
            exportTable.Columns.Add("VendorName", typeof(string));
            exportTable.Columns.Add("Paymode", typeof(string));
            exportTable.Columns.Add("ItemName", typeof(string));
            exportTable.Columns.Add("Unit", typeof(string));
            exportTable.Columns.Add("Quantity", typeof(decimal));
            exportTable.Columns.Add("Cost", typeof(decimal));
            exportTable.Columns.Add("TaxPer", typeof(decimal));
            exportTable.Columns.Add("TaxAmt", typeof(decimal));
            exportTable.Columns.Add("Amount", typeof(decimal));
            exportTable.Columns.Add("Reason", typeof(string));
            exportTable.Columns.Add("SubTotal", typeof(decimal));
            exportTable.Columns.Add("GrandTotal", typeof(decimal));

            // Flatten the hierarchical data
            foreach (DataRow masterRow in dsHierarchical.Tables["PurchaseReturnMaster"].Rows)
            {
                DataRow[] detailRows = dsHierarchical.Tables["PurchaseReturnDetail"].Select($"PReturnNo = {masterRow["PReturnNo"]}");

                if (detailRows.Length > 0)
                {
                    foreach (DataRow detailRow in detailRows)
                    {
                        DataRow exportRow = exportTable.NewRow();
                        exportRow["PReturnNo"] = masterRow["PReturnNo"];
                        exportRow["PReturnDate"] = masterRow["PReturnDate"];
                        exportRow["InvoiceNo"] = masterRow["InvoiceNo"];
                        exportRow["InvoiceDate"] = masterRow["InvoiceDate"];
                        exportRow["VendorName"] = masterRow["VendorName"];
                        exportRow["Paymode"] = masterRow["Paymode"];
                        exportRow["ItemName"] = detailRow["ItemName"];
                        exportRow["Unit"] = detailRow["Unit"];
                        exportRow["Quantity"] = detailRow["Qty"];
                        exportRow["Cost"] = detailRow["Cost"];
                        exportRow["TaxPer"] = detailRow["TaxPer"];
                        exportRow["TaxAmt"] = detailRow["TaxAmt"];
                        exportRow["Amount"] = detailRow["Amount"];
                        exportRow["Reason"] = detailRow["Reason"];
                        exportRow["SubTotal"] = masterRow["SubTotal"];
                        exportRow["GrandTotal"] = masterRow["GrandTotal"];
                        exportTable.Rows.Add(exportRow);
                    }
                }
                else
                {
                    // Add master row even if no details
                    DataRow exportRow = exportTable.NewRow();
                    exportRow["PReturnNo"] = masterRow["PReturnNo"];
                    exportRow["PReturnDate"] = masterRow["PReturnDate"];
                    exportRow["InvoiceNo"] = masterRow["InvoiceNo"];
                    exportRow["InvoiceDate"] = masterRow["InvoiceDate"];
                    exportRow["VendorName"] = masterRow["VendorName"];
                    exportRow["Paymode"] = masterRow["Paymode"];
                    exportRow["SubTotal"] = masterRow["SubTotal"];
                    exportRow["GrandTotal"] = masterRow["GrandTotal"];
                    exportTable.Rows.Add(exportRow);
                }
            }

            return exportTable;
        }

        /// <summary>
        /// Export DataTable to CSV file
        /// </summary>
        private void ExportToCSV(DataTable dataTable, string fileName)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName))
            {
                // Write headers
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    writer.Write(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1)
                        writer.Write(",");
                }
                writer.WriteLine();

                // Write data
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        string value = row[i].ToString();
                        // Escape commas and quotes
                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = "\"" + value.Replace("\"", "\"\"") + "\"";
                        }
                        writer.Write(value);
                        if (i < dataTable.Columns.Count - 1)
                            writer.Write(",");
                    }
                    writer.WriteLine();
                }
            }
        }
        #endregion

        #region Form Events
        private void PurchaseReturnReport_Load(object sender, EventArgs e)
        {
            // Load data after form is shown with wait cursor
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Application.DoEvents(); // Allow form to paint first
                LoadPurchaseReturnData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void PurchaseReturnReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Cleanup if needed
                if (dsHierarchical != null)
                {
                    dsHierarchical.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}
