using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class FrmSalesHoldReport : Form
    {
        private SalesHoldReportRepository _repository;
        private Dropdowns _dropdowns;
        private readonly List<SalesHoldSummaryItem> _summaryItems;
        private readonly List<SalesHoldDetailItem> _detailItems;
        private readonly List<ComboItem> _userOptions;
        private readonly List<ComboItem> _customerOptions;
        private readonly Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalQty",
            "Qty",
            "SubTotal",
            "DiscountAmt",
            "DiscountAmount",
            "TaxAmt",
            "NetAmount",
            "TotalAmount",
            "Amount",
            "ItemCount"
        };

        private readonly HashSet<string> internalGridColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BranchId",
            "CompanyId",
            "CounterId",
            "ItemId",
            "LedgerId",
            "UserId"
        };

        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private bool summaryFooterInitialized;
        private readonly System.Windows.Forms.ToolTip columnChooserToolTip = new System.Windows.Forms.ToolTip();

        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        private sealed class ColumnItem
        {
            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public string ColumnKey { get; private set; }
            public string DisplayText { get; private set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        public FrmSalesHoldReport()
        {
            _summaryItems = new List<SalesHoldSummaryItem>();
            _detailItems = new List<SalesHoldDetailItem>();
            _userOptions = new List<ComboItem>();
            _customerOptions = new List<ComboItem>();

            InitializeComponent();
            Load += FrmSalesHoldReport_Load;
            FormClosed += FrmSalesHoldReport_FormClosed;
        }

        private void FrmSalesHoldReport_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
            {
                return;
            }

            InitializeRuntimeAppearance();
            LoadLookupData();
            ResetFilters(false);
            LoadData();
        }

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Sales Hold Report - Print Preview");
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void BtnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooserDialog();
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
        }

        private void UltraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            UpdateDateControlsVisibility();
            LoadData();
        }

        private void UpdateDateControlsVisibility()
        {
            bool isRange = string.Equals(ultraComboDateMode.Value?.ToString(), "RANGE", StringComparison.OrdinalIgnoreCase);
            lblFromDate.Visible = isRange;
            dtFromDate.Visible = isRange;
            lblToDate.Visible = isRange;
            dtToDate.Visible = isRange;

            if (isRange)
            {
                lblViewMode.Location = new Point(645, 14);
                cmbViewMode.Location = new Point(725, 11);
            }
            else
            {
                lblViewMode.Location = new Point(232, 14);
                cmbViewMode.Location = new Point(312, 11);
            }
        }

        private void CmbViewMode_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void FrmSalesHoldReport_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private void InitializeRuntimeAppearance()
        {
            ConfigureButton(btnViewGrid, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(94, 116, 202), Color.FromArgb(121, 141, 222));
            ConfigureButton(btnExportExcel, Color.FromArgb(46, 125, 50), Color.FromArgb(76, 175, 80));
            ConfigureButton(btnColumnChooser, Color.FromArgb(90, 110, 160), Color.FromArgb(115, 135, 185));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            ConfigureGridAppearance(gridHold);
            InitializeSummaryFooterPanel();
            InitializeColumnChooserBehavior();
        }

        private void ConfigureButton(Infragistics.Win.Misc.UltraButton button, Color startColor, Color endColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = startColor;
            button.Appearance.BackColor2 = endColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.BorderColor = startColor;
            button.HotTrackAppearance.BackColor = endColor;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            targetGrid.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 28;
            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            targetGrid.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
        }

        private void InitializeSummaryFooterPanel()
        {
            if (summaryFooterInitialized || gridFooterPanel == null || gridHold == null)
            {
                return;
            }

            gridHold.AfterColPosChanged += (s, e) => UpdateSummaryFooterPositions();
            gridHold.AfterColRegionScroll += (s, e) => UpdateSummaryFooterPositions();
            gridHold.AfterRowRegionScroll += (s, e) => UpdateSummaryFooterPositions();
            gridHold.Resize += (s, e) => UpdateSummaryFooterPositions();
            gridHold.Paint += (s, e) => UpdateSummaryFooterPositions();

            gridHold.MouseDown += GridHold_MouseDown;
            gridHold.MouseMove += GridHold_MouseMove;
            gridHold.MouseUp += GridHold_MouseUp;

            gridFooterPanel.ClientArea.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
                {
                    UltraGridColumn col = GetColumnAtFooterPoint(e.Location);
                    if (col != null && summaryDefaultNumericColumns.Contains(col.Key))
                    {
                        string caption = string.IsNullOrEmpty(col.Header.Caption) ? col.Key : col.Header.Caption;
                        CreateFooterLabelMenu(col.Key, caption).Show(gridFooterPanel.ClientArea, e.Location);
                    }
                }
            };

            summaryFooterInitialized = true;
        }

        private void ResetFilters(bool reload = true)
        {
            ultraComboDateMode.Value = "ALL";
            dtFromDate.DateTime = DateTime.Today;
            dtToDate.DateTime = DateTime.Today;
            UpdateDateControlsVisibility();

            cmbViewMode.Value = "Summary";
            cmbCustomer.Value = "";
            cmbUser.Value = "";
            cmbStatus.Value = "ALL";

            if (reload)
            {
                LoadData();
            }
        }

        private void LoadLookupData()
        {
            _repository = new SalesHoldReportRepository();
            _dropdowns = new Dropdowns();

            // Date Mode
            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL");
            ultraComboDateMode.Items.Add("RANGE", "Date Range");
            ultraComboDateMode.Value = "ALL";
            dtFromDate.DateTime = DateTime.Today;
            dtToDate.DateTime = DateTime.Today;
            UpdateDateControlsVisibility();

            // View Mode
            cmbViewMode.Items.Clear();
            cmbViewMode.Items.Add("Summary", "Summary (Bills)");
            cmbViewMode.Items.Add("Detail", "Detailed (Items)");
            cmbViewMode.Value = "Summary";

            // Status
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("ALL", "All Statuses");
            cmbStatus.Items.Add("Hold", "Active Hold");
            cmbStatus.Items.Add("Complete", "Billed / Complete");
            cmbStatus.Value = "ALL";

            // Customers
            try
            {
                var custGrid = _dropdowns.CustomerDDl();
                _customerOptions.Clear();
                _customerOptions.Add(new ComboItem { Text = "-- All Customers --", Value = "" });
                cmbCustomer.Items.Clear();
                cmbCustomer.Items.Add("", "-- All Customers --");
                if (custGrid != null && custGrid.List != null)
                {
                    foreach (var c in custGrid.List)
                    {
                        if (!string.IsNullOrEmpty(c.LedgerName))
                        {
                            _customerOptions.Add(new ComboItem { Text = c.LedgerName, Value = c.LedgerID.ToString() });
                            cmbCustomer.Items.Add(c.LedgerID.ToString(), c.LedgerName);
                        }
                    }
                }
                cmbCustomer.Value = "";
            }
            catch { }

            // Users / Sales Persons / Cashiers
            try
            {
                var usersGrid = _dropdowns.getUsersDDl();
                _userOptions.Clear();
                _userOptions.Add(new ComboItem { Text = "-- All Users --", Value = "" });
                cmbUser.Items.Clear();
                cmbUser.Items.Add("", "-- All Users --");
                if (usersGrid != null && usersGrid.List != null)
                {
                    foreach (var u in usersGrid.List)
                    {
                        if (!string.IsNullOrEmpty(u.UserName))
                        {
                            _userOptions.Add(new ComboItem { Text = u.UserName, Value = u.UserID.ToString() });
                            cmbUser.Items.Add(u.UserID.ToString(), u.UserName);
                        }
                    }
                }
                cmbUser.Value = "";
            }
            catch { }
        }

        private void LoadData()
        {
            if (IsDesignTime())
            {
                return;
            }

            try
            {
                if (string.Equals(ultraComboDateMode.Value?.ToString(), "RANGE", StringComparison.OrdinalIgnoreCase))
                {
                    if (dtFromDate.DateTime.Date > dtToDate.DateTime.Date)
                    {
                        MessageBox.Show("From Date cannot be greater than To Date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                Cursor = Cursors.WaitCursor;

                SalesHoldFilter filter = new SalesHoldFilter
                {
                    DateMode = ultraComboDateMode.Value?.ToString() ?? "ALL",
                    FromDate = dtFromDate.DateTime,
                    ToDate = dtToDate.DateTime,
                    ViewMode = cmbViewMode.Value?.ToString() ?? "Summary",
                    CustomerName = cmbCustomer.Text != "-- All Customers --" && !string.IsNullOrWhiteSpace(cmbCustomer.Text) ? cmbCustomer.Text.Trim() : null,
                    UserId = int.TryParse(cmbUser.Value?.ToString(), out int uId) && uId > 0 ? (int?)uId : null,
                    Status = cmbStatus.Value?.ToString() ?? "ALL"
                };

                if (string.Equals(filter.ViewMode, "Detail", StringComparison.OrdinalIgnoreCase))
                {
                    _detailItems.Clear();
                    var list = _repository.GetSalesHoldDetail(filter);
                    if (list != null)
                    {
                        _detailItems.AddRange(list);
                    }
                    BindDetailGrid();
                }
                else
                {
                    _summaryItems.Clear();
                    var list = _repository.GetSalesHoldSummary(filter);
                    if (list != null)
                    {
                        _summaryItems.AddRange(list);
                    }
                    BindSummaryGrid();
                }

                UpdateSummaryFooterCalculations();
                UpdateSummaryFooterPositions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching Sales Hold report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BindSummaryGrid()
        {
            gridHold.DataSource = null;
            gridHold.DataSource = _summaryItems.ToList();
            FormatSummaryGridColumns();
        }

        private void BindDetailGrid()
        {
            gridHold.DataSource = null;
            gridHold.DataSource = _detailItems.ToList();
            FormatDetailGridColumns();
        }

        private void FormatSummaryGridColumns()
        {
            if (gridHold.DisplayLayout.Bands.Count == 0) return;
            var cols = gridHold.DisplayLayout.Bands[0].Columns;

            HideInternalColumns(cols);

            ConfigureColumn(cols, "BillNo", "Bill No", 90, HAlign.Right);
            ConfigureColumn(cols, "BillDate", "Hold Date", 110, HAlign.Center, "dd/MM/yyyy HH:mm");
            ConfigureColumn(cols, "CustomerName", "Customer", 180, HAlign.Left);
            ConfigureColumn(cols, "UserName", "User / Cashier", 120, HAlign.Left);
            ConfigureColumn(cols, "PaymodeName", "Pay Mode", 100, HAlign.Left);
            ConfigureColumn(cols, "ItemCount", "Items", 70, HAlign.Right, "N0");
            ConfigureColumn(cols, "TotalQty", "Total Qty", 90, HAlign.Right, "N2");
            ConfigureColumn(cols, "SubTotal", "Gross Amount", 120, HAlign.Right, "N2");
            ConfigureColumn(cols, "DiscountAmt", "Discount", 100, HAlign.Right, "N2");
            ConfigureColumn(cols, "TaxAmt", "Tax Amount", 100, HAlign.Right, "N2");
            ConfigureColumn(cols, "NetAmount", "Net Amount", 130, HAlign.Right, "N2");
            ConfigureColumn(cols, "Status", "Status", 90, HAlign.Center);

            RestoreSavedWidths(cols);
        }

        private void FormatDetailGridColumns()
        {
            if (gridHold.DisplayLayout.Bands.Count == 0) return;
            var cols = gridHold.DisplayLayout.Bands[0].Columns;

            HideInternalColumns(cols);

            ConfigureColumn(cols, "BillNo", "Bill No", 80, HAlign.Right);
            ConfigureColumn(cols, "BillDate", "Hold Date", 105, HAlign.Center, "dd/MM/yyyy");
            ConfigureColumn(cols, "CustomerName", "Customer", 140, HAlign.Left);
            ConfigureColumn(cols, "UserName", "Cashier", 100, HAlign.Left);
            ConfigureColumn(cols, "SlNo", "Sl No", 50, HAlign.Right);
            ConfigureColumn(cols, "Barcode", "Barcode", 110, HAlign.Left);
            ConfigureColumn(cols, "ItemName", "Item Description", 220, HAlign.Left);
            ConfigureColumn(cols, "GroupName", "Group", 110, HAlign.Left);
            ConfigureColumn(cols, "CategoryName", "Category", 110, HAlign.Left);
            ConfigureColumn(cols, "BrandName", "Brand", 100, HAlign.Left);
            ConfigureColumn(cols, "Unit", "Unit", 60, HAlign.Center);
            ConfigureColumn(cols, "Qty", "Qty", 80, HAlign.Right, "N2");
            ConfigureColumn(cols, "UnitPrice", "Price", 90, HAlign.Right, "N2");
            ConfigureColumn(cols, "Amount", "Gross Amt", 100, HAlign.Right, "N2");
            ConfigureColumn(cols, "DiscountAmount", "Disc Amt", 90, HAlign.Right, "N2");
            ConfigureColumn(cols, "TaxAmt", "Tax Amt", 90, HAlign.Right, "N2");
            ConfigureColumn(cols, "TotalAmount", "Net Amount", 115, HAlign.Right, "N2");
            ConfigureColumn(cols, "Status", "Status", 80, HAlign.Center);

            RestoreSavedWidths(cols);
        }

        private void HideInternalColumns(ColumnsCollection cols)
        {
            foreach (UltraGridColumn col in cols)
            {
                if (internalGridColumns.Contains(col.Key))
                {
                    col.Hidden = true;
                }
            }
        }

        private void ConfigureColumn(ColumnsCollection cols, string key, string header, int width, HAlign align, string format = null)
        {
            if (cols.Exists(key))
            {
                var col = cols[key];
                col.Header.Caption = header;
                col.Width = width;
                col.CellAppearance.TextHAlign = align;
                col.Header.Appearance.TextHAlign = align == HAlign.Right ? HAlign.Right : (align == HAlign.Center ? HAlign.Center : HAlign.Left);
                if (!string.IsNullOrEmpty(format))
                {
                    col.Format = format;
                }
            }
        }

        private void RestoreSavedWidths(ColumnsCollection cols)
        {
            foreach (UltraGridColumn col in cols)
            {
                if (savedColumnWidths.TryGetValue(col.Key, out int w))
                {
                    col.Width = w;
                }
            }
        }

        private void UpdateSummaryFooterCalculations()
        {
            int rowCount = gridHold.Rows.Count;
            lblCount.Text = $"Count : {rowCount}";

            EnsureSummaryFooterLabels();

            if (gridHold.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            foreach (UltraGridColumn col in gridHold.DisplayLayout.Bands[0].Columns)
            {
                if (!summaryDefaultNumericColumns.Contains(col.Key))
                {
                    continue;
                }

                if (!summaryLabels.TryGetValue(col.Key, out Label label))
                {
                    continue;
                }

                string aggType = columnAggregations.TryGetValue(col.Key, out string savedAgg) ? savedAgg : "Sum";
                if (aggType == "None" || rowCount == 0)
                {
                    label.Text = "";
                    continue;
                }

                decimal val = CalculateAggregate(col.Key, aggType);
                string format = col.Key.IndexOf("Qty", StringComparison.OrdinalIgnoreCase) >= 0 || col.Key == "ItemCount" ? "N2" : "N2";
                label.Text = aggType == "Sum" ? val.ToString(format) : $"{aggType}: {val.ToString(format)}";
            }
        }

        private decimal CalculateAggregate(string columnKey, string aggType)
        {
            List<decimal> values = new List<decimal>();

            foreach (var row in gridHold.Rows)
            {
                if (row.IsFilteredOut) continue;
                object cellVal = row.Cells[columnKey].Value;
                if (cellVal != null && cellVal != DBNull.Value && decimal.TryParse(cellVal.ToString(), out decimal d))
                {
                    values.Add(d);
                }
            }

            if (values.Count == 0) return 0;

            switch (aggType)
            {
                case "Sum":
                    return values.Sum();
                case "Min":
                    return values.Min();
                case "Max":
                    return values.Max();
                case "Average":
                    return values.Average();
                case "Count":
                    return values.Count;
                default:
                    return 0;
            }
        }

        private void EnsureSummaryFooterLabels()
        {
            if (gridHold.DisplayLayout.Bands.Count == 0) return;

            foreach (UltraGridColumn col in gridHold.DisplayLayout.Bands[0].Columns)
            {
                if (!summaryDefaultNumericColumns.Contains(col.Key)) continue;

                if (!summaryLabels.ContainsKey(col.Key))
                {
                    string colKey = col.Key;
                    string caption = string.IsNullOrEmpty(col.Header.Caption) ? col.Key : col.Header.Caption;
                    Label lbl = new Label
                    {
                        AutoSize = false,
                        Height = 22,
                        TextAlign = ContentAlignment.MiddleRight,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(17, 52, 102),
                        BackColor = Color.Transparent,
                        Cursor = Cursors.Hand
                    };
                    lbl.MouseDown += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                        {
                            CreateFooterLabelMenu(colKey, caption).Show(lbl, new Point(0, lbl.Height));
                        }
                    };
                    columnChooserToolTip.SetToolTip(lbl, $"Click to change {caption} summary (Sum, Min, Max, Average, Count, None)");
                    gridFooterPanel.ClientArea.Controls.Add(lbl);
                    summaryLabels[col.Key] = lbl;
                }
            }
        }

        private ContextMenuStrip CreateFooterLabelMenu(string columnKey, string columnCaption)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            string curAgg = columnAggregations.TryGetValue(columnKey, out string sVal) ? sVal : "Sum";

            var titleItem = new ToolStripMenuItem($"Summary Function: {columnCaption}")
            {
                Enabled = false,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            menu.Items.Add(titleItem);
            menu.Items.Add(new ToolStripSeparator());

            foreach (string t in summaryTypes)
            {
                string summaryType = t;
                var itm = new ToolStripMenuItem(summaryType, null, (s, e) =>
                {
                    columnAggregations[columnKey] = summaryType;
                    UpdateSummaryFooterCalculations();
                    UpdateSummaryFooterPositions();
                })
                {
                    Checked = string.Equals(curAgg, summaryType, StringComparison.OrdinalIgnoreCase)
                };
                menu.Items.Add(itm);
            }

            return menu;
        }

        private UltraGridColumn GetColumnAtFooterPoint(Point footerPoint)
        {
            if (gridHold == null || gridHold.DisplayLayout.Bands.Count == 0) return null;

            foreach (UltraGridColumn col in gridHold.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden) continue;

                try
                {
                    var colUI = gridHold.DisplayLayout.Bands[0].Layout.UIElement?.GetDescendant(typeof(HeaderUIElement), col) as HeaderUIElement;
                    if (colUI != null)
                    {
                        Point scrPt = gridHold.PointToScreen(new Point(colUI.Rect.Left, 0));
                        Point panelPt = gridFooterPanel.PointToClient(scrPt);
                        Rectangle colBounds = new Rectangle(panelPt.X, 0, colUI.Rect.Width, gridFooterPanel.Height);
                        if (colBounds.Contains(footerPoint))
                        {
                            return col;
                        }
                    }
                }
                catch { }
            }
            return null;
        }

        private void UpdateSummaryFooterPositions()
        {
            if (gridHold.DisplayLayout.Bands.Count == 0) return;

            foreach (UltraGridColumn col in gridHold.DisplayLayout.Bands[0].Columns)
            {
                if (!summaryLabels.TryGetValue(col.Key, out Label label)) continue;

                if (col.Hidden)
                {
                    label.Visible = false;
                    continue;
                }

                try
                {
                    var colUI = gridHold.DisplayLayout.Bands[0].Layout.UIElement?.GetDescendant(typeof(HeaderUIElement), col) as HeaderUIElement;
                    if (colUI != null)
                    {
                        Point scrPt = gridHold.PointToScreen(new Point(colUI.Rect.Left, 0));
                        Point panelPt = gridFooterPanel.PointToClient(scrPt);

                        label.Left = panelPt.X;
                        label.Width = colUI.Rect.Width;
                        label.Top = (gridFooterPanel.Height - label.Height) / 2;
                        label.Visible = label.Right > 0 && label.Left < gridFooterPanel.Width;
                    }
                    else
                    {
                        label.Visible = false;
                    }
                }
                catch
                {
                    label.Visible = false;
                }
            }
        }

        private void GridHold_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var element = gridHold.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                var headerUI = element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerUI?.Header?.Column != null)
                {
                    ShowHeaderContextMenu(headerUI.Header.Column, e.Location);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                var element = gridHold.DisplayLayout.UIElement.ElementFromPoint(e.Location);
                var headerUI = element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerUI?.Header?.Column != null)
                {
                    headerDragStartPoint = e.Location;
                    columnToHideByDrag = headerUI.Header.Column;
                    isDraggingHeaderColumn = false;
                }
            }
        }

        private void GridHold_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnToHideByDrag != null)
            {
                int dx = Math.Abs(e.X - headerDragStartPoint.X);
                int dy = e.Y - headerDragStartPoint.Y;

                if (!isDraggingHeaderColumn)
                {
                    if (Math.Abs(dx) > SystemInformation.DragSize.Width || Math.Abs(dy) > SystemInformation.DragSize.Height)
                    {
                        isDraggingHeaderColumn = true;
                    }
                }

                if (isDraggingHeaderColumn)
                {
                    if (dy > 40)
                    {
                        Cursor = Cursors.No;
                        columnChooserToolTip.SetToolTip(gridHold, $"Release to hide '{columnToHideByDrag.Header.Caption ?? columnToHideByDrag.Key}'");
                    }
                    else if (columnChooserForm != null && columnChooserForm.Visible)
                    {
                        Point clientPt = columnChooserForm.PointToClient(gridHold.PointToScreen(e.Location));
                        if (columnChooserForm.ClientRectangle.Contains(clientPt))
                        {
                            Cursor = Cursors.Hand;
                            columnChooserToolTip.SetToolTip(gridHold, $"Drop in Column Chooser to hide '{columnToHideByDrag.Header.Caption ?? columnToHideByDrag.Key}'");
                        }
                        else
                        {
                            Cursor = Cursors.Default;
                            columnChooserToolTip.SetToolTip(gridHold, string.Empty);
                        }
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                        columnChooserToolTip.SetToolTip(gridHold, string.Empty);
                    }
                }
            }
        }

        private void GridHold_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderColumn && columnToHideByDrag != null)
            {
                int dy = e.Y - headerDragStartPoint.Y;
                bool shouldHide = false;

                if (dy > 40)
                {
                    shouldHide = true;
                }
                else if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    Point clientPt = columnChooserForm.PointToClient(gridHold.PointToScreen(e.Location));
                    if (columnChooserForm.ClientRectangle.Contains(clientPt))
                    {
                        shouldHide = true;
                    }
                }

                if (shouldHide)
                {
                    columnToHideByDrag.Hidden = true;
                    PopulateColumnChooserItems();
                    UpdateSummaryFooterPositions();
                }
            }

            columnToHideByDrag = null;
            isDraggingHeaderColumn = false;
            Cursor = Cursors.Default;
            columnChooserToolTip.SetToolTip(gridHold, string.Empty);
        }

        private void ShowHeaderContextMenu(UltraGridColumn column, Point loc)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            var hideItem = new ToolStripMenuItem($"Hide Column '{column.Header.Caption ?? column.Key}'", null, (s, e) =>
            {
                column.Hidden = true;
                PopulateColumnChooserItems();
                UpdateSummaryFooterPositions();
            });
            menu.Items.Add(hideItem);

            var chooserItem = new ToolStripMenuItem("Field/Column Chooser...", null, (s, e) => ShowColumnChooserDialog());
            menu.Items.Add(chooserItem);

            var showAllItem = new ToolStripMenuItem("Show All Columns", null, (s, e) =>
            {
                if (gridHold.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn c in gridHold.DisplayLayout.Bands[0].Columns)
                    {
                        if (!internalGridColumns.Contains(c.Key))
                        {
                            c.Hidden = false;
                        }
                    }
                    PopulateColumnChooserItems();
                    UpdateSummaryFooterPositions();
                }
            });
            menu.Items.Add(showAllItem);

            if (summaryDefaultNumericColumns.Contains(column.Key))
            {
                menu.Items.Add(new ToolStripSeparator());
                string curAgg = columnAggregations.TryGetValue(column.Key, out string sVal) ? sVal : "Sum";
                ToolStripMenuItem summaryMenu = new ToolStripMenuItem("Summary Function");

                foreach (string t in summaryTypes)
                {
                    string summaryType = t;
                    var itm = new ToolStripMenuItem(summaryType, null, (s, e) =>
                    {
                        columnAggregations[column.Key] = summaryType;
                        UpdateSummaryFooterCalculations();
                        UpdateSummaryFooterPositions();
                    })
                    {
                        Checked = string.Equals(curAgg, summaryType, StringComparison.OrdinalIgnoreCase)
                    };
                    summaryMenu.DropDownItems.Add(itm);
                }
                menu.Items.Add(summaryMenu);
            }

            menu.Show(gridHold, loc);
        }

        private void InitializeColumnChooserBehavior()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                Size = new Size(210, 270),
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                ShowInTaskbar = false
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Font = new Font("Segoe UI", 9f),
                ItemHeight = 24
            };

            ContextMenuStrip chooserMenu = new ContextMenuStrip();
            var showColItem = new ToolStripMenuItem("Show Column", null, (s, e) =>
            {
                if (columnChooserListBox.SelectedItem is ColumnItem item)
                {
                    if (gridHold.DisplayLayout.Bands.Count > 0 && gridHold.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        gridHold.DisplayLayout.Bands[0].Columns[item.ColumnKey].Hidden = false;
                        PopulateColumnChooserItems();
                        UpdateSummaryFooterPositions();
                    }
                }
            });
            chooserMenu.Items.Add(showColItem);

            var showAllColsItem = new ToolStripMenuItem("Show All Columns", null, (s, e) =>
            {
                if (gridHold.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn c in gridHold.DisplayLayout.Bands[0].Columns)
                    {
                        if (!internalGridColumns.Contains(c.Key))
                        {
                            c.Hidden = false;
                        }
                    }
                    PopulateColumnChooserItems();
                    UpdateSummaryFooterPositions();
                }
            });
            chooserMenu.Items.Add(showAllColsItem);
            columnChooserListBox.ContextMenuStrip = chooserMenu;

            columnChooserListBox.DoubleClick += (s, e) =>
            {
                if (columnChooserListBox.SelectedItem is ColumnItem item)
                {
                    if (gridHold.DisplayLayout.Bands.Count > 0 && gridHold.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                    {
                        gridHold.DisplayLayout.Bands[0].Columns[item.ColumnKey].Hidden = false;
                        PopulateColumnChooserItems();
                        UpdateSummaryFooterPositions();
                    }
                }
            };

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            LocationChanged += (s, e) => PositionColumnChooser();
            SizeChanged += (s, e) => PositionColumnChooser();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                Point formLoc = this.PointToScreen(Point.Empty);
                columnChooserForm.Location = new Point(
                    formLoc.X + this.Width - columnChooserForm.Width - 30,
                    formLoc.Y + this.Height - columnChooserForm.Height - 60);
                columnChooserForm.BringToFront();
            }
        }

        private void ShowColumnChooserDialog()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                InitializeColumnChooserBehavior();
            }

            PopulateColumnChooserItems();
            PositionColumnChooser();
            columnChooserForm.Show(this);
            columnChooserForm.BringToFront();
        }

        private void PopulateColumnChooserItems()
        {
            if (columnChooserListBox == null || gridHold.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.Items.Clear();
            foreach (UltraGridColumn col in gridHold.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden && !internalGridColumns.Contains(col.Key))
                {
                    columnChooserListBox.Items.Add(new ColumnItem(col.Key, string.IsNullOrEmpty(col.Header.Caption) ? col.Key : col.Header.Caption));
                }
            }
        }

        private void ExportToExcel()
        {
            if (gridHold.Rows.Count == 0)
            {
                MessageBox.Show("No records available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV (Comma delimited) (*.csv)|*.csv|All Files (*.*)|*.*";
                    sfd.FileName = $"SalesHoldReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        var visibleCols = gridHold.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>()
                            .Where(c => !c.Hidden && !internalGridColumns.Contains(c.Key))
                            .OrderBy(c => c.Header.VisiblePosition)
                            .ToList();

                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.AppendLine(string.Join(",", visibleCols.Select(c => EscapeCsv(string.IsNullOrEmpty(c.Header.Caption) ? c.Key : c.Header.Caption))));

                        foreach (var row in gridHold.Rows)
                        {
                            if (row.IsFilteredOut) continue;
                            var rowValues = visibleCols.Select(c =>
                            {
                                object val = row.Cells[c.Key].Value;
                                string strVal = val != null && val != DBNull.Value ? val.ToString() : "";
                                return EscapeCsv(strVal);
                            });
                            sb.AppendLine(string.Join(",", rowValues));
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show("Report exported successfully!", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n") || text.Contains("\r"))
                return $"\"{text.Replace("\"", "\"\"")}\"";
            return text;
        }

        private void ShowGridPreview(string title)
        {
            if (gridHold.Rows.Count == 0)
            {
                MessageBox.Show("No data available to preview.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                PrintPreviewDialog previewDialog = new PrintPreviewDialog();
                Infragistics.Win.UltraWinGrid.UltraGridPrintDocument printDoc = new Infragistics.Win.UltraWinGrid.UltraGridPrintDocument
                {
                    Grid = gridHold,
                    Header = { TextLeft = title, TextRight = $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}" },
                    Footer = { TextCenter = "Page [Page #] of [Pages #]" }
                };

                previewDialog.Document = printDoc;
                previewDialog.Text = title;
                previewDialog.WindowState = FormWindowState.Maximized;
                previewDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating preview: " + ex.Message, "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;
        }
    }
}
