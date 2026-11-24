using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static PosBranch_Win.Reports.FinancialReports.FinancialReportGrid;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmVendorOutstandingReport : Form
    {
        #region Private Fields
        private readonly VendorOutstandingReportRepository _repository;
        private List<VendorOutstandingReportRow> _reportRows;
        private List<VendorGridList> _vendors;
        private bool _isLoading;
        private bool _isSyncingVendorControls;
        private readonly bool _getUnallocatedReturnsOnly;
        private Panel pnlWarning;
        private Label lblWarning;

        private readonly FinancialReportGrid reportGrid;
        #endregion

        #region Constructors
        public frmVendorOutstandingReport()
        {
            _repository = new VendorOutstandingReportRepository();
            _reportRows = new List<VendorOutstandingReportRow>();
            _vendors = new List<VendorGridList>();
            InitializeComponent();
            reportGrid = new FinancialReportGrid(this, gridReport, ultraPanelGridFooter, GetLayoutKey,
                "DocAmt", "PaidAmount", "Balance");

            Load += frmVendorOutstandingReport_Load;
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnVendorPicker.Click += btnVendorPicker_Click;
            btnVendorFromPicker.Click += btnVendorFromPicker_Click;
            btnVendorToPicker.Click += btnVendorToPicker_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            ultraComboVendorMode.ValueChanged += ultraComboVendorMode_ValueChanged;
            ultraComboDateMode.ValueChanged += ultraComboDateMode_ValueChanged;
            ultraComboVendor.ValueChanged += ultraComboVendor_ValueChanged;
            ultraComboVendor.KeyDown += ultraComboVendor_KeyDown;
            txtVendorSearch.TextChanged += txtVendorSearch_TextChanged;
            txtVendorSearch.KeyDown += txtVendorSearch_KeyDown;
            ultraComboVendorFrom.ValueChanged += ultraComboVendorRange_ValueChanged;
            ultraComboVendorTo.ValueChanged += ultraComboVendorRange_ValueChanged;
            chkPaymentDueOnly.CheckedChanged += filter_CheckedChanged;

            // Grid event registrations for footer cell sync and column drag
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.BeforeRowExpanded += gridReport_BeforeRowExpanded;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            KeyPreview = true;
            KeyDown += frmVendorOutstandingReport_KeyDown;
            FormClosing += FrmVendorOutstandingReport_FormClosing;
        }

        public frmVendorOutstandingReport(bool getUnallocatedReturnsOnly) : this()
        {
            _getUnallocatedReturnsOnly = getUnallocatedReturnsOnly;
        }
        #endregion

        #region Form Lifecycle
        private string GetLayoutKey() => _getUnallocatedReturnsOnly ? "UnallocatedPurchaseReturns" : "VendorOutstandingReport";

        private void frmVendorOutstandingReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void FrmVendorOutstandingReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            reportGrid.SaveLayout();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = _getUnallocatedReturnsOnly ? "Unallocated Purchase Returns" : "Vendor Outstanding Listing";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                reportGrid.Initialize();
                LoadVendors();
                ResetReportView();
                InitializeWarningPanel();
                LoadReport();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditor1.Value = dtFrom.Value;
            ultraDateTimeEditor2.Value = dtTo.Value;
            ultraDateTimeEditor1.MaskInput = "{date}";
            ultraDateTimeEditor2.MaskInput = "{date}";
            ultraDateTimeEditor1.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditor2.FormatString = "dd/MM/yyyy";

            ultraComboVendorMode.Items.Clear();
            ultraComboVendorMode.Items.Add("ALL", "ALL");
            ultraComboVendorMode.Items.Add("SELECTION", "Filter By Selection");
            ultraComboVendorMode.Items.Add("RANGE", "By Range");
            ultraComboVendorMode.Value = "ALL";

            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL");
            ultraComboDateMode.Items.Add("DOC_DATE", "Purchase Date by Range");
            ultraComboDateMode.Items.Add("INV_DATE", "Invoice Date by Range");
            ultraComboDateMode.Value = "ALL";

            chkPaymentDueOnly.Checked = false;
            txtVendorSearch.Text = string.Empty;
            ultraComboVendorFrom.Value = 0;
            ultraComboVendorTo.Value = 0;
            UpdateDateControlState();
            UpdateVendorControlState();
        }
        #endregion

        #region Styling & Layout
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelControls.Dock = DockStyle.Top;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelAction.Dock = DockStyle.Top;
                ultraPanelAction.Height = 47;
            }

            if (ultraPanelMaster != null)
            {
                ultraPanelMaster.Appearance.BackColor = FormBackColor;
                ultraPanelMaster.Appearance.BorderColor = BorderBlue;
                ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelMaster.Dock = DockStyle.Fill;
            }

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
                ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
                ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelGridFooter.Dock = DockStyle.Bottom;
                ultraPanelGridFooter.Height = 26;
            }

            if (gridReport != null)
            {
                gridReport.Dock = DockStyle.Fill;
                gridReport.BringToFront();
            }

            StyleLabel(lblVendor);
            StyleLabel(lblVendorSelection);
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);

            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnToggleSelection);
            StylePickerButton(btnVendorPicker);
            StylePickerButton(btnVendorFromPicker);
            StylePickerButton(btnVendorToPicker);
        }

        private static void StylePickerButton(Button button)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboVendorMode, true);
            StyleFilterCombo(ultraComboDateMode, true);
            StyleFilterCombo(ultraComboVendor, false);
            StyleFilterCombo(ultraComboVendorFrom, false);
            StyleFilterCombo(ultraComboVendorTo, false);
            StyleFilterCombo(txtVendorSearch, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleDateEditor(ultraDateTimeEditor1);
            StyleDateEditor(ultraDateTimeEditor2);
            StyleCheckEditor(chkPaymentDueOnly);
        }

        private static void StyleCheckEditor(Infragistics.Win.UltraWinEditors.UltraCheckEditor checkEditor)
        {
            if (checkEditor == null) return;
            checkEditor.UseAppStyling = false;
            checkEditor.UseOsThemes = DefaultableBoolean.False;
            checkEditor.Appearance.BackColor = Color.Transparent;
            checkEditor.Appearance.ForeColor = Color.Black;
            checkEditor.Appearance.FontData.Name = "Tahoma";
            checkEditor.Appearance.FontData.SizeInPoints = 10;
        }

        #endregion

        #region Data Loading & Grid Binding
        private void LoadVendors()
        {
            _vendors = _repository.GetVendors();
            ultraComboVendor.Items.Clear();
            ultraComboVendor.Items.Add(0, "All Vendors");
            txtVendorSearch.Items.Clear();

            ultraComboVendorFrom.Items.Clear();
            ultraComboVendorTo.Items.Clear();

            foreach (VendorGridList vendor in _vendors)
            {
                string displayText = GetVendorDisplayText(vendor);
                ultraComboVendor.Items.Add(vendor.LedgerID, displayText);
                txtVendorSearch.Items.Add("display_" + vendor.LedgerID, displayText);
                txtVendorSearch.Items.Add("id_" + vendor.LedgerID, vendor.LedgerID.ToString());
                txtVendorSearch.Items.Add("name_" + vendor.LedgerID, vendor.LedgerName ?? string.Empty);
                ultraComboVendorFrom.Items.Add(vendor.LedgerID, displayText);
                ultraComboVendorTo.Items.Add(vendor.LedgerID, displayText);
            }

            ultraComboVendor.Value = 0;
            ultraComboVendorFrom.Value = 0;
            ultraComboVendorTo.Value = 0;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                VendorOutstandingReportFilter filter = new VendorOutstandingReportFilter
                {
                    BranchId = SessionContext.BranchId,
                    GetUnallocatedReturnsOnly = _getUnallocatedReturnsOnly,
                    FromDate = dtFrom.Value == null ? DateTime.MinValue : Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = dtTo.Value == null ? DateTime.MaxValue : Convert.ToDateTime(dtTo.Value).Date,
                    DateFilterMode = Convert.ToString(ultraComboDateMode.Value),
                    UseDateFilter = IsDateRangeMode(),
                    PaymentDueOnly = false,
                    LedgerId = GetSelectedLedgerId(),
                    FromLedgerId = GetLedgerId(ultraComboVendorFrom),
                    ToLedgerId = GetLedgerId(ultraComboVendorTo)
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();

                if (!_getUnallocatedReturnsOnly && pnlWarning != null)
                {
                    VendorOutstandingReportFilter unallocatedFilter = new VendorOutstandingReportFilter
                    {
                        BranchId = SessionContext.BranchId,
                        GetUnallocatedReturnsOnly = true,
                        FromDate = DateTime.MinValue,
                        ToDate = DateTime.MaxValue
                    };
                    List<VendorOutstandingReportRow> unallocated = _repository.GetReport(unallocatedFilter);
                    pnlWarning.Visible = unallocated != null && unallocated.Count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor outstanding report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<VendorOutstandingReportRow> filteredRows = _reportRows ?? Enumerable.Empty<VendorOutstandingReportRow>();

            if (IsVendorSelectionMode())
            {
                int selectedLedgerId = GetSelectedLedgerId();
                if (selectedLedgerId > 0)
                {
                    filteredRows = filteredRows.Where(x => x.AcctCode == selectedLedgerId);
                }
            }
            else if (IsVendorRangeMode())
            {
                int fromId = GetLedgerId(ultraComboVendorFrom);
                int toId = GetLedgerId(ultraComboVendorTo);
                if (fromId > 0 || toId > 0)
                {
                    int lowerBound = fromId > 0 && toId > 0 ? Math.Min(fromId, toId) : Math.Max(fromId, toId);
                    int upperBound = fromId > 0 && toId > 0 ? Math.Max(fromId, toId) : Math.Max(fromId, toId);
                    filteredRows = filteredRows.Where(x => x.AcctCode >= lowerBound && x.AcctCode <= upperBound);
                }
            }

            string searchText = GetSearchText();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.Company) && x.Company.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Name) && x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Phone) && x.Phone.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.DocNo) && x.DocNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Reference) && x.Reference.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    x.AcctCode.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (chkPaymentDueOnly.Checked)
            {
                filteredRows = filteredRows.Where(x => x.Balance != 0);
            }

            List<VendorOutstandingReportRow> boundRows = filteredRows
                .OrderBy(x => x.Company)
                .ThenByDescending(x => x.Date)
                .ThenByDescending(x => x.DocNo)
                .ToList();

            reportGrid.ClearFilters();
            gridReport.DataSource = boundRows;
            reportGrid.DataBound();
        }

        private bool ValidatePaymentDueDateRange()
        {
            return true;
        }

        private bool ValidateDateRange()
        {
            if (!IsDateRangeMode())
                return true;

            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("From date cannot be greater than to date.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtFrom.Focus();
                return false;
            }

            return true;
        }

        private int GetSelectedLedgerId()
        {
            return GetLedgerId(ultraComboVendor);
        }

        private static int GetLedgerId(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null || combo.Value == null)
                return 0;

            int ledgerId;
            return int.TryParse(combo.Value.ToString(), out ledgerId) ? ledgerId : 0;
        }

        private string GetSearchText()
        {
            return string.IsNullOrWhiteSpace(txtVendorSearch.Text) ? string.Empty : txtVendorSearch.Text.Trim();
        }

        private bool IsVendorSelectionMode()
        {
            return string.Equals(Convert.ToString(ultraComboVendorMode.Value), "SELECTION", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsVendorRangeMode()
        {
            return string.Equals(Convert.ToString(ultraComboVendorMode.Value), "RANGE", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsDateRangeMode()
        {
            string mode = Convert.ToString(ultraComboDateMode.Value);
            return string.Equals(mode, "DOC_DATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "INV_DATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "POST_DATE", StringComparison.OrdinalIgnoreCase);
        }

        private void ResetReportView()
        {
            gridReport.DataSource = null;
            reportGrid.DataBound();
        }

        private void ExportCsv()
        {
            List<VendorOutstandingReportRow> rows = gridReport.DataSource as List<VendorOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                dialog.FileName = string.Format("VendorOutstandingListing_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Vendor Outstanding Report")) return;
                StringBuilder builder = new StringBuilder();
                string docNoHeader = _getUnallocatedReturnsOnly ? "Return No" : "Doc No";
                string dateHeader = _getUnallocatedReturnsOnly ? "Return Date" : "Purchase Date";
                string docAmtHeader = _getUnallocatedReturnsOnly ? "Return Amt" : "Doc Amt";
                string paidAmtHeader = _getUnallocatedReturnsOnly ? "Adjusted Amt" : "Paid Amt";
                string balanceHeader = _getUnallocatedReturnsOnly ? "Unallocated Amt" : "Balance";
                builder.AppendLine($"AcctCode,Company,Name,Phone,{docNoHeader},{dateHeader},Reference,Invoice Date,Payment Date,{docAmtHeader},{paidAmtHeader},{balanceHeader}");

                var visibleRows = reportGrid.GetVisibleDataRows().Select(r => r.ListObject as VendorOutstandingReportRow).Where(r => r != null).ToList();
                foreach (VendorOutstandingReportRow row in visibleRows)
                {
                    builder.AppendLine(string.Join(",",
                        row.AcctCode.ToString(),
                        EscapeCsv(row.Company),
                        EscapeCsv(row.Name),
                        EscapeCsv(row.Phone),
                        EscapeCsv(row.DocNo),
                        EscapeCsv(row.Date == DateTime.MinValue ? string.Empty : row.Date.ToString("yyyy-MM-dd")),
                        EscapeCsv(row.Reference),
                        EscapeCsv(row.InvoiceDate.HasValue ? row.InvoiceDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        EscapeCsv(row.PaymentDate.HasValue ? row.PaymentDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        row.DocAmt.ToString("F2"),
                        row.PaidAmount.ToString("F2"),
                        row.Balance.ToString("F2")));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return string.Format("\"{0}\"", safeValue.Replace("\"", "\"\""));
        }

        private void ConfigureChildGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BorderColor = GridRowLine;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
                column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            reportGrid.ConfigureGridColumn(band, "AcctCode", "AcctCode", 92, null, HAlign.Left, 0);
            reportGrid.ConfigureGridColumn(band, "Company", "Company", 210, null, HAlign.Left, 1);
            reportGrid.ConfigureGridColumn(band, "Name", "Name", 170, null, HAlign.Left, 2);
            reportGrid.ConfigureGridColumn(band, "Phone", "Phone", 120, null, HAlign.Left, 3);
            reportGrid.ConfigureGridColumn(band, "DocNo", _getUnallocatedReturnsOnly ? "Return No" : "Doc No", 102, null, HAlign.Left, 4);
            reportGrid.ConfigureGridColumn(band, "Date", _getUnallocatedReturnsOnly ? "Return Date" : "Purchase Date", 105, "dd-MMM-yyyy", HAlign.Left, 5);
            reportGrid.ConfigureGridColumn(band, "Reference", "Reference", 128, null, HAlign.Left, 6);
            reportGrid.ConfigureGridColumn(band, "InvoiceDate", "Invoice Date", 105, "dd-MMM-yyyy", HAlign.Left, 7);
            if (band.Columns.Exists("PostDate"))
            {
                band.Columns["PostDate"].Hidden = true;
                band.Columns["PostDate"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }
            reportGrid.ConfigureGridColumn(band, "PaymentDate", "Payment Date", 105, "dd-MMM-yyyy", HAlign.Left, 8);
            reportGrid.ConfigureGridColumn(band, "DocAmt", _getUnallocatedReturnsOnly ? "Return Amt" : "Doc Amt", 95, "#,##0.00", HAlign.Right, 9);
            reportGrid.ConfigureGridColumn(band, "PaidAmount", _getUnallocatedReturnsOnly ? "Adjusted Amt" : "Paid Amt", 95, "#,##0.00", HAlign.Right, 10);
            reportGrid.ConfigureGridColumn(band, "Balance", _getUnallocatedReturnsOnly ? "Unallocated Amt" : "Balance", 95, "#,##0.00", HAlign.Right, 11);

            if (band.Columns.Exists("Company"))
            {
                band.Columns["Company"].CellAppearance.FontData.Bold = DefaultableBoolean.False;
            }

            if (band.Columns.Exists("DocAmt"))
            {
                band.Columns["DocAmt"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                band.Columns["DocAmt"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("PaidAmount"))
            {
                band.Columns["PaidAmount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            }

            // Configure Child Band 1 for Payment History drilldown
            if (e.Layout.Bands.Count > 1)
            {
                UltraGridBand childBand = e.Layout.Bands[1];
                childBand.HeaderVisible = true;
                childBand.Header.Caption = "Payment History & Adjustments";
                childBand.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                childBand.Header.Appearance.ForeColor = Color.White;
                childBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                childBand.Override.AllowAddNew = AllowAddNew.No;
                childBand.Override.AllowDelete = DefaultableBoolean.False;
                childBand.Override.AllowUpdate = DefaultableBoolean.False;
                childBand.Override.RowSelectors = DefaultableBoolean.False;
                childBand.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                childBand.Override.HeaderAppearance.ForeColor = Color.White;
                childBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                childBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);

                foreach (UltraGridColumn column in childBand.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureChildGridColumn(childBand, "PaymentDate", "Payment Date", 105, "dd-MMM-yyyy", HAlign.Left, 0);
                ConfigureChildGridColumn(childBand, "VoucherNo", "Voucher No", 115, null, HAlign.Left, 1);
                ConfigureChildGridColumn(childBand, "PaymentMode", "Payment Mode", 125, null, HAlign.Left, 2);
                ConfigureChildGridColumn(childBand, "BillAmount", "Bill Amount", 100, "#,##0.00", HAlign.Right, 3);
                ConfigureChildGridColumn(childBand, "PaymentAmount", "Paid / Adjusted", 105, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(childBand, "BalanceAmount", "Balance After", 105, "#,##0.00", HAlign.Right, 5);
                ConfigureChildGridColumn(childBand, "Remarks", "Remarks / Narration", 220, null, HAlign.Left, 6);

                if (childBand.Columns.Exists("PaymentAmount"))
                {
                    childBand.Columns["PaymentAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                    childBand.Columns["PaymentAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (childBand.Columns.Exists("BalanceAmount"))
                {
                    childBand.Columns["BalanceAmount"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
                }
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            reportGrid.ApplySavedLayout();
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Band.Index == 0)
            {
                var row = e.Row.ListObject as VendorOutstandingReportRow;
                if (row != null)
                {
                    e.Row.ExpansionIndicator = row.PaidAmount > 0
                        ? ShowExpansionIndicator.Always
                        : ShowExpansionIndicator.Never;
                }

                if (e.Row.Cells.Exists("Balance"))
                {
                    decimal balance = 0m;
                    if (e.Row.Cells["Balance"].Value != null)
                    {
                        decimal.TryParse(e.Row.Cells["Balance"].Value.ToString(), out balance);
                    }

                    e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    e.Row.Cells["Balance"].Appearance.ForeColor = balance > 0
                        ? Color.FromArgb(191, 54, 12)
                        : Color.FromArgb(46, 125, 50);
                }
            }
        }

        private void gridReport_BeforeRowExpanded(object sender, CancelableRowEventArgs e)
        {
            if (e.Row.Band.Index == 0)
            {
                var row = e.Row.ListObject as VendorOutstandingReportRow;
                if (row != null && !row.HasLoadedPayments)
                {
                    row.HasLoadedPayments = true;
                    if (row.PaidAmount > 0)
                    {
                        Cursor currentCursor = Cursor;
                        Cursor = Cursors.WaitCursor;
                        try
                        {
                            int branchId = SessionContext.BranchId;
                            var payments = _repository.GetBillPaymentHistory(row.AcctCode, row.PurchaseNo, branchId, row.DocAmt, row.Balance);
                            if (payments != null && payments.Count > 0)
                            {
                                row.PaymentDetails.Clear();
                                row.PaymentDetails.AddRange(payments);
                            }
                            else
                            {
                                // Synthesize payment summary row if no detailed vouchers found
                                row.PaymentDetails.Clear();
                                row.PaymentDetails.Add(new VendorBillPaymentDetail
                                {
                                    PurchaseNo = row.PurchaseNo,
                                    PaymentDate = row.Date,
                                    VoucherNo = "PAYMENT",
                                    PaymentMode = "Settlement / Adjustment",
                                    BillAmount = row.DocAmt,
                                    PaymentAmount = row.PaidAmount,
                                    BalanceAmount = row.Balance,
                                    Remarks = "Direct Settlement / Voucher Adjustment"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error expanding payment details: {ex.Message}");
                        }
                        finally
                        {
                            Cursor = currentCursor;
                        }

                        if (e.Row.ChildBands.Count > 0 && e.Row.ChildBands[0].Rows != null)
                        {
                            e.Row.ChildBands[0].Rows.Refresh(RefreshRow.ReloadData);
                        }
                    }
                }
            }
        }
        #endregion

        #region Button & Control Handlers
        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            ShowReportPreview();
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            ShowReportFormatDialog(
                "VENDOR OUTSTANDING LISTING",
                new[]
                {
                    "VENDOR OUTSTANDING LISTING",
                    "VENDOR OUTSTANDING LISTING - GROUP BY COMPANY",
                    "VENDOR OUTSTANDING LISTING - SUMMARY"
                });
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void btnVendorPicker_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void btnVendorFromPicker_Click(object sender, EventArgs e)
        {
            OpenVendorRangeDialog(ultraComboVendorFrom);
        }

        private void btnVendorToPicker_Click(object sender, EventArgs e)
        {
            OpenVendorRangeDialog(ultraComboVendorTo);
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void ultraComboVendorMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            UpdateVendorControlState();
        }

        private void ultraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            UpdateDateControlState();
        }

        private void ultraComboVendor_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingVendorControls)
                return;

            SyncSearchFromSelectedVendor();
        }

        private void ultraComboVendorRange_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;
        }

        private void txtVendorSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            TrySyncVendorSelectionFromSearchText();
        }

        private void filter_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyClientFilters();
        }

        private void frmVendorOutstandingReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                btnExportGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnViewGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void txtVendorSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void ultraComboVendor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void UpdateDateControlState()
        {
            bool dateRange = IsDateRangeMode();
            dtFrom.Enabled = dateRange;
            dtTo.Enabled = dateRange;
            lblFromDate.Visible = dateRange;
            lblToDate.Visible = dateRange;
            dtFrom.Visible = dateRange;
            dtTo.Visible = dateRange;
           
            ultraLabel1.Visible = false;
            ultraLabel2.Visible = false;
            ultraDateTimeEditor1.Visible = false;
            ultraDateTimeEditor2.Visible = false;
        }

        private void UpdateVendorControlState()
        {
            bool selectionMode = IsVendorSelectionMode();
            bool rangeMode = IsVendorRangeMode();
            ultraComboVendor.Enabled = false;
            txtVendorSearch.Enabled = selectionMode;
            btnVendorPicker.Enabled = selectionMode;
            btnVendorFromPicker.Enabled = rangeMode;
            btnVendorToPicker.Enabled = rangeMode;
            ultraComboVendor.Visible = false;
            txtVendorSearch.Visible = selectionMode;
            btnVendorPicker.Visible = selectionMode;
            btnVendorFromPicker.Visible = rangeMode;
            btnVendorToPicker.Visible = rangeMode;
            lblFromVendor.Visible = rangeMode;
            lblToVendor.Visible = rangeMode;
            ultraComboVendorFrom.Visible = rangeMode;
            ultraComboVendorTo.Visible = rangeMode;
            ultraComboVendorFrom.Enabled = rangeMode;
            ultraComboVendorTo.Enabled = rangeMode;

            if (!selectionMode && !rangeMode)
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendor.Value = 0;
                    txtVendorSearch.Text = string.Empty;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }
            }

            if (!rangeMode)
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendorFrom.Value = 0;
                    ultraComboVendorTo.Value = 0;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }
            }
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
        }

        private void ShowReportFormatDialog(string reportCaption, IEnumerable<string> formatDescriptions)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(reportCaption, formatDescriptions))
            {
                dialog.ShowDialog(this);
            }
        }

        private void ShowReportPreview()
        {
            List<VendorOutstandingReportRow> rows = gridReport.DataSource as List<VendorOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (Panel footer = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Vendor Outstanding Listing - Report Preview";
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.WindowState = FormWindowState.Maximized;
                preview.MinimumSize = new Size(1024, 600);
                preview.BackColor = FormBackColor;
                preview.Padding = new Padding(10);

                header.Dock = DockStyle.Top;
                header.Height = 72;
                header.BackColor = GridHeaderBlueDark;
                header.Padding = new Padding(18, 10, 18, 8);

                Label titleLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    Text = "VENDOR OUTSTANDING LISTING",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = BuildPreviewSubtitle(),
                    ForeColor = Color.FromArgb(224, 238, 252),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(subtitleLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.BackColor = Color.White;
                previewGrid.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                previewGrid.InitializeLayout += PreviewGrid_InitializeLayout;
                previewGrid.InitializeRow += gridReport_InitializeRow;
                previewGrid.BeforeRowExpanded += gridReport_BeforeRowExpanded;
                previewGrid.DataSource = rows.ToList();

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                decimal documentTotal = rows.Sum(x => x.DocAmt);
                decimal paidTotal = rows.Sum(x => x.PaidAmount);
                decimal balanceTotal = rows.Sum(x => x.Balance);
                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format(_getUnallocatedReturnsOnly 
                        ? "Returns: {0:N0}    |    Return Total: {1:N2}    |    Adjusted Total: {2:N2}    |    Pending Unallocated: {3:N2}"
                        : "Documents: {0:N0}    |    Document Total: {1:N2}    |    Paid Total: {2:N2}    |    Outstanding Balance: {3:N2}",
                        rows.Count, documentTotal, paidTotal, balanceTotal),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight
                };
                footer.Controls.Add(footerLabel);

                preview.Controls.Add(previewGrid);
                preview.Controls.Add(footer);
                preview.Controls.Add(header);
                preview.ShowDialog(this);
            }
        }

        private string BuildPreviewSubtitle()
        {
            string vendorMode = Convert.ToString(ultraComboVendorMode.Value);
            string dateMode = Convert.ToString(ultraComboDateMode.Value);
            string dateText = IsDateRangeMode()
                ? string.Format("{0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", Convert.ToDateTime(dtFrom.Value), Convert.ToDateTime(dtTo.Value))
                : "All dates";

            return string.Format("Vendor: {0}    |    Date: {1}    |    Hide Fully Paid: {2}",
                string.IsNullOrWhiteSpace(vendorMode) ? "All" : vendorMode,
                (string.IsNullOrWhiteSpace(dateMode) || string.Equals(dateMode, "ALL", StringComparison.OrdinalIgnoreCase)
                    ? dateText
                    : string.Format("{0} ({1})", dateText, dateMode)),
                chkPaymentDueOnly.Checked ? "Yes" : "No");
        }

        private void PreviewGrid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            gridReport_InitializeLayout(sender, e);
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.Override.RowSelectors = DefaultableBoolean.False;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            e.Layout.Override.DefaultRowHeight = 23;
            e.Layout.Override.MinRowHeight = 23;
            e.Layout.Override.CellAppearance.FontData.SizeInPoints = 9;
            e.Layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            e.Layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            e.Layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            e.Layout.Override.CellAppearance.BorderColor = GridRowLine;
            e.Layout.Override.RowAppearance.BorderColor = GridRowLine;
            e.Layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
        }

        private void OpenVendorDialog()
        {
            using (frmVendorDig vendorDialog = new frmVendorDig())
            {
                if (vendorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (vendorDialog.SelectedVendorId <= 0)
                    return;

                ultraComboVendorMode.Value = "SELECTION";
                SelectVendor(vendorDialog.SelectedVendorId, vendorDialog.SelectedVendorName);
            }
        }

        private void OpenVendorRangeDialog(Infragistics.Win.UltraWinEditors.UltraComboEditor targetCombo)
        {
            using (frmVendorDig vendorDialog = new frmVendorDig())
            {
                if (vendorDialog.ShowDialog(this) != DialogResult.OK || vendorDialog.SelectedVendorId <= 0)
                    return;

                targetCombo.Value = vendorDialog.SelectedVendorId;
            }
        }

        private void SelectVendor(int vendorId, string vendorName)
        {
            _isSyncingVendorControls = true;

            try
            {
                ultraComboVendor.Value = vendorId;
                VendorGridList vendor = _vendors.FirstOrDefault(x => x.LedgerID == vendorId);
                txtVendorSearch.Text = vendor != null ? vendor.LedgerName ?? string.Empty : vendorName ?? string.Empty;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private void SyncSearchFromSelectedVendor()
        {
            int selectedLedgerId = GetSelectedLedgerId();
            if (selectedLedgerId <= 0)
                return;

            VendorGridList vendor = _vendors.FirstOrDefault(x => x.LedgerID == selectedLedgerId);
            if (vendor == null)
                return;

            _isSyncingVendorControls = true;
            try
            {
                txtVendorSearch.Text = vendor.LedgerName ?? string.Empty;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private void TrySyncVendorSelectionFromSearchText()
        {
            if (_isSyncingVendorControls || !IsVendorSelectionMode())
                return;

            string searchText = GetSearchText();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendor.Value = 0;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }

                return;
            }

            VendorGridList vendor = _vendors.FirstOrDefault(x =>
                x.LedgerID.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(x.LedgerName) && x.LedgerName.Equals(searchText, StringComparison.OrdinalIgnoreCase)) ||
                GetVendorDisplayText(x).Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (vendor == null)
                return;

            _isSyncingVendorControls = true;
            try
            {
                ultraComboVendor.Value = vendor.LedgerID;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private static string GetVendorDisplayText(VendorGridList vendor)
        {
            if (vendor == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(vendor.LedgerName)
                ? vendor.LedgerID.ToString()
                : string.Format("{0} - {1}", vendor.LedgerID, vendor.LedgerName);
        }

        private void InitializeWarningPanel()
        {
            if (_getUnallocatedReturnsOnly) return;

            pnlWarning = new Panel();
            pnlWarning.Dock = DockStyle.Top;
            pnlWarning.Height = 35;
            pnlWarning.BackColor = Color.FromArgb(254, 243, 199);
            pnlWarning.Visible = false;
            pnlWarning.BorderStyle = BorderStyle.FixedSingle;

            lblWarning = new Label();
            lblWarning.Text = "⚠️ There are unallocated purchase returns. Click here to view/allocate them.";
            lblWarning.ForeColor = Color.FromArgb(146, 64, 14);
            lblWarning.Font = new Font("Tahoma", 9.5F, FontStyle.Bold);
            lblWarning.TextAlign = ContentAlignment.MiddleLeft;
            lblWarning.Dock = DockStyle.Fill;
            lblWarning.Cursor = Cursors.Hand;
            lblWarning.Click += (s, e) =>
            {
                try
                {
                    var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (homeForm != null)
                    {
                        var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (openFormInTabMethod != null)
                        {
                            var newForm = new frmVendorOutstandingReport(true);
                            openFormInTabMethod.Invoke(homeForm, new object[] { newForm, "Unallocated Purchase Returns" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening Unallocated Purchase Returns: " + ex.Message);
                }
            };

            pnlWarning.Controls.Add(lblWarning);
            this.Controls.Add(pnlWarning);
            ultraPanelMaster.BringToFront();
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row == null || !e.Row.IsDataRow) return;
                if (e.Row.Band != null && e.Row.Band.Index != 0) return;

                var row = e.Row.ListObject as VendorOutstandingReportRow;
                if (row == null) return;

                if (row.IsPR == 1)
                {
                    decimal returnAmount = Math.Abs(row.Balance);
                    if (returnAmount <= 0)
                    {
                        MessageBox.Show("This purchase return has already been fully allocated.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var frmDebitNote = new PosBranch_Win.Accounts.FrmDebitNote(
                        Convert.ToInt32(row.PurchaseNo),
                        row.LedgerID,
                        row.Company,
                        returnAmount,
                        row.Reference
                    );

                    var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (homeForm != null)
                    {
                        var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (openFormInTabMethod != null)
                        {
                            openFormInTabMethod.Invoke(homeForm, new object[] { frmDebitNote, $"Debit Note - {row.Company}" });
                            return;
                        }
                    }

                    frmDebitNote.Show();
                    frmDebitNote.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Debit Note: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RibbonClear() => Clear();

        public void Clear()
        {
            ClearForm();
        }

        public void ClearForm()
        {
            _isLoading = true;
            try
            {
                ResetFilterControls(ultraPanelControls);
                UpdateDateControlState();
                UpdateVendorControlState();
                _reportRows = new List<VendorOutstandingReportRow>();
                ResetReportView();
                if (pnlWarning != null)
                {
                    pnlWarning.Visible = false;
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetFilterControls(Control parent)
        {
            if (parent == null)
                return;

            var ultraPanel = parent as Infragistics.Win.Misc.UltraPanel;
            if (ultraPanel != null)
            {
                ResetFilterControls(ultraPanel.ClientArea);
            }

            foreach (Control control in parent.Controls)
            {
                ResetFilterControl(control);
                ResetFilterControls(control);
            }
        }

        private void ResetFilterControl(Control control)
        {
            var combo = control as Infragistics.Win.UltraWinEditors.UltraComboEditor;
            if (combo != null)
            {
                ResetComboToDefault(combo);
                return;
            }

            var textEditor = control as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            if (textEditor != null)
            {
                textEditor.Text = string.Empty;
                return;
            }

            var textBox = control as TextBox;
            if (textBox != null)
            {
                textBox.Text = string.Empty;
                return;
            }

            var checkEditor = control as Infragistics.Win.UltraWinEditors.UltraCheckEditor;
            if (checkEditor != null)
            {
                checkEditor.Checked = false;
                return;
            }

            var checkBox = control as CheckBox;
            if (checkBox != null)
            {
                checkBox.Checked = false;
                return;
            }

            var dateEditor = control as Infragistics.Win.UltraWinEditors.UltraDateTimeEditor;
            if (dateEditor != null)
            {
                dateEditor.Value = DateTime.Today;
            }
        }

        private void ResetComboToDefault(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            foreach (ValueListItem item in combo.Items)
            {
                object value = item.DataValue;
                if (value != null && (string.Equals(Convert.ToString(value), "ALL", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(Convert.ToString(value), "0", StringComparison.OrdinalIgnoreCase)))
                {
                    SetComboValue(combo, value);
                    return;
                }
            }

            SetComboValue(combo, null);
        }

        private void SetComboValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, object value)
        {
            try
            {
                combo.Value = value;
                if (value == null)
                {
                    combo.Text = string.Empty;
                }
            }
            catch
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
            }
        }
        #endregion

        private void frmVendorOutstandingReport_Load_1(object sender, EventArgs e)
        {

        }
    }
}
