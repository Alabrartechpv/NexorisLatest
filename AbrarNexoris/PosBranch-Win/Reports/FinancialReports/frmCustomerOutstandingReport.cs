using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
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
    public partial class frmCustomerOutstandingReport : Form
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

        private readonly CustomerOutstandingReportRepository _repository;
        private List<CustomerOutstandingReportRow> _reportRows;
        private List<CustomerGridList> _customers;
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private bool _isLoading;
        private bool _isSyncingCustomerControls;
        private bool _suppressLayoutSave;

        private Form _columnChooserForm;
        private ListBox _columnChooserListBox;
        private Point _dragStartPoint;
        private bool _isDraggingColumn;
        private UltraGridColumn _columnToMove;
        private readonly System.Windows.Forms.ToolTip _toolTip = new System.Windows.Forms.ToolTip();
        private readonly Dictionary<string, int> _savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private const string GRID_LAYOUT_FILE = "CustomerOutstandingReport_GridLayout.xml";
        private string GridLayoutPath => Path.Combine(Application.StartupPath, GRID_LAYOUT_FILE);

        public frmCustomerOutstandingReport()
        {
            _repository = new CustomerOutstandingReportRepository();
            _reportRows = new List<CustomerOutstandingReportRow>();
            _customers = new List<CustomerGridList>();
            _footerLabels = new Dictionary<string, Label>();
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmCustomerOutstandingReport_Load;
            FormClosing += frmCustomerOutstandingReport_FormClosing;
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnColumnChooser.Click += btnColumnChooser_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;

            btnCustomerPicker.Click += btnCustomerPicker_Click;
            btnCustomerFromPicker.Click += btnCustomerFromPicker_Click;
            btnCustomerToPicker.Click += btnCustomerToPicker_Click;

            ultraComboCustomerMode.ValueChanged += ultraComboCustomerMode_ValueChanged;
            ultraComboCustomer.ValueChanged += ultraComboCustomer_ValueChanged;
            ultraComboCustomer.KeyDown += ultraComboCustomer_KeyDown;
            txtCustomerSearch.TextChanged += txtCustomerSearch_TextChanged;
            txtCustomerSearch.KeyDown += txtCustomerSearch_KeyDown;

            ultraComboDateMode.ValueChanged += ultraComboDateMode_ValueChanged;
            chkPaymentDueOnly.CheckedChanged += filter_CheckedChanged;

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += gridReport_Resize;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            gridReport.AfterColPosChanged += (s, ev) =>
            {
                if (!_suppressLayoutSave)
                    SaveGridLayout();
                UpdateFooterCellPositions();
                PopulateColumnChooserListBox();
            };
            gridReport.AfterColRegionScroll += (s, ev) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, ev) => UpdateFooterCellPositions();
            gridReport.Paint += (s, ev) => UpdateFooterCellPositions();

            LocationChanged += (s, ev) => PositionColumnChooserAtBottomRight();
            SizeChanged += (s, ev) => PositionColumnChooserAtBottomRight();
            Activated += (s, ev) => PositionColumnChooserAtBottomRight();

            KeyPreview = true;
            KeyDown += frmCustomerOutstandingReport_KeyDown;
        }

        private void frmCustomerOutstandingReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayout();
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed)
            {
                _columnChooserForm.Close();
            }
        }

        private void btnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        private void frmCustomerOutstandingReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "Customer Outstanding Listing";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                LoadCustomers();
                InitializeGridFooter();
                ResetReportView();
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

            ultraComboCustomerMode.Items.Clear();
            ultraComboCustomerMode.Items.Add("ALL", "ALL");
            ultraComboCustomerMode.Items.Add("SELECTION", "Filter By Selection");
            ultraComboCustomerMode.Items.Add("RANGE", "By Range");
            ultraComboCustomerMode.Value = "ALL";

            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL");
            ultraComboDateMode.Items.Add("DOC_DATE", "Doc. Date by Range");
            ultraComboDateMode.Items.Add("INV_DATE", "Inv. Date by Range");
            ultraComboDateMode.Items.Add("POST_DATE", "Post. Date by Range");
            ultraComboDateMode.Value = "ALL";

            chkPaymentDueOnly.Checked = false;
            txtCustomerSearch.Text = string.Empty;
            ultraComboCustomerFrom.Value = 0;
            ultraComboCustomerTo.Value = 0;

            UpdateCustomerControlState();
            UpdateDateControlState();
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

            StyleLabel(lblCustomer);
            StyleLabel(lblFromCustomer);
            StyleLabel(lblToCustomer);
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(ultraLabel1);
            StyleLabel(ultraLabel2);

            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnToggleSelection);

            StylePickerButton(btnCustomerPicker);
            StylePickerButton(btnCustomerFromPicker);
            StylePickerButton(btnCustomerToPicker);
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
            StyleFilterCombo(ultraComboCustomerMode, true);
            StyleFilterCombo(txtCustomerSearch, false);
            StyleFilterCombo(ultraComboCustomer, false);
            StyleFilterCombo(ultraComboCustomerFrom, false);
            StyleFilterCombo(ultraComboCustomerTo, false);

            StyleFilterCombo(ultraComboDateMode, true);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleDateEditor(ultraDateTimeEditor1);
            StyleDateEditor(ultraDateTimeEditor2);
            StyleCheckEditor(chkPaymentDueOnly);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
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
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
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
            editor.Appearance.FontData.SizeInPoints = 10;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleCheckEditor(Infragistics.Win.UltraWinEditors.UltraCheckEditor checkEditor)
        {
            if (checkEditor == null) return;
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
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;

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

            SetupColumnChooserMenu();
        }

        private void LoadCustomers()
        {
            _customers = _repository.GetCustomers()
                .Where(x => x != null && x.LedgerID > 0 && (SessionContext.BranchId <= 0 || x.BranchID == SessionContext.BranchId))
                .OrderBy(x => x.LedgerName)
                .ToList();

            ultraComboCustomer.Items.Clear();
            ultraComboCustomer.Items.Add(0, string.Empty);
            ultraComboCustomerFrom.Items.Clear();
            ultraComboCustomerFrom.Items.Add(0, string.Empty);
            ultraComboCustomerTo.Items.Clear();
            ultraComboCustomerTo.Items.Add(0, string.Empty);
            txtCustomerSearch.Items.Clear();

            foreach (CustomerGridList customer in _customers)
            {
                string displayText = GetCustomerDisplayText(customer);
                ultraComboCustomer.Items.Add(customer.LedgerID, displayText);
                ultraComboCustomerFrom.Items.Add(customer.LedgerID, displayText);
                ultraComboCustomerTo.Items.Add(customer.LedgerID, displayText);
                txtCustomerSearch.Items.Add("display_" + customer.LedgerID, displayText);
                txtCustomerSearch.Items.Add("id_" + customer.LedgerID, customer.LedgerID.ToString());
                txtCustomerSearch.Items.Add("name_" + customer.LedgerID, customer.LedgerName ?? string.Empty);
            }

            ultraComboCustomer.Value = 0;
            ultraComboCustomerFrom.Value = 0;
            ultraComboCustomerTo.Value = 0;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                CustomerOutstandingReportFilter filter = new CustomerOutstandingReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    LedgerId = IsCustomerSelectionMode() ? GetSelectedCustomerId() : 0,
                    FromLedgerId = IsCustomerRangeMode() ? GetComboId(ultraComboCustomerFrom) : 0,
                    ToLedgerId = IsCustomerRangeMode() ? GetComboId(ultraComboCustomerTo) : 0,
                    DateFilterMode = Convert.ToString(ultraComboDateMode.Value),
                    UseDateFilter = IsDateRangeMode(),
                    PaymentDueOnly = chkPaymentDueOnly.Checked
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load customer outstanding listing.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<CustomerOutstandingReportRow> filteredRows = _reportRows ?? Enumerable.Empty<CustomerOutstandingReportRow>();
            string customerSearchText = GetCustomerSearchText();

            // Customer selection filter
            if (IsCustomerSelectionMode() && GetSelectedCustomerId() > 0)
            {
                int selectedCustomerId = GetSelectedCustomerId();
                filteredRows = filteredRows.Where(x => x.LedgerID == selectedCustomerId);
            }

            // Customer range filter
            if (IsCustomerRangeMode())
            {
                int fromId = GetComboId(ultraComboCustomerFrom);
                int toId = GetComboId(ultraComboCustomerTo);
                if (fromId > 0 && toId > 0)
                {
                    int lower = Math.Min(fromId, toId);
                    int upper = Math.Max(fromId, toId);
                    filteredRows = filteredRows.Where(x => x.LedgerID >= lower && x.LedgerID <= upper);
                }
            }

            // Customer & salesperson text search
            if (!string.IsNullOrWhiteSpace(customerSearchText))
            {
                filteredRows = filteredRows.Where(x =>
                    x.LedgerID.ToString().IndexOf(customerSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrWhiteSpace(x.LedgerName) && x.LedgerName.IndexOf(customerSearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    x.BillNo.ToString().IndexOf(customerSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrWhiteSpace(x.SalesPerson) && x.SalesPerson.IndexOf(customerSearchText, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // Positive balance / payment due filter
            if (chkPaymentDueOnly.Checked)
            {
                filteredRows = filteredRows.Where(x => x.Balance > 0);
            }

            List<CustomerOutstandingReportRow> boundRows = filteredRows
                .OrderBy(x => x.LedgerName)
                .ThenBy(x => x.BillDate)
                .ThenBy(x => x.BillNo)
                .ToList();

            _suppressLayoutSave = true;
            try
            {
                gridReport.DataSource = boundRows;
            }
            finally
            {
                _suppressLayoutSave = false;
            }
            ApplySavedLayoutIfAvailable();
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

        private int GetSelectedCustomerId() => GetComboId(ultraComboCustomer);

        private static int GetComboId(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null || combo.Value == null)
                return 0;

            int id;
            return int.TryParse(combo.Value.ToString(), out id) ? id : 0;
        }

        private string GetCustomerSearchText() => string.IsNullOrWhiteSpace(txtCustomerSearch.Text) ? string.Empty : txtCustomerSearch.Text.Trim();

        private bool IsCustomerSelectionMode() => string.Equals(Convert.ToString(ultraComboCustomerMode.Value), "SELECTION", StringComparison.OrdinalIgnoreCase);
        private bool IsCustomerRangeMode() => string.Equals(Convert.ToString(ultraComboCustomerMode.Value), "RANGE", StringComparison.OrdinalIgnoreCase);

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
            UpdateFooterValues(new List<CustomerOutstandingReportRow>());
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            _suppressLayoutSave = true;
            try
            {
                UltraGridLayout layout = e.Layout;
                UltraGridBand band = layout.Bands[0];

                band.Override.HeaderAppearance.BackColor = GridHeaderBlue;
                band.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
                band.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                band.Override.HeaderAppearance.ForeColor = Color.White;
                band.Override.HeaderAppearance.BorderColor = BorderBlue;

                if (band.Columns.Exists("SalesmanId")) { band.Columns["SalesmanId"].Hidden = true; band.Columns["SalesmanId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
                if (band.Columns.Exists("CategoryId")) { band.Columns["CategoryId"].Hidden = true; band.Columns["CategoryId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
                if (band.Columns.Exists("CategoryName")) { band.Columns["CategoryName"].Hidden = true; band.Columns["CategoryName"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
                if (band.Columns.Exists("DocNo")) { band.Columns["DocNo"].Hidden = true; band.Columns["DocNo"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }

                ConfigureColumn(band, "LedgerID", "Cust. ID", 70, HAlign.Center, visiblePosition: 0);
                ConfigureColumn(band, "LedgerName", "Customer Name", 200, HAlign.Left, visiblePosition: 1);
                ConfigureColumn(band, "SalesPerson", "Salesperson", 140, HAlign.Left, visiblePosition: 2);
                ConfigureColumn(band, "BillNo", "Bill No", 80, HAlign.Center, visiblePosition: 3);
                ConfigureColumn(band, "BillDate", "Date", 90, HAlign.Center, "{date}", "dd/MM/yyyy", visiblePosition: 4);
                ConfigureColumn(band, "DueDate", "Due Date", 90, HAlign.Center, "{date}", "dd/MM/yyyy", visiblePosition: 5);
                ConfigureColumn(band, "InvoiceAmount", "Invoice Amount", 120, HAlign.Right, "{currency}", "#,##0.00", visiblePosition: 6);
                ConfigureColumn(band, "ReceivedAmount", "Received Amount", 120, HAlign.Right, "{currency}", "#,##0.00", visiblePosition: 7);
                ConfigureColumn(band, "Balance", "Balance", 120, HAlign.Right, "{currency}", "#,##0.00", visiblePosition: 8);

                SetDefaultAggregation("InvoiceAmount", "Sum");
                SetDefaultAggregation("ReceivedAmount", "Sum");
                SetDefaultAggregation("Balance", "Sum");

                ApplySavedLayoutIfAvailable();
            }
            finally
            {
                _suppressLayoutSave = false;
            }
        }

        private void ConfigureColumn(UltraGridBand band, string key, string caption, int width, HAlign align, string maskInput = null, string format = null, int visiblePosition = -1)
        {
            if (!band.Columns.Exists(key)) return;
            UltraGridColumn col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
            if (visiblePosition >= 0)
            {
                col.Header.VisiblePosition = visiblePosition;
            }
            if (!string.IsNullOrEmpty(maskInput)) col.MaskInput = maskInput;
            if (!string.IsNullOrEmpty(format)) col.Format = format;
        }

        private void SetDefaultAggregation(string columnKey, string aggregationType)
        {
            if (!_columnAggregations.ContainsKey(columnKey))
                _columnAggregations[columnKey] = aggregationType;
        }

        private void gridReport_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row == null || !e.Row.IsDataRow) return;

            var row = e.Row.ListObject as CustomerOutstandingReportRow;
            if (row != null && row.Balance > 0)
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(185, 28, 28);
                e.Row.Cells["Balance"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void gridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row == null || !e.Row.IsDataRow) return;
                var row = e.Row.ListObject as CustomerOutstandingReportRow;
                if (row == null) return;

                if (row.BillNo > 0)
                {
                    var homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
                    if (homeForm != null)
                    {
                        var openFormInTabMethod = homeForm.GetType().GetMethod("OpenFormInTab",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (openFormInTabMethod != null)
                        {
                            var invoiceForm = new PosBranch_Win.Transaction.frmSalesInvoice();
                            openFormInTabMethod.Invoke(homeForm, new object[] { invoiceForm, $"Invoice - {row.BillNo}" });
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Ignore navigation error
            }
        }

        #region Footer & Aggregations

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(new List<CustomerOutstandingReportRow>());
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = gridReport.ActiveColScrollRegion != null ? gridReport.ActiveColScrollRegion.Position : 0;
            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
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
                    Left = calculatedX,
                    Top = 0,
                    Padding = new Padding(4, 0, 4, 0),
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };
                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                    _columnAggregations[column.Key] = "None";

                calculatedX += column.Width;
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0 || ultraPanelGridFooter == null)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = gridReport.ActiveColScrollRegion != null ? gridReport.ActiveColScrollRegion.Position : 0;
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
                footerLabel.Height = ultraPanelGridFooter.Height;
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues(IList<CustomerOutstandingReportRow> rows)
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

        private void AddFooterMenuItem(ContextMenuStrip menu, string text, string aggType, bool enabled)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text)
            {
                Tag = aggType,
                Enabled = enabled
            };
            item.Click += (s, e) =>
            {
                string columnKey = menu.Tag as string;
                if (!string.IsNullOrEmpty(columnKey))
                {
                    _columnAggregations[columnKey] = aggType;
                    List<CustomerOutstandingReportRow> rows = gridReport.DataSource as List<CustomerOutstandingReportRow>;
                    UpdateFooterValues(rows ?? new List<CustomerOutstandingReportRow>());
                }
            };
            menu.Items.Add(item);
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
                   type == typeof(int) || type == typeof(long) || type == typeof(short);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null)
                yield break;

            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row.IsDataRow && !row.Hidden && !row.IsFilteredOut)
                    yield return row;
            }
        }

        private object CalculateAggregation(string columnKey, string aggType, List<UltraGridRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return aggType == "Count" ? (object)0 : null;

            if (string.Equals(aggType, "Count", StringComparison.OrdinalIgnoreCase))
            {
                return rows.Count(r => r.Cells.Exists(columnKey) && HasCellValue(r.Cells[columnKey].Value));
            }

            var values = rows.Where(r => r.Cells.Exists(columnKey))
                             .Select(r => r.Cells[columnKey].Value)
                             .Where(HasCellValue)
                             .ToList();

            if (values.Count == 0) return null;

            if (string.Equals(aggType, "Sum", StringComparison.OrdinalIgnoreCase))
            {
                decimal sum = 0;
                foreach (var v in values)
                {
                    decimal? d = GetNumericValue(v);
                    if (d.HasValue) sum += d.Value;
                }
                return sum;
            }
            if (string.Equals(aggType, "Avg", StringComparison.OrdinalIgnoreCase))
            {
                decimal sum = 0;
                int count = 0;
                foreach (var v in values)
                {
                    decimal? d = GetNumericValue(v);
                    if (d.HasValue) { sum += d.Value; count++; }
                }
                return count > 0 ? (decimal?)(sum / count) : null;
            }
            if (string.Equals(aggType, "Min", StringComparison.OrdinalIgnoreCase))
            {
                decimal min = decimal.MaxValue;
                bool found = false;
                foreach (var v in values)
                {
                    decimal? d = GetNumericValue(v);
                    if (d.HasValue)
                    {
                        if (d.Value < min) min = d.Value;
                        found = true;
                    }
                }
                if (found) return min;

                return values.Cast<IComparable>().OrderBy(v => v).FirstOrDefault();
            }
            if (string.Equals(aggType, "Max", StringComparison.OrdinalIgnoreCase))
            {
                decimal max = decimal.MinValue;
                bool found = false;
                foreach (var v in values)
                {
                    decimal? d = GetNumericValue(v);
                    if (d.HasValue)
                    {
                        if (d.Value > max) max = d.Value;
                        found = true;
                    }
                }
                if (found) return max;

                return values.Cast<IComparable>().OrderByDescending(v => v).FirstOrDefault();
            }

            return null;
        }

        private string FormatAggregationResult(string columnKey, string aggType, object value)
        {
            if (value == null) return string.Empty;

            if (string.Equals(aggType, "Count", StringComparison.OrdinalIgnoreCase))
                return $"{value:N0}";

            decimal? d = GetNumericValue(value);
            if (d.HasValue)
                return $"{d.Value:N2}";

            return $"{value}";
        }

        private static bool HasCellValue(object value)
        {
            return value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;
            ControlPaint.DrawBorder(e.Graphics, lbl.ClientRectangle,
                BorderBlue, 0, ButtonBorderStyle.None,
                BorderBlue, 0, ButtonBorderStyle.None,
                BorderBlue, 1, ButtonBorderStyle.Solid,
                BorderBlue, 0, ButtonBorderStyle.None);
        }

        #endregion

        #region Event Handlers & Syncing

        private void btnViewGrid_Click(object sender, EventArgs e) => LoadReport();

        private void btnPreviewGrid_Click(object sender, EventArgs e) => ShowGridPreview("Customer Outstanding Listing - Grid Preview");

        private void btnPreviewReport_Click(object sender, EventArgs e) => ShowReportPreview();

        private void btnExportGrid_Click(object sender, EventArgs e) => ExportCsv();

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void UpdateSelectionToggleButtonText()
        {
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
        }

        private void ultraComboCustomerMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdateCustomerControlState();
        }

        private void ultraComboDateMode_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdateDateControlState();
        }

        private void filter_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            UpdateDateControlState();
        }

        private void ultraComboCustomer_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingCustomerControls) return;
            SyncSearchFromSelectedCustomer();
        }

        private void txtCustomerSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            TrySyncCustomerSelectionFromSearchText();
        }

        private void btnCustomerPicker_Click(object sender, EventArgs e) => OpenCustomerDialog();
        private void btnCustomerFromPicker_Click(object sender, EventArgs e) => OpenCustomerRangeDialog(ultraComboCustomerFrom);
        private void btnCustomerToPicker_Click(object sender, EventArgs e) => OpenCustomerRangeDialog(ultraComboCustomerTo);

        private void frmCustomerOutstandingReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ClearForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
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

        private void txtCustomerSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ClearForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
                e.Handled = true;
            }
        }

        private void ultraComboCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ClearForm();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
                e.Handled = true;
            }
        }

        private void UpdateCustomerControlState()
        {
            bool selectionMode = IsCustomerSelectionMode();
            bool rangeMode = IsCustomerRangeMode();

            ultraComboCustomer.Visible = false;
            ultraComboCustomer.Enabled = false;

            txtCustomerSearch.Visible = selectionMode;
            txtCustomerSearch.Enabled = selectionMode;
            btnCustomerPicker.Visible = selectionMode;
            btnCustomerPicker.Enabled = selectionMode;

            lblFromCustomer.Visible = rangeMode;
            ultraComboCustomerFrom.Visible = rangeMode;
            ultraComboCustomerFrom.Enabled = rangeMode;
            btnCustomerFromPicker.Visible = rangeMode;
            btnCustomerFromPicker.Enabled = rangeMode;

            lblToCustomer.Visible = rangeMode;
            ultraComboCustomerTo.Visible = rangeMode;
            ultraComboCustomerTo.Enabled = rangeMode;
            btnCustomerToPicker.Visible = rangeMode;
            btnCustomerToPicker.Enabled = rangeMode;

            if (!selectionMode && !rangeMode)
            {
                _isSyncingCustomerControls = true;
                try
                {
                    ultraComboCustomer.Value = 0;
                    txtCustomerSearch.Text = string.Empty;
                }
                finally
                {
                    _isSyncingCustomerControls = false;
                }
            }

            if (!rangeMode)
            {
                _isSyncingCustomerControls = true;
                try
                {
                    ultraComboCustomerFrom.Value = 0;
                    ultraComboCustomerTo.Value = 0;
                }
                finally
                {
                    _isSyncingCustomerControls = false;
                }
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

        private void OpenCustomerDialog()
        {
            using (var dlg = new frmCustomerDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK || dlg.SelectedCustomerId <= 0)
                    return;

                ultraComboCustomerMode.Value = "SELECTION";
                SelectCustomer(dlg.SelectedCustomerId, dlg.SelectedCustomerName);
            }
        }

        private void OpenCustomerRangeDialog(Infragistics.Win.UltraWinEditors.UltraComboEditor targetCombo)
        {
            using (var dlg = new frmCustomerDialog())
            {
                if (dlg.ShowDialog(this) != DialogResult.OK || dlg.SelectedCustomerId <= 0)
                    return;

                targetCombo.Value = dlg.SelectedCustomerId;
            }
        }

        private void SelectCustomer(int customerId, string customerName)
        {
            _isSyncingCustomerControls = true;
            try
            {
                ultraComboCustomer.Value = customerId;
                CustomerGridList cust = _customers.FirstOrDefault(x => x.LedgerID == customerId);
                txtCustomerSearch.Text = cust != null ? cust.LedgerName ?? string.Empty : customerName ?? string.Empty;
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }
        }

        private void SyncSearchFromSelectedCustomer()
        {
            int selectedCustomerId = GetSelectedCustomerId();
            if (selectedCustomerId <= 0) return;

            CustomerGridList cust = _customers.FirstOrDefault(x => x.LedgerID == selectedCustomerId);
            if (cust == null) return;

            _isSyncingCustomerControls = true;
            try
            {
                txtCustomerSearch.Text = cust.LedgerName ?? string.Empty;
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }
        }

        private void TrySyncCustomerSelectionFromSearchText()
        {
            if (_isSyncingCustomerControls || !IsCustomerSelectionMode())
                return;

            string searchText = GetCustomerSearchText();
            if (string.IsNullOrWhiteSpace(searchText))
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
                return;
            }

            CustomerGridList cust = _customers.FirstOrDefault(x =>
                x.LedgerID.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(x.LedgerName) && x.LedgerName.Equals(searchText, StringComparison.OrdinalIgnoreCase)) ||
                GetCustomerDisplayText(x).Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (cust == null) return;

            _isSyncingCustomerControls = true;
            try
            {
                ultraComboCustomer.Value = cust.LedgerID;
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }
        }

        private static string GetCustomerDisplayText(CustomerGridList customer)
        {
            if (customer == null) return string.Empty;
            return string.IsNullOrWhiteSpace(customer.LedgerName)
                ? customer.LedgerID.ToString()
                : $"{customer.LedgerID} - {customer.LedgerName}";
        }

        #endregion

        #region Previews & Export

        private void ShowGridPreview(string title)
        {
            List<CustomerOutstandingReportRow> rows = gridReport.DataSource as List<CustomerOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            List<CustomerOutstandingReportRow> rows = gridReport.DataSource as List<CustomerOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview. Click View Grid first.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (Panel footer = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Customer Outstanding Listing - Report Preview";
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
                    Text = "CUSTOMER OUTSTANDING LISTING",
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
                previewGrid.InitializeLayout += gridReport_InitializeLayout;
                previewGrid.InitializeRow += gridReport_InitializeRow;
                previewGrid.DataSource = rows.ToList();

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                decimal invoiceTotal = rows.Sum(x => x.InvoiceAmount);
                decimal balanceTotal = rows.Sum(x => x.Balance);
                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = $"Invoices: {rows.Count:N0}    |    Total Invoiced: {invoiceTotal:N2}    |    Outstanding Balance: {balanceTotal:N2}",
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
            string custMode = Convert.ToString(ultraComboCustomerMode.Value);
            string dateMode = Convert.ToString(ultraComboDateMode.Value);
            string dateText = IsDateRangeMode()
                ? $"{Convert.ToDateTime(dtFrom.Value):dd-MMM-yyyy} to {Convert.ToDateTime(dtTo.Value):dd-MMM-yyyy}"
                : "All dates";

            return $"Customer: {(string.IsNullOrWhiteSpace(custMode) ? "ALL" : custMode)}    |    Date: {dateText}    |    Payment Due Only: {(chkPaymentDueOnly.Checked ? "Yes" : "No")}";
        }

        private void ExportCsv()
        {
            List<CustomerOutstandingReportRow> rows = gridReport.DataSource as List<CustomerOutstandingReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.FileName = $"CustomerOutstandingListing_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Cust ID,Customer Name,Salesperson,Bill No,Date,Due Date,Invoice Amount,Received Amount,Balance");

                    foreach (var row in rows)
                    {
                        sb.AppendLine(string.Join(",",
                            EscapeCsv(row.LedgerID.ToString()),
                            EscapeCsv(row.LedgerName),
                            EscapeCsv(row.SalesPerson),
                            EscapeCsv(row.BillNo.ToString()),
                            EscapeCsv(row.BillDate.HasValue ? row.BillDate.Value.ToString("dd/MM/yyyy") : ""),
                            EscapeCsv(row.DueDate.HasValue ? row.DueDate.Value.ToString("dd/MM/yyyy") : ""),
                            row.InvoiceAmount.ToString("F2"),
                            row.ReceivedAmount.ToString("F2"),
                            row.Balance.ToString("F2")));
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Export completed successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to export data.\n" + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
                return $"\"{text.Replace("\"", "\"\"")}\"";
            return text;
        }

        #endregion

        #region Public Methods & Ribbon Integration

        public void Clear() => ClearForm();
        public void Reset() => ClearForm();
        public void RibbonClear() => ClearForm();
        public void ClearRecord() => ClearForm();
        public void ClearFields() => ClearForm();
        public void ResetForm() => ClearForm();

        public void ClearForm()
        {
            _isLoading = true;
            try
            {
                ResetFilterControls(ultraPanelControls);
                UpdateCustomerControlState();
                UpdateDateControlState();
                _reportRows = new List<CustomerOutstandingReportRow>();
                ResetReportView();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetFilterControls(Control parent)
        {
            if (parent == null) return;

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
                    combo.Text = string.Empty;
            }
            catch
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
            }
        }

        #endregion

        #region Column Chooser

        private class ColumnItem
        {
            public string ColumnKey { get; set; }
            public string DisplayText { get; set; }

            public ColumnItem(string columnKey, string displayText)
            {
                ColumnKey = columnKey;
                DisplayText = displayText;
            }

            public override string ToString() => DisplayText;
        }

        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += (s, e) => ShowColumnChooser();
            gridContextMenu.Items.Add(columnChooserMenuItem);
            gridReport.ContextMenuStrip = gridContextMenu;
            SetupDirectHeaderDragDrop();
        }

        private void SetupDirectHeaderDragDrop()
        {
            gridReport.AllowDrop = true;
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;
        }

        private void CreateColumnChooserForm()
        {
            _columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 300),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };
            _columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
            _columnChooserForm.Shown += (s, e) => PositionColumnChooserAtBottomRight();

            _columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 240, 240),
                ItemHeight = 30,
                IntegralHeight = false
            };

            _columnChooserListBox.DrawItem += (s, evt) =>
            {
                if (evt.Index < 0) return;

                ColumnItem item = _columnChooserListBox.Items[evt.Index] as ColumnItem;
                if (item == null) return;

                Rectangle rect = evt.Bounds;
                rect.Inflate(-3, -3);
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(33, 150, 243)))
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
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
                    evt.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    evt.Graphics.FillPath(bgBrush, path);
                }

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    evt.Graphics.DrawString(item.DisplayText, evt.Font, textBrush, rect, stringFormat);
                }
            };

            _columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            _columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
            _columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            _columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            _columnChooserForm.Controls.Add(_columnChooserListBox);
            PopulateColumnChooserListBox();
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed && _columnChooserForm.Visible)
            {
                _columnChooserForm.Location = new Point(
                    Right - _columnChooserForm.Width - 20,
                    Bottom - _columnChooserForm.Height - 20);
                _columnChooserForm.BringToFront();
            }
        }

        private void ShowColumnChooser()
        {
            if (_columnChooserForm != null && !_columnChooserForm.IsDisposed)
            {
                PopulateColumnChooserListBox();
                _columnChooserForm.Show();
                PositionColumnChooserAtBottomRight();
                return;
            }

            CreateColumnChooserForm();
            _columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void PopulateColumnChooserListBox()
        {
            if (_columnChooserListBox == null) return;

            _columnChooserListBox.Items.Clear();
            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            foreach (UltraGridColumn col in gridReport.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
                {
                    string displayText = !string.IsNullOrWhiteSpace(col.Header.Caption) ? col.Header.Caption : col.Key;
                    _columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText));
                }
            }
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            _isDraggingColumn = false;
            _columnToMove = null;
            _dragStartPoint = new Point(e.X, e.Y);

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(e.Location);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
            UltraGridColumn col = headerUI?.Header?.Column;

            if (col != null && col.ExcludeFromColumnChooser != ExcludeFromColumnChooser.True)
            {
                if (e.Button == MouseButtons.Right)
                {
                    ShowHeaderContextMenu(col, e.Location);
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    _columnToMove = col;
                    _isDraggingColumn = true;
                }
            }
        }

        private void ShowHeaderContextMenu(UltraGridColumn column, Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            string colCaption = !string.IsNullOrWhiteSpace(column.Header.Caption) ? column.Header.Caption : column.Key;
            ToolStripMenuItem hideItem = new ToolStripMenuItem($"Hide '{colCaption}'");
            hideItem.Click += (s, e) => HideColumn(column);
            menu.Items.Add(hideItem);

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("Field / Column Chooser...");
            chooserItem.Click += (s, e) => ShowColumnChooser();
            menu.Items.Add(chooserItem);

            menu.Show(gridReport, location);
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingColumn || _columnToMove == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - _dragStartPoint.X);
            int deltaY = e.Y - _dragStartPoint.Y;

            // Only activate drag-to-hide when moving distinctly downwards into the data grid, not horizontal column moving
            if (deltaY > 35 && deltaY > deltaX * 2)
            {
                gridReport.Cursor = Cursors.No;
                string columnName = !string.IsNullOrWhiteSpace(_columnToMove.Header.Caption) ? _columnToMove.Header.Caption : _columnToMove.Key;
                _toolTip.SetToolTip(gridReport, $"Drag down to hide '{columnName}' column");

                if (deltaY > 70)
                {
                    UltraGridColumn colToHide = _columnToMove;
                    _columnToMove = null;
                    _isDraggingColumn = false;
                    gridReport.Cursor = Cursors.Default;
                    _toolTip.SetToolTip(gridReport, string.Empty);
                    HideColumn(colToHide);
                }
            }
            else
            {
                gridReport.Cursor = Cursors.Default;
                _toolTip.SetToolTip(gridReport, string.Empty);
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            gridReport.Cursor = Cursors.Default;
            _toolTip.SetToolTip(gridReport, string.Empty);
            _isDraggingColumn = false;
            _columnToMove = null;
        }

        private void HideColumn(UltraGridColumn column)
        {
            if (column == null || column.Hidden) return;

            _savedColumnWidths[column.Key] = column.Width;
            gridReport.SuspendLayout();
            column.Hidden = true;
            gridReport.ResumeLayout();

            SaveGridLayout();
            PopulateColumnChooserListBox();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(_reportRows);
        }

        private void ShowColumn(string columnKey)
        {
            if (gridReport.DisplayLayout.Bands.Count == 0 || !gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                return;

            UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
            gridReport.SuspendLayout();
            column.Hidden = false;
            if (_savedColumnWidths.ContainsKey(column.Key))
            {
                column.Width = _savedColumnWidths[column.Key];
            }
            gridReport.ResumeLayout();

            SaveGridLayout();
            PopulateColumnChooserListBox();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(_reportRows);
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_columnChooserListBox != null && _columnChooserListBox.SelectedItem is ColumnItem item)
            {
                ShowColumn(item.ColumnKey);
                _toolTip.Show($"'{item.DisplayText}' restored", gridReport, gridReport.PointToClient(MousePosition), 1500);
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = _columnChooserListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches && _columnChooserListBox.Items[index] is ColumnItem item)
            {
                _columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(UltraGridColumn)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(UltraGridColumn)) is UltraGridColumn column && !column.Hidden)
            {
                HideColumn(column);
            }
        }

        private void GridReport_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void GridReport_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(ColumnItem)) is ColumnItem item))
                return;

            ShowColumn(item.ColumnKey);
            _toolTip.Show($"'{item.DisplayText}' restored", gridReport, gridReport.PointToClient(MousePosition), 1500);
        }

        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_columnChooserListBox != null)
            {
                _columnChooserListBox.MouseDown -= ColumnChooserListBox_MouseDown;
                _columnChooserListBox.DoubleClick -= ColumnChooserListBox_DoubleClick;
                _columnChooserListBox.DragOver -= ColumnChooserListBox_DragOver;
                _columnChooserListBox.DragDrop -= ColumnChooserListBox_DragDrop;
                _columnChooserListBox = null;
            }
        }

        private void ApplySavedLayoutIfAvailable()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || gridReport.DisplayLayout.Bands[0].Columns.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            // Always enforce technical hidden columns
            if (band.Columns.Exists("SalesmanId")) { band.Columns["SalesmanId"].Hidden = true; band.Columns["SalesmanId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
            if (band.Columns.Exists("CategoryId")) { band.Columns["CategoryId"].Hidden = true; band.Columns["CategoryId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
            if (band.Columns.Exists("CategoryName")) { band.Columns["CategoryName"].Hidden = true; band.Columns["CategoryName"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
            if (band.Columns.Exists("DocNo")) { band.Columns["DocNo"].Hidden = true; band.Columns["DocNo"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }

            // 1. Try Infragistics LoadFromXml
            try
            {
                if (File.Exists(GridLayoutPath))
                {
                    gridReport.DisplayLayout.LoadFromXml(GridLayoutPath, PropertyCategories.All);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFromXml failed: {ex.Message}");
            }

            // 2. Apply explicit state file to guarantee hidden columns, widths, and positions
            try
            {
                string statePath = Path.Combine(Application.StartupPath, "CustomerOutstandingReport_LayoutState.txt");
                if (File.Exists(statePath))
                {
                    string[] lines = File.ReadAllLines(statePath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("HIDDEN:", StringComparison.OrdinalIgnoreCase))
                        {
                            string raw = line.Substring(7);
                            var hiddenKeys = new HashSet<string>(raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
                            foreach (UltraGridColumn col in band.Columns)
                            {
                                if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True)
                                    continue;

                                col.Hidden = hiddenKeys.Contains(col.Key);
                            }
                        }
                        else if (line.StartsWith("WIDTHS:", StringComparison.OrdinalIgnoreCase))
                        {
                            string raw = line.Substring(7);
                            var pairs = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string pair in pairs)
                            {
                                var parts = pair.Split('=');
                                if (parts.Length == 2 && band.Columns.Exists(parts[0]) && int.TryParse(parts[1], out int w))
                                {
                                    band.Columns[parts[0]].Width = w;
                                }
                            }
                        }
                        else if (line.StartsWith("POSITIONS:", StringComparison.OrdinalIgnoreCase))
                        {
                            string raw = line.Substring(10);
                            var pairs = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            var sorted = pairs
                                .Select(p => p.Split('='))
                                .Where(p => p.Length == 2 && band.Columns.Exists(p[0]) && int.TryParse(p[1], out _))
                                .OrderBy(p => int.Parse(p[1]))
                                .ToList();

                            foreach (var p in sorted)
                            {
                                band.Columns[p[0]].Header.VisiblePosition = int.Parse(p[1]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Apply state file failed: {ex.Message}");
            }

            // Always ensure technical columns stay hidden
            if (band.Columns.Exists("SalesmanId")) { band.Columns["SalesmanId"].Hidden = true; band.Columns["SalesmanId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
            if (band.Columns.Exists("CategoryId")) { band.Columns["CategoryId"].Hidden = true; band.Columns["CategoryId"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
            if (band.Columns.Exists("CategoryName")) { band.Columns["CategoryName"].Hidden = true; band.Columns["CategoryName"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }
            if (band.Columns.Exists("DocNo")) { band.Columns["DocNo"].Hidden = true; band.Columns["DocNo"].ExcludeFromColumnChooser = ExcludeFromColumnChooser.True; }

            PopulateColumnChooserListBox();
        }

        private void SaveGridLayout()
        {
            if (_suppressLayoutSave)
                return;
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            try
            {
                gridReport.DisplayLayout.SaveAsXml(GridLayoutPath, PropertyCategories.All);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveAsXml failed: {ex.Message}");
            }

            try
            {
                UltraGridBand band = gridReport.DisplayLayout.Bands[0];
                var hiddenCols = new List<string>();
                var widths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True)
                        continue;

                    if (col.Hidden)
                    {
                        hiddenCols.Add(col.Key);
                    }
                    widths[col.Key] = col.Width;
                    positions[col.Key] = col.Header.VisiblePosition;
                }

                string statePath = Path.Combine(Application.StartupPath, "CustomerOutstandingReport_LayoutState.txt");
                var lines = new List<string>
                {
                    "HIDDEN:" + string.Join(",", hiddenCols),
                    "WIDTHS:" + string.Join(";", widths.Select(kv => $"{kv.Key}={kv.Value}")),
                    "POSITIONS:" + string.Join(";", positions.Select(kv => $"{kv.Key}={kv.Value}"))
                };
                File.WriteAllLines(statePath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save state file failed: {ex.Message}");
            }
        }

        #endregion
    }
}
