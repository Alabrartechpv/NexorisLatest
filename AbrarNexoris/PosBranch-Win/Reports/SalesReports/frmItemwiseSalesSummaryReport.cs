using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmItemwiseSalesSummaryReport : Form
    {
        // ════════════════════════════════════════════════════════════
        //  Fields
        // ════════════════════════════════════════════════════════════
        private ItemwiseSalesSummaryRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _isChangingDates = false;
        private bool _accentPanelsCreated = false;

        /// <summary>All rows fetched from the DB (before any in-memory filter)</summary>
        private List<ItemwiseSalesSummaryItem> _allRows = new List<ItemwiseSalesSummaryItem>();

        // ════════════════════════════════════════════════════════════
        //  Constructor
        // ════════════════════════════════════════════════════════════
        public frmItemwiseSalesSummaryReport()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 9F);
            InitializeBackgroundWorker();
            InitializeForm();
        }

        // ════════════════════════════════════════════════════════════
        //  Initialization
        // ════════════════════════════════════════════════════════════
        private void InitializeBackgroundWorker()
        {
            _searchWorker = new BackgroundWorker();
            _searchWorker.WorkerSupportsCancellation = true;
            _searchWorker.DoWork += SearchWorker_DoWork;
            _searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        }

        private void InitializeForm()
        {
            try
            {
                _reportRepo = new ItemwiseSalesSummaryRepo();
                _dropdownRepo = new Dropdowns();

                // Setup date presets
                comboPeriod.Items.Clear();
                comboPeriod.Items.Add("Today", "Today");
                comboPeriod.Items.Add("This Week", "This Week");
                comboPeriod.Items.Add("This Month", "This Month");
                comboPeriod.Items.Add("Last Month", "Last Month");
                comboPeriod.Items.Add("This Quarter", "This Quarter");
                comboPeriod.Items.Add("This Year", "This Year");
                comboPeriod.Items.Add("Custom", "Custom Range");
                comboPeriod.Value = "This Month";

                // Setup stock filters
                comboStockFilter.Items.Clear();
                comboStockFilter.Items.Add("All Items", "All Items");
                comboStockFilter.Items.Add("High Profit (>30%)", "High Profit (>30%)");
                comboStockFilter.Items.Add("Low Profit (<10%)", "Low Profit (<10%)");
                comboStockFilter.Items.Add("Top Sold (>100 Qty)", "Top Sold (>100 Qty)");
                comboStockFilter.Value = "All Items";

                // Load initial dropdown list choices
                LoadGroups();
                LoadCategories();

                // Configure grid properties
                StyleGrid();

                // Style buttons
                StyleButtons();

                // Apply dynamic layout coordinates and colors on card panels
                LayoutSummaryCards();

                // Wire event triggers
                btnSearch.Click          += BtnSearch_Click;
                btnReset.Click           += BtnReset_Click;
                btnExport.Click          += BtnExport_Click;
                btnPrint.Click           += BtnPrint_Click;
                btnClose.Click           += BtnClose_Click;
                comboPeriod.ValueChanged += ComboPeriod_ValueChanged;
                dtFrom.ValueChanged      += DtDate_ValueChanged;
                dtTo.ValueChanged        += DtDate_ValueChanged;
                txtSearch.TextChanged    += TxtSearch_TextChanged;

                // Handle keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += FrmItemwiseSalesSummaryReport_KeyDown;

                this.FormClosing += (s, ev) =>
                {
                    if (_searchWorker != null && _searchWorker.IsBusy)
                        _searchWorker.CancelAsync();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization Error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGroups()
        {
            try
            {
                var gps = _dropdownRepo.getGroupDDl();
                if (gps?.List != null)
                {
                    comboGroup.DataSource    = gps.List.ToList();
                    comboGroup.ValueMember   = "Id";
                    comboGroup.DisplayMember = "GroupName";
                }
            }
            catch { }
        }

        private void LoadCategories()
        {
            try
            {
                var cats = _dropdownRepo.getCategoryDDl(null);
                if (cats?.List != null)
                {
                    comboCategory.DataSource    = cats.List.ToList();
                    comboCategory.ValueMember   = "Id";
                    comboCategory.DisplayMember = "CategoryName";
                }
            }
            catch { }
        }

        //  Grid styling
        // ════════════════════════════════════════════════════════════
        private void StyleGrid()
        {
            gridReport.UseOsThemes = DefaultableBoolean.False;
            gridReport.DisplayLayout.Override.AllowRowFiltering   = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.FilterUIType        = FilterUIType.FilterRow;
            gridReport.DisplayLayout.Override.HeaderClickAction   = HeaderClickAction.SortMulti;
            gridReport.DisplayLayout.AutoFitStyle                  = AutoFitStyle.ResizeAllColumns;
            gridReport.DisplayLayout.CaptionVisible                = DefaultableBoolean.False;
            gridReport.DisplayLayout.GroupByBox.Hidden             = true;

            // Header
            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor  = Color.FromArgb(30, 40, 55);
            gridReport.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(42, 55, 72);
            gridReport.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            gridReport.DisplayLayout.Override.HeaderAppearance.ForeColor  = Color.White;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.Bold      = DefaultableBoolean.True;
            gridReport.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            gridReport.DisplayLayout.Override.HeaderStyle          = HeaderStyle.Standard;

            // Rows
            gridReport.DisplayLayout.Override.RowAppearance.BackColor          = Color.White;
            gridReport.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(248, 250, 253);
            gridReport.DisplayLayout.Override.MinRowHeight                     = 24;
            gridReport.DisplayLayout.Override.DefaultRowHeight                 = 24;

            // Selection
            gridReport.DisplayLayout.Override.SelectedRowAppearance.BackColor  = Color.FromArgb(66, 165, 245);
            gridReport.DisplayLayout.Override.SelectedRowAppearance.ForeColor  = Color.White;
            gridReport.DisplayLayout.Override.ActiveRowAppearance.BackColor    = Color.FromArgb(227, 242, 253);
            gridReport.DisplayLayout.Override.ActiveRowAppearance.ForeColor    = Color.FromArgb(33, 33, 33);
            gridReport.DisplayLayout.Override.ActiveRowAppearance.BorderColor  = Color.FromArgb(66, 165, 245);
            gridReport.DisplayLayout.Override.CellClickAction                  = CellClickAction.RowSelect;

            // Explicit filter row styling for dark-themed OS
            gridReport.DisplayLayout.Override.FilterRowAppearance.BackColor   = Color.FromArgb(255, 255, 230);
            gridReport.DisplayLayout.Override.FilterRowAppearance.ForeColor   = Color.FromArgb(33, 33, 33);
            gridReport.DisplayLayout.Override.FilterRowPromptAppearance.ForeColor = Color.FromArgb(140, 140, 140);

            gridReport.InitializeLayout += GridReport_InitializeLayout;
            gridReport.InitializeRow    += GridReport_InitializeRow;
        }

        private void GridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            var band = e.Layout.Bands[0];

            // Define column order and captions
            var columns = new[]
            {
                ("Barcode",           "Barcode",            80,  HAlign.Left),
                ("ItemName",          "Product Name",      200,  HAlign.Left),
                ("GroupName",         "Group",             110,  HAlign.Left),
                ("CategoryName",      "Category",          110,  HAlign.Left),
                ("BaseUnitName",      "Unit",               60,  HAlign.Center),
                ("TotalQtySold",      "Qty Sold",           80,  HAlign.Right),
                ("AvgUnitPrice",      "Avg Price",          90,  HAlign.Right),
                ("TotalSalesAmount",  "Sales Amount",      110,  HAlign.Right),
                ("TotalCostValue",    "Cost Value",        110,  HAlign.Right),
                ("TotalMarginProfit", "Profit Margin",     110,  HAlign.Right),
                ("MarginPercent",     "Margin %",           85,  HAlign.Right),
            };

            int pos = 0;
            foreach (var (key, caption, width, align) in columns)
            {
                if (!band.Columns.Exists(key)) continue;
                var col = band.Columns[key];
                col.Header.Caption = caption;
                col.Width          = width;
                col.CellAppearance.TextHAlign = align;
                col.Header.VisiblePosition    = pos++;

                // Number formatting
                if (key == "TotalQtySold" || key == "AvgUnitPrice" || key == "MarginPercent")
                    col.Format = "N2";

                // Currency formatting for values
                if (key == "TotalSalesAmount" || key == "TotalCostValue" || key == "TotalMarginProfit")
                {
                    col.Format = "N2";
                    if (key == "TotalMarginProfit")
                        col.CellAppearance.ForeColor = Color.FromArgb(0, 150, 60);
                }
            }

            // Hide internal columns
            if (band.Columns.Exists("ItemId")) band.Columns["ItemId"].Hidden = true;
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row.Cells.Exists("MarginPercent"))
                {
                    decimal margin = Convert.ToDecimal(e.Row.Cells["MarginPercent"].Value ?? 0);
                    if (margin > 30)
                    {
                        // High Profit
                        e.Row.Appearance.BackColor = Color.FromArgb(240, 253, 244); // soft green
                        e.Row.Appearance.ForeColor = Color.FromArgb(21, 128, 61);   // dark green
                    }
                    else if (margin < 10 && margin > 0)
                    {
                        // Low Profit
                        e.Row.Appearance.BackColor = Color.FromArgb(254, 226, 226); // soft red
                        e.Row.Appearance.ForeColor = Color.FromArgb(153, 27, 27);   // dark red
                    }
                    else
                    {
                        e.Row.Appearance.Reset();
                    }
                }
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════
        //  Button styling
        // ════════════════════════════════════════════════════════════
        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(66, 165, 245));
            StyleButton(btnReset,  Color.FromArgb(245, 124, 0),  Color.FromArgb(255, 152, 0),  Color.FromArgb(255, 167, 38));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107),  Color.FromArgb(0, 150, 136),  Color.FromArgb(38, 166, 154));
            StyleButton(btnPrint,  Color.FromArgb(81, 45, 168),  Color.FromArgb(103, 58, 183), Color.FromArgb(126, 87, 194));
            StyleButton(btnClose,  Color.FromArgb(198, 40, 40),  Color.FromArgb(244, 67, 54),  Color.FromArgb(229, 115, 115));
        }

        private void StyleButton(Infragistics.Win.Misc.UltraButton btn, Color c1, Color c2, Color hover)
        {
            btn.UseAppStyling  = false;
            btn.UseOsThemes    = DefaultableBoolean.False;
            btn.Appearance.BackColor            = c1;
            btn.Appearance.BackColor2           = c2;
            btn.Appearance.BackGradientStyle    = GradientStyle.Vertical;
            btn.Appearance.ForeColor            = Color.White;
            btn.Appearance.FontData.Bold        = DefaultableBoolean.True;
            btn.Appearance.FontData.SizeInPoints = 9;
            btn.HotTrackAppearance.BackColor    = hover;
            btn.HotTrackAppearance.ForeColor    = Color.White;
        }

        // ════════════════════════════════════════════════════════════
        //  Summary card layout
        // ════════════════════════════════════════════════════════════
        private void LayoutSummaryCards()
        {
            // Set styles dynamically to prevent visual studio designer load bugs
            SetupCardControls(cardItems, lblItemsCaption, lblItemsValue, "UNIQUE PRODUCTS SOLD", "pnlCardItems");
            SetupCardControls(cardQty, lblQtyCaption, lblQtyValue, "TOTAL QUANTITY SOLD", "pnlCardQty");
            SetupCardControls(cardCostVal, lblCostCaption, lblCostValue, "TOTAL VALUE @ COST", "pnlCardCostVal");
            SetupCardControls(cardRetailVal, lblRetailCaption, lblRetailValue, "TOTAL SALES VALUE", "pnlCardRetailVal");
            SetupCardControls(cardProfit, lblProfitCaption, lblProfitValue, "TOTAL MARGIN PROFIT", "pnlCardProfit");

            var cards = new[]
            {
                (cardItems,     lblItemsValue,     Color.FromArgb(25, 118, 210)),
                (cardQty,       lblQtyValue,       Color.FromArgb(0, 137, 123)),
                (cardCostVal,   lblCostValue,      Color.FromArgb(56, 142, 60)),
                (cardRetailVal, lblRetailValue,    Color.FromArgb(123, 31, 162)),
                (cardProfit,    lblProfitValue,    Color.FromArgb(211, 47, 47)),
            };

            int x = 15, y = 6, w = 238, h = 62, gap = 10;
            foreach (var (card, val, valColor) in cards)
            {
                card.Location = new Point(x, y);
                card.Size = new Size(w, h);

                if (!_accentPanelsCreated)
                {
                    var accentLine = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 3,
                        BackColor = valColor
                    };
                    card.ClientArea.Controls.Add(accentLine);
                    accentLine.BringToFront();
                }

                val.Appearance.ForeColor = valColor;
                x += w + gap;
            }
            _accentPanelsCreated = true;
        }

        private void SetupCardControls(Infragistics.Win.Misc.UltraPanel card, Infragistics.Win.Misc.UltraLabel caption, Infragistics.Win.Misc.UltraLabel value, string captionText, string cardName)
        {
            card.Name = cardName;
            card.Size = new System.Drawing.Size(238, 62);
            card.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            card.UseAppStyling = false;
            card.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            card.Appearance.BackColor = System.Drawing.Color.White;
            card.Appearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);

            // Caption
            caption.Text = captionText;
            caption.Location = new Point(12, 8);
            caption.Size = new System.Drawing.Size(210, 15);
            caption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            caption.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            caption.UseAppStyling = false;
            caption.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // Value label
            value.Location = new Point(12, 26);
            value.Size = new System.Drawing.Size(210, 28);
            value.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            value.Text = "–";
            value.UseAppStyling = false;
            value.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }

        // ════════════════════════════════════════════════════════════
        //  Search & Filtering
        // ════════════════════════════════════════════════════════════
        private void BtnSearch_Click(object sender, EventArgs e) => RunSearch();

        private void RunSearch()
        {
            if (_searchWorker.IsBusy) return;

            if (dtFrom.Value == null || dtTo.Value == null)
            {
                MessageBox.Show("Please select a valid date range.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var filter = new ItemwiseSalesSummaryFilter
            {
                FromDate        = ((DateTime)dtFrom.Value).Date,
                ToDate          = ((DateTime)dtTo.Value).Date,
                CompanyId       = !string.IsNullOrEmpty(DataBase.CompanyId) ? int.Parse(DataBase.CompanyId) : 1,
                BranchId        = !string.IsNullOrEmpty(DataBase.BranchId)  ? (int.TryParse(DataBase.BranchId, out int bid) ? bid : 0) : 1,
                FinYearId       = !string.IsNullOrEmpty(DataBase.FinyearId) ? int.Parse(DataBase.FinyearId) : 1,
                GroupId         = GetComboIntValue(comboGroup),
                CategoryId      = GetComboIntValue(comboCategory),
                BarcodeContains = txtSearch.Text.Trim()
            };

            btnSearch.Text    = "Searching…";
            btnSearch.Enabled = false;
            this.Cursor       = Cursors.WaitCursor;
            SetStatus("Fetching transaction details from server…");

            _searchWorker.RunWorkerAsync(filter);
        }

        private int? GetComboIntValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo.Value == null) return null;
            if (int.TryParse(combo.Value.ToString(), out int val) && val > 0) return val;
            return null;
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var filter = (ItemwiseSalesSummaryFilter)e.Argument;
                e.Result = _reportRepo.GetItemwiseSalesSummary(filter);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;

            try
            {
                if (e.Cancelled) return;

                if (e.Error != null || e.Result is Exception)
                {
                    string msg = e.Error?.Message ?? ((Exception)e.Result).Message;
                    MessageBox.Show("Error loading data: " + msg, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("Error loading data.");
                    return;
                }

                var raw = e.Result as List<ItemwiseSalesSummaryItem>;
                if (raw == null || raw.Count == 0)
                {
                    _allRows = new List<ItemwiseSalesSummaryItem>();
                    gridReport.DataSource = null;
                    UpdateSummaryCards();
                    SetStatus("No transaction records found.");
                    MessageBox.Show("No records found.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _allRows = raw;
                ApplyFilters();
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                {
                    btnSearch.Text    = "🔍  Search";
                    btnSearch.Enabled = true;
                    this.Cursor       = Cursors.Default;
                }
            }
        }

        private void ApplyFilters()
        {
            string search = txtSearch.Text.Trim();
            string categoryFilter = comboStockFilter.Text;

            var filtered = _allRows.AsEnumerable();

            // In-Memory Search
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(r => 
                    r.ItemName.Contains(search) || 
                    r.Barcode.Contains(search)  ||
                    r.GroupName.Contains(search) ||
                    r.CategoryName.Contains(search)
                );
            }

            // Margin Filter Dropdown
            if (categoryFilter == "High Profit (>30%)")
                filtered = filtered.Where(r => r.MarginPercent > 30);
            else if (categoryFilter == "Low Profit (<10%)")
                filtered = filtered.Where(r => r.MarginPercent < 10);
            else if (categoryFilter == "Top Sold (>100 Qty)")
                filtered = filtered.Where(r => r.TotalQtySold > 100);

            var list = filtered.ToList();
            gridReport.DataSource = list;
            UpdateSummaryCards(list);
            SetStatus($"Found {list.Count} unique products sold.");
        }

        private void UpdateSummaryCards(List<ItemwiseSalesSummaryItem> list = null)
        {
            if (list == null || list.Count == 0)
            {
                lblItemsValue.Text  = "0";
                lblQtyValue.Text    = "0.00";
                lblCostValue.Text   = "₹0.00";
                lblRetailValue.Text = "₹0.00";
                lblProfitValue.Text = "₹0.00";
                return;
            }

            int uniqueItems = list.Count;
            decimal totalQty = list.Sum(r => r.TotalQtySold);
            decimal totalCostVal = list.Sum(r => r.TotalCostValue);
            decimal totalSalesVal = list.Sum(r => r.TotalSalesAmount);
            decimal totalProfitVal = list.Sum(r => r.TotalMarginProfit);

            lblItemsValue.Text  = uniqueItems.ToString();
            lblQtyValue.Text    = totalQty.ToString("N2");
            lblCostValue.Text   = "₹" + totalCostVal.ToString("N2");
            lblRetailValue.Text = "₹" + totalSalesVal.ToString("N2");
            lblProfitValue.Text = "₹" + totalProfitVal.ToString("N2");
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        // ════════════════════════════════════════════════════════════
        //  Action Events
        // ════════════════════════════════════════════════════════════
        private void BtnReset_Click(object sender, EventArgs e)
        {
            comboPeriod.Value      = "This Month";
            comboStockFilter.Value = "All Items";
            comboGroup.Value       = null;
            comboCategory.Value    = null;
            txtSearch.Text         = "";
            gridReport.DataSource  = null;
            _allRows.Clear();
            UpdateSummaryCards();
            SetStatus("Ready. Filters reset.");
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No records to export.", "Empty Grid", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter   = "CSV Files (*.csv)|*.csv";
                sfd.FileName = $"Itemwise_Sales_Profit_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportToCsv(sfd.FileName);
                        MessageBox.Show("Exported successfully!", "Export Complete", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Export failed: " + ex.Message, "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToCsv(string path)
        {
            var sb = new StringBuilder();
            var band = gridReport.DisplayLayout.Bands[0];

            // Header
            foreach (UltraGridColumn col in band.Columns)
                if (!col.Hidden) sb.Append(col.Header.Caption + ",");
            if (sb.Length > 0) sb.Length--;
            sb.AppendLine();

            // Data
            foreach (UltraGridRow row in gridReport.Rows)
            {
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden)
                    {
                        string v = row.Cells[col].Value?.ToString() ?? "";
                        if (v.Contains(",")) v = "\"" + v + "\"";
                        sb.Append(v + ",");
                    }
                }
                if (sb.Length > 0) sb.Length--;
                sb.AppendLine();
            }

            // Summary Row
            sb.AppendLine();
            int itemsCount = gridReport.Rows.Count;
            decimal totalQty = 0;
            decimal totalValueCost = 0;
            decimal totalValueRetail = 0;
            decimal totalProfit = 0;

            foreach (UltraGridRow row in gridReport.Rows)
            {
                totalQty         += Convert.ToDecimal(row.Cells["TotalQtySold"].Value ?? 0);
                totalValueCost   += Convert.ToDecimal(row.Cells["TotalCostValue"].Value ?? 0);
                totalValueRetail += Convert.ToDecimal(row.Cells["TotalSalesAmount"].Value ?? 0);
                totalProfit      += Convert.ToDecimal(row.Cells["TotalMarginProfit"].Value ?? 0);
            }

            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.Hidden)
                {
                    if (col.Key == "Barcode")
                        sb.Append("TOTALS,");
                    else if (col.Key == "ItemName")
                        sb.Append($"({itemsCount} Items),");
                    else if (col.Key == "TotalQtySold")
                        sb.Append($"{totalQty:F2},");
                    else if (col.Key == "TotalCostValue")
                        sb.Append($"{totalValueCost:F2},");
                    else if (col.Key == "TotalSalesAmount")
                        sb.Append($"{totalValueRetail:F2},");
                    else if (col.Key == "TotalMarginProfit")
                        sb.Append($"{totalProfit:F2},");
                    else if (col.Key == "MarginPercent")
                        sb.Append($"{(totalValueRetail > 0 ? (totalProfit / totalValueRetail * 100) : 0):F2}%,");
                    else
                        sb.Append(",");
                }
            }
            if (sb.Length > 0) sb.Length--;
            sb.AppendLine();

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Empty Report", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            gridReport.PrintPreview();
        }

        private void BtnClose_Click(object sender, EventArgs e) => this.Close();

        private void ComboPeriod_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangingDates) return;
            if (comboPeriod.Value == null) return;

            _isChangingDates = true;
            try
            {
                string val = comboPeriod.Value.ToString();
                DateTime now = DateTime.Now;
                switch (val)
                {
                    case "Today":         dtFrom.Value = now.Date;                              dtTo.Value = now.Date;                              break;
                    case "This Week":     dtFrom.Value = now.Date.AddDays(-(int)now.DayOfWeek); dtTo.Value = now.Date;                              break;
                    case "This Month":    dtFrom.Value = new DateTime(now.Year, now.Month, 1);  dtTo.Value = now.Date;                              break;
                    case "Last Month":    var lm = now.AddMonths(-1); dtFrom.Value = new DateTime(lm.Year, lm.Month, 1); dtTo.Value = new DateTime(now.Year, now.Month, 1).AddDays(-1); break;
                    case "This Quarter":  int q = (now.Month - 1) / 3; dtFrom.Value = new DateTime(now.Year, q * 3 + 1, 1); dtTo.Value = now.Date; break;
                    case "This Year":     dtFrom.Value = new DateTime(now.Year, 1, 1);           dtTo.Value = now.Date;                              break;
                }
            }
            finally
            {
                _isChangingDates = false;
            }

            RunSearch();
        }

        private void DtDate_ValueChanged(object sender, EventArgs e)
        {
            if (_isChangingDates) return;

            _isChangingDates = true;
            try
            {
                if (comboPeriod.Text != "Custom") comboPeriod.Text = "Custom";
            }
            finally
            {
                _isChangingDates = false;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_allRows.Count > 0) ApplyFilters();
        }

        private void FrmItemwiseSalesSummaryReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)                         { e.Handled = true; RunSearch(); }
            else if (e.Control && e.KeyCode == Keys.E)        { e.Handled = true; BtnExport_Click(sender, e); }
            else if (e.Control && e.KeyCode == Keys.P)        { e.Handled = true; BtnPrint_Click(sender, e); }
            else if (e.KeyCode == Keys.Escape)                { e.Handled = true; this.Close(); }
        }
    }
}
