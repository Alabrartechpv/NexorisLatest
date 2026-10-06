using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmItemwiseSalesSummaryReport : Form
    {
        #region Colour Palette (exact matches frmPurchaseReturn / frmStockReport theme)
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(245, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop        = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom     = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);
        #endregion

        #region Fields
        private ItemwiseSalesSummaryRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _isChangingDates = false;
        private bool _accentPanelsCreated = false;

        private List<ItemwiseSalesSummaryItem> _allRows = new List<ItemwiseSalesSummaryItem>();

        // Dynamic footer panel and cell controls (exact frmPurchaseReturn implementation)
        private UltraPanel gridFooterPanel;
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalQtySold",
            "AvgUnitPrice",
            "TotalSalesAmount",
            "TotalCostValue",
            "TotalMarginProfit",
            "MarginPercent"
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

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString() => DisplayText;
        }
        #endregion

        public frmItemwiseSalesSummaryReport()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 9F);
            InitializeBackgroundWorker();
            InitializeForm();
        }

        private void InitializeBackgroundWorker()
        {
            _searchWorker = new BackgroundWorker();
            _searchWorker.WorkerSupportsCancellation = true;
            _searchWorker.DoWork += SearchWorker_DoWork;
            _searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        }

        private void InitializeForm()
        {
            try
            {
                this.BackColor = FormBackColor;
                panelHeader.Appearance.BackColor = Color.White;
                lblTitle.Appearance.ForeColor    = Color.FromArgb(18, 49, 102);
                lblSubtitle.Appearance.ForeColor = Color.FromArgb(70, 90, 120);

                _reportRepo = new ItemwiseSalesSummaryRepo();
                _dropdownRepo = new Dropdowns();

                // Setup date presets
                comboPeriod.Items.Clear();
                comboPeriod.Items.Add("Today", "Today");
                comboPeriod.Items.Add("This Week", "This Week");
                comboPeriod.Items.Add("This Month", "This Month");
                comboPeriod.Items.Add("Last Month", "Last Month");
                comboPeriod.Items.Add("This Quarter", "This Quarter");
                comboPeriod.Items.Add("This Year", "This Year");
                comboPeriod.Items.Add("Custom", "Custom Range");
                comboPeriod.Value = "This Month";

                // Setup stock filters
                comboStockFilter.Items.Clear();
                comboStockFilter.Items.Add("All Items", "All Items");
                comboStockFilter.Items.Add("High Profit (>30%)", "High Profit (>30%)");
                comboStockFilter.Items.Add("Low Profit (<10%)", "Low Profit (<10%)");
                comboStockFilter.Items.Add("Top Sold (>100 Qty)", "Top Sold (>100 Qty)");
                comboStockFilter.Value = "All Items";

                // Load initial dropdown list choices
                LoadGroups();
                LoadCategories();

                // Configure grid properties
                StyleGrid();

                // Style filter controls
                StyleFilterControls();

                // Style buttons
                StyleButtons();

                // Apply dynamic layout coordinates and colors on card panels
                LayoutSummaryCards();

                // Footer panel & Drag-to-hide setup
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();

                // Wire event triggers
                btnSearch.Click          += BtnSearch_Click;
                btnReset.Click           += BtnReset_Click;
                btnExport.Click          += BtnExport_Click;
                btnPrint.Click           += BtnPrint_Click;
                btnClose.Click           += BtnClose_Click;
                comboPeriod.ValueChanged += ComboPeriod_ValueChanged;
                dtFrom.ValueChanged      += DtDate_ValueChanged;
                dtTo.ValueChanged        += DtDate_ValueChanged;
                txtSearch.TextChanged    += TxtSearch_TextChanged;

                // Handle keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += FrmItemwiseSalesSummaryReport_KeyDown;

                this.FormClosing += (s, ev) =>
                {
                    if (_searchWorker != null && _searchWorker.IsBusy)
                        _searchWorker.CancelAsync();

                    if (columnChooserForm != null && !columnChooserForm.IsDisposed)
                    {
                        columnChooserForm.Dispose();
                        columnChooserForm = null;
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGroups()
        {
            try
            {
                var gps = _dropdownRepo.getGroupDDl();
                if (gps?.List != null)
                {
                    comboGroup.DataSource    = gps.List.ToList();
                    comboGroup.ValueMember   = "Id";
                    comboGroup.DisplayMember = "GroupName";
                }
            }
            catch { }
        }

        private void LoadCategories()
        {
            try
            {
                var cats = _dropdownRepo.getCategoryDDl(null);
                if (cats?.List != null)
                {
                    comboCategory.DataSource    = cats.List.ToList();
                    comboCategory.ValueMember   = "Id";
                    comboCategory.DisplayMember = "CategoryName";
                }
            }
            catch { }
        }

        private void StyleFilterControls()
        {
            panelFilters.Appearance.BackColor = FilterPanelBackColor;
            panelFilters.Appearance.BorderColor = BorderBlue;

            StyleFilterCombo(comboPeriod);
            StyleFilterCombo(comboGroup);
            StyleFilterCombo(comboCategory);
            StyleFilterCombo(comboStockFilter);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleTextEditor(txtSearch);
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = BorderBlue;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Segoe UI";
            combo.Appearance.FontData.SizeInPoints = 9;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtEditor)
        {
            if (dtEditor == null) return;
            dtEditor.UseAppStyling = false;
            dtEditor.UseOsThemes = DefaultableBoolean.False;
            dtEditor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            dtEditor.BorderStyle = UIElementBorderStyle.Solid;
            dtEditor.Appearance.BackColor = ControlBackColor;
            dtEditor.Appearance.BorderColor = BorderBlue;
            dtEditor.Appearance.ForeColor = ControlTextColor;
            dtEditor.Appearance.FontData.Name = "Segoe UI";
            dtEditor.Appearance.FontData.SizeInPoints = 9;
            dtEditor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = BorderBlue;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 9;
        }

        private void StyleGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes   = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.AutoFitStyle = AutoFitStyle.None;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.Scrollbars = Scrollbars.Both;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle    = UIElementBorderStyle.Solid;

            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew       = AllowAddNew.No;
            layout.Override.AllowDelete       = DefaultableBoolean.False;
            layout.Override.AllowUpdate       = DefaultableBoolean.False;
            layout.Override.AllowColMoving    = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing    = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType      = FilterUIType.FilterRow;
            layout.Override.CellClickAction   = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow     = SelectType.Single;
            layout.Override.RowSelectors      = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth  = 35;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor  = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;

            layout.Override.RowSelectorAppearance.BackColor  = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor   = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign  = HAlign.Center;

            layout.Override.HeaderStyle = HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor  = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor  = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;

            layout.Override.RowAppearance.BackColor          = Color.White;
            layout.Override.RowAppearance.ForeColor          = Color.FromArgb(10, 31, 79);
            layout.Override.RowAppearance.BorderColor        = GridRowLine;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor    = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor    = Color.FromArgb(10, 31, 79);
            layout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.SelectedRowAppearance.BackColor  = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor  = Color.FromArgb(10, 31, 79);
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.CellAppearance.BorderColor       = GridRowLine;
            layout.Override.CellAppearance.ForeColor         = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name     = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.CellAppearance.TextVAlign        = VAlign.Middle;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell   = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow    = UIElementBorderStyle.Solid;
            layout.Override.DefaultRowHeight  = 22;
            layout.Override.CellPadding       = 2;
            layout.Override.CellSpacing       = 0;
            layout.Override.RowSpacingBefore  = 0;
            layout.Override.RowSpacingAfter   = 0;

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow    += GridReport_InitializeRow;

            // Footer synchronization events
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
                UpdateSummaryCardValues();
            };
            gridReport.AfterSortChange += (s, e) => UpdateFooterValues();
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            var band = e.Layout.Bands[0];

            // Define column order and captions
            var columns = new[]
            {
                ("Barcode",           "Barcode",            95,  HAlign.Left),
                ("ItemName",          "Product Name",      230,  HAlign.Left),
                ("GroupName",         "Group",             120,  HAlign.Left),
                ("CategoryName",      "Category",          120,  HAlign.Left),
                ("BaseUnitName",      "Unit",               70,  HAlign.Center),
                ("TotalQtySold",      "Qty Sold",           95,  HAlign.Right),
                ("AvgUnitPrice",      "Avg Price",         105,  HAlign.Right),
                ("TotalSalesAmount",  "Sales Amount",      130,  HAlign.Right),
                ("TotalCostValue",    "Cost Value",        130,  HAlign.Right),
                ("TotalMarginProfit", "Profit Margin",     130,  HAlign.Right),
                ("MarginPercent",     "Margin %",           95,  HAlign.Right),
            };

            int pos = 0;
            foreach (var (key, caption, width, align) in columns)
            {
                if (!band.Columns.Exists(key)) continue;
                var col = band.Columns[key];
                col.Header.Caption = caption;
                col.Width          = width;
                col.Header.VisiblePosition = pos++;
                col.CellAppearance.TextHAlign = align;
                col.CellAppearance.TextVAlign = VAlign.Middle;

                if (key == "TotalQtySold")
                {
                    col.Format = "#,##0.00";
                }
                else if (key == "AvgUnitPrice" || key == "TotalSalesAmount" || key == "TotalCostValue" || key == "TotalMarginProfit")
                {
                    col.Format = "₹ #,##0.00";
                }
                else if (key == "MarginPercent")
                {
                    col.Format = "#0.00'%'";
                }
            }

            if (band.Columns.Exists("ItemId"))
            {
                band.Columns["ItemId"].Hidden = true;
                band.Columns["ItemId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            ApplyUserHiddenColumns();

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ApplyUserHiddenColumns()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0) return;
            var band = gridReport.DisplayLayout.Bands[0];
            foreach (var key in userHiddenColumnKeys)
            {
                if (band.Columns.Exists(key))
                {
                    band.Columns[key].Hidden = true;
                }
            }
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("TotalMarginProfit") && e.Row.Cells.Exists("MarginPercent"))
            {
                try
                {
                    decimal profit = Convert.ToDecimal(e.Row.Cells["TotalMarginProfit"].Value ?? 0);
                    decimal margin = Convert.ToDecimal(e.Row.Cells["MarginPercent"].Value ?? 0);

                    if (profit < 0 || margin < 0)
                    {
                        e.Row.Cells["TotalMarginProfit"].Appearance.ForeColor = Color.FromArgb(220, 38, 38);
                        e.Row.Cells["MarginPercent"].Appearance.ForeColor     = Color.FromArgb(220, 38, 38);
                        e.Row.Cells["TotalMarginProfit"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["MarginPercent"].Appearance.FontData.Bold     = DefaultableBoolean.True;
                    }
                    else if (margin > 30)
                    {
                        e.Row.Cells["TotalMarginProfit"].Appearance.ForeColor = Color.FromArgb(22, 101, 52);
                        e.Row.Cells["MarginPercent"].Appearance.ForeColor     = Color.FromArgb(22, 101, 52);
                        e.Row.Cells["TotalMarginProfit"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        e.Row.Cells["MarginPercent"].Appearance.FontData.Bold     = DefaultableBoolean.True;
                    }
                }
                catch { }
            }
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnReset);
            StyleClassicButton(btnExport);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnClose);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.Name = "Segoe UI";
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
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

        #region Summary Cards Layout
        private void LayoutSummaryCards()
        {
            panelSummary.Appearance.BackColor = ActionPanelBackColor;
            panelSummary.Appearance.BorderColor = BorderBlue;
            lblStatus.Appearance.ForeColor     = Color.FromArgb(70, 90, 120);

            // Style individual summary cards matching clean palette
            StyleCard(cardItems,     lblItemsCaption,  lblItemsValue,  "PRODUCTS SOLD", "0", Color.FromArgb(37, 99, 235));
            StyleCard(cardQty,       lblQtyCaption,    lblQtyValue,    "TOTAL QTY SOLD", "0.00", Color.FromArgb(13, 148, 136));
            StyleCard(cardCostVal,   lblCostCaption,   lblCostValue,   "TOTAL COST", "₹0.00", Color.FromArgb(217, 119, 6));
            StyleCard(cardRetailVal, lblRetailCaption, lblRetailValue, "TOTAL REVENUE", "₹0.00", Color.FromArgb(79, 70, 229));
            StyleCard(cardProfit,    lblProfitCaption, lblProfitValue, "TOTAL PROFIT", "₹0.00", Color.FromArgb(22, 163, 74));

            this.Resize += (s, e) => ReflowSummaryCards();
            ReflowSummaryCards();
        }

        private void StyleCard(UltraPanel card, UltraLabel lblCap, UltraLabel lblVal, string caption, string initialVal, Color accent)
        {
            card.UseAppStyling = false;
            card.UseOsThemes   = DefaultableBoolean.False;
            card.Appearance.BackColor   = Color.White;
            card.Appearance.BorderColor = Color.FromArgb(203, 213, 225);
            card.BorderStyle            = UIElementBorderStyle.Solid;

            lblCap.Text = caption;
            lblCap.UseAppStyling = false;
            lblCap.UseOsThemes   = DefaultableBoolean.False;
            lblCap.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lblCap.Appearance.FontData.Name = "Segoe UI Semibold";
            lblCap.Appearance.FontData.SizeInPoints = 8.5F;
            lblCap.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblCap.Location = new Point(14, 8);
            lblCap.Size     = new Size(180, 16);

            lblVal.Text = initialVal;
            lblVal.UseAppStyling = false;
            lblVal.UseOsThemes   = DefaultableBoolean.False;
            lblVal.Appearance.ForeColor = accent;
            lblVal.Appearance.FontData.Name = "Segoe UI";
            lblVal.Appearance.FontData.SizeInPoints = 14F;
            lblVal.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblVal.Location = new Point(14, 26);
            lblVal.Size     = new Size(180, 28);
        }

        private void ReflowSummaryCards()
        {
            if (panelSummary == null) return;
            int totalWidth = panelSummary.ClientArea.Width;
            if (totalWidth < 600) return;

            UltraPanel[] cards = { cardItems, cardQty, cardCostVal, cardRetailVal, cardProfit };
            int spacing = 12;
            int leftMargin = 16;
            int rightMargin = 16;
            int usableWidth = totalWidth - leftMargin - rightMargin - (spacing * (cards.Length - 1));
            int cardWidth = Math.Max(120, usableWidth / cards.Length);

            int currentLeft = leftMargin;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(currentLeft, 10);
                cards[i].Size = new Size(cardWidth, 58);
                currentLeft += cardWidth + spacing;
            }
        }
        #endregion

        #region UltraPanelGridFooter Dynamic Alignment & Calculation (Parity with frmPurchaseReturn)
        private void InitializeGridFooter()
        {
            if (gridFooterPanel == null)
            {
                gridFooterPanel = new UltraPanel();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 26;
                gridFooterPanel.UseAppStyling = false;
                gridFooterPanel.UseOsThemes = DefaultableBoolean.False;
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;

                if (panelGrid != null)
                {
                    panelGrid.ClientArea.Controls.Clear();
                    panelGrid.ClientArea.Controls.Add(gridReport);
                    panelGrid.ClientArea.Controls.Add(gridFooterPanel);

                    gridFooterPanel.Dock = DockStyle.Bottom;
                    gridFooterPanel.Height = 26;

                    gridReport.Dock = DockStyle.Fill;
                    gridReport.BringToFront();
                }
            }

            // Set default aggregations
            foreach (var col in summaryDefaultNumericColumns)
            {
                if (!_columnAggregations.ContainsKey(col))
                    _columnAggregations[col] = "Sum";
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (gridFooterPanel == null) return;
            gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? gridReport.DisplayLayout.Override.RowSelectorWidth
                : 0;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = (column.CellAppearance.TextHAlign == HAlign.Right || IsNumericColumn(column))
                        ? ContentAlignment.MiddleRight
                        : ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                gridFooterPanel.ClientArea.Controls.Add(footerLabel);
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
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum") { Tag = "Sum", Enabled = isNumeric };
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min") { Tag = "Min" };
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max") { Tag = "Max" };
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count") { Tag = "Count" };
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average") { Tag = "Avg", Enabled = isNumeric };
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None") { Tag = "None" };
            itemNone.Click += FooterContextMenu_Click;

            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAverage);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);

            menu.Opening += (sender, e) =>
            {
                string currentAggregation = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : "None";

                foreach (ToolStripItem menuItem in menu.Items)
                {
                    if (menuItem is ToolStripMenuItem toolStripMenuItem && toolStripMenuItem.Tag != null)
                    {
                        toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short) ||
                   summaryDefaultNumericColumns.Contains(column.Key);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Owner is ContextMenuStrip menu && menu.Tag != null && item.Tag != null)
            {
                string columnKey = menu.Tag.ToString();
                string aggregation = item.Tag.ToString();
                _columnAggregations[columnKey] = aggregation;
                UpdateFooterValues();
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || gridFooterPanel == null)
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
                footerLabel.Top = 0;
                footerLabel.Height = gridFooterPanel.Height;
                footerLabel.Visible = (left + width > 0 && left < gridFooterPanel.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0) return;

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

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null) yield break;
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return null;

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
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Count(row => HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                case "Average":
                    var values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count > 0 ? (decimal?)values.Average() : null;
                default:
                    return null;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
        }

        private static decimal? GetNumericValue(object value)
        {
            if (!HasCellValue(value))
                return null;

            decimal result;
            if (decimal.TryParse(value.ToString().Replace("₹", "").Replace("%", "").Trim(), out result))
                return result;

            return null;
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            bool isCurrency = columnKey == "AvgUnitPrice" || columnKey == "TotalSalesAmount" ||
                              columnKey == "TotalCostValue" || columnKey == "TotalMarginProfit";
            bool isPercent = columnKey == "MarginPercent";

            if (aggregation == "Count")
                return result.ToString();

            if (result is decimal decValue)
            {
                string numStr;
                if (isCurrency) numStr = "₹" + decValue.ToString("N2");
                else if (isPercent) numStr = decValue.ToString("N2") + "%";
                else numStr = decValue.ToString("N2");

                if (aggregation == "Avg" || aggregation == "Average") return $"Avg: {numStr}";
                if (aggregation == "Min") return $"Min: {numStr}";
                if (aggregation == "Max") return $"Max: {numStr}";
                return numStr;
            }

            return result.ToString();
        }

        private bool IsNumericColumn(UltraGridColumn col)
        {
            if (col == null) return false;
            return summaryDefaultNumericColumns.Contains(col.Key) ||
                   col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                   col.DataType == typeof(int) || col.DataType == typeof(float) ||
                   col.DataType == typeof(long);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser (Parity with frmPurchaseReturn & frmStockReport)
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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
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
                BorderStyle = BorderStyle.FixedSingle
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                ItemHeight = 34,
                IntegralHeight = false
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(txtColumnSearch);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnChooserListBox != null)
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
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            string filterText = txtColumnSearch?.Text?.Trim() ?? "";

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && !string.Equals(col.Key, "ItemId", StringComparison.OrdinalIgnoreCase))
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filterText) || caption.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey);
            }
        }

        private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
        {
            userHiddenColumnKeys.Remove(columnKey);
            if (gridReport.DisplayLayout.Bands.Count > 0 && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
                col.Hidden = false;
                if (targetVisiblePosition.HasValue)
                {
                    col.Header.VisiblePosition = targetVisiblePosition.Value;
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
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (!string.Equals(col.Key, "ItemId", StringComparison.OrdinalIgnoreCase))
                {
                    col.Hidden = false;
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
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
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

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 121, 211)))
            using (GraphicsPath path = RoundedRect(rect, 4))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(item.DisplayText, textFont, textBrush, rect, sf);
                }
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
        #endregion

        #region Asynchronous DB Fetching
        private void FetchFromDatabase()
        {
            if (_searchWorker.IsBusy) return;

            Cursor.Current = Cursors.WaitCursor;
            btnSearch.Enabled = false;
            lblStatus.Text = "Searching database records... Please wait.";

            var filter = new ItemwiseSalesSummaryFilter
            {
                CompanyId       = SessionContext.CompanyId,
                BranchId        = SessionContext.BranchId,
                FinYearId       = SessionContext.FinYearId,
                FromDate        = dtFrom.DateTime.Date,
                ToDate          = dtTo.DateTime.Date,
                GroupId         = comboGroup.Value != null ? Convert.ToInt32(comboGroup.Value) : (int?)null,
                CategoryId      = comboCategory.Value != null ? Convert.ToInt32(comboCategory.Value) : (int?)null,
                BarcodeContains = txtSearch.Text.Trim()
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filter = (ItemwiseSalesSummaryFilter)e.Argument;
            e.Result = _reportRepo.GetItemwiseSalesSummary(filter);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;

            try
            {
                if (e.Cancelled) return;

                if (e.Error != null)
                {
                    MessageBox.Show($"Search failed: {e.Error.Message}", "Database Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Search failed.";
                    return;
                }

                _allRows = (List<ItemwiseSalesSummaryItem>)e.Result ?? new List<ItemwiseSalesSummaryItem>();
                FilterFetchedRows();
            }
            finally
            {
                btnSearch.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }

        private void FilterFetchedRows()
        {
            if (_allRows == null) return;

            var filtered = _allRows.AsEnumerable();
            string search = txtSearch.Text.Trim();

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(r => 
                    (r.Barcode != null && r.Barcode.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.ItemName != null && r.ItemName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.GroupName != null && r.GroupName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.CategoryName != null && r.CategoryName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            string stockOpt = comboStockFilter.Value?.ToString() ?? "All Items";
            if (stockOpt == "High Profit (>30%)")
            {
                filtered = filtered.Where(r => r.MarginPercent > 30);
            }
            else if (stockOpt == "Low Profit (<10%)")
            {
                filtered = filtered.Where(r => r.MarginPercent < 10);
            }
            else if (stockOpt == "Top Sold (>100 Qty)")
            {
                filtered = filtered.Where(r => r.TotalQtySold > 100);
            }

            var resultList = filtered.ToList();
            gridReport.DataSource = null;
            gridReport.DataSource = resultList;

            ApplyUserHiddenColumns();

            UpdateSummaryCardValues();
            UpdateFooterValues();
            UpdateFooterCellPositions();
            PopulateColumnChooserListBox();

            lblStatus.Text = $"Showing {resultList.Count:N0} of {_allRows.Count:N0} product items.";
        }

        private void UpdateSummaryCardValues()
        {
            var visibleRows = GetVisibleDataRows().ToList();
            var items = new List<ItemwiseSalesSummaryItem>();

            foreach (var r in visibleRows)
            {
                if (r.ListObject is ItemwiseSalesSummaryItem item)
                    items.Add(item);
            }

            int count = items.Count;
            decimal totalQty = items.Sum(r => r.TotalQtySold);
            decimal totalCost = items.Sum(r => r.TotalCostValue);
            decimal totalRevenue = items.Sum(r => r.TotalSalesAmount);
            decimal totalProfit = items.Sum(r => r.TotalMarginProfit);

            lblItemsValue.Text  = count.ToString("N0");
            lblQtyValue.Text    = totalQty.ToString("N2");
            lblCostValue.Text   = "₹" + totalCost.ToString("N2");
            lblRetailValue.Text = "₹" + totalRevenue.ToString("N2");
            lblProfitValue.Text = "₹" + totalProfit.ToString("N2");
        }
        #endregion

        #region Action Handlers & Key Events
        public void RibbonClear() => BtnReset_Click(this, EventArgs.Empty);
        public void Clear() => BtnReset_Click(this, EventArgs.Empty);

        private void BtnSearch_Click(object sender, EventArgs e) => FetchFromDatabase();

        private void BtnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            comboGroup.Value = null;
            comboGroup.Text = string.Empty;
            comboCategory.Value = null;
            comboCategory.Text = string.Empty;
            comboStockFilter.Value = "All Items";
            comboPeriod.Value = "This Month";

            FetchFromDatabase();
        }

        private void ComboPeriod_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangingDates) return;
            string period = comboPeriod.Value?.ToString() ?? "This Month";
            DateTime now = DateTime.Today;

            _isChangingDates = true;
            try
            {
                switch (period)
                {
                    case "Today":
                        dtFrom.Value = now;
                        dtTo.Value   = now;
                        break;
                    case "This Week":
                        int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                        dtFrom.Value = now.AddDays(-diff);
                        dtTo.Value   = now;
                        break;
                    case "This Month":
                        dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                        dtTo.Value   = now;
                        break;
                    case "Last Month":
                        var firstDayLastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                        dtFrom.Value = firstDayLastMonth;
                        dtTo.Value   = firstDayLastMonth.AddMonths(1).AddDays(-1);
                        break;
                    case "This Quarter":
                        int qtr = (now.Month - 1) / 3;
                        dtFrom.Value = new DateTime(now.Year, qtr * 3 + 1, 1);
                        dtTo.Value   = now;
                        break;
                    case "This Year":
                        dtFrom.Value = new DateTime(now.Year, 1, 1);
                        dtTo.Value   = now;
                        break;
                }
            }
            finally
            {
                _isChangingDates = false;
            }

            FetchFromDatabase();
        }

        private void DtDate_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangingDates) return;
            _isChangingDates = true;
            try
            {
                if (comboPeriod.Text != "Custom") comboPeriod.Text = "Custom";
            }
            finally
            {
                _isChangingDates = false;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => FilterFetchedRows();

        private void FrmItemwiseSalesSummaryReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                FetchFromDatabase();
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                e.Handled = true;
                btnExport.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                e.Handled = true;
                btnPrint.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (gridReport.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var saveDlg = new SaveFileDialog())
                {
                    saveDlg.Filter = "CSV File (*.csv)|*.csv";
                    saveDlg.FileName = $"ItemwiseSalesSummary_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDlg.ShowDialog(this) != DialogResult.OK) return;

                    var visibleCols = gridReport.DisplayLayout.Bands[0].Columns
                        .Cast<UltraGridColumn>()
                        .Where(c => !c.Hidden && !string.Equals(c.Key, "ItemId", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(c => c.Header.VisiblePosition)
                        .ToList();

                    var sb = new StringBuilder();
                    sb.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption}\"")));

                    foreach (var row in gridReport.Rows.GetFilteredInNonGroupByRows())
                    {
                        var cells = visibleCols.Select(c =>
                        {
                            object v = row.Cells[c.Key].Value;
                            string s = v != null && v != DBNull.Value ? v.ToString() : "";
                            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                            {
                                s = "\"" + s.Replace("\"", "\"\"") + "\"";
                            }
                            return s;
                        });
                        sb.AppendLine(string.Join(",", cells));
                    }

                    File.WriteAllText(saveDlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            gridReport.PrintPreview();
        }

        private void BtnClose_Click(object sender, EventArgs e) => Close();
        #endregion
    }
}
