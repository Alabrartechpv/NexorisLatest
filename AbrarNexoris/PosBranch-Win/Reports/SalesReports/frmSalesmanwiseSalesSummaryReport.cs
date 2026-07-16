using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using Repository;
using PosBranch_Win.DialogBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesmanwiseSalesSummaryReport : Form
    {
        // ════════════════════════════════════════════════════════════
        //  Fields
        // ════════════════════════════════════════════════════════════
        private SalesmanwiseSalesSummaryRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _isChangingDates = false;
        private bool _accentPanelsCreated = false;
        private int? _selectedSalesmanId = null;
        private string _selectedSalesmanName = string.Empty;

        /// <summary>All rows fetched from the DB (before any in-memory filter)</summary>
        private List<SalesmanwiseSalesSummaryItem> _allRows = new List<SalesmanwiseSalesSummaryItem>();

        // ════════════════════════════════════════════════════════════
        //  Constructor
        // ════════════════════════════════════════════════════════════
        public frmSalesmanwiseSalesSummaryReport()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 9F);
            InitializeBackgroundWorker();
            InitializeForm();
        }

        // ════════════════════════════════════════════════════════════
        //  Initialization
        // ════════════════════════════════════════════════════════════
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

                // Configure grid properties
                StyleGrid();

                // Setup static card control designs
                SetupCardControls(cardSalesmanCount,   lblSalesmanCountCaption,   lblSalesmanCountValue,   "TOTAL SALESMEN", "cardSalesmanCount");
                SetupCardControls(cardInvoiceCount,    lblInvoiceCountCaption,    lblInvoiceCountValue,    "TOTAL INVOICES", "cardInvoiceCount");
                SetupCardControls(cardTotalQty,        lblTotalQtyCaption,        lblTotalQtyValue,        "TOTAL QTY SOLD", "cardTotalQty");
                SetupCardControls(cardTotalSales,      lblTotalSalesCaption,      lblTotalSalesValue,      "TOTAL SALES AMOUNT", "cardTotalSales");
                SetupCardControls(cardTotalCommission, lblTotalCommissionCaption, lblTotalCommissionValue, "TOTAL COMMISSION", "cardTotalCommission");

                // Style buttons
                StyleButtons();

                // Apply dynamic layout coordinates and colors on card panels
                LayoutSummaryCards();

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

                // Cancel background work on form close to avoid disposed-control crash
                this.FormClosing += (s, ev) =>
                {
                    if (_searchWorker != null && _searchWorker.IsBusy)
                        _searchWorker.CancelAsync();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════
        //  Grid styling
        // ════════════════════════════════════════════════════════════
        private void StyleGrid()
        {
            gridReport.UseOsThemes = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.AllowRowFiltering   = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.FilterUIType        = FilterUIType.FilterRow;
            gridReport.DisplayLayout.Override.HeaderClickAction   = HeaderClickAction.SortMulti;
            gridReport.DisplayLayout.AutoFitStyle                  = AutoFitStyle.ResizeAllColumns;
            gridReport.DisplayLayout.CaptionVisible                = DefaultableBoolean.False;
            gridReport.DisplayLayout.GroupByBox.Hidden             = true;

            // Header
            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor  = Color.FromArgb(30, 40, 55);
            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(42, 55, 72);
            gridReport.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            gridReport.DisplayLayout.Override.HeaderAppearance.ForeColor  = Color.White;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.Bold      = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            gridReport.DisplayLayout.Override.HeaderStyle          = HeaderStyle.Standard;

            // Rows
            gridReport.DisplayLayout.Override.RowAppearance.BackColor          = Color.White;
            gridReport.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 253);
            gridReport.DisplayLayout.Override.MinRowHeight                     = 24;
            gridReport.DisplayLayout.Override.DefaultRowHeight                 = 24;

            // Selection
            gridReport.DisplayLayout.Override.SelectedRowAppearance.BackColor  = Color.FromArgb(66, 165, 245);
            gridReport.DisplayLayout.Override.SelectedRowAppearance.ForeColor  = Color.White;
            gridReport.DisplayLayout.Override.ActiveRowAppearance.BackColor    = Color.FromArgb(227, 242, 253);
            gridReport.DisplayLayout.Override.ActiveRowAppearance.ForeColor    = Color.FromArgb(33, 33, 33);
            gridReport.DisplayLayout.Override.ActiveRowAppearance.BorderColor  = Color.FromArgb(66, 165, 245);
            gridReport.DisplayLayout.Override.CellClickAction                  = CellClickAction.RowSelect;

            // Fix 7: Explicit filter row styling for dark-themed OS
            gridReport.DisplayLayout.Override.FilterRowAppearance.BackColor   = Color.FromArgb(255, 255, 230);
            gridReport.DisplayLayout.Override.FilterRowAppearance.ForeColor   = Color.FromArgb(33, 33, 33);
            gridReport.DisplayLayout.Override.FilterRowPromptAppearance.ForeColor = Color.FromArgb(140, 140, 140);

            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow    += GridReport_InitializeRow;
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            var band = e.Layout.Bands[0];

            // Define column order and captions
            var columns = new[]
            {
                ("SlNo",              "S.No",               50,  HAlign.Center),
                ("SalesmanName",      "Salesman Name",     180,  HAlign.Left),
                ("Email",             "Email Address",     180,  HAlign.Left),
                ("InvoiceCount",      "Invoice Count",     100,  HAlign.Right),
                ("TotalQtySold",      "Total Qty Sold",    110,  HAlign.Right),
                ("TotalSalesAmount",  "Total Sales",       130,  HAlign.Right),
                ("CommissionPercent", "Comm. %",           90,   HAlign.Right),
                ("CommissionAmount",  "Comm. Earned",      130,  HAlign.Right),
            };

            int index = 0;
            foreach (var col in columns)
            {
                if (band.Columns.Exists(col.Item1))
                {
                    var bandCol = band.Columns[col.Item1];
                    bandCol.Header.Caption = col.Item2;
                    bandCol.Width          = col.Item3;
                    bandCol.CellAppearance.TextHAlign = col.Item4;
                    bandCol.Header.VisiblePosition = index++;

                    // Format columns
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

            // Hide unused columns if any auto-generate
            foreach (UltraGridColumn col in band.Columns)
            {
                bool isPlanned = false;
                foreach (var pc in columns)
                {
                    if (pc.Item1 == col.Key) { isPlanned = true; break; }
                }
                if (!isPlanned) col.Hidden = true;
            }
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(66, 165, 245));
            StyleButton(btnReset,  Color.FromArgb(245, 124, 0),  Color.FromArgb(255, 152, 0),  Color.FromArgb(255, 167, 38));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107),  Color.FromArgb(0, 150, 136),  Color.FromArgb(38, 166, 154));
            StyleButton(btnPrint,  Color.FromArgb(81, 45, 168),  Color.FromArgb(103, 58, 183), Color.FromArgb(126, 87, 194));
            StyleButton(btnClose,  Color.FromArgb(198, 40, 40),  Color.FromArgb(244, 67, 54),  Color.FromArgb(229, 115, 115));
            
            // Standard small dialog buttons styling
            StyleDialogButton(btnSelectSalesman, Color.FromArgb(51, 65, 85));
            StyleDialogButton(btnClearSalesman, Color.FromArgb(100, 116, 139));
        }

        private void StyleButton(Infragistics.Win.Misc.UltraButton btn, Color c1, Color c2, Color hover)
        {
            btn.UseAppStyling  = false;
            btn.UseOsThemes    = DefaultableBoolean.False;
            btn.Appearance.BackColor            = c1;
            btn.Appearance.BackColor2           = c2;
            btn.Appearance.BackGradientStyle    = GradientStyle.Vertical;
            btn.Appearance.ForeColor            = Color.White;
            btn.Appearance.FontData.Bold        = DefaultableBoolean.True;
            btn.Appearance.FontData.SizeInPoints = 9;
            btn.HotTrackAppearance.BackColor    = hover;
            btn.HotTrackAppearance.ForeColor    = Color.White;
        }

        private void StyleDialogButton(Infragistics.Win.Misc.UltraButton btn, Color baseColor)
        {
            btn.UseAppStyling = false;
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = baseColor;
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.FontData.SizeInPoints = 8.5F;
            btn.HotTrackAppearance.BackColor = Color.FromArgb(15, 23, 42);
        }

        // ════════════════════════════════════════════════════════════
        //  Layout Summary Cards
        // ════════════════════════════════════════════════════════════
        private void LayoutSummaryCards()
        {
            var cards = new[]
            {
                (cardSalesmanCount,   lblSalesmanCountValue,   Color.FromArgb(25, 118, 210)),
                (cardInvoiceCount,    lblInvoiceCountValue,    Color.FromArgb(103, 58, 183)),
                (cardTotalQty,        lblTotalQtyValue,        Color.FromArgb(245, 124, 0)),
                (cardTotalSales,      lblTotalSalesValue,      Color.FromArgb(0, 150, 136)),
                (cardTotalCommission, lblTotalCommissionValue, Color.FromArgb(211, 47, 47))
            };

            int totalWidth = panelSummary.ClientArea.Width - 30; // 15px left/right margin
            int cardCount = cards.Length;
            int gap = 10;
            int cardWidth = (totalWidth - (gap * (cardCount - 1))) / cardCount;
            if (cardWidth < 160) cardWidth = 160;
            int cardHeight = 62;

            int x = 15, y = 10;
            foreach (var (card, val, valColor) in cards)
            {
                card.Location = new Point(x, y);
                card.Size = new Size(cardWidth, cardHeight);

                // Fix 1: Only create accent panels once to prevent duplicates on resize/reload
                if (!_accentPanelsCreated)
                {
                    var accentLine = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 3,
                        BackColor = valColor
                    };
                    card.ClientArea.Controls.Add(accentLine);
                    accentLine.BringToFront();
                }

                val.Appearance.ForeColor = valColor;
                x += cardWidth + gap;
            }
            _accentPanelsCreated = true;
        }

        private void SetupCardControls(Infragistics.Win.Misc.UltraPanel card, Infragistics.Win.Misc.UltraLabel caption, Infragistics.Win.Misc.UltraLabel value, string captionText, string cardName)
        {
            card.Name = cardName;
            card.Size = new System.Drawing.Size(180, 62);
            card.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            card.UseAppStyling = false;
            card.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            card.Appearance.BackColor = System.Drawing.Color.White;
            card.Appearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);

            // Caption
            caption.Text = captionText;
            caption.Location = new Point(12, 8);
            caption.Size = new System.Drawing.Size(160, 15);
            caption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            caption.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            caption.UseAppStyling = false;
            caption.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // Value label
            value.Location = new Point(12, 26);
            value.Size = new System.Drawing.Size(160, 28);
            value.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            value.Text = "0";
            value.UseAppStyling = false;
            value.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }

        // ════════════════════════════════════════════════════════════
        //  Event Handlers
        // ════════════════════════════════════════════════════════════
        private void FrmSalesmanwiseSalesSummaryReport_Load(object sender, EventArgs e)
        {
            InitializeDateControls();
            FetchFromDatabase();
        }

        private void InitializeDateControls()
        {
            _isChangingDates = true;
            try
            {
                DateTime now = DateTime.Now;
                dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                dtTo.Value   = now.Date;
            }
            finally
            {
                _isChangingDates = false;
            }
        }

        private void ComboPeriod_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangingDates) return;
            if (comboPeriod.Value == null) return;

            _isChangingDates = true;
            try
            {
                string val = comboPeriod.Value.ToString();
                DateTime now = DateTime.Now;
                switch (val)
                {
                    case "Today":
                        dtFrom.Value = now.Date;
                        dtTo.Value   = now.Date;
                        break;
                    case "This Week":
                        dtFrom.Value = now.Date.AddDays(-(int)now.DayOfWeek);
                        dtTo.Value   = now.Date;
                        break;
                    case "This Month":
                        dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                        dtTo.Value   = now.Date;
                        break;
                    case "Last Month":
                        var lm = now.AddMonths(-1);
                        dtFrom.Value = new DateTime(lm.Year, lm.Month, 1);
                        dtTo.Value   = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                        break;
                    case "This Quarter":
                        int q = (now.Month - 1) / 3;
                        dtFrom.Value = new DateTime(now.Year, q * 3 + 1, 1);
                        dtTo.Value   = now.Date;
                        break;
                    case "This Year":
                        dtFrom.Value = new DateTime(now.Year, 1, 1);
                        dtTo.Value   = now.Date;
                        break;
                }
            }
            finally
            {
                _isChangingDates = false;
            }

            // Fix 4: Auto-search when period preset changes
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

        private void BtnSelectSalesman_Click(object sender, EventArgs e)
        {
            using (var dlg = new frmSalesPersonDial())
            {
                string selectedName = string.Empty;
                dlg.OnSalesPersonSelected += (name) =>
                {
                    selectedName = name;
                };

                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                
                if (!string.IsNullOrEmpty(selectedName))
                {
                    // Get list of users to resolve name to ID
                    var salesmanList = _dropdownRepo.getUsersDDl()?.List;
                    if (salesmanList != null)
                    {
                        var user = salesmanList.FirstOrDefault(u => string.Equals(u.UserName, selectedName, StringComparison.OrdinalIgnoreCase));
                        if (user != null)
                        {
                            _selectedSalesmanId = user.UserID;
                            _selectedSalesmanName = user.UserName;
                            txtSalesmanName.Text = _selectedSalesmanName;
                            
                            // Auto Search
                            FetchFromDatabase();
                        }
                    }
                }
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

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _selectedSalesmanId   = null;
            _selectedSalesmanName = string.Empty;
            txtSalesmanName.Text  = string.Empty;
            txtSearch.Text        = string.Empty;
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

        // ════════════════════════════════════════════════════════════
        //  Asynchronous DB Fetching
        // ════════════════════════════════════════════════════════════
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
            // Fix 5: Guard against updating disposed controls after form close
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
                    r.SalesmanName.ToLower().Contains(searchVal) ||
                    r.Email.ToLower().Contains(searchVal)
                ).ToList();
            }

            gridReport.DataSource = filtered;
            CalculateSummaryValues(filtered);
            lblStatus.Text = $"Ready  |  Found {filtered.Count} records.";
        }

        private void CalculateSummaryValues(List<SalesmanwiseSalesSummaryItem> items)
        {
            if (items == null || items.Count == 0)
            {
                lblSalesmanCountValue.Text   = "0";
                lblInvoiceCountValue.Text    = "0";
                lblTotalQtyValue.Text        = "0.00";
                lblTotalSalesValue.Text      = "0.00";
                lblTotalCommissionValue.Text = "0.00";
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
            lblTotalSalesValue.Text      = totalSales.ToString("N2");
            lblTotalCommissionValue.Text = totalCommission.ToString("N2");
        }

        // ════════════════════════════════════════════════════════════
        //  CSV Export & Print Operations
        // ════════════════════════════════════════════════════════════
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

                    // Add summary total row at bottom
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
    }
}
