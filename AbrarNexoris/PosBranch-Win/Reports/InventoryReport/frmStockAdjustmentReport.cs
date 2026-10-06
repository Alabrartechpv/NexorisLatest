using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmStockAdjustmentReport : Form
    {
        // ─── Theme Palette (matches FrmSmartReorderDashboard / frmVendorOutstandingReport) ────────
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
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);

        private static readonly Color ButtonTopColor       = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor    = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor    = Color.FromArgb(73, 119, 184);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private readonly StockAdjustmentReportRepository _repository = new StockAdjustmentReportRepository();
        private BackgroundWorker _searchWorker;
        private List<StockAdjustmentReportRow> _allRows = new List<StockAdjustmentReportRow>();
        private bool _accentPanelsCreated;
        private Infragistics.Win.Misc.UltraLabel lblPeriod;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor comboPeriod;

        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ─── Column Chooser & Drag-Down to Hide State ────────────────────────────────
        private ListBox columnChooserListBox;
        private Form columnChooserForm;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Cursor blackXCursor = CreateBlackXCursor();

        public frmStockAdjustmentReport()
        {
            InitializeComponent();
            Font = new Font("Segoe UI", 9F);
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
            lblPeriod = new Infragistics.Win.Misc.UltraLabel();
            comboPeriod = new Infragistics.Win.UltraWinEditors.UltraComboEditor();

            lblPeriod.Text = "Period:";
            lblPeriod.Location = new Point(12, 20);
            lblPeriod.Size = new Size(45, 20);

            comboPeriod.Location = new Point(58, 16);
            comboPeriod.Size = new Size(95, 25);
            comboPeriod.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;

            comboPeriod.Items.Clear();
            comboPeriod.Items.Add("ALL", "ALL");
            comboPeriod.Items.Add("Today", "Today");
            comboPeriod.Items.Add("Yesterday", "Yesterday");
            comboPeriod.Items.Add("This Week", "This Week");
            comboPeriod.Items.Add("This Month", "This Month");
            comboPeriod.Items.Add("Last Month", "Last Month");
            comboPeriod.Items.Add("This Quarter", "This Quarter");
            comboPeriod.Items.Add("This Year", "This Year");
            comboPeriod.Items.Add("Custom Range", "Custom Range");

            lblFromDate.Text = "From:";
            lblFromDate.Location = new Point(158, 20);
            lblFromDate.Size = new Size(38, 20);

            dtpFromDate.Location = new Point(198, 16);
            dtpFromDate.Size = new Size(100, 25);

            lblToDate.Text = "To:";
            lblToDate.Location = new Point(303, 20);
            lblToDate.Size = new Size(24, 20);

            dtpToDate.Location = new Point(329, 16);
            dtpToDate.Size = new Size(100, 25);

            lblType.Text = "Type:";
            lblType.Location = new Point(434, 20);
            lblType.Size = new Size(36, 20);

            comboType.Location = new Point(472, 16);
            comboType.Size = new Size(90, 25);

            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(567, 20);
            lblSearch.Size = new Size(48, 20);

            txtSearch.Location = new Point(617, 16);
            txtSearch.Size = new Size(110, 25);

            btnSearch.Text = "Search (F5)";
            btnSearch.Location = new Point(735, 15);
            btnSearch.Size = new Size(95, 27);

            btnReset.Location = new Point(836, 15);
            btnReset.Size = new Size(68, 27);

            btnExport.Location = new Point(910, 15);
            btnExport.Size = new Size(105, 27);

            btnPrint.Location = new Point(1021, 15);
            btnPrint.Size = new Size(98, 27);

            btnClose.Location = new Point(1125, 15);
            btnClose.Size = new Size(68, 27);

            panelFilters.ClientArea.Controls.Add(lblPeriod);
            panelFilters.ClientArea.Controls.Add(comboPeriod);

            comboPeriod.ValueChanged += ComboPeriod_ValueChanged;
            comboPeriod.Value = "ALL";

            comboType.Items.Clear();
            comboType.Items.Add("", "All");
            comboType.Items.Add("Stock IN", "Stock IN");
            comboType.Items.Add("Stock OUT", "Stock OUT");
            comboType.Value = "";
            comboType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;

            InitializeRuntimeAppearance();
            StyleGrid();
            StyleButtons();
            SetupCardControls(cardDocCount, lblDocCountCaption, lblDocCountValue, "ADJUSTMENT DOCUMENTS", Color.FromArgb(25, 118, 210));
            SetupCardControls(cardStockIn, lblStockInCaption, lblStockInValue, "TOTAL STOCK IN QTY", Color.FromArgb(0, 150, 136));
            SetupCardControls(cardStockOut, lblStockOutCaption, lblStockOutValue, "TOTAL STOCK OUT QTY", Color.FromArgb(198, 40, 40));
            SetupCardControls(cardNetValue, lblNetValueCaption, lblNetValueValue, "NET ADJUSTMENT VALUE", Color.FromArgb(81, 45, 168));
            LayoutSummaryCards();

            // Register footer cell sync and column drag-to-hide handlers matching frmPurchaseReturn / frmStockReport
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) => UpdateFooterValues();

            SetupHeaderDragToHideAndColumnChooser();
            InitializeGridFooter();

            btnSearch.Click += BtnSearch_Click;
            btnReset.Click += BtnReset_Click;
            btnExport.Click += BtnExport_Click;
            btnPrint.Click += BtnPrint_Click;
            btnClose.Click += BtnClose_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            panelSummary.Resize += (s, e) => LayoutSummaryCards();

            KeyPreview = true;
            KeyDown += FrmStockAdjustmentReport_KeyDown;
            FormClosing += (s, e) =>
            {
                if (_searchWorker != null && _searchWorker.IsBusy)
                    _searchWorker.CancelAsync();
            };
        }

        private void ComboPeriod_ValueChanged(object sender, EventArgs e)
        {
            if (comboPeriod.Value == null) return;
            string val = comboPeriod.Value.ToString();
            DateTime today = DateTime.Today;

            switch (val)
            {
                case "ALL":
                    dtpFromDate.Value = new DateTime(1753, 1, 1);
                    dtpToDate.Value = today;
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "Today":
                    dtpFromDate.Value = today;
                    dtpToDate.Value = today;
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "Yesterday":
                    dtpFromDate.Value = today.AddDays(-1);
                    dtpToDate.Value = today.AddDays(-1);
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "This Week":
                    int daysFromMonday = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dtpFromDate.Value = today.AddDays(-daysFromMonday);
                    dtpToDate.Value = today;
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "This Month":
                    dtpFromDate.Value = new DateTime(today.Year, today.Month, 1);
                    dtpToDate.Value = today;
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "Last Month":
                    DateTime lm = today.AddMonths(-1);
                    dtpFromDate.Value = new DateTime(lm.Year, lm.Month, 1);
                    dtpToDate.Value = dtpFromDate.Value.AddMonths(1).AddDays(-1);
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "This Quarter":
                    int qtrMonth = ((today.Month - 1) / 3) * 3 + 1;
                    dtpFromDate.Value = new DateTime(today.Year, qtrMonth, 1);
                    dtpToDate.Value = today;
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "This Year":
                    dtpFromDate.Value = new DateTime(today.Year, 1, 1);
                    dtpToDate.Value = today;
                    dtpFromDate.Enabled = false;
                    dtpToDate.Enabled = false;
                    break;
                case "Custom Range":
                    dtpFromDate.Enabled = true;
                    dtpToDate.Enabled = true;
                    break;
            }
        }

        private void InitializeRuntimeAppearance()
        {
            BackColor = FormBackColor;

            if (panelFilters != null)
            {
                panelFilters.Appearance.BackColor = FilterPanelBackColor;
                panelFilters.Appearance.BorderColor = BorderBlue;
                panelFilters.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (panelGrid != null)
            {
                panelGrid.Appearance.BackColor = FormBackColor;
                panelGrid.Appearance.BorderColor = BorderBlue;
                panelGrid.BorderStyle = UIElementBorderStyle.Solid;
            }

            // Style Labels
            StyleLabel(lblPeriod);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblType);
            StyleLabel(lblSearch);

            // Style Inputs
            StyleFilterCombo(comboPeriod);
            StyleFilterCombo(comboType);
            StyleTextEditor(txtSearch);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void StyleButtons()
        {
            ApplyButtonStyle(btnSearch);
            ApplyButtonStyle(btnReset);
            ApplyButtonStyle(btnExport);
            ApplyButtonStyle(btnPrint);
            ApplyButtonStyle(btnClose);
        }

        private void ApplyButtonStyle(Infragistics.Win.Misc.UltraButton btn)
        {
            if (btn == null) return;
            btn.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Office2013Button;
            btn.UseAppStyling = false;
            btn.UseOsThemes = DefaultableBoolean.False;
        }

        private void SetupCardControls(Infragistics.Win.Misc.UltraPanel card, Infragistics.Win.Misc.UltraLabel caption, Infragistics.Win.Misc.UltraLabel value, string captionText, Color accentColor)
        {
            if (card == null) return;
            card.Size = new System.Drawing.Size(220, 62);
            card.BorderStyle = UIElementBorderStyle.Solid;
            card.UseAppStyling = false;
            card.UseOsThemes = DefaultableBoolean.False;
            card.Appearance.BackColor = Color.White;
            card.Appearance.BorderColor = Color.FromArgb(226, 232, 240);

            if (caption != null)
            {
                caption.Text = captionText;
                caption.Location = new Point(12, 8);
                caption.Size = new Size(200, 15);
                caption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                caption.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
                caption.UseAppStyling = false;
                caption.UseOsThemes = DefaultableBoolean.False;
            }

            if (value != null)
            {
                value.Location = new Point(12, 26);
                value.Size = new Size(200, 28);
                value.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
                value.Text = "0";
                value.Appearance.ForeColor = accentColor;
                value.UseAppStyling = false;
                value.UseOsThemes = DefaultableBoolean.False;
            }
        }

        private void LayoutSummaryCards()
        {
            if (panelSummary == null) return;
            int totalWidth = panelSummary.ClientArea.Width;
            int cardCount = 4;
            int gap = 12;
            int margin = 16;
            int availableWidth = totalWidth - (margin * 2) - (gap * (cardCount - 1));
            int cardWidth = Math.Max(180, availableWidth / cardCount);
            int cardHeight = 62;
            int y = (panelSummary.ClientArea.Height - cardHeight) / 2;

            Infragistics.Win.Misc.UltraPanel[] cards = new[] { cardDocCount, cardStockIn, cardStockOut, cardNetValue };
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].Location = new Point(margin + i * (cardWidth + gap), Math.Max(6, y));
                    cards[i].Size = new Size(cardWidth, cardHeight);
                }
            }
        }

        private void StyleGrid()
        {
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;
            gridReport.DisplayLayout.Appearance.BackColor = FormBackColor;
            gridReport.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            gridReport.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            gridReport.DisplayLayout.Scrollbars = Scrollbars.Both;
            gridReport.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            gridReport.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            gridReport.DisplayLayout.GroupByBox.Hidden = true;

            gridReport.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            gridReport.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            gridReport.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            gridReport.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            gridReport.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            gridReport.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            gridReport.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            gridReport.DisplayLayout.Override.RowSelectorWidth = 25;
            gridReport.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            gridReport.DisplayLayout.Override.MinRowHeight = 24;
            gridReport.DisplayLayout.Override.DefaultRowHeight = 24;
            gridReport.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            gridReport.DisplayLayout.Override.RowAppearance.ForeColor = ControlTextColor;
            gridReport.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            gridReport.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            gridReport.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            gridReport.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            gridReport.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            gridReport.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            gridReport.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;

            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            gridReport.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            gridReport.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            gridReport.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            gridReport.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            gridReport.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            gridReport.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            gridReport.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            gridReport.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            gridReport.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            gridReport.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            gridReport.DisplayLayout.Override.CellAppearance.ForeColor = ControlTextColor;
            gridReport.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            gridReport.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            gridReport.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;

            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            UltraGridBand band = e.Layout.Bands[0];

            ConfigureColumn(band, "SlNo", "S.No", 55, HAlign.Center, 0, "N0");
            ConfigureColumn(band, "StockAdjustmentNo", "Adj No", 85, HAlign.Center, 1, "N0");
            ConfigureColumn(band, "StockAdjustmentDate", "Date", 105, HAlign.Center, 2, "dd-MM-yyyy");
            ConfigureColumn(band, "AdjustmentType", "Type", 95, HAlign.Center, 3, null);
            ConfigureColumn(band, "Barcode", "Barcode", 110, HAlign.Left, 4, null);
            ConfigureColumn(band, "ItemName", "Item", 220, HAlign.Left, 5, null);
            ConfigureColumn(band, "UnitName", "Unit", 70, HAlign.Center, 6, null);
            ConfigureColumn(band, "SystemStock", "System Stock", 105, HAlign.Right, 7, "N2");
            ConfigureColumn(band, "PhysicalStock", "Physical Stock", 110, HAlign.Right, 8, "N2");
            ConfigureColumn(band, "QtyDifference", "Diff Qty", 95, HAlign.Right, 9, "N2");
            ConfigureColumn(band, "Cost", "Cost", 90, HAlign.Right, 10, "N2");
            ConfigureColumn(band, "AdjustmentValue", "Value", 100, HAlign.Right, 11, "N2");
            ConfigureColumn(band, "Reason", "Reason", 160, HAlign.Left, 12, null);
            ConfigureColumn(band, "LedgerName", "Ledger", 130, HAlign.Left, 13, null);
            ConfigureColumn(band, "UserName", "User", 95, HAlign.Left, 14, null);

            string[] visibleColumns =
            {
                "SlNo", "StockAdjustmentNo", "StockAdjustmentDate", "AdjustmentType", "Barcode", "ItemName",
                "UnitName", "SystemStock", "PhysicalStock", "QtyDifference", "Cost", "AdjustmentValue",
                "Reason", "LedgerName", "UserName"
            };

            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = !visibleColumns.Contains(column.Key);
            }

            // Re-apply user hidden columns
            foreach (UltraGridColumn col in band.Columns)
            {
                if (userHiddenColumnKeys.Contains(col.Key))
                {
                    col.Hidden = true;
                }
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string caption, int width, HAlign align, int position, string format)
        {
            if (!band.Columns.Exists(key)) return;
            UltraGridColumn column = band.Columns[key];
            column.Header.Caption = caption;
            column.Width = width;
            column.CellAppearance.TextHAlign = align;
            column.Header.VisiblePosition = position;
            if (!string.IsNullOrEmpty(format))
                column.Format = format;
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (!e.Row.Cells.Exists("AdjustmentType")) return;
                string type = Convert.ToString(e.Row.Cells["AdjustmentType"].Value);
                if (type == "Stock IN")
                {
                    e.Row.Cells["AdjustmentType"].Appearance.ForeColor = Color.FromArgb(0, 121, 107);
                    e.Row.Cells["AdjustmentType"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                else if (type == "Stock OUT")
                {
                    e.Row.Cells["AdjustmentType"].Appearance.ForeColor = Color.FromArgb(198, 40, 40);
                    e.Row.Cells["AdjustmentType"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
            catch { }
        }

        #region Column Chooser & Drag-Down to Hide

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    {
                        g.FillEllipse(shadowBrush, 5, 5, 24, 24);
                    }

                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 20)))
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
            gridReport.MouseDown += Grid_MouseDown;
            gridReport.MouseMove += Grid_MouseMove;
            gridReport.MouseUp += Grid_MouseUp;
            gridReport.DragOver += Grid_DragOver;
            gridReport.DragDrop += Grid_DragDrop;

            ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerMenu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            headerMenu.Items.Add(showAllItem);

            gridReport.ContextMenuStrip = headerMenu;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            UltraGridColumn col = headerUI?.Header?.Column;
            if (headerUI != null && col != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowHeaderContextMenu(col, e.Location);
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    isDraggingHeaderToHide = true;
                    columnBeingDragged = col;
                    headerDragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = e.Y - headerDragStartPoint.Y;

            if (deltaY > 20 && deltaY > deltaX)
            {
                gridReport.Cursor = blackXCursor;
                string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                headerToolTip.SetToolTip(gridReport, $"✖ Drag down to hide '{colName}' column");
            }
            else
            {
                gridReport.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(gridReport, string.Empty);
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                if (columnBeingDragged != null && (e.Y - headerDragStartPoint.Y) > 40)
                {
                    HideColumn(columnBeingDragged);
                }
                isDraggingHeaderToHide = false;
                columnBeingDragged = null;
                gridReport.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(gridReport, string.Empty);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
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
                Size = new Size(240, 300),
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

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
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
                if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
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

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        #endregion

        #region GridFooterPanel Dynamic Alignment & Calculation

        private void InitializeGridFooter()
        {
            if (gridFooterPanel == null && panelGrid != null)
            {
                gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 28;
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
                panelGrid.ClientArea.Controls.Add(gridFooterPanel);
                gridFooterPanel.BringToFront();
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
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                gridFooterPanel.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = "None";
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

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
            itemSum.Tag = "Sum";
            itemSum.Enabled = isNumeric;
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min");
            itemMin.Tag = "Min";
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max");
            itemMax.Tag = "Max";
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count");
            itemCount.Tag = "Count";
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average");
            itemAverage.Tag = "Avg";
            itemAverage.Enabled = isNumeric;
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
            itemNone.Tag = "None";
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
                    ToolStripMenuItem toolStripMenuItem = menuItem as ToolStripMenuItem;
                    if (toolStripMenuItem != null && toolStripMenuItem.Tag != null)
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
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            string columnKey = menu.Tag.ToString();
            string aggregation = item.Tag.ToString();

            _columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
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
            if (_footerLabels.Count == 0)
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
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (result == null)
            {
                if (string.Equals(aggregation, "Count", StringComparison.OrdinalIgnoreCase))
                    return "0";
                if (string.Equals(aggregation, "Sum", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Avg", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Min", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Max", StringComparison.OrdinalIgnoreCase))
                    return "0.00";
                return string.Empty;
            }

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
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        return numericValue.Value.ToString(column.Format);

                    return numericValue.Value.ToString("N2");
                }
            }

            return Convert.ToString(result);
        }

        private static decimal? GetNumericValue(object rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
                return null;

            if (rawValue is decimal decVal) return decVal;
            if (rawValue is double dblVal) return Convert.ToDecimal(dblVal);
            if (rawValue is float fltVal) return Convert.ToDecimal(fltVal);
            if (rawValue is int intVal) return intVal;
            if (rawValue is long longVal) return longVal;
            if (rawValue is short shortVal) return shortVal;

            return decimal.TryParse(Convert.ToString(rawValue), out decimal parsed) ? parsed : (decimal?)null;
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        #endregion

        private void FrmStockAdjustmentReport_Load(object sender, EventArgs e)
        {
            FetchFromDatabase();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            FetchFromDatabase();
        }

        public void RibbonClear() => BtnReset_Click(this, EventArgs.Empty);
        public void Clear() => BtnReset_Click(this, EventArgs.Empty);

        private void BtnReset_Click(object sender, EventArgs e)
        {
            comboPeriod.Value = "ALL";
            dtpFromDate.Value = new DateTime(1753, 1, 1);
            dtpToDate.Value = DateTime.Today;
            comboType.Value = "";
            txtSearch.Text = string.Empty;
            _allRows.Clear();
            gridReport.DataSource = null;
            CalculateSummaryValues(new List<StockAdjustmentReportRow>());
            UpdateFooterValues();
            lblStatus.Text = "Ready | Select filters and press Search (F5)";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterFetchedRows();
        }

        private void FrmStockAdjustmentReport_KeyDown(object sender, KeyEventArgs e)
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

        private void FetchFromDatabase()
        {
            if (_searchWorker.IsBusy) return;

            Cursor.Current = Cursors.WaitCursor;
            btnSearch.Enabled = false;
            lblStatus.Text = "Searching adjustment records... Please wait.";

            StockAdjustmentReportFilter filter = new StockAdjustmentReportFilter
            {
                CompanyId = SessionContext.CompanyId,
                BranchId = SessionContext.BranchId,
                FinYearId = SessionContext.FinYearId,
                FromDate = dtpFromDate.Value != null ? Convert.ToDateTime(dtpFromDate.Value).Date : DateTime.MinValue,
                ToDate = dtpToDate.Value != null ? Convert.ToDateTime(dtpToDate.Value).Date : DateTime.MaxValue,
                AdjustmentType = Convert.ToString(comboType.Value),
                SearchQuery = txtSearch.Text.Trim()
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StockAdjustmentReportFilter filter = (StockAdjustmentReportFilter)e.Argument;
            e.Result = _repository.GetStockAdjustmentReport(filter);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed || Disposing) return;

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

                _allRows = e.Result as List<StockAdjustmentReportRow> ?? new List<StockAdjustmentReportRow>();
                FilterFetchedRows();
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    btnSearch.Enabled = true;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void FilterFetchedRows()
        {
            string searchVal = txtSearch.Text.Trim().ToLower();
            List<StockAdjustmentReportRow> filtered;

            if (string.IsNullOrEmpty(searchVal))
            {
                filtered = _allRows;
            }
            else
            {
                filtered = _allRows.Where(r =>
                    (r.ItemName != null && r.ItemName.ToLower().Contains(searchVal)) ||
                    (r.Barcode != null && r.Barcode.ToLower().Contains(searchVal)) ||
                    (r.Reason != null && r.Reason.ToLower().Contains(searchVal)) ||
                    (r.LedgerName != null && r.LedgerName.ToLower().Contains(searchVal)) ||
                    (r.UserName != null && r.UserName.ToLower().Contains(searchVal)) ||
                    r.StockAdjustmentNo.ToString().Contains(searchVal)
                ).ToList();
            }

            gridReport.DataSource = filtered;
            CalculateSummaryValues(filtered);
            UpdateFooterValues();
            lblStatus.Text = $"Ready | Found {filtered.Count} records.";
        }

        private void CalculateSummaryValues(List<StockAdjustmentReportRow> items)
        {
            if (items == null || items.Count == 0)
            {
                lblDocCountValue.Text = "0";
                lblStockInValue.Text = "0.00";
                lblStockOutValue.Text = "0.00";
                lblNetValueValue.Text = "₹0.00";
                return;
            }

            int distinctDocs = items.Select(x => x.StockAdjustmentNo).Distinct().Count();
            decimal totalIn = items.Where(x => x.AdjustmentType == "Stock IN").Sum(x => x.QtyDifference);
            decimal totalOut = items.Where(x => x.AdjustmentType == "Stock OUT").Sum(x => x.QtyDifference);
            decimal netVal = items.Sum(x => x.AdjustmentValue);

            lblDocCountValue.Text = distinctDocs.ToString("N0");
            lblStockInValue.Text = totalIn.ToString("N2");
            lblStockOutValue.Text = totalOut.ToString("N2");
            lblNetValueValue.Text = "₹" + netVal.ToString("N2");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            List<StockAdjustmentReportRow> rows = gridReport.DataSource as List<StockAdjustmentReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SaveFileDialog saveDlg = new SaveFileDialog())
                {
                    saveDlg.Filter = "CSV Files (*.csv)|*.csv";
                    saveDlg.FileName = $"StockAdjustments_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDlg.ShowDialog() != DialogResult.OK) return;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("S.No,Adj No,Date,Type,Barcode,Item Name,Unit,System Stock,Physical Stock,Diff Qty,Cost,Value,Reason,Ledger,User");

                    foreach (StockAdjustmentReportRow r in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(r.SlNo.ToString()),
                            CsvCell(r.StockAdjustmentNo.ToString()),
                            CsvCell(r.StockAdjustmentDate.ToString("dd-MM-yyyy")),
                            CsvCell(r.AdjustmentType),
                            CsvCell(r.Barcode),
                            CsvCell(r.ItemName),
                            CsvCell(r.UnitName),
                            r.SystemStock.ToString("F2"),
                            r.PhysicalStock.ToString("F2"),
                            r.QtyDifference.ToString("F2"),
                            r.Cost.ToString("F2"),
                            r.AdjustmentValue.ToString("F2"),
                            CsvCell(r.Reason),
                            CsvCell(r.LedgerName),
                            CsvCell(r.UserName)
                        ));
                    }

                    sb.AppendLine();
                    sb.AppendLine(string.Join(",",
                        "",
                        "",
                        "",
                        "TOTALS",
                        "",
                        "",
                        "",
                        "",
                        "",
                        rows.Sum(r => r.QtyDifference).ToString("F2"),
                        "",
                        rows.Sum(r => r.AdjustmentValue).ToString("F2"),
                        "",
                        "",
                        ""
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

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string CsvCell(string val)
        {
            if (string.IsNullOrEmpty(val)) return "\"\"";
            if (val.Contains(",") || val.Contains("\"") || val.Contains("\n") || val.Contains("\r"))
            {
                return "\"" + val.Replace("\"", "\"\"") + "\"";
            }
            return "\"" + val + "\"";
        }
    }
}
