using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Accounts;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmdatewiseexpensereport : Form
    {
        #region Colour Palette (Exact Parity with frmVendorOutstandingReport)
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

        // Expense Accent Colors
        private static readonly Color DirectExpenseColor   = Color.FromArgb(230, 81, 0);   // Dark Amber/Orange
        private static readonly Color IndirectExpenseColor = Color.FromArgb(13, 71, 161);  // Deep Navy
        private static readonly Color TotalExpenseColor    = Color.FromArgb(191, 54, 12);  // Crimson / Red
        private static readonly Color ZeroMutedColor       = Color.FromArgb(170, 170, 170);
        #endregion

        #region Helper Classes
        private sealed class PivotDateColumnInfo
        {
            public string ColumnKey { get; set; }
            public string HeaderCaption { get; set; }
            public string Tooltip { get; set; }
            public DateTime? Date { get; set; }
            public int DayNumber { get; set; }
            public int MonthNumber { get; set; }
        }

        private sealed class ExpenseLedgerSummary
        {
            public int LedgerID { get; set; }
            public string LedgerName { get; set; }
            public string GroupName { get; set; }
            public string ExpenseCategory { get; set; }
            public decimal TotalExpense { get; set; }
            public int VoucherCount { get; set; }
            public DateTime LastExpenseDate { get; set; }
            public string PrimaryPaymentMode { get; set; }
            public string BranchName { get; set; }
        }

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Private Fields
        private readonly DatewiseExpenseReportRepository _repository;
        private DatewiseExpenseReportData _reportData;
        private List<DatewiseExpenseReportRow> _reportRows;
        private List<AccountGroupDDLItem> _categories;
        private List<ExpenseLedgerDDLItem> _ledgers;
        private bool _isLoading;

        private readonly List<PivotDateColumnInfo> _currentMatrixDateColumns = new List<PivotDateColumnInfo>();
        private DataSet _matrixDataSet;
        private DataTable _matrixTable;

        // Dynamic footer panel and cell controls (exact parity with frmVendorOutstandingReport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Total",
            "TotalExpense",
            "ExpenseAmount",
            "AverageAmount",
            "AverageVoucher",
            "DirectExpense",
            "IndirectExpense",
            "Debit",
            "Credit"
        };

        // Header drag-to-hide & Column Chooser (exact parity with frmVendorOutstandingReport)
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private ReportColumnFilterManager _filterManager;
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        #endregion

        #region Constructors & Form Lifecycle
        public frmdatewiseexpensereport()
        {
            _repository = new DatewiseExpenseReportRepository();
            _reportData = new DatewiseExpenseReportData();
            _reportRows = new List<DatewiseExpenseReportRow>();
            _categories = new List<AccountGroupDDLItem>();
            _ledgers = new List<ExpenseLedgerDDLItem>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmdatewiseexpensereport_Load;
            btnViewGrid.Click += (s, e) => LoadReport();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPreviewReport.Click += (s, e) => PreviewReport();
            btnExportGrid.Click += (s, e) => ExportGridData();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnColumnChooser.Click += (s, e) => ShowColumnChooserForm();
            btnExpandAll.Click += (s, e) => ExpandAllBands();
            btnCollapseAll.Click += (s, e) => CollapseAllBands();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();
            btnExpenseLedgerPicker.Click += BtnExpenseLedgerPicker_Click;

            ultraComboPeriodMode.ValueChanged += UltraComboPeriodMode_ValueChanged;
            dtFrom.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtTo.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboMonth.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboYear.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboExpenseCategory.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboExpenseLedger.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboPaymentMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            txtSearch.ValueChanged += (s, e) => ApplySearchFilter();

            // Grid event registrations for footer cell sync and column drag
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
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

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Resize += (s, e) => UpdateFooterCellPositions();
            }

            KeyPreview = true;
            KeyDown += frmdatewiseexpensereport_KeyDown;
            FormClosing += frmdatewiseexpensereport_FormClosing;
        }

        private string GetLayoutKey() => "DatewiseExpenseReport";

        private void frmdatewiseexpensereport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);
                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void frmdatewiseexpensereport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
        }

        private void frmdatewiseexpensereport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                e.Handled = true;
                BtnExpenseLedgerPicker_Click(btnExpenseLedgerPicker, EventArgs.Empty);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                LoadReport();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    columnChooserForm.Hide();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Styling & Layout (Exact Parity with frmVendorOutstandingReport)
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelControls.Dock = DockStyle.Top;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelAction.Dock = DockStyle.Top;
                ultraPanelAction.Height = 45;
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

            StyleLabel(lblPeriodMode);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblViewMode);
            StyleLabel(lblExpenseCategory);
            StyleLabel(lblExpenseLedger);
            StyleLabel(lblPaymentMode);
            StyleLabel(lblSearch);
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnExpandAll);
            StyleClassicButton(btnCollapseAll);
            StyleClassicButton(btnToggleSelection);
            StylePickerButton(btnExpenseLedgerPicker);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
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
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboPeriodMode, true);
            StyleFilterCombo(ultraComboMonth, true);
            StyleFilterCombo(ultraComboYear, true);
            StyleFilterCombo(ultraComboViewMode, true);
            StyleFilterCombo(ultraComboExpenseCategory, true);
            StyleFilterCombo(ultraComboExpenseLedger, false);
            StyleFilterCombo(ultraComboPaymentMode, true);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);

            if (txtSearch != null)
            {
                txtSearch.UseAppStyling = false;
                txtSearch.UseOsThemes = DefaultableBoolean.False;
                txtSearch.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
                txtSearch.BorderStyle = UIElementBorderStyle.Solid;
                txtSearch.Appearance.BackColor = ControlBackColor;
                txtSearch.Appearance.BorderColor = SkyBlueOutline;
                txtSearch.Appearance.ForeColor = ControlTextColor;
                txtSearch.Appearance.FontData.Name = "Tahoma";
                txtSearch.Appearance.FontData.SizeInPoints = 9.5f;
            }
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = ControlTextColor;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9;
            label.Appearance.TextVAlign = VAlign.Middle;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
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
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
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
            editor.Appearance.FontData.SizeInPoints = 10;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
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

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            layout.ViewStyleBand = ViewStyleBand.Vertical;
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

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            // Period Modes
            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date Range");
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("ALL", "All Dates");
            ultraComboPeriodMode.Value = "DATE_RANGE";

            // Months
            ultraComboMonth.Items.Clear();
            string[] monthNames = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            for (int i = 1; i <= 12; i++)
            {
                ultraComboMonth.Items.Add(i, monthNames[i - 1]);
            }
            ultraComboMonth.Value = DateTime.Today.Month;

            // Years
            ultraComboYear.Items.Clear();
            int currentYear = DateTime.Today.Year;
            for (int y = currentYear - 5; y <= currentYear + 2; y++)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = currentYear;

            // View Modes
            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Expense vs Dates)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Voucher Level Breakdown)");
            ultraComboViewMode.Items.Add("DATE_SUMMARY", "Date-wise Summary (Date vs Expense)");
            ultraComboViewMode.Items.Add("EXPENSE_SUMMARY", "Category Summary (Category vs Expense)");
            ultraComboViewMode.Value = "DATE_MATRIX";

            // Payment Modes
            ultraComboPaymentMode.Items.Clear();
            ultraComboPaymentMode.Items.Add("ALL", "ALL Modes");
            ultraComboPaymentMode.Items.Add("CASH", "Cash");
            ultraComboPaymentMode.Items.Add("BANK", "Bank Transfer / Online");
            ultraComboPaymentMode.Items.Add("CHEQUE", "Cheque");
            ultraComboPaymentMode.Value = "ALL";

            // Expense Categories
            PopulateCategories();

            // Expense Ledgers
            PopulateLedgers();

            UpdatePeriodModeVisibility();
        }

        private void PopulateCategories()
        {
            ultraComboExpenseCategory.Items.Clear();
            ultraComboExpenseCategory.Items.Add(0, "ALL Categories");
            ultraComboExpenseCategory.Items.Add(10, "Direct Expenses");
            ultraComboExpenseCategory.Items.Add(12, "Indirect Expenses");

            try
            {
                _categories = _repository.GetExpenseCategories();
                foreach (var cat in _categories)
                {
                    if (cat.GroupID != 10 && cat.GroupID != 12)
                    {
                        ultraComboExpenseCategory.Items.Add(cat.GroupID, cat.GroupName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
            }

            ultraComboExpenseCategory.Value = 0;
        }

        private void PopulateLedgers()
        {
            ultraComboExpenseLedger.Items.Clear();
            ultraComboExpenseLedger.Items.Add(0, "ALL Expense Ledgers");

            try
            {
                _ledgers = _repository.GetExpenseLedgers();
                foreach (var led in _ledgers)
                {
                    ultraComboExpenseLedger.Items.Add(led.LedgerID, led.LedgerName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ledgers: {ex.Message}");
            }

            ultraComboExpenseLedger.Value = 0;
        }

        private void UltraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            UpdatePeriodModeVisibility();
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void UpdatePeriodModeVisibility()
        {
            string mode = ultraComboPeriodMode.Value?.ToString() ?? "DATE_RANGE";

            bool isMonthWise = string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase);
            bool isYearWise  = string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase);
            bool isDateRange = string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = isDateRange;
            dtFrom.Visible      = isDateRange;
            lblToDate.Visible   = isDateRange;
            dtTo.Visible        = isDateRange;

            lblMonth.Visible    = isMonthWise;
            ultraComboMonth.Visible = isMonthWise;

            lblYear.Visible     = isMonthWise || isYearWise;
            ultraComboYear.Visible = isMonthWise || isYearWise;
        }
        #endregion

        #region Report Data Loading & Binding
        private void LoadReport()
        {
            if (_isLoading) return;

            Cursor prevCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                DatewiseExpenseReportFilter filter = BuildFilterFromControls();
                _reportData = _repository.GetReportData(filter);
                _reportRows = _reportData.ExpenseTransactions ?? new List<DatewiseExpenseReportRow>();

                if (filter.ViewMode == "DATE_MATRIX")
                {
                    BindMatrixView(filter);
                }
                else
                {
                    BindDirectTable(filter);
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating Date-wise Expense Report:\n{ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = prevCursor;
            }
        }

        private DatewiseExpenseReportFilter BuildFilterFromControls()
        {
            DatewiseExpenseReportFilter f = new DatewiseExpenseReportFilter();
            f.PeriodMode = ultraComboPeriodMode.Value?.ToString() ?? "DATE_RANGE";
            f.ViewMode   = ultraComboViewMode.Value?.ToString() ?? "DATE_MATRIX";

            if (f.PeriodMode == "MONTH_WISE")
            {
                f.Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : DateTime.Today.Month;
                f.Year  = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : DateTime.Today.Year;
                f.FromDate = new DateTime(f.Year, f.Month, 1);
                f.ToDate   = f.FromDate.AddMonths(1).AddSeconds(-1);
            }
            else if (f.PeriodMode == "YEAR_WISE")
            {
                f.Year  = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : DateTime.Today.Year;
                f.FromDate = new DateTime(f.Year, 1, 1);
                f.ToDate   = f.FromDate.AddYears(1).AddSeconds(-1);
            }
            else if (f.PeriodMode == "ALL")
            {
                f.FromDate = new DateTime(2000, 1, 1);
                f.ToDate   = new DateTime(2099, 12, 31, 23, 59, 59);
            }
            else
            {
                f.FromDate = dtFrom.DateTime != DateTime.MinValue ? dtFrom.DateTime.Date : DateTime.Today;
                f.ToDate   = dtTo.DateTime != DateTime.MinValue ? dtTo.DateTime.Date.AddDays(1).AddSeconds(-1) : DateTime.Today.AddDays(1).AddSeconds(-1);
            }

            f.ExpenseCategoryId = ultraComboExpenseCategory.Value != null ? Convert.ToInt32(ultraComboExpenseCategory.Value) : 0;
            f.LedgerId          = ultraComboExpenseLedger.Value != null ? Convert.ToInt32(ultraComboExpenseLedger.Value) : 0;
            f.PaymentMode       = ultraComboPaymentMode.Value?.ToString() ?? "ALL";
            f.SearchText        = (txtSearch.Text ?? string.Empty).Trim();
            f.CompanyId         = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : 0;
            f.BranchId          = SessionContext.BranchId > 0 ? SessionContext.BranchId : 0;

            return f;
        }

        private void BindMatrixView(DatewiseExpenseReportFilter filter)
        {
            _currentMatrixDateColumns.Clear();
            _matrixDataSet = BuildMatrixDataSet(_reportData, _reportRows, filter, out List<PivotDateColumnInfo> dateColumns);
            _currentMatrixDateColumns.AddRange(dateColumns);

            gridReport.DataSource = null;
            gridReport.DataSource = _matrixDataSet;
            gridReport.DataMember = "ExpenseMatrix";

            if (_matrixDataSet.Tables.Count > 0)
            {
                _matrixTable = _matrixDataSet.Tables["ExpenseMatrix"];
            }

            ConfigureMatrixBands();
        }

        private void BindDirectTable(DatewiseExpenseReportFilter filter)
        {
            gridReport.DataSource = null;

            if (_reportData.ReportDataSet != null && _reportData.ReportDataSet.Tables.Count > 0)
            {
                DataTable table = _reportData.ReportDataSet.Tables[0];
                gridReport.DataSource = table;

                if (gridReport.DisplayLayout.Bands.Count > 0)
                {
                    var band = gridReport.DisplayLayout.Bands[0];
                    FormatDirectTableBand(band, filter.ViewMode);
                }
            }
        }
        #endregion

        #region Matrix Building (Band 0 & Band 1)
        private DataSet BuildMatrixDataSet(DatewiseExpenseReportData reportData, List<DatewiseExpenseReportRow> rawRows, DatewiseExpenseReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable vouchersTable = BuildVouchersDataTable(reportData, rawRows);

            DataSet ds = new DataSet("DatewiseExpenseDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(vouchersTable);

            DataRelation relation = new DataRelation("ExpenseVoucherBreakdown", matrixTable.Columns["LedgerKey"], vouchersTable.Columns["LedgerKey"], false);
            ds.Relations.Add(relation);

            return ds;
        }

        private DataTable BuildMatrixDataTable(List<DatewiseExpenseReportRow> rawRows, DatewiseExpenseReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            dateColumns = new List<PivotDateColumnInfo>();
            string periodMode = filter.PeriodMode ?? "DATE_RANGE";

            if (periodMode == "YEAR_WISE")
            {
                string[] monthShortNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
                for (int m = 1; m <= 12; m++)
                {
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = monthShortNames[m - 1],
                        HeaderCaption = monthShortNames[m - 1],
                        Tooltip = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[m - 1],
                        MonthNumber = m
                    });
                }
            }
            else if (periodMode == "MONTH_WISE")
            {
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;
                int month = filter.Month >= 1 && filter.Month <= 12 ? filter.Month : DateTime.Today.Month;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime colDate = new DateTime(year, month, day);
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = day.ToString(),
                        HeaderCaption = day.ToString(),
                        Tooltip = $"{colDate:dd-MMM-yyyy (dddd)}",
                        Date = colDate,
                        DayNumber = day
                    });
                }
            }
            else // DATE_RANGE or ALL
            {
                DateTime start = filter.FromDate.Date;
                DateTime end = filter.ToDate.Date;
                int totalDays = (int)(end - start).TotalDays + 1;

                if (totalDays > 0 && totalDays <= 31)
                {
                    for (int i = 0; i < totalDays; i++)
                    {
                        DateTime d = start.AddDays(i);
                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = $"D_{d:yyyyMMdd}",
                            HeaderCaption = d.ToString("dd-MMM"),
                            Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                            Date = d,
                            DayNumber = d.Day
                        });
                    }
                }
                else
                {
                    var distinctDates = (rawRows ?? new List<DatewiseExpenseReportRow>())
                        .Select(r => r.VoucherDate.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    if (distinctDates.Count == 0)
                    {
                        distinctDates.Add(start);
                    }

                    foreach (var d in distinctDates)
                    {
                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = $"D_{d:yyyyMMdd}",
                            HeaderCaption = d.ToString("dd-MMM"),
                            Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                            Date = d,
                            DayNumber = d.Day
                        });
                    }
                }
            }

            // Create DataTable Schema
            DataTable table = new DataTable("ExpenseMatrix");
            table.Columns.Add("LedgerKey", typeof(string));
            table.Columns.Add("ExpenseCategory", typeof(string));
            table.Columns.Add("GroupName", typeof(string));
            table.Columns.Add("LedgerName", typeof(string));

            foreach (var col in dateColumns)
            {
                var dc = table.Columns.Add(col.ColumnKey, typeof(decimal));
                dc.DefaultValue = 0m;
            }

            var cTotal = table.Columns.Add("TotalExpense", typeof(decimal));
            cTotal.DefaultValue = 0m;

            var cCount = table.Columns.Add("VoucherCount", typeof(int));
            cCount.DefaultValue = 0;

            var cAvg = table.Columns.Add("AverageVoucher", typeof(decimal));
            cAvg.DefaultValue = 0m;

            var cLastDate = table.Columns.Add("LastExpenseDate", typeof(DateTime));

            var cMode = table.Columns.Add("PrimaryPaymentMode", typeof(string));
            cMode.DefaultValue = string.Empty;

            var cBranch = table.Columns.Add("BranchName", typeof(string));
            cBranch.DefaultValue = string.Empty;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Aggregation by ledger and date
            Dictionary<string, decimal> dateExpenseMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ExpenseLedgerSummary> ledgerSummaryMap = new Dictionary<string, ExpenseLedgerSummary>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, HashSet<int>> ledgerVouchersMap = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseExpenseReportRow r in rawRows)
            {
                string ledgerKey = r.LedgerID > 0 ? r.LedgerID.ToString() : (r.LedgerName ?? string.Empty);

                ExpenseLedgerSummary summary;
                if (!ledgerSummaryMap.TryGetValue(ledgerKey, out summary))
                {
                    summary = new ExpenseLedgerSummary
                    {
                        LedgerID = r.LedgerID,
                        LedgerName = !string.IsNullOrEmpty(r.LedgerName) ? r.LedgerName : "Unknown Expense",
                        GroupName = r.GroupName ?? "EXPENSES",
                        ExpenseCategory = r.ExpenseCategory ?? "Operating Expense",
                        TotalExpense = 0m,
                        VoucherCount = 0,
                        LastExpenseDate = r.VoucherDate,
                        PrimaryPaymentMode = r.PaymentMode ?? "CASH",
                        BranchName = r.BranchName ?? string.Empty
                    };
                    ledgerSummaryMap[ledgerKey] = summary;
                    ledgerVouchersMap[ledgerKey] = new HashSet<int>();
                }
                else
                {
                    if (r.VoucherDate > summary.LastExpenseDate)
                    {
                        summary.LastExpenseDate = r.VoucherDate;
                        if (!string.IsNullOrEmpty(r.PaymentMode))
                            summary.PrimaryPaymentMode = r.PaymentMode;
                    }
                }

                summary.TotalExpense += r.ExpenseAmount;
                if (r.VoucherID > 0)
                {
                    ledgerVouchersMap[ledgerKey].Add(r.VoucherID);
                }

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    bool isMatch = false;
                    if (periodMode == "YEAR_WISE")
                    {
                        isMatch = (r.VoucherDate.Month == dateCol.MonthNumber);
                    }
                    else if (periodMode == "MONTH_WISE")
                    {
                        isMatch = (r.VoucherDate.Day == dateCol.DayNumber);
                    }
                    else
                    {
                        if (dateCol.Date.HasValue)
                        {
                            isMatch = (r.VoucherDate.Date == dateCol.Date.Value.Date);
                        }
                    }

                    if (isMatch)
                    {
                        string cellKey = $"{ledgerKey}_{dateCol.ColumnKey}";
                        if (!dateExpenseMap.ContainsKey(cellKey))
                            dateExpenseMap[cellKey] = 0m;
                        dateExpenseMap[cellKey] += r.ExpenseAmount;
                    }
                }
            }

            // Populate rows
            var sortedSummaries = ledgerSummaryMap.Values
                .OrderBy(s => s.ExpenseCategory == "Direct Expense" ? 0 : 1)
                .ThenBy(s => s.GroupName)
                .ThenBy(s => s.LedgerName)
                .ToList();

            foreach (var s in sortedSummaries)
            {
                string ledgerKey = s.LedgerID > 0 ? s.LedgerID.ToString() : s.LedgerName;
                DataRow dr = table.NewRow();
                dr["LedgerKey"] = ledgerKey;
                dr["ExpenseCategory"] = s.ExpenseCategory;
                dr["GroupName"] = s.GroupName;
                dr["LedgerName"] = s.LedgerName;

                decimal rowTotal = 0m;
                foreach (PivotDateColumnInfo col in dateColumns)
                {
                    string cellKey = $"{ledgerKey}_{col.ColumnKey}";
                    decimal val = dateExpenseMap.ContainsKey(cellKey) ? dateExpenseMap[cellKey] : 0m;
                    dr[col.ColumnKey] = val;
                    rowTotal += val;
                }

                dr["TotalExpense"] = s.TotalExpense;
                int vCount = ledgerVouchersMap.ContainsKey(ledgerKey) ? ledgerVouchersMap[ledgerKey].Count : 0;
                dr["VoucherCount"] = vCount;
                dr["AverageVoucher"] = vCount > 0 ? Math.Round(s.TotalExpense / vCount, 2) : 0m;
                if (s.LastExpenseDate != DateTime.MinValue)
                {
                    dr["LastExpenseDate"] = s.LastExpenseDate.Date;
                }
                dr["PrimaryPaymentMode"] = s.PrimaryPaymentMode;
                dr["BranchName"] = s.BranchName;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildVouchersDataTable(DatewiseExpenseReportData reportData, List<DatewiseExpenseReportRow> rawRows)
        {
            DataTable table = new DataTable("ExpenseVoucherBreakdown");
            table.Columns.Add("LedgerKey", typeof(string));
            table.Columns.Add("VoucherDate", typeof(DateTime));
            table.Columns.Add("VoucherNumber", typeof(string));
            table.Columns.Add("VoucherType", typeof(string));
            table.Columns.Add("ExpenseCategory", typeof(string));
            table.Columns.Add("ExpenseAmount", typeof(decimal));
            table.Columns.Add("PaidThrough", typeof(string));
            table.Columns.Add("PaymentMode", typeof(string));
            table.Columns.Add("Narration", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("BranchName", typeof(string));

            var sourceRows = reportData.VoucherDetails != null && reportData.VoucherDetails.Count > 0
                ? reportData.VoucherDetails.Select(v => new DatewiseExpenseReportRow
                {
                    LedgerID = v.LedgerID,
                    VoucherID = v.VoucherID,
                    VoucherNumber = v.VoucherNumber,
                    VoucherDate = v.VoucherDate,
                    VoucherType = v.VoucherType,
                    ExpenseCategory = v.ExpenseCategory,
                    ExpenseAmount = v.ExpenseAmount,
                    PaidThrough = v.PaidThrough,
                    PaymentMode = v.PaymentMode,
                    Narration = v.Narration,
                    UserName = v.UserName,
                    BranchName = v.BranchName
                }).ToList()
                : (rawRows ?? new List<DatewiseExpenseReportRow>());

            foreach (var r in sourceRows.OrderBy(x => x.LedgerName).ThenBy(x => x.VoucherDate).ThenBy(x => x.VoucherID))
            {
                DataRow dr = table.NewRow();
                dr["LedgerKey"] = r.LedgerID > 0 ? r.LedgerID.ToString() : (r.LedgerName ?? string.Empty);
                if (r.VoucherDate != DateTime.MinValue)
                    dr["VoucherDate"] = r.VoucherDate.Date;
                dr["VoucherNumber"] = !string.IsNullOrEmpty(r.VoucherNumber) ? r.VoucherNumber : r.VoucherID.ToString();
                dr["VoucherType"] = r.VoucherType ?? "GENPAY";
                dr["ExpenseCategory"] = r.ExpenseCategory ?? "Operating Expense";
                dr["ExpenseAmount"] = r.ExpenseAmount;
                dr["PaidThrough"] = !string.IsNullOrEmpty(r.PaidThrough) ? r.PaidThrough : "CASH-IN-HAND";
                dr["PaymentMode"] = !string.IsNullOrEmpty(r.PaymentMode) ? r.PaymentMode : "CASH";
                dr["Narration"] = !string.IsNullOrEmpty(r.Narration) ? r.Narration : "-";
                dr["UserName"] = r.UserName ?? "Admin";
                dr["BranchName"] = r.BranchName ?? string.Empty;
                table.Rows.Add(dr);
            }

            return table;
        }
        #endregion

        #region Band Configuration & Formatting (Exact Parity with frmVendorOutstandingReport)
        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
            column.Hidden = userHiddenColumnKeys.Contains(key);
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

        private void ConfigureChildGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            column.Hidden = false;
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

        private void ConfigureMatrixBands()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            // Band 0 (Expense Matrix)
            var band0 = gridReport.DisplayLayout.Bands[0];
            int pos = 0;

            if (band0.Columns.Exists("LedgerKey"))
            {
                band0.Columns["LedgerKey"].Hidden = true;
                band0.Columns["LedgerKey"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            ConfigureGridColumn(band0, "ExpenseCategory", "Category", 110, null, HAlign.Left, pos++);
            ConfigureGridColumn(band0, "GroupName", "Group", 130, null, HAlign.Left, pos++);
            ConfigureGridColumn(band0, "LedgerName", "Expense Ledger", 210, null, HAlign.Left, pos++);

            if (band0.Columns.Exists("LedgerName"))
            {
                band0.Columns["LedgerName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            // Dynamic date columns
            foreach (var dateCol in _currentMatrixDateColumns)
            {
                if (band0.Columns.Exists(dateCol.ColumnKey))
                {
                    ConfigureGridColumn(band0, dateCol.ColumnKey, dateCol.HeaderCaption, 65, "#,##0.00", HAlign.Right, pos++);
                    band0.Columns[dateCol.ColumnKey].Header.ToolTipText = dateCol.Tooltip;
                }
            }

            ConfigureGridColumn(band0, "TotalExpense", "Total Expense", 105, "#,##0.00", HAlign.Right, pos++);
            ConfigureGridColumn(band0, "VoucherCount", "Vouchers", 65, null, HAlign.Center, pos++);
            ConfigureGridColumn(band0, "AverageVoucher", "Avg Voucher", 90, "#,##0.00", HAlign.Right, pos++);
            ConfigureGridColumn(band0, "LastExpenseDate", "Last Date", 90, "dd-MMM-yyyy", HAlign.Left, pos++);
            ConfigureGridColumn(band0, "PrimaryPaymentMode", "Primary Mode", 85, null, HAlign.Left, pos++);
            ConfigureGridColumn(band0, "BranchName", "Branch", 100, null, HAlign.Left, pos++);

            if (band0.Columns.Exists("TotalExpense"))
            {
                band0.Columns["TotalExpense"].CellAppearance.ForeColor = TotalExpenseColor;
                band0.Columns["TotalExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            // Band 1 (Child: Expense Vouchers Breakdown)
            if (gridReport.DisplayLayout.Bands.Count > 1)
            {
                var band1 = gridReport.DisplayLayout.Bands[1];
                band1.HeaderVisible = true;
                band1.Header.Caption = "Expense Vouchers Breakdown (Double-click row to open Voucher)";
                band1.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                band1.Header.Appearance.ForeColor = Color.White;
                band1.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                band1.Override.AllowAddNew = AllowAddNew.No;
                band1.Override.AllowDelete = DefaultableBoolean.False;
                band1.Override.AllowUpdate = DefaultableBoolean.False;
                band1.Override.RowSelectors = DefaultableBoolean.False;
                band1.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                band1.Override.HeaderAppearance.ForeColor = Color.White;
                band1.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                band1.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);

                foreach (UltraGridColumn column in band1.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                int cpos = 0;
                ConfigureChildGridColumn(band1, "VoucherDate", "Date", 95, "dd-MMM-yyyy", HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "VoucherNumber", "Voucher No", 100, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "VoucherType", "Type", 75, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "ExpenseCategory", "Category", 105, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "ExpenseAmount", "Amount", 95, "#,##0.00", HAlign.Right, cpos++);
                ConfigureChildGridColumn(band1, "PaidThrough", "Paid Through", 130, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "PaymentMode", "Mode", 70, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "Narration", "Narration / Remarks", 200, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "UserName", "Created By", 85, null, HAlign.Left, cpos++);
                ConfigureChildGridColumn(band1, "BranchName", "Branch", 95, null, HAlign.Left, cpos++);

                if (band1.Columns.Exists("ExpenseAmount"))
                {
                    band1.Columns["ExpenseAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                    band1.Columns["ExpenseAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                // Child summaries matching FrmPayment / Vendor Payment breakdown
                band1.Override.SummaryDisplayArea = SummaryDisplayAreas.Bottom;
                band1.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;
                band1.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(235, 243, 252);
                band1.Override.SummaryValueAppearance.FontData.Bold = DefaultableBoolean.True;

                if (!band1.Summaries.Exists("Summary_VoucherCount") && band1.Columns.Exists("VoucherDate"))
                {
                    var sumCount = band1.Summaries.Add("Summary_VoucherCount", SummaryType.Count, band1.Columns["VoucherDate"]);
                    sumCount.DisplayFormat = "Vouchers: {0}";
                    sumCount.Appearance.TextHAlign = HAlign.Left;
                }
                if (!band1.Summaries.Exists("Summary_ExpenseAmount") && band1.Columns.Exists("ExpenseAmount"))
                {
                    var sumAmt = band1.Summaries.Add("Summary_ExpenseAmount", SummaryType.Sum, band1.Columns["ExpenseAmount"]);
                    sumAmt.DisplayFormat = "{0:#,##0.00}";
                    sumAmt.Appearance.TextHAlign = HAlign.Right;
                    sumAmt.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                }
            }

            ReportGridLayoutHelper.ApplySavedLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private static void FormatDirectTableBand(UltraGridBand band, string viewMode)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Header.Appearance.BorderColor = GridRowLine;
                col.CellAppearance.BorderColor = GridRowLine;
                col.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                col.CellAppearance.FontData.SizeInPoints = 8.25F;

                if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(float))
                {
                    col.Format = "#,##0.00";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                }
                else if (col.DataType == typeof(DateTime))
                {
                    col.Format = "dd-MMM-yyyy";
                    col.CellAppearance.TextHAlign = HAlign.Left;
                }
            }

            if (band.Columns.Exists("DirectExpense"))
            {
                band.Columns["DirectExpense"].CellAppearance.ForeColor = DirectExpenseColor;
                band.Columns["DirectExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("IndirectExpense"))
            {
                band.Columns["IndirectExpense"].CellAppearance.ForeColor = IndirectExpenseColor;
                band.Columns["IndirectExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("TotalExpense"))
            {
                band.Columns["TotalExpense"].CellAppearance.ForeColor = TotalExpenseColor;
                band.Columns["TotalExpense"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("PercentageOfTotal"))
            {
                band.Columns["PercentageOfTotal"].Header.Caption = "% of Total";
                band.Columns["PercentageOfTotal"].Format = "0.00'%'";
            }
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
                if (column.Hidden)
                    continue;

                ContentAlignment align = ContentAlignment.MiddleCenter;
                if (column.CellAppearance.TextHAlign == HAlign.Right || IsSummableColumn(column))
                {
                    align = ContentAlignment.MiddleRight;
                }
                else if (column.CellAppearance.TextHAlign == HAlign.Left)
                {
                    align = ContentAlignment.MiddleLeft;
                }

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
                    _columnAggregations[column.Key] = (summaryDefaultNumericColumns.Contains(column.Key) || IsSummableColumn(column)) ? "Sum" : "None";
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
                    : ((summaryDefaultNumericColumns.Contains(columnKey) || isNumeric) ? "Sum" : "None");

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
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
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

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            UltraGridColumn firstVisibleCol = band.Columns.Cast<UltraGridColumn>()
                .Where(c => !c.Hidden && c.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                .OrderBy(c => c.Header.VisiblePosition)
                .FirstOrDefault();

            string firstColKey = firstVisibleCol?.Key;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!string.IsNullOrEmpty(firstColKey) && string.Equals(columnKey, firstColKey, StringComparison.OrdinalIgnoreCase) && !IsSummableColumn(firstVisibleCol))
                {
                    string unit = "Expenses";
                    string vMode = ultraComboViewMode.Value?.ToString() ?? "DATE_MATRIX";
                    if (vMode == "DATE_MATRIX") unit = "Expenses";
                    else if (vMode == "DETAILED") unit = "Vouchers";
                    else if (vMode == "DATE_SUMMARY") unit = "Days";
                    else if (vMode == "EXPENSE_SUMMARY") unit = "Categories";

                    footerLabel.Text = $"Total ({visibleRows.Count} {unit})";
                    footerLabel.TextAlign = ContentAlignment.MiddleLeft;
                    footerLabel.Tag = Tuple.Create(columnKey, footerLabel.Text);
                    footerLabel.ForeColor = Color.White;
                    footerLabel.Invalidate();
                    continue;
                }

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
                if (row != null && row.IsDataRow && !row.IsFilteredOut && !row.Hidden)
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
            string clean = Convert.ToString(value).Replace("₹", "").Replace(",", "").Trim();
            return decimal.TryParse(clean, out result) ? result : (decimal?)null;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null) return false;
            return summaryDefaultNumericColumns.Contains(column.Key) ||
                   _currentMatrixDateColumns.Any(d => string.Equals(d.ColumnKey, column.Key, StringComparison.OrdinalIgnoreCase)) ||
                   column.DataType == typeof(decimal) ||
                   column.DataType == typeof(double) ||
                   column.DataType == typeof(float) ||
                   column.DataType == typeof(int) ||
                   column.DataType == typeof(long) ||
                   column.DataType == typeof(short);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser (Exact Parity with frmVendorOutstandingReport)
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

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.AllowDrop = true;
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;
            _filterManager = new ReportColumnFilterManager(gridReport, () => {
                UpdateFooterCellPositions();
                UpdateFooterValues();
            });
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null)
                {
                    ShowHeaderContextMenu(headerUI.Header.Column, pt);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                if (_filterManager != null && _filterManager.HandleMouseDown(e))
                {
                    isDraggingHeaderToHide = false;
                    columnBeingDragged = null;
                    return;
                }

                Point pt = new Point(e.X, e.Y);
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null)
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
            if (!isDraggingHeaderToHide && _filterManager != null && e.Button != MouseButtons.Left)
            {
                if (_filterManager.HandleMouseMove(e, headerToolTip))
                    return;
            }

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
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (!col.Hidden)
                {
                    UIElement hUI = col.Header.GetUIElement();
                    if (hUI != null && pt.X >= hUI.Rect.Left && pt.X <= hUI.Rect.Right)
                    {
                        return col.Header.VisiblePosition;
                    }
                }
            }

            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null) return;
            userHiddenColumnKeys.Add(col.Key);
            col.Hidden = true;
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);
            if (_filterManager != null)
            {
                menu.Items.Add(new ToolStripSeparator());
                _filterManager.AddFilterContextMenuItems(menu, col, location);
            }

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
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

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox?.SelectedItem is ColumnChooserItem item)
            {
                int pos = gridReport.DisplayLayout.Bands.Count > 0 ? gridReport.DisplayLayout.Bands[0].Columns.Count : 0;
                UnhideColumn(item.ColumnKey, pos);
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            string filter = txtColumnSearch?.Text?.Trim() ?? string.Empty;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.Caption))
            {
                if (col.Hidden && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filter) || caption.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
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

        private void UnhideColumn(string columnKey, int targetPosition)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            if (band.Columns.Exists(columnKey))
            {
                userHiddenColumnKeys.Remove(columnKey);
                UltraGridColumn col = band.Columns[columnKey];
                col.Hidden = false;
                col.Header.VisiblePosition = targetPosition;
                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
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

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    col.Hidden = false;
                }
            }
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
            }
        }
        #endregion

        #region Grid Events & Interactions
        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            // Initial layout setup handled in ConfigureMatrixBands or FormatDirectTableBand
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Band.Index == 0)
            {
                // Mute zero values across date columns
                foreach (var dateCol in _currentMatrixDateColumns)
                {
                    if (e.Row.Band.Columns.Exists(dateCol.ColumnKey))
                    {
                        var cell = e.Row.Cells[dateCol.ColumnKey];
                        if (cell.Value != null && cell.Value != DBNull.Value)
                        {
                            decimal val = Convert.ToDecimal(cell.Value);
                            if (val == 0m)
                            {
                                cell.Appearance.ForeColor = ZeroMutedColor;
                            }
                            else
                            {
                                cell.Appearance.ForeColor = TotalExpenseColor;
                                cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                            }
                        }
                    }
                }

                // Category badge colors
                if (e.Row.Band.Columns.Exists("ExpenseCategory"))
                {
                    string cat = e.Row.Cells["ExpenseCategory"].Value?.ToString() ?? string.Empty;
                    if (string.Equals(cat, "Direct Expense", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["ExpenseCategory"].Appearance.ForeColor = DirectExpenseColor;
                        e.Row.Cells["ExpenseCategory"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                    else if (string.Equals(cat, "Indirect Expense", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["ExpenseCategory"].Appearance.ForeColor = IndirectExpenseColor;
                        e.Row.Cells["ExpenseCategory"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
            }
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null) return;

            // Double clicking Band 1 (Child Voucher): open FrmGeneralPayment
            if (e.Row.Band.Index == 1)
            {
                string vNo = e.Row.Cells["VoucherNumber"].Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(vNo))
                {
                    try
                    {
                        FrmGeneralPayment paymentForm = new FrmGeneralPayment(vNo);
                        paymentForm.ShowDialog(this);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open Voucher #{vNo}: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else if (e.Row.Band.Index == 0)
            {
                // Toggle row expansion
                e.Row.Expanded = !e.Row.Expanded;
            }
        }
        #endregion

        #region Actions & Navigation
        private void BtnExpenseLedgerPicker_Click(object sender, EventArgs e)
        {
            try
            {
                using (Form pickerForm = new Form())
                {
                    pickerForm.Text = "Select Expense Ledger";
                    pickerForm.Size = new Size(500, 450);
                    pickerForm.StartPosition = FormStartPosition.CenterParent;
                    pickerForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    pickerForm.MaximizeBox = false;
                    pickerForm.MinimizeBox = false;
                    pickerForm.BackColor = FormBackColor;

                    TextBox txtFilter = new TextBox
                    {
                        Dock = DockStyle.Top,
                        Font = new Font("Segoe UI", 10F),
                        Height = 28
                    };

                    ListBox lst = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 9.5F),
                        IntegralHeight = false
                    };

                    Action populateList = () =>
                    {
                        lst.Items.Clear();
                        string q = txtFilter.Text.Trim().ToLowerInvariant();
                        lst.Items.Add(new ExpenseLedgerDDLItem { LedgerID = 0, LedgerName = "ALL Expense Ledgers" });
                        foreach (var l in _ledgers)
                        {
                            if (string.IsNullOrEmpty(q) || l.LedgerName.ToLowerInvariant().Contains(q) || l.GroupName.ToLowerInvariant().Contains(q))
                            {
                                lst.Items.Add(l);
                            }
                        }
                        if (lst.Items.Count > 0) lst.SelectedIndex = 0;
                    };

                    txtFilter.TextChanged += (s, ev) => populateList();
                    lst.Format += (s, ev) =>
                    {
                        if (ev.ListItem is ExpenseLedgerDDLItem itm)
                        {
                            ev.Value = itm.LedgerID > 0 ? $"{itm.LedgerName} ({itm.ExpenseCategory})" : itm.LedgerName;
                        }
                    };

                    Button btnSelect = new Button
                    {
                        Text = "Select",
                        Dock = DockStyle.Bottom,
                        Height = 36,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = ButtonBlueTop,
                        ForeColor = ButtonTextBlue,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                    };

                    Action doSelect = () =>
                    {
                        if (lst.SelectedItem is ExpenseLedgerDDLItem sel)
                        {
                            ultraComboExpenseLedger.Value = sel.LedgerID;
                            pickerForm.DialogResult = DialogResult.OK;
                            pickerForm.Close();
                            LoadReport();
                        }
                    };

                    btnSelect.Click += (s, ev) => doSelect();
                    lst.DoubleClick += (s, ev) => doSelect();
                    txtFilter.KeyDown += (s, ev) =>
                    {
                        if (ev.KeyCode == Keys.Enter) doSelect();
                        else if (ev.KeyCode == Keys.Down && lst.Items.Count > 0)
                        {
                            lst.Focus();
                            if (lst.SelectedIndex < lst.Items.Count - 1) lst.SelectedIndex++;
                        }
                    };
                    lst.KeyDown += (s, ev) =>
                    {
                        if (ev.KeyCode == Keys.Enter) doSelect();
                    };

                    Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4) };
                    pnlTop.Controls.Add(txtFilter);

                    pickerForm.Controls.Add(lst);
                    pickerForm.Controls.Add(pnlTop);
                    pickerForm.Controls.Add(btnSelect);

                    populateList();
                    txtFilter.Focus();
                    pickerForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in Expense Ledger picker: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PreviewGrid()
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not preview grid: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PreviewReport()
        {
            if (_matrixDataSet == null || _matrixDataSet.Tables.Count == 0)
            {
                MessageBox.Show("No data available to preview.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Form previewForm = new Form
                {
                    Text = "Report Preview - Date-wise Expense Report",
                    Size = new Size(1100, 700),
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false
                };

                UltraGrid previewGrid = new UltraGrid { Dock = DockStyle.Fill };
                previewForm.Controls.Add(previewGrid);
                previewGrid.DataSource = _matrixDataSet.Copy();
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open report preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportGridData()
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = $"DatewiseExpenseReport_{DateTime.Now:yyyyMMdd_HHmm}.csv";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        ExportToCsv(sfd.FileName);
                        MessageBox.Show($"Successfully exported to:\n{sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToCsv(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            var band = gridReport.DisplayLayout.Bands[0];

            // Headers
            var visibleCols = band.Columns.Cast<UltraGridColumn>().Where(c => !c.Hidden).ToList();
            sb.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption.Replace("\"", "\"\"")}\"")));

            // Rows
            foreach (var row in gridReport.Rows)
            {
                if (row.IsFilteredOut || row.Hidden) continue;

                var values = visibleCols.Select(c =>
                {
                    object val = row.GetCellValue(c);
                    if (val == null || val == DBNull.Value) return "\"\"";
                    if (val is decimal || val is double || val is float)
                        return ((decimal)Convert.ToDecimal(val)).ToString("F2");
                    if (val is DateTime dt)
                        return dt.ToString("yyyy-MM-dd");
                    return $"\"{val.ToString().Replace("\"", "\"\"")}\"";
                });
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void ResetFilters()
        {
            _isLoading = true;
            try
            {
                ultraComboPeriodMode.Value = "DATE_RANGE";
                ultraComboViewMode.Value = "DATE_MATRIX";
                ultraComboExpenseCategory.Value = 0;
                ultraComboExpenseLedger.Value = 0;
                ultraComboPaymentMode.Value = "ALL";
                txtSearch.Text = string.Empty;
                DateTime today = DateTime.Today;
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                UpdatePeriodModeVisibility();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void ExpandAllBands()
        {
            gridReport.Rows.ExpandAll(true);
        }

        private void CollapseAllBands()
        {
            gridReport.Rows.CollapseAll(true);
        }

        private void ToggleSelectionPanel()
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void ApplySearchFilter()
        {
            string search = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(search))
            {
                foreach (var row in gridReport.Rows)
                {
                    row.Hidden = false;
                }
                UpdateFooterValues();
                return;
            }

            foreach (var row in gridReport.Rows)
            {
                bool match = false;
                foreach (UltraGridCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLowerInvariant().Contains(search))
                    {
                        match = true;
                        break;
                    }
                }
                row.Hidden = !match;
            }

            UpdateFooterValues();
        }
        #endregion
    }
}
