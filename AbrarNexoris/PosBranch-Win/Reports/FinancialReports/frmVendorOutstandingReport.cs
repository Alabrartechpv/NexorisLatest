using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmVendorOutstandingReport : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private readonly VendorOutstandingReportRepository _repository;
        private List<VendorOutstandingReportRow> _reportRows;
        private List<VendorGridList> _vendors;
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private bool _isLoading;
        private bool _isSyncingVendorControls;
        private readonly bool _getUnallocatedReturnsOnly;
        private Panel pnlWarning;
        private Label lblWarning;

        public frmVendorOutstandingReport()
        {
            _repository = new VendorOutstandingReportRepository();
            _reportRows = new List<VendorOutstandingReportRow>();
            _vendors = new List<VendorGridList>();
            _footerLabels = new Dictionary<string, Label>();
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmVendorOutstandingReport_Load;
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnVendorPicker.Click += btnVendorPicker_Click;
            btnVendorFromPicker.Click += btnVendorFromPicker_Click;
            btnVendorToPicker.Click += btnVendorToPicker_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;
            ultraComboVendorMode.ValueChanged += ultraComboVendorMode_ValueChanged;
            ultraComboDateMode.ValueChanged += ultraComboDateMode_ValueChanged;
            ultraComboVendor.ValueChanged += ultraComboVendor_ValueChanged;
            ultraComboVendor.KeyDown += ultraComboVendor_KeyDown;
            txtVendorSearch.TextChanged += txtVendorSearch_TextChanged;
            txtVendorSearch.KeyDown += txtVendorSearch_KeyDown;
            ultraComboVendorFrom.ValueChanged += ultraComboVendorRange_ValueChanged;
            ultraComboVendorTo.ValueChanged += ultraComboVendorRange_ValueChanged;
            chkPaymentDueOnly.CheckedChanged += filter_CheckedChanged;
            
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += gridReport_Resize;

            KeyPreview = true;
            KeyDown += frmVendorOutstandingReport_KeyDown;
        }

        public frmVendorOutstandingReport(bool getUnallocatedReturnsOnly) : this()
        {
            _getUnallocatedReturnsOnly = getUnallocatedReturnsOnly;
        }

        private void frmVendorOutstandingReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = _getUnallocatedReturnsOnly ? "Unallocated Purchase Returns" : "Vendor Outstanding Listing";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                LoadVendors();
                InitializeGridFooter();
                ResetReportView();
                InitializeWarningPanel();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void InitializeFilterControls()
        {
            DateTime today = DateTime.Today;
            dtFrom.Value = new DateTime(today.Year, today.Month, 1);
            dtTo.Value = today;
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditor1.Value = dtFrom.Value;
            ultraDateTimeEditor2.Value = dtTo.Value;
            ultraDateTimeEditor1.MaskInput = "{date}";
            ultraDateTimeEditor2.MaskInput = "{date}";
            ultraDateTimeEditor1.FormatString = "dd/MM/yyyy";
            ultraDateTimeEditor2.FormatString = "dd/MM/yyyy";

            ultraComboVendorMode.Items.Clear();
            ultraComboVendorMode.Items.Add("ALL", "ALL");
            ultraComboVendorMode.Items.Add("SELECTION", "Filter By Selection");
            ultraComboVendorMode.Items.Add("RANGE", "By Range");
            ultraComboVendorMode.Value = "ALL";

            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL");
            ultraComboDateMode.Items.Add("DOC_DATE", "Doc. Date by Range");
            ultraComboDateMode.Items.Add("INV_DATE", "Inv. Date by Range");
            ultraComboDateMode.Items.Add("POST_DATE", "Post. Date by Range");
            ultraComboDateMode.Value = "ALL";

            chkPaymentDueOnly.Checked = false;
            txtVendorSearch.Text = string.Empty;
            ultraComboVendorFrom.Value = 0;
            ultraComboVendorTo.Value = 0;
            UpdateDateControlState();
            UpdateVendorControlState();
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelAction.Appearance.BackColor = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabel(lblVendor);
            StyleLabel(lblVendorSelection);
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
            StyleClassicButton(btnToggleSelection);
            StylePickerButton(btnVendorPicker);
            StylePickerButton(btnVendorFromPicker);
            StylePickerButton(btnVendorToPicker);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton button)
        {
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
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboVendorMode, true);
            StyleFilterCombo(ultraComboDateMode, true);
            StyleFilterCombo(ultraComboVendor, false);
            StyleFilterCombo(ultraComboVendorFrom, false);
            StyleFilterCombo(ultraComboVendorTo, false);
            StyleFilterCombo(txtVendorSearch, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleDateEditor(ultraDateTimeEditor1);
            StyleDateEditor(ultraDateTimeEditor2);
            StyleCheckEditor(chkPaymentDueOnly);
           
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
        {
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleCheckEditor(Infragistics.Win.UltraWinEditors.UltraCheckEditor checkEditor)
        {
            checkEditor.UseAppStyling = false;
            checkEditor.UseOsThemes = DefaultableBoolean.False;
            checkEditor.Appearance.BackColor = Color.Transparent;
            checkEditor.Appearance.ForeColor = Color.Black;
            checkEditor.Appearance.FontData.Name = "Tahoma";
            checkEditor.Appearance.FontData.SizeInPoints = 10;
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

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
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
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
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
        }

        private void LoadVendors()
        {
            _vendors = _repository.GetVendors()
                .Where(x => x != null && x.LedgerID > 0 && (SessionContext.BranchId <= 0 || x.BranchID == SessionContext.BranchId))
                .OrderBy(x => x.LedgerName)
                .ToList();

            ultraComboVendor.Items.Clear();
            ultraComboVendor.Items.Add(0, string.Empty);
            ultraComboVendorFrom.Items.Clear();
            ultraComboVendorFrom.Items.Add(0, string.Empty);
            ultraComboVendorTo.Items.Clear();
            ultraComboVendorTo.Items.Add(0, string.Empty);
            txtVendorSearch.Items.Clear();

            foreach (VendorGridList vendor in _vendors)
            {
                string displayText = GetVendorDisplayText(vendor);
                ultraComboVendor.Items.Add(vendor.LedgerID, displayText);
                ultraComboVendorFrom.Items.Add(vendor.LedgerID, displayText);
                ultraComboVendorTo.Items.Add(vendor.LedgerID, displayText);
                txtVendorSearch.Items.Add("display_" + vendor.LedgerID, displayText);
                txtVendorSearch.Items.Add("id_" + vendor.LedgerID, vendor.LedgerID.ToString());
                txtVendorSearch.Items.Add("name_" + vendor.LedgerID, vendor.LedgerName ?? string.Empty);
            }

            ultraComboVendor.Value = 0;
            ultraComboVendorFrom.Value = 0;
            ultraComboVendorTo.Value = 0;
        }

        private bool ValidatePaymentDueDateRange()
        {
            if (!chkPaymentDueOnly.Checked)
                return true;

            if (ultraDateTimeEditor1.Value == null || ultraDateTimeEditor2.Value == null)
            {
                MessageBox.Show("Please select both the From and To dates for Payment Due.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            DateTime fromDate = Convert.ToDateTime(ultraDateTimeEditor1.Value).Date;
            DateTime toDate = Convert.ToDateTime(ultraDateTimeEditor2.Value).Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("Payment Due From date cannot be greater than To date.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDateTimeEditor1.Focus();
                return false;
            }

            return true;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange() || !ValidatePaymentDueDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                VendorOutstandingReportFilter filter = new VendorOutstandingReportFilter
                {
                    FromDate = chkPaymentDueOnly.Checked ? Convert.ToDateTime(ultraDateTimeEditor1.Value).Date : Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = chkPaymentDueOnly.Checked ? Convert.ToDateTime(ultraDateTimeEditor2.Value).Date : Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId = IsVendorSelectionMode() ? GetSelectedLedgerId() : 0,
                    FromLedgerId = IsVendorRangeMode() ? GetLedgerId(ultraComboVendorFrom) : 0,
                    ToLedgerId = IsVendorRangeMode() ? GetLedgerId(ultraComboVendorTo) : 0,
                    DateFilterMode = chkPaymentDueOnly.Checked ? "DOC_DATE" : Convert.ToString(ultraComboDateMode.Value),
                    UseDateFilter = chkPaymentDueOnly.Checked || IsDateRangeMode(),
                    PaymentDueOnly = chkPaymentDueOnly.Checked,
                    GetUnallocatedReturnsOnly = _getUnallocatedReturnsOnly
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor outstanding listing.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<VendorOutstandingReportRow> filteredRows = _reportRows ?? Enumerable.Empty<VendorOutstandingReportRow>();
            string searchText = GetSearchText();

            if (IsVendorSelectionMode() && GetSelectedLedgerId() > 0)
            {
                int selectedLedgerId = GetSelectedLedgerId();
                filteredRows = filteredRows.Where(x => x.AcctCode == selectedLedgerId);
            }

            if (IsVendorRangeMode())
            {
                int fromLedgerId = GetLedgerId(ultraComboVendorFrom);
                int toLedgerId = GetLedgerId(ultraComboVendorTo);
                if (fromLedgerId > 0 && toLedgerId > 0)
                {
                    int lowerBound = Math.Min(fromLedgerId, toLedgerId);
                    int upperBound = Math.Max(fromLedgerId, toLedgerId);
                    filteredRows = filteredRows.Where(x => x.AcctCode >= lowerBound && x.AcctCode <= upperBound);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    x.AcctCode.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrWhiteSpace(x.Company) && x.Company.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Name) && x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Phone) && x.Phone.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.DocNo) && x.DocNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Reference) && x.Reference.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (chkPaymentDueOnly.Checked)
            {
                filteredRows = filteredRows.Where(x => x.Balance != 0);
            }

            List<VendorOutstandingReportRow> boundRows = filteredRows
                .OrderBy(x => x.Company)
                .ThenBy(x => x.Date)
                .ThenBy(x => x.PurchaseNo)
                .ToList();

            gridReport.DataSource = boundRows;
            if (pnlWarning != null)
            {
                pnlWarning.Visible = boundRows.Any(x => x.IsPR == 1);
                this.PerformLayout();
            }
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(boundRows);
        }

        private bool ValidateDateRange()
        {
            if (!IsDateRangeMode())
                return true;

            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date;

            if (fromDate > toDate)
            {
                MessageBox.Show("From date cannot be greater than to date.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtFrom.Focus();
                return false;
            }

            return true;
        }

        private int GetSelectedLedgerId()
        {
            return GetLedgerId(ultraComboVendor);
        }

        private static int GetLedgerId(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null || combo.Value == null)
                return 0;

            int ledgerId;
            return int.TryParse(combo.Value.ToString(), out ledgerId) ? ledgerId : 0;
        }

        private string GetSearchText()
        {
            return string.IsNullOrWhiteSpace(txtVendorSearch.Text) ? string.Empty : txtVendorSearch.Text.Trim();
        }

        private bool IsVendorSelectionMode()
        {
            return string.Equals(Convert.ToString(ultraComboVendorMode.Value), "SELECTION", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsVendorRangeMode()
        {
            return string.Equals(Convert.ToString(ultraComboVendorMode.Value), "RANGE", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsDateRangeMode()
        {
            string mode = Convert.ToString(ultraComboDateMode.Value);
            return string.Equals(mode, "DOC_DATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "INV_DATE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "POST_DATE", StringComparison.OrdinalIgnoreCase);
        }

        private void ResetReportView()
        {
            gridReport.DataSource = null;
            UpdateFooterValues(new List<VendorOutstandingReportRow>());
        }

        private void UpdateFooterValues(IList<VendorOutstandingReportRow> rows)
        {
            if (_footerLabels.Count == 0)
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

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(new List<VendorOutstandingReportRow>());
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label
                {
                    Name = "footer_" + column.Key,
                    Text = string.Empty,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize = false,
                    Width = column.Width,
                    Height = Math.Max(ultraPanelGridFooter.Height - 2, 20),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };
                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                    _columnAggregations[column.Key] = "None";

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Tag = columnKey };
            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            AddFooterMenuItem(menu, "Sum", "Sum", isNumeric);
            AddFooterMenuItem(menu, "Min", "Min", true);
            AddFooterMenuItem(menu, "Max", "Max", true);
            AddFooterMenuItem(menu, "Count", "Count", true);
            AddFooterMenuItem(menu, "Average", "Avg", isNumeric);
            menu.Items.Add(new ToolStripSeparator());
            AddFooterMenuItem(menu, "None", "None", true);

            menu.Opening += (sender, e) =>
            {
                string current = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";
                foreach (ToolStripItem item in menu.Items)
                {
                    ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                    if (menuItem != null && menuItem.Tag != null)
                        menuItem.Checked = string.Equals(menuItem.Tag.ToString(), current, StringComparison.OrdinalIgnoreCase);
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
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ContextMenuStrip menu = item == null ? null : item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            _columnAggregations[menu.Tag.ToString()] = item.Tag.ToString();
            UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return aggregation == "Count" ? (object)0 : null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue).Sum(value => value.Value);
                case "Min":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderBy(value => value).FirstOrDefault();
                case "Max":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => row.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderByDescending(value => value).FirstOrDefault();
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                case "Avg":
                    List<decimal> values = visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value)).Where(value => value.HasValue)
                        .Select(value => value.Value).ToList();
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

            UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey)
                ? gridReport.DisplayLayout.Bands[0].Columns[columnKey]
                : null;
            decimal? numericValue = GetNumericValue(result);
            if (numericValue.HasValue)
                return column != null && !string.IsNullOrWhiteSpace(column.Format)
                    ? numericValue.Value.ToString(column.Format)
                    : numericValue.Value.ToString("N2");
            return Convert.ToString(result);
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0)
                return;

            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in gridReport.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                footerLabel.Left = xOffset;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                xOffset += column.Width;
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null)
                return false;

            Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
                   type == typeof(int) || type == typeof(long) || type == typeof(short);
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label footerLabel = sender as Label;
            if (footerLabel == null || footerLabel.Tag == null)
                return;

            Tuple<string, string> value = footerLabel.Tag as Tuple<string, string>;
            if (value == null || string.IsNullOrEmpty(value.Item2))
                return;

            using (Pen pen = new Pen(GridFooterBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, footerLabel.Width - 1, footerLabel.Height - 1);
        }

        private void ExportCsv()
        {
            List<VendorOutstandingReportRow> rows = gridReport.DataSource as List<VendorOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = string.Format("VendorOutstandingListing_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();
                string docNoHeader = _getUnallocatedReturnsOnly ? "Return No" : "Doc No";
                string dateHeader = _getUnallocatedReturnsOnly ? "Return Date" : "Date";
                string docAmtHeader = _getUnallocatedReturnsOnly ? "Return Amt" : "Doc Amt";
                string balanceHeader = _getUnallocatedReturnsOnly ? "Unallocated Amt" : "Balance";
                builder.AppendLine($"AcctCode,Company,Name,Phone,{docNoHeader},{dateHeader},Reference,Invoice Date,Post Date,{docAmtHeader},{balanceHeader}");

                foreach (VendorOutstandingReportRow row in rows)
                {
                    builder.AppendLine(string.Join(",",
                        row.AcctCode.ToString(),
                        EscapeCsv(row.Company),
                        EscapeCsv(row.Name),
                        EscapeCsv(row.Phone),
                        EscapeCsv(row.DocNo),
                        EscapeCsv(row.Date == DateTime.MinValue ? string.Empty : row.Date.ToString("yyyy-MM-dd")),
                        EscapeCsv(row.Reference),
                        EscapeCsv(row.InvoiceDate.HasValue ? row.InvoiceDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        EscapeCsv(row.PostDate.HasValue ? row.PostDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                        row.DocAmt.ToString("F2"),
                        row.Balance.ToString("F2")));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export",
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

        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
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

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "AcctCode", "AcctCode", 92, null, HAlign.Left, 0);
            ConfigureGridColumn(band, "Company", "Company", 210, null, HAlign.Left, 1);
            ConfigureGridColumn(band, "Name", "Name", 170, null, HAlign.Left, 2);
            ConfigureGridColumn(band, "Phone", "Phone", 120, null, HAlign.Left, 3);
            ConfigureGridColumn(band, "DocNo", _getUnallocatedReturnsOnly ? "Return No" : "Doc No", 102, null, HAlign.Left, 4);
            ConfigureGridColumn(band, "Date", _getUnallocatedReturnsOnly ? "Return Date" : "Date", 105, "dd-MMM-yyyy", HAlign.Left, 5);
            ConfigureGridColumn(band, "Reference", "Reference", 128, null, HAlign.Left, 6);
            ConfigureGridColumn(band, "InvoiceDate", "Invoice Date", 105, "dd-MMM-yyyy", HAlign.Left, 7);
            ConfigureGridColumn(band, "PostDate", "Post Date", 105, "dd-MMM-yyyy", HAlign.Left, 8);
            ConfigureGridColumn(band, "DocAmt", _getUnallocatedReturnsOnly ? "Return Amt" : "Doc Amt", 95, "#,##0.00", HAlign.Right, 9);
            ConfigureGridColumn(band, "Balance", _getUnallocatedReturnsOnly ? "Unallocated Amt" : "Balance", 95, "#,##0.00", HAlign.Right, 10);

            if (band.Columns.Exists("Company"))
            {
                band.Columns["Company"].CellAppearance.FontData.Bold = DefaultableBoolean.False;
            }

            if (band.Columns.Exists("DocAmt"))
            {
                band.Columns["DocAmt"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (!e.Row.Cells.Exists("Balance"))
                return;

            decimal balance = 0m;
            if (e.Row.Cells["Balance"].Value != null)
            {
                decimal.TryParse(e.Row.Cells["Balance"].Value.ToString(), out balance);
            }

            e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;
            e.Row.Cells["Balance"].Appearance.ForeColor = balance > 0
                ? Color.FromArgb(191, 54, 12)
                : Color.FromArgb(46, 125, 50);
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            ShowReportPreview();
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            ShowReportFormatDialog(
                "VENDOR OUTSTANDING LISTING",
                new[]
                {
                    "VENDOR OUTSTANDING LISTING",
                    "VENDOR OUTSTANDING LISTING - GROUP BY COMPANY",
                    "VENDOR OUTSTANDING LISTING - SUMMARY"
                });
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void btnVendorPicker_Click(object sender, EventArgs e)
        {
            OpenVendorDialog();
        }

        private void btnVendorFromPicker_Click(object sender, EventArgs e)
        {
            OpenVendorRangeDialog(ultraComboVendorFrom);
        }

        private void btnVendorToPicker_Click(object sender, EventArgs e)
        {
            OpenVendorRangeDialog(ultraComboVendorTo);
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void ultraComboVendorMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            UpdateVendorControlState();
        }

        private void ultraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            UpdateDateControlState();
        }

        private void ultraComboVendor_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingVendorControls)
                return;

            SyncSearchFromSelectedVendor();
        }

        private void ultraComboVendorRange_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

        }

        private void txtVendorSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            TrySyncVendorSelectionFromSearchText();
        }

        private void filter_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            UpdateDateControlState();

            if (chkPaymentDueOnly.Checked)
            {
                ValidatePaymentDueDateRange();
            }
        }

        private void frmVendorOutstandingReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                btnExportGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnViewGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
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

        private void ultraComboVendor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenVendorDialog();
                e.Handled = true;
            }
        }

        private void UpdateDateControlState()
        {
            bool dateRange = IsDateRangeMode();
            bool paymentDue = chkPaymentDueOnly.Checked;
            dtFrom.Enabled = dateRange;
            dtTo.Enabled = dateRange;
            lblFromDate.Visible = dateRange;
            lblToDate.Visible = dateRange;
            dtFrom.Visible = dateRange;
            dtTo.Visible = dateRange;
           
            ultraLabel1.Visible = paymentDue;
            ultraLabel2.Visible = paymentDue;
            ultraDateTimeEditor1.Visible = paymentDue;
            ultraDateTimeEditor2.Visible = paymentDue;
        }

        private void UpdateVendorControlState()
        {
            bool selectionMode = IsVendorSelectionMode();
            bool rangeMode = IsVendorRangeMode();
            ultraComboVendor.Enabled = false;
            txtVendorSearch.Enabled = selectionMode;
            btnVendorPicker.Enabled = selectionMode;
            btnVendorFromPicker.Enabled = rangeMode;
            btnVendorToPicker.Enabled = rangeMode;
            ultraComboVendor.Visible = false;
            txtVendorSearch.Visible = selectionMode;
            btnVendorPicker.Visible = selectionMode;
            btnVendorFromPicker.Visible = rangeMode;
            btnVendorToPicker.Visible = rangeMode;
            lblFromVendor.Visible = rangeMode;
            lblToVendor.Visible = rangeMode;
            ultraComboVendorFrom.Visible = rangeMode;
            ultraComboVendorTo.Visible = rangeMode;
            ultraComboVendorFrom.Enabled = rangeMode;
            ultraComboVendorTo.Enabled = rangeMode;

            if (!selectionMode && !rangeMode)
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendor.Value = 0;
                    txtVendorSearch.Text = string.Empty;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }
            }

            if (!rangeMode)
            {
                _isSyncingVendorControls = true;
                try
                {
                    ultraComboVendorFrom.Value = 0;
                    ultraComboVendorTo.Value = 0;
                }
                finally
                {
                    _isSyncingVendorControls = false;
                }
            }
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
        }

        private void ShowReportFormatDialog(string reportCaption, IEnumerable<string> formatDescriptions)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(reportCaption, formatDescriptions))
            {
                dialog.ShowDialog(this);
            }
        }

        private void ShowGridPreview(string title)
        {
            List<VendorOutstandingReportRow> rows = gridReport.DataSource as List<VendorOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview.", "Preview",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = title;
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.WindowState = FormWindowState.Maximized;
                preview.BackColor = FormBackColor;
                previewGrid.Dock = DockStyle.Fill;
                previewGrid.DataSource = rows.ToList();
                previewGrid.InitializeLayout += gridReport_InitializeLayout;
                previewGrid.InitializeRow += gridReport_InitializeRow;
                preview.Controls.Add(previewGrid);
                preview.ShowDialog(this);
            }
        }

        private void ShowReportPreview()
        {
            List<VendorOutstandingReportRow> rows = gridReport.DataSource as List<VendorOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
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
                preview.Text = "Vendor Outstanding Listing - Report Preview";
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
                    Text = "VENDOR OUTSTANDING LISTING",
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
                previewGrid.DataSource = rows.ToList();

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                decimal documentTotal = rows.Sum(x => x.DocAmt);
                decimal balanceTotal = rows.Sum(x => x.Balance);
                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format(_getUnallocatedReturnsOnly 
                        ? "Returns: {0:N0}    |    Return Total: {1:N2}    |    Pending Unallocated: {2:N2}"
                        : "Documents: {0:N0}    |    Document Total: {1:N2}    |    Outstanding Balance: {2:N2}",
                        rows.Count, documentTotal, balanceTotal),
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
            string vendorMode = Convert.ToString(ultraComboVendorMode.Value);
            string dateMode = Convert.ToString(ultraComboDateMode.Value);
            string dateText;

            if (chkPaymentDueOnly.Checked)
            {
                dateText = string.Format("{0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", 
                    Convert.ToDateTime(ultraDateTimeEditor1.Value), 
                    Convert.ToDateTime(ultraDateTimeEditor2.Value));
            }
            else
            {
                dateText = IsDateRangeMode()
                    ? string.Format("{0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}", Convert.ToDateTime(dtFrom.Value), Convert.ToDateTime(dtTo.Value))
                    : "All dates";
            }

            return string.Format("Vendor: {0}    |    Date: {1}    |    Payment Due Only: {2}",
                string.IsNullOrWhiteSpace(vendorMode) ? "All" : vendorMode,
                chkPaymentDueOnly.Checked 
                    ? dateText 
                    : (string.IsNullOrWhiteSpace(dateMode) || string.Equals(dateMode, "ALL", StringComparison.OrdinalIgnoreCase)
                        ? dateText
                        : string.Format("{0} ({1})", dateText, dateMode)),
                chkPaymentDueOnly.Checked ? "Yes" : "No");
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

        private void OpenVendorDialog()
        {
            using (frmVendorDig vendorDialog = new frmVendorDig())
            {
                if (vendorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (vendorDialog.SelectedVendorId <= 0)
                    return;

                ultraComboVendorMode.Value = "SELECTION";
                SelectVendor(vendorDialog.SelectedVendorId, vendorDialog.SelectedVendorName);
            }
        }

        private void OpenVendorRangeDialog(Infragistics.Win.UltraWinEditors.UltraComboEditor targetCombo)
        {
            using (frmVendorDig vendorDialog = new frmVendorDig())
            {
                if (vendorDialog.ShowDialog(this) != DialogResult.OK || vendorDialog.SelectedVendorId <= 0)
                    return;

                targetCombo.Value = vendorDialog.SelectedVendorId;
            }
        }

        private void SelectVendor(int vendorId, string vendorName)
        {
            _isSyncingVendorControls = true;

            try
            {
                ultraComboVendor.Value = vendorId;
                VendorGridList vendor = _vendors.FirstOrDefault(x => x.LedgerID == vendorId);
                txtVendorSearch.Text = vendor != null ? vendor.LedgerName ?? string.Empty : vendorName ?? string.Empty;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private void SyncSearchFromSelectedVendor()
        {
            int selectedLedgerId = GetSelectedLedgerId();
            if (selectedLedgerId <= 0)
                return;

            VendorGridList vendor = _vendors.FirstOrDefault(x => x.LedgerID == selectedLedgerId);
            if (vendor == null)
                return;

            _isSyncingVendorControls = true;
            try
            {
                txtVendorSearch.Text = vendor.LedgerName ?? string.Empty;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private void TrySyncVendorSelectionFromSearchText()
        {
            if (_isSyncingVendorControls || !IsVendorSelectionMode())
                return;

            string searchText = GetSearchText();
            if (string.IsNullOrWhiteSpace(searchText))
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

                return;
            }

            VendorGridList vendor = _vendors.FirstOrDefault(x =>
                x.LedgerID.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(x.LedgerName) && x.LedgerName.Equals(searchText, StringComparison.OrdinalIgnoreCase)) ||
                GetVendorDisplayText(x).Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (vendor == null)
                return;

            _isSyncingVendorControls = true;
            try
            {
                ultraComboVendor.Value = vendor.LedgerID;
            }
            finally
            {
                _isSyncingVendorControls = false;
            }
        }

        private static string GetVendorDisplayText(VendorGridList vendor)
        {
            if (vendor == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(vendor.LedgerName)
                ? vendor.LedgerID.ToString()
                : string.Format("{0} - {1}", vendor.LedgerID, vendor.LedgerName);
        }

        private void InitializeWarningPanel()
        {
            if (_getUnallocatedReturnsOnly) return;

            pnlWarning = new Panel();
            pnlWarning.Dock = DockStyle.Top;
            pnlWarning.Height = 35;
            pnlWarning.BackColor = Color.FromArgb(254, 243, 199);
            pnlWarning.Visible = false;
            pnlWarning.BorderStyle = BorderStyle.FixedSingle;

            lblWarning = new Label();
            lblWarning.Text = "⚠️ There are unallocated purchase returns. Click here to view/allocate them.";
            lblWarning.ForeColor = Color.FromArgb(146, 64, 14);
            lblWarning.Font = new Font("Tahoma", 9.5F, FontStyle.Bold);
            lblWarning.TextAlign = ContentAlignment.MiddleLeft;
            lblWarning.Dock = DockStyle.Fill;
            lblWarning.Cursor = Cursors.Hand;
            lblWarning.Click += (s, e) =>
            {
                try
                {
                    var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (homeForm != null)
                    {
                        var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (openFormInTabMethod != null)
                        {
                            var newForm = new frmVendorOutstandingReport(true);
                            openFormInTabMethod.Invoke(homeForm, new object[] { newForm, "Unallocated Purchase Returns" });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening Unallocated Purchase Returns: " + ex.Message);
                }
            };

            pnlWarning.Controls.Add(lblWarning);
            this.Controls.Add(pnlWarning);
            ultraPanelMaster.BringToFront();
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row == null || !e.Row.IsDataRow) return;

                var row = e.Row.ListObject as VendorOutstandingReportRow;
                if (row == null) return;

                if (row.IsPR == 1)
                {
                    decimal returnAmount = Math.Abs(row.Balance);
                    if (returnAmount <= 0)
                    {
                        MessageBox.Show("This purchase return has already been fully allocated.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var frmDebitNote = new PosBranch_Win.Accounts.FrmDebitNote(
                        Convert.ToInt32(row.PurchaseNo),
                        row.LedgerID,
                        row.Company,
                        returnAmount,
                        row.Reference
                    );

                    var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (homeForm != null)
                    {
                        var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (openFormInTabMethod != null)
                        {
                            openFormInTabMethod.Invoke(homeForm, new object[] { frmDebitNote, $"Debit Note - {row.Company}" });
                            return;
                        }
                    }

                    frmDebitNote.Show();
                    frmDebitNote.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Debit Note: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear()
        {
            ClearForm();
        }

        public void ClearForm()
        {
            _isLoading = true;
            try
            {
                ResetFilterControls(ultraPanelControls);
                UpdateDateControlState();
                UpdateVendorControlState();
                _reportRows = new List<VendorOutstandingReportRow>();
                ResetReportView();
                if (pnlWarning != null)
                {
                    pnlWarning.Visible = false;
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetFilterControls(Control parent)
        {
            if (parent == null)
                return;

            var ultraPanel = parent as Infragistics.Win.Misc.UltraPanel;
            if (ultraPanel != null)
            {
                ResetFilterControls(ultraPanel.ClientArea);
            }

            foreach (Control control in parent.Controls)
            {
                ResetFilterControl(control);
                ResetFilterControls(control);
            }
        }

        private void ResetFilterControl(Control control)
        {
            var combo = control as Infragistics.Win.UltraWinEditors.UltraComboEditor;
            if (combo != null)
            {
                ResetComboToDefault(combo);
                return;
            }

            var textEditor = control as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            if (textEditor != null)
            {
                textEditor.Text = string.Empty;
                return;
            }

            var textBox = control as TextBox;
            if (textBox != null)
            {
                textBox.Text = string.Empty;
                return;
            }

            var checkEditor = control as Infragistics.Win.UltraWinEditors.UltraCheckEditor;
            if (checkEditor != null)
            {
                checkEditor.Checked = false;
                return;
            }

            var checkBox = control as CheckBox;
            if (checkBox != null)
            {
                checkBox.Checked = false;
                return;
            }

            var dateEditor = control as Infragistics.Win.UltraWinEditors.UltraDateTimeEditor;
            if (dateEditor != null)
            {
                dateEditor.Value = DateTime.Today;
            }
        }

        private void ResetComboToDefault(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            foreach (ValueListItem item in combo.Items)
            {
                object value = item.DataValue;
                if (value != null && (string.Equals(Convert.ToString(value), "ALL", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(Convert.ToString(value), "0", StringComparison.OrdinalIgnoreCase)))
                {
                    SetComboValue(combo, value);
                    return;
                }
            }

            SetComboValue(combo, null);
        }

        private void SetComboValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, object value)
        {
            try
            {
                combo.Value = value;
                if (value == null)
                {
                    combo.Text = string.Empty;
                }
            }
            catch
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
            }
        }
    }
}
