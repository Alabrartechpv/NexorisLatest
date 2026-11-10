using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
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

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmdatewiseitemstockreport : Form
    {
        #region Colour Palette (Parity with frmVendorOutstandingReport)
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

        // Movement Color Accents
        private static readonly Color InGreenColor         = Color.FromArgb(27, 94, 32);
        private static readonly Color InBgColor            = Color.FromArgb(232, 245, 233);
        private static readonly Color OutRedColor          = Color.FromArgb(183, 28, 28);
        private static readonly Color OutBgColor           = Color.FromArgb(255, 235, 238);
        private static readonly Color NetBlueColor         = Color.FromArgb(13, 71, 161);
        private static readonly Color ZeroMutedColor       = Color.FromArgb(170, 170, 170);
        #endregion

        #region Enums & Inner Helper Types
        public enum ValueDisplayMode
        {
            NetMovement = 0,
            StockInOnly = 1,
            StockOutOnly = 2
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

        private sealed class ItemMovementSummary
        {
            public int ItemID { get; set; }
            public string BarCode { get; set; }
            public string ItemName { get; set; }
            public string Unit { get; set; }
            public decimal TotalIn { get; set; }
            public decimal TotalOut { get; set; }
            public decimal NetMovement => TotalIn - TotalOut;
            public decimal TotalInAmount { get; set; }
            public decimal TotalOutAmount { get; set; }
            public decimal CurrentStock { get; set; }
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
        private readonly DatewiseItemStockReportRepository _repository;
        private List<DatewiseItemStockReportRow> _reportRows;
        private bool _isLoading;

        private ValueDisplayMode _currentValueMode = ValueDisplayMode.NetMovement;
        private readonly List<PivotDateColumnInfo> _currentMatrixDateColumns = new List<PivotDateColumnInfo>();
        private DataSet _matrixDataSet;
        private DataTable _matrixTable;

        // Custom Dynamic Footer
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Total",
            "TotalIn",
            "TotalOut",
            "NetMovement",
            "CurrentStock",
            "InQty",
            "OutQty",
            "Qty",
            "Amount",
            "TotalInAmount",
            "TotalOutAmount"
        };

        // Header drag-to-hide & Column Chooser
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private ReportColumnFilterManager _filterManager;
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        #endregion

        #region Constructor & Form Lifecycle
        public frmdatewiseitemstockreport()
        {
            _repository = new DatewiseItemStockReportRepository();
            _reportRows = new List<DatewiseItemStockReportRow>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmdatewiseitemstockreport_Load;
            FormClosing += frmdatewiseitemstockreport_FormClosing;
            KeyDown += frmdatewiseitemstockreport_KeyDown;
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

            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboMonth.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboYear.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtFrom.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtTo.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboWay.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboOperation.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };

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

        private void frmdatewiseitemstockreport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                Text = "Date-wise Item Stock Movement Report";
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
                UpdateValueModeButtonText();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void frmdatewiseitemstockreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
            }
        }

        private void frmdatewiseitemstockreport_KeyDown(object sender, KeyEventArgs e)
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
            else if (e.KeyCode == Keys.F11 && txtItemSearch.Focused)
            {
                btnItemPicker.PerformClick();
                e.Handled = true;
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
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Item vs Dates Movement)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Transaction Movement Log)");
            ultraComboViewMode.Items.Add("DATE_ITEM_SUMMARY", "Daily Summary (Date & Item)");
            ultraComboViewMode.Items.Add("ITEM_SUMMARY", "Item Summary (Total Movement by Item)");
            ultraComboViewMode.Value = "DATE_MATRIX";

            ultraComboWay.Items.Clear();
            ultraComboWay.Items.Add("ALL", "ALL Movements");
            ultraComboWay.Items.Add("IN", "Stock Came (IN)");
            ultraComboWay.Items.Add("OUT", "Stock Gone (OUT)");
            ultraComboWay.Value = "ALL";

            ultraComboOperation.Items.Clear();
            ultraComboOperation.Items.Add("ALL", "ALL Operations");
            ultraComboOperation.Items.Add("Purchase", "Purchase (IN)");
            ultraComboOperation.Items.Add("Sales", "Sales (OUT)");
            ultraComboOperation.Items.Add("Sales Return", "Sales Return (IN)");
            ultraComboOperation.Items.Add("Purchase Return", "Purchase Return (OUT)");
            ultraComboOperation.Items.Add("Stock Adjustment In", "Stock Adjustment (IN)");
            ultraComboOperation.Items.Add("Stock Adjustment Out", "Stock Adjustment (OUT)");
            ultraComboOperation.Items.Add("Stock Transfer In", "Stock Transfer (IN)");
            ultraComboOperation.Items.Add("Stock Transfer Out", "Stock Transfer (OUT)");
            ultraComboOperation.Items.Add("Opening Stock", "Opening Stock (IN)");
            ultraComboOperation.Items.Add("Office Supplies", "Office Supplies (OUT)");
            ultraComboOperation.Value = "ALL";

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

        private void UpdateValueModeButtonText()
        {
            if (btnToggleValueMode == null) return;

            switch (_currentValueMode)
            {
                case ValueDisplayMode.NetMovement:
                    btnToggleValueMode.Text = "Show Came (IN)";
                    break;
                case ValueDisplayMode.StockInOnly:
                    btnToggleValueMode.Text = "Show Gone (OUT)";
                    break;
                case ValueDisplayMode.StockOutOnly:
                    btnToggleValueMode.Text = "Show Net Movement";
                    break;
            }
        }
        #endregion

        #region UI Styling (Exact Parity with frmVendorOutstandingReport)
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
            StyleFilterCombo(ultraComboViewMode, true);
            StyleFilterCombo(ultraComboMonth, true);
            StyleFilterCombo(ultraComboYear, true);
            StyleFilterCombo(ultraComboWay, true);
            StyleFilterCombo(ultraComboOperation, true);
            StyleFilterCombo(txtItemSearch, false);

            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);

            StyleLabel(lblPeriodMode);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblViewMode);
            StyleLabel(lblItem);
            StyleLabel(lblWay);
            StyleLabel(lblOperation);
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
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase))
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
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase))
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
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (_footerLabels.TryGetValue(column.Key, out Label cell))
                {
                    string aggType = _columnAggregations.ContainsKey(column.Key) ? _columnAggregations[column.Key] : "None";
                    if (string.Equals(aggType, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        cell.Text = string.Empty;
                        continue;
                    }

                    List<decimal> numericValues = new List<decimal>();
                    foreach (UltraGridRow row in visibleRows)
                    {
                        if (row.Cells.Exists(column.Key))
                        {
                            object val = row.Cells[column.Key].Value;
                            if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal decVal))
                            {
                                numericValues.Add(decVal);
                            }
                        }
                    }

                    string text = string.Empty;
                    switch (aggType.ToUpperInvariant())
                    {
                        case "SUM":
                            text = numericValues.Count > 0 ? numericValues.Sum().ToString("#,##0.00") : "0.00";
                            break;
                        case "COUNT":
                            text = numericValues.Count.ToString();
                            break;
                        case "AVERAGE":
                        case "AVG":
                            text = numericValues.Count > 0 ? numericValues.Average().ToString("#,##0.00") : "0.00";
                            break;
                        case "MIN":
                            text = numericValues.Count > 0 ? numericValues.Min().ToString("#,##0.00") : "0.00";
                            break;
                        case "MAX":
                            text = numericValues.Count > 0 ? numericValues.Max().ToString("#,##0.00") : "0.00";
                            break;
                    }

                    cell.Text = text;
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
            if (col == null || col.IsChaptered || string.Equals(col.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase)) return;

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
                if (col.IsChaptered || string.Equals(col.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase)) continue;

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

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = e.Bounds;
            using (SolidBrush bgBrush = new SolidBrush(FormBackColor))
            {
                g.FillRectangle(bgBrush, bounds);
            }

            Rectangle cardRect = new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4);
            Color cardBg = isSelected ? Color.FromArgb(200, 222, 245) : Color.FromArgb(242, 247, 253);
            Color cardBorder = isSelected ? GridHeaderBlueDark : ButtonLightOutline;

            using (GraphicsPath path = GetRoundedRectPath(cardRect, 4))
            {
                using (SolidBrush b = new SolidBrush(cardBg))
                {
                    g.FillPath(b, path);
                }
                using (Pen p = new Pen(cardBorder, 1))
                {
                    g.DrawPath(p, path);
                }
            }

            Rectangle textRect = new Rectangle(cardRect.X + 8, cardRect.Y, cardRect.Width - 16, cardRect.Height);
            TextRenderer.DrawText(g, item.DisplayText, columnChooserListBox.Font, textRect, ButtonTextBlue,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
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
                if (col.IsChaptered || string.Equals(col.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase)) continue;

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
        private DataTable BuildMatrixDataTable(List<DatewiseItemStockReportRow> rawRows, DatewiseItemStockReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
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
                        var distinctDates = (rawRows ?? new List<DatewiseItemStockReportRow>())
                            .Select(r => r.TransactionDate.Date)
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
                var distinctDates = (rawRows ?? new List<DatewiseItemStockReportRow>())
                    .Select(r => r.TransactionDate.Date)
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

            // Build DataTable Schema
            DataTable table = new DataTable("StockMovementMatrix");
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

            var cTotalIn = table.Columns.Add("TotalIn", typeof(decimal));
            cTotalIn.DefaultValue = 0m;

            var cTotalOut = table.Columns.Add("TotalOut", typeof(decimal));
            cTotalOut.DefaultValue = 0m;

            var cNet = table.Columns.Add("NetMovement", typeof(decimal));
            cNet.DefaultValue = 0m;

            var cCurrentStock = table.Columns.Add("CurrentStock", typeof(decimal));
            cCurrentStock.DefaultValue = 0m;

            var cTotalInAmt = table.Columns.Add("TotalInAmount", typeof(decimal));
            cTotalInAmt.DefaultValue = 0m;

            var cTotalOutAmt = table.Columns.Add("TotalOutAmount", typeof(decimal));
            cTotalOutAmt.DefaultValue = 0m;

            var cAvgCost = table.Columns.Add("AvgCost", typeof(decimal));
            cAvgCost.DefaultValue = 0m;

            var cAvgRate = table.Columns.Add("AvgRate", typeof(decimal));
            cAvgRate.DefaultValue = 0m;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Aggregation maps: cell values for In, Out, and Net Movement
            Dictionary<string, decimal> dateInMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> dateOutMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> dateNetMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ItemMovementSummary> itemSummaryMap = new Dictionary<string, ItemMovementSummary>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseItemStockReportRow r in rawRows)
            {
                string itemKey = (r.ItemID > 0 ? r.ItemID.ToString() : (r.ItemName ?? string.Empty)) + "_" + (r.Unit ?? string.Empty);

                ItemMovementSummary summary;
                if (!itemSummaryMap.TryGetValue(itemKey, out summary))
                {
                    summary = new ItemMovementSummary
                    {
                        ItemID = r.ItemID,
                        BarCode = r.BarCode ?? string.Empty,
                        ItemName = !string.IsNullOrEmpty(r.ItemName) ? r.ItemName : "Unknown Item",
                        Unit = r.Unit ?? string.Empty,
                        TotalIn = 0m,
                        TotalOut = 0m,
                        TotalInAmount = 0m,
                        TotalOutAmount = 0m,
                        CurrentStock = r.CurrentStock
                    };
                    itemSummaryMap[itemKey] = summary;
                }
                else
                {
                    if (string.IsNullOrEmpty(summary.BarCode) && !string.IsNullOrEmpty(r.BarCode))
                        summary.BarCode = r.BarCode;

                    if (r.CurrentStock != 0m)
                        summary.CurrentStock = r.CurrentStock;
                }

                summary.TotalIn += r.InQty;
                summary.TotalOut += r.OutQty;
                if (string.Equals(r.Way, "IN", StringComparison.OrdinalIgnoreCase))
                {
                    summary.TotalInAmount += r.Amount;
                }
                else
                {
                    summary.TotalOutAmount += r.Amount;
                }

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    bool matches = false;
                    if (dateCol.Date.HasValue)
                    {
                        matches = (r.TransactionDate.Date == dateCol.Date.Value.Date);
                    }
                    else if (dateCol.MonthNumber > 0)
                    {
                        matches = (r.TransactionDate.Month == dateCol.MonthNumber);
                    }
                    else if (dateCol.DayNumber > 0)
                    {
                        matches = (r.TransactionDate.Day == dateCol.DayNumber);
                    }

                    if (matches)
                    {
                        string cellKey = itemKey + "_" + dateCol.ColumnKey;

                        decimal curIn = 0m;
                        dateInMap.TryGetValue(cellKey, out curIn);
                        dateInMap[cellKey] = curIn + r.InQty;

                        decimal curOut = 0m;
                        dateOutMap.TryGetValue(cellKey, out curOut);
                        dateOutMap[cellKey] = curOut + r.OutQty;

                        decimal curNet = 0m;
                        dateNetMap.TryGetValue(cellKey, out curNet);
                        dateNetMap[cellKey] = curNet + (r.InQty - r.OutQty);
                    }
                }
            }

            // Populate rows
            foreach (ItemMovementSummary item in itemSummaryMap.Values.OrderBy(x => x.ItemName))
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

                    switch (_currentValueMode)
                    {
                        case ValueDisplayMode.StockInOnly:
                            dateInMap.TryGetValue(cellKey, out cellVal);
                            break;
                        case ValueDisplayMode.StockOutOnly:
                            dateOutMap.TryGetValue(cellKey, out cellVal);
                            break;
                        case ValueDisplayMode.NetMovement:
                        default:
                            dateNetMap.TryGetValue(cellKey, out cellVal);
                            break;
                    }

                    dr[dateCol.ColumnKey] = cellVal;
                }

                switch (_currentValueMode)
                {
                    case ValueDisplayMode.StockInOnly:
                        dr["Total"] = item.TotalIn;
                        break;
                    case ValueDisplayMode.StockOutOnly:
                        dr["Total"] = item.TotalOut;
                        break;
                    case ValueDisplayMode.NetMovement:
                    default:
                        dr["Total"] = item.NetMovement;
                        break;
                }

                dr["TotalIn"] = item.TotalIn;
                dr["TotalOut"] = item.TotalOut;
                dr["NetMovement"] = item.NetMovement;
                dr["CurrentStock"] = item.CurrentStock;
                dr["TotalInAmount"] = item.TotalInAmount;
                dr["TotalOutAmount"] = item.TotalOutAmount;
                dr["AvgCost"] = item.TotalIn > 0 ? Math.Round(item.TotalInAmount / item.TotalIn, 2) : 0m;
                dr["AvgRate"] = item.TotalOut > 0 ? Math.Round(item.TotalOutAmount / item.TotalOut, 2) : 0m;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildDetailsDataTable(List<DatewiseItemStockReportRow> rawRows)
        {
            DataTable details = new DataTable("StockMovements");
            details.Columns.Add("ItemKey", typeof(string));
            details.Columns.Add("TransactionDate", typeof(DateTime));
            details.Columns.Add("Operation", typeof(string));
            details.Columns.Add("Way", typeof(string));
            details.Columns.Add("RefNo", typeof(string));
            details.Columns.Add("SlNo", typeof(int));
            details.Columns.Add("Qty", typeof(decimal));
            details.Columns.Add("Unit", typeof(string));
            details.Columns.Add("Rate", typeof(decimal));
            details.Columns.Add("Cost", typeof(decimal));
            details.Columns.Add("Amount", typeof(decimal));
            details.Columns.Add("Account", typeof(string));
            details.Columns.Add("Remarks", typeof(string));
            details.Columns.Add("BranchName", typeof(string));
            details.Columns.Add("CurrentStock", typeof(decimal));

            if (rawRows != null && rawRows.Count > 0)
            {
                foreach (DatewiseItemStockReportRow r in rawRows.OrderBy(x => x.TransactionDate).ThenBy(x => x.SlNo))
                {
                    string itemKey = (r.ItemID > 0 ? r.ItemID.ToString() : (r.ItemName ?? string.Empty)) + "_" + (r.Unit ?? string.Empty);
                    DataRow dr = details.NewRow();
                    dr["ItemKey"] = itemKey;
                    dr["TransactionDate"] = r.TransactionDate.Date;
                    dr["Operation"] = r.Operation ?? string.Empty;
                    dr["Way"] = r.Way ?? string.Empty;
                    dr["RefNo"] = !string.IsNullOrEmpty(r.RefNo) ? r.RefNo : "-";
                    dr["SlNo"] = r.SlNo;
                    dr["Qty"] = r.Qty;
                    dr["Unit"] = r.Unit ?? string.Empty;
                    dr["Rate"] = r.Rate;
                    dr["Cost"] = r.Cost;
                    dr["Amount"] = r.Amount;
                    dr["Account"] = !string.IsNullOrEmpty(r.Account) ? r.Account : "-";
                    dr["Remarks"] = !string.IsNullOrEmpty(r.Remarks) ? r.Remarks : "-";
                    dr["BranchName"] = r.BranchName ?? string.Empty;
                    dr["CurrentStock"] = r.CurrentStock;
                    details.Rows.Add(dr);
                }
            }

            return details;
        }

        private DataSet BuildMatrixDataSet(List<DatewiseItemStockReportRow> rawRows, DatewiseItemStockReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable detailsTable = BuildDetailsDataTable(rawRows);

            DataSet ds = new DataSet("DatewiseItemStockDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(detailsTable);

            DataRelation relation = new DataRelation("ItemStockMovementBreakdown", matrixTable.Columns["ItemKey"], detailsTable.Columns["ItemKey"], false);
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
                string wayFilter = Convert.ToString(ultraComboWay.Value) ?? "ALL";
                string opFilter = Convert.ToString(ultraComboOperation.Value) ?? "ALL";

                DatewiseItemStockReportFilter filter = new DatewiseItemStockReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    WayFilter = wayFilter,
                    OperationFilter = opFilter,
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DETAILED" // Fetch transaction details so drilldown displays real movement details
                };

                _reportRows = _repository.GetReport(filter);

                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    List<PivotDateColumnInfo> matrixCols;
                    DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                    _matrixDataSet = ds;
                    _matrixTable = ds.Tables["StockMovementMatrix"];
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
                MessageBox.Show($"Error loading Date-wise Item Stock Movement Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = prev;
            }
        }
        #endregion

        #region UltraGrid Layout & Row Formatting
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
                    if (column.IsChaptered || string.Equals(column.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase))
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
                        string modeDesc = _currentValueMode == ValueDisplayMode.StockInOnly
                            ? "Stock Came (IN)"
                            : (_currentValueMode == ValueDisplayMode.StockOutOnly ? "Stock Gone (OUT)" : "Net Movement");
                        band.Columns[dateCol.ColumnKey].Header.ToolTipText = $"{dateCol.Tooltip} ({modeDesc})";
                    }
                }

                ConfigureGridColumn(band, "Total", "Total", 95, "#,##0.00", HAlign.Right, visPos++);
                if (band.Columns.Exists("Total"))
                {
                    band.Columns["Total"].Hidden = false;
                    band.Columns["Total"].Header.Caption = "Total";
                    band.Columns["Total"].Header.ToolTipText = _currentValueMode == ValueDisplayMode.StockInOnly
                        ? "Total Stock Came across dates"
                        : (_currentValueMode == ValueDisplayMode.StockOutOnly ? "Total Stock Gone across dates" : "Total Net Movement across dates");
                    band.Columns["Total"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                ConfigureGridColumn(band, "TotalIn", "Total In (Came)", 95, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalOut", "Total Out (Gone)", 95, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "NetMovement", "Net Movement", 100, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "CurrentStock", "Current Stock", 95, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
                ConfigureGridColumn(band, "BarCode", "Barcode", 110, null, HAlign.Left, visPos++);

                ConfigureGridColumn(band, "TotalInAmount", "Total In Amount", 105, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalOutAmount", "Total Out Amount", 105, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "AvgCost", "Avg Cost", 85, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "AvgRate", "Avg Rate", 85, "#,##0.00", HAlign.Right, visPos++);

                // By default hide chooser-only columns unless user explicitly unhid them
                if (band.Columns.Exists("TotalInAmount") && !userHiddenColumnKeys.Contains("TotalInAmount"))
                {
                    band.Columns["TotalInAmount"].Hidden = true;
                }
                if (band.Columns.Exists("TotalOutAmount") && !userHiddenColumnKeys.Contains("TotalOutAmount"))
                {
                    band.Columns["TotalOutAmount"].Hidden = true;
                }
                if (band.Columns.Exists("AvgCost") && !userHiddenColumnKeys.Contains("AvgCost"))
                {
                    band.Columns["AvgCost"].Hidden = true;
                }
                if (band.Columns.Exists("AvgRate") && !userHiddenColumnKeys.Contains("AvgRate"))
                {
                    band.Columns["AvgRate"].Hidden = true;
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
                if (band.Columns.Exists("TotalIn"))
                {
                    band.Columns["TotalIn"].CellAppearance.ForeColor = InGreenColor;
                    band.Columns["TotalIn"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("TotalOut"))
                {
                    band.Columns["TotalOut"].CellAppearance.ForeColor = OutRedColor;
                    band.Columns["TotalOut"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("NetMovement"))
                {
                    band.Columns["NetMovement"].CellAppearance.ForeColor = NetBlueColor;
                    band.Columns["NetMovement"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("CurrentStock"))
                {
                    band.Columns["CurrentStock"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
            else
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band, "TransactionDate", "Movement Date", 105, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureGridColumn(band, "RefNo", "Ref / Doc #", 110, null, HAlign.Left, 1);
                ConfigureGridColumn(band, "Operation", "Operation", 130, null, HAlign.Left, 2);
                ConfigureGridColumn(band, "Way", "Way", 70, null, HAlign.Center, 3);
                ConfigureGridColumn(band, "ItemName", "Item Description", 220, null, HAlign.Left, 4);
                ConfigureGridColumn(band, "BarCode", "Barcode", 110, null, HAlign.Left, 5);
                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, 6);
                ConfigureGridColumn(band, "InQty", "In Qty", 85, "#,##0.00", HAlign.Right, 7);
                ConfigureGridColumn(band, "OutQty", "Out Qty", 85, "#,##0.00", HAlign.Right, 8);
                ConfigureGridColumn(band, "Qty", "Qty", 80, "#,##0.00", HAlign.Right, 9);
                ConfigureGridColumn(band, "Rate", "Rate", 85, "#,##0.00", HAlign.Right, 10);
                ConfigureGridColumn(band, "Cost", "Cost Rate", 85, "#,##0.00", HAlign.Right, 11);
                ConfigureGridColumn(band, "Amount", "Amount", 100, "#,##0.00", HAlign.Right, 12);
                ConfigureGridColumn(band, "Account", "Party / Account", 160, null, HAlign.Left, 13);
                ConfigureGridColumn(band, "Remarks", "Remarks / Reason", 180, null, HAlign.Left, 14);
                ConfigureGridColumn(band, "CurrentStock", "Current Stock", 95, "#,##0.00", HAlign.Right, 15);

                if (band.Columns.Exists("ItemName"))
                {
                    band.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("InQty"))
                {
                    band.Columns["InQty"].CellAppearance.ForeColor = InGreenColor;
                    band.Columns["InQty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("OutQty"))
                {
                    band.Columns["OutQty"].CellAppearance.ForeColor = OutRedColor;
                    band.Columns["OutQty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            // Band 1: Hierarchical Drilldown Breakdown
            if (e.Layout.Bands.Count > 1 && isMatrix)
            {
                UltraGridBand childBand = e.Layout.Bands[1];
                childBand.ColHeadersVisible = true;
                childBand.HeaderVisible = true;
                childBand.Header.Caption = "Stock Movement Breakdown (Movement Date, Operation, Way [IN/OUT], Ref/Doc No, Quantity, Unit, Rate, Cost Rate, Total Amount, Party/Account, Remarks)";
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

                ConfigureChildGridColumn(childBand, "TransactionDate", "Movement Date", 105, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureChildGridColumn(childBand, "Operation", "Operation", 130, null, HAlign.Left, 1);
                ConfigureChildGridColumn(childBand, "Way", "Way", 70, null, HAlign.Center, 2);
                ConfigureChildGridColumn(childBand, "RefNo", "Ref / Doc #", 115, null, HAlign.Left, 3);
                ConfigureChildGridColumn(childBand, "Qty", "Quantity", 85, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(childBand, "Unit", "Unit", 60, null, HAlign.Center, 5);
                ConfigureChildGridColumn(childBand, "Rate", "Rate", 85, "#,##0.00", HAlign.Right, 6);
                ConfigureChildGridColumn(childBand, "Cost", "Cost Rate", 85, "#,##0.00", HAlign.Right, 7);
                ConfigureChildGridColumn(childBand, "Amount", "Total Amount", 105, "#,##0.00", HAlign.Right, 8);
                ConfigureChildGridColumn(childBand, "Account", "Party / Account", 160, null, HAlign.Left, 9);
                ConfigureChildGridColumn(childBand, "Remarks", "Remarks / Reason", 180, null, HAlign.Left, 10);
                ConfigureChildGridColumn(childBand, "BranchName", "Branch", 110, null, HAlign.Left, 11);

                if (childBand.Columns.Exists("Qty"))
                {
                    childBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            ReportGridLayoutHelper.ApplySavedLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
        }

        private static void ConfigureChildGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
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
            if (e.Row == null) return;

            // Child Band 1
            if (e.Row.Band != null && e.Row.Band.Index == 1)
            {
                if (e.Row.Cells.Exists("Way") && e.Row.Cells["Way"].Value != null)
                {
                    string way = Convert.ToString(e.Row.Cells["Way"].Value).ToUpperInvariant();
                    if (way == "IN")
                    {
                        e.Row.Cells["Way"].Appearance.ForeColor = InGreenColor;
                        e.Row.Cells["Way"].Appearance.BackColor = InBgColor;
                        e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;

                        if (e.Row.Cells.Exists("Qty"))
                        {
                            e.Row.Cells["Qty"].Appearance.ForeColor = InGreenColor;
                            e.Row.Cells["Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                    }
                    else if (way == "OUT")
                    {
                        e.Row.Cells["Way"].Appearance.ForeColor = OutRedColor;
                        e.Row.Cells["Way"].Appearance.BackColor = OutBgColor;
                        e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;

                        if (e.Row.Cells.Exists("Qty"))
                        {
                            e.Row.Cells["Qty"].Appearance.ForeColor = OutRedColor;
                            e.Row.Cells["Qty"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                    }
                }
                return;
            }

            // Master Band 0
            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            if (isMatrix)
            {
                foreach (var dateCol in _currentMatrixDateColumns)
                {
                    if (e.Row.Cells.Exists(dateCol.ColumnKey))
                    {
                        object val = e.Row.Cells[dateCol.ColumnKey].Value;
                        if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal numVal))
                        {
                            if (numVal == 0m)
                            {
                                e.Row.Cells[dateCol.ColumnKey].Appearance.ForeColor = ZeroMutedColor;
                                e.Row.Cells[dateCol.ColumnKey].Appearance.FontData.Bold = DefaultableBoolean.False;
                            }
                            else if (numVal > 0m)
                            {
                                e.Row.Cells[dateCol.ColumnKey].Appearance.ForeColor = (_currentValueMode == ValueDisplayMode.StockInOnly) ? InGreenColor : NetBlueColor;
                                e.Row.Cells[dateCol.ColumnKey].Appearance.FontData.Bold = DefaultableBoolean.True;
                            }
                            else
                            {
                                e.Row.Cells[dateCol.ColumnKey].Appearance.ForeColor = OutRedColor;
                                e.Row.Cells[dateCol.ColumnKey].Appearance.FontData.Bold = DefaultableBoolean.True;
                            }
                        }
                    }
                }

                if (e.Row.Cells.Exists("Total"))
                {
                    object val = e.Row.Cells["Total"].Value;
                    if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal totalVal))
                    {
                        if (totalVal > 0m)
                        {
                            e.Row.Cells["Total"].Appearance.ForeColor = (_currentValueMode == ValueDisplayMode.StockInOnly) ? InGreenColor : NetBlueColor;
                        }
                        else if (totalVal < 0m)
                        {
                            e.Row.Cells["Total"].Appearance.ForeColor = OutRedColor;
                        }
                        else
                        {
                            e.Row.Cells["Total"].Appearance.ForeColor = ZeroMutedColor;
                        }
                        e.Row.Cells["Total"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }

                if (e.Row.Cells.Exists("NetMovement"))
                {
                    object val = e.Row.Cells["NetMovement"].Value;
                    if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal netVal))
                    {
                        if (netVal > 0m)
                            e.Row.Cells["NetMovement"].Appearance.ForeColor = InGreenColor;
                        else if (netVal < 0m)
                            e.Row.Cells["NetMovement"].Appearance.ForeColor = OutRedColor;
                        else
                            e.Row.Cells["NetMovement"].Appearance.ForeColor = ZeroMutedColor;
                        e.Row.Cells["NetMovement"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
            }
            else
            {
                if (e.Row.Cells.Exists("Way") && e.Row.Cells["Way"].Value != null)
                {
                    string way = Convert.ToString(e.Row.Cells["Way"].Value).ToUpperInvariant();
                    if (way == "IN")
                    {
                        e.Row.Cells["Way"].Appearance.ForeColor = InGreenColor;
                        e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                    else if (way == "OUT")
                    {
                        e.Row.Cells["Way"].Appearance.ForeColor = OutRedColor;
                        e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
            }
        }
        #endregion

        #region Actions & Event Handlers
        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnToggleValueMode_Click(object sender, EventArgs e)
        {
            switch (_currentValueMode)
            {
                case ValueDisplayMode.NetMovement:
                    _currentValueMode = ValueDisplayMode.StockInOnly;
                    break;
                case ValueDisplayMode.StockInOnly:
                    _currentValueMode = ValueDisplayMode.StockOutOnly;
                    break;
                case ValueDisplayMode.StockOutOnly:
                default:
                    _currentValueMode = ValueDisplayMode.NetMovement;
                    break;
            }

            UpdateValueModeButtonText();

            string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
            if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                string wayFilter = Convert.ToString(ultraComboWay.Value) ?? "ALL";
                string opFilter = Convert.ToString(ultraComboOperation.Value) ?? "ALL";

                DatewiseItemStockReportFilter filter = new DatewiseItemStockReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    WayFilter = wayFilter,
                    OperationFilter = opFilter,
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DETAILED"
                };

                List<PivotDateColumnInfo> matrixCols;
                DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                _matrixDataSet = ds;
                _matrixTable = ds.Tables["StockMovementMatrix"];
                _currentMatrixDateColumns.Clear();
                _currentMatrixDateColumns.AddRange(matrixCols);

                gridReport.DataSource = null;
                gridReport.DataSource = _matrixDataSet;

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
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
                preview.Text = "Date-wise Item Stock Movement Report - Preview";
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
                    Text = "DATE-WISE ITEM STOCK MOVEMENT REPORT",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Height = 24
                };

                string periodText = $"Period: {ultraComboPeriodMode.Text} | From: {dtFrom.DateTime:dd/MM/yyyy} To: {dtTo.DateTime:dd/MM/yyyy}";
                string wayText = $"Stock Way: {ultraComboWay.Text}";
                string opText = $"Operation: {ultraComboOperation.Text}";
                string itemText = string.IsNullOrWhiteSpace(txtItemSearch.Text) ? "All Items" : $"Item: {txtItemSearch.Text}";
                string valueModeText = $"Display: {_currentValueMode}";

                Label metaLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"{periodText}  |  {wayText}  |  {opText}  |  {itemText}  |  {valueModeText}\nGenerated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
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
                dialog.FileName = $"DatewiseItemStockReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();

                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                var visibleCols = band.Columns.Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "ItemStockMovementBreakdown", StringComparison.OrdinalIgnoreCase))
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
                _currentValueMode = ValueDisplayMode.NetMovement;
                UpdateValueModeButtonText();
                ultraComboPeriodMode.Value = "DATE_RANGE";
                dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtTo.Value = DateTime.Today;
                ultraComboMonth.Value = DateTime.Today.Month;
                ultraComboYear.Value = DateTime.Today.Year;
                ultraComboViewMode.Value = "DATE_MATRIX";
                ultraComboWay.Value = "ALL";
                ultraComboOperation.Value = "ALL";
                txtItemSearch.Text = string.Empty;
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
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmdatewiseitemstockreport"))
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
            return $"DatewiseItemStockReport_{viewMode}";
        }
        #endregion
    }
}
