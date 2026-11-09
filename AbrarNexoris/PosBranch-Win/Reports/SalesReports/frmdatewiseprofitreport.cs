using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports;
using Repository;
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

namespace PosBranch_Win.Reports.SalesReports
{
    /// <summary>
    /// Date-wise Profit Report (frmdatewiseprofitreport)
    /// Features parity with frmdatewisepurchasereport & frmdatewisesalesreport:
    /// exact theme, grid styling, dynamic footer cell aggregations with context menu,
    /// header drag-to-hide, floating column chooser, layout persistence, hierarchical (+) drilldown,
    /// toggle between Profit and Sales Amount, and CSV/PDF export.
    /// </summary>
    public partial class frmdatewiseprofitreport : Form
    {
        #region Colour Palette (Exact match with frmdatewisepurchasereport & frmdatewisesalesreport)
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

        private static readonly Color ProfitGreenColor     = Color.FromArgb(27, 94, 32);
        private static readonly Color LossRedColor         = Color.FromArgb(183, 28, 28);
        #endregion

        #region Private Fields
        private readonly DatewiseProfitReportRepository _repository;
        private List<DatewiseProfitReportRow> _reportRows;
        private readonly List<PivotDateColumnInfo> _currentMatrixDateColumns = new List<PivotDateColumnInfo>();
        private DataSet _matrixDataSet;
        private DataTable _matrixTable;
        private bool _isLoading;
        private bool _isSalesAmountMode = false; // False = Show Profit (default); True = Show Sales Amount

        // Dynamic footer panel and cell controls
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Qty",
            "Rate",
            "CostRate",
            "SalesAmount",
            "DiscountAmt",
            "TaxAmt",
            "NetAmount",
            "CostAmount",
            "Profit",
            "MarginPercent",
            "Total",
            "TotalProfit",
            "TotalSales",
            "TotalCost",
            "TotalQty",
            "MarginPct"
        };

        // Header drag-to-hide & Column Chooser
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

        #region Helper Classes
        private sealed class ItemMatrixInfo
        {
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public string Unit { get; set; }
            public string BarCode { get; set; }
            public decimal TotalQty { get; set; }
            public decimal TotalSales { get; set; }
            public decimal TotalCost { get; set; }
            public decimal TotalProfit { get; set; }
            public decimal MarginPct => TotalSales > 0 ? Math.Round((TotalProfit / TotalSales) * 100m, 2) : 0m;
        }

        private sealed class PivotDateColumnInfo
        {
            public string ColumnKey { get; set; }
            public string HeaderCaption { get; set; }
            public string Tooltip { get; set; }
            public DateTime? Date { get; set; }
            public int DayNumber { get; set; }
            public int MonthNumber { get; set; }
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

        #region Constructor & Form Lifecycle
        public frmdatewiseprofitreport()
        {
            _repository = new DatewiseProfitReportRepository();
            _reportRows = new List<DatewiseProfitReportRow>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmdatewiseprofitreport_Load;
            FormClosing += frmdatewiseprofitreport_FormClosing;
            KeyDown += frmdatewiseprofitreport_KeyDown;
            KeyPreview = true;

            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnColumnChooser.Click += btnColumnChooser_Click;
            btnToggleValueMode.Click += btnToggleValueMode_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            btnItemPicker.Click += btnItemPicker_Click;
            btnCustomerPicker.Click += btnCustomerPicker_Click;

            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboMonth.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboYear.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtFrom.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtTo.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboCustomer.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            txtItemSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { LoadReport(); e.Handled = true; }
                else if (e.KeyCode == Keys.F11) { btnItemPicker.PerformClick(); e.Handled = true; }
            };

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            gridReport.DoubleClickRow += (s, e) =>
            {
                if (e.Row != null && (e.Row.Band == null || e.Row.Band.Index == 0))
                {
                    e.Row.Expanded = !e.Row.Expanded;
                }
            };
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
            gridReport.AfterRowFilterChanged += (s, e) => UpdateFooterValues();

            gridReport.MouseDown += gridReport_MouseDown;
            gridReport.MouseMove += gridReport_MouseMove;
            gridReport.MouseUp += gridReport_MouseUp;
            gridReport.GiveFeedback += gridReport_GiveFeedback;
        }

        private void frmdatewiseprofitreport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                Text = "Date-wise Profit Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = FormBackColor;
                Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);

                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                InitializeFilterControls();
                LoadCustomers();
                UpdateValueModeButtonText();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void frmdatewiseprofitreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
            }
        }

        private void frmdatewiseprofitreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (txtItemSearch.Focused)
                {
                    btnItemPicker.PerformClick();
                    e.Handled = true;
                }
                else if (ultraComboCustomer.Focused)
                {
                    btnCustomerPicker.PerformClick();
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;

            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date Range");
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("ALL", "ALL Dates");
            ultraComboPeriodMode.Value = "DATE_RANGE";

            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            ultraComboMonth.Items.Clear();
            ultraComboMonth.Items.Add(0, "ALL Months");
            for (int m = 1; m <= 12; m++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                ultraComboMonth.Items.Add(m, monthName);
            }
            ultraComboMonth.Value = today.Month;

            ultraComboYear.Items.Clear();
            ultraComboYear.Items.Add(0, "ALL Years");
            for (int y = today.Year; y >= 2000; y--)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = today.Year;

            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Item vs Dates Profit)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Bill & Item Wise)");
            ultraComboViewMode.Items.Add("DATE_ITEM_SUMMARY", "Daily Summary (Date & Item)");
            ultraComboViewMode.Items.Add("ITEM_SUMMARY", "Item Summary (Total by Item)");
            ultraComboViewMode.Value = "DATE_MATRIX";

            txtItemSearch.Text = string.Empty;

            UpdatePeriodControlVisibility();
        }

        private void UpdatePeriodControlVisibility()
        {
            string mode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
            bool isDateRange = string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase);
            bool isMonthWise = string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase);
            bool isYearWise  = string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = isDateRange;
            dtFrom.Visible = isDateRange;
            lblToDate.Visible = isDateRange;
            dtTo.Visible = isDateRange;

            lblMonth.Visible = isMonthWise;
            ultraComboMonth.Visible = isMonthWise;

            lblYear.Visible = isMonthWise || isYearWise;
            ultraComboYear.Visible = isMonthWise || isYearWise;

            if (isYearWise)
            {
                lblYear.Location = new Point(248, 14);
                ultraComboYear.Location = new Point(288, 12);
            }
            else if (isMonthWise)
            {
                lblMonth.Location = new Point(248, 14);
                ultraComboMonth.Location = new Point(296, 12);
                lblYear.Location = new Point(418, 14);
                ultraComboYear.Location = new Point(456, 12);
            }
        }

        private void ultraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            UpdatePeriodControlVisibility();
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void LoadCustomers()
        {
            try
            {
                ultraComboCustomer.Items.Clear();
                ultraComboCustomer.Items.Add(0, "ALL Customers");

                Dropdowns dropdowns = new Dropdowns();
                var customerData = dropdowns.CustomerDDl();
                if (customerData?.List != null)
                {
                    foreach (var c in customerData.List.Where(x => x != null && !string.IsNullOrWhiteSpace(x.LedgerName)).OrderBy(x => x.LedgerName))
                    {
                        ultraComboCustomer.Items.Add(c.LedgerID, c.LedgerName);
                    }
                }
                ultraComboCustomer.Value = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
        }

        private void UpdateValueModeButtonText()
        {
            if (btnToggleValueMode == null) return;
            btnToggleValueMode.Text = _isSalesAmountMode ? "Show Profit" : "Show Sales Amount";
        }
        #endregion

        #region UI Styling
        private void InitializePanels()
        {
            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelMaster != null)
            {
                ultraPanelMaster.Appearance.BackColor = FormBackColor;
                ultraPanelMaster.Appearance.BorderColor = BorderBlue;
                ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (gridReport != null)
            {
                gridReport.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                gridReport.DisplayLayout.Appearance.BorderColor = BorderBlue;
            }
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnToggleValueMode);
            StyleClassicButton(btnExpandAll);
            StyleClassicButton(btnCollapseAll);
            StyleClassicButton(btnToggleSelection);

            StylePickerButton(btnItemPicker);
            StylePickerButton(btnCustomerPicker);
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
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboPeriodMode, true);
            StyleFilterCombo(ultraComboMonth, true);
            StyleFilterCombo(ultraComboYear, true);
            StyleFilterCombo(ultraComboViewMode, true);
            StyleFilterCombo(txtItemSearch, false);
            StyleFilterCombo(ultraComboCustomer, true);

            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);

            StyleLabel(lblPeriodMode);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblViewMode);
            StyleLabel(lblItem);
            StyleLabel(lblCustomer);
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
            combo.Appearance.FontData.SizeInPoints = 9;
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
            editor.Appearance.FontData.SizeInPoints = 9;
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

            _filterManager = new ReportColumnFilterManager(gridReport);
        }

        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Height = 26;
        }
        #endregion

        #region Custom Footer Sync & Aggregation
        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null || gridReport == null) return;

            foreach (var kvp in _footerLabels)
            {
                if (kvp.Value != null && !kvp.Value.IsDisposed)
                {
                    ultraPanelGridFooter.ClientArea.Controls.Remove(kvp.Value);
                    kvp.Value.Dispose();
                }
            }
            _footerLabels.Clear();

            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            Label lblCount = new Label
            {
                Name = "_lblRecordCount",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold),
                Text = "Total (0 Items)"
            };
            ultraPanelGridFooter.ClientArea.Controls.Add(lblCount);
            _footerLabels["_RECORD_COUNT_"] = lblCount;

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Label cell = new Label
                {
                    Name = "footer_" + column.Key,
                    Tag = column.Key,
                    AutoSize = false,
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold),
                    TextAlign = GetAlignmentForColumn(column)
                };

                cell.MouseUp += FooterCell_MouseUp;
                ultraPanelGridFooter.ClientArea.Controls.Add(cell);
                _footerLabels[column.Key] = cell;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    if (summaryDefaultNumericColumns.Contains(column.Key) || column.Key.StartsWith("Day_") || column.Key.StartsWith("Date_") || column.Key.StartsWith("Month_"))
                    {
                        _columnAggregations[column.Key] = "Sum";
                    }
                    else
                    {
                        _columnAggregations[column.Key] = "None";
                    }
                }
            }
        }

        private ContentAlignment GetAlignmentForColumn(UltraGridColumn col)
        {
            if (col == null) return ContentAlignment.MiddleRight;
            HAlign align = col.CellAppearance.TextHAlign;
            if (align == HAlign.Left) return ContentAlignment.MiddleLeft;
            if (align == HAlign.Center) return ContentAlignment.MiddleCenter;
            return ContentAlignment.MiddleRight;
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraPanelGridFooter == null || gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int gridScrollLeft = 0;
            if (gridReport.ActiveColScrollRegion != null)
            {
                gridScrollLeft = gridReport.ActiveColScrollRegion.Position;
            }

            int rowSelectorWidth = 0;
            if (gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
            {
                rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
                if (rowSelectorWidth <= 0) rowSelectorWidth = 20;
            }

            int countLabelWidth = 200;
            if (band.Columns.Exists("ItemName") && !band.Columns["ItemName"].Hidden)
            {
                countLabelWidth = Math.Max(160, band.Columns["ItemName"].Width);
            }

            if (_footerLabels.TryGetValue("_RECORD_COUNT_", out Label lblCount))
            {
                lblCount.SetBounds(rowSelectorWidth + 4, 3, countLabelWidth - 8, 20);
            }

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (_footerLabels.TryGetValue(column.Key, out Label cell))
                {
                    UIElement colHeaderElement = column.Header.GetUIElement();
                    if (colHeaderElement != null)
                    {
                        Rectangle rect = colHeaderElement.Rect;
                        int x = rect.X;
                        int w = rect.Width;
                        cell.SetBounds(x, 3, w, 20);
                        cell.Visible = (x + w > 0 && x < ultraPanelGridFooter.Width);
                    }
                    else
                    {
                        int fallbackX = rowSelectorWidth - gridScrollLeft;
                        foreach (UltraGridColumn c in band.Columns)
                        {
                            if (c.Header.VisiblePosition < column.Header.VisiblePosition && !c.Hidden && !c.IsChaptered)
                            {
                                fallbackX += c.Width;
                            }
                        }
                        cell.SetBounds(fallbackX, 3, column.Width, 20);
                        cell.Visible = (fallbackX + column.Width > 0 && fallbackX < ultraPanelGridFooter.Width);
                    }
                }
            }
        }

        private void UpdateFooterValues()
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            List<UltraGridRow> visibleRows = GetVisibleDataRows();

            if (_footerLabels.TryGetValue("_RECORD_COUNT_", out Label lblCount))
            {
                lblCount.Text = $"Total ({visibleRows.Count} Items)";
            }

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!_footerLabels.TryGetValue(column.Key, out Label cell)) continue;

                string aggType = "None";
                _columnAggregations.TryGetValue(column.Key, out aggType);

                if (string.Equals(aggType, "None", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(aggType))
                {
                    cell.Text = string.Empty;
                    continue;
                }

                decimal val = CalculateAggregation(visibleRows, column.Key, aggType);
                if (string.Equals(aggType, "Count", StringComparison.OrdinalIgnoreCase))
                {
                    cell.Text = val.ToString("N0");
                }
                else
                {
                    cell.Text = val.ToString("#,##0.00");
                }
            }
        }

        private List<UltraGridRow> GetVisibleDataRows()
        {
            List<UltraGridRow> list = new List<UltraGridRow>();
            if (gridReport?.Rows == null) return list;

            foreach (UltraGridRow row in gridReport.Rows.GetAllNonGroupByRows())
            {
                if (row.IsDataRow && !row.IsFilteredOut && (row.Band == null || row.Band.Index == 0))
                {
                    list.Add(row);
                }
            }
            return list;
        }

        private decimal CalculateAggregation(List<UltraGridRow> rows, string columnKey, string aggType)
        {
            if (rows == null || rows.Count == 0) return 0m;

            if (string.Equals(aggType, "Count", StringComparison.OrdinalIgnoreCase))
            {
                return rows.Count(r => r.Cells.Exists(columnKey) && r.Cells[columnKey].Value != null && r.Cells[columnKey].Value != DBNull.Value);
            }

            List<decimal> numericValues = new List<decimal>();
            foreach (UltraGridRow r in rows)
            {
                if (r.Cells.Exists(columnKey))
                {
                    object o = r.Cells[columnKey].Value;
                    if (o != null && o != DBNull.Value && decimal.TryParse(Convert.ToString(o), out decimal decVal))
                    {
                        numericValues.Add(decVal);
                    }
                }
            }

            if (numericValues.Count == 0) return 0m;

            if (string.Equals(aggType, "Sum", StringComparison.OrdinalIgnoreCase))
            {
                return numericValues.Sum();
            }
            if (string.Equals(aggType, "Average", StringComparison.OrdinalIgnoreCase) || string.Equals(aggType, "Avg", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round(numericValues.Average(), 2);
            }
            if (string.Equals(aggType, "Min", StringComparison.OrdinalIgnoreCase))
            {
                return numericValues.Min();
            }
            if (string.Equals(aggType, "Max", StringComparison.OrdinalIgnoreCase))
            {
                return numericValues.Max();
            }

            return 0m;
        }

        private void FooterCell_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            Label cell = sender as Label;
            if (cell == null) return;
            string colKey = Convert.ToString(cell.Tag);
            if (string.IsNullOrEmpty(colKey)) return;

            ContextMenuStrip menu = new ContextMenuStrip();
            string currentAgg = _columnAggregations.ContainsKey(colKey) ? _columnAggregations[colKey] : "None";

            string[] types = new[] { "Sum", "Average", "Count", "Min", "Max", "None" };
            foreach (string t in types)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(t)
                {
                    Checked = string.Equals(currentAgg, t, StringComparison.OrdinalIgnoreCase)
                };
                string capturedType = t;
                item.Click += (s, ev) =>
                {
                    _columnAggregations[colKey] = capturedType;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }

            menu.Show(cell, e.Location);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser
        private void gridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UIElement element = gridReport.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                HeaderUIElement headerElement = element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerElement?.GetContext(typeof(UltraGridColumn)) is UltraGridColumn col && !col.IsChaptered)
                {
                    columnBeingDragged = col;
                    headerDragStartPoint = e.Location;
                    isDraggingHeaderToHide = false;
                }
            }
        }

        private void gridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                int dy = Math.Abs(e.Y - headerDragStartPoint.Y);
                int dx = Math.Abs(e.X - headerDragStartPoint.X);

                if (dy > 12 && dy > dx)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                }
                else if (isDraggingHeaderToHide)
                {
                    Cursor.Current = blackXCursor;
                }
            }
        }

        private void gridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide && columnBeingDragged != null)
            {
                Rectangle gridRect = gridReport.ClientRectangle;
                if (!gridRect.Contains(e.Location) || e.Y > 50)
                {
                    HideColumn(columnBeingDragged);
                }
            }

            columnBeingDragged = null;
            isDraggingHeaderToHide = false;
            Cursor.Current = Cursors.Default;
        }

        private void gridReport_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                e.UseDefaultCursors = false;
                Cursor.Current = blackXCursor;
            }
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered || string.Equals(col.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase)) return;

            col.Hidden = true;
            userHiddenColumnKeys.Add(col.Key);

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();

            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            PopulateColumnChooserListBox();
        }

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(24, 24))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen pen = new Pen(Color.FromArgb(200, 30, 30), 3f))
                    {
                        g.DrawLine(pen, 4, 4, 20, 20);
                        g.DrawLine(pen, 20, 4, 4, 20);
                    }
                    return new Cursor(bmp.GetHicon());
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                columnChooserForm = new Form
                {
                    Text = "Customization (Field Chooser)",
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    StartPosition = FormStartPosition.Manual,
                    Width = 260,
                    Height = 380,
                    TopMost = true,
                    BackColor = FormBackColor,
                    ShowInTaskbar = false
                };

                Panel headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 44,
                    BackColor = FormBackColor,
                    Padding = new Padding(6)
                };

                txtColumnSearch = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.Gray,
                    Text = "Search columns..."
                };
                txtColumnSearch.Enter += (s, e) =>
                {
                    if (txtColumnSearch.Text == "Search columns...")
                    {
                        txtColumnSearch.Text = string.Empty;
                        txtColumnSearch.ForeColor = Color.Black;
                    }
                };
                txtColumnSearch.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtColumnSearch.Text))
                    {
                        txtColumnSearch.Text = "Search columns...";
                        txtColumnSearch.ForeColor = Color.Gray;
                    }
                };
                txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();
                headerPanel.Controls.Add(txtColumnSearch);

                Panel footerPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 36,
                    BackColor = FormBackColor,
                    Padding = new Padding(6, 4, 6, 4)
                };

                Button btnReset = new Button
                {
                    Text = "Show All",
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ButtonBlueTop,
                    ForeColor = ButtonTextBlue,
                    Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnReset.FlatAppearance.BorderColor = ButtonBlueBorder;
                btnReset.Click += (s, e) => UnhideAllColumns();
                footerPanel.Controls.Add(btnReset);

                columnChooserListBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    DrawMode = DrawMode.OwnerDrawFixed,
                    ItemHeight = 32,
                    BorderStyle = BorderStyle.None,
                    BackColor = FormBackColor,
                    Font = new Font("Segoe UI", 9F)
                };
                columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
                columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

                columnChooserForm.Controls.Add(columnChooserListBox);
                columnChooserForm.Controls.Add(headerPanel);
                columnChooserForm.Controls.Add(footerPanel);

                columnChooserForm.FormClosing += (s, e) =>
                {
                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        columnChooserForm.Hide();
                    }
                };
            }

            PopulateColumnChooserListBox();

            Point btnPos = btnColumnChooser.PointToScreen(Point.Empty);
            columnChooserForm.Location = new Point(btnPos.X, btnPos.Y + btnColumnChooser.Height + 2);
            columnChooserForm.Show();
            columnChooserForm.BringToFront();
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.Items.Clear();
            string filterText = txtColumnSearch != null && txtColumnSearch.Text != "Search columns..."
                ? txtColumnSearch.Text.Trim()
                : string.Empty;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.IsChaptered || string.Equals(col.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase)) continue;

                if (col.Hidden && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    string caption = !string.IsNullOrWhiteSpace(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filterText) || caption.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count) return;

            ColumnChooserItem item = columnChooserListBox.Items[e.Index] as ColumnChooserItem;
            if (item == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);

            Color topColor = (e.State & DrawItemState.Selected) != 0 ? Color.FromArgb(206, 226, 252) : Color.White;
            Color bottomColor = (e.State & DrawItemState.Selected) != 0 ? Color.FromArgb(170, 202, 245) : Color.FromArgb(240, 245, 252);

            using (LinearGradientBrush brush = new LinearGradientBrush(rect, topColor, bottomColor, LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(BorderBlue))
            {
                using (GraphicsPath path = CreateRoundedRectanglePath(rect, 4))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(borderPen, path);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                item.DisplayText,
                columnChooserListBox.Font,
                new Rectangle(rect.X + 8, rect.Y, rect.Width - 16, rect.Height),
                ControlTextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis
            );
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem selectedItem)
            {
                RestoreColumn(selectedItem.ColumnKey);
            }
        }

        private void RestoreColumn(string columnKey)
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            if (band.Columns.Exists(columnKey))
            {
                band.Columns[columnKey].Hidden = false;
                userHiddenColumnKeys.Remove(columnKey);

                PopulateColumnChooserListBox();
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();

                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            }
        }

        private void UnhideAllColumns()
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.IsChaptered || string.Equals(col.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase)) continue;

                if (col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    col.Hidden = false;
                }
            }

            userHiddenColumnKeys.Clear();

            if (isMatrix)
            {
                if (band.Columns.Exists("ItemKey")) band.Columns["ItemKey"].Hidden = true;
                if (band.Columns.Exists("ItemID")) band.Columns["ItemID"].Hidden = true;
            }

            PopulateColumnChooserListBox();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();

            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        #endregion

        #region Matrix Building & Data Loading
        private DataTable BuildMatrixDataTable(List<DatewiseProfitReportRow> rawRows, DatewiseProfitReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            dateColumns = new List<PivotDateColumnInfo>();
            string mode = (filter.PeriodMode ?? "DATE_RANGE").ToUpperInvariant();

            if (mode == "MONTH_WISE")
            {
                int month = filter.Month > 0 ? filter.Month : DateTime.Today.Month;
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;

                if (filter.Month > 0)
                {
                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= 31; day++)
                    {
                        string key = $"Day_{day:D2}";
                        string caption = day.ToString();
                        string tip;
                        DateTime? dt = null;

                        if (day <= daysInMonth)
                        {
                            dt = new DateTime(year, month, day);
                            tip = $"{dt.Value:dd-MMM-yyyy (dddd)}";
                        }
                        else
                        {
                            tip = $"Day {day} (N/A for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)})";
                        }

                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = key,
                            HeaderCaption = caption,
                            Tooltip = tip,
                            Date = dt,
                            DayNumber = day
                        });
                    }
                }
                else
                {
                    for (int m = 1; m <= 12; m++)
                    {
                        string mName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m);
                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = $"Month_{m:D2}",
                            HeaderCaption = mName,
                            Tooltip = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)} {year}",
                            MonthNumber = m
                        });
                    }
                }
            }
            else if (mode == "YEAR_WISE")
            {
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;
                for (int m = 1; m <= 12; m++)
                {
                    string mName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m);
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = $"Month_{m:D2}",
                        HeaderCaption = mName,
                        Tooltip = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)} {year}",
                        MonthNumber = m
                    });
                }
            }
            else if (mode == "DATE_RANGE")
            {
                DateTime start = filter.FromDate.Date;
                DateTime end = filter.ToDate.Date;
                if (start > end) { DateTime tmp = start; start = end; end = tmp; }

                if (start.Year == end.Year && start.Month == end.Month && start.Day == 1 && end.Day == DateTime.DaysInMonth(start.Year, start.Month))
                {
                    int daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);
                    for (int day = 1; day <= 31; day++)
                    {
                        string key = $"Day_{day:D2}";
                        string caption = day.ToString();
                        string tip;
                        DateTime? dt = null;

                        if (day <= daysInMonth)
                        {
                            dt = new DateTime(start.Year, start.Month, day);
                            tip = $"{dt.Value:dd-MMM-yyyy (dddd)}";
                        }
                        else
                        {
                            tip = $"Day {day} (N/A for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(start.Month)})";
                        }

                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = key,
                            HeaderCaption = caption,
                            Tooltip = tip,
                            Date = dt,
                            DayNumber = day
                        });
                    }
                }
                else
                {
                    int totalDays = (int)(end - start).TotalDays + 1;
                    if (totalDays <= 93)
                    {
                        for (DateTime d = start; d <= end; d = d.AddDays(1))
                        {
                            string key = $"Date_{d:yyyyMMdd}";
                            string caption = (start.Year == end.Year && start.Month == end.Month)
                                ? d.Day.ToString()
                                : d.ToString("dd-MMM");

                            dateColumns.Add(new PivotDateColumnInfo
                            {
                                ColumnKey = key,
                                HeaderCaption = caption,
                                Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                                Date = d,
                                DayNumber = d.Day
                            });
                        }
                    }
                    else
                    {
                        var distinctDates = (rawRows ?? new List<DatewiseProfitReportRow>())
                            .Select(r => r.BillDate.Date)
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
                                ColumnKey = $"Date_{d:yyyyMMdd}",
                                HeaderCaption = d.ToString("dd-MMM"),
                                Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                                Date = d,
                                DayNumber = d.Day
                            });
                        }
                    }
                }
            }
            else // ALL
            {
                var distinctDates = (rawRows ?? new List<DatewiseProfitReportRow>())
                    .Select(r => r.BillDate.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                if (distinctDates.Count == 0)
                {
                    distinctDates.Add(DateTime.Today);
                }

                foreach (var d in distinctDates)
                {
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = $"Date_{d:yyyyMMdd}",
                        HeaderCaption = d.ToString("dd-MMM"),
                        Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                        Date = d,
                        DayNumber = d.Day
                    });
                }
            }

            // Build DataTable
            DataTable table = new DataTable("ProfitMatrix");
            table.Columns.Add("ItemKey", typeof(string));
            table.Columns.Add("ItemID", typeof(int));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("Unit", typeof(string));

            foreach (var col in dateColumns)
            {
                var dc = table.Columns.Add(col.ColumnKey, typeof(decimal));
                dc.DefaultValue = 0m;
            }

            var cTotal = table.Columns.Add("Total", typeof(decimal));
            cTotal.DefaultValue = 0m;

            var cTotalProfit = table.Columns.Add("TotalProfit", typeof(decimal));
            cTotalProfit.DefaultValue = 0m;

            var cTotalSales = table.Columns.Add("TotalSales", typeof(decimal));
            cTotalSales.DefaultValue = 0m;

            var cTotalCost = table.Columns.Add("TotalCost", typeof(decimal));
            cTotalCost.DefaultValue = 0m;

            var cMarginPct = table.Columns.Add("MarginPct", typeof(decimal));
            cMarginPct.DefaultValue = 0m;

            var cTotalQty = table.Columns.Add("TotalQty", typeof(decimal));
            cTotalQty.DefaultValue = 0m;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Fast pre-aggregation by item and date for Profit, Sales Amount, and Cost
            Dictionary<string, decimal> dateProfitMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> dateSalesMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> dateQtyMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ItemMatrixInfo> itemInfoMap = new Dictionary<string, ItemMatrixInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseProfitReportRow r in rawRows)
            {
                string itemKey = (r.ItemID > 0 ? r.ItemID.ToString() : (r.ItemName ?? string.Empty)) + "_" + (r.Unit ?? string.Empty);

                ItemMatrixInfo itemInfo;
                if (!itemInfoMap.TryGetValue(itemKey, out itemInfo))
                {
                    itemInfo = new ItemMatrixInfo
                    {
                        ItemID = r.ItemID,
                        ItemName = !string.IsNullOrEmpty(r.ItemName) ? r.ItemName : "Unknown Item",
                        Unit = r.Unit ?? string.Empty,
                        BarCode = r.BarCode ?? string.Empty,
                        TotalQty = 0m,
                        TotalSales = 0m,
                        TotalCost = 0m,
                        TotalProfit = 0m
                    };
                    itemInfoMap[itemKey] = itemInfo;
                }
                else if (string.IsNullOrEmpty(itemInfo.BarCode) && !string.IsNullOrEmpty(r.BarCode))
                {
                    itemInfo.BarCode = r.BarCode;
                }

                decimal salesAmt = (r.NetAmount != 0m) ? r.NetAmount : r.SalesAmount;
                decimal costAmt = (r.CostAmount != 0m) ? r.CostAmount : (r.Qty * r.CostRate);
                decimal profitVal = r.Profit;

                itemInfo.TotalQty += r.Qty;
                itemInfo.TotalSales += salesAmt;
                itemInfo.TotalCost += costAmt;
                itemInfo.TotalProfit += profitVal;

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    bool matches = false;
                    if (dateCol.Date.HasValue)
                    {
                        matches = (r.BillDate.Date == dateCol.Date.Value.Date);
                    }
                    else if (dateCol.MonthNumber > 0)
                    {
                        matches = (r.BillDate.Month == dateCol.MonthNumber);
                    }
                    else if (dateCol.DayNumber > 0)
                    {
                        matches = (r.BillDate.Day == dateCol.DayNumber);
                    }

                    if (matches)
                    {
                        string cellKey = itemKey + "_" + dateCol.ColumnKey;

                        decimal currentProfit = 0m;
                        dateProfitMap.TryGetValue(cellKey, out currentProfit);
                        dateProfitMap[cellKey] = currentProfit + profitVal;

                        decimal currentSales = 0m;
                        dateSalesMap.TryGetValue(cellKey, out currentSales);
                        dateSalesMap[cellKey] = currentSales + salesAmt;

                        decimal currentQty = 0m;
                        dateQtyMap.TryGetValue(cellKey, out currentQty);
                        dateQtyMap[cellKey] = currentQty + r.Qty;
                    }
                }
            }

            foreach (ItemMatrixInfo item in itemInfoMap.Values.OrderBy(x => x.ItemName))
            {
                string itemKey = (item.ItemID > 0 ? item.ItemID.ToString() : item.ItemName) + "_" + (item.Unit ?? string.Empty);

                DataRow dr = table.NewRow();
                dr["ItemKey"] = itemKey;
                dr["ItemID"] = item.ItemID;
                dr["BarCode"] = item.BarCode;
                dr["ItemName"] = item.ItemName;
                dr["Unit"] = item.Unit;

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    string cellKey = itemKey + "_" + dateCol.ColumnKey;
                    decimal cellVal = 0m;
                    if (_isSalesAmountMode)
                    {
                        dateSalesMap.TryGetValue(cellKey, out cellVal);
                    }
                    else
                    {
                        dateProfitMap.TryGetValue(cellKey, out cellVal);
                    }
                    dr[dateCol.ColumnKey] = cellVal;
                }

                dr["Total"] = _isSalesAmountMode ? item.TotalSales : item.TotalProfit;
                dr["TotalProfit"] = item.TotalProfit;
                dr["TotalSales"] = item.TotalSales;
                dr["TotalCost"] = item.TotalCost;
                dr["MarginPct"] = item.MarginPct;
                dr["TotalQty"] = item.TotalQty;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildDetailsDataTable(List<DatewiseProfitReportRow> rawRows)
        {
            DataTable details = new DataTable("ProfitDetails");
            details.Columns.Add("ItemKey", typeof(string));
            details.Columns.Add("BillDate", typeof(DateTime));
            details.Columns.Add("BillNo", typeof(long));
            details.Columns.Add("InvoiceNo", typeof(string));
            details.Columns.Add("CustomerName", typeof(string));
            details.Columns.Add("Unit", typeof(string));
            details.Columns.Add("Qty", typeof(decimal));
            details.Columns.Add("Rate", typeof(decimal));
            details.Columns.Add("CostRate", typeof(decimal));
            details.Columns.Add("SalesAmount", typeof(decimal));
            details.Columns.Add("CostAmount", typeof(decimal));
            details.Columns.Add("Profit", typeof(decimal));
            details.Columns.Add("MarginPercent", typeof(decimal));

            if (rawRows != null && rawRows.Count > 0)
            {
                foreach (DatewiseProfitReportRow r in rawRows.OrderBy(x => x.BillDate).ThenBy(x => x.BillNo).ThenBy(x => x.SlNo))
                {
                    string itemKey = (r.ItemID > 0 ? r.ItemID.ToString() : (r.ItemName ?? string.Empty)) + "_" + (r.Unit ?? string.Empty);
                    DataRow dr = details.NewRow();
                    dr["ItemKey"] = itemKey;
                    dr["BillDate"] = r.BillDate.Date;
                    dr["BillNo"] = r.BillNo;
                    dr["InvoiceNo"] = !string.IsNullOrEmpty(r.InvoiceNo) ? r.InvoiceNo : (r.BillNo > 0 ? "INV-" + r.BillNo : "-");
                    dr["CustomerName"] = !string.IsNullOrEmpty(r.CustomerName) ? r.CustomerName : "Cash Customer";
                    dr["Unit"] = r.Unit ?? string.Empty;
                    dr["Qty"] = r.Qty;
                    dr["Rate"] = r.Rate;
                    dr["CostRate"] = r.CostRate;
                    dr["SalesAmount"] = (r.NetAmount != 0 ? r.NetAmount : r.SalesAmount);
                    dr["CostAmount"] = (r.CostAmount != 0 ? r.CostAmount : (r.Qty * r.CostRate));
                    dr["Profit"] = r.Profit;
                    dr["MarginPercent"] = r.MarginPercent;
                    details.Rows.Add(dr);
                }
            }

            return details;
        }

        private DataSet BuildMatrixDataSet(List<DatewiseProfitReportRow> rawRows, DatewiseProfitReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable detailsTable = BuildDetailsDataTable(rawRows);

            DataSet ds = new DataSet("DatewiseProfitDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(detailsTable);

            DataRelation relation = new DataRelation("ItemProfitBreakdown", matrixTable.Columns["ItemKey"], detailsTable.Columns["ItemKey"], false);
            ds.Relations.Add(relation);

            return ds;
        }

        private void LoadReport()
        {
            Cursor prev = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";

                DatewiseProfitReportFilter filter = new DatewiseProfitReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    CustomerLedgerId = ultraComboCustomer.Value != null ? Convert.ToInt32(ultraComboCustomer.Value) : 0,
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DETAILED" // Fetch transaction details so drilldown displays real bill details
                };

                _reportRows = _repository.GetReport(filter);

                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    List<PivotDateColumnInfo> matrixCols;
                    DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                    _matrixDataSet = ds;
                    _matrixTable = ds.Tables["ProfitMatrix"];
                    _currentMatrixDateColumns.Clear();
                    _currentMatrixDateColumns.AddRange(matrixCols);

                    gridReport.DataSource = null;
                    gridReport.DataSource = _matrixDataSet;
                }
                else
                {
                    _currentMatrixDateColumns.Clear();
                    _matrixTable = null;
                    _matrixDataSet = null;

                    // If viewMode is DATE_ITEM_SUMMARY or ITEM_SUMMARY, query specifically for that aggregated view
                    if (viewMode == "DATE_ITEM_SUMMARY" || viewMode == "ITEM_SUMMARY")
                    {
                        filter.ViewMode = viewMode;
                        _reportRows = _repository.GetReport(filter);
                    }

                    gridReport.DataSource = null;
                    gridReport.DataSource = _reportRows;
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Date-wise Profit Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = prev;
            }
        }
        #endregion

        #region UltraGrid Layout & Row Initialization
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
            column.Header.Appearance.BackColor = GridHeaderBlue;
            column.Header.Appearance.BackColor2 = GridHeaderBlueDark;
            column.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
            column.Header.Appearance.ForeColor = Color.White;
            column.Header.Appearance.BorderColor = BorderBlue;
            column.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
            column.Header.Appearance.FontData.SizeInPoints = 8.25F;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = e.Layout;
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

            UltraGridBand band = e.Layout.Bands[0];
            band.ColHeadersVisible = true;
            band.HeaderVisible = false;
            band.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band.Override.HeaderAppearance.ForeColor = Color.White;
            band.Override.HeaderAppearance.BorderColor = BorderBlue;
            band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            band.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            band.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            if (isMatrix)
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    if (column.IsChaptered || string.Equals(column.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                if (band.Columns.Exists("ItemKey"))
                {
                    band.Columns["ItemKey"].Hidden = true;
                    band.Columns["ItemKey"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                if (band.Columns.Exists("ItemID"))
                {
                    band.Columns["ItemID"].Hidden = true;
                    band.Columns["ItemID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band, "ItemName", "Item Description", 240, null, HAlign.Left, 0);

                int visPos = 1;
                foreach (var dateCol in _currentMatrixDateColumns)
                {
                    if (band.Columns.Exists(dateCol.ColumnKey))
                    {
                        ConfigureGridColumn(band, dateCol.ColumnKey, dateCol.HeaderCaption, 60, "#,##0.00", HAlign.Right, visPos++);
                        string modeDesc = _isSalesAmountMode ? "Sales Amount" : "Profit";
                        band.Columns[dateCol.ColumnKey].Header.ToolTipText = $"{dateCol.Tooltip} ({modeDesc})";
                    }
                }

                ConfigureGridColumn(band, "Total", "Total", 95, "#,##0.00", HAlign.Right, visPos++);
                if (band.Columns.Exists("Total"))
                {
                    band.Columns["Total"].Hidden = false;
                    band.Columns["Total"].Header.Caption = "Total";
                    band.Columns["Total"].Header.ToolTipText = _isSalesAmountMode ? "Total Sales Amount across dates" : "Total Profit across dates";
                    band.Columns["Total"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Total"].CellAppearance.ForeColor = _isSalesAmountMode ? Color.FromArgb(13, 71, 161) : ProfitGreenColor;
                }

                ConfigureGridColumn(band, "TotalProfit", "Total Profit", 100, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalSales", "Total Sales", 100, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalCost", "Total Cost", 100, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "MarginPct", "Margin %", 80, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalQty", "Total Qty", 85, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
                ConfigureGridColumn(band, "BarCode", "Barcode", 110, null, HAlign.Left, visPos++);

                // By default hide chooser-only columns unless user explicitly showed them
                if (band.Columns.Exists("TotalProfit") && !userHiddenColumnKeys.Contains("TotalProfit"))
                {
                    band.Columns["TotalProfit"].Hidden = true;
                }
                if (band.Columns.Exists("TotalSales") && !userHiddenColumnKeys.Contains("TotalSales"))
                {
                    band.Columns["TotalSales"].Hidden = true;
                }
                if (band.Columns.Exists("TotalCost") && !userHiddenColumnKeys.Contains("TotalCost"))
                {
                    band.Columns["TotalCost"].Hidden = true;
                }
                if (band.Columns.Exists("MarginPct") && !userHiddenColumnKeys.Contains("MarginPct"))
                {
                    band.Columns["MarginPct"].Hidden = true;
                }
                if (band.Columns.Exists("TotalQty") && !userHiddenColumnKeys.Contains("TotalQty"))
                {
                    band.Columns["TotalQty"].Hidden = true;
                }
                if (band.Columns.Exists("Unit") && !userHiddenColumnKeys.Contains("Unit"))
                {
                    band.Columns["Unit"].Hidden = true;
                }
                if (band.Columns.Exists("BarCode") && !userHiddenColumnKeys.Contains("BarCode"))
                {
                    band.Columns["BarCode"].Hidden = true;
                }

                if (band.Columns.Exists("ItemName"))
                {
                    band.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
            else
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band, "BillDate", "Sales Date", 105, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureGridColumn(band, "InvoiceNo", "Invoice #", 100, null, HAlign.Left, 1);
                ConfigureGridColumn(band, "CustomerName", "Customer", 170, null, HAlign.Left, 2);
                ConfigureGridColumn(band, "ItemName", "Item Description", 220, null, HAlign.Left, 3);
                ConfigureGridColumn(band, "BarCode", "Barcode", 110, null, HAlign.Left, 4);
                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, 5);
                ConfigureGridColumn(band, "Qty", "Sold Qty", 80, "#,##0.00", HAlign.Right, 6);
                ConfigureGridColumn(band, "Rate", "Selling Rate", 85, "#,##0.00", HAlign.Right, 7);
                ConfigureGridColumn(band, "CostRate", "Cost Rate", 85, "#,##0.00", HAlign.Right, 8);
                ConfigureGridColumn(band, "SalesAmount", "Sales Amt", 100, "#,##0.00", HAlign.Right, 9);
                ConfigureGridColumn(band, "DiscountAmt", "Discount", 80, "#,##0.00", HAlign.Right, 10);
                ConfigureGridColumn(band, "TaxAmt", "Tax", 80, "#,##0.00", HAlign.Right, 11);
                ConfigureGridColumn(band, "NetAmount", "Net Sales", 105, "#,##0.00", HAlign.Right, 12);
                ConfigureGridColumn(band, "CostAmount", "Total Cost", 100, "#,##0.00", HAlign.Right, 13);
                ConfigureGridColumn(band, "Profit", "Profit", 105, "#,##0.00", HAlign.Right, 14);
                ConfigureGridColumn(band, "MarginPercent", "Margin %", 80, "#,##0.00", HAlign.Right, 15);
                ConfigureGridColumn(band, "Paymode", "Pay Mode", 90, null, HAlign.Center, 16);
                ConfigureGridColumn(band, "BilledBy", "Billed By", 90, null, HAlign.Left, 17);

                if (band.Columns.Exists("ItemName"))
                {
                    band.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("Profit"))
                {
                    band.Columns["Profit"].CellAppearance.ForeColor = ProfitGreenColor;
                    band.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            // Band 1: Hierarchical Drilldown Breakdown
            if (e.Layout.Bands.Count > 1 && isMatrix)
            {
                UltraGridBand childBand = e.Layout.Bands[1];
                childBand.ColHeadersVisible = true;
                childBand.HeaderVisible = true;
                childBand.Header.Caption = "Sales & Profit Breakdown (Date, Bill No, Customer, Qty, Selling Rate, Cost Rate, Sales Amt, Cost Amt, Profit, Margin %)";
                childBand.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                childBand.Header.Appearance.BackColor2 = Color.FromArgb(31, 98, 145);
                childBand.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                childBand.Header.Appearance.ForeColor = Color.White;
                childBand.Header.Appearance.BorderColor = BorderBlue;
                childBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                childBand.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                childBand.Header.Appearance.FontData.SizeInPoints = 8.5F;

                childBand.Override.AllowAddNew = AllowAddNew.No;
                childBand.Override.AllowDelete = DefaultableBoolean.False;
                childBand.Override.AllowUpdate = DefaultableBoolean.False;
                childBand.Override.RowSelectors = DefaultableBoolean.False;

                childBand.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                childBand.Override.HeaderAppearance.BackColor2 = Color.FromArgb(50, 100, 150);
                childBand.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                childBand.Override.HeaderAppearance.ForeColor = Color.White;
                childBand.Override.HeaderAppearance.BorderColor = BorderBlue;
                childBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                childBand.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                childBand.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

                childBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);
                childBand.Override.RowAppearance.BackColor = Color.White;
                childBand.Override.RowAppearance.BorderColor = GridRowLine;
                childBand.Override.RowAlternateAppearance.BorderColor = GridRowLine;
                childBand.Override.CellAppearance.BorderColor = GridRowLine;
                childBand.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                childBand.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                childBand.Override.BorderStyleRow = UIElementBorderStyle.Solid;

                foreach (UltraGridColumn column in childBand.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureChildGridColumn(childBand, "BillDate", "Sales Date", 105, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureChildGridColumn(childBand, "InvoiceNo", "Invoice / Bill No", 115, null, HAlign.Left, 1);
                ConfigureChildGridColumn(childBand, "CustomerName", "Customer", 180, null, HAlign.Left, 2);
                ConfigureChildGridColumn(childBand, "Unit", "Unit", 60, null, HAlign.Center, 3);
                ConfigureChildGridColumn(childBand, "Qty", "Sold Qty", 85, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(childBand, "Rate", "Selling Rate", 90, "#,##0.00", HAlign.Right, 5);
                ConfigureChildGridColumn(childBand, "CostRate", "Cost Rate", 90, "#,##0.00", HAlign.Right, 6);
                ConfigureChildGridColumn(childBand, "SalesAmount", "Sales Amt", 105, "#,##0.00", HAlign.Right, 7);
                ConfigureChildGridColumn(childBand, "CostAmount", "Total Cost", 100, "#,##0.00", HAlign.Right, 8);
                ConfigureChildGridColumn(childBand, "Profit", "Profit", 105, "#,##0.00", HAlign.Right, 9);
                ConfigureChildGridColumn(childBand, "MarginPercent", "Margin %", 80, "#,##0.00", HAlign.Right, 10);

                if (childBand.Columns.Exists("Qty"))
                {
                    childBand.Columns["Qty"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    childBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (childBand.Columns.Exists("SalesAmount"))
                {
                    childBand.Columns["SalesAmount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    childBand.Columns["SalesAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (childBand.Columns.Exists("Profit"))
                {
                    childBand.Columns["Profit"].CellAppearance.ForeColor = ProfitGreenColor;
                    childBand.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            ReportGridLayoutHelper.ApplySavedLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (isMatrix && band.Columns.Exists("Total"))
            {
                band.Columns["Total"].Hidden = false;
                band.Columns["Total"].Header.Caption = "Total";
                band.Columns["Total"].Header.VisiblePosition = _currentMatrixDateColumns.Count + 1;
            }

            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
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
            column.Header.Appearance.BackColor = Color.FromArgb(70, 130, 180);
            column.Header.Appearance.BackColor2 = Color.FromArgb(50, 100, 150);
            column.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
            column.Header.Appearance.ForeColor = Color.White;
            column.Header.Appearance.BorderColor = BorderBlue;
            column.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
            column.Header.Appearance.FontData.SizeInPoints = 8.25F;
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
            if (e.Row.Band == null || e.Row.Band.Index == 0)
            {
                bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

                if (isMatrix)
                {
                    e.Row.ExpansionIndicator = ShowExpansionIndicator.Always;

                    foreach (var dateCol in _currentMatrixDateColumns)
                    {
                        if (e.Row.Cells.Exists(dateCol.ColumnKey))
                        {
                            var cell = e.Row.Cells[dateCol.ColumnKey];
                            if (cell.Value != null && cell.Value != DBNull.Value && decimal.TryParse(Convert.ToString(cell.Value), out decimal val))
                            {
                                if (val > 0)
                                {
                                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                                    cell.Appearance.ForeColor = _isSalesAmountMode ? Color.FromArgb(13, 71, 161) : ProfitGreenColor;
                                    cell.Appearance.BackColor = _isSalesAmountMode ? Color.FromArgb(242, 248, 255) : Color.FromArgb(242, 250, 244);
                                }
                                else if (val < 0)
                                {
                                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                                    cell.Appearance.ForeColor = LossRedColor;
                                    cell.Appearance.BackColor = Color.FromArgb(255, 242, 242);
                                }
                                else
                                {
                                    cell.Appearance.ForeColor = Color.FromArgb(170, 185, 205);
                                }
                            }
                        }
                    }

                    if (e.Row.Cells.Exists("Total"))
                    {
                        var totalCell = e.Row.Cells["Total"];
                        totalCell.Appearance.FontData.Bold = DefaultableBoolean.True;
                        if (totalCell.Value != null && totalCell.Value != DBNull.Value && decimal.TryParse(Convert.ToString(totalCell.Value), out decimal tv))
                        {
                            if (tv > 0)
                            {
                                totalCell.Appearance.ForeColor = _isSalesAmountMode ? Color.FromArgb(13, 71, 161) : ProfitGreenColor;
                                totalCell.Appearance.BackColor = _isSalesAmountMode ? Color.FromArgb(238, 245, 255) : Color.FromArgb(238, 250, 240);
                            }
                            else if (tv < 0)
                            {
                                totalCell.Appearance.ForeColor = LossRedColor;
                                totalCell.Appearance.BackColor = Color.FromArgb(255, 238, 238);
                            }
                        }
                    }

                    if (e.Row.Cells.Exists("TotalProfit"))
                    {
                        e.Row.Cells["TotalProfit"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["TotalProfit"].Appearance.ForeColor = ProfitGreenColor;
                    }

                    if (e.Row.Cells.Exists("TotalSales"))
                    {
                        e.Row.Cells["TotalSales"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["TotalSales"].Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                    }

                    if (e.Row.Cells.Exists("MarginPct"))
                    {
                        e.Row.Cells["MarginPct"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        if (decimal.TryParse(Convert.ToString(e.Row.Cells["MarginPct"].Value), out decimal mVal))
                        {
                            e.Row.Cells["MarginPct"].Appearance.ForeColor = mVal >= 0 ? ProfitGreenColor : LossRedColor;
                        }
                    }
                }
                else
                {
                    if (e.Row.Cells.Exists("Profit"))
                    {
                        e.Row.Cells["Profit"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        if (decimal.TryParse(Convert.ToString(e.Row.Cells["Profit"].Value), out decimal pVal))
                        {
                            e.Row.Cells["Profit"].Appearance.ForeColor = pVal >= 0 ? ProfitGreenColor : LossRedColor;
                        }
                    }

                    if (e.Row.Cells.Exists("NetAmount"))
                    {
                        e.Row.Cells["NetAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["NetAmount"].Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                    }
                }
            }
            else if (e.Row.Band.Index == 1)
            {
                if (e.Row.Cells.Exists("Profit"))
                {
                    var cell = e.Row.Cells["Profit"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    if (decimal.TryParse(Convert.ToString(cell.Value), out decimal p))
                    {
                        cell.Appearance.ForeColor = p >= 0 ? ProfitGreenColor : LossRedColor;
                    }
                }
                if (e.Row.Cells.Exists("MarginPercent"))
                {
                    var cell = e.Row.Cells["MarginPercent"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    if (decimal.TryParse(Convert.ToString(cell.Value), out decimal m))
                    {
                        cell.Appearance.ForeColor = m >= 0 ? ProfitGreenColor : LossRedColor;
                    }
                }
                if (e.Row.Cells.Exists("SalesAmount"))
                {
                    var cell = e.Row.Cells["SalesAmount"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    cell.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                }
                if (e.Row.Cells.Exists("CostAmount"))
                {
                    var cell = e.Row.Cells["CostAmount"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    cell.Appearance.ForeColor = Color.FromArgb(71, 85, 105);
                }
            }
        }
        #endregion

        #region Actions & Exports
        private void btnToggleValueMode_Click(object sender, EventArgs e)
        {
            _isSalesAmountMode = !_isSalesAmountMode;
            UpdateValueModeButtonText();

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode?.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);
            if (isMatrix && _reportRows != null && _reportRows.Count > 0)
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                DatewiseProfitReportFilter filter = new DatewiseProfitReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    CustomerLedgerId = ultraComboCustomer.Value != null ? Convert.ToInt32(ultraComboCustomer.Value) : 0,
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DATE_MATRIX"
                };

                List<PivotDateColumnInfo> matrixCols;
                DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                _matrixDataSet = ds;
                _matrixTable = ds.Tables["ProfitMatrix"];
                _currentMatrixDateColumns.Clear();
                _currentMatrixDateColumns.AddRange(matrixCols);

                gridReport.DataSource = null;
                gridReport.DataSource = _matrixDataSet;

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            else
            {
                LoadReport();
            }
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating print preview: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            ShowReportPreview();
        }

        private void ShowReportPreview()
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Date-wise Profit Report - Preview";
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
                    Text = "DATE-WISE PROFIT REPORT",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Height = 24
                };

                string periodText = $"Period: {ultraComboPeriodMode.Text} | From: {dtFrom.DateTime:dd/MM/yyyy} To: {dtTo.DateTime:dd/MM/yyyy}";
                string customerText = $"Customer: {ultraComboCustomer.Text}";
                string itemText = string.IsNullOrWhiteSpace(txtItemSearch.Text) ? "All Items" : $"Item: {txtItemSearch.Text}";
                string valueModeText = $"Displaying: {(_isSalesAmountMode ? "Sales Amount" : "Profit")}";

                Label metaLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"{periodText}  |  {customerText}  |  {itemText}  |  {valueModeText}\nGenerated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                    ForeColor = Color.FromArgb(232, 246, 255),
                    Font = new Font("Segoe UI", 8.25F, FontStyle.Regular)
                };

                header.Controls.Add(metaLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                preview.Controls.Add(previewGrid);
                preview.Controls.Add(header);

                if (_matrixDataSet != null)
                {
                    previewGrid.DataSource = _matrixDataSet.Copy();
                }
                else if (gridReport.DataSource != null)
                {
                    previewGrid.DataSource = gridReport.DataSource;
                }

                preview.ShowDialog(this);
            }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = $"DatewiseProfitReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();

                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                var visibleCols = band.Columns.Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "ItemProfitBreakdown", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Header.VisiblePosition)
                    .ToList();

                builder.AppendLine(string.Join(",", visibleCols.Select(c => EscapeCsv(c.Header.Caption ?? c.Key))));

                foreach (UltraGridRow row in GetVisibleDataRows())
                {
                    var lineValues = visibleCols.Select(c =>
                    {
                        object val = row.Cells.Exists(c.Key) ? row.Cells[c.Key].Value : null;
                        if (val == null || val == DBNull.Value) return string.Empty;
                        if (val is decimal decVal)
                            return decVal.ToString("F2");
                        return EscapeCsv(Convert.ToString(val));
                    });
                    builder.AppendLine(string.Join(",", lineValues));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            string safe = value ?? string.Empty;
            if (!safe.Contains(",") && !safe.Contains("\"") && !safe.Contains("\n"))
                return safe;
            return $"\"{safe.Replace("\"", "\"\"")}\"";
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _isSalesAmountMode = false;
                UpdateValueModeButtonText();
                ultraComboPeriodMode.Value = "DATE_RANGE";
                dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtTo.Value = DateTime.Today;
                ultraComboMonth.Value = DateTime.Today.Month;
                ultraComboYear.Value = DateTime.Today.Year;
                ultraComboViewMode.Value = "DATE_MATRIX";
                txtItemSearch.Text = string.Empty;
                ultraComboCustomer.Value = 0;
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }
            LoadReport();
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
        }

        private void btnItemPicker_Click(object sender, EventArgs e)
        {
            try
            {
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmdatewiseprofitreport"))
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
                            txtItemSearch.Text = itemName;
                            LoadReport();
                        }
                    }
                }
            }
            catch
            {
                txtItemSearch.Focus();
            }
        }

        private void btnCustomerPicker_Click(object sender, EventArgs e)
        {
            try
            {
                using (frmCustomerDialog customerDialog = new frmCustomerDialog())
                {
                    if (customerDialog.ShowDialog(this) == DialogResult.OK &&
                        customerDialog.SelectedCustomerId > 0)
                    {
                        ultraComboCustomer.Value = customerDialog.SelectedCustomerId;
                        LoadReport();
                    }
                }
            }
            catch
            {
                ultraComboCustomer.Focus();
            }
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.Rows.ExpandAll(true);
                UpdateFooterCellPositions();
            }
            catch { }
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.Rows.CollapseAll(true);
                UpdateFooterCellPositions();
            }
            catch { }
        }

        private string GetLayoutKey()
        {
            string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
            return $"DatewiseProfitReport_{viewMode}";
        }
        #endregion
    }
}
