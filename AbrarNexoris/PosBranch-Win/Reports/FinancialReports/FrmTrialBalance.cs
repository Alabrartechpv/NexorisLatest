using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmTrialBalance : Form
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
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        // Muted gray for zero values
        private static readonly Color ZeroValueGray = Color.FromArgb(165, 175, 185);
        private static readonly Color ActiveAmountColor = Color.FromArgb(18, 49, 102);
        #endregion

        #region Private Fields
        private TrialBalanceRepository reportRepository;
        private TrialBalanceReport currentReport;
        private List<TrialBalanceLineItem> displayedLineItems;
        #endregion

        #region Constructor
        public FrmTrialBalance()
        {
            InitializeComponent();
            InitializeForm();
        }
        #endregion

        public void RibbonClear()
        {
            ultraComboPresetDates.Value = "ALL";
            ApplyDatePreset("ALL");
            txtSearch.Text = string.Empty;
            currentReport = null;
            displayedLineItems = null;
            ultraGridTrialBalance.DataSource = null;
            ClearSummary();
        }

        public void Clear() => RibbonClear();

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new TrialBalanceRepository();

                // Form Properties
                this.Text = "Trial Balance";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                // Setup Date Controls (Default: ALL records as requested)
                InitializeDateControls();

                // Keyboard shortcuts & Search Events
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;
                txtSearch.TextChanged += txtSearch_TextChanged;

                // Setup Panels
                InitializePanels();

                // Setup Grid
                SetupTrialBalanceGrid();

                // Button Styling
                StyleButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeDateControls()
        {
            ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
            ultraDateTimeTo.FormatString = "dd-MM-yyyy";

            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("ALL", "ALL");
            ultraComboPresetDates.Items.Add("CURRENT_FY", "Current FY");
            ultraComboPresetDates.Items.Add("TODAY", "Today");
            ultraComboPresetDates.Items.Add("THIS_MONTH", "This Month");
            ultraComboPresetDates.Items.Add("DATE_RANGE", "Date by Range");

            // Default: ALL (Everything as before)
            ultraComboPresetDates.Value = "ALL";
            ApplyDatePreset("ALL");
        }

        private void ApplyDatePreset(string presetKey)
        {
            int currentYear = DateTime.Now.Year;
            switch (presetKey)
            {
                case "ALL":
                    ultraDateTimeFrom.Value = new DateTime(1990, 1, 1);
                    ultraDateTimeTo.Value = DateTime.Today;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "CURRENT_FY":
                    int fyStartYear = DateTime.Now.Month >= 4 ? currentYear : currentYear - 1;
                    ultraDateTimeFrom.Value = new DateTime(fyStartYear, 4, 1);
                    ultraDateTimeTo.Value = DateTime.Today;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "TODAY":
                    ultraDateTimeFrom.Value = DateTime.Today;
                    ultraDateTimeTo.Value = DateTime.Today;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "THIS_MONTH":
                    ultraDateTimeFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    ultraDateTimeTo.Value = DateTime.Today;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "DATE_RANGE":
                default:
                    ultraDateTimeFrom.Enabled = true;
                    ultraDateTimeTo.Enabled = true;
                    break;
            }
        }

        private void UltraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            string selected = Convert.ToString(ultraComboPresetDates.Value ?? ultraComboPresetDates.Text);
            ApplyDatePreset(selected);
        }

        private void InitializePanels()
        {
            this.BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelControls.Height = 78;

            // Label Styling
            lblSearch.Appearance.ForeColor = ControlTextColor;
            lblPreset.Appearance.ForeColor = ControlTextColor;
            lblFromDate.Appearance.ForeColor = ControlTextColor;
            lblToDate.Appearance.ForeColor = ControlTextColor;
            lblSearchStatus.Appearance.ForeColor = ControlTextColor;
            lblSearchStatus.Appearance.FontData.SizeInPoints = 8.5f;

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

            // Footer Panel
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Dock = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 34;

            // Footer Labels Styling
            lblOpeningSummary.Appearance.ForeColor = Color.FromArgb(255, 255, 200);
            lblOpeningSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblOpeningSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblTransactionSummary.Appearance.ForeColor = Color.FromArgb(220, 240, 255);
            lblTransactionSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTransactionSummary.Appearance.FontData.SizeInPoints = 8.5f;

            lblClosingSummary.Appearance.ForeColor = Color.FromArgb(220, 255, 220);
            lblClosingSummary.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblClosingSummary.Appearance.FontData.SizeInPoints = 8.5f;

            // Initial Badge State (Balanced Default)
            SetBadgeState(isMismatch: false, difference: 0);

            // Dock & Z-Order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridTrialBalance.BringToFront();

            UpdateSelectionToggleButtonText();
        }

        private void SetBadgeState(bool isMismatch, decimal difference)
        {
            if (isMismatch)
            {
                // High-contrast Warning Red Pill
                lblDifferenceBadge.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                lblDifferenceBadge.Appearance.BackColor2 = Color.FromArgb(255, 205, 210);
                lblDifferenceBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblDifferenceBadge.Appearance.BorderColor = Color.FromArgb(198, 40, 40);
                lblDifferenceBadge.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblDifferenceBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblDifferenceBadge.Appearance.FontData.SizeInPoints = 9.5f;
                lblDifferenceBadge.Text = $"MISMATCH: ₹ {Math.Abs(difference):N2}";
            }
            else
            {
                // Clean Success Emerald Pill
                lblDifferenceBadge.Appearance.BackColor = Color.FromArgb(232, 245, 233);
                lblDifferenceBadge.Appearance.BackColor2 = Color.FromArgb(200, 230, 201);
                lblDifferenceBadge.Appearance.BackGradientStyle = GradientStyle.Vertical;
                lblDifferenceBadge.Appearance.BorderColor = Color.FromArgb(46, 125, 50);
                lblDifferenceBadge.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                lblDifferenceBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblDifferenceBadge.Appearance.FontData.SizeInPoints = 9.5f;
                lblDifferenceBadge.Text = "BALANCED: ₹ 0.00";
            }
        }

        private void StyleButtons()
        {
            btnGenerate.Text = "View Grid";
            btnPreviewGrid.Text = "Preview Grid";
            btnPrint.Text = "Preview Report";
            btnExport.Text = "Export Grid";
            btnClearFilters.Text = "Reset Filters";
            btnToggleSelection.Text = "Hide Selection";

            StyleClassicButton(btnGenerate);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnExport);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnToggleSelection);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
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

        private void SetupTrialBalanceGrid()
        {
            ultraGridTrialBalance.DisplayLayout.Reset();
            ApplyGridBaseSettings(ultraGridTrialBalance);
            
            // Header colors
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9f;
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

            // Highlight cells
            ultraGridTrialBalance.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ultraGridTrialBalance.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridTrialBalance.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

            ultraGridTrialBalance.InitializeLayout += UltraGridTrialBalance_InitializeLayout;
            ultraGridTrialBalance.InitializeRow += UltraGridTrialBalance_InitializeRow;
        }

        private void UltraGridTrialBalance_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.IsDataRow) return;

            // Format Amount Columns (Mute zero values, style active amounts clearly)
            FormatAmountCell(e.Row.Cells["OpeningDebit"]);
            FormatAmountCell(e.Row.Cells["OpeningCredit"]);
            FormatAmountCell(e.Row.Cells["TransactionDebit"]);
            FormatAmountCell(e.Row.Cells["TransactionCredit"]);
            FormatClosingAmountCell(e.Row.Cells["ClosingDebit"]);
            FormatClosingAmountCell(e.Row.Cells["ClosingCredit"]);
        }

        private void FormatAmountCell(UltraGridCell cell)
        {
            if (cell == null || cell.Value == null) return;

            if (decimal.TryParse(cell.Value.ToString(), out decimal val))
            {
                if (val == 0)
                {
                    cell.Appearance.ForeColor = ZeroValueGray;
                    cell.Appearance.FontData.Bold = DefaultableBoolean.False;
                }
                else
                {
                    cell.Appearance.ForeColor = ActiveAmountColor;
                    cell.Appearance.FontData.Bold = DefaultableBoolean.False;
                }
            }
        }

        private void FormatClosingAmountCell(UltraGridCell cell)
        {
            if (cell == null || cell.Value == null) return;

            if (decimal.TryParse(cell.Value.ToString(), out decimal val))
            {
                if (val == 0)
                {
                    cell.Appearance.ForeColor = ZeroValueGray;
                    cell.Appearance.FontData.Bold = DefaultableBoolean.False;
                }
                else
                {
                    cell.Appearance.ForeColor = ActiveAmountColor;
                    cell.Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        private void ApplyGridBaseSettings(UltraGrid grid)
        {
            grid.UseOsThemes = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 40;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.GroupByBox.Hidden = true;
            
            grid.DisplayLayout.Override.MinRowHeight = 26;
            grid.DisplayLayout.Override.DefaultRowHeight = 26;
            
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            grid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            grid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }

        private void UltraGridTrialBalance_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            band.ColHeadersVisible = true;

            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "LedgerName", "Particulars / Ledger", 220, HAlign.Left);
            band.Columns["LedgerName"].Header.VisiblePosition = 0;

            ConfigureColumn(band, "GroupName", "Account Group", 150, HAlign.Left);
            band.Columns["GroupName"].Header.VisiblePosition = 1;

            ConfigureColumn(band, "GroupType", "Category", 110, HAlign.Left);
            band.Columns["GroupType"].Header.VisiblePosition = 2;
            band.Columns["GroupType"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["GroupType"].CellAppearance.ForeColor = ButtonTextBlue;

            ConfigureAmountColumn(band, "OpeningDebit", "Opening Dr", 3);
            ConfigureAmountColumn(band, "OpeningCredit", "Opening Cr", 4);
            ConfigureAmountColumn(band, "TransactionDebit", "Transactions Dr", 5);
            ConfigureAmountColumn(band, "TransactionCredit", "Transactions Cr", 6);
            ConfigureAmountColumn(band, "ClosingDebit", "Closing Dr", 7);
            ConfigureAmountColumn(band, "ClosingCredit", "Closing Cr", 8);

            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void ConfigureAmountColumn(UltraGridBand band, string key, string headerText, int visiblePosition)
        {
            ConfigureColumn(band, key, headerText, 105, HAlign.Right);
            if (band.Columns.Exists(key))
            {
                band.Columns[key].Header.VisiblePosition = visiblePosition;
                band.Columns[key].Format = "N2";
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string headerText, int width, HAlign align)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            var col = band.Columns[key];
            col.Hidden = false;
            col.Header.Caption = headerText;
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
        }
        #endregion

        #region Data Loading & Searching
        private void LoadReport()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = ultraDateTimeFrom.DateTime.Date;
                DateTime toDate = ultraDateTimeTo.DateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                currentReport = reportRepository.GetTrialBalanceReport(fromDate, toDate);
                ApplySearchFilter();

                if (currentReport == null || currentReport.LineItems.Count == 0)
                {
                    MessageBox.Show("No trial balance rows found for the selected period.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ultraGridTrialBalance.DataSource = null;
                displayedLineItems = null;
                ClearSummary();
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ApplySearchFilter()
        {
            if (currentReport == null)
            {
                displayedLineItems = null;
                ultraGridTrialBalance.DataSource = null;
                ClearSummary();
                UpdateSearchStatus(0, 0);
                return;
            }

            string searchText = txtSearch.Text.Trim();
            IEnumerable<TrialBalanceLineItem> query = currentReport.LineItems;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(item =>
                    ContainsText(item.LedgerName, searchText)
                    || ContainsText(item.GroupName, searchText)
                    || ContainsText(item.GroupType, searchText)
                    || item.LedgerID.ToString().Contains(searchText));
            }

            displayedLineItems = query.ToList();
            ultraGridTrialBalance.DataSource = displayedLineItems;
            ultraGridTrialBalance.DataBind();
            UpdateSummary(displayedLineItems);
            UpdateSearchStatus(displayedLineItems.Count, currentReport.LineItems.Count);
        }

        private bool ContainsText(string value, string searchText)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void UpdateSearchStatus(int visibleRows, int totalRows)
        {
            if (totalRows == 0)
            {
                lblSearchStatus.Text = string.Empty;
                return;
            }

            lblSearchStatus.Text = visibleRows == totalRows
                ? $"Total: {totalRows:N0} rows"
                : $"Showing {visibleRows:N0} of {totalRows:N0} rows";
        }

        private void UpdateSummary(IEnumerable<TrialBalanceLineItem> lineItems = null)
        {
            if (currentReport?.Summary == null)
            {
                ClearSummary();
                return;
            }

            TrialBalanceSummary s = lineItems == null
                ? currentReport.Summary
                : CalculateSummary(lineItems);

            lblOpeningSummary.Text = $"Opening: Dr ₹ {s.TotalOpeningDebit:N2} | Cr ₹ {s.TotalOpeningCredit:N2}";
            lblTransactionSummary.Text = $"Period: Dr ₹ {s.TotalTransactionDebit:N2} | Cr ₹ {s.TotalTransactionCredit:N2}";
            lblClosingSummary.Text = $"Closing: Dr ₹ {s.TotalClosingDebit:N2} | Cr ₹ {s.TotalClosingCredit:N2}";

            bool isMismatch = Math.Abs(s.Difference) > 0.001m;
            SetBadgeState(isMismatch, s.Difference);
        }

        private TrialBalanceSummary CalculateSummary(IEnumerable<TrialBalanceLineItem> lineItems)
        {
            var items = lineItems?.ToList() ?? new List<TrialBalanceLineItem>();
            return new TrialBalanceSummary
            {
                TotalOpeningDebit = items.Sum(item => item.OpeningDebit),
                TotalOpeningCredit = items.Sum(item => item.OpeningCredit),
                TotalTransactionDebit = items.Sum(item => item.TransactionDebit),
                TotalTransactionCredit = items.Sum(item => item.TransactionCredit),
                TotalClosingDebit = items.Sum(item => item.ClosingDebit),
                TotalClosingCredit = items.Sum(item => item.ClosingCredit),
                Difference = items.Sum(item => item.ClosingDebit) - items.Sum(item => item.ClosingCredit)
            };
        }

        private void ClearSummary()
        {
            lblOpeningSummary.Text = "Opening: Dr ₹ 0.00 | Cr ₹ 0.00";
            lblTransactionSummary.Text = "Period: Dr ₹ 0.00 | Cr ₹ 0.00";
            lblClosingSummary.Text = "Closing: Dr ₹ 0.00 | Cr ₹ 0.00";
            SetBadgeState(isMismatch: false, difference: 0);
            lblSearchStatus.Text = string.Empty;
        }
        #endregion

        #region Button Events
        private void FrmTrialBalance_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGridTrialBalance.Rows.Count == 0)
                {
                    MessageBox.Show("No data to preview.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ultraGridTrialBalance.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during print preview: {ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                var exportItems = GetDisplayedLineItems();
                if (currentReport == null || exportItems.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"TrialBalance_{ultraDateTimeFrom.DateTime:yyyyMMdd}_to_{ultraDateTimeTo.DateTime:yyyyMMdd}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Trial Balance");
                        sb.AppendLine($"Period: {ultraDateTimeFrom.DateTime:dd/MM/yyyy} to {ultraDateTimeTo.DateTime:dd/MM/yyyy}");
                        sb.AppendLine();
                        sb.AppendLine("Particulars / Ledger,Account Group,Category,Opening Dr,Opening Cr,Transaction Dr,Transaction Cr,Closing Dr,Closing Cr");

                        foreach (var item in exportItems)
                        {
                            sb.AppendLine($"\"{item.LedgerName}\",\"{item.GroupName}\",\"{item.GroupType}\",{item.OpeningDebit:N2},{item.OpeningCredit:N2},{item.TransactionDebit:N2},{item.TransactionCredit:N2},{item.ClosingDebit:N2},{item.ClosingCredit:N2}");
                        }

                        TrialBalanceSummary exportSummary = CalculateSummary(exportItems);
                        sb.AppendLine();
                        sb.AppendLine($"TOTAL,,,{exportSummary.TotalOpeningDebit:N2},{exportSummary.TotalOpeningCredit:N2},{exportSummary.TotalTransactionDebit:N2},{exportSummary.TotalTransactionCredit:N2},{exportSummary.TotalClosingDebit:N2},{exportSummary.TotalClosingCredit:N2}");
                        sb.AppendLine($"Difference,,,,,,,,{exportSummary.Difference:N2}");
                        File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Report exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentReport == null || currentReport.LineItems.Count == 0)
                {
                    return;
                }

                ultraGridTrialBalance.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting print: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            RibbonClear();
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            if (btnToggleSelection != null)
            {
                btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private List<TrialBalanceLineItem> GetDisplayedLineItems()
        {
            return displayedLineItems ?? currentReport?.LineItems ?? new List<TrialBalanceLineItem>();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplySearchFilter();
        }
        #endregion

        #region Keyboard Shortcuts
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.F)
                {
                    txtSearch.Focus();
                    txtSearch.SelectAll();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F5)
                {
                    if (e.Control)
                        btnPreviewGrid_Click(sender, e);
                    else
                        btnGenerate_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F6)
                {
                    btnClearFilters_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape && txtSearch.Focused && !string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = string.Empty;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    btnClose_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.E)
                {
                    btnExportCsv_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    btnPrint_Click(sender, e);
                    e.Handled = true;
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
