using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports.FinancialReports;
using Repository;
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

namespace PosBranch_Win.Reports.PurchaseReports
{
    /// <summary>
    /// Purchase Audit Report (frmpurchaseAuditreport)
    /// Displays all purchases and their item details Month-wise, Day-wise, and Year-wise,
    /// styled with the exact theme, gridReport, and ultraPanelGridFooter of frmVendorOutstandingReport.
    /// </summary>
    public partial class frmpurchaseAuditreport : Form
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
        private readonly PurchaseReportRepository _purchaseRepository;
        private readonly Dictionary<int, List<PurchaseReportDetail>> _detailsCache;
        private List<PurchaseReportMaster> _loadedBills;
        private DataSet _currentDataSet;
        private bool _isLoading;

        // Dynamic footer panel and cell controls (matching frmVendorOutstandingReport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalBills",
            "TotalItems",
            "ItemCount",
            "TotalQty",
            "TotalFreeQty",
            "Qty",
            "Free",
            "Cost",
            "Amount",
            "SubTotal",
            "GrandTotal",
            "PayedAmount",
            "BalanceAmount"
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

        private sealed class FilteredPurchaseRecord
        {
            public PurchaseReportMaster Master { get; set; }
            public List<PurchaseReportDetail> Details { get; set; }
            public string PeriodKey { get; set; }
            public string PeriodLabel { get; set; }
            public DateTime PeriodSortDate { get; set; }
        }
        #endregion

        #region Constructor & Form Lifecycle
        public frmpurchaseAuditreport()
        {
            _purchaseRepository = new PurchaseReportRepository();
            _detailsCache = new Dictionary<int, List<PurchaseReportDetail>>();
            _loadedBills = new List<PurchaseReportMaster>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmpurchaseAuditreport_Load;
            FormClosing += frmpurchaseAuditreport_FormClosing;
            KeyPreview = true;
            KeyDown += frmpurchaseAuditreport_KeyDown;

            // Button events
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            btnVendorPicker.Click += btnVendorPicker_Click;
            btnItemPicker.Click += btnItemPicker_Click;

            // Filter change events
            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboMonth.ValueChanged += FilterSelection_ValueChanged;
            ultraComboYear.ValueChanged += FilterSelection_ValueChanged;
            ultraComboViewMode.ValueChanged += ultraComboViewMode_ValueChanged;
            chkSingleDay.CheckedChanged += chkSingleDay_CheckedChanged;
            dtFrom.ValueChanged += dtFrom_ValueChanged;
            dtTo.ValueChanged += dtTo_ValueChanged;
            txtVendorSearch.KeyDown += txtVendorSearch_KeyDown;
            txtPurchaseNo.KeyDown += FilterTextBox_KeyDown;
            txtItemFilter.KeyDown += txtItemFilter_KeyDown;

            // Grid events for layout, row styling, footer sync, and drag-to-hide
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            gridReport.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Resize += (s, e) => UpdateFooterCellPositions();
            }
        }

        private void frmpurchaseAuditreport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmpurchaseAuditreport_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                Text = "Purchase Audit Report";
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
                LoadVendors();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
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
            ultraComboViewMode.Items.Add("PERIOD_BILLS_ITEMS", "Period -> Purchases -> Item Details");
            ultraComboViewMode.Items.Add("BILLS_ITEMS", "Purchases & Item Details (Bill Wise)");
            ultraComboViewMode.Items.Add("PERIOD_ITEMS", "Period -> All Purchased Items");
            ultraComboViewMode.Items.Add("ITEMS_FLAT", "All Purchased Items (Flat List)");
            ultraComboViewMode.Value = "PERIOD_BILLS_ITEMS";

            txtVendorSearch.Text = string.Empty;
            txtPurchaseNo.Text = string.Empty;
            txtPurchaseNo.NullText = "Purchase / Inv No...";
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

        private void LoadVendors()
        {
            try
            {
                txtVendorSearch.Items.Clear();
                txtVendorSearch.Items.Add(string.Empty, "ALL Vendors");

                VendorRepository vendorRepository = new VendorRepository();
                VendorDDLGrid data = vendorRepository.GetVendorDDL();
                if (data?.List != null)
                {
                    foreach (VendorGridList vendor in data.List.Where(x => x != null && !string.IsNullOrWhiteSpace(x.LedgerName)).OrderBy(x => x.LedgerName))
                    {
                        txtVendorSearch.Items.Add(vendor.LedgerName, vendor.LedgerName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading vendor list: " + ex.Message);
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
            StyleLabel(lblVendor);
            StyleLabel(lblDocNo);
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
            StylePickerButton(btnVendorPicker);
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
            StyleFilterCombo(txtVendorSearch, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleCheckEditor(chkSingleDay);
            StyleTextEditor(txtPurchaseNo);
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
                gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numericValue = GetNumericValue(result);
                if (numericValue.HasValue)
                {
                    string formatted;
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        formatted = numericValue.Value.ToString(column.Format);
                    else
                        formatted = numericValue.Value.ToString("N2");

                    if (aggregation == "Avg") return "Avg: " + formatted;
                    if (aggregation == "Min") return "Min: " + formatted;
                    if (aggregation == "Max") return "Max: " + formatted;
                    return formatted;
                }
            }

            return Convert.ToString(result);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null) yield break;
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                {
                    yield return row;
                }
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null &&
                   value != DBNull.Value &&
                   !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            decimal result;
            string clean = Convert.ToString(value).Replace("₹", "").Trim();
            return decimal.TryParse(clean, out result) ? result : (decimal?)null;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.IsChaptered) return false;
            if (summaryDefaultNumericColumns.Contains(column.Key)) return true;

            Type type = column.DataType != null ? (System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType) : null;
            return type == typeof(decimal) ||
                   type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(short);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser (Parity with frmVendorOutstandingReport)
        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillEllipse(bgBrush, 4, 4, 24, 24);
                    }

                    using (Pen whitePen = new Pen(Color.White, 3.5f))
                    {
                        whitePen.StartCap = LineCap.Round;
                        whitePen.EndCap = LineCap.Round;
                        g.DrawLine(whitePen, 11, 11, 21, 21);
                        g.DrawLine(whitePen, 21, 11, 11, 21);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    return new Cursor(hIcon);
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private bool IsCustomizableColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            if (string.Equals(col.Key, "PeriodKey", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase)) return false;
            if (col.Band != null && col.Band.Index > 0 && string.Equals(col.Key, "PurchaseNo", StringComparison.OrdinalIgnoreCase) && col.Band.Key == "PurchaseDetail")
                return false;
            return true;
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.AllowDrop = true;
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null && IsCustomizableColumn(headerUI.Header.Column))
                {
                    ShowHeaderContextMenu(headerUI.Header.Column, pt);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null && IsCustomizableColumn(headerUI.Header.Column))
                {
                    columnBeingDragged = headerUI.Header.Column;
                    headerDragStartPoint = pt;
                    isDraggingHeaderToHide = false;
                }
                else
                {
                    columnBeingDragged = null;
                }
            }
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                int deltaY = e.Y - headerDragStartPoint.Y;
                int deltaX = Math.Abs(e.X - headerDragStartPoint.X);

                if (deltaY > 12 && deltaY > deltaX)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                    headerToolTip.Show("Drag down to hide column", gridReport, e.X + 15, e.Y + 15, 500);
                }
                else
                {
                    isDraggingHeaderToHide = false;
                    Cursor.Current = Cursors.Default;
                    headerToolTip.Hide(gridReport);
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                if (isDraggingHeaderToHide)
                {
                    HideColumn(columnBeingDragged);
                    headerToolTip.Hide(gridReport);
                }
                columnBeingDragged = null;
                isDraggingHeaderToHide = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void GridReport_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void GridReport_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = gridReport.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt, item.BandIndex);
                UnhideColumn(item.BandIndex, item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt, int bandIndex)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count <= bandIndex)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null && headerUI.Header.Column.Band.Index == bandIndex)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[bandIndex];
            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null || !IsCustomizableColumn(col)) return;
            int bandIndex = col.Band != null ? col.Band.Index : 0;
            userHiddenColumnKeys.Add($"{bandIndex}:{col.Key}");
            col.Hidden = true;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point location)
        {
            if (col == null) return;
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col))
            {
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(gridReport, location);
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooser();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization (Field Chooser)",
                Size = new Size(240, 320),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 244, 248),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                Height = 25
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 34,
                IntegralHeight = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248)
            };
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(4, 4, 4, 4),
                BackColor = Color.FromArgb(232, 240, 250)
            };
            searchPanel.Controls.Add(txtColumnSearch);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(searchPanel);
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            string filter = txtColumnSearch?.Text?.Trim() ?? string.Empty;

            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[b];
                string prefix = gridReport.DisplayLayout.Bands.Count > 1 && b > 0 ? $"[Band {b + 1}] " : string.Empty;

                foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.Caption))
                {
                    if (col.Hidden && IsCustomizableColumn(col))
                    {
                        string caption = prefix + (!string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key);
                        if (string.IsNullOrEmpty(filter) || caption.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption, b));
                        }
                    }
                }
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index >= 0 && index < columnChooserListBox.Items.Count)
            {
                if (columnChooserListBox.Items[index] is ColumnChooserItem item)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox?.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.BandIndex, item.ColumnKey, -1);
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                return;

            if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                return;

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color badgeColor = Color.FromArgb(0, 121, 211);

            using (SolidBrush bgBrush = new SolidBrush(badgeColor))
            using (GraphicsPath path = RoundedRect(rect, 4))
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
                UltraGridColumn col = band.Columns[columnKey];
                col.Hidden = false;
                if (targetPosition >= 0)
                {
                    col.Header.VisiblePosition = targetPosition;
                }
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
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
                    if (IsCustomizableColumn(col))
                    {
                        col.Hidden = false;
                    }
                }
            }
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
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
            return Convert.ToString(ultraComboViewMode.Value ?? "PERIOD_BILLS_ITEMS");
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
                _loadedBills = _purchaseRepository.GetPurchaseBills(fromDate, toDate, branchId) ?? new List<PurchaseReportMaster>();

                // Ensure any new years in database are available in ultraComboYear
                if (_loadedBills.Count > 0)
                {
                    int maxBillYear = _loadedBills.Max(b => b.PurchaseDate.Year);
                    if (maxBillYear > DateTime.Today.Year)
                    {
                        PopulateYearCombo(maxBillYear);
                    }
                }

                BuildAndBindReportData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Purchase Audit Report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Cursor = previousCursor;
            }
        }

        private List<PurchaseReportDetail> GetDetailsForPurchase(int purchaseNo)
        {
            if (purchaseNo <= 0)
                return new List<PurchaseReportDetail>();

            if (_detailsCache.TryGetValue(purchaseNo, out List<PurchaseReportDetail> cached))
                return cached;

            PurchaseReportData data = _purchaseRepository.GetPurchaseReportDetails(purchaseNo);
            List<PurchaseReportDetail> details = data?.Details ?? new List<PurchaseReportDetail>();
            _detailsCache[purchaseNo] = details;
            return details;
        }

        private List<FilteredPurchaseRecord> GetFilteredPurchaseRecords()
        {
            string periodMode = GetPeriodMode();
            int selectedMonth = GetSelectedMonth();
            int selectedYear = GetSelectedYear();
            string vendorFilter = (txtVendorSearch.Text ?? string.Empty).Trim();
            if (string.Equals(vendorFilter, "ALL Vendors", StringComparison.OrdinalIgnoreCase))
                vendorFilter = string.Empty;

            string docNoFilter = (txtPurchaseNo.Text ?? string.Empty).Trim();
            string itemFilter = (txtItemFilter.Text ?? string.Empty).Trim();

            List<FilteredPurchaseRecord> records = new List<FilteredPurchaseRecord>();

            foreach (PurchaseReportMaster bill in _loadedBills)
            {
                if (bill == null || bill.PurchaseNo <= 0)
                    continue;

                // Month filter when "ALL Years" + specific Month is selected
                if (string.Equals(periodMode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
                {
                    if (selectedMonth > 0 && bill.PurchaseDate.Month != selectedMonth)
                        continue;
                    if (selectedYear > 0 && bill.PurchaseDate.Year != selectedYear)
                        continue;
                }
                else if (string.Equals(periodMode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
                {
                    if (selectedYear > 0 && bill.PurchaseDate.Year != selectedYear)
                        continue;
                }

                // Vendor filter
                if (!string.IsNullOrEmpty(vendorFilter))
                {
                    if (string.IsNullOrEmpty(bill.VendorName) ||
                        bill.VendorName.IndexOf(vendorFilter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                // Purchase No / Invoice No filter
                if (!string.IsNullOrEmpty(docNoFilter))
                {
                    bool matchPurchaseNo = bill.PurchaseNo.ToString().IndexOf(docNoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchInvoiceNo = !string.IsNullOrEmpty(bill.InvoiceNo) &&
                                          bill.InvoiceNo.IndexOf(docNoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchPurchaseNo && !matchInvoiceNo)
                        continue;
                }

                List<PurchaseReportDetail> allDetails = GetDetailsForPurchase(bill.PurchaseNo);
                List<PurchaseReportDetail> matchingDetails = allDetails;

                if (!string.IsNullOrEmpty(itemFilter))
                {
                    matchingDetails = allDetails
                        .Where(d => (!string.IsNullOrEmpty(d.ItemName) && d.ItemName.IndexOf(itemFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(d.BarCode) && d.BarCode.IndexOf(itemFilter, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();

                    if (matchingDetails.Count == 0)
                        continue;
                }

                GetPeriodKeyAndLabel(bill.PurchaseDate, periodMode, out string periodKey, out string periodLabel, out DateTime periodSortDate);

                records.Add(new FilteredPurchaseRecord
                {
                    Master = bill,
                    Details = matchingDetails,
                    PeriodKey = periodKey,
                    PeriodLabel = periodLabel,
                    PeriodSortDate = periodSortDate
                });
            }

            return records
                .OrderByDescending(r => r.Master.PurchaseDate)
                .ThenByDescending(r => r.Master.PurchaseNo)
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
            List<FilteredPurchaseRecord> records = GetFilteredPurchaseRecords();
            string viewMode = GetViewMode();

            gridReport.DataSource = null;
            gridReport.DataMember = null;

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _currentDataSet = null;
            }

            if (string.Equals(viewMode, "PERIOD_BILLS_ITEMS", StringComparison.OrdinalIgnoreCase))
            {
                _currentDataSet = BuildPeriodPurchasesItemsDataSet(records);
                gridReport.DataSource = _currentDataSet;
                gridReport.DataMember = "PeriodSummary";
            }
            else if (string.Equals(viewMode, "BILLS_ITEMS", StringComparison.OrdinalIgnoreCase))
            {
                _currentDataSet = BuildPurchasesItemsDataSet(records);
                gridReport.DataSource = _currentDataSet;
                gridReport.DataMember = "PurchaseMaster";
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
            // auto-expand it so the user immediately sees all purchases and details of that selected period!
            if (gridReport.Rows != null && gridReport.Rows.Count == 1 &&
                (string.Equals(viewMode, "PERIOD_BILLS_ITEMS", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(viewMode, "PERIOD_ITEMS", StringComparison.OrdinalIgnoreCase)))
            {
                gridReport.Rows[0].Expanded = true;
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private DataSet BuildPeriodPurchasesItemsDataSet(List<FilteredPurchaseRecord> records)
        {
            DataSet ds = new DataSet("PurchaseAudit_PeriodBillsItems");

            DataTable periodTable = CreatePeriodSummaryTable();
            DataTable masterTable = CreatePurchaseMasterTable(includePeriodKey: true);
            DataTable detailTable = CreatePurchaseDetailTable();

            var periodGroups = records
                .GroupBy(r => new { r.PeriodKey, r.PeriodLabel, r.PeriodSortDate })
                .OrderByDescending(g => g.Key.PeriodSortDate);

            foreach (var group in periodGroups)
            {
                DataRow pRow = periodTable.NewRow();
                pRow["PeriodKey"] = group.Key.PeriodKey;
                pRow["PeriodLabel"] = group.Key.PeriodLabel;
                pRow["FromDate"] = group.Min(x => x.Master.PurchaseDate.Date);
                pRow["ToDate"] = group.Max(x => x.Master.PurchaseDate.Date);
                pRow["TotalBills"] = group.Count();
                pRow["TotalItems"] = group.Sum(x => x.Details.Count);
                pRow["TotalQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                pRow["TotalFreeQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Free)));
                decimal subTotal = group.Sum(x => Convert.ToDecimal(x.Master.SubTotal));
                decimal grandTotal = group.Sum(x => Convert.ToDecimal(x.Master.GrandTotal));
                decimal payed = group.Sum(x => Convert.ToDecimal(x.Master.PayedAmount));
                pRow["SubTotal"] = subTotal;
                pRow["GrandTotal"] = grandTotal;
                pRow["PayedAmount"] = payed;
                pRow["BalanceAmount"] = grandTotal - payed;
                periodTable.Rows.Add(pRow);

                foreach (FilteredPurchaseRecord rec in group.OrderByDescending(x => x.Master.PurchaseDate).ThenByDescending(x => x.Master.PurchaseNo))
                {
                    AddPurchaseMasterRow(masterTable, rec, includePeriodKey: true);
                    AddPurchaseDetailRows(detailTable, rec);
                }
            }

            ds.Tables.Add(periodTable);
            ds.Tables.Add(masterTable);
            ds.Tables.Add(detailTable);

            ds.Relations.Add(new DataRelation("PeriodPurchases", periodTable.Columns["PeriodKey"], masterTable.Columns["PeriodKey"], true));
            ds.Relations.Add(new DataRelation("PurchaseItems", masterTable.Columns["PurchaseNo"], detailTable.Columns["PurchaseNo"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildPurchasesItemsDataSet(List<FilteredPurchaseRecord> records)
        {
            DataSet ds = new DataSet("PurchaseAudit_BillsItems");

            DataTable masterTable = CreatePurchaseMasterTable(includePeriodKey: false);
            DataTable detailTable = CreatePurchaseDetailTable();

            foreach (FilteredPurchaseRecord rec in records)
            {
                AddPurchaseMasterRow(masterTable, rec, includePeriodKey: false);
                AddPurchaseDetailRows(detailTable, rec);
            }

            ds.Tables.Add(masterTable);
            ds.Tables.Add(detailTable);
            ds.Relations.Add(new DataRelation("PurchaseItems", masterTable.Columns["PurchaseNo"], detailTable.Columns["PurchaseNo"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildPeriodItemsDataSet(List<FilteredPurchaseRecord> records)
        {
            DataSet ds = new DataSet("PurchaseAudit_PeriodItems");

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
                pRow["FromDate"] = group.Min(x => x.Master.PurchaseDate.Date);
                pRow["ToDate"] = group.Max(x => x.Master.PurchaseDate.Date);
                pRow["TotalBills"] = group.Count();
                pRow["TotalItems"] = group.Sum(x => x.Details.Count);
                pRow["TotalQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                pRow["TotalFreeQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Free)));
                decimal subTotal = group.Sum(x => Convert.ToDecimal(x.Master.SubTotal));
                decimal grandTotal = group.Sum(x => Convert.ToDecimal(x.Master.GrandTotal));
                decimal payed = group.Sum(x => Convert.ToDecimal(x.Master.PayedAmount));
                pRow["SubTotal"] = subTotal;
                pRow["GrandTotal"] = grandTotal;
                pRow["PayedAmount"] = payed;
                pRow["BalanceAmount"] = grandTotal - payed;
                periodTable.Rows.Add(pRow);

                foreach (FilteredPurchaseRecord rec in group.OrderByDescending(x => x.Master.PurchaseDate).ThenByDescending(x => x.Master.PurchaseNo))
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

        private DataSet BuildFlatItemsDataSet(List<FilteredPurchaseRecord> records)
        {
            DataSet ds = new DataSet("PurchaseAudit_ItemsFlat");
            DataTable itemsTable = CreateFlatItemsTable("ItemsFlat", includePeriodKey: false);

            foreach (FilteredPurchaseRecord rec in records)
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
            dt.Columns.Add("TotalBills", typeof(int));
            dt.Columns.Add("TotalItems", typeof(int));
            dt.Columns.Add("TotalQty", typeof(decimal));
            dt.Columns.Add("TotalFreeQty", typeof(decimal));
            dt.Columns.Add("SubTotal", typeof(decimal));
            dt.Columns.Add("GrandTotal", typeof(decimal));
            dt.Columns.Add("PayedAmount", typeof(decimal));
            dt.Columns.Add("BalanceAmount", typeof(decimal));
            dt.PrimaryKey = new[] { dt.Columns["PeriodKey"] };
            return dt;
        }

        private static DataTable CreatePurchaseMasterTable(bool includePeriodKey)
        {
            DataTable dt = new DataTable("PurchaseMaster");
            if (includePeriodKey)
            {
                dt.Columns.Add("PeriodKey", typeof(string));
            }
            dt.Columns.Add("PeriodLabel", typeof(string));
            dt.Columns.Add("PurchaseNo", typeof(int));
            dt.Columns.Add("PurchaseDate", typeof(DateTime));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("ItemCount", typeof(int));
            dt.Columns.Add("TotalQty", typeof(decimal));
            dt.Columns.Add("SubTotal", typeof(decimal));
            dt.Columns.Add("GrandTotal", typeof(decimal));
            dt.Columns.Add("PayedAmount", typeof(decimal));
            dt.Columns.Add("BalanceAmount", typeof(decimal));
            dt.Columns.Add("BilledBy", typeof(string));
            dt.PrimaryKey = new[] { dt.Columns["PurchaseNo"] };
            return dt;
        }

        private static DataTable CreatePurchaseDetailTable()
        {
            DataTable dt = new DataTable("PurchaseDetail");
            DataColumn idCol = dt.Columns.Add("DetailID", typeof(int));
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = 1;
            idCol.AutoIncrementStep = 1;

            dt.Columns.Add("PurchaseNo", typeof(int));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("BarCode", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("Free", typeof(decimal));
            dt.Columns.Add("Cost", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
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
            dt.Columns.Add("PurchaseNo", typeof(int));
            dt.Columns.Add("PurchaseDate", typeof(DateTime));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("BarCode", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("Free", typeof(decimal));
            dt.Columns.Add("Cost", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("BilledBy", typeof(string));
            return dt;
        }

        private static void AddPurchaseMasterRow(DataTable masterTable, FilteredPurchaseRecord rec, bool includePeriodKey)
        {
            DataRow row = masterTable.NewRow();
            if (includePeriodKey)
            {
                row["PeriodKey"] = rec.PeriodKey;
            }
            row["PeriodLabel"] = rec.PeriodLabel;
            row["PurchaseNo"] = rec.Master.PurchaseNo;
            row["PurchaseDate"] = rec.Master.PurchaseDate;
            row["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
            row["InvoiceDate"] = rec.Master.InvoiceDate;
            row["VendorName"] = rec.Master.VendorName ?? string.Empty;
            row["Paymode"] = rec.Master.Paymode ?? string.Empty;
            row["ItemCount"] = rec.Details.Count;
            row["TotalQty"] = rec.Details.Sum(d => Convert.ToDecimal(d.Qty));
            decimal subTotal = Convert.ToDecimal(rec.Master.SubTotal);
            decimal grandTotal = Convert.ToDecimal(rec.Master.GrandTotal);
            decimal payed = Convert.ToDecimal(rec.Master.PayedAmount);
            row["SubTotal"] = subTotal;
            row["GrandTotal"] = grandTotal;
            row["PayedAmount"] = payed;
            row["BalanceAmount"] = grandTotal - payed;
            row["BilledBy"] = rec.Master.BilledBy ?? string.Empty;
            masterTable.Rows.Add(row);
        }

        private static void AddPurchaseDetailRows(DataTable detailTable, FilteredPurchaseRecord rec)
        {
            foreach (PurchaseReportDetail detail in rec.Details)
            {
                DataRow row = detailTable.NewRow();
                row["PurchaseNo"] = rec.Master.PurchaseNo;
                row["SlNo"] = detail.SlNo;
                row["ItemName"] = detail.ItemName ?? string.Empty;
                row["BarCode"] = detail.BarCode ?? string.Empty;
                row["Unit"] = detail.Unit ?? string.Empty;
                row["Packing"] = detail.Packing ?? string.Empty;
                row["Qty"] = Convert.ToDecimal(detail.Qty);
                row["Free"] = Convert.ToDecimal(detail.Free);
                row["Cost"] = Convert.ToDecimal(detail.Cost);
                row["Amount"] = Convert.ToDecimal(detail.Amount);
                detailTable.Rows.Add(row);
            }
        }

        private static void AddFlatItemRows(DataTable itemsTable, FilteredPurchaseRecord rec, bool includePeriodKey)
        {
            foreach (PurchaseReportDetail detail in rec.Details)
            {
                DataRow row = itemsTable.NewRow();
                if (includePeriodKey)
                {
                    row["PeriodKey"] = rec.PeriodKey;
                }
                row["PeriodLabel"] = rec.PeriodLabel;
                row["PurchaseNo"] = rec.Master.PurchaseNo;
                row["PurchaseDate"] = rec.Master.PurchaseDate;
                row["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
                row["InvoiceDate"] = rec.Master.InvoiceDate;
                row["VendorName"] = rec.Master.VendorName ?? string.Empty;
                row["Paymode"] = rec.Master.Paymode ?? string.Empty;
                row["SlNo"] = detail.SlNo;
                row["ItemName"] = detail.ItemName ?? string.Empty;
                row["BarCode"] = detail.BarCode ?? string.Empty;
                row["Unit"] = detail.Unit ?? string.Empty;
                row["Packing"] = detail.Packing ?? string.Empty;
                row["Qty"] = Convert.ToDecimal(detail.Qty);
                row["Free"] = Convert.ToDecimal(detail.Free);
                row["Cost"] = Convert.ToDecimal(detail.Cost);
                row["Amount"] = Convert.ToDecimal(detail.Amount);
                row["BilledBy"] = rec.Master.BilledBy ?? string.Empty;
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
                ConfigureBandByKey(band, b);
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
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
                    periodHeader = "Purchase Day";
                else if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
                    periodHeader = "Year";

                int pos = 0;
                ConfigureGridColumn(band, bandIndex, "PeriodLabel", periodHeader, 190, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "FromDate", "First Purchase", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ToDate", "Last Purchase", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalBills", "Total Bills", 90, "#,##0", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalItems", "Line Items", 90, "#,##0", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalQty", "Total Qty", 100, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalFreeQty", "Free Qty", 90, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "SubTotal", "Sub Total", 120, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "GrandTotal", "Grand Total", 125, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "PayedAmount", "Paid Amt", 120, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "BalanceAmount", "Balance", 120, "#,##0.00", HAlign.Right, pos++);

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
                if (band.Columns.Exists("BalanceAmount"))
                {
                    band.Columns["BalanceAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["BalanceAmount"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
                }
            }
            else if (string.Equals(band.Key, "PurchaseMaster", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "PeriodPurchases", StringComparison.OrdinalIgnoreCase))
            {
                if (bandIndex > 0)
                {
                    band.HeaderVisible = true;
                    band.Header.Caption = "Purchase Bills";
                    StyleChildBandHeader(band, Color.FromArgb(52, 116, 180));
                }

                int pos = 0;
                if (bandIndex == 0)
                {
                    ConfigureGridColumn(band, bandIndex, "PeriodLabel", "Period", 145, null, HAlign.Left, pos++);
                }
                ConfigureGridColumn(band, bandIndex, "PurchaseNo", "Purchase No", 95, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "PurchaseDate", "Purchase Date", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "InvoiceNo", "Invoice No", 105, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "InvoiceDate", "Invoice Date", 105, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "VendorName", "Vendor", 210, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Paymode", "Payment Mode", 105, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ItemCount", "Items", 65, "#,##0", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "TotalQty", "Total Qty", 90, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "SubTotal", "Sub Total", 110, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "GrandTotal", "Grand Total", 115, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "PayedAmount", "Paid Amt", 110, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "BalanceAmount", "Balance", 110, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "BilledBy", "Billed By", 105, null, HAlign.Left, pos++);

                if (band.Columns.Exists("PurchaseNo"))
                {
                    band.Columns["PurchaseNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["PurchaseNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                }
                if (band.Columns.Exists("GrandTotal"))
                {
                    band.Columns["GrandTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["GrandTotal"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                }
                if (band.Columns.Exists("BalanceAmount"))
                {
                    band.Columns["BalanceAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["BalanceAmount"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
                }

                if (bandIndex > 0)
                {
                    ConfigureChildBandSummaries(band, new[] { "TotalQty", "SubTotal", "GrandTotal", "PayedAmount", "BalanceAmount" });
                }
            }
            else if (string.Equals(band.Key, "PurchaseDetail", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "PurchaseItems", StringComparison.OrdinalIgnoreCase))
            {
                band.HeaderVisible = true;
                band.Header.Caption = "Purchase Item Details";
                StyleChildBandHeader(band, Color.FromArgb(41, 128, 185));

                int pos = 0;
                ConfigureGridColumn(band, bandIndex, "SlNo", "S.No", 55, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ItemName", "Item Name", 240, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "BarCode", "Barcode", 125, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Unit", "Unit", 70, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Packing", "Packing", 85, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Qty", "Quantity", 90, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Free", "Free Qty", 85, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Cost", "Cost", 100, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Amount", "Amount", 120, "#,##0.00", HAlign.Right, pos++);

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

                ConfigureChildBandSummaries(band, new[] { "Qty", "Free", "Amount" });
            }
            else if (string.Equals(band.Key, "PeriodItems", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "PeriodAllItems", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(band.Key, "ItemsFlat", StringComparison.OrdinalIgnoreCase))
            {
                if (bandIndex > 0)
                {
                    band.HeaderVisible = true;
                    band.Header.Caption = "All Purchased Items in Period";
                    StyleChildBandHeader(band, Color.FromArgb(41, 128, 185));
                }

                int pos = 0;
                if (bandIndex == 0)
                {
                    ConfigureGridColumn(band, bandIndex, "PeriodLabel", "Period", 140, null, HAlign.Left, pos++);
                }
                ConfigureGridColumn(band, bandIndex, "PurchaseNo", "Purchase No", 90, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "PurchaseDate", "Purchase Date", 100, "dd-MMM-yyyy", HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "InvoiceNo", "Invoice No", 95, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "VendorName", "Vendor", 185, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "SlNo", "S.No", 50, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "ItemName", "Item Name", 220, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "BarCode", "Barcode", 115, null, HAlign.Left, pos++);
                ConfigureGridColumn(band, bandIndex, "Unit", "Unit", 65, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Packing", "Packing", 75, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "Qty", "Quantity", 85, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Free", "Free Qty", 80, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Cost", "Cost", 95, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Amount", "Amount", 115, "#,##0.00", HAlign.Right, pos++);
                ConfigureGridColumn(band, bandIndex, "Paymode", "Pay Mode", 90, null, HAlign.Center, pos++);
                ConfigureGridColumn(band, bandIndex, "BilledBy", "Billed By", 95, null, HAlign.Left, pos++);

                if (band.Columns.Exists("PurchaseNo"))
                {
                    band.Columns["PurchaseNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["PurchaseNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
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
                    ConfigureChildBandSummaries(band, new[] { "Qty", "Free", "Amount" });
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
            column.Hidden = userHiddenColumnKeys.Contains($"{bandIndex}:{key}");
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
            if (e.Row.Cells.Exists("BalanceAmount"))
            {
                decimal balance = 0m;
                if (e.Row.Cells["BalanceAmount"].Value != null && e.Row.Cells["BalanceAmount"].Value != DBNull.Value)
                {
                    decimal.TryParse(e.Row.Cells["BalanceAmount"].Value.ToString(), out balance);
                }

                e.Row.Cells["BalanceAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Cells["BalanceAmount"].Appearance.ForeColor = balance > 0
                    ? Color.FromArgb(191, 54, 12)
                    : Color.FromArgb(46, 125, 50);
            }
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (e.Row.ChildBands != null && e.Row.ChildBands.Count > 0)
            {
                e.Row.Expanded = !e.Row.Expanded;
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
            ShowReportPreview("PURCHASE AUDIT REPORT");
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(
                "PURCHASE AUDIT REPORT",
                new[]
                {
                    "PURCHASE AUDIT REPORT - PERIOD -> PURCHASES -> ITEMS",
                    "PURCHASE AUDIT REPORT - BILL WISE & ITEM DETAILS",
                    "PURCHASE AUDIT REPORT - PERIOD -> ALL PURCHASED ITEMS",
                    "PURCHASE AUDIT REPORT - ALL PURCHASED ITEMS (FLAT)"
                }))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string selectedDesc = dialog.SelectedFormatDescription ?? string.Empty;
                    if (selectedDesc.IndexOf("BILL WISE", StringComparison.OrdinalIgnoreCase) >= 0)
                        ultraComboViewMode.Value = "BILLS_ITEMS";
                    else if (selectedDesc.IndexOf("ALL PURCHASED ITEMS (FLAT)", StringComparison.OrdinalIgnoreCase) >= 0)
                        ultraComboViewMode.Value = "ITEMS_FLAT";
                    else if (selectedDesc.IndexOf("PERIOD -> ALL PURCHASED ITEMS", StringComparison.OrdinalIgnoreCase) >= 0)
                        ultraComboViewMode.Value = "PERIOD_ITEMS";
                    else
                        ultraComboViewMode.Value = "PERIOD_BILLS_ITEMS";

                    if (string.Equals(dialog.SelectedAction, "print", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(dialog.SelectedAction, "printer", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintReport();
                    }
                    else
                    {
                        ShowReportPreview(string.IsNullOrWhiteSpace(selectedDesc) ? "PURCHASE AUDIT REPORT" : selectedDesc);
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
            ClearForm();
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows != null)
            {
                gridReport.Rows.ExpandAll(true);
                UpdateFooterCellPositions();
            }
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows != null)
            {
                gridReport.Rows.CollapseAll(true);
                UpdateFooterCellPositions();
            }
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            if (btnToggleSelection != null && ultraPanelControls != null)
            {
                btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
            }
        }

        private void ultraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdatePeriodControlVisibility();
            LoadReport();
        }

        private void FilterSelection_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            LoadReport();
        }

        private void ultraComboViewMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            BuildAndBindReportData();
        }

        private void chkSingleDay_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdatePeriodControlVisibility();
            LoadReport();
        }

        private void dtFrom_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            if (chkSingleDay.Checked && dtFrom.Value != null)
            {
                dtTo.Value = dtFrom.Value;
            }
        }

        private void dtTo_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
        }

        private void btnVendorPicker_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void btnItemPicker_Click(object sender, EventArgs e)
        {
            OpenItemDialog();
        }

        private void txtVendorSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                BuildAndBindReportData();
                e.Handled = true;
            }
        }

        private void txtItemFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenItemDialog();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                BuildAndBindReportData();
                e.Handled = true;
            }
        }

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BuildAndBindReportData();
                e.Handled = true;
            }
        }

        private void frmpurchaseAuditreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                if (txtItemFilter.Focused)
                    OpenItemDialog();
                else
                    OpenVendorDialog();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                btnExportGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                btnPreviewReport.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnViewGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnClearFilters.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void OpenVendorDialog()
        {
            try
            {
                using (frmVendorDig vendorDialog = new frmVendorDig())
                {
                    if (vendorDialog.ShowDialog(this) == DialogResult.OK &&
                        !string.IsNullOrWhiteSpace(vendorDialog.SelectedVendorName))
                    {
                        txtVendorSearch.Text = vendorDialog.SelectedVendorName;
                        BuildAndBindReportData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening vendor lookup: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenItemDialog()
        {
            try
            {
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmpurchaseAuditreport"))
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

        #region Preview, Print, Export & Clear
        private void ShowReportPreview(string title)
        {
            List<FilteredPurchaseRecord> records = GetFilteredPurchaseRecords();
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
                preview.Text = "Purchase Audit Report - Preview";
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
                    Text = string.IsNullOrWhiteSpace(title) ? "PURCHASE AUDIT REPORT" : title,
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

                int totalBills = records.Count;
                int totalItems = records.Sum(r => r.Details.Count);
                decimal totalQty = records.Sum(r => r.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                decimal grandTotal = records.Sum(r => Convert.ToDecimal(r.Master.GrandTotal));
                decimal totalPaid = records.Sum(r => Convert.ToDecimal(r.Master.PayedAmount));
                decimal totalBalance = grandTotal - totalPaid;

                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Purchases: {0:N0}    |    Items: {1:N0}    |    Total Qty: {2:N2}    |    Grand Total: {3:N2}    |    Paid: {4:N2}    |    Balance: {5:N2}",
                        totalBills, totalItems, totalQty, grandTotal, totalPaid, totalBalance),
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

            string vendor = string.IsNullOrWhiteSpace(txtVendorSearch.Text) ? "ALL" : txtVendorSearch.Text.Trim();
            string item = string.IsNullOrWhiteSpace(txtItemFilter.Text) ? "ALL" : txtItemFilter.Text.Trim();

            return string.Format("Mode: {0} ({1})    |    Vendor: {2}    |    Item: {3}    |    View: {4}",
                modeText, periodSelection, vendor, item, ultraComboViewMode.Text);
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
            List<FilteredPurchaseRecord> records = GetFilteredPurchaseRecords();
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
                    printDocument.DocumentName = "Purchase Audit Report";
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

                        e.Graphics.DrawString("PURCHASE AUDIT REPORT", titleFont, Brushes.Black, left, y);
                        y += 24;
                        e.Graphics.DrawString(BuildPreviewSubtitle(), subFont, Brushes.DimGray, left, y);
                        y += 24;

                        string[] headers = { "Period", "Pur No", "Date", "Vendor", "Items", "Qty", "Grand Total", "Paid" };
                        float[] widths = { 110, 60, 75, 160, 50, 65, 90, 90 };
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

                            FilteredPurchaseRecord rec = records[printIndex++];
                            x = left;
                            string[] vals =
                            {
                                rec.PeriodLabel,
                                rec.Master.PurchaseNo.ToString(),
                                rec.Master.PurchaseDate.ToString("dd/MM/yyyy"),
                                rec.Master.VendorName ?? string.Empty,
                                rec.Details.Count.ToString(),
                                rec.Details.Sum(d => d.Qty).ToString("N2"),
                                rec.Master.GrandTotal.ToString("N2"),
                                rec.Master.PayedAmount.ToString("N2")
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
            List<FilteredPurchaseRecord> records = GetFilteredPurchaseRecords();
            if (records.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = string.Format("PurchaseAuditReport_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Period,PurchaseNo,PurchaseDate,InvoiceNo,InvoiceDate,Vendor,PaymentMode,SlNo,ItemName,Barcode,Unit,Packing,Qty,FreeQty,Cost,ItemAmount,BillSubTotal,BillGrandTotal,BillPaidAmount,BillBalance,BilledBy");

                foreach (FilteredPurchaseRecord rec in records)
                {
                    decimal balance = Convert.ToDecimal(rec.Master.GrandTotal) - Convert.ToDecimal(rec.Master.PayedAmount);
                    if (rec.Details.Count > 0)
                    {
                        foreach (PurchaseReportDetail detail in rec.Details)
                        {
                            builder.AppendLine(string.Join(",",
                                EscapeCsv(rec.PeriodLabel),
                                rec.Master.PurchaseNo.ToString(),
                                EscapeCsv(rec.Master.PurchaseDate.ToString("yyyy-MM-dd")),
                                EscapeCsv(rec.Master.InvoiceNo),
                                EscapeCsv(rec.Master.InvoiceDate.ToString("yyyy-MM-dd")),
                                EscapeCsv(rec.Master.VendorName),
                                EscapeCsv(rec.Master.Paymode),
                                detail.SlNo.ToString(),
                                EscapeCsv(detail.ItemName),
                                EscapeCsv(detail.BarCode),
                                EscapeCsv(detail.Unit),
                                EscapeCsv(detail.Packing),
                                detail.Qty.ToString("F2"),
                                detail.Free.ToString("F2"),
                                detail.Cost.ToString("F2"),
                                detail.Amount.ToString("F2"),
                                rec.Master.SubTotal.ToString("F2"),
                                rec.Master.GrandTotal.ToString("F2"),
                                rec.Master.PayedAmount.ToString("F2"),
                                balance.ToString("F2"),
                                EscapeCsv(rec.Master.BilledBy)));
                        }
                    }
                    else
                    {
                        builder.AppendLine(string.Join(",",
                            EscapeCsv(rec.PeriodLabel),
                            rec.Master.PurchaseNo.ToString(),
                            EscapeCsv(rec.Master.PurchaseDate.ToString("yyyy-MM-dd")),
                            EscapeCsv(rec.Master.InvoiceNo),
                            EscapeCsv(rec.Master.InvoiceDate.ToString("yyyy-MM-dd")),
                            EscapeCsv(rec.Master.VendorName),
                            EscapeCsv(rec.Master.Paymode),
                            "",
                            "",
                            "",
                            "",
                            "",
                            "0.00",
                            "0.00",
                            "0.00",
                            "0.00",
                            rec.Master.SubTotal.ToString("F2"),
                            rec.Master.GrandTotal.ToString("F2"),
                            rec.Master.PayedAmount.ToString("F2"),
                            balance.ToString("F2"),
                            EscapeCsv(rec.Master.BilledBy)));
                    }
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Purchase Audit Report exported successfully.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return string.Format("\"{0}\"", safeValue.Replace("\"", "\"\""));
        }

        public void RibbonClear() => ClearForm();
        public void Clear() => ClearForm();
        public void ResetForm() => ClearForm();
        public void ResetFilters() => ClearForm();

        public void ClearForm()
        {
            _isLoading = true;
            try
            {
                DateTime today = DateTime.Today;
                ultraComboPeriodMode.Value = "MONTH_WISE";
                ultraComboMonth.Value = today.Month;
                ultraComboYear.Value = today.Year;
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                chkSingleDay.Checked = false;
                ultraComboViewMode.Value = "PERIOD_BILLS_ITEMS";
                txtVendorSearch.Text = string.Empty;
                txtPurchaseNo.Text = string.Empty;
                txtItemFilter.Text = string.Empty;
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }
        #endregion
    }
}
