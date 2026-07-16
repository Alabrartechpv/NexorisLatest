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
    public partial class FrmProfitLossAccount : Form
    {
        #region Private Fields
        private TradingPLRepository reportRepository;
        private TradingPLReport currentReport;
        #endregion

        #region Constructor
        public FrmProfitLossAccount()
        {
            InitializeComponent();
            InitializeForm();
        }
        #endregion

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new TradingPLRepository();

                // Form Properties
                this.Text = "Profit & Loss Account";
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

                // Setup Grid
                SetupProfitLossGrid();

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
            ultraGroupBoxPL.Appearance.BackColor = Color.FromArgb(74, 20, 140);
            ultraGroupBoxPL.Appearance.ForeColor = Color.White;
            ultraGroupBoxPL.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraGroupBoxPL.Appearance.FontData.SizeInPoints = 11;

            ultraPanelSummary.Appearance.BackColor = Color.FromArgb(38, 50, 56);
            
            // Net Profit Panel
            panelNetProfit.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            lblNetProfitCaption.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblNetProfitCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitCaption.Appearance.FontData.SizeInPoints = 12;
            lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblNetProfitValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitValue.Appearance.FontData.SizeInPoints = 16;

            // Summary Bottom Panel
            StyleSummaryLabel(lblGrossProfitBfCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblGrossProfitBfValue, Color.FromArgb(102, 187, 106), 12);
            
            StyleSummaryLabel(lblIndirectIncomesCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblIndirectIncomesValue, Color.FromArgb(102, 187, 106), 12);

            StyleSummaryLabel(lblIndirectExpensesCaption, Color.FromArgb(176, 190, 197), false);
            StyleSummaryValueLabel(lblIndirectExpensesValue, Color.FromArgb(239, 83, 80), 12);
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

        private void SetupProfitLossGrid()
        {
            // Reset grid
            ultraGridProfitLoss.DisplayLayout.Reset();
            
            // Basic settings
            ApplyGridBaseSettings(ultraGridProfitLoss);
            
            // Header colors
            ultraGridProfitLoss.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(74, 20, 140);
            ultraGridProfitLoss.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(106, 27, 154);
            
            // Highlight cells
            ultraGridProfitLoss.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(243, 229, 245);
            
            ultraGridProfitLoss.InitializeLayout += UltraGridProfitLoss_InitializeLayout;
            ultraGridProfitLoss.InitializeRow += UltraGridProfitLoss_InitializeRow;
        }

        private void UltraGridProfitLoss_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.IsDataRow) return;

            string category = e.Row.Cells["Category"].Value?.ToString();
            if (category == "Gross Profit b/f" || category == "Gross Loss b/f" || category == "Net Profit" || category == "Net Loss")
            {
                e.Row.Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Appearance.BackColor = Color.FromArgb(245, 245, 245);
                
                if (category == "Gross Profit b/f" || category == "Net Profit")
                {
                    e.Row.Cells["LedgerName"].Appearance.ForeColor = Color.FromArgb(27, 94, 32); // Dark Green
                }
                else
                {
                    e.Row.Cells["LedgerName"].Appearance.ForeColor = Color.FromArgb(198, 40, 40); // Dark Red
                }
            }
        }

        private void ApplyGridBaseSettings(UltraGrid grid)
        {
            grid.UseOsThemes = DefaultableBoolean.False; // Bypass OS theming to allow custom header appearance
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

        private void UltraGridProfitLoss_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            band.ColHeadersVisible = true; // Force column headers to be visible
            
            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            // Show and configure required columns
            ConfigureColumn(band, "LedgerName", "Particulars", 300, HAlign.Left);
            band.Columns["LedgerName"].Header.VisiblePosition = 0;

            ConfigureColumn(band, "GroupName", "Account Group", 200, HAlign.Left);
            band.Columns["GroupName"].Header.VisiblePosition = 1;

            ConfigureColumn(band, "Category", "Category", 160, HAlign.Left);
            band.Columns["Category"].Header.VisiblePosition = 2;
            band.Columns["Category"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["Category"].CellAppearance.ForeColor = Color.FromArgb(74, 20, 140);
            
            ConfigureColumn(band, "TotalDebit", "Debit (₹)", 130, HAlign.Right);
            band.Columns["TotalDebit"].Header.VisiblePosition = 3;
            band.Columns["TotalDebit"].Format = "N2";
            band.Columns["TotalDebit"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);

            ConfigureColumn(band, "TotalCredit", "Credit (₹)", 130, HAlign.Right);
            band.Columns["TotalCredit"].Header.VisiblePosition = 4;
            band.Columns["TotalCredit"].Format = "N2";
            band.Columns["TotalCredit"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);

            ConfigureColumn(band, "EffectiveAmount", "Amount (₹)", 140, HAlign.Right);
            band.Columns["EffectiveAmount"].Header.VisiblePosition = 5;
            band.Columns["EffectiveAmount"].Format = "N2";
            band.Columns["EffectiveAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            
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

                currentReport = reportRepository.GetTradingPLReport(fromDate, toDate);

                if (currentReport != null)
                {
                    // Copy list to avoid modifying repository cache
                    var plItems = new List<TradingPLLineItem>(currentReport.ProfitLossItems);

                    // Add Gross Profit/Loss brought forward (b/f) row at index 0
                    if (currentReport.Summary.GrossProfit >= 0)
                    {
                        plItems.Insert(0, new TradingPLLineItem
                        {
                            LedgerID = 0,
                            LedgerName = "Gross Profit b/f",
                            GroupID = 99,
                            GroupName = "TRADING SUMMARY",
                            Category = "Gross Profit b/f",
                            NormalBalance = "CREDIT",
                            TotalDebit = 0,
                            TotalCredit = currentReport.Summary.GrossProfit,
                            NetBalance = currentReport.Summary.GrossProfit
                        });
                    }
                    else
                    {
                        plItems.Insert(0, new TradingPLLineItem
                        {
                            LedgerID = 0,
                            LedgerName = "Gross Loss b/f",
                            GroupID = 99,
                            GroupName = "TRADING SUMMARY",
                            Category = "Gross Loss b/f",
                            NormalBalance = "DEBIT",
                            TotalDebit = Math.Abs(currentReport.Summary.GrossProfit),
                            TotalCredit = 0,
                            NetBalance = -Math.Abs(currentReport.Summary.GrossProfit)
                        });
                    }

                    // Add Net Profit/Loss balancing row at the end
                    if (currentReport.Summary.NetProfit >= 0)
                    {
                        plItems.Add(new TradingPLLineItem
                        {
                            LedgerID = 0,
                            LedgerName = "Net Profit",
                            GroupID = 100,
                            GroupName = "PL SUMMARY",
                            Category = "Net Profit",
                            NormalBalance = "DEBIT",
                            TotalDebit = currentReport.Summary.NetProfit,
                            TotalCredit = 0,
                            NetBalance = currentReport.Summary.NetProfit
                        });
                    }
                    else
                    {
                        plItems.Add(new TradingPLLineItem
                        {
                            LedgerID = 0,
                            LedgerName = "Net Loss",
                            GroupID = 100,
                            GroupName = "PL SUMMARY",
                            Category = "Net Loss",
                            NormalBalance = "CREDIT",
                            TotalDebit = 0,
                            TotalCredit = Math.Abs(currentReport.Summary.NetProfit),
                            NetBalance = -Math.Abs(currentReport.Summary.NetProfit)
                        });
                    }

                    // Bind Profit & Loss grid
                    ultraGridProfitLoss.DataSource = plItems;
                    ultraGridProfitLoss.DataBind();

                    // Update summary
                    UpdateSummary();
                }
                else
                {
                    ultraGridProfitLoss.DataSource = null;
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
            lblGrossProfitBfValue.Text = $"₹ {Math.Abs(s.GrossProfit):N2}";
            lblGrossProfitBfCaption.Text = s.GrossProfit >= 0
                ? "Gross Profit (B/F):"
                : "Gross Loss (B/F):";
            lblGrossProfitBfValue.Appearance.ForeColor = s.GrossProfit >= 0
                ? Color.FromArgb(102, 187, 106)
                : Color.FromArgb(239, 83, 80);
            lblIndirectIncomesValue.Text = $"₹ {s.TotalIndirectIncomes:N2}";
            lblIndirectExpensesValue.Text = $"₹ {s.TotalIndirectExpenses:N2}";

            // Net Profit
            lblNetProfitValue.Text = $"₹ {Math.Abs(s.NetProfit):N2}";
            if (s.NetProfit >= 0)
            {
                lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                lblNetProfitCaption.Text = "★ NET PROFIT:";
                panelNetProfit.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            }
            else
            {
                lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(183, 28, 28);
                lblNetProfitCaption.Text = "★ NET LOSS:";
                panelNetProfit.Appearance.BackColor = Color.FromArgb(255, 235, 238);
            }
        }

        private void ClearSummary()
        {
            lblGrossProfitBfValue.Text = "₹ 0.00";
            lblIndirectIncomesValue.Text = "₹ 0.00";
            lblIndirectExpensesValue.Text = "₹ 0.00";
            lblNetProfitValue.Text = "₹ 0.00";
        }
        #endregion

        #region Button Events
        private void FrmProfitLossAccount_Load(object sender, EventArgs e)
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
                var items = ultraGridProfitLoss.DataSource as List<TradingPLLineItem>;
                if (items == null || items.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"ProfitLossAccount_{ultraDateTimeFrom.DateTime:yyyyMMdd}_to_{ultraDateTimeTo.DateTime:yyyyMMdd}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();

                        // Header Info
                        sb.AppendLine($"Profit & Loss Account");
                        sb.AppendLine($"Period: {ultraDateTimeFrom.DateTime:dd/MM/yyyy} to {ultraDateTimeTo.DateTime:dd/MM/yyyy}");
                        sb.AppendLine();

                        sb.AppendLine("Category,Particulars,Account Group,Debit,Credit,Amount");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.Category}\",\"{item.LedgerName}\",\"{item.GroupName}\",{item.TotalDebit:N2},{item.TotalCredit:N2},{item.EffectiveAmount:N2}");
                        }
                        sb.AppendLine($",,,,Net Profit/Loss:,{currentReport.Summary.NetProfit:N2}");

                        File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Report exported successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentReport == null || currentReport.ProfitLossItems.Count == 0)
                {
                    MessageBox.Show("No data to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ultraGridProfitLoss.Print();
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling shortcut: {ex.Message}");
            }
        }
        #endregion
    }
}
