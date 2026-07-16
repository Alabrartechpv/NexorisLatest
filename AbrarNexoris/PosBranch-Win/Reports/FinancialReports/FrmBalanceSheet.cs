using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;

namespace PosBranch_Win.Reports.FinancialReports
{
    /// <summary>
    /// Balance Sheet Report Form
    /// Displays a professional financial report using Infragistics UltraGrid
    /// following Schedule III vertical format standards
    /// </summary>
    public partial class FrmBalanceSheet : Form
    {
        #region Private Fields
        private BalanceSheetRepository reportRepository;
        private BalanceSheetReport currentReport;
        #endregion

        #region Constructor
        public FrmBalanceSheet()
        {
            InitializeComponent();
            InitializeForm();

            // Set grid custom themes
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
        }
        #endregion

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new BalanceSheetRepository();

                // Form Properties
                this.Text = "Balance Sheet";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                // Set default date range: current financial year (April 1 to today)
                int currentYear = DateTime.Now.Year;
                int fyStartYear = DateTime.Now.Month >= 4 ? currentYear : currentYear - 1;
                ultraDateTimeFrom.Value = new DateTime(fyStartYear, 4, 1);
                ultraDateTimeFrom.FormatString = "dd-MM-yyyy";
                ultraDateTimeTo.Value = DateTime.Now;
                ultraDateTimeTo.FormatString = "dd-MM-yyyy";

                // Keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;

                // Setup Panels
                InitializePanels();

                // Setup Grids
                SetupLiabilitiesGrid();
                SetupAssetsGrid();

                // Button Styling
                StyleButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePanels()
        {
            // Set panel colors
            ultraGroupBoxLiabilities.Appearance.BackColor = Color.FromArgb(183, 28, 28); // Dark Red
            ultraGroupBoxLiabilities.Appearance.ForeColor = Color.White;
            ultraGroupBoxLiabilities.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraGroupBoxLiabilities.Appearance.FontData.SizeInPoints = 11;

            ultraGroupBoxAssets.Appearance.BackColor = Color.FromArgb(27, 94, 32); // Dark Green
            ultraGroupBoxAssets.Appearance.ForeColor = Color.White;
            ultraGroupBoxAssets.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraGroupBoxAssets.Appearance.FontData.SizeInPoints = 11;

            ultraPanelSummary.Appearance.BackColor = Color.FromArgb(38, 50, 56);
            
            // Net Profit Panel
            panelNetProfit.Appearance.BackColor = Color.FromArgb(227, 242, 253);
            lblNetProfitCaption.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
            lblNetProfitCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitCaption.Appearance.FontData.SizeInPoints = 11;
            lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
            lblNetProfitValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitValue.Appearance.FontData.SizeInPoints = 14;

            // Difference Panel
            panelDifference.Appearance.BackColor = Color.FromArgb(250, 250, 250);
            lblDifferenceCaption.Appearance.ForeColor = Color.FromArgb(66, 66, 66);
            lblDifferenceCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceCaption.Appearance.FontData.SizeInPoints = 10;
            lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(66, 66, 66);
            lblDifferenceValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceValue.Appearance.FontData.SizeInPoints = 12;

            // Summary Bottom Panel
            StyleSummaryLabel(lblTotalLiabilitiesCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalLiabilitiesValue, Color.FromArgb(239, 83, 80), 12);
            
            StyleSummaryLabel(lblTotalCapitalCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalCapitalValue, Color.FromArgb(66, 165, 245), 12);

            StyleSummaryLabel(lblTotalAssetsCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblTotalAssetsValue, Color.FromArgb(102, 187, 106), 12);
        }

        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 9;
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

        private void SetupLiabilitiesGrid()
        {
            ultraGridLiabilities.DisplayLayout.Reset();
            ApplyGridBaseSettings(ultraGridLiabilities);
            
            // Header colors
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(211, 47, 47);
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(198, 40, 40);
            
            ultraGridLiabilities.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(255, 235, 238);
            ultraGridLiabilities.InitializeLayout += UltraGridLiabilities_InitializeLayout;
        }

        private void SetupAssetsGrid()
        {
            ultraGridAssets.DisplayLayout.Reset();
            ApplyGridBaseSettings(ultraGridAssets);
            
            // Header colors
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(56, 142, 60);
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(46, 125, 50);
            
            ultraGridAssets.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(232, 245, 233);
            ultraGridAssets.InitializeLayout += UltraGridAssets_InitializeLayout;
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

        private void UltraGridLiabilities_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            
            foreach (var col in band.Columns)
                col.Hidden = true;

            ConfigureColumn(band, "ParentGroupName", "Category", 160, HAlign.Left);
            band.Columns["ParentGroupName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["ParentGroupName"].CellAppearance.ForeColor = Color.FromArgb(183, 28, 28);

            ConfigureColumn(band, "GroupName", "Account Group", 180, HAlign.Left);
            ConfigureColumn(band, "LedgerName", "Particulars", 250, HAlign.Left);
            
            ConfigureColumn(band, "ClosingBalance", "Amount (₹)", 140, HAlign.Right);
            band.Columns["ClosingBalance"].Format = "N2";
            band.Columns["ClosingBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
        }

        private void UltraGridAssets_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            
            foreach (var col in band.Columns)
                col.Hidden = true;

            ConfigureColumn(band, "ParentGroupName", "Category", 160, HAlign.Left);
            band.Columns["ParentGroupName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["ParentGroupName"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);

            ConfigureColumn(band, "GroupName", "Account Group", 180, HAlign.Left);
            ConfigureColumn(band, "LedgerName", "Particulars", 250, HAlign.Left);
            
            ConfigureColumn(band, "ClosingBalance", "Amount (₹)", 140, HAlign.Right);
            band.Columns["ClosingBalance"].Format = "N2";
            band.Columns["ClosingBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ExtendLastColumn;
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

        #region Data Loading
        private void LoadReport()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = ultraDateTimeFrom.DateTime.Date;
                DateTime toDate = ultraDateTimeTo.DateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                currentReport = reportRepository.GetBalanceSheetReport(fromDate, toDate);

                if (currentReport != null)
                {
                    ultraGridLiabilities.DataSource = currentReport.LiabilitiesItems;
                    ultraGridLiabilities.DataBind();

                    ultraGridAssets.DataSource = currentReport.AssetsItems;
                    ultraGridAssets.DataBind();

                    UpdateSummary();
                }
                else
                {
                    ultraGridLiabilities.DataSource = null;
                    ultraGridAssets.DataSource = null;
                    ClearSummary();
                    MessageBox.Show("No data found for the selected period.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateSummary()
        {
            if (currentReport?.Summary == null)
            {
                ClearSummary();
                return;
            }

            var s = currentReport.Summary;

            // Summary bar
            lblTotalLiabilitiesValue.Text = $"₹ {s.TotalLiabilities:N2}";
            lblTotalCapitalValue.Text = $"₹ {s.TotalCapital:N2}";
            lblTotalAssetsValue.Text = $"₹ {s.TotalAssets:N2}";

            // Net Profit/Loss
            lblNetProfitValue.Text = $"₹ {Math.Abs(s.NetProfitLoss):N2}";
            if (s.NetProfitLoss >= 0)
            {
                lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
                lblNetProfitCaption.Text = "★ NET PROFIT:";
                panelNetProfit.Appearance.BackColor = Color.FromArgb(227, 242, 253);
            }
            else
            {
                lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblNetProfitCaption.Text = "★ NET LOSS:";
                panelNetProfit.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            }

            // Difference Panel
            lblDifferenceValue.Text = $"₹ {s.Difference:N2}";
            if (s.Difference == 0)
            {
                lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(46, 125, 50); // Green
                panelDifference.Appearance.BackColor = Color.FromArgb(232, 245, 233);
                lblDifferenceCaption.Text = "DIFFERENCE (Books Balance):";
            }
            else
            {
                lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(198, 40, 40); // Red
                panelDifference.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                lblDifferenceCaption.Text = "DIFFERENCE (Mismatch):";
            }

            // Total Header Banners
            ultraGroupBoxAssets.Text = $"ASSETS ( Total: ₹ {s.TotalAssets:N2} )";
            ultraGroupBoxLiabilities.Text = $"LIABILITIES & CAPITAL ( Total: ₹ {s.TotalLiabilities + s.TotalCapital + s.NetProfitLoss:N2} )";
        }

        private void ClearSummary()
        {
            lblTotalLiabilitiesValue.Text = "₹ 0.00";
            lblTotalCapitalValue.Text = "₹ 0.00";
            lblTotalAssetsValue.Text = "₹ 0.00";
            lblNetProfitValue.Text = "₹ 0.00";
            lblDifferenceValue.Text = "₹ 0.00";
            ultraGroupBoxAssets.Text = "ASSETS";
            ultraGroupBoxLiabilities.Text = "LIABILITIES & CAPITAL";
        }
        #endregion

        #region Button Events
        private void FrmBalanceSheet_Load(object sender, EventArgs e)
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
                if (currentReport == null || (currentReport.LiabilitiesItems.Count == 0 && currentReport.AssetsItems.Count == 0))
                {
                    MessageBox.Show("No data to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"BalanceSheet_{ultraDateTimeFrom.DateTime:yyyyMMdd}_to_{ultraDateTimeTo.DateTime:yyyyMMdd}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();

                        // Header Info
                        sb.AppendLine($"Balance Sheet");
                        sb.AppendLine($"Period: {ultraDateTimeFrom.DateTime:dd/MM/yyyy} to {ultraDateTimeTo.DateTime:dd/MM/yyyy}");
                        sb.AppendLine();

                        // Liabilities 
                        sb.AppendLine("=== LIABILITIES & CAPITAL ===");
                        sb.AppendLine("Category,Account Group,Particulars,Amount");
                        foreach (var item in currentReport.LiabilitiesItems)
                        {
                            sb.AppendLine($"\"{item.ParentGroupName}\",\"{item.GroupName}\",\"{item.LedgerName}\",{item.ClosingBalance:N2}");
                        }
                        sb.AppendLine($",,Net Profit/Loss:,{currentReport.Summary.NetProfitLoss:N2}");
                        sb.AppendLine($",,TOTAL LIABILITIES & EQUITY:,{(currentReport.Summary.TotalLiabilities + currentReport.Summary.TotalCapital + currentReport.Summary.NetProfitLoss):N2}");
                        sb.AppendLine();

                        // Assets
                        sb.AppendLine("=== ASSETS ===");
                        sb.AppendLine("Category,Account Group,Particulars,Amount");
                        foreach (var item in currentReport.AssetsItems)
                        {
                            sb.AppendLine($"\"{item.ParentGroupName}\",\"{item.GroupName}\",\"{item.LedgerName}\",{item.ClosingBalance:N2}");
                        }
                        sb.AppendLine($",,TOTAL ASSETS:,{currentReport.Summary.TotalAssets:N2}");
                        sb.AppendLine();

                        sb.AppendLine($"Difference:,{currentReport.Summary.Difference:N2}");

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
                if (currentReport == null) return;
                ultraGridAssets.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting print: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Keyboard Shortcuts
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.F5)
                {
                    btnGenerate_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    btnClose_Click(sender, e);
                    e.Handled = true;
                }
            }
            catch { }
        }
        #endregion
    }
}
