using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports;
using PosBranch_Win.Reports.FinancialReports;
using PosBranch_Win.Transaction;
using Repository;
using Repository.MasterRepositry;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    /// <summary>
    /// Sales Return Audit Report (salesreturnauditreport)
    /// Displays all sales returns and their item details Month-wise, Day-wise, and Year-wise,
    /// styled with the exact theme, gridReport, and ultraPanelGridFooter of frmVendorOutstandingReport / frmpurchaseAuditreport.
    /// </summary>
    public partial class salesreturnauditreport : Form
    {
        #region Colour Palette (Exact match with frmVendorOutstandingReport)
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop        = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom     = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder     = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);
        #endregion

        #region Private Fields
        private readonly SalesReturnReportRepository _reportRepository;
        private readonly Dictionary<int, List<SalesReturnReportDetail>> _detailsCache;
        private List<SalesReturnReportMaster> _loadedReturns;
        private DataSet _currentDataSet;
        private bool _isLoading;

        // Dynamic footer panel and cell controls (matching frmVendorOutstandingReport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalReturns",
            "TotalItems",
            "ItemCount",
            "TotalQty",
            "Qty",
            "SalesPrice",
            "Amount",
            "SubTotal",
            "TaxAmt",
            "GrandTotal"
        };

        // Header drag-to-hide & Column Chooser
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        private ReportColumnFilterManager _filterManager;
        #endregion

        #region Helper Classes
        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }
            public int BandIndex { get; }

            public ColumnChooserItem(string key, string text, int bandIndex = 0)
            {
                ColumnKey = key;
                DisplayText = text;
                BandIndex = bandIndex;
            }

            public override string ToString() => DisplayText;
        }

        private sealed class FilteredReturnRecord
        {
            public SalesReturnReportMaster Master { get; set; }
            public List<SalesReturnReportDetail> Details { get; set; }
            public string PeriodKey { get; set; }
            public string PeriodLabel { get; set; }
            public DateTime PeriodSortDate { get; set; }
        }
        #endregion

        #region Constructor & Form Lifecycle
        public salesreturnauditreport()
        {
            _reportRepository = new SalesReturnReportRepository();
            _detailsCache = new Dictionary<int, List<SalesReturnReportDetail>>();
            _loadedReturns = new List<SalesReturnReportMaster>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += salesreturnauditreport_Load;
            FormClosing += salesreturnauditreport_FormClosing;
            KeyPreview = true;
            KeyDown += salesreturnauditreport_KeyDown;

            // Button events
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            btnCustomerPicker.Click += btnCustomerPicker_Click;
            btnItemPicker.Click += btnItemPicker_Click;

            // Filter change events
            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboMonth.ValueChanged += FilterSelection_ValueChanged;
            ultraComboYear.ValueChanged += FilterSelection_ValueChanged;
            ultraComboViewMode.ValueChanged += FilterSelection_ValueChanged;
            ultraComboPayMode.ValueChanged += FilterSelection_ValueChanged;
            chkSingleDay.CheckedChanged += chkSingleDay_CheckedChanged;
            dtFrom.ValueChanged += FilterSelection_ValueChanged;
            dtTo.ValueChanged += FilterSelection_ValueChanged;
            txtCustomerSearch.ValueChanged += FilterSelection_ValueChanged;
            txtReturnNo.KeyDown += SearchTextBox_KeyDown;
            txtItemFilter.KeyDown += SearchTextBox_KeyDown;

            // Grid events
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            gridReport.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                ReportGridLayoutHelper.SaveLayout("SalesReturnAuditReport", gridReport, userHiddenColumnKeys);
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) => UpdateFooterValues();
        }

        private void salesreturnauditreport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void salesreturnauditreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout("SalesReturnAuditReport", gridReport, userHiddenColumnKeys);

            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _currentDataSet = null;
            }
        }

        private void InitializeForm()
        {
            _isLoading = true;
            try
            {
                ReportGridLayoutHelper.LoadHiddenColumns("SalesReturnAuditReport", userHiddenColumnKeys);
                Text = "Sales Return Audit Report";
                if (TopLevel)
                {
                    WindowState = FormWindowState.Maximized;
                    StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    WindowState = FormWindowState.Normal;
                    Dock = DockStyle.Fill;
                }

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                LoadCustomers();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
                _filterManager = new ReportColumnFilterManager(gridReport, () => { UpdateFooterCellPositions(); UpdateFooterValues(); });
            }
            finally
            {
                _isLoading = false;
            }

            BeginInvoke(new Action(() =>
            {
                LoadReport();
            }));
        }
        #endregion

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;

            // Report Period Mode: Month Wise, Day Wise, Year Wise, Date Range, ALL
            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("DAY_WISE", "Day Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date by Range");
            ultraComboPeriodMode.Items.Add("ALL", "ALL");
            ultraComboPeriodMode.Value = "MONTH_WISE";

            // Month selector (0 = ALL Months, 1..12 = Jan..Dec)
            ultraComboMonth.Items.Clear();
            ultraComboMonth.Items.Add(0, "ALL Months");
            for (int m = 1; m <= 12; m++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                ultraComboMonth.Items.Add(m, monthName);
            }
            ultraComboMonth.Value = today.Month;

            // Year selector (0 = ALL Years, current year down to 2000)
            PopulateYearCombo(today.Year);
            ultraComboYear.Value = today.Year;

            // Date pickers
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
            chkSingleDay.Checked = false;

            // View Mode selector
            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("PERIOD_RETURNS_ITEMS", "Period -> Sales Returns -> Item Details");
            ultraComboViewMode.Items.Add("RETURNS_ITEMS", "Sales Returns & Item Details (Return Wise)");
            ultraComboViewMode.Items.Add("PERIOD_ITEMS", "Period -> All Returned Items");
            ultraComboViewMode.Items.Add("ITEMS_FLAT", "All Returned Items (Flat List)");
            ultraComboViewMode.Value = "PERIOD_RETURNS_ITEMS";

            // Pay Mode dropdown
            ultraComboPayMode.Items.Clear();
            ultraComboPayMode.Items.Add("ALL", "ALL");
            ultraComboPayMode.Items.Add("Cash", "Cash");
            ultraComboPayMode.Items.Add("Card", "Card");
            ultraComboPayMode.Items.Add("Credit", "Credit");
            ultraComboPayMode.Items.Add("UPI", "UPI");
            ultraComboPayMode.Items.Add("Bank Transfer", "Bank Transfer");
            ultraComboPayMode.Value = "ALL";

            txtCustomerSearch.Text = string.Empty;
            txtReturnNo.Text = string.Empty;
            txtReturnNo.NullText = "Return / Inv No...";
            txtItemFilter.Text = string.Empty;
            txtItemFilter.NullText = "Item name or barcode...";

            UpdatePeriodControlVisibility();
        }

        private void PopulateYearCombo(int maxYear)
        {
            object prevVal = ultraComboYear.Value;
            ultraComboYear.Items.Clear();
            ultraComboYear.Items.Add(0, "ALL Years");

            int topYear = Math.Max(maxYear, DateTime.Today.Year);
            for (int y = topYear; y >= 2000; y--)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }

            if (prevVal != null)
            {
                ultraComboYear.Value = prevVal;
            }
        }

        private void LoadCustomers()
        {
            try
            {
                txtCustomerSearch.Items.Clear();
                txtCustomerSearch.Items.Add(string.Empty, "ALL Customers");

                Dropdowns dropdowns = new Dropdowns();
                var customerData = dropdowns.CustomerDDl();
                if (customerData?.List != null)
                {
                    foreach (var cust in customerData.List.Where(x => x != null && !string.IsNullOrWhiteSpace(x.LedgerName)).OrderBy(x => x.LedgerName))
                    {
                        txtCustomerSearch.Items.Add(cust.LedgerName, cust.LedgerName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading customer list: " + ex.Message);
            }
        }

        private void UpdatePeriodControlVisibility()
        {
            string mode = GetPeriodMode();
            bool isMonthWise = string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase);
            bool isDayWise = string.Equals(mode, "DAY_WISE", StringComparison.OrdinalIgnoreCase);
            bool isYearWise = string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase);
            bool isDateRange = string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase);

            lblMonth.Visible = isMonthWise;
            ultraComboMonth.Visible = isMonthWise;

            lblYear.Visible = isMonthWise || isYearWise;
            ultraComboYear.Visible = isMonthWise || isYearWise;

            if (isYearWise)
            {
                lblYear.Location = new Point(268, 17);
                ultraComboYear.Location = new Point(314, 14);
            }
            else if (isMonthWise)
            {
                lblYear.Location = new Point(462, 17);
                ultraComboYear.Location = new Point(500, 14);
            }

            chkSingleDay.Visible = isDayWise;
            bool showFrom = isDayWise || isDateRange;
            bool showTo = isDateRange || (isDayWise && !chkSingleDay.Checked);

            lblFromDate.Text = (isDayWise && chkSingleDay.Checked) ? "Date" : "From";
            lblFromDate.Visible = showFrom;
            dtFrom.Visible = showFrom;
            lblToDate.Visible = showTo;
            dtTo.Visible = showTo;
        }
        #endregion

        #region Styling & Layout (Exact match with frmVendorOutstandingReport)
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelControls.Dock = DockStyle.Top;
                ultraPanelControls.Height = 84;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelAction.Dock = DockStyle.Top;
                ultraPanelAction.Height = 47;
            }

            if (ultraPanelMaster != null)
            {
                ultraPanelMaster.Appearance.BackColor = FormBackColor;
                ultraPanelMaster.Appearance.BorderColor = BorderBlue;
                ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelMaster.Dock = DockStyle.Fill;
            }

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
                ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
                ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelGridFooter.Dock = DockStyle.Bottom;
                ultraPanelGridFooter.Height = 26;
            }

            if (gridReport != null)
            {
                gridReport.Dock = DockStyle.Fill;
                gridReport.BringToFront();
            }

            // Enforce docking order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.SendToBack();
            }
            gridReport.BringToFront();

            StyleLabel(lblPeriodMode);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblViewMode);
            StyleLabel(lblCustomer);
            StyleLabel(lblDocNo);
            StyleLabel(lblPayMode);
            StyleLabel(lblItemFilter);

            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnExpandAll);
            StyleClassicButton(btnCollapseAll);
            StyleClassicButton(btnToggleSelection);
            StylePickerButton(btnCustomerPicker);
            StylePickerButton(btnItemPicker);
        }

        private static void StyleClassicButton(UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = ButtonLightOutline;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;
            button.PressedAppearance.BackColor = Color.FromArgb(118, 161, 214);
            button.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private static void StylePickerButton(Button button)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button.Cursor = Cursors.Hand;
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboPeriodMode, true);
            StyleFilterCombo(ultraComboMonth, true);
            StyleFilterCombo(ultraComboYear, true);
            StyleFilterCombo(ultraComboViewMode, true);
            StyleFilterCombo(ultraComboPayMode, true);
            StyleFilterCombo(txtCustomerSearch, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleCheckEditor(chkSingleDay);
            StyleTextEditor(txtReturnNo);
            StyleTextEditor(txtItemFilter);
        }

        private static void StyleLabel(UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9;
            label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleFilterCombo(UltraComboEditor combo, bool isDropDownList)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 9.5F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleDateEditor(UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9.5F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9.5F;
        }

        private static void StyleCheckEditor(UltraCheckEditor checkEditor)
        {
            if (checkEditor == null) return;
            checkEditor.UseAppStyling = false;
            checkEditor.UseOsThemes = DefaultableBoolean.False;
            checkEditor.Appearance.BackColor = Color.Transparent;
            checkEditor.Appearance.ForeColor = ControlTextColor;
            checkEditor.Appearance.FontData.Name = "Tahoma";
            checkEditor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;
            layout.ViewStyleBand = ViewStyleBand.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType = FilterUIType.HeaderIcons;
            layout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            layout.Override.FilterOperandStyle = FilterOperandStyle.Combo;
            layout.Override.FilterClearButtonLocation = FilterClearButtonLocation.Hidden;

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.SeparateElement;

            layout.Appearance.BackColor = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 20;
            layout.Override.DefaultRowHeight = 22;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;
            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }
        #endregion

        #region Grid Footer Setup & Alignment (Exact Parity with frmVendorOutstandingReport)
        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Height = 26;

            _columnAggregations.Clear();
            foreach (var col in summaryDefaultNumericColumns)
            {
                _columnAggregations[col] = "Sum";
            }

            CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || column.IsChaptered)
                    continue;

                ContentAlignment align = ContentAlignment.MiddleLeft;
                if (column.CellAppearance.TextHAlign == HAlign.Right || IsSummableColumn(column))
                    align = ContentAlignment.MiddleRight;
                else if (column.CellAppearance.TextHAlign == HAlign.Center)
                    align = ContentAlignment.MiddleCenter;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = align,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(ultraPanelGridFooter.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = summaryDefaultNumericColumns.Contains(column.Key) ? "Sum" : "None";
                }

                xOffset += column.Width;
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;

            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                e.Graphics.DrawLine(borderPen, lbl.Width - 1, 0, lbl.Width - 1, lbl.Height);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Tag = columnKey };

            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            AddFooterMenuItem(menu, "Sum", "Sum", isNumeric);
            AddFooterMenuItem(menu, "Min", "Min", isNumeric);
            AddFooterMenuItem(menu, "Max", "Max", isNumeric);
            AddFooterMenuItem(menu, "Count", "Count", true);
            AddFooterMenuItem(menu, "Average", "Avg", isNumeric);
            menu.Items.Add(new ToolStripSeparator());
            AddFooterMenuItem(menu, "None", "None", true);

            menu.Opening += (sender, e) =>
            {
                string current = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : (summaryDefaultNumericColumns.Contains(columnKey) ? "Sum" : "None");

                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null)
                    {
                        menuItem.Checked = string.Equals(menuItem.Tag.ToString(), current, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private void AddFooterMenuItem(ContextMenuStrip menu, string text, string tag, bool enabled)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text) { Tag = tag, Enabled = enabled };
            item.Click += FooterContextMenu_Click;
            menu.Items.Add(item);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Owner is ContextMenuStrip menu && menu.Tag != null && item.Tag != null)
            {
                _columnAggregations[menu.Tag.ToString()] = item.Tag.ToString();
                UpdateFooterValues();
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? gridReport.DisplayLayout.Override.RowSelectorWidth
                : 0;

            int scrollOffset = 0;
            if (gridReport.ActiveColScrollRegion != null)
            {
                scrollOffset = gridReport.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || column.IsChaptered || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                var headerUI = column.Header.GetUIElement();
                int left, width;

                if (headerUI != null)
                {
                    left = headerUI.Rect.Left;
                    width = headerUI.Rect.Width;
                }
                else
                {
                    left = calculatedX;
                    width = column.Width;
                }

                calculatedX += column.Width;

                footerLabel.Left = left;
                footerLabel.Width = width;
                footerLabel.Top = 1;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0 || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!_columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, _columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, _columnAggregations[columnKey], result);

                footerLabel.Text = displayValue;
                footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
            {
                return aggregation == "Count" ? (object)0 : null;
            }

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Sum(value => value.Value);
                case "Min":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderBy(value => value)
                        .FirstOrDefault();
                case "Max":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderByDescending(value => value)
                        .FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count == 0 ? 0m : values.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (gridReport.DisplayLayout != null &&
                gridReport.DisplayLayout.Bands.Count > 0 &&
                gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                !string.IsNullOrEmpty(gridReport.DisplayLayout.Bands[0].Columns[columnKey].Format))
            {
                return string.Format("{0:" + gridReport.DisplayLayout.Bands[0].Columns[columnKey].Format + "}", result);
            }

            if (result is decimal || result is double || result is float)
            {
                return string.Format("{0:#,##0.00}", result);
            }

            if (result is DateTime dt)
            {
                return dt.ToString("dd-MMM-yyyy");
            }

            return Convert.ToString(result);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
                yield break;

            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row.IsFilteredOut || !row.IsDataRow || row.Hidden)
                    continue;

                yield return row;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
        }

        private static decimal? GetNumericValue(object value)
        {
            if (!HasCellValue(value)) return null;
            if (decimal.TryParse(value.ToString(), out decimal parsed)) return parsed;
            return null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser
        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.GiveFeedback += (s, e) =>
            {
                if (isDraggingHeaderToHide)
                {
                    e.UseDefaultCursors = false;
                    Cursor.Current = blackXCursor;
                }
            };
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                UIElement element = gridReport.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerUI?.Header is Infragistics.Win.UltraWinGrid.ColumnHeader header && header.Column != null)
                {
                    ShowHeaderContextMenu(header.Column, e.Location);
                }
                return;
            }

            if (e.Button == MouseButtons.Left && _filterManager != null && _filterManager.HandleMouseDown(e)) { isDraggingHeaderToHide = false; columnBeingDragged = null; return; }
            if (e.Button == MouseButtons.Left)
            {
                UIElement element = gridReport.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerUI?.Header is Infragistics.Win.UltraWinGrid.ColumnHeader header && header.Column != null)
                {
                    if (IsCustomizableColumn(header.Column))
                    {
                        columnBeingDragged = header.Column;
                        headerDragStartPoint = e.Location;
                        isDraggingHeaderToHide = false;
                    }
                }
            }
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide && _filterManager != null && _filterManager.HandleMouseMove(e, headerToolTip)) return;
            if (columnBeingDragged != null && e.Button == MouseButtons.Left)
            {
                int dy = Math.Abs(e.Y - headerDragStartPoint.Y);
                int dx = Math.Abs(e.X - headerDragStartPoint.X);

                if (!isDraggingHeaderToHide && dy > 18 && dy > dx)
                {
                    isDraggingHeaderToHide = true;
                    Cursor = blackXCursor;
                }

                if (isDraggingHeaderToHide)
                {
                    Cursor = blackXCursor;
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide && columnBeingDragged != null)
            {
                HideColumnWithAnimation(columnBeingDragged);
            }

            isDraggingHeaderToHide = false;
            columnBeingDragged = null;
            Cursor = Cursors.Default;
        }

        private static bool IsCustomizableColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered || col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            string key = col.Key;
            return !string.Equals(key, "PeriodKey", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(key, "DetailID", StringComparison.OrdinalIgnoreCase) &&
                   !key.EndsWith("Id", StringComparison.OrdinalIgnoreCase);
        }

        private void HideColumnWithAnimation(UltraGridColumn column)
        {
            if (column == null || !IsCustomizableColumn(column)) return;

            string keyWithBand = $"{column.Band.Index}:{column.Key}";
            userHiddenColumnKeys.Add(keyWithBand);
            userHiddenColumnKeys.Add(column.Key);
            column.Hidden = true;

            CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("SalesReturnAuditReport", gridReport, userHiddenColumnKeys);
            ShowColumnChooser(autoOpenIfClosed: false);
        }

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                Bitmap bmp = new Bitmap(24, 24);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    using (Pen whitePen = new Pen(Color.White, 4f))
                    {
                        g.DrawLine(whitePen, 4, 4, 20, 20);
                        g.DrawLine(whitePen, 20, 4, 4, 20);
                    }
                    using (Pen redPen = new Pen(Color.FromArgb(200, 30, 30), 2.2f))
                    {
                        g.DrawLine(redPen, 4, 4, 20, 20);
                        g.DrawLine(redPen, 20, 4, 4, 20);
                    }
                }
                return new Cursor(bmp.GetHicon());
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn column, Point location) { ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) }; _filterManager?.AddFilterContextMenuItems(menu, column, location); ToolStripMenuItem itemHide = new ToolStripMenuItem($"Hide Column '{column.Header.Caption}'"); itemHide.Click += (s, e) => HideColumnWithAnimation(column); menu.Items.Add(itemHide); ToolStripMenuItem itemChooser = new ToolStripMenuItem("Column Chooser..."); itemChooser.Click += (s, e) => ShowColumnChooser(autoOpenIfClosed: true); menu.Items.Add(itemChooser); ToolStripMenuItem itemRestoreAll = new ToolStripMenuItem("Restore All Columns"); itemRestoreAll.Click += (s, e) => UnhideAllColumns(); menu.Items.Add(itemRestoreAll); menu.Show(gridReport, location); } private void DummyContextMenu(UltraGridColumn column, Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem itemHide = new ToolStripMenuItem($"Hide Column '{column.Header.Caption}'");
            itemHide.Click += (s, e) => HideColumnWithAnimation(column);
            menu.Items.Add(itemHide);

            ToolStripMenuItem itemChooser = new ToolStripMenuItem("Column Chooser...");
            itemChooser.Click += (s, e) => ShowColumnChooser(autoOpenIfClosed: true);
            menu.Items.Add(itemChooser);

            ToolStripMenuItem itemRestoreAll = new ToolStripMenuItem("Restore All Columns");
            itemRestoreAll.Click += (s, e) => UnhideAllColumns();
            menu.Items.Add(itemRestoreAll);

            menu.Show(gridReport, location);
        }

        private void ShowColumnChooser(bool autoOpenIfClosed = true)
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                if (!autoOpenIfClosed) return;
                BuildColumnChooserForm();
            }

            PopulateColumnChooserListBox();

            if (!columnChooserForm.Visible)
            {
                PositionColumnChooser();
                columnChooserForm.Show(this);
            }
            else
            {
                columnChooserForm.BringToFront();
            }
        }

        private void BuildColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization - Drag & Drop",
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(240, 320),
                TopMost = false,
                ShowInTaskbar = false,
                BackColor = FormBackColor
            };

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(6, 4, 6, 4),
                BackColor = ActionPanelBackColor
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8.5F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Text = "Search columns..."
            };
            txtColumnSearch.GotFocus += (s, e) =>
            {
                if (txtColumnSearch.Text == "Search columns...")
                {
                    txtColumnSearch.Text = string.Empty;
                    txtColumnSearch.ForeColor = ControlTextColor;
                }
            };
            txtColumnSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtColumnSearch.Text))
                {
                    txtColumnSearch.Text = "Search columns...";
                    txtColumnSearch.ForeColor = Color.DimGray;
                }
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();
            topPanel.Controls.Add(txtColumnSearch);

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 26,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Tahoma", 8.5F, FontStyle.Regular),
                SelectionMode = SelectionMode.One
            };
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            Button btnRestore = new Button
            {
                Text = "Restore All Columns",
                Dock = DockStyle.Bottom,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = GridHeaderBlue,
                ForeColor = Color.White,
                Font = new Font("Tahoma", 8.25F, FontStyle.Bold)
            };
            btnRestore.FlatAppearance.BorderColor = BorderBlue;
            btnRestore.Click += (s, e) => UnhideAllColumns();

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(btnRestore);
            columnChooserForm.Controls.Add(topPanel);

            columnChooserForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    columnChooserForm.Hide();
                }
            };
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout == null)
                return;

            columnChooserListBox.Items.Clear();
            string filter = (txtColumnSearch?.Text ?? string.Empty).Trim();
            if (filter == "Search columns...") filter = string.Empty;

            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[b];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!IsCustomizableColumn(col)) continue;
                    if (!col.Hidden && !userHiddenColumnKeys.Contains($"{b}:{col.Key}"))
                        continue;

                    string caption = string.IsNullOrWhiteSpace(col.Header.Caption) ? col.Key : col.Header.Caption;
                    if (band.Index > 0)
                    {
                        caption = $"{band.Header?.Caption ?? band.Key} -> {caption}";
                    }

                    if (string.IsNullOrEmpty(filter) || caption.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption, band.Index));
                    }
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.BandIndex, item.ColumnKey, -1);
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index >= 0 && e.Button == MouseButtons.Left)
            {
                ColumnChooserItem item = columnChooserListBox.Items[index] as ColumnChooserItem;
                if (item != null)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count)
                return;

            ColumnChooserItem item = columnChooserListBox.Items[e.Index] as ColumnChooserItem;
            if (item == null) return;

            e.Graphics.FillRectangle(new SolidBrush(Color.White), e.Bounds);

            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
            using (GraphicsPath path = RoundedRect(rect, 4))
            using (LinearGradientBrush bgBrush = new LinearGradientBrush(rect, GridHeaderBlue, GridHeaderBlueDark, LinearGradientMode.Vertical))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(item.DisplayText, columnChooserListBox.Font, textBrush, rect, sf);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void UnhideColumn(int bandIndex, string columnKey, int targetPosition)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count <= bandIndex)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[bandIndex];
            if (band.Columns.Exists(columnKey))
            {
                userHiddenColumnKeys.Remove($"{bandIndex}:{columnKey}");
                userHiddenColumnKeys.Remove(columnKey);
                UltraGridColumn col = band.Columns[columnKey];
                col.Hidden = false;
                if (targetPosition >= 0)
                {
                    col.Header.VisiblePosition = targetPosition;
                }
                CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                ReportGridLayoutHelper.SaveLayout("SalesReturnAuditReport", gridReport, userHiddenColumnKeys);
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                foreach (UltraGridColumn col in gridReport.DisplayLayout.Bands[b].Columns)
                {
                    if (IsCustomizableColumn(col) && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                    {
                        col.Hidden = false;
                    }
                }
            }
            CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("SalesReturnAuditReport", gridReport, userHiddenColumnKeys);
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                Point screenPt = PointToScreen(Point.Empty);
                columnChooserForm.Location = new Point(
                    screenPt.X + Width - columnChooserForm.Width - 30,
                    screenPt.Y + Height - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
            }
        }
        #endregion

        #region Data Loading & Period / Item Building
        private string GetPeriodMode()
        {
            return Convert.ToString(ultraComboPeriodMode.Value ?? "MONTH_WISE");
        }

        private string GetViewMode()
        {
            return Convert.ToString(ultraComboViewMode.Value ?? "PERIOD_RETURNS_ITEMS");
        }

        private int GetSelectedMonth()
        {
            if (ultraComboMonth.Value == null) return 0;
            return int.TryParse(ultraComboMonth.Value.ToString(), out int month) ? month : 0;
        }

        private int GetSelectedYear()
        {
            if (ultraComboYear.Value == null) return 0;
            return int.TryParse(ultraComboYear.Value.ToString(), out int year) ? year : 0;
        }

        private bool ResolveQueryDateRange(out DateTime fromDate, out DateTime toDate)
        {
            string mode = GetPeriodMode();
            int year = GetSelectedYear();
            int month = GetSelectedMonth();

            fromDate = new DateTime(1990, 1, 1);
            toDate = DateTime.Today;

            if (string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
            {
                if (year > 0 && month > 0)
                {
                    fromDate = new DateTime(year, month, 1);
                    toDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                }
                else if (year > 0)
                {
                    fromDate = new DateTime(year, 1, 1);
                    toDate = new DateTime(year, 12, 31);
                }
                return true;
            }

            if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                if (year > 0)
                {
                    fromDate = new DateTime(year, 1, 1);
                    toDate = new DateTime(year, 12, 31);
                }
                return true;
            }

            if (string.Equals(mode, "DAY_WISE", StringComparison.OrdinalIgnoreCase))
            {
                DateTime start = dtFrom.Value == null ? DateTime.Today : Convert.ToDateTime(dtFrom.Value).Date;
                DateTime end = chkSingleDay.Checked
                    ? start
                    : (dtTo.Value == null ? DateTime.Today : Convert.ToDateTime(dtTo.Value).Date);

                if (start > end)
                {
                    MessageBox.Show("From date cannot be greater than To date.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtFrom.Focus();
                    return false;
                }

                fromDate = start;
                toDate = end;
                return true;
            }

            if (string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase))
            {
                DateTime start = dtFrom.Value == null ? DateTime.Today : Convert.ToDateTime(dtFrom.Value).Date;
                DateTime end = dtTo.Value == null ? DateTime.Today : Convert.ToDateTime(dtTo.Value).Date;

                if (start > end)
                {
                    MessageBox.Show("From date cannot be greater than To date.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtFrom.Focus();
                    return false;
                }

                fromDate = start;
                toDate = end;
                return true;
            }

            return true;
        }

        private void LoadReport()
        {
            if (_isLoading) return;
            if (!ResolveQueryDateRange(out DateTime fromDate, out DateTime toDate))
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            _isLoading = true;

            try
            {
                int branchId = SessionContext.BranchId;
                _loadedReturns = _reportRepository.GetSalesReturnRecords(fromDate, toDate, branchId) ?? new List<SalesReturnReportMaster>();

                // Ensure any new years in database are available in ultraComboYear
                if (_loadedReturns.Count > 0)
                {
                    int maxReturnYear = _loadedReturns.Max(r => r.SReturnDate.Year);
                    if (maxReturnYear > DateTime.Today.Year)
                    {
                        PopulateYearCombo(maxReturnYear);
                    }
                }

                BuildAndBindReportData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Sales Return Audit Report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Cursor = previousCursor;
            }
        }

        private List<SalesReturnReportDetail> GetDetailsForSalesReturn(int sReturnNo)
        {
            if (sReturnNo <= 0)
                return new List<SalesReturnReportDetail>();

            if (_detailsCache.TryGetValue(sReturnNo, out List<SalesReturnReportDetail> cached))
                return cached;

            SalesReturnReportData data = _reportRepository.GetSalesReturnReportDetails(sReturnNo);
            List<SalesReturnReportDetail> details = data?.Details ?? new List<SalesReturnReportDetail>();
            _detailsCache[sReturnNo] = details;
            return details;
        }

        private List<FilteredReturnRecord> GetFilteredReturnRecords()
        {
            string periodMode = GetPeriodMode();
            int selectedMonth = GetSelectedMonth();
            int selectedYear = GetSelectedYear();

            string customerFilter = (txtCustomerSearch.Text ?? string.Empty).Trim();
            if (string.Equals(customerFilter, "ALL Customers", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(customerFilter, "ALL", StringComparison.OrdinalIgnoreCase))
                customerFilter = string.Empty;

            string docNoFilter = (txtReturnNo.Text ?? string.Empty).Trim();
            string paymentFilter = Convert.ToString(ultraComboPayMode.Value ?? "ALL");
            string itemFilter = (txtItemFilter.Text ?? string.Empty).Trim();

            List<FilteredReturnRecord> records = new List<FilteredReturnRecord>();

            foreach (SalesReturnReportMaster ret in _loadedReturns)
            {
                if (ret == null || ret.SReturnNo <= 0)
                    continue;

                // Month filter when "ALL Years" + specific Month is selected
                if (string.Equals(periodMode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
                {
                    if (selectedMonth > 0 && ret.SReturnDate.Month != selectedMonth)
                        continue;
                    if (selectedYear > 0 && ret.SReturnDate.Year != selectedYear)
                        continue;
                }
                else if (string.Equals(periodMode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
                {
                    if (selectedYear > 0 && ret.SReturnDate.Year != selectedYear)
                        continue;
                }

                // Customer filter
                if (!string.IsNullOrEmpty(customerFilter))
                {
                    if (string.IsNullOrEmpty(ret.CustomerName) ||
                        ret.CustomerName.IndexOf(customerFilter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                // Return No / Invoice No filter
                if (!string.IsNullOrEmpty(docNoFilter))
                {
                    bool matchReturnNo = ret.SReturnNo.ToString().IndexOf(docNoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchInvoiceNo = !string.IsNullOrEmpty(ret.InvoiceNo) &&
                                          ret.InvoiceNo.IndexOf(docNoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchReturnNo && !matchInvoiceNo)
                        continue;
                }

                // Payment mode filter
                if (!string.Equals(paymentFilter, "ALL", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(paymentFilter))
                {
                    if (string.IsNullOrEmpty(ret.Paymode) || ret.Paymode.IndexOf(paymentFilter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                List<SalesReturnReportDetail> allDetails = GetDetailsForSalesReturn(ret.SReturnNo);
                List<SalesReturnReportDetail> matchingDetails = allDetails;

                if (!string.IsNullOrEmpty(itemFilter))
                {
                    matchingDetails = allDetails
                        .Where(d => !string.IsNullOrEmpty(d.ItemName) && d.ItemName.IndexOf(itemFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();

                    if (matchingDetails.Count == 0)
                        continue;
                }

                GetPeriodKeyAndLabel(ret.SReturnDate, periodMode, out string periodKey, out string periodLabel, out DateTime periodSortDate);

                records.Add(new FilteredReturnRecord
                {
                    Master = ret,
                    Details = matchingDetails,
                    PeriodKey = periodKey,
                    PeriodLabel = periodLabel,
                    PeriodSortDate = periodSortDate
                });
            }

            return records
                .OrderByDescending(r => r.Master.SReturnDate)
                .ThenByDescending(r => r.Master.SReturnNo)
                .ToList();
        }

        private static void GetPeriodKeyAndLabel(DateTime date, string periodMode, out string periodKey, out string periodLabel, out DateTime periodSortDate)
        {
            if (string.Equals(periodMode, "DAY_WISE", StringComparison.OrdinalIgnoreCase))
            {
                periodKey = date.ToString("yyyy-MM-dd");
                periodLabel = date.ToString("dd-MMM-yyyy (dddd)");
                periodSortDate = date.Date;
            }
            else if (string.Equals(periodMode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                periodKey = date.ToString("yyyy");
                periodLabel = "Year " + date.ToString("yyyy");
                periodSortDate = new DateTime(date.Year, 1, 1);
            }
            else
            {
                periodKey = date.ToString("yyyy-MM");
                periodLabel = date.ToString("MMMM yyyy");
                periodSortDate = new DateTime(date.Year, date.Month, 1);
            }
        }

        private void BuildAndBindReportData()
        {
            List<FilteredReturnRecord> records = GetFilteredReturnRecords();
            string viewMode = GetViewMode();

            gridReport.DataSource = null;
            gridReport.DataMember = null;
            _filterManager?.ClearFilters();

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _currentDataSet = null;
            }

            if (string.Equals(viewMode, "PERIOD_RETURNS_ITEMS", StringComparison.OrdinalIgnoreCase))
            {
                _currentDataSet = BuildPeriodReturnsItemsDataSet(records);
                gridReport.DataSource = _currentDataSet;
                gridReport.DataMember = "PeriodSummary";
            }
            else if (string.Equals(viewMode, "RETURNS_ITEMS", StringComparison.OrdinalIgnoreCase))
            {
                _currentDataSet = BuildReturnsItemsDataSet(records);
                gridReport.DataSource = _currentDataSet;
                gridReport.DataMember = "SalesReturnMaster";
            }
            else if (string.Equals(viewMode, "PERIOD_ITEMS", StringComparison.OrdinalIgnoreCase))
            {
                _currentDataSet = BuildPeriodItemsDataSet(records);
                gridReport.DataSource = _currentDataSet;
                gridReport.DataMember = "PeriodSummary";
            }
            else
            {
                _currentDataSet = BuildFlatItemsDataSet(records);
                gridReport.DataSource = _currentDataSet;
                gridReport.DataMember = "ItemsFlat";
            }

            // If a single specific period (e.g. a selected Month, Day, or Year) is shown in PeriodSummary,
            // auto-expand it so the user immediately sees all sales returns and details of that selected period!
            if (gridReport.Rows != null && gridReport.Rows.Count == 1 &&
                (string.Equals(viewMode, "PERIOD_RETURNS_ITEMS", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(viewMode, "PERIOD_ITEMS", StringComparison.OrdinalIgnoreCase)))
            {
                gridReport.Rows[0].Expanded = true;
            }

            CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private DataSet BuildPeriodReturnsItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("SalesReturnAudit_PeriodBillsItems");

            DataTable periodTable = CreatePeriodSummaryTable();
            DataTable masterTable = CreateSalesReturnMasterTable(includePeriodKey: true);
            DataTable detailTable = CreateSalesReturnDetailTable();

            var periodGroups = records
                .GroupBy(r => new { r.PeriodKey, r.PeriodLabel, r.PeriodSortDate })
                .OrderByDescending(g => g.Key.PeriodSortDate);

            foreach (var group in periodGroups)
            {
                DataRow pRow = periodTable.NewRow();
                pRow["PeriodKey"] = group.Key.PeriodKey;
                pRow["PeriodLabel"] = group.Key.PeriodLabel;
                pRow["FromDate"] = group.Min(x => x.Master.SReturnDate.Date);
                pRow["ToDate"] = group.Max(x => x.Master.SReturnDate.Date);
                pRow["TotalReturns"] = group.Count();
                pRow["TotalItems"] = group.Sum(x => x.Details.Count);
                pRow["TotalQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                pRow["SubTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.SubTotal));
                pRow["TaxAmt"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.TaxAmt)));
                pRow["GrandTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.GrandTotal));
                periodTable.Rows.Add(pRow);

                foreach (FilteredReturnRecord rec in group.OrderByDescending(x => x.Master.SReturnDate).ThenByDescending(x => x.Master.SReturnNo))
                {
                    AddSalesReturnMasterRow(masterTable, rec, includePeriodKey: true);
                    AddSalesReturnDetailRows(detailTable, rec);
                }
            }

            ds.Tables.Add(periodTable);
            ds.Tables.Add(masterTable);
            ds.Tables.Add(detailTable);

            ds.Relations.Add(new DataRelation("PeriodReturns", periodTable.Columns["PeriodKey"], masterTable.Columns["PeriodKey"], true));
            ds.Relations.Add(new DataRelation("ReturnItems", masterTable.Columns["SReturnNo"], detailTable.Columns["SReturnNo"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildReturnsItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("SalesReturnAudit_BillsItems");

            DataTable masterTable = CreateSalesReturnMasterTable(includePeriodKey: false);
            DataTable detailTable = CreateSalesReturnDetailTable();

            foreach (FilteredReturnRecord rec in records)
            {
                AddSalesReturnMasterRow(masterTable, rec, includePeriodKey: false);
                AddSalesReturnDetailRows(detailTable, rec);
            }

            ds.Tables.Add(masterTable);
            ds.Tables.Add(detailTable);
            ds.Relations.Add(new DataRelation("ReturnItems", masterTable.Columns["SReturnNo"], detailTable.Columns["SReturnNo"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildPeriodItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("SalesReturnAudit_PeriodItems");

            DataTable periodTable = CreatePeriodSummaryTable();
            DataTable itemsTable = CreateFlatItemsTable("PeriodItems", includePeriodKey: true);

            var periodGroups = records
                .GroupBy(r => new { r.PeriodKey, r.PeriodLabel, r.PeriodSortDate })
                .OrderByDescending(g => g.Key.PeriodSortDate);

            foreach (var group in periodGroups)
            {
                DataRow pRow = periodTable.NewRow();
                pRow["PeriodKey"] = group.Key.PeriodKey;
                pRow["PeriodLabel"] = group.Key.PeriodLabel;
                pRow["FromDate"] = group.Min(x => x.Master.SReturnDate.Date);
                pRow["ToDate"] = group.Max(x => x.Master.SReturnDate.Date);
                pRow["TotalReturns"] = group.Count();
                pRow["TotalItems"] = group.Sum(x => x.Details.Count);
                pRow["TotalQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                pRow["SubTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.SubTotal));
                pRow["TaxAmt"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.TaxAmt)));
                pRow["GrandTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.GrandTotal));
                periodTable.Rows.Add(pRow);

                foreach (FilteredReturnRecord rec in group.OrderByDescending(x => x.Master.SReturnDate).ThenByDescending(x => x.Master.SReturnNo))
                {
                    AddFlatItemRows(itemsTable, rec, includePeriodKey: true);
                }
            }

            ds.Tables.Add(periodTable);
            ds.Tables.Add(itemsTable);
            ds.Relations.Add(new DataRelation("PeriodAllItems", periodTable.Columns["PeriodKey"], itemsTable.Columns["PeriodKey"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildFlatItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("SalesReturnAudit_ItemsFlat");
            DataTable itemsTable = CreateFlatItemsTable("ItemsFlat", includePeriodKey: false);

            foreach (FilteredReturnRecord rec in records)
            {
                AddFlatItemRows(itemsTable, rec, includePeriodKey: false);
            }

            ds.Tables.Add(itemsTable);
            ds.AcceptChanges();
            return ds;
        }

        private static DataTable CreatePeriodSummaryTable()
        {
            DataTable dt = new DataTable("PeriodSummary");
            dt.Columns.Add("PeriodKey", typeof(string));
            dt.Columns.Add("PeriodLabel", typeof(string));
            dt.Columns.Add("FromDate", typeof(DateTime));
            dt.Columns.Add("ToDate", typeof(DateTime));
            dt.Columns.Add("TotalReturns", typeof(int));
            dt.Columns.Add("TotalItems", typeof(int));
            dt.Columns.Add("TotalQty", typeof(decimal));
            dt.Columns.Add("SubTotal", typeof(decimal));
            dt.Columns.Add("TaxAmt", typeof(decimal));
            dt.Columns.Add("GrandTotal", typeof(decimal));
            dt.PrimaryKey = new[] { dt.Columns["PeriodKey"] };
            return dt;
        }

        private static DataTable CreateSalesReturnMasterTable(bool includePeriodKey)
        {
            DataTable dt = new DataTable("SalesReturnMaster");
            if (includePeriodKey)
            {
                dt.Columns.Add("PeriodKey", typeof(string));
            }
            dt.Columns.Add("PeriodLabel", typeof(string));
            dt.Columns.Add("SReturnNo", typeof(int));
            dt.Columns.Add("SReturnDate", typeof(DateTime));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("CustomerName", typeof(string));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("ItemCount", typeof(int));
            dt.Columns.Add("TotalQty", typeof(decimal));
            dt.Columns.Add("SubTotal", typeof(decimal));
            dt.Columns.Add("TaxAmt", typeof(decimal));
            dt.Columns.Add("GrandTotal", typeof(decimal));
            dt.PrimaryKey = new[] { dt.Columns["SReturnNo"] };
            return dt;
        }

        private static DataTable CreateSalesReturnDetailTable()
        {
            DataTable dt = new DataTable("SalesReturnDetail");
            DataColumn idCol = dt.Columns.Add("DetailID", typeof(int));
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = 1;
            idCol.AutoIncrementStep = 1;

            dt.Columns.Add("SReturnNo", typeof(int));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("SalesPrice", typeof(decimal));
            dt.Columns.Add("TaxPer", typeof(decimal));
            dt.Columns.Add("TaxAmt", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("Reason", typeof(string));
            return dt;
        }

        private static DataTable CreateFlatItemsTable(string tableName, bool includePeriodKey)
        {
            DataTable dt = new DataTable(tableName);
            DataColumn idCol = dt.Columns.Add("DetailID", typeof(int));
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = 1;
            idCol.AutoIncrementStep = 1;

            if (includePeriodKey)
            {
                dt.Columns.Add("PeriodKey", typeof(string));
            }
            dt.Columns.Add("PeriodLabel", typeof(string));
            dt.Columns.Add("SReturnNo", typeof(int));
            dt.Columns.Add("SReturnDate", typeof(DateTime));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("CustomerName", typeof(string));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("SalesPrice", typeof(decimal));
            dt.Columns.Add("TaxPer", typeof(decimal));
            dt.Columns.Add("TaxAmt", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("Reason", typeof(string));
            return dt;
        }

        private static void AddSalesReturnMasterRow(DataTable masterTable, FilteredReturnRecord rec, bool includePeriodKey)
        {
            DataRow row = masterTable.NewRow();
            if (includePeriodKey)
            {
                row["PeriodKey"] = rec.PeriodKey;
            }
            row["PeriodLabel"] = rec.PeriodLabel;
            row["SReturnNo"] = rec.Master.SReturnNo;
            row["SReturnDate"] = rec.Master.SReturnDate;
            row["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
            row["InvoiceDate"] = rec.Master.InvoiceDate;
            row["CustomerName"] = rec.Master.CustomerName ?? string.Empty;
            row["Paymode"] = rec.Master.Paymode ?? string.Empty;
            row["ItemCount"] = rec.Details.Count;
            row["TotalQty"] = rec.Details.Sum(d => Convert.ToDecimal(d.Qty));
            row["SubTotal"] = Convert.ToDecimal(rec.Master.SubTotal);
            row["TaxAmt"] = rec.Details.Sum(d => Convert.ToDecimal(d.TaxAmt));
            row["GrandTotal"] = Convert.ToDecimal(rec.Master.GrandTotal);
            masterTable.Rows.Add(row);
        }

        private static void AddSalesReturnDetailRows(DataTable detailTable, FilteredReturnRecord rec)
        {
            foreach (SalesReturnReportDetail detail in rec.Details)
            {
                DataRow row = detailTable.NewRow();
                row["SReturnNo"] = rec.Master.SReturnNo;
                row["SlNo"] = detail.SlNo;
                row["ItemName"] = detail.ItemName ?? string.Empty;
                row["Unit"] = detail.Unit ?? string.Empty;
                row["Packing"] = detail.Packing ?? string.Empty;
                row["Qty"] = Convert.ToDecimal(detail.Qty);
                row["SalesPrice"] = Convert.ToDecimal(detail.SalesPrice);
                row["TaxPer"] = Convert.ToDecimal(detail.TaxPer);
                row["TaxAmt"] = Convert.ToDecimal(detail.TaxAmt);
                row["Amount"] = Convert.ToDecimal(detail.Amount);
                row["Reason"] = detail.Reason ?? string.Empty;
                detailTable.Rows.Add(row);
            }
        }

        private static void AddFlatItemRows(DataTable itemsTable, FilteredReturnRecord rec, bool includePeriodKey)
        {
            foreach (SalesReturnReportDetail detail in rec.Details)
            {
                DataRow row = itemsTable.NewRow();
                if (includePeriodKey)
                {
                    row["PeriodKey"] = rec.PeriodKey;
                }
                row["PeriodLabel"] = rec.PeriodLabel;
                row["SReturnNo"] = rec.Master.SReturnNo;
                row["SReturnDate"] = rec.Master.SReturnDate;
                row["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
                row["InvoiceDate"] = rec.Master.InvoiceDate;
                row["CustomerName"] = rec.Master.CustomerName ?? string.Empty;
                row["Paymode"] = rec.Master.Paymode ?? string.Empty;
                row["SlNo"] = detail.SlNo;
                row["ItemName"] = detail.ItemName ?? string.Empty;
                row["Unit"] = detail.Unit ?? string.Empty;
                row["Packing"] = detail.Packing ?? string.Empty;
                row["Qty"] = Convert.ToDecimal(detail.Qty);
                row["SalesPrice"] = Convert.ToDecimal(detail.SalesPrice);
                row["TaxPer"] = Convert.ToDecimal(detail.TaxPer);
                row["TaxAmt"] = Convert.ToDecimal(detail.TaxAmt);
                row["Amount"] = Convert.ToDecimal(detail.Amount);
                row["Reason"] = detail.Reason ?? string.Empty;
                itemsTable.Rows.Add(row);
            }
        }
        #endregion

        #region Grid Layout & Band Configuration
        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            e.Layout.ViewStyleBand = ViewStyleBand.Vertical;
            e.Layout.AutoFitStyle = AutoFitStyle.None;

            for (int b = 0; b < e.Layout.Bands.Count; b++)
            {
                UltraGridBand band = e.Layout.Bands[b];
                foreach (UltraGridColumn col in band.Columns)
                {
                    col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }
                ConfigureBandByKey(band, b);

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Key == "PeriodKey" || col.Key == "DetailID" || col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        col.Hidden = true;
                        col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                    }
                }
            }

            ReportGridLayoutHelper.ApplySavedLayout("SalesReturnAuditReport", gridReport, userHiddenColumnKeys);

            CreateFooterCells();
            _filterManager?.UpdateHeaderFilterIcons();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ConfigureBandByKey(UltraGridBand band, int bandIndex)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.IsChaptered)
                {
                    col.Hidden = true;
                }
            }

            if (string.Equals(band.Key, "PeriodSummary", StringComparison.OrdinalIgnoreCase))
            {
                string periodHeader = "Period (Month / Day / Year)";
                string mode = GetPeriodMode();
                if (string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
                    periodHeader = "Month & Year";
                else if (string.Equals(mode, "DAY_WISE", StringComparison.OrdinalIgnoreCase))
                    periodHeader = "Return Day";
                else if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
                    periodHeader = "Year";

                int pos = 0;
                ConfigureGridColumn(band, bandIndex, "PeriodLabel", periodHeader, 190, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "FromDate", "First Return", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ToDate", "Last Return", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalReturns", "Total Returns", 90, "#,##0", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalItems", "Line Items", 90, "#,##0", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalQty", "Total Qty", 100, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "SubTotal", "Sub Total", 120, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TaxAmt", "Tax Amt", 110, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "GrandTotal", "Grand Total", 125, "#,##0.00", HAlign.Right, pos++);

                if (band.Columns.Exists("PeriodLabel"))
                {
                    band.Columns["PeriodLabel"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["PeriodLabel"].CellAppearance.ForeColor = Color.FromArgb(18, 49, 102);
                }
                if (band.Columns.Exists("GrandTotal"))
                {
                    band.Columns["GrandTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["GrandTotal"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                }
            }
            else if (string.Equals(band.Key, "SalesReturnMaster", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "PeriodReturns", StringComparison.OrdinalIgnoreCase))
            {
                if (bandIndex > 0)
                {
                    band.HeaderVisible = true;
                    band.Header.Caption = "Sales Return Invoices";
                    StyleChildBandHeader(band, Color.FromArgb(52, 116, 180));
                }

                int pos = 0;
                if (bandIndex == 0)
                {
                    ConfigureGridColumn(band, bandIndex, "PeriodLabel", "Period", 145, null, HAlign.Left, pos++);
                }
                ConfigureGridColumn(band, bandIndex, "SReturnNo", "Return No", 95, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "SReturnDate", "Return Date", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "InvoiceNo", "Invoice No", 105, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "InvoiceDate", "Invoice Date", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "CustomerName", "Customer Name", 210, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Paymode", "Payment Mode", 105, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ItemCount", "Items", 65, "#,##0", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalQty", "Total Qty", 90, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "SubTotal", "Sub Total", 110, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TaxAmt", "Tax Amt", 100, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "GrandTotal", "Grand Total", 115, "#,##0.00", HAlign.Right, pos++);

                if (band.Columns.Exists("SReturnNo"))
                {
                    band.Columns["SReturnNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["SReturnNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                }
                if (band.Columns.Exists("GrandTotal"))
                {
                    band.Columns["GrandTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["GrandTotal"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                }

                if (bandIndex > 0)
                {
                    ConfigureChildBandSummaries(band, new[] { "TotalQty", "SubTotal", "TaxAmt", "GrandTotal" });
                }
            }
            else if (string.Equals(band.Key, "SalesReturnDetail", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "ReturnItems", StringComparison.OrdinalIgnoreCase))
            {
                band.HeaderVisible = true;
                band.Header.Caption = "Returned Item Details";
                StyleChildBandHeader(band, Color.FromArgb(41, 128, 185));

                int pos = 0;
                ConfigureGridColumn(band, bandIndex, "SlNo", "S.No", 55, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ItemName", "Item Name", 240, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Unit", "Unit", 70, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Packing", "Packing", 85, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Qty", "Quantity", 90, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "SalesPrice", "Unit Price", 100, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TaxPer", "Tax %", 75, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TaxAmt", "Tax Amt", 95, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Amount", "Total Amount", 120, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Reason", "Reason", 130, null, HAlign.Left, pos++);

                if (band.Columns.Exists("ItemName"))
                {
                    band.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("Qty"))
                {
                    band.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("Amount"))
                {
                    band.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                }

                ConfigureChildBandSummaries(band, new[] { "Qty", "TaxAmt", "Amount" });
            }
            else if (string.Equals(band.Key, "PeriodItems", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "PeriodAllItems", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "ItemsFlat", StringComparison.OrdinalIgnoreCase))
            {
                if (bandIndex > 0)
                {
                    band.HeaderVisible = true;
                    band.Header.Caption = "All Returned Items in Period";
                    StyleChildBandHeader(band, Color.FromArgb(41, 128, 185));
                }

                int pos = 0;
                if (bandIndex == 0)
                {
                    ConfigureGridColumn(band, bandIndex, "PeriodLabel", "Period", 140, null, HAlign.Left, pos++);
                }
                ConfigureGridColumn(band, bandIndex, "SReturnNo", "Return No", 90, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "SReturnDate", "Return Date", 100, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "InvoiceNo", "Invoice No", 95, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "CustomerName", "Customer", 185, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Paymode", "Pay Mode", 90, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "SlNo", "S.No", 50, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ItemName", "Item Name", 220, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Unit", "Unit", 65, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Packing", "Packing", 75, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Qty", "Quantity", 85, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "SalesPrice", "Unit Price", 95, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TaxPer", "Tax %", 70, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TaxAmt", "Tax Amt", 90, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Amount", "Total Amount", 115, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Reason", "Reason", 120, null, HAlign.Left, pos++);

                if (band.Columns.Exists("SReturnNo"))
                {
                    band.Columns["SReturnNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["SReturnNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                }
                if (band.Columns.Exists("ItemName"))
                {
                    band.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("Amount"))
                {
                    band.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                }

                if (bandIndex > 0)
                {
                    ConfigureChildBandSummaries(band, new[] { "Qty", "TaxAmt", "Amount" });
                }
            }
        }

        private static void StyleChildBandHeader(UltraGridBand band, Color headerBackColor)
        {
            band.Header.Appearance.BackColor = headerBackColor;
            band.Header.Appearance.ForeColor = Color.White;
            band.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
            band.Override.RowAppearance.BackColor = Color.FromArgb(252, 253, 255);
            band.Override.RowAlternateAppearance.BackColor = Color.FromArgb(242, 248, 255);
        }

        private static void ConfigureChildBandSummaries(UltraGridBand band, IEnumerable<string> columnKeys)
        {
            band.Summaries.Clear();
            band.SummaryFooterCaption = "Totals:";
            band.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;
            band.Override.SummaryFooterCaptionVisible = DefaultableBoolean.True;
            band.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(226, 238, 252);
            band.Override.SummaryFooterAppearance.ForeColor = ControlTextColor;
            band.Override.SummaryFooterAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Override.SummaryFooterAppearance.BorderColor = BorderBlue;

            foreach (string key in columnKeys)
            {
                if (!band.Columns.Exists(key)) continue;
                string summaryKey = $"Sum_{band.Key}_{key}";
                SummarySettings sum = band.Summaries.Add(summaryKey, SummaryType.Sum, band.Columns[key], SummaryPosition.UseSummaryPositionColumn);
                sum.DisplayFormat = "{0:#,##0.00}";
                sum.Appearance.TextHAlign = HAlign.Right;
                sum.Appearance.BackColor = Color.FromArgb(226, 238, 252);
                sum.Appearance.ForeColor = ControlTextColor;
                sum.Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void ConfigureGridColumn(UltraGridBand band, int bandIndex, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
            column.Hidden = userHiddenColumnKeys.Contains($"{bandIndex}:{key}") || userHiddenColumnKeys.Contains(key);
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BorderColor = GridRowLine;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("GrandTotal"))
            {
                e.Row.Cells["GrandTotal"].Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Cells["GrandTotal"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            }
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (e.Row.ChildBands != null && e.Row.ChildBands.Count > 0)
            {
                e.Row.Expanded = !e.Row.Expanded;
            }
            else
            {
                int returnNo = 0;
                if (e.Row.Cells.Exists("SReturnNo") && e.Row.Cells["SReturnNo"].Value != null)
                {
                    int.TryParse(Convert.ToString(e.Row.Cells["SReturnNo"].Value), out returnNo);
                }

                if (returnNo > 0)
                {
                    OpenSalesReturnTransaction(returnNo);
                }
            }
        }

        private void OpenSalesReturnTransaction(int returnNo)
        {
            try
            {
                Home homeInstance = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeInstance != null && !homeInstance.IsDisposed)
                {
                    homeInstance.OpenFormByType(typeof(frmSalesReturn), "Sales Return");
                }
                else
                {
                    frmSalesReturn form = new frmSalesReturn();
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open Sales Return for Return No: {returnNo}.\n{ex.Message}", "Sales Return",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Button & Control Event Handlers
        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            ShowReportPreview("SALES RETURN AUDIT REPORT");
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(
                "SALES RETURN AUDIT REPORT",
                new[]
                {
                    "SALES RETURN AUDIT REPORT - PERIOD -> SALES RETURNS -> ITEMS",
                    "SALES RETURN AUDIT REPORT - RETURN WISE & ITEM DETAILS",
                    "SALES RETURN AUDIT REPORT - PERIOD -> ALL RETURNED ITEMS",
                    "SALES RETURN AUDIT REPORT - ALL RETURNED ITEMS (FLAT)"
                }))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string selectedDesc = dialog.SelectedFormatDescription ?? string.Empty;
                    if (selectedDesc.IndexOf("RETURN WISE", StringComparison.OrdinalIgnoreCase) >= 0)
                        ultraComboViewMode.Value = "RETURNS_ITEMS";
                    else if (selectedDesc.IndexOf("ALL RETURNED ITEMS (FLAT)", StringComparison.OrdinalIgnoreCase) >= 0)
                        ultraComboViewMode.Value = "ITEMS_FLAT";
                    else if (selectedDesc.IndexOf("PERIOD -> ALL RETURNED ITEMS", StringComparison.OrdinalIgnoreCase) >= 0)
                        ultraComboViewMode.Value = "PERIOD_ITEMS";
                    else
                        ultraComboViewMode.Value = "PERIOD_RETURNS_ITEMS";

                    if (string.Equals(dialog.SelectedAction, "print", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(dialog.SelectedAction, "printer", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintReport();
                    }
                    else
                    {
                        ShowReportPreview("SALES RETURN AUDIT REPORT - " + dialog.SelectedFormatDescription);
                    }
                }
            }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void ResetFilters()
        {
            _isLoading = true;
            try
            {
                ultraComboPeriodMode.Value = "MONTH_WISE";
                ultraComboMonth.Value = DateTime.Today.Month;
                ultraComboYear.Value = DateTime.Today.Year;
                ultraComboViewMode.Value = "PERIOD_RETURNS_ITEMS";
                ultraComboPayMode.Value = "ALL";
                txtCustomerSearch.Text = string.Empty;
                txtReturnNo.Text = string.Empty;
                txtItemFilter.Text = string.Empty;
                dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtTo.Value = DateTime.Today;
                chkSingleDay.Checked = false;
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            gridReport.Rows.ExpandAll(true);
            UpdateFooterCellPositions();
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            gridReport.Rows.CollapseAll(true);
            UpdateFooterCellPositions();
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void ultraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            UpdatePeriodControlVisibility();
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void chkSingleDay_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePeriodControlVisibility();
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void FilterSelection_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                BuildAndBindReportData();
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                BuildAndBindReportData();
            }
        }

        private void salesreturnauditreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                LoadReport();
            }
            else if (e.KeyCode == Keys.F11)
            {
                e.Handled = true;
                if (txtItemFilter.Focused)
                {
                    OpenItemLookupDialog();
                }
                else
                {
                    OpenCustomerDialog();
                }
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                e.Handled = true;
                ExportCsv();
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                e.Handled = true;
                ShowReportPreview("SALES RETURN AUDIT REPORT");
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void btnCustomerPicker_Click(object sender, EventArgs e)
        {
            OpenCustomerDialog();
        }

        private void OpenCustomerDialog()
        {
            try
            {
                using (frmCustomerDialog dlg = new frmCustomerDialog())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedCustomerId > 0)
                    {
                        txtCustomerSearch.Text = dlg.SelectedCustomerName;
                        BuildAndBindReportData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening customer lookup: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnItemPicker_Click(object sender, EventArgs e)
        {
            OpenItemLookupDialog();
        }

        private void OpenItemLookupDialog()
        {
            try
            {
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("salesreturnauditreport"))
                {
                    if (itemDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        var data = itemDialog.GetSelectedItemData();
                        string itemName = string.Empty;
                        if (data != null)
                        {
                            if (data.ContainsKey("Description") && data["Description"] != null)
                                itemName = Convert.ToString(data["Description"]);
                            else if (data.ContainsKey("ItemName") && data["ItemName"] != null)
                                itemName = Convert.ToString(data["ItemName"]);
                        }

                        if (!string.IsNullOrWhiteSpace(itemName))
                        {
                            txtItemFilter.Text = itemName;
                            BuildAndBindReportData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening item lookup: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Preview, Print & Export
        private void ShowReportPreview(string title)
        {
            List<FilteredReturnRecord> records = GetFilteredReturnRecords();
            if (records.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (Panel footer = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Sales Return Audit Report - Preview";
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.WindowState = FormWindowState.Maximized;
                preview.MinimumSize = new Size(1024, 600);
                preview.BackColor = FormBackColor;
                preview.Padding = new Padding(10);

                header.Dock = DockStyle.Top;
                header.Height = 72;
                header.BackColor = GridHeaderBlueDark;
                header.Padding = new Padding(18, 10, 18, 8);

                Label titleLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    Text = string.IsNullOrWhiteSpace(title) ? "SALES RETURN AUDIT REPORT" : title,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = BuildPreviewSubtitle(),
                    ForeColor = Color.FromArgb(224, 238, 252),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(subtitleLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.BackColor = Color.White;
                previewGrid.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                previewGrid.InitializeLayout += PreviewGrid_InitializeLayout;
                previewGrid.InitializeRow += gridReport_InitializeRow;
                previewGrid.DataSource = _currentDataSet;
                previewGrid.DataMember = gridReport.DataMember;

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                int totalReturns = records.Count;
                int totalItems = records.Sum(r => r.Details.Count);
                decimal totalQty = records.Sum(r => r.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                decimal grandTotal = records.Sum(r => Convert.ToDecimal(r.Master.GrandTotal));

                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Returns: {0:N0}    |    Items: {1:N0}    |    Total Qty: {2:N2}    |    Grand Total: {3:N2}",
                        totalReturns, totalItems, totalQty, grandTotal),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight
                };
                footer.Controls.Add(footerLabel);

                preview.Controls.Add(previewGrid);
                preview.Controls.Add(footer);
                preview.Controls.Add(header);
                preview.ShowDialog(this);
            }
        }

        private string BuildPreviewSubtitle()
        {
            string modeText = ultraComboPeriodMode.Text;
            string periodSelection = "All";
            string mode = GetPeriodMode();

            if (string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
            {
                periodSelection = $"{ultraComboMonth.Text} / {ultraComboYear.Text}";
            }
            else if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                periodSelection = ultraComboYear.Text;
            }
            else if (string.Equals(mode, "DAY_WISE", StringComparison.OrdinalIgnoreCase))
            {
                periodSelection = chkSingleDay.Checked
                    ? $"{Convert.ToDateTime(dtFrom.Value):dd-MMM-yyyy}"
                    : $"{Convert.ToDateTime(dtFrom.Value):dd-MMM-yyyy} to {Convert.ToDateTime(dtTo.Value):dd-MMM-yyyy}";
            }
            else if (string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase))
            {
                periodSelection = $"{Convert.ToDateTime(dtFrom.Value):dd-MMM-yyyy} to {Convert.ToDateTime(dtTo.Value):dd-MMM-yyyy}";
            }

            string customer = string.IsNullOrWhiteSpace(txtCustomerSearch.Text) ? "ALL" : txtCustomerSearch.Text.Trim();
            string item = string.IsNullOrWhiteSpace(txtItemFilter.Text) ? "ALL" : txtItemFilter.Text.Trim();
            string paymode = Convert.ToString(ultraComboPayMode.Value ?? "ALL");

            return string.Format("Mode: {0} ({1})    |    Customer: {2}    |    Pay: {3}    |    Item: {4}    |    View: {5}",
                modeText, periodSelection, customer, paymode, item, ultraComboViewMode.Text);
        }

        private void PreviewGrid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            gridReport_InitializeLayout(sender, e);
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.Override.RowSelectors = DefaultableBoolean.False;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            e.Layout.Override.DefaultRowHeight = 23;
            e.Layout.Override.MinRowHeight = 23;
            e.Layout.Override.CellAppearance.FontData.SizeInPoints = 9;
            e.Layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            e.Layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            e.Layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            e.Layout.Override.CellAppearance.BorderColor = GridRowLine;
            e.Layout.Override.RowAppearance.BorderColor = GridRowLine;
            e.Layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
        }

        private void PrintReport()
        {
            List<FilteredReturnRecord> records = GetFilteredReturnRecords();
            if (records.Count == 0)
            {
                MessageBox.Show("No data available to print.", "Print",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (PrintDialog printDialog = new PrintDialog())
                using (PrintDocument printDocument = new PrintDocument())
                {
                    printDocument.DocumentName = "Sales Return Audit Report";
                    int printIndex = 0;

                    printDocument.PrintPage += (s, e) =>
                    {
                        Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                        Font subFont = new Font("Arial", 9, FontStyle.Regular);
                        Font headerFont = new Font("Arial", 9, FontStyle.Bold);
                        Font dataFont = new Font("Arial", 8.5f, FontStyle.Regular);

                        float y = 40;
                        float left = 40;
                        float right = e.MarginBounds.Right;

                        e.Graphics.DrawString("SALES RETURN AUDIT REPORT", titleFont, Brushes.Black, left, y);
                        y += 24;
                        e.Graphics.DrawString(BuildPreviewSubtitle(), subFont, Brushes.DimGray, left, y);
                        y += 24;

                        string[] headers = { "Period", "Return No", "Date", "Customer", "Items", "Qty", "Grand Total" };
                        float[] widths = { 110, 70, 75, 170, 50, 65, 100 };
                        float x = left;
                        for (int i = 0; i < headers.Length; i++)
                        {
                            e.Graphics.DrawString(headers[i], headerFont, Brushes.Black, x, y);
                            x += widths[i];
                        }
                        y += 18;
                        e.Graphics.DrawLine(Pens.Black, left, y, right, y);
                        y += 6;

                        while (printIndex < records.Count)
                        {
                            if (y > e.MarginBounds.Bottom - 60)
                            {
                                e.HasMorePages = true;
                                return;
                            }

                            FilteredReturnRecord rec = records[printIndex++];
                            x = left;
                            string[] vals =
                            {
                                rec.PeriodLabel,
                                rec.Master.SReturnNo.ToString(),
                                rec.Master.SReturnDate.ToString("dd/MM/yyyy"),
                                rec.Master.CustomerName ?? string.Empty,
                                rec.Details.Count.ToString(),
                                rec.Details.Sum(d => d.Qty).ToString("N2"),
                                rec.Master.GrandTotal.ToString("N2")
                            };

                            for (int i = 0; i < vals.Length; i++)
                            {
                                e.Graphics.DrawString(vals[i], dataFont, Brushes.Black, x, y);
                                x += widths[i];
                            }
                            y += 18;
                        }

                        e.HasMorePages = false;
                    };

                    printDialog.Document = printDocument;
                    if (printDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        printDocument.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing report: " + ex.Message, "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCsv()
        {
            List<FilteredReturnRecord> records = GetFilteredReturnRecords();
            if (records.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                dialog.FileName = string.Format("SalesReturnAuditReport_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Sales Return Audit Report", BuildPreviewSubtitle())) return;
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Period,ReturnNo,ReturnDate,InvoiceNo,InvoiceDate,Customer,Paymode,SlNo,ItemName,Unit,Packing,Qty,SalesPrice,TaxPer,TaxAmt,ItemAmount,Reason,ReturnSubTotal,ReturnGrandTotal");

                foreach (FilteredReturnRecord rec in records)
                {
                    if (rec.Details.Count > 0)
                    {
                        foreach (SalesReturnReportDetail item in rec.Details)
                        {
                            builder.AppendFormat(
                                "\"{0}\",{1},\"{2:dd/MM/yyyy}\",\"{3}\",\"{4:dd/MM/yyyy}\",\"{5}\",\"{6}\",{7},\"{8}\",\"{9}\",\"{10}\",{11},{12},{13},{14},{15},\"{16}\",{17},{18}\r\n",
                                EscapeCsv(rec.PeriodLabel),
                                rec.Master.SReturnNo,
                                rec.Master.SReturnDate,
                                EscapeCsv(rec.Master.InvoiceNo ?? string.Empty),
                                rec.Master.InvoiceDate,
                                EscapeCsv(rec.Master.CustomerName ?? string.Empty),
                                EscapeCsv(rec.Master.Paymode ?? string.Empty),
                                item.SlNo,
                                EscapeCsv(item.ItemName ?? string.Empty),
                                EscapeCsv(item.Unit ?? string.Empty),
                                EscapeCsv(item.Packing ?? string.Empty),
                                item.Qty,
                                item.SalesPrice,
                                item.TaxPer,
                                item.TaxAmt,
                                item.Amount,
                                EscapeCsv(item.Reason ?? string.Empty),
                                rec.Master.SubTotal,
                                rec.Master.GrandTotal
                            );
                        }
                    }
                    else
                    {
                        builder.AppendFormat(
                            "\"{0}\",{1},\"{2:dd/MM/yyyy}\",\"{3}\",\"{4:dd/MM/yyyy}\",\"{5}\",\"{6}\",0,\"\",\"\",\"\",0,0,0,0,0,\"\",{7},{8}\r\n",
                            EscapeCsv(rec.PeriodLabel),
                            rec.Master.SReturnNo,
                            rec.Master.SReturnDate,
                            EscapeCsv(rec.Master.InvoiceNo ?? string.Empty),
                            rec.Master.InvoiceDate,
                            EscapeCsv(rec.Master.CustomerName ?? string.Empty),
                            EscapeCsv(rec.Master.Paymode ?? string.Empty),
                            rec.Master.SubTotal,
                            rec.Master.GrandTotal
                        );
                    }
                }

                try
                {
                    File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                    MessageBox.Show("Sales Return Audit Report exported successfully!\nFile: " + dialog.FileName,
                        "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save export file: " + ex.Message, "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string val)
        {
            if (string.IsNullOrEmpty(val)) return string.Empty;
            return val.Replace("\"", "\"\"");
        }
        #endregion
    }
}
