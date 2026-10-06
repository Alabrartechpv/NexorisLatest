using Infragistics.Win;
using Infragistics.Win.Misc;
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
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesReportMasterDetail : Form
    {
        #region Colour Palette (exact matches frmPurchaseReturn / frmStockReport theme)
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(245, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop        = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom     = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);
        #endregion

        #region Private Fields
        private SalesReportRepository _reportRepository;
        private Dropdowns _dropdowns;
        private readonly List<ComboItem> _customerOptions = new List<ComboItem>();

        // Dynamic footer panel and cell controls (exact frmPurchaseReturn implementation)
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SubTotal",
            "TaxAmt",
            "TaxAmount",
            "NetAmount",
            "Profit"
        };

        // Column Chooser & Drag-Down to Hide
        private ListBox columnChooserListBox;
        private Form columnChooserForm;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        private bool isLoading = false;
        private DataTable _currentDataTable;
        #endregion

        #region Helper Classes
        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Constructor & Lifecycle
        public frmSalesReportMasterDetail()
        {
            _reportRepository = new SalesReportRepository();
            _dropdowns = new Dropdowns();

            InitializeComponent();

            Load += FrmSalesReportMasterDetail_Load;
            FormClosed += FrmSalesReportMasterDetail_FormClosed;
            KeyDown += FrmSalesReportMasterDetail_KeyDown;
            KeyPreview = true;
        }

        private void FrmSalesReportMasterDetail_Load(object sender, EventArgs e)
        {
            if (IsDesignTime()) return;

            InitializeRuntimeAppearance();
            LoadLookupData();
            ResetFilters(false);
            LoadData();
        }

        private void FrmSalesReportMasterDetail_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
        }

        private bool IsDesignTime()
        {
            return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void FrmSalesReportMasterDetail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                LoadData();
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                e.Handled = true;
                ExportToExcel();
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                e.Handled = true;
                ShowGridPreview("Sales Details Report - Print Preview");
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        #endregion

        #region UI Setup & Styling
        private void InitializeRuntimeAppearance()
        {
            this.BackColor = FormBackColor;

            // Style action buttons
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportExcel);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnHideSelection);

            // Style filters
            StyleFilterCombo(ultraComboDateMode);
            StyleFilterCombo(cmbPaymentMode);
            StyleFilterCombo(cmbSalesType);
            StyleFilterCombo(cmbCustomer);
            StyleDateEditor(dtFromDate);
            StyleDateEditor(dtToDate);

            // Setup Grid
            SetupGridAppearance(ultraGridMaster);

            // Setup Footer
            InitializeGridFooter();

            // Setup Column Drag-To-Hide & Chooser
            SetupHeaderDragToHideAndColumnChooser();
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
            button.Appearance.FontData.Name = "Tahoma";
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

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = BorderBlue;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 9;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor dtEditor)
        {
            if (dtEditor == null) return;
            dtEditor.UseAppStyling = false;
            dtEditor.UseOsThemes = DefaultableBoolean.False;
            dtEditor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            dtEditor.BorderStyle = UIElementBorderStyle.Solid;
            dtEditor.Appearance.BackColor = ControlBackColor;
            dtEditor.Appearance.BorderColor = BorderBlue;
            dtEditor.Appearance.ForeColor = ControlTextColor;
            dtEditor.Appearance.FontData.Name = "Tahoma";
            dtEditor.Appearance.FontData.SizeInPoints = 9;
            dtEditor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private void SetupGridAppearance(UltraGrid grid)
        {
            if (grid == null) return;

            grid.ResetDisplayLayout();
            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;

            // Background matching Image / frmPurchaseReturn
            layout.Appearance.BackColor = FormBackColor;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Appearance.BorderColor = BorderBlue;
            layout.BorderStyle = UIElementBorderStyle.Solid;

            // Flat single-band view (no hierarchical tree +/- expansion icons)
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.GroupByBox.Hidden = true;
            layout.AutoFitStyle = AutoFitStyle.None;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.Scrollbars = Scrollbars.Both;

            // Header style matching frmPurchaseReturn
            layout.Override.HeaderStyle = HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Row selectors matching frmPurchaseReturn
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Row & Cell appearance
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;

            // Selected & Active Row
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            // Borders & Cells
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Row Sizing & Behavior
            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType = FilterUIType.FilterRow;
            layout.Override.DefaultRowHeight = 22;
            layout.Override.CellPadding = 2;
            layout.Override.CellSpacing = 0;
            layout.Override.RowSpacingBefore = 0;
            layout.Override.RowSpacingAfter = 0;

            // Sync Footer with Grid Scroll/Resize/Paint
            grid.InitializeLayout += UltraGridMaster_InitializeLayout;
            grid.Resize += (s, e) => UpdateFooterCellPositions();
            grid.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            grid.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            grid.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            grid.Paint += (s, e) => UpdateFooterCellPositions();
            grid.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            grid.AfterSortChange += (s, e) => UpdateFooterValues();
        }

        private void LoadLookupData()
        {
            // Date mode
            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("TODAY", "Today");
            ultraComboDateMode.Items.Add("THIS_WEEK", "This Week");
            ultraComboDateMode.Items.Add("THIS_MONTH", "This Month");
            ultraComboDateMode.Items.Add("THIS_YEAR", "This Year");
            ultraComboDateMode.Items.Add("RANGE", "Custom Range");
            ultraComboDateMode.Value = "THIS_MONTH";

            dtFromDate.DateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtToDate.DateTime = DateTime.Today;
            UpdateDateControlsVisibility();

            // Payment Mode
            cmbPaymentMode.Items.Clear();
            cmbPaymentMode.Items.Add("ALL", "ALL");
            cmbPaymentMode.Items.Add("Cash", "Cash");
            cmbPaymentMode.Items.Add("Card", "Card");
            cmbPaymentMode.Items.Add("Credit", "Credit");
            cmbPaymentMode.Items.Add("UPI", "UPI");
            cmbPaymentMode.Items.Add("Bank Transfer", "Bank Transfer");
            cmbPaymentMode.Value = "ALL";

            // Sales Type
            cmbSalesType.Items.Clear();
            cmbSalesType.Items.Add("ALL", "ALL");
            cmbSalesType.Items.Add("Cash Sales", "Cash Sales");
            cmbSalesType.Items.Add("Credit Sales", "Credit Sales");
            cmbSalesType.Items.Add("Return", "Return");
            cmbSalesType.Value = "ALL";

            // Customers
            try
            {
                var custGrid = _dropdowns.CustomerDDl();
                _customerOptions.Clear();
                _customerOptions.Add(new ComboItem { Text = "ALL", Value = "" });
                cmbCustomer.Items.Clear();
                cmbCustomer.Items.Add("", "ALL");
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
            catch
            {
                cmbCustomer.Items.Clear();
                cmbCustomer.Items.Add("", "ALL");
                cmbCustomer.Value = "";
            }
        }

        public void RibbonClear() => ResetFilters(true);
        public void Clear() => ResetFilters(true);

        private void ResetFilters(bool reload = true)
        {
            ultraComboDateMode.Value = "THIS_MONTH";
            dtFromDate.DateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtToDate.DateTime = DateTime.Today;
            UpdateDateControlsVisibility();

            cmbPaymentMode.Value = "ALL";
            cmbSalesType.Value = "ALL";
            cmbCustomer.Value = "";

            if (reload)
            {
                LoadData();
            }
        }

        private void UpdateDateControlsVisibility()
        {
            bool isRange = string.Equals(ultraComboDateMode.Value?.ToString(), "RANGE", StringComparison.OrdinalIgnoreCase);
            lblFromDate.Visible = isRange;
            dtFromDate.Visible = isRange;
            lblToDate.Visible = isRange;
            dtToDate.Visible = isRange;
        }
        #endregion

        #region Button & Control Handlers
        private void BtnViewGrid_Click(object sender, EventArgs e) => LoadData();

        private void BtnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Sales Report - Print Preview");
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Sales Report - Detailed Report");
        }

        private void BtnExportExcel_Click(object sender, EventArgs e) => ExportToExcel();

        private void BtnClearFilters_Click(object sender, EventArgs e) => ResetFilters(true);

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
        }

        private void UltraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            if (ultraComboDateMode.Value == null) return;
            string mode = ultraComboDateMode.Value.ToString();
            DateTime today = DateTime.Today;

            switch (mode)
            {
                case "TODAY":
                    dtFromDate.DateTime = today;
                    dtToDate.DateTime = today;
                    break;
                case "THIS_WEEK":
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dtFromDate.DateTime = today.AddDays(-diff);
                    dtToDate.DateTime = today;
                    break;
                case "THIS_MONTH":
                    dtFromDate.DateTime = new DateTime(today.Year, today.Month, 1);
                    dtToDate.DateTime = today;
                    break;
                case "THIS_YEAR":
                    dtFromDate.DateTime = new DateTime(today.Year, 1, 1);
                    dtToDate.DateTime = today;
                    break;
            }

            UpdateDateControlsVisibility();
        }
        #endregion

        #region Data Retrieval & Grid Binding (Clean Flat Report)
        private void LoadData()
        {
            if (isLoading) return;

            try
            {
                isLoading = true;
                this.Cursor = Cursors.WaitCursor;

                DateTime fromDate = dtFromDate.DateTime.Date;
                DateTime toDate = dtToDate.DateTime.Date;
                string customerFilter = cmbCustomer.Text?.Trim() ?? "";
                if (string.Equals(customerFilter, "ALL", StringComparison.OrdinalIgnoreCase)) customerFilter = "";
                string paymentFilter = cmbPaymentMode.Value?.ToString() ?? "ALL";
                string salesTypeFilter = cmbSalesType.Value?.ToString() ?? "ALL";

                // Fetch bills from repository
                var bills = _reportRepository.GetSalesBills(fromDate, toDate, 0) ?? new List<SalesReportMaster>();

                // Filter bills
                var filteredBills = bills.AsEnumerable();

                if (!string.IsNullOrEmpty(customerFilter))
                {
                    filteredBills = filteredBills.Where(b => (b.CustomerName ?? "").IndexOf(customerFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.Equals(paymentFilter, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    filteredBills = filteredBills.Where(b => (b.CashMode ?? "").IndexOf(paymentFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                            (b.PaymodeName ?? "").IndexOf(paymentFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.Equals(salesTypeFilter, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    filteredBills = filteredBills.Where(b => (b.PaymodeName ?? "").IndexOf(salesTypeFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var finalBills = filteredBills.ToList();

                // Build a clean single-table flat DataTable for the grid
                DataTable dt = new DataTable("SalesMaster");
                dt.Columns.Add("SlNo", typeof(int));
                dt.Columns.Add("BillNo", typeof(int));
                dt.Columns.Add("BillDate", typeof(DateTime));
                dt.Columns.Add("CustomerName", typeof(string));
                dt.Columns.Add("PaymodeName", typeof(string));
                dt.Columns.Add("SubTotal", typeof(decimal));
                dt.Columns.Add("TaxAmt", typeof(decimal));
                dt.Columns.Add("NetAmount", typeof(decimal));
                dt.Columns.Add("Profit", typeof(decimal));

                int serial = 1;
                foreach (var b in finalBills)
                {
                    DataRow row = dt.NewRow();
                    row["SlNo"] = serial++;
                    row["BillNo"] = b.BillNo;
                    row["BillDate"] = b.BillDate;
                    row["CustomerName"] = b.CustomerName ?? "";
                    row["PaymodeName"] = b.PaymodeName ?? "";
                    row["SubTotal"] = Convert.ToDecimal(b.SubTotal);
                    row["TaxAmt"] = Convert.ToDecimal(b.TaxAmt);
                    row["NetAmount"] = Convert.ToDecimal(b.NetAmount);
                    row["Profit"] = Convert.ToDecimal(b.Profit);
                    dt.Rows.Add(row);
                }

                _currentDataTable = dt;
                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataSource = dt;

                ApplyUserHiddenColumns();

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales details report: {ex.Message}", "Sales Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void UltraGridMaster_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                if (e.Layout.Bands.Count == 0) return;

                UltraGridBand band = e.Layout.Bands[0];
                int pos = 0;

                if (band.Columns.Exists("SlNo"))
                {
                    band.Columns["SlNo"].Header.Caption = "S.No";
                    band.Columns["SlNo"].Width = 45;
                    band.Columns["SlNo"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["SlNo"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["SlNo"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("BillNo"))
                {
                    band.Columns["BillNo"].Header.Caption = "Bill No";
                    band.Columns["BillNo"].Width = 85;
                    band.Columns["BillNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["BillNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                    band.Columns["BillNo"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["BillNo"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["BillNo"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("BillDate"))
                {
                    band.Columns["BillDate"].Header.Caption = "Bill Date";
                    band.Columns["BillDate"].Format = "dd-MM-yyyy hh:mm tt";
                    band.Columns["BillDate"].Width = 145;
                    band.Columns["BillDate"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["BillDate"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["BillDate"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("CustomerName"))
                {
                    band.Columns["CustomerName"].Header.Caption = "Customer";
                    band.Columns["CustomerName"].Width = 240;
                    band.Columns["CustomerName"].CellAppearance.TextHAlign = HAlign.Left;
                    band.Columns["CustomerName"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["CustomerName"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("PaymodeName"))
                {
                    band.Columns["PaymodeName"].Header.Caption = "Pay Mode";
                    band.Columns["PaymodeName"].Width = 95;
                    band.Columns["PaymodeName"].CellAppearance.TextHAlign = HAlign.Center;
                    band.Columns["PaymodeName"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["PaymodeName"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("SubTotal"))
                {
                    band.Columns["SubTotal"].Header.Caption = "Sub Total";
                    band.Columns["SubTotal"].Format = "₹ #,##0.00";
                    band.Columns["SubTotal"].Width = 120;
                    band.Columns["SubTotal"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["SubTotal"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["SubTotal"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                    band.Columns["SubTotal"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("TaxAmt"))
                {
                    band.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                    band.Columns["TaxAmt"].Format = "₹ #,##0.00";
                    band.Columns["TaxAmt"].Width = 115;
                    band.Columns["TaxAmt"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["TaxAmt"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
                    band.Columns["TaxAmt"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("NetAmount"))
                {
                    band.Columns["NetAmount"].Header.Caption = "Net Amount";
                    band.Columns["NetAmount"].Format = "₹ #,##0.00";
                    band.Columns["NetAmount"].Width = 135;
                    band.Columns["NetAmount"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["NetAmount"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                    band.Columns["NetAmount"].Header.VisiblePosition = pos++;
                }
                if (band.Columns.Exists("Profit"))
                {
                    band.Columns["Profit"].Header.Caption = "Profit";
                    band.Columns["Profit"].Format = "₹ #,##0.00";
                    band.Columns["Profit"].Width = 120;
                    band.Columns["Profit"].CellAppearance.TextHAlign = HAlign.Right;
                    band.Columns["Profit"].CellAppearance.TextVAlign = VAlign.Middle;
                    band.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                    band.Columns["Profit"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
                    band.Columns["Profit"].Header.VisiblePosition = pos++;
                }

                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeLayout failed: {ex.Message}");
            }
        }

        private void ApplyUserHiddenColumns()
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;
            var band = ultraGridMaster.DisplayLayout.Bands[0];
            foreach (var key in userHiddenColumnKeys)
            {
                if (band.Columns.Exists(key))
                {
                    band.Columns[key].Hidden = true;
                }
            }
        }
        #endregion

        #region UltraPanelGridFooter Dynamic Alignment & Calculation (Parity with frmPurchaseReturn)
        private void InitializeGridFooter()
        {
            if (gridFooterPanel == null) return;

            gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
            gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
            gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
            gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
            gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
            gridFooterPanel.Height = 26;
            gridFooterPanel.Visible = true;

            // Set default aggregations
            foreach (var col in summaryDefaultNumericColumns)
            {
                if (!_columnAggregations.ContainsKey(col))
                    _columnAggregations[col] = "Sum";
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (gridFooterPanel == null) return;
            gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int xOffset = ultraGridMaster.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? ultraGridMaster.DisplayLayout.Override.RowSelectorWidth
                : 0;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = (column.CellAppearance.TextHAlign == HAlign.Right || IsNumericColumn(column))
                        ? ContentAlignment.MiddleRight
                        : ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                gridFooterPanel.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = summaryDefaultNumericColumns.Contains(column.Key) ? "Sum" : "None";
                }

                xOffset += column.Width;
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;

            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                e.Graphics.DrawLine(borderPen, lbl.Width - 1, 0, lbl.Width - 1, lbl.Height);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = ultraGridMaster.DisplayLayout.Bands.Count > 0 &&
                             ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum") { Tag = "Sum", Enabled = isNumeric };
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min") { Tag = "Min" };
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max") { Tag = "Max" };
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count") { Tag = "Count" };
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average") { Tag = "Avg", Enabled = isNumeric };
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None") { Tag = "None" };
            itemNone.Click += FooterContextMenu_Click;

            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAverage);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);

            menu.Opening += (sender, e) =>
            {
                string currentAggregation = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : "None";

                foreach (ToolStripItem menuItem in menu.Items)
                {
                    if (menuItem is ToolStripMenuItem toolStripMenuItem && toolStripMenuItem.Tag != null)
                    {
                        toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Owner is ContextMenuStrip menu && menu.Tag != null && item.Tag != null)
            {
                string columnKey = menu.Tag.ToString();
                string aggregation = item.Tag.ToString();
                _columnAggregations[columnKey] = aggregation;
                UpdateFooterValues();
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || gridFooterPanel == null)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGridMaster.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? ultraGridMaster.DisplayLayout.Override.RowSelectorWidth
                : 0;
            int scrollOffset = 0;
            if (ultraGridMaster.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGridMaster.ActiveColScrollRegion.Position;
            }

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
                footerLabel.Height = gridFooterPanel.Height;
                footerLabel.Visible = (left + width > 0 && left < gridFooterPanel.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0) return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!_columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, _columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, _columnAggregations[columnKey], result);

                footerLabel.Text = displayValue;
                footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (ultraGridMaster.Rows == null) yield break;
            foreach (UltraGridRow row in ultraGridMaster.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Sum(value => value.Value);
                case "Min":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderBy(value => value)
                        .FirstOrDefault();
                case "Max":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value)
                        .Where(HasCellValue)
                        .Cast<IComparable>()
                        .OrderByDescending(value => value)
                        .FirstOrDefault();
                case "Count":
                    return visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Count(row => HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                case "Average":
                    var values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count > 0 ? (decimal?)values.Average() : null;
                default:
                    return null;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
        }

        private static decimal? GetNumericValue(object value)
        {
            if (!HasCellValue(value))
                return null;

            decimal result;
            if (decimal.TryParse(value.ToString(), out result))
                return result;

            return null;
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            bool isCurrency = columnKey == "SubTotal" || columnKey == "TaxAmt" || columnKey == "NetAmount" || columnKey == "Profit";

            if (aggregation == "Count")
                return result.ToString();

            if (result is decimal decValue)
            {
                string numStr = isCurrency ? ("₹" + decValue.ToString("N2")) : decValue.ToString("N2");
                if (aggregation == "Avg" || aggregation == "Average") return $"Avg: {numStr}";
                if (aggregation == "Min") return $"Min: {numStr}";
                if (aggregation == "Max") return $"Max: {numStr}";
                return numStr;
            }

            if (result is DateTime dtValue)
            {
                return dtValue.ToString("dd-MM-yyyy");
            }

            return result.ToString();
        }

        private bool IsNumericColumn(UltraGridColumn col)
        {
            if (col == null) return false;
            return summaryDefaultNumericColumns.Contains(col.Key) ||
                   col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                   col.DataType == typeof(int) || col.DataType == typeof(float) ||
                   col.DataType == typeof(long);
        }
        #endregion

        #region Column Chooser & Drag-Down to Hide (Parity with frmPurchaseReturn & frmStockReport)
        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillEllipse(bgBrush, 4, 4, 24, 24);
                    }

                    using (Pen whitePen = new Pen(Color.White, 3.5f))
                    {
                        whitePen.StartCap = LineCap.Round;
                        whitePen.EndCap = LineCap.Round;
                        g.DrawLine(whitePen, 11, 11, 21, 21);
                        g.DrawLine(whitePen, 21, 11, 11, 21);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    return new Cursor(hIcon);
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            ultraGridMaster.AllowDrop = true;
            ultraGridMaster.MouseDown += Grid_MouseDown;
            ultraGridMaster.MouseMove += Grid_MouseMove;
            ultraGridMaster.MouseUp += Grid_MouseUp;
            ultraGridMaster.DragOver += Grid_DragOver;
            ultraGridMaster.DragDrop += Grid_DragDrop;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = ultraGridMaster.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null)
                {
                    ShowHeaderContextMenu(headerUI.Header.Column, pt);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = ultraGridMaster.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null)
                {
                    columnBeingDragged = headerUI.Header.Column;
                    headerDragStartPoint = pt;
                    isDraggingHeaderToHide = false;
                }
                else
                {
                    columnBeingDragged = null;
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                int deltaY = e.Y - headerDragStartPoint.Y;
                int deltaX = Math.Abs(e.X - headerDragStartPoint.X);

                if (deltaY > 12 && deltaY > deltaX)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                    headerToolTip.Show("Drag down to hide column", ultraGridMaster, e.X + 15, e.Y + 15, 500);
                }
                else
                {
                    isDraggingHeaderToHide = false;
                    Cursor.Current = Cursors.Default;
                    headerToolTip.Hide(ultraGridMaster);
                }
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                if (isDraggingHeaderToHide)
                {
                    HideColumn(columnBeingDragged);
                    headerToolTip.Hide(ultraGridMaster);
                }
                columnBeingDragged = null;
                isDraggingHeaderToHide = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = ultraGridMaster.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = ultraGridMaster.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (!col.Hidden)
                {
                    UIElement hUI = col.Header.GetUIElement();
                    if (hUI != null && pt.X >= hUI.Rect.Left && pt.X <= hUI.Rect.Right)
                    {
                        return col.Header.VisiblePosition;
                    }
                }
            }

            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null) return;
            userHiddenColumnKeys.Add(col.Key);
            col.Hidden = true;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point location)
        {
            if (col == null) return;
            ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(ultraGridMaster, location);
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooser();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization (Field Chooser)",
                Size = new Size(240, 320),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 244, 248),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                columnChooserForm.Hide();
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                ItemHeight = 34,
                IntegralHeight = false
            };

            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(txtColumnSearch);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnChooserListBox != null)
            {
                int index = columnChooserListBox.IndexFromPoint(e.Location);
                if (index >= 0 && index < columnChooserListBox.Items.Count)
                {
                    if (columnChooserListBox.Items[index] is ColumnChooserItem item)
                    {
                        columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                    }
                }
            }
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            string filterText = txtColumnSearch?.Text?.Trim() ?? "";

            foreach (UltraGridColumn col in band.Columns)
            {
                if (col.Hidden)
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filterText) || caption.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey);
            }
        }

        private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
        {
            userHiddenColumnKeys.Remove(columnKey);
            if (ultraGridMaster.DisplayLayout.Bands.Count > 0 && ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey];
                col.Hidden = false;
                if (targetVisiblePosition.HasValue)
                {
                    col.Header.VisiblePosition = targetVisiblePosition.Value;
                }
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (ultraGridMaster.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = false;
            }
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
                columnChooserForm.BringToFront();
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                return;

            if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                return;

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 121, 211)))
            using (GraphicsPath path = RoundedRect(rect, 4))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(item.DisplayText, textFont, textBrush, rect, sf);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

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

        #region Export & Print Preview
        private void ExportToExcel()
        {
            try
            {
                if (ultraGridMaster.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV File (*.csv)|*.csv";
                    sfd.FileName = $"SalesDetailsReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var visibleCols = ultraGridMaster.DisplayLayout.Bands[0].Columns
                            .Cast<UltraGridColumn>()
                            .Where(c => !c.Hidden)
                            .OrderBy(c => c.Header.VisiblePosition)
                            .ToList();

                        var sb = new StringBuilder();
                        sb.AppendLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption}\"")));

                        foreach (var row in ultraGridMaster.Rows.GetFilteredInNonGroupByRows())
                        {
                            var cells = visibleCols.Select(c =>
                            {
                                object v = row.Cells[c.Key].Value;
                                string s = v != null && v != DBNull.Value ? v.ToString() : "";
                                if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                                {
                                    s = "\"" + s.Replace("\"", "\"\"") + "\"";
                                }
                                return s;
                            });
                            sb.AppendLine(string.Join(",", cells));
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Sales report exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowGridPreview(string title)
        {
            try
            {
                if (ultraGridMaster.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to print preview.", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ultraGridMaster.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print preview error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}