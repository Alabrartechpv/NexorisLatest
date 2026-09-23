using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
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
            gridReport.InitializeRow += gridReport_InitializeRow;
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

        public void RibbonClear()
        {
            InitializeFilterControls();
            txtSearch.Text = string.Empty;
            ultraComboDateMode.Value = "ALL";
            ApplySearchFilter();
        }

        public void Clear() => RibbonClear();

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
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;

            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

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
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
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
                Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Bold),
                Location = new Point(10, 6)
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
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
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
            SetupGrid();
            gridReport.DataSource = _filteredRows;
            UpdateGridFooter();
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0 || e.Layout.Bands[0].Columns.Count == 0)
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

            if (band.Columns.Exists("Stock"))
                band.Columns["Stock"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            if (band.Columns.Exists("UnitCost"))
                band.Columns["UnitCost"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            if (band.Columns.Exists("RetailPrice"))
                band.Columns["RetailPrice"].CellAppearance.ForeColor = Color.FromArgb(1, 87, 155);
            if (band.Columns.Exists("WalkinPrice"))
                band.Columns["WalkinPrice"].CellAppearance.ForeColor = Color.FromArgb(0, 102, 204);
            if (band.Columns.Exists("StatusDate"))
                band.Columns["StatusDate"].CellAppearance.ForeColor = Color.FromArgb(128, 0, 128);

            e.Layout.AutoFitStyle = AutoFitStyle.None;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row.Cells.Exists("Stock"))
                {
                    decimal stock = Convert.ToDecimal(e.Row.Cells["Stock"].Value ?? 0);
                    if (stock < 0)
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(254, 226, 226);
                        e.Row.Appearance.ForeColor = Color.FromArgb(153, 27, 27);
                    }
                }
            }
            catch { }
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
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

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
