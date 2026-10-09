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
using System.Windows.Forms;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class SalesReturnReport : Form
    {
        #region Colour Palette (exact matches frmStockReport / frmPurchaseReturn theme)
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

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);
        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);
        #endregion

        #region Private Fields
        private SalesReturnReportRepository _reportRepository;
        private Dropdowns _dropdowns;
        private readonly List<ComboItem> _customerOptions;

        // Dynamic footer panel and cell controls (exact frmStockReport implementation)
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SubTotal",
            "TaxAmt",
            "GrandTotal"
        };

        // Header drag-to-hide & Column Chooser
        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        private bool isLoading = false;
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
            public int BandIndex { get; }

            public ColumnChooserItem(string key, string text, int bandIndex = 0)
            {
                ColumnKey = key;
                DisplayText = text;
                BandIndex = bandIndex;
            }

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Constructor
        public SalesReturnReport()
        {
            _customerOptions = new List<ComboItem>();

            InitializeComponent();
            this.Font = new Font("Segoe UI", 9F);
            Load += SalesReturnReport_Load;
            FormClosed += SalesReturnReport_FormClosed;
            SetupKeyboardShortcuts();
        }
        #endregion

        #region Form Lifecycle Events
        private void SalesReturnReport_Load(object sender, EventArgs e)
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

        private void SalesReturnReport_FormClosed(object sender, FormClosedEventArgs e)
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

        #region Keyboard Shortcuts
        private void SetupKeyboardShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.E)
                {
                    ExportToExcel();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    ShowGridPreview("Sales Return Report - Detailed Report");
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F5)
                {
                    LoadData();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                    e.Handled = true;
                }
            };
        }
        #endregion

        #region UI Setup & Styling
        private void InitializeRuntimeAppearance()
        {
            this.AutoScroll = false;
            this.MinimumSize = Size.Empty;
            BackColor = FormBackColor;

            // Panels
            if (ultraPanelSelection != null)
            {
                ultraPanelSelection.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelSelection.Appearance.BorderColor = BorderBlue;
                ultraPanelSelection.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelActionBar != null)
            {
                ultraPanelActionBar.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
                ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelActionBar.Size = new Size(ultraPanelActionBar.Width, 38);
            }

            if (ultraPanelGrid != null)
            {
                ultraPanelGrid.Appearance.BackColor = FormBackColor;
                ultraPanelGrid.Appearance.BorderColor = BorderBlue;
                ultraPanelGrid.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (gridFooterPanel != null)
            {
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
                gridFooterPanel.Height = 26;
            }

            // Labels
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblReturnNo);
            StyleLabel(lblPaymentMode);
            StyleLabel(lblCustomer);

            // Controls
            StyleFilterCombo(ultraComboDateMode);
            StyleFilterCombo(cmbPaymentMode);
            StyleFilterCombo(cmbCustomer);
            StyleTextEditor(txtReturnNo);
            StyleDateTimeEditor(dtFromDate);
            StyleDateTimeEditor(dtToDate);

            // Buttons
            StyleButton(btnViewGrid);
            StyleButton(btnPreviewGrid);
            StyleButton(btnPreviewReport);
            StyleButton(btnExportExcel);
            StyleButton(btnColumnChooser);
            StyleButton(btnHideSelection);

            // Grid Appearance & Events
            ConfigureGridAppearance(ultraGridMaster);
            InitializeGridFooter();
            if (gridFooterPanel != null)
            {
                gridFooterPanel.Resize += (s, e) => UpdateFooterCellPositions();
            }
            if (ultraGridMaster != null)
            {
                ultraGridMaster.Resize += (s, e) => UpdateFooterCellPositions();
            }
            if (ultraPanelGrid != null)
            {
                ultraPanelGrid.Resize += (s, e) => UpdateFooterCellPositions();
                if (ultraPanelGrid.ClientArea != null)
                {
                    ultraPanelGrid.ClientArea.Resize += (s, e) => UpdateFooterCellPositions();
                }
            }
            SetupHeaderDragToHideAndColumnChooser();
            LayoutPanels();
        }

        private static void StyleLabel(UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Segoe UI";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Segoe UI";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleDateTimeEditor(UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 9F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleButton(UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = BorderBlue;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.FontData.Name = "Segoe UI";
            button.Appearance.FontData.SizeInPoints = 9F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = BorderBlue;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = BorderBlue;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            targetGrid.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            targetGrid.DisplayLayout.Scrollbars = Scrollbars.Both;
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
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 35;
            targetGrid.DisplayLayout.Override.MinRowHeight = 20;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 22;

            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            targetGrid.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;

            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;

            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;

            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);
            targetGrid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            targetGrid.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.SummaryDisplayArea = SummaryDisplayAreas.None;
            targetGrid.DisplayLayout.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;
            targetGrid.AllowDrop = true;

            // Events
            targetGrid.InitializeLayout += UltraGridMaster_InitializeLayout;
            targetGrid.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            targetGrid.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            targetGrid.Resize += (s, e) => UpdateFooterCellPositions();
            targetGrid.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            };
            targetGrid.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            targetGrid.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            targetGrid.Paint += (s, e) => UpdateFooterCellPositions();
            targetGrid.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            targetGrid.AfterSortChange += (s, e) => UpdateFooterValues();
        }

        private void LoadLookupData()
        {
            _reportRepository = new SalesReturnReportRepository();
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
            cmbCustomer.Value = "";
            txtReturnNo.Text = "";

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
            ShowGridPreview("Sales Return Report - Print Preview");
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadData();
            ShowGridPreview("Sales Return Report - Detailed Report");
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void BtnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooserForm();
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
            LayoutPanels();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutPanels();
        }

        private void LayoutPanels()
        {
            if (ultraPanelActionBar == null || ultraPanelGrid == null || ultraPanelSelection == null) return;

            if (TopLevel == false)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Dock = DockStyle.Fill;
            }

            SuspendLayout();

            this.Controls.Clear();
            this.Controls.Add(ultraPanelGrid);         // Fill (docked first = background)
            this.Controls.Add(ultraPanelActionBar);    // Top (docked second)
            this.Controls.Add(ultraPanelSelection);    // Top (docked topmost)

            ultraPanelSelection.Dock = DockStyle.Top;
            ultraPanelActionBar.Dock = DockStyle.Top;
            ultraPanelActionBar.Height = 42;
            ultraPanelGrid.Dock = DockStyle.Fill;

            if (ultraPanelGrid.ClientArea != null && ultraGridMaster != null && gridFooterPanel != null)
            {
                ultraPanelGrid.ClientArea.Controls.Clear();
                ultraPanelGrid.ClientArea.Controls.Add(ultraGridMaster);
                ultraPanelGrid.ClientArea.Controls.Add(gridFooterPanel);

                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 26;
                gridFooterPanel.Visible = true;

                ultraGridMaster.Dock = DockStyle.Fill;
                ultraGridMaster.BringToFront();
            }

            ResumeLayout(true);
            PerformLayout();

            UpdateFooterCellPositions();
        }

        private void UltraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            UpdateDateControlsVisibility();
            if (!isLoading) LoadData();
        }
        #endregion

        #region Data Loading & Hierarchical Binding
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
                string customerFilter = cmbCustomer.Text?.Trim() ?? "";
                if (customerFilter.Equals("ALL", StringComparison.OrdinalIgnoreCase)) customerFilter = "";
                string returnNoFilter = txtReturnNo.Text?.Trim() ?? "";

                // Fetch Returns from repository
                List<SalesReturnReportMaster> returns = _reportRepository.GetSalesReturnRecords(fromDate, toDate, SessionContext.BranchId);

                // Filter master returns
                var filteredReturns = returns.AsEnumerable();

                if (!string.IsNullOrEmpty(customerFilter))
                {
                    filteredReturns = filteredReturns.Where(r => (r.CustomerName ?? "").IndexOf(customerFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.Equals(paymentFilter, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    filteredReturns = filteredReturns.Where(r => (r.Paymode ?? "").IndexOf(paymentFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(returnNoFilter))
                {
                    filteredReturns = filteredReturns.Where(r => r.SReturnNo.ToString().Contains(returnNoFilter) ||
                                                                (r.InvoiceNo ?? "").IndexOf(returnNoFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var finalReturns = filteredReturns.ToList();

                // Create Master-Detail DataSet
                DataSet dsHierarchical = new DataSet("SalesReturnMasterDetail");

                // 1. Master Table (Returns)
                DataTable masterTable = new DataTable("SalesReturnMaster");
                masterTable.Columns.Add("SlNo", typeof(int));
                masterTable.Columns.Add("SReturnNo", typeof(int));
                masterTable.Columns.Add("SReturnDate", typeof(DateTime));
                masterTable.Columns.Add("InvoiceNo", typeof(string));
                masterTable.Columns.Add("InvoiceDate", typeof(DateTime));
                masterTable.Columns.Add("CustomerName", typeof(string));
                masterTable.Columns.Add("Paymode", typeof(string));
                masterTable.Columns.Add("SubTotal", typeof(decimal));
                masterTable.Columns.Add("TaxAmt", typeof(decimal));
                masterTable.Columns.Add("GrandTotal", typeof(decimal));

                // 2. Detail Table (Return Items)
                DataTable detailTable = new DataTable("SalesReturnDetail");
                detailTable.Columns.Add("DetailID", typeof(int));
                detailTable.Columns["DetailID"].AutoIncrement = true;
                detailTable.Columns["DetailID"].AutoIncrementSeed = 1;
                detailTable.Columns["DetailID"].AutoIncrementStep = 1;

                detailTable.Columns.Add("SReturnNo", typeof(int));
                detailTable.Columns.Add("SlNo", typeof(int));
                detailTable.Columns.Add("ItemName", typeof(string));
                detailTable.Columns.Add("Unit", typeof(string));
                detailTable.Columns.Add("Packing", typeof(string));
                detailTable.Columns.Add("Qty", typeof(decimal));
                detailTable.Columns.Add("SalesPrice", typeof(decimal));
                detailTable.Columns.Add("TaxPer", typeof(decimal));
                detailTable.Columns.Add("TaxAmt", typeof(decimal));
                detailTable.Columns.Add("Amount", typeof(decimal));
                detailTable.Columns.Add("Reason", typeof(string));

                decimal totalQty = 0;
                decimal totalSubTotal = 0;
                decimal totalTax = 0;
                decimal totalGrandTotal = 0;
                int masterSerial = 1;

                foreach (var ret in finalReturns)
                {
                    decimal retTaxTotal = 0;
                    var details = _reportRepository.GetSalesReturnReportDetails(ret.SReturnNo);

                    if (details?.Details != null && details.Details.Count > 0)
                    {
                        int detailSerial = 1;
                        foreach (var d in details.Details)
                        {
                            DataRow dRow = detailTable.NewRow();
                            dRow["SReturnNo"] = ret.SReturnNo;
                            dRow["SlNo"] = detailSerial++;
                            dRow["ItemName"] = d.ItemName ?? "";
                            dRow["Unit"] = d.Unit ?? "";
                            dRow["Packing"] = d.Packing ?? "";
                            dRow["Qty"] = Convert.ToDecimal(d.Qty);
                            dRow["SalesPrice"] = Convert.ToDecimal(d.SalesPrice);
                            dRow["TaxPer"] = Convert.ToDecimal(d.TaxPer);
                            dRow["TaxAmt"] = Convert.ToDecimal(d.TaxAmt);
                            dRow["Amount"] = Convert.ToDecimal(d.Amount);
                            dRow["Reason"] = d.Reason ?? "";
                            detailTable.Rows.Add(dRow);

                            totalQty += Convert.ToDecimal(d.Qty);
                            retTaxTotal += Convert.ToDecimal(d.TaxAmt);
                        }
                    }

                    DataRow mRow = masterTable.NewRow();
                    mRow["SlNo"] = masterSerial++;
                    mRow["SReturnNo"] = ret.SReturnNo;
                    mRow["SReturnDate"] = ret.SReturnDate;
                    mRow["InvoiceNo"] = ret.InvoiceNo ?? "";
                    mRow["InvoiceDate"] = ret.InvoiceDate;
                    mRow["CustomerName"] = ret.CustomerName ?? "";
                    mRow["Paymode"] = ret.Paymode ?? "";
                    mRow["SubTotal"] = Convert.ToDecimal(ret.SubTotal);
                    mRow["TaxAmt"] = retTaxTotal;
                    mRow["GrandTotal"] = Convert.ToDecimal(ret.GrandTotal);
                    masterTable.Rows.Add(mRow);

                    totalSubTotal += Convert.ToDecimal(ret.SubTotal);
                    totalTax += retTaxTotal;
                    totalGrandTotal += Convert.ToDecimal(ret.GrandTotal);
                }

                dsHierarchical.Tables.Add(masterTable);
                dsHierarchical.Tables.Add(detailTable);

                // Create relationship
                DataRelation relation = new DataRelation(
                    "MasterDetail",
                    masterTable.Columns["SReturnNo"],
                    detailTable.Columns["SReturnNo"],
                    false
                );
                dsHierarchical.Relations.Add(relation);

                ultraGridMaster.DataSource = null;
                ultraGridMaster.DataSource = dsHierarchical;

                ApplyUserHiddenColumns();

                CreateFooterCells();
                UpdateFooterValues();
                UpdateFooterCellPositions();
                PopulateColumnChooserListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales return report: {ex.Message}", "Sales Return Report Error",
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
                foreach (var band in e.Layout.Bands)
                {
                    foreach (var col in band.Columns)
                    {
                        if (col.IsChaptered || string.Equals(col.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase))
                        {
                            col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                        }
                    }
                }

                // Master Band (Band 0 - Returns)
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
            if (masterBand.Columns.Exists("SReturnNo"))
            {
                masterBand.Columns["SReturnNo"].Header.Caption = "Return No";
                masterBand.Columns["SReturnNo"].Width = 90;
                masterBand.Columns["SReturnNo"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["SReturnNo"].CellAppearance.ForeColor = Color.FromArgb(21, 101, 192);
                masterBand.Columns["SReturnNo"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["SReturnNo"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("SReturnDate"))
            {
                masterBand.Columns["SReturnDate"].Header.Caption = "Return Date";
                masterBand.Columns["SReturnDate"].Format = "dd/MM/yyyy hh:mm tt";
                masterBand.Columns["SReturnDate"].Width = 145;
                masterBand.Columns["SReturnDate"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["SReturnDate"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("InvoiceNo"))
            {
                masterBand.Columns["InvoiceNo"].Header.Caption = "Invoice No";
                masterBand.Columns["InvoiceNo"].Width = 100;
                masterBand.Columns["InvoiceNo"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["InvoiceNo"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("InvoiceDate"))
            {
                masterBand.Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                masterBand.Columns["InvoiceDate"].Format = "dd/MM/yyyy";
                masterBand.Columns["InvoiceDate"].Width = 110;
                masterBand.Columns["InvoiceDate"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["InvoiceDate"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("CustomerName"))
            {
                masterBand.Columns["CustomerName"].Header.Caption = "Customer";
                masterBand.Columns["CustomerName"].Width = 220;
                masterBand.Columns["CustomerName"].Header.VisiblePosition = pos++;
            }
            if (masterBand.Columns.Exists("Paymode"))
            {
                masterBand.Columns["Paymode"].Header.Caption = "Pay Mode";
                masterBand.Columns["Paymode"].Width = 95;
                masterBand.Columns["Paymode"].CellAppearance.TextHAlign = HAlign.Center;
                masterBand.Columns["Paymode"].Header.VisiblePosition = pos++;
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
            if (masterBand.Columns.Exists("GrandTotal"))
            {
                masterBand.Columns["GrandTotal"].Header.Caption = "Grand Total";
                masterBand.Columns["GrandTotal"].Format = "₹ #,##0.00";
                masterBand.Columns["GrandTotal"].Width = 135;
                masterBand.Columns["GrandTotal"].CellAppearance.TextHAlign = HAlign.Right;
                masterBand.Columns["GrandTotal"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                masterBand.Columns["GrandTotal"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                masterBand.Columns["GrandTotal"].Header.VisiblePosition = pos++;
            }
        }

        private void ConfigureDetailBandColumns(UltraGridBand detailBand)
        {
            detailBand.Header.Caption = "Return Item Details";
            detailBand.HeaderVisible = true;
            detailBand.Header.Appearance.BackColor = GridHeaderBlue;
            detailBand.Header.Appearance.BackColor2 = GridHeaderBlueDark;
            detailBand.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
            detailBand.Header.Appearance.ForeColor = Color.White;
            detailBand.Header.Appearance.FontData.Bold = DefaultableBoolean.True;

            // Hide internal keys and exclude from column chooser
            if (detailBand.Columns.Exists("DetailID"))
            {
                detailBand.Columns["DetailID"].Hidden = true;
                detailBand.Columns["DetailID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
            }
            if (detailBand.Columns.Exists("SReturnNo"))
            {
                detailBand.Columns["SReturnNo"].Hidden = true;
                detailBand.Columns["SReturnNo"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
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
            if (detailBand.Columns.Exists("Unit"))
            {
                detailBand.Columns["Unit"].Header.Caption = "Unit";
                detailBand.Columns["Unit"].Width = 60;
                detailBand.Columns["Unit"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["Unit"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Packing"))
            {
                detailBand.Columns["Packing"].Header.Caption = "Packing";
                detailBand.Columns["Packing"].Width = 70;
                detailBand.Columns["Packing"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["Packing"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Qty"))
            {
                detailBand.Columns["Qty"].Header.Caption = "Quantity";
                detailBand.Columns["Qty"].Format = "0.00";
                detailBand.Columns["Qty"].Width = 70;
                detailBand.Columns["Qty"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Qty"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("SalesPrice"))
            {
                detailBand.Columns["SalesPrice"].Header.Caption = "Price";
                detailBand.Columns["SalesPrice"].Format = "₹ #,##0.00";
                detailBand.Columns["SalesPrice"].Width = 90;
                detailBand.Columns["SalesPrice"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["SalesPrice"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("TaxPer"))
            {
                detailBand.Columns["TaxPer"].Header.Caption = "Tax %";
                detailBand.Columns["TaxPer"].Format = "0.00 %";
                detailBand.Columns["TaxPer"].Width = 65;
                detailBand.Columns["TaxPer"].CellAppearance.TextHAlign = HAlign.Center;
                detailBand.Columns["TaxPer"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("TaxAmt"))
            {
                detailBand.Columns["TaxAmt"].Header.Caption = "Tax Amt";
                detailBand.Columns["TaxAmt"].Format = "₹ #,##0.00";
                detailBand.Columns["TaxAmt"].Width = 90;
                detailBand.Columns["TaxAmt"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["TaxAmt"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Amount"))
            {
                detailBand.Columns["Amount"].Header.Caption = "Amount";
                detailBand.Columns["Amount"].Format = "₹ #,##0.00";
                detailBand.Columns["Amount"].Width = 100;
                detailBand.Columns["Amount"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                detailBand.Columns["Amount"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Reason"))
            {
                detailBand.Columns["Reason"].Header.Caption = "Return Reason";
                detailBand.Columns["Reason"].Width = 160;
                detailBand.Columns["Reason"].Header.VisiblePosition = pos++;
            }
        }

        private void ApplyUserHiddenColumns()
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

            foreach (var key in userHiddenColumnKeys)
            {
                for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
                {
                    var band = ultraGridMaster.DisplayLayout.Bands[b];
                    if (band.Columns.Exists(key))
                    {
                        band.Columns[key].Hidden = true;
                    }
                }
            }
        }
        #endregion

        #region UltraPanelGridFooter Dynamic Alignment & Calculation (Parity with frmStockReport)
        private void InitializeGridFooter()
        {
            if (gridFooterPanel == null) return;

            gridFooterPanel.UseAppStyling = false;
            gridFooterPanel.UseOsThemes = DefaultableBoolean.False;
            gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
            gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
            gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
            gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
            gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;

            _columnAggregations.Clear();
            foreach (var col in summaryDefaultNumericColumns)
            {
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
            int xOffset = ultraGridMaster.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase))
                    continue;

                ContentAlignment align = ContentAlignment.MiddleLeft;
                if (column.CellAppearance.TextHAlign == HAlign.Right || IsSummableColumn(column))
                    align = ContentAlignment.MiddleRight;
                else if (column.CellAppearance.TextHAlign == HAlign.Center)
                    align = ContentAlignment.MiddleCenter;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = align,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(gridFooterPanel.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
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

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min") { Tag = "Min", Enabled = isNumeric };
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max") { Tag = "Max", Enabled = isNumeric };
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
                    : (summaryDefaultNumericColumns.Contains(columnKey) ? "Sum" : "None");

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
            if (column == null) return false;
            return summaryDefaultNumericColumns.Contains(column.Key) ||
                   string.Equals(column.Key, "Amount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Qty", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "SalesPrice", StringComparison.OrdinalIgnoreCase);
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
            if (ultraGridMaster == null || ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || gridFooterPanel == null)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGridMaster.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = 0;
            if (ultraGridMaster.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGridMaster.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || column.IsChaptered || string.Equals(column.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase) || !_footerLabels.ContainsKey(column.Key))
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
            if (_footerLabels.Count == 0 || ultraGridMaster == null || ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
                return;

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
            foreach (UltraGridRow row in ultraGridMaster.Rows.GetFilteredInNonGroupByRows())
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

            if (value is decimal decVal) return decVal;
            if (value is double dblVal) return Convert.ToDecimal(dblVal);
            if (value is float fltVal) return Convert.ToDecimal(fltVal);
            if (value is int intVal) return intVal;
            if (value is long longVal) return longVal;
            if (value is short shortVal) return shortVal;

            decimal result;
            if (decimal.TryParse(value.ToString().Replace("₹", "").Trim(), out result))
                return result;

            return null;
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            if (result == null)
            {
                if (string.Equals(aggregation, "Count", StringComparison.OrdinalIgnoreCase))
                    return "0";
                if (string.Equals(aggregation, "Sum", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Avg", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Min", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(aggregation, "Max", StringComparison.OrdinalIgnoreCase))
                    return "0.00";
                return string.Empty;
            }

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (ultraGridMaster.DisplayLayout != null &&
                ultraGridMaster.DisplayLayout.Bands.Count > 0 &&
                ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numVal = GetNumericValue(result);
                if (numVal.HasValue)
                {
                    string numStr = !string.IsNullOrWhiteSpace(column.Format)
                        ? numVal.Value.ToString(column.Format)
                        : numVal.Value.ToString("N2");

                    if (string.Equals(aggregation, "Avg", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(aggregation, "Average", StringComparison.OrdinalIgnoreCase))
                        return $"Avg: {numStr}";
                    if (string.Equals(aggregation, "Min", StringComparison.OrdinalIgnoreCase))
                        return $"Min: {numStr}";
                    if (string.Equals(aggregation, "Max", StringComparison.OrdinalIgnoreCase))
                        return $"Max: {numStr}";

                    return numStr;
                }
            }

            return Convert.ToString(result);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser (Parity with frmStockReport)
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
            ultraGridMaster.MouseDown += UltraGridMaster_MouseDown;
            ultraGridMaster.MouseMove += UltraGridMaster_MouseMove;
            ultraGridMaster.MouseUp += UltraGridMaster_MouseUp;
            ultraGridMaster.DragOver += UltraGridMaster_DragOver;
            ultraGridMaster.DragDrop += UltraGridMaster_DragDrop;
        }

        private void UltraGridMaster_MouseDown(object sender, MouseEventArgs e)
        {
            if (ultraGridMaster.DisplayLayout.Bands.Count == 0) return;

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

        private void UltraGridMaster_MouseMove(object sender, MouseEventArgs e)
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

        private void UltraGridMaster_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                if (isDraggingHeaderToHide)
                {
                    HideGridColumn(columnBeingDragged);
                    headerToolTip.Hide(ultraGridMaster);
                }
                columnBeingDragged = null;
                isDraggingHeaderToHide = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void UltraGridMaster_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void UltraGridMaster_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = ultraGridMaster.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt, item.BandIndex);
                UnhideColumn(item.ColumnKey, item.BandIndex, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt, int bandIndex)
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count <= bandIndex)
                return 0;

            UIElement element = ultraGridMaster.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null && headerUI.Header.Column.Band.Index == bandIndex)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[bandIndex];
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

        private void HideGridColumn(UltraGridColumn col)
        {
            if (col == null || col.Hidden) return;
            int bandIdx = col.Band != null ? col.Band.Index : 0;
            if (!IsCustomizableColumn(col, bandIdx))
            {
                MessageBox.Show($"The '{col.Header.Caption}' column cannot be hidden.", "Cannot Hide Column", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideGridColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(ultraGridMaster, location);
        }

        private bool IsCustomizableColumn(UltraGridColumn col, int bandIndex)
        {
            if (col == null || col.IsChaptered) return false;
            if (string.Equals(col.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase)) return false;
            if (bandIndex > 0 && string.Equals(col.Key, "SReturnNo", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
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
            string filterText = txtColumnSearch?.Text?.Trim() ?? "";

            for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[b];
                string bandPrefix = b > 0 ? "[Item] " : "";

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden && IsCustomizableColumn(col, b))
                    {
                        string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                        string display = bandPrefix + caption;

                        if (string.IsNullOrEmpty(filterText) || display.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, display, b));
                        }
                    }
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.ColumnKey, item.BandIndex);
            }
        }

        private void UnhideColumn(string columnKey, int bandIndex, int? targetVisiblePosition = null)
        {
            userHiddenColumnKeys.Remove(columnKey);
            if (ultraGridMaster.DisplayLayout.Bands.Count > bandIndex && ultraGridMaster.DisplayLayout.Bands[bandIndex].Columns.Exists(columnKey))
            {
                UltraGridColumn col = ultraGridMaster.DisplayLayout.Bands[bandIndex].Columns[columnKey];
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
            for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[b];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (IsCustomizableColumn(col, b))
                    {
                        col.Hidden = false;
                    }
                }
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

            Color badgeColor = item.BandIndex == 0 ? Color.FromArgb(0, 121, 211) : Color.FromArgb(70, 90, 120);

            using (SolidBrush bgBrush = new SolidBrush(badgeColor))
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
                    FileName = $"SalesReturnReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        if (ultraGridMaster.DataSource is DataSet ds && ds.Tables.Contains("SalesReturnMaster"))
                        {
                            DataTable mTable = ds.Tables["SalesReturnMaster"];

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

                    MessageBox.Show("Sales return report exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
