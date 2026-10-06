using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
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
    public partial class frmSalesmanwiseSalesSummaryReport : Form
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
        private SalesmanwiseSalesSummaryRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _isChangingDates = false;
        private int? _selectedSalesmanId = null;
        private string _selectedSalesmanName = string.Empty;

        private List<SalesmanwiseSalesSummaryItem> _allRows = new List<SalesmanwiseSalesSummaryItem>();

        // Dynamic footer panel and cell controls (exact frmPurchaseReturn implementation)
        private UltraPanel gridFooterPanel;
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "InvoiceCount",
            "TotalQtySold",
            "TotalSalesAmount",
            "CommissionPercent",
            "CommissionAmount"
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

        public frmSalesmanwiseSalesSummaryReport()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 9F);
            InitializeBackgroundWorker();
            InitializeForm();
        }

        private void FrmSalesmanwiseSalesSummaryReport_Load(object sender, EventArgs e)
        {
            FetchFromDatabase();
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

                _reportRepo = new SalesmanwiseSalesSummaryRepo();
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

                // Initialize dates
                InitializeDateControls();

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
                btnSelectSalesman.Click  += BtnSelectSalesman_Click;
                btnClearSalesman.Click   += BtnClearSalesman_Click;
                comboPeriod.ValueChanged += ComboPeriod_ValueChanged;
                dtFrom.ValueChanged      += DtDate_ValueChanged;
                dtTo.ValueChanged        += DtDate_ValueChanged;
                txtSearch.TextChanged    += TxtSearch_TextChanged;
                numCommissionPercent.ValueChanged += NumCommissionPercent_ValueChanged;

                // Handle keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += FrmSalesmanwiseSalesSummaryReport_KeyDown;

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

        private void InitializeDateControls()
        {
            _isChangingDates = true;
            try
            {
                DateTime now = DateTime.Today;
                dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                dtTo.Value   = now;
            }
            finally
            {
                _isChangingDates = false;
            }
        }

        #region UI Theming & Grid Styling (Parity with frmPurchaseReturn / frmStockReport)
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
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
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

            layout.Override.HeaderAppearance.BackColor  = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor  = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;

            layout.Override.RowAppearance.BackColor          = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor        = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor    = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor    = Color.FromArgb(10, 31, 79);
            layout.Override.ActiveRowAppearance.BorderColor  = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor  = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor  = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.BorderColor       = GridRowLine;
            layout.Override.CellAppearance.ForeColor         = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name     = "Segoe UI";
            layout.Override.CellAppearance.FontData.SizeInPoints = 9F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell   = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow    = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight      = 24;
            layout.Override.DefaultRowHeight  = 24;
            layout.RowConnectorStyle  = RowConnectorStyle.Solid;
            layout.RowConnectorColor  = GridRowLine;

            // Scrollbar look
            layout.ScrollBarLook.Appearance.BackColor  = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor  = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            // Filter row styling
            layout.Override.FilterRowAppearance.BackColor   = Color.FromArgb(255, 255, 230);
            layout.Override.FilterRowAppearance.ForeColor   = Color.FromArgb(33, 33, 33);
            layout.Override.FilterRowPromptAppearance.ForeColor = Color.FromArgb(140, 140, 140);

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            // Attach grid event handlers
            gridReport.InitializeLayout     += GridReport_InitializeLayout;
            gridReport.AfterRowRegionScroll += (s, ev) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged   += (s, ev) => { UpdateFooterCellPositions(); UpdateFooterValues(); };
            gridReport.Resize               += (s, ev) => UpdateFooterCellPositions();
            gridReport.SizeChanged          += (s, ev) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, ev) => { UpdateFooterValues(); UpdateFooterCellPositions(); };
            gridReport.AfterSortChange      += (s, ev) => UpdateFooterValues();
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            UltraGridBand band = e.Layout.Bands[0];

            var columns = new[]
            {
                ("SlNo",              "S.No",               60,  HAlign.Center),
                ("SalesmanName",      "Salesman Name",     180,  HAlign.Left),
                ("Email",             "Email Address",     180,  HAlign.Left),
                ("InvoiceCount",      "Invoice Count",     100,  HAlign.Right),
                ("TotalQtySold",      "Total Qty Sold",    110,  HAlign.Right),
                ("TotalSalesAmount",  "Total Sales",       130,  HAlign.Right),
                ("CommissionPercent", "Comm. %",           90,   HAlign.Right),
                ("CommissionAmount",  "Comm. Earned",      130,  HAlign.Right)
            };

            int index = 0;
            foreach (var col in columns)
            {
                if (band.Columns.Exists(col.Item1))
                {
                    UltraGridColumn bandCol = band.Columns[col.Item1];
                    bandCol.Header.Caption = col.Item2;
                    bandCol.Width          = col.Item3;
                    bandCol.CellAppearance.TextHAlign = col.Item4;
                    bandCol.Header.VisiblePosition = index++;

                    if (col.Item1 == "SlNo" || col.Item1 == "InvoiceCount")
                    {
                        bandCol.Format = "N0";
                    }
                    else if (col.Item1 == "TotalQtySold" || col.Item1 == "TotalSalesAmount" || 
                             col.Item1 == "CommissionPercent" || col.Item1 == "CommissionAmount")
                    {
                        bandCol.Format = "N2";
                    }
                }
            }

            foreach (UltraGridColumn col in band.Columns)
            {
                bool isPlanned = false;
                foreach (var pc in columns)
                {
                    if (pc.Item1 == col.Key) { isPlanned = true; break; }
                }
                if (!isPlanned) col.Hidden = true;
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void StyleFilterControls()
        {
            if (panelFilters != null)
            {
                panelFilters.UseAppStyling = false;
                panelFilters.UseOsThemes = DefaultableBoolean.False;
                panelFilters.Appearance.BackColor = FilterPanelBackColor;
                panelFilters.Appearance.BorderColor = BorderBlue;
            }

            if (panelHeader != null)
            {
                panelHeader.UseAppStyling = false;
                panelHeader.UseOsThemes = DefaultableBoolean.False;
                panelHeader.Appearance.BackColor = Color.White;
                panelHeader.Appearance.BorderColor = BorderBlue;
            }

            if (lblTitle != null)
            {
                lblTitle.Appearance.ForeColor = ControlTextColor;
                lblTitle.Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnReset);
            StyleClassicButton(btnExport);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnClose);
            StyleClassicButton(btnSelectSalesman);
            StyleClassicButton(btnClearSalesman);
        }

        private static void StyleClassicButton(UltraButton btn)
        {
            if (btn == null) return;
            btn.UseAppStyling  = false;
            btn.UseOsThemes    = DefaultableBoolean.False;
            btn.ButtonStyle    = UIElementButtonStyle.Office2013Button;
            btn.UseFlatMode    = DefaultableBoolean.False;
            btn.Appearance.BackColor  = ButtonBlueTop;
            btn.Appearance.BackColor2 = ButtonBlueBottom;
            btn.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btn.Appearance.ForeColor   = ButtonTextBlue;
            btn.Appearance.BorderColor = ButtonLightOutline;
            btn.Appearance.TextHAlign  = HAlign.Center;
            btn.Appearance.TextVAlign  = VAlign.Middle;
            btn.Appearance.FontData.Bold = DefaultableBoolean.False;
            btn.Appearance.FontData.SizeInPoints = 9;
            btn.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn.HotTrackAppearance.BackColor  = Color.FromArgb(241, 247, 254);
            btn.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            btn.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.HotTrackAppearance.BorderColor = ButtonLightOutline;
            btn.HotTrackAppearance.ForeColor   = ButtonTextBlue;
            btn.PressedAppearance.BackColor  = Color.FromArgb(118, 161, 214);
            btn.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            btn.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            btn.PressedAppearance.ForeColor   = ButtonTextBlue;
        }

        private void LayoutSummaryCards()
        {
            if (panelSummary == null) return;
            panelSummary.UseAppStyling = false;
            panelSummary.UseOsThemes   = DefaultableBoolean.False;
            panelSummary.Appearance.BackColor  = FormBackColor;
            panelSummary.Appearance.BorderColor = BorderBlue;

            SetupSummaryCard(cardSalesmanCount,   lblSalesmanCountCaption,   lblSalesmanCountValue,   "TOTAL SALESMEN",       "0",    Color.FromArgb(25, 118, 210));
            SetupSummaryCard(cardInvoiceCount,    lblInvoiceCountCaption,    lblInvoiceCountValue,    "TOTAL INVOICES",       "0",    Color.FromArgb(103, 58, 183));
            SetupSummaryCard(cardTotalQty,        lblTotalQtyCaption,        lblTotalQtyValue,        "TOTAL QTY SOLD",       "0.00", Color.FromArgb(245, 124, 0));
            SetupSummaryCard(cardTotalSales,      lblTotalSalesCaption,      lblTotalSalesValue,      "TOTAL SALES AMOUNT",   "0.00", Color.FromArgb(0, 150, 136));
            SetupSummaryCard(cardTotalCommission, lblTotalCommissionCaption, lblTotalCommissionValue, "TOTAL COMMISSION",     "0.00", Color.FromArgb(211, 47, 47));

            ReflowSummaryCards();
            panelSummary.Resize += (s, e) => ReflowSummaryCards();
        }

        private void SetupSummaryCard(UltraPanel card, UltraLabel lblCap, UltraLabel lblVal, string caption, string initialVal, Color accent)
        {
            if (card == null) return;
            card.UseAppStyling = false;
            card.UseOsThemes   = DefaultableBoolean.False;
            card.Appearance.BackColor  = Color.White;
            card.Appearance.BorderColor = Color.FromArgb(200, 218, 237);
            card.BorderStyle = UIElementBorderStyle.Solid;

            lblCap.Text = caption;
            lblCap.UseAppStyling = false;
            lblCap.UseOsThemes   = DefaultableBoolean.False;
            lblCap.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            lblCap.Appearance.FontData.Name = "Segoe UI";
            lblCap.Appearance.FontData.SizeInPoints = 8F;
            lblCap.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblCap.Location = new Point(12, 6);
            lblCap.Size     = new Size(180, 16);

            lblVal.Text = initialVal;
            lblVal.UseAppStyling = false;
            lblVal.UseOsThemes   = DefaultableBoolean.False;
            lblVal.Appearance.ForeColor = accent;
            lblVal.Appearance.FontData.Name = "Segoe UI";
            lblVal.Appearance.FontData.SizeInPoints = 14F;
            lblVal.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblVal.Location = new Point(12, 24);
            lblVal.Size     = new Size(180, 28);
        }

        private void ReflowSummaryCards()
        {
            if (panelSummary == null) return;
            int totalWidth = panelSummary.ClientArea.Width;
            if (totalWidth < 600) return;

            UltraPanel[] cards = { cardSalesmanCount, cardInvoiceCount, cardTotalQty, cardTotalSales, cardTotalCommission };
            int spacing = 10;
            int leftMargin = 12;
            int rightMargin = 12;
            int usableWidth = totalWidth - leftMargin - rightMargin - (spacing * (cards.Length - 1));
            int cardWidth = Math.Max(120, usableWidth / cards.Length);

            int currentLeft = leftMargin;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(currentLeft, 8);
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
            if (decimal.TryParse(value.ToString().Replace("₹", "").Trim(), out result))
                return result;

            return null;
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            bool isCurrency = columnKey == "TotalSalesAmount" || columnKey == "CommissionAmount";

            if (aggregation == "Count")
                return result.ToString();

            if (result is decimal decValue)
            {
                string numStr = isCurrency ? ("₹" + decValue.ToString("N2")) : decValue.ToString("N2");

                if (aggregation == "Avg" || aggregation == "Average") return $"Avg: {numStr}";
                if (aggregation == "Min") return $"Min: {numStr}";
                if (aggregation == "Max") return $"Max: {numStr}";
                return numStr;
            }

            if (result is DateTime dtValue)
            {
                return dtValue.ToString("dd-MM-yyyy");
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
            gridReport.MouseUp   += GridReport_MouseUp;
            gridReport.DragOver  += GridReport_DragOver;
            gridReport.DragDrop  += GridReport_DragDrop;
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
                if (col.Hidden && !string.Equals(col.Key, "SalesmanId", StringComparison.OrdinalIgnoreCase))
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
                if (!string.Equals(col.Key, "SalesmanId", StringComparison.OrdinalIgnoreCase))
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

            var filter = new SalesmanwiseSalesSummaryFilter
            {
                CompanyId   = SessionContext.CompanyId,
                BranchId    = SessionContext.BranchId,
                FinYearId   = SessionContext.FinYearId,
                FromDate    = dtFrom.DateTime.Date,
                ToDate      = dtTo.DateTime.Date,
                SalesmanId  = _selectedSalesmanId,
                SearchQuery = txtSearch.Text.Trim()
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filter = (SalesmanwiseSalesSummaryFilter)e.Argument;
            e.Result = _reportRepo.GetSalesmanwiseSalesSummary(filter);
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

                _allRows = e.Result as List<SalesmanwiseSalesSummaryItem> ?? new List<SalesmanwiseSalesSummaryItem>();
                RecalculateCommissions();
                FilterFetchedRows();
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                {
                    btnSearch.Enabled = true;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void RecalculateCommissions()
        {
            double commRate = Convert.ToDouble(numCommissionPercent.Value);
            foreach (var item in _allRows)
            {
                item.CommissionPercent = commRate;
                item.CommissionAmount  = item.TotalSalesAmount * (commRate / 100.0);
            }
        }

        private void FilterFetchedRows()
        {
            string searchVal = txtSearch.Text.Trim().ToLower();
            List<SalesmanwiseSalesSummaryItem> filtered;

            if (string.IsNullOrEmpty(searchVal))
            {
                filtered = _allRows;
            }
            else
            {
                filtered = _allRows.Where(r => 
                    (r.SalesmanName != null && r.SalesmanName.ToLower().Contains(searchVal)) ||
                    (r.Email != null && r.Email.ToLower().Contains(searchVal))
                ).ToList();
            }

            gridReport.DataSource = filtered;
            ApplyUserHiddenColumns();
            CalculateSummaryValues(filtered);
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
            lblStatus.Text = $"Ready  |  Found {filtered.Count} records.";
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

        private void CalculateSummaryValues(List<SalesmanwiseSalesSummaryItem> items)
        {
            if (items == null || items.Count == 0)
            {
                lblSalesmanCountValue.Text   = "0";
                lblInvoiceCountValue.Text    = "0";
                lblTotalQtyValue.Text        = "0.00";
                lblTotalSalesValue.Text      = "₹0.00";
                lblTotalCommissionValue.Text = "₹0.00";
                return;
            }

            var uniqueSalesmanCount = items.Select(i => i.SalesmanId).Distinct().Count();
            var totalInvoices       = items.Sum(i => i.InvoiceCount);
            var totalQty            = items.Sum(i => i.TotalQtySold);
            var totalSales          = items.Sum(i => i.TotalSalesAmount);
            var totalCommission     = items.Sum(i => i.CommissionAmount);

            lblSalesmanCountValue.Text   = uniqueSalesmanCount.ToString("N0");
            lblInvoiceCountValue.Text    = totalInvoices.ToString("N0");
            lblTotalQtyValue.Text        = totalQty.ToString("N2");
            lblTotalSalesValue.Text      = "₹" + totalSales.ToString("N2");
            lblTotalCommissionValue.Text = "₹" + totalCommission.ToString("N2");
        }
        #endregion

        #region Event Handlers
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
                        dtFrom.Value = now.AddDays(-1 * diff).Date;
                        dtTo.Value   = now;
                        break;
                    case "This Month":
                        dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                        dtTo.Value   = now;
                        break;
                    case "Last Month":
                        DateTime lastMonthFirst = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                        DateTime lastMonthLast  = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                        dtFrom.Value = lastMonthFirst;
                        dtTo.Value   = lastMonthLast;
                        break;
                    case "This Quarter":
                        int quarterNumber = (now.Month - 1) / 3 + 1;
                        DateTime firstDayOfQuarter = new DateTime(now.Year, (quarterNumber - 1) * 3 + 1, 1);
                        dtFrom.Value = firstDayOfQuarter;
                        dtTo.Value   = now;
                        break;
                    case "This Year":
                        dtFrom.Value = new DateTime(now.Year, 1, 1);
                        dtTo.Value   = now;
                        break;
                    case "Custom":
                    default:
                        break;
                }
            }
            finally
            {
                _isChangingDates = false;
            }

            if (period != "Custom")
            {
                FetchFromDatabase();
            }
        }

        private void DtDate_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangingDates) return;
            _isChangingDates = true;
            try
            {
                comboPeriod.Value = "Custom";
            }
            finally
            {
                _isChangingDates = false;
            }
        }

        private void BtnSelectSalesman_Click(object sender, EventArgs e)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmSalesPersonDial())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.OnSalesPersonSelected += (name) =>
                {
                    _selectedSalesmanName = name ?? string.Empty;
                    txtSalesmanName.Text  = _selectedSalesmanName;
                    FetchFromDatabase();
                };
                dlg.ShowDialog(this);
            }
        }

        private void BtnClearSalesman_Click(object sender, EventArgs e)
        {
            _selectedSalesmanId   = null;
            _selectedSalesmanName = string.Empty;
            txtSalesmanName.Text  = string.Empty;
            FetchFromDatabase();
        }

        private void BtnSearch_Click(object sender, EventArgs e) => FetchFromDatabase();

        public void RibbonClear() => BtnReset_Click(this, EventArgs.Empty);
        public void Clear() => BtnReset_Click(this, EventArgs.Empty);

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _selectedSalesmanId        = null;
            _selectedSalesmanName      = string.Empty;
            txtSalesmanName.Text       = string.Empty;
            txtSearch.Text             = string.Empty;
            numCommissionPercent.Value = 5.00D;

            InitializeDateControls();
            comboPeriod.Value = "This Month";

            FetchFromDatabase();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => FilterFetchedRows();

        private void NumCommissionPercent_ValueChanged(object sender, EventArgs e)
        {
            RecalculateCommissions();
            FilterFetchedRows();
        }

        private void FrmSalesmanwiseSalesSummaryReport_KeyDown(object sender, KeyEventArgs e)
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
        #endregion

        #region CSV Export & Print Operations
        private void BtnExport_Click(object sender, EventArgs e)
        {
            var rows = gridReport.DataSource as List<SalesmanwiseSalesSummaryItem>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var saveDlg = new SaveFileDialog())
                {
                    saveDlg.Filter = "CSV Files (*.csv)|*.csv";
                    saveDlg.FileName = $"SalesmanwiseSalesSummary_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDlg.ShowDialog() != DialogResult.OK) return;

                    var sb = new StringBuilder();
                    sb.AppendLine("S.No,Salesman Name,Email Address,Invoice Count,Total Qty Sold,Total Sales,Commission %,Commission Earned");

                    foreach (var r in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(r.SlNo.ToString()),
                            CsvCell(r.SalesmanName),
                            CsvCell(r.Email),
                            r.InvoiceCount.ToString(),
                            r.TotalQtySold.ToString("F2"),
                            r.TotalSalesAmount.ToString("F2"),
                            r.CommissionPercent.ToString("F2"),
                            r.CommissionAmount.ToString("F2")
                        ));
                    }

                    sb.AppendLine();
                    sb.AppendLine(string.Join(",",
                        "",
                        "TOTALS",
                        "",
                        rows.Sum(r => r.InvoiceCount).ToString(),
                        rows.Sum(r => r.TotalQtySold).ToString("F2"),
                        rows.Sum(r => r.TotalSalesAmount).ToString("F2"),
                        "",
                        rows.Sum(r => r.CommissionAmount).ToString("F2")
                    ));

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

        private string CsvCell(string val)
        {
            if (string.IsNullOrEmpty(val)) return "\"\"";
            if (val.Contains(",") || val.Contains("\"") || val.Contains("\n") || val.Contains("\r"))
            {
                return "\"" + val.Replace("\"", "\"\"") + "\"";
            }
            return "\"" + val + "\"";
        }
        #endregion
    }
}
