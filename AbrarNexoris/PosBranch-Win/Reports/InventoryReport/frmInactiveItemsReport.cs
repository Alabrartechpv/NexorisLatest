using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmInactiveItemsReport : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private readonly InactiveItemsReportRepository _repository;
        private List<InactiveItemsReportRow> _allRows;
        private List<InactiveItemsReportRow> _filteredRows;
        private readonly Dictionary<string, Label> _footerLabels;
        private bool _isLoading;

        public frmInactiveItemsReport()
        {
            _repository = new InactiveItemsReportRepository();
            _allRows = new List<InactiveItemsReportRow>();
            _filteredRows = new List<InactiveItemsReportRow>();
            _footerLabels = new Dictionary<string, Label>();

            InitializeComponent();

            Load += frmInactiveItemsReport_Load;
            btnViewGrid.Click += btnViewGrid_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;

            ultraComboDateMode.ValueChanged += ultraComboDateMode_ValueChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.Resize += gridReport_Resize;

            KeyPreview = true;
            KeyDown += frmInactiveItemsReport_KeyDown;
        }

        private void frmInactiveItemsReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;
            try
            {
                Text = "Inactive Items Audit & History Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                LoadReportData();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL");
            ultraComboDateMode.Items.Add("RANGE", "Date Period by Range");
            ultraComboDateMode.Value = "ALL";

            UpdateDateControlState();
        }

        private void UpdateDateControlState()
        {
            string mode = Convert.ToString(ultraComboDateMode.Value ?? "ALL");
            bool isRangeMode = string.Equals(mode, "RANGE", StringComparison.OrdinalIgnoreCase);

            dtFrom.Enabled = isRangeMode;
            dtTo.Enabled = isRangeMode;
            lblFromDate.Enabled = isRangeMode;
            lblToDate.Enabled = isRangeMode;
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;

            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblSearch);

            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnPreviewGrid);
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

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboDateMode);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleTextEditor(txtSearch);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
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
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
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

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10;
        }

        private void SetupGrid()
        {
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;
            gridReport.DisplayLayout.Reset();
            gridReport.DisplayLayout.CaptionVisible = DefaultableBoolean.True;
            gridReport.Text = "Inactive Items Detailed Audit Report";
            gridReport.DisplayLayout.Appearance.BackColor = FormBackColor;
            gridReport.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            gridReport.DisplayLayout.MaxColScrollRegions = 1;
            gridReport.DisplayLayout.MaxRowScrollRegions = 1;

            UltraGridOverride ov = gridReport.DisplayLayout.Override;
            ov.AllowAddNew = AllowAddNew.No;
            ov.AllowDelete = DefaultableBoolean.False;
            ov.AllowUpdate = DefaultableBoolean.False;
            ov.CellClickAction = CellClickAction.RowSelect;
            ov.SelectTypeRow = SelectType.Single;
            ov.HeaderClickAction = HeaderClickAction.SortMulti;

            ov.HeaderAppearance.BackColor = GridHeaderBlue;
            ov.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ov.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ov.HeaderAppearance.ForeColor = Color.White;
            ov.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ov.HeaderAppearance.FontData.Name = "Segoe UI";
            ov.HeaderAppearance.FontData.SizeInPoints = 9F;

            ov.RowAppearance.BackColor = Color.White;
            ov.RowAppearance.ForeColor = Color.FromArgb(20, 20, 20);
            ov.RowAppearance.FontData.Name = "Segoe UI";
            ov.RowAppearance.FontData.SizeInPoints = 9F;

            ov.RowAlternateAppearance.BackColor = GridAltRow;

            ov.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ov.SelectedRowAppearance.ForeColor = Color.White;
            ov.ActiveRowAppearance.BackColor = GridSelectedBlue;
            ov.ActiveRowAppearance.ForeColor = Color.White;
        }

        private void InitializeGridFooter()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            Label lblSummary = new Label
            {
                Name = "lblSummary",
                Text = "Total Inactive Items: 0 | Total Stock: 0.00",
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(10, 3)
            };
            ultraPanelGridFooter.ClientArea.Controls.Add(lblSummary);
            _footerLabels["Summary"] = lblSummary;
        }

        private void UpdateGridFooter()
        {
            if (_footerLabels.TryGetValue("Summary", out Label lblSummary))
            {
                int count = _filteredRows?.Count ?? 0;
                decimal totalStock = _filteredRows?.Sum(x => x.Stock) ?? 0m;
                lblSummary.Text = $"Total Inactive Items: {count} | Total Stock: {totalStock:N2}";
            }
        }

        private void LoadReportData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                InactiveItemsReportFilter filter = new InactiveItemsReportFilter
                {
                    DateFilterMode = Convert.ToString(ultraComboDateMode.Value ?? "ALL"),
                    FromDate = dtFrom.Value != null ? Convert.ToDateTime(dtFrom.Value) : (DateTime?)null,
                    ToDate = dtTo.Value != null ? Convert.ToDateTime(dtTo.Value) : (DateTime?)null
                };

                _allRows = _repository.GetInactiveItemsReport(filter) ?? new List<InactiveItemsReportRow>();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inactive items report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ApplySearchFilter()
        {
            string search = txtSearch.Text?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(search))
            {
                _filteredRows = new List<InactiveItemsReportRow>(_allRows);
            }
            else
            {
                _filteredRows = _allRows.Where(x =>
                    (x.ItemNo != null && x.ItemNo.ToLowerInvariant().Contains(search)) ||
                    (x.Barcode != null && x.Barcode.ToLowerInvariant().Contains(search)) ||
                    (x.ItemName != null && x.ItemName.ToLowerInvariant().Contains(search)) ||
                    (x.CategoryName != null && x.CategoryName.ToLowerInvariant().Contains(search)) ||
                    (x.GroupName != null && x.GroupName.ToLowerInvariant().Contains(search)) ||
                    (x.BrandName != null && x.BrandName.ToLowerInvariant().Contains(search)) ||
                    (x.StatusReason != null && x.StatusReason.ToLowerInvariant().Contains(search)) ||
                    (x.InactivatedByUser != null && x.InactivatedByUser.ToLowerInvariant().Contains(search)) ||
                    (x.CounterName != null && x.CounterName.ToLowerInvariant().Contains(search)) ||
                    (x.BranchName != null && x.BranchName.ToLowerInvariant().Contains(search))
                ).ToList();
            }

            gridReport.DataSource = null;
            gridReport.DataSource = _filteredRows;
            UpdateGridFooter();
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            ConfigureGridColumn(band, "ItemNo", "Item Code", 100, null, HAlign.Left, 0);
            ConfigureGridColumn(band, "Barcode", "Barcode", 110, null, HAlign.Left, 1);
            ConfigureGridColumn(band, "ItemName", "Item Description", 220, null, HAlign.Left, 2);
            ConfigureGridColumn(band, "CategoryName", "Category", 120, null, HAlign.Left, 3);
            ConfigureGridColumn(band, "GroupName", "Group", 120, null, HAlign.Left, 4);
            ConfigureGridColumn(band, "BrandName", "Brand", 110, null, HAlign.Left, 5);
            ConfigureGridColumn(band, "Unit", "Unit", 70, null, HAlign.Left, 6);
            ConfigureGridColumn(band, "Stock", "Stock Qty", 90, "#,##0.00", HAlign.Right, 7);
            ConfigureGridColumn(band, "UnitCost", "Unit Cost", 90, "#,##0.00", HAlign.Right, 8);
            ConfigureGridColumn(band, "RetailPrice", "Retail Price", 95, "#,##0.00", HAlign.Right, 9);
            ConfigureGridColumn(band, "WalkinPrice", "Selling Price", 95, "#,##0.00", HAlign.Right, 10);
            ConfigureGridColumn(band, "StatusDate", "Inactivated Date", 140, "dd/MM/yyyy HH:mm", HAlign.Center, 11);
            ConfigureGridColumn(band, "StatusReason", "Inactivation Reason", 180, null, HAlign.Left, 12);
            ConfigureGridColumn(band, "InactivatedByUser", "Inactivated By User", 130, null, HAlign.Left, 13);
            ConfigureGridColumn(band, "CounterName", "Counter", 110, null, HAlign.Left, 14);
            ConfigureGridColumn(band, "BranchName", "Branch", 120, null, HAlign.Left, 15);

            e.Layout.AutoFitStyle = AutoFitStyle.None;
        }

        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BorderColor = GridRowLine;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Segoe UI";
            column.CellAppearance.FontData.SizeInPoints = 9F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReportData();
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void ExportToCsv()
        {
            try
            {
                if (_filteredRows == null || !_filteredRows.Any())
                {
                    MessageBox.Show("No data available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*";
                    sfd.FileName = $"Inactive_Items_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Item Code,Barcode,Item Description,Category,Group,Brand,Unit,Stock Qty,Unit Cost,Retail Price,Selling Price,Inactivated Date,Inactivation Reason,Inactivated By User,Counter,Branch");

                        foreach (var row in _filteredRows)
                        {
                            string dateStr = row.StatusDate.HasValue ? row.StatusDate.Value.ToString("dd/MM/yyyy HH:mm") : "";
                            sb.AppendLine($"\"{EscapeCsv(row.ItemNo)}\",\"{EscapeCsv(row.Barcode)}\",\"{EscapeCsv(row.ItemName)}\",\"{EscapeCsv(row.CategoryName)}\",\"{EscapeCsv(row.GroupName)}\",\"{EscapeCsv(row.BrandName)}\",\"{EscapeCsv(row.Unit)}\",{row.Stock:F2},{row.UnitCost:F2},{row.RetailPrice:F2},{row.WalkinPrice:F2},\"{dateStr}\",\"{EscapeCsv(row.StatusReason)}\",\"{EscapeCsv(row.InactivatedByUser)}\",\"{EscapeCsv(row.CounterName)}\",\"{EscapeCsv(row.BranchName)}\"");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Report exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("\"", "\"\"");
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            gridReport.PrintPreview();
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void ultraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                UpdateDateControlState();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                ApplySearchFilter();
            }
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateGridFooter();
        }

        private void frmInactiveItemsReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
