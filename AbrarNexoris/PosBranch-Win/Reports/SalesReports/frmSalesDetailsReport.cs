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

namespace PosBranch_Win.Reports.SalesReports
{
    /// <summary>
    /// Professional Sales Report Form with Hierarchical Master-Detail UltraGrid
    /// </summary>
    public partial class frmSalesReportMasterDetail : Form
    {
        #region Private Fields
        private SalesReportRepository reportRepository;
        private DataSet dsHierarchical;
        private bool isLoading = false;
        #endregion

        #region Constructor
        public frmSalesReportMasterDetail()
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
                reportRepository = new SalesReportRepository();

                // Set form properties
                this.Text = "Sales Report - Master Detail View";
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
            DateTime today = DateTime.Now;
            ultraDateTimeEditorFrom.Value = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0).AddDays(-30);
            ultraDateTimeEditorFromTime.Value = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            ultraDateTimeEditorTo.Value = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
            ultraDateTimeEditorToTime.Value = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

            // Set date format
            ultraDateTimeEditorFrom.MaskInput = "{date}";
            ultraDateTimeEditorTo.MaskInput = "{date}";
            ultraDateTimeEditorFrom.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditorTo.FormatString = "dd/MM/yyyy";

            // Set time format
            ultraDateTimeEditorFromTime.MaskInput = "{time}";
            ultraDateTimeEditorToTime.MaskInput = "{time}";
            ultraDateTimeEditorFromTime.FormatString = "hh:mm tt";
            ultraDateTimeEditorToTime.FormatString = "hh:mm tt";

            // Configure time editors to behave like time selectors
            ultraDateTimeEditorFromTime.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            ultraDateTimeEditorFromTime.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Never;
            ultraDateTimeEditorToTime.SpinButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            ultraDateTimeEditorToTime.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Never;
        }

        private void InitializeSearchControls()
        {
            // Initialize preset date options
            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("Today", "Today");
            ultraComboPresetDates.Items.Add("Yesterday", "Yesterday");
            ultraComboPresetDates.Items.Add("ThisWeek", "This Week");
            ultraComboPresetDates.Items.Add("LastWeek", "Last Week");
            ultraComboPresetDates.Items.Add("ThisMonth", "This Month");
            ultraComboPresetDates.Items.Add("LastMonth", "Last Month");
            ultraComboPresetDates.Items.Add("ThisQuarter", "This Quarter");
            ultraComboPresetDates.Items.Add("LastQuarter", "Last Quarter");
            ultraComboPresetDates.Items.Add("ThisYear", "This Year");
            ultraComboPresetDates.Items.Add("LastYear", "Last Year");
            ultraComboPresetDates.Items.Add("Custom", "Custom Range");

            // Set default to "This Month"
            ultraComboPresetDates.Value = "ThisMonth";

            // Initialize numeric editors
            ultraNumericEditorAmountFrom.FormatString = "N2";
            ultraNumericEditorAmountTo.FormatString = "N2";
            ultraNumericEditorBillNo.FormatString = "0";

            // Set placeholder text
            ultraTextEditorCustomer.NullText = "Enter customer name...";

            // Initialize payment mode filter
            ultraComboPaymentMode.Items.Clear();
            ultraComboPaymentMode.Items.Add("All", "All Modes");
            ultraComboPaymentMode.Items.Add("Cash", "Cash");
            ultraComboPaymentMode.Items.Add("UPI", "UPI");
            ultraComboPaymentMode.Items.Add("Card", "Card");
            ultraComboPaymentMode.Items.Add("BankTransfer", "Bank Transfer");
            ultraComboPaymentMode.Value = "All";

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
                toolTip.SetToolTip(ultraDateTimeEditorFromTime, "Select start time for the report");
                toolTip.SetToolTip(ultraDateTimeEditorTo, "Select end date for the report");
                toolTip.SetToolTip(ultraDateTimeEditorToTime, "Select end time for the report");
                toolTip.SetToolTip(ultraComboPresetDates, "Quick date range selection");
                toolTip.SetToolTip(ultraNumericEditorBillNo, "Enter specific bill number to search");
                toolTip.SetToolTip(ultraTextEditorCustomer, "Enter customer name (partial match supported)");
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
                    btnClose_Click(sender, e);
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
            dsHierarchical = new DataSet("SalesReport");

            // Create Master Table (Sales Bills)
            DataTable masterTable = new DataTable("SalesMaster");
            masterTable.Columns.Add("BillNo", typeof(int));
            masterTable.Columns.Add("BillDate", typeof(DateTime));
            masterTable.Columns.Add("customername", typeof(string));
            masterTable.Columns.Add("paymodename", typeof(string));
            masterTable.Columns.Add("CashMode", typeof(string));
            // TaxPer removed - not available in master table
            masterTable.Columns.Add("TaxAmt", typeof(decimal));
            masterTable.Columns.Add("SubTotal", typeof(decimal));
            masterTable.Columns.Add("NetAmount", typeof(decimal));
            masterTable.Columns.Add("Profit", typeof(decimal));
            masterTable.PrimaryKey = new[] { masterTable.Columns["BillNo"] };

            // Create Detail Table (Bill Items)
            DataTable detailTable = new DataTable("SalesDetail");
            detailTable.Columns.Add("DetailID", typeof(int)); // Auto-increment unique ID
            detailTable.Columns.Add("BillNo", typeof(int));
            detailTable.Columns.Add("slno", typeof(int));
            detailTable.Columns.Add("ItemName", typeof(string));
            detailTable.Columns.Add("barcode", typeof(string));
            detailTable.Columns.Add("Unit", typeof(string));
            detailTable.Columns.Add("Packing", typeof(string));
            detailTable.Columns.Add("qty", typeof(decimal));
            detailTable.Columns.Add("UnitPrice", typeof(decimal));
            detailTable.Columns.Add("amount", typeof(decimal));
            detailTable.Columns.Add("MarginPer", typeof(decimal));
            detailTable.Columns.Add("Profit", typeof(decimal));  // Renamed from MarginAmt
            detailTable.Columns.Add("TaxPer", typeof(decimal));
            detailTable.Columns.Add("TaxAmt", typeof(decimal));
            detailTable.Columns.Add("TotalAmount", typeof(decimal));

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
                masterTable.Columns["BillNo"],
                detailTable.Columns["BillNo"],
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
            StyleSummaryLabel(ultraLabelTotalBillsCaption, Color.FromArgb(25, 118, 210), true);
            StyleSummaryLabel(ultraLabelSubTotalCaption, Color.FromArgb(56, 142, 60), true);
            StyleSummaryLabel(ultraLabelTaxTotalCaption, Color.FromArgb(211, 84, 0), true);
            StyleSummaryLabel(ultraLabelNetTotalCaption, Color.FromArgb(123, 31, 162), true);
            StyleSummaryLabel(ultraLabelTotalProfitCaption, Color.FromArgb(22, 160, 133), true);

            // Style summary value labels - Large, bold, colorful
            StyleSummaryValueLabel(ultraLabelTotalBillsValue, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(ultraLabelSubTotalValue, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(ultraLabelTaxTotalValue, Color.FromArgb(191, 54, 12), 14);
            StyleSummaryValueLabel(ultraLabelNetTotalValue, Color.FromArgb(74, 20, 140), 16);
            StyleSummaryValueLabel(ultraLabelTotalProfitValue, Color.FromArgb(22, 160, 133), 16);

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
                    e.Layout.Bands[1].Header.Caption = "Bill Details";
                    e.Layout.Bands[1].HeaderVisible = true;
                }

                // Enable AutoFit for all columns
                e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

                // Update status
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring grid layout: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configure columns for Master Band (Sales Bills)
        /// </summary>
        private void ConfigureMasterBandColumns(UltraGridBand masterBand)
        {
            // BillNo column - keep visible in master for reference
            if (masterBand.Columns["BillNo"] != null)
            {
                masterBand.Columns["BillNo"].Header.Caption = "Bill No";
                masterBand.Columns["BillNo"].Width = 80;
                masterBand.Columns["BillNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["BillNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
            }

            // Format date column
            if (masterBand.Columns["BillDate"] != null)
            {
                masterBand.Columns["BillDate"].Format = "dd/MM/yyyy hh:mm tt";
                masterBand.Columns["BillDate"].Header.Caption = "Date & Time";
                masterBand.Columns["BillDate"].Width = 140;
            }

            // Format currency columns with modern styling
            if (masterBand.Columns["SubTotal"] != null)
            {
                masterBand.Columns["SubTotal"].Format = "₹ #,##0.00";
                masterBand.Columns["SubTotal"].Header.Caption = "Sub Total";
                masterBand.Columns["SubTotal"].Width = 110;
                masterBand.Columns["SubTotal"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (masterBand.Columns["TaxAmt"] != null)
            {
                masterBand.Columns["TaxAmt"].Format = "₹ #,##0.00";
                masterBand.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                masterBand.Columns["TaxAmt"].Width = 110;
                masterBand.Columns["TaxAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                masterBand.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
            }

            if (masterBand.Columns["NetAmount"] != null)
            {
                masterBand.Columns["NetAmount"].Format = "₹ #,##0.00";
                masterBand.Columns["NetAmount"].Header.Caption = "Net Amount";
                masterBand.Columns["NetAmount"].Width = 120;
                masterBand.Columns["NetAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                masterBand.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            if (masterBand.Columns["Profit"] != null)
            {
                masterBand.Columns["Profit"].Format = "₹ #,##0.00";
                masterBand.Columns["Profit"].Header.Caption = "Profit";
                masterBand.Columns["Profit"].Width = 110;
                masterBand.Columns["Profit"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                masterBand.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["Profit"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
            }

            // TaxPer removed from master table - no longer available

            // Set column captions and widths
            if (masterBand.Columns["customername"] != null)
            {
                masterBand.Columns["customername"].Header.Caption = "Customer";
                masterBand.Columns["customername"].Width = 200;
            }
            if (masterBand.Columns["paymodename"] != null)
            {
                masterBand.Columns["paymodename"].Header.Caption = "Bill Type";
                masterBand.Columns["paymodename"].Width = 100;
                masterBand.Columns["paymodename"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }
            if (masterBand.Columns["CashMode"] != null)
            {
                masterBand.Columns["CashMode"].Header.Caption = "Payment Mode";
                masterBand.Columns["CashMode"].Width = 150;
                masterBand.Columns["CashMode"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }
        }

        /// <summary>
        /// Configure columns for Detail Band (Bill Items)
        /// </summary>
        private void ConfigureDetailBandColumns(UltraGridBand detailBand)
        {
            // Hide DetailID (auto-increment) and BillNo (foreign key)
            if (detailBand.Columns["DetailID"] != null)
                detailBand.Columns["DetailID"].Hidden = true;
            if (detailBand.Columns["BillNo"] != null)
                detailBand.Columns["BillNo"].Hidden = true;

            // Set column captions and widths with modern formatting
            if (detailBand.Columns["slno"] != null)
            {
                detailBand.Columns["slno"].Header.Caption = "S.No";
                detailBand.Columns["slno"].Width = 50;
                detailBand.Columns["slno"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["ItemName"] != null)
            {
                detailBand.Columns["ItemName"].Header.Caption = "Item Name";
                detailBand.Columns["ItemName"].Width = 220;
                detailBand.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (detailBand.Columns["barcode"] != null)
            {
                detailBand.Columns["barcode"].Header.Caption = "Barcode";
                detailBand.Columns["barcode"].Width = 120;
                detailBand.Columns["barcode"].CellAppearance.ForeColor = Color.FromArgb(84, 110, 122);
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

            if (detailBand.Columns["qty"] != null)
            {
                detailBand.Columns["qty"].Header.Caption = "Quantity";
                detailBand.Columns["qty"].Format = "0.00";
                detailBand.Columns["qty"].Width = 80;
                detailBand.Columns["qty"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (detailBand.Columns["UnitPrice"] != null)
            {
                detailBand.Columns["UnitPrice"].Header.Caption = "Unit Price";
                detailBand.Columns["UnitPrice"].Format = "₹ #,##0.00";
                detailBand.Columns["UnitPrice"].Width = 90;
                detailBand.Columns["UnitPrice"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
            }

            if (detailBand.Columns["amount"] != null)
            {
                detailBand.Columns["amount"].Header.Caption = "Amount";
                detailBand.Columns["amount"].Format = "₹ #,##0.00";
                detailBand.Columns["amount"].Width = 100;
                detailBand.Columns["amount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["amount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
            }

            if (detailBand.Columns["MarginPer"] != null)
            {
                detailBand.Columns["MarginPer"].Header.Caption = "Margin %";
                detailBand.Columns["MarginPer"].Format = "0.00 %";
                detailBand.Columns["MarginPer"].Width = 80;
                detailBand.Columns["MarginPer"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (detailBand.Columns["Profit"] != null)
            {
                detailBand.Columns["Profit"].Header.Caption = "Profit";
                detailBand.Columns["Profit"].Format = "₹ #,##0.00";
                detailBand.Columns["Profit"].Width = 110;
                detailBand.Columns["Profit"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["Profit"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
                detailBand.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
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
                detailBand.Columns["TaxAmt"].Format = "₹ #,##0.00";
                detailBand.Columns["TaxAmt"].Width = 100;
                detailBand.Columns["TaxAmt"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
            }

            if (detailBand.Columns["TotalAmount"] != null)
            {
                detailBand.Columns["TotalAmount"].Header.Caption = "Total Amount";
                detailBand.Columns["TotalAmount"].Format = "₹ #,##0.00";
                detailBand.Columns["TotalAmount"].Width = 120;
                detailBand.Columns["TotalAmount"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                detailBand.Columns["TotalAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["TotalAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            // *** NEW: Enable summaries for detail band ***
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
                if (detailBand.Columns["qty"] != null)
                {
                    SummarySettings sumQty = detailBand.Summaries.Add("SumQty", SummaryType.Sum, detailBand.Columns["qty"], SummaryPosition.UseSummaryPositionColumn);
                    sumQty.DisplayFormat = "Qty: {0:N2}";
                    sumQty.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumQty.Appearance.ForeColor = Color.FromArgb(44, 62, 80);
                    sumQty.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (detailBand.Columns["amount"] != null)
                {
                    SummarySettings sumAmount = detailBand.Summaries.Add("SumAmount", SummaryType.Sum, detailBand.Columns["amount"], SummaryPosition.UseSummaryPositionColumn);
                    sumAmount.DisplayFormat = "₹ {0:N2}";
                    sumAmount.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumAmount.Appearance.ForeColor = Color.FromArgb(44, 62, 80);
                    sumAmount.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (detailBand.Columns["Profit"] != null)
                {
                    SummarySettings sumProfit = detailBand.Summaries.Add("SumProfit", SummaryType.Sum, detailBand.Columns["Profit"], SummaryPosition.UseSummaryPositionColumn);
                    sumProfit.DisplayFormat = "Total: ₹ {0:N2}";
                    sumProfit.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumProfit.Appearance.ForeColor = Color.FromArgb(22, 160, 133);
                    sumProfit.Appearance.FontData.Bold = DefaultableBoolean.True;
                    sumProfit.Appearance.FontData.SizeInPoints = 10;
                }

                if (detailBand.Columns["TaxAmt"] != null)
                {
                    SummarySettings sumTax = detailBand.Summaries.Add("SumTax", SummaryType.Sum, detailBand.Columns["TaxAmt"], SummaryPosition.UseSummaryPositionColumn);
                    sumTax.DisplayFormat = "₹ {0:N2}";
                    sumTax.Appearance.BackColor = Color.FromArgb(236, 240, 241);
                    sumTax.Appearance.ForeColor = Color.FromArgb(211, 84, 0);
                    sumTax.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (detailBand.Columns["TotalAmount"] != null)
                {
                    SummarySettings sumTotal = detailBand.Summaries.Add("SumTotal", SummaryType.Sum, detailBand.Columns["TotalAmount"], SummaryPosition.UseSummaryPositionColumn);
                    sumTotal.DisplayFormat = "₹ {0:N2}";
                    sumTotal.Appearance.BackColor = Color.FromArgb(52, 73, 94);
                    sumTotal.Appearance.ForeColor = Color.White;
                    sumTotal.Appearance.FontData.Bold = DefaultableBoolean.True;
                    sumTotal.Appearance.FontData.SizeInPoints = 10;
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
        /// Load all sales data without filters - REVISED
        /// </summary>
        private void LoadSalesData()
        {
            try
            {
                isLoading = true;

                DateTime fromDateValue = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime fromTimeValue = Convert.ToDateTime(ultraDateTimeEditorFromTime.Value);
                DateTime fromDate = new DateTime(fromDateValue.Year, fromDateValue.Month, fromDateValue.Day, fromTimeValue.Hour, fromTimeValue.Minute, fromTimeValue.Second);

                DateTime toDateValue = Convert.ToDateTime(ultraDateTimeEditorTo.Value);
                DateTime toTimeValue = Convert.ToDateTime(ultraDateTimeEditorToTime.Value);
                DateTime toDate = new DateTime(toDateValue.Year, toDateValue.Month, toDateValue.Day, toTimeValue.Hour, toTimeValue.Minute, toTimeValue.Second);

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
                dsHierarchical.Tables["SalesDetail"].Clear();
                dsHierarchical.Tables["SalesMaster"].Clear();

                // STEP 4: Load master data
                LoadMasterData(fromDate, toDate);

                // STEP 5: Load detail data
                LoadAllDetailData(fromDate, toDate);

                // STEP 6: Recreate relation
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];
                DataTable detailTable = dsHierarchical.Tables["SalesDetail"];

                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["BillNo"],
                    detailTable.Columns["BillNo"],
                    true
                );
                dsHierarchical.Relations.Add(relation);

                // STEP 7: Accept changes
                dsHierarchical.AcceptChanges();

                // STEP 8: Rebind grid
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "SalesMaster";

                // STEP 9: Force refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // STEP 10: Update totals
                UpdateGrandTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Load master data (Sales Bills)
        /// </summary>
        private void LoadMasterData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<SalesReportMaster> masterData = reportRepository.GetSalesBills(fromDate, toDate, SessionContext.BranchId);
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];

                foreach (var bill in masterData)
                {
                    if (bill.BillNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["BillNo"] = bill.BillNo;
                        row["BillDate"] = bill.BillDate;
                        row["customername"] = bill.CustomerName ?? "";
                        row["paymodename"] = bill.PaymodeName ?? "";
                        row["CashMode"] = bill.CashMode ?? "";
                        // TaxPer removed - not available in master table
                        row["TaxAmt"] = bill.TaxAmt;
                        row["SubTotal"] = bill.SubTotal;
                        row["NetAmount"] = bill.NetAmount;
                        row["Profit"] = bill.Profit;
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
                DataTable detailTable = dsHierarchical.Tables["SalesDetail"];

                // Get all BillNo values from master
                var billNumbers = dsHierarchical.Tables["SalesMaster"].AsEnumerable()
                    .Select(r => r.Field<int>("BillNo"))
                    .ToList();

                // Load details for each bill
                foreach (int billNo in billNumbers)
                {
                    SalesReportData reportData = reportRepository.GetSalesReportDetails(billNo, fromDate, toDate);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            // DetailID will auto-increment
                            row["BillNo"] = billNo;
                            row["slno"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["barcode"] = detail.Barcode ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["qty"] = detail.Qty;
                            row["UnitPrice"] = detail.UnitPrice;
                            row["amount"] = detail.Amount;
                            row["MarginPer"] = detail.MarginPer;
                            row["Profit"] = detail.Profit;  // Renamed from MarginAmt
                            row["TaxPer"] = detail.TaxPer;
                            row["TaxAmt"] = detail.TaxAmt;
                            row["TotalAmount"] = detail.TotalAmount;
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
        /// Update summary labels with bill information - Modern styled
        /// </summary>


        /// <summary>
        /// Update grand totals for all loaded data and display in summary panel
        /// </summary>
        private void UpdateGrandTotals()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];
                decimal grandTotal = 0;
                decimal grandTax = 0;
                decimal grandSubTotal = 0;
                decimal grandProfit = 0;
                int totalBills = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    grandSubTotal += Convert.ToDecimal(row["SubTotal"]);
                    grandTax += Convert.ToDecimal(row["TaxAmt"]);
                    grandTotal += Convert.ToDecimal(row["NetAmount"]);
                    grandProfit += Convert.ToDecimal(row["Profit"]);
                }

                // Update summary labels with calculated totals
                ultraLabelTotalBillsValue.Text = totalBills.ToString("N0");
                ultraLabelSubTotalValue.Text = $"₹ {grandSubTotal:N2}";
                ultraLabelTaxTotalValue.Text = $"₹ {grandTax:N2}";
                ultraLabelNetTotalValue.Text = $"₹ {grandTotal:N2}";
                ultraLabelTotalProfitValue.Text = $"₹ {grandProfit:N2}";

                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Grand Totals Updated - Bills: {totalBills}, SubTotal: {grandSubTotal:N2}, Tax: {grandTax:N2}, Net: {grandTotal:N2}, Profit: {grandProfit:N2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating grand totals: {ex.Message}");
                // Reset to zeros on error
                ultraLabelTotalBillsValue.Text = "0";
                ultraLabelSubTotalValue.Text = "₹ 0.00";
                ultraLabelTaxTotalValue.Text = "₹ 0.00";
                ultraLabelNetTotalValue.Text = "₹ 0.00";
                ultraLabelTotalProfitValue.Text = "₹ 0.00";
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
        /// Before row expanded event - load detail data if needed
        /// </summary>
        private void UltraGridMaster_BeforeRowExpanded(object sender, CancelableRowEventArgs e)
        {
            try
            {
                // Details are already loaded, so just update summary
                if (e.Row.Band.Index == 0 && e.Row.Cells["BillNo"] != null)
                {
                    var dataRowView = e.Row.ListObject as DataRowView;
                    if (dataRowView != null)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BeforeRowExpanded: {ex.Message}");
            }
        }

        /// <summary>
        /// After row expanded event - update status and show detail totals
        /// </summary>
        private void UltraGridMaster_AfterRowExpanded(object sender, RowEventArgs e)
        {
            try
            {
                if (e.Row.Band.Index == 0 && e.Row.Cells["BillNo"] != null)
                {
                    int billNo = Convert.ToInt32(e.Row.Cells["BillNo"].Value);
                    int detailCount = e.Row.ChildBands[0].Rows.Count;

                    // Calculate detail totals for status display
                    decimal totalQty = 0;
                    decimal totalAmount = 0;
                    decimal totalTax = 0;
                    decimal grandTotal = 0;

                    foreach (UltraGridRow detailRow in e.Row.ChildBands[0].Rows)
                    {
                        if (detailRow.Cells["qty"] != null && detailRow.Cells["qty"].Value != null)
                            totalQty += Convert.ToDecimal(detailRow.Cells["qty"].Value);

                        if (detailRow.Cells["amount"] != null && detailRow.Cells["amount"].Value != null)
                            totalAmount += Convert.ToDecimal(detailRow.Cells["amount"].Value);

                        if (detailRow.Cells["TaxAmt"] != null && detailRow.Cells["TaxAmt"].Value != null)
                            totalTax += Convert.ToDecimal(detailRow.Cells["TaxAmt"].Value);

                        if (detailRow.Cells["TotalAmount"] != null && detailRow.Cells["TotalAmount"].Value != null)
                            grandTotal += Convert.ToDecimal(detailRow.Cells["TotalAmount"].Value);
                    }


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
                LoadSalesData();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (dsHierarchical.Tables["SalesMaster"].Rows.Count > 0)
                {
                    PrintSalesReport();
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
        /// Print the sales report
        /// </summary>
        private void PrintSalesReport()
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = "Sales Report";
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
                e.Graphics.DrawString("SALES REPORT - MASTER DETAIL VIEW", titleFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print date range
                DateTime fromDateValue = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime fromTimeValue = Convert.ToDateTime(ultraDateTimeEditorFromTime.Value);
                DateTime fromDate = new DateTime(fromDateValue.Year, fromDateValue.Month, fromDateValue.Day, fromTimeValue.Hour, fromTimeValue.Minute, fromTimeValue.Second);

                DateTime toDateValue = Convert.ToDateTime(ultraDateTimeEditorTo.Value);
                DateTime toTimeValue = Convert.ToDateTime(ultraDateTimeEditorToTime.Value);
                DateTime toDate = new DateTime(toDateValue.Year, toDateValue.Month, toDateValue.Day, toTimeValue.Hour, toTimeValue.Minute, toTimeValue.Second);

                string dateRange = $"From: {fromDate:dd/MM/yyyy hh:mm tt} To: {toDate:dd/MM/yyyy hh:mm tt}";
                e.Graphics.DrawString(dateRange, dataFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;

                // Print summary
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];
                decimal totalAmount = 0;
                decimal totalTax = 0;
                int totalBills = masterTable.Rows.Count;

                foreach (DataRow row in masterTable.Rows)
                {
                    totalAmount += Convert.ToDecimal(row["NetAmount"]);
                    totalTax += Convert.ToDecimal(row["TaxAmt"]);
                }

                e.Graphics.DrawString($"Total Bills: {totalBills}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Total Amount: {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 20;
                e.Graphics.DrawString($"Total Tax: {totalTax:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
                yPosition += 30;

                // Print master data headers
                string[] headers = { "Bill No", "Date & Time", "Customer", "Payment", "Sub Total", "Tax", "Net Amount" };
                float[] columnWidths = { 70, 130, 140, 90, 90, 70, 100 };
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
                        row["BillNo"].ToString(),
                        Convert.ToDateTime(row["BillDate"]).ToString("dd/MM/yyyy hh:mm tt"),
                        row["customername"].ToString(),
                        row["paymodename"].ToString(),
                        Convert.ToDecimal(row["SubTotal"]).ToString("N2"),
                        Convert.ToDecimal(row["TaxAmt"]).ToString("N2"),
                        Convert.ToDecimal(row["NetAmount"]).ToString("N2")
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
                e.Graphics.DrawString($"GRAND TOTAL: {totalAmount:N2}", summaryFont, Brushes.Black, leftMargin, yPosition);
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
                if (dsHierarchical.Tables["SalesMaster"].Rows.Count > 0)
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
                LoadSalesDataWithFilters();
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
                DateTime fromDateValue = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime fromTimeValue = Convert.ToDateTime(ultraDateTimeEditorFromTime.Value);
                DateTime fromDate = new DateTime(fromDateValue.Year, fromDateValue.Month, fromDateValue.Day, fromTimeValue.Hour, fromTimeValue.Minute, fromTimeValue.Second);

                DateTime toDateValue = Convert.ToDateTime(ultraDateTimeEditorTo.Value);
                DateTime toTimeValue = Convert.ToDateTime(ultraDateTimeEditorToTime.Value);
                DateTime toDate = new DateTime(toDateValue.Year, toDateValue.Month, toDateValue.Day, toTimeValue.Hour, toTimeValue.Minute, toTimeValue.Second);

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
        /// Clear filters and reload - REVISED
        /// </summary>
        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                // Clear all search filters
                DateTime today = DateTime.Now;
                ultraDateTimeEditorFrom.Value = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0).AddDays(-30);
                ultraDateTimeEditorFromTime.Value = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
                ultraDateTimeEditorTo.Value = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
                ultraDateTimeEditorToTime.Value = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);
                ultraNumericEditorAmountFrom.Value = null;
                ultraNumericEditorAmountTo.Value = null;
                ultraNumericEditorBillNo.Value = null;

                // Clear customer text field - set both Value and Text to ensure it's cleared
                ultraTextEditorCustomer.Value = null;
                ultraTextEditorCustomer.Text = string.Empty;

                ultraComboPresetDates.Value = "ThisMonth";
                ultraComboPaymentMode.Value = "All";

                // Force UI update
                Application.DoEvents();

                // Reload all data
                LoadSalesData();
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
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "Yesterday":
                        fromDate = DateTime.Now.AddDays(-1).Date;
                        toDate = DateTime.Now.AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek).Date;
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastWeek":
                        fromDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7).Date;
                        toDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastMonth":
                        fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
                        toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisQuarter":
                        int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                        fromDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
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
                        toDate = fromDate.AddMonths(3).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "ThisYear":
                        fromDate = new DateTime(DateTime.Now.Year, 1, 1);
                        toDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case "LastYear":
                        fromDate = new DateTime(DateTime.Now.Year - 1, 1, 1);
                        toDate = new DateTime(DateTime.Now.Year - 1, 12, 31).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    default:
                        return; // Custom range - don't change dates
                }

                ultraDateTimeEditorFrom.Value = fromDate;
                ultraDateTimeEditorFromTime.Value = fromDate;
                ultraDateTimeEditorTo.Value = toDate;
                ultraDateTimeEditorToTime.Value = toDate;

                // Auto-load data when a preset is selected (skip during form init)
                if (!isLoading && dsHierarchical != null && dsHierarchical.Tables.Count > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        LoadSalesData();
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting preset dates: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load sales data with applied filters - COMPLETELY REVISED
        /// </summary>
        private void LoadSalesDataWithFilters()
        {
            try
            {
                isLoading = true;
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDateValue = Convert.ToDateTime(ultraDateTimeEditorFrom.Value);
                DateTime fromTimeValue = Convert.ToDateTime(ultraDateTimeEditorFromTime.Value);
                DateTime fromDate = new DateTime(fromDateValue.Year, fromDateValue.Month, fromDateValue.Day, fromTimeValue.Hour, fromTimeValue.Minute, fromTimeValue.Second);

                DateTime toDateValue = Convert.ToDateTime(ultraDateTimeEditorTo.Value);
                DateTime toTimeValue = Convert.ToDateTime(ultraDateTimeEditorToTime.Value);
                DateTime toDate = new DateTime(toDateValue.Year, toDateValue.Month, toDateValue.Day, toTimeValue.Hour, toTimeValue.Minute, toTimeValue.Second);

                // CRITICAL: Completely unbind and reset grid
                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataMember = null;
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Clear the DataSet completely and rebuild from scratch
                RebuildDataSetWithFilters(fromDate, toDate);

                // Rebind the grid with fresh data
                ultraGridMaster.DataSource = dsHierarchical;
                ultraGridMaster.DataMember = "SalesMaster";

                // Force complete grid refresh
                ultraGridMaster.DisplayLayout.PerformAutoResizeColumns(false, PerformAutoSizeType.AllRowsInBand);
                ultraGridMaster.Refresh();
                Application.DoEvents();

                // Update totals
                UpdateGrandTotals();

                // Show result count
                int resultCount = dsHierarchical.Tables["SalesMaster"].Rows.Count;
                if (resultCount == 0)
                {
                    MessageBox.Show("No records found matching the search criteria.", "Search Results",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching sales data: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Load master data with basic date filters
        /// </summary>
        private void LoadMasterDataWithFilters(DateTime fromDate, DateTime toDate)
        {
            try
            {
                List<SalesReportMaster> masterData = reportRepository.GetSalesBills(fromDate, toDate, SessionContext.BranchId);
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];

                foreach (var bill in masterData)
                {
                    if (bill.BillNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["BillNo"] = bill.BillNo;
                        row["BillDate"] = bill.BillDate;
                        row["customername"] = bill.CustomerName ?? "";
                        row["paymodename"] = bill.PaymodeName ?? "";
                        // TaxPer removed - not available in master table
                        row["TaxAmt"] = bill.TaxAmt;
                        row["SubTotal"] = bill.SubTotal;
                        row["NetAmount"] = bill.NetAmount;
                        row["Profit"] = bill.Profit;
                        masterTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading filtered master data: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Apply additional filters to loaded data
        /// </summary>
        private void ApplyDataFilters()
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];
                DataTable detailTable = dsHierarchical.Tables["SalesDetail"];

                // Create a copy to iterate and modify
                DataRow[] rowsToRemove = new DataRow[masterTable.Rows.Count];
                int removeIndex = 0;

                foreach (DataRow row in masterTable.Rows)
                {
                    bool shouldRemove = false;

                    // Filter by Bill Number
                    if (ultraNumericEditorBillNo.Value != null)
                    {
                        int billNo = Convert.ToInt32(ultraNumericEditorBillNo.Value);
                        if (Convert.ToInt32(row["BillNo"]) != billNo)
                        {
                            shouldRemove = true;
                        }
                    }

                    // Filter by Customer Name
                    if (!shouldRemove && !string.IsNullOrEmpty(ultraTextEditorCustomer.Text))
                    {
                        string customerName = row["customername"].ToString().ToLower();
                        string searchCustomer = ultraTextEditorCustomer.Text.ToLower();
                        if (!customerName.Contains(searchCustomer))
                        {
                            shouldRemove = true;
                        }
                    }

                    // Filter by Amount Range
                    if (!shouldRemove && (ultraNumericEditorAmountFrom.Value != null || ultraNumericEditorAmountTo.Value != null))
                    {
                        decimal netAmount = Convert.ToDecimal(row["NetAmount"]);

                        if (ultraNumericEditorAmountFrom.Value != null)
                        {
                            decimal fromAmount = Convert.ToDecimal(ultraNumericEditorAmountFrom.Value);
                            if (netAmount < fromAmount)
                            {
                                shouldRemove = true;
                            }
                        }

                        if (!shouldRemove && ultraNumericEditorAmountTo.Value != null)
                        {
                            decimal toAmount = Convert.ToDecimal(ultraNumericEditorAmountTo.Value);
                            if (netAmount > toAmount)
                            {
                                shouldRemove = true;
                            }
                        }
                    }

                    if (shouldRemove)
                    {
                        rowsToRemove[removeIndex] = row;
                        removeIndex++;
                    }
                }

                // Remove filtered rows from master table
                for (int i = 0; i < removeIndex; i++)
                {
                    int billNo = Convert.ToInt32(rowsToRemove[i]["BillNo"]);

                    // Remove from master
                    rowsToRemove[i].Delete();

                    // Remove corresponding details
                    DataRow[] detailRows = detailTable.Select($"BillNo = {billNo}");
                    foreach (DataRow detailRow in detailRows)
                    {
                        detailRow.Delete();
                    }
                }

                // Accept changes
                masterTable.AcceptChanges();
                detailTable.AcceptChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error applying data filters: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Rebuild the entire DataSet with filters applied - NEW METHOD
        /// This avoids the issue of deleting rows with active relationships
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
                dsHierarchical.Tables["SalesDetail"].Clear();
                dsHierarchical.Tables["SalesMaster"].Clear();

                // Step 3: Load master data from database
                List<SalesReportMaster> masterData = reportRepository.GetSalesBills(fromDate, toDate, SessionContext.BranchId);

                // Step 4: Apply filters to master data BEFORE adding to DataTable
                var filteredMasterData = ApplyMasterFilters(masterData);

                // Step 5: Add filtered master data to DataTable
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];
                foreach (var bill in filteredMasterData)
                {
                    if (bill.BillNo > 0)
                    {
                        DataRow row = masterTable.NewRow();
                        row["BillNo"] = bill.BillNo;
                        row["BillDate"] = bill.BillDate;
                        row["customername"] = bill.CustomerName ?? "";
                        row["paymodename"] = bill.PaymodeName ?? "";
                        row["CashMode"] = bill.CashMode ?? "";
                        row["TaxAmt"] = bill.TaxAmt;
                        row["SubTotal"] = bill.SubTotal;
                        row["NetAmount"] = bill.NetAmount;
                        row["Profit"] = bill.Profit;
                        masterTable.Rows.Add(row);
                    }
                }

                // Step 6: Load details only for filtered bills
                if (masterTable.Rows.Count > 0)
                {
                    LoadDetailsForFilteredBills(fromDate, toDate);
                }

                // Step 7: Recreate the relation
                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["BillNo"],
                    dsHierarchical.Tables["SalesDetail"].Columns["BillNo"],
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
        /// Apply filters to master data list BEFORE adding to DataTable - NEW METHOD
        /// </summary>
        private List<SalesReportMaster> ApplyMasterFilters(List<SalesReportMaster> masterData)
        {
            var filtered = masterData.AsEnumerable();

            // Filter by Bill Number
            if (ultraNumericEditorBillNo.Value != null && Convert.ToInt32(ultraNumericEditorBillNo.Value) > 0)
            {
                int billNo = Convert.ToInt32(ultraNumericEditorBillNo.Value);
                filtered = filtered.Where(x => x.BillNo == billNo);
            }

            // Filter by Customer Name (case-insensitive partial match)
            // Check both Value and Text to handle Infragistics control behavior
            string customerText = ultraTextEditorCustomer.Value?.ToString() ?? ultraTextEditorCustomer.Text;
            if (!string.IsNullOrWhiteSpace(customerText))
            {
                string customerSearch = customerText.Trim().ToLower();
                filtered = filtered.Where(x =>
                    !string.IsNullOrEmpty(x.CustomerName) &&
                    x.CustomerName.ToLower().Contains(customerSearch));
            }

            // Filter by Amount Range
            if (ultraNumericEditorAmountFrom.Value != null && Convert.ToDecimal(ultraNumericEditorAmountFrom.Value) > 0)
            {
                decimal fromAmount = Convert.ToDecimal(ultraNumericEditorAmountFrom.Value);
                filtered = filtered.Where(x => x.NetAmount >= (double)fromAmount);
            }

            if (ultraNumericEditorAmountTo.Value != null && Convert.ToDecimal(ultraNumericEditorAmountTo.Value) > 0)
            {
                decimal toAmount = Convert.ToDecimal(ultraNumericEditorAmountTo.Value);
                filtered = filtered.Where(x => x.NetAmount <= (double)toAmount);
            }

            // Filter by Payment Mode (CashMode column)
            if (ultraComboPaymentMode.Value != null && ultraComboPaymentMode.Value.ToString() != "All")
            {
                string selectedMode = ultraComboPaymentMode.Value.ToString();
                filtered = filtered.Where(x =>
                    !string.IsNullOrEmpty(x.CashMode) &&
                    x.CashMode.IndexOf(selectedMode, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return filtered.ToList();
        }

        /// <summary>
        /// Load details only for bills that passed the filter - NEW METHOD
        /// </summary>
        private void LoadDetailsForFilteredBills(DateTime fromDate, DateTime toDate)
        {
            try
            {
                DataTable masterTable = dsHierarchical.Tables["SalesMaster"];
                DataTable detailTable = dsHierarchical.Tables["SalesDetail"];

                // Get all BillNo values from filtered master
                var billNumbers = masterTable.AsEnumerable()
                    .Select(r => r.Field<int>("BillNo"))
                    .ToList();

                // Load details for each filtered bill
                foreach (int billNo in billNumbers)
                {
                    SalesReportData reportData = reportRepository.GetSalesReportDetails(billNo, fromDate, toDate);

                    if (reportData?.Details != null && reportData.Details.Count > 0)
                    {
                        foreach (var detail in reportData.Details)
                        {
                            DataRow row = detailTable.NewRow();
                            row["BillNo"] = billNo;
                            row["slno"] = detail.SlNo;
                            row["ItemName"] = detail.ItemName ?? "";
                            row["barcode"] = detail.Barcode ?? "";
                            row["Unit"] = detail.Unit ?? "";
                            row["Packing"] = detail.Packing ?? "";
                            row["qty"] = detail.Qty;
                            row["UnitPrice"] = detail.UnitPrice;
                            row["amount"] = detail.Amount;
                            row["MarginPer"] = detail.MarginPer;
                            row["Profit"] = detail.Profit;  // Renamed from MarginAmt
                            row["TaxPer"] = detail.TaxPer;
                            row["TaxAmt"] = detail.TaxAmt;
                            row["TotalAmount"] = detail.TotalAmount;
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
                saveDialog.FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create a new DataTable for export with flattened data
                    DataTable exportTable = CreateExportTable();

                    // Export to Excel using simple CSV approach (can be enhanced with Excel library)
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
            DataTable exportTable = new DataTable("SalesReport");

            // Add columns
            exportTable.Columns.Add("BillNo", typeof(int));
            exportTable.Columns.Add("BillDate", typeof(DateTime));
            exportTable.Columns.Add("CustomerName", typeof(string));
            exportTable.Columns.Add("PaymentMode", typeof(string));
            exportTable.Columns.Add("ItemName", typeof(string));
            exportTable.Columns.Add("Barcode", typeof(string));
            exportTable.Columns.Add("Unit", typeof(string));
            exportTable.Columns.Add("Quantity", typeof(decimal));
            exportTable.Columns.Add("UnitPrice", typeof(decimal));
            exportTable.Columns.Add("Amount", typeof(decimal));
            exportTable.Columns.Add("Profit", typeof(decimal));
            exportTable.Columns.Add("TaxPer", typeof(decimal));
            exportTable.Columns.Add("TaxAmt", typeof(decimal));
            exportTable.Columns.Add("TotalAmount", typeof(decimal));
            exportTable.Columns.Add("SubTotal", typeof(decimal));
            exportTable.Columns.Add("NetAmount", typeof(decimal));
            exportTable.Columns.Add("BillProfit", typeof(decimal));

            // Flatten the hierarchical data
            foreach (DataRow masterRow in dsHierarchical.Tables["SalesMaster"].Rows)
            {
                DataRow[] detailRows = dsHierarchical.Tables["SalesDetail"].Select($"BillNo = {masterRow["BillNo"]}");

                if (detailRows.Length > 0)
                {
                    foreach (DataRow detailRow in detailRows)
                    {
                        DataRow exportRow = exportTable.NewRow();
                        exportRow["BillNo"] = masterRow["BillNo"];
                        exportRow["BillDate"] = masterRow["BillDate"];
                        exportRow["CustomerName"] = masterRow["customername"];
                        exportRow["PaymentMode"] = masterRow["paymodename"];
                        exportRow["ItemName"] = detailRow["ItemName"];
                        exportRow["Barcode"] = detailRow["barcode"];
                        exportRow["Unit"] = detailRow["Unit"];
                        exportRow["Quantity"] = detailRow["qty"];
                        exportRow["UnitPrice"] = detailRow["UnitPrice"];
                        exportRow["Amount"] = detailRow["amount"];
                        exportRow["Profit"] = detailRow["Profit"];
                        exportRow["TaxPer"] = detailRow["TaxPer"];
                        exportRow["TaxAmt"] = detailRow["TaxAmt"];
                        exportRow["TotalAmount"] = detailRow["TotalAmount"];
                        exportRow["SubTotal"] = masterRow["SubTotal"];
                        exportRow["NetAmount"] = masterRow["NetAmount"];
                        exportRow["BillProfit"] = masterRow["Profit"];
                        exportTable.Rows.Add(exportRow);
                    }
                }
                else
                {
                    // Add master row even if no details
                    DataRow exportRow = exportTable.NewRow();
                    exportRow["BillNo"] = masterRow["BillNo"];
                    exportRow["BillDate"] = masterRow["BillDate"];
                    exportRow["CustomerName"] = masterRow["customername"];
                    exportRow["PaymentMode"] = masterRow["paymodename"];
                    exportRow["SubTotal"] = masterRow["SubTotal"];
                    exportRow["NetAmount"] = masterRow["NetAmount"];
                    exportRow["BillProfit"] = masterRow["Profit"];
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
        private void frmSalesReportMasterDetail_Load(object sender, EventArgs e)
        {
            // Load data after form is shown with wait cursor
            this.Cursor = Cursors.WaitCursor;

            try
            {
                Application.DoEvents(); // Allow form to paint first
                LoadSalesData();
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

        private void frmSalesReportMasterDetail_FormClosing(object sender, FormClosingEventArgs e)
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