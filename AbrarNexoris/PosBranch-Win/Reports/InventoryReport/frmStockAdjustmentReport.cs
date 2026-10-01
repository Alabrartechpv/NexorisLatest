using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

        // ─── Attached Cell Footer Summary Panel ─────────────────────────────────────
        private Infragistics.Win.Misc.UltraPanel _gridFooterPanel;
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ─── Column Chooser (Hide & Retake) ──────────────────────────────────────────
        private Infragistics.Win.Misc.UltraButton btnColumnChooser;
        private Form _columnChooserForm;
        private CheckedListBox _columnChooserListBox;
        private TextBox _txtColumnSearch;
        private bool _isUpdatingColumnChooser = false;
        private ContextMenuStrip _gridContextMenu;

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

            InitializeGridFooterPanel();

            comboType.Items.Clear();
            comboType.Items.Add("", "All");
            comboType.Items.Add("Stock IN", "Stock IN");
            comboType.Items.Add("Stock OUT", "Stock OUT");
            comboType.Value = "";
            comboType.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;

            InitializeRuntimeAppearance();
            StyleGrid();
            StyleButtons();

            // Column Chooser Button
            btnColumnChooser = new Infragistics.Win.Misc.UltraButton();
            btnColumnChooser.Text = "📋 Columns";
            btnColumnChooser.Location = new Point(1200, 15);
            btnColumnChooser.Size = new Size(95, 27);
            btnColumnChooser.TabIndex = 19;
            panelFilters.ClientArea.Controls.Add(btnColumnChooser);
            btnColumnChooser.Click += (s, e) => ShowColumnChooser();
            StyleButton(btnColumnChooser);

            SetupGridContextMenu();
            SetupCardControls(cardDocCount, lblDocCountCaption, lblDocCountValue, "ADJUSTMENT DOCUMENTS", Color.FromArgb(25, 118, 210));
            SetupCardControls(cardStockIn, lblStockInCaption, lblStockInValue, "TOTAL STOCK IN QTY", Color.FromArgb(0, 150, 136));
            SetupCardControls(cardStockOut, lblStockOutCaption, lblStockOutValue, "TOTAL STOCK OUT QTY", Color.FromArgb(198, 40, 40));
            SetupCardControls(cardNetValue, lblNetValueCaption, lblNetValueValue, "NET ADJUSTMENT VALUE", Color.FromArgb(81, 45, 168));
            LayoutSummaryCards();

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

        private void StyleGrid()
        {
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;
            gridReport.DisplayLayout.Appearance.BackColor = FormBackColor;
            gridReport.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
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
            gridReport.BeforeColumnChooserDisplayed += (s, e) =>
            {
                e.Cancel = true;
                ShowColumnChooser();
            };
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
            ConfigureColumn(band, "StockAdjustmentNo", "Adj No", 80, HAlign.Center, 1, "N0");
            ConfigureColumn(band, "StockAdjustmentDate", "Date", 95, HAlign.Center, 2, "dd-MM-yyyy");
            ConfigureColumn(band, "AdjustmentType", "Type", 90, HAlign.Center, 3, null);
            ConfigureColumn(band, "Barcode", "Barcode", 105, HAlign.Left, 4, null);
            ConfigureColumn(band, "ItemName", "Item", 220, HAlign.Left, 5, null);
            ConfigureColumn(band, "UnitName", "Unit", 65, HAlign.Center, 6, null);
            ConfigureColumn(band, "SystemStock", "System Stock", 105, HAlign.Right, 7, "N2");
            ConfigureColumn(band, "PhysicalStock", "Physical Stock", 110, HAlign.Right, 8, "N2");
            ConfigureColumn(band, "QtyDifference", "Diff Qty", 95, HAlign.Right, 9, "N2");
            ConfigureColumn(band, "Cost", "Cost", 85, HAlign.Right, 10, "N2");
            ConfigureColumn(band, "AdjustmentValue", "Value", 100, HAlign.Right, 11, "N2");
            ConfigureColumn(band, "Reason", "Reason", 160, HAlign.Left, 12, null);
            ConfigureColumn(band, "LedgerName", "Ledger", 130, HAlign.Left, 13, null);
            ConfigureColumn(band, "UserName", "User", 90, HAlign.Left, 14, null);

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

            InitializeGridFooter();
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

        private void StyleButtons()
        {
            StyleButton(btnSearch);
            StyleButton(btnReset);
            StyleButton(btnExport);
            StyleButton(btnPrint);
            StyleButton(btnClose);
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.Appearance.BackColor = ButtonTopColor;
            button.Appearance.BackColor2 = ButtonBottomColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = ButtonBorderColor;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.FontData.Name = "Segoe UI";
            button.Appearance.FontData.SizeInPoints = 8.75F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = ButtonBorderColor;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;
            button.HotTrackAppearance.TextHAlign = HAlign.Center;
            button.HotTrackAppearance.TextVAlign = VAlign.Middle;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = ButtonBorderColor;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
            button.PressedAppearance.TextHAlign = HAlign.Center;
            button.PressedAppearance.TextVAlign = VAlign.Middle;
        }

        private void SetupCardControls(Infragistics.Win.Misc.UltraPanel card, Infragistics.Win.Misc.UltraLabel caption, Infragistics.Win.Misc.UltraLabel value, string captionText, Color valueColor)
        {
            card.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            card.UseAppStyling = false;
            card.UseOsThemes = DefaultableBoolean.False;
            card.Appearance.BackColor = Color.White;
            card.Appearance.BorderColor = Color.FromArgb(226, 232, 240);

            caption.Text = captionText;
            caption.Location = new Point(12, 8);
            caption.Size = new Size(200, 15);
            caption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            caption.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            caption.UseAppStyling = false;
            caption.UseOsThemes = DefaultableBoolean.False;

            value.Location = new Point(12, 26);
            value.Size = new Size(220, 28);
            value.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            value.Text = "0";
            value.Appearance.ForeColor = valueColor;
            value.UseAppStyling = false;
            value.UseOsThemes = DefaultableBoolean.False;
        }

        private void LayoutSummaryCards()
        {
            Infragistics.Win.Misc.UltraPanel[] cards = { cardDocCount, cardStockIn, cardStockOut, cardNetValue };
            Color[] colors =
            {
                Color.FromArgb(25, 118, 210),
                Color.FromArgb(0, 150, 136),
                Color.FromArgb(198, 40, 40),
                Color.FromArgb(81, 45, 168)
            };

            int totalWidth = panelSummary.ClientArea.Width - 30;
            int gap = 15;
            int cardWidth = (totalWidth - (gap * (cards.Length - 1))) / cards.Length;
            if (cardWidth < 190) cardWidth = 190;

            int x = 15;
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(x, 10);
                cards[i].Size = new Size(cardWidth, 62);

                if (!_accentPanelsCreated)
                {
                    Panel accentLine = new Panel();
                    accentLine.Dock = DockStyle.Top;
                    accentLine.Height = 3;
                    accentLine.BackColor = colors[i];
                    cards[i].ClientArea.Controls.Add(accentLine);
                    accentLine.BringToFront();
                }

                x += cardWidth + gap;
            }

            _accentPanelsCreated = true;
        }

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

            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                MessageBox.Show("From date cannot be greater than To date.", "Stock Adjustment Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            btnSearch.Enabled = false;
            lblStatus.Text = "Searching stock adjustment records...";

            StockAdjustmentReportFilter filter = new StockAdjustmentReportFilter
            {
                CompanyId = SessionContext.CompanyId,
                BranchId = SessionContext.BranchId,
                FinYearId = SessionContext.FinYearId,
                FromDate = dtpFromDate.Value.Date,
                ToDate = dtpToDate.Value.Date,
                AdjustmentType = Convert.ToString(comboType.Value),
                SearchQuery = txtSearch.Text.Trim()
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _repository.GetStockAdjustmentReport((StockAdjustmentReportFilter)e.Argument);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed || Disposing) return;

            try
            {
                if (e.Cancelled) return;

                if (e.Error != null)
                {
                    MessageBox.Show("Search failed: " + e.Error.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string search = txtSearch.Text.Trim().ToLowerInvariant();
            List<StockAdjustmentReportRow> filtered;

            if (string.IsNullOrEmpty(search))
            {
                filtered = _allRows;
            }
            else
            {
                filtered = _allRows.Where(row =>
                    SafeContains(row.StockAdjustmentNo.ToString(), search) ||
                    SafeContains(row.Barcode, search) ||
                    SafeContains(row.ItemName, search) ||
                    SafeContains(row.Reason, search) ||
                    SafeContains(row.LedgerName, search) ||
                    SafeContains(row.UserName, search)).ToList();
            }

            gridReport.DataSource = filtered;
            CalculateSummaryValues(filtered);
            UpdateFooterValues(filtered);
            lblStatus.Text = "Ready | Found " + filtered.Count.ToString("N0") + " records.";
        }

        private bool SafeContains(string value, string search)
        {
            return !string.IsNullOrEmpty(value) && value.ToLowerInvariant().Contains(search);
        }

        private void CalculateSummaryValues(List<StockAdjustmentReportRow> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                lblDocCountValue.Text = "0";
                lblStockInValue.Text = "0.00";
                lblStockOutValue.Text = "0.00";
                lblNetValueValue.Text = "0.00";
                return;
            }

            int documents = rows.Select(r => r.StockAdjustmentId).Distinct().Count();
            decimal stockIn = rows.Sum(r => r.StockInQty);
            decimal stockOut = rows.Sum(r => r.StockOutQty);
            decimal netValue = rows.Sum(r => r.AdjustmentValue);

            lblDocCountValue.Text = documents.ToString("N0");
            lblStockInValue.Text = stockIn.ToString("N2");
            lblStockOutValue.Text = stockOut.ToString("N2");
            lblNetValueValue.Text = netValue.ToString("N2");
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
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV Files (*.csv)|*.csv";
                    dialog.FileName = "StockAdjustmentReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                    if (dialog.ShowDialog() != DialogResult.OK) return;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("S.No,Adj No,Date,Type,Barcode,Item,Unit,System Stock,Physical Stock,Diff Qty,Stock In,Stock Out,Cost,Value,Reason,Ledger,User,Comments");

                    foreach (StockAdjustmentReportRow row in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(row.SlNo.ToString()),
                            CsvCell(row.StockAdjustmentNo.ToString()),
                            CsvCell(row.StockAdjustmentDate.ToString("dd-MM-yyyy")),
                            CsvCell(row.AdjustmentType),
                            CsvCell(row.Barcode),
                            CsvCell(row.ItemName),
                            CsvCell(row.UnitName),
                            row.SystemStock.ToString("F2"),
                            row.PhysicalStock.ToString("F2"),
                            row.QtyDifference.ToString("F2"),
                            row.StockInQty.ToString("F2"),
                            row.StockOutQty.ToString("F2"),
                            row.Cost.ToString("F2"),
                            row.AdjustmentValue.ToString("F2"),
                            CsvCell(row.Reason),
                            CsvCell(row.LedgerName),
                            CsvCell(row.UserName),
                            CsvCell(row.Comments)));
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private string CsvCell(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return "\"" + value + "\"";
        }

        // ════════════════════════════════════════════════════════════
        //  Attached Cell Footer Summary Implementation
        // ════════════════════════════════════════════════════════════
        private void InitializeGridFooterPanel()
        {
            if (_gridFooterPanel != null) return;
            _gridFooterPanel = new Infragistics.Win.Misc.UltraPanel
            {
                Height = 24,
                Dock = DockStyle.Bottom,
                BorderStyle = UIElementBorderStyle.Solid
            };
            _gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
            _gridFooterPanel.Appearance.BorderColor = BorderBlue;

            if (panelGrid != null && panelGrid.ClientArea != null)
            {
                panelGrid.ClientArea.Controls.Add(_gridFooterPanel);
                _gridFooterPanel.BringToFront();
            }

            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
        }

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(gridReport.DataSource as List<StockAdjustmentReportRow>);
        }

        private void CreateFooterCells()
        {
            if (_gridFooterPanel == null) return;
            _gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
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
                    Height = Math.Max(_gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key),
                    Cursor = Cursors.Hand
                };
                footerLabel.Paint += FooterLabel_Paint;
                footerLabel.Click += (s, e) =>
                {
                    if (footerLabel.ContextMenuStrip != null)
                        footerLabel.ContextMenuStrip.Show(footerLabel, new Point(0, footerLabel.Height));
                };

                _gridFooterPanel.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    if (column.Key == "SystemStock" || column.Key == "PhysicalStock" || column.Key == "QtyDifference" || column.Key == "AdjustmentValue" || column.Key == "StockInQty" || column.Key == "StockOutQty")
                        _columnAggregations[column.Key] = "Sum";
                    else if (column.Key == "ItemName" || column.Key == "StockAdjustmentNo")
                        _columnAggregations[column.Key] = "Count";
                    else
                        _columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Tag = columnKey };
            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            AddFooterMenuItem(menu, "Sum", "Sum", isNumeric);
            AddFooterMenuItem(menu, "Min", "Min", true);
            AddFooterMenuItem(menu, "Max", "Max", true);
            AddFooterMenuItem(menu, "Count", "Count", true);
            AddFooterMenuItem(menu, "Average", "Avg", isNumeric);
            menu.Items.Add(new ToolStripSeparator());
            AddFooterMenuItem(menu, "None", "None", true);

            menu.Opening += (sender, e) =>
            {
                string current = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";
                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null)
                        menuItem.Checked = string.Equals(menuItem.Tag.ToString(), current, StringComparison.OrdinalIgnoreCase);
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
                UpdateFooterValues(gridReport.DataSource as List<StockAdjustmentReportRow>);
            }
        }

        private void UpdateFooterValues(List<StockAdjustmentReportRow> rows = null)
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

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return aggregation == "Count" ? (object)0 : null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue).Sum(value => value.Value);
                case "Min":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderBy(value => value).FirstOrDefault();
                case "Max":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderByDescending(value => value).FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value)).Where(value => value.HasValue)
                        .Select(value => value.Value).ToList();
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

            UltraGridColumn column = gridReport.DisplayLayout != null && gridReport.DisplayLayout.Bands.Count > 0 && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey)
                ? gridReport.DisplayLayout.Bands[0].Columns[columnKey]
                : null;
            decimal? numericValue = GetNumericValue(result);
            if (numericValue.HasValue)
                return column != null && !string.IsNullOrWhiteSpace(column.Format)
                    ? numericValue.Value.ToString(column.Format)
                    : numericValue.Value.ToString("N2");
            return Convert.ToString(result);
        }

        private void UpdateFooterCellPositions()
        {
            if (_gridFooterPanel == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
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
                footerLabel.Height = Math.Max(_gridFooterPanel.Height - 2, 20);
                footerLabel.Visible = (left + width > 0 && left < _gridFooterPanel.Width);
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (decimal.TryParse(Convert.ToString(value), out decimal result))
                return result;
            return null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null)
                return false;

            System.Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
                   type == typeof(int) || type == typeof(long) || type == typeof(short);
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Label footerLabel && footerLabel.Tag is Tuple<string, string> value && !string.IsNullOrEmpty(value.Item2))
            {
                using (Pen pen = new Pen(GridFooterBorder))
                    e.Graphics.DrawRectangle(pen, 0, 0, footerLabel.Width - 1, footerLabel.Height - 1);
            }
        }

        // ─── Column Chooser (Hide & Retake) Implementation ───────────────────────────
        private void SetupGridContextMenu()
        {
            _gridContextMenu = new ContextMenuStrip();

            ToolStripMenuItem itemChooser = new ToolStripMenuItem("📋 Field / Column Chooser (Hide & Retake)...", null, (s, e) => ShowColumnChooser());
            ToolStripMenuItem itemShowAll = new ToolStripMenuItem("👁 Show / Retake All Columns", null, (s, e) => SetAllColumnsVisibility(true));
            ToolStripMenuItem itemReset = new ToolStripMenuItem("🔄 Reset Default Columns", null, (s, e) => ResetDefaultColumns());
            ToolStripMenuItem itemExport = new ToolStripMenuItem("📥 Export Grid Data...", null, (s, e) => BtnExport_Click(null, null));

            _gridContextMenu.Items.Add(itemChooser);
            _gridContextMenu.Items.Add(new ToolStripSeparator());
            _gridContextMenu.Items.Add(itemShowAll);
            _gridContextMenu.Items.Add(itemReset);
            _gridContextMenu.Items.Add(new ToolStripSeparator());
            _gridContextMenu.Items.Add(itemExport);

            gridReport.ContextMenuStrip = _gridContextMenu;
        }

        private void ShowColumnChooser()
        {
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            RefreshColumnChooserList();
            PositionColumnChooser();
            _columnChooserForm.Show(this);
            _columnChooserForm.BringToFront();
            _txtColumnSearch?.Focus();
        }

        private void CreateColumnChooserForm()
        {
            _columnChooserForm = new Form
            {
                Text = "📋 Column Chooser - Hide / Retake Columns",
                Size = new Size(320, 480),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                ShowIcon = false,
                ShowInTaskbar = false,
                BackColor = Color.FromArgb(241, 245, 249),
                MinimumSize = new Size(260, 320)
            };

            _columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                _columnChooserForm.Hide();
            };

            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(6, 6, 6, 4)
            };

            _txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            _txtColumnSearch.TextChanged += (s, e) => RefreshColumnChooserList();

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 2, 0, 0)
            };

            Button btnShowAll = new Button
            {
                Text = "☑ All",
                Size = new Size(58, 26),
                Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 231, 255),
                FlatStyle = FlatStyle.Flat
            };
            btnShowAll.Click += (s, e) => SetAllColumnsVisibility(true);

            Button btnHideAll = new Button
            {
                Text = "⬜ None",
                Size = new Size(62, 26),
                Font = new Font("Segoe UI", 8.25F),
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat
            };
            btnHideAll.Click += (s, e) => SetAllColumnsVisibility(false);

            Button btnResetCols = new Button
            {
                Text = "🔄 Reset",
                Size = new Size(68, 26),
                Font = new Font("Segoe UI", 8.25F),
                BackColor = Color.FromArgb(241, 245, 249),
                FlatStyle = FlatStyle.Flat
            };
            btnResetCols.Click += (s, e) => ResetDefaultColumns();

            pnlButtons.Controls.Add(btnShowAll);
            pnlButtons.Controls.Add(btnHideAll);
            pnlButtons.Controls.Add(btnResetCols);

            pnlTop.Controls.Add(pnlButtons);
            pnlTop.Controls.Add(_txtColumnSearch);

            _columnChooserListBox = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                IntegralHeight = false
            };
            _columnChooserListBox.ItemCheck += ColumnChooserListBox_ItemCheck;

            _columnChooserForm.Controls.Add(_columnChooserListBox);
            _columnChooserForm.Controls.Add(pnlTop);

            LocationChanged += (s, e) => PositionColumnChooser();
            SizeChanged += (s, e) => PositionColumnChooser();
        }

        private sealed class ColumnChooserItem
        {
            public UltraGridColumn Column { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(UltraGridColumn column)
            {
                Column = column;
                string caption = column.Header?.Caption;
                DisplayText = !string.IsNullOrWhiteSpace(caption) ? caption : column.Key;
            }

            public override string ToString() => DisplayText;
        }

        private void RefreshColumnChooserList()
        {
            if (_columnChooserListBox == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            string filterText = _txtColumnSearch?.Text?.Trim().ToLowerInvariant() ?? "";

            _isUpdatingColumnChooser = true;
            _columnChooserListBox.Items.Clear();

            foreach (UltraGridColumn column in band.Columns)
            {
                if (IsInternalNonDisplayColumn(column.Key)) continue;

                string display = !string.IsNullOrWhiteSpace(column.Header?.Caption) ? column.Header.Caption : column.Key;
                if (!string.IsNullOrEmpty(filterText) && !display.ToLowerInvariant().Contains(filterText) && !column.Key.ToLowerInvariant().Contains(filterText))
                    continue;

                ColumnChooserItem item = new ColumnChooserItem(column);
                int index = _columnChooserListBox.Items.Add(item);
                _columnChooserListBox.SetItemChecked(index, !column.Hidden);
            }
            _isUpdatingColumnChooser = false;
        }

        private void ColumnChooserListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_isUpdatingColumnChooser) return;

            BeginInvoke(new Action(() =>
            {
                if (e.Index < 0 || e.Index >= _columnChooserListBox.Items.Count) return;
                if (_columnChooserListBox.Items[e.Index] is ColumnChooserItem item && item.Column != null)
                {
                    item.Column.Hidden = e.NewValue != CheckState.Checked;
                    CreateFooterCells();
                    UpdateFooterCellPositions();
                    UpdateFooterValues();
                }
            }));
        }

        private void SetAllColumnsVisibility(bool visible)
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            gridReport.BeginUpdate();
            foreach (UltraGridColumn col in band.Columns)
            {
                if (IsInternalNonDisplayColumn(col.Key)) continue;
                if (!visible && (string.Equals(col.Key, "StockAdjustmentNo", StringComparison.OrdinalIgnoreCase) || string.Equals(col.Key, "ItemName", StringComparison.OrdinalIgnoreCase)))
                {
                    col.Hidden = false;
                    continue;
                }
                col.Hidden = !visible;
            }
            gridReport.EndUpdate();

            RefreshColumnChooserList();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ResetDefaultColumns()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            var defaultVisible = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SlNo", "StockAdjustmentNo", "StockAdjustmentDate", "AdjustmentType", "Barcode", "ItemName",
                "UnitName", "SystemStock", "PhysicalStock", "QtyDifference", "Cost", "AdjustmentValue",
                "Reason", "LedgerName", "UserName"
            };

            gridReport.BeginUpdate();
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = !defaultVisible.Contains(col.Key);
            }
            gridReport.EndUpdate();

            RefreshColumnChooserList();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void PositionColumnChooser()
        {
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed || !_columnChooserForm.Visible) return;
            Point screenPoint = PointToScreen(new Point(ClientSize.Width - _columnChooserForm.Width - 20, ClientSize.Height - _columnChooserForm.Height - 40));
            _columnChooserForm.Location = screenPoint;
        }

        private static bool IsInternalNonDisplayColumn(string key)
        {
            return string.Equals(key, "StockAdjustmentDetailsId", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(key, "StockAdjustmentMasterId", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(key, "ItemId", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(key, "UnitId", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(key, "ReasonId", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(key, "LedgerId", StringComparison.OrdinalIgnoreCase);
        }
    }
}
