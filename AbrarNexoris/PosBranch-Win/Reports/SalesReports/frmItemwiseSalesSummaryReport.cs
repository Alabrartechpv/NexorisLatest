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
        // ─── Colour Palette (matches frmStockReport theme) ─────────────────────────
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop        = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom     = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

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
                this.BackColor = FormBackColor;
                panelHeader.Appearance.BackColor = Color.White;
                lblTitle.Appearance.ForeColor    = Color.FromArgb(18, 49, 102);
                lblSubtitle.Appearance.ForeColor = Color.FromArgb(70, 90, 120);

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
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes   = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle    = UIElementBorderStyle.Solid;

            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew       = AllowAddNew.No;
            layout.Override.AllowDelete       = DefaultableBoolean.False;
            layout.Override.AllowUpdate       = DefaultableBoolean.False;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType      = FilterUIType.FilterRow;
            layout.Override.CellClickAction   = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            layout.Override.SelectTypeRow     = SelectType.Single;
            layout.Override.RowSelectors      = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth  = 35;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor  = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;

            layout.Override.RowSelectorAppearance.BackColor  = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor   = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign  = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor  = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor  = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;

            layout.Override.RowAppearance.BackColor          = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor        = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor    = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor    = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor  = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor  = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor  = Color.White;
            layout.Override.CellAppearance.BorderColor       = GridRowLine;
            layout.Override.CellAppearance.ForeColor         = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name     = "Segoe UI";
            layout.Override.CellAppearance.FontData.SizeInPoints = 9F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell   = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow    = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight      = 24;
            layout.Override.DefaultRowHeight  = 24;
            layout.RowConnectorStyle  = RowConnectorStyle.Solid;
            layout.RowConnectorColor  = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor  = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor  = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            // Filter row styling
            layout.Override.FilterRowAppearance.BackColor   = Color.FromArgb(255, 255, 230);
            layout.Override.FilterRowAppearance.ForeColor   = Color.FromArgb(33, 33, 33);
            layout.Override.FilterRowPromptAppearance.ForeColor = Color.FromArgb(140, 140, 140);

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

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
        //  Button styling (matches frmStockReport theme)
        // ════════════════════════════════════════════════════════════
        private void StyleButtons()
        {
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnReset);
            StyleClassicButton(btnExport);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnClose);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton btn)
        {
            if (btn == null) return;
            btn.UseAppStyling  = false;
            btn.UseOsThemes    = DefaultableBoolean.False;
            btn.ButtonStyle    = UIElementButtonStyle.Flat;
            btn.UseFlatMode    = DefaultableBoolean.False;
            btn.Appearance.BackColor  = ButtonBlueTop;
            btn.Appearance.BackColor2 = ButtonBlueBottom;
            btn.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btn.Appearance.ForeColor   = ButtonTextBlue;
            btn.Appearance.BorderColor = ButtonLightOutline;
            btn.Appearance.TextHAlign  = HAlign.Center;
            btn.Appearance.TextVAlign  = VAlign.Middle;
            btn.Appearance.FontData.Bold = DefaultableBoolean.False;
            btn.Appearance.FontData.SizeInPoints = 9;
            btn.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn.HotTrackAppearance.BackColor  = Color.FromArgb(241, 247, 254);
            btn.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            btn.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.HotTrackAppearance.BorderColor = ButtonLightOutline;
            btn.HotTrackAppearance.ForeColor   = ButtonTextBlue;
            btn.PressedAppearance.BackColor  = Color.FromArgb(118, 161, 214);
            btn.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            btn.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            btn.PressedAppearance.ForeColor   = ButtonTextBlue;
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
        public void RibbonClear() => BtnReset_Click(this, EventArgs.Empty);
        public void Clear() => BtnReset_Click(this, EventArgs.Empty);

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
