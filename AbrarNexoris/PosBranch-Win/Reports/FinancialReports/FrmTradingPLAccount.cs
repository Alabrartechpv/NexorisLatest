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
    public partial class FrmTradingPLAccount : Form
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
        #endregion

        #region Private Fields
        private TradingPLRepository reportRepository;
        private TradingPLReport currentReport;
        #endregion

        #region Constructor
        public FrmTradingPLAccount()
        {
            InitializeComponent();
            InitializeForm();
        }
        #endregion

        public void RibbonClear()
        {
            ultraComboPresetDates.Value = "CURRENT_FY";
            ApplyDatePreset("CURRENT_FY");
            currentReport = null;
            ultraGridTrading.DataSource = null;
            ClearSummary();
        }

        public void Clear() => RibbonClear();

        #region Form Initialization
        private void InitializeForm()
        {
            try
            {
                reportRepository = new TradingPLRepository();

                // Form Properties
                this.Text = "Trading Account";
                this.WindowState = FormWindowState.Maximized;
                this.StartPosition = FormStartPosition.CenterScreen;

                // Setup Date Presets
                InitializeDateControls();

                // Keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += Form_KeyDown;

                // Setup Panels
                InitializePanels();

                // Setup Grid
                SetupTradingGrid();

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
            ultraComboPresetDates.Items.Add("CURRENT_FY", "Current FY");
            ultraComboPresetDates.Items.Add("TODAY", "Today");
            ultraComboPresetDates.Items.Add("THIS_MONTH", "This Month");
            ultraComboPresetDates.Items.Add("DATE_RANGE", "Date by Range");
            ultraComboPresetDates.Items.Add("ALL", "ALL");

            ultraComboPresetDates.Value = "CURRENT_FY";
            ApplyDatePreset("CURRENT_FY");
        }

        private void ApplyDatePreset(string presetKey)
        {
            int currentYear = DateTime.Now.Year;
            switch (presetKey)
            {
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

                case "ALL":
                    ultraDateTimeFrom.Value = new DateTime(1990, 1, 1);
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
            ultraPanelControls.Height = 58;

            // Label Styling
            lblPreset.Appearance.ForeColor = ControlTextColor;
            lblPreset.Appearance.FontData.Bold = DefaultableBoolean.False;
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

            // Footer Panel
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = BorderBlue;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Dock = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 34;

            // Footer Labels Styling
            lblOpeningStock.Appearance.ForeColor = Color.White;
            lblOpeningStock.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblOpeningStock.Appearance.FontData.SizeInPoints = 8.5f;

            lblTotalPurchases.Appearance.ForeColor = Color.FromArgb(255, 230, 230);
            lblTotalPurchases.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalPurchases.Appearance.FontData.SizeInPoints = 8.5f;

            lblTotalSales.Appearance.ForeColor = Color.FromArgb(230, 255, 230);
            lblTotalSales.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalSales.Appearance.FontData.SizeInPoints = 8.5f;

            lblClosingStock.Appearance.ForeColor = Color.White;
            lblClosingStock.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblClosingStock.Appearance.FontData.SizeInPoints = 8.5f;

            lblGrossProfit.Appearance.ForeColor = Color.FromArgb(255, 255, 180);
            lblGrossProfit.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblGrossProfit.Appearance.FontData.SizeInPoints = 10.5f;
            lblGrossProfit.Appearance.TextHAlign = HAlign.Right;

            // Dock & Z-Order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridTrading.BringToFront();

            UpdateSelectionToggleButtonText();
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

        private void SetupTradingGrid()
        {
            // Reset grid
            ultraGridTrading.DisplayLayout.Reset();
            
            // Basic settings
            ApplyGridBaseSettings(ultraGridTrading);
            
            // Header colors
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9f;
            ultraGridTrading.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

            // Highlight cells
            ultraGridTrading.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ultraGridTrading.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridTrading.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

            ultraGridTrading.InitializeLayout += UltraGridTrading_InitializeLayout;
            ultraGridTrading.InitializeRow += UltraGridTrading_InitializeRow;
        }

        private void UltraGridTrading_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.IsDataRow) return;

            string category = e.Row.Cells["Category"].Value?.ToString();
            if (category == "Opening Stock" || category == "Closing Stock" || category == "Gross Profit c/o" || category == "Gross Loss c/o")
            {
                e.Row.Appearance.FontData.Bold = DefaultableBoolean.True;

                if (category == "Closing Stock" || category == "Gross Profit c/o")
                {
                    e.Row.Appearance.BackColor = Color.FromArgb(232, 245, 233); // Soft Green
                    e.Row.Cells["LedgerName"].Appearance.ForeColor = Color.FromArgb(27, 94, 32); // Dark Green
                    e.Row.Cells["EffectiveAmount"].Appearance.ForeColor = Color.FromArgb(27, 94, 32);
                }
                else // Opening Stock or Gross Loss c/o
                {
                    e.Row.Appearance.BackColor = Color.FromArgb(255, 235, 238); // Soft Red
                    e.Row.Cells["LedgerName"].Appearance.ForeColor = Color.FromArgb(198, 40, 40); // Dark Red
                    e.Row.Cells["EffectiveAmount"].Appearance.ForeColor = Color.FromArgb(198, 40, 40);
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

        private void UltraGridTrading_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            band.ColHeadersVisible = true;
            
            // Hide unwanted columns if binding to an object directly
            foreach (var col in band.Columns)
            {
                col.Hidden = true;
            }

            // Show and configure required columns
            ConfigureColumn(band, "LedgerName", "Particulars", 320, HAlign.Left);
            band.Columns["LedgerName"].Header.VisiblePosition = 0;

            ConfigureColumn(band, "GroupName", "Account Group", 220, HAlign.Left);
            band.Columns["GroupName"].Header.VisiblePosition = 1;

            ConfigureColumn(band, "Category", "Category", 180, HAlign.Left);
            band.Columns["Category"].Header.VisiblePosition = 2;
            band.Columns["Category"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["Category"].CellAppearance.ForeColor = ButtonTextBlue;
            
            ConfigureColumn(band, "TotalDebit", "Debit (₹)", 140, HAlign.Right);
            band.Columns["TotalDebit"].Header.VisiblePosition = 3;
            band.Columns["TotalDebit"].Format = "N2";
            band.Columns["TotalDebit"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);

            ConfigureColumn(band, "TotalCredit", "Credit (₹)", 140, HAlign.Right);
            band.Columns["TotalCredit"].Header.VisiblePosition = 4;
            band.Columns["TotalCredit"].Format = "N2";
            band.Columns["TotalCredit"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);

            ConfigureColumn(band, "EffectiveAmount", "Amount (₹)", 150, HAlign.Right);
            band.Columns["EffectiveAmount"].Header.VisiblePosition = 5;
            band.Columns["EffectiveAmount"].Format = "N2";
            band.Columns["EffectiveAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
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
                    var tradingItems = new List<TradingPLLineItem>(currentReport.TradingItems);

                    // Add Gross Profit/Loss balancing row
                    if (currentReport.Summary.GrossProfit >= 0)
                    {
                        tradingItems.Add(new TradingPLLineItem
                        {
                            LedgerID = 0,
                            LedgerName = "Gross Profit c/o",
                            GroupID = 99,
                            GroupName = "TRADING SUMMARY",
                            Category = "Gross Profit c/o",
                            NormalBalance = "DEBIT",
                            TotalDebit = currentReport.Summary.GrossProfit,
                            TotalCredit = 0,
                            NetBalance = currentReport.Summary.GrossProfit
                        });
                    }
                    else
                    {
                        tradingItems.Add(new TradingPLLineItem
                        {
                            LedgerID = 0,
                            LedgerName = "Gross Loss c/o",
                            GroupID = 99,
                            GroupName = "TRADING SUMMARY",
                            Category = "Gross Loss c/o",
                            NormalBalance = "CREDIT",
                            TotalDebit = 0,
                            TotalCredit = Math.Abs(currentReport.Summary.GrossProfit),
                            NetBalance = -Math.Abs(currentReport.Summary.GrossProfit)
                        });
                    }

                    // Bind Trading Account grid
                    ultraGridTrading.DataSource = tradingItems;
                    ultraGridTrading.DataBind();

                    // Update summary
                    UpdateSummary();
                }
                else
                {
                    ultraGridTrading.DataSource = null;
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

            // Summary bar labels
            lblOpeningStock.Text = $"Opening Stock: ₹ {s.OpeningStock:N2}";
            lblTotalPurchases.Text = $"Purchases: ₹ {s.TotalPurchases:N2}";
            lblTotalSales.Text = $"Sales: ₹ {s.TotalSales:N2}";
            lblClosingStock.Text = $"Closing Stock: ₹ {s.ClosingStock:N2}";

            // Gross Profit badge
            if (s.GrossProfit >= 0)
            {
                lblGrossProfit.Text = $"★ GROSS PROFIT: ₹ {s.GrossProfit:N2}";
                lblGrossProfit.Appearance.ForeColor = Color.FromArgb(255, 255, 180); // Gold/Yellow highlight
            }
            else
            {
                lblGrossProfit.Text = $"★ GROSS LOSS: ₹ {Math.Abs(s.GrossProfit):N2}";
                lblGrossProfit.Appearance.ForeColor = Color.FromArgb(255, 200, 200); // Light Red highlight
            }
        }

        private void ClearSummary()
        {
            lblOpeningStock.Text = "Opening Stock: ₹ 0.00";
            lblTotalPurchases.Text = "Purchases: ₹ 0.00";
            lblTotalSales.Text = "Sales: ₹ 0.00";
            lblClosingStock.Text = "Closing Stock: ₹ 0.00";
            lblGrossProfit.Text = "★ GROSS PROFIT: ₹ 0.00";
        }
        #endregion

        #region Button Events
        private void FrmTradingPLAccount_Load(object sender, EventArgs e)
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
                if (ultraGridTrading.Rows.Count == 0)
                {
                    MessageBox.Show("No data to preview.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ultraGridTrading.PrintPreview();
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
                var items = ultraGridTrading.DataSource as List<TradingPLLineItem>;
                if (items == null || items.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"TradingAccount_{ultraDateTimeFrom.DateTime:yyyyMMdd}_to_{ultraDateTimeTo.DateTime:yyyyMMdd}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();

                        // Header Info
                        sb.AppendLine($"Trading Account");
                        sb.AppendLine($"Period: {ultraDateTimeFrom.DateTime:dd/MM/yyyy} to {ultraDateTimeTo.DateTime:dd/MM/yyyy}");
                        sb.AppendLine();

                        sb.AppendLine("Category,Particulars,Account Group,Debit,Credit,Amount");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"\"{item.Category}\",\"{item.LedgerName}\",\"{item.GroupName}\",{item.TotalDebit:N2},{item.TotalCredit:N2},{item.EffectiveAmount:N2}");
                        }
                        sb.AppendLine($",,,,Gross Profit/Loss:,{currentReport.Summary.GrossProfit:N2}");

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
                if (currentReport == null || currentReport.TradingItems.Count == 0)
                {
                    MessageBox.Show("No data to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ultraGridTrading.Print();
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
