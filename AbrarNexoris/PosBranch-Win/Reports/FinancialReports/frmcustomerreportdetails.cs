using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    /// <summary>
    /// Customer Details & Bills Report (frmcustomerreportdetails)
    /// Displays a master-detail hierarchical grid where Band 0 shows 1 customer summary row
    /// with total amounts, and expanding (+) shows the detail bills/invoices under that customer.
    /// Fully styled with the theme, gridReport cell features, and footer features of frmpurchaseAuditreport.
    /// </summary>
    public partial class frmcustomerreportdetails : Form
    {
        #region Colour Palette (Exact match with frmpurchaseAuditreport / frmVendorOutstandingReport)
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
        private static readonly Color ButtonBlueBorder     = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);
        #endregion

        #region Private Fields
        private readonly CustomerOutstandingReportRepository _repository;
        private List<CustomerOutstandingReportRow> _allBills;
        private List<CustomerGridList> _customers;
        private DataSet _currentDataSet;
        private bool _isLoading;
        private bool _isSyncingCustomerControls;

        // Dynamic footer panel and cell controls (matching frmpurchaseAuditreport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalBills",
            "InvoiceAmount",
            "ReceivedAmount",
            "Balance"
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
        private ReportColumnFilterManager _filterManager;
        #endregion

        #region Helper Classes
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

        #region Constructor & Form Lifecycle
        public frmcustomerreportdetails()
        {
            _repository = new CustomerOutstandingReportRepository();
            _allBills = new List<CustomerOutstandingReportRow>();
            _customers = new List<CustomerGridList>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmcustomerreportdetails_Load;
            FormClosing += frmcustomerreportdetails_FormClosing;
            KeyPreview = true;
            KeyDown += frmcustomerreportdetails_KeyDown;

            // Action buttons
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnColumnChooser.Click += btnColumnChooser_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            btnCustomerPicker.Click += btnCustomerPicker_Click;

            // Filters
            ultraComboCustomerMode_Init();
            ultraComboDateMode.ValueChanged += ultraComboDateMode_ValueChanged;
            ultraComboCustomer.ValueChanged += ultraComboCustomer_ValueChanged;
            ultraComboCustomer.KeyDown += ultraComboCustomer_KeyDown;
            txtCustomerSearch.TextChanged += txtCustomerSearch_TextChanged;
            txtCustomerSearch.KeyDown += txtCustomerSearch_KeyDown;
            chkPaymentDueOnly.CheckedChanged += chkPaymentDueOnly_CheckedChanged;
            dtFrom.ValueChanged += dtFrom_ValueChanged;
            dtTo.ValueChanged += dtTo_ValueChanged;

            // Grid events
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            gridReport.AfterRowExpanded += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowCollapsed += (s, e) => UpdateFooterCellPositions();
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                ReportGridLayoutHelper.SaveLayout("CustomerReportDetails", gridReport, userHiddenColumnKeys);
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Resize += (s, e) => UpdateFooterCellPositions();
            }
        }

        private void frmcustomerreportdetails_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmcustomerreportdetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout("CustomerReportDetails", gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _currentDataSet = null;
            }
        }

        private void InitializeForm()
        {
            _isLoading = true;
            try
            {
                Text = "Customer Details & Bills Report";
                if (TopLevel)
                {
                    WindowState = FormWindowState.Maximized;
                    StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    WindowState = FormWindowState.Normal;
                    Dock = DockStyle.Fill;
                }

                InitializeDateControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                LoadCustomers();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
                ReportGridLayoutHelper.LoadHiddenColumns("CustomerReportDetails", userHiddenColumnKeys);
                UpdateDateControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }

            BeginInvoke(new Action(() =>
            {
                LoadReport();
            }));
        }
        #endregion

        #region Filter Controls Setup
        private void ultraComboCustomerMode_Init()
        {
            // Set up presets in Date Mode
            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL Dates");
            ultraComboDateMode.Items.Add("ByRange", "Date by Range");
            ultraComboDateMode.Items.Add("Today", "Today");
            ultraComboDateMode.Items.Add("Yesterday", "Yesterday");
            ultraComboDateMode.Items.Add("ThisMonth", "This Month");
            ultraComboDateMode.Items.Add("LastMonth", "Last Month");
            ultraComboDateMode.Value = "ALL";
        }

        private void InitializeDateControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
            chkPaymentDueOnly.Checked = false;
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;

            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelControls.Dock = DockStyle.Top;
                ultraPanelControls.Height = 84;
            }

            if (ultraPanelAction != null)
            {
                ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelAction.Appearance.BorderColor = BorderBlue;
                ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelAction.Dock = DockStyle.Top;
                ultraPanelAction.Height = 47;
            }

            if (ultraPanelMaster != null)
            {
                ultraPanelMaster.Appearance.BackColor = FormBackColor;
                ultraPanelMaster.Appearance.BorderColor = BorderBlue;
                ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelMaster.Dock = DockStyle.Fill;
            }

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
                ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
                ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelGridFooter.Dock = DockStyle.Bottom;
                ultraPanelGridFooter.Height = 26;
            }

            if (gridReport != null)
            {
                gridReport.Dock = DockStyle.Fill;
                gridReport.BringToFront();
            }

            // Docking order
            ultraPanelControls.SendToBack();
            ultraPanelAction.BringToFront();
            ultraPanelMaster.BringToFront();
            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.SendToBack();
            }
            gridReport.BringToFront();

            StyleLabel(lblCustomer);
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);

            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnExpandAll);
            StyleClassicButton(btnCollapseAll);
            StyleClassicButton(btnToggleSelection);

            StylePickerButton(btnCustomerPicker);
        }

        private static void StyleClassicButton(UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = ButtonBlueTop;
            button.Appearance.BackColor2 = ButtonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.BorderColor = ButtonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;
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

        private static void StylePickerButton(Button button)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button.Cursor = Cursors.Hand;
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(txtCustomerSearch, false);
            StyleFilterCombo(ultraComboCustomer, true);
            StyleFilterCombo(ultraComboDateMode, true);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleCheckEditor(chkPaymentDueOnly);
        }

        private static void StyleLabel(UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9;
            label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleFilterCombo(UltraComboEditor combo, bool isDropDownList)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 9.5F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleDateEditor(UltraDateTimeEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9.5F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleCheckEditor(UltraCheckEditor checkEditor)
        {
            if (checkEditor == null) return;
            checkEditor.UseAppStyling = false;
            checkEditor.UseOsThemes = DefaultableBoolean.False;
            checkEditor.Appearance.BackColor = Color.Transparent;
            checkEditor.Appearance.ForeColor = ControlTextColor;
            checkEditor.Appearance.FontData.Name = "Tahoma";
            checkEditor.Appearance.FontData.SizeInPoints = 9F;
        }
        #endregion

        #region Customers Loading & Synchronization
        private void LoadCustomers()
        {
            try
            {
                _customers = _repository.GetCustomers()
                    .Where(x => x != null && x.LedgerID > 0)
                    .OrderBy(x => x.LedgerName)
                    .ToList();

                ultraComboCustomer.Items.Clear();
                ultraComboCustomer.Items.Add(0, "ALL Customers");

                txtCustomerSearch.Items.Clear();

                foreach (CustomerGridList c in _customers)
                {
                    string display = $"{c.LedgerID} - {c.LedgerName}";
                    ultraComboCustomer.Items.Add(c.LedgerID, display);
                    txtCustomerSearch.Items.Add("id_" + c.LedgerID, c.LedgerID.ToString());
                    txtCustomerSearch.Items.Add("name_" + c.LedgerID, c.LedgerName ?? string.Empty);
                }

                ultraComboCustomer.Value = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            }
        }

        private int GetSelectedCustomerId()
        {
            if (ultraComboCustomer == null || ultraComboCustomer.Value == null)
                return 0;

            int id;
            return int.TryParse(ultraComboCustomer.Value.ToString(), out id) ? id : 0;
        }

        private void ultraComboCustomer_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingCustomerControls) return;

            int custId = GetSelectedCustomerId();
            _isSyncingCustomerControls = true;
            try
            {
                if (custId > 0)
                {
                    CustomerGridList match = _customers.FirstOrDefault(c => c.LedgerID == custId);
                    txtCustomerSearch.Text = match != null ? match.LedgerName ?? string.Empty : string.Empty;
                }
                else
                {
                    txtCustomerSearch.Text = string.Empty;
                }
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }

            BuildAndBindReportData();
        }

        private void txtCustomerSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingCustomerControls) return;

            string text = (txtCustomerSearch.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(text))
            {
                _isSyncingCustomerControls = true;
                try
                {
                    ultraComboCustomer.Value = 0;
                }
                finally
                {
                    _isSyncingCustomerControls = false;
                }
                BuildAndBindReportData();
                return;
            }

            CustomerGridList match = _customers.FirstOrDefault(c =>
                c.LedgerID.ToString().Equals(text, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(c.LedgerName) && c.LedgerName.Equals(text, StringComparison.OrdinalIgnoreCase)));

            if (match != null)
            {
                _isSyncingCustomerControls = true;
                try
                {
                    ultraComboCustomer.Value = match.LedgerID;
                }
                finally
                {
                    _isSyncingCustomerControls = false;
                }
            }
            else
            {
                if (GetSelectedCustomerId() > 0)
                {
                    _isSyncingCustomerControls = true;
                    try
                    {
                        ultraComboCustomer.Value = 0;
                    }
                    finally
                    {
                        _isSyncingCustomerControls = false;
                    }
                }
            }

            BuildAndBindReportData();
        }

        private void ultraComboCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
                e.Handled = true;
            }
        }

        private void txtCustomerSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
                e.Handled = true;
            }
        }

        private void btnCustomerPicker_Click(object sender, EventArgs e)
        {
            OpenCustomerDialog();
        }

        private void OpenCustomerDialog()
        {
            try
            {
                using (frmCustomerDialog dialog = new frmCustomerDialog())
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedCustomerId > 0)
                    {
                        _isSyncingCustomerControls = true;
                        try
                        {
                            ultraComboCustomer.Value = dialog.SelectedCustomerId;
                            txtCustomerSearch.Text = dialog.SelectedCustomerName ?? string.Empty;
                        }
                        finally
                        {
                            _isSyncingCustomerControls = false;
                        }
                        BuildAndBindReportData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening customer lookup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Date Filtering & Presets
        private void ultraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ApplyDatePreset();
            UpdateDateControlVisibility();
            LoadReport();
        }

        private void ApplyDatePreset()
        {
            string mode = Convert.ToString(ultraComboDateMode.Value);
            DateTime today = DateTime.Today;

            switch (mode)
            {
                case "Today":
                    dtFrom.Value = today;
                    dtTo.Value = today;
                    break;
                case "Yesterday":
                    dtFrom.Value = today.AddDays(-1);
                    dtTo.Value = today.AddDays(-1);
                    break;
                case "ThisMonth":
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today;
                    break;
                case "LastMonth":
                    DateTime firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
                    DateTime lastMonth = firstOfThisMonth.AddMonths(-1);
                    dtFrom.Value = lastMonth;
                    dtTo.Value = firstOfThisMonth.AddDays(-1);
                    break;
                case "ByRange":
                    break;
                case "ALL":
                default:
                    break;
            }
        }

        private void UpdateDateControlVisibility()
        {
            string mode = Convert.ToString(ultraComboDateMode.Value);
            bool showRange = !string.Equals(mode, "ALL", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = showRange;
            dtFrom.Visible = showRange;
            lblToDate.Visible = showRange;
            dtTo.Visible = showRange;
        }

        private void chkPaymentDueOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            BuildAndBindReportData();
        }

        private void dtFrom_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            LoadReport();
        }

        private void dtTo_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            LoadReport();
        }
        #endregion

        #region Grid Setup & Layout
        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;
            layout.ViewStyleBand = ViewStyleBand.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.ExpansionIndicator = ShowExpansionIndicator.CheckOnDisplay;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.True;
            layout.Override.FilterUIType = FilterUIType.HeaderIcons;
            layout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            layout.Override.FilterOperandStyle = FilterOperandStyle.Combo;
            layout.Override.FilterClearButtonLocation = FilterClearButtonLocation.Hidden;

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 35;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.SeparateElement;

            layout.Appearance.BackColor = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;

            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;

            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.BorderColor = BorderBlue;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;

            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 20;
            layout.Override.DefaultRowHeight = 22;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;

            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridLayout layout = e.Layout;
            if (layout.Bands.Count == 0) return;

            // Band 0: Customer Master / Summary
            UltraGridBand band0 = layout.Bands[0];
            band0.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band0.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band0.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band0.Override.HeaderAppearance.ForeColor = Color.White;

            ConfigureGridColumn(band0, 0, "CustID", "Cust. ID", 75, null, HAlign.Center, 0);
            ConfigureGridColumn(band0, 0, "CustomerName", "Customer Name", 230, null, HAlign.Left, 1);
            ConfigureGridColumn(band0, 0, "Phone", "Phone / Mobile", 125, null, HAlign.Left, 2);
            ConfigureGridColumn(band0, 0, "Email", "Email", 160, null, HAlign.Left, 3);
            ConfigureGridColumn(band0, 0, "TotalBills", "Total Bills", 90, "#,##0", HAlign.Right, 4);
            ConfigureGridColumn(band0, 0, "InvoiceAmount", "Total Invoice Amt", 135, "#,##0.00", HAlign.Right, 5);
            ConfigureGridColumn(band0, 0, "ReceivedAmount", "Total Received", 135, "#,##0.00", HAlign.Right, 6);
            ConfigureGridColumn(band0, 0, "Balance", "Total Balance", 135, "#,##0.00", HAlign.Right, 7);

            // Band 1: Detail Bills (on clicking +)
            if (layout.Bands.Count > 1)
            {
                UltraGridBand band1 = layout.Bands[1];
                band1.HeaderVisible = true;
                band1.Header.Caption = "Bills & Invoices";
                band1.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                band1.Header.Appearance.ForeColor = Color.White;
                band1.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                band1.Override.RowSelectors = DefaultableBoolean.False;
                band1.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                band1.Override.HeaderAppearance.ForeColor = Color.White;
                band1.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                band1.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);

                if (band1.Columns.Exists("CustID"))
                {
                    band1.Columns["CustID"].Hidden = true;
                    band1.Columns["CustID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band1, 1, "BillNo", "Bill No", 85, null, HAlign.Center, 0);
                ConfigureGridColumn(band1, 1, "DocNo", "Doc / Invoice No", 115, null, HAlign.Left, 1);
                ConfigureGridColumn(band1, 1, "BillDate", "Bill Date", 95, "dd/MM/yyyy", HAlign.Center, 2);
                ConfigureGridColumn(band1, 1, "DueDate", "Due Date", 95, "dd/MM/yyyy", HAlign.Center, 3);
                ConfigureGridColumn(band1, 1, "InvoiceAmount", "Invoice Amount", 125, "#,##0.00", HAlign.Right, 4);
                ConfigureGridColumn(band1, 1, "ReceivedAmount", "Received Amount", 125, "#,##0.00", HAlign.Right, 5);
                ConfigureGridColumn(band1, 1, "Balance", "Balance Amount", 125, "#,##0.00", HAlign.Right, 6);
                ConfigureGridColumn(band1, 1, "SalesPerson", "Salesperson", 140, null, HAlign.Left, 7);

                ConfigureChildBandSummaries(band1, new[] { "InvoiceAmount", "ReceivedAmount", "Balance" });
            }

            ReportGridLayoutHelper.ApplySavedLayout("CustomerReportDetails", gridReport, userHiddenColumnKeys);
        }

        private static void ConfigureChildBandSummaries(UltraGridBand band, IEnumerable<string> columnKeys)
        {
            band.Summaries.Clear();
            band.SummaryFooterCaption = "Totals:";
            band.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;
            band.Override.SummaryFooterCaptionVisible = DefaultableBoolean.True;
            band.Override.SummaryFooterAppearance.BackColor = Color.FromArgb(226, 238, 252);
            band.Override.SummaryFooterAppearance.ForeColor = ControlTextColor;
            band.Override.SummaryFooterAppearance.FontData.Bold = DefaultableBoolean.True;
            band.Override.SummaryFooterAppearance.BorderColor = BorderBlue;

            foreach (string key in columnKeys)
            {
                if (!band.Columns.Exists(key)) continue;
                string summaryKey = $"Sum_{band.Key}_{key}";
                SummarySettings sum = band.Summaries.Add(summaryKey, SummaryType.Sum, band.Columns[key], SummaryPosition.UseSummaryPositionColumn);
                sum.DisplayFormat = "{0:#,##0.00}";
                sum.Appearance.TextHAlign = HAlign.Right;
                sum.Appearance.BackColor = Color.FromArgb(226, 238, 252);
                sum.Appearance.ForeColor = ControlTextColor;
                sum.Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void ConfigureGridColumn(UltraGridBand band, int bandIndex, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
            column.Hidden = userHiddenColumnKeys.Contains(key) || userHiddenColumnKeys.Contains($"{bandIndex}:{key}");
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BorderColor = GridRowLine;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (e.Row.Cells.Exists("Balance"))
            {
                decimal balance = 0m;
                if (e.Row.Cells["Balance"].Value != null && e.Row.Cells["Balance"].Value != DBNull.Value)
                {
                    decimal.TryParse(e.Row.Cells["Balance"].Value.ToString(), out balance);
                }

                e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;
                e.Row.Cells["Balance"].Appearance.ForeColor = balance > 0
                    ? Color.FromArgb(191, 54, 12)
                    : Color.FromArgb(46, 125, 50);
            }
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            if (e.Row.Band.Index == 0)
            {
                e.Row.Expanded = !e.Row.Expanded;
            }
            else if (e.Row.Band.Index == 1)
            {
                // Double-clicking child bill row opens sales invoice
                try
                {
                    if (e.Row.Cells.Exists("BillNo") && e.Row.Cells["BillNo"].Value != null)
                    {
                        long billNo;
                        if (long.TryParse(e.Row.Cells["BillNo"].Value.ToString(), out billNo) && billNo > 0)
                        {
                            Home home = Application.OpenForms.OfType<Home>().FirstOrDefault();
                            if (home != null)
                            {
                                var openMethod = home.GetType().GetMethod("OpenFormInTab",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (openMethod != null)
                                {
                                    PosBranch_Win.Transaction.frmSalesInvoice invoiceForm = new PosBranch_Win.Transaction.frmSalesInvoice();
                                    openMethod.Invoke(home, new object[] { invoiceForm, $"Sales Invoice - {billNo}" });
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
        #endregion

        #region Data Retrieval & DataSet Construction
        private void LoadReport()
        {
            if (_isLoading) return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            _isLoading = true;

            try
            {
                string dateMode = Convert.ToString(ultraComboDateMode.Value);
                bool useDateFilter = !string.Equals(dateMode, "ALL", StringComparison.OrdinalIgnoreCase);

                CustomerOutstandingReportFilter filter = new CustomerOutstandingReportFilter
                {
                    FromDate = (useDateFilter && dtFrom.Value != null) ? Convert.ToDateTime(dtFrom.Value).Date : (DateTime?)null,
                    ToDate = (useDateFilter && dtTo.Value != null) ? Convert.ToDateTime(dtTo.Value).Date : (DateTime?)null,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId = GetSelectedCustomerId(),
                    UseDateFilter = useDateFilter,
                    PaymentDueOnly = chkPaymentDueOnly.Checked
                };

                _allBills = _repository.GetReport(filter) ?? new List<CustomerOutstandingReportRow>();
                BuildAndBindReportData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Customer Details Report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                Cursor = previousCursor;
            }
        }

        private void BuildAndBindReportData()
        {
            _filterManager?.ClearFilters();

            // Client-side filtering
            IEnumerable<CustomerOutstandingReportRow> query = _allBills ?? Enumerable.Empty<CustomerOutstandingReportRow>();

            int selectedCustId = GetSelectedCustomerId();
            if (selectedCustId > 0)
            {
                query = query.Where(b => b.LedgerID == selectedCustId);
            }

            string searchText = (txtCustomerSearch.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(b =>
                    b.LedgerID.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrEmpty(b.LedgerName) && b.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    b.BillNo.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrEmpty(b.DocNo) && b.DocNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.SalesPerson) && b.SalesPerson.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (chkPaymentDueOnly.Checked)
            {
                query = query.Where(b => b.Balance > 0);
            }

            List<CustomerOutstandingReportRow> filteredBills = query.ToList();

            gridReport.DataSource = null;
            gridReport.DataMember = null;

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _currentDataSet = null;
            }

            _currentDataSet = BuildHierarchicalCustomerDataSet(filteredBills);
            gridReport.DataSource = _currentDataSet;
            gridReport.DataMember = "CustomerSummary";

            // If a single customer is selected, auto-expand to show its bills immediately
            if (gridReport.Rows != null && gridReport.Rows.Count == 1)
            {
                gridReport.Rows[0].Expanded = true;
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            _filterManager?.UpdateHeaderFilterIcons();

            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private DataSet BuildHierarchicalCustomerDataSet(List<CustomerOutstandingReportRow> bills)
        {
            DataSet ds = new DataSet("CustomerReportDetailsDataSet");

            DataTable summaryTable = CreateCustomerSummaryTable();
            DataTable billsTable = CreateCustomerBillsTable();

            Dictionary<int, CustomerGridList> customerLookup = _customers
                .GroupBy(c => c.LedgerID)
                .ToDictionary(g => g.Key, g => g.First());

            // Group bills by customer
            var billGroups = bills
                .GroupBy(b => b.LedgerID)
                .OrderBy(g => g.First().LedgerName ?? string.Empty);

            HashSet<int> processedCustomers = new HashSet<int>();

            foreach (var group in billGroups)
            {
                int custId = group.Key;
                processedCustomers.Add(custId);

                string custName = group.First().LedgerName ?? string.Empty;
                string phone = string.Empty;
                string email = string.Empty;

                if (customerLookup.TryGetValue(custId, out CustomerGridList cInfo))
                {
                    if (string.IsNullOrWhiteSpace(custName)) custName = cInfo.LedgerName ?? string.Empty;
                    phone = cInfo.Phone ?? string.Empty;
                    email = cInfo.Email ?? string.Empty;
                }

                int totalBills = group.Count();
                decimal totalInvoiceAmt = group.Sum(b => b.InvoiceAmount);
                decimal totalReceivedAmt = group.Sum(b => b.ReceivedAmount);
                decimal totalBalance = group.Sum(b => b.Balance);

                DataRow sRow = summaryTable.NewRow();
                sRow["CustID"] = custId;
                sRow["CustomerName"] = custName;
                sRow["Phone"] = phone;
                sRow["Email"] = email;
                sRow["TotalBills"] = totalBills;
                sRow["InvoiceAmount"] = totalInvoiceAmt;
                sRow["ReceivedAmount"] = totalReceivedAmt;
                sRow["Balance"] = totalBalance;
                summaryTable.Rows.Add(sRow);

                foreach (CustomerOutstandingReportRow b in group.OrderByDescending(x => x.BillDate).ThenByDescending(x => x.BillNo))
                {
                    DataRow bRow = billsTable.NewRow();
                    bRow["CustID"] = custId;
                    bRow["BillNo"] = b.BillNo;
                    bRow["DocNo"] = b.DocNo ?? string.Empty;
                    if (b.BillDate.HasValue) bRow["BillDate"] = b.BillDate.Value;
                    if (b.DueDate.HasValue) bRow["DueDate"] = b.DueDate.Value;
                    bRow["InvoiceAmount"] = b.InvoiceAmount;
                    bRow["ReceivedAmount"] = b.ReceivedAmount;
                    bRow["Balance"] = b.Balance;
                    bRow["SalesPerson"] = b.SalesPerson ?? string.Empty;
                    billsTable.Rows.Add(bRow);
                }
            }

            // If a specific customer is selected but has 0 bills in this period, still display their summary row
            int selectedCustId = GetSelectedCustomerId();
            if (selectedCustId > 0 && !processedCustomers.Contains(selectedCustId) && customerLookup.TryGetValue(selectedCustId, out CustomerGridList singleCust))
            {
                DataRow sRow = summaryTable.NewRow();
                sRow["CustID"] = singleCust.LedgerID;
                sRow["CustomerName"] = singleCust.LedgerName ?? string.Empty;
                sRow["Phone"] = singleCust.Phone ?? string.Empty;
                sRow["Email"] = singleCust.Email ?? string.Empty;
                sRow["TotalBills"] = 0;
                sRow["InvoiceAmount"] = 0m;
                sRow["ReceivedAmount"] = 0m;
                sRow["Balance"] = 0m;
                summaryTable.Rows.Add(sRow);
            }

            ds.Tables.Add(summaryTable);
            ds.Tables.Add(billsTable);

            ds.Relations.Add(new DataRelation("CustomerBillDetails",
                summaryTable.Columns["CustID"],
                billsTable.Columns["CustID"],
                false));

            ds.AcceptChanges();
            return ds;
        }

        private static DataTable CreateCustomerSummaryTable()
        {
            DataTable dt = new DataTable("CustomerSummary");
            dt.Columns.Add("CustID", typeof(int));
            dt.Columns.Add("CustomerName", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("TotalBills", typeof(int));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("ReceivedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            return dt;
        }

        private static DataTable CreateCustomerBillsTable()
        {
            DataTable dt = new DataTable("CustomerBills");
            dt.Columns.Add("CustID", typeof(int));
            dt.Columns.Add("BillNo", typeof(long));
            dt.Columns.Add("DocNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("ReceivedAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("SalesPerson", typeof(string));
            return dt;
        }
        #endregion

        #region Dynamic Grid Footer & Aggregations
        private void InitializeGridFooter()
        {
            if (ultraPanelGridFooter == null) return;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Height = 26;

            _columnAggregations.Clear();
            foreach (string col in summaryDefaultNumericColumns)
            {
                _columnAggregations[col] = "Sum";
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || column.IsChaptered)
                    continue;

                ContentAlignment align = ContentAlignment.MiddleCenter;
                if (column.CellAppearance.TextHAlign == HAlign.Right)
                    align = ContentAlignment.MiddleRight;
                else if (column.CellAppearance.TextHAlign == HAlign.Left)
                    align = ContentAlignment.MiddleLeft;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = align,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(ultraPanelGridFooter.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
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
            ContextMenuStrip menu = new ContextMenuStrip { Tag = columnKey };
            bool isNumeric = summaryDefaultNumericColumns.Contains(columnKey);

            AddFooterMenuItem(menu, "Sum", "Sum", isNumeric);
            AddFooterMenuItem(menu, "Average", "Avg", isNumeric);
            AddFooterMenuItem(menu, "Minimum", "Min", true);
            AddFooterMenuItem(menu, "Maximum", "Max", true);
            AddFooterMenuItem(menu, "Count", "Count", true);
            AddFooterMenuItem(menu, "None", "None", true);

            menu.Opening += (s, e) =>
            {
                string active = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = string.Equals(item.Tag?.ToString(), active, StringComparison.OrdinalIgnoreCase);
                }
            };

            return menu;
        }

        private void AddFooterMenuItem(ContextMenuStrip menu, string text, string tag, bool enabled)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text) { Tag = tag, Enabled = enabled };
            item.Click += FooterContextMenu_Click;
            menu.Items.Add(item);
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Owner is ContextMenuStrip menu && menu.Tag != null && item.Tag != null)
            {
                _columnAggregations[menu.Tag.ToString()] = item.Tag.ToString();
                UpdateFooterValues();
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True
                ? gridReport.DisplayLayout.Override.RowSelectorWidth
                : 0;

            int scrollOffset = 0;
            if (gridReport.ActiveColScrollRegion != null)
            {
                scrollOffset = gridReport.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || column.IsChaptered || !_footerLabels.ContainsKey(column.Key))
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
                footerLabel.Top = 1;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0 || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
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

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
            {
                return aggregation == "Count" ? (object)0 : null;
            }

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
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows
                        .Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue)
                        .Select(value => value.Value)
                        .ToList();
                    return values.Count == 0 ? 0m : values.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            if (aggregation == "Count")
                return string.Format("{0:N0}", result);

            if (summaryDefaultNumericColumns.Contains(columnKey) && decimal.TryParse(result.ToString(), out decimal decVal))
            {
                return string.Format("{0:#,##0.00}", decVal);
            }

            return result.ToString();
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null)
                return Enumerable.Empty<UltraGridRow>();

            return gridReport.Rows
                .Cast<UltraGridRow>()
                .Where(r => r.IsDataRow && !r.IsFilteredOut && !r.Hidden);
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString());
        }

        private static decimal? GetNumericValue(object value)
        {
            if (!HasCellValue(value)) return null;
            if (decimal.TryParse(value.ToString(), out decimal result)) return result;
            return null;
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser
        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(24, 24))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        using (Pen pen = new Pen(Color.FromArgb(200, 30, 30), 3))
                        {
                            g.DrawLine(pen, 4, 4, 19, 19);
                            g.DrawLine(pen, 19, 4, 4, 19);
                        }
                    }
                    return new Cursor(bmp.GetHicon());
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private bool IsCustomizableColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            if (col.Band != null && col.Band.Index > 0 && string.Equals(col.Key, "CustID", StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.AllowDrop = true;
            _filterManager = new ReportColumnFilterManager(gridReport, () => { UpdateFooterCellPositions(); UpdateFooterValues(); });
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null && IsCustomizableColumn(headerUI.Header.Column))
                {
                    ShowHeaderContextMenu(headerUI.Header.Column, pt);
                    return;
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                Point pt = new Point(e.X, e.Y);
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null && IsCustomizableColumn(headerUI.Header.Column))
                {
                    if (_filterManager != null && _filterManager.HandleMouseDown(e)) { isDraggingHeaderToHide = false; columnBeingDragged = null; return; }
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

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide && _filterManager != null && e.Button != MouseButtons.Left && _filterManager.HandleMouseMove(e, headerToolTip)) return;

            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                int deltaY = e.Y - headerDragStartPoint.Y;
                int deltaX = Math.Abs(e.X - headerDragStartPoint.X);

                if (deltaY > 12 && deltaY > deltaX)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                    headerToolTip.Show("Drag down to hide column", gridReport, e.X + 15, e.Y + 15, 500);
                }
                else
                {
                    isDraggingHeaderToHide = false;
                    Cursor.Current = Cursors.Default;
                    headerToolTip.Hide(gridReport);
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                if (isDraggingHeaderToHide)
                {
                    HideColumn(columnBeingDragged);
                    headerToolTip.Hide(gridReport);
                }
                columnBeingDragged = null;
                isDraggingHeaderToHide = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void GridReport_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void GridReport_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = gridReport.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt, item.BandIndex);
                UnhideColumn(item.BandIndex, item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt, int bandIndex)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count <= bandIndex)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null && headerUI.Header.Column.Band.Index == bandIndex)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[bandIndex];
            return band.Columns.Count;
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null || !IsCustomizableColumn(col)) return;
            int bandIndex = col.Band != null ? col.Band.Index : 0;
            string key = gridReport.DisplayLayout.Bands.Count > 1 ? $"{bandIndex}:{col.Key}" : col.Key;
            userHiddenColumnKeys.Add(key);
            col.Hidden = true;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("CustomerReportDetails", gridReport, userHiddenColumnKeys);
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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col))
            {
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            menu.Items.Add(hideItem);
            _filterManager?.AddFilterContextMenuItems(menu, col, location);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(gridReport, location);
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
                Size = new Size(250, 340),
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
                Height = 25
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 34,
                IntegralHeight = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248)
            };
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(4),
                BackColor = Color.FromArgb(232, 240, 250)
            };
            searchPanel.Controls.Add(txtColumnSearch);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(searchPanel);
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            string filterText = txtColumnSearch?.Text?.Trim() ?? string.Empty;

            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[b];
                string bandPrefix = band.Index == 0 ? "Customer" : "Bills";

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Hidden || !IsCustomizableColumn(col))
                        continue;

                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    string display = gridReport.DisplayLayout.Bands.Count > 1
                        ? $"{caption} ({bandPrefix})"
                        : caption;

                    if (!string.IsNullOrEmpty(filterText) &&
                        display.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) < 0 &&
                        col.Key.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, display, band.Index));
                }
            }
        }

        private void PositionColumnChooser()
        {
            if (columnChooserForm == null) return;
            Point screenPt = PointToScreen(Point.Empty);
            int x = screenPt.X + Width - columnChooserForm.Width - 30;
            int y = screenPt.Y + Height - columnChooserForm.Height - 50;
            columnChooserForm.Location = new Point(Math.Max(0, x), Math.Max(0, y));
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count) return;

            ColumnChooserItem item = columnChooserListBox.Items[e.Index] as ColumnChooserItem;
            if (item == null) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = isSelected ? Color.FromArgb(215, 235, 255) : Color.White;
            Color borderColor = isSelected ? Color.FromArgb(70, 130, 200) : Color.FromArgb(200, 215, 230);
            Color textColor = Color.FromArgb(20, 50, 90);

            using (SolidBrush b = new SolidBrush(Color.FromArgb(240, 244, 248)))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }

            Rectangle cardRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
            using (SolidBrush cardBrush = new SolidBrush(backColor))
            using (Pen borderPen = new Pen(borderColor))
            {
                e.Graphics.FillRectangle(cardBrush, cardRect);
                e.Graphics.DrawRectangle(borderPen, cardRect);
            }

            TextRenderer.DrawText(e.Graphics, item.DisplayText, columnChooserListBox.Font,
                new Rectangle(cardRect.X + 8, cardRect.Y, cardRect.Width - 16, cardRect.Height),
                textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = columnChooserListBox.IndexFromPoint(e.Location);
            if (index >= 0 && index < columnChooserListBox.Items.Count && e.Button == MouseButtons.Left)
            {
                ColumnChooserItem item = columnChooserListBox.Items[index] as ColumnChooserItem;
                if (item != null)
                {
                    columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                }
            }
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                UnhideColumn(item.BandIndex, item.ColumnKey, -1);
            }
        }

        private void UnhideColumn(int bandIndex, string colKey, int targetPosition)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count <= bandIndex)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[bandIndex];
            if (!band.Columns.Exists(colKey)) return;

            UltraGridColumn col = band.Columns[colKey];
            col.Hidden = false;
            userHiddenColumnKeys.Remove(colKey);
            userHiddenColumnKeys.Remove($"{bandIndex}:{colKey}");

            if (targetPosition >= 0)
            {
                col.Header.VisiblePosition = targetPosition;
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("CustomerReportDetails", gridReport, userHiddenColumnKeys);
            PopulateColumnChooserListBox();
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[b];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (IsCustomizableColumn(col))
                    {
                        col.Hidden = false;
                    }
                }
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("CustomerReportDetails", gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }
        #endregion

        #region Button & Control Event Handlers
        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            ShowReportPreview("CUSTOMER DETAILS & BILLS REPORT");
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(
                "CUSTOMER REPORT",
                new[]
                {
                    "CUSTOMER REPORT - SUMMARY & BILL DETAILS",
                    "CUSTOMER REPORT - SUMMARY ONLY",
                    "CUSTOMER REPORT - BILL DETAILS (FLAT)"
                }))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string selectedDesc = dialog.SelectedFormatDescription ?? string.Empty;

                    if (string.Equals(dialog.SelectedAction, "print", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(dialog.SelectedAction, "printer", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintReport();
                    }
                    else
                    {
                        ShowReportPreview(string.IsNullOrWhiteSpace(selectedDesc) ? "CUSTOMER REPORT" : selectedDesc);
                    }
                }
            }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooserForm();
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows != null)
            {
                gridReport.Rows.ExpandAll(true);
                UpdateFooterCellPositions();
            }
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows != null)
            {
                gridReport.Rows.CollapseAll(true);
                UpdateFooterCellPositions();
            }
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            if (btnToggleSelection != null && ultraPanelControls != null)
            {
                btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
            }
        }

        public void ClearForm()
        {
            _isLoading = true;
            try
            {
                ultraComboCustomer.Value = 0;
                txtCustomerSearch.Text = string.Empty;
                ultraComboDateMode.Value = "ALL";
                InitializeDateControls();
                UpdateDateControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void frmcustomerreportdetails_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
        }
        #endregion

        #region Preview, Print & Export
        private void ShowReportPreview(string title)
        {
            if (_currentDataSet == null || _currentDataSet.Tables["CustomerSummary"].Rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (Panel footer = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Customer Report Details - Preview";
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.WindowState = FormWindowState.Maximized;
                preview.MinimumSize = new Size(1024, 600);
                preview.BackColor = FormBackColor;
                preview.Padding = new Padding(10);

                header.Dock = DockStyle.Top;
                header.Height = 72;
                header.BackColor = GridHeaderBlueDark;
                header.Padding = new Padding(18, 10, 18, 8);

                Label titleLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    Text = string.IsNullOrWhiteSpace(title) ? "CUSTOMER DETAILS & BILLS REPORT" : title,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = BuildPreviewSubtitle(),
                    ForeColor = Color.FromArgb(224, 238, 252),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(subtitleLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.BackColor = Color.White;
                previewGrid.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                previewGrid.InitializeLayout += PreviewGrid_InitializeLayout;
                previewGrid.InitializeRow += gridReport_InitializeRow;
                previewGrid.DataSource = _currentDataSet;
                previewGrid.DataMember = "CustomerSummary";

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                DataTable dtSum = _currentDataSet.Tables["CustomerSummary"];
                int totalCusts = dtSum.Rows.Count;
                int totalBills = dtSum.AsEnumerable().Sum(r => r.Field<int>("TotalBills"));
                decimal grandInvoice = dtSum.AsEnumerable().Sum(r => r.Field<decimal>("InvoiceAmount"));
                decimal grandReceived = dtSum.AsEnumerable().Sum(r => r.Field<decimal>("ReceivedAmount"));
                decimal grandBalance = dtSum.AsEnumerable().Sum(r => r.Field<decimal>("Balance"));

                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Customers: {0:N0}    |    Total Bills: {1:N0}    |    Invoice Amt: {2:N2}    |    Received: {3:N2}    |    Balance: {4:N2}",
                        totalCusts, totalBills, grandInvoice, grandReceived, grandBalance),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight
                };
                footer.Controls.Add(footerLabel);

                preview.Controls.Add(previewGrid);
                preview.Controls.Add(footer);
                preview.Controls.Add(header);
                preview.ShowDialog(this);
            }
        }

        private string BuildPreviewSubtitle()
        {
            string dateMode = ultraComboDateMode.Text;
            string dateRange = "ALL Dates";
            if (!string.Equals(Convert.ToString(ultraComboDateMode.Value), "ALL", StringComparison.OrdinalIgnoreCase))
            {
                dateRange = $"{Convert.ToDateTime(dtFrom.Value):dd-MMM-yyyy} to {Convert.ToDateTime(dtTo.Value):dd-MMM-yyyy}";
            }

            string customer = string.IsNullOrWhiteSpace(txtCustomerSearch.Text) ? "ALL Customers" : txtCustomerSearch.Text.Trim();
            string dueFilter = chkPaymentDueOnly.Checked ? "Payment Due Only: YES" : "Payment Due Only: NO";

            return string.Format("Customer: {0}    |    Date: {1} ({2})    |    {3}",
                customer, dateMode, dateRange, dueFilter);
        }

        private void PreviewGrid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            gridReport_InitializeLayout(sender, e);
            e.Layout.GroupByBox.Hidden = true;
            e.Layout.Override.RowSelectors = DefaultableBoolean.False;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            e.Layout.Override.DefaultRowHeight = 23;
            e.Layout.Override.MinRowHeight = 23;
            e.Layout.Override.CellAppearance.FontData.SizeInPoints = 9;
            e.Layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            e.Layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            e.Layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            e.Layout.Override.CellAppearance.BorderColor = GridRowLine;
            e.Layout.Override.RowAppearance.BorderColor = GridRowLine;
            e.Layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
        }

        private void PrintReport()
        {
            if (_currentDataSet == null || _currentDataSet.Tables["CustomerSummary"].Rows.Count == 0)
            {
                MessageBox.Show("No data available to print.", "Print",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (PrintDialog printDialog = new PrintDialog())
                using (PrintDocument printDocument = new PrintDocument())
                {
                    printDocument.DocumentName = "Customer Report Details";
                    DataTable dt = _currentDataSet.Tables["CustomerSummary"];
                    int printIndex = 0;

                    printDocument.PrintPage += (s, e) =>
                    {
                        Font titleFont = new Font("Arial", 14, FontStyle.Bold);
                        Font subFont = new Font("Arial", 9, FontStyle.Regular);
                        Font headerFont = new Font("Arial", 9, FontStyle.Bold);
                        Font dataFont = new Font("Arial", 8.5f, FontStyle.Regular);

                        float y = 40;
                        float left = 40;
                        float right = e.MarginBounds.Right;

                        e.Graphics.DrawString("CUSTOMER DETAILS & BILLS REPORT", titleFont, Brushes.Black, left, y);
                        y += 24;
                        e.Graphics.DrawString(BuildPreviewSubtitle(), subFont, Brushes.DimGray, left, y);
                        y += 24;

                        string[] headers = { "Cust. ID", "Customer Name", "Phone", "Bills", "Invoice Amount", "Received", "Balance" };
                        float[] widths = { 65, 180, 110, 50, 100, 100, 100 };
                        float x = left;
                        for (int i = 0; i < headers.Length; i++)
                        {
                            e.Graphics.DrawString(headers[i], headerFont, Brushes.Black, x, y);
                            x += widths[i];
                        }
                        y += 18;
                        e.Graphics.DrawLine(Pens.Black, left, y, right, y);
                        y += 6;

                        while (printIndex < dt.Rows.Count)
                        {
                            if (y > e.MarginBounds.Bottom - 60)
                            {
                                e.HasMorePages = true;
                                return;
                            }

                            DataRow r = dt.Rows[printIndex++];
                            x = left;
                            string[] vals =
                            {
                                Convert.ToString(r["CustID"]),
                                Convert.ToString(r["CustomerName"]),
                                Convert.ToString(r["Phone"]),
                                Convert.ToString(r["TotalBills"]),
                                Convert.ToDecimal(r["InvoiceAmount"]).ToString("N2"),
                                Convert.ToDecimal(r["ReceivedAmount"]).ToString("N2"),
                                Convert.ToDecimal(r["Balance"]).ToString("N2")
                            };

                            for (int i = 0; i < vals.Length; i++)
                            {
                                e.Graphics.DrawString(vals[i], dataFont, Brushes.Black, x, y);
                                x += widths[i];
                            }
                            y += 18;
                        }

                        e.HasMorePages = false;
                    };

                    printDialog.Document = printDocument;
                    if (printDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        printDocument.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing report: " + ex.Message, "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportCsv()
        {
            if (_currentDataSet == null || _currentDataSet.Tables["CustomerSummary"].Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                dialog.FileName = string.Format("CustomerReportDetails_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Customer Report Details", BuildPreviewSubtitle()))
                    return;

                /* [Legacy export retained for reference]
                builder.AppendLine("CustID,CustomerName,Phone,Email,TotalBills,CustomerInvoiceAmount,CustomerReceivedAmount,CustomerBalance,BillNo,DocNo,BillDate,DueDate,BillInvoiceAmount,BillReceivedAmount,BillBalance,SalesPerson");

                DataTable summary = _currentDataSet.Tables["CustomerSummary"];
                DataTable bills = _currentDataSet.Tables["CustomerBills"];

                foreach (DataRow s in summary.Rows)
                {
                    int custId = Convert.ToInt32(s["CustID"]);
                    DataRow[] childBills = bills.Select($"CustID = {custId}");

                    if (childBills.Length > 0)
                    {
                        foreach (DataRow b in childBills)
                        {
                            builder.AppendLine(string.Join(",",
                                EscapeCsv(s["CustID"].ToString()),
                                EscapeCsv(s["CustomerName"].ToString()),
                                EscapeCsv(s["Phone"].ToString()),
                                EscapeCsv(s["Email"].ToString()),
                                EscapeCsv(s["TotalBills"].ToString()),
                                EscapeCsv(Convert.ToDecimal(s["InvoiceAmount"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(s["ReceivedAmount"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(s["Balance"]).ToString("F2")),
                                EscapeCsv(b["BillNo"].ToString()),
                                EscapeCsv(b["DocNo"].ToString()),
                                b["BillDate"] != DBNull.Value ? EscapeCsv(Convert.ToDateTime(b["BillDate"]).ToString("yyyy-MM-dd")) : "",
                                b["DueDate"] != DBNull.Value ? EscapeCsv(Convert.ToDateTime(b["DueDate"]).ToString("yyyy-MM-dd")) : "",
                                EscapeCsv(Convert.ToDecimal(b["InvoiceAmount"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(b["ReceivedAmount"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(b["Balance"]).ToString("F2")),
                                EscapeCsv(b["SalesPerson"].ToString())));
                        }
                    }
                    else
                    {
                        builder.AppendLine(string.Join(",",
                            EscapeCsv(s["CustID"].ToString()),
                            EscapeCsv(s["CustomerName"].ToString()),
                            EscapeCsv(s["Phone"].ToString()),
                            EscapeCsv(s["Email"].ToString()),
                            EscapeCsv(s["TotalBills"].ToString()),
                            EscapeCsv(Convert.ToDecimal(s["InvoiceAmount"]).ToString("F2")),
                            EscapeCsv(Convert.ToDecimal(s["ReceivedAmount"]).ToString("F2")),
                            EscapeCsv(Convert.ToDecimal(s["Balance"]).ToString("F2")),
                            "", "", "", "", "0.00", "0.00", "0.00", ""));
                    }
                }

                */
                File.WriteAllText(dialog.FileName, BuildCustomerExportCsv().ToString(), Encoding.UTF8);
                MessageBox.Show("Customer Report Details exported successfully.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private StringBuilder BuildCustomerExportCsv()
        {
            StringBuilder builder = new StringBuilder();

            bool anyRowExpanded = gridReport.Rows != null && gridReport.Rows.Cast<UltraGridRow>().Any(r => r.IsDataRow && !r.Hidden && !r.IsFilteredOut && r.Expanded);

            if (!anyRowExpanded)
            {
                // Grid report is NOT expanded: export as same as in gridreport (Band 0 visible columns only)
                var visibleCols = gridReport.DisplayLayout.Bands[0].Columns
                    .Cast<UltraGridColumn>()
                    .Where(c => !c.Hidden && !c.IsChaptered)
                    .OrderBy(c => c.Header.VisiblePosition)
                    .ToList();

                builder.AppendLine(string.Join(",", visibleCols.Select(c => EscapeCsv(!string.IsNullOrEmpty(c.Header.Caption) ? c.Header.Caption : c.Key))));

                foreach (UltraGridRow row in gridReport.Rows)
                {
                    if (!row.IsDataRow || row.Hidden || row.IsFilteredOut) continue;

                    builder.AppendLine(string.Join(",", visibleCols.Select(c =>
                    {
                        if (!row.Cells.Exists(c.Key)) return string.Empty;
                        var cell = row.Cells[c.Key];
                        string text = cell.Text;
                        if (!string.IsNullOrEmpty(text)) return EscapeCsv(text.Replace("₹", "").Trim());
                        return cell.Value != null && cell.Value != DBNull.Value ? EscapeCsv(cell.Value.ToString()) : string.Empty;
                    })));
                }
            }
            else
            {
                // At least one row is expanded: include bill details only for rows that are expanded
                builder.AppendLine("CustID,CustomerName,Phone,Email,TotalBills,CustomerInvoiceAmount,CustomerReceivedAmount,CustomerBalance,BillNo,DocNo,BillDate,DueDate,BillInvoiceAmount,BillReceivedAmount,BillBalance,SalesPerson");

                DataTable bills = _currentDataSet != null && _currentDataSet.Tables.Contains("CustomerBills") ? _currentDataSet.Tables["CustomerBills"] : null;

                foreach (UltraGridRow row in gridReport.Rows)
                {
                    if (!row.IsDataRow || row.Hidden || row.IsFilteredOut) continue;

                    string custIdStr = row.Cells.Exists("CustID") ? Convert.ToString(row.Cells["CustID"].Value) : "0";
                    string custName = row.Cells.Exists("CustomerName") ? Convert.ToString(row.Cells["CustomerName"].Value) : "";
                    string phone = row.Cells.Exists("Phone") ? Convert.ToString(row.Cells["Phone"].Value) : "";
                    string email = row.Cells.Exists("Email") ? Convert.ToString(row.Cells["Email"].Value) : "";
                    string totalBills = row.Cells.Exists("TotalBills") ? Convert.ToString(row.Cells["TotalBills"].Value) : "0";
                    string invAmt = row.Cells.Exists("InvoiceAmount") && row.Cells["InvoiceAmount"].Value != DBNull.Value ? Convert.ToDecimal(row.Cells["InvoiceAmount"].Value).ToString("F2") : "0.00";
                    string recAmt = row.Cells.Exists("ReceivedAmount") && row.Cells["ReceivedAmount"].Value != DBNull.Value ? Convert.ToDecimal(row.Cells["ReceivedAmount"].Value).ToString("F2") : "0.00";
                    string balAmt = row.Cells.Exists("Balance") && row.Cells["Balance"].Value != DBNull.Value ? Convert.ToDecimal(row.Cells["Balance"].Value).ToString("F2") : "0.00";

                    int custId;
                    int.TryParse(custIdStr, out custId);

                    if (row.Expanded && bills != null)
                    {
                        DataRow[] childBills = bills.Select($"CustID = {custId}");
                        if (childBills.Length > 0)
                        {
                            foreach (DataRow b in childBills)
                            {
                                builder.AppendLine(string.Join(",",
                                    EscapeCsv(custIdStr),
                                    EscapeCsv(custName),
                                    EscapeCsv(phone),
                                    EscapeCsv(email),
                                    EscapeCsv(totalBills),
                                    EscapeCsv(invAmt),
                                    EscapeCsv(recAmt),
                                    EscapeCsv(balAmt),
                                    EscapeCsv(b["BillNo"].ToString()),
                                    EscapeCsv(b["DocNo"].ToString()),
                                    b["BillDate"] != DBNull.Value ? EscapeCsv(Convert.ToDateTime(b["BillDate"]).ToString("yyyy-MM-dd")) : "",
                                    b["DueDate"] != DBNull.Value ? EscapeCsv(Convert.ToDateTime(b["DueDate"]).ToString("yyyy-MM-dd")) : "",
                                    EscapeCsv(Convert.ToDecimal(b["InvoiceAmount"]).ToString("F2")),
                                    EscapeCsv(Convert.ToDecimal(b["ReceivedAmount"]).ToString("F2")),
                                    EscapeCsv(Convert.ToDecimal(b["Balance"]).ToString("F2")),
                                    EscapeCsv(b["SalesPerson"].ToString())));
                            }
                            continue;
                        }
                    }

                    builder.AppendLine(string.Join(",",
                        EscapeCsv(custIdStr),
                        EscapeCsv(custName),
                        EscapeCsv(phone),
                        EscapeCsv(email),
                        EscapeCsv(totalBills),
                        EscapeCsv(invAmt),
                        EscapeCsv(recAmt),
                        EscapeCsv(balAmt),
                        "", "", "", "", "0.00", "0.00", "0.00", ""));
                }
            }

            return builder;
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;

            return string.Format("\"{0}\"", safeValue.Replace("\"", "\"\""));
        }
        #endregion
    }
}