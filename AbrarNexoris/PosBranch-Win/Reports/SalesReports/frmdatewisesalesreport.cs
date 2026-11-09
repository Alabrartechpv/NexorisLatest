using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
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
    /// Date-wise Sales Report (frmdatewisesalesreport)
    /// Displays item-wise sales across selected date ranges, months, years, or all dates,
    /// with dynamic date matrix columns, Quantity / Net Amount toggle, and customizable footer totals.
    /// Theme and features match frmdatewisepurchasereport and frmVendorOutstandingReport.
    /// </summary>
    public partial class frmdatewisesalesreport : Form
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
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);
        #endregion

        #region Private Fields
        private readonly DatewiseSalesReportRepository _repository;
        private List<DatewiseSalesReportRow> _reportRows;
        private readonly List<PivotDateColumnInfo> _currentMatrixDateColumns = new List<PivotDateColumnInfo>();
        private DataSet _matrixDataSet;
        private DataTable _matrixTable;
        private bool _isLoading;
        private bool _isAmountMode = false;

        // Dynamic footer panel and cell controls
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Qty",
            "Rate",
            "Amount",
            "DiscountAmt",
            "TaxAmt",
            "NetAmount",
            "Profit",
            "Total",
            "TotalQty",
            "TotalAmount"
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
            public decimal TotalAmount { get; set; }
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
        public frmdatewisesalesreport()
        {
            _repository = new DatewiseSalesReportRepository();
            _reportRows = new List<DatewiseSalesReportRow>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmdatewisesalesreport_Load;
            FormClosing += frmdatewisesalesreport_FormClosing;
            KeyDown += frmdatewisesalesreport_KeyDown;
            KeyPreview = true;

            // Action buttons
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

            // Filter value changes
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

            // Grid event registrations for footer cell sync and column drag
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
        }

        private string GetLayoutKey()
        {
            string viewMode = Convert.ToString(ultraComboViewMode?.Value) ?? "DATE_MATRIX";
            return $"DatewiseSalesReport_{viewMode}";
        }

        private void frmdatewisesalesreport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmdatewisesalesreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
        }

        private void frmdatewisesalesreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
            {
                btnExportGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnViewGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void InitializeForm()
        {
            _isLoading = true;
            try
            {
                Text = "Date-wise Sales Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                UpdateValueModeButtonText();
                StyleFilterControls();
                SetupGrid();
                LoadCustomers();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
                LoadReport();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;

            // Period Modes: Date Range, Month Wise, Year Wise, ALL
            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date Range");
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("ALL", "ALL Dates");
            ultraComboPeriodMode.Value = "DATE_RANGE";

            // Date Pickers
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            // Month Selector (0 = ALL Months, 1..12 = Jan..Dec)
            ultraComboMonth.Items.Clear();
            ultraComboMonth.Items.Add(0, "ALL Months");
            for (int m = 1; m <= 12; m++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                ultraComboMonth.Items.Add(m, monthName);
            }
            ultraComboMonth.Value = today.Month;

            // Year Selector (0 = ALL Years, current down to 2000)
            ultraComboYear.Items.Clear();
            ultraComboYear.Items.Add(0, "ALL Years");
            for (int y = today.Year; y >= 2000; y--)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = today.Year;

            // View Mode Selector
            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Item vs Dates)");
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
                gridReport.Dock = DockStyle.Fill;
                gridReport.BringToFront();
            }

            StyleLabel(lblPeriodMode);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblViewMode);
            StyleLabel(lblItem);
            StyleLabel(lblCustomer);
        }

        private void StyleButtons()
        {
            StyleActionButton(btnViewGrid);
            StyleActionButton(btnPreviewGrid);
            StyleActionButton(btnPreviewReport);
            StyleActionButton(btnExportGrid);
            StyleActionButton(btnClearFilters);
            StyleActionButton(btnColumnChooser);
            StyleActionButton(btnToggleValueMode);
            StyleActionButton(btnToggleSelection);

            StyleSmallPickerButton(btnItemPicker);
            StyleSmallPickerButton(btnCustomerPicker);
        }

        private static void StyleActionButton(UltraButton button)
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
            button.Appearance.FontData.Name = "Microsoft Sans Serif";
            button.Appearance.FontData.SizeInPoints = 8.25F;
            button.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BorderColor = Color.FromArgb(60, 120, 200);
            button.HotTrackAppearance.ForeColor = Color.FromArgb(10, 35, 85);
        }

        private static void StyleSmallPickerButton(Button button)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = SkyBlueOutline;
            button.BackColor = ButtonBlueTop;
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private void StyleFilterControls()
        {
            StyleCombo(ultraComboPeriodMode);
            StyleCombo(ultraComboMonth);
            StyleCombo(ultraComboYear);
            StyleCombo(ultraComboViewMode);
            StyleCombo(txtItemSearch);
            StyleCombo(ultraComboCustomer);

            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
        }

        private static void StyleCombo(UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
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
            editor.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleLabel(UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false; // Enable Outlook GroupByBox so user can group by Date or Item!
            layout.GroupByBox.Prompt = "Drag a column header here (such as Sale Date or Item Name) to group records.";

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
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

        #region Custom Footer Sync & Aggregation
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
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemSalesBreakdown", StringComparison.OrdinalIgnoreCase))
                    continue;

                ContentAlignment align = ContentAlignment.MiddleCenter;
                if (column.CellAppearance.TextHAlign == HAlign.Right)
                    align = ContentAlignment.MiddleRight;
                else if (column.CellAppearance.TextHAlign == HAlign.Left)
                    align = ContentAlignment.MiddleLeft;

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
                    : ((summaryDefaultNumericColumns.Contains(columnKey) || (gridReport.DisplayLayout.Bands.Count > 0 && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) && IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]))) ? "Sum" : "None");

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

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();

            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (string.Equals(columnKey, "ItemName", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = $"Total ({visibleRows.Count} Items)";
                    footerLabel.TextAlign = ContentAlignment.MiddleLeft;
                    footerLabel.Tag = Tuple.Create(columnKey, footerLabel.Text);
                    footerLabel.ForeColor = Color.White;
                    footerLabel.Invalidate();
                    continue;
                }

                if (string.Equals(columnKey, "Unit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "AvgRate", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "ItemID", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "BarCode", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "SlNo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "BillNo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "InvoiceNo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "Paymode", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "BilledBy", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
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
                        .Where(val => val.HasValue)
                        .Select(val => val.Value)
                        .DefaultIfEmpty(0m)
                        .Sum();

                case "Avg":
                case "Average":
                    var avgValues = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(val => val.HasValue)
                        .Select(val => val.Value)
                        .ToList();
                    return avgValues.Count > 0 ? (object)avgValues.Average() : null;

                case "Min":
                    var minValues = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(val => val.HasValue)
                        .Select(val => val.Value)
                        .ToList();
                    return minValues.Count > 0 ? (object)minValues.Min() : null;

                case "Max":
                    var maxValues = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(val => val.HasValue)
                        .Select(val => val.Value)
                        .ToList();
                    return maxValues.Count > 0 ? (object)maxValues.Max() : null;

                case "Count":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Count(row => HasCellValue(row.Cells[columnKey].Value));

                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null) return string.Empty;

            if (aggregation == "Count")
            {
                return string.Format("{0:N0}", Convert.ToInt64(result));
            }

            if (decimal.TryParse(Convert.ToString(result), out decimal numVal))
            {
                return numVal.ToString("#,##0.00");
            }

            return Convert.ToString(result);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null) yield break;
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut && !row.Hidden && (row.Band == null || row.Band.Index == 0))
                {
                    yield return row;
                }
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            string clean = Convert.ToString(value).Replace("₹", "").Replace("$", "").Replace("RM", "").Trim();
            return decimal.TryParse(clean, out decimal res) ? res : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null) return false;
            return string.Equals(column.Key, "Qty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Rate", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Amount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "DiscountAmt", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TaxAmt", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "NetAmount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Profit", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Total", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TotalQty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TotalAmount", StringComparison.OrdinalIgnoreCase) ||
                   column.DataType == typeof(decimal) ||
                   column.DataType == typeof(double) ||
                   column.DataType == typeof(float) ||
                   column.DataType == typeof(int) ||
                   column.DataType == typeof(long);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser
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
            _filterManager = new ReportColumnFilterManager(gridReport, () =>
            {
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

                if (headerUI != null && headerUI.Header != null && headerUI.Header.Column != null)
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

                if (headerUI != null && headerUI.Header != null && headerUI.Header.Column != null)
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
            if (!e.Data.GetDataPresent(typeof(ColumnChooserItem))) return;

            ColumnChooserItem item = e.Data.GetData(typeof(ColumnChooserItem)) as ColumnChooserItem;
            if (item != null)
            {
                Point clientPt = gridReport.PointToClient(new Point(e.X, e.Y));
                int targetPos = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, targetPos);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header != null && headerUI.Header.Column != null)
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
            if (col == null || col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True)
                return;

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

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field / Column Chooser...", null, (s, e) => btnColumnChooser.PerformClick());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(gridReport, location);
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                BuildColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            PositionColumnChooser();
            columnChooserForm.Show(this);
            columnChooserForm.BringToFront();
        }

        private void BuildColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Column Chooser",
                Size = new Size(240, 360),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                TopMost = false,
                BackColor = FormBackColor
            };

            Panel pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(6),
                BackColor = ActionPanelBackColor
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0)
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();
            pnlSearch.Controls.Add(txtColumnSearch);

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };

            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;

            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(6),
                BackColor = ActionPanelBackColor
            };

            Button btnResetColumns = new Button
            {
                Text = "Reset All",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonBlueTop,
                ForeColor = ButtonTextBlue,
                Cursor = Cursors.Hand
            };
            btnResetColumns.FlatAppearance.BorderColor = SkyBlueOutline;
            btnResetColumns.Click += (s, e) => UnhideAllColumns();
            pnlBottom.Controls.Add(btnResetColumns);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(pnlSearch);
            columnChooserForm.Controls.Add(pnlBottom);
            columnChooserForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    columnChooserForm.Hide();
                }
            };
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                int pos = gridReport.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>().Count(c => !c.Hidden);
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
                ColumnChooserItem item = columnChooserListBox.Items[index] as ColumnChooserItem;
                if (item != null)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = e.Bounds;
            rect.Inflate(-2, -2);

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color topGrad = isSelected ? Color.FromArgb(240, 246, 254) : Color.FromArgb(254, 255, 255);
            Color botGrad = isSelected ? Color.FromArgb(198, 222, 250) : Color.FromArgb(224, 233, 244);
            Color borderCol = isSelected ? Color.FromArgb(120, 168, 222) : Color.FromArgb(184, 200, 218);

            using (GraphicsPath path = CreateRoundedRectanglePath(rect, 4))
            {
                using (LinearGradientBrush lgb = new LinearGradientBrush(rect, topGrad, botGrad, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(lgb, path);
                }
                using (Pen p = new Pen(borderCol, 1f))
                {
                    e.Graphics.DrawPath(p, path);
                }
            }

            string text = columnChooserListBox.Items[e.Index].ToString();
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(14, 47, 108)))
            {
                e.Graphics.DrawString(text, columnChooserListBox.Font, textBrush, rect.Left + 8, rect.Top + 7);
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
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
                if (col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True && !col.IsChaptered && !string.Equals(col.Key, "ItemSalesBreakdown", StringComparison.OrdinalIgnoreCase))
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

        #region Data Loading & Grid Binding
        private DataTable BuildMatrixDataTable(List<DatewiseSalesReportRow> rawRows, DatewiseSalesReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
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
                        var distinctDates = (rawRows ?? new List<DatewiseSalesReportRow>())
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
                var distinctDates = (rawRows ?? new List<DatewiseSalesReportRow>())
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
            DataTable table = new DataTable("SalesMatrix");
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

            var cTotalQty = table.Columns.Add("TotalQty", typeof(decimal));
            cTotalQty.DefaultValue = 0m;

            var cTotalAmt = table.Columns.Add("TotalAmount", typeof(decimal));
            cTotalAmt.DefaultValue = 0m;

            var cAvgRate = table.Columns.Add("AvgRate", typeof(decimal));
            cAvgRate.DefaultValue = 0m;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Fast pre-aggregation by item and date for Quantity and Net Amount
            Dictionary<string, decimal> dateQtyMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> dateAmtMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ItemMatrixInfo> itemInfoMap = new Dictionary<string, ItemMatrixInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseSalesReportRow r in rawRows)
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
                        TotalAmount = 0m
                    };
                    itemInfoMap[itemKey] = itemInfo;
                }
                else if (string.IsNullOrEmpty(itemInfo.BarCode) && !string.IsNullOrEmpty(r.BarCode))
                {
                    itemInfo.BarCode = r.BarCode;
                }

                decimal rowAmount = (r.NetAmount != 0m) ? r.NetAmount : r.Amount;
                itemInfo.TotalQty += r.Qty;
                itemInfo.TotalAmount += rowAmount;

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

                        decimal currentQty = 0m;
                        dateQtyMap.TryGetValue(cellKey, out currentQty);
                        dateQtyMap[cellKey] = currentQty + r.Qty;

                        decimal currentAmt = 0m;
                        dateAmtMap.TryGetValue(cellKey, out currentAmt);
                        dateAmtMap[cellKey] = currentAmt + rowAmount;
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
                    if (_isAmountMode)
                    {
                        dateAmtMap.TryGetValue(cellKey, out cellVal);
                    }
                    else
                    {
                        dateQtyMap.TryGetValue(cellKey, out cellVal);
                    }
                    dr[dateCol.ColumnKey] = cellVal;
                }

                dr["Total"] = _isAmountMode ? item.TotalAmount : item.TotalQty;
                dr["TotalQty"] = item.TotalQty;
                dr["TotalAmount"] = item.TotalAmount;
                dr["AvgRate"] = item.TotalQty > 0 ? Math.Round(item.TotalAmount / item.TotalQty, 2) : 0m;

                table.Rows.Add(dr);
            }

            return table;
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

        private DataTable BuildDetailsDataTable(List<DatewiseSalesReportRow> rawRows)
        {
            DataTable details = new DataTable("SalesDetails");
            details.Columns.Add("ItemKey", typeof(string));
            details.Columns.Add("BillDate", typeof(DateTime));
            details.Columns.Add("BillNo", typeof(long));
            details.Columns.Add("InvoiceNo", typeof(string));
            details.Columns.Add("CustomerName", typeof(string));
            details.Columns.Add("Unit", typeof(string));
            details.Columns.Add("Qty", typeof(decimal));
            details.Columns.Add("Rate", typeof(decimal));
            details.Columns.Add("DiscountAmt", typeof(decimal));
            details.Columns.Add("TaxAmt", typeof(decimal));
            details.Columns.Add("NetAmount", typeof(decimal));

            if (rawRows != null && rawRows.Count > 0)
            {
                foreach (DatewiseSalesReportRow r in rawRows.OrderBy(x => x.BillDate).ThenBy(x => x.BillNo).ThenBy(x => x.SlNo))
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
                    dr["DiscountAmt"] = r.DiscountAmt;
                    dr["TaxAmt"] = r.TaxAmt;
                    dr["NetAmount"] = (r.NetAmount != 0 ? r.NetAmount : (r.Qty * r.Rate));
                    details.Rows.Add(dr);
                }
            }

            return details;
        }

        private DataSet BuildMatrixDataSet(List<DatewiseSalesReportRow> rawRows, DatewiseSalesReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable detailsTable = BuildDetailsDataTable(rawRows);

            DataSet ds = new DataSet("DatewiseSalesDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(detailsTable);

            DataRelation relation = new DataRelation("ItemSalesBreakdown", matrixTable.Columns["ItemKey"], detailsTable.Columns["ItemKey"], false);
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

                DatewiseSalesReportFilter filter = new DatewiseSalesReportFilter
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
                    ViewMode = "DETAILED" // Fetch transaction details so drilldown displays real invoice, customer, and item rates
                };

                _reportRows = _repository.GetReport(filter);

                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    List<PivotDateColumnInfo> matrixCols;
                    DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                    _matrixDataSet = ds;
                    _matrixTable = ds.Tables["SalesMatrix"];
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

                    gridReport.DataSource = null;
                    gridReport.DataSource = _reportRows;
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Date-wise Sales Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = prev;
            }
        }

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

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            e.Layout.ViewStyleBand = ViewStyleBand.Vertical;
            e.Layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;

            UltraGridBand band = e.Layout.Bands[0];
            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            if (isMatrix)
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    if (column.IsChaptered || string.Equals(column.Key, "ItemSalesBreakdown", StringComparison.OrdinalIgnoreCase))
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
                        ConfigureGridColumn(band, dateCol.ColumnKey, dateCol.HeaderCaption, 55, "#,##0.00", HAlign.Right, visPos++);
                        string modeDesc = _isAmountMode ? "Net Amount" : "Qty";
                        band.Columns[dateCol.ColumnKey].Header.ToolTipText = $"{dateCol.Tooltip} ({modeDesc})";
                    }
                }

                ConfigureGridColumn(band, "Total", "Total", 90, "#,##0.00", HAlign.Right, visPos++);
                if (band.Columns.Exists("Total"))
                {
                    band.Columns["Total"].Hidden = false;
                    band.Columns["Total"].Header.Caption = "Total";
                    band.Columns["Total"].Header.ToolTipText = _isAmountMode ? "Total Net Amount across dates" : "Total Quantity across dates";
                    band.Columns["Total"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Total"].CellAppearance.ForeColor = _isAmountMode ? Color.FromArgb(27, 94, 32) : Color.FromArgb(13, 71, 161);
                }

                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
                ConfigureGridColumn(band, "TotalQty", "Total Qty", 85, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalAmount", "Total Amount", 105, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "AvgRate", "Avg Rate", 80, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "BarCode", "Barcode", 110, null, HAlign.Left, visPos++);

                if (band.Columns.Exists("Unit") && !userHiddenColumnKeys.Contains("Unit"))
                {
                    band.Columns["Unit"].Hidden = true;
                }
                if (band.Columns.Exists("TotalQty"))
                {
                    band.Columns["TotalQty"].Hidden = true;
                }
                if (band.Columns.Exists("TotalAmount"))
                {
                    band.Columns["TotalAmount"].Hidden = true;
                }
                if (band.Columns.Exists("AvgRate"))
                {
                    band.Columns["AvgRate"].Hidden = true;
                }
                if (band.Columns.Exists("BarCode"))
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

                ConfigureGridColumn(band, "BillDate", "Sale Date", 105, "dd-MMM-yyyy", HAlign.Left, 0);
                ConfigureGridColumn(band, "BillNo", "Bill No", 95, null, HAlign.Left, 1);
                ConfigureGridColumn(band, "InvoiceNo", "Invoice No", 110, null, HAlign.Left, 2);
                ConfigureGridColumn(band, "InvoiceDate", "Invoice Date", 100, "dd-MMM-yyyy", HAlign.Left, 3);
                ConfigureGridColumn(band, "CustomerName", "Customer Name", 200, null, HAlign.Left, 4);
                ConfigureGridColumn(band, "BarCode", "Barcode", 115, null, HAlign.Left, 5);
                ConfigureGridColumn(band, "ItemName", "Item Description", 230, null, HAlign.Left, 6);
                ConfigureGridColumn(band, "Unit", "Unit", 65, null, HAlign.Center, 7);
                ConfigureGridColumn(band, "Qty", "Qty", 80, "#,##0.00", HAlign.Right, 8);
                ConfigureGridColumn(band, "Rate", "Rate", 95, "#,##0.00", HAlign.Right, 9);
                ConfigureGridColumn(band, "Amount", "Amount", 105, "#,##0.00", HAlign.Right, 10);
                ConfigureGridColumn(band, "DiscountAmt", "Discount", 85, "#,##0.00", HAlign.Right, 11);
                ConfigureGridColumn(band, "TaxAmt", "Tax Amount", 90, "#,##0.00", HAlign.Right, 12);
                ConfigureGridColumn(band, "NetAmount", "Net Amount", 110, "#,##0.00", HAlign.Right, 13);
                ConfigureGridColumn(band, "Profit", "Profit", 95, "#,##0.00", HAlign.Right, 14);
                ConfigureGridColumn(band, "Paymode", "Pay Mode", 90, null, HAlign.Left, 15);
                ConfigureGridColumn(band, "BilledBy", "Billed By", 100, null, HAlign.Left, 16);

                if (band.Columns.Exists("ItemName"))
                {
                    band.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("Qty"))
                {
                    band.Columns["Qty"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    band.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                    band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            if (e.Layout.Bands.Count > 1 && isMatrix)
            {
                UltraGridBand childBand = e.Layout.Bands[1];
                childBand.ColHeadersVisible = true;
                childBand.HeaderVisible = true;
                childBand.Header.Caption = "Sales Breakdown (Date, Invoice No, Customer, Qty, Rate, Net Amount)";
                childBand.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                childBand.Header.Appearance.ForeColor = Color.White;
                childBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                childBand.Override.AllowAddNew = AllowAddNew.No;
                childBand.Override.AllowDelete = DefaultableBoolean.False;
                childBand.Override.AllowUpdate = DefaultableBoolean.False;
                childBand.Override.RowSelectors = DefaultableBoolean.False;
                childBand.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                childBand.Override.HeaderAppearance.ForeColor = Color.White;
                childBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                childBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);
                childBand.Override.CellAppearance.BorderColor = GridRowLine;

                foreach (UltraGridColumn column in childBand.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureChildGridColumn(childBand, "BillDate", "Sales Date", 110, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureChildGridColumn(childBand, "InvoiceNo", "Invoice / Bill No", 125, null, HAlign.Left, 1);
                ConfigureChildGridColumn(childBand, "CustomerName", "Customer", 200, null, HAlign.Left, 2);
                ConfigureChildGridColumn(childBand, "Unit", "Unit", 65, null, HAlign.Center, 3);
                ConfigureChildGridColumn(childBand, "Qty", "Sold Qty", 100, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(childBand, "Rate", "Unit Rate", 100, "#,##0.00", HAlign.Right, 5);
                ConfigureChildGridColumn(childBand, "DiscountAmt", "Discount", 90, "#,##0.00", HAlign.Right, 6);
                ConfigureChildGridColumn(childBand, "TaxAmt", "Tax Amount", 95, "#,##0.00", HAlign.Right, 7);
                ConfigureChildGridColumn(childBand, "NetAmount", "Net Amount", 120, "#,##0.00", HAlign.Right, 8);

                if (childBand.Columns.Exists("Qty"))
                {
                    childBand.Columns["Qty"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    childBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (childBand.Columns.Exists("Rate"))
                {
                    childBand.Columns["Rate"].CellAppearance.ForeColor = Color.FromArgb(20, 40, 80);
                    childBand.Columns["Rate"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (childBand.Columns.Exists("NetAmount"))
                {
                    childBand.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                    childBand.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
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
                            if (cell.Value != null && cell.Value != DBNull.Value && decimal.TryParse(Convert.ToString(cell.Value), out decimal q))
                            {
                                if (q > 0)
                                {
                                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                                    cell.Appearance.ForeColor = _isAmountMode ? Color.FromArgb(27, 94, 32) : Color.FromArgb(10, 45, 120);
                                    cell.Appearance.BackColor = Color.FromArgb(242, 248, 255);
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
                        totalCell.Appearance.ForeColor = _isAmountMode ? Color.FromArgb(27, 94, 32) : Color.FromArgb(13, 71, 161);
                        if (totalCell.Value != null && totalCell.Value != DBNull.Value && decimal.TryParse(Convert.ToString(totalCell.Value), out decimal tv) && tv > 0)
                        {
                            totalCell.Appearance.BackColor = Color.FromArgb(238, 245, 255);
                        }
                    }

                    if (e.Row.Cells.Exists("TotalQty"))
                    {
                        e.Row.Cells["TotalQty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["TotalQty"].Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                    }

                    if (e.Row.Cells.Exists("TotalAmount"))
                    {
                        e.Row.Cells["TotalAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["TotalAmount"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                    }
                }
                else
                {
                    if (e.Row.Cells.Exists("NetAmount"))
                    {
                        e.Row.Cells["NetAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["NetAmount"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                    }
                }
            }
            else if (e.Row.Band.Index == 1)
            {
                if (e.Row.Cells.Exists("Qty"))
                {
                    var cell = e.Row.Cells["Qty"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    cell.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                }
                if (e.Row.Cells.Exists("Rate"))
                {
                    var cell = e.Row.Cells["Rate"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    cell.Appearance.ForeColor = Color.FromArgb(20, 40, 80);
                }
                if (e.Row.Cells.Exists("NetAmount"))
                {
                    var cell = e.Row.Cells["NetAmount"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    cell.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                }
            }
        }
        #endregion

        #region Actions & Exports
        private void btnToggleValueMode_Click(object sender, EventArgs e)
        {
            _isAmountMode = !_isAmountMode;
            UpdateValueModeButtonText();

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode?.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);
            if (isMatrix && _reportRows != null && _reportRows.Count > 0)
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                DatewiseSalesReportFilter filter = new DatewiseSalesReportFilter
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
                _matrixTable = ds.Tables["SalesMatrix"];
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

        private void UpdateValueModeButtonText()
        {
            if (btnToggleValueMode != null)
            {
                btnToggleValueMode.Text = _isAmountMode ? "Show Qty" : "Show Amount";
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
                preview.Text = "Date-wise Sales Report - Preview";
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
                    Text = "DATE-WISE SALES REPORT",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    Height = 28
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"Generated On: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Mode: {ultraComboPeriodMode.Text} | Total Records: {gridReport.Rows.Count}",
                    ForeColor = Color.FromArgb(220, 235, 255),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(subtitleLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.DataSource = gridReport.DataSource;

                preview.Controls.Add(previewGrid);
                preview.Controls.Add(header);

                preview.ShowDialog(this);
            }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void ExportCsv()
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                dialog.FileName = $"DatewiseSalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Date-wise Sales Report")) return;

                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                var visibleCols = band.Columns.Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "ItemSalesBreakdown", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Header.VisiblePosition)
                    .ToList();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine(string.Join(",", visibleCols.Select(c => EscapeCsv(!string.IsNullOrEmpty(c.Header.Caption) ? c.Header.Caption : c.Key))));

                var visibleRows = GetVisibleDataRows().ToList();
                foreach (var row in visibleRows)
                {
                    var lineValues = visibleCols.Select(c =>
                    {
                        object val = row.Cells.Exists(c.Key) ? row.Cells[c.Key].Value : null;
                        if (val == null || val == DBNull.Value) return "";
                        if (decimal.TryParse(Convert.ToString(val), out decimal decVal))
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
                _isAmountMode = false;
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
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmdatewisesalesreport"))
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
        #endregion
    }
}
