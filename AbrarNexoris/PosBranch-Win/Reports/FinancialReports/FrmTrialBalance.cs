using System;
using System.Collections.Generic;
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
        private TrialBalanceRepository reportRepository;
        private TrialBalanceReport currentReport;
        private List<TrialBalanceLineItem> displayedLineItems;

        public FrmTrialBalance()
        {
            InitializeComponent();
            InitializeForm();
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
        }

        private void InitializeForm()
        {
            try
            {
                reportRepository = new TrialBalanceRepository();

                Text = "Trial Balance";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                int currentYear = DateTime.Now.Year;
                int fyStartYear = DateTime.Now.Month >= 4 ? currentYear : currentYear - 1;
                ultraDateTimeFrom.Value = new DateTime(fyStartYear, 4, 1);
                ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
                ultraDateTimeTo.Value = DateTime.Now;
                ultraDateTimeTo.FormatString = "dd-MM-yyyy";

                KeyPreview = true;
                KeyDown += Form_KeyDown;
                txtSearch.TextChanged += txtSearch_TextChanged;

                InitializePanels();
                SetupTrialBalanceGrid();
                StyleButtons();
                StyleSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePanels()
        {
            ultraPanelSummary.Appearance.BackColor = Color.FromArgb(38, 50, 56);

            StyleSummaryLabel(lblTotalOpeningDrCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalOpeningDrValue, Color.FromArgb(255, 235, 59), 10);
            StyleSummaryLabel(lblTotalOpeningCrCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalOpeningCrValue, Color.FromArgb(255, 235, 59), 10);
            StyleSummaryLabel(lblTotalTransactionDrCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalTransactionDrValue, Color.FromArgb(64, 196, 255), 10);
            StyleSummaryLabel(lblTotalTransactionCrCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalTransactionCrValue, Color.FromArgb(64, 196, 255), 10);
            StyleSummaryLabel(lblTotalClosingDrCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalClosingDrValue, Color.FromArgb(105, 240, 174), 11);
            StyleSummaryLabel(lblTotalClosingCrCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalClosingCrValue, Color.FromArgb(105, 240, 174), 11);

            panelDifference.Appearance.BackColor = Color.FromArgb(250, 250, 250);
            lblDifferenceCaption.Appearance.ForeColor = Color.FromArgb(66, 66, 66);
            lblDifferenceCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceCaption.Appearance.FontData.SizeInPoints = 9;
            lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(66, 66, 66);
            lblDifferenceValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceValue.Appearance.FontData.SizeInPoints = 11;
        }

        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 8.5f;
        }

        private void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, int fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
        }

        private void StyleButtons()
        {
            StyleButton(btnGenerate, Color.FromArgb(25, 118, 210), Color.White);
            StyleButton(btnExport, Color.FromArgb(0, 121, 107), Color.White);
            StyleButton(btnPrint, Color.FromArgb(81, 45, 168), Color.White);
            StyleButton(btnClose, Color.FromArgb(198, 40, 40), Color.White);
        }

        private void StyleSearch()
        {
            lblSearch.Appearance.ForeColor = Color.FromArgb(55, 71, 79);
            lblSearch.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblSearch.Appearance.FontData.SizeInPoints = 8.5f;
            txtSearch.Appearance.BackColor = Color.White;
            txtSearch.Appearance.ForeColor = Color.FromArgb(33, 33, 33);
            txtSearch.Appearance.FontData.SizeInPoints = 9.5f;
            txtSearch.BorderStyle = UIElementBorderStyle.Solid;
            lblSearchStatus.Appearance.ForeColor = Color.FromArgb(84, 110, 122);
            lblSearchStatus.Appearance.FontData.SizeInPoints = 8.5f;
        }

        private void StyleButton(Infragistics.Win.Misc.UltraButton btn, Color backColor, Color foreColor)
        {
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = backColor;
            btn.Appearance.ForeColor = foreColor;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.BorderColor = backColor;
            btn.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
            btn.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btn.HotTrackAppearance.ForeColor = Color.White;
            btn.HotTrackAppearance.BorderColor = backColor;
        }

        private void SetupTrialBalanceGrid()
        {
            ultraGridTrialBalance.DisplayLayout.Reset();
            ApplyGridBaseSettings(ultraGridTrialBalance);
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(26, 35, 126);
            ultraGridTrialBalance.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(40, 53, 147);
            ultraGridTrialBalance.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(232, 234, 246);
            ultraGridTrialBalance.InitializeLayout += UltraGridTrialBalance_InitializeLayout;
        }

        private void ApplyGridBaseSettings(UltraGrid grid)
        {
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
            grid.DisplayLayout.Override.MinRowHeight = 28;
            grid.DisplayLayout.Override.DefaultRowHeight = 28;
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5f;
            grid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
        }

        private void UltraGridTrialBalance_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];

            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureColumn(band, "GroupType", "Category", 130, HAlign.Left);
            band.Columns["GroupType"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            ConfigureColumn(band, "GroupName", "Account Group", 150, HAlign.Left);
            ConfigureColumn(band, "LedgerName", "Particulars / Ledger", 220, HAlign.Left);
            ConfigureAmountColumn(band, "OpeningDebit", "Opening Dr");
            ConfigureAmountColumn(band, "OpeningCredit", "Opening Cr");
            ConfigureAmountColumn(band, "TransactionDebit", "Transactions Dr");
            ConfigureAmountColumn(band, "TransactionCredit", "Transactions Cr");
            ConfigureAmountColumn(band, "ClosingDebit", "Closing Dr");
            ConfigureAmountColumn(band, "ClosingCredit", "Closing Cr");

            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void ConfigureAmountColumn(UltraGridBand band, string key, string headerText)
        {
            ConfigureColumn(band, key, headerText, 120, HAlign.Right);
            if (band.Columns.Exists(key))
            {
                band.Columns[key].Format = "N2";
                if (key.StartsWith("Closing", StringComparison.Ordinal))
                {
                    band.Columns[key].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                }
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

        private void LoadReport()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                DateTime fromDate = ultraDateTimeFrom.DateTime.Date;
                DateTime toDate = ultraDateTimeTo.DateTime.Date.AddDays(1).AddSeconds(-1);

                currentReport = reportRepository.GetTrialBalanceReport(fromDate, toDate);
                ApplySearchFilter();

                if (currentReport.LineItems.Count == 0)
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
                Cursor = Cursors.Default;
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
                ? $"{totalRows:N0} rows"
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

            lblTotalOpeningDrValue.Text = FormatAmount(s.TotalOpeningDebit);
            lblTotalOpeningCrValue.Text = FormatAmount(s.TotalOpeningCredit);
            lblTotalTransactionDrValue.Text = FormatAmount(s.TotalTransactionDebit);
            lblTotalTransactionCrValue.Text = FormatAmount(s.TotalTransactionCredit);
            lblTotalClosingDrValue.Text = FormatAmount(s.TotalClosingDebit);
            lblTotalClosingCrValue.Text = FormatAmount(s.TotalClosingCredit);
            lblDifferenceValue.Text = FormatAmount(s.Difference);

            if (s.Difference == 0)
            {
                lblDifferenceCaption.Text = "DIFFERENCE:";
                lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(46, 125, 50);
                panelDifference.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            }
            else
            {
                lblDifferenceCaption.Text = "DIFFERENCE:";
                lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(198, 40, 40);
                panelDifference.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            }
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
            lblTotalOpeningDrValue.Text = FormatAmount(0);
            lblTotalOpeningCrValue.Text = FormatAmount(0);
            lblTotalTransactionDrValue.Text = FormatAmount(0);
            lblTotalTransactionCrValue.Text = FormatAmount(0);
            lblTotalClosingDrValue.Text = FormatAmount(0);
            lblTotalClosingCrValue.Text = FormatAmount(0);
            lblDifferenceValue.Text = FormatAmount(0);
            lblSearchStatus.Text = string.Empty;
        }

        private string FormatAmount(decimal amount)
        {
            return amount.ToString("N2");
        }

        private void FrmTrialBalance_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            LoadReport();
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
                        sb.AppendLine("Category,Account Group,Ledger,Opening Dr,Opening Cr,Transaction Dr,Transaction Cr,Closing Dr,Closing Cr");

                        foreach (var item in exportItems)
                        {
                            sb.AppendLine($"\"{item.GroupType}\",\"{item.GroupName}\",\"{item.LedgerName}\",{item.OpeningDebit:N2},{item.OpeningCredit:N2},{item.TransactionDebit:N2},{item.TransactionCredit:N2},{item.ClosingDebit:N2},{item.ClosingCredit:N2}");
                        }

                        TrialBalanceSummary exportSummary = CalculateSummary(exportItems);
                        sb.AppendLine();
                        sb.AppendLine($",,TOTAL,{exportSummary.TotalOpeningDebit:N2},{exportSummary.TotalOpeningCredit:N2},{exportSummary.TotalTransactionDebit:N2},{exportSummary.TotalTransactionCredit:N2},{exportSummary.TotalClosingDebit:N2},{exportSummary.TotalClosingCredit:N2}");
                        sb.AppendLine($",,Difference,,,,,,{exportSummary.Difference:N2}");
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
                    btnGenerate_Click(sender, e);
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
            }
            catch
            {
            }
        }
    }
}
