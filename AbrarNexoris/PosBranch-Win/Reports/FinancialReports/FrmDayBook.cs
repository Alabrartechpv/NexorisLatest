using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmDayBook : Form
    {
        private static readonly Color HeaderGradStart = Color.FromArgb(45, 45, 48);
        private static readonly Color HeaderGradEnd = Color.FromArgb(62, 62, 66);
        private static readonly Color ReceiptColor = Color.FromArgb(46, 125, 50); // Green
        private static readonly Color PaymentColor = Color.FromArgb(198, 40, 40); // Red
        private static readonly Color SelectedRowColor = Color.FromArgb(227, 242, 253);
        private DayBookResponse _currentReportData = new DayBookResponse();
        private DataSet _dsDayBook;
        private bool _gridGroupedMode;

        public FrmDayBook()
        {
            InitializeComponent();
            SetupGrid();
            StyleSummaryPanels();
            StyleButtons();
            
            // Events
            this.Load += FrmDayBook_Load;
            btnGenerate.Click += BtnGenerate_Click;
            btnExportCsv.Click += BtnExportCsv_Click;
            btnPrint.Click += BtnPrint_Click;
            btnClose.Click += (s, e) => this.Close();

            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;
            txtSearch.ValueChanged += TxtSearch_ValueChanged;
            chkGroupByVoucher.CheckedChanged += ChkGroupByVoucher_CheckedChanged;

            // UltraGrid events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;

            // Keyboard Shortcuts
            this.KeyPreview = true;
            this.KeyDown += FrmDayBook_KeyDown;
        }

        private void FrmDayBook_Load(object sender, EventArgs e)
        {
            dtFromDate.DateTime = DateTime.Today;
            dtToDate.DateTime = DateTime.Today;
            cmbDateQuickSelect.Text = "Today";
        }

        private void FrmDayBook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            if (e.KeyCode == Keys.F5) BtnGenerate_Click(null, null);
            if (e.Control && e.KeyCode == Keys.P) BtnPrint_Click(null, null);
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            string sel = cmbDateQuickSelect.Text;
            DateTime now = DateTime.Today;

            if (sel == "Today")
            {
                dtFromDate.DateTime = now;
                dtToDate.DateTime = now;
            }
            else if (sel == "Yesterday")
            {
                dtFromDate.DateTime = now.AddDays(-1);
                dtToDate.DateTime = now.AddDays(-1);
            }
            else if (sel == "This Month")
            {
                dtFromDate.DateTime = new DateTime(now.Year, now.Month, 1);
                dtToDate.DateTime = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            }
            else if (sel == "Last Month")
            {
                var lm = now.AddMonths(-1);
                dtFromDate.DateTime = new DateTime(lm.Year, lm.Month, 1);
                dtToDate.DateTime = new DateTime(lm.Year, lm.Month, DateTime.DaysInMonth(lm.Year, lm.Month));
            }
            else if (sel == "This Financial Year")
            {
                int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
                dtFromDate.DateTime = new DateTime(startYear, 4, 1);
                dtToDate.DateTime = new DateTime(startYear + 1, 3, 31);
            }
        }

        #region UltraGrid Configuration

        private void SetupGrid()
        {
            var displayLayout = ultraGridTransactions.DisplayLayout;
            displayLayout.ViewStyleBand = ViewStyleBand.OutlookGroupBy;
            displayLayout.GroupByBox.Hidden = true; // Clean interface
            displayLayout.CaptionVisible = DefaultableBoolean.False;
            
            // Selection Style
            displayLayout.Override.SelectTypeRow = SelectType.Single;
            displayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            displayLayout.Override.SelectedRowAppearance.BackColor = SelectedRowColor;
            displayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;

            // Header Style
            var headerApp = displayLayout.Override.HeaderAppearance;
            headerApp.BackColor = HeaderGradStart;
            headerApp.BackColor2 = HeaderGradEnd;
            headerApp.BackGradientStyle = GradientStyle.Vertical;
            headerApp.ForeColor = Color.White;
            headerApp.FontData.Bold = DefaultableBoolean.True;
            headerApp.FontData.SizeInPoints = 9f;
            headerApp.TextHAlign = HAlign.Center;
            headerApp.ThemedElementAlpha = Alpha.Transparent;
            
            displayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            displayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            displayLayout.Override.RowAppearance.BorderColor = Color.LightGray;
        }

        private void SetupGroupedGrid()
        {
            var displayLayout = ultraGridTransactions.DisplayLayout;
            displayLayout.ViewStyleBand = ViewStyleBand.Vertical;
            displayLayout.GroupByBox.Hidden = true;
            displayLayout.CaptionVisible = DefaultableBoolean.False;

            displayLayout.Override.SelectTypeRow = SelectType.Single;
            displayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            displayLayout.Override.SelectedRowAppearance.BackColor = SelectedRowColor;
            displayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;
            displayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            displayLayout.Override.RowSelectors = DefaultableBoolean.True;

            var headerApp = displayLayout.Override.HeaderAppearance;
            headerApp.BackColor = HeaderGradStart;
            headerApp.BackColor2 = HeaderGradEnd;
            headerApp.BackGradientStyle = GradientStyle.Vertical;
            headerApp.ForeColor = Color.White;
            headerApp.FontData.Bold = DefaultableBoolean.True;
            headerApp.FontData.SizeInPoints = 9f;
            headerApp.TextHAlign = HAlign.Center;

            displayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            displayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            displayLayout.Override.RowAppearance.BorderColor = Color.LightGray;
        }

        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (_gridGroupedMode)
            {
                if (e.Layout.Bands.Count > 1)
                {
                    UltraGridBand masterBand = FindBand(e.Layout, "VoucherMaster", 0);
                    UltraGridBand detailBand = FindBand(e.Layout, "VoucherLines", 1);
                    if (masterBand != null)
                    {
                        ConfigureGroupedMasterBand(masterBand);
                    }
                    if (detailBand != null)
                    {
                        ConfigureGroupedDetailBand(detailBand);
                    }
                }
                return;
            }

            if (e.Layout.Bands.Count > 0)
            {
                ConfigureDetailedBand(e.Layout.Bands[0]);
            }
        }

        private static UltraGridBand FindBand(UltraGridLayout layout, string preferredKey, int fallbackIndex)
        {
            if (layout == null || layout.Bands.Count == 0)
            {
                return null;
            }

            foreach (UltraGridBand band in layout.Bands)
            {
                if (string.Equals(band.Key, preferredKey, StringComparison.OrdinalIgnoreCase))
                {
                    return band;
                }
            }

            if (fallbackIndex >= 0 && fallbackIndex < layout.Bands.Count)
            {
                return layout.Bands[fallbackIndex];
            }

            return null;
        }

        private void ConfigureDetailedBand(UltraGridBand band)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            if (!band.Columns.Exists("IconCol"))
            {
                var iconCol = band.Columns.Add("IconCol", "");
                iconCol.DataType = typeof(string);
                iconCol.Header.VisiblePosition = 0;
                iconCol.Width = 30;
                iconCol.CellAppearance.TextHAlign = HAlign.Center;
            }

            ConfigureColumn(band, "VoucherDate", "Date", 100, HAlign.Center);
            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
            }

            ConfigureColumn(band, "VoucherID", "Voucher ID", 90, HAlign.Center);
            ConfigureColumn(band, "VoucherTypeName", "Type", 110, HAlign.Left);
            if (band.Columns.Exists("VoucherTypeName"))
            {
                band.Columns["VoucherTypeName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["VoucherTypeName"].CellAppearance.ForeColor = Color.DarkSlateGray;
            }

            ConfigureColumn(band, "Particulars", "Particulars", 250, HAlign.Left);
            ConfigureColumn(band, "Narration", "Narration", 300, HAlign.Left);

            ConfigureAmountColumn(band, "DebitAmount", "Debit (Dr) ₹", ReceiptColor);
            ConfigureAmountColumn(band, "CreditAmount", "Credit (Cr) ₹", PaymentColor);

            band.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridTransactions.DisplayLayout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
            AddBandSummaries(band, "DebitAmount", "CreditAmount", "TotalDebits", "TotalCredits");
        }

        private void ConfigureGroupedMasterBand(UltraGridBand band)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "VoucherDate", "Date", 100, HAlign.Center);
            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
            }

            ConfigureColumn(band, "VoucherID", "Voucher ID", 90, HAlign.Center);
            ConfigureColumn(band, "VoucherTypeName", "Type", 110, HAlign.Left);
            ConfigureColumn(band, "Narration", "Narration", 300, HAlign.Left);
            ConfigureAmountColumn(band, "DebitTotal", "Debit (Dr) ₹", ReceiptColor);
            ConfigureAmountColumn(band, "CreditTotal", "Credit (Cr) ₹", PaymentColor);

            band.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridTransactions.DisplayLayout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
            AddBandSummaries(band, "DebitTotal", "CreditTotal", "GrpTotalDebits", "GrpTotalCredits");
        }

        private void ConfigureGroupedDetailBand(UltraGridBand band)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            if (band.Columns.Exists("VoucherID"))
            {
                band.Columns["VoucherID"].Hidden = true;
            }
            if (band.Columns.Exists("LineID"))
            {
                band.Columns["LineID"].Hidden = true;
            }

            ConfigureColumn(band, "Particulars", "Particulars", 280, HAlign.Left);
            ConfigureAmountColumn(band, "DebitAmount", "Debit (Dr) ₹", ReceiptColor);
            ConfigureAmountColumn(band, "CreditAmount", "Credit (Cr) ₹", PaymentColor);

            band.Override.RowAppearance.BackColor = Color.FromArgb(252, 252, 255);
            band.Override.HeaderAppearance.BackColor = Color.FromArgb(69, 90, 100);
            band.Override.HeaderAppearance.ForeColor = Color.White;
        }

        private void ConfigureAmountColumn(UltraGridBand band, string key, string headerText, Color color)
        {
            ConfigureColumn(band, key, headerText, 130, HAlign.Right);
            if (band.Columns.Exists(key))
            {
                band.Columns[key].Format = "N2";
                band.Columns[key].CellAppearance.ForeColor = color;
                band.Columns[key].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void AddBandSummaries(UltraGridBand band, string debitKey, string creditKey, string debitSummaryKey, string creditSummaryKey)
        {
            band.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;
            band.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;

            if (band.Columns.Exists(debitKey) && !band.Summaries.Exists(debitSummaryKey))
            {
                var s = band.Summaries.Add(debitSummaryKey, SummaryType.Sum, band.Columns[debitKey]);
                s.DisplayFormat = "₹ {0:N2}";
                s.Appearance.TextHAlign = HAlign.Right;
                s.Appearance.FontData.Bold = DefaultableBoolean.True;
                s.Appearance.ForeColor = ReceiptColor;
            }

            if (band.Columns.Exists(creditKey) && !band.Summaries.Exists(creditSummaryKey))
            {
                var s = band.Summaries.Add(creditSummaryKey, SummaryType.Sum, band.Columns[creditKey]);
                s.DisplayFormat = "₹ {0:N2}";
                s.Appearance.TextHAlign = HAlign.Right;
                s.Appearance.FontData.Bold = DefaultableBoolean.True;
                s.Appearance.ForeColor = PaymentColor;
            }
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.IsDataRow || e.Row.Band == null)
            {
                return;
            }

            if (IsVoucherMasterBand(e.Row.Band))
            {
                StyleAmountCell(e.Row, "DebitTotal", ReceiptColor);
                StyleAmountCell(e.Row, "CreditTotal", PaymentColor);
                return;
            }

            if (IsVoucherLinesBand(e.Row.Band))
            {
                StyleAmountCell(e.Row, "DebitAmount", ReceiptColor);
                StyleAmountCell(e.Row, "CreditAmount", PaymentColor);
                return;
            }

            if (e.Row.Band.Columns.Exists("IconCol")
                && e.Row.Band.Columns.Exists("DebitAmount")
                && e.Row.Band.Columns.Exists("CreditAmount"))
            {
                decimal debit = GetCellDecimal(e.Row, "DebitAmount");
                decimal credit = GetCellDecimal(e.Row, "CreditAmount");

                if (debit > 0 && credit == 0)
                {
                    e.Row.Cells["IconCol"].Value = "💰";
                }
                else if (credit > 0 && debit == 0)
                {
                    e.Row.Cells["IconCol"].Value = "💸";
                }
                else if (debit > 0 && credit > 0)
                {
                    e.Row.Cells["IconCol"].Value = "";
                }
            }

            StyleAmountCell(e.Row, "DebitAmount", ReceiptColor);
            StyleAmountCell(e.Row, "CreditAmount", PaymentColor);
        }

        private static bool IsVoucherMasterBand(UltraGridBand band)
        {
            return band != null && band.Columns.Exists("DebitTotal");
        }

        private static bool IsVoucherLinesBand(UltraGridBand band)
        {
            return band != null
                && band.Columns.Exists("DebitAmount")
                && band.Columns.Exists("Particulars")
                && !band.Columns.Exists("DebitTotal");
        }

        private static decimal GetCellDecimal(UltraGridRow row, string columnKey)
        {
            if (row?.Band == null || !row.Band.Columns.Exists(columnKey))
            {
                return 0m;
            }

            var val = row.Cells[columnKey].Value;
            if (val == null || val == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(val);
        }

        private void StyleAmountCell(UltraGridRow row, string columnKey, Color activeColor)
        {
            if (row?.Band == null || !row.Band.Columns.Exists(columnKey))
            {
                return;
            }

            var val = row.Cells[columnKey].Value;
            if (val != null && val != DBNull.Value && Convert.ToDecimal(val) == 0)
            {
                row.Cells[columnKey].Appearance.ForeColor = Color.LightGray;
            }
            else
            {
                row.Cells[columnKey].Appearance.ForeColor = activeColor;
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string headerText, int width, HAlign align)
        {
            if (band.Columns.Exists(key))
            {
                var col = band.Columns[key];
                col.Hidden = false;
                col.Header.Caption = headerText;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
            }
        }

        #endregion

        #region View Modes (Detailed / Group By Voucher)

        private void ChkGroupByVoucher_CheckedChanged(object sender, EventArgs e)
        {
            ApplyGroupByVoucher();
        }

        private void EnsureDayBookDataSet()
        {
            if (_dsDayBook != null)
            {
                return;
            }

            _dsDayBook = new DataSet("DayBook");

            var master = new DataTable("VoucherMaster");
            master.Columns.Add("VoucherID", typeof(int));
            master.Columns.Add("VoucherDate", typeof(DateTime));
            master.Columns.Add("VoucherTypeName", typeof(string));
            master.Columns.Add("Narration", typeof(string));
            master.Columns.Add("DebitTotal", typeof(decimal));
            master.Columns.Add("CreditTotal", typeof(decimal));
            master.PrimaryKey = new[] { master.Columns["VoucherID"] };

            var detail = new DataTable("VoucherLines");
            detail.Columns.Add("LineID", typeof(int));
            detail.Columns["LineID"].AutoIncrement = true;
            detail.Columns["LineID"].AutoIncrementSeed = 1;
            detail.Columns["LineID"].AutoIncrementStep = 1;
            detail.Columns.Add("VoucherID", typeof(int));
            detail.Columns.Add("Particulars", typeof(string));
            detail.Columns.Add("DebitAmount", typeof(decimal));
            detail.Columns.Add("CreditAmount", typeof(decimal));

            _dsDayBook.Tables.Add(master);
            _dsDayBook.Tables.Add(detail);
            _dsDayBook.Relations.Add(
                "VoucherLines",
                master.Columns["VoucherID"],
                detail.Columns["VoucherID"]);
        }

        private void PopulateDayBookDataSet(IEnumerable<DayBookTransaction> transactions)
        {
            EnsureDayBookDataSet();

            // Child rows must be cleared before parent (FK: VoucherLines -> VoucherMaster)
            _dsDayBook.Tables["VoucherLines"].Rows.Clear();
            _dsDayBook.Tables["VoucherMaster"].Rows.Clear();

            foreach (var grp in transactions
                .GroupBy(t => t.VoucherID)
                .OrderBy(g => g.Min(t => t.VoucherDate))
                .ThenBy(g => g.Key))
            {
                var lines = grp.ToList();
                var first = lines[0];

                var masterRow = _dsDayBook.Tables["VoucherMaster"].NewRow();
                masterRow["VoucherID"] = first.VoucherID;
                masterRow["VoucherDate"] = first.VoucherDate;
                masterRow["VoucherTypeName"] = first.VoucherTypeName ?? string.Empty;
                masterRow["Narration"] = first.Narration ?? string.Empty;
                masterRow["DebitTotal"] = lines.Sum(l => l.DebitAmount);
                masterRow["CreditTotal"] = lines.Sum(l => l.CreditAmount);
                _dsDayBook.Tables["VoucherMaster"].Rows.Add(masterRow);

                foreach (var line in lines)
                {
                    var detailRow = _dsDayBook.Tables["VoucherLines"].NewRow();
                    detailRow["VoucherID"] = line.VoucherID;
                    detailRow["Particulars"] = line.Particulars ?? string.Empty;
                    detailRow["DebitAmount"] = line.DebitAmount;
                    detailRow["CreditAmount"] = line.CreditAmount;
                    _dsDayBook.Tables["VoucherLines"].Rows.Add(detailRow);
                }
            }
        }

        /// <summary>
        /// Flat view: only matching lines. Grouped view: whole voucher if any line matches (ERP-style audit).
        /// </summary>
        private List<DayBookTransaction> GetFilteredTransactions()
        {
            string filterText = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(filterText))
            {
                return _currentReportData.Transactions.ToList();
            }

            if (_gridGroupedMode || chkGroupByVoucher.Checked)
            {
                var voucherIds = new HashSet<int>(
                    _currentReportData.Transactions
                        .Where(t => IsTransactionVisibleForSearch(t, filterText))
                        .Select(t => t.VoucherID));

                return _currentReportData.Transactions
                    .Where(t => voucherIds.Contains(t.VoucherID))
                    .ToList();
            }

            return _currentReportData.Transactions
                .Where(t => IsTransactionVisibleForSearch(t, filterText))
                .ToList();
        }

        private void ApplyGroupByVoucher()
        {
            bool groupByVoucher = chkGroupByVoucher.Checked;
            if (groupByVoucher == _gridGroupedMode && ultraGridTransactions.DataSource != null)
            {
                if (groupByVoucher)
                {
                    PopulateDayBookDataSet(GetFilteredTransactions());
                    ultraGridTransactions.DataBind();
                }
                return;
            }

            ultraGridTransactions.DisplayLayout.Reset();

            if (groupByVoucher)
            {
                _gridGroupedMode = true;
                SetupGroupedGrid();
                PopulateDayBookDataSet(GetFilteredTransactions());
                ultraGridTransactions.DataSource = _dsDayBook;
                ultraGridTransactions.DataMember = "VoucherMaster";
            }
            else
            {
                _gridGroupedMode = false;
                SetupGrid();
                ultraGridTransactions.DataSource = _currentReportData.Transactions;
                ultraGridTransactions.DataMember = string.Empty;
            }

            ultraGridTransactions.DataBind();
        }

        #endregion

        #region Extra Features (Search & Drill-Down)

        private void TxtSearch_ValueChanged(object sender, EventArgs e)
        {
            string filterText = txtSearch.Text.Trim().ToLower();

            if (_gridGroupedMode)
            {
                PopulateDayBookDataSet(GetFilteredTransactions());
                ultraGridTransactions.DataBind();
                UpdateSummaryForSearch(filterText);
                return;
            }

            if (ultraGridTransactions.DisplayLayout.Bands.Count == 0)
            {
                UpdateSummaryForSearch(filterText);
                return;
            }

            var band = ultraGridTransactions.DisplayLayout.Bands[0];
            band.ColumnFilters.ClearAllFilters();

            if (!string.IsNullOrEmpty(filterText))
            {
                band.ColumnFilters.LogicalOperator = FilterLogicalOperator.Or;
                if (band.Columns.Exists("Particulars")) band.ColumnFilters["Particulars"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
                if (band.Columns.Exists("Narration")) band.ColumnFilters["Narration"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
                if (band.Columns.Exists("VoucherTypeName")) band.ColumnFilters["VoucherTypeName"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
                if (band.Columns.Exists("VoucherID")) band.ColumnFilters["VoucherID"].FilterConditions.Add(FilterComparisionOperator.Contains, filterText);
            }

            UpdateSummaryForSearch(filterText);
        }

        private void UltraGridTransactions_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (IsVoucherMasterBand(e.Row.Band))
            {
                e.Row.Expanded = !e.Row.Expanded;
                return;
            }

            try
            {
                string voucherType;
                int voucherId;

                UltraGridRow voucherRow = IsVoucherLinesBand(e.Row.Band) && e.Row.ParentRow != null
                    ? e.Row.ParentRow
                    : e.Row;

                voucherType = voucherRow.Band != null && voucherRow.Band.Columns.Exists("VoucherTypeName")
                    ? voucherRow.Cells["VoucherTypeName"].Value?.ToString() ?? ""
                    : "";
                voucherId = voucherRow.Band != null && voucherRow.Band.Columns.Exists("VoucherID")
                    ? Convert.ToInt32(voucherRow.Cells["VoucherID"].Value ?? 0)
                    : 0;

                if (voucherId == 0) return;

                // Open appropriate form based on type
                if (voucherType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Sales Voucher #{voucherId}\n(Routing to frmSalesInvoice...)", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Payment Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down Triggered]\nOpening Receipt Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Voucher Type '{voucherType}' (ID: {voucherId}) cannot be opened from here.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening voucher: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Summary Panel & Button Styling

        private void StyleSummaryPanels()
        {
            // Total Debits — soft green
            StyleSinglePanel(panelReceipts, lblTotalReceiptsTitle, lblTotalReceiptsValue, 
                Color.FromArgb(232, 245, 233), Color.FromArgb(46, 125, 50));
            lblTotalReceiptsTitle.Text = "Visible Debits:";
            
            // Total Credits — soft pink
            StyleSinglePanel(panelPayments, lblTotalPaymentsTitle, lblTotalPaymentsValue, 
                Color.FromArgb(252, 228, 236), Color.FromArgb(194, 24, 91));
            lblTotalPaymentsTitle.Text = "Visible Credits:";
        }

        private void StyleSinglePanel(Infragistics.Win.Misc.UltraPanel panel, Infragistics.Win.Misc.UltraLabel lblTitle, Infragistics.Win.Misc.UltraLabel lblVal, Color bgColor, Color fgColor)
        {
            panel.Appearance.BackColor = bgColor;
            panel.Appearance.BorderColor = Color.LightGray;
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            lblTitle.Appearance.ForeColor = fgColor;
            lblVal.Appearance.ForeColor = fgColor;
        }

        private void StyleButtons()
        {
            StyleSingleButton(btnGenerate, Color.FromArgb(21, 101, 192), Color.White); // Blue
            StyleSingleButton(btnExportCsv, Color.FromArgb(46, 125, 50), Color.White); // Green
            StyleSingleButton(btnPrint, Color.FromArgb(81, 45, 168), Color.White); // Purple
            StyleSingleButton(btnClose, Color.FromArgb(198, 40, 40), Color.White); // Red
        }

        private void StyleSingleButton(Infragistics.Win.Misc.UltraButton btn, Color bg, Color fg)
        {
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = bg;
            btn.Appearance.ForeColor = fg;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.BorderColor = bg;
            
            // Hover
            btn.HotTrackAppearance.BackColor = Color.FromArgb((int)(bg.R * 0.8), (int)(bg.G * 0.8), (int)(bg.B * 0.8));
            btn.HotTrackAppearance.ForeColor = fg;
        }

        #endregion

        #region Data Loading & Export

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var repo = new DayBookRepository();
                var reportData = repo.GetDayBook(dtFromDate.DateTime.Date, dtToDate.DateTime.Date);
                _currentReportData = reportData;

                _gridGroupedMode = !chkGroupByVoucher.Checked;
                ApplyGroupByVoucher();

                UpdateSummaryForSearch(txtSearch.Text.Trim().ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading report data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV Files|*.csv";
                    dialog.Title = "Save Day Book Export";
                    dialog.FileName = $"DayBook_{dtFromDate.DateTime:ddMMyyyy}_to_{dtToDate.DateTime:ddMMyyyy}.csv";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        using (StreamWriter writer = new StreamWriter(dialog.FileName))
                        {
                            // Write Headers
                            var headerLine = "Date,Voucher ID,Type,Particulars,Narration,Debit,Credit";
                            writer.WriteLine(headerLine);

                            string filterText = txtSearch.Text.Trim().ToLower();
                            foreach (var transaction in _currentReportData.Transactions)
                            {
                                if (!IsTransactionVisibleForSearch(transaction, filterText))
                                {
                                    continue;
                                }

                                var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                                    transaction.VoucherDate.ToString("dd-MMM-yyyy"),
                                    transaction.VoucherID,
                                    transaction.VoucherTypeName,
                                    transaction.Particulars?.Replace("\"", "\"\""),
                                    transaction.Narration?.Replace("\"", "\"\""),
                                    transaction.DebitAmount,
                                    transaction.CreditAmount);
                                writer.WriteLine(line);
                            }
                        }
                        MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (ultraGridTransactions.Rows.Count == 0 || ultraGridTransactions.DisplayLayout.Bands[0].Columns.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var printDoc = new Infragistics.Win.UltraWinGrid.UltraGridPrintDocument();
                printDoc.Grid = this.ultraGridTransactions;
                printDoc.Header.TextCenter = "DAY BOOK REPORT\n" + 
                    $"Period: {dtFromDate.DateTime:dd-MMM-yyyy} to {dtToDate.DateTime:dd-MMM-yyyy}\n\n";

                var previewDialog = new PrintPreviewDialog();
                previewDialog.Document = printDoc;
                previewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating print preview: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSummaryForSearch(string filterText)
        {
            IEnumerable<DayBookTransaction> transactions = (_gridGroupedMode || chkGroupByVoucher.Checked)
                ? GetFilteredTransactions()
                : _currentReportData.Transactions.Where(t => IsTransactionVisibleForSearch(t, filterText));

            decimal totalDebits = transactions.Sum(t => t.DebitAmount);
            decimal totalCredits = transactions.Sum(t => t.CreditAmount);

            lblTotalReceiptsValue.Text = totalDebits.ToString("N2");
            lblTotalPaymentsValue.Text = totalCredits.ToString("N2");
        }

        private bool IsTransactionVisibleForSearch(DayBookTransaction transaction, string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                return true;
            }

            return ContainsSearchText(transaction.Particulars, filterText)
                || ContainsSearchText(transaction.Narration, filterText)
                || ContainsSearchText(transaction.VoucherTypeName, filterText)
                || transaction.VoucherID.ToString().Contains(filterText);
        }

        private bool ContainsSearchText(string value, string filterText)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion
    }
}
