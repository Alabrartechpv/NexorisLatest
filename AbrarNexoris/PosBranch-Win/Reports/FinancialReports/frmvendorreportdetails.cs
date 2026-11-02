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
    /// Vendor Details & Bills Report (frmvendorreportdetails)
    /// Displays a master-detail hierarchical grid where Band 0 shows 1 vendor summary row
    /// with total amounts, and expanding (+) shows the detail bills/purchases under that vendor.
    /// Fully styled with the theme, gridReport cell features, and footer features of frmpurchaseAuditreport.
    /// </summary>
    public partial class frmvendorreportdetails : Form
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
        private readonly VendorOutstandingReportRepository _repository;
        private List<VendorOutstandingReportRow> _allBills;
        private List<VendorGridList> _vendors;
        private DataSet _currentDataSet;
        private bool _isLoading;
        private bool _isSyncingVendorControls;

        // Dynamic footer panel and cell controls (matching frmpurchaseAuditreport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalBills",
            "DocAmt",
            "PaidAmount",
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
        public frmvendorreportdetails()
        {
            _repository = new VendorOutstandingReportRepository();
            _allBills = new List<VendorOutstandingReportRow>();
            _vendors = new List<VendorGridList>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmvendorreportdetails_Load;
            FormClosing += frmvendorreportdetails_FormClosing;
            KeyPreview = true;
            KeyDown += frmvendorreportdetails_KeyDown;

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
            btnVendorPicker.Click += btnVendorPicker_Click;

            // Filters
            ultraComboFilterModes_Init();
            ultraComboDateMode.ValueChanged += ultraComboDateMode_ValueChanged;
            ultraComboVendor.ValueChanged += ultraComboVendor_ValueChanged;
            ultraComboVendor.KeyDown += ultraComboVendor_KeyDown;
            txtVendorSearch.TextChanged += txtVendorSearch_TextChanged;
            txtVendorSearch.KeyDown += txtVendorSearch_KeyDown;
            ultraComboPreset.ValueChanged += ultraComboPreset_ValueChanged;
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
                ReportGridLayoutHelper.SaveLayout("VendorReportDetails", gridReport, userHiddenColumnKeys);
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

        private void frmvendorreportdetails_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void frmvendorreportdetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReportGridLayoutHelper.SaveLayout("VendorReportDetails", gridReport, userHiddenColumnKeys);
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
                Text = "Vendor Details & Bills Report";
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
                LoadVendors();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
                ReportGridLayoutHelper.LoadHiddenColumns("VendorReportDetails", userHiddenColumnKeys);
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
        private void ultraComboFilterModes_Init()
        {
            // Set up presets in Date Mode
            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL Dates");
            ultraComboDateMode.Items.Add("ByRange", "Date by Range");
            ultraComboDateMode.Items.Add("Today", "Today");
            ultraComboDateMode.Items.Add("Yesterday", "Yesterday");
            ultraComboDateMode.Items.Add("ThisWeek", "This Week");
            ultraComboDateMode.Items.Add("ThisMonth", "This Month");
            ultraComboDateMode.Items.Add("LastMonth", "Last Month");
            ultraComboDateMode.Value = "ALL";

            // Preset types: All, Purchases Only, Debit Notes / Returns
            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("ALL", "All Types");
            ultraComboPreset.Items.Add("Purchases", "Purchases Only");
            ultraComboPreset.Items.Add("DebitNote", "Debit Notes / PR");
            ultraComboPreset.Value = "ALL";
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
                ultraPanelControls.Height = 88;
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

            StyleLabel(lblVendor);
            StyleLabel(lblPreset);
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

            StylePickerButton(btnVendorPicker);
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
            StyleFilterCombo(txtVendorSearch, false);
            StyleFilterCombo(ultraComboVendor, true);
            StyleFilterCombo(ultraComboPreset, true);
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

        #region Vendors Loading & Synchronization
        private void LoadVendors()
        {
            try
            {
                _vendors = _repository.GetVendors()
                    .Where(x => x != null && x.LedgerID > 0)
                    .OrderBy(x => x.LedgerName)
                    .ToList();

                ultraComboVendor.Items.Clear();
                ultraComboVendor.Items.Add(0, "ALL Vendors");

                txtVendorSearch.Items.Clear();

                foreach (VendorGridList v in _vendors)
                {
                    string display = $"{v.LedgerID} - {v.LedgerName}";
                    ultraComboVendor.Items.Add(v.LedgerID, display);
                    txtVendorSearch.Items.Add("id_" + v.LedgerID, v.LedgerID.ToString());
                    txtVendorSearch.Items.Add("name_" + v.LedgerID, v.LedgerName ?? string.Empty);
                }

                ultraComboVendor.Value = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading vendors: {ex.Message}");
            }
        }

        private int GetSelectedVendorId()
        {
            if (ultraComboVendor == null || ultraComboVendor.Value == null)
                return 0;

            int id;
            return int.TryParse(ultraComboVendor.Value.ToString(), out id) ? id : 0;
        }

        private void ultraComboVendor_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingVendorControls) return;

            int vendorId = GetSelectedVendorId();
            _isSyncingVendorControls = true;
            try
            {
                if (vendorId > 0)
                {
                    VendorGridList match = _vendors.FirstOrDefault(v => v.LedgerID == vendorId);
                    txtVendorSearch.Text = match != null ? match.LedgerName ?? string.Empty : string.Empty;
                }
                else
                {
                    txtVendorSearch.Text = string.Empty;
                }
            }
            finally
            {
                _isSyncingVendorControls = false;
            }

            BuildAndBindReportData();
        }

        private void txtVendorSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingVendorControls) return;

            string text = (txtVendorSearch.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(text))
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendor.Value = 0;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }
                BuildAndBindReportData();
                return;
            }

            VendorGridList match = _vendors.FirstOrDefault(v =>
                v.LedgerID.ToString().Equals(text, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(v.LedgerName) && v.LedgerName.Equals(text, StringComparison.OrdinalIgnoreCase)));

            if (match != null)
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendor.Value = match.LedgerID;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }
            }
            else
            {
                if (GetSelectedVendorId() > 0)
                {
                    _isSyncingVendorControls = true;
                    try
                    {
                        ultraComboVendor.Value = 0;
                    }
                    finally
                    {
                        _isSyncingVendorControls = false;
                    }
                }
            }

            BuildAndBindReportData();
        }

        private void ultraComboVendor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void txtVendorSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void btnVendorPicker_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void OpenVendorDialog()
        {
            try
            {
                using (frmVendorDig dialog = new frmVendorDig())
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedVendorId > 0)
                    {
                        _isSyncingVendorControls = true;
                        try
                        {
                            ultraComboVendor.Value = dialog.SelectedVendorId;
                            txtVendorSearch.Text = dialog.SelectedVendorName ?? string.Empty;
                        }
                        finally
                        {
                            _isSyncingVendorControls = false;
                        }
                        BuildAndBindReportData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening vendor lookup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            BuildAndBindReportData();
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
                case "ThisWeek":
                    int daysFromMonday = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dtFrom.Value = today.AddDays(-daysFromMonday);
                    dtTo.Value = today;
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

            // Band 0: Vendor Master / Summary
            UltraGridBand band0 = layout.Bands[0];
            band0.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            band0.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            band0.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            band0.Override.HeaderAppearance.ForeColor = Color.White;

            ConfigureGridColumn(band0, 0, "VendorID", "Vendor Code", 85, null, HAlign.Center, 0);
            ConfigureGridColumn(band0, 0, "VendorName", "Vendor / Company Name", 250, null, HAlign.Left, 1);
            ConfigureGridColumn(band0, 0, "Phone", "Phone / Contact", 130, null, HAlign.Left, 2);
            ConfigureGridColumn(band0, 0, "TotalBills", "Total Bills", 90, "#,##0", HAlign.Right, 3);
            ConfigureGridColumn(band0, 0, "DocAmt", "Total Bill Amt", 140, "#,##0.00", HAlign.Right, 4);
            ConfigureGridColumn(band0, 0, "PaidAmount", "Total Paid Amt", 140, "#,##0.00", HAlign.Right, 5);
            ConfigureGridColumn(band0, 0, "Balance", "Total Balance", 140, "#,##0.00", HAlign.Right, 6);

            // Band 1: Detail Bills (on clicking +)
            if (layout.Bands.Count > 1)
            {
                UltraGridBand band1 = layout.Bands[1];
                band1.HeaderVisible = true;
                band1.Header.Caption = "Purchases & Invoices";
                band1.Header.Appearance.BackColor = Color.FromArgb(41, 128, 185);
                band1.Header.Appearance.ForeColor = Color.White;
                band1.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                band1.Override.RowSelectors = DefaultableBoolean.False;
                band1.Override.HeaderAppearance.BackColor = Color.FromArgb(70, 130, 180);
                band1.Override.HeaderAppearance.ForeColor = Color.White;
                band1.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                band1.Override.RowAlternateAppearance.BackColor = Color.FromArgb(246, 250, 255);

                if (band1.Columns.Exists("VendorID"))
                {
                    band1.Columns["VendorID"].Hidden = true;
                    band1.Columns["VendorID"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                }

                ConfigureGridColumn(band1, 1, "BillNo", "Bill No", 85, null, HAlign.Center, 0);
                ConfigureGridColumn(band1, 1, "DocNo", "Doc / Return No", 115, null, HAlign.Left, 1);
                ConfigureGridColumn(band1, 1, "BillDate", "Bill Date", 95, "dd/MM/yyyy", HAlign.Center, 2);
                ConfigureGridColumn(band1, 1, "DueDate", "Due Date", 95, "dd/MM/yyyy", HAlign.Center, 3);
                ConfigureGridColumn(band1, 1, "DocAmt", "Bill Amount", 125, "#,##0.00", HAlign.Right, 4);
                ConfigureGridColumn(band1, 1, "PaidAmount", "Paid Amount", 125, "#,##0.00", HAlign.Right, 5);
                ConfigureGridColumn(band1, 1, "Balance", "Balance Amount", 125, "#,##0.00", HAlign.Right, 6);
                ConfigureGridColumn(band1, 1, "Reference", "Reference / Remarks", 150, null, HAlign.Left, 7);

                ConfigureChildBandSummaries(band1, new[] { "DocAmt", "PaidAmount", "Balance" });
            }

            ReportGridLayoutHelper.ApplySavedLayout("VendorReportDetails", gridReport, userHiddenColumnKeys);
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
                // Double-clicking child bill row opens Purchase
                try
                {
                    if (e.Row.Cells.Exists("BillNo") && e.Row.Cells["BillNo"].Value != null)
                    {
                        long purchaseNo;
                        if (long.TryParse(e.Row.Cells["BillNo"].Value.ToString(), out purchaseNo) && purchaseNo > 0)
                        {
                            Home home = Application.OpenForms.OfType<Home>().FirstOrDefault();
                            if (home != null)
                            {
                                var openMethod = home.GetType().GetMethod("OpenFormInTabSafe",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                    ?? home.GetType().GetMethod("OpenFormInTab",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                                if (openMethod != null)
                                {
                                    PosBranch_Win.Transaction.FrmPurchase purchaseForm = new PosBranch_Win.Transaction.FrmPurchase();
                                    openMethod.Invoke(home, new object[] { purchaseForm, $"Purchase - {purchaseNo}" });
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

                VendorOutstandingReportFilter filter = new VendorOutstandingReportFilter
                {
                    FromDate = (useDateFilter && dtFrom.Value != null) ? Convert.ToDateTime(dtFrom.Value).Date : DateTime.MinValue,
                    ToDate = (useDateFilter && dtTo.Value != null) ? Convert.ToDateTime(dtTo.Value).Date : DateTime.MaxValue,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId = GetSelectedVendorId(),
                    UseDateFilter = useDateFilter,
                    PaymentDueOnly = chkPaymentDueOnly.Checked,
                    DateFilterMode = useDateFilter ? "DOC_DATE" : "ALL"
                };

                _allBills = _repository.GetReport(filter) ?? new List<VendorOutstandingReportRow>();
                BuildAndBindReportData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Vendor Details Report.\n" + ex.Message, "Report Error",
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
            IEnumerable<VendorOutstandingReportRow> query = _allBills ?? Enumerable.Empty<VendorOutstandingReportRow>();

            int selectedVendorId = GetSelectedVendorId();
            if (selectedVendorId > 0)
            {
                query = query.Where(b => b.AcctCode == selectedVendorId);
            }

            string preset = Convert.ToString(ultraComboPreset.Value);
            if (string.Equals(preset, "DebitNote", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(b => b.IsPR == 1 || (!string.IsNullOrWhiteSpace(b.DocNo) && b.DocNo.StartsWith("DN", StringComparison.OrdinalIgnoreCase)));
            }
            else if (string.Equals(preset, "Purchases", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(b => b.IsPR == 0 && (string.IsNullOrWhiteSpace(b.DocNo) || !b.DocNo.StartsWith("DN", StringComparison.OrdinalIgnoreCase)));
            }

            string searchText = (txtVendorSearch.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(b =>
                    b.AcctCode.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrEmpty(b.Company) && b.Company.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.Name) && b.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    b.PurchaseNo.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrEmpty(b.DocNo) && b.DocNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(b.Reference) && b.Reference.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (chkPaymentDueOnly.Checked)
            {
                query = query.Where(b => b.Balance > 0);
            }

            List<VendorOutstandingReportRow> filteredBills = query.ToList();

            gridReport.DataSource = null;
            gridReport.DataMember = null;

            if (_currentDataSet != null)
            {
                _currentDataSet.Dispose();
                _currentDataSet = null;
            }

            _currentDataSet = BuildHierarchicalVendorDataSet(filteredBills);
            gridReport.DataSource = _currentDataSet;
            gridReport.DataMember = "VendorSummary";

            // If a single vendor is selected, auto-expand to show its bills immediately
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

        private DataSet BuildHierarchicalVendorDataSet(List<VendorOutstandingReportRow> bills)
        {
            DataSet ds = new DataSet("VendorReportDetailsDataSet");

            DataTable summaryTable = CreateVendorSummaryTable();
            DataTable billsTable = CreateVendorBillsTable();

            Dictionary<int, VendorGridList> vendorLookup = _vendors
                .GroupBy(v => v.LedgerID)
                .ToDictionary(g => g.Key, g => g.First());

            // Group bills by vendor
            var billGroups = bills
                .GroupBy(b => b.AcctCode)
                .OrderBy(g => g.First().Company ?? g.First().Name ?? string.Empty);

            HashSet<int> processedVendors = new HashSet<int>();

            foreach (var group in billGroups)
            {
                int vendorId = group.Key;
                processedVendors.Add(vendorId);

                string vendorName = group.First().Company ?? group.First().Name ?? string.Empty;
                string phone = group.First().Phone ?? string.Empty;

                if (vendorLookup.TryGetValue(vendorId, out VendorGridList vInfo))
                {
                    if (string.IsNullOrWhiteSpace(vendorName)) vendorName = vInfo.LedgerName ?? string.Empty;
                }

                int totalBills = group.Count();
                decimal totalDocAmt = group.Sum(b => b.DocAmt);
                decimal totalPaidAmt = group.Sum(b => b.PaidAmount);
                decimal totalBalance = group.Sum(b => b.Balance);

                DataRow sRow = summaryTable.NewRow();
                sRow["VendorID"] = vendorId;
                sRow["VendorName"] = vendorName;
                sRow["Phone"] = phone;
                sRow["TotalBills"] = totalBills;
                sRow["DocAmt"] = totalDocAmt;
                sRow["PaidAmount"] = totalPaidAmt;
                sRow["Balance"] = totalBalance;
                summaryTable.Rows.Add(sRow);

                foreach (VendorOutstandingReportRow b in group.OrderByDescending(x => x.Date).ThenByDescending(x => x.PurchaseNo))
                {
                    DataRow bRow = billsTable.NewRow();
                    bRow["VendorID"] = vendorId;
                    bRow["BillNo"] = b.PurchaseNo;
                    bRow["DocNo"] = b.DocNo ?? string.Empty;
                    if (b.Date != DateTime.MinValue) bRow["BillDate"] = b.Date;
                    if (b.InvoiceDate.HasValue) bRow["DueDate"] = b.InvoiceDate.Value;
                    else if (b.PostDate.HasValue) bRow["DueDate"] = b.PostDate.Value;
                    bRow["DocAmt"] = b.DocAmt;
                    bRow["PaidAmount"] = b.PaidAmount;
                    bRow["Balance"] = b.Balance;
                    bRow["Reference"] = b.Reference ?? string.Empty;
                    billsTable.Rows.Add(bRow);
                }
            }

            // If a specific vendor is selected but has 0 bills in this period, still display their summary row
            int selectedVendorId = GetSelectedVendorId();
            if (selectedVendorId > 0 && !processedVendors.Contains(selectedVendorId) && vendorLookup.TryGetValue(selectedVendorId, out VendorGridList singleVendor))
            {
                DataRow sRow = summaryTable.NewRow();
                sRow["VendorID"] = singleVendor.LedgerID;
                sRow["VendorName"] = singleVendor.LedgerName ?? string.Empty;
                sRow["Phone"] = string.Empty;
                sRow["TotalBills"] = 0;
                sRow["DocAmt"] = 0m;
                sRow["PaidAmount"] = 0m;
                sRow["Balance"] = 0m;
                summaryTable.Rows.Add(sRow);
            }

            ds.Tables.Add(summaryTable);
            ds.Tables.Add(billsTable);

            ds.Relations.Add(new DataRelation("VendorBillDetails",
                summaryTable.Columns["VendorID"],
                billsTable.Columns["VendorID"],
                false));

            ds.AcceptChanges();
            return ds;
        }

        private static DataTable CreateVendorSummaryTable()
        {
            DataTable dt = new DataTable("VendorSummary");
            dt.Columns.Add("VendorID", typeof(int));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("TotalBills", typeof(int));
            dt.Columns.Add("DocAmt", typeof(decimal));
            dt.Columns.Add("PaidAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            return dt;
        }

        private static DataTable CreateVendorBillsTable()
        {
            DataTable dt = new DataTable("VendorBills");
            dt.Columns.Add("VendorID", typeof(int));
            dt.Columns.Add("BillNo", typeof(long));
            dt.Columns.Add("DocNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("DueDate", typeof(DateTime));
            dt.Columns.Add("DocAmt", typeof(decimal));
            dt.Columns.Add("PaidAmount", typeof(decimal));
            dt.Columns.Add("Balance", typeof(decimal));
            dt.Columns.Add("Reference", typeof(string));
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

            bool isNumeric = summaryDefaultNumericColumns.Contains(columnKey) ||
                             (gridReport.DisplayLayout.Bands.Count > 0 &&
                              gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                              IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]));

            AddFooterMenuItem(menu, "Sum", "Sum", isNumeric);
            AddFooterMenuItem(menu, "Min", "Min", isNumeric);
            AddFooterMenuItem(menu, "Max", "Max", isNumeric);
            AddFooterMenuItem(menu, "Count", "Count", true);
            AddFooterMenuItem(menu, "Average", "Avg", isNumeric);
            menu.Items.Add(new ToolStripSeparator());
            AddFooterMenuItem(menu, "None", "None", true);

            menu.Opening += (sender, e) =>
            {
                string current = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : (summaryDefaultNumericColumns.Contains(columnKey) ? "Sum" : "None");

                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null)
                    {
                        menuItem.Checked = string.Equals(menuItem.Tag.ToString(), current, StringComparison.OrdinalIgnoreCase);
                    }
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
                return Convert.ToString(result);

            if (gridReport.DisplayLayout != null &&
                gridReport.DisplayLayout.Bands.Count > 0 &&
                gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numericValue = GetNumericValue(result);
                if (numericValue.HasValue)
                {
                    string formatted;
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        formatted = numericValue.Value.ToString(column.Format);
                    else
                        formatted = numericValue.Value.ToString("N2");

                    if (aggregation == "Avg") return "Avg: " + formatted;
                    if (aggregation == "Min") return "Min: " + formatted;
                    if (aggregation == "Max") return "Max: " + formatted;
                    return formatted;
                }
            }

            return Convert.ToString(result);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null) yield break;
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut && !row.Hidden)
                {
                    yield return row;
                }
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null &&
                   value != DBNull.Value &&
                   !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            decimal result;
            string clean = Convert.ToString(value).Replace("₹", "").Trim();
            return decimal.TryParse(clean, out result) ? result : (decimal?)null;
        }

        private bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.IsChaptered) return false;
            if (summaryDefaultNumericColumns.Contains(column.Key)) return true;

            Type type = column.DataType != null ? (System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType) : null;
            return type == typeof(decimal) ||
                   type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(short);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser
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

        private bool IsCustomizableColumn(UltraGridColumn col)
        {
            if (col == null || col.IsChaptered) return false;
            if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) return false;
            if (string.Equals(col.Key, "VendorID", StringComparison.OrdinalIgnoreCase) && col.Band != null && col.Band.Index > 0)
                return false;
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
                    columnBeingDragged = headerUI.Header.Column;
                    headerDragStartPoint = pt;
                    isDraggingHeaderToHide = false;
                }
            }
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && columnBeingDragged != null)
            {
                int deltaY = e.Y - headerDragStartPoint.Y;
                int deltaX = Math.Abs(e.X - headerDragStartPoint.X);

                if (deltaY > 20 && deltaY > deltaX)
                {
                    isDraggingHeaderToHide = true;
                    Cursor.Current = blackXCursor;
                    headerToolTip.SetToolTip(gridReport, $"Release to hide '{columnBeingDragged.Header.Caption}'");
                }
                else if (isDraggingHeaderToHide)
                {
                    isDraggingHeaderToHide = false;
                    Cursor.Current = Cursors.Default;
                    headerToolTip.Hide(gridReport);
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide && columnBeingDragged != null)
            {
                HideColumn(columnBeingDragged);
            }

            columnBeingDragged = null;
            isDraggingHeaderToHide = false;
            Cursor.Current = Cursors.Default;
            headerToolTip.Hide(gridReport);
        }

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null || !IsCustomizableColumn(col)) return;

            int bandIndex = col.Band != null ? col.Band.Index : 0;
            string keyToStore = bandIndex > 0 ? $"{bandIndex}:{col.Key}" : col.Key;

            col.Hidden = true;
            userHiddenColumnKeys.Add(keyToStore);

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            ReportGridLayoutHelper.SaveLayout("VendorReportDetails", gridReport, userHiddenColumnKeys);

            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn col, Point screenPoint)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide '{col.Header.Caption}'");
            hideItem.Click += (s, ev) => HideColumn(col);
            menu.Items.Add(hideItem);

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("Column Chooser...");
            chooserItem.Click += (s, ev) => ShowColumnChooserForm();
            menu.Items.Add(chooserItem);

            ToolStripMenuItem restoreAllItem = new ToolStripMenuItem("Show All Columns");
            restoreAllItem.Click += (s, ev) => UnhideAllColumns();
            menu.Items.Add(restoreAllItem);

            menu.Show(gridReport, screenPoint);
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
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                ColumnChooserItem item = e.Data.GetData(typeof(ColumnChooserItem)) as ColumnChooserItem;
                if (item != null)
                {
                    Point clientPt = gridReport.PointToClient(new Point(e.X, e.Y));
                    UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(clientPt);
                    HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                    int targetPos = headerUI?.Header?.Column != null ? headerUI.Header.Column.Header.VisiblePosition : -1;
                    UnhideColumn(item.BandIndex, item.ColumnKey, targetPos);
                }
            }
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.StartPosition = FormStartPosition.Manual;
            Point screenPt = PointToScreen(new Point(Width - columnChooserForm.Width - 30, 160));
            columnChooserForm.Location = screenPt;
            columnChooserForm.Show(this);
            columnChooserForm.BringToFront();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                Size = new Size(250, 360),
                MinimumSize = new Size(200, 250),
                BackColor = FormBackColor,
                ShowInTaskbar = false,
                TopMost = false
            };

            Label lblHint = new Label
            {
                Text = "Drag a column to the grid to display it",
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ControlTextColor,
                Font = new Font("Tahoma", 8.25F, FontStyle.Regular)
            };

            txtColumnSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Tahoma", 9F)
            };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9F),
                ItemHeight = 20,
                DrawMode = DrawMode.OwnerDrawFixed,
                IntegralHeight = false
            };
            columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            Button btnShowAll = new Button
            {
                Text = "Show All",
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = ButtonBlueTop,
                FlatStyle = FlatStyle.Flat
            };
            btnShowAll.Click += (s, e) => UnhideAllColumns();

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(btnShowAll);
            columnChooserForm.Controls.Add(txtColumnSearch);
            columnChooserForm.Controls.Add(lblHint);
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout == null) return;

            string search = (txtColumnSearch?.Text ?? string.Empty).Trim().ToLowerInvariant();
            columnChooserListBox.Items.Clear();

            for (int b = 0; b < gridReport.DisplayLayout.Bands.Count; b++)
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[b];
                string bandPrefix = b > 0 ? "Bill: " : "";

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (IsCustomizableColumn(col) && col.Hidden)
                    {
                        string caption = bandPrefix + (col.Header.Caption ?? col.Key);
                        if (string.IsNullOrEmpty(search) || caption.ToLowerInvariant().Contains(search))
                        {
                            columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption, b));
                        }
                    }
                }
            }
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= columnChooserListBox.Items.Count) return;

            e.DrawBackground();
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color textColor = isSelected ? Color.White : ControlTextColor;
            Color backColor = isSelected ? GridSelectedBlue : Color.White;

            using (SolidBrush b = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }

            ColumnChooserItem item = columnChooserListBox.Items[e.Index] as ColumnChooserItem;
            string text = item?.DisplayText ?? string.Empty;

            TextRenderer.DrawText(e.Graphics, text, e.Font, new Point(e.Bounds.Left + 6, e.Bounds.Top + 2),
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
            ReportGridLayoutHelper.SaveLayout("VendorReportDetails", gridReport, userHiddenColumnKeys);
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
            ReportGridLayoutHelper.SaveLayout("VendorReportDetails", gridReport, userHiddenColumnKeys);
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
            ShowReportPreview("VENDOR DETAILS & BILLS REPORT");
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(
                "VENDOR REPORT",
                new[]
                {
                    "VENDOR REPORT - SUMMARY & BILL DETAILS",
                    "VENDOR REPORT - SUMMARY ONLY",
                    "VENDOR REPORT - BILL DETAILS (FLAT)"
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
                        ShowReportPreview(string.IsNullOrWhiteSpace(selectedDesc) ? "VENDOR REPORT" : selectedDesc);
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
                ultraComboVendor.Value = 0;
                txtVendorSearch.Text = string.Empty;
                ultraComboDateMode.Value = "ALL";
                ultraComboPreset.Value = "ALL";
                InitializeDateControls();
                UpdateDateControlVisibility();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void frmvendorreportdetails_KeyDown(object sender, KeyEventArgs e)
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
            if (_currentDataSet == null || _currentDataSet.Tables["VendorSummary"].Rows.Count == 0)
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
                preview.Text = "Vendor Report Details - Preview";
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
                    Text = string.IsNullOrWhiteSpace(title) ? "VENDOR DETAILS & BILLS REPORT" : title,
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
                previewGrid.DataMember = "VendorSummary";

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                DataTable dtSum = _currentDataSet.Tables["VendorSummary"];
                int totalVendors = dtSum.Rows.Count;
                int totalBills = dtSum.AsEnumerable().Sum(r => r.Field<int>("TotalBills"));
                decimal grandDocAmt = dtSum.AsEnumerable().Sum(r => r.Field<decimal>("DocAmt"));
                decimal grandPaid = dtSum.AsEnumerable().Sum(r => r.Field<decimal>("PaidAmount"));
                decimal grandBalance = dtSum.AsEnumerable().Sum(r => r.Field<decimal>("Balance"));

                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Vendors: {0:N0}    |    Total Bills: {1:N0}    |    Bill Amt: {2:N2}    |    Paid: {3:N2}    |    Balance: {4:N2}",
                        totalVendors, totalBills, grandDocAmt, grandPaid, grandBalance),
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

            string vendor = string.IsNullOrWhiteSpace(txtVendorSearch.Text) ? "ALL Vendors" : txtVendorSearch.Text.Trim();
            string preset = ultraComboPreset.Text;
            string dueFilter = chkPaymentDueOnly.Checked ? "Payment Due Only: YES" : "Payment Due Only: NO";

            return string.Format("Vendor: {0}    |    Type: {1}    |    Date: {2} ({3})    |    {4}",
                vendor, preset, dateMode, dateRange, dueFilter);
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
            if (_currentDataSet == null || _currentDataSet.Tables["VendorSummary"].Rows.Count == 0)
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
                    printDocument.DocumentName = "Vendor Report Details";
                    DataTable dt = _currentDataSet.Tables["VendorSummary"];
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

                        e.Graphics.DrawString("VENDOR DETAILS & BILLS REPORT", titleFont, Brushes.Black, left, y);
                        y += 24;
                        e.Graphics.DrawString(BuildPreviewSubtitle(), subFont, Brushes.DimGray, left, y);
                        y += 24;

                        string[] headers = { "Vendor Code", "Vendor / Company Name", "Phone", "Bills", "Bill Amount", "Paid Amount", "Balance" };
                        float[] widths = { 80, 200, 100, 50, 95, 95, 95 };
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
                                Convert.ToString(r["VendorID"]),
                                Convert.ToString(r["VendorName"]),
                                Convert.ToString(r["Phone"]),
                                Convert.ToString(r["TotalBills"]),
                                Convert.ToDecimal(r["DocAmt"]).ToString("N2"),
                                Convert.ToDecimal(r["PaidAmount"]).ToString("N2"),
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
            if (_currentDataSet == null || _currentDataSet.Tables["VendorSummary"].Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                dialog.FileName = string.Format("VendorReportDetails_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Vendor Report Details", BuildPreviewSubtitle()))
                    return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("VendorID,VendorName,Phone,TotalBills,VendorDocAmt,VendorPaidAmount,VendorBalance,BillNo,DocNo,BillDate,DueDate,BillDocAmt,BillPaidAmount,BillBalance,Reference");

                DataTable summary = _currentDataSet.Tables["VendorSummary"];
                DataTable bills = _currentDataSet.Tables["VendorBills"];

                foreach (DataRow s in summary.Rows)
                {
                    int vendorId = Convert.ToInt32(s["VendorID"]);
                    DataRow[] childBills = bills.Select($"VendorID = {vendorId}");

                    if (childBills.Length > 0)
                    {
                        foreach (DataRow b in childBills)
                        {
                            builder.AppendLine(string.Join(",",
                                EscapeCsv(s["VendorID"].ToString()),
                                EscapeCsv(s["VendorName"].ToString()),
                                EscapeCsv(s["Phone"].ToString()),
                                EscapeCsv(s["TotalBills"].ToString()),
                                EscapeCsv(Convert.ToDecimal(s["DocAmt"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(s["PaidAmount"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(s["Balance"]).ToString("F2")),
                                EscapeCsv(b["BillNo"].ToString()),
                                EscapeCsv(b["DocNo"].ToString()),
                                b["BillDate"] != DBNull.Value ? EscapeCsv(Convert.ToDateTime(b["BillDate"]).ToString("yyyy-MM-dd")) : "",
                                b["DueDate"] != DBNull.Value ? EscapeCsv(Convert.ToDateTime(b["DueDate"]).ToString("yyyy-MM-dd")) : "",
                                EscapeCsv(Convert.ToDecimal(b["DocAmt"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(b["PaidAmount"]).ToString("F2")),
                                EscapeCsv(Convert.ToDecimal(b["Balance"]).ToString("F2")),
                                EscapeCsv(b["Reference"].ToString())));
                        }
                    }
                    else
                    {
                        builder.AppendLine(string.Join(",",
                            EscapeCsv(s["VendorID"].ToString()),
                            EscapeCsv(s["VendorName"].ToString()),
                            EscapeCsv(s["Phone"].ToString()),
                            EscapeCsv(s["TotalBills"].ToString()),
                            EscapeCsv(Convert.ToDecimal(s["DocAmt"]).ToString("F2")),
                            EscapeCsv(Convert.ToDecimal(s["PaidAmount"]).ToString("F2")),
                            EscapeCsv(Convert.ToDecimal(s["Balance"]).ToString("F2")),
                            "", "", "", "", "0.00", "0.00", "0.00", ""));
                    }
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Vendor Report Details exported successfully.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
