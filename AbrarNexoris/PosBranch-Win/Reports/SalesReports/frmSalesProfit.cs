using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesProfit : Form
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
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private static readonly Color ProfitGreenColor = Color.FromArgb(27, 94, 32);
        private static readonly Color LossRedColor = Color.FromArgb(183, 28, 28);
        #endregion

        #region Private Fields
        private SalesProfitReportRepository _reportRepository;
        private List<SalesProfitViewModel> _rawData = new List<SalesProfitViewModel>();
        private List<SalesProfitViewModel> _filteredData = new List<SalesProfitViewModel>();
        private bool _isSelectionHidden = false;

        // Dynamic summary footer fields
        private Label lblCount;
        private readonly Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Average", "Min", "Max", "Count", "None" };
        private readonly HashSet<string> numericSummaryColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BillAmount",
            "Profit",
            "ProfitMarginPercent",
            "SubTotal",
            "TaxAmt"
        };

        // Column Chooser & Drag-Drop fields
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private readonly System.Windows.Forms.ToolTip gridToolTip = new System.Windows.Forms.ToolTip();
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Helper Classes
        private sealed class ColumnItem
        {
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public string ColumnKey { get; }
            public string DisplayText { get; }

            public override string ToString() => DisplayText;
        }

        public class SalesProfitViewModel
        {
            public int BillNo { get; set; }
            public DateTime BillDate { get; set; }
            public double BillAmount { get; set; }
            public double Profit { get; set; }
            public double ProfitMarginPercent { get; set; }
            public string PayMode { get; set; }
            public string CashMode { get; set; }
            public double SubTotal { get; set; }
            public double TaxAmt { get; set; }
        }
        #endregion

        #region Constructor
        public frmSalesProfit()
        {
            InitializeComponent();
            InitializePanels();
            InitializeSummaryFooter();
            InitializeGridContextMenuAndDragDrop();

            _reportRepository = new SalesProfitReportRepository();

            Load += FrmSalesProfit_Load;
            KeyPreview = true;
            KeyDown += FrmSalesProfit_KeyDown;

            // Wire action buttons
            btnGenerate.Click += (s, e) => LoadData();
            btnPreviewGrid.Click += (s, e) => PreviewGrid();
            btnPrint.Click += (s, e) => PrintReport();
            btnExportCsv.Click += (s, e) => ExportCsv();
            btnColumnChooser.Click += (s, e) => ShowColumnChooserDialog();
            btnClearFilters.Click += (s, e) => ResetFilters();
            btnToggleSelection.Click += (s, e) => ToggleSelectionPanel();

            ultraComboPresetDates.ValueChanged += UltraComboPresetDates_ValueChanged;
            txtSearch.ValueChanged += TxtSearch_ValueChanged;

            // Grid events
            ultraGridProfit.InitializeLayout += UltraGridProfit_InitializeLayout;
            ultraGridProfit.InitializeRow += UltraGridProfit_InitializeRow;
            ultraGridProfit.DoubleClickRow += UltraGridProfit_DoubleClickRow;
        }
        #endregion

        #region Form Lifecycle & Presets
        private void FrmSalesProfit_Load(object sender, EventArgs e)
        {
            PopulatePresetCombo();
            ultraComboPresetDates.Value = "THIS_MONTH";
            ApplyDatePreset("THIS_MONTH");
            LoadData();
        }

        private void PopulatePresetCombo()
        {
            ultraComboPresetDates.Items.Clear();
            ultraComboPresetDates.Items.Add("TODAY", "Today");
            ultraComboPresetDates.Items.Add("YESTERDAY", "Yesterday");
            ultraComboPresetDates.Items.Add("THIS_WEEK", "This Week");
            ultraComboPresetDates.Items.Add("LAST_WEEK", "Last Week");
            ultraComboPresetDates.Items.Add("THIS_MONTH", "This Month");
            ultraComboPresetDates.Items.Add("LAST_MONTH", "Last Month");
            ultraComboPresetDates.Items.Add("THIS_QUARTER", "This Quarter");
            ultraComboPresetDates.Items.Add("LAST_QUARTER", "Last Quarter");
            ultraComboPresetDates.Items.Add("CURRENT_FY", "Current Financial Year");
            ultraComboPresetDates.Items.Add("LAST_YEAR", "Last Year");
            ultraComboPresetDates.Items.Add("ALL", "ALL (Full History)");
            ultraComboPresetDates.Items.Add("DATE_RANGE", "Date by Range");
        }

        private void UltraComboPresetDates_ValueChanged(object sender, EventArgs e)
        {
            string presetKey = ultraComboPresetDates.Value?.ToString() ?? ultraComboPresetDates.Text;
            ApplyDatePreset(presetKey);
        }

        private void ApplyDatePreset(string presetKey)
        {
            DateTime now = DateTime.Today;

            switch (presetKey)
            {
                case "TODAY":
                    ultraDateTimeFrom.DateTime = now;
                    ultraDateTimeTo.DateTime = now;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "YESTERDAY":
                    ultraDateTimeFrom.DateTime = now.AddDays(-1);
                    ultraDateTimeTo.DateTime = now.AddDays(-1);
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "THIS_WEEK":
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime startOfWeek = now.AddDays(-1 * diff).Date;
                    ultraDateTimeFrom.DateTime = startOfWeek;
                    ultraDateTimeTo.DateTime = now;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "LAST_WEEK":
                    int diffLast = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    DateTime startOfLastWeek = now.AddDays(-1 * diffLast - 7).Date;
                    DateTime endOfLastWeek = startOfLastWeek.AddDays(6).Date;
                    ultraDateTimeFrom.DateTime = startOfLastWeek;
                    ultraDateTimeTo.DateTime = endOfLastWeek;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "THIS_MONTH":
                    ultraDateTimeFrom.DateTime = new DateTime(now.Year, now.Month, 1);
                    ultraDateTimeTo.DateTime = now;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "LAST_MONTH":
                    DateTime lastMonth = now.AddMonths(-1);
                    ultraDateTimeFrom.DateTime = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    ultraDateTimeTo.DateTime = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "THIS_QUARTER":
                    int quarter = (now.Month - 1) / 3 + 1;
                    ultraDateTimeFrom.DateTime = new DateTime(now.Year, (quarter - 1) * 3 + 1, 1);
                    ultraDateTimeTo.DateTime = now;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "LAST_QUARTER":
                    int currentQ = (now.Month - 1) / 3 + 1;
                    int lastQ = currentQ == 1 ? 4 : currentQ - 1;
                    int lastQYear = currentQ == 1 ? now.Year - 1 : now.Year;
                    DateTime lqStart = new DateTime(lastQYear, (lastQ - 1) * 3 + 1, 1);
                    DateTime lqEnd = lqStart.AddMonths(3).AddDays(-1);
                    ultraDateTimeFrom.DateTime = lqStart;
                    ultraDateTimeTo.DateTime = lqEnd;
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "CURRENT_FY":
                    int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
                    ultraDateTimeFrom.DateTime = new DateTime(startYear, 4, 1);
                    ultraDateTimeTo.DateTime = new DateTime(startYear + 1, 3, 31);
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "LAST_YEAR":
                    ultraDateTimeFrom.DateTime = new DateTime(now.Year - 1, 1, 1);
                    ultraDateTimeTo.DateTime = new DateTime(now.Year - 1, 12, 31);
                    ultraDateTimeFrom.Enabled = false;
                    ultraDateTimeTo.Enabled = false;
                    break;

                case "ALL":
                    ultraDateTimeFrom.DateTime = new DateTime(1990, 1, 1);
                    ultraDateTimeTo.DateTime = now;
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

        public void RibbonClear()
        {
            txtSearch.Text = string.Empty;
            ultraNumericBillNo.Value = null;
            ultraComboPresetDates.Value = "THIS_MONTH";
            ApplyDatePreset("THIS_MONTH");
            _rawData.Clear();
            _filteredData.Clear();
            ultraGridProfit.DataSource = null;
            UpdateSummaryFooter();
        }

        public void Clear() => RibbonClear();

        private void ResetFilters()
        {
            txtSearch.Text = string.Empty;
            ultraNumericBillNo.Value = null;
            ultraComboPresetDates.Value = "THIS_MONTH";
            ApplyDatePreset("THIS_MONTH");
            LoadData();
        }
        #endregion

        #region Theme & UI Styling
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            // Filter Panel
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelControls.Height = 78;

            // Labels
            lblSearch.Appearance.ForeColor = ControlTextColor;
            lblSearchStatus.Appearance.ForeColor = ControlTextColor;
            lblSearchStatus.Appearance.FontData.SizeInPoints = 8.5f;
            lblPreset.Appearance.ForeColor = ControlTextColor;
            lblFromDate.Appearance.ForeColor = ControlTextColor;
            lblToDate.Appearance.ForeColor = ControlTextColor;
            lblBillNo.Appearance.ForeColor = ControlTextColor;

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
            ultraPanelGridFooter.Height = 36;

            // Grid
            ultraGridProfit.Dock = DockStyle.Fill;

            // Z-Order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            ultraPanelGridFooter.SendToBack();
            ultraGridProfit.BringToFront();

            StyleButtons();
            SetupGridAppearance();
            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            btnGenerate.Text = "View Grid";
            btnPreviewGrid.Text = "Preview Grid";
            btnPrint.Text = "Preview Report";
            btnExportCsv.Text = "Export Grid";
            btnColumnChooser.Text = "Column Chooser";
            btnClearFilters.Text = "Reset Filters";
            btnToggleSelection.Text = "Hide Selection";

            StyleClassicButton(btnGenerate);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnExportCsv);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnToggleSelection);
        }

        private static void StyleClassicButton(UltraButton button)
        {
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            button.Appearance.BackColor = Color.FromArgb(240, 248, 255);
            button.Appearance.BackColor2 = Color.FromArgb(196, 222, 245);
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = BorderBlue;
            button.Appearance.ForeColor = ButtonTextBlue;

            button.HotTrackAppearance.BackColor = Color.FromArgb(223, 240, 255);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(173, 209, 240);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = Color.FromArgb(43, 107, 168);
            button.HotTrackAppearance.ForeColor = Color.FromArgb(10, 35, 75);

            button.PressedAppearance.BackColor = Color.FromArgb(180, 213, 242);
            button.PressedAppearance.BackColor2 = Color.FromArgb(150, 192, 228);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(28, 80, 135);
        }

        private void SetupGridAppearance()
        {
            var layout = ultraGridProfit.DisplayLayout;
            layout.GroupByBox.Hidden = true;
            layout.CaptionVisible = DefaultableBoolean.False;

            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButtonFixedSize;

            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;

            var headerApp = layout.Override.HeaderAppearance;
            headerApp.BackColor = GridHeaderBlue;
            headerApp.BackColor2 = GridHeaderBlueDark;
            headerApp.BackGradientStyle = GradientStyle.Vertical;
            headerApp.ForeColor = Color.White;
            headerApp.FontData.Bold = DefaultableBoolean.True;
            headerApp.FontData.SizeInPoints = 9f;
            headerApp.TextHAlign = HAlign.Center;
            headerApp.ThemedElementAlpha = Alpha.Transparent;

            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.Override.RowAppearance.BorderColor = Color.FromArgb(197, 217, 241);

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.MinRowHeight = 24;
            layout.Override.DefaultRowHeight = 24;
        }
        #endregion

        #region Dynamic Column Summary Footer
        private void InitializeSummaryFooter()
        {
            columnAggregations["BillAmount"] = "Sum";
            columnAggregations["Profit"] = "Sum";
            columnAggregations["ProfitMarginPercent"] = "Average";
            columnAggregations["SubTotal"] = "Sum";
            columnAggregations["TaxAmt"] = "Sum";

            if (lblCount == null)
            {
                lblCount = new Label
                {
                    Name = "lblCount",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Location = new Point(8, 6),
                    Size = new Size(130, 24),
                    Text = "Bills: 0"
                };
                ultraPanelGridFooter.ClientArea.Controls.Add(lblCount);
            }

            // Panel-wide right-click menu
            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            ultraPanelGridFooter.ClientArea.ContextMenuStrip = panelMenu;

            // Wire alignment events
            ultraPanelGridFooter.Paint += (s, e) => AlignSummaryLabels();
            ultraPanelGridFooter.Resize += (s, e) => AlignSummaryLabels();
            ultraGridProfit.AfterColPosChanged += (s, e) => { UpdateSummaryFooter(); AlignSummaryLabels(); };
            ultraGridProfit.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGridProfit.AfterRowFilterChanged += (s, e) => { UpdateFooterValues(); AlignSummaryLabels(); };
            ultraGridProfit.SizeChanged += (s, e) => AlignSummaryLabels();
            ultraGridProfit.Paint += (s, e) => AlignSummaryLabels();
        }

        private void UpdateSummaryFooter()
        {
            if (ultraPanelGridFooter == null || ultraGridProfit.DisplayLayout.Bands.Count == 0) return;

            ultraPanelGridFooter.ClientArea.SuspendLayout();

            // Clear old column labels (preserve lblCount)
            foreach (var lbl in summaryLabels.Values)
            {
                ultraPanelGridFooter.ClientArea.Controls.Remove(lbl);
                lbl.Dispose();
            }
            summaryLabels.Clear();

            var band = ultraGridProfit.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (col.Hidden || col.IsChaptered) continue;
                if (!numericSummaryColumns.Contains(col.Key)) continue;
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;

                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    Padding = new Padding(0, 0, 4, 0),
                    Height = ultraPanelGridFooter.Height - 6,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };

                summaryLabels[col.Key] = lbl;
                ultraPanelGridFooter.ClientArea.Controls.Add(lbl);
            }

            ultraPanelGridFooter.ClientArea.ResumeLayout();
            UpdateFooterValues();
            AlignSummaryLabels();
        }

        private void UpdateFooterValues()
        {
            if (_filteredData == null || _filteredData.Count == 0)
            {
                lblCount.Text = "Bills: 0";
                foreach (var lbl in summaryLabels.Values) lbl.Text = "";
                lblSearchStatus.Text = _rawData.Count > 0 ? $"0 of {_rawData.Count} bills match search" : "No records found";
                return;
            }

            int count = _filteredData.Count;
            lblCount.Text = $"Bills: {count:N0}";

            if (_rawData.Count != _filteredData.Count)
                lblSearchStatus.Text = $"Showing {_filteredData.Count:N0} of {_rawData.Count:N0} bills";
            else
                lblSearchStatus.Text = $"{_rawData.Count:N0} bills loaded";

            foreach (var kvp in summaryLabels)
            {
                string colKey = kvp.Key;
                Label lbl = kvp.Value;
                string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "Sum";

                if (string.Equals(agg, "None", StringComparison.OrdinalIgnoreCase))
                {
                    lbl.Text = "";
                    continue;
                }

                List<double> values = GetColumnValues(colKey);
                bool isPercent = string.Equals(colKey, "ProfitMarginPercent", StringComparison.OrdinalIgnoreCase);

                if (values.Count == 0)
                {
                    lbl.Text = isPercent ? "0.00 %" : "₹ 0.00";
                    continue;
                }

                string text = "";
                switch (agg)
                {
                    case "Sum":
                        double sum = values.Sum();
                        text = isPercent ? (sum.ToString("N2") + " %") : ("₹ " + sum.ToString("N2"));
                        break;
                    case "Average":
                        double avg = values.Average();
                        text = isPercent ? ("Avg: " + avg.ToString("N2") + " %") : ("Avg: ₹ " + avg.ToString("N2"));
                        break;
                    case "Min":
                        double min = values.Min();
                        text = isPercent ? ("Min: " + min.ToString("N2") + " %") : ("Min: ₹ " + min.ToString("N2"));
                        break;
                    case "Max":
                        double max = values.Max();
                        text = isPercent ? ("Max: " + max.ToString("N2") + " %") : ("Max: ₹ " + max.ToString("N2"));
                        break;
                    case "Count":
                        text = values.Count.ToString("N0");
                        break;
                }
                lbl.Text = text;
            }
        }

        private List<double> GetColumnValues(string colKey)
        {
            switch (colKey)
            {
                case "BillAmount": return _filteredData.Select(x => x.BillAmount).ToList();
                case "Profit": return _filteredData.Select(x => x.Profit).ToList();
                case "ProfitMarginPercent": return _filteredData.Select(x => x.ProfitMarginPercent).ToList();
                case "SubTotal": return _filteredData.Select(x => x.SubTotal).ToList();
                case "TaxAmt": return _filteredData.Select(x => x.TaxAmt).ToList();
                default: return new List<double>();
            }
        }

        private void AlignSummaryLabels()
        {
            if (ultraPanelGridFooter == null || ultraGridProfit == null ||
                ultraGridProfit.DisplayLayout.Bands.Count == 0) return;

            var band = ultraGridProfit.DisplayLayout.Bands[0];
            int panelScreenX = ultraPanelGridFooter.PointToScreen(Point.Empty).X;

            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;

                if (col.Hidden || col.IsChaptered)
                {
                    lbl.Visible = false;
                    continue;
                }

                var headerUI = col.Header?.GetUIElement() ??
                               ultraGridProfit.DisplayLayout.UIElement?.GetDescendant(typeof(HeaderUIElement), col) as HeaderUIElement;

                if (headerUI != null)
                {
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - panelScreenX;
                    int colWidth = headerUI.Rect.Width;

                    if (colLeft + colWidth > 0 && colLeft < ultraPanelGridFooter.Width)
                    {
                        lbl.Left = colLeft;
                        lbl.Width = Math.Max(0, colWidth - 2);
                        lbl.Top = 4;
                        lbl.Height = ultraPanelGridFooter.Height - 8;
                        lbl.Visible = true;
                        lbl.BringToFront();
                    }
                    else
                    {
                        lbl.Visible = false;
                    }
                }
                else
                {
                    lbl.Visible = false;
                }
            }
        }

        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type) { Tag = type };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }

            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = columnAggregations.ContainsKey(columnKey) &&
                                  columnAggregations[columnKey] == (string)item.Tag;
                }
            };
            return menu;
        }

        private void OnPanelSummaryTypeSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string type)
            {
                foreach (var key in numericSummaryColumns)
                {
                    columnAggregations[key] = type;
                }
                UpdateSummaryFooter();
            }
        }
        #endregion

        #region Column Chooser & Drag Drop
        private void InitializeGridContextMenuAndDragDrop()
        {
            ultraGridProfit.AllowDrop = true;
            ultraGridProfit.MouseDown += GridProfit_MouseDown;
            ultraGridProfit.MouseMove += GridProfit_MouseMove;
            ultraGridProfit.MouseUp += GridProfit_MouseUp;
            ultraGridProfit.DragOver += GridProfit_DragOver;
            ultraGridProfit.DragDrop += GridProfit_DragDrop;
        }

        private bool IsCustomizableColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            return true;
        }

        private void GridProfit_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGridProfit.DisplayLayout.Bands.Count == 0) return;

            UIElement elem = ultraGridProfit.DisplayLayout.UIElement.ElementFromPoint(e.Location);
            HeaderUIElement headerElem = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip menu = new ContextMenuStrip();

                if (headerElem?.Header?.Column != null)
                {
                    UltraGridColumn col = headerElem.Header.Column;
                    if (IsCustomizableColumn(col))
                    {
                        ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide '{col.Header.Caption}'");
                        hideItem.Click += (s, ev) => HideGridColumn(col);
                        menu.Items.Add(hideItem);
                    }
                }

                ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field/Column Chooser...");
                chooserItem.Click += (s, ev) => ShowColumnChooserDialog();
                menu.Items.Add(chooserItem);

                menu.Show(ultraGridProfit, e.Location);
                return;
            }

            if (e.Button == MouseButtons.Left && headerElem?.Header?.Column != null)
            {
                UltraGridColumn col = headerElem.Header.Column;
                if (IsCustomizableColumn(col))
                {
                    headerDragStartPoint = e.Location;
                    columnToHideByDrag = col;
                    isDraggingHeaderColumn = false;
                }
            }
        }

        private void GridProfit_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToHideByDrag != null)
            {
                int dx = Math.Abs(e.X - headerDragStartPoint.X);
                int dy = Math.Abs(e.Y - headerDragStartPoint.Y);

                if (!isDraggingHeaderColumn && (dx > 8 || dy > 8))
                {
                    isDraggingHeaderColumn = true;
                }

                if (isDraggingHeaderColumn)
                {
                    bool isDraggingDown = e.Y - headerDragStartPoint.Y > 30;
                    bool isOutside = !ultraGridProfit.ClientRectangle.Contains(e.Location);

                    if (isDraggingDown || isOutside)
                    {
                        ultraGridProfit.Cursor = Cursors.No;
                        gridToolTip.SetToolTip(ultraGridProfit, $"Drag down to hide '{columnToHideByDrag.Header.Caption}'");
                    }
                    else
                    {
                        ultraGridProfit.Cursor = Cursors.Default;
                        gridToolTip.SetToolTip(ultraGridProfit, string.Empty);
                    }
                }
            }
        }

        private void GridProfit_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderColumn && columnToHideByDrag != null)
            {
                bool isDraggingDown = e.Y - headerDragStartPoint.Y > 40;
                bool isOutside = !ultraGridProfit.ClientRectangle.Contains(e.Location);

                if (isDraggingDown || isOutside)
                {
                    HideGridColumn(columnToHideByDrag);
                }
            }

            columnToHideByDrag = null;
            isDraggingHeaderColumn = false;
            ultraGridProfit.Cursor = Cursors.Default;
            gridToolTip.SetToolTip(ultraGridProfit, string.Empty);
        }

        private void GridProfit_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)) ||
                e.Data.GetDataPresent(typeof(UltraGridColumn)) ||
                e.Data.GetDataPresent(typeof(string)) ||
                e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void GridProfit_DragDrop(object sender, DragEventArgs e)
        {
            string columnKey = null;

            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                columnKey = item?.ColumnKey;
            }
            else if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                UltraGridColumn ugc = (UltraGridColumn)e.Data.GetData(typeof(UltraGridColumn));
                columnKey = ugc?.Key;
            }
            else if (e.Data.GetDataPresent(typeof(string)))
            {
                columnKey = (string)e.Data.GetData(typeof(string));
            }
            else if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                columnKey = (string)e.Data.GetData(DataFormats.StringFormat);
            }

            if (!string.IsNullOrEmpty(columnKey) && ultraGridProfit.DisplayLayout.Bands.Count > 0)
            {
                var band = ultraGridProfit.DisplayLayout.Bands[0];
                if (band.Columns.Exists(columnKey))
                {
                    var col = band.Columns[columnKey];
                    col.Hidden = false;
                    if (savedColumnWidths.ContainsKey(col.Key))
                    {
                        col.Width = savedColumnWidths[col.Key];
                    }

                    // Check if dropped directly over a specific column header to insert at that position
                    Point pt = ultraGridProfit.PointToClient(new Point(e.X, e.Y));
                    UIElement elem = ultraGridProfit.DisplayLayout.UIElement.ElementFromPoint(pt);
                    HeaderUIElement headerElem = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                    if (headerElem?.Header?.Column != null && headerElem.Header.Column.Key != col.Key)
                    {
                        col.Header.VisiblePosition = headerElem.Header.Column.Header.VisiblePosition;
                    }

                    UpdateSummaryFooter();
                    RefreshColumnChooserList();
                }
            }
        }

        private void HideGridColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden) return;
            savedColumnWidths[column.Key] = column.Width;
            column.Hidden = true;
            UpdateSummaryFooter();
            RefreshColumnChooserList();
        }

        private void ShowColumnChooserDialog()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Show(this);
                PositionColumnChooserAtBottomRight();
                RefreshColumnChooserList();
                return;
            }

            CreateColumnChooserForm();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
            RefreshColumnChooserList();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(240, 340),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(245, 247, 250),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            Label lblInfo = new Label
            {
                Text = "Drag columns into grid or double-click to restore:",
                Dock = DockStyle.Top,
                Height = 32,
                Padding = new Padding(8, 6, 8, 2),
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 80, 95),
                BackColor = Color.FromArgb(240, 244, 248)
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 247, 250),
                ItemHeight = 32,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(lblInfo);

            LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            SizeChanged += (s, e) => PositionColumnChooserAtBottomRight();
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
            {
                UltraGridColumn col = (UltraGridColumn)e.Data.GetData(typeof(UltraGridColumn));
                if (col != null && !col.Hidden && IsCustomizableColumn(col))
                {
                    HideGridColumn(col);
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count) return;

            ColumnItem item = columnChooserListBox.Items[e.Index] as ColumnItem;
            if (item == null) return;

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(245, 247, 250)), e.Bounds);

            Rectangle cardRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
            using (GraphicsPath path = GetRoundedRectangle(cardRect, 4))
            {
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color top = isSelected ? Color.FromArgb(220, 235, 252) : Color.FromArgb(240, 244, 248);
                Color bot = isSelected ? Color.FromArgb(190, 215, 245) : Color.FromArgb(225, 232, 240);

                using (LinearGradientBrush lgb = new LinearGradientBrush(cardRect, top, bot, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(lgb, path);
                }
                using (Pen p = new Pen(isSelected ? Color.FromArgb(100, 150, 220) : Color.FromArgb(180, 195, 210)))
                {
                    e.Graphics.DrawPath(p, path);
                }
            }

            Rectangle textRect = new Rectangle(cardRect.X + 8, cardRect.Y, cardRect.Width - 16, cardRect.Height);
            TextRenderer.DrawText(e.Graphics, item.DisplayText, columnChooserListBox.Font, textRect, Color.FromArgb(20, 50, 90), TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int index = columnChooserListBox.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches && index < columnChooserListBox.Items.Count)
                {
                    ColumnItem item = (ColumnItem)columnChooserListBox.Items[index];
                    if (item != null)
                    {
                        columnChooserListBox.SelectedIndex = index;
                        columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnItem item)
            {
                if (ultraGridProfit.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGridProfit.DisplayLayout.Bands[0];
                    if (band.Columns.Exists(item.ColumnKey))
                    {
                        var col = band.Columns[item.ColumnKey];
                        col.Hidden = false;
                        if (savedColumnWidths.ContainsKey(col.Key)) col.Width = savedColumnWidths[col.Key];
                        UpdateSummaryFooter();
                        RefreshColumnChooserList();
                    }
                }
            }
        }

        private void RefreshColumnChooserList()
        {
            if (columnChooserListBox == null || ultraGridProfit.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.Items.Clear();
            var band = ultraGridProfit.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (col.Hidden && IsCustomizableColumn(col))
                {
                    columnChooserListBox.Items.Add(new ColumnItem(col.Key, col.Header.Caption));
                }
            }
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm == null || !columnChooserForm.Visible) return;
            Rectangle rect = RectangleToScreen(ClientRectangle);
            int x = rect.Right - columnChooserForm.Width - 15;
            int y = rect.Bottom - columnChooserForm.Height - 45;
            columnChooserForm.Location = new Point(Math.Max(rect.Left + 10, x), Math.Max(rect.Top + 10, y));
        }

        private static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
        #endregion

        #region Data Loading & Search
        private void LoadData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                DateTime fromDate = Convert.ToDateTime(ultraDateTimeFrom.Value);
                DateTime toDate = Convert.ToDateTime(ultraDateTimeTo.Value);
                int billNo = Convert.ToInt32(ultraNumericBillNo.Value ?? 0);

                var rawList = _reportRepository.GetSalesProfitReport(billNo, fromDate, toDate);

                _rawData = rawList != null
                    ? rawList.Select(r => new SalesProfitViewModel
                    {
                        BillNo = r.BillNo,
                        BillDate = r.BillDate,
                        BillAmount = r.BillAmount,
                        Profit = r.Profit,
                        ProfitMarginPercent = r.BillAmount > 0 ? (r.Profit / r.BillAmount) * 100 : 0,
                        PayMode = r.PayMode ?? string.Empty,
                        CashMode = r.CashMode ?? string.Empty,
                        SubTotal = r.SubTotal,
                        TaxAmt = r.TaxAmt
                    }).ToList()
                    : new List<SalesProfitViewModel>();

                ApplyClientFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales profit data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void TxtSearch_ValueChanged(object sender, EventArgs e)
        {
            ApplyClientFilter();
        }

        private void ApplyClientFilter()
        {
            string query = txtSearch.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(query))
            {
                _filteredData = new List<SalesProfitViewModel>(_rawData);
            }
            else
            {
                _filteredData = _rawData.Where(x =>
                    x.BillNo.ToString().Contains(query) ||
                    (!string.IsNullOrEmpty(x.PayMode) && x.PayMode.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(x.CashMode) && x.CashMode.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    x.BillAmount.ToString("N2").Contains(query) ||
                    x.Profit.ToString("N2").Contains(query) ||
                    x.ProfitMarginPercent.ToString("N2").Contains(query) ||
                    x.BillDate.ToString("dd-MM-yyyy").Contains(query)
                ).ToList();
            }

            ultraGridProfit.DataSource = null;
            ultraGridProfit.DataSource = _filteredData;

            UpdateSummaryFooter();
        }
        #endregion

        #region UltraGrid Configuration
        private void UltraGridProfit_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            UltraGridBand band = e.Layout.Bands[0];

            // Primary Sales Profit Columns (Visible by default)
            ConfigureColumn(band, "BillNo", "Bill No", 90, HAlign.Center);
            ConfigureDateColumn(band, "BillDate", "Bill Date", 110, HAlign.Center);
            ConfigureMoneyColumn(band, "BillAmount", "Sales Amount (₹)", 150, Color.FromArgb(20, 50, 90));
            ConfigureMoneyColumn(band, "Profit", "Profit Amount (₹)", 150, Color.FromArgb(27, 94, 32));
            ConfigurePercentColumn(band, "ProfitMarginPercent", "Profit Margin %", 130);
            ConfigureColumn(band, "PayMode", "Pay Mode", 120, HAlign.Left);
            ConfigureColumn(band, "CashMode", "Cash Mode", 120, HAlign.Left);

            // Optional Tax breakdown columns (Hidden by default, available in Column Chooser)
            if (band.Columns.Exists("SubTotal"))
            {
                ConfigureMoneyColumn(band, "SubTotal", "Amount (Without Tax)", 140, Color.FromArgb(20, 50, 90));
                band.Columns["SubTotal"].Hidden = true;
            }

            if (band.Columns.Exists("TaxAmt"))
            {
                ConfigureMoneyColumn(band, "TaxAmt", "Tax Amount (GST ₹)", 130, Color.FromArgb(123, 31, 162));
                band.Columns["TaxAmt"].Hidden = true;
            }

            // Ensure all columns can be dragged / customized via Column Chooser
            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void UltraGridProfit_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Band.Columns.Exists("Profit"))
            {
                if (double.TryParse(e.Row.GetCellValue("Profit")?.ToString(), out double profitVal))
                {
                    e.Row.Cells["Profit"].Appearance.ForeColor = profitVal >= 0 ? ProfitGreenColor : LossRedColor;
                    e.Row.Cells["Profit"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }

            if (e.Row.Band.Columns.Exists("ProfitMarginPercent"))
            {
                if (double.TryParse(e.Row.GetCellValue("ProfitMarginPercent")?.ToString(), out double marginVal))
                {
                    e.Row.Cells["ProfitMarginPercent"].Appearance.ForeColor = marginVal >= 0 ? ProfitGreenColor : LossRedColor;
                    e.Row.Cells["ProfitMarginPercent"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        private void UltraGridProfit_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;
        }

        private static void ConfigureColumn(UltraGridBand band, string key, string caption, int width, HAlign align)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
        }

        private static void ConfigureDateColumn(UltraGridBand band, string key, string caption, int width, HAlign align)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.Format = "dd-MM-yyyy";
            col.CellAppearance.TextHAlign = align;
        }

        private static void ConfigureMoneyColumn(UltraGridBand band, string key, string caption, int width, Color foreColor)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.Format = "#,##0.00";
            col.CellAppearance.TextHAlign = HAlign.Right;
            col.CellAppearance.FontData.Bold = DefaultableBoolean.True;
            col.CellAppearance.ForeColor = foreColor;
        }

        private static void ConfigurePercentColumn(UltraGridBand band, string key, string caption, int width)
        {
            if (!band.Columns.Exists(key)) return;
            var col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.Format = "0.00' %'";
            col.CellAppearance.TextHAlign = HAlign.Right;
            col.CellAppearance.FontData.Bold = DefaultableBoolean.True;
        }
        #endregion

        #region Actions & Exports
        private void PreviewGrid()
        {
            if (_filteredData == null || _filteredData.Count == 0)
            {
                MessageBox.Show("No data available to preview.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ultraGridProfit.PrintPreview();
        }

        private void PrintReport()
        {
            if (_filteredData == null || _filteredData.Count == 0)
            {
                MessageBox.Show("No data available to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ultraGridProfit.Print();
        }

        private void ExportCsv()
        {
            if (_filteredData == null || _filteredData.Count == 0)
            {
                MessageBox.Show("No data available to export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = $"SalesProfitReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Bill No,Bill Date,Sales Amount,Profit Amount,Profit Margin %,Pay Mode,Cash Mode");

                        foreach (var item in _filteredData)
                        {
                            sb.AppendLine(string.Format("{0},{1:dd-MM-yyyy},{2:F2},{3:F2},{4:F2}%,\"{5}\",\"{6}\"",
                                item.BillNo,
                                item.BillDate,
                                item.BillAmount,
                                item.Profit,
                                item.ProfitMarginPercent,
                                item.PayMode.Replace("\"", "\"\""),
                                item.CashMode.Replace("\"", "\"\"")));
                        }

                        sb.AppendLine();
                        sb.AppendLine($"Total Bills,{_filteredData.Count}");
                        sb.AppendLine($"Total Sales Amount,{_filteredData.Sum(x => x.BillAmount):F2}");
                        sb.AppendLine($"Total Profit Amount,{_filteredData.Sum(x => x.Profit):F2}");
                        double totalSales = _filteredData.Sum(x => x.BillAmount);
                        double totalProfit = _filteredData.Sum(x => x.Profit);
                        double margin = totalSales > 0 ? (totalProfit / totalSales) * 100 : 0;
                        sb.AppendLine($"Overall Profit Margin %,{margin:F2}%");

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Grid data exported successfully!", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting data: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ToggleSelectionPanel()
        {
            _isSelectionHidden = !_isSelectionHidden;
            ultraPanelControls.Visible = !_isSelectionHidden;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = _isSelectionHidden ? "Show Selection" : "Hide Selection";
        }
        #endregion

        #region Keyboard Shortcuts
        private void FrmSalesProfit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadData();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                PrintReport();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                ExportCsv();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.Handled = true;
            }
        }
        #endregion
    }
}
