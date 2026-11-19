using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports;
using PosBranch_Win.Reports.FinancialReports;
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

namespace PosBranch_Win.Reports.PurchaseReports
{
    /// <summary>
    /// Date-wise Purchase Return Report (frmdatewisepurchasereturnreport)
    /// Displays purchase return (debit note) transactions and item details filtered by Date Range, Month Wise, Year Wise, or ALL dates.
    /// Features exact parity with frmVendorOutstandingReport: theme, grid styling, dynamic footer cell aggregations,
    /// header drag-to-hide, floating column chooser, layout persistence, and CSV/PDF export.
    /// </summary>
    public partial class frmdatewisepurchasereturnreport : Form
    {
        #region Colour Palette (Exact parity with frmVendorOutstandingReport)
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
        private readonly DatewisePurchaseReturnReportRepository _repository;
        private List<DatewisePurchaseReturnReportRow> _reportRows;
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
            "Returned",
            "Free",
            "TotalQty",
            "Cost",
            "Amount",
            "DisAmt",
            "TaxAmt",
            "NetAmount",
            "Total",
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
            public int ReturnCount { get; set; }
            public Dictionary<string, decimal> DayValues { get; } = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class PivotDateColumnInfo
        {
            public string Key { get; set; }
            public string Caption { get; set; }
            public DateTime Date { get; set; }
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

        #region Constructors
        public frmdatewisepurchasereturnreport()
        {
            InitializeComponent();

            _repository = new DatewisePurchaseReturnReportRepository();
            _reportRows = new List<DatewisePurchaseReturnReportRow>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Load += frmdatewisepurchasereturnreport_Load;
            FormClosing += frmdatewisepurchasereturnreport_FormClosing;
            KeyDown += frmdatewisepurchasereturnreport_KeyDown;

            // Wire filter events
            ultraComboPeriodMode.ValueChanged += (s, e) => { if (!_isLoading) { UpdatePeriodControlVisibility(); LoadReport(); } };
            dtFrom.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtTo.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboMonth.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboYear.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboVendor.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            txtItemSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && !_isLoading) { LoadReport(); e.Handled = true; } };

            btnItemPicker.Click += (s, e) => OpenItemPickerDialog();
            btnVendorPicker.Click += (s, e) => OpenVendorPickerDialog();

            // Wire action buttons
            btnViewGrid.Click += (s, e) => LoadReport();
            btnPreviewGrid.Click += (s, e) => PrintGrid(false);
            btnPreviewReport.Click += (s, e) => PrintGrid(true);
            btnExportGrid.Click += (s, e) => ExportGridToCsv();
            btnClearFilters.Click += (s, e) => ClearAllFilters();
            btnColumnChooser.Click += (s, e) => ToggleColumnChooser();
            btnToggleValueMode.Click += (s, e) => ToggleValueMode();
            btnExpandAll.Click += (s, e) => ExpandAllGroups();
            btnCollapseAll.Click += (s, e) => CollapseAllGroups();
            btnToggleSelection.Click += (s, e) => ToggleRowSelection();

            // Grid events
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += (s, e) =>
            {
                if (e.Row != null)
                {
                    if (e.Row.Band.Index == 0 && e.Row.ChildBands != null && e.Row.ChildBands.Count > 0)
                    {
                        e.Row.Expanded = !e.Row.Expanded;
                    }
                    else
                    {
                        OpenPurchaseReturnVoucher(e.Row);
                    }
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
            return $"DatewisePurchaseReturnReport_{viewMode}";
        }

        private void frmdatewisepurchasereturnreport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmdatewisepurchasereturnreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
        }

        private void frmdatewisepurchasereturnreport_KeyDown(object sender, KeyEventArgs e)
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
                Text = "Date-wise Purchase Return Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                UpdateValueModeButtonText();
                StyleFilterControls();
                SetupGrid();
                LoadVendors();
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
                string mName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                ultraComboMonth.Items.Add(m, $"{m:D2} - {mName}");
            }
            ultraComboMonth.Value = today.Month;

            // Year Selector
            ultraComboYear.Items.Clear();
            ultraComboYear.Items.Add(0, "ALL Years");
            for (int y = today.Year; y >= 2000; y--)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = today.Year;

            // View Mode Selector
            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Item vs Dates Return)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Voucher & Item Wise)");
            ultraComboViewMode.Items.Add("DATE_ITEM_SUMMARY", "Daily Summary (Date & Item)");
            ultraComboViewMode.Items.Add("ITEM_SUMMARY", "Item Summary (Total by Item)");
            ultraComboViewMode.Items.Add("VENDOR_SUMMARY", "Vendor Summary (Total by Vendor)");
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

            if (isMonthWise)
            {
                lblMonth.Location = new Point(248, 14);
                ultraComboMonth.Location = new Point(296, 12);
                lblYear.Location = new Point(420, 14);
                ultraComboYear.Location = new Point(458, 12);
            }
            else if (isYearWise)
            {
                lblYear.Location = new Point(248, 14);
                ultraComboYear.Location = new Point(288, 12);
            }
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;

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

            StyleLabel(lblPeriodMode);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblViewMode);
            StyleLabel(lblItem);
            StyleLabel(lblVendor);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.ForeColor = ControlTextColor;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.FontData.Name = "Microsoft Sans Serif";
            label.Appearance.FontData.SizeInPoints = 8.25F;
            label.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
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
            StylePickerButton(btnVendorPicker);
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
            StyleFilterCombo(ultraComboVendor, true);
            StyleFilterCombo(txtItemSearch, false);
            StyleDateTimeEditor(dtFrom);
            StyleDateTimeEditor(dtTo);
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool readOnly)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = BorderBlue;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 8.25F;
            combo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            combo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2007;
            combo.DropDownStyle = readOnly ? DropDownStyle.DropDownList : DropDownStyle.DropDown;
        }

        private static void StyleDateTimeEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = BorderBlue;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 8.25F;
            editor.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            editor.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2007;
        }

        private void LoadVendors()
        {
            try
            {
                ultraComboVendor.Items.Clear();
                ultraComboVendor.Items.Add(0, "ALL Vendors");

                VendorRepository vendorRepo = new VendorRepository();
                VendorDDLGrid ddl = vendorRepo.GetVendorDDL();
                if (ddl?.List != null)
                {
                    foreach (var v in ddl.List.Where(x => x != null && !string.IsNullOrWhiteSpace(x.LedgerName)).OrderBy(x => x.LedgerName))
                    {
                        ultraComboVendor.Items.Add(v.LedgerID, v.LedgerName);
                    }
                }
                ultraComboVendor.Value = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load vendors: {ex.Message}");
            }
        }
        #endregion

        #region Grid Configuration
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

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
            UltraGridBand band0 = e.Layout.Bands[0];

            band0.Override.RowAppearance.FontData.SizeInPoints = 8.5f;
            band0.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band0.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band0.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band0.Override.HeaderAppearance.ForeColor = Color.White;
            band0.Override.HeaderAppearance.BorderColor = BorderBlue;
            band0.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            band0.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            band0.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            foreach (UltraGridColumn column in band0.Columns)
            {
                if (column.IsChaptered || string.Equals(column.Key, "ItemReturnBreakdown", StringComparison.OrdinalIgnoreCase))
                    continue;

                column.Hidden = true;
                column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureMatrixBand(band0);
            }
            else if (string.Equals(viewMode, "DETAILED", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureDetailedBand(band0);
            }
            else if (string.Equals(viewMode, "DATE_ITEM_SUMMARY", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureDateItemSummaryBand(band0);
            }
            else if (string.Equals(viewMode, "ITEM_SUMMARY", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureItemSummaryBand(band0);
            }
            else if (string.Equals(viewMode, "VENDOR_SUMMARY", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureVendorSummaryBand(band0);
            }

            ApplyUserHiddenColumns(band0);

            // Band 1: Child Return Details in Date Matrix view
            if (e.Layout.Bands.Count > 1)
            {
                UltraGridBand band1 = e.Layout.Bands[1];
                ConfigureDetailsChildBand(band1);
            }

            ReportGridLayoutHelper.ApplySavedLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ConfigureMatrixBand(UltraGridBand band)
        {
            band.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band.Override.HeaderAppearance.ForeColor = Color.White;
            band.Override.HeaderAppearance.BorderColor = BorderBlue;
            band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            band.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            band.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

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

            int visPos = 0;
            ConfigureGridColumn(band, "ItemName", "Item Description", 240, null, HAlign.Left, visPos++);

            // Dynamic date columns
            foreach (var dateCol in _currentMatrixDateColumns)
            {
                if (band.Columns.Exists(dateCol.Key))
                {
                    ConfigureGridColumn(band, dateCol.Key, dateCol.Caption, 55, "#,##0.00", HAlign.Right, visPos++);
                    band.Columns[dateCol.Key].Header.Appearance.TextHAlign = HAlign.Center;
                    string modeDesc = _isAmountMode ? "Net Amount" : "Qty";
                    band.Columns[dateCol.Key].Header.ToolTipText = $"{dateCol.Date:dd-MMM-yyyy} ({modeDesc})";
                }
            }

            if (band.Columns.Exists("TotalAmount"))
            {
                ConfigureGridColumn(band, "TotalAmount", "Total Amount", 100, "#,##0.00", HAlign.Right, visPos++);
                band.Columns["TotalAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["TotalAmount"].CellAppearance.ForeColor = Color.FromArgb(180, 40, 40);
            }

            if (band.Columns.Exists("TotalQty"))
            {
                ConfigureGridColumn(band, "TotalQty", "Total Qty", 85, "#,##0.00", HAlign.Right, visPos++);
                band.Columns["TotalQty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("ReturnCount"))
            {
                ConfigureGridColumn(band, "ReturnCount", "Vouchers", 70, "#,##0", HAlign.Right, visPos++);
            }

            if (band.Columns.Exists("Unit"))
            {
                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
            }

            if (band.Columns.Exists("BarCode"))
            {
                ConfigureGridColumn(band, "BarCode", "Barcode", 100, null, HAlign.Left, visPos++);
            }
        }

        private void ConfigureDetailsChildBand(UltraGridBand childBand)
        {
            childBand.HeaderVisible = true;
            childBand.Header.Caption = "Purchase Return Voucher Breakdown (matching FrmPurchaseReturn)";
            childBand.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
            childBand.Header.Appearance.BackColor2 = Color.FromArgb(31, 98, 145);
            childBand.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
            childBand.Header.Appearance.ForeColor = Color.White;
            childBand.Header.Appearance.BorderColor = BorderBlue;
            childBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
            childBand.Override.AllowAddNew = AllowAddNew.No;
            childBand.Override.AllowDelete = DefaultableBoolean.False;
            childBand.Override.AllowUpdate = DefaultableBoolean.False;
            childBand.Override.RowSelectors = DefaultableBoolean.False;
            childBand.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
            childBand.Override.HeaderAppearance.ForeColor = Color.White;
            childBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            childBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);

            foreach (UltraGridColumn column in childBand.Columns)
            {
                column.Hidden = true;
                column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            int childVisPos = 0;
            ConfigureChildGridColumn(childBand, "ReturnDate", "Return Date", 90, "dd-MMM-yyyy", HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "ReturnNo", "Debit Note #", 95, null, HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "InvoiceNo", "Ref Bill #", 95, null, HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "InvoiceDate", "Bill Date", 90, "dd-MMM-yyyy", HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "VendorName", "Vendor Name", 170, null, HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "Qty", "Inv Qty", 75, "#,##0.00", HAlign.Right, childVisPos++);
            ConfigureChildGridColumn(childBand, "Returned", "Return Qty", 80, "#,##0.00", HAlign.Right, childVisPos++);
            ConfigureChildGridColumn(childBand, "Cost", "Rate", 80, "#,##0.00", HAlign.Right, childVisPos++);
            ConfigureChildGridColumn(childBand, "NetAmount", "Net Return", 95, "#,##0.00", HAlign.Right, childVisPos++);
            ConfigureChildGridColumn(childBand, "Reason", "Reason", 130, null, HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "Paymode", "Paymode", 80, null, HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "BranchName", "Branch", 90, null, HAlign.Left, childVisPos++);
            ConfigureChildGridColumn(childBand, "BilledBy", "User", 80, null, HAlign.Left, childVisPos++);

            if (childBand.Columns.Exists("NetAmount"))
            {
                childBand.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(180, 40, 40);
                childBand.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void ConfigureDetailedBand(UltraGridBand band)
        {
            if (band.Columns.Exists("VendorLedgerId"))
                band.Columns["VendorLedgerId"].Hidden = true;
            if (band.Columns.Exists("BranchId"))
                band.Columns["BranchId"].Hidden = true;
            if (band.Columns.Exists("SlNo"))
                band.Columns["SlNo"].Hidden = true;
            if (band.Columns.Exists("ItemID"))
                band.Columns["ItemID"].Hidden = true;

            int visPos = 0;
            ConfigureGridColumn(band, "ReturnDate", "Return Date", 90, "dd-MMM-yyyy", HAlign.Left, visPos++);
            ConfigureGridColumn(band, "ReturnNo", "Debit Note #", 95, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "InvoiceNo", "Ref Bill #", 95, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "InvoiceDate", "Bill Date", 90, "dd-MMM-yyyy", HAlign.Left, visPos++);
            ConfigureGridColumn(band, "VendorName", "Vendor Name", 170, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "ItemName", "Item Description", 210, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "BarCode", "Barcode", 95, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
            ConfigureGridColumn(band, "Qty", "Inv Qty", 65, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Returned", "Return Qty", 75, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Free", "Free", 55, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TotalQty", "Total Return", 80, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Cost", "Rate", 75, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Amount", "Amount", 85, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "DisAmt", "Discount", 65, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TaxAmt", "Tax", 65, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "NetAmount", "Net Return", 95, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Reason", "Return Reason", 130, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "Paymode", "Paymode", 80, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "BranchName", "Branch", 90, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "BilledBy", "User", 80, null, HAlign.Left, visPos++);

            if (band.Columns.Exists("NetAmount"))
            {
                band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(180, 40, 40);
                band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void ConfigureDateItemSummaryBand(UltraGridBand band)
        {
            if (band.Columns.Exists("ItemID"))
                band.Columns["ItemID"].Hidden = true;

            int visPos = 0;
            ConfigureGridColumn(band, "ReturnDate", "Return Date", 95, "dd-MMM-yyyy", HAlign.Left, visPos++);
            ConfigureGridColumn(band, "ItemName", "Item Description", 220, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "BarCode", "Barcode", 95, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
            ConfigureGridColumn(band, "ReturnCount", "Vouchers", 70, "#,##0", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TotalQty", "Total Qty", 85, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Returned", "Return Qty", 85, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Amount", "Amount", 95, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "DisAmt", "Discount", 75, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TaxAmt", "Tax", 75, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "NetAmount", "Net Return", 100, "#,##0.00", HAlign.Right, visPos++);

            if (band.Columns.Exists("NetAmount"))
            {
                band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(180, 40, 40);
                band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void ConfigureItemSummaryBand(UltraGridBand band)
        {
            if (band.Columns.Exists("ItemID"))
                band.Columns["ItemID"].Hidden = true;

            int visPos = 0;
            ConfigureGridColumn(band, "ItemName", "Item Description", 240, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "BarCode", "Barcode", 100, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
            ConfigureGridColumn(band, "ReturnCount", "Vouchers", 75, "#,##0", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TotalQty", "Total Qty", 90, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Returned", "Return Qty", 90, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Amount", "Amount", 100, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "DisAmt", "Discount", 80, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TaxAmt", "Tax", 80, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "NetAmount", "Net Return", 110, "#,##0.00", HAlign.Right, visPos++);

            if (band.Columns.Exists("NetAmount"))
            {
                band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(180, 40, 40);
                band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void ConfigureVendorSummaryBand(UltraGridBand band)
        {
            if (band.Columns.Exists("VendorLedgerId"))
                band.Columns["VendorLedgerId"].Hidden = true;

            int visPos = 0;
            ConfigureGridColumn(band, "VendorName", "Vendor Name", 260, null, HAlign.Left, visPos++);
            ConfigureGridColumn(band, "ReturnCount", "Vouchers", 75, "#,##0", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TotalQty", "Total Qty", 90, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Returned", "Return Qty", 90, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "Amount", "Amount", 100, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "DisAmt", "Discount", 80, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "TaxAmt", "Tax", 80, "#,##0.00", HAlign.Right, visPos++);
            ConfigureGridColumn(band, "NetAmount", "Net Return", 110, "#,##0.00", HAlign.Right, visPos++);

            if (band.Columns.Exists("NetAmount"))
            {
                band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(180, 40, 40);
                band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
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

        private void ConfigureChildGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
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
            if (e.Row.Band.Index == 0)
            {
                string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    // Highlight active day return cells
                    foreach (var dateCol in _currentMatrixDateColumns)
                    {
                        if (e.Row.Cells.Exists(dateCol.Key))
                        {
                            decimal val = 0;
                            if (e.Row.Cells[dateCol.Key].Value != null && e.Row.Cells[dateCol.Key].Value != DBNull.Value)
                            {
                                decimal.TryParse(e.Row.Cells[dateCol.Key].Value.ToString(), out val);
                            }
                            if (val > 0)
                            {
                                e.Row.Cells[dateCol.Key].Appearance.FontData.Bold = DefaultableBoolean.True;
                                e.Row.Cells[dateCol.Key].Appearance.ForeColor = Color.FromArgb(180, 40, 40);
                            }
                            else
                            {
                                e.Row.Cells[dateCol.Key].Appearance.ForeColor = Color.FromArgb(170, 185, 205);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Report Data Loading & Dynamic Matrix Building
        private void LoadReport()
        {
            Cursor prev = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";

                DatewisePurchaseReturnReportFilter filter = new DatewisePurchaseReturnReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    VendorLedgerId = ultraComboVendor.Value != null ? Convert.ToInt32(ultraComboVendor.Value) : 0,
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DETAILED"
                };

                _reportRows = _repository.GetReport(filter);

                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    List<PivotDateColumnInfo> matrixCols;
                    DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                    _matrixDataSet = ds;
                    _matrixTable = ds.Tables["PurchaseReturnMatrix"];
                    _currentMatrixDateColumns.Clear();
                    _currentMatrixDateColumns.AddRange(matrixCols);

                    gridReport.DataSource = null;
                    gridReport.DataSource = ds;
                }
                else if (string.Equals(viewMode, "DETAILED", StringComparison.OrdinalIgnoreCase))
                {
                    _matrixDataSet = null;
                    _matrixTable = null;
                    _currentMatrixDateColumns.Clear();

                    gridReport.DataSource = null;
                    gridReport.DataSource = _reportRows;
                }
                else if (string.Equals(viewMode, "DATE_ITEM_SUMMARY", StringComparison.OrdinalIgnoreCase))
                {
                    _matrixDataSet = null;
                    _matrixTable = null;
                    _currentMatrixDateColumns.Clear();

                    DataTable summaryTable = BuildDateItemSummaryDataTable(_reportRows);
                    gridReport.DataSource = null;
                    gridReport.DataSource = summaryTable;
                }
                else if (string.Equals(viewMode, "ITEM_SUMMARY", StringComparison.OrdinalIgnoreCase))
                {
                    _matrixDataSet = null;
                    _matrixTable = null;
                    _currentMatrixDateColumns.Clear();

                    DataTable summaryTable = BuildItemSummaryDataTable(_reportRows);
                    gridReport.DataSource = null;
                    gridReport.DataSource = summaryTable;
                }
                else if (string.Equals(viewMode, "VENDOR_SUMMARY", StringComparison.OrdinalIgnoreCase))
                {
                    _matrixDataSet = null;
                    _matrixTable = null;
                    _currentMatrixDateColumns.Clear();

                    DataTable summaryTable = BuildVendorSummaryDataTable(_reportRows);
                    gridReport.DataSource = null;
                    gridReport.DataSource = summaryTable;
                }

                btnToggleValueMode.Visible = string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);
                btnExpandAll.Visible = string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);
                btnCollapseAll.Visible = string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load Date-wise Purchase Return Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = prev;
            }
        }

        private DataSet BuildMatrixDataSet(List<DatewisePurchaseReturnReportRow> rawRows, DatewisePurchaseReturnReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable detailsTable = BuildDetailsDataTable(rawRows);

            DataSet ds = new DataSet("DatewisePurchaseReturnDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(detailsTable);

            DataRelation relation = new DataRelation("ItemReturnBreakdown", matrixTable.Columns["ItemKey"], detailsTable.Columns["ItemKey"], false);
            ds.Relations.Add(relation);

            return ds;
        }

        private DataTable BuildMatrixDataTable(List<DatewisePurchaseReturnReportRow> rawRows, DatewisePurchaseReturnReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            dateColumns = new List<PivotDateColumnInfo>();
            DataTable table = new DataTable("PurchaseReturnMatrix");

            table.Columns.Add("ItemKey", typeof(string));
            table.Columns.Add("ItemID", typeof(int));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("Unit", typeof(string));

            DateTime startDate = filter.FromDate.Date;
            DateTime endDate = filter.ToDate.Date;

            string periodMode = (filter.PeriodMode ?? "DATE_RANGE").ToUpperInvariant();
            if (periodMode == "MONTH_WISE" && filter.Year > 0 && filter.Month > 0)
            {
                startDate = new DateTime(filter.Year, filter.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (periodMode == "ALL" || startDate < new DateTime(2000, 1, 1))
            {
                if (rawRows != null && rawRows.Count > 0)
                {
                    startDate = rawRows.Min(r => r.ReturnDate).Date;
                    endDate = rawRows.Max(r => r.ReturnDate).Date;
                }
                else
                {
                    startDate = DateTime.Today.AddDays(-30);
                    endDate = DateTime.Today;
                }
            }

            // Cap matrix to at most 60 daily columns
            if ((endDate - startDate).TotalDays > 60)
            {
                if (rawRows != null && rawRows.Count > 0)
                {
                    var distinctDates = rawRows.Select(r => r.ReturnDate.Date).Distinct().OrderBy(d => d).ToList();
                    foreach (var d in distinctDates)
                    {
                        string colKey = "Date_" + d.ToString("yyyyMMdd");
                        string colCaption = d.ToString("dd-MMM");
                        dateColumns.Add(new PivotDateColumnInfo { Key = colKey, Caption = colCaption, Date = d });
                        table.Columns.Add(colKey, typeof(decimal));
                    }
                }
            }
            else
            {
                for (DateTime d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    string colKey = periodMode == "MONTH_WISE" ? "Day_" + d.Day : "Date_" + d.ToString("yyyyMMdd");
                    string colCaption = periodMode == "MONTH_WISE" ? d.Day.ToString() : d.ToString("dd-MMM");

                    dateColumns.Add(new PivotDateColumnInfo { Key = colKey, Caption = colCaption, Date = d });
                    table.Columns.Add(colKey, typeof(decimal));
                }
            }

            table.Columns.Add("ReturnCount", typeof(int));
            table.Columns.Add("TotalQty", typeof(decimal));
            table.Columns.Add("TotalAmount", typeof(decimal));

            if (rawRows == null || rawRows.Count == 0)
                return table;

            var itemsDict = new Dictionary<string, ItemMatrixInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in rawRows)
            {
                string key = $"{r.ItemID}_{r.ItemName}";
                if (!itemsDict.TryGetValue(key, out ItemMatrixInfo item))
                {
                    item = new ItemMatrixInfo
                    {
                        ItemID = r.ItemID,
                        ItemName = r.ItemName,
                        Unit = r.Unit,
                        BarCode = r.BarCode
                    };
                    itemsDict[key] = item;
                }

                decimal returnedQty = r.Returned > 0 ? r.Returned : (r.TotalQty > 0 ? r.TotalQty : r.Qty);
                decimal actualQty = (r.Qty + r.Free) > 0 ? (r.Qty + r.Free) : (r.TotalQty > 0 ? r.TotalQty : returnedQty);
                item.TotalQty += actualQty;
                item.TotalAmount += r.NetAmount;
                item.ReturnCount++;

                string dayKey = periodMode == "MONTH_WISE"
                    ? "Day_" + r.ReturnDate.Day
                    : "Date_" + r.ReturnDate.ToString("yyyyMMdd");

                decimal cellVal = _isAmountMode ? r.NetAmount : returnedQty;
                if (!item.DayValues.ContainsKey(dayKey))
                    item.DayValues[dayKey] = 0;
                item.DayValues[dayKey] += cellVal;
            }

            foreach (var kvp in itemsDict.OrderByDescending(x => x.Value.TotalAmount))
            {
                DataRow dr = table.NewRow();
                var item = kvp.Value;

                dr["ItemKey"] = kvp.Key;
                dr["ItemID"] = item.ItemID;
                dr["ItemName"] = item.ItemName;
                dr["BarCode"] = item.BarCode ?? string.Empty;
                dr["Unit"] = item.Unit ?? string.Empty;

                foreach (var dateCol in dateColumns)
                {
                    if (item.DayValues.TryGetValue(dateCol.Key, out decimal dayVal))
                    {
                        dr[dateCol.Key] = dayVal;
                    }
                    else
                    {
                        dr[dateCol.Key] = 0m;
                    }
                }

                dr["ReturnCount"] = item.ReturnCount;
                dr["TotalQty"] = item.TotalQty;
                dr["TotalAmount"] = item.TotalAmount;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildDetailsDataTable(List<DatewisePurchaseReturnReportRow> rawRows)
        {
            DataTable table = new DataTable("PurchaseReturnDetails");

            table.Columns.Add("ItemKey", typeof(string));
            table.Columns.Add("ItemID", typeof(int));
            table.Columns.Add("ReturnDate", typeof(DateTime));
            table.Columns.Add("ReturnNo", typeof(long));
            table.Columns.Add("InvoiceNo", typeof(string));
            table.Columns.Add("InvoiceDate", typeof(DateTime));
            table.Columns.Add("VendorName", typeof(string));
            table.Columns.Add("Qty", typeof(decimal));
            table.Columns.Add("Returned", typeof(decimal));
            table.Columns.Add("Cost", typeof(decimal));
            table.Columns.Add("DisAmt", typeof(decimal));
            table.Columns.Add("TaxAmt", typeof(decimal));
            table.Columns.Add("NetAmount", typeof(decimal));
            table.Columns.Add("Reason", typeof(string));
            table.Columns.Add("Paymode", typeof(string));
            table.Columns.Add("BranchName", typeof(string));
            table.Columns.Add("BilledBy", typeof(string));

            if (rawRows == null || rawRows.Count == 0)
                return table;

            foreach (var r in rawRows)
            {
                DataRow dr = table.NewRow();
                dr["ItemKey"] = $"{r.ItemID}_{r.ItemName}";
                dr["ItemID"] = r.ItemID;
                dr["ReturnDate"] = r.ReturnDate;
                dr["ReturnNo"] = r.ReturnNo;
                dr["InvoiceNo"] = r.InvoiceNo;
                dr["InvoiceDate"] = r.InvoiceDate;
                dr["VendorName"] = r.VendorName;
                dr["Qty"] = r.Qty;
                dr["Returned"] = r.Returned > 0 ? r.Returned : r.TotalQty;
                dr["Cost"] = r.Cost;
                dr["DisAmt"] = r.DisAmt;
                dr["TaxAmt"] = r.TaxAmt;
                dr["NetAmount"] = r.NetAmount;
                dr["Reason"] = r.Reason;
                dr["Paymode"] = r.Paymode;
                dr["BranchName"] = r.BranchName;
                dr["BilledBy"] = r.BilledBy;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildDateItemSummaryDataTable(List<DatewisePurchaseReturnReportRow> rawRows)
        {
            DataTable table = new DataTable("DateItemSummary");
            table.Columns.Add("ReturnDate", typeof(DateTime));
            table.Columns.Add("ItemID", typeof(int));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("ReturnCount", typeof(int));
            table.Columns.Add("TotalQty", typeof(decimal));
            table.Columns.Add("Returned", typeof(decimal));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("DisAmt", typeof(decimal));
            table.Columns.Add("TaxAmt", typeof(decimal));
            table.Columns.Add("NetAmount", typeof(decimal));

            if (rawRows == null || rawRows.Count == 0) return table;

            var grouped = rawRows.GroupBy(r => new { r.ReturnDate.Date, r.ItemID, r.ItemName, r.BarCode, r.Unit })
                                 .OrderByDescending(g => g.Key.Date)
                                 .ThenByDescending(g => g.Sum(x => x.NetAmount));

            foreach (var g in grouped)
            {
                DataRow dr = table.NewRow();
                dr["ReturnDate"] = g.Key.Date;
                dr["ItemID"] = g.Key.ItemID;
                dr["BarCode"] = g.Key.BarCode ?? string.Empty;
                dr["ItemName"] = g.Key.ItemName;
                dr["Unit"] = g.Key.Unit ?? string.Empty;
                dr["ReturnCount"] = g.Select(x => x.ReturnNo).Distinct().Count();
                dr["TotalQty"] = g.Sum(x => (x.Qty + x.Free > 0 ? (x.Qty + x.Free) : x.TotalQty));
                dr["Returned"] = g.Sum(x => (x.Returned > 0 ? x.Returned : x.TotalQty));
                dr["Amount"] = g.Sum(x => x.Amount);
                dr["DisAmt"] = g.Sum(x => x.DisAmt);
                dr["TaxAmt"] = g.Sum(x => x.TaxAmt);
                dr["NetAmount"] = g.Sum(x => x.NetAmount);
                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildItemSummaryDataTable(List<DatewisePurchaseReturnReportRow> rawRows)
        {
            DataTable table = new DataTable("ItemSummary");
            table.Columns.Add("ItemID", typeof(int));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("ReturnCount", typeof(int));
            table.Columns.Add("TotalQty", typeof(decimal));
            table.Columns.Add("Returned", typeof(decimal));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("DisAmt", typeof(decimal));
            table.Columns.Add("TaxAmt", typeof(decimal));
            table.Columns.Add("NetAmount", typeof(decimal));

            if (rawRows == null || rawRows.Count == 0) return table;

            var grouped = rawRows.GroupBy(r => new { r.ItemID, r.ItemName, r.BarCode, r.Unit })
                                 .OrderByDescending(g => g.Sum(x => x.NetAmount));

            foreach (var g in grouped)
            {
                DataRow dr = table.NewRow();
                dr["ItemID"] = g.Key.ItemID;
                dr["BarCode"] = g.Key.BarCode ?? string.Empty;
                dr["ItemName"] = g.Key.ItemName;
                dr["Unit"] = g.Key.Unit ?? string.Empty;
                dr["ReturnCount"] = g.Select(x => x.ReturnNo).Distinct().Count();
                dr["TotalQty"] = g.Sum(x => (x.Qty + x.Free > 0 ? (x.Qty + x.Free) : x.TotalQty));
                dr["Returned"] = g.Sum(x => (x.Returned > 0 ? x.Returned : x.TotalQty));
                dr["Amount"] = g.Sum(x => x.Amount);
                dr["DisAmt"] = g.Sum(x => x.DisAmt);
                dr["TaxAmt"] = g.Sum(x => x.TaxAmt);
                dr["NetAmount"] = g.Sum(x => x.NetAmount);
                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildVendorSummaryDataTable(List<DatewisePurchaseReturnReportRow> rawRows)
        {
            DataTable table = new DataTable("VendorSummary");
            table.Columns.Add("VendorLedgerId", typeof(int));
            table.Columns.Add("VendorName", typeof(string));
            table.Columns.Add("ReturnCount", typeof(int));
            table.Columns.Add("TotalQty", typeof(decimal));
            table.Columns.Add("Returned", typeof(decimal));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("DisAmt", typeof(decimal));
            table.Columns.Add("TaxAmt", typeof(decimal));
            table.Columns.Add("NetAmount", typeof(decimal));

            if (rawRows == null || rawRows.Count == 0) return table;

            var grouped = rawRows.GroupBy(r => new { r.VendorLedgerId, r.VendorName })
                                 .OrderByDescending(g => g.Sum(x => x.NetAmount));

            foreach (var g in grouped)
            {
                DataRow dr = table.NewRow();
                dr["VendorLedgerId"] = g.Key.VendorLedgerId;
                dr["VendorName"] = g.Key.VendorName;
                dr["ReturnCount"] = g.Select(x => x.ReturnNo).Distinct().Count();
                dr["TotalQty"] = g.Sum(x => (x.Qty + x.Free > 0 ? (x.Qty + x.Free) : x.TotalQty));
                dr["Returned"] = g.Sum(x => (x.Returned > 0 ? x.Returned : x.TotalQty));
                dr["Amount"] = g.Sum(x => x.Amount);
                dr["DisAmt"] = g.Sum(x => x.DisAmt);
                dr["TaxAmt"] = g.Sum(x => x.TaxAmt);
                dr["NetAmount"] = g.Sum(x => x.NetAmount);
                table.Rows.Add(dr);
            }

            return table;
        }
        #endregion

        #region Value Mode Toggle & Tree Controls
        private void ToggleValueMode()
        {
            _isAmountMode = !_isAmountMode;
            UpdateValueModeButtonText();
            LoadReport();
        }

        private void UpdateValueModeButtonText()
        {
            btnToggleValueMode.Text = _isAmountMode ? "Show Quantity" : "Show Amount";
        }

        private void ExpandAllGroups()
        {
            if (gridReport?.Rows == null || gridReport.Rows.Count == 0) return;
            gridReport.BeginUpdate();
            try
            {
                foreach (UltraGridRow row in gridReport.Rows)
                {
                    row.Expanded = true;
                }
            }
            finally
            {
                gridReport.EndUpdate();
            }
        }

        private void CollapseAllGroups()
        {
            if (gridReport?.Rows == null || gridReport.Rows.Count == 0) return;
            gridReport.BeginUpdate();
            try
            {
                foreach (UltraGridRow row in gridReport.Rows)
                {
                    row.Expanded = false;
                }
            }
            finally
            {
                gridReport.EndUpdate();
            }
        }

        private void ToggleRowSelection()
        {
            if (gridReport.ActiveRow != null)
            {
                gridReport.ActiveRow.Selected = !gridReport.ActiveRow.Selected;
            }
        }

        private void ClearAllFilters()
        {
            _isLoading = true;
            try
            {
                DateTime today = DateTime.Today;
                ultraComboPeriodMode.Value = "DATE_RANGE";
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                ultraComboMonth.Value = today.Month;
                ultraComboYear.Value = today.Year;
                ultraComboViewMode.Value = "DATE_MATRIX";
                ultraComboVendor.Value = 0;
                txtItemSearch.Text = string.Empty;
                _isAmountMode = false;
                UpdateValueModeButtonText();
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }
            LoadReport();
        }

        private void OpenPurchaseReturnVoucher(UltraGridRow row)
        {
            try
            {
                long returnNo = 0;
                if (row.Cells.Exists("ReturnNo") && row.Cells["ReturnNo"].Value != null && row.Cells["ReturnNo"].Value != DBNull.Value)
                {
                    long.TryParse(row.Cells["ReturnNo"].Value.ToString(), out returnNo);
                }

                if (returnNo > 0)
                {
                    var homeInstance = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (homeInstance != null)
                    {
                        Transaction.frmPurchaseReturn form = new Transaction.frmPurchaseReturn();
                        homeInstance.OpenFormInTabSafe(form, $"Purchase Return #{returnNo}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open purchase return: {ex.Message}");
            }
        }
        #endregion

        #region F11 Item & Vendor Dialogs
        private void OpenItemPickerDialog()
        {
            try
            {
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmdatewisepurchasereturnreport"))
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open item dialog: {ex.Message}");
            }
        }

        private void OpenVendorPickerDialog()
        {
            try
            {
                using (frmVendorDig vendorDialog = new frmVendorDig())
                {
                    if (vendorDialog.ShowDialog(this) == DialogResult.OK &&
                        !string.IsNullOrWhiteSpace(vendorDialog.SelectedVendorName))
                    {
                        foreach (ValueListItem item in ultraComboVendor.Items)
                        {
                            if (string.Equals(Convert.ToString(item.DisplayText), vendorDialog.SelectedVendorName, StringComparison.OrdinalIgnoreCase))
                            {
                                ultraComboVendor.Value = item.DataValue;
                                break;
                            }
                        }
                        LoadReport();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open vendor dialog: {ex.Message}");
            }
        }
        #endregion

        #region Export & Print
        private void ExportGridToCsv()
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                    sfd.FileName = $"DatewisePurchaseReturnReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        var sb = new StringBuilder();

                        if (gridReport.DisplayLayout.Bands.Count > 0)
                        {
                            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                            var visibleCols = band.Columns.Cast<UltraGridColumn>()
                                .Where(c => !c.Hidden && !c.IsChaptered)
                                .OrderBy(c => c.Header.VisiblePosition)
                                .ToList();

                            // Headers
                            sb.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption ?? c.Key}\"")));

                            // Rows
                            foreach (UltraGridRow row in gridReport.Rows)
                            {
                                if (row.IsFilteredOut) continue;
                                var values = visibleCols.Select(c =>
                                {
                                    object val = row.Cells[c.Key].Value;
                                    string str = val != null ? val.ToString().Replace("\"", "\"\"") : string.Empty;
                                    return $"\"{str}\"";
                                });
                                sb.AppendLine(string.Join(",", values));
                            }
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Grid successfully exported to CSV!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintGrid(bool showPreview)
        {
            try
            {
                using (Infragistics.Win.UltraWinGrid.UltraGridPrintDocument doc = new Infragistics.Win.UltraWinGrid.UltraGridPrintDocument())
                {
                    doc.Grid = gridReport;
                    doc.Header.TextCenter = "Date-wise Purchase Return Report";
                    doc.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                    doc.Header.Appearance.FontData.SizeInPoints = 12;
                    doc.Footer.TextRight = "Page [Page #] of [Total Pages]";
                    doc.DefaultPageSettings.Landscape = true;

                    if (showPreview)
                    {
                        using (PrintPreviewDialog ppd = new PrintPreviewDialog())
                        {
                            ppd.Document = doc;
                            ppd.WindowState = FormWindowState.Maximized;
                            ppd.ShowDialog(this);
                        }
                    }
                    else
                    {
                        using (PrintDialog pd = new PrintDialog())
                        {
                            pd.Document = doc;
                            if (pd.ShowDialog(this) == DialogResult.OK)
                            {
                                doc.Print();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print failed: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Dynamic Interactive Footer (Full parity with frmVendorOutstandingReport)
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

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            UltraGridColumn firstVisibleCol = band.Columns.Cast<UltraGridColumn>()
                .Where(c => !c.Hidden && !c.IsChaptered && c.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
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
                    string unit = "Items";
                    string vMode = ultraComboViewMode.Value?.ToString() ?? "DATE_MATRIX";
                    if (vMode == "DATE_MATRIX" || vMode == "ITEM_SUMMARY") unit = "Items";
                    else if (vMode == "DETAILED") unit = "Vouchers";
                    else if (vMode == "DATE_ITEM_SUMMARY") unit = "Rows";
                    else if (vMode == "VENDOR_SUMMARY") unit = "Vendors";

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
                case "Average":
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

                    if (aggregation == "Avg" || aggregation == "Average") return "Avg: " + formatted;
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
            string clean = Convert.ToString(value).Replace("₹", "").Trim();
            return decimal.TryParse(clean, out result) ? result : (decimal?)null;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null) return false;
            if (column.Key.StartsWith("Day_") || column.Key.StartsWith("Date_")) return true;
            return summaryDefaultNumericColumns.Contains(column.Key) ||
                   column.DataType == typeof(decimal) ||
                   column.DataType == typeof(double) ||
                   column.DataType == typeof(float) ||
                   column.DataType == typeof(int) ||
                   column.DataType == typeof(long) ||
                   column.DataType == typeof(short);
        }
        #endregion

        #region Column Chooser & Drag-to-Hide
        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    UIElement element = gridReport.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                    HeaderUIElement headerElement = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                    if (headerElement != null && headerElement.Header != null)
                    {
                        columnBeingDragged = headerElement.Header.Column;
                        headerDragStartPoint = e.Location;
                        isDraggingHeaderToHide = false;
                    }
                }
            };

            gridReport.MouseMove += (s, e) =>
            {
                if (columnBeingDragged != null && e.Button == MouseButtons.Left)
                {
                    int dx = Math.Abs(e.Location.X - headerDragStartPoint.X);
                    int dy = Math.Abs(e.Location.Y - headerDragStartPoint.Y);

                    if (dx > 8 || dy > 8)
                    {
                        Rectangle gridRect = gridReport.ClientRectangle;
                        if (!gridRect.Contains(e.Location))
                        {
                            isDraggingHeaderToHide = true;
                            Cursor.Current = blackXCursor;
                        }
                        else
                        {
                            isDraggingHeaderToHide = false;
                            Cursor.Current = Cursors.Default;
                        }
                    }
                }
            };

            gridReport.MouseUp += (s, e) =>
            {
                if (isDraggingHeaderToHide && columnBeingDragged != null)
                {
                    HideColumn(columnBeingDragged.Key);
                }
                columnBeingDragged = null;
                isDraggingHeaderToHide = false;
                Cursor.Current = Cursors.Default;
            };
        }

        private void HideColumn(string colKey)
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            if (band.Columns.Exists(colKey))
            {
                band.Columns[colKey].Hidden = true;
                userHiddenColumnKeys.Add(colKey);
                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideColumn(string colKey, int position)
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            if (band.Columns.Exists(colKey))
            {
                band.Columns[colKey].Hidden = false;
                userHiddenColumnKeys.Remove(colKey);
                band.Columns[colKey].Header.VisiblePosition = position;
                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void ToggleColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            if (columnChooserForm.Visible)
            {
                columnChooserForm.Hide();
            }
            else
            {
                PopulateColumnChooserListBox();
                columnChooserForm.Location = new Point(
                    Location.X + Width - columnChooserForm.Width - 30,
                    Location.Y + 180
                );
                columnChooserForm.Show(this);
            }
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Column Chooser",
                Size = new Size(240, 360),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = false,
                ShowInTaskbar = false
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 8.25f)
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8.25f),
                IntegralHeight = false
            };
            columnChooserListBox.DoubleClick += (s, e) =>
            {
                if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
                {
                    UnhideColumn(item.ColumnKey, 0);
                }
            };

            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(2)
            };
            pnlTop.Controls.Add(txtColumnSearch);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(pnlTop);
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
                if (col.Hidden && !col.IsChaptered && !string.Equals(col.Key, "ItemReturnBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filter) || caption.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ApplyUserHiddenColumns(UltraGridBand band)
        {
            if (userHiddenColumnKeys != null && userHiddenColumnKeys.Count > 0)
            {
                foreach (string key in userHiddenColumnKeys)
                {
                    if (band.Columns.Exists(key))
                    {
                        band.Columns[key].Hidden = true;
                    }
                }
            }
        }

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                Bitmap bmp = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // Draw red or black X cursor
                    using (Pen p = new Pen(Color.FromArgb(200, 30, 30), 3))
                    {
                        g.DrawLine(p, 8, 8, 24, 24);
                        g.DrawLine(p, 24, 8, 8, 24);
                    }
                }
                return new Cursor(bmp.GetHicon());
            }
            catch
            {
                return Cursors.No;
            }
        }
        #endregion
    }
}
