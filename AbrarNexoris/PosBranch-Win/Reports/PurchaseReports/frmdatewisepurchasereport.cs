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
    /// Date-wise Purchase Report (frmdatewisepurchasereport)
    /// Displays purchase transactions and item details filtered by Date Range, Month Wise, Year Wise, or ALL dates.
    /// Features full parity with frmVendorOutstandingReport: exact theme, grid styling, dynamic footer cell aggregations,
    /// header drag-to-hide, floating column chooser, layout persistence, and CSV/PDF export.
    /// </summary>
    public partial class frmdatewisepurchasereport : Form
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
        private readonly DatewisePurchaseReportRepository _repository;
        private List<DatewisePurchaseReportRow> _reportRows;
        private bool _isLoading;

        // Dynamic footer panel and cell controls
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Qty",
            "Free",
            "TotalQty",
            "Cost",
            "Amount",
            "TaxAmt",
            "NetAmount"
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

        #region Constructor & Lifecycle
        public frmdatewisepurchasereport()
        {
            _repository = new DatewisePurchaseReportRepository();
            _reportRows = new List<DatewisePurchaseReportRow>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmdatewisepurchasereport_Load;
            FormClosing += frmdatewisepurchasereport_FormClosing;
            KeyDown += frmdatewisepurchasereport_KeyDown;
            KeyPreview = true;

            // Action buttons
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnColumnChooser.Click += btnColumnChooser_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            btnItemPicker.Click += btnItemPicker_Click;
            btnVendorPicker.Click += btnVendorPicker_Click;

            // Filter value changes
            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            txtItemSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { LoadReport(); e.Handled = true; }
                else if (e.KeyCode == Keys.F11) { btnItemPicker.PerformClick(); e.Handled = true; }
            };

            // Grid event registrations for footer cell sync and column drag
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) => UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Resize += (s, e) => UpdateFooterCellPositions();
            }
        }

        private string GetLayoutKey() => "DatewisePurchaseReport";

        private void frmdatewisepurchasereport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmdatewisepurchasereport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
        }

        private void frmdatewisepurchasereport_KeyDown(object sender, KeyEventArgs e)
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
                Text = "Date-wise Purchase Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
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
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Bill & Item Wise)");
            ultraComboViewMode.Items.Add("DATE_ITEM_SUMMARY", "Daily Summary (Date & Item)");
            ultraComboViewMode.Items.Add("ITEM_SUMMARY", "Item Summary (Total by Item)");
            ultraComboViewMode.Value = "DETAILED";

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
                System.Diagnostics.Debug.WriteLine($"Error loading vendor combo: {ex.Message}");
            }
        }
        #endregion

        #region Styling & Layout
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
            StyleLabel(lblItem);
            StyleLabel(lblVendor);
        }

        private void StyleButtons()
        {
            StyleActionButton(btnViewGrid);
            StyleActionButton(btnPreviewGrid);
            StyleActionButton(btnPreviewReport);
            StyleActionButton(btnExportGrid);
            StyleActionButton(btnClearFilters);
            StyleActionButton(btnColumnChooser);
            StyleActionButton(btnToggleSelection);

            StyleSmallPickerButton(btnItemPicker);
            StyleSmallPickerButton(btnVendorPicker);
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
            StyleFilterCombo(ultraComboVendor, true);

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
            combo.Appearance.FontData.SizeInPoints = 9;
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
            layout.GroupByBox.Prompt = "Drag a column header here (such as Purchase Date or Item Name) to group records.";

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

        #region Grid Footer Setup & Alignment (Parity with frmVendorOutstandingReport)
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
            UpdateFooterValues(new List<DatewisePurchaseReportRow>());
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
                UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
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

        private void UpdateFooterValues(IList<DatewisePurchaseReportRow> rows = null)
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
                if (string.Equals(columnKey, "Qty", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "Free", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(columnKey, "TotalQty", StringComparison.OrdinalIgnoreCase))
                {
                    return numVal.ToString("#,##0.00");
                }
                return numVal.ToString("#,##0.00");
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
                   string.Equals(column.Key, "Free", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TotalQty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Cost", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Amount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "TaxAmt", StringComparison.OrdinalIgnoreCase) ||
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
                UpdateFooterValues(_reportRows ?? new List<DatewisePurchaseReportRow>());
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
            UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            if (_filterManager != null)
            {
                menu.Items.Add(new ToolStripSeparator());
                _filterManager.AddFilterContextMenuItems(menu, col, location);
            }

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
                Padding = new Padding(4),
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
                UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
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
            UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
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
        private void LoadReport()
        {
            Cursor prev = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DETAILED";

                DatewisePurchaseReportFilter filter = new DatewisePurchaseReportFilter
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
                    ViewMode = viewMode
                };

                _reportRows = _repository.GetReport(filter);

                gridReport.DataSource = null;
                gridReport.DataSource = _reportRows;

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues(_reportRows);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Date-wise Purchase Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
                column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            ConfigureGridColumn(band, "PurchaseDate", "Purchase Date", 105, "dd-MMM-yyyy", HAlign.Left, 0);
            ConfigureGridColumn(band, "PurchaseNo", "Purchase No", 95, null, HAlign.Left, 1);
            ConfigureGridColumn(band, "InvoiceNo", "Invoice No", 110, null, HAlign.Left, 2);
            ConfigureGridColumn(band, "InvoiceDate", "Invoice Date", 100, "dd-MMM-yyyy", HAlign.Left, 3);
            ConfigureGridColumn(band, "VendorName", "Vendor Name", 200, null, HAlign.Left, 4);
            ConfigureGridColumn(band, "BarCode", "Barcode", 115, null, HAlign.Left, 5);
            ConfigureGridColumn(band, "ItemName", "Item Description", 230, null, HAlign.Left, 6);
            ConfigureGridColumn(band, "Unit", "Unit", 65, null, HAlign.Center, 7);
            ConfigureGridColumn(band, "Qty", "Qty", 80, "#,##0.00", HAlign.Right, 8);
            ConfigureGridColumn(band, "Free", "Free Qty", 75, "#,##0.00", HAlign.Right, 9);
            ConfigureGridColumn(band, "TotalQty", "Total Qty", 85, "#,##0.00", HAlign.Right, 10);
            ConfigureGridColumn(band, "Cost", "Purchase Rate", 95, "#,##0.00", HAlign.Right, 11);
            ConfigureGridColumn(band, "Amount", "Amount", 105, "#,##0.00", HAlign.Right, 12);
            ConfigureGridColumn(band, "TaxAmt", "Tax Amount", 90, "#,##0.00", HAlign.Right, 13);
            ConfigureGridColumn(band, "NetAmount", "Net Amount", 110, "#,##0.00", HAlign.Right, 14);
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

            ReportGridLayoutHelper.ApplySavedLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(gridReport.DataSource as IList<DatewisePurchaseReportRow>);
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Band.Index == 0)
            {
                if (e.Row.Cells.Exists("NetAmount"))
                {
                    e.Row.Cells["NetAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    e.Row.Cells["NetAmount"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                }
            }
        }
        #endregion

        #region Actions & Exports
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
            List<DatewisePurchaseReportRow> rows = gridReport.DataSource as List<DatewisePurchaseReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Date-wise Purchase Report - Preview";
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
                    Text = "DATE-WISE PURCHASE REPORT",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"Generated On: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Mode: {ultraComboPeriodMode.Text} | Total Records: {rows.Count}",
                    ForeColor = Color.FromArgb(220, 235, 255),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(subtitleLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.DataSource = rows;

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
            List<DatewisePurchaseReportRow> rows = gridReport.DataSource as List<DatewisePurchaseReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                dialog.FileName = $"DatewisePurchaseReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Date-wise Purchase Report")) return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("PurchaseDate,PurchaseNo,InvoiceNo,InvoiceDate,VendorName,Barcode,ItemName,Unit,Qty,FreeQty,TotalQty,Rate,Amount,TaxAmount,NetAmount,PayMode,BilledBy");

                var visibleRows = GetVisibleDataRows().Select(r => r.ListObject as DatewisePurchaseReportRow).Where(r => r != null).ToList();
                foreach (var row in visibleRows)
                {
                    builder.AppendLine(string.Join(",",
                        row.PurchaseDate.ToString("yyyy-MM-dd"),
                        row.PurchaseNo.ToString(),
                        EscapeCsv(row.InvoiceNo),
                        row.InvoiceDate.ToString("yyyy-MM-dd"),
                        EscapeCsv(row.VendorName),
                        EscapeCsv(row.BarCode),
                        EscapeCsv(row.ItemName),
                        EscapeCsv(row.Unit),
                        row.Qty.ToString("F2"),
                        row.Free.ToString("F2"),
                        row.TotalQty.ToString("F2"),
                        row.Cost.ToString("F2"),
                        row.Amount.ToString("F2"),
                        row.TaxAmt.ToString("F2"),
                        row.NetAmount.ToString("F2"),
                        EscapeCsv(row.Paymode),
                        EscapeCsv(row.BilledBy)
                    ));
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
                ultraComboPeriodMode.Value = "DATE_RANGE";
                dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtTo.Value = DateTime.Today;
                ultraComboMonth.Value = DateTime.Today.Month;
                ultraComboYear.Value = DateTime.Today.Year;
                ultraComboViewMode.Value = "DETAILED";
                txtItemSearch.Text = string.Empty;
                ultraComboVendor.Value = 0;
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }
            LoadReport();
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooserForm();
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
                using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmdatewisepurchasereport"))
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

        private void btnVendorPicker_Click(object sender, EventArgs e)
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
            catch
            {
                ultraComboVendor.Focus();
            }
        }
        #endregion
    }
}
