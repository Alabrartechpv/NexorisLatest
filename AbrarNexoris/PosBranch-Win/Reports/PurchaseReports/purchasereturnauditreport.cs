using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports.FinancialReports;
using PosBranch_Win.Transaction;
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
    /// Purchase Return Audit Report (purchasereturnauditreport)
    /// Displays all purchase returns and their item details Month-wise, Day-wise, and Year-wise,
    /// styled with the exact theme, gridReport, and ultraPanelGridFooter of frmVendorOutstandingReport.
    /// </summary>
    public partial class purchasereturnauditreport : Form
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
        private readonly PurchaseReturnReportRepository _reportRepository;
        private readonly Dictionary<int, List<PurchaseReturnReportDetail>> _detailsCache;
        private List<PurchaseReturnReportMaster> _loadedReturns;
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
            "Cost",
            "TaxPer",
            "TaxAmt",
            "Amount",
            "SubTotal",
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
            public PurchaseReturnReportMaster Master { get; set; }
            public List<PurchaseReturnReportDetail> Details { get; set; }
            public string PeriodKey { get; set; }
            public string PeriodLabel { get; set; }
            public DateTime PeriodSortDate { get; set; }
        }
        #endregion

        #region Constructor & Form Lifecycle
        public purchasereturnauditreport()
        {
            _reportRepository = new PurchaseReturnReportRepository();
            _detailsCache = new Dictionary<int, List<PurchaseReturnReportDetail>>();
            _loadedReturns = new List<PurchaseReturnReportMaster>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += purchasereturnauditreport_Load;
            FormClosing += purchasereturnauditreport_FormClosing;
            KeyPreview = true;
            KeyDown += purchasereturnauditreport_KeyDown;

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
            txtReturnNo.KeyDown += FilterTextBox_KeyDown;
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
                ReportGridLayoutHelper.SaveLayout("PurchaseReturnAuditReport", gridReport, userHiddenColumnKeys);
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
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;
            gridReport.AllowDrop = true;
            _filterManager = new ReportColumnFilterManager(gridReport, () => { UpdateFooterCellPositions(); UpdateFooterValues(); });
        }

        private void purchasereturnauditreport_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _isLoading = true;

                Text = "Purchase Return Audit Report";
                WindowState = FormWindowState.Maximized;

                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeFilterControls();
                LoadVendors();
                InitializeGridFooter();

                ReportGridLayoutHelper.LoadHiddenColumns("PurchaseReturnAuditReport", userHiddenColumnKeys);

                _isLoading = false;
                LoadReport();
            }
            catch (Exception ex)
            {
                _isLoading = false;
                MessageBox.Show("Error initializing report: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void purchasereturnauditreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ReportGridLayoutHelper.SaveLayout("PurchaseReturnAuditReport", gridReport, userHiddenColumnKeys);
                if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                {
                    columnChooserForm.Dispose();
                }
                if (_currentDataSet != null)
                {
                    _currentDataSet.Dispose();
                _filterManager?.ClearFilters();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error during form closing: " + ex.Message);
            }
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
            ultraComboViewMode.Items.Add("PERIOD_RETURNS_ITEMS", "Period -> Purchase Returns -> Return Item Details");
            ultraComboViewMode.Items.Add("RETURNS_ITEMS", "Purchase Returns & Return Item Details (Return Wise)");
            ultraComboViewMode.Items.Add("PERIOD_ITEMS", "Period -> All Returned Items");
            ultraComboViewMode.Items.Add("ITEMS_FLAT", "All Returned Items (Flat List)");
            ultraComboViewMode.Value = "PERIOD_RETURNS_ITEMS";

            txtVendorSearch.Text = string.Empty;
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

            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.BorderColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(215, 235, 255);
            layout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;

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

                ContentAlignment align = ContentAlignment.MiddleCenter;
                if (false && (column.CellAppearance.TextHAlign == HAlign.Right || IsSummableColumn(column)))
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

            using (Pen borderPen = new Pen(GridFooterBorder))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, lbl.Width - 1, lbl.Height - 1);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };

            string[] operations = { "Sum", "Average", "Min", "Max", "Count", "None" };
            foreach (string op in operations)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(op);
                item.Click += (s, e) =>
                {
                    _columnAggregations[columnKey] = op;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }

            menu.Opening += (s, e) =>
            {
                string currentOp = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";
                foreach (ToolStripItem itm in menu.Items)
                {
                    if (itm is ToolStripMenuItem mi)
                    {
                        mi.Checked = string.Equals(mi.Text, currentOp, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraPanelGridFooter == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            try
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                int scrollOffset = 0;
                ColScrollRegion colRegion = gridReport.DisplayLayout.ColScrollRegions.Count > 0
                    ? gridReport.DisplayLayout.ColScrollRegions[0]
                    : null;

                if (colRegion != null)
                {
                    scrollOffset = colRegion.Position;
                }

                int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
                int currentX = rowSelectorWidth - scrollOffset;

                ultraPanelGridFooter.SuspendLayout();

                foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
                {
                    if (column.Hidden || column.IsChaptered)
                    {
                        if (_footerLabels.TryGetValue(column.Key, out Label hiddenLbl))
                        {
                            hiddenLbl.Visible = false;
                        }
                        continue;
                    }

                    if (!_footerLabels.TryGetValue(column.Key, out Label lbl))
                    {
                        CreateFooterCells();
                        if (!_footerLabels.TryGetValue(column.Key, out lbl))
                        {
                            continue;
                        }
                    }

                    lbl.Left = currentX;
                    lbl.Width = column.Width;
                    lbl.Visible = (lbl.Right > 0 && lbl.Left < ultraPanelGridFooter.Width);

                    currentX += column.Width;
                }
            }
            finally
            {
                ultraPanelGridFooter.ResumeLayout();
            }
        }

        private void UpdateFooterValues()
        {
            if (ultraPanelGridFooter == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            List<UltraGridRow> visibleRows = gridReport.Rows.GetFilteredInNonGroupByRows().Where(r => !r.Hidden).ToList();

            foreach (var kvp in _footerLabels)
            {
                string colKey = kvp.Key;
                Label lbl = kvp.Value;

                if (!band.Columns.Exists(colKey))
                {
                    lbl.Text = string.Empty;
                    continue;
                }

                UltraGridColumn column = band.Columns[colKey];
                string op = _columnAggregations.ContainsKey(colKey) ? _columnAggregations[colKey] : "None";

                if (op == "None")
                {
                    lbl.Text = string.Empty;
                    continue;
                }

                if (op == "Count")
                {
                    lbl.Text = $"Count: {visibleRows.Count:N0}";
                    continue;
                }

                List<decimal> numericValues = new List<decimal>();
                foreach (UltraGridRow row in visibleRows)
                {
                    if (row == null || !row.IsDataRow || !row.Cells.Exists(colKey)) continue;
                    decimal? val = ExtractDecimal(row.Cells[colKey].Value);
                    if (val.HasValue)
                    {
                        numericValues.Add(val.Value);
                    }
                }

                if (numericValues.Count == 0)
                {
                    lbl.Text = "-";
                    continue;
                }

                decimal result = 0m;
                switch (op)
                {
                    case "Sum":
                        result = numericValues.Sum();
                        break;
                    case "Average":
                        result = numericValues.Average();
                        break;
                    case "Min":
                        result = numericValues.Min();
                        break;
                    case "Max":
                        result = numericValues.Max();
                        break;
                }

                string formatted;
                if (colKey.IndexOf("Qty", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    colKey.IndexOf("Count", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    colKey.IndexOf("TotalReturns", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    colKey.IndexOf("TotalItems", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    formatted = result.ToString("N2");
                }
                else
                {
                    formatted = "₹ " + result.ToString("N2");
                }

                lbl.Text = $"{op}: {formatted}";
            }
        }

        private static decimal? ExtractDecimal(object value)
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
                    using (Pen p = new Pen(Color.FromArgb(200, 30, 30), 3f))
                    {
                        g.DrawLine(p, 6, 6, 26, 26);
                        g.DrawLine(p, 26, 6, 6, 26);
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

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(e.Location);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                UltraGridColumn col = headerUI.Header.Column;

                if (e.Button == MouseButtons.Right)
                {
                    ShowHeaderContextMenu(col, e.Location);
                    return;
                }

                if (e.Button == MouseButtons.Left && _filterManager != null && _filterManager.HandleMouseDown(e))
                {
                    isDraggingHeaderToHide = false;
                    columnBeingDragged = null;
                    return;
                }

                if (e.Button == MouseButtons.Left && IsCustomizableColumn(col))
                {
                    columnBeingDragged = col;
                    headerDragStartPoint = e.Location;
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
            if (!isDraggingHeaderToHide && _filterManager != null && e.Button != MouseButtons.Left && _filterManager.HandleMouseMove(e, headerToolTip))
                return;

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
            if (!isDraggingHeaderToHide && _filterManager != null && e.Button != MouseButtons.Left && _filterManager.HandleMouseMove(e, headerToolTip))
                return;

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
            string key = gridReport.DisplayLayout.Bands.Count > 1 ? $"{bandIndex}:{col.Key}" : col.Key;
            userHiddenColumnKeys.Add(key);
            col.Hidden = true;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("PurchaseReturnAuditReport", gridReport, userHiddenColumnKeys);
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

            _filterManager?.AddFilterContextMenuItems(menu, col, location);
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
                userHiddenColumnKeys.Remove(columnKey);
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
                ReportGridLayoutHelper.SaveLayout("PurchaseReturnAuditReport", gridReport, userHiddenColumnKeys);
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            if (gridReport.DisplayLayout == null) return;
            userHiddenColumnKeys.Clear();

            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[b];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (IsCustomizableColumn(col) && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                    {
                        col.Hidden = false;
                    }
                }
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("PurchaseReturnAuditReport", gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm == null) return;
            Point screenPt = PointToScreen(Point.Empty);
            int x = screenPt.X + Width - columnChooserForm.Width - 25;
            int y = screenPt.Y + Height - columnChooserForm.Height - 35;
            columnChooserForm.Location = new Point(Math.Max(10, x), Math.Max(10, y));
        }

        private static bool IsCustomizableColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered || col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            string key = col.Key;
            return !string.Equals(key, "PeriodKey", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(key, "DetailID", StringComparison.OrdinalIgnoreCase) &&
                   !key.EndsWith("Id", StringComparison.OrdinalIgnoreCase);
        }

        private string GetLayoutFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "NexorisPOS");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "PurchaseReturnAuditReportGridLayout.xml");
        }

        private void SaveGridLayout()
        {
            try
            {
                if (gridReport.DisplayLayout != null && gridReport.Rows.Count > 0)
                {
                    gridReport.DisplayLayout.SaveAsXml(GetLayoutFilePath(), PropertyCategories.All);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving layout: " + ex.Message);
            }
        }

        private void LoadGridLayout()
        {
            try
            {
                string path = GetLayoutFilePath();
                if (File.Exists(path))
                {
                    gridReport.DisplayLayout.LoadFromXml(path, PropertyCategories.All);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading layout: " + ex.Message);
            }
        }
        #endregion

        #region Data Retrieval & Detail Cache
        private void LoadReport()
        {
            if (_isLoading) return;

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _isLoading = true;

                DateTime fromDate = GetEffectiveFromDate();
                DateTime toDate = GetEffectiveToDate();

                int branchId = 0;
                try
                {
                    branchId = SessionContext.BranchId;
                }
                catch
                {
                    branchId = 0;
                }

                _loadedReturns = _reportRepository.GetPurchaseReturnRecords(fromDate, toDate, branchId) ?? new List<PurchaseReturnReportMaster>();

                // Pre-populate details cache for records in range
                PreloadDetailsForVisibleReturns(_loadedReturns);

                BuildAndBindReportData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading purchase return records: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void PreloadDetailsForVisibleReturns(List<PurchaseReturnReportMaster> returns)
        {
            if (returns == null || returns.Count == 0) return;

            foreach (var ret in returns)
            {
                if (!_detailsCache.ContainsKey(ret.PReturnNo))
                {

                    try
                    {
                        var data = _reportRepository.GetPurchaseReturnReportDetails(ret.PReturnNo);
                        _detailsCache[ret.PReturnNo] = data?.Details ?? new List<PurchaseReturnReportDetail>();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error caching details for return #{ret.PReturnNo}: {ex.Message}");
                        _detailsCache[ret.PReturnNo] = new List<PurchaseReturnReportDetail>();
                    }
                }
            }
        }

        private List<PurchaseReturnReportDetail> GetDetailsForReturn(int returnNo)
        {
            if (_detailsCache.TryGetValue(returnNo, out List<PurchaseReturnReportDetail> cached))
            {
                return cached;
            }

            try
            {
                var data = _reportRepository.GetPurchaseReturnReportDetails(returnNo);
                List<PurchaseReturnReportDetail> details = data?.Details ?? new List<PurchaseReturnReportDetail>();
                _detailsCache[returnNo] = details;
                return details;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching details for return #{returnNo}: {ex.Message}");
                List<PurchaseReturnReportDetail> empty = new List<PurchaseReturnReportDetail>();
                _detailsCache[returnNo] = empty;
                return empty;
            }
        }
        #endregion

        #region Hierarchical Dataset Builders
        private List<FilteredReturnRecord> GetFilteredReturnRecords()
        {
            List<FilteredReturnRecord> list = new List<FilteredReturnRecord>();
            if (_loadedReturns == null || _loadedReturns.Count == 0)
                return list;

            string vendorFilter = (txtVendorSearch.Text ?? string.Empty).Trim();
            if (string.Equals(vendorFilter, "ALL Vendors", StringComparison.OrdinalIgnoreCase))
                vendorFilter = string.Empty;

            string returnNoFilter = (txtReturnNo.Text ?? string.Empty).Trim();
            string itemFilter = (txtItemFilter.Text ?? string.Empty).Trim();

            DateTime effFrom = GetEffectiveFromDate();
            DateTime effTo = GetEffectiveToDate();
            string mode = GetPeriodMode();

            foreach (PurchaseReturnReportMaster master in _loadedReturns)
            {
                DateTime dt = master.PReturnDate.Date;
                if (dt < effFrom.Date || dt > effTo.Date)
                    continue;

                if (!string.IsNullOrEmpty(vendorFilter) &&
                    (master.VendorName == null || master.VendorName.IndexOf(vendorFilter, StringComparison.OrdinalIgnoreCase) < 0))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(returnNoFilter))
                {
                    bool matchNo = master.PReturnNo.ToString().IndexOf(returnNoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool matchInv = !string.IsNullOrEmpty(master.InvoiceNo) && master.InvoiceNo.IndexOf(returnNoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matchNo && !matchInv)
                        continue;
                }

                List<PurchaseReturnReportDetail> details = GetDetailsForReturn(master.PReturnNo);

                if (!string.IsNullOrEmpty(itemFilter))
                {
                    bool itemMatched = details.Any(d =>
                        (!string.IsNullOrEmpty(d.ItemName) && d.ItemName.IndexOf(itemFilter, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(d.Reason) && d.Reason.IndexOf(itemFilter, StringComparison.OrdinalIgnoreCase) >= 0));

                    if (!itemMatched)
                        continue;
                }

                string pKey;
                string pLabel;
                DateTime pSort;
                GetPeriodKeyAndLabel(dt, mode, out pKey, out pLabel, out pSort);

                list.Add(new FilteredReturnRecord
                {
                    Master = master,
                    Details = details,
                    PeriodKey = pKey,
                    PeriodLabel = pLabel,
                    PeriodSortDate = pSort
                });
            }

            return list;
        }

        private void GetPeriodKeyAndLabel(DateTime dt, string mode, out string key, out string label, out DateTime sortDate)
        {
            if (string.Equals(mode, "DAY_WISE", StringComparison.OrdinalIgnoreCase))
            {
                key = dt.ToString("yyyy-MM-dd");
                label = dt.ToString("dd/MM/yyyy (ddd)");
                sortDate = dt.Date;
            }
            else if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                key = dt.ToString("yyyy");
                label = "Year " + dt.Year;
                sortDate = new DateTime(dt.Year, 1, 1);
            }
            else
            {
                // MONTH_WISE or default
                key = dt.ToString("yyyy-MM");
                label = dt.ToString("MMMM yyyy");
                sortDate = new DateTime(dt.Year, dt.Month, 1);
            }
        }

        private void BuildAndBindReportData()
        {
            List<FilteredReturnRecord> records = GetFilteredReturnRecords();
            string viewMode = GetViewMode();

            gridReport.DataSource = null;
            gridReport.DataMember = null;

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _filterManager?.ClearFilters();
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
                gridReport.DataMember = "ReturnMaster";
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

            // Auto-expand single period row so user immediately inspects records
            if (gridReport.Rows != null && gridReport.Rows.Count == 1 &&
                (string.Equals(viewMode, "PERIOD_RETURNS_ITEMS", StringComparison.OrdinalIgnoreCase) ||
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

        private DataSet BuildPeriodReturnsItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("PurchaseReturnAudit_PeriodReturnsItems");

            DataTable periodTable = CreatePeriodSummaryTable();
            DataTable masterTable = CreateReturnMasterTable(includePeriodKey: true);
            DataTable detailTable = CreateReturnDetailTable();

            var periodGroups = records
                .GroupBy(r => new { r.PeriodKey, r.PeriodLabel, r.PeriodSortDate })
                .OrderByDescending(g => g.Key.PeriodSortDate);

            foreach (var group in periodGroups)
            {
                DataRow pRow = periodTable.NewRow();
                pRow["PeriodKey"] = group.Key.PeriodKey;
                pRow["PeriodLabel"] = group.Key.PeriodLabel;
                pRow["FromDate"] = group.Min(x => x.Master.PReturnDate.Date);
                pRow["ToDate"] = group.Max(x => x.Master.PReturnDate.Date);
                pRow["TotalReturns"] = group.Count();
                pRow["TotalItems"] = group.Sum(x => x.Details.Count);
                pRow["TotalQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                pRow["SubTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.SubTotal));
                pRow["GrandTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.GrandTotal));
                periodTable.Rows.Add(pRow);

                foreach (FilteredReturnRecord rec in group.OrderByDescending(x => x.Master.PReturnDate).ThenByDescending(x => x.Master.PReturnNo))
                {
                    AddReturnMasterRow(masterTable, rec, includePeriodKey: true);
                    AddReturnDetailRows(detailTable, rec);
                }
            }

            ds.Tables.Add(periodTable);
            ds.Tables.Add(masterTable);
            ds.Tables.Add(detailTable);

            ds.Relations.Add(new DataRelation("PeriodReturns", periodTable.Columns["PeriodKey"], masterTable.Columns["PeriodKey"], true));
            ds.Relations.Add(new DataRelation("ReturnItems", masterTable.Columns["PReturnNo"], detailTable.Columns["PReturnNo"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildReturnsItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("PurchaseReturnAudit_ReturnsItems");

            DataTable masterTable = CreateReturnMasterTable(includePeriodKey: false);
            DataTable detailTable = CreateReturnDetailTable();

            foreach (FilteredReturnRecord rec in records)
            {
                AddReturnMasterRow(masterTable, rec, includePeriodKey: false);
                AddReturnDetailRows(detailTable, rec);
            }

            ds.Tables.Add(masterTable);
            ds.Tables.Add(detailTable);
            ds.Relations.Add(new DataRelation("ReturnItems", masterTable.Columns["PReturnNo"], detailTable.Columns["PReturnNo"], true));
            ds.AcceptChanges();
            return ds;
        }

        private DataSet BuildPeriodItemsDataSet(List<FilteredReturnRecord> records)
        {
            DataSet ds = new DataSet("PurchaseReturnAudit_PeriodItems");

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
                pRow["FromDate"] = group.Min(x => x.Master.PReturnDate.Date);
                pRow["ToDate"] = group.Max(x => x.Master.PReturnDate.Date);
                pRow["TotalReturns"] = group.Count();
                pRow["TotalItems"] = group.Sum(x => x.Details.Count);
                pRow["TotalQty"] = group.Sum(x => x.Details.Sum(d => Convert.ToDecimal(d.Qty)));
                pRow["SubTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.SubTotal));
                pRow["GrandTotal"] = group.Sum(x => Convert.ToDecimal(x.Master.GrandTotal));
                periodTable.Rows.Add(pRow);

                foreach (FilteredReturnRecord rec in group.OrderByDescending(x => x.Master.PReturnDate).ThenByDescending(x => x.Master.PReturnNo))
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
            DataSet ds = new DataSet("PurchaseReturnAudit_ItemsFlat");
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
            dt.Columns.Add("GrandTotal", typeof(decimal));
            dt.PrimaryKey = new[] { dt.Columns["PeriodKey"] };
            return dt;
        }

        private static DataTable CreateReturnMasterTable(bool includePeriodKey)
        {
            DataTable dt = new DataTable("ReturnMaster");
            if (includePeriodKey)
            {
                dt.Columns.Add("PeriodKey", typeof(string));
            }
            dt.Columns.Add("PeriodLabel", typeof(string));
            dt.Columns.Add("PReturnNo", typeof(int));
            dt.Columns.Add("PReturnDate", typeof(DateTime));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("ItemCount", typeof(int));
            dt.Columns.Add("TotalQty", typeof(decimal));
            dt.Columns.Add("SubTotal", typeof(decimal));
            dt.Columns.Add("GrandTotal", typeof(decimal));
            dt.PrimaryKey = new[] { dt.Columns["PReturnNo"] };
            return dt;
        }

        private static DataTable CreateReturnDetailTable()
        {
            DataTable dt = new DataTable("ReturnDetail");
            DataColumn idCol = dt.Columns.Add("DetailID", typeof(int));
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = 1;
            idCol.AutoIncrementStep = 1;

            dt.Columns.Add("PReturnNo", typeof(int));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("Cost", typeof(decimal));
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
            dt.Columns.Add("PReturnNo", typeof(int));
            dt.Columns.Add("PReturnDate", typeof(DateTime));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Paymode", typeof(string));
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("Cost", typeof(decimal));
            dt.Columns.Add("TaxPer", typeof(decimal));
            dt.Columns.Add("TaxAmt", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("Reason", typeof(string));
            return dt;
        }

        private static void AddReturnMasterRow(DataTable table, FilteredReturnRecord rec, bool includePeriodKey)
        {
            DataRow row = table.NewRow();
            if (includePeriodKey)
            {
                row["PeriodKey"] = rec.PeriodKey;
            }
            row["PeriodLabel"] = rec.PeriodLabel;
            row["PReturnNo"] = rec.Master.PReturnNo;
            row["PReturnDate"] = rec.Master.PReturnDate;
            row["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
            row["InvoiceDate"] = rec.Master.InvoiceDate != DateTime.MinValue ? (object)rec.Master.InvoiceDate : DBNull.Value;
            row["VendorName"] = rec.Master.VendorName ?? string.Empty;
            row["Paymode"] = rec.Master.Paymode ?? string.Empty;
            row["ItemCount"] = rec.Details.Count;
            row["TotalQty"] = rec.Details.Sum(d => Convert.ToDecimal(d.Qty));
            row["SubTotal"] = Convert.ToDecimal(rec.Master.SubTotal);
            row["GrandTotal"] = Convert.ToDecimal(rec.Master.GrandTotal);
            table.Rows.Add(row);
        }

        private static void AddReturnDetailRows(DataTable table, FilteredReturnRecord rec)
        {
            foreach (var d in rec.Details)
            {
                DataRow dRow = table.NewRow();
                dRow["PReturnNo"] = rec.Master.PReturnNo;
                dRow["SlNo"] = d.SlNo;
                dRow["ItemName"] = d.ItemName ?? string.Empty;
                dRow["Unit"] = d.Unit ?? string.Empty;
                dRow["Packing"] = d.Packing ?? string.Empty;
                dRow["Qty"] = Convert.ToDecimal(d.Qty);
                dRow["Cost"] = Convert.ToDecimal(d.Cost);
                dRow["TaxPer"] = Convert.ToDecimal(d.TaxPer);
                dRow["TaxAmt"] = Convert.ToDecimal(d.TaxAmt);
                dRow["Amount"] = Convert.ToDecimal(d.Amount);
                dRow["Reason"] = d.Reason ?? string.Empty;
                table.Rows.Add(dRow);
            }
        }

        private static void AddFlatItemRows(DataTable table, FilteredReturnRecord rec, bool includePeriodKey)
        {
            if (rec.Details == null || rec.Details.Count == 0)
            {
                DataRow fRow = table.NewRow();
                if (includePeriodKey)
                {
                    fRow["PeriodKey"] = rec.PeriodKey;
                }
                fRow["PeriodLabel"] = rec.PeriodLabel;
                fRow["PReturnNo"] = rec.Master.PReturnNo;
                fRow["PReturnDate"] = rec.Master.PReturnDate;
                fRow["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
                fRow["InvoiceDate"] = rec.Master.InvoiceDate != DateTime.MinValue ? (object)rec.Master.InvoiceDate : DBNull.Value;
                fRow["VendorName"] = rec.Master.VendorName ?? string.Empty;
                fRow["Paymode"] = rec.Master.Paymode ?? string.Empty;
                fRow["SlNo"] = 1;
                fRow["ItemName"] = "(No Items)";
                fRow["Unit"] = string.Empty;
                fRow["Packing"] = string.Empty;
                fRow["Qty"] = 0m;
                fRow["Cost"] = 0m;
                fRow["TaxPer"] = 0m;
                fRow["TaxAmt"] = 0m;
                fRow["Amount"] = Convert.ToDecimal(rec.Master.GrandTotal);
                fRow["Reason"] = string.Empty;
                table.Rows.Add(fRow);
                return;
            }

            foreach (var d in rec.Details)
            {
                DataRow fRow = table.NewRow();
                if (includePeriodKey)
                {
                    fRow["PeriodKey"] = rec.PeriodKey;
                }
                fRow["PeriodLabel"] = rec.PeriodLabel;
                fRow["PReturnNo"] = rec.Master.PReturnNo;
                fRow["PReturnDate"] = rec.Master.PReturnDate;
                fRow["InvoiceNo"] = rec.Master.InvoiceNo ?? string.Empty;
                fRow["InvoiceDate"] = rec.Master.InvoiceDate != DateTime.MinValue ? (object)rec.Master.InvoiceDate : DBNull.Value;
                fRow["VendorName"] = rec.Master.VendorName ?? string.Empty;
                fRow["Paymode"] = rec.Master.Paymode ?? string.Empty;
                fRow["SlNo"] = d.SlNo;
                fRow["ItemName"] = d.ItemName ?? string.Empty;
                fRow["Unit"] = d.Unit ?? string.Empty;
                fRow["Packing"] = d.Packing ?? string.Empty;
                fRow["Qty"] = Convert.ToDecimal(d.Qty);
                fRow["Cost"] = Convert.ToDecimal(d.Cost);
                fRow["TaxPer"] = Convert.ToDecimal(d.TaxPer);
                fRow["TaxAmt"] = Convert.ToDecimal(d.TaxAmt);
                fRow["Amount"] = Convert.ToDecimal(d.Amount);
                fRow["Reason"] = d.Reason ?? string.Empty;
                table.Rows.Add(fRow);
            }
        }
        #endregion

        #region UltraGrid Layout & Styling Events
        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                UltraGridLayout layout = e.Layout;
                layout.ViewStyleBand = ViewStyleBand.Vertical;
                layout.Override.RowSelectors = DefaultableBoolean.True;
                layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

                for (int b = 0; b < layout.Bands.Count; b++)
                {
                    UltraGridBand band = layout.Bands[b];
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                    }
                    ConfigureBand(band, b);
                }

                ReportGridLayoutHelper.ApplySavedLayout("PurchaseReturnAuditReport", gridReport, userHiddenColumnKeys);
                _filterManager?.UpdateHeaderFilterIcons();

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    PopulateColumnChooserListBox();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error initializing grid layout: " + ex.Message);
            }
        }

        private void ConfigureBand(UltraGridBand band, int bandIndex)
        {
            band.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band.Override.HeaderAppearance.ForeColor = Color.White;
            band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            band.Override.RowAppearance.BackColor = Color.White;
            band.Override.RowAlternateAppearance.BackColor = GridAltRow;
            band.Override.RowAppearance.BorderColor = GridRowLine;
            band.Override.RowAlternateAppearance.BorderColor = GridRowLine;

            if (band.Columns.Exists("PeriodKey"))
            {
                band.Columns["PeriodKey"].Hidden = true;
                band.Columns["PeriodKey"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }
            if (band.Columns.Exists("DetailID"))
            {
                band.Columns["DetailID"].Hidden = true;
                band.Columns["DetailID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            string bandName = band.Key;
            if (string.Equals(bandName, "PeriodSummary", StringComparison.OrdinalIgnoreCase))
            {
                band.Header.Caption = "Period Summary";
                band.HeaderVisible = true;
                FormatColumn(band, "PeriodLabel", "Period", 180, HAlign.Left);
                FormatColumn(band, "FromDate", "From Date", 100, HAlign.Center, "{0:dd/MM/yyyy}");
                FormatColumn(band, "ToDate", "To Date", 100, HAlign.Center, "{0:dd/MM/yyyy}");
                FormatColumn(band, "TotalReturns", "Returns Count", 100, HAlign.Right, "{0:N0}");
                FormatColumn(band, "TotalItems", "Items Count", 100, HAlign.Right, "{0:N0}");
                FormatColumn(band, "TotalQty", "Total Qty", 100, HAlign.Right, "{0:N2}");
                FormatColumn(band, "SubTotal", "Sub Total", 120, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "GrandTotal", "Total Return Amount", 140, HAlign.Right, "₹ {0:N2}");
            }
            else if (string.Equals(bandName, "ReturnMaster", StringComparison.OrdinalIgnoreCase))
            {
                band.Header.Caption = "Purchase Returns";
                band.HeaderVisible = true;
                FormatColumn(band, "PeriodLabel", "Period", 140, HAlign.Left);
                FormatColumn(band, "PReturnNo", "Return No", 90, HAlign.Center);
                FormatColumn(band, "PReturnDate", "Return Date", 100, HAlign.Center, "{0:dd/MM/yyyy}");
                FormatColumn(band, "InvoiceNo", "Original Inv / GRN No", 140, HAlign.Left);
                FormatColumn(band, "InvoiceDate", "Invoice Date", 100, HAlign.Center, "{0:dd/MM/yyyy}");
                FormatColumn(band, "VendorName", "Vendor Name", 220, HAlign.Left);
                FormatColumn(band, "Paymode", "Paymode", 100, HAlign.Center);
                FormatColumn(band, "ItemCount", "Items", 70, HAlign.Right, "{0:N0}");
                FormatColumn(band, "TotalQty", "Returned Qty", 95, HAlign.Right, "{0:N2}");
                FormatColumn(band, "SubTotal", "Sub Total", 110, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "GrandTotal", "Return Amount", 125, HAlign.Right, "₹ {0:N2}");
            }
            else if (string.Equals(bandName, "ReturnDetail", StringComparison.OrdinalIgnoreCase))
            {
                band.Header.Caption = "Return Item Details";
                band.HeaderVisible = true;
                FormatColumn(band, "PReturnNo", "Return No", 80, HAlign.Center);
                FormatColumn(band, "SlNo", "Sl No", 60, HAlign.Center);
                FormatColumn(band, "ItemName", "Item Description", 240, HAlign.Left);
                FormatColumn(band, "Unit", "Unit", 70, HAlign.Center);
                FormatColumn(band, "Packing", "Packing", 80, HAlign.Center);
                FormatColumn(band, "Qty", "Qty", 80, HAlign.Right, "{0:N2}");
                FormatColumn(band, "Cost", "Cost Price", 95, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "TaxPer", "Tax %", 75, HAlign.Right, "{0:N2}%");
                FormatColumn(band, "TaxAmt", "Tax Amount", 95, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "Amount", "Total Amount", 110, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "Reason", "Return Reason", 180, HAlign.Left);
            }
            else
            {
                // Flat Items band
                band.Header.Caption = "Returned Items (Flat View)";
                FormatColumn(band, "PeriodLabel", "Period", 130, HAlign.Left);
                FormatColumn(band, "PReturnNo", "Return No", 90, HAlign.Center);
                FormatColumn(band, "PReturnDate", "Return Date", 100, HAlign.Center, "{0:dd/MM/yyyy}");
                FormatColumn(band, "InvoiceNo", "Invoice No", 120, HAlign.Left);
                FormatColumn(band, "InvoiceDate", "Invoice Date", 100, HAlign.Center, "{0:dd/MM/yyyy}");
                FormatColumn(band, "VendorName", "Vendor Name", 190, HAlign.Left);
                FormatColumn(band, "Paymode", "Paymode", 90, HAlign.Center);
                FormatColumn(band, "SlNo", "Sl", 50, HAlign.Center);
                FormatColumn(band, "ItemName", "Item Description", 220, HAlign.Left);
                FormatColumn(band, "Unit", "Unit", 65, HAlign.Center);
                FormatColumn(band, "Packing", "Packing", 75, HAlign.Center);
                FormatColumn(band, "Qty", "Qty", 75, HAlign.Right, "{0:N2}");
                FormatColumn(band, "Cost", "Cost", 90, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "TaxPer", "Tax %", 70, HAlign.Right, "{0:N2}%");
                FormatColumn(band, "TaxAmt", "Tax Amount", 90, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "Amount", "Total Amount", 105, HAlign.Right, "₹ {0:N2}");
                FormatColumn(band, "Reason", "Return Reason", 160, HAlign.Left);
            }
        }

        private static void FormatColumn(UltraGridBand band, string key, string caption, int width, HAlign align, string format = null)
        {
            if (band.Columns.Exists(key))
            {
                UltraGridColumn col = band.Columns[key];
                col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
                col.Header.Caption = caption;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
                if (!string.IsNullOrEmpty(format))
                {
                    col.Format = format;
                }
            }
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            string bandKey = e.Row.Band.Key;
            if (string.Equals(bandKey, "PeriodSummary", StringComparison.OrdinalIgnoreCase))
            {
                e.Row.Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Appearance.BackColor = Color.FromArgb(240, 246, 255);
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
                // If user double clicks on a Return row or Item row, open the transaction in frmPurchaseReturn
                int returnNo = 0;
                if (e.Row.Cells.Exists("PReturnNo") && e.Row.Cells["PReturnNo"].Value != null)
                {
                    int.TryParse(Convert.ToString(e.Row.Cells["PReturnNo"].Value), out returnNo);
                }

                if (returnNo > 0)
                {
                    OpenPurchaseReturnTransaction(returnNo);
                }
            }
        }

        private void OpenPurchaseReturnTransaction(int returnNo)
        {
            try
            {
                Home homeInstance = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeInstance != null && !homeInstance.IsDisposed)
                {
                    homeInstance.OpenFormByType(typeof(frmPurchaseReturn), "Purchase Return");
                }
                else
                {
                    frmPurchaseReturn form = new frmPurchaseReturn();
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.Show();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error opening Purchase Return transaction: " + ex.Message);
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
            ShowReportPreview("PURCHASE RETURN AUDIT REPORT");
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            ShowReportPreview("PURCHASE RETURN AUDIT - PERIOD BREAKDOWN");
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportGridData();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.BeginUpdate();
                gridReport.Rows.ExpandAll(true);
            }
            finally
            {
                gridReport.EndUpdate();
                UpdateFooterCellPositions();
            }
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.BeginUpdate();
                gridReport.Rows.CollapseAll(true);
            }
            finally
            {
                gridReport.EndUpdate();
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
            if (btnToggleSelection != null)
            {
                btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
            }
        }

        private void btnVendorPicker_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void btnItemPicker_Click(object sender, EventArgs e)
        {
            OpenItemDialog();
        }

        private void ultraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            UpdatePeriodControlVisibility();
            SyncDateRangeWithPeriodControls();
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void FilterSelection_ValueChanged(object sender, EventArgs e)
        {
            SyncDateRangeWithPeriodControls();
            if (!_isLoading)
            {
                BuildAndBindReportData();
            }
        }

        private void ultraComboViewMode_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                BuildAndBindReportData();
            }
        }

        private void chkSingleDay_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePeriodControlVisibility();
            if (chkSingleDay.Checked && dtFrom.Value != null)
            {
                dtTo.Value = dtFrom.Value;
            }
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void dtFrom_ValueChanged(object sender, EventArgs e)
        {
            if (chkSingleDay.Checked && dtFrom.Value != null)
            {
                dtTo.Value = dtFrom.Value;
            }
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void dtTo_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                LoadReport();
            }
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

        private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

        private void purchasereturnauditreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
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
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("purchasereturnauditreport"))
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
                preview.Text = "Purchase Return Audit Report - Preview";
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
                    Text = string.IsNullOrWhiteSpace(title) ? "PURCHASE RETURN AUDIT REPORT" : title,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Bottom,
                    Height = 24,
                    Text = $"Period: {GetEffectiveFromDate():dd/MM/yyyy} to {GetEffectiveToDate():dd/MM/yyyy}   |   Total Returns: {records.Count:N0}   |   Grand Total: ₹ {records.Sum(r => Convert.ToDecimal(r.Master.GrandTotal)):N2}",
                    ForeColor = Color.FromArgb(230, 242, 255),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(titleLabel);
                header.Controls.Add(subtitleLabel);

                footer.Dock = DockStyle.Bottom;
                footer.Height = 44;
                footer.BackColor = ActionPanelBackColor;

                Button btnClose = new Button
                {
                    Text = "Close",
                    Size = new Size(95, 28),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Location = new Point(footer.Width - 110, 8),
                    BackColor = ButtonBlueTop,
                    ForeColor = ButtonTextBlue,
                    FlatStyle = FlatStyle.Flat
                };
                btnClose.Click += (s, e) => preview.Close();

                Button btnPrint = new Button
                {
                    Text = "🖨 Print",
                    Size = new Size(95, 28),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Location = new Point(footer.Width - 215, 8),
                    BackColor = ButtonBlueTop,
                    ForeColor = ButtonTextBlue,
                    FlatStyle = FlatStyle.Flat
                };
                btnPrint.Click += (s, e) =>
                {

                    try
                    {
                        previewGrid.Print();
                    }
                    catch (Exception exPrint)
                    {
                        MessageBox.Show("Printing error: " + exPrint.Message, "Print", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                footer.Controls.Add(btnClose);
                footer.Controls.Add(btnPrint);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.DisplayLayout.CopyFrom(gridReport.DisplayLayout);
                previewGrid.DataSource = gridReport.DataSource;
                previewGrid.DataMember = gridReport.DataMember;

                preview.Controls.Add(previewGrid);
                preview.Controls.Add(footer);
                preview.Controls.Add(header);

                preview.ShowDialog(this);
            }
        }

        private void ExportGridData()
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = ReportPdfExporter.ExcelCsvPdfFilter;
                sfd.FileName = $"PurchaseReturnAuditReport_{DateTime.Now:yyyyMMdd_HHmmss}";
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {

                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        if (ReportPdfExporter.CheckAndExportPdf(sfd, gridReport, "Purchase Return Audit Report")) return;
                        ExportGridToCsv(gridReport, sfd.FileName);
                        MessageBox.Show("Report successfully exported to:\n" + sfd.FileName, "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting report: " + ex.Message, "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }

        private static void ExportGridToCsv(UltraGrid grid, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            if (grid.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = grid.DisplayLayout.Bands[0];
                var visibleCols = band.Columns.Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered)
                    .OrderBy(c => c.Header.VisiblePosition)
                    .ToList();

                // Header row
                sb.AppendLine(string.Join(",", visibleCols.Select(c => EscapeCsv(c.Header.Caption ?? c.Key))));

                // Data rows
                foreach (UltraGridRow row in grid.Rows.GetFilteredInNonGroupByRows())
                {
                    if (row.IsDataRow && !row.Hidden)
                    {
                        var cellValues = visibleCols.Select(c =>
                        {
                            object val = row.Cells.Exists(c.Key) ? row.Cells[c.Key].Value : null;
                            return EscapeCsv(Convert.ToString(val));
                        });
                        sb.AppendLine(string.Join(",", cellValues));

                        // Export child rows if any
                        if (row.ChildBands != null)
                        {
                            foreach (UltraGridChildBand childBand in row.ChildBands)
                            {
                                foreach (UltraGridRow childRow in childBand.Rows)
                                {
                                    if (childRow.IsDataRow && !childRow.Hidden)
                                    {
                                        var childCols = childBand.Band.Columns.Cast<UltraGridColumn>()
                                            .Where(cc => !cc.Hidden && !cc.IsChaptered)
                                            .OrderBy(cc => cc.Header.VisiblePosition)
                                            .ToList();
                                        var childValues = childCols.Select(cc =>
                                        {
                                            object val = childRow.Cells.Exists(cc.Key) ? childRow.Cells[cc.Key].Value : null;
                                            return EscapeCsv(Convert.ToString(val));
                                        });
                                        sb.AppendLine("  ," + string.Join(",", childValues));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";
            string clean = text.Replace("\"", "\"\"");
            return $"\"{clean}\"";
        }

        private void ResetFilters()
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
                ultraComboViewMode.Value = "PERIOD_RETURNS_ITEMS";
                txtVendorSearch.Text = string.Empty;
                txtReturnNo.Text = string.Empty;
                txtItemFilter.Text = string.Empty;
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
                LoadReport();
            }
        }
        #endregion

        #region Period & Date Helpers
        private string GetPeriodMode()
        {
            return Convert.ToString(ultraComboPeriodMode.Value ?? ultraComboPeriodMode.Text ?? "MONTH_WISE");
        }

        private string GetViewMode()
        {
            return Convert.ToString(ultraComboViewMode.Value ?? ultraComboViewMode.Text ?? "PERIOD_RETURNS_ITEMS");
        }

        private void SyncDateRangeWithPeriodControls()
        {
            string mode = GetPeriodMode();
            DateTime today = DateTime.Today;

            if (string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
            {
                int month = Convert.ToInt32(ultraComboMonth.Value ?? today.Month);
                int year = Convert.ToInt32(ultraComboYear.Value ?? today.Year);
                if (year <= 0) year = today.Year;

                if (month >= 1 && month <= 12)
                {
                    DateTime start = new DateTime(year, month, 1);
                    DateTime end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                    dtFrom.Value = start;
                    dtTo.Value = end;
                }
                else
                {
                    // ALL Months of that Year
                    dtFrom.Value = new DateTime(year, 1, 1);
                    dtTo.Value = new DateTime(year, 12, 31);
                }
            }
            else if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                int year = Convert.ToInt32(ultraComboYear.Value ?? today.Year);
                if (year <= 0) year = today.Year;
                dtFrom.Value = new DateTime(year, 1, 1);
                dtTo.Value = new DateTime(year, 12, 31);
            }
            else if (string.Equals(mode, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                dtFrom.Value = new DateTime(2000, 1, 1);
                dtTo.Value = today;
            }
        }

        private DateTime GetEffectiveFromDate()
        {
            string mode = GetPeriodMode();
            DateTime today = DateTime.Today;

            if (string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
            {
                int month = Convert.ToInt32(ultraComboMonth.Value ?? today.Month);
                int year = Convert.ToInt32(ultraComboYear.Value ?? today.Year);
                if (year <= 0) year = today.Year;

                if (month >= 1 && month <= 12)
                    return new DateTime(year, month, 1);
                else
                    return new DateTime(year, 1, 1);
            }
            if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                int year = Convert.ToInt32(ultraComboYear.Value ?? today.Year);
                if (year <= 0) year = today.Year;
                return new DateTime(year, 1, 1);
            }
            if (string.Equals(mode, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return new DateTime(2000, 1, 1);
            }

            return dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : new DateTime(today.Year, today.Month, 1);
        }

        private DateTime GetEffectiveToDate()
        {
            string mode = GetPeriodMode();
            DateTime today = DateTime.Today;

            if (string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase))
            {
                int month = Convert.ToInt32(ultraComboMonth.Value ?? today.Month);
                int year = Convert.ToInt32(ultraComboYear.Value ?? today.Year);
                if (year <= 0) year = today.Year;

                if (month >= 1 && month <= 12)
                    return new DateTime(year, month, DateTime.DaysInMonth(year, month));
                else
                    return new DateTime(year, 12, 31);
            }
            if (string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase))
            {
                int year = Convert.ToInt32(ultraComboYear.Value ?? today.Year);
                if (year <= 0) year = today.Year;
                return new DateTime(year, 12, 31);
            }
            if (string.Equals(mode, "DAY_WISE", StringComparison.OrdinalIgnoreCase) && chkSingleDay.Checked)
            {
                return dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : today;
            }
            if (string.Equals(mode, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return today;
            }

            return dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : today;
        }
        #endregion
    }

    /// <summary>
    /// Alias class to ensure both 'purchasereturnauditreport' and 'frmpurchasereturnauditreport' resolve smoothly.
    /// </summary>
    public class frmpurchasereturnauditreport : purchasereturnauditreport
    {
    }
}
