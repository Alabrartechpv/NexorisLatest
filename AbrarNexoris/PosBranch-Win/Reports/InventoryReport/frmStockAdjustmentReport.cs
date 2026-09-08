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
            dtpFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpToDate.Value = DateTime.Today;

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
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblType);
            StyleLabel(lblSearch);

            // Style Inputs
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
            button.Appearance.FontData.Name = "Microsoft Sans Serif";
            button.Appearance.FontData.SizeInPoints = 9F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = ButtonBorderColor;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = ButtonBorderColor;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
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

        private void BtnReset_Click(object sender, EventArgs e)
        {
            dtpFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
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
    }
}
