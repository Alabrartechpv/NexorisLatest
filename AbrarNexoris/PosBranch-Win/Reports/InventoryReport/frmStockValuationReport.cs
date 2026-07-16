using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    /// <summary>
    /// Stock Valuation Report — shows inventory asset value calculated at Cost and Retail price per item.
    /// </summary>
    public partial class frmStockValuationReport : Form
    {
        // ════════════════════════════════════════════════════════════
        //  Fields
        // ════════════════════════════════════════════════════════════
        private StockReportAdvanceRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _isChangingDates = false;

        /// <summary>All rows fetched from the DB (before any in-memory filter)</summary>
        private List<StockValuationRow> _allRows = new List<StockValuationRow>();

        // ════════════════════════════════════════════════════════════
        //  Constructor
        // ════════════════════════════════════════════════════════════
        public frmStockValuationReport()
        {
            InitializeComponent();
            this.Font = new Font("Segoe UI", 9F);
            InitializeBackgroundWorker();
            InitializeForm();
        }

        // ════════════════════════════════════════════════════════════
        //  Initialisation helpers
        // ════════════════════════════════════════════════════════════
        private void InitializeBackgroundWorker()
        {
            _searchWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = false
            };
            _searchWorker.DoWork             += SearchWorker_DoWork;
            _searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        }

        private void InitializeForm()
        {
            try
            {
                _reportRepo   = new StockReportAdvanceRepo();
                _dropdownRepo = new Dropdowns();

                // Default dates - Current Financial Year
                DateTime now = DateTime.Now;
                dtFrom.Value = new DateTime(now.Year, 1, 1);
                dtTo.Value   = now;

                InitializePeriodCombo();
                InitializeStockFilterCombo();
                LoadGroups();
                LoadCategories();
                StyleGrid();
                StyleButtons();
                LayoutSummaryCards();

                // Keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown    += FrmStockValuationReport_KeyDown;

                // Events
                btnSearch.Click  += BtnSearch_Click;
                btnReset.Click   += BtnReset_Click;
                btnExport.Click  += BtnExport_Click;
                btnPrint.Click   += BtnPrint_Click;
                btnClose.Click   += BtnClose_Click;
                comboPeriod.ValueChanged  += ComboPeriod_ValueChanged;
                txtSearch.TextChanged     += TxtSearch_TextChanged;
                dtFrom.ValueChanged += DtDate_ValueChanged;
                dtTo.ValueChanged   += DtDate_ValueChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initialising form: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializePeriodCombo()
        {
            comboPeriod.Items.Clear();
            comboPeriod.Items.Add("Today",        "Today");
            comboPeriod.Items.Add("This Week",    "This Week");
            comboPeriod.Items.Add("This Month",   "This Month");
            comboPeriod.Items.Add("Last Month",   "Last Month");
            comboPeriod.Items.Add("This Quarter", "This Quarter");
            comboPeriod.Items.Add("This Year",    "This Year");
            comboPeriod.Items.Add("Custom",       "Custom");
            comboPeriod.Text = "This Year";
        }

        private void InitializeStockFilterCombo()
        {
            comboStockFilter.Items.Clear();
            comboStockFilter.Items.Add("All Items",        "All Items");
            comboStockFilter.Items.Add("Positive Stock",   "Positive Stock");
            comboStockFilter.Items.Add("Zero Stock",       "Zero Stock");
            comboStockFilter.Items.Add("Negative Stock",   "Negative Stock");
            comboStockFilter.Items.Add("Low Stock",        "Low Stock");
            comboStockFilter.Text = "All Items";
        }

        private void LoadGroups()
        {
            try
            {
                var groups = _dropdownRepo.getGroupDDl();
                if (groups?.List != null)
                {
                    comboGroup.DataSource    = groups.List.ToList();
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
                var cats = _dropdownRepo.getCategoryDDl("");
                if (cats?.List != null)
                {
                    comboCategory.DataSource    = cats.List.ToList();
                    comboCategory.ValueMember   = "Id";
                    comboCategory.DisplayMember = "CategoryName";
                }
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════
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
            gridReport.DisplayLayout.Override.CellClickAction                  = CellClickAction.RowSelect;

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
                ("Barcode",          "Barcode",            70,  HAlign.Left),
                ("ItemName",         "Product",           200,  HAlign.Left),
                ("GroupName",        "Group",             110,  HAlign.Left),
                ("CategoryName",     "Category",          110,  HAlign.Left),
                ("BaseUnitName",     "Unit",               60,  HAlign.Center),
                ("ClosingStock",     "Qty",                75,  HAlign.Right),
                ("Cost",             "Unit Cost",           90,  HAlign.Right),
                ("RetailPrice",      "Unit Retail",         90,  HAlign.Right),
                ("ValueAtCost",      "Value @ Cost",       110,  HAlign.Right),
                ("ValueAtRetail",    "Value @ Retail",     120,  HAlign.Right),
                ("PotentialProfit",  "Potential Profit",   120,  HAlign.Right),
                ("MarginPercent",    "Margin %",           85,  HAlign.Right),
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
                if (key == "ClosingStock" || key == "Cost" || key == "RetailPrice" || key == "MarginPercent")
                    col.Format = "N2";
                // Currency formatting for value columns
                if (key == "ValueAtCost" || key == "ValueAtRetail" || key == "PotentialProfit")
                {
                    col.Format = "N2";
                    if (key == "PotentialProfit")
                        col.CellAppearance.ForeColor = Color.FromArgb(0, 150, 60);
                }
            }

            // Hide internal-only or unneeded columns
            var hiddenCols = new[] { "ItemId", "SubCategoryName", "WholeSalePrice", "CreditPrice",
                                     "OpeningStock", "Purchase", "PurchaseReturn", "StockAdjustmentIn",
                                     "StockAdjustmentOut", "StockTransferIn", "StockTransferOut",
                                     "Sales", "SalesReturn", "OrderedStock", "HoldQty",
                                     "AvailableStock", "TotalIn", "TotalOut", "StockValue",
                                     "Profit", "SaleAmount" };
            foreach (var h in hiddenCols)
                if (band.Columns.Exists(h)) band.Columns[h].Hidden = true;
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row.Cells.Exists("ClosingStock"))
                {
                    decimal stock = Convert.ToDecimal(e.Row.Cells["ClosingStock"].Value ?? 0);
                    if (stock < 0)
                    {
                        // Soft Red background for Negative Stock
                        e.Row.Appearance.BackColor = Color.FromArgb(254, 226, 226);
                        e.Row.Appearance.ForeColor = Color.FromArgb(153, 27, 27);
                    }
                    else if (stock == 0)
                    {
                        // Soft Orange background for Zero Stock
                        e.Row.Appearance.BackColor = Color.FromArgb(255, 247, 237);
                        e.Row.Appearance.ForeColor = Color.FromArgb(194, 65, 12);
                    }
                    else
                    {
                        // Clear general styling
                        e.Row.Appearance.Reset();

                        // Highlight High-Value Assets in bold green
                        if (e.Row.Cells.Exists("ValueAtCost"))
                        {
                            decimal valAtCost = Convert.ToDecimal(e.Row.Cells["ValueAtCost"].Value ?? 0);
                            if (valAtCost >= 10000)
                            {
                                e.Row.Cells["ValueAtCost"].Appearance.FontData.Bold = DefaultableBoolean.True;
                                e.Row.Cells["ValueAtCost"].Appearance.ForeColor = Color.FromArgb(21, 128, 61);
                            }
                        }
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
            // First apply card settings at runtime to avoid Designer parse bugs
            SetupCardControls(cardItems, lblItemsCaption, lblItemsValue, "TOTAL UNIQUE ITEMS", "pnlCardItems");
            SetupCardControls(cardQty, lblQtyCaption, lblQtyValue, "TOTAL STOCK QUANTITY", "pnlCardQty");
            SetupCardControls(cardCostVal, lblCostCaption, lblCostValue, "TOTAL VALUE @ COST", "pnlCardCostVal");
            SetupCardControls(cardRetailVal, lblRetailCaption, lblRetailValue, "TOTAL VALUE @ RETAIL", "pnlCardRetailVal");
            SetupCardControls(cardProfit, lblProfitCaption, lblProfitValue, "POTENTIAL MARGIN PROFIT", "pnlCardProfit");

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

                // Add a top colored accent line to each card
                var accentLine = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 3,
                    BackColor = valColor
                };
                card.ClientArea.Controls.Add(accentLine);
                accentLine.BringToFront();

                // Style the value label color
                val.Appearance.ForeColor = valColor;

                x += w + gap;
            }
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
        //  Search / Filter
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

            btnSearch.Text    = "⏳ Searching…";
            btnSearch.Enabled = false;
            this.Cursor       = Cursors.WaitCursor;
            SetStatus("Loading data…");

            var filter = new StockReportFilter
            {
                FromDate   = ((DateTime)dtFrom.Value).Date,
                ToDate     = ((DateTime)dtTo.Value).Date.AddDays(1).AddTicks(-1),
                CompanyId  = !string.IsNullOrEmpty(DataBase.CompanyId)  ? int.Parse(DataBase.CompanyId)  : 1,
                BranchId   = !string.IsNullOrEmpty(DataBase.BranchId)   ? (int.TryParse(DataBase.BranchId, out int bid) ? bid : 0) : 1,
                FinYearId  = !string.IsNullOrEmpty(DataBase.FinyearId)  ? int.Parse(DataBase.FinyearId)  : 1,
                GroupId    = GetComboIntValue(comboGroup),
                CategoryId = GetComboIntValue(comboCategory),
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filter = (StockReportFilter)e.Argument;
            e.Result = _reportRepo.GetStockReport(filter);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null || e.Result is Exception)
                {
                    string msg = e.Error?.Message ?? ((Exception)e.Result).Message;
                    MessageBox.Show("Error loading data: " + msg, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("Error loading data.");
                    return;
                }

                var raw = e.Result as List<StockReportItem>;
                if (raw == null || raw.Count == 0)
                {
                    _allRows = new List<StockValuationRow>();
                    gridReport.DataSource = null;
                    UpdateSummaryCards();
                    SetStatus("No records found for the selected criteria.");
                    MessageBox.Show("No records found.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Map to valuation rows
                _allRows = raw.Select(r => new StockValuationRow
                {
                    ItemId          = r.ItemId,
                    Barcode         = r.Barcode,
                    ItemName        = r.ItemName,
                    GroupName       = r.GroupName,
                    CategoryName    = r.CategoryName,
                    SubCategoryName = r.SubCategoryName,
                    BaseUnitName    = r.BaseUnitName,
                    ClosingStock    = r.ClosingStock,
                    Cost            = r.Cost,
                    RetailPrice     = r.RetailPrice,
                    ValueAtCost     = r.ClosingStock * r.Cost,
                    ValueAtRetail   = r.ClosingStock * r.RetailPrice,
                    PotentialProfit = (r.ClosingStock * r.RetailPrice) - (r.ClosingStock * r.Cost),
                }).ToList();

                ApplyFilters();
            }
            finally
            {
                btnSearch.Text    = "🔍  Search";
                btnSearch.Enabled = true;
                this.Cursor       = Cursors.Default;
            }
        }

        private void ApplyFilters()
        {
            string search = txtSearch.Text.Trim();
            string stockFilter = comboStockFilter.Text;

            var filtered = _allRows.AsEnumerable();

            // Text search
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(r =>
                    (r.Barcode   ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.ItemName  ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.GroupName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.CategoryName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            // Stock filter
            switch (stockFilter)
            {
                case "Positive Stock":  filtered = filtered.Where(r => r.ClosingStock > 0);  break;
                case "Zero Stock":      filtered = filtered.Where(r => r.ClosingStock == 0); break;
                case "Negative Stock":  filtered = filtered.Where(r => r.ClosingStock < 0);  break;
                case "Low Stock":       filtered = filtered.Where(r => r.ClosingStock > 0 && r.ClosingStock < 5); break;
            }

            var result = filtered.ToList();
            gridReport.DataSource = result;
            UpdateSummaryCards(result);
            SetStatus($"Showing {result.Count} of {_allRows.Count} item(s)  |  F5 = Search  |  Ctrl+E = Export  |  Ctrl+P = Print  |  Esc = Close");
        }

        private void UpdateSummaryCards(List<StockValuationRow> rows = null)
        {
            rows = rows ?? _allRows;
            lblItemsValue.Text  = rows.Count.ToString("N0");
            lblQtyValue.Text    = rows.Sum(r => r.ClosingStock).ToString("N2");
            lblCostValue.Text   = rows.Sum(r => r.ValueAtCost).ToString("N2");
            lblRetailValue.Text = rows.Sum(r => r.ValueAtRetail).ToString("N2");
            lblProfitValue.Text = rows.Sum(r => r.PotentialProfit).ToString("N2");
        }

        // ════════════════════════════════════════════════════════════
        //  Helpers
        // ════════════════════════════════════════════════════════════
        private int? GetComboIntValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo.Value == null) return null;
            if (int.TryParse(combo.Value.ToString(), out int id) && id > 0) return id;
            return null;
        }

        private void SetStatus(string message)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(new Action(() => lblStatus.Text = message));
            else
                lblStatus.Text = message;
        }

        // ════════════════════════════════════════════════════════════
        //  Event handlers
        // ════════════════════════════════════════════════════════════
        private void BtnReset_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            dtFrom.Value = new DateTime(now.Year, 1, 1);
            dtTo.Value   = now;
            comboPeriod.Text      = "This Year";
            comboGroup.Value      = null;
            comboCategory.Value   = null;
            comboStockFilter.Text = "All Items";
            txtSearch.Text        = "";
            _allRows              = new List<StockValuationRow>();
            gridReport.DataSource = null;
            UpdateSummaryCards();
            SetStatus("Filters reset.  |  F5 = Search  |  Esc = Close");
        }

        private void BtnClose_Click(object sender, EventArgs e) => this.Close();

        private void BtnPrint_Click(object sender, EventArgs e) => gridReport.PrintPreview();

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    FileName = $"StockValuation_{DateTime.Now:yyyyMMdd_HHmm}.csv"
                };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                if (gridReport.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ExportToCsv(dlg.FileName);
                MessageBox.Show("Export completed successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Summary Row at the bottom
            sb.AppendLine();
            
            int itemsCount = gridReport.Rows.Count;
            decimal totalQty = 0;
            decimal totalValueCost = 0;
            decimal totalValueRetail = 0;
            decimal totalProfit = 0;
            
            foreach (UltraGridRow row in gridReport.Rows)
            {
                totalQty         += Convert.ToDecimal(row.Cells["ClosingStock"].Value ?? 0);
                totalValueCost   += Convert.ToDecimal(row.Cells["ValueAtCost"].Value ?? 0);
                totalValueRetail += Convert.ToDecimal(row.Cells["ValueAtRetail"].Value ?? 0);
                totalProfit      += Convert.ToDecimal(row.Cells["PotentialProfit"].Value ?? 0);
            }

            foreach (UltraGridColumn col in band.Columns)
            {
                if (!col.Hidden)
                {
                    if (col.Key == "Barcode")
                        sb.Append("TOTALS,");
                    else if (col.Key == "ItemName")
                        sb.Append($"({itemsCount} Items),");
                    else if (col.Key == "ClosingStock")
                        sb.Append($"{totalQty:F2},");
                    else if (col.Key == "ValueAtCost")
                        sb.Append($"{totalValueCost:F2},");
                    else if (col.Key == "ValueAtRetail")
                        sb.Append($"{totalValueRetail:F2},");
                    else if (col.Key == "PotentialProfit")
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

        private void FrmStockValuationReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)                         { e.Handled = true; RunSearch(); }
            else if (e.Control && e.KeyCode == Keys.E)        { e.Handled = true; BtnExport_Click(sender, e); }
            else if (e.Control && e.KeyCode == Keys.P)        { e.Handled = true; BtnPrint_Click(sender, e); }
            else if (e.KeyCode == Keys.Escape)                { e.Handled = true; this.Close(); }
        }
    }

    // ════════════════════════════════════════════════════════════
    //  Valuation Row Model (calculated in-memory from StockReportItem)
    // ════════════════════════════════════════════════════════════
    public class StockValuationRow
    {
        public int     ItemId          { get; set; }
        public string  Barcode         { get; set; }
        public string  ItemName        { get; set; }
        public string  GroupName       { get; set; }
        public string  CategoryName    { get; set; }
        public string  SubCategoryName { get; set; }
        public string  BaseUnitName    { get; set; }
        public decimal ClosingStock    { get; set; }
        public decimal Cost            { get; set; }
        public decimal RetailPrice     { get; set; }
        public decimal ValueAtCost     { get; set; }
        public decimal ValueAtRetail   { get; set; }
        public decimal PotentialProfit { get; set; }
        public decimal MarginPercent   => RetailPrice > 0 ? ((RetailPrice - Cost) / RetailPrice) * 100 : 0;
    }
}
