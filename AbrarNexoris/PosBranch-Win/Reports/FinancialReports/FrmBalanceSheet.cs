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
    /// following Schedule III format standards
    /// </summary>
    public partial class FrmBalanceSheet : Form
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
        private BalanceSheetRepository reportRepository;
        private BalanceSheetReport currentReport;
        #endregion

        #region Constructor
        public FrmBalanceSheet()
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
            ultraGridLiabilities.DataSource = null;
            ultraGridAssets.DataSource = null;
            ClearSummary();
        }

        public void Clear() => RibbonClear();

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

                // Setup Date Presets
                InitializeDateControls();

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
            lblTotalLiabilities.Appearance.ForeColor = Color.FromArgb(255, 230, 230);
            lblTotalLiabilities.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalLiabilities.Appearance.FontData.SizeInPoints = 9f;

            lblTotalCapital.Appearance.ForeColor = Color.White;
            lblTotalCapital.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalCapital.Appearance.FontData.SizeInPoints = 9f;

            lblTotalAssets.Appearance.ForeColor = Color.FromArgb(230, 255, 230);
            lblTotalAssets.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblTotalAssets.Appearance.FontData.SizeInPoints = 9f;

            lblDifferenceBadge.Appearance.ForeColor = Color.FromArgb(255, 255, 180);
            lblDifferenceBadge.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceBadge.Appearance.FontData.SizeInPoints = 10f;
            lblDifferenceBadge.Appearance.TextHAlign = HAlign.Right;

            // Grid Sub-panels
            panelNetProfit.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            lblNetProfitCaption.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblNetProfitCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblNetProfitValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblNetProfitValue.Appearance.FontData.Bold = DefaultableBoolean.True;

            panelDifference.Appearance.BackColor = Color.FromArgb(232, 245, 233);
            lblDifferenceCaption.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblDifferenceCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            lblDifferenceValue.Appearance.FontData.Bold = DefaultableBoolean.True;

            // Dock & Z-Order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();

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

        private void SetupLiabilitiesGrid()
        {
            ultraGridLiabilities.DisplayLayout.Reset();
            ApplyGridBaseSettings(ultraGridLiabilities);
            
            // Header colors
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9f;
            ultraGridLiabilities.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

            ultraGridLiabilities.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ultraGridLiabilities.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridLiabilities.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            
            ultraGridLiabilities.InitializeLayout += UltraGridLiabilities_InitializeLayout;
        }

        private void SetupAssetsGrid()
        {
            ultraGridAssets.DisplayLayout.Reset();
            ApplyGridBaseSettings(ultraGridAssets);
            
            // Header colors
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9f;
            ultraGridAssets.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

            ultraGridAssets.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ultraGridAssets.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridAssets.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;
            
            ultraGridAssets.InitializeLayout += UltraGridAssets_InitializeLayout;
        }

        private void ApplyGridBaseSettings(UltraGrid grid)
        {
            grid.UseOsThemes = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            grid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            grid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            
            grid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            grid.DisplayLayout.Override.RowSelectorWidth = 35;
            grid.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.GroupByBox.Hidden = true;
            
            grid.DisplayLayout.Override.MinRowHeight = 25;
            grid.DisplayLayout.Override.DefaultRowHeight = 25;
            
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            grid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            grid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }

        private void UltraGridLiabilities_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            band.ColHeadersVisible = true;
            
            foreach (var col in band.Columns)
                col.Hidden = true;

            ConfigureColumn(band, "LedgerName", "Particulars", 220, HAlign.Left, 0);
            ConfigureColumn(band, "GroupName", "Account Group", 140, HAlign.Left, 1);
            
            ConfigureColumn(band, "ParentGroupName", "Category", 130, HAlign.Left, 2);
            band.Columns["ParentGroupName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["ParentGroupName"].CellAppearance.ForeColor = ButtonTextBlue;

            ConfigureColumn(band, "ClosingBalance", "Amount (₹)", 130, HAlign.Right, 3);
            band.Columns["ClosingBalance"].Format = "N2";
            band.Columns["ClosingBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["ClosingBalance"].CellAppearance.ForeColor = Color.FromArgb(198, 40, 40);
            band.Columns["ClosingBalance"].Header.Appearance.TextHAlign = HAlign.Right;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void UltraGridAssets_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            band.ColHeadersVisible = true;
            
            foreach (var col in band.Columns)
                col.Hidden = true;

            ConfigureColumn(band, "LedgerName", "Particulars", 220, HAlign.Left, 0);
            ConfigureColumn(band, "GroupName", "Account Group", 140, HAlign.Left, 1);

            ConfigureColumn(band, "ParentGroupName", "Category", 130, HAlign.Left, 2);
            band.Columns["ParentGroupName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["ParentGroupName"].CellAppearance.ForeColor = ButtonTextBlue;

            ConfigureColumn(band, "ClosingBalance", "Amount (₹)", 130, HAlign.Right, 3);
            band.Columns["ClosingBalance"].Format = "N2";
            band.Columns["ClosingBalance"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Columns["ClosingBalance"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            band.Columns["ClosingBalance"].Header.Appearance.TextHAlign = HAlign.Right;
            
            band.Override.AllowColSizing = AllowColSizing.Free;
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void ConfigureColumn(UltraGridBand band, string key, string headerText, int width, HAlign align, int visiblePosition = -1)
        {
            if (band.Columns.Exists(key))
            {
                var col = band.Columns[key];
                col.Hidden = false;
                col.Header.Caption = headerText;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
                if (visiblePosition >= 0)
                {
                    col.Header.VisiblePosition = visiblePosition;
                }
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

            // Summary bar labels
            lblTotalLiabilities.Text = $"Total Liabilities: ₹ {s.TotalLiabilities:N2}";
            lblTotalCapital.Text = $"Total Capital: ₹ {s.TotalCapital:N2}";
            lblTotalAssets.Text = $"Total Assets: ₹ {s.TotalAssets:N2}";

            // Net Profit/Loss
            lblNetProfitValue.Text = $"₹ {Math.Abs(s.NetProfitLoss):N2}";
            if (s.NetProfitLoss >= 0)
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

            // Difference Panel & Footer Badge
            lblDifferenceValue.Text = $"₹ {s.Difference:N2}";
            if (s.Difference == 0)
            {
                lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32); // Green
                panelDifference.Appearance.BackColor = Color.FromArgb(232, 245, 233);
                lblDifferenceCaption.Text = "DIFFERENCE (Books Balance):";

                lblDifferenceBadge.Text = "★ BALANCED: ₹ 0.00";
                lblDifferenceBadge.Appearance.ForeColor = Color.FromArgb(255, 255, 180); // Gold
            }
            else
            {
                lblDifferenceValue.Appearance.ForeColor = Color.FromArgb(198, 40, 40); // Red
                panelDifference.Appearance.BackColor = Color.FromArgb(255, 235, 238);
                lblDifferenceCaption.Text = "DIFFERENCE (Mismatch):";

                lblDifferenceBadge.Text = $"★ MISMATCH: ₹ {s.Difference:N2}";
                lblDifferenceBadge.Appearance.ForeColor = Color.FromArgb(255, 200, 200); // Light Red
            }

            // Total Header Banners
            lblAssetsTitle.Text = $"  ASSETS ( Total: ₹ {s.TotalAssets:N2} )";
            lblLiabilitiesTitle.Text = $"  LIABILITIES & CAPITAL ( Total: ₹ {s.TotalLiabilities + s.TotalCapital + s.NetProfitLoss:N2} )";
        }

        private void ClearSummary()
        {
            lblTotalLiabilities.Text = "Total Liabilities: ₹ 0.00";
            lblTotalCapital.Text = "Total Capital: ₹ 0.00";
            lblTotalAssets.Text = "Total Assets: ₹ 0.00";
            lblNetProfitValue.Text = "₹ 0.00";
            lblDifferenceValue.Text = "₹ 0.00";
            lblDifferenceBadge.Text = "★ BALANCED: ₹ 0.00";
            lblAssetsTitle.Text = "  ASSETS";
            lblLiabilitiesTitle.Text = "  LIABILITIES & CAPITAL";
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

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGridLiabilities.Rows.Count == 0 && ultraGridAssets.Rows.Count == 0)
                {
                    MessageBox.Show("No data to preview.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ultraGridAssets.PrintPreview();
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
            finally
            {
                this.Cursor = Cursors.Default;
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
