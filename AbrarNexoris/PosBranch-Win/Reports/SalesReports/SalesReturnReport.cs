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
        private readonly Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };
        private string currentSummaryType = "Sum";

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
        private bool summaryFooterInitialized;
        private readonly System.Windows.Forms.ToolTip gridToolTip = new System.Windows.Forms.ToolTip();
        private bool isLoading = false;
        private bool isLayoutLoading = false;

        private string LayoutXmlPath => Path.Combine(Application.StartupPath, "SalesReturnReport_GridLayout.xml");
        private string LayoutStatePath => Path.Combine(Application.StartupPath, "SalesReturnReport_LayoutState.txt");
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
            FormClosing += SalesReturnReport_FormClosing;
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

        private void SalesReturnReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayout();
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
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
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
            InitializeSummaryFooterPanel();
            InitializeGridContextMenuAndDragDrop();
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

            foreach (var pnl in panels)
            {
                if (pnl == null) continue;
                pnl.Appearance.BackColor = Color.White;
                pnl.Appearance.BorderColor = SkyBlueOutline;
                pnl.BorderStyle = UIElementBorderStyle.Solid;
            }

            foreach (var title in titles)
            {
                if (title == null) continue;
                title.Appearance.BackColor = Color.Transparent;
                title.Appearance.ForeColor = Color.FromArgb(14, 47, 108);
                title.Appearance.FontData.Name = "Microsoft Sans Serif";
                title.Appearance.FontData.SizeInPoints = 8.25F;
                title.Appearance.FontData.Bold = DefaultableBoolean.True;
                title.Appearance.TextHAlign = HAlign.Center;
                title.Appearance.TextVAlign = VAlign.Middle;
            }

            foreach (var val in values)
            {
                if (val == null) continue;
                val.Appearance.BackColor = Color.Transparent;
                val.Appearance.ForeColor = ControlTextColor;
                val.Appearance.FontData.Name = "Microsoft Sans Serif";
                val.Appearance.FontData.SizeInPoints = 12F;
                val.Appearance.FontData.Bold = DefaultableBoolean.True;
                val.Appearance.TextHAlign = HAlign.Center;
                val.Appearance.TextVAlign = VAlign.Middle;
            }
        }

        private void ConfigureGridAppearance(UltraGrid targetGrid)
        {
            if (targetGrid == null) return;

            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Appearance.BackColor = FormBackColor;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;

            // Hierarchical Master-Detail setup
            targetGrid.DisplayLayout.ViewStyleBand = ViewStyleBand.Vertical;
            targetGrid.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;

            targetGrid.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            targetGrid.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 25;
            targetGrid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;

            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = SkyBlueOutline;
            targetGrid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.CellAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
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

            gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
            gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
            gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
            gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
            gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
            gridFooterPanel.Visible = true;
            gridFooterPanel.Height = 26;

            if (lblCount != null)
            {
                lblCount.Appearance.ForeColor = Color.FromArgb(17, 52, 102);
                lblCount.Appearance.FontData.Bold = DefaultableBoolean.True;
                lblCount.Appearance.BackColor = Color.Transparent;
                lblCount.Location = new Point(8, 3);
                lblCount.Size = new Size(180, 20);
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
        private bool IsCustomizableColumn(UltraGridColumn col, int bandIndex)
        {
            if (col == null) return false;
            if (col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            if (string.Equals(col.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(col.Key, "SlNo", StringComparison.OrdinalIgnoreCase)) return false;
            if (bandIndex > 0 && string.Equals(col.Key, "SReturnNo", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
        #endregion

        #region Lookups and Filters
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

            if (!Controls.Contains(ultraPanelSelection)) Controls.Add(ultraPanelSelection);
            if (!Controls.Contains(ultraPanelActionBar)) Controls.Add(ultraPanelActionBar);
            if (!Controls.Contains(ultraPanelSummaryCards)) Controls.Add(ultraPanelSummaryCards);
            if (!Controls.Contains(ultraPanelGrid)) Controls.Add(ultraPanelGrid);

            Controls.SetChildIndex(ultraPanelSelection, 3);
            Controls.SetChildIndex(ultraPanelActionBar, 2);
            Controls.SetChildIndex(ultraPanelSummaryCards, 1);
            Controls.SetChildIndex(ultraPanelGrid, 0);

            if (ultraPanelGrid.ClientArea != null)
            {
                ultraPanelGrid.ClientArea.SuspendLayout();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 26;
                ultraGridMaster.Dock = DockStyle.Fill;

                if (!ultraPanelGrid.ClientArea.Controls.Contains(gridFooterPanel))
                    ultraPanelGrid.ClientArea.Controls.Add(gridFooterPanel);
                if (!ultraPanelGrid.ClientArea.Controls.Contains(ultraGridMaster))
                    ultraPanelGrid.ClientArea.Controls.Add(ultraGridMaster);

                ultraPanelGrid.ClientArea.Controls.SetChildIndex(gridFooterPanel, 1);
                ultraPanelGrid.ClientArea.Controls.SetChildIndex(ultraGridMaster, 0);

                ultraPanelGrid.ClientArea.ResumeLayout(true);
                ultraPanelGrid.ClientArea.PerformLayout();
            }

            ResumeLayout(true);
            PerformLayout();

            ReflowSummaryCards();
            AlignSummaryLabels();
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

                LoadGridLayout();

                // Update Bottom Summary Cards
                lblCardReturnsValue.Text = $"{finalReturns.Count:N0}";
                lblCardQtyValue.Text = $"{totalQty:N2}";
                lblCardSubTotalValue.Text = $"₹ {totalSubTotal:N2}";
                lblCardTaxValue.Text = $"₹ {totalTax:N2}";
                lblCardGrandTotalValue.Text = $"₹ {totalGrandTotal:N2}";

                lblCount.Text = $"Total Returns: {finalReturns.Count:N0}";

                UpdateSummaryFooter();
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
                // Ensure relation columns and internal IDs are completely excluded from chooser
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
                detailBand.Columns["Qty"].Width = 80;
                detailBand.Columns["Qty"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Qty"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Qty"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("SalesPrice"))
            {
                detailBand.Columns["SalesPrice"].Header.Caption = "Sales Price";
                detailBand.Columns["SalesPrice"].Format = "₹ #,##0.00";
                detailBand.Columns["SalesPrice"].Width = 95;
                detailBand.Columns["SalesPrice"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["SalesPrice"].Header.VisiblePosition = pos++;
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
            if (detailBand.Columns.Exists("Amount"))
            {
                detailBand.Columns["Amount"].Header.Caption = "Amount";
                detailBand.Columns["Amount"].Format = "₹ #,##0.00";
                detailBand.Columns["Amount"].Width = 115;
                detailBand.Columns["Amount"].CellAppearance.TextHAlign = HAlign.Right;
                detailBand.Columns["Amount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                detailBand.Columns["Amount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                detailBand.Columns["Amount"].Header.VisiblePosition = pos++;
            }
            if (detailBand.Columns.Exists("Reason"))
            {
                detailBand.Columns["Reason"].Header.Caption = "Reason";
                detailBand.Columns["Reason"].Width = 150;
                detailBand.Columns["Reason"].Header.VisiblePosition = pos++;
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
                ultraGridMaster.DisplayLayout.SaveAsXml(LayoutXmlPath, PropertyCategories.All);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveAsXml failed: {ex.Message}");
            }

            try
            {
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

                            foreach (UltraGridColumn col in band.Columns)
                            {
                                if (col.IsChaptered || string.Equals(col.Key, "MasterDetail", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(col.Key, "DetailID", StringComparison.OrdinalIgnoreCase) ||
                                    (b > 0 && string.Equals(col.Key, "SReturnNo", StringComparison.OrdinalIgnoreCase)))
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
        private void UpdateSummaryFooter()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGridMaster == null ||
                ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            gridFooterPanel.ClientArea.SuspendLayout();

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
                    ForeColor = Color.FromArgb(17, 52, 102),
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
                                  colKey == "GrandTotal" || colKey == "SalesPrice";
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
            if (ultraPanelSummaryCards == null || ultraPanelSummaryCards.ClientArea == null) return;

            int totalWidth = ultraPanelSummaryCards.ClientArea.Width;
            if (totalWidth <= 0) return;

            Infragistics.Win.Misc.UltraPanel[] cards = new Infragistics.Win.Misc.UltraPanel[]
            {
                pnlCardReturns, pnlCardQty, pnlCardSubTotal, pnlCardTax, pnlCardGrandTotal
            };

            Infragistics.Win.Misc.UltraLabel[] titles = new Infragistics.Win.Misc.UltraLabel[]
            {
                lblCardReturnsTitle, lblCardQtyTitle, lblCardSubTotalTitle, lblCardTaxTitle, lblCardGrandTotalTitle
            };

            Infragistics.Win.Misc.UltraLabel[] values = new Infragistics.Win.Misc.UltraLabel[]
            {
                lblCardReturnsValue, lblCardQtyValue, lblCardSubTotalValue, lblCardTaxValue, lblCardGrandTotalValue
            };

            int count = 5;
            int padding = 12;
            int baseCardWidth = 160;

            int availableWidth = totalWidth - (padding * 2);
            if (availableWidth <= 0) return;

            int gap = 10;
            int computedWidth = (availableWidth - (gap * (count - 1))) / count;
            int cardWidth = Math.Max(baseCardWidth, Math.Min(260, computedWidth));

            int remainingForGaps = availableWidth - (count * cardWidth);
            if (count > 1)
            {
                gap = Math.Max(6, remainingForGaps / (count - 1));
            }

            int cardHeight = 58;
            int yPos = (ultraPanelSummaryCards.ClientArea.Height - cardHeight) / 2;
            if (yPos < 4) yPos = 4;

            int currentX = padding;
            for (int i = 0; i < count; i++)
            {
                if (cards[i] != null)
                {
                    cards[i].Location = new Point(currentX, yPos);
                    cards[i].Size = new Size(cardWidth, cardHeight);
                }
                if (titles[i] != null)
                {
                    titles[i].Location = new Point(4, 4);
                    titles[i].Size = new Size(cardWidth - 8, 16);
                }
                if (values[i] != null)
                {
                    values[i].Location = new Point(4, 22);
                    values[i].Size = new Size(cardWidth - 8, 30);
                }
                currentX += cardWidth + gap;
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
