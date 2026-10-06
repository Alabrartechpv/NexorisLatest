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
        // ─── Theme Palette (matches frmItemReport / FrmSmartReorderDashboard) ────────
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
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);

        private static readonly Color ButtonTopColor       = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor    = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor    = Color.FromArgb(73, 119, 184);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        #region Private Fields
        private SalesReturnReportRepository _reportRepository;
        private Dropdowns _dropdowns;
        private readonly List<ComboItem> _customerOptions;
        private readonly Dictionary<string, UltraLabel> footerLabels = new Dictionary<string, UltraLabel>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SubTotal",
            "TaxAmt",
            "GrandTotal",
            "Amount"
        };

        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private Point headerDragStartPoint;
        private UltraGridColumn columnToHideByDrag;
        private bool isDraggingHeaderColumn;
        private readonly System.Windows.Forms.ToolTip gridToolTip = new System.Windows.Forms.ToolTip();
        private Cursor blackXCursor;
        private bool isLoading = false;
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
        public SalesReturnReport()
        {
            _customerOptions = new List<ComboItem>();

            InitializeComponent();
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
                gridFooterPanel.Appearance.BackColor = Color.FromArgb(93, 151, 214);
                gridFooterPanel.Appearance.BackColor2 = Color.FromArgb(67, 118, 184);
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
                gridFooterPanel.Height = 26;
            }

            if (ultraPanelSummaryCards != null)
            {
                ultraPanelSummaryCards.Appearance.BackColor = FormBackColor;
                ultraPanelSummaryCards.Appearance.BorderColor = BorderBlue;
                ultraPanelSummaryCards.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelSummaryCards.Height = 72;
            }

            // Labels
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblReturnNo);
            StyleLabel(lblPaymentMode);
            StyleLabel(lblCustomer);
            StyleLabel(lblCount);

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

            // Summary Cards
            StyleSummaryCards();

            ConfigureGridAppearance(ultraGridMaster);
            InitializeGridFooter();
            SetupHeaderDragToHideAndColumnChooser();
            ReflowSummaryCards();
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleDateTimeEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.Appearance.BackColor = ButtonTopColor;
            button.Appearance.BackColor2 = ButtonBottomColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = ButtonBorderColor;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.FontData.Name = "Microsoft Sans Serif";
            button.Appearance.FontData.SizeInPoints = 9F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = ButtonBorderColor;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = ButtonBorderColor;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void StyleSummaryCards()
        {
            Infragistics.Win.Misc.UltraLabel[] titles = new Infragistics.Win.Misc.UltraLabel[]
            {
                lblCardReturnsTitle, lblCardQtyTitle, lblCardSubTotalTitle, lblCardTaxTitle, lblCardGrandTotalTitle
            };

            Infragistics.Win.Misc.UltraLabel[] values = new Infragistics.Win.Misc.UltraLabel[]
            {
                lblCardReturnsValue, lblCardQtyValue, lblCardSubTotalValue, lblCardTaxValue, lblCardGrandTotalValue
            };

            Infragistics.Win.Misc.UltraPanel[] panels = new Infragistics.Win.Misc.UltraPanel[]
            {
                pnlCardReturns, pnlCardQty, pnlCardSubTotal, pnlCardTax, pnlCardGrandTotal
            };

            Color[] colors = new Color[]
            {
                Color.FromArgb(25, 118, 210),
                Color.FromArgb(0, 150, 136),
                Color.FromArgb(239, 108, 0),
                Color.FromArgb(142, 36, 170),
                Color.FromArgb(46, 125, 50)
            };

            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null)
                {
                    panels[i].Appearance.BackColor = Color.White;
                    panels[i].Appearance.BorderColor = Color.FromArgb(200, 220, 240);
                    panels[i].BorderStyle = UIElementBorderStyle.Solid;
                    panels[i].UseAppStyling = false;
                    panels[i].UseOsThemes = DefaultableBoolean.False;
                }

                if (titles[i] != null)
                {
                    titles[i].Appearance.BackColor = Color.Transparent;
                    titles[i].Appearance.ForeColor = Color.FromArgb(70, 90, 120);
                    titles[i].Appearance.FontData.Name = "Microsoft Sans Serif";
                    titles[i].Appearance.FontData.SizeInPoints = 8.25F;
                    titles[i].Appearance.FontData.Bold = DefaultableBoolean.False;
                }

                if (values[i] != null)
                {
                    values[i].Appearance.BackColor = Color.Transparent;
                    values[i].Appearance.ForeColor = colors[i];
                    values[i].Appearance.FontData.Name = "Microsoft Sans Serif";
                    values[i].Appearance.FontData.SizeInPoints = 12F;
                    values[i].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        private void ReflowSummaryCards()
        {
            if (ultraPanelSummaryCards == null || ultraPanelSummaryCards.ClientArea == null) return;

            Infragistics.Win.Misc.UltraPanel[] panels = new Infragistics.Win.Misc.UltraPanel[]
            {
                pnlCardReturns, pnlCardQty, pnlCardSubTotal, pnlCardTax, pnlCardGrandTotal
            };

            int count = panels.Length;
            int totalWidth = ultraPanelSummaryCards.ClientArea.Width;
            int margin = 12;
            int spacing = 10;
            int usableWidth = totalWidth - (margin * 2) - (spacing * (count - 1));
            int cardWidth = Math.Max(160, usableWidth / count);
            int cardHeight = 56;
            int top = 8;

            for (int i = 0; i < count; i++)
            {
                if (panels[i] != null)
                {
                    int left = margin + (i * (cardWidth + spacing));
                    panels[i].SetBounds(left, top, cardWidth, cardHeight);
                }
            }
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
            targetGrid.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            targetGrid.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            targetGrid.Resize += (s, e) => UpdateFooterCellPositions();
            targetGrid.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
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
            ShowColumnChooserDialog();
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
            if (ultraPanelActionBar == null || ultraPanelGrid == null || ultraPanelSummaryCards == null || ultraPanelSelection == null) return;

            if (TopLevel == false)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Dock = DockStyle.Fill;
            }

            SuspendLayout();

            ultraPanelSelection.Dock = DockStyle.Top;
            ultraPanelActionBar.Dock = DockStyle.Top;
            ultraPanelSummaryCards.Dock = DockStyle.Bottom;
            ultraPanelSummaryCards.Height = 72;
            ultraPanelSummaryCards.Visible = true;
            ultraPanelGrid.Dock = DockStyle.Fill;

            if (ultraPanelGrid.ClientArea != null)
            {
                ultraPanelGrid.ClientArea.SuspendLayout();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 26;
                ultraGridMaster.Dock = DockStyle.Fill;
                ultraPanelGrid.ClientArea.ResumeLayout(true);
            }

            ResumeLayout(true);
            PerformLayout();

            ReflowSummaryCards();
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

                // Update Bottom Summary Cards
                lblCardReturnsValue.Text = $"{finalReturns.Count:N0}";
                lblCardQtyValue.Text = $"{totalQty:N2}";
                lblCardSubTotalValue.Text = $"₹ {totalSubTotal:N2}";
                lblCardTaxValue.Text = $"₹ {totalTax:N2}";
                lblCardGrandTotalValue.Text = $"₹ {totalGrandTotal:N2}";

                if (lblCount != null)
                {
                    lblCount.Text = $"Total Returns: {finalReturns.Count:N0}";
                }

                UpdateFooterValues();
                UpdateFooterCellPositions();
                RefreshColumnChooserList();
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

        #region Summary Footer Panel Implementation
        private void InitializeGridFooter()
        {
            if (gridFooterPanel == null) return;

            gridFooterPanel.Appearance.BackColor = Color.FromArgb(93, 151, 214);
            gridFooterPanel.Appearance.BackColor2 = Color.FromArgb(67, 118, 184);
            gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.Vertical;
            gridFooterPanel.Appearance.BorderColor = BorderBlue;
            gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
            gridFooterPanel.Height = 26;
            gridFooterPanel.Visible = true;

            foreach (var col in summaryDefaultNumericColumns)
            {
                if (!columnAggregations.ContainsKey(col))
                    columnAggregations[col] = "Sum";
            }

            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type) { Tag = type };
                item.Click += (s, e) =>
                {
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
                    UpdateFooterValues();
                };
                panelMenu.Items.Add(item);
            }
            gridFooterPanel.ClientArea.ContextMenuStrip = panelMenu;
        }

        private void UpdateFooterCellPositions()
        {
            if (gridFooterPanel == null || ultraGridMaster == null ||
                ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            gridFooterPanel.ClientArea.SuspendLayout();

            var band = ultraGridMaster.DisplayLayout.Bands[0];
            int panelScreenX = gridFooterPanel.PointToScreen(Point.Empty).X;

            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (col.IsChaptered) continue;

                if (!footerLabels.TryGetValue(col.Key, out UltraLabel lbl))
                {
                    lbl = new UltraLabel
                    {
                        Name = $"lblFooter_{col.Key}",
                        BorderStyleOuter = UIElementBorderStyle.Solid,
                        UseAppStyling = false,
                        UseOsThemes = DefaultableBoolean.False
                    };
                    lbl.Appearance.BackColor = Color.FromArgb(232, 246, 255);
                    lbl.Appearance.BorderColor = Color.FromArgb(144, 181, 223);
                    lbl.Appearance.ForeColor = Color.FromArgb(18, 49, 102);
                    lbl.Appearance.FontData.Bold = DefaultableBoolean.True;
                    lbl.Appearance.FontData.Name = "Segoe UI";
                    lbl.Appearance.FontData.SizeInPoints = 8.5F;
                    lbl.Appearance.TextHAlign = (col.CellAppearance.TextHAlign == HAlign.Right || IsNumericColumn(col)) ? HAlign.Right : HAlign.Left;
                    lbl.Appearance.TextVAlign = VAlign.Middle;
                    lbl.ContextMenuStrip = CreateFooterCellContextMenu(col.Key);

                    footerLabels[col.Key] = lbl;
                    gridFooterPanel.ClientArea.Controls.Add(lbl);
                }

                if (col.Hidden)
                {
                    lbl.Visible = false;
                    continue;
                }

                Rectangle headerRect = GetColHeaderRect(col);
                if (headerRect.Width > 0)
                {
                    int leftOnFooter = headerRect.Left - panelScreenX;
                    int rightOnFooter = leftOnFooter + headerRect.Width;

                    if (rightOnFooter > 0 && leftOnFooter < gridFooterPanel.Width)
                    {
                        lbl.SetBounds(leftOnFooter, 1, headerRect.Width, gridFooterPanel.Height - 2);
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

            gridFooterPanel.ClientArea.ResumeLayout();
        }

        private Rectangle GetColHeaderRect(UltraGridColumn col)
        {
            if (col?.Header == null) return Rectangle.Empty;

            var headerUI = col.Header.GetUIElement();
            if (headerUI != null)
            {
                Point screenPt = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                return new Rectangle(screenPt, headerUI.Rect.Size);
            }

            if (ultraGridMaster.Rows.Count > 0)
            {
                var firstVisibleRow = ultraGridMaster.Rows.GetFilteredInNonGroupByRows().FirstOrDefault();
                if (firstVisibleRow != null)
                {
                    var cell = firstVisibleRow.Cells[col];
                    var cellUI = cell?.GetUIElement();
                    if (cellUI != null)
                    {
                        Point screenPt = cellUI.Control.PointToScreen(cellUI.Rect.Location);
                        return new Rectangle(screenPt, cellUI.Rect.Size);
                    }
                }
            }

            return Rectangle.Empty;
        }

        private void UpdateFooterValues()
        {
            if (ultraGridMaster == null || ultraGridMaster.DataSource == null) return;

            DataTable dt = null;
            if (ultraGridMaster.DataSource is DataSet ds && ds.Tables.Contains("SalesReturnMaster"))
            {
                dt = ds.Tables["SalesReturnMaster"];
            }
            else if (ultraGridMaster.DataSource is DataTable d)
            {
                dt = d;
            }
            if (dt == null) return;

            var visibleGridRows = ultraGridMaster.Rows.GetFilteredInNonGroupByRows();
            var rowList = new List<DataRow>();
            foreach (var gr in visibleGridRows)
            {
                if (gr.ListObject is DataRowView drv)
                {
                    rowList.Add(drv.Row);
                }
            }

            if (ultraGridMaster.DisplayLayout.Bands.Count == 0) return;
            var band = ultraGridMaster.DisplayLayout.Bands[0];

            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (col.IsChaptered || !footerLabels.TryGetValue(col.Key, out UltraLabel lbl)) continue;

                string agg = columnAggregations.ContainsKey(col.Key) ? columnAggregations[col.Key] : (IsNumericColumn(col) ? "Sum" : "None");
                if (string.Equals(agg, "None", StringComparison.OrdinalIgnoreCase) || !dt.Columns.Contains(col.Key))
                {
                    lbl.Text = "";
                    continue;
                }

                var values = rowList
                    .Where(r => r[col.Key] != DBNull.Value && !string.IsNullOrEmpty(r[col.Key].ToString()))
                    .Select(r => Convert.ToDecimal(r[col.Key]))
                    .ToList();

                string text = "";
                bool isCurrency = col.Key == "Amount" || col.Key == "SubTotal" || col.Key == "TaxAmt" ||
                                  col.Key == "GrandTotal" || col.Key == "SalesPrice";

                switch (agg)
                {
                    case "Sum":
                        decimal sum = values.Count > 0 ? values.Sum() : 0m;
                        text = isCurrency ? ("₹" + sum.ToString("N2")) : sum.ToString("N2");
                        break;
                    case "Min":
                        decimal min = values.Count > 0 ? values.Min() : 0m;
                        text = isCurrency ? ("Min: ₹" + min.ToString("N2")) : ("Min: " + min.ToString("N2"));
                        break;
                    case "Max":
                        decimal max = values.Count > 0 ? values.Max() : 0m;
                        text = isCurrency ? ("Max: ₹" + max.ToString("N2")) : ("Max: " + max.ToString("N2"));
                        break;
                    case "Average":
                        decimal avg = values.Count > 0 ? values.Average() : 0m;
                        text = isCurrency ? ("Avg: ₹" + avg.ToString("N2")) : ("Avg: " + avg.ToString("N2"));
                        break;
                    case "Count":
                        text = values.Count.ToString();
                        break;
                }
                lbl.Text = text;
            }
        }

        private ContextMenuStrip CreateFooterCellContextMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            bool isNumeric = IsNumericColumnKey(columnKey);

            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type)
                {
                    Tag = type,
                    Enabled = isNumeric || type == "Count" || type == "None"
                };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }

            menu.Opening += (s, e) =>
            {
                string cur = columnAggregations.ContainsKey(columnKey) ? columnAggregations[columnKey] : (isNumeric ? "Sum" : "None");
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = string.Equals((string)item.Tag, cur, StringComparison.OrdinalIgnoreCase);
                }
            };

            return menu;
        }

        private bool IsNumericColumn(UltraGridColumn col)
        {
            if (col == null) return false;
            return IsNumericColumnKey(col.Key) || col.DataType == typeof(decimal) ||
                   col.DataType == typeof(double) || col.DataType == typeof(int) ||
                   col.DataType == typeof(float) || col.DataType == typeof(long);
        }

        private bool IsNumericColumnKey(string colKey)
        {
            return summaryDefaultNumericColumns.Contains(colKey);
        }
        #endregion

        #region Column Chooser & Drag-and-Drop Implementation
        private void SetupHeaderDragToHideAndColumnChooser()
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
                        ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide Column '{col.Header.Caption}'");
                        hideItem.Click += (s, ev) => HideGridColumn(col);
                        menu.Items.Add(hideItem);
                    }
                }

                ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field / Column Chooser...");
                chooserItem.Click += (s, ev) => ShowColumnChooserDialog();
                menu.Items.Add(chooserItem);

                ToolStripMenuItem showAllItem = new ToolStripMenuItem("Show / Unhide All Columns");
                showAllItem.Click += (s, ev) => ShowAllColumns();
                menu.Items.Add(showAllItem);

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
                    bool isDraggingDown = e.Y - headerDragStartPoint.Y > 15;
                    bool isOutside = !ultraGridMaster.ClientRectangle.Contains(e.Location);

                    if (isDraggingDown || isOutside)
                    {
                        if (blackXCursor == null) blackXCursor = CreateBlackXCursor();
                        ultraGridMaster.Cursor = blackXCursor;
                        gridToolTip.SetToolTip(ultraGridMaster, $"✖ Drag down to hide '{columnToHideByDrag.Header.Caption}' column");
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
                bool isDraggingDown = e.Y - headerDragStartPoint.Y > 20;
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
                        col.Hidden = false;
                        userHiddenColumnKeys.Remove(col.Key);

                        Point clientPt = ultraGridMaster.PointToClient(new Point(e.X, e.Y));
                        UIElement dropElem = ultraGridMaster.DisplayLayout.UIElement.ElementFromPoint(clientPt);
                        HeaderUIElement targetHeader = dropElem as HeaderUIElement ?? dropElem?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                        if (targetHeader?.Header?.Column != null && targetHeader.Header.Column.Band.Index == item.BandIndex)
                        {
                            col.Header.VisiblePosition = targetHeader.Header.Column.Header.VisiblePosition;
                        }

                        UpdateFooterValues();
                        UpdateFooterCellPositions();
                        RefreshColumnChooserList();
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

            column.Hidden = true;
            userHiddenColumnKeys.Add(column.Key);

            UpdateFooterValues();
            UpdateFooterCellPositions();
            RefreshColumnChooserList();
        }

        private void ShowAllColumns()
        {
            userHiddenColumnKeys.Clear();
            for (int b = 0; b < ultraGridMaster.DisplayLayout.Bands.Count; b++)
            {
                var band = ultraGridMaster.DisplayLayout.Bands[b];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (IsCustomizableColumn(col, b))
                    {
                        col.Hidden = false;
                    }
                }
            }

            UpdateFooterValues();
            UpdateFooterCellPositions();
            RefreshColumnChooserList();
        }

        private bool IsCustomizableColumn(UltraGridColumn col, int bandIndex)
        {
            if (col == null || col.IsChaptered) return false;
            if (string.Equals(col.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase)) return false;
            if (bandIndex > 0 && string.Equals(col.Key, "SReturnNo", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
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
                    col.Hidden = false;
                    userHiddenColumnKeys.Remove(col.Key);

                    UpdateFooterValues();
                    UpdateFooterCellPositions();
                    RefreshColumnChooserList();
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

        private Cursor CreateBlackXCursor()
        {
            using (Bitmap bmp = new Bitmap(24, 24))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Black, 3.5F))
                {
                    g.DrawLine(pen, 4, 4, 20, 20);
                    g.DrawLine(pen, 20, 4, 4, 20);
                }
                IntPtr ptr = bmp.GetHicon();
                return new Cursor(ptr);
            }
        }

        private void SummaryCards_Resize(object sender, EventArgs e)
        {
            ReflowSummaryCards();
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
