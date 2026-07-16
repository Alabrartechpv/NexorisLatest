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

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmLowStockAlertReport : Form
    {
        // ════════════════════════════════════════════════════════════
        //  Fields
        // ════════════════════════════════════════════════════════════
        private LowStockAlertRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _accentPanelsCreated = false;

        /// <summary>All rows fetched from the DB (before any in-memory filter)</summary>
        private List<LowStockAlertItem> _allRows = new List<LowStockAlertItem>();

        // ════════════════════════════════════════════════════════════
        //  Constructor
        // ════════════════════════════════════════════════════════════
        public frmLowStockAlertReport()
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
                _reportRepo = new LowStockAlertRepo();
                _dropdownRepo = new Dropdowns();

                // Load initial dropdown list choices
                LoadGroups();
                LoadCategories();

                // Configure grid properties
                StyleGrid();

                // Setup static card control designs
                SetupCardControls(cardItemCount,     lblItemCountCaption,     lblItemCountValue,     "ITEMS BELOW REORDER LEVEL", "cardItemCount");
                SetupCardControls(cardStockValueCost, lblStockValueCostCaption, lblStockValueCostValue, "CURRENT STOCK VALUE @ COST", "cardStockValueCost");
                SetupCardControls(cardShortageCost,   lblShortageCostCaption,   lblShortageCostValue,   "EST. REPLENISHMENT COST", "cardShortageCost");

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
                txtSearch.TextChanged    += TxtSearch_TextChanged;

                // Handle keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += FrmLowStockAlertReport_KeyDown;

                // Cancel background work on form close to avoid disposed-control crash
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
                ("SlNo",          "S.No",                50,   HAlign.Center),
                ("Barcode",       "Barcode",            100,   HAlign.Left),
                ("ItemName",      "Product Name",       220,   HAlign.Left),
                ("GroupName",     "Group",              120,   HAlign.Left),
                ("CategoryName",  "Category",           120,   HAlign.Left),
                ("BaseUnitName",  "Unit",                70,   HAlign.Center),
                ("CostPrice",     "Cost Price",         100,   HAlign.Right),
                ("RetailPrice",   "Retail Price",       100,   HAlign.Right),
                ("ReorderLevel",  "Reorder Level",      110,   HAlign.Right),
                ("CurrentStock",  "Current Stock",      110,   HAlign.Right),
                ("ShortageQty",   "Shortage Qty",       110,   HAlign.Right),
            };

            int index = 0;
            foreach (var col in columns)
            {
                if (band.Columns.Exists(col.Item1))
                {
                    var bandCol = band.Columns[col.Item1];
                    bandCol.Header.Caption = col.Item2;
                    bandCol.Width          = col.Item3;
                    bandCol.CellAppearance.TextHAlign = col.Item4;
                    bandCol.Header.VisiblePosition = index++;

                    // Format numeric values
                    if (col.Item1 == "SlNo")
                    {
                        bandCol.Format = "N0";
                    }
                    else if (col.Item1 == "CostPrice" || col.Item1 == "RetailPrice" || 
                             col.Item1 == "ReorderLevel" || col.Item1 == "CurrentStock" || col.Item1 == "ShortageQty")
                    {
                        bandCol.Format = "N2";
                    }
                }
            }

            // Hide unused columns
            foreach (UltraGridColumn col in band.Columns)
            {
                bool isPlanned = false;
                foreach (var pc in columns)
                {
                    if (pc.Item1 == col.Key) { isPlanned = true; break; }
                }
                if (!isPlanned) col.Hidden = true;
            }
        }

        private void GridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            // Soft highlight shortage rows for visually fast warnings
            try
            {
                if (e.Row.Cells.Exists("CurrentStock") && e.Row.Cells.Exists("ReorderLevel"))
                {
                    double stock = Convert.ToDouble(e.Row.Cells["CurrentStock"].Value ?? 0.0);
                    double reorder = Convert.ToDouble(e.Row.Cells["ReorderLevel"].Value ?? 0.0);
                    if (stock <= 0.0)
                    {
                        // Critical shortage - soft red highlight
                        e.Row.Appearance.BackColor = Color.FromArgb(254, 242, 242);
                        e.Row.Cells["CurrentStock"].Appearance.ForeColor = Color.FromArgb(220, 38, 38);
                        e.Row.Cells["CurrentStock"].Appearance.FontData.Bold = DefaultableBoolean.True;
                    }
                    else if (stock <= reorder)
                    {
                        // Standard low stock - soft orange/yellow highlight
                        e.Row.Appearance.BackColor = Color.FromArgb(255, 251, 235);
                        e.Row.Cells["CurrentStock"].Appearance.ForeColor = Color.FromArgb(217, 119, 6);
                    }
                }
            }
            catch { }
        }

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
        //  Layout Summary Cards
        // ════════════════════════════════════════════════════════════
        private void LayoutSummaryCards()
        {
            var cards = new[]
            {
                (cardItemCount,     lblItemCountValue,     Color.FromArgb(211, 47, 47)),
                (cardStockValueCost, lblStockValueCostValue, Color.FromArgb(25, 118, 210)),
                (cardShortageCost,   lblShortageCostValue,   Color.FromArgb(0, 150, 136))
            };

            int totalWidth = panelSummary.ClientArea.Width - 30; // 15px left/right margin
            int cardCount = cards.Length;
            int gap = 15;
            int cardWidth = (totalWidth - (gap * (cardCount - 1))) / cardCount;
            if (cardWidth < 220) cardWidth = 220;
            int cardHeight = 62;

            int x = 15, y = 10;
            foreach (var (card, val, valColor) in cards)
            {
                card.Location = new Point(x, y);
                card.Size = new Size(cardWidth, cardHeight);

                // Add accent line at top of card
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
                x += cardWidth + gap;
            }
            _accentPanelsCreated = true;
        }

        private void SetupCardControls(Infragistics.Win.Misc.UltraPanel card, Infragistics.Win.Misc.UltraLabel caption, Infragistics.Win.Misc.UltraLabel value, string captionText, string cardName)
        {
            card.Name = cardName;
            card.Size = new System.Drawing.Size(220, 62);
            card.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            card.UseAppStyling = false;
            card.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            card.Appearance.BackColor = System.Drawing.Color.White;
            card.Appearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);

            // Caption
            caption.Text = captionText;
            caption.Location = new Point(12, 8);
            caption.Size = new System.Drawing.Size(200, 15);
            caption.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            caption.Appearance.ForeColor = Color.FromArgb(100, 116, 139);
            caption.UseAppStyling = false;
            caption.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

            // Value label
            value.Location = new Point(12, 26);
            value.Size = new System.Drawing.Size(200, 28);
            value.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            value.Text = "0";
            value.UseAppStyling = false;
            value.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }

        // ════════════════════════════════════════════════════════════
        //  Event Handlers
        // ════════════════════════════════════════════════════════════
        private void FrmLowStockAlertReport_Load(object sender, EventArgs e)
        {
            FetchFromDatabase();
        }

        private void BtnSearch_Click(object sender, EventArgs e) => FetchFromDatabase();

        private void BtnReset_Click(object sender, EventArgs e)
        {
            comboGroup.Value    = null;
            comboCategory.Value = null;
            txtSearch.Text      = string.Empty;
            gridReport.DataSource = null;
            _allRows.Clear();
            CalculateSummaryValues(new List<LowStockAlertItem>());
            lblStatus.Text = "Ready  |  Select filters and press Search (F5)";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => FilterFetchedRows();

        private void FrmLowStockAlertReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                FetchFromDatabase();
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                e.Handled = true;
                btnExport.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                e.Handled = true;
                btnPrint.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        // ════════════════════════════════════════════════════════════
        //  Asynchronous DB Fetching
        // ════════════════════════════════════════════════════════════
        private void FetchFromDatabase()
        {
            if (_searchWorker.IsBusy) return;

            Cursor.Current = Cursors.WaitCursor;
            btnSearch.Enabled = false;
            lblStatus.Text = "Searching database records... Please wait.";

            var filter = new LowStockAlertFilter
            {
                CompanyId   = SessionContext.CompanyId,
                BranchId    = SessionContext.BranchId,
                FinYearId   = SessionContext.FinYearId,
                GroupId     = GetComboIntValue(comboGroup),
                CategoryId  = GetComboIntValue(comboCategory),
                SearchQuery = txtSearch.Text.Trim()
            };

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
            var filter = (LowStockAlertFilter)e.Argument;
            e.Result = _reportRepo.GetLowStockAlerts(filter);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;

            try
            {
                if (e.Cancelled) return;

                if (e.Error != null)
                {
                    MessageBox.Show($"Search failed: {e.Error.Message}", "Database Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Search failed.";
                    return;
                }

                _allRows = e.Result as List<LowStockAlertItem> ?? new List<LowStockAlertItem>();
                FilterFetchedRows();
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                {
                    btnSearch.Enabled = true;
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void FilterFetchedRows()
        {
            string searchVal = txtSearch.Text.Trim().ToLower();
            List<LowStockAlertItem> filtered;

            if (string.IsNullOrEmpty(searchVal))
            {
                filtered = _allRows;
            }
            else
            {
                filtered = _allRows.Where(r => 
                    r.ItemName.ToLower().Contains(searchVal) ||
                    r.Barcode.ToLower().Contains(searchVal) ||
                    r.GroupName.ToLower().Contains(searchVal) ||
                    r.CategoryName.ToLower().Contains(searchVal)
                ).ToList();
            }

            gridReport.DataSource = filtered;
            CalculateSummaryValues(filtered);
            lblStatus.Text = $"Ready  |  Found {filtered.Count} records.";
        }

        private void CalculateSummaryValues(List<LowStockAlertItem> items)
        {
            if (items == null || items.Count == 0)
            {
                lblItemCountValue.Text      = "0";
                lblStockValueCostValue.Text = "₹0.00";
                lblShortageCostValue.Text   = "₹0.00";
                return;
            }

            int totalCount = items.Count;
            double totalStockValueCost = items.Sum(i => i.CurrentStock * i.CostPrice);
            double totalShortageCost   = items.Sum(i => i.ShortageQty * i.CostPrice);

            lblItemCountValue.Text      = totalCount.ToString("N0");
            lblStockValueCostValue.Text = "₹" + totalStockValueCost.ToString("N2");
            lblShortageCostValue.Text   = "₹" + totalShortageCost.ToString("N2");
        }

        // ════════════════════════════════════════════════════════════
        //  CSV Export & Print Operations
        // ════════════════════════════════════════════════════════════
        private void BtnExport_Click(object sender, EventArgs e)
        {
            var rows = gridReport.DataSource as List<LowStockAlertItem>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var saveDlg = new SaveFileDialog())
                {
                    saveDlg.Filter = "CSV Files (*.csv)|*.csv";
                    saveDlg.FileName = $"LowStockAlerts_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDlg.ShowDialog() != DialogResult.OK) return;

                    var sb = new StringBuilder();
                    sb.AppendLine("S.No,Barcode,Product Name,Group,Category,Unit,Cost Price,Retail Price,Reorder Level,Current Stock,Shortage Qty");

                    foreach (var r in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(r.SlNo.ToString()),
                            CsvCell(r.Barcode),
                            CsvCell(r.ItemName),
                            CsvCell(r.GroupName),
                            CsvCell(r.CategoryName),
                            CsvCell(r.BaseUnitName),
                            r.CostPrice.ToString("F2"),
                            r.RetailPrice.ToString("F2"),
                            r.ReorderLevel.ToString("F2"),
                            r.CurrentStock.ToString("F2"),
                            r.ShortageQty.ToString("F2")
                        ));
                    }

                    // Add summary totals row
                    sb.AppendLine();
                    sb.AppendLine(string.Join(",",
                        "",
                        "",
                        "TOTALS",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        rows.Sum(r => r.CurrentStock).ToString("F2"),
                        rows.Sum(r => r.ShortageQty).ToString("F2")
                    ));

                    File.WriteAllText(saveDlg.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Report exported successfully!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            gridReport.PrintPreview();
        }

        private void BtnClose_Click(object sender, EventArgs e) => Close();

        private string CsvCell(string val)
        {
            if (string.IsNullOrEmpty(val)) return "\"\"";
            if (val.Contains(",") || val.Contains("\"") || val.Contains("\n") || val.Contains("\r"))
            {
                return "\"" + val.Replace("\"", "\"\"") + "\"";
            }
            return "\"" + val + "\"";
        }
    }
}
