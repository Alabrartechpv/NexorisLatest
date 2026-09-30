using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmCustomerLedgerReport : Form
    {
        // ── Repository ─────────────────────────────────────────────
        private readonly CustomerLedgerReportRepository _repository;

        // ── Data ────────────────────────────────────────────────────
        private List<CustomerLedgerReportRow> _allFetchedRows;   // raw DB result
        private List<CustomerLedgerReportRow> _reportRows;        // after text filter

        // ── Selected customer (set via dialog) ───────────────────────
        private int    _selectedCustomerId;
        private string _selectedCustomerName;

        // ── Summary ─────────────────────────────────────────────────
        private decimal _openingBalance;
        private decimal _totalDebit;
        private decimal _totalCredit;
        private decimal _closingBalance;

        // ── State ───────────────────────────────────────────────────
        private bool     _isLoading;
        private DateTime _lastRefreshed;

        // ── Theme palette ────────────────────────────────────────────
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

        // ════════════════════════════════════════════════════════════
        public frmCustomerLedgerReport()
        {
            _repository     = new CustomerLedgerReportRepository();
            _allFetchedRows = new List<CustomerLedgerReportRow>();
            _reportRows     = new List<CustomerLedgerReportRow>();

            InitializeComponent();

            Load                         += frmCustomerLedgerReport_Load;
            btnSearch.Click              += btnSearch_Click;
            btnReset.Click               += btnReset_Click;
            btnExport.Click              += btnExport_Click;
            btnPrint.Click               += btnPrint_Click;
            btnClose.Click               += btnClose_Click;
            btnSelectCustomer.Click      += btnSelectCustomer_Click;
            ultraComboPreset.ValueChanged    += ultraComboPreset_ValueChanged;
            dtFrom.ValueChanged              += dtDate_ValueChanged;
            dtTo.ValueChanged                += dtDate_ValueChanged;
            txtSearch.TextChanged            += txtSearch_TextChanged;
            gridReport.InitializeLayout      += gridReport_InitializeLayout;
            gridReport.InitializeRow         += gridReport_InitializeRow;

            KeyPreview = true;
            KeyDown    += frmCustomerLedgerReport_KeyDown;
        }

        // ════════════════════════════════════════════════════════════
        //  Initialization
        // ════════════════════════════════════════════════════════════

        private void frmCustomerLedgerReport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                WindowState = FormWindowState.Maximized;
                Text        = "Customer Ledger Statement";

                ApplyTheme();
                InitializeDateControls();
                InitializePresetCombo();
                StyleFilterControls();
                ApplyButtonStyles();
                ApplyGridStyles();
                SetStatus("Ready  |  Select a customer and press Search (F5)  |  Ctrl+E = Export  |  Ctrl+P = Print  |  Esc = Close");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyTheme()
        {
            BackColor = FormBackColor;

            // Controls panel
            ultraPanelControls.Appearance.BackColor  = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            // Grid container
            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;

            // Summary panel
            ultraPanelSummary.Appearance.BackColor  = GridHeaderBlue;
            ultraPanelSummary.Appearance.BorderColor = GridFooterBorder;
            ultraPanelSummary.BorderStyle = UIElementBorderStyle.Solid;

            // Style summary cards
            StyleSummaryCard(pnlCardOpening, lblOpeningCap, lblOpeningVal);
            StyleSummaryCard(pnlCardDebit, lblDebitCap, lblDebitVal);
            StyleSummaryCard(pnlCardCredit, lblCreditCap, lblCreditVal);
            StyleSummaryCard(pnlCardClosing, lblClosingCap, lblClosingVal);

            // Filter labels
            StyleFilterLabel(lblFromDate);
            StyleFilterLabel(lblToDate);
            StyleFilterLabel(lblPreset);
            StyleFilterLabel(lblSearch);
            StyleFilterLabel(lblCustomer);

            if (lblStatus != null)
            {
                lblStatus.Appearance.BackColor = Color.Transparent;
                lblStatus.Appearance.ForeColor = Color.White;
                lblStatus.Appearance.FontData.Name = "Tahoma";
                lblStatus.Appearance.FontData.SizeInPoints = 8.5F;
                lblStatus.Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private static void StyleSummaryCard(Infragistics.Win.Misc.UltraPanel cardPanel, Infragistics.Win.Misc.UltraLabel lblCap, Infragistics.Win.Misc.UltraLabel lblVal)
        {
            if (cardPanel != null)
            {
                cardPanel.Appearance.BackColor = GridHeaderBlueDark;
                cardPanel.Appearance.BorderColor = BorderBlue;
                cardPanel.BorderStyle = UIElementBorderStyle.Solid;
            }
            if (lblCap != null)
            {
                lblCap.Appearance.BackColor = Color.Transparent;
                lblCap.Appearance.ForeColor = Color.FromArgb(220, 235, 255);
                lblCap.Appearance.FontData.Name = "Tahoma";
                lblCap.Appearance.FontData.SizeInPoints = 8F;
                lblCap.Appearance.FontData.Bold = DefaultableBoolean.True;
            }
            if (lblVal != null)
            {
                lblVal.Appearance.BackColor = Color.Transparent;
                lblVal.Appearance.ForeColor = Color.White;
                lblVal.Appearance.FontData.Name = "Tahoma";
                lblVal.Appearance.FontData.SizeInPoints = 13F;
                lblVal.Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private static void StyleFilterLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9.5F;
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboPreset);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleTextEditor(txtSearch);
            StyleTextEditor(txtCustomerName);
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
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 9.5F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
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
            editor.Appearance.FontData.SizeInPoints = 9.5F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.MaskInput = "{date}";
            editor.FormatString = "dd/MM/yyyy";
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
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9.5F;
        }

        private void InitializeDateControls()
        {
            DateTime today     = DateTime.Today;
            dtFrom.Value       = new DateTime(today.Year, today.Month, 1);
            dtTo.Value         = today;
            dtFrom.MaskInput   = "{date}";
            dtTo.MaskInput     = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString   = "dd/MM/yyyy";
        }

        private void InitializePresetCombo()
        {
            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("Today",        "Today");
            ultraComboPreset.Items.Add("Yesterday",    "Yesterday");
            ultraComboPreset.Items.Add("ThisWeek",     "This Week");
            ultraComboPreset.Items.Add("ThisMonth",    "This Month");
            ultraComboPreset.Items.Add("Last30Days",   "Last 30 Days");
            ultraComboPreset.Items.Add("Last3Months",  "Last 3 Months");
            ultraComboPreset.Items.Add("ThisYear",     "This Year");
            ultraComboPreset.Items.Add("Custom",       "Custom");
            ultraComboPreset.Value = "ThisMonth";
        }

        private void ApplyButtonStyles()
        {
            StylePickerButton(btnSelectCustomer);
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

        private static void StylePickerButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = Color.FromArgb(155, 188, 224);
            button.Appearance.BackColor2 = Color.FromArgb(155, 188, 224);
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonBlueBorder;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.FontData.SizeInPoints = 8.5f;
            button.Font = new Font("Tahoma", 8.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void ApplyGridStyles()
        {
            if (gridReport == null) return;
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType = FilterUIType.HeaderIcons;
            layout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            layout.Override.SelectTypeRow = SelectType.Single;

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 25;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

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
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
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

            layout.Override.FilterCellAppearance.BackColor = Color.White;
            layout.Override.FilterCellAppearance.BorderColor = SkyBlueOutline;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 22;
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

        private void LoadCustomers() { /* No longer needed – dialog handles search */ }

        // ════════════════════════════════════════════════════════════
        //  Data access
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Hits the database. Called only when customer or date range changes.
        /// </summary>
        private void FetchFromDatabase()
        {
            if (!ValidateDateRange()) return;

            if (_selectedCustomerId <= 0)
            {
                MessageBox.Show("Please select a Customer first (click \"Select Customer\" or press F3).",
                                 "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            SetStatus("Loading data from database…");
            Application.DoEvents();             // let UI refresh the status msg

            try
            {
                int customerId = _selectedCustomerId;
                var filter = new CustomerLedgerReportFilter
                {
                    FromDate  = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate    = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId  = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId  = customerId
                };

                _allFetchedRows = _repository.GetReport(
                    filter,
                    out _openingBalance,
                    out _totalDebit,
                    out _totalCredit,
                    out _closingBalance);

                _lastRefreshed = DateTime.Now;

              

                ApplyLocalFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching report: {ex.Message}", "Error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error occurred. Please check your filters and try again.");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Filters _allFetchedRows in memory (no DB round-trip).
        /// Called whenever the search text changes.
        /// </summary>
        private void ApplyLocalFilter()
        {
            string search = (txtSearch.Text ?? "").Trim();
            List<CustomerLedgerReportRow> filteredList;

            if (string.IsNullOrEmpty(search))
            {
                filteredList = _allFetchedRows;
            }
            else
            {
                string lower = search.ToLowerInvariant();
                filteredList = _allFetchedRows.Where(r =>
                    ContainsIgnoreCase(r.VoucherNo,       lower) ||
                    ContainsIgnoreCase(r.VoucherTypeName, lower) ||
                    ContainsIgnoreCase(r.Particulars,     lower) ||
                    ContainsIgnoreCase(r.Narration,       lower)
                ).ToList();
            }

            // Create list for binding (always prepend virtual Opening Balance row)
            var bindingList = new List<CustomerLedgerReportRow>();
            bindingList.Add(new CustomerLedgerReportRow
            {
                VoucherID       = 0,
                VoucherDate     = Convert.ToDateTime(dtFrom.Value).Date,
                VoucherNo       = "-",
                VoucherTypeName = "Opening Balance",
                Particulars     = "Balance Brought Forward",
                Narration       = "Opening Balance",
                ReceiptAmount   = 0,
                PaymentAmount   = 0,
                RunningBalance  = _openingBalance
            });
            bindingList.AddRange(filteredList);

            _reportRows = bindingList;
            gridReport.DataSource = _reportRows;
            UpdateSummaryCards();

            string statusSuffix = _lastRefreshed == default
                ? ""
                : $"  |  Last refreshed: {_lastRefreshed:HH:mm:ss}";

            int shown = filteredList.Count;
            int total = _allFetchedRows.Count;

            SetStatus(shown == total
                ? $"Showing {total} record(s){statusSuffix}  |  F5 = Refresh  |  Ctrl+E = Export  |  Ctrl+P = Print  |  Esc = Close"
                : $"Showing {shown} of {total} record(s)  (filtered){statusSuffix}  |  F5 = Refresh  |  Esc = Close");
        }

        // ════════════════════════════════════════════════════════════
        //  UI helpers
        // ════════════════════════════════════════════════════════════

        private bool ValidateDateRange()
        {
            if (dtFrom.Value == null || dtTo.Value == null)
            {
                MessageBox.Show("Please enter both From and To dates.", "Validation",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            DateTime from = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime to   = Convert.ToDateTime(dtTo.Value).Date;

            if (from > to)
            {
                MessageBox.Show("'From Date' cannot be after 'To Date'.", "Invalid Date Range",
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtFrom.Focus();
                return false;
            }
            return true;
        }

        private void UpdateSummaryCards()
        {
            lblOpeningVal.Text = _openingBalance.ToString("N2");
            lblDebitVal.Text   = _totalDebit.ToString("N2");
            lblCreditVal.Text  = _totalCredit.ToString("N2");
            lblClosingVal.Text = _closingBalance.ToString("N2");

            SetBalanceColor(lblOpeningVal, _openingBalance);
            SetBalanceColor(lblClosingVal, _closingBalance);
            lblDebitVal.Appearance.ForeColor  = Color.FromArgb(144, 238, 144);
            lblCreditVal.Appearance.ForeColor = Color.FromArgb(255, 182, 193);
        }

        private static void SetBalanceColor(Infragistics.Win.Misc.UltraLabel lbl, decimal value)
        {
            lbl.Appearance.ForeColor = value >= 0 ? Color.FromArgb(144, 238, 144) : Color.FromArgb(255, 182, 193);
        }

        private void ResetSummaryCards()
        {
            Color neutral = Color.FromArgb(220, 235, 255);
            lblOpeningVal.Text = "–"; lblOpeningVal.Appearance.ForeColor = neutral;
            lblDebitVal.Text   = "–"; lblDebitVal.Appearance.ForeColor   = neutral;
            lblCreditVal.Text  = "–"; lblCreditVal.Appearance.ForeColor  = neutral;
            lblClosingVal.Text = "–"; lblClosingVal.Appearance.ForeColor = neutral;
        }

        private void SetStatus(string message)
        {
            if (lblStatus != null)
                lblStatus.Text = message;
        }

        // ════════════════════════════════════════════════════════════
        //  Event handlers
        // ════════════════════════════════════════════════════════════

        private void btnSearch_Click(object sender, EventArgs e)   => FetchFromDatabase();

        public void RibbonClear() => btnReset_Click(this, EventArgs.Empty);
        public void Clear() => btnReset_Click(this, EventArgs.Empty);

        private void btnReset_Click(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _selectedCustomerId   = 0;
                _selectedCustomerName = string.Empty;
                txtCustomerName.Text  = string.Empty;
                txtSearch.Text        = string.Empty;
                _allFetchedRows       = new List<CustomerLedgerReportRow>();
                _reportRows           = new List<CustomerLedgerReportRow>();
                gridReport.DataSource = null;

                _openingBalance = 0; _totalDebit = 0; _totalCredit = 0; _closingBalance = 0;
                InitializeDateControls();
                ultraComboPreset.Value     = "ThisMonth";

                ResetSummaryCards();
                SetStatus("Ready  |  Select a customer and press Search (F5)");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_reportRows == null || _reportRows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string safeCustomer = SanitizeFileName(_selectedCustomerName);
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter   = "CSV Files (*.csv)|*.csv";
                    dlg.FileName = $"CustomerLedger_{safeCustomer}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (dlg.ShowDialog() != DialogResult.OK) return;

                    var sb = new StringBuilder();
                    sb.AppendLine("Date,Voucher No,Voucher Type,Particulars,Narration,Debit (Dr),Credit (Cr),Running Balance");

                    foreach (var row in _reportRows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(row.VoucherDate.ToString("yyyy-MM-dd")),
                            CsvCell(row.VoucherNo),
                            CsvCell(row.VoucherTypeName),
                            CsvCell(row.Particulars),
                            CsvCell(row.Narration),
                            row.ReceiptAmount.ToString("F2"),
                            row.PaymentAmount.ToString("F2"),
                            row.RunningBalance.ToString("F2")
                        ));
                    }

                    File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully!", "Export",
                                     MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_reportRows == null || _reportRows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print",
                                 MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            gridReport.PrintPreview();
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        /// <summary>Opens frmCustomerDialog and stores the selection.</summary>
        private void btnSelectCustomer_Click(object sender, EventArgs e)
        {
            OpenCustomerDialog();
        }

        private void OpenCustomerDialog()
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmCustomerDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                if (dlg.SelectedCustomerId <= 0) return;

                _selectedCustomerId   = dlg.SelectedCustomerId;
                _selectedCustomerName = dlg.SelectedCustomerName ?? string.Empty;
                txtCustomerName.Text  = _selectedCustomerName;

                // Auto-search once a customer is selected
                FetchFromDatabase();
            }
        }

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null) return;

            DateTime today = DateTime.Today;
            _isLoading = true;
            try
            {
                switch (ultraComboPreset.Value.ToString())
                {
                    case "Today":        dtFrom.Value = today;               dtTo.Value = today;               break;
                    case "Yesterday":    dtFrom.Value = today.AddDays(-1);   dtTo.Value = today.AddDays(-1);   break;
                    case "ThisWeek":     dtFrom.Value = today.AddDays(-(int)today.DayOfWeek); dtTo.Value = today; break;
                    case "ThisMonth":    dtFrom.Value = new DateTime(today.Year, today.Month, 1); dtTo.Value = today; break;
                    case "Last30Days":   dtFrom.Value = today.AddDays(-30);  dtTo.Value = today;               break;
                    case "Last3Months":  dtFrom.Value = today.AddMonths(-3); dtTo.Value = today;               break;
                    case "ThisYear":     dtFrom.Value = new DateTime(today.Year, 1, 1); dtTo.Value = today;    break;
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void dtDate_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                ultraComboPreset.Value = "Custom";
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>Text search → local in-memory filter only. No DB call.</summary>
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading || _allFetchedRows.Count == 0) return;
            ApplyLocalFilter();
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            // Hide internal ID
            if (band.Columns.Exists("VoucherID")) band.Columns["VoucherID"].Hidden = true;

            // Column order, widths, alignment
            SetCol(band, "VoucherDate",    "Date",            100, "dd/MM/yyyy", HAlign.Center, 0);
            SetCol(band, "VoucherNo",      "Voucher No",      115, null,         HAlign.Left,   1);
            SetCol(band, "VoucherTypeName","Type",            110, null,         HAlign.Left,   2);
            SetCol(band, "Particulars",    "Particulars",     200, null,         HAlign.Left,   3);
            SetCol(band, "Narration",      "Narration",       280, null,         HAlign.Left,   4);
            SetCol(band, "ReceiptAmount",  "Debit (Dr)",      115, "N2",         HAlign.Right,  5);
            SetCol(band, "PaymentAmount",  "Credit (Cr)",     115, "N2",         HAlign.Right,  6);
            SetCol(band, "RunningBalance", "Running Balance", 135, "N2",         HAlign.Right,  7);
        }

        private static void SetCol(UltraGridBand band, string key, string caption,
                                    int width, string fmt, HAlign align, int pos)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption            = caption;
            col.Header.VisiblePosition    = pos;
            col.Width                     = width;
            col.CellAppearance.TextHAlign = align;
            if (!string.IsNullOrEmpty(fmt))
                col.Format = fmt;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("RunningBalance")) return;
            object val = e.Row.Cells["RunningBalance"].Value;
            if (val == null || val == DBNull.Value) return;

            decimal bal = Convert.ToDecimal(val);
            e.Row.Cells["RunningBalance"].Appearance.ForeColor =
                bal < 0 ? Color.FromArgb(185, 28, 28) :
                bal > 0 ? Color.FromArgb(27, 94, 32) :
                          Color.FromArgb(10, 31, 79);
        }

        private void frmCustomerLedgerReport_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:                              Close();             break;
                case Keys.F5:                                  FetchFromDatabase(); break;
                case Keys.F3:                                  OpenCustomerDialog(); break;
                case Keys.E when e.Control:                    btnExport_Click(this, e); break;
                case Keys.P when e.Control:                    btnPrint_Click(this, e);  break;
            }
        }

        // ════════════════════════════════════════════════════════════
        //  Static helpers
        // ════════════════════════════════════════════════════════════

        private static bool ContainsIgnoreCase(string source, string lower)
            => source != null && source.ToLowerInvariant().Contains(lower);

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Customer";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");
            return name.Trim();
        }

        private static string CsvCell(string value)
        {
            string s = value ?? string.Empty;
            if (!s.Contains(",") && !s.Contains("\"") && !s.Contains("\n"))
                return s;
            return $"\"{s.Replace("\"", "\"\"")}\"";
        }
    }
}
