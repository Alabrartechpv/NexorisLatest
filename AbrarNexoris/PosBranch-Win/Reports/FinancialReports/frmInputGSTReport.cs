using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmInputGSTReport : Form
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
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private readonly InputGSTReportRepository _repository;
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private bool _isLoading;

        public frmInputGSTReport()
        {
            _repository = new InputGSTReportRepository();
            _footerLabels = new Dictionary<string, Label>();
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmInputGSTReport_Load;
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;

            ultraComboReportView.ValueChanged += filter_ValueChanged;
            txtSearch.TextChanged += filter_ValueChanged;
            dtFrom.ValueChanged += filter_ValueChanged;
            dtTo.ValueChanged += filter_ValueChanged;

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.Resize += gridReport_Resize;
            gridReport.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();

            KeyPreview = true;
            KeyDown += frmInputGSTReport_KeyDown;
        }

        private void frmInputGSTReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "Input GST & ITC Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = today.AddDays(-30);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            ultraComboReportView.Items.Clear();
            ultraComboReportView.Items.Add("REGISTER", "Purchase GST Register");
            ultraComboReportView.Items.Add("SUMMARY", "Input GST Summary");
            ultraComboReportView.Items.Add("RATE_WISE", "Input GST Rate-wise");
            ultraComboReportView.Items.Add("ITC", "ITC Eligible / Ineligible");
            ultraComboReportView.Items.Add("RECON", "GSTR-2B Reconciliation");
            ultraComboReportView.Value = "REGISTER";

            txtSearch.Text = string.Empty;
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

            StyleLabel(lblReportView);
            StyleLabel(lblSearch);
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
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
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboReportView, true);
            StyleFilterCombo(txtSearch, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
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
            combo.DropDownStyle = isDropDownList ? DropDownStyle.DropDownList : DropDownStyle.DropDown;
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

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.Prompt = "Drag a column header here to group by supplier, date, or rate";
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.HeaderStyle = HeaderStyle.Standard;

            // Row Selectors (matching Image 2)
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.ThemedElementAlpha = Alpha.Transparent;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Header Appearance (matching Image 2: regular font, non-bold 8.25pt)
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            // Active & Selected Row/Cell Appearance (matching Image 2)
            layout.Override.ActiveCellAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveCellAppearance.ForeColor = Color.White;
            layout.Override.ActiveCellAppearance.BorderColor = BorderBlue;
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

            gridReport.AfterColPosChanged += (s, ev) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, ev) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, ev) => UpdateFooterCellPositions();
            gridReport.Paint += (s, ev) => UpdateFooterCellPositions();
        }

        private void LoadReport()
        {
            if (_isLoading) return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                InputGSTReportFilter filter = new InputGSTReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    SearchText = txtSearch.Text.Trim()
                };

                gridReport.DataSource = null;

                string rawVal = Convert.ToString(ultraComboReportView.Value ?? "");
                string rawText = Convert.ToString(ultraComboReportView.Text ?? "");

                if (rawVal == "SUMMARY" || rawText.Contains("Summary"))
                {
                    gridReport.DataSource = _repository.GetInputSummary(filter);
                }
                else if (rawVal == "RATE_WISE" || rawText.Contains("Rate"))
                {
                    gridReport.DataSource = _repository.GetRateWiseSummary(filter);
                }
                else if (rawVal == "ITC" || rawText.Contains("ITC"))
                {
                    gridReport.DataSource = _repository.GetITCReport(filter);
                }
                else if (rawVal == "RECON" || rawText.Contains("Reconciliation") || rawText.Contains("2B"))
                {
                    gridReport.DataSource = _repository.GetGSTR2BReconciliation(filter);
                }
                else
                {
                    gridReport.DataSource = _repository.GetPurchaseRegister(filter);
                }

                CreateFooterCells();
                UpdateFooterValues();
                UpdateFooterCellPositions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Input GST Report.\n" + ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Header.Appearance.ThemedElementAlpha = Alpha.Transparent;
                col.Header.Appearance.BackColor = GridHeaderBlue;
                col.Header.Appearance.BackColor2 = GridHeaderBlueDark;
                col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                col.Header.Appearance.ForeColor = Color.White;
                col.Header.Appearance.FontData.Bold = DefaultableBoolean.False;
                col.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                col.Header.Appearance.FontData.SizeInPoints = 8.25F;

                if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(float) || col.DataType == typeof(int) || col.DataType == typeof(long))
                {
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Header.Appearance.TextHAlign = HAlign.Right;
                    if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(float))
                        col.Format = "N2";
                }
                else
                {
                    col.CellAppearance.TextHAlign = HAlign.Left;
                    col.Header.Appearance.TextHAlign = HAlign.Left;
                }
            }
        }

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden) continue;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(ultraPanelGridFooter.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };
                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                    _columnAggregations[column.Key] = "None"; // Defaults to None on page load!

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Tag = columnKey };
            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            AddFooterMenuItem(menu, "Sum", "Sum", isNumeric);
            AddFooterMenuItem(menu, "Min", "Min", true);
            AddFooterMenuItem(menu, "Max", "Max", true);
            AddFooterMenuItem(menu, "Count", "Count", true);
            AddFooterMenuItem(menu, "Average", "Avg", isNumeric);
            menu.Items.Add(new ToolStripSeparator());
            AddFooterMenuItem(menu, "None", "None", true);

            menu.Opening += (sender, e) =>
            {
                string current = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";
                foreach (ToolStripItem item in menu.Items)
                {
                    ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                    if (menuItem != null && menuItem.Tag != null)
                        menuItem.Checked = string.Equals(menuItem.Tag.ToString(), current, StringComparison.OrdinalIgnoreCase);
                }
            };
            return menu;
        }

        private void AddFooterMenuItem(ContextMenuStrip menu, string text, string tag, bool enabled)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text) { Tag = tag, Enabled = enabled };
            item.Click += FooterContextMenu_Click;
            menu.Items.Add(item);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ContextMenuStrip menu = item == null ? null : item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            _columnAggregations[menu.Tag.ToString()] = item.Tag.ToString();
            UpdateFooterValues();
        }

        private void UpdateFooterValues()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0) return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (var pair in _footerLabels)
            {
                string columnKey = pair.Key;
                Label footerLabel = pair.Value;
                string aggregation = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";

                if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, aggregation, visibleRows);
                string formattedText = FormatAggregationResult(columnKey, aggregation, result);
                footerLabel.Text = string.IsNullOrEmpty(formattedText) ? string.Empty : $"{aggregation}: {formattedText}";
                footerLabel.Tag = Tuple.Create(columnKey, footerLabel.Text);
                footerLabel.Invalidate();
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return aggregation == "Count" ? (object)0 : null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue).Sum(value => value.Value);
                case "Min":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderBy(value => value).FirstOrDefault();
                case "Max":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderByDescending(value => value).FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value)).Where(value => value.HasValue)
                        .Select(value => value.Value).ToList();
                    return values.Count == 0 ? 0m : values.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            if (result == null) return string.Empty;
            if (aggregation == "Count") return Convert.ToString(result);

            UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey)
                ? gridReport.DisplayLayout.Bands[0].Columns[columnKey]
                : null;
            decimal? numericValue = GetNumericValue(result);
            if (numericValue.HasValue)
                return column != null && !string.IsNullOrWhiteSpace(column.Format)
                    ? numericValue.Value.ToString(column.Format)
                    : numericValue.Value.ToString("N2");
            return Convert.ToString(result);
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
                   type == typeof(int) || type == typeof(long) || type == typeof(short);
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label footerLabel = sender as Label;
            if (footerLabel == null || footerLabel.Tag == null) return;

            Tuple<string, string> value = footerLabel.Tag as Tuple<string, string>;
            if (value == null || string.IsNullOrEmpty(value.Item2)) return;

            using (Pen pen = new Pen(GridFooterBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, footerLabel.Width - 1, footerLabel.Height - 1);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = gridReport.ActiveColScrollRegion != null ? gridReport.ActiveColScrollRegion.Position : 0;
            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                var headerUI = column.Header.GetUIElement();
                int left, width;

                if (headerUI != null)
                {
                    left = headerUI.Rect.Left;
                    width = headerUI.Rect.Width;
                }
                else
                {
                    left = calculatedX;
                    width = column.Width;
                }

                calculatedX += column.Width;

                footerLabel.Left = left;
                footerLabel.Width = width;
                footerLabel.Top = 0;
                footerLabel.Height = ultraPanelGridFooter.Height;
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private void gridReport_Resize(object sender, EventArgs e) => UpdateFooterCellPositions();

        private void filter_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            LoadReport();
        }

        private void btnViewGrid_Click(object sender, EventArgs e) => LoadReport();

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try { gridReport.PrintPreview(); }
            catch (Exception ex) { MessageBox.Show("Unable to preview grid.\n" + ex.Message, "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void btnPreviewReport_Click(object sender, EventArgs e) => btnPreviewGrid_Click(sender, e);

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "CSV File (*.csv)|*.csv";
                    dialog.FileName = $"Input_GST_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Export process initialized.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Export error: " + ex.Message); }
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void frmInputGSTReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) { LoadReport(); e.Handled = true; }
            else if (e.KeyCode == Keys.F6) { btnPreviewGrid_Click(sender, e); e.Handled = true; }
            else if (e.KeyCode == Keys.F7) { btnExportGrid_Click(sender, e); e.Handled = true; }
        }
    }
}
