using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmDayBook : Form
    {
        #region Styling Constants
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private static readonly Color ReceiptColor = Color.FromArgb(27, 94, 32);   // Dark Green
        private static readonly Color PaymentColor = Color.FromArgb(183, 28, 28);  // Dark Red
        #endregion

        #region Private Fields
        private DayBookResponse _currentReportData = new DayBookResponse();
        private DataSet _dsDayBook;
        private bool _gridGroupedMode;
        private bool _isSelectionHidden = false;
        #endregion

        #region Constructor
        public FrmDayBook()
        {
            InitializeComponent();

            // Setup Panels, Docking and Z-Order immediately in constructor
            InitializePanels();

            this.Load += FrmDayBook_Load;
            this.KeyPreview = true;
            this.KeyDown += FrmDayBook_KeyDown;

            // Wire Actions
            btnGenerate.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPrint.Click += (s, e) => PrintReport();
            btnExportCsv.Click += (s, e) => ExportCsv();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            cmbDateQuickSelect.ValueChanged += CmbDateQuickSelect_ValueChanged;
            txtSearch.ValueChanged += TxtSearch_ValueChanged;
            chkGroupByVoucher.CheckedChanged += ChkGroupByVoucher_CheckedChanged;

            // UltraGrid events
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            ultraGridTransactions.DoubleClickRow += UltraGridTransactions_DoubleClickRow;
        }
        #endregion

        #region Form Lifecycle & Presets
        private void FrmDayBook_Load(object sender, EventArgs e)
        {
            PopulatePresetCombo();

            // Default preset is TODAY for Day Book
            cmbDateQuickSelect.Value = "TODAY";
            ApplyDatePreset("TODAY");

            // Auto-load today's records
            LoadData();
        }

        private void PopulatePresetCombo()
        {
            cmbDateQuickSelect.Items.Clear();
            cmbDateQuickSelect.Items.Add("TODAY", "Today");
            cmbDateQuickSelect.Items.Add("YESTERDAY", "Yesterday");
            cmbDateQuickSelect.Items.Add("THIS_MONTH", "This Month");
            cmbDateQuickSelect.Items.Add("LAST_MONTH", "Last Month");
            cmbDateQuickSelect.Items.Add("CURRENT_FY", "Current Financial Year");
            cmbDateQuickSelect.Items.Add("ALL", "ALL (Full History)");
            cmbDateQuickSelect.Items.Add("DATE_RANGE", "Date by Range");
        }

        private void CmbDateQuickSelect_ValueChanged(object sender, EventArgs e)
        {
            string presetKey = cmbDateQuickSelect.Value?.ToString() ?? cmbDateQuickSelect.Text;
            ApplyDatePreset(presetKey);
        }

        private void ApplyDatePreset(string presetKey)
        {
            DateTime now = DateTime.Today;

            switch (presetKey)
            {
                case "TODAY":
                    dtFromDate.DateTime = now;
                    dtToDate.DateTime = now;
                    dtFromDate.Enabled = false;
                    dtToDate.Enabled = false;
                    break;

                case "YESTERDAY":
                    dtFromDate.DateTime = now.AddDays(-1);
                    dtToDate.DateTime = now.AddDays(-1);
                    dtFromDate.Enabled = false;
                    dtToDate.Enabled = false;
                    break;

                case "THIS_MONTH":
                    dtFromDate.DateTime = new DateTime(now.Year, now.Month, 1);
                    dtToDate.DateTime = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                    dtFromDate.Enabled = false;
                    dtToDate.Enabled = false;
                    break;

                case "LAST_MONTH":
                    var lm = now.AddMonths(-1);
                    dtFromDate.DateTime = new DateTime(lm.Year, lm.Month, 1);
                    dtToDate.DateTime = new DateTime(lm.Year, lm.Month, DateTime.DaysInMonth(lm.Year, lm.Month));
                    dtFromDate.Enabled = false;
                    dtToDate.Enabled = false;
                    break;

                case "CURRENT_FY":
                    int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
                    dtFromDate.DateTime = new DateTime(startYear, 4, 1);
                    dtToDate.DateTime = new DateTime(startYear + 1, 3, 31);
                    dtFromDate.Enabled = false;
                    dtToDate.Enabled = false;
                    break;

                case "ALL":
                    dtFromDate.DateTime = new DateTime(1990, 1, 1);
                    dtToDate.DateTime = now;
                    dtFromDate.Enabled = false;
                    dtToDate.Enabled = false;
                    break;

                case "DATE_RANGE":
                default:
                    dtFromDate.Enabled = true;
                    dtToDate.Enabled = true;
                    break;
            }
        }

        public void RibbonClear()
        {
            cmbDateQuickSelect.Value = "TODAY";
            ApplyDatePreset("TODAY");
            txtSearch.Text = string.Empty;
            chkGroupByVoucher.Checked = false;
            _currentReportData = new DayBookResponse();
            _dsDayBook = null;
            ultraGridTransactions.DataSource = null;
            ClearSummary();
        }

        public void Clear() => RibbonClear();

        private void ResetFilters()
        {
            txtSearch.Text = string.Empty;
            chkGroupByVoucher.Checked = false;
            cmbDateQuickSelect.Value = "TODAY";
            ApplyDatePreset("TODAY");
            LoadData();
        }
        #endregion

        #region Theme & UI Styling
        private void InitializePanels()
        {
            this.BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelControls.Height = 78;

            // Labels
            lblSearch.Appearance.ForeColor = ControlTextColor;
            lblRowCount.Appearance.ForeColor = ControlTextColor;
            lblRowCount.Appearance.FontData.SizeInPoints = 8.5f;
            lblPreset.Appearance.ForeColor = ControlTextColor;
            lblFromDate.Appearance.ForeColor = ControlTextColor;
            lblToDate.Appearance.ForeColor = ControlTextColor;

            // Action Panel
            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelAction.Dock = DockStyle.Top;
            ultraPanelAction.Height = 45;

            // Master Panel
            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelMaster.Dock = DockStyle.Fill;

            // Footer Panel (Dock: Bottom, Height: 34)
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Dock = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 34;

            // Grid (Dock: Fill)
            ultraGridTransactions.Dock = DockStyle.Fill;

            // Dock & Z-Order (Proven standard matching Trial Balance & Cash/Bank Book)
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridTransactions.BringToFront();

            // Footer Labels
            lblTotalDebits.Appearance.ForeColor = Color.FromArgb(220, 255, 220);
            lblTotalDebits.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalDebits.Appearance.FontData.SizeInPoints = 8.5f;

            lblTotalCredits.Appearance.ForeColor = Color.FromArgb(255, 220, 220);
            lblTotalCredits.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalCredits.Appearance.FontData.SizeInPoints = 8.5f;

            SetNetBadgeState(0, 0);
            StyleButtons();
            SetupGridAppearance();
            UpdateSelectionToggleButtonText();
        }

        private void SetNetBadgeState(decimal netAmount, int voucherCount)
        {
            lblNetBadge.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            lblNetBadge.Appearance.BackColor2 = Color.FromArgb(200, 230, 201);
            lblNetBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
            lblNetBadge.Appearance.BorderColor = Color.FromArgb(46, 125, 50);
            lblNetBadge.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblNetBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetBadge.Appearance.FontData.SizeInPoints = 9.5f;

            string netStr = netAmount >= 0 
                ? $"NET: ₹ {netAmount:N2} Dr ({voucherCount} Vouchers)" 
                : $"NET: ₹ {Math.Abs(netAmount):N2} Cr ({voucherCount} Vouchers)";

            lblNetBadge.Text = netStr;
        }

        private void StyleButtons()
        {
            btnGenerate.Text = "View Grid";
            btnPreviewGrid.Text = "Preview Grid";
            btnPrint.Text = "Preview Report";
            btnExportCsv.Text = "Export Grid";
            btnClearFilters.Text = "Reset Filters";
            btnToggleSelection.Text = "Hide Selection";

            StyleClassicButton(btnGenerate);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnExportCsv);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnToggleSelection);
        }

        private static void StyleClassicButton(UltraButton button)
        {
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            button.Appearance.BackColor = Color.FromArgb(240, 248, 255);
            button.Appearance.BackColor2 = Color.FromArgb(196, 222, 245);
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = BorderBlue;
            button.Appearance.ForeColor = ButtonTextBlue;

            button.HotTrackAppearance.BackColor = Color.FromArgb(223, 240, 255);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(173, 209, 240);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = Color.FromArgb(43, 107, 168);
            button.HotTrackAppearance.ForeColor = Color.FromArgb(10, 35, 75);

            button.PressedAppearance.BackColor = Color.FromArgb(180, 213, 242);
            button.PressedAppearance.BackColor2 = Color.FromArgb(150, 192, 228);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(28, 80, 135);
        }

        private void SetupGridAppearance()
        {
            var layout = ultraGridTransactions.DisplayLayout;
            layout.GroupByBox.Hidden = true;
            layout.CaptionVisible = DefaultableBoolean.False;

            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;

            var headerApp = layout.Override.HeaderAppearance;
            headerApp.BackColor = GridHeaderBlue;
            headerApp.BackColor2 = GridHeaderBlueDark;
            headerApp.BackGradientStyle = GradientStyle.Vertical;
            headerApp.ForeColor = Color.White;
            headerApp.FontData.Bold = DefaultableBoolean.True;
            headerApp.FontData.SizeInPoints = 9f;
            headerApp.TextHAlign = HAlign.Center;
            headerApp.ThemedElementAlpha = Alpha.Transparent;

            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.Override.RowAppearance.BorderColor = Color.FromArgb(197, 217, 241);
        }
        #endregion

        #region UltraGrid Configuration
        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (_gridGroupedMode)
            {
                if (e.Layout.Bands.Count > 1)
                {
                    UltraGridBand masterBand = FindBand(e.Layout, "VoucherMaster", 0);
                    UltraGridBand detailBand = FindBand(e.Layout, "VoucherLines", 1);
                    if (masterBand != null) ConfigureGroupedMasterBand(masterBand);
                    if (detailBand != null) ConfigureGroupedDetailBand(detailBand);
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
            if (layout == null || layout.Bands.Count == 0) return null;

            foreach (UltraGridBand band in layout.Bands)
            {
                if (string.Equals(band.Key, preferredKey, StringComparison.OrdinalIgnoreCase))
                    return band;
            }

            if (fallbackIndex >= 0 && fallbackIndex < layout.Bands.Count)
                return layout.Bands[fallbackIndex];

            return null;
        }

        private void ConfigureDetailedBand(UltraGridBand band)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "VoucherDate", "Date", 105, HAlign.Center);
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

            ConfigureColumn(band, "Particulars", "Particulars / Account", 260, HAlign.Left);
            ConfigureColumn(band, "Narration", "Narration", 280, HAlign.Left);

            ConfigureAmountColumn(band, "DebitAmount", "Debit (Dr) ₹", ReceiptColor);
            ConfigureAmountColumn(band, "CreditAmount", "Credit (Cr) ₹", PaymentColor);

            band.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridTransactions.DisplayLayout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void ConfigureGroupedMasterBand(UltraGridBand band)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "VoucherDate", "Date", 105, HAlign.Center);
            if (band.Columns.Exists("VoucherDate"))
            {
                band.Columns["VoucherDate"].Format = "dd-MMM-yyyy";
            }

            ConfigureColumn(band, "VoucherID", "Voucher ID", 90, HAlign.Center);
            ConfigureColumn(band, "VoucherTypeName", "Type", 110, HAlign.Left);
            ConfigureColumn(band, "Narration", "Narration", 320, HAlign.Left);
            ConfigureAmountColumn(band, "DebitTotal", "Debit (Dr) ₹", ReceiptColor);
            ConfigureAmountColumn(band, "CreditTotal", "Credit (Cr) ₹", PaymentColor);

            band.Override.AllowColSizing = AllowColSizing.Free;
            ultraGridTransactions.DisplayLayout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void ConfigureGroupedDetailBand(UltraGridBand band)
        {
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            if (band.Columns.Exists("VoucherID")) band.Columns["VoucherID"].Hidden = true;
            if (band.Columns.Exists("LineID")) band.Columns["LineID"].Hidden = true;

            ConfigureColumn(band, "Particulars", "Particulars", 300, HAlign.Left);
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

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.IsDataRow || e.Row.Band == null) return;

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

        private void StyleAmountCell(UltraGridRow row, string columnKey, Color activeColor)
        {
            if (row?.Band == null || !row.Band.Columns.Exists(columnKey)) return;

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
        #endregion

        #region Data Loading & Grouping
        private void LoadData()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var repo = new DayBookRepository();
                var reportData = repo.GetDayBook(dtFromDate.DateTime.Date, dtToDate.DateTime.Date);
                _currentReportData = reportData ?? new DayBookResponse();

                _gridGroupedMode = chkGroupByVoucher.Checked;
                ApplyGroupByVoucher();

                UpdateSummaryForSearch(txtSearch.Text.Trim().ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Day Book data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ChkGroupByVoucher_CheckedChanged(object sender, EventArgs e)
        {
            ApplyGroupByVoucher();
        }

        private void EnsureDayBookDataSet()
        {
            if (_dsDayBook != null) return;

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
            _gridGroupedMode = groupByVoucher;

            ultraGridTransactions.DisplayLayout.Reset();

            if (groupByVoucher)
            {
                SetupGridAppearance();
                PopulateDayBookDataSet(GetFilteredTransactions());
                ultraGridTransactions.DataSource = _dsDayBook;
                ultraGridTransactions.DataMember = "VoucherMaster";
            }
            else
            {
                SetupGridAppearance();
                ultraGridTransactions.DataSource = GetFilteredTransactions();
                ultraGridTransactions.DataMember = string.Empty;
            }

            ultraGridTransactions.DataBind();
        }
        #endregion

        #region Search & Filtering
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

            ultraGridTransactions.DataSource = GetFilteredTransactions();
            ultraGridTransactions.DataBind();
            UpdateSummaryForSearch(filterText);
        }

        private void UpdateSummaryForSearch(string filterText)
        {
            var transactions = GetFilteredTransactions();

            decimal totalDebits = transactions.Sum(t => t.DebitAmount);
            decimal totalCredits = transactions.Sum(t => t.CreditAmount);
            decimal net = totalDebits - totalCredits;
            int distinctVouchers = transactions.Select(t => t.VoucherID).Distinct().Count();

            lblTotalDebits.Text = $"Total Debits (Dr): ₹ {totalDebits:N2}";
            lblTotalCredits.Text = $"Total Credits (Cr): ₹ {totalCredits:N2}";
            SetNetBadgeState(net, distinctVouchers);

            lblRowCount.Text = $"Total: {transactions.Count} rows ({distinctVouchers} vouchers)";
        }

        private void ClearSummary()
        {
            lblTotalDebits.Text = "Total Debits (Dr): ₹ 0.00";
            lblTotalCredits.Text = "Total Credits (Cr): ₹ 0.00";
            SetNetBadgeState(0, 0);
            lblRowCount.Text = "Total: 0 rows";
        }

        private bool IsTransactionVisibleForSearch(DayBookTransaction transaction, string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText)) return true;

            return ContainsSearchText(transaction.Particulars, filterText)
                || ContainsSearchText(transaction.Narration, filterText)
                || ContainsSearchText(transaction.VoucherTypeName, filterText)
                || transaction.VoucherID.ToString().Contains(filterText);
        }

        private static bool ContainsSearchText(string value, string filterText)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion

        #region Drill-Down, Export & Print
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
                UltraGridRow voucherRow = IsVoucherLinesBand(e.Row.Band) && e.Row.ParentRow != null
                    ? e.Row.ParentRow
                    : e.Row;

                string voucherType = voucherRow.Band != null && voucherRow.Band.Columns.Exists("VoucherTypeName")
                    ? voucherRow.Cells["VoucherTypeName"].Value?.ToString() ?? ""
                    : "";
                int voucherId = voucherRow.Band != null && voucherRow.Band.Columns.Exists("VoucherID")
                    ? Convert.ToInt32(voucherRow.Cells["VoucherID"].Value ?? 0)
                    : 0;

                if (voucherId == 0) return;

                if (voucherType.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down]\nOpening Sales Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Payment", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down]\nOpening Payment Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (voucherType.Equals("Receipt", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"[Drill-Down]\nOpening Receipt Voucher #{voucherId}", "Drill-Down", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void PreviewGrid()
        {
            if (ultraGridTransactions.Rows.Count == 0)
            {
                MessageBox.Show("No data to preview.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var printDoc = new UltraGridPrintDocument();
                printDoc.Grid = this.ultraGridTransactions;
                printDoc.Header.TextCenter = $"DAY BOOK REPORT\nPeriod: {dtFromDate.DateTime:dd-MMM-yyyy} to {dtToDate.DateTime:dd-MMM-yyyy}\n\n";

                using (var previewDialog = new PrintPreviewDialog())
                {
                    previewDialog.Document = printDoc;
                    previewDialog.Width = 1000;
                    previewDialog.Height = 700;
                    previewDialog.StartPosition = FormStartPosition.CenterScreen;
                    previewDialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying print preview: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReport()
        {
            PreviewGrid();
        }

        private void ExportCsv()
        {
            var transactions = GetFilteredTransactions();
            if (transactions.Count == 0)
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
                            writer.WriteLine("Date,Voucher ID,Type,Particulars,Narration,Debit (Dr),Credit (Cr)");

                            foreach (var t in transactions)
                            {
                                var line = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5:F2}\",\"{6:F2}\"",
                                    t.VoucherDate.ToString("dd-MMM-yyyy"),
                                    t.VoucherID,
                                    t.VoucherTypeName,
                                    t.Particulars?.Replace("\"", "\"\""),
                                    t.Narration?.Replace("\"", "\"\""),
                                    t.DebitAmount,
                                    t.CreditAmount);
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

        private void ToggleSelectionPanel()
        {
            _isSelectionHidden = !_isSelectionHidden;
            ultraPanelControls.Visible = !_isSelectionHidden;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = _isSelectionHidden ? "Show Selection" : "Hide Selection";
        }

        private void FrmDayBook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Close();
            else if (e.KeyCode == Keys.F5) LoadData();
            else if (e.Control && e.KeyCode == Keys.E) ExportCsv();
            else if (e.Control && e.KeyCode == Keys.P) PreviewGrid();
        }
        #endregion
    }
}
