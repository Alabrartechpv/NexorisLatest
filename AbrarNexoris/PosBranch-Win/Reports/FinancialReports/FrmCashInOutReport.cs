using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmCashInOutReport : Form
    {
        #region Colour Palette
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionBarBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);

        private static readonly Color ButtonBlueTop = Color.FromArgb(241, 248, 254);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(196, 218, 243);
        private static readonly Color ButtonTextBlue = Color.FromArgb(18, 49, 102);
        private static readonly Color ButtonLightOutline = Color.FromArgb(118, 154, 198);

        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(245, 250, 255);
        private static readonly Color CashInGreen = Color.FromArgb(46, 125, 50);
        private static readonly Color CashOutRed = Color.FromArgb(198, 40, 40);
        #endregion

        private readonly CashInOutRepository _repository;
        private CashInOutReportResponse _currentData;
        private List<CashInOutTransaction> _filteredTransactions;
        private bool _isLoading;

        public FrmCashInOutReport() : this(DateTime.Today, DateTime.Today)
        {
        }

        public FrmCashInOutReport(DateTime fromDate, DateTime toDate)
        {
            _repository = new CashInOutRepository();
            _currentData = new CashInOutReportResponse();
            _filteredTransactions = new List<CashInOutTransaction>();

            InitializeComponent();

            Load += FrmCashInOutReport_Load;
            KeyDown += FrmCashInOutReport_KeyDown;
            KeyPreview = true;

            // Action Buttons
            btnViewGrid.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => ShowGridPreview("Cash In / Cash Out Flow - Grid Preview");
            btnPreviewReport.Click += (s, e) => ShowReportPreview();
            btnExportExcel.Click += (s, e) => ExportToExcel();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnColumnChooser.Click += (s, e) => ShowColumnChooser();
            btnHideSelection.Click += (s, e) => ToggleSelection();

            // Filters
            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;
            cmbCategory.ValueChanged += (s, e) => ApplyFilter();
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            // Grid Events
            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;

            // Responsive Summary Cards
            ultraPanelSummary.Resize += (s, e) => LayoutSummaryCards();

            // Set Initial Dates
            dtFrom.DateTime = fromDate;
            dtTo.DateTime = toDate;
        }

        private void FrmCashInOutReport_Load(object sender, EventArgs e)
        {
            InitializeStyling();
            PopulateCombos();
            LayoutSummaryCards();
            LoadData();
        }

        private void FrmCashInOutReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                LoadData();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void InitializeStyling()
        {
            BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            // Action Bar Panel
            ultraPanelActionBar.Appearance.BackColor = ActionBarBackColor;
            ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
            ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;

            // Summary Panel
            ultraPanelSummary.Appearance.BackColor = FormBackColor;
            ultraPanelSummary.Appearance.BorderColor = BorderBlue;

            // Footer Panel
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlueDark;
            lblFooterSummary.Appearance.ForeColor = Color.White;

            // Style All Action Buttons (Standard Nexoris Gradient Buttons)
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportExcel);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnHideSelection);

            SetupGridAppearance(gridReport);
        }

        private static void StyleClassicButton(UltraButton button)
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
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = ButtonLightOutline;
        }

        private void SetupGridAppearance(UltraGrid grid)
        {
            if (grid == null) return;

            grid.ResetDisplayLayout();
            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;
            layout.Appearance.BackColor = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.BorderStyle = UIElementBorderStyle.Solid;

            layout.CaptionVisible = DefaultableBoolean.False;

            // Override settings
            UltraGridOverride ov = layout.Override;
            ov.HeaderAppearance.BackColor = GridHeaderBlue;
            ov.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ov.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ov.HeaderAppearance.ForeColor = Color.White;
            ov.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ov.HeaderAppearance.FontData.SizeInPoints = 9;
            ov.HeaderAppearance.TextHAlign = HAlign.Center;
            ov.HeaderAppearance.BorderColor = BorderBlue;
            ov.HeaderStyle = HeaderStyle.Standard;

            ov.RowAppearance.BackColor = Color.White;
            ov.RowAppearance.ForeColor = Color.FromArgb(30, 30, 30);
            ov.RowAppearance.BorderColor = GridRowLine;
            ov.RowAppearance.FontData.SizeInPoints = 9;

            ov.RowAlternateAppearance.BackColor = GridAltRow;
            ov.RowAlternateAppearance.ForeColor = Color.FromArgb(30, 30, 30);
            ov.RowAlternateAppearance.BorderColor = GridRowLine;

            ov.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ov.SelectedRowAppearance.ForeColor = Color.Black;

            ov.ActiveRowAppearance.BackColor = GridSelectedBlue;
            ov.ActiveRowAppearance.ForeColor = Color.Black;

            ov.BorderStyleRow = UIElementBorderStyle.Solid;
            ov.BorderStyleCell = UIElementBorderStyle.Solid;
            ov.CellPadding = 2;

            ov.AllowAddNew = AllowAddNew.No;
            ov.AllowDelete = DefaultableBoolean.False;
            ov.AllowUpdate = DefaultableBoolean.False;
            ov.CellClickAction = CellClickAction.RowSelect;
            ov.SelectTypeRow = SelectType.Single;

            ov.AllowColSizing = AllowColSizing.Free;
            ov.AllowColMoving = AllowColMoving.WithinBand;
            ov.AllowColSwapping = AllowColSwapping.WithinBand;
            ov.RowSelectors = DefaultableBoolean.True;
            ov.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.ScrollStyle = ScrollStyle.Immediate;
        }

        private void PopulateCombos()
        {
            // Date Quick Select
            cmbDateQuickSelect.Items.Clear();
            cmbDateQuickSelect.Items.Add("TODAY", "Today");
            cmbDateQuickSelect.Items.Add("YESTERDAY", "Yesterday");
            cmbDateQuickSelect.Items.Add("THIS_WEEK", "This Week");
            cmbDateQuickSelect.Items.Add("THIS_MONTH", "This Month");
            cmbDateQuickSelect.Items.Add("THIS_YEAR", "This Year");
            cmbDateQuickSelect.Items.Add("ALL", "All Dates");
            cmbDateQuickSelect.SelectedIndex = 0;

            // Category Filter
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("ALL", "All Categories");
            cmbCategory.Items.Add("IN", "All Inflows [IN]");
            cmbCategory.Items.Add("OUT", "All Outflows [OUT]");
            cmbCategory.Items.Add("VENDOR_PAYMENT", "Vendor Payments");
            cmbCategory.Items.Add("CUSTOMER_RECEIPT", "Customer Receipts");
            cmbCategory.Items.Add("COUNTER_CASH_SALES", "Counter Cash Sales");
            cmbCategory.Items.Add("DIGITAL_SALES", "Digital Sales");
            cmbCategory.Items.Add("DIRECT_PURCHASE", "Direct Purchases");
            cmbCategory.Items.Add("EXPENSE", "Operating Expenses");
            cmbCategory.Items.Add("GENERAL_RECEIPT", "General Receipts");
            cmbCategory.Items.Add("DRAWINGS", "Drawings / Other");
            cmbCategory.SelectedIndex = 0;
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            if (cmbDateQuickSelect.Value == null) return;
            string key = cmbDateQuickSelect.Value.ToString();
            DateTime today = DateTime.Today;

            switch (key)
            {
                case "TODAY":
                    dtFrom.DateTime = today;
                    dtTo.DateTime = today;
                    break;
                case "YESTERDAY":
                    dtFrom.DateTime = today.AddDays(-1);
                    dtTo.DateTime = today.AddDays(-1);
                    break;
                case "THIS_WEEK":
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dtFrom.DateTime = today.AddDays(-diff);
                    dtTo.DateTime = today;
                    break;
                case "THIS_MONTH":
                    dtFrom.DateTime = new DateTime(today.Year, today.Month, 1);
                    dtTo.DateTime = today;
                    break;
                case "THIS_YEAR":
                    dtFrom.DateTime = new DateTime(today.Year, 1, 1);
                    dtTo.DateTime = today;
                    break;
                case "ALL":
                    dtFrom.DateTime = new DateTime(1990, 1, 1);
                    dtTo.DateTime = today;
                    break;
            }
        }

        public void LoadData()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;
                Cursor = Cursors.WaitCursor;

                DateTime fromDate = dtFrom.DateTime.Date;
                DateTime toDate = dtTo.DateTime.Date.AddDays(1).AddMilliseconds(-3);

                _currentData = _repository.GetCashInOutReport(fromDate, toDate, SessionContext.BranchId, SessionContext.CompanyId);

                UpdateSummaryCards();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Cash In / Cash Out report: {ex.Message}", "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Cursor = Cursors.Default;
            }
        }

        private void UpdateSummaryCards()
        {
            if (_currentData == null || _currentData.Summary == null) return;

            var s = _currentData.Summary;
            lblCardInValue.Text = $"₹ {s.TotalCashIn:N2}";
            lblCardInTitle.Text = $"TOTAL CASH IN ({s.InflowCount:N0} Txns)";

            lblCardOutValue.Text = $"₹ {s.TotalCashOut:N2}";
            lblCardOutTitle.Text = $"TOTAL CASH OUT ({s.OutflowCount:N0} Txns)";

            lblCardNetValue.Text = $"₹ {s.NetCashFlow:N2}";
            lblCardNetValue.ForeColor = s.NetCashFlow >= 0 ? CashInGreen : CashOutRed;

            lblCardBalanceValue.Text = $"₹ {s.ClosingBalance:N2}";
            lblCardBalanceTitle.Text = $"CLOSING (Opn: ₹ {s.OpeningBalance:N0})";
        }

        private void LayoutSummaryCards()
        {
            try
            {
                if (ultraPanelSummary == null || pnlCardIn == null || pnlCardOut == null || pnlCardNet == null || pnlCardBalance == null) return;
                int containerW = ultraPanelSummary.ClientArea.ClientSize.Width;
                if (containerW <= 100) return;
                int margin = 10;
                int gap = 10;
                int cardCount = 4;
                int availableW = containerW - (margin * 2) - (gap * (cardCount - 1));
                int cardW = Math.Max(200, availableW / cardCount);

                pnlCardIn.Location = new Point(margin, 6);
                pnlCardIn.Size = new Size(cardW, 54);

                pnlCardOut.Location = new Point(margin + cardW + gap, 6);
                pnlCardOut.Size = new Size(cardW, 54);

                pnlCardNet.Location = new Point(margin + (cardW + gap) * 2, 6);
                pnlCardNet.Size = new Size(cardW, 54);

                pnlCardBalance.Location = new Point(margin + (cardW + gap) * 3, 6);
                pnlCardBalance.Size = new Size(cardW, 54);
            }
            catch
            {
                // Ignore sizing errors
            }
        }

        private void ApplyFilter()
        {
            if (_currentData == null || _currentData.Transactions == null)
            {
                gridReport.DataSource = null;
                lblFooterSummary.Text = "No transactions available";
                return;
            }

            var query = _currentData.Transactions.AsEnumerable();

            // Category / Direction Filter
            string catFilter = cmbCategory.Value?.ToString() ?? "ALL";
            if (catFilter == "IN")
            {
                query = query.Where(t => t.Direction == "IN");
            }
            else if (catFilter == "OUT")
            {
                query = query.Where(t => t.Direction == "OUT");
            }
            else if (catFilter != "ALL")
            {
                query = query.Where(t => string.Equals(t.Category, catFilter, StringComparison.OrdinalIgnoreCase));
            }

            // Search Text
            string search = txtSearch.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    (t.VoucherNo ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Particulars ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Narration ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.PaymentMode ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.VoucherType ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _filteredTransactions = query.ToList();

            // Bind to grid
            gridReport.DataSource = null;
            gridReport.DataSource = _filteredTransactions;

            // Footer calculation
            decimal totalIn = _filteredTransactions.Sum(t => t.InflowAmount);
            decimal totalOut = _filteredTransactions.Sum(t => t.OutflowAmount);
            decimal net = totalIn - totalOut;

            lblFooterSummary.Text = $"  Total Transactions: {_filteredTransactions.Count:N0}  |  Total Inflow: ₹ {totalIn:N2}  |  Total Outflow: ₹ {totalOut:N2}  |  Net Period Movement: ₹ {net:N2}";
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                if (e.Layout.Bands.Count == 0) return;
                var band = e.Layout.Bands[0];

                // Configure Columns
                int pos = 0;
                SetCol(band, "SlNo", "S.No", 45, HAlign.Center, pos++);
                SetCol(band, "VoucherDate", "Date & Time", 125, HAlign.Center, pos++, "dd-MM-yyyy HH:mm");
                SetCol(band, "VoucherNo", "Voucher / Bill No", 110, HAlign.Center, pos++);
                SetCol(band, "VoucherType", "Voucher Type", 100, HAlign.Left, pos++);
                SetCol(band, "Category", "Category", 130, HAlign.Left, pos++);
                SetCol(band, "Direction", "Direction", 70, HAlign.Center, pos++);
                SetCol(band, "Particulars", "Particulars / Party / Ledger", 230, HAlign.Left, pos++);
                SetCol(band, "PaymentMode", "Mode", 80, HAlign.Center, pos++);
                SetCol(band, "InflowAmount", "Cash In (₹)", 110, HAlign.Right, pos++, "#,##0.00");
                SetCol(band, "OutflowAmount", "Cash Out (₹)", 110, HAlign.Right, pos++, "#,##0.00");
                SetCol(band, "Narration", "Remarks / Narration", 200, HAlign.Left, pos++);

                if (band.Columns.Exists("Direction"))
                {
                    band.Columns["Direction"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("InflowAmount"))
                {
                    band.Columns["InflowAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["InflowAmount"].CellAppearance.ForeColor = CashInGreen;
                }
                if (band.Columns.Exists("OutflowAmount"))
                {
                    band.Columns["OutflowAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["OutflowAmount"].CellAppearance.ForeColor = CashOutRed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Grid layout error: {ex.Message}");
            }
        }

        private static void SetCol(UltraGridBand band, string key, string caption, int width, HAlign align, int visiblePosition, string format = null)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
            col.Header.VisiblePosition = visiblePosition;
            if (!string.IsNullOrEmpty(format))
            {
                col.Format = format;
            }
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row == null || !e.Row.Band.Columns.Exists("Direction")) return;

                string dir = e.Row.Cells["Direction"].Value?.ToString() ?? "";
                if (dir == "IN")
                {
                    e.Row.Cells["Direction"].Appearance.ForeColor = CashInGreen;
                    e.Row.Cells["Direction"].Appearance.BackColor = Color.FromArgb(232, 245, 233);
                }
                else if (dir == "OUT")
                {
                    e.Row.Cells["Direction"].Appearance.ForeColor = CashOutRed;
                    e.Row.Cells["Direction"].Appearance.BackColor = Color.FromArgb(255, 235, 238);
                }
            }
            catch
            {
                // Ignore row styling error
            }
        }

        private void ResetFilters()
        {
            cmbDateQuickSelect.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
            txtSearch.Text = "";
            LoadData();
        }

        private void ToggleSelection()
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnHideSelection.Text = ultraPanelControls.Visible ? "&Hide Selection" : "&Show Selection";
        }

        private void ShowColumnChooser()
        {
            try
            {
                gridReport.ShowColumnChooser("Column Chooser");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open Column Chooser: {ex.Message}", "Column Chooser", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowGridPreview(string title)
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open print preview: {ex.Message}", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowReportPreview()
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (Form preview = new Form())
                using (Panel header = new Panel())
                using (Panel footer = new Panel())
                using (UltraGrid previewGrid = new UltraGrid())
                {
                    preview.Text = "Cash In / Cash Out Flow - Report Preview";
                    preview.StartPosition = FormStartPosition.CenterParent;
                    preview.WindowState = FormWindowState.Maximized;
                    preview.MinimumSize = new Size(1024, 600);
                    preview.BackColor = FormBackColor;
                    preview.Padding = new Padding(10);

                    // Header banner
                    header.Dock = DockStyle.Top;
                    header.Height = 70;
                    header.BackColor = GridHeaderBlueDark;
                    header.Padding = new Padding(16, 8, 16, 8);

                    Label titleLabel = new Label
                    {
                        Dock = DockStyle.Top,
                        Height = 28,
                        Text = "CASH IN / CASH OUT FLOW REPORT",
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    decimal totalIn = _filteredTransactions.Sum(t => t.InflowAmount);
                    decimal totalOut = _filteredTransactions.Sum(t => t.OutflowAmount);
                    decimal net = totalIn - totalOut;

                    Label subtitleLabel = new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Period: {dtFrom.DateTime:dd-MMM-yyyy} to {dtTo.DateTime:dd-MMM-yyyy}   |   Total Cash In: ₹ {totalIn:N2}   |   Total Cash Out: ₹ {totalOut:N2}   |   Net Flow: ₹ {net:N2}",
                        ForeColor = Color.FromArgb(224, 238, 252),
                        BackColor = Color.Transparent,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    header.Controls.Add(subtitleLabel);
                    header.Controls.Add(titleLabel);

                    // Grid
                    previewGrid.Dock = DockStyle.Fill;
                    previewGrid.BackColor = Color.White;
                    SetupGridAppearance(previewGrid);
                    previewGrid.InitializeLayout += GridReport_InitializeLayout;
                    previewGrid.InitializeRow += GridReport_InitializeRow;
                    previewGrid.DataSource = _filteredTransactions.ToList();

                    preview.Controls.Add(previewGrid);
                    preview.Controls.Add(header);

                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to display report preview: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                if (_filteredTransactions == null || _filteredTransactions.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV File (*.csv)|*.csv",
                    FileName = $"CashInOutReport_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("SlNo,Date,VoucherNo,VoucherType,Category,Direction,Particulars,Mode,InflowAmount,OutflowAmount,Narration");

                        foreach (var t in _filteredTransactions)
                        {
                            sb.AppendLine($"{t.SlNo},\"{t.VoucherDate:dd-MM-yyyy HH:mm}\",\"{t.VoucherNo}\",\"{t.VoucherType}\",\"{t.Category}\",\"{t.Direction}\",\"{t.Particulars?.Replace("\"", "\"\"")}\",\"{t.PaymentMode}\",{t.InflowAmount:F2},{t.OutflowAmount:F2},\"{t.Narration?.Replace("\"", "\"\"")}\"");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Report exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
