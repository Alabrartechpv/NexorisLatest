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

namespace PosBranch_Win.Reports.InventoryReport
{
    /// <summary>
    /// Date-wise Stock Adjustment Report (frmdatewisestockadjustmentreport)
    /// Features full parity with frmdatewisepurchasereport, frmdatewisesalesreport, and frmVendorOutstandingReport:
    /// exact theme, grid styling, dynamic footer cell aggregations with context menu,
    /// header drag-to-hide, floating column chooser, layout persistence, and CSV/PDF export.
    /// </summary>
    public partial class frmdatewisestockadjustmentreport : Form
    {
        #region Colour Palette (Exact match with frmdatewisepurchasereport & frmVendorOutstandingReport)
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
        private readonly DatewiseStockAdjustmentReportRepository _repository;
        private List<DatewiseStockAdjustmentReportRow> _reportRows;
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
            "SystemStock",
            "PhysicalStock",
            "QtyDifference",
            "Qty",
            "StockInQty",
            "StockOutQty",
            "Cost",
            "Amount",
            "NetAmount",
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
            public string AdjustmentType { get; set; }
            public decimal TotalQty { get; set; }
            public decimal TotalAmount { get; set; }
            // Physical Stock tracking for last-reading-per-period logic
            public DateTime LastPhysStockDate { get; set; }
            public int LastPhysStockAdjNo { get; set; }
            public decimal LastPhysStock { get; set; }
            public bool HasPhysStock { get; set; }
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
        public frmdatewisestockadjustmentreport()
        {
            _repository = new DatewiseStockAdjustmentReportRepository();
            _reportRows = new List<DatewiseStockAdjustmentReportRow>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            // Wire form lifecycle
            Load += frmdatewisestockadjustmentreport_Load;
            FormClosing += frmdatewisestockadjustmentreport_FormClosing;
            KeyDown += frmdatewisestockadjustmentreport_KeyDown;
            KeyPreview = true;

            // Wire action buttons
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

            // Filter value changes
            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboMonth.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboYear.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtFrom.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtTo.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboAdjustmentType.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
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
            return $"DatewiseStockAdjustmentReport_{viewMode}";
        }

        private void frmdatewisestockadjustmentreport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmdatewisestockadjustmentreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
        }

        private void frmdatewisestockadjustmentreport_KeyDown(object sender, KeyEventArgs e)
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
                Text = "Date-wise Stock Adjustment Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                UpdateValueModeButtonText();
                SetupGrid();
                SetupHeaderDragToHideAndColumnChooser();
                InitializeGridFooter();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }
        #endregion

        #region Styling & Layout (Exact match with frmdatewisepurchasereport)
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelControls.Dock = DockStyle.Top;
                ultraPanelControls.Height = 78;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelAction.Dock = DockStyle.Top;
                ultraPanelAction.Height = 44;
            }

            if (ultraPanelMaster != null)
            {
                ultraPanelMaster.Appearance.BackColor = FormBackColor;
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
            StyleLabel(lblAdjustmentType);
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

        private static void StyleSmallPickerButton(Button button)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = SkyBlueOutline;
            button.FlatAppearance.BorderSize = 1;
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
            StyleFilterCombo(ultraComboAdjustmentType, true);

            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
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
            layout.GroupByBox.Hidden = false; // Enable Outlook GroupByBox so user can group by Date or Item!
            layout.GroupByBox.Prompt = "Drag a column header here (such as Adjustment Date or Item Name) to group records.";

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
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "ItemAdjustmentBreakdown", StringComparison.OrdinalIgnoreCase))
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
                    string.Equals(columnKey, "ItemID", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "BarCode", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "SlNo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "AdjustmentNo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "AdjustmentType", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "Reason", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "Comments", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "LedgerName", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "UserName", StringComparison.OrdinalIgnoreCase))
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
                   string.Equals(column.Key, "SystemStock", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "PhysicalStock", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "QtyDifference", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "StockInQty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "StockOutQty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TotalQty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TotalAmount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Total", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Cost", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Amount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "NetAmount", StringComparison.OrdinalIgnoreCase) ||
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
            ColumnChooserItem item = e.Data.GetData(typeof(ColumnChooserItem)) as ColumnChooserItem;
            if (item != null)
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

        private void UnhideColumn(string key, int targetVisiblePosition)
        {
            if (string.IsNullOrEmpty(key) || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn col = band.Columns[key];
            userHiddenColumnKeys.Remove(key);
            col.Hidden = false;
            col.Header.VisiblePosition = targetVisiblePosition;

            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point pt)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide '{col.Header.Caption}'");
            hideItem.Click += (s, ev) => HideColumn(col);
            menu.Items.Add(hideItem);

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("Column Chooser...");
            chooserItem.Click += (s, ev) => ShowColumnChooser();
            menu.Items.Add(chooserItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem sortAsc = new ToolStripMenuItem("Sort Ascending");
            sortAsc.Click += (s, ev) =>
            {
                gridReport.DisplayLayout.Bands[0].SortedColumns.Clear();
                gridReport.DisplayLayout.Bands[0].SortedColumns.Add(col.Key, false);
            };
            menu.Items.Add(sortAsc);

            ToolStripMenuItem sortDesc = new ToolStripMenuItem("Sort Descending");
            sortDesc.Click += (s, ev) =>
            {
                gridReport.DisplayLayout.Bands[0].SortedColumns.Clear();
                gridReport.DisplayLayout.Bands[0].SortedColumns.Add(col.Key, true);
            };
            menu.Items.Add(sortDesc);

            ToolStripMenuItem clearFilter = new ToolStripMenuItem("Clear Filter on this column");
            clearFilter.Click += (s, ev) =>
            {
                if (gridReport.DisplayLayout.Bands[0].ColumnFilters.Exists(col.Key))
                {
                    gridReport.DisplayLayout.Bands[0].ColumnFilters[col.Key].FilterConditions.Clear();
                    UpdateFooterValues();
                    UpdateFooterCellPositions();
                }
            };
            menu.Items.Add(clearFilter);

            menu.Show(gridReport, pt);
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.BringToFront();
                return;
            }

            columnChooserForm = new Form
            {
                Text = "Customization",
                Width = 260,
                Height = 350,
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                BackColor = FormBackColor
            };

            Point btnPos = btnColumnChooser.PointToScreen(Point.Empty);
            columnChooserForm.Location = new Point(btnPos.X, btnPos.Y + btnColumnChooser.Height + 2);

            Label instructions = new Label
            {
                Text = "Drag columns between this window and the column headers to add or remove them from view.",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 2, 6, 2),
                Font = new Font("Tahoma", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Tahoma", 8.5F, FontStyle.Regular),
                ForeColor = Color.Gray,
                Text = "Search columns..."
            };
            txtColumnSearch.Enter += (s, e) => { if (txtColumnSearch.Text == "Search columns...") { txtColumnSearch.Text = ""; txtColumnSearch.ForeColor = Color.Black; } };
            txtColumnSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtColumnSearch.Text)) { txtColumnSearch.Text = "Search columns..."; txtColumnSearch.ForeColor = Color.Gray; } };
            txtColumnSearch.TextChanged += txtColumnSearch_TextChanged;

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8.25F, FontStyle.Regular),
                SelectionMode = SelectionMode.One,
                BorderStyle = BorderStyle.FixedSingle
            };
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += (s, e) =>
            {
                ColumnChooserItem item = columnChooserListBox.SelectedItem as ColumnChooserItem;
                if (item != null)
                {
                    UnhideColumn(item.ColumnKey, 0);
                }
            };

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = ActionPanelBackColor
            };

            Button btnSelectAll = new Button
            {
                Text = "Select All",
                Size = new Size(70, 24),
                Location = new Point(4, 4),
                Font = new Font("Tahoma", 8F),
                FlatStyle = FlatStyle.Flat
            };
            btnSelectAll.Click += btnSelectAll_Click;

            Button btnDeselectAll = new Button
            {
                Text = "Deselect",
                Size = new Size(70, 24),
                Location = new Point(78, 4),
                Font = new Font("Tahoma", 8F),
                FlatStyle = FlatStyle.Flat
            };
            btnDeselectAll.Click += btnDeselectAll_Click;

            Button btnResetDefault = new Button
            {
                Text = "Restore",
                Size = new Size(70, 24),
                Location = new Point(152, 4),
                Font = new Font("Tahoma", 8F),
                FlatStyle = FlatStyle.Flat
            };
            btnResetDefault.Click += btnResetDefault_Click;

            buttonPanel.Controls.Add(btnSelectAll);
            buttonPanel.Controls.Add(btnDeselectAll);
            buttonPanel.Controls.Add(btnResetDefault);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(buttonPanel);
            columnChooserForm.Controls.Add(txtColumnSearch);
            columnChooserForm.Controls.Add(instructions);

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            string filter = (txtColumnSearch != null && txtColumnSearch.Text != "Search columns...")
                ? txtColumnSearch.Text.Trim().ToLowerInvariant()
                : string.Empty;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filter) || caption.ToLowerInvariant().Contains(filter) || col.Key.ToLowerInvariant().Contains(filter))
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
            }
        }

        private void txtColumnSearch_TextChanged(object sender, EventArgs e)
        {
            PopulateColumnChooserListBox();
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True && !col.IsChaptered && !string.Equals(col.Key, "ItemAdjustmentBreakdown", StringComparison.OrdinalIgnoreCase))
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

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            UnhideAllColumns();
        }

        private void btnDeselectAll_Click(object sender, EventArgs e)
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Key != "ItemName" && col.Key != "AdjustmentDate" && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    userHiddenColumnKeys.Add(col.Key);
                    col.Hidden = true;
                }
            }
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void btnResetDefault_Click(object sender, EventArgs e)
        {
            UnhideAllColumns();
        }
        #endregion

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            // Period mode dropdown
            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date Range");
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("ALL", "All Dates");
            ultraComboPeriodMode.Value = "DATE_RANGE";

            // Default dates
            dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtTo.Value = DateTime.Today;

            // Month dropdown
            ultraComboMonth.Items.Clear();
            ultraComboMonth.Items.Add(0, "-- ALL Months --");
            for (int m = 1; m <= 12; m++)
            {
                ultraComboMonth.Items.Add(m, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m));
            }
            ultraComboMonth.Value = DateTime.Today.Month;

            // Year dropdown
            ultraComboYear.Items.Clear();
            ultraComboYear.Items.Add(0, "-- ALL Years --");
            int curYear = DateTime.Today.Year;
            for (int y = curYear - 5; y <= curYear + 1; y++)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = curYear;

            // View modes
            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Item vs Dates Qty)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Line by Line)");
            ultraComboViewMode.Items.Add("DATE_ITEM_SUMMARY", "Date & Item Summary");
            ultraComboViewMode.Items.Add("ITEM_SUMMARY", "Item Summary (Total by Item)");
            ultraComboViewMode.Value = "DATE_MATRIX";

            // Adjustment Types dropdown
            DataTable dtAdj = _repository.GetAdjustmentTypeDDL();
            ultraComboAdjustmentType.DataSource = dtAdj;
            ultraComboAdjustmentType.ValueMember = "Code";
            ultraComboAdjustmentType.DisplayMember = "Name";
            ultraComboAdjustmentType.Value = "ALL";

            txtItemSearch.Text = string.Empty;

            UpdatePeriodControlVisibility();
            StyleFilterControls();
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
                lblYear.Location = new Point(380, 14);
                ultraComboYear.Location = new Point(415, 12);
            }
            else if (isYearWise)
            {
                lblYear.Location = new Point(220, 14);
                ultraComboYear.Location = new Point(264, 12);
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
            btnToggleValueMode.Text = _isAmountMode ? "Show Phys Stock" : "Show Amount";
        }
        #endregion

        #region Matrix Building & Data Loading
        private DataTable BuildMatrixDataTable(List<DatewiseStockAdjustmentReportRow> rawRows, DatewiseStockAdjustmentReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            dateColumns = new List<PivotDateColumnInfo>();
            string mode = filter.PeriodMode ?? "DATE_RANGE";

            if (mode == "MONTH_WISE")
            {
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;
                int month = filter.Month > 0 ? filter.Month : DateTime.Today.Month;

                if (month > 0)
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
                        var distinctDates = (rawRows ?? new List<DatewiseStockAdjustmentReportRow>())
                            .Select(r => r.AdjustmentDate.Date)
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
                var distinctDates = (rawRows ?? new List<DatewiseStockAdjustmentReportRow>())
                    .Select(r => r.AdjustmentDate.Date)
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
            DataTable table = new DataTable("StockAdjustmentMatrix");
            table.Columns.Add("ItemKey", typeof(string));
            table.Columns.Add("ItemID", typeof(int));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("ItemName", typeof(string));
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("AdjustmentType", typeof(string));

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

            var cAvgRate = table.Columns.Add("AvgCost", typeof(decimal));
            cAvgRate.DefaultValue = 0m;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Pre-aggregation by item:
            //   Non-amount mode → Physical Stock = LAST reading per item per date period
            //   Amount mode     → Net Amount = SUM per item per date period
            // "Last reading" is determined by the most recent (AdjustmentDate, AdjustmentNo) pair.

            // datePhysStockMap: cellKey → (lastDate, lastAdjNo, physStock)
            Dictionary<string, DateTime> datePhysStockLastDate = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> datePhysStockLastAdjNo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> datePhysStockMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            Dictionary<string, decimal> dateAmtMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, ItemMatrixInfo> itemInfoMap = new Dictionary<string, ItemMatrixInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseStockAdjustmentReportRow r in rawRows)
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
                        AdjustmentType = r.AdjustmentType ?? string.Empty,
                        TotalQty = 0m,
                        TotalAmount = 0m,
                        HasPhysStock = false,
                        LastPhysStock = 0m,
                        LastPhysStockDate = DateTime.MinValue,
                        LastPhysStockAdjNo = -1
                    };
                    itemInfoMap[itemKey] = itemInfo;
                }
                else if (string.IsNullOrEmpty(itemInfo.BarCode) && !string.IsNullOrEmpty(r.BarCode))
                {
                    itemInfo.BarCode = r.BarCode;
                }

                decimal rowAmount = (r.NetAmount != 0m) ? r.NetAmount : Math.Abs(r.Amount);
                itemInfo.TotalQty += r.Qty;
                itemInfo.TotalAmount += rowAmount;

                // Track last PhysicalStock across all dates for this item (for Total column)
                bool isMoreRecent = !itemInfo.HasPhysStock
                    || r.AdjustmentDate.Date > itemInfo.LastPhysStockDate
                    || (r.AdjustmentDate.Date == itemInfo.LastPhysStockDate && r.AdjustmentNo > itemInfo.LastPhysStockAdjNo);
                if (isMoreRecent)
                {
                    itemInfo.HasPhysStock = true;
                    itemInfo.LastPhysStock = r.PhysicalStock;
                    itemInfo.LastPhysStockDate = r.AdjustmentDate.Date;
                    itemInfo.LastPhysStockAdjNo = r.AdjustmentNo;
                }

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    bool matches = false;
                    if (dateCol.Date.HasValue)
                    {
                        matches = (r.AdjustmentDate.Date == dateCol.Date.Value.Date);
                    }
                    else if (dateCol.MonthNumber > 0)
                    {
                        matches = (r.AdjustmentDate.Month == dateCol.MonthNumber);
                    }
                    else if (dateCol.DayNumber > 0)
                    {
                        matches = (r.AdjustmentDate.Day == dateCol.DayNumber);
                    }

                    if (matches)
                    {
                        string cellKey = itemKey + "_" + dateCol.ColumnKey;

                        // Physical Stock: keep only the LAST adjustment's reading per cell
                        DateTime existingDate;
                        int existingAdjNo;
                        datePhysStockLastDate.TryGetValue(cellKey, out existingDate);
                        datePhysStockLastAdjNo.TryGetValue(cellKey, out existingAdjNo);

                        bool isCellMoreRecent = !datePhysStockMap.ContainsKey(cellKey)
                            || r.AdjustmentDate.Date > existingDate
                            || (r.AdjustmentDate.Date == existingDate && r.AdjustmentNo > existingAdjNo);

                        if (isCellMoreRecent)
                        {
                            datePhysStockMap[cellKey] = r.PhysicalStock;
                            datePhysStockLastDate[cellKey] = r.AdjustmentDate.Date;
                            datePhysStockLastAdjNo[cellKey] = r.AdjustmentNo;
                        }

                        // Amount: sum all adjustments per cell
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
                dr["AdjustmentType"] = item.AdjustmentType;

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
                        // Physical Stock: last reading for this item in this date period
                        datePhysStockMap.TryGetValue(cellKey, out cellVal);
                    }
                    dr[dateCol.ColumnKey] = cellVal;
                }

                // Total column: Amount mode = sum of amounts; Phys Stock mode = last physical stock across all dates
                dr["Total"] = _isAmountMode ? item.TotalAmount : item.LastPhysStock;
                dr["TotalQty"] = item.TotalQty;
                dr["TotalAmount"] = item.TotalAmount;
                dr["AvgCost"] = item.TotalQty > 0 ? Math.Round(item.TotalAmount / item.TotalQty, 2) : 0m;

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

        private DataTable BuildDetailsDataTable(List<DatewiseStockAdjustmentReportRow> rawRows)
        {
            DataTable details = new DataTable("AdjustmentDetails");
            details.Columns.Add("ItemKey", typeof(string));
            details.Columns.Add("AdjustmentDate", typeof(DateTime));
            details.Columns.Add("AdjustmentNo", typeof(string));
            details.Columns.Add("AdjustmentType", typeof(string));
            details.Columns.Add("SystemStock", typeof(decimal));
            details.Columns.Add("PhysicalStock", typeof(decimal));
            details.Columns.Add("QtyDifference", typeof(decimal));
            details.Columns.Add("Cost", typeof(decimal));
            details.Columns.Add("NetAmount", typeof(decimal));
            details.Columns.Add("Reason", typeof(string));

            if (rawRows != null && rawRows.Count > 0)
            {
                foreach (DatewiseStockAdjustmentReportRow r in rawRows.OrderBy(x => x.AdjustmentDate).ThenBy(x => x.AdjustmentNo).ThenBy(x => x.SlNo))
                {
                    string itemKey = (r.ItemID > 0 ? r.ItemID.ToString() : (r.ItemName ?? string.Empty)) + "_" + (r.Unit ?? string.Empty);
                    DataRow dr = details.NewRow();
                    dr["ItemKey"] = itemKey;
                    dr["AdjustmentDate"] = r.AdjustmentDate.Date;
                    dr["AdjustmentNo"] = r.AdjustmentNo > 0 ? "ADJ-" + r.AdjustmentNo : "-";
                    dr["AdjustmentType"] = !string.IsNullOrEmpty(r.AdjustmentType) ? r.AdjustmentType : (r.QtyDifference >= 0 ? "Stock IN" : "Stock OUT");
                    dr["SystemStock"] = r.SystemStock;
                    dr["PhysicalStock"] = r.PhysicalStock;
                    dr["QtyDifference"] = r.QtyDifference;
                    dr["Cost"] = r.Cost;
                    dr["NetAmount"] = (r.NetAmount != 0 ? r.NetAmount : Math.Round(Math.Abs(r.QtyDifference) * r.Cost, 2));
                    dr["Reason"] = !string.IsNullOrEmpty(r.Reason) ? r.Reason : (!string.IsNullOrEmpty(r.Comments) ? r.Comments : "-");
                    details.Rows.Add(dr);
                }
            }

            return details;
        }

        private DataSet BuildMatrixDataSet(List<DatewiseStockAdjustmentReportRow> rawRows, DatewiseStockAdjustmentReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable detailsTable = BuildDetailsDataTable(rawRows);

            DataSet ds = new DataSet("DatewiseStockAdjustmentDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(detailsTable);

            DataRelation relation = new DataRelation("ItemAdjustmentBreakdown", matrixTable.Columns["ItemKey"], detailsTable.Columns["ItemKey"], false);
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

                DatewiseStockAdjustmentReportFilter filter = new DatewiseStockAdjustmentReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    AdjustmentType = ultraComboAdjustmentType.Value != null ? Convert.ToString(ultraComboAdjustmentType.Value) : "ALL",
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DETAILED" // Fetch transaction details so drilldown displays real adjustment details
                };

                _reportRows = _repository.GetReport(filter);

                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    List<PivotDateColumnInfo> matrixCols;
                    DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                    _matrixDataSet = ds;
                    _matrixTable = ds.Tables["StockAdjustmentMatrix"];
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
                MessageBox.Show($"Error loading Date-wise Stock Adjustment Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (column.IsChaptered || string.Equals(column.Key, "ItemAdjustmentBreakdown", StringComparison.OrdinalIgnoreCase))
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
                        string modeDesc = _isAmountMode ? "Net Amount" : "Physical Stock (Last Reading)";
                        band.Columns[dateCol.ColumnKey].Header.ToolTipText = $"{dateCol.Tooltip} ({modeDesc})";
                    }
                }

                ConfigureGridColumn(band, "Total", "Total", 90, "#,##0.00", HAlign.Right, visPos++);
                if (band.Columns.Exists("Total"))
                {
                    band.Columns["Total"].Hidden = false;
                    band.Columns["Total"].Header.Caption = "Total";
                    band.Columns["Total"].Header.ToolTipText = _isAmountMode ? "Total Net Amount across dates" : "Physical Stock (last reading across all dates)";
                    band.Columns["Total"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Total"].CellAppearance.ForeColor = _isAmountMode ? Color.FromArgb(27, 94, 32) : Color.FromArgb(13, 71, 161);
                }

                ConfigureGridColumn(band, "AdjustmentType", "Adj Type", 85, null, HAlign.Center, visPos++);
                ConfigureGridColumn(band, "Unit", "Unit", 55, null, HAlign.Center, visPos++);
                ConfigureGridColumn(band, "TotalQty", "Total Qty", 85, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalAmount", "Total Amount", 105, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "AvgCost", "Avg Rate", 80, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "BarCode", "Barcode", 110, null, HAlign.Left, visPos++);

                if (band.Columns.Exists("AdjustmentType") && !userHiddenColumnKeys.Contains("AdjustmentType"))
                {
                    band.Columns["AdjustmentType"].Hidden = true;
                }
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
                if (band.Columns.Exists("AvgCost"))
                {
                    band.Columns["AvgCost"].Hidden = true;
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

                ConfigureGridColumn(band, "AdjustmentDate", "Adjustment Date", 105, "dd-MMM-yyyy", HAlign.Left, 0);
                ConfigureGridColumn(band, "AdjustmentNo", "Adj #", 75, null, HAlign.Center, 1);
                ConfigureGridColumn(band, "ItemName", "Item Description", 230, null, HAlign.Left, 2);
                ConfigureGridColumn(band, "BarCode", "Barcode", 115, null, HAlign.Left, 3);
                ConfigureGridColumn(band, "Unit", "Unit", 60, null, HAlign.Center, 4);
                ConfigureGridColumn(band, "AdjustmentType", "Adj Type", 90, null, HAlign.Center, 5);
                ConfigureGridColumn(band, "SystemStock", "System Stock", 90, "#,##0.00", HAlign.Right, 6);
                ConfigureGridColumn(band, "PhysicalStock", "Physical Stock", 90, "#,##0.00", HAlign.Right, 7);
                ConfigureGridColumn(band, "QtyDifference", "Difference", 85, "#,##0.00", HAlign.Right, 8);
                ConfigureGridColumn(band, "Qty", "Qty", 80, "#,##0.00", HAlign.Right, 9);
                ConfigureGridColumn(band, "Cost", "Cost Rate", 90, "#,##0.00", HAlign.Right, 10);
                ConfigureGridColumn(band, "Amount", "Diff Amount", 100, "#,##0.00", HAlign.Right, 11);
                ConfigureGridColumn(band, "NetAmount", "Net Value", 110, "#,##0.00", HAlign.Right, 12);
                ConfigureGridColumn(band, "Reason", "Reason", 130, null, HAlign.Left, 13);
                ConfigureGridColumn(band, "Comments", "Comments", 130, null, HAlign.Left, 14);
                ConfigureGridColumn(band, "LedgerName", "Account", 120, null, HAlign.Left, 15);
                ConfigureGridColumn(band, "UserName", "Adjusted By", 100, null, HAlign.Left, 16);

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
                childBand.Header.Caption = "Stock Adjustment Breakdown (Date, Adj No, Type, Sys Stock, Phys Stock, Qty Diff, Cost, Total Cost)";
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

                ConfigureChildGridColumn(childBand, "AdjustmentDate", "Adjustment Date", 110, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureChildGridColumn(childBand, "AdjustmentNo", "Adjustment No", 110, null, HAlign.Left, 1);
                ConfigureChildGridColumn(childBand, "AdjustmentType", "Type", 90, null, HAlign.Center, 2);
                ConfigureChildGridColumn(childBand, "SystemStock", "System Stock", 95, "#,##0.00", HAlign.Right, 3);
                ConfigureChildGridColumn(childBand, "PhysicalStock", "Physical Stock", 95, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(childBand, "QtyDifference", "Qty Difference", 100, "#,##0.00", HAlign.Right, 5);
                ConfigureChildGridColumn(childBand, "Cost", "Unit Cost", 95, "#,##0.00", HAlign.Right, 6);
                ConfigureChildGridColumn(childBand, "NetAmount", "Total Cost", 115, "#,##0.00", HAlign.Right, 7);
                ConfigureChildGridColumn(childBand, "Reason", "Reason", 140, null, HAlign.Left, 8);

                if (childBand.Columns.Exists("QtyDifference"))
                {
                    childBand.Columns["QtyDifference"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    childBand.Columns["QtyDifference"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (childBand.Columns.Exists("Cost"))
                {
                    childBand.Columns["Cost"].CellAppearance.ForeColor = Color.FromArgb(20, 40, 80);
                    childBand.Columns["Cost"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
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
                    if (e.Row.Cells.Exists("AdjustmentType"))
                    {
                        string adjType = Convert.ToString(e.Row.Cells["AdjustmentType"].Value) ?? string.Empty;
                        if (adjType.IndexOf("IN", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            e.Row.Cells["AdjustmentType"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                            e.Row.Cells["AdjustmentType"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                        else if (adjType.IndexOf("OUT", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            e.Row.Cells["AdjustmentType"].Appearance.ForeColor = Color.FromArgb(198, 40, 40);
                            e.Row.Cells["AdjustmentType"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                    }

                    if (e.Row.Cells.Exists("NetAmount"))
                    {
                        e.Row.Cells["NetAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["NetAmount"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                    }
                }
            }
            else if (e.Row.Band.Index == 1)
            {
                if (e.Row.Cells.Exists("QtyDifference"))
                {
                    var cell = e.Row.Cells["QtyDifference"];
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                    cell.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                }
                if (e.Row.Cells.Exists("Cost"))
                {
                    var cell = e.Row.Cells["Cost"];
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
                DatewiseStockAdjustmentReportFilter filter = new DatewiseStockAdjustmentReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    AdjustmentType = ultraComboAdjustmentType.Value != null ? Convert.ToString(ultraComboAdjustmentType.Value) : "ALL",
                    ItemSearch = txtItemSearch.Text != null ? txtItemSearch.Text.Trim() : string.Empty,
                    ViewMode = "DATE_MATRIX"
                };

                List<PivotDateColumnInfo> matrixCols;
                DataSet ds = BuildMatrixDataSet(_reportRows, filter, out matrixCols);
                _matrixDataSet = ds;
                _matrixTable = ds.Tables["StockAdjustmentMatrix"];
                _currentMatrixDateColumns.Clear();
                _currentMatrixDateColumns.AddRange(matrixCols);

                gridReport.DataSource = null;
                gridReport.DataSource = _matrixDataSet;

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open print preview: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            {
                preview.Text = "Date-wise Stock Adjustment Report - Grid Preview";
                preview.Size = new Size(1100, 700);
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.BackColor = FormBackColor;

                UltraGrid previewGrid = new UltraGrid();
                previewGrid.DisplayLayout.CopyFrom(gridReport.DisplayLayout);
                previewGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                previewGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                previewGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
                previewGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                previewGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                Panel header = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 52,
                    BackColor = GridHeaderBlue
                };

                Label titleLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 26,
                    Text = "Date-wise Stock Adjustment Report",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Bottom,
                    Height = 22,
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
                dialog.FileName = $"DatewiseStockAdjustmentReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Date-wise Stock Adjustment Report")) return;

                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                var visibleCols = band.Columns.Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "ItemAdjustmentBreakdown", StringComparison.OrdinalIgnoreCase))
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
                ultraComboAdjustmentType.Value = "ALL";
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
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmdatewisestockadjustmentreport"))
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

                        if (!string.IsNullOrEmpty(itemName))
                        {
                            txtItemSearch.Text = itemName;
                            LoadReport();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open item selection dialog: {ex.Message}", "Item Lookup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        #endregion
    }
}
