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
    public partial class frmCustomerwiseSalesSummaryReport : Form
    {
        // ════════════════════════════════════════════════════════════
        //  Fields
        // ════════════════════════════════════════════════════════════
        private CustomerwiseSalesSummaryRepo _reportRepo;
        private Dropdowns _dropdownRepo;
        private BackgroundWorker _searchWorker;
        private bool _isChangingDates = false;
        private int? _selectedCustomerId = null;
        private string _selectedCustomerName = string.Empty;

        /// <summary>All rows fetched from the DB (before any in-memory filter)</summary>
        private List<CustomerwiseSalesSummaryItem> _allRows = new List<CustomerwiseSalesSummaryItem>();

        // ════════════════════════════════════════════════════════════
        //  Constructor
        // ════════════════════════════════════════════════════════════
        public frmCustomerwiseSalesSummaryReport()
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
            _searchWorker.DoWork += SearchWorker_DoWork;
            _searchWorker.RunWorkerCompleted += SearchWorker_RunWorkerCompleted;
        }

        private void InitializeForm()
        {
            try
            {
                _reportRepo = new CustomerwiseSalesSummaryRepo();
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

                // Load initial dropdown list choices
                LoadGroups();
                LoadCategories();

                // Configure grid properties
                StyleGrid();

                // Setup static card control designs
                SetupCardControls(cardCustCount,  lblCustCountCaption,  lblCustCountValue,  "UNIQUE CUSTOMERS", "cardCustCount");
                SetupCardControls(cardItemCount,  lblItemCountCaption,  lblItemCountValue,  "UNIQUE ITEMS SOLD", "cardItemCount");
                SetupCardControls(cardTotalQty,   lblTotalQtyCaption,   lblTotalQtyValue,   "TOTAL QTY PURCHASED", "cardTotalQty");
                SetupCardControls(cardTotalSales, lblTotalSalesCaption, lblTotalSalesValue, "TOTAL SPENT AMOUNT", "cardTotalSales");

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
                btnSelectCustomer.Click  += BtnSelectCustomer_Click;
                btnClearCustomer.Click   += BtnClearCustomer_Click;
                comboPeriod.ValueChanged += ComboPeriod_ValueChanged;
                dtFrom.ValueChanged      += DtDate_ValueChanged;
                dtTo.ValueChanged        += DtDate_ValueChanged;
                txtSearch.TextChanged    += TxtSearch_TextChanged;

                // Handle keyboard shortcuts
                this.KeyPreview = true;
                this.KeyDown += FrmCustomerwiseSalesSummaryReport_KeyDown;
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
                ("SlNo",              "S.No",               50,  HAlign.Center),
                ("BillDate",          "Date & Time",       130,  HAlign.Center),
                ("CustomerId",        "Cust ID",            60,  HAlign.Center),
                ("CustomerName",      "Customer Name",     160,  HAlign.Left),
                ("Phone",             "Contact No",        100,  HAlign.Left),
                ("Barcode",           "Barcode",            90,  HAlign.Left),
                ("ItemName",          "Product Name",      180,  HAlign.Left),
                ("GroupName",         "Group",             100,  HAlign.Left),
                ("CategoryName",      "Category",          100,  HAlign.Left),
                ("BaseUnitName",      "Unit",               60,  HAlign.Center),
                ("TotalQtySold",      "Qty Sold",           80,  HAlign.Right),
                ("TotalSalesAmount",  "Total Spent",       110,  HAlign.Right),
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

                    // Format decimal columns
                    if (col.Item1 == "TotalQtySold")
                    {
                        bandCol.Format = "N2";
                    }
                    else if (col.Item1 == "TotalSalesAmount")
                    {
                        bandCol.Format = "N2";
                    }
                    else if (col.Item1 == "BillDate")
                    {
                        bandCol.Format = "yyyy-MM-dd hh:mm tt";
                    }
                }
            }

            // Hide unused columns if any auto-generate
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
            // Optional styling based on values can be added here
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch, Color.FromArgb(25, 118, 210), Color.FromArgb(33, 150, 243), Color.FromArgb(66, 165, 245));
            StyleButton(btnReset,  Color.FromArgb(245, 124, 0),  Color.FromArgb(255, 152, 0),  Color.FromArgb(255, 167, 38));
            StyleButton(btnExport, Color.FromArgb(0, 121, 107),  Color.FromArgb(0, 150, 136),  Color.FromArgb(38, 166, 154));
            StyleButton(btnPrint,  Color.FromArgb(81, 45, 168),  Color.FromArgb(103, 58, 183), Color.FromArgb(126, 87, 194));
            StyleButton(btnClose,  Color.FromArgb(198, 40, 40),  Color.FromArgb(244, 67, 54),  Color.FromArgb(229, 115, 115));
            
            // Standard small dialog buttons styling
            StyleDialogButton(btnSelectCustomer, Color.FromArgb(51, 65, 85));
            StyleDialogButton(btnClearCustomer, Color.FromArgb(100, 116, 139));
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

        private void StyleDialogButton(Infragistics.Win.Misc.UltraButton btn, Color baseColor)
        {
            btn.UseAppStyling = false;
            btn.UseOsThemes = DefaultableBoolean.False;
            btn.Appearance.BackColor = baseColor;
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.FontData.Bold = DefaultableBoolean.True;
            btn.Appearance.FontData.SizeInPoints = 8.5F;
            btn.HotTrackAppearance.BackColor = Color.FromArgb(15, 23, 42);
        }

        // ════════════════════════════════════════════════════════════
        //  Layout Summary Cards
        // ════════════════════════════════════════════════════════════
        private void LayoutSummaryCards()
        {
            var cards = new[]
            {
                (cardCustCount,  lblCustCountValue,  Color.FromArgb(25, 118, 210)),
                (cardItemCount,  lblItemCountValue,  Color.FromArgb(103, 58, 183)),
                (cardTotalQty,   lblTotalQtyValue,   Color.FromArgb(245, 124, 0)),
                (cardTotalSales, lblTotalSalesValue, Color.FromArgb(0, 150, 136)),
            };

            int totalWidth = panelSummary.ClientArea.Width - 30; // 15px left/right margin
            int cardCount = cards.Length;
            int gap = 10;
            int cardWidth = (totalWidth - (gap * (cardCount - 1))) / cardCount;
            if (cardWidth < 200) cardWidth = 200;
            int cardHeight = 62;

            int x = 15, y = 10;
            foreach (var (card, val, valColor) in cards)
            {
                card.Location = new Point(x, y);
                card.Size = new Size(cardWidth, cardHeight);

                // Add accent line at top of card
                var accentLine = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 3,
                    BackColor = valColor
                };
                card.ClientArea.Controls.Add(accentLine);
                accentLine.BringToFront();

                val.Appearance.ForeColor = valColor;
                x += cardWidth + gap;
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
            value.Text = "0";
            value.UseAppStyling = false;
            value.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }

        // ════════════════════════════════════════════════════════════
        //  Event Handlers
        // ════════════════════════════════════════════════════════════
        private void FrmCustomerwiseSalesSummaryReport_Load(object sender, EventArgs e)
        {
            InitializeDateControls();
            FetchFromDatabase();
        }

        private void InitializeDateControls()
        {
            _isChangingDates = true;
            try
            {
                DateTime now = DateTime.Now;
                dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                dtTo.Value   = now.Date;
            }
            finally
            {
                _isChangingDates = false;
            }
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
                    case "Today":
                        dtFrom.Value = now.Date;
                        dtTo.Value   = now.Date;
                        break;
                    case "This Week":
                        dtFrom.Value = now.Date.AddDays(-(int)now.DayOfWeek);
                        dtTo.Value   = now.Date;
                        break;
                    case "This Month":
                        dtFrom.Value = new DateTime(now.Year, now.Month, 1);
                        dtTo.Value   = now.Date;
                        break;
                    case "Last Month":
                        var lm = now.AddMonths(-1);
                        dtFrom.Value = new DateTime(lm.Year, lm.Month, 1);
                        dtTo.Value   = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                        break;
                    case "This Quarter":
                        int q = (now.Month - 1) / 3;
                        dtFrom.Value = new DateTime(now.Year, q * 3 + 1, 1);
                        dtTo.Value   = now.Date;
                        break;
                    case "This Year":
                        dtFrom.Value = new DateTime(now.Year, 1, 1);
                        dtTo.Value   = now.Date;
                        break;
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

        private void BtnSelectCustomer_Click(object sender, EventArgs e)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmCustomerDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                if (dlg.SelectedCustomerId <= 0) return;

                _selectedCustomerId   = dlg.SelectedCustomerId;
                _selectedCustomerName = dlg.SelectedCustomerName ?? string.Empty;
                txtCustomerName.Text  = _selectedCustomerName;

                // Auto search
                FetchFromDatabase();
            }
        }

        private void BtnClearCustomer_Click(object sender, EventArgs e)
        {
            _selectedCustomerId   = null;
            _selectedCustomerName = string.Empty;
            txtCustomerName.Text  = string.Empty;
            FetchFromDatabase();
        }

        private void BtnSearch_Click(object sender, EventArgs e) => FetchFromDatabase();

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _selectedCustomerId   = null;
            _selectedCustomerName = string.Empty;
            txtCustomerName.Text  = string.Empty;
            comboGroup.Value      = null;
            comboCategory.Value   = null;
            txtSearch.Text        = string.Empty;

            InitializeDateControls();
            comboPeriod.Value = "This Month";

            FetchFromDatabase();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => FilterFetchedRows();

        private void FrmCustomerwiseSalesSummaryReport_KeyDown(object sender, KeyEventArgs e)
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

            var filter = new CustomerwiseSalesSummaryFilter
            {
                CompanyId    = SessionContext.CompanyId,
                BranchId     = SessionContext.BranchId,
                FinYearId    = SessionContext.FinYearId,
                FromDate     = dtFrom.DateTime.Date,
                ToDate       = dtTo.DateTime.Date,
                CustomerId   = _selectedCustomerId,
                GroupId      = comboGroup.Value != null && Convert.ToInt32(comboGroup.Value) > 0 ? (int?)Convert.ToInt32(comboGroup.Value) : null,
                CategoryId   = comboCategory.Value != null && Convert.ToInt32(comboCategory.Value) > 0 ? (int?)Convert.ToInt32(comboCategory.Value) : null,
                SearchQuery  = txtSearch.Text.Trim()
            };

            _searchWorker.RunWorkerAsync(filter);
        }

        private void SearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filter = (CustomerwiseSalesSummaryFilter)e.Argument;
            e.Result = _reportRepo.GetCustomerwiseSalesSummary(filter);
        }

        private void SearchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    MessageBox.Show($"Search failed: {e.Error.Message}", "Database Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Search failed.";
                    return;
                }

                _allRows = e.Result as List<CustomerwiseSalesSummaryItem> ?? new List<CustomerwiseSalesSummaryItem>();
                FilterFetchedRows();
            }
            finally
            {
                btnSearch.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }

        private void FilterFetchedRows()
        {
            string searchVal = txtSearch.Text.Trim().ToLower();
            List<CustomerwiseSalesSummaryItem> filtered;

            if (string.IsNullOrEmpty(searchVal))
            {
                filtered = _allRows;
            }
            else
            {
                filtered = _allRows.Where(r => 
                    r.CustomerName.ToLower().Contains(searchVal) ||
                    r.Phone.Contains(searchVal) ||
                    r.ItemName.ToLower().Contains(searchVal) ||
                    r.Barcode.Contains(searchVal)
                ).ToList();
            }

            gridReport.DataSource = filtered;
            CalculateSummaryValues(filtered);
            lblStatus.Text = $"Ready  |  Found {filtered.Count} records.";
        }

        private void CalculateSummaryValues(List<CustomerwiseSalesSummaryItem> items)
        {
            if (items == null || items.Count == 0)
            {
                lblCustCountValue.Text  = "0";
                lblItemCountValue.Text  = "0";
                lblTotalQtyValue.Text   = "0.00";
                lblTotalSalesValue.Text = "0.00";
                return;
            }

            var uniqueCustCount = items.Select(i => i.CustomerId).Distinct().Count();
            var uniqueItemCount = items.Select(i => i.ItemId).Distinct().Count();
            var totalQty        = items.Sum(i => i.TotalQtySold);
            var totalSales      = items.Sum(i => i.TotalSalesAmount);

            lblCustCountValue.Text  = uniqueCustCount.ToString("N0");
            lblItemCountValue.Text  = uniqueItemCount.ToString("N0");
            lblTotalQtyValue.Text   = totalQty.ToString("N2");
            lblTotalSalesValue.Text = totalSales.ToString("N2");
        }

        // ════════════════════════════════════════════════════════════
        //  CSV Export & Print Operations
        // ════════════════════════════════════════════════════════════
        private void BtnExport_Click(object sender, EventArgs e)
        {
            var rows = gridReport.DataSource as List<CustomerwiseSalesSummaryItem>;
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
                    saveDlg.FileName = $"CustomerwiseSalesSummary_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDlg.ShowDialog() != DialogResult.OK) return;

                    var sb = new StringBuilder();
                    sb.AppendLine("S.No,Date & Time,Customer ID,Customer Name,Contact No,Barcode,Product Name,Group,Category,Unit,Qty Sold,Total Spent");

                    foreach (var r in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            CsvCell(r.SlNo.ToString()),
                            CsvCell(r.BillDate.ToString("yyyy-MM-dd hh:mm tt")),
                            CsvCell(r.CustomerId.ToString()),
                            CsvCell(r.CustomerName),
                            CsvCell(r.Phone),
                            CsvCell(r.Barcode),
                            CsvCell(r.ItemName),
                            CsvCell(r.GroupName),
                            CsvCell(r.CategoryName),
                            CsvCell(r.BaseUnitName),
                            r.TotalQtySold.ToString("F2"),
                            r.TotalSalesAmount.ToString("F2")
                        ));
                    }

                    // Add summary total row at bottom
                    sb.AppendLine();
                    sb.AppendLine(string.Join(",",
                        "",
                        "",
                        "",
                        "TOTALS",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        rows.Sum(r => r.TotalQtySold).ToString("F2"),
                        rows.Sum(r => r.TotalSalesAmount).ToString("F2")
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
