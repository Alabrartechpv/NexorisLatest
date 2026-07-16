using ModelClass.Accounts;
using Repository.Accounts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class FrmManualPartyBalanceReport : Form
    {
        private readonly ManualPartyBalanceRepository _repository;

        // Using standard tracking variables for loading state
        private bool isLoading = false;

        public FrmManualPartyBalanceReport()
        {
            _repository = new ManualPartyBalanceRepository();
            InitializeComponent();
            
            this.Load += FrmManualPartyBalanceReport_Load;
            
            // Wire up events
            btnLoad.Click += (s, e) => LoadReport();
            btnExport.Click += (s, e) => ExportCsv();
            btnClose.Click += (s, e) => Close();
            btnClearFilters.Click += BtnClearFilters_Click;
            
            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow += GridReport_InitializeRow;

            // Setup keyboard shortcuts
            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;
        }

        private void FrmManualPartyBalanceReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            isLoading = true;
            try
            {
                // Set defaults
                cmbPartyType.SelectedIndex = 0;
                cmbBalanceType.SelectedIndex = 0;
                dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                dtTo.Value = DateTime.Today;

                InitializePanels();
                StyleButtons();
                SetupGrid();
                
                LoadReport();
            }
            finally
            {
                isLoading = false;
            }
        }

        private void InitializePanels()
        {
            // Setup master panel (contains grid) - Modern clean white
            ultraPanelMaster.BackColor = Color.FromArgb(250, 251, 252);
            ultraPanelMaster.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;

            // Setup control panel - Modern gradient-like appearance
            ultraPanelControls.BackColor = Color.FromArgb(236, 240, 245);

            // Style summary caption labels - Modern bold headers
            StyleSummaryLabel(lblTotalAmountCaption, Color.FromArgb(25, 118, 210), true);
            StyleSummaryLabel(lblTotalSettledCaption, Color.FromArgb(56, 142, 60), true);
            StyleSummaryLabel(lblTotalRemainingCaption, Color.FromArgb(123, 31, 162), true);

            // Style summary value labels - Large, bold, colorful
            StyleSummaryValueLabel(lblTotalAmount, Color.FromArgb(13, 71, 161), 14);
            StyleSummaryValueLabel(lblTotalSettled, Color.FromArgb(27, 94, 32), 14);
            StyleSummaryValueLabel(lblTotalRemaining, Color.FromArgb(74, 20, 140), 16);
            
            // Set placeholders
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
            
            chkOpenOnly.Appearance.BackColor = Color.Transparent;
        }

        private void StyleSummaryLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, bool isBold)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = isBold ? DefaultableBoolean.True : DefaultableBoolean.False;
            label.Appearance.FontData.SizeInPoints = 10;
            label.Appearance.TextHAlign = Infragistics.Win.HAlign.Left;
        }

        private void StyleSummaryValueLabel(Infragistics.Win.Misc.UltraLabel label, Color foreColor, float fontSize)
        {
            label.Appearance.ForeColor = foreColor;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.SizeInPoints = fontSize;
            label.Appearance.TextHAlign = Infragistics.Win.HAlign.Left;
        }

        private void StyleButtons()
        {
            // Style search button - Primary Blue
            btnLoad.UseAppStyling = false;
            btnLoad.UseOsThemes = DefaultableBoolean.False;
            btnLoad.Appearance.BackColor = Color.FromArgb(25, 118, 210);
            btnLoad.Appearance.BackColor2 = Color.FromArgb(33, 150, 243);
            btnLoad.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnLoad.Appearance.ForeColor = Color.White;
            btnLoad.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnLoad.Appearance.FontData.SizeInPoints = 10;
            btnLoad.Appearance.BorderColor = Color.FromArgb(21, 101, 192);

            // Style clear filters button - Orange Accent
            btnClearFilters.UseAppStyling = false;
            btnClearFilters.UseOsThemes = DefaultableBoolean.False;
            btnClearFilters.Appearance.BackColor = Color.FromArgb(245, 124, 0);
            btnClearFilters.Appearance.BackColor2 = Color.FromArgb(255, 152, 0);
            btnClearFilters.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnClearFilters.Appearance.ForeColor = Color.White;
            btnClearFilters.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnClearFilters.Appearance.FontData.SizeInPoints = 10;
            btnClearFilters.Appearance.BorderColor = Color.FromArgb(230, 81, 0);

            // Style export button - Teal
            btnExport.UseAppStyling = false;
            btnExport.UseOsThemes = DefaultableBoolean.False;
            btnExport.Appearance.BackColor = Color.FromArgb(0, 121, 107);
            btnExport.Appearance.BackColor2 = Color.FromArgb(0, 150, 136);
            btnExport.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnExport.Appearance.ForeColor = Color.White;
            btnExport.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnExport.Appearance.FontData.SizeInPoints = 10;
            btnExport.Appearance.BorderColor = Color.FromArgb(0, 105, 92);
            
            // Style close button - Grey/Slate
            btnClose.UseAppStyling = false;
            btnClose.UseOsThemes = DefaultableBoolean.False;
            btnClose.Appearance.BackColor = Color.FromArgb(96, 125, 139);
            btnClose.Appearance.BackColor2 = Color.FromArgb(120, 144, 156);
            btnClose.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnClose.Appearance.FontData.SizeInPoints = 10;
            btnClose.Appearance.BorderColor = Color.FromArgb(69, 90, 100);

            SetButtonHoverEffects();
        }

        private void SetButtonHoverEffects()
        {
            // Search button hover
            btnLoad.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btnLoad.HotTrackAppearance.ForeColor = Color.White;

            // Clear filters button hover
            btnClearFilters.HotTrackAppearance.BackColor = Color.FromArgb(255, 167, 38);
            btnClearFilters.HotTrackAppearance.ForeColor = Color.White;

            // Export button hover
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
            btnExport.HotTrackAppearance.ForeColor = Color.White;
            
            // Close button hover
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(144, 164, 174);
            btnClose.HotTrackAppearance.ForeColor = Color.White;
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
            {
                btnExport.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnLoad.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnClearFilters.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnClose.PerformClick();
                e.Handled = true;
            }
        }
        
        private void BtnClearFilters_Click(object sender, EventArgs e)
        {
            dtFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtTo.Value = DateTime.Today;
            cmbPartyType.SelectedIndex = 0;
            cmbBalanceType.SelectedIndex = 0;
            txtSearch.Text = string.Empty;
            chkOpenOnly.Checked = true;
            
            LoadReport();
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            var layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorWidth = 40;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

            // Style row selectors
            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.CellClickAction = CellClickAction.RowSelect;
            
            layout.Override.MinRowHeight = 25;
            layout.Override.DefaultRowHeight = 25;

            // Modern selection colors
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

            // Modern header styling
            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

            // Alternate row colors
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);

            // Hover effects
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            layout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);

            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            var band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "EntryDate", "Date", 110, "dd-MMM-yyyy", HAlign.Left);
            ConfigureGridColumn(band, "PartyType", "Party Type", 95, null, HAlign.Left);
            ConfigureGridColumn(band, "PartyName", "Party Name", 220, null, HAlign.Left);
            ConfigureGridColumn(band, "BalanceType", "Type", 90, null, HAlign.Left);
            ConfigureGridColumn(band, "Amount", "Amount", 120, "₹ #,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "SettledAmount", "Settled", 120, "₹ #,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "RemainingAmount", "Remaining", 125, "₹ #,##0.00", HAlign.Right);
            ConfigureGridColumn(band, "Status", "Status", 90, null, HAlign.Center);
            ConfigureGridColumn(band, "Remarks", "Remarks", 260, null, HAlign.Left);
            
            // Apply specifics
            if (band.Columns.Exists("Amount"))
            {
                band.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                band.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
            if (band.Columns.Exists("SettledAmount"))
            {
                band.Columns["SettledAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }
            if (band.Columns.Exists("RemainingAmount"))
            {
                band.Columns["RemainingAmount"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
                band.Columns["RemainingAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
            if (band.Columns.Exists("PartyName"))
            {
                band.Columns["PartyName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }
            
            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private static void ConfigureGridColumn(
            UltraGridBand band,
            string key,
            string header,
            int width,
            string format = null,
            HAlign textAlign = HAlign.Left)
        {
            if (!band.Columns.Exists(key)) return;

            var col = band.Columns[key];
            col.Hidden = false;
            col.Header.Caption = header;
            col.Width = width;
            col.CellAppearance.TextHAlign = textAlign;

            if (!string.IsNullOrEmpty(format))
            {
                col.Format = format;
            }
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("Status"))
            {
                e.Row.Cells["Status"].Appearance.FontData.Bold = DefaultableBoolean.True;
                string status = e.Row.Cells["Status"].Value?.ToString();
                if (status == "Settled")
                {
                    e.Row.Cells["Status"].Appearance.ForeColor = Color.FromArgb(27, 94, 32); // Green
                }
                else
                {
                    e.Row.Cells["Status"].Appearance.ForeColor = Color.FromArgb(191, 54, 12); // Red-Orange
                }
            }
        }

        private void LoadReport()
        {
            List<ManualPartyBalanceEntry> entries = _repository.GetEntries(
                NormalizeFilter(cmbPartyType),
                NormalizeFilter(cmbBalanceType),
                txtSearch.Text.Trim(),
                chkOpenOnly.Checked,
                Convert.ToDateTime(dtFrom.Value).Date,
                Convert.ToDateTime(dtTo.Value).Date);

            gridReport.DataSource = entries;

            lblTotalAmount.Text = $"₹ {entries.Sum(x => x.Amount):N2}";
            lblTotalSettled.Text = $"₹ {entries.Sum(x => x.SettledAmount):N2}";
            lblTotalRemaining.Text = $"₹ {entries.Sum(x => x.RemainingAmount):N2}";
        }

        private void ExportCsv()
        {
            var entries = gridReport.DataSource as List<ManualPartyBalanceEntry>;
            if (entries == null || entries.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"ManualPartyBalanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var builder = new StringBuilder();
                builder.AppendLine("Date,Party Type,Party Name,Balance Type,Amount,Settled,Remaining,Status,Remarks");

                foreach (var entry in entries)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(entry.EntryDate.ToString("yyyy-MM-dd")),
                        EscapeCsv(entry.PartyType),
                        EscapeCsv(entry.PartyName),
                        EscapeCsv(entry.BalanceType),
                        entry.Amount.ToString("F2"),
                        entry.SettledAmount.ToString("F2"),
                        entry.RemainingAmount.ToString("F2"),
                        EscapeCsv(entry.Status),
                        EscapeCsv(entry.Remarks)));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string NormalizeFilter(Infragistics.Win.UltraWinEditors.UltraComboEditor comboBox)
        {
            string value = comboBox.Text;
            return string.Equals(value, "All", StringComparison.OrdinalIgnoreCase) ? null : value;
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
            {
                return safeValue;
            }

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }
    }
}
