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
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesReportMasterDetail : Form
    {
        #region Private Fields
        private SalesReportRepository _reportRepository;
        private Dropdowns _dropdowns;
        private readonly List<ComboItem> _customerOptions;
        private readonly Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private string currentSummaryType = "Sum";

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SubTotal",
            "TaxAmt",
            "TaxAmount",
            "NetAmount",
            "Profit",
            "TotalAmount"
        };

        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private bool summaryFooterInitialized;
        private readonly System.Windows.Forms.ToolTip gridToolTip = new System.Windows.Forms.ToolTip();
        private bool isLoading = false;
        private bool isLayoutLoading = false;

        private string LayoutXmlPath => Path.Combine(Application.StartupPath, "SalesDetailsReport_GridLayout.xml");
        private string LayoutStatePath => Path.Combine(Application.StartupPath, "SalesDetailsReport_LayoutState.txt");
        #endregion

        #region Helper Classes
        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        private sealed class ColumnItem
        {
            public ColumnItem(string columnKey, string displayText, int bandIndex = 0)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
                BandIndex = bandIndex;
            }

            public string ColumnKey { get; }
            public string DisplayText { get; }
            public int BandIndex { get; }

            public override string ToString()
            {
                return DisplayText;
            }
        }
        #endregion

        #region Constructor
        public frmSalesReportMasterDetail()
        {
            _customerOptions = new List<ComboItem>();

            InitializeComponent();
            Load += FrmSalesReportMasterDetail_Load;
            FormClosing += FrmSalesReportMasterDetail_FormClosing;
            FormClosed += FrmSalesReportMasterDetail_FormClosed;
        }
        #endregion

        #region Form Lifecycle Events
        private void FrmSalesReportMasterDetail_Load(object sender, EventArgs e)
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

        private void FrmSalesReportMasterDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayout();
        }

        private void FrmSalesReportMasterDetail_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private bool IsDesignTime()
        {
            return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
        #endregion

        #region UI Setup & Styling
        private void InitializeRuntimeAppearance()
        {
            ConfigureButton(btnViewGrid, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
            ConfigureButton(btnPreviewGrid, Color.FromArgb(94, 116, 202), Color.FromArgb(121, 141, 222));
            ConfigureButton(btnPreviewReport, Color.FromArgb(108, 92, 231), Color.FromArgb(135, 120, 245));
            ConfigureButton(btnExportExcel, Color.FromArgb(46, 125, 50), Color.FromArgb(76, 175, 80));
            ConfigureButton(btnColumnChooser, Color.FromArgb(90, 110, 160), Color.FromArgb(115, 135, 185));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            ConfigureGridAppearance(ultraGridMaster);
            InitializeSummaryFooterPanel();
            InitializeGridContextMenuAndDragDrop();
            ReflowSummaryCards();
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
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;

            // Hierarchical Master-Detail setup
            targetGrid.DisplayLayout.ViewStyleBand = ViewStyleBand.Vertical;
            targetGrid.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;

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
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 32;
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
            targetGrid.DisplayLayout.Override.SummaryDisplayArea = SummaryDisplayAreas.None;
            targetGrid.DisplayLayout.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;
            targetGrid.AllowDrop = true;

            // Events
            targetGrid.InitializeLayout += UltraGridMaster_InitializeLayout;
            targetGrid.AfterRowExpanded += UltraGridMaster_AfterRowExpanded;
            targetGrid.AfterRowCollapsed += UltraGridMaster_AfterRowCollapsed;
        }

        private void InitializeSummaryFooterPanel()
        {
            if (summaryFooterInitialized || gridFooterPanel == null || ultraGridMaster == null)
            {
                return;
            }

            // Style gridFooterPanel to match SalesInvoice
            gridFooterPanel.Appearance.BackColor = Color.FromArgb(0, 122, 204);
            gridFooterPanel.Appearance.BackColor2 = Color.FromArgb(0, 102, 184);
            gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            gridFooterPanel.Appearance.BorderColor = Color.FromArgb(0, 100, 182);
            gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
            gridFooterPanel.Visible = true;

            if (lblCount != null)
            {
                lblCount.Appearance.ForeColor = Color.White;
                lblCount.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblCount.Appearance.BackColor = Color.Transparent;
                lblCount.Location = new Point(8, 4);
                lblCount.Size = new Size(180, 22);
            }

            // Set default aggregations for Master Band numeric columns
            foreach (var col in summaryDefaultNumericColumns)
            {
                if (!columnAggregations.ContainsKey(col))
                    columnAggregations[col] = "Sum";
            }

            // Create panel-wide context menu
            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            gridFooterPanel.ClientArea.ContextMenuStrip = panelMenu;
            gridFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctrl = gridFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                        panelMenu.Show(gridFooterPanel.ClientArea, e.Location);
                }
            };

            // Wire up alignment & resize events
            gridFooterPanel.Paint += (s, e) => AlignSummaryLabels();
            gridFooterPanel.Resize += (s, e) => AlignSummaryLabels();
            ultraGridMaster.AfterColPosChanged += (s, e) =>
            {
                AlignSummaryLabels();
                if (!isLayoutLoading && !isLoading)
                {
                    SaveGridLayout();
                }
            };
            ultraGridMaster.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGridMaster.AfterRowFilterChanged += (s, e) => { UpdateFooterValues(); AlignSummaryLabels(); };
            ultraGridMaster.SizeChanged += (s, e) => AlignSummaryLabels();

            summaryFooterInitialized = true;
        }
        #endregion

        #region Helper: Column Customization Eligibility Check
        /// <summary>
        /// Determines if a grid column is eligible for Column Chooser, hiding, or drag-and-drop.
        /// Prevents chaptered relation columns, internal IDs, and essential columns from throwing exceptions.
        /// </summary>
        private bool IsCustomizableColumn(UltraGridColumn col, int bandIndex)
        {
            if (col == null) return false;
            if (col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            if (string.Equals(col.Key, "BillDetails", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "SlNo", StringComparison.OrdinalIgnoreCase)) return false;
            if (bandIndex > 0 && string.Equals(col.Key, "BillNo", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
        #endregion

        #region Lookups and Filters
        private void LoadLookupData()
        {
            _reportRepository = new SalesReportRepository();
            _dropdowns = new Dropdowns();

            // Date Mode
            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("RANGE", "By Range");
            ultraComboDateMode.Items.Add("ALL", "ALL");
            ultraComboDateMode.Value = "RANGE";
            dtFromDate.DateTime = DateTime.Today;
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
            ultraComboDateMode.Value = "RANGE";
            dtFromDate.DateTime = DateTime.Today;
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
        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
        }

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
            if (!isLoading) LoadData();
        }
        #endregion

        #region Data Loading & Hierarchical Binding (Master-Detail with + expansion)
        private void LoadData()
        {
            if (IsDesignTime())
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            try
            {
                isLoading = true;

                bool isRange = string.Equals(ultraComboDateMode.Value?.ToString(), "RANGE", StringComparison.OrdinalIgnoreCase);
                DateTime fromDate = isRange ? dtFromDate.DateTime.Date : new DateTime(2000, 1, 1);
                DateTime toDate = isRange ? dtToDate.DateTime.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : DateTime.Today.AddDays(1);

                string paymentFilter = cmbPaymentMode.Value?.ToString() ?? "ALL";
                string salesTypeFilter = cmbSalesType.Value?.ToString() ?? "ALL";
                string customerFilter = cmbCustomer.Text?.Trim() ?? "";
                if (customerFilter.Equals("ALL", StringComparison.OrdinalIgnoreCase)) customerFilter = "";

                // Fetch Bills from repository
                List<SalesReportMaster> bills = _reportRepository.GetSalesBills(fromDate, toDate, SessionContext.BranchId);

                // Filter master bills
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

                // Create Master-Detail DataSet
                DataSet dsHierarchical = new DataSet("SalesMasterDetail");

                // 1. Master Table (Bills)
                DataTable masterTable = new DataTable("SalesMaster");
                masterTable.Columns.Add("SlNo", typeof(int));
                masterTable.Columns.Add("BillNo", typeof(int));
                masterTable.Columns.Add("BillDate", typeof(DateTime));
                masterTable.Columns.Add("CustomerName", typeof(string));
                masterTable.Columns.Add("PaymodeName", typeof(string));
                masterTable.Columns.Add("SubTotal", typeof(decimal));
                masterTable.Columns.Add("TaxAmt", typeof(decimal));
                masterTable.Columns.Add("NetAmount", typeof(decimal));
                masterTable.Columns.Add("Profit", typeof(decimal));

                // 2. Detail Table (Bill Items)
                DataTable detailTable = new DataTable("SalesDetails");
                detailTable.Columns.Add("DetailID", typeof(int));
                detailTable.Columns["DetailID"].AutoIncrement = true;
                detailTable.Columns["DetailID"].AutoIncrementSeed = 1;
                detailTable.Columns["DetailID"].AutoIncrementStep = 1;

                detailTable.Columns.Add("BillNo", typeof(int));
                detailTable.Columns.Add("SlNo", typeof(int));
                detailTable.Columns.Add("ItemName", typeof(string));
                detailTable.Columns.Add("Barcode", typeof(string));
                detailTable.Columns.Add("Unit", typeof(string));
                detailTable.Columns.Add("Packing", typeof(string));
                detailTable.Columns.Add("Qty", typeof(decimal));
                detailTable.Columns.Add("UnitPrice", typeof(decimal));
                detailTable.Columns.Add("Amount", typeof(decimal));
                detailTable.Columns.Add("MarginPer", typeof(decimal));
                detailTable.Columns.Add("Profit", typeof(decimal));
                detailTable.Columns.Add("TaxPer", typeof(decimal));
                detailTable.Columns.Add("TaxAmt", typeof(decimal));
                detailTable.Columns.Add("TotalAmount", typeof(decimal));

                decimal totalQty = 0;
                decimal totalSubTotal = 0;
                decimal totalTax = 0;
                decimal totalNet = 0;
                decimal totalProfit = 0;
                int masterSerial = 1;

                foreach (var bill in finalBills)
                {
                    DataRow mRow = masterTable.NewRow();
                    mRow["SlNo"] = masterSerial++;
                    mRow["BillNo"] = bill.BillNo;
                    mRow["BillDate"] = bill.BillDate;
                    mRow["CustomerName"] = bill.CustomerName ?? "";
                    mRow["PaymodeName"] = bill.PaymodeName ?? "";
                    mRow["SubTotal"] = bill.SubTotal;
                    mRow["TaxAmt"] = bill.TaxAmt;
                    mRow["NetAmount"] = bill.NetAmount;
                    mRow["Profit"] = bill.Profit;
                    masterTable.Rows.Add(mRow);

                    totalSubTotal += Convert.ToDecimal(bill.SubTotal);
                    totalTax += Convert.ToDecimal(bill.TaxAmt);
                    totalNet += Convert.ToDecimal(bill.NetAmount);
                    totalProfit += Convert.ToDecimal(bill.Profit);

                    // Fetch detail items for this bill
                    var details = _reportRepository.GetSalesReportDetails(bill.BillNo, fromDate, toDate);
                    if (details?.Details != null && details.Details.Count > 0)
                    {
                        int detailSerial = 1;
                        foreach (var d in details.Details)
                        {
                            DataRow dRow = detailTable.NewRow();
                            dRow["BillNo"] = bill.BillNo;
                            dRow["SlNo"] = detailSerial++;
                            dRow["ItemName"] = d.ItemName ?? "";
                            dRow["Barcode"] = d.Barcode ?? "";
                            dRow["Unit"] = d.Unit ?? "";
                            dRow["Packing"] = d.Packing ?? "";
                            dRow["Qty"] = d.Qty;
                            dRow["UnitPrice"] = d.UnitPrice;
                            dRow["Amount"] = d.Amount;
                            dRow["MarginPer"] = d.MarginPer;
                            dRow["Profit"] = d.Profit;
                            dRow["TaxPer"] = d.TaxPer;
                            dRow["TaxAmt"] = d.TaxAmt;
                            dRow["TotalAmount"] = d.TotalAmount;
                            detailTable.Rows.Add(dRow);

                            totalQty += Convert.ToDecimal(d.Qty);
                        }
                    }
                }

                dsHierarchical.Tables.Add(masterTable);
                dsHierarchical.Tables.Add(detailTable);

                // Create relationship
                DataRelation relation = new DataRelation(
                    "BillDetails",
                    masterTable.Columns["BillNo"],
                    detailTable.Columns["BillNo"],
                    false
                );
                dsHierarchical.Relations.Add(relation);

                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataSource = dsHierarchical;

                LoadGridLayout();

                // Update Bottom Summary Cards
                lblCardBillsValue.Text = $"{finalBills.Count:N0}";
                lblCardQtyValue.Text = $"{totalQty:N2}";
                lblCardSubTotalValue.Text = $"₹ {totalSubTotal:N2}";
                lblCardTaxValue.Text = $"₹ {totalTax:N2}";
                lblCardNetValue.Text = $"₹ {totalNet:N2}";
                lblCardProfitValue.Text = $"₹ {totalProfit:N2}";

                lblCount.Text = $"Total Bills: {finalBills.Count:N0}";
                UpdateSummaryFooter();
                RefreshColumnChooserList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales master-detail report: {ex.Message}", "Sales Report Error",
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
                // Ensure chaptered / relation columns are completely excluded from chooser
                foreach (var band in e.Layout.Bands)
                {
                    foreach (var col in band.Columns)
                    {
                        if (col.IsChaptered || string.Equals(col.Key, "BillDetails", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase))
                        {
                            col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                        }
                    }
                }

                // Master Band (Band 0 - Bills)
                if (e.Layout.Bands.Count > 0)
                {
                    ConfigureMasterBandColumns(e.Layout.Bands[0]);
                }

                // Detail Band (Band 1 - Items)
                if (e.Layout.Bands.Count > 1)
                {
                    ConfigureDetailBandColumns(e.Layout.Bands[1]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeLayout failed: {ex.Message}");
            }
        }

        private void ConfigureMasterBandColumns(UltraGridBand masterBand)
        {
            int pos = 0;

            if (masterBand.Columns.Exists("SlNo"))
            {
                masterBand.Columns["SlNo"].Header.Caption = "S.No";
                masterBand.Columns["SlNo"].Width = 55;
                masterBand.Columns["SlNo"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["SlNo"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("BillNo"))
            {
                masterBand.Columns["BillNo"].Header.Caption = "Bill No";
                masterBand.Columns["BillNo"].Width = 85;
                masterBand.Columns["BillNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["BillNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                masterBand.Columns["BillNo"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["BillNo"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("BillDate"))
            {
                masterBand.Columns["BillDate"].Header.Caption = "Bill Date";
                masterBand.Columns["BillDate"].Format = "dd/MM/yyyy hh:mm tt";
                masterBand.Columns["BillDate"].Width = 145;
                masterBand.Columns["BillDate"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("CustomerName"))
            {
                masterBand.Columns["CustomerName"].Header.Caption = "Customer";
                masterBand.Columns["CustomerName"].Width = 220;
                masterBand.Columns["CustomerName"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("PaymodeName"))
            {
                masterBand.Columns["PaymodeName"].Header.Caption = "Pay Mode";
                masterBand.Columns["PaymodeName"].Width = 90;
                masterBand.Columns["PaymodeName"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["PaymodeName"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("SubTotal"))
            {
                masterBand.Columns["SubTotal"].Header.Caption = "Sub Total";
                masterBand.Columns["SubTotal"].Format = "₹ #,##0.00";
                masterBand.Columns["SubTotal"].Width = 125;
                masterBand.Columns["SubTotal"].CellAppearance.TextHAlign = HAlign.Right;
                masterBand.Columns["SubTotal"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                masterBand.Columns["SubTotal"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("TaxAmt"))
            {
                masterBand.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                masterBand.Columns["TaxAmt"].Format = "₹ #,##0.00";
                masterBand.Columns["TaxAmt"].Width = 115;
                masterBand.Columns["TaxAmt"].CellAppearance.TextHAlign = HAlign.Right;
                masterBand.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
                masterBand.Columns["TaxAmt"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("NetAmount"))
            {
                masterBand.Columns["NetAmount"].Header.Caption = "Net Amount";
                masterBand.Columns["NetAmount"].Format = "₹ #,##0.00";
                masterBand.Columns["NetAmount"].Width = 135;
                masterBand.Columns["NetAmount"].CellAppearance.TextHAlign = HAlign.Right;
                masterBand.Columns["NetAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["NetAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                masterBand.Columns["NetAmount"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("Profit"))
            {
                masterBand.Columns["Profit"].Header.Caption = "Profit";
                masterBand.Columns["Profit"].Format = "₹ #,##0.00";
                masterBand.Columns["Profit"].Width = 120;
                masterBand.Columns["Profit"].CellAppearance.TextHAlign = HAlign.Right;
                masterBand.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["Profit"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
                masterBand.Columns["Profit"].Header.VisiblePosition = pos++;
            }
        }

        private void ConfigureDetailBandColumns(UltraGridBand detailBand)
        {
            detailBand.Header.Caption = "Item Details";
            detailBand.HeaderVisible = true;
            detailBand.Header.Appearance.BackColor = Color.FromArgb(70, 90, 120);
            detailBand.Header.Appearance.ForeColor = Color.White;
            detailBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;

            // Hide internal keys and exclude from column chooser
            if (detailBand.Columns.Exists("DetailID"))
            {
                detailBand.Columns["DetailID"].Hidden = true;
                detailBand.Columns["DetailID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }
            if (detailBand.Columns.Exists("BillNo"))
            {
                detailBand.Columns["BillNo"].Hidden = true;
                detailBand.Columns["BillNo"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }

            int pos = 0;
            if (detailBand.Columns.Exists("SlNo"))
            {
                detailBand.Columns["SlNo"].Header.Caption = "S.No";
                detailBand.Columns["SlNo"].Width = 45;
                detailBand.Columns["SlNo"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["SlNo"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("ItemName"))
            {
                detailBand.Columns["ItemName"].Header.Caption = "Item Name";
                detailBand.Columns["ItemName"].Width = 200;
                detailBand.Columns["ItemName"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["ItemName"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Barcode"))
            {
                detailBand.Columns["Barcode"].Header.Caption = "Barcode";
                detailBand.Columns["Barcode"].Width = 85;
                detailBand.Columns["Barcode"].CellAppearance.ForeColor = Color.FromArgb(84, 110, 122);
                detailBand.Columns["Barcode"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Unit"))
            {
                detailBand.Columns["Unit"].Header.Caption = "Unit";
                detailBand.Columns["Unit"].Width = 55;
                detailBand.Columns["Unit"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["Unit"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Packing"))
            {
                detailBand.Columns["Packing"].Header.Caption = "Packing";
                detailBand.Columns["Packing"].Width = 65;
                detailBand.Columns["Packing"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["Packing"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Qty"))
            {
                detailBand.Columns["Qty"].Header.Caption = "Quantity";
                detailBand.Columns["Qty"].Format = "0.00";
                detailBand.Columns["Qty"].Width = 80;
                detailBand.Columns["Qty"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Qty"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("UnitPrice"))
            {
                detailBand.Columns["UnitPrice"].Header.Caption = "Unit Price";
                detailBand.Columns["UnitPrice"].Format = "₹ #,##0.00";
                detailBand.Columns["UnitPrice"].Width = 90;
                detailBand.Columns["UnitPrice"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["UnitPrice"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Amount"))
            {
                detailBand.Columns["Amount"].Header.Caption = "Amount";
                detailBand.Columns["Amount"].Format = "₹ #,##0.00";
                detailBand.Columns["Amount"].Width = 105;
                detailBand.Columns["Amount"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(13, 71, 161);
                detailBand.Columns["Amount"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("MarginPer"))
            {
                detailBand.Columns["MarginPer"].Header.Caption = "Margin %";
                detailBand.Columns["MarginPer"].Format = "0.00 %";
                detailBand.Columns["MarginPer"].Width = 75;
                detailBand.Columns["MarginPer"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["MarginPer"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Profit"))
            {
                detailBand.Columns["Profit"].Header.Caption = "Profit";
                detailBand.Columns["Profit"].Format = "₹ #,##0.00";
                detailBand.Columns["Profit"].Width = 105;
                detailBand.Columns["Profit"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Profit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Profit"].CellAppearance.ForeColor = Color.FromArgb(22, 160, 133);
                detailBand.Columns["Profit"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("TaxPer"))
            {
                detailBand.Columns["TaxPer"].Header.Caption = "Tax %";
                detailBand.Columns["TaxPer"].Format = "0.00 %";
                detailBand.Columns["TaxPer"].Width = 70;
                detailBand.Columns["TaxPer"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["TaxPer"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("TaxAmt"))
            {
                detailBand.Columns["TaxAmt"].Header.Caption = "Tax Amount";
                detailBand.Columns["TaxAmt"].Format = "₹ #,##0.00";
                detailBand.Columns["TaxAmt"].Width = 105;
                detailBand.Columns["TaxAmt"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["TaxAmt"].CellAppearance.ForeColor = Color.FromArgb(211, 84, 0);
                detailBand.Columns["TaxAmt"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("TotalAmount"))
            {
                detailBand.Columns["TotalAmount"].Header.Caption = "Total Amount";
                detailBand.Columns["TotalAmount"].Format = "₹ #,##0.00";
                detailBand.Columns["TotalAmount"].Width = 115;
                detailBand.Columns["TotalAmount"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["TotalAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["TotalAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                detailBand.Columns["TotalAmount"].Header.VisiblePosition = pos++;
            }
        }

        private void UltraGridMaster_AfterRowExpanded(object sender, RowEventArgs e)
        {
            AlignSummaryLabels();
        }

        private void UltraGridMaster_AfterRowCollapsed(object sender, RowEventArgs e)
        {
            AlignSummaryLabels();
        }
        #endregion

        #region Grid Layout Persistence (Save / Load across sessions)
        private void SaveGridLayout()
        {
            if (isLayoutLoading || isLoading) return;
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            try
            {
                // 1. Save standard Infragistics XML layout
                ultraGridMaster.DisplayLayout.SaveAsXml(LayoutXmlPath, PropertyCategories.All);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveAsXml failed: {ex.Message}");
            }

            try
            {
                // 2. Save explicit plain text state for robust restoration across both bands
                var lines = new List<string>();

                for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
                {
                    var band = ultraGridMaster.DisplayLayout.Bands[b];
                    lines.Add($"[BAND_{b}_HIDDEN]");
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (col.Hidden && IsCustomizableColumn(col, b))
                        {
                            lines.Add(col.Key);
                        }
                    }

                    lines.Add($"[BAND_{b}_WIDTHS]");
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.IsChaptered)
                        {
                            lines.Add($"{col.Key}={col.Width}");
                        }
                    }

                    lines.Add($"[BAND_{b}_POSITIONS]");
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        if (!col.IsChaptered)
                        {
                            lines.Add($"{col.Key}={col.Header.VisiblePosition}");
                        }
                    }
                }

                lines.Add("[AGGREGATIONS]");
                foreach (var kvp in columnAggregations)
                {
                    lines.Add($"{kvp.Key}={kvp.Value}");
                }

                File.WriteAllLines(LayoutStatePath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save LayoutState failed: {ex.Message}");
            }
        }

        private void LoadGridLayout()
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            try
            {
                isLayoutLoading = true;

                // 1. If XML exists, load it
                if (File.Exists(LayoutXmlPath))
                {
                    try
                    {
                        ultraGridMaster.DisplayLayout.LoadFromXml(LayoutXmlPath, PropertyCategories.All);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"LoadFromXml failed: {ex.Message}");
                    }
                }

                // 2. If state text exists, parse and enforce column states
                if (File.Exists(LayoutStatePath))
                {
                    var lines = File.ReadAllLines(LayoutStatePath);
                    string section = "";

                    foreach (var rawLine in lines)
                    {
                        string line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            section = line;
                            continue;
                        }

                        for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
                        {
                            var band = ultraGridMaster.DisplayLayout.Bands[b];

                            // Ensure chaptered / relation columns remain excluded and hidden
                            foreach (UltraGridColumn col in band.Columns)
                            {
                                if (col.IsChaptered || string.Equals(col.Key, "BillDetails", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase) ||
                                    (b > 0 && string.Equals(col.Key, "BillNo", StringComparison.OrdinalIgnoreCase)))
                                {
                                    col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                                }
                            }

                            if (section == $"[BAND_{b}_HIDDEN]")
                            {
                                if (band.Columns.Exists(line) && IsCustomizableColumn(band.Columns[line], b))
                                {
                                    band.Columns[line].Hidden = true;
                                }
                            }
                            else if (section == $"[BAND_{b}_WIDTHS]")
                            {
                                var parts = line.Split('=');
                                if (parts.Length == 2 && band.Columns.Exists(parts[0]) && int.TryParse(parts[1], out int w))
                                {
                                    if (!band.Columns[parts[0]].IsChaptered)
                                    {
                                        band.Columns[parts[0]].Width = w;
                                        savedColumnWidths[$"{b}_{parts[0]}"] = w;
                                    }
                                }
                            }
                            else if (section == $"[BAND_{b}_POSITIONS]")
                            {
                                var parts = line.Split('=');
                                if (parts.Length == 2 && band.Columns.Exists(parts[0]) && int.TryParse(parts[1], out int p))
                                {
                                    if (!band.Columns[parts[0]].IsChaptered)
                                    {
                                        band.Columns[parts[0]].Header.VisiblePosition = p;
                                    }
                                }
                            }
                        }

                        if (section == "[AGGREGATIONS]")
                        {
                            var parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                columnAggregations[parts[0]] = parts[1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadGridLayout failed: {ex.Message}");
            }
            finally
            {
                isLayoutLoading = false;
            }
        }
        #endregion

        #region Summary Footer & Calculations (Master Band Footer Panel)
        /// <summary>
        /// Updates the summary footer panel by creating aligned summary labels for each visible numeric column in Master Band.
        /// </summary>
        private void UpdateSummaryFooter()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGridMaster == null ||
                ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            gridFooterPanel.ClientArea.SuspendLayout();

            // Clear previous summary labels but keep lblCount
            var controlsToRemove = gridFooterPanel.ClientArea.Controls.Cast<Control>()
                .Where(c => c != lblCount)
                .ToList();

            foreach (var ctrl in controlsToRemove)
            {
                gridFooterPanel.ClientArea.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
            summaryLabels.Clear();

            var masterBand = ultraGridMaster.DisplayLayout.Bands[0];
            foreach (var col in masterBand.Columns.Cast<UltraGridColumn>())
            {
                if (col.Hidden || col.IsChaptered) continue;
                if (!IsNumericColumn(col)) continue;
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;

                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 8.25F, FontStyle.Bold),
                    Padding = new Padding(0, 0, 2, 0),
                    Margin = Padding.Empty,
                    Height = gridFooterPanel.Height - 6,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };

                summaryLabels[col.Key] = lbl;
                gridFooterPanel.ClientArea.Controls.Add(lbl);
            }

            gridFooterPanel.ClientArea.ResumeLayout();
            UpdateFooterValues();
            AlignSummaryLabels();
        }

        /// <summary>
        /// Updates aggregate values for master band summary labels
        /// </summary>
        private void UpdateFooterValues()
        {
            if (ultraGridMaster == null || ultraGridMaster.DataSource == null) return;

            DataTable dt = null;
            if (ultraGridMaster.DataSource is DataSet ds && ds.Tables.Contains("SalesMaster"))
            {
                dt = ds.Tables["SalesMaster"];
            }
            else if (ultraGridMaster.DataSource is DataTable d)
            {
                dt = d;
            }
            if (dt == null) return;

            DataRow[] dataRows;
            if (ultraGridMaster.Rows != null && ultraGridMaster.Rows.Count > 0)
            {
                var visibleGridRows = ultraGridMaster.Rows.GetFilteredInNonGroupByRows();
                var rowList = new List<DataRow>();
                foreach (var gr in visibleGridRows)
                {
                    if (gr.ListObject is DataRowView drv)
                    {
                        rowList.Add(drv.Row);
                    }
                }
                dataRows = rowList.ToArray();
            }
            else
            {
                dataRows = dt.Select();
            }

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

                if (!dt.Columns.Contains(colKey)) continue;

                var values = dataRows
                    .Where(r => r[colKey] != DBNull.Value && !string.IsNullOrEmpty(r[colKey].ToString()))
                    .Select(r => Convert.ToDouble(r[colKey]))
                    .ToList();

                string text = "";
                bool isCurrency = colKey == "Amount" || colKey == "SubTotal" || colKey == "TaxAmt" ||
                                  colKey == "TaxAmount" || colKey == "NetAmount" || colKey == "TotalAmount" ||
                                  colKey == "Profit" || colKey == "UnitPrice";
                bool isPercent = colKey.IndexOf("Per", StringComparison.OrdinalIgnoreCase) >= 0;

                switch (agg)
                {
                    case "Sum":
                        double sum = values.Count > 0 ? values.Sum() : 0.0;
                        text = isCurrency ? ("₹ " + sum.ToString("N2")) : (isPercent ? (sum.ToString("0.00") + " %") : sum.ToString("N2"));
                        break;
                    case "Min":
                        double min = values.Count > 0 ? values.Min() : 0.0;
                        text = isCurrency ? ("Min: ₹ " + min.ToString("N2")) : ("Min: " + min.ToString("N2"));
                        break;
                    case "Max":
                        double max = values.Count > 0 ? values.Max() : 0.0;
                        text = isCurrency ? ("Max: ₹ " + max.ToString("N2")) : ("Max: " + max.ToString("N2"));
                        break;
                    case "Average":
                        double avg = values.Count > 0 ? values.Average() : 0.0;
                        text = isCurrency ? ("₹ " + avg.ToString("N2")) : avg.ToString("N2");
                        break;
                    case "Count":
                        text = values.Count.ToString();
                        break;
                }
                lbl.Text = text;
            }
        }

        /// <summary>
        /// Aligns summary labels to match master band column headers
        /// </summary>
        private void AlignSummaryLabels()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGridMaster == null ||
                ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            var band = ultraGridMaster.DisplayLayout.Bands[0];
            int panelScreenX = gridFooterPanel.PointToScreen(Point.Empty).X;

            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;

                if (col.Hidden || col.IsChaptered)
                {
                    lbl.Visible = false;
                    continue;
                }

                var headerUI = col.Header?.GetUIElement();
                if (headerUI != null)
                {
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - panelScreenX;
                    int colWidth = headerUI.Rect.Width;

                    if (colLeft + colWidth > 0 && colLeft < gridFooterPanel.Width)
                    {
                        lbl.Left = colLeft;
                        lbl.Width = Math.Max(0, colWidth - 2);
                        lbl.Top = 3;
                        lbl.Height = gridFooterPanel.Height - 6;
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
                var item = new ToolStripMenuItem(type)
                {
                    Tag = type
                };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                    SaveGridLayout();
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
                currentSummaryType = type;
                if (ultraGridMaster.DisplayLayout.Bands.Count > 0)
                {
                    foreach (var col in ultraGridMaster.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>())
                    {
                        if (!col.Hidden && !col.IsChaptered && IsNumericColumn(col))
                        {
                            columnAggregations[col.Key] = type;
                        }
                    }
                }
                UpdateSummaryFooter();
                SaveGridLayout();
            }
        }

        private bool IsNumericColumn(UltraGridColumn col)
        {
            if (col == null) return false;
            return summaryDefaultNumericColumns.Contains(col.Key);
        }

        public void RefreshSummaryFooter()
        {
            UpdateFooterValues();
            AlignSummaryLabels();
        }
        #endregion

        #region Summary Cards Reflow
        private void SummaryCards_Resize(object sender, EventArgs e)
        {
            ReflowSummaryCards();
        }

        private void ReflowSummaryCards()
        {
            if (ultraPanelSummaryCards == null) return;

            int panelWidth = ultraPanelSummaryCards.ClientArea.Width;
            if (panelWidth < 100) return;

            Infragistics.Win.Misc.UltraPanel[] cards = new Infragistics.Win.Misc.UltraPanel[]
            {
                pnlCardBills, pnlCardQty, pnlCardSubTotal, pnlCardTax, pnlCardNet, pnlCardProfit
            };

            int cardCount = cards.Length;
            int margin = 10;
            int edgePadding = 12;
            int totalMargins = edgePadding * 2 + margin * (cardCount - 1);
            int cardWidth = Math.Max(100, (panelWidth - totalMargins) / cardCount);
            int cardHeight = 60;
            int yPos = (ultraPanelSummaryCards.ClientArea.Height - cardHeight) / 2;
            if (yPos < 4) yPos = 4;

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                int xPos = edgePadding + i * (cardWidth + margin);
                cards[i].Location = new Point(xPos, yPos);
                cards[i].Size = new Size(cardWidth, cardHeight);

                foreach (Control ctrl in cards[i].ClientArea.Controls)
                {
                    if (ctrl is Infragistics.Win.Misc.UltraLabel lbl)
                    {
                        lbl.Size = new Size(cardWidth - 16, lbl.Height);
                    }
                }
            }
        }
        #endregion

        #region Column Chooser & Drag-and-Drop Implementation
        private void InitializeGridContextMenuAndDragDrop()
        {
            ultraGridMaster.MouseDown += GridMaster_MouseDown;
            ultraGridMaster.MouseMove += GridMaster_MouseMove;
            ultraGridMaster.MouseUp += GridMaster_MouseUp;
            ultraGridMaster.DragOver += GridMaster_DragOver;
            ultraGridMaster.DragDrop += GridMaster_DragDrop;
        }

        private void GridMaster_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            UIElement elem = ultraGridMaster.DisplayLayout.UIElement.ElementFromPoint(e.Location);
            HeaderUIElement headerElem = elem as HeaderUIElement ?? elem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip menu = new ContextMenuStrip();

                if (headerElem?.Header?.Column != null)
                {
                    UltraGridColumn col = headerElem.Header.Column;
                    int bIdx = col.Band != null ? col.Band.Index : 0;
                    if (IsCustomizableColumn(col, bIdx))
                    {
                        ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide '{col.Header.Caption}'");
                        hideItem.Click += (s, ev) => HideGridColumn(col);
                        menu.Items.Add(hideItem);
                    }
                }

                ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field/Column Chooser...");
                chooserItem.Click += (s, ev) => ShowColumnChooserDialog();
                menu.Items.Add(chooserItem);

                menu.Show(ultraGridMaster, e.Location);
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (headerElem?.Header?.Column != null)
                {
                    UltraGridColumn col = headerElem.Header.Column;
                    int bIdx = col.Band != null ? col.Band.Index : 0;
                    if (IsCustomizableColumn(col, bIdx))
                    {
                        headerDragStartPoint = e.Location;
                        columnToHideByDrag = col;
                        isDraggingHeaderColumn = false;
                    }
                }
            }
        }

        private void GridMaster_MouseMove(object sender, MouseEventArgs e)
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
                    bool isOutside = !ultraGridMaster.ClientRectangle.Contains(e.Location);

                    if (isDraggingDown || isOutside)
                    {
                        ultraGridMaster.Cursor = Cursors.No;
                        gridToolTip.SetToolTip(ultraGridMaster, $"Drag down/out to hide '{columnToHideByDrag.Header.Caption}'");
                    }
                    else
                    {
                        ultraGridMaster.Cursor = Cursors.Default;
                        gridToolTip.SetToolTip(ultraGridMaster, string.Empty);
                    }
                }
            }
        }

        private void GridMaster_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderColumn && columnToHideByDrag != null)
            {
                bool isDraggingDown = e.Y - headerDragStartPoint.Y > 40;
                bool isOutside = !ultraGridMaster.ClientRectangle.Contains(e.Location);

                if (isDraggingDown || isOutside)
                {
                    HideGridColumn(columnToHideByDrag);
                }
            }

            columnToHideByDrag = null;
            isDraggingHeaderColumn = false;
            ultraGridMaster.Cursor = Cursors.Default;
            gridToolTip.SetToolTip(ultraGridMaster, string.Empty);
        }

        private void GridMaster_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void GridMaster_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnItem)))
            {
                ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                if (item != null && ultraGridMaster.DisplayLayout.Bands.Count > item.BandIndex)
                {
                    var band = ultraGridMaster.DisplayLayout.Bands[item.BandIndex];
                    if (band.Columns.Exists(item.ColumnKey))
                    {
                        var col = band.Columns[item.ColumnKey];
                        if (IsCustomizableColumn(col, item.BandIndex))
                        {
                            try
                            {
                                col.Hidden = false;
                                string key = $"{item.BandIndex}_{col.Key}";
                                if (savedColumnWidths.ContainsKey(key))
                                {
                                    col.Width = savedColumnWidths[key];
                                }
                                UpdateSummaryFooter();
                                RefreshColumnChooserList();
                                SaveGridLayout();
                                gridToolTip.Show($"Column '{item.DisplayText}' restored to grid", ultraGridMaster, ultraGridMaster.PointToClient(Cursor.Position), 1500);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error unhiding column on drag-drop: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void HideGridColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden) return;
            int bandIdx = column.Band != null ? column.Band.Index : 0;
            if (!IsCustomizableColumn(column, bandIdx))
            {
                MessageBox.Show($"The '{column.Header.Caption}' column cannot be hidden.", "Cannot Hide Column", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                savedColumnWidths[$"{bandIdx}_{column.Key}"] = column.Width;
                column.Hidden = true;

                UpdateSummaryFooter();
                RefreshColumnChooserList();
                SaveGridLayout();

                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    PositionColumnChooserAtBottomRight();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hide column failed: {ex.Message}");
            }
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

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(lblInfo);

            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.SizeChanged += (s, e) => PositionColumnChooserAtBottomRight();
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count) return;

            ColumnItem item = columnChooserListBox.Items[e.Index] as ColumnItem;
            if (item == null) return;

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(245, 247, 250)), e.Bounds);

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            Color badgeColor = item.BandIndex == 0 ? Color.FromArgb(41, 128, 185) : Color.FromArgb(70, 90, 120);

            using (SolidBrush bgBrush = new SolidBrush(badgeColor))
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 4;
                int diameter = radius * 2;
                Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
                path.AddArc(arcRect, 180, 90);
                arcRect.X = rect.Right - diameter;
                path.AddArc(arcRect, 270, 90);
                arcRect.Y = rect.Bottom - diameter;
                path.AddArc(arcRect, 0, 90);
                arcRect.X = rect.Left;
                path.AddArc(arcRect, 90, 90);
                path.CloseFigure();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                e.Graphics.DrawString(item.DisplayText, e.Font, textBrush, rect, sf);
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (columnChooserListBox == null) return;

            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches && index < columnChooserListBox.Items.Count)
            {
                ColumnItem item = columnChooserListBox.Items[index] as ColumnItem;
                if (item != null)
                {
                    columnChooserListBox.SelectedIndex = index;
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox == null) return;

            ColumnItem item = columnChooserListBox.SelectedItem as ColumnItem;
            if (item != null && ultraGridMaster.DisplayLayout.Bands.Count > item.BandIndex)
            {
                var band = ultraGridMaster.DisplayLayout.Bands[item.BandIndex];
                if (band.Columns.Exists(item.ColumnKey))
                {
                    var col = band.Columns[item.ColumnKey];
                    if (IsCustomizableColumn(col, item.BandIndex))
                    {
                        try
                        {
                            col.Hidden = false;
                            string key = $"{item.BandIndex}_{col.Key}";
                            if (savedColumnWidths.ContainsKey(key))
                            {
                                col.Width = savedColumnWidths[key];
                            }
                            UpdateSummaryFooter();
                            RefreshColumnChooserList();
                            SaveGridLayout();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error unhiding column on double-click: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                Point screenPoint = this.PointToScreen(Point.Empty);
                columnChooserForm.Location = new Point(
                    screenPoint.X + this.Width - columnChooserForm.Width - 30,
                    screenPoint.Y + this.Height - columnChooserForm.Height - 50);
                columnChooserForm.BringToFront();
            }
        }

        private void RefreshColumnChooserList()
        {
            if (columnChooserListBox == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.Items.Clear();

            for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
            {
                var band = ultraGridMaster.DisplayLayout.Bands[b];
                string bandPrefix = b > 0 ? "[Item] " : "";

                foreach (var col in band.Columns)
                {
                    if (col.Hidden && IsCustomizableColumn(col, b))
                    {
                        string caption = string.IsNullOrEmpty(col.Header.Caption) ? col.Key : col.Header.Caption;
                        columnChooserListBox.Items.Add(new ColumnItem(col.Key, bandPrefix + caption, b));
                    }
                }
            }
        }
        #endregion

        #region Export & Print Preview
        private void ExportToExcel()
        {
            if (ultraGridMaster.DataSource == null)
            {
                MessageBox.Show("No data available to export.", "Export to Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV (Comma delimited) (*.csv)|*.csv",
                    FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        if (ultraGridMaster.DataSource is DataSet ds && ds.Tables.Contains("SalesMaster"))
                        {
                            DataTable mTable = ds.Tables["SalesMaster"];

                            var visibleCols = ultraGridMaster.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>()
                                .Where(c => !c.Hidden && !c.IsChaptered)
                                .OrderBy(c => c.Header.VisiblePosition)
                                .ToList();

                            sw.WriteLine(string.Join(",", visibleCols.Select(c => $"\"{c.Header.Caption}\"")));

                            foreach (DataRow row in mTable.Rows)
                            {
                                var fields = visibleCols.Select(c =>
                                {
                                    object val = mTable.Columns.Contains(c.Key) ? row[c.Key] : "";
                                    string str = val?.ToString() ?? "";
                                    if (str.Contains(",") || str.Contains("\""))
                                    {
                                        str = "\"" + str.Replace("\"", "\"\"") + "\"";
                                    }
                                    return str;
                                });
                                sw.WriteLine(string.Join(",", fields));
                            }
                        }
                    }

                    MessageBox.Show("Sales report exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowGridPreview(string title)
        {
            try
            {
                PrintDocument pd = new PrintDocument();
                pd.DocumentName = title;
                PrintPreviewDialog ppd = new PrintPreviewDialog
                {
                    Document = pd,
                    Width = 900,
                    Height = 650,
                    StartPosition = FormStartPosition.CenterScreen
                };
                ppd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Preview: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion
    }
}