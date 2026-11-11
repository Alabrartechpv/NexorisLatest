using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmdatewisevendorpaymentreport : Form
    {
        #region Colour Palette (Parity with frmVendorOutstandingReport)
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

        // Accent Colors
        private static readonly Color PaidBlueColor        = Color.FromArgb(13, 71, 161);
        private static readonly Color OutstandingRedColor  = Color.FromArgb(183, 28, 28);
        private static readonly Color ClearedGreenColor    = Color.FromArgb(27, 94, 32);
        private static readonly Color ZeroMutedColor       = Color.FromArgb(170, 170, 170);
        #endregion

        #region Enums & Inner Helper Types
        public enum ValueDisplayMode
        {
            PaymentAmount = 0,
            BillBalance = 1
        }

        private sealed class PivotDateColumnInfo
        {
            public string ColumnKey { get; set; }
            public string HeaderCaption { get; set; }
            public string Tooltip { get; set; }
            public DateTime? Date { get; set; }
            public int DayNumber { get; set; }
            public int MonthNumber { get; set; }
        }

        private sealed class VendorPaymentSummary
        {
            public int VendorLedgerId { get; set; }
            public string VendorName { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal TotalBillAmount { get; set; }
            public decimal TotalBillBalance { get; set; }
            public decimal CurrentVendorOutstanding { get; set; }
            public int VoucherCount { get; set; }
            public DateTime LastPaymentDate { get; set; }
            public string PrimaryPaymentMode { get; set; }
            public string BranchName { get; set; }
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

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Private Fields
        private readonly DatewiseVendorPaymentReportRepository _repository;
        private DatewiseVendorPaymentReportData _reportData;
        private List<DatewiseVendorPaymentReportRow> _reportRows;
        private List<VendorGridList> _vendors;
        private bool _isLoading;

        private ValueDisplayMode _currentValueMode = ValueDisplayMode.PaymentAmount;
        private readonly List<PivotDateColumnInfo> _currentMatrixDateColumns = new List<PivotDateColumnInfo>();
        private DataSet _matrixDataSet;
        private DataTable _matrixTable;

        // Custom Dynamic Footer
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Total",
            "PaymentAmount",
            "BillAmount",
            "TotalBillPaid",
            "ReturnedAmount",
            "BillBalance",
            "MasterPaymentAmount",
            "BalanceAmount",
            "CurrentVendorOutstanding",
            "TotalBillAmount",
            "TotalBillBalance",
            "InvoiceAmount",
            "PaidAmount",
            "Balance",
            "PaidInPeriod"
        };

        // Header drag-to-hide & Column Chooser
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private ReportColumnFilterManager _filterManager;
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        #endregion

        #region Constructor & Form Lifecycle
        public frmdatewisevendorpaymentreport()
        {
            _repository = new DatewiseVendorPaymentReportRepository();
            _reportData = new DatewiseVendorPaymentReportData();
            _reportRows = new List<DatewiseVendorPaymentReportRow>();
            _vendors = new List<VendorGridList>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmdatewisevendorpaymentreport_Load;
            FormClosing += frmdatewisevendorpaymentreport_FormClosing;
            KeyDown += frmdatewisevendorpaymentreport_KeyDown;
            KeyPreview = true;

            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnColumnChooser.Click += btnColumnChooser_Click;
            btnToggleValueMode.Click += btnToggleValueMode_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            btnVendorPicker.Click += btnVendorPicker_Click;

            ultraComboPeriodMode.ValueChanged += ultraComboPeriodMode_ValueChanged;
            ultraComboViewMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboMonth.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboYear.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtFrom.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            dtTo.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboVendor.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboPaymentType.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };
            ultraComboPaymentMode.ValueChanged += (s, e) => { if (!_isLoading) LoadReport(); };

            ultraComboVendor.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { LoadReport(); e.Handled = true; }
                else if (e.KeyCode == Keys.F11) { btnVendorPicker.PerformClick(); e.Handled = true; }
            };

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            gridReport.DoubleClickRow += (s, e) =>
            {
                if (e.Row == null) return;

                bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

                if (!isMatrix)
                {
                    int voucherId = 0;
                    if (e.Row.Cells.Exists("VoucherId") && e.Row.Cells["VoucherId"].Value != null && e.Row.Cells["VoucherId"].Value != DBNull.Value)
                    {
                        int.TryParse(e.Row.Cells["VoucherId"].Value.ToString(), out voucherId);
                    }
                    if (voucherId == 0 && e.Row.Cells.Exists("VoucherNo") && e.Row.Cells["VoucherNo"].Value != null)
                    {
                        string sNo = e.Row.Cells["VoucherNo"].Value.ToString().Replace("VP-", "").Replace("PAY", "").Replace("#", "").Trim();
                        int.TryParse(sNo, out voucherId);
                    }

                    if (voucherId > 0)
                    {
                        OpenPaymentForm(voucherId);
                        return;
                    }
                }
                else
                {
                    // In Matrix Mode
                    if (e.Row.Band == null || e.Row.Band.Index == 0)
                    {
                        e.Row.Expanded = !e.Row.Expanded;
                        return;
                    }
                    else if (e.Row.Band.Index == 1)
                    {
                        // Open the exact FrmViewPayment popup dialog matching Images 2 & 3
                        int vendorId = 0;
                        long purchaseNo = 0;

                        if (e.Row.Cells.Exists("VendorKey") && e.Row.Cells["VendorKey"].Value != null)
                        {
                            int.TryParse(e.Row.Cells["VendorKey"].Value.ToString(), out vendorId);
                        }
                        if (e.Row.Cells.Exists("PurchaseNo") && e.Row.Cells["PurchaseNo"].Value != null)
                        {
                            long.TryParse(e.Row.Cells["PurchaseNo"].Value.ToString(), out purchaseNo);
                        }

                        if (vendorId > 0 && purchaseNo > 0)
                        {
                            try
                            {
                                using (var viewPaymentForm = new PosBranch_Win.DialogBox.FrmViewPayment(vendorId, purchaseNo.ToString()))
                                {
                                    viewPaymentForm.ShowDialog(this);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error opening Payment History: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else if (e.Row.HasChild())
                        {
                            e.Row.Expanded = !e.Row.Expanded;
                        }
                        return;
                    }
                    else if (e.Row.Band.Index == 2)
                    {
                        // Voucher Details band
                        int voucherId = 0;
                        if (e.Row.Cells.Exists("VoucherId") && e.Row.Cells["VoucherId"].Value != null && e.Row.Cells["VoucherId"].Value != DBNull.Value)
                        {
                            int.TryParse(e.Row.Cells["VoucherId"].Value.ToString(), out voucherId);
                        }
                        if (voucherId == 0 && e.Row.Cells.Exists("VoucherNo") && e.Row.Cells["VoucherNo"].Value != null)
                        {
                            string sNo = e.Row.Cells["VoucherNo"].Value.ToString().Replace("VP-", "").Replace("PAY", "").Replace("#", "").Trim();
                            int.TryParse(sNo, out voucherId);
                        }

                        if (voucherId > 0)
                        {
                            OpenPaymentForm(voucherId);
                            return;
                        }
                    }
                }
            };
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) => UpdateFooterValues();

            gridReport.MouseDown += gridReport_MouseDown;
            gridReport.MouseMove += gridReport_MouseMove;
            gridReport.MouseUp += gridReport_MouseUp;
            gridReport.GiveFeedback += gridReport_GiveFeedback;
        }

        private void frmdatewisevendorpaymentreport_Load(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                Text = "Date-wise Vendor Payment Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = FormBackColor;
                Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

                ReportGridLayoutHelper.LoadHiddenColumns(GetLayoutKey(), userHiddenColumnKeys);

                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                InitializeFilterControls();
                LoadVendors();
                UpdateValueModeButtonText();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void frmdatewisevendorpaymentreport_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
            }
        }

        private void frmdatewisevendorpaymentreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11 && ultraComboVendor.Focused)
            {
                btnVendorPicker.PerformClick();
                e.Handled = true;
            }
        }
        #endregion

        #region Filter Controls Setup
        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;

            ultraComboPeriodMode.Items.Clear();
            ultraComboPeriodMode.Items.Add("DATE_RANGE", "Date Range");
            ultraComboPeriodMode.Items.Add("MONTH_WISE", "Month Wise");
            ultraComboPeriodMode.Items.Add("YEAR_WISE", "Year Wise");
            ultraComboPeriodMode.Items.Add("ALL", "ALL Dates");
            ultraComboPeriodMode.Value = "DATE_RANGE";

            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            ultraComboMonth.Items.Clear();
            ultraComboMonth.Items.Add(0, "ALL Months");
            for (int m = 1; m <= 12; m++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m);
                ultraComboMonth.Items.Add(m, monthName);
            }
            ultraComboMonth.Value = today.Month;

            ultraComboYear.Items.Clear();
            ultraComboYear.Items.Add(0, "ALL Years");
            for (int y = today.Year; y >= 2000; y--)
            {
                ultraComboYear.Items.Add(y, y.ToString());
            }
            ultraComboYear.Value = today.Year;

            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("DATE_MATRIX", "Date Matrix (Vendor vs Dates Payment)");
            ultraComboViewMode.Items.Add("DETAILED", "Detailed (Voucher & Bill Wise)");
            ultraComboViewMode.Items.Add("DATE_VENDOR_SUMMARY", "Daily Summary (Date & Vendor)");
            ultraComboViewMode.Items.Add("VENDOR_SUMMARY", "Vendor Summary (Total by Vendor)");
            ultraComboViewMode.Value = "DATE_MATRIX";

            ultraComboPaymentType.Items.Clear();
            ultraComboPaymentType.Items.Add("ALL", "ALL Types");
            ultraComboPaymentType.Items.Add("Vendor Payment", "Vendor Payment (FrmPayment)");
            ultraComboPaymentType.Items.Add("Debit Note", "Debit Note");
            ultraComboPaymentType.Value = "Vendor Payment";

            ultraComboPaymentMode.Items.Clear();
            ultraComboPaymentMode.Items.Add("ALL", "ALL Modes");
            try
            {
                Repository.Dropdowns drop = new Repository.Dropdowns();
                var pm = drop.GetPaymode();
                if (pm?.List != null)
                {
                    foreach (var item in pm.List)
                    {
                        if (!string.IsNullOrEmpty(item.PayModeName))
                        {
                            ultraComboPaymentMode.Items.Add(item.PayModeName, item.PayModeName);
                        }
                    }
                }
            }
            catch
            {
                ultraComboPaymentMode.Items.Add("CASH", "CASH");
                ultraComboPaymentMode.Items.Add("BANK TRANSFER", "BANK TRANSFER");
                ultraComboPaymentMode.Items.Add("UPI", "UPI");
                ultraComboPaymentMode.Items.Add("CHEQUE", "CHEQUE");
            }
            ultraComboPaymentMode.Value = "ALL";

            UpdatePeriodControlVisibility();
        }

        private void LoadVendors()
        {
            try
            {
                ultraComboVendor.Items.Clear();
                ultraComboVendor.Items.Add(0, "ALL Vendors");

                _vendors = _repository.GetVendors();
                if (_vendors != null)
                {
                    foreach (var v in _vendors.OrderBy(x => x.LedgerName))
                    {
                        ultraComboVendor.Items.Add(v.LedgerID, v.LedgerName);
                    }
                }

                ultraComboVendor.Value = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vendors: {ex.Message}");
            }
        }

        private void UpdatePeriodControlVisibility()
        {
            string mode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
            bool isDateRange = string.Equals(mode, "DATE_RANGE", StringComparison.OrdinalIgnoreCase);
            bool isMonthWise = string.Equals(mode, "MONTH_WISE", StringComparison.OrdinalIgnoreCase);
            bool isYearWise  = string.Equals(mode, "YEAR_WISE", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = isDateRange;
            dtFrom.Visible = isDateRange;
            lblToDate.Visible = isDateRange;
            dtTo.Visible = isDateRange;

            lblMonth.Visible = isMonthWise;
            ultraComboMonth.Visible = isMonthWise;

            lblYear.Visible = isMonthWise || isYearWise;
            ultraComboYear.Visible = isMonthWise || isYearWise;

            if (isYearWise)
            {
                lblYear.Location = new Point(248, 14);
                ultraComboYear.Location = new Point(288, 12);
            }
            else if (isMonthWise)
            {
                lblMonth.Location = new Point(248, 14);
                ultraComboMonth.Location = new Point(296, 12);
                lblYear.Location = new Point(418, 14);
                ultraComboYear.Location = new Point(456, 12);
            }
        }

        private void ultraComboPeriodMode_ValueChanged(object sender, EventArgs e)
        {
            UpdatePeriodControlVisibility();
            if (!_isLoading)
            {
                LoadReport();
            }
        }

        private void UpdateValueModeButtonText()
        {
            if (btnToggleValueMode == null) return;
            btnToggleValueMode.Text = (_currentValueMode == ValueDisplayMode.PaymentAmount)
                ? "Show Bill Balance"
                : "Show Payment Amount";
        }
        #endregion

        #region UI Styling (Exact Parity with frmVendorOutstandingReport)
        private void InitializePanels()
        {
            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelMaster != null)
            {
                ultraPanelMaster.Appearance.BackColor = FormBackColor;
                ultraPanelMaster.Appearance.BorderColor = BorderBlue;
                ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (gridReport != null)
            {
                gridReport.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                gridReport.DisplayLayout.Appearance.BorderColor = BorderBlue;
            }
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnToggleValueMode);
            StyleClassicButton(btnExpandAll);
            StyleClassicButton(btnCollapseAll);
            StyleClassicButton(btnToggleSelection);

            StylePickerButton(btnVendorPicker);
        }

        private static void StyleClassicButton(UltraButton button)
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
            StyleFilterCombo(ultraComboPeriodMode, true);
            StyleFilterCombo(ultraComboViewMode, true);
            StyleFilterCombo(ultraComboMonth, true);
            StyleFilterCombo(ultraComboYear, true);
            StyleFilterCombo(ultraComboVendor, false);
            StyleFilterCombo(ultraComboPaymentType, true);
            StyleFilterCombo(ultraComboPaymentMode, true);

            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);

            StyleLabel(lblPeriodMode);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblMonth);
            StyleLabel(lblYear);
            StyleLabel(lblViewMode);
            StyleLabel(lblVendor);
            StyleLabel(lblPaymentType);
            StyleLabel(lblPaymentMode);
        }

        private static void StyleLabel(UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9;
            label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
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
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
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
            editor.Appearance.FontData.SizeInPoints = 10;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            layout.ViewStyleBand = ViewStyleBand.Vertical;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType = FilterUIType.HeaderIcons;
            layout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            layout.Override.FilterOperandStyle = FilterOperandStyle.Combo;
            layout.Override.FilterClearButtonLocation = FilterClearButtonLocation.Hidden;

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.SeparateElement;

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
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
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
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 20;
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

            _filterManager = new ReportColumnFilterManager(gridReport);
        }

        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Height = 26;
        }
        #endregion

        #region Custom Footer Sync & Aggregation
        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null || gridReport == null) return;

            foreach (var kvp in _footerLabels)
            {
                if (kvp.Value != null && !kvp.Value.IsDisposed)
                {
                    ultraPanelGridFooter.ClientArea.Controls.Remove(kvp.Value);
                    kvp.Value.Dispose();
                }
            }
            _footerLabels.Clear();

            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            Label lblCount = new Label
            {
                Name = "_lblRecordCount",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold),
                Text = "Total (0 Vendors)"
            };
            ultraPanelGridFooter.ClientArea.Controls.Add(lblCount);
            _footerLabels["_RECORD_COUNT_"] = lblCount;

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Label cell = new Label
                {
                    Name = "footer_" + column.Key,
                    Tag = column.Key,
                    AutoSize = false,
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold),
                    TextAlign = GetAlignmentForColumn(column)
                };

                cell.MouseUp += FooterCell_MouseUp;
                ultraPanelGridFooter.ClientArea.Controls.Add(cell);
                _footerLabels[column.Key] = cell;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    if (summaryDefaultNumericColumns.Contains(column.Key) || column.Key.StartsWith("Day_") || column.Key.StartsWith("Date_") || column.Key.StartsWith("Month_"))
                    {
                        _columnAggregations[column.Key] = "Sum";
                    }
                    else
                    {
                        _columnAggregations[column.Key] = "None";
                    }
                }
            }
        }

        private ContentAlignment GetAlignmentForColumn(UltraGridColumn col)
        {
            if (col == null) return ContentAlignment.MiddleRight;
            HAlign align = col.CellAppearance.TextHAlign;
            if (align == HAlign.Left) return ContentAlignment.MiddleLeft;
            if (align == HAlign.Center) return ContentAlignment.MiddleCenter;
            return ContentAlignment.MiddleRight;
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraPanelGridFooter == null || gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int gridScrollLeft = 0;
            if (gridReport.ActiveColScrollRegion != null)
            {
                gridScrollLeft = gridReport.ActiveColScrollRegion.Position;
            }

            int rowSelectorWidth = 0;
            if (gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
            {
                rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
                if (rowSelectorWidth <= 0) rowSelectorWidth = 20;
            }

            int countLabelWidth = 200;
            if (band.Columns.Exists("VendorName") && !band.Columns["VendorName"].Hidden)
            {
                countLabelWidth = Math.Max(160, band.Columns["VendorName"].Width);
            }

            if (_footerLabels.TryGetValue("_RECORD_COUNT_", out Label lblCount))
            {
                lblCount.SetBounds(rowSelectorWidth + 4, 3, countLabelWidth - 8, 20);
            }

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (_footerLabels.TryGetValue(column.Key, out Label cell))
                {
                    UIElement colHeaderElement = column.Header.GetUIElement();
                    if (colHeaderElement != null)
                    {
                        Rectangle rect = colHeaderElement.Rect;
                        int x = rect.X;
                        int w = rect.Width;
                        cell.SetBounds(x, 3, w, 20);
                        cell.Visible = (x + w > 0 && x < ultraPanelGridFooter.Width);
                    }
                    else
                    {
                        int fallbackX = rowSelectorWidth - gridScrollLeft;
                        foreach (UltraGridColumn c in band.Columns)
                        {
                            if (c.Header.VisiblePosition < column.Header.VisiblePosition && !c.Hidden && !c.IsChaptered)
                            {
                                fallbackX += c.Width;
                            }
                        }
                        cell.SetBounds(fallbackX, 3, column.Width, 20);
                        cell.Visible = (fallbackX + column.Width > 0 && fallbackX < ultraPanelGridFooter.Width);
                    }
                }
            }
        }

        private void UpdateFooterValues()
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            List<UltraGridRow> visibleRows = GetVisibleDataRows();

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            if (_footerLabels.TryGetValue("_RECORD_COUNT_", out Label lblCount))
            {
                lblCount.Text = isMatrix ? $"Total ({visibleRows.Count} Vendors)" : $"Total ({visibleRows.Count} Records)";
            }

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (_footerLabels.TryGetValue(column.Key, out Label cell))
                {
                    string aggType = _columnAggregations.ContainsKey(column.Key) ? _columnAggregations[column.Key] : "None";
                    if (string.Equals(aggType, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        cell.Text = string.Empty;
                        continue;
                    }

                    List<decimal> numericValues = new List<decimal>();
                    foreach (UltraGridRow row in visibleRows)
                    {
                        if (row.Cells.Exists(column.Key))
                        {
                            object val = row.Cells[column.Key].Value;
                            if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal decVal))
                            {
                                numericValues.Add(decVal);
                            }
                        }
                    }

                    string text = string.Empty;
                    switch (aggType.ToUpperInvariant())
                    {
                        case "SUM":
                            text = numericValues.Count > 0 ? numericValues.Sum().ToString("#,##0.00") : "0.00";
                            break;
                        case "COUNT":
                            text = numericValues.Count.ToString();
                            break;
                        case "AVERAGE":
                        case "AVG":
                            text = numericValues.Count > 0 ? numericValues.Average().ToString("#,##0.00") : "0.00";
                            break;
                        case "MIN":
                            text = numericValues.Count > 0 ? numericValues.Min().ToString("#,##0.00") : "0.00";
                            break;
                        case "MAX":
                            text = numericValues.Count > 0 ? numericValues.Max().ToString("#,##0.00") : "0.00";
                            break;
                    }

                    cell.Text = text;
                }
            }
        }

        private List<UltraGridRow> GetVisibleDataRows()
        {
            List<UltraGridRow> list = new List<UltraGridRow>();
            if (gridReport?.Rows == null) return list;

            foreach (UltraGridRow row in gridReport.Rows.GetAllNonGroupByRows())
            {
                if (row.IsDataRow && !row.IsFilteredOut && (row.Band == null || row.Band.Index == 0))
                {
                    list.Add(row);
                }
            }
            return list;
        }

        private void FooterCell_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            Label cell = sender as Label;
            if (cell == null) return;
            string colKey = Convert.ToString(cell.Tag);
            if (string.IsNullOrEmpty(colKey)) return;

            ContextMenuStrip menu = new ContextMenuStrip();
            string currentAgg = _columnAggregations.ContainsKey(colKey) ? _columnAggregations[colKey] : "None";

            string[] types = new[] { "Sum", "Average", "Count", "Min", "Max", "None" };
            foreach (string t in types)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(t)
                {
                    Checked = string.Equals(currentAgg, t, StringComparison.OrdinalIgnoreCase)
                };
                string capturedType = t;
                item.Click += (s, ev) =>
                {
                    _columnAggregations[colKey] = capturedType;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }

            menu.Show(cell, e.Location);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser
        private void gridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UIElement element = gridReport.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                HeaderUIElement headerElement = element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerElement?.GetContext(typeof(UltraGridColumn)) is UltraGridColumn col && !col.IsChaptered)
                {
                    columnBeingDragged = col;
                    headerDragStartPoint = e.Location;
                    isDraggingHeaderToHide = false;
                }
            }
        }

        private void gridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                int dy = Math.Abs(e.Y - headerDragStartPoint.Y);
                int dx = Math.Abs(e.X - headerDragStartPoint.X);

                if (dy > 12 && dy > dx)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                }
                else if (isDraggingHeaderToHide)
                {
                    Cursor.Current = blackXCursor;
                }
            }
        }

        private void gridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide && columnBeingDragged != null)
            {
                Rectangle gridRect = gridReport.ClientRectangle;
                if (!gridRect.Contains(e.Location) || e.Y > 50)
                {
                    HideColumn(columnBeingDragged);
                }
            }

            columnBeingDragged = null;
            isDraggingHeaderToHide = false;
            Cursor.Current = Cursors.Default;
        }

        private void gridReport_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                e.UseDefaultCursors = false;
                Cursor.Current = blackXCursor;
            }
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered || string.Equals(col.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase)) return;

            col.Hidden = true;
            userHiddenColumnKeys.Add(col.Key);

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();

            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            PopulateColumnChooserListBox();
        }

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(24, 24))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen pen = new Pen(Color.FromArgb(200, 30, 30), 3f))
                    {
                        g.DrawLine(pen, 4, 4, 20, 20);
                        g.DrawLine(pen, 20, 4, 4, 20);
                    }
                    return new Cursor(bmp.GetHicon());
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                columnChooserForm = new Form
                {
                    Text = "Customization (Field Chooser)",
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    StartPosition = FormStartPosition.Manual,
                    Width = 260,
                    Height = 380,
                    TopMost = true,
                    BackColor = FormBackColor,
                    ShowInTaskbar = false
                };

                Panel headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 44,
                    BackColor = FormBackColor,
                    Padding = new Padding(6)
                };

                txtColumnSearch = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.Gray,
                    Text = "Search columns..."
                };
                txtColumnSearch.Enter += (s, e) =>
                {
                    if (txtColumnSearch.Text == "Search columns...")
                    {
                        txtColumnSearch.Text = string.Empty;
                        txtColumnSearch.ForeColor = Color.Black;
                    }
                };
                txtColumnSearch.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtColumnSearch.Text))
                    {
                        txtColumnSearch.Text = "Search columns...";
                        txtColumnSearch.ForeColor = Color.Gray;
                    }
                };
                txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();
                headerPanel.Controls.Add(txtColumnSearch);

                Panel footerPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 36,
                    BackColor = FormBackColor,
                    Padding = new Padding(6, 4, 6, 4)
                };

                Button btnReset = new Button
                {
                    Text = "Show All",
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ButtonBlueTop,
                    ForeColor = ButtonTextBlue,
                    Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnReset.FlatAppearance.BorderColor = ButtonBlueBorder;
                btnReset.Click += (s, e) => UnhideAllColumns();
                footerPanel.Controls.Add(btnReset);

                columnChooserListBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    DrawMode = DrawMode.OwnerDrawFixed,
                    ItemHeight = 32,
                    BorderStyle = BorderStyle.None,
                    BackColor = FormBackColor,
                    Font = new Font("Segoe UI", 9F)
                };
                columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
                columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

                columnChooserForm.Controls.Add(columnChooserListBox);
                columnChooserForm.Controls.Add(headerPanel);
                columnChooserForm.Controls.Add(footerPanel);

                columnChooserForm.FormClosing += (s, e) =>
                {
                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        e.Cancel = true;
                        columnChooserForm.Hide();
                    }
                };
            }

            PopulateColumnChooserListBox();

            Point btnPos = btnColumnChooser.PointToScreen(Point.Empty);
            columnChooserForm.Location = new Point(btnPos.X, btnPos.Y + btnColumnChooser.Height + 2);
            columnChooserForm.Show();
            columnChooserForm.BringToFront();
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.Items.Clear();
            string filterText = txtColumnSearch != null && txtColumnSearch.Text != "Search columns..."
                ? txtColumnSearch.Text.Trim()
                : string.Empty;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.IsChaptered || string.Equals(col.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase)) continue;

                if (col.Hidden && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    string caption = !string.IsNullOrWhiteSpace(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filterText) || caption.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count) return;

            ColumnChooserItem item = columnChooserListBox.Items[e.Index] as ColumnChooserItem;
            if (item == null) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = e.Bounds;
            using (SolidBrush bgBrush = new SolidBrush(FormBackColor))
            {
                g.FillRectangle(bgBrush, bounds);
            }

            Rectangle cardRect = new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4);
            Color cardBg = isSelected ? Color.FromArgb(200, 222, 245) : Color.FromArgb(242, 247, 253);
            Color cardBorder = isSelected ? GridHeaderBlueDark : ButtonLightOutline;

            using (GraphicsPath path = GetRoundedRectPath(cardRect, 4))
            {
                using (SolidBrush b = new SolidBrush(cardBg))
                {
                    g.FillPath(b, path);
                }
                using (Pen p = new Pen(cardBorder, 1))
                {
                    g.DrawPath(p, path);
                }
            }

            Rectangle textRect = new Rectangle(cardRect.X + 8, cardRect.Y, cardRect.Width - 16, cardRect.Height);
            TextRenderer.DrawText(g, item.DisplayText, columnChooserListBox.Font, textRect, ButtonTextBlue,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem selectedItem)
            {
                RestoreColumn(selectedItem.ColumnKey);
            }
        }

        private void RestoreColumn(string columnKey)
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            if (band.Columns.Exists(columnKey))
            {
                band.Columns[columnKey].Hidden = false;
                userHiddenColumnKeys.Remove(columnKey);

                PopulateColumnChooserListBox();
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();

                ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
            }
        }

        private void UnhideAllColumns()
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.IsChaptered || string.Equals(col.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase)) continue;

                if (col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    col.Hidden = false;
                }
            }

            userHiddenColumnKeys.Clear();

            if (isMatrix)
            {
                if (band.Columns.Exists("VendorKey")) band.Columns["VendorKey"].Hidden = true;
                if (band.Columns.Exists("VendorLedgerId")) band.Columns["VendorLedgerId"].Hidden = true;
            }

            PopulateColumnChooserListBox();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();

            ReportGridLayoutHelper.SaveLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }
        #endregion

        #region Matrix Building & Data Loading
        private DataTable BuildMatrixDataTable(List<DatewiseVendorPaymentReportRow> rawRows, DatewiseVendorPaymentReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            dateColumns = new List<PivotDateColumnInfo>();
            string mode = (filter.PeriodMode ?? "DATE_RANGE").ToUpperInvariant();

            if (mode == "MONTH_WISE")
            {
                int month = filter.Month > 0 ? filter.Month : DateTime.Today.Month;
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;

                if (filter.Month > 0)
                {
                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= 31; day++)
                    {
                        string key = $"Day_{day:D2}";
                        string caption = day.ToString();
                        string tip;
                        DateTime? dt = null;

                        if (day <= daysInMonth)
                        {
                            dt = new DateTime(year, month, day);
                            tip = $"{dt.Value:dd-MMM-yyyy (dddd)}";
                        }
                        else
                        {
                            tip = $"Day {day} (N/A for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)})";
                        }

                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = key,
                            HeaderCaption = caption,
                            Tooltip = tip,
                            Date = dt,
                            DayNumber = day
                        });
                    }
                }
                else
                {
                    for (int m = 1; m <= 12; m++)
                    {
                        string mName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m);
                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = $"Month_{m:D2}",
                            HeaderCaption = mName,
                            Tooltip = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)} {year}",
                            MonthNumber = m
                        });
                    }
                }
            }
            else if (mode == "YEAR_WISE")
            {
                int year = filter.Year > 0 ? filter.Year : DateTime.Today.Year;
                for (int m = 1; m <= 12; m++)
                {
                    string mName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m);
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = $"Month_{m:D2}",
                        HeaderCaption = mName,
                        Tooltip = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)} {year}",
                        MonthNumber = m
                    });
                }
            }
            else if (mode == "DATE_RANGE")
            {
                DateTime start = filter.FromDate.Date;
                DateTime end = filter.ToDate.Date;
                if (start > end) { DateTime tmp = start; start = end; end = tmp; }

                if (start.Year == end.Year && start.Month == end.Month && start.Day == 1 && end.Day == DateTime.DaysInMonth(start.Year, start.Month))
                {
                    int daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);
                    for (int day = 1; day <= 31; day++)
                    {
                        string key = $"Day_{day:D2}";
                        string caption = day.ToString();
                        string tip;
                        DateTime? dt = null;

                        if (day <= daysInMonth)
                        {
                            dt = new DateTime(start.Year, start.Month, day);
                            tip = $"{dt.Value:dd-MMM-yyyy (dddd)}";
                        }
                        else
                        {
                            tip = $"Day {day} (N/A for {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(start.Month)})";
                        }

                        dateColumns.Add(new PivotDateColumnInfo
                        {
                            ColumnKey = key,
                            HeaderCaption = caption,
                            Tooltip = tip,
                            Date = dt,
                            DayNumber = day
                        });
                    }
                }
                else
                {
                    int totalDays = (int)(end - start).TotalDays + 1;
                    if (totalDays <= 93)
                    {
                        for (DateTime d = start; d <= end; d = d.AddDays(1))
                        {
                            string key = $"Date_{d:yyyyMMdd}";
                            string caption = (start.Year == end.Year && start.Month == end.Month)
                                ? d.Day.ToString()
                                : d.ToString("dd-MMM");

                            dateColumns.Add(new PivotDateColumnInfo
                            {
                                ColumnKey = key,
                                HeaderCaption = caption,
                                Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                                Date = d,
                                DayNumber = d.Day
                            });
                        }
                    }
                    else
                    {
                        var distinctDates = (rawRows ?? new List<DatewiseVendorPaymentReportRow>())
                            .Select(r => r.VoucherDate.Date)
                            .Distinct()
                            .OrderBy(d => d)
                            .ToList();

                        if (distinctDates.Count == 0)
                        {
                            distinctDates.Add(start);
                        }

                        foreach (var d in distinctDates)
                        {
                            dateColumns.Add(new PivotDateColumnInfo
                            {
                                ColumnKey = $"Date_{d:yyyyMMdd}",
                                HeaderCaption = d.ToString("dd-MMM"),
                                Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                                Date = d,
                                DayNumber = d.Day
                            });
                        }
                    }
                }
            }
            else // ALL
            {
                var distinctDates = (rawRows ?? new List<DatewiseVendorPaymentReportRow>())
                    .Select(r => r.VoucherDate.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                if (distinctDates.Count == 0)
                {
                    distinctDates.Add(DateTime.Today);
                }

                foreach (var d in distinctDates)
                {
                    dateColumns.Add(new PivotDateColumnInfo
                    {
                        ColumnKey = $"Date_{d:yyyyMMdd}",
                        HeaderCaption = d.ToString("dd-MMM"),
                        Tooltip = $"{d:dd-MMM-yyyy (dddd)}",
                        Date = d,
                        DayNumber = d.Day
                    });
                }
            }

            // Build DataTable Schema
            DataTable table = new DataTable("VendorPaymentMatrix");
            table.Columns.Add("VendorKey", typeof(string));
            table.Columns.Add("VendorLedgerId", typeof(int));
            table.Columns.Add("VendorName", typeof(string));

            foreach (var col in dateColumns)
            {
                var dc = table.Columns.Add(col.ColumnKey, typeof(decimal));
                dc.DefaultValue = 0m;
            }

            var cTotal = table.Columns.Add("Total", typeof(decimal));
            cTotal.DefaultValue = 0m;

            var cOutstanding = table.Columns.Add("CurrentVendorOutstanding", typeof(decimal));
            cOutstanding.DefaultValue = 0m;

            var cVoucherCount = table.Columns.Add("VoucherCount", typeof(int));
            cVoucherCount.DefaultValue = 0;

            var cLastDate = table.Columns.Add("LastPaymentDate", typeof(DateTime));

            var cMode = table.Columns.Add("PrimaryPaymentMode", typeof(string));
            cMode.DefaultValue = string.Empty;

            var cTotalBillAmt = table.Columns.Add("TotalBillAmount", typeof(decimal));
            cTotalBillAmt.DefaultValue = 0m;

            var cTotalBillBal = table.Columns.Add("TotalBillBalance", typeof(decimal));
            cTotalBillBal.DefaultValue = 0m;

            var cBranch = table.Columns.Add("BranchName", typeof(string));
            cBranch.DefaultValue = string.Empty;

            if (rawRows == null || rawRows.Count == 0)
            {
                return table;
            }

            // Fast pre-aggregation by vendor and date
            Dictionary<string, decimal> datePaymentMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, decimal> dateBalanceMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, VendorPaymentSummary> vendorSummaryMap = new Dictionary<string, VendorPaymentSummary>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, HashSet<int>> vendorVouchersMap = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

            foreach (DatewiseVendorPaymentReportRow r in rawRows)
            {
                string vendorKey = r.VendorLedgerId > 0 ? r.VendorLedgerId.ToString() : (r.VendorName ?? string.Empty);

                VendorPaymentSummary summary;
                if (!vendorSummaryMap.TryGetValue(vendorKey, out summary))
                {
                    summary = new VendorPaymentSummary
                    {
                        VendorLedgerId = r.VendorLedgerId,
                        VendorName = !string.IsNullOrEmpty(r.VendorName) ? r.VendorName : "Unknown Vendor",
                        TotalPaid = 0m,
                        TotalBillAmount = 0m,
                        TotalBillBalance = 0m,
                        CurrentVendorOutstanding = r.CurrentVendorOutstanding,
                        VoucherCount = 0,
                        LastPaymentDate = r.VoucherDate,
                        PrimaryPaymentMode = r.PaymentMode ?? "Cash",
                        BranchName = r.BranchName ?? string.Empty
                    };
                    vendorSummaryMap[vendorKey] = summary;
                    vendorVouchersMap[vendorKey] = new HashSet<int>();
                }
                else
                {
                    if (r.CurrentVendorOutstanding != 0m)
                        summary.CurrentVendorOutstanding = r.CurrentVendorOutstanding;

                    if (r.VoucherDate > summary.LastPaymentDate)
                    {
                        summary.LastPaymentDate = r.VoucherDate;
                        if (!string.IsNullOrEmpty(r.PaymentMode))
                            summary.PrimaryPaymentMode = r.PaymentMode;
                    }
                }

                summary.TotalPaid += r.PaymentAmount;
                summary.TotalBillAmount += r.BillAmount;
                summary.TotalBillBalance += r.BillBalance;
                if (r.VoucherId > 0)
                {
                    vendorVouchersMap[vendorKey].Add(r.VoucherId);
                }

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    bool matches = false;
                    if (dateCol.Date.HasValue)
                    {
                        matches = (r.VoucherDate.Date == dateCol.Date.Value.Date);
                    }
                    else if (dateCol.MonthNumber > 0)
                    {
                        matches = (r.VoucherDate.Month == dateCol.MonthNumber);
                    }
                    else if (dateCol.DayNumber > 0)
                    {
                        matches = (r.VoucherDate.Day == dateCol.DayNumber);
                    }

                    if (matches)
                    {
                        string cellKey = vendorKey + "_" + dateCol.ColumnKey;

                        decimal curPaid = 0m;
                        datePaymentMap.TryGetValue(cellKey, out curPaid);
                        datePaymentMap[cellKey] = curPaid + r.PaymentAmount;

                        decimal curBal = 0m;
                        dateBalanceMap.TryGetValue(cellKey, out curBal);
                        dateBalanceMap[cellKey] = curBal + r.BillBalance;
                    }
                }
            }

            foreach (var kvp in vendorVouchersMap)
            {
                if (vendorSummaryMap.TryGetValue(kvp.Key, out VendorPaymentSummary s))
                {
                    s.VoucherCount = kvp.Value.Count > 0 ? kvp.Value.Count : 1;
                }
            }

            // Populate rows
            foreach (VendorPaymentSummary vendor in vendorSummaryMap.Values.OrderBy(x => x.VendorName))
            {
                string vendorKey = vendor.VendorLedgerId > 0 ? vendor.VendorLedgerId.ToString() : vendor.VendorName;

                DataRow dr = table.NewRow();
                dr["VendorKey"] = vendorKey;
                dr["VendorLedgerId"] = vendor.VendorLedgerId;
                dr["VendorName"] = vendor.VendorName;

                foreach (PivotDateColumnInfo dateCol in dateColumns)
                {
                    string cellKey = vendorKey + "_" + dateCol.ColumnKey;
                    decimal cellVal = 0m;

                    if (_currentValueMode == ValueDisplayMode.BillBalance)
                    {
                        dateBalanceMap.TryGetValue(cellKey, out cellVal);
                    }
                    else
                    {
                        datePaymentMap.TryGetValue(cellKey, out cellVal);
                    }

                    dr[dateCol.ColumnKey] = cellVal;
                }

                dr["Total"] = (_currentValueMode == ValueDisplayMode.BillBalance)
                    ? vendor.TotalBillBalance
                    : vendor.TotalPaid;

                dr["CurrentVendorOutstanding"] = vendor.CurrentVendorOutstanding;
                dr["VoucherCount"] = vendor.VoucherCount;
                if (vendor.LastPaymentDate != DateTime.MinValue)
                {
                    dr["LastPaymentDate"] = vendor.LastPaymentDate;
                }
                dr["PrimaryPaymentMode"] = vendor.PrimaryPaymentMode;
                dr["TotalBillAmount"] = vendor.TotalBillAmount;
                dr["TotalBillBalance"] = vendor.TotalBillBalance;
                dr["BranchName"] = vendor.BranchName;

                table.Rows.Add(dr);
            }

            return table;
        }

        private DataTable BuildInvoicesDataTable(DatewiseVendorPaymentReportData reportData, List<DatewiseVendorPaymentReportRow> rawRows)
        {
            DataTable details = new DataTable("VendorInvoices");
            details.Columns.Add("VendorKey", typeof(string));
            details.Columns.Add("BillKey", typeof(string));
            details.Columns.Add("PurchaseNo", typeof(long));
            details.Columns.Add("InvoiceNo", typeof(string));
            details.Columns.Add("BillDate", typeof(DateTime));
            details.Columns.Add("InvoiceAmount", typeof(decimal));
            details.Columns.Add("PaidAmount", typeof(decimal));
            details.Columns.Add("ReturnedAmount", typeof(decimal));
            details.Columns.Add("Balance", typeof(decimal));
            details.Columns.Add("PaidInPeriod", typeof(decimal));
            details.Columns.Add("Vouchers", typeof(string));
            details.Columns.Add("PaymentMethod", typeof(string));
            details.Columns.Add("Status", typeof(string));
            details.Columns.Add("BranchName", typeof(string));

            HashSet<string> addedBills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (reportData?.Invoices != null && reportData.Invoices.Count > 0)
            {
                foreach (var inv in reportData.Invoices.OrderBy(x => x.VendorName).ThenByDescending(x => x.PurchaseNo))
                {
                    string vendorKey = inv.VendorLedgerId > 0 ? inv.VendorLedgerId.ToString() : (inv.VendorName ?? string.Empty);
                    string billKey = !string.IsNullOrEmpty(inv.BillKey) ? inv.BillKey : $"{vendorKey}_{inv.PurchaseNo}";
                    addedBills.Add(billKey);

                    DataRow dr = details.NewRow();
                    dr["VendorKey"] = vendorKey;
                    dr["BillKey"] = billKey;
                    dr["PurchaseNo"] = inv.PurchaseNo;
                    dr["InvoiceNo"] = !string.IsNullOrEmpty(inv.InvoiceNo) ? inv.InvoiceNo : (inv.PurchaseNo > 0 ? inv.PurchaseNo.ToString() : "-");
                    if (inv.BillDate != DateTime.MinValue && inv.BillDate > new DateTime(1900, 1, 1))
                    {
                        dr["BillDate"] = inv.BillDate.Date;
                    }
                    dr["InvoiceAmount"] = inv.InvoiceAmount;
                    dr["PaidAmount"] = inv.PaidAmount;
                    dr["ReturnedAmount"] = inv.ReturnedAmount;
                    dr["Balance"] = inv.Balance;
                    dr["PaidInPeriod"] = inv.PaidInPeriod;
                    dr["Vouchers"] = !string.IsNullOrEmpty(inv.Vouchers) ? inv.Vouchers : "-";
                    dr["PaymentMethod"] = !string.IsNullOrEmpty(inv.PaymentMethod) ? inv.PaymentMethod : "CASH";
                    dr["Status"] = !string.IsNullOrEmpty(inv.Status) ? inv.Status : "Active";
                    dr["BranchName"] = inv.BranchName ?? string.Empty;
                    details.Rows.Add(dr);
                }
            }

            // Fallback: If any vendor or payment was in rawRows but not in Invoices, add them
            if (rawRows != null && rawRows.Count > 0)
            {
                foreach (var r in rawRows.OrderBy(x => x.VendorName).ThenByDescending(x => x.PurchaseNo))
                {
                    string vendorKey = r.VendorLedgerId > 0 ? r.VendorLedgerId.ToString() : (r.VendorName ?? string.Empty);
                    string billKey = $"{vendorKey}_{r.PurchaseNo}";

                    if (!addedBills.Contains(billKey))
                    {
                        addedBills.Add(billKey);
                        DataRow dr = details.NewRow();
                        dr["VendorKey"] = vendorKey;
                        dr["BillKey"] = billKey;
                        dr["PurchaseNo"] = r.PurchaseNo;
                        dr["InvoiceNo"] = !string.IsNullOrEmpty(r.InvoiceNo) ? r.InvoiceNo : (r.PurchaseNo > 0 ? r.PurchaseNo.ToString() : "-");
                        if (r.BillDate != DateTime.MinValue && r.BillDate > new DateTime(1900, 1, 1))
                        {
                            dr["BillDate"] = r.BillDate.Date;
                        }
                        dr["InvoiceAmount"] = r.BillAmount;
                        dr["PaidAmount"] = r.TotalBillPaid;
                        dr["ReturnedAmount"] = r.ReturnedAmount;
                        dr["Balance"] = r.BillBalance;
                        dr["PaidInPeriod"] = r.PaymentAmount;
                        dr["Vouchers"] = !string.IsNullOrEmpty(r.VoucherNo) ? $"#{r.VoucherNo}" : (r.VoucherId > 0 ? $"#{r.VoucherId}" : "-");
                        dr["PaymentMethod"] = !string.IsNullOrEmpty(r.PaymentMode) ? r.PaymentMode : "CASH";
                        dr["Status"] = !string.IsNullOrEmpty(r.Status) ? r.Status : "Active";
                        dr["BranchName"] = r.BranchName ?? string.Empty;
                        details.Rows.Add(dr);
                    }
                }
            }

            return details;
        }

        private DataTable BuildVouchersDataTable(DatewiseVendorPaymentReportData reportData, List<DatewiseVendorPaymentReportRow> rawRows)
        {
            DataTable vouchers = new DataTable("VoucherDetails");
            vouchers.Columns.Add("BillKey", typeof(string));
            vouchers.Columns.Add("VendorLedgerId", typeof(int));
            vouchers.Columns.Add("PurchaseNo", typeof(long));
            vouchers.Columns.Add("VoucherDate", typeof(DateTime));
            vouchers.Columns.Add("VoucherId", typeof(int));
            vouchers.Columns.Add("VoucherNo", typeof(string));
            vouchers.Columns.Add("InvoiceAmount", typeof(decimal));
            vouchers.Columns.Add("PaymentAmount", typeof(decimal));
            vouchers.Columns.Add("BalanceAmount", typeof(decimal));
            vouchers.Columns.Add("RunningBalance", typeof(decimal));
            vouchers.Columns.Add("PaymentMode", typeof(string));
            vouchers.Columns.Add("Narration", typeof(string));
            vouchers.Columns.Add("Status", typeof(string));
            vouchers.Columns.Add("BranchName", typeof(string));

            // Gather candidate vouchers
            List<DatewiseVendorVoucherRow> allVouchers = new List<DatewiseVendorVoucherRow>();

            if (reportData?.VoucherDetails != null && reportData.VoucherDetails.Count > 0)
            {
                allVouchers.AddRange(reportData.VoucherDetails);
            }
            else if (rawRows != null && rawRows.Count > 0)
            {
                foreach (var r in rawRows)
                {
                    string vendorKey = r.VendorLedgerId > 0 ? r.VendorLedgerId.ToString() : (r.VendorName ?? string.Empty);
                    string billKey = $"{vendorKey}_{r.PurchaseNo}";
                    allVouchers.Add(new DatewiseVendorVoucherRow
                    {
                        BillKey = billKey,
                        VendorLedgerId = r.VendorLedgerId,
                        PurchaseNo = r.PurchaseNo,
                        VoucherDate = r.VoucherDate,
                        VoucherId = r.VoucherId,
                        VoucherNo = r.VoucherNo,
                        PaymentAmount = r.PaymentAmount,
                        BalanceAmount = r.BalanceAmount,
                        PaymentMode = r.PaymentMode,
                        Narration = r.Narration,
                        Status = r.Status,
                        BranchName = r.BranchName
                    });
                }
            }

            // Group by BillKey to calculate accurate running balance (as in FrmPayment / FrmViewPayment)
            var groupedByBill = allVouchers
                .GroupBy(x => x.BillKey, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groupedByBill)
            {
                string billKey = group.Key;
                long purchaseNo = group.First().PurchaseNo;

                // Find original bill invoice amount
                decimal originalBillAmount = 0m;
                var inv = reportData?.Invoices?.FirstOrDefault(x => string.Equals(x.BillKey, billKey, StringComparison.OrdinalIgnoreCase) || x.PurchaseNo == purchaseNo);
                if (inv != null)
                {
                    originalBillAmount = inv.InvoiceAmount;
                }
                else
                {
                    var raw = rawRows?.FirstOrDefault(x => x.PurchaseNo == purchaseNo);
                    if (raw != null) originalBillAmount = raw.BillAmount;
                }

                // Sort vouchers chronologically
                var sortedVouchers = group.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherId).ToList();

                decimal runningInvoiceAmount = 0m;
                decimal runningBalance = 0m;

                for (int i = 0; i < sortedVouchers.Count; i++)
                {
                    var v = sortedVouchers[i];
                    decimal payAmt = v.PaymentAmount;

                    if (i == 0)
                    {
                        runningInvoiceAmount = originalBillAmount > 0 ? originalBillAmount : (v.BalanceAmount + payAmt);
                    }
                    else
                    {
                        runningInvoiceAmount = runningBalance;
                    }

                    runningBalance = runningInvoiceAmount - payAmt;

                    DataRow dr = vouchers.NewRow();
                    dr["BillKey"] = billKey;
                    dr["VendorLedgerId"] = v.VendorLedgerId;
                    dr["PurchaseNo"] = v.PurchaseNo;
                    if (v.VoucherDate != DateTime.MinValue)
                        dr["VoucherDate"] = v.VoucherDate.Date;
                    dr["VoucherId"] = v.VoucherId;
                    dr["VoucherNo"] = !string.IsNullOrEmpty(v.VoucherNo) ? v.VoucherNo : (v.VoucherId > 0 ? v.VoucherId.ToString() : "-");
                    dr["InvoiceAmount"] = runningInvoiceAmount;
                    dr["PaymentAmount"] = payAmt;
                    dr["BalanceAmount"] = runningBalance;
                    dr["RunningBalance"] = runningBalance;
                    dr["PaymentMode"] = !string.IsNullOrEmpty(v.PaymentMode) ? v.PaymentMode : "CASH";
                    dr["Narration"] = !string.IsNullOrEmpty(v.Narration) ? v.Narration : "-";
                    dr["Status"] = !string.IsNullOrEmpty(v.Status) ? v.Status : "Active";
                    dr["BranchName"] = v.BranchName ?? string.Empty;
                    vouchers.Rows.Add(dr);
                }
            }

            return vouchers;
        }

        private DataSet BuildMatrixDataSet(DatewiseVendorPaymentReportData reportData, List<DatewiseVendorPaymentReportRow> rawRows, DatewiseVendorPaymentReportFilter filter, out List<PivotDateColumnInfo> dateColumns)
        {
            DataTable matrixTable = BuildMatrixDataTable(rawRows, filter, out dateColumns);
            DataTable invoicesTable = BuildInvoicesDataTable(reportData, rawRows);
            DataTable vouchersTable = BuildVouchersDataTable(reportData, rawRows);

            DataSet ds = new DataSet("DatewiseVendorPaymentDataSet");
            ds.Tables.Add(matrixTable);
            ds.Tables.Add(invoicesTable);
            ds.Tables.Add(vouchersTable);

            DataRelation relationVendorInvoices = new DataRelation("VendorPaymentBreakdown", matrixTable.Columns["VendorKey"], invoicesTable.Columns["VendorKey"], false);
            ds.Relations.Add(relationVendorInvoices);

            DataRelation relationInvoiceVouchers = new DataRelation("InvoicePaymentHistory", invoicesTable.Columns["BillKey"], vouchersTable.Columns["BillKey"], false);
            ds.Relations.Add(relationInvoiceVouchers);

            return ds;
        }

        private void LoadReport()
        {
            Cursor prev = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
                string paymentType = Convert.ToString(ultraComboPaymentType.Value) ?? "ALL";
                string paymentMode = Convert.ToString(ultraComboPaymentMode.Value) ?? "ALL";
                int vendorId = ultraComboVendor.Value != null ? Convert.ToInt32(ultraComboVendor.Value) : 0;

                DatewiseVendorPaymentReportFilter filter = new DatewiseVendorPaymentReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    VendorLedgerId = vendorId,
                    PaymentType = paymentType,
                    PaymentModeFilter = paymentMode,
                    Status = "Active",
                    SearchText = string.Empty,
                    ViewMode = viewMode
                };

                if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
                {
                    filter.ViewMode = "DATE_MATRIX";
                    _reportData = _repository.GetReportData(filter);
                    _reportRows = _reportData.Payments;

                    List<PivotDateColumnInfo> matrixCols;
                    DataSet ds = BuildMatrixDataSet(_reportData, _reportRows, filter, out matrixCols);
                    _matrixDataSet = ds;
                    _matrixTable = ds.Tables["VendorPaymentMatrix"];
                    _currentMatrixDateColumns.Clear();
                    _currentMatrixDateColumns.AddRange(matrixCols);

                    gridReport.DataSource = null;
                    gridReport.DataSource = _matrixDataSet;
                }
                else
                {
                    _currentMatrixDateColumns.Clear();
                    _matrixTable = null;
                    _matrixDataSet = null;

                    filter.ViewMode = viewMode;
                    _reportData = _repository.GetReportData(filter);
                    _reportRows = _reportData.Payments;

                    gridReport.DataSource = null;
                    gridReport.DataSource = _reportRows;
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Date-wise Vendor Payment Report: {ex.Message}", "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = prev;
            }
        }
        #endregion

        #region UltraGrid Layout & Row Formatting
        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
            column.Hidden = userHiddenColumnKeys.Contains(key);
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BackColor = GridHeaderBlue;
            column.Header.Appearance.BackColor2 = GridHeaderBlueDark;
            column.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
            column.Header.Appearance.ForeColor = Color.White;
            column.Header.Appearance.BorderColor = BorderBlue;
            column.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
            column.Header.Appearance.FontData.SizeInPoints = 8.25F;
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

            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = e.Layout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            layout.ViewStyleBand = ViewStyleBand.Vertical;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType = FilterUIType.HeaderIcons;
            layout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            layout.Override.FilterOperandStyle = FilterOperandStyle.Combo;
            layout.Override.FilterClearButtonLocation = FilterClearButtonLocation.Hidden;

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.SeparateElement;

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
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
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
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 20;
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

            UltraGridBand band = e.Layout.Bands[0];
            band.ColHeadersVisible = true;
            band.HeaderVisible = false;
            band.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band.Override.HeaderAppearance.ForeColor = Color.White;
            band.Override.HeaderAppearance.BorderColor = BorderBlue;
            band.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            band.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            band.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            if (isMatrix)
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    if (column.IsChaptered || string.Equals(column.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                if (band.Columns.Exists("VendorKey"))
                {
                    band.Columns["VendorKey"].Hidden = true;
                    band.Columns["VendorKey"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                if (band.Columns.Exists("VendorLedgerId"))
                {
                    band.Columns["VendorLedgerId"].Hidden = true;
                    band.Columns["VendorLedgerId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band, "VendorName", "Vendor Name", 240, null, HAlign.Left, 0);

                int visPos = 1;
                foreach (var dateCol in _currentMatrixDateColumns)
                {
                    if (band.Columns.Exists(dateCol.ColumnKey))
                    {
                        ConfigureGridColumn(band, dateCol.ColumnKey, dateCol.HeaderCaption, 60, "#,##0.00", HAlign.Right, visPos++);
                        string modeDesc = _currentValueMode == ValueDisplayMode.BillBalance ? "Bill Balance" : "Payment Amount";
                        band.Columns[dateCol.ColumnKey].Header.ToolTipText = $"{dateCol.Tooltip} ({modeDesc})";
                    }
                }

                ConfigureGridColumn(band, "Total", "Total Paid", 100, "#,##0.00", HAlign.Right, visPos++);
                if (band.Columns.Exists("Total"))
                {
                    band.Columns["Total"].Hidden = false;
                    band.Columns["Total"].Header.Caption = (_currentValueMode == ValueDisplayMode.BillBalance) ? "Total Balance" : "Total Paid";
                    band.Columns["Total"].Header.ToolTipText = "Total for period";
                    band.Columns["Total"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Total"].CellAppearance.ForeColor = PaidBlueColor;
                }

                ConfigureGridColumn(band, "CurrentVendorOutstanding", "Outstanding Bal", 115, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "VoucherCount", "Vouchers", 75, null, HAlign.Center, visPos++);
                ConfigureGridColumn(band, "LastPaymentDate", "Last Payment", 95, "dd-MMM-yyyy", HAlign.Center, visPos++);
                ConfigureGridColumn(band, "PrimaryPaymentMode", "Primary Mode", 95, null, HAlign.Center, visPos++);

                ConfigureGridColumn(band, "TotalBillAmount", "Total Bill Amt", 105, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "TotalBillBalance", "Total Bill Bal", 105, "#,##0.00", HAlign.Right, visPos++);
                ConfigureGridColumn(band, "BranchName", "Branch", 100, null, HAlign.Left, visPos++);

                // By default hide chooser-only columns
                if (band.Columns.Exists("TotalBillAmount") && !userHiddenColumnKeys.Contains("TotalBillAmount"))
                {
                    band.Columns["TotalBillAmount"].Hidden = true;
                }
                if (band.Columns.Exists("TotalBillBalance") && !userHiddenColumnKeys.Contains("TotalBillBalance"))
                {
                    band.Columns["TotalBillBalance"].Hidden = true;
                }
                if (band.Columns.Exists("BranchName") && !userHiddenColumnKeys.Contains("BranchName"))
                {
                    band.Columns["BranchName"].Hidden = true;
                }

                if (band.Columns.Exists("VendorName"))
                {
                    band.Columns["VendorName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("CurrentVendorOutstanding"))
                {
                    band.Columns["CurrentVendorOutstanding"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
            else
            {
                foreach (UltraGridColumn column in band.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band, "VoucherDate", "Payment Date", 100, "dd-MMM-yyyy", HAlign.Center, 0);
                ConfigureGridColumn(band, "VoucherNo", "Voucher #", 80, null, HAlign.Center, 1);
                ConfigureGridColumn(band, "VendorName", "Vendor Name", 200, null, HAlign.Left, 2);
                ConfigureGridColumn(band, "PurchaseNo", "Purchase No", 90, null, HAlign.Left, 3);
                ConfigureGridColumn(band, "InvoiceNo", "Invoice No", 100, null, HAlign.Left, 4);
                ConfigureGridColumn(band, "BillDate", "Purchase Date", 95, "dd-MMM-yyyy", HAlign.Center, 5);
                ConfigureGridColumn(band, "BillAmount", "Invoice Amount", 105, "#,##0.00", HAlign.Right, 6);
                ConfigureGridColumn(band, "TotalBillPaid", "Paid Amount", 100, "#,##0.00", HAlign.Right, 7);
                ConfigureGridColumn(band, "ReturnedAmount", "Returned Amount", 100, "#,##0.00", HAlign.Right, 8);
                ConfigureGridColumn(band, "PaymentAmount", "Adjusted Amount", 110, "#,##0.00", HAlign.Right, 9);
                ConfigureGridColumn(band, "BillBalance", "Balance", 100, "#,##0.00", HAlign.Right, 10);
                ConfigureGridColumn(band, "PaymentMode", "Payment Method", 105, null, HAlign.Center, 11);
                ConfigureGridColumn(band, "Narration", "Remarks", 140, null, HAlign.Left, 12);
                ConfigureGridColumn(band, "Status", "Status", 75, null, HAlign.Center, 13);
                ConfigureGridColumn(band, "CurrentVendorOutstanding", "Outstanding Bal", 115, "#,##0.00", HAlign.Right, 14);
                ConfigureGridColumn(band, "BranchName", "Branch", 90, null, HAlign.Left, 15);

                if (band.Columns.Exists("VendorName"))
                {
                    band.Columns["VendorName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("PaymentAmount"))
                {
                    band.Columns["PaymentAmount"].CellAppearance.ForeColor = PaidBlueColor;
                    band.Columns["PaymentAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (band.Columns.Exists("CurrentVendorOutstanding"))
                {
                    band.Columns["CurrentVendorOutstanding"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            // Band 1: Hierarchical Vendor Invoices matching FrmPayment.cs
            if (e.Layout.Bands.Count > 1 && isMatrix)
            {
                UltraGridBand invoicesBand = e.Layout.Bands[1];
                invoicesBand.ColHeadersVisible = true;
                invoicesBand.HeaderVisible = true;
                invoicesBand.Header.Caption = "Vendor Invoices & Payment Status (matching FrmPayment)";
                invoicesBand.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                invoicesBand.Header.Appearance.BackColor2 = Color.FromArgb(31, 98, 145);
                invoicesBand.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                invoicesBand.Header.Appearance.ForeColor = Color.White;
                invoicesBand.Header.Appearance.BorderColor = BorderBlue;
                invoicesBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                invoicesBand.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                invoicesBand.Header.Appearance.FontData.SizeInPoints = 8.5F;

                invoicesBand.Override.AllowAddNew = AllowAddNew.No;
                invoicesBand.Override.AllowDelete = DefaultableBoolean.False;
                invoicesBand.Override.AllowUpdate = DefaultableBoolean.False;
                invoicesBand.Override.RowSelectors = DefaultableBoolean.False;

                invoicesBand.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                invoicesBand.Override.HeaderAppearance.BackColor2 = Color.FromArgb(50, 100, 150);
                invoicesBand.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                invoicesBand.Override.HeaderAppearance.ForeColor = Color.White;
                invoicesBand.Override.HeaderAppearance.BorderColor = BorderBlue;
                invoicesBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                invoicesBand.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                invoicesBand.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

                invoicesBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);
                invoicesBand.Override.RowAppearance.BackColor = Color.White;
                invoicesBand.Override.RowAppearance.BorderColor = GridRowLine;
                invoicesBand.Override.RowAlternateAppearance.BorderColor = GridRowLine;
                invoicesBand.Override.CellAppearance.BorderColor = GridRowLine;
                invoicesBand.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                invoicesBand.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                invoicesBand.Override.BorderStyleRow = UIElementBorderStyle.Solid;

                foreach (UltraGridColumn column in invoicesBand.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureChildGridColumn(invoicesBand, "PurchaseNo", "Purchase No", 95, null, HAlign.Left, 0);
                ConfigureChildGridColumn(invoicesBand, "InvoiceNo", "Invoice No", 105, null, HAlign.Left, 1);
                ConfigureChildGridColumn(invoicesBand, "BillDate", "Purchase Date", 95, "dd-MMM-yyyy", HAlign.Center, 2);
                ConfigureChildGridColumn(invoicesBand, "InvoiceAmount", "Invoice Amount", 115, "#,##0.00", HAlign.Right, 3);
                ConfigureChildGridColumn(invoicesBand, "PaidAmount", "Paid Amount", 115, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(invoicesBand, "ReturnedAmount", "Returned Amount", 105, "#,##0.00", HAlign.Right, 5);
                ConfigureChildGridColumn(invoicesBand, "Balance", "Balance", 105, "#,##0.00", HAlign.Right, 6);
                ConfigureChildGridColumn(invoicesBand, "PaidInPeriod", "Paid in Period", 115, "#,##0.00", HAlign.Right, 7);
                ConfigureChildGridColumn(invoicesBand, "Vouchers", "Vouchers", 110, null, HAlign.Center, 8);
                ConfigureChildGridColumn(invoicesBand, "PaymentMethod", "Payment Method", 100, null, HAlign.Center, 9);
                ConfigureChildGridColumn(invoicesBand, "Status", "Status", 90, null, HAlign.Center, 10);
                ConfigureChildGridColumn(invoicesBand, "BranchName", "Branch", 90, null, HAlign.Left, 11);

                if (invoicesBand.Columns.Exists("InvoiceAmount"))
                {
                    invoicesBand.Columns["InvoiceAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (invoicesBand.Columns.Exists("PaidAmount"))
                {
                    invoicesBand.Columns["PaidAmount"].CellAppearance.ForeColor = PaidBlueColor;
                    invoicesBand.Columns["PaidAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (invoicesBand.Columns.Exists("PaidInPeriod"))
                {
                    invoicesBand.Columns["PaidInPeriod"].CellAppearance.ForeColor = PaidBlueColor;
                    invoicesBand.Columns["PaidInPeriod"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }

                // Child summaries matching FrmPayment.cs footer
                invoicesBand.Override.SummaryDisplayArea = SummaryDisplayAreas.Bottom;
                invoicesBand.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;
                invoicesBand.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(235, 243, 252);
                invoicesBand.Override.SummaryValueAppearance.FontData.Bold = DefaultableBoolean.True;

                if (!invoicesBand.Summaries.Exists("Summary_Count") && invoicesBand.Columns.Exists("PurchaseNo"))
                {
                    var sumCount = invoicesBand.Summaries.Add("Summary_Count", SummaryType.Count, invoicesBand.Columns["PurchaseNo"]);
                    sumCount.DisplayFormat = "Invoices: {0}";
                    sumCount.Appearance.TextHAlign = HAlign.Left;
                }
                if (!invoicesBand.Summaries.Exists("Summary_InvoiceAmount") && invoicesBand.Columns.Exists("InvoiceAmount"))
                {
                    var sumInv = invoicesBand.Summaries.Add("Summary_InvoiceAmount", SummaryType.Sum, invoicesBand.Columns["InvoiceAmount"]);
                    sumInv.DisplayFormat = "{0:#,##0.00}";
                    sumInv.Appearance.TextHAlign = HAlign.Right;
                }
                if (!invoicesBand.Summaries.Exists("Summary_PaidAmount") && invoicesBand.Columns.Exists("PaidAmount"))
                {
                    var sumPaid = invoicesBand.Summaries.Add("Summary_PaidAmount", SummaryType.Sum, invoicesBand.Columns["PaidAmount"]);
                    sumPaid.DisplayFormat = "{0:#,##0.00}";
                    sumPaid.Appearance.TextHAlign = HAlign.Right;
                    sumPaid.Appearance.ForeColor = PaidBlueColor;
                }
                if (!invoicesBand.Summaries.Exists("Summary_ReturnedAmount") && invoicesBand.Columns.Exists("ReturnedAmount"))
                {
                    var sumRet = invoicesBand.Summaries.Add("Summary_ReturnedAmount", SummaryType.Sum, invoicesBand.Columns["ReturnedAmount"]);
                    sumRet.DisplayFormat = "{0:#,##0.00}";
                    sumRet.Appearance.TextHAlign = HAlign.Right;
                }
                if (!invoicesBand.Summaries.Exists("Summary_Balance") && invoicesBand.Columns.Exists("Balance"))
                {
                    var sumBal = invoicesBand.Summaries.Add("Summary_Balance", SummaryType.Sum, invoicesBand.Columns["Balance"]);
                    sumBal.DisplayFormat = "{0:#,##0.00}";
                    sumBal.Appearance.TextHAlign = HAlign.Right;
                }
                if (!invoicesBand.Summaries.Exists("Summary_PaidInPeriod") && invoicesBand.Columns.Exists("PaidInPeriod"))
                {
                    var sumPeriod = invoicesBand.Summaries.Add("Summary_PaidInPeriod", SummaryType.Sum, invoicesBand.Columns["PaidInPeriod"]);
                    sumPeriod.DisplayFormat = "{0:#,##0.00}";
                    sumPeriod.Appearance.TextHAlign = HAlign.Right;
                    sumPeriod.Appearance.ForeColor = PaidBlueColor;
                }
            }

            // Band 2: Voucher payment breakdown per invoice with Running Balance (matching FrmViewPayment)
            if (e.Layout.Bands.Count > 2 && isMatrix)
            {
                UltraGridBand vouchersBand = e.Layout.Bands[2];
                vouchersBand.ColHeadersVisible = true;
                vouchersBand.HeaderVisible = true;
                vouchersBand.Header.Caption = "Payment History & Running Balance (as in FrmPayment / FrmViewPayment)";
                vouchersBand.Header.Appearance.BackColor = Color.FromArgb(0, 122, 204);
                vouchersBand.Header.Appearance.BackColor2 = Color.FromArgb(0, 90, 160);
                vouchersBand.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                vouchersBand.Header.Appearance.ForeColor = Color.White;
                vouchersBand.Header.Appearance.BorderColor = BorderBlue;
                vouchersBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                vouchersBand.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                vouchersBand.Header.Appearance.FontData.SizeInPoints = 8.25F;

                vouchersBand.Override.AllowAddNew = AllowAddNew.No;
                vouchersBand.Override.AllowDelete = DefaultableBoolean.False;
                vouchersBand.Override.AllowUpdate = DefaultableBoolean.False;
                vouchersBand.Override.RowSelectors = DefaultableBoolean.False;

                vouchersBand.Override.HeaderAppearance.BackColor = Color.FromArgb(0, 122, 204);
                vouchersBand.Override.HeaderAppearance.ForeColor = Color.White;
                vouchersBand.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                vouchersBand.Override.RowAlternateAppearance.BackColor = Color.FromArgb(242, 242, 242);
                vouchersBand.Override.RowAppearance.BackColor = Color.White;
                vouchersBand.Override.RowAppearance.BorderColor = GridRowLine;
                vouchersBand.Override.RowAlternateAppearance.BorderColor = GridRowLine;
                vouchersBand.Override.CellAppearance.BorderColor = GridRowLine;

                foreach (UltraGridColumn column in vouchersBand.Columns)
                {
                    column.Hidden = true;
                    column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureChildGridColumn(vouchersBand, "PurchaseNo", "Purchase No", 100, null, HAlign.Center, 0);
                ConfigureChildGridColumn(vouchersBand, "VoucherDate", "Purchase Date", 100, "dd-MM-yyyy", HAlign.Center, 1);
                ConfigureChildGridColumn(vouchersBand, "InvoiceAmount", "Invoice Amount", 120, "#,##0.00", HAlign.Right, 2);
                ConfigureChildGridColumn(vouchersBand, "PaymentAmount", "Payment Amount", 120, "#,##0.00", HAlign.Right, 3);
                ConfigureChildGridColumn(vouchersBand, "BalanceAmount", "Balance", 120, "#,##0.00", HAlign.Right, 4);
                ConfigureChildGridColumn(vouchersBand, "RunningBalance", "RunningBalance", 120, "#,##0.00", HAlign.Right, 5);
                ConfigureChildGridColumn(vouchersBand, "VoucherNo", "Voucher #", 80, null, HAlign.Center, 6);
                ConfigureChildGridColumn(vouchersBand, "PaymentMode", "Payment Method", 100, null, HAlign.Center, 7);
                ConfigureChildGridColumn(vouchersBand, "Narration", "Remarks", 150, null, HAlign.Left, 8);
                ConfigureChildGridColumn(vouchersBand, "Status", "Status", 75, null, HAlign.Center, 9);
                ConfigureChildGridColumn(vouchersBand, "BranchName", "Branch", 90, null, HAlign.Left, 10);

                if (vouchersBand.Columns.Exists("RunningBalance"))
                {
                    vouchersBand.Columns["RunningBalance"].Hidden = true; // Exists in schema matching FrmViewPayment
                }

                if (vouchersBand.Columns.Exists("PaymentAmount"))
                {
                    vouchersBand.Columns["PaymentAmount"].CellAppearance.ForeColor = Color.Green;
                    vouchersBand.Columns["PaymentAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            ReportGridLayoutHelper.ApplySavedLayout(GetLayoutKey(), gridReport, userHiddenColumnKeys);
        }

        private static void ConfigureChildGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null) return;

            // Child Band 1: Vendor Invoices matching FrmPayment.cs
            if (e.Row.Band != null && e.Row.Band.Index == 1)
            {
                if (e.Row.Cells.Exists("PaidAmount"))
                {
                    e.Row.Cells["PaidAmount"].Appearance.ForeColor = PaidBlueColor;
                    e.Row.Cells["PaidAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (e.Row.Cells.Exists("PaidInPeriod"))
                {
                    if (decimal.TryParse(Convert.ToString(e.Row.Cells["PaidInPeriod"].Value), out decimal pVal) && pVal > 0m)
                    {
                        e.Row.Cells["PaidInPeriod"].Appearance.ForeColor = PaidBlueColor;
                        e.Row.Cells["PaidInPeriod"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
                if (e.Row.Cells.Exists("Balance") && e.Row.Cells["Balance"].Value != null)
                {
                    if (decimal.TryParse(Convert.ToString(e.Row.Cells["Balance"].Value), out decimal bal))
                    {
                        if (bal == 0m)
                        {
                            e.Row.Cells["Balance"].Appearance.ForeColor = ClearedGreenColor;
                            e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.False;
                        }
                        else if (bal > 0m)
                        {
                            e.Row.Cells["Balance"].Appearance.ForeColor = OutstandingRedColor;
                            e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;
                        }
                    }
                }
                if (e.Row.Cells.Exists("Status"))
                {
                    string status = Convert.ToString(e.Row.Cells["Status"].Value);
                    if (string.Equals(status, "Fully Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["Status"].Appearance.ForeColor = ClearedGreenColor;
                        e.Row.Cells["Status"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                    else if (string.Equals(status, "Partially Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["Status"].Appearance.ForeColor = Color.FromArgb(230, 81, 0);
                        e.Row.Cells["Status"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                    else if (string.Equals(status, "Unpaid", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Row.Cells["Status"].Appearance.ForeColor = OutstandingRedColor;
                        e.Row.Cells["Status"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
                return;
            }

            // Child Band 2: Voucher payment details
            if (e.Row.Band != null && e.Row.Band.Index == 2)
            {
                if (e.Row.Cells.Exists("PaymentAmount"))
                {
                    e.Row.Cells["PaymentAmount"].Appearance.ForeColor = ClearedGreenColor;
                    e.Row.Cells["PaymentAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (e.Row.Cells.Exists("BalanceAmount") && e.Row.Cells["BalanceAmount"].Value != null)
                {
                    if (decimal.TryParse(Convert.ToString(e.Row.Cells["BalanceAmount"].Value), out decimal balAfter))
                    {
                        if (balAfter > 0m)
                            e.Row.Cells["BalanceAmount"].Appearance.ForeColor = OutstandingRedColor;
                        else
                            e.Row.Cells["BalanceAmount"].Appearance.ForeColor = ClearedGreenColor;
                    }
                }
                return;
            }

            // Master Band 0
            bool isMatrix = string.Equals(Convert.ToString(ultraComboViewMode.Value), "DATE_MATRIX", StringComparison.OrdinalIgnoreCase);

            if (isMatrix)
            {
                foreach (var dateCol in _currentMatrixDateColumns)
                {
                    if (e.Row.Cells.Exists(dateCol.ColumnKey))
                    {
                        object val = e.Row.Cells[dateCol.ColumnKey].Value;
                        if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal numVal))
                        {
                            if (numVal == 0m)
                            {
                                e.Row.Cells[dateCol.ColumnKey].Appearance.ForeColor = ZeroMutedColor;
                                e.Row.Cells[dateCol.ColumnKey].Appearance.FontData.Bold = DefaultableBoolean.False;
                            }
                            else
                            {
                                e.Row.Cells[dateCol.ColumnKey].Appearance.ForeColor = PaidBlueColor;
                                e.Row.Cells[dateCol.ColumnKey].Appearance.FontData.Bold = DefaultableBoolean.True;
                            }
                        }
                    }
                }

                if (e.Row.Cells.Exists("Total"))
                {
                    object val = e.Row.Cells["Total"].Value;
                    if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal totalVal))
                    {
                        e.Row.Cells["Total"].Appearance.ForeColor = (totalVal > 0m) ? PaidBlueColor : ZeroMutedColor;
                        e.Row.Cells["Total"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }

                if (e.Row.Cells.Exists("CurrentVendorOutstanding"))
                {
                    object val = e.Row.Cells["CurrentVendorOutstanding"].Value;
                    if (val != null && val != DBNull.Value && decimal.TryParse(Convert.ToString(val), out decimal outstVal))
                    {
                        if (outstVal > 0m)
                            e.Row.Cells["CurrentVendorOutstanding"].Appearance.ForeColor = OutstandingRedColor;
                        else if (outstVal == 0m)
                            e.Row.Cells["CurrentVendorOutstanding"].Appearance.ForeColor = ClearedGreenColor;
                        else
                            e.Row.Cells["CurrentVendorOutstanding"].Appearance.ForeColor = ZeroMutedColor;
                        e.Row.Cells["CurrentVendorOutstanding"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                }
            }
            else
            {
                if (e.Row.Cells.Exists("PaymentAmount"))
                {
                    e.Row.Cells["PaymentAmount"].Appearance.ForeColor = PaidBlueColor;
                    e.Row.Cells["PaymentAmount"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                if (e.Row.Cells.Exists("BillBalance") && e.Row.Cells["BillBalance"].Value != null)
                {
                    if (decimal.TryParse(Convert.ToString(e.Row.Cells["BillBalance"].Value), out decimal bal))
                    {
                        if (bal == 0m)
                            e.Row.Cells["BillBalance"].Appearance.ForeColor = ClearedGreenColor;
                        else if (bal > 0m)
                            e.Row.Cells["BillBalance"].Appearance.ForeColor = OutstandingRedColor;
                    }
                }
            }
        }
        #endregion

        #region Actions & Event Handlers
        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnToggleValueMode_Click(object sender, EventArgs e)
        {
            _currentValueMode = (_currentValueMode == ValueDisplayMode.PaymentAmount)
                ? ValueDisplayMode.BillBalance
                : ValueDisplayMode.PaymentAmount;

            UpdateValueModeButtonText();

            string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
            if (string.Equals(viewMode, "DATE_MATRIX", StringComparison.OrdinalIgnoreCase))
            {
                string periodMode = Convert.ToString(ultraComboPeriodMode.Value) ?? "DATE_RANGE";
                string paymentType = Convert.ToString(ultraComboPaymentType.Value) ?? "ALL";
                string paymentMode = Convert.ToString(ultraComboPaymentMode.Value) ?? "ALL";
                int vendorId = ultraComboVendor.Value != null ? Convert.ToInt32(ultraComboVendor.Value) : 0;

                DatewiseVendorPaymentReportFilter filter = new DatewiseVendorPaymentReportFilter
                {
                    PeriodMode = periodMode,
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    Month = ultraComboMonth.Value != null ? Convert.ToInt32(ultraComboMonth.Value) : 0,
                    Year = ultraComboYear.Value != null ? Convert.ToInt32(ultraComboYear.Value) : 0,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    VendorLedgerId = vendorId,
                    PaymentType = paymentType,
                    PaymentModeFilter = paymentMode,
                    SearchText = string.Empty,
                    ViewMode = "DETAILED"
                };

                List<PivotDateColumnInfo> matrixCols;
                DataSet ds = BuildMatrixDataSet(_reportData, _reportRows, filter, out matrixCols);
                _matrixDataSet = ds;
                _matrixTable = ds.Tables["VendorPaymentMatrix"];
                _currentMatrixDateColumns.Clear();
                _currentMatrixDateColumns.AddRange(matrixCols);

                gridReport.DataSource = null;
                gridReport.DataSource = _matrixDataSet;

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating print preview: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            ShowReportPreview();
        }

        private void ShowReportPreview()
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Date-wise Vendor Payment Report - Preview";
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
                    Text = "DATE-WISE VENDOR PAYMENT REPORT",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Height = 24
                };

                string periodText = $"Period: {ultraComboPeriodMode.Text} | From: {dtFrom.DateTime:dd/MM/yyyy} To: {dtTo.DateTime:dd/MM/yyyy}";
                string vendorText = $"Vendor: {ultraComboVendor.Text}";
                string typeText = $"Type: {ultraComboPaymentType.Text}";
                string modeText = $"Mode: {ultraComboPaymentMode.Text}";
                string valueModeText = $"Display: {_currentValueMode}";

                Label metaLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"{periodText}  |  {vendorText}  |  {typeText}  |  {modeText}  |  {valueModeText}\nGenerated: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                    ForeColor = Color.FromArgb(232, 246, 255),
                    Font = new Font("Segoe UI", 8.25F, FontStyle.Regular)
                };

                header.Controls.Add(metaLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                preview.Controls.Add(previewGrid);
                preview.Controls.Add(header);

                if (_matrixDataSet != null)
                {
                    previewGrid.DataSource = _matrixDataSet.Copy();
                }
                else if (gridReport.DataSource != null)
                {
                    previewGrid.DataSource = gridReport.DataSource;
                }

                preview.ShowDialog(this);
            }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = $"DatewiseVendorPaymentReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();

                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                var visibleCols = band.Columns.Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered && !string.Equals(c.Key, "VendorPaymentBreakdown", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Header.VisiblePosition)
                    .ToList();

                builder.AppendLine(string.Join(",", visibleCols.Select(c => EscapeCsv(c.Header.Caption ?? c.Key))));

                foreach (UltraGridRow row in GetVisibleDataRows())
                {
                    var lineValues = visibleCols.Select(c =>
                    {
                        object val = row.Cells.Exists(c.Key) ? row.Cells[c.Key].Value : null;
                        if (val == null || val == DBNull.Value) return string.Empty;
                        if (val is decimal decVal)
                            return decVal.ToString("F2");
                        return EscapeCsv(Convert.ToString(val));
                    });
                    builder.AppendLine(string.Join(",", lineValues));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            string safe = value ?? string.Empty;
            if (!safe.Contains(",") && !safe.Contains("\"") && !safe.Contains("\n"))
                return safe;
            return $"\"{safe.Replace("\"", "\"\"")}\"";
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            _isLoading = true;
            try
            {
                _currentValueMode = ValueDisplayMode.PaymentAmount;
                UpdateValueModeButtonText();
                ultraComboPeriodMode.Value = "DATE_RANGE";
                dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtTo.Value = DateTime.Today;
                ultraComboMonth.Value = DateTime.Today.Month;
                ultraComboYear.Value = DateTime.Today.Year;
                ultraComboViewMode.Value = "DATE_MATRIX";
                ultraComboVendor.Value = 0;
                ultraComboPaymentType.Value = "ALL";
                ultraComboPaymentMode.Value = "ALL";
                UpdatePeriodControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }
            LoadReport();
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
        }

        private void btnVendorPicker_Click(object sender, EventArgs e)
        {
            try
            {
                using (frmVendorDig vendorDialog = new frmVendorDig())
                {
                    if (vendorDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        if (vendorDialog.SelectedVendorId > 0)
                        {
                            ultraComboVendor.Value = vendorDialog.SelectedVendorId;
                            LoadReport();
                        }
                    }
                }
            }
            catch
            {
                ultraComboVendor.Focus();
            }
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.Rows.ExpandAll(true);
                UpdateFooterCellPositions();
            }
            catch { }
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.Rows.CollapseAll(true);
                UpdateFooterCellPositions();
            }
            catch { }
        }

        private void OpenPaymentForm(long voucherId)
        {
            if (voucherId <= 0) return;
            try
            {
                var frmPayment = new PosBranch_Win.Accounts.FrmPayment(voucherId);
                var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                if (homeForm != null)
                {
                    var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (openFormInTabMethod != null)
                    {
                        openFormInTabMethod.Invoke(homeForm, new object[] { frmPayment, $"Payment #{voucherId}" });
                        return;
                    }
                }
                frmPayment.Show();
                frmPayment.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Payment #{voucherId}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetLayoutKey()
        {
            string viewMode = Convert.ToString(ultraComboViewMode.Value) ?? "DATE_MATRIX";
            return $"DatewiseVendorPaymentReport_{viewMode}";
        }
        #endregion
    }
}
