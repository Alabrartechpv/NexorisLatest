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
                this.BackColor = FormBackColor;
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
        //  Grid styling (matches frmStockReport theme)
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
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnReset);
            StyleClassicButton(btnExport);
            StyleClassicButton(btnPrint);
            StyleClassicButton(btnClose);
            
            // Standard small dialog buttons styling
            StyleClassicButton(btnSelectCustomer);
            StyleClassicButton(btnClearCustomer);
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

        public void RibbonClear() => BtnReset_Click(this, EventArgs.Empty);
        public void Clear() => BtnReset_Click(this, EventArgs.Empty);

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
