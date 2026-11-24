using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PosBranch_Win.Reports;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmDeletionAuditReport : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private readonly DeletionAuditReportRepository _repository;
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Amount",
            "Qty"
        };

        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private TextBox txtColumnSearch;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private static readonly Cursor blackXCursor = CreateBlackXCursor();
        private bool _isLoading;
        private ReportColumnFilterManager _filterManager;

        private const string LayoutStorageKey = "Audit_DeletionReport_Layout";

        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }
            public ColumnChooserItem(string key, string text) { ColumnKey = key; DisplayText = text; }
            public override string ToString() => DisplayText;
        }

        public frmDeletionAuditReport()
        {
            _repository = new DeletionAuditReportRepository();
            _footerLabels = new Dictionary<string, Label>();
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmDeletionAuditReport_Load;
            btnViewGrid.Click += (s, e) => LoadReport();
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += (s, e) => btnPreviewGrid_Click(s, e);
            btnExportGrid.Click += btnExportGrid_Click;
            btnColumnChooser.Click += (s, e) => ShowColumnChooserForm();
            btnClearFilters.Click += (s, e) => RibbonClear();
            btnToggleSelection.Click += btnToggleSelection_Click;

            comboPeriod.ValueChanged += filter_ValueChanged;
            dtFrom.ValueChanged += filter_ValueChanged;
            dtTo.ValueChanged += filter_ValueChanged;
            ultraComboTransactionType.ValueChanged += filter_ValueChanged;
            ultraComboUser.ValueChanged += filter_ValueChanged;
            txtSearch.TextChanged += filter_ValueChanged;

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.Resize += gridReport_Resize;
            gridReport.AfterColPosChanged += (s, e) =>
            {
                UpdateFooterCellPositions();
                ReportGridLayoutHelper.SaveLayout(LayoutStorageKey, gridReport, userHiddenColumnKeys);
            };
            gridReport.BeforeColumnChooserDisplayed += (s, e) =>
            {
                e.Cancel = true;
                ShowColumnChooserForm();
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();

            KeyPreview = true;
            KeyDown += frmDeletionAuditReport_KeyDown;
        }

        private void frmDeletionAuditReport_Load(object sender, EventArgs e)
        {
            // Security verification: ONLY Administrator role is allowed
            if (!SessionContext.IsAdmin && !string.Equals(SessionContext.UserName, "admin", StringComparison.OrdinalIgnoreCase) && SessionContext.RoleId != 1)
            {
                MessageBox.Show("Access Denied.\n\nThis Deletion & Cancellation Audit Report contains sensitive security records and is strictly restricted to Administrators only.",
                    "Security Alert - Admin Only", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                BeginInvoke((Action)Close);
                return;
            }

            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;
            try
            {
                Text = "Deletion & Cancellation Audit Report (Admin Only)";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                ReportGridLayoutHelper.LoadHiddenColumns(LayoutStorageKey, userHiddenColumnKeys);

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                SetupHeaderDragToHideAndColumnChooser();
            }
            finally
            {
                _isLoading = false;
            }

            LoadReport();
        }

        private void InitializeFilterControls()
        {
            dtFrom.MaskInput = "{date}";
            dtTo.MaskInput = "{date}";
            dtFrom.FormatString = "dd/MM/yyyy";
            dtTo.FormatString = "dd/MM/yyyy";

            comboPeriod.Items.Clear();
            comboPeriod.Items.Add("ALL", "ALL");
            comboPeriod.Items.Add("Today", "Today");
            comboPeriod.Items.Add("Yesterday", "Yesterday");
            comboPeriod.Items.Add("This Week", "This Week");
            comboPeriod.Items.Add("This Month", "This Month");
            comboPeriod.Items.Add("Last Month", "Last Month");
            comboPeriod.Items.Add("This Quarter", "This Quarter");
            comboPeriod.Items.Add("This Year", "This Year");
            comboPeriod.Items.Add("Custom Range", "Custom Range");
            comboPeriod.Value = "ALL";

            ultraComboTransactionType.Items.Clear();
            ultraComboTransactionType.Items.Add("ALL", "ALL Deleted Transactions & Records");
            ultraComboTransactionType.Items.Add("SALES", "Sales Invoices");
            ultraComboTransactionType.Items.Add("PURCHASE", "Purchase (GRN) Invoices");
            ultraComboTransactionType.Items.Add("SALES_RETURN", "Sales Returns");
            ultraComboTransactionType.Items.Add("PURCHASE_RETURN", "Purchase Returns");
            ultraComboTransactionType.Items.Add("CUSTOMER_RECEIPT", "Customer Receipts");
            ultraComboTransactionType.Items.Add("VENDOR_PAYMENT", "Vendor Payments");
            ultraComboTransactionType.Items.Add("GENERAL_PAYMENT", "General Payments");
            ultraComboTransactionType.Items.Add("GENERAL_RECEIPT", "General Receipts");
            ultraComboTransactionType.Items.Add("JOURNAL_VOUCHER", "Journal Vouchers");
            ultraComboTransactionType.Items.Add("CONTRA_VOUCHER", "Contra Vouchers");
            ultraComboTransactionType.Items.Add("CREDIT_NOTE", "Credit Notes");
            ultraComboTransactionType.Items.Add("DEBIT_NOTE", "Debit Notes");
            ultraComboTransactionType.Items.Add("STOCK_ADJUSTMENT", "Stock Adjustments");
            ultraComboTransactionType.Items.Add("HOLD_BILL", "Hold Bills");
            ultraComboTransactionType.Items.Add("ITEM", "Item Deletions");
            ultraComboTransactionType.Items.Add("LEDGER", "Ledger / Account Deletions");
            ultraComboTransactionType.Value = "ALL";

            ultraComboUser.Items.Clear();
            List<string> users = _repository.GetDistinctUsers();
            foreach (string u in users)
            {
                ultraComboUser.Items.Add(u, u);
            }
            ultraComboUser.Value = "ALL";

            txtSearch.Text = string.Empty;
            ApplyPeriodSelection();
        }

        public void RibbonClear()
        {
            InitializeFilterControls();
            LoadReport();
        }

        public void Clear() => RibbonClear();

        private void filter_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            if (sender == comboPeriod)
                ApplyPeriodSelection();
            LoadReport();
        }

        private void ApplyPeriodSelection()
        {
            if (comboPeriod == null || comboPeriod.Value == null) return;
            string val = comboPeriod.Value.ToString();
            DateTime today = DateTime.Today;

            bool isAll = string.Equals(val, "ALL", StringComparison.OrdinalIgnoreCase);
            lblFromDate.Visible = !isAll;
            dtFrom.Visible = !isAll;
            lblToDate.Visible = !isAll;
            dtTo.Visible = !isAll;

            switch (val)
            {
                case "ALL":
                    dtFrom.Value = new DateTime(1900, 1, 1);
                    dtTo.Value = today;
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "Today":
                    dtFrom.Value = today;
                    dtTo.Value = today;
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "Yesterday":
                    dtFrom.Value = today.AddDays(-1);
                    dtTo.Value = today.AddDays(-1);
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "This Week":
                    int daysFromMonday = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    dtFrom.Value = today.AddDays(-daysFromMonday);
                    dtTo.Value = today;
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "This Month":
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today;
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "Last Month":
                    DateTime lm = today.AddMonths(-1);
                    DateTime firstOfLastMonth = new DateTime(lm.Year, lm.Month, 1);
                    dtFrom.Value = firstOfLastMonth;
                    dtTo.Value = firstOfLastMonth.AddMonths(1).AddDays(-1);
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "This Quarter":
                    int qtrMonth = ((today.Month - 1) / 3) * 3 + 1;
                    dtFrom.Value = new DateTime(today.Year, qtrMonth, 1);
                    dtTo.Value = today;
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "This Year":
                    dtFrom.Value = new DateTime(today.Year, 1, 1);
                    dtTo.Value = today;
                    dtFrom.Enabled = false;
                    dtTo.Enabled = false;
                    break;
                case "Custom Range":
                    dtFrom.Enabled = true;
                    dtTo.Enabled = true;
                    break;
            }
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

            StyleLabel(lblPeriod);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblTransType);
            StyleLabel(lblUser);
            StyleLabel(lblSearch);
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnColumnChooser);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnToggleSelection);
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
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(comboPeriod, true);
            StyleFilterCombo(ultraComboTransactionType, true);
            StyleFilterCombo(ultraComboUser, true);
            StyleFilterCombo(txtSearch, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
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
            combo.DropDownStyle = isDropDownList ? DropDownStyle.DropDownList : DropDownStyle.DropDown;
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

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.Appearance.BackColor = FormBackColor;
            layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.HeaderStyle = HeaderStyle.Standard;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.AllowColSizing = AllowColSizing.Free;
            layout.Override.AllowRowFiltering = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.SelectTypeRow = SelectType.Single;

            // Row Selectors matching frmVendorOutstandingReport
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            layout.Override.RowSelectorWidth = 25;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Header Appearance
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            // Row Appearance
            layout.Override.MinRowHeight = 24;
            layout.Override.DefaultRowHeight = 24;
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.ForeColor = ControlTextColor;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;

            // Active & Selected Row Appearance
            layout.Override.ActiveCellAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveCellAppearance.ForeColor = ControlTextColor;
            layout.Override.ActiveCellAppearance.BorderColor = GridSelectedBlue;
            layout.Override.ActiveCellAppearance.BorderAlpha = Alpha.Transparent;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;

            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = ControlTextColor;
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.RowSizing = RowSizing.AutoFree;

            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

            _filterManager = new ReportColumnFilterManager(gridReport, OnFilterChanged);
        }

        private void OnFilterChanged()
        {
            UpdateFooterValues();
            UpdateFooterCellPositions();
        }

        private void LoadReport()
        {
            if (_isLoading) return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                ReportGridLayoutHelper.LoadHiddenColumns(LayoutStorageKey, userHiddenColumnKeys);

                DeletionAuditReportFilter filter = new DeletionAuditReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    TransactionType = Convert.ToString(ultraComboTransactionType.Value ?? "ALL"),
                    UserFilter = Convert.ToString(ultraComboUser.Value ?? "ALL"),
                    SearchText = txtSearch.Text.Trim()
                };

                _filterManager?.ClearFilters();
                gridReport.DataSource = null;

                List<DeletionAuditReportRow> rows = _repository.GetDeletionAuditReport(filter);
                gridReport.DataSource = rows;

                CreateFooterCells();
                UpdateFooterValues();
                UpdateFooterCellPositions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Deletion Audit Report.\n" + ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.False;
                if (userHiddenColumnKeys.Contains(col.Key))
                {
                    col.Hidden = true;
                }

                col.Header.Appearance.ThemedElementAlpha = Alpha.Transparent;
                col.Header.Appearance.BackColor = GridHeaderBlue;
                col.Header.Appearance.BackColor2 = GridHeaderBlueDark;
                col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                col.Header.Appearance.ForeColor = Color.White;
                col.Header.Appearance.FontData.Bold = DefaultableBoolean.False;
                col.Header.Appearance.FontData.Name = "Microsoft Sans Serif";
                col.Header.Appearance.FontData.SizeInPoints = 8.25F;

                switch (col.Key)
                {
                    case "SlNo":
                        col.Header.Caption = "Sl#";
                        col.Width = 45;
                        col.CellAppearance.TextHAlign = HAlign.Center;
                        col.Header.Appearance.TextHAlign = HAlign.Center;
                        break;
                    case "DeletedDate":
                        col.Header.Caption = "Deleted Date & Time";
                        col.Format = "dd-MM-yyyy HH:mm";
                        col.Width = 125;
                        break;
                    case "DocDate":
                        col.Header.Caption = "Doc Date";
                        col.Format = "dd-MM-yyyy";
                        col.Width = 85;
                        break;
                    case "TransactionType":
                        col.Header.Caption = "Transaction Type";
                        col.Width = 120;
                        break;
                    case "DocNo":
                        col.Header.Caption = "Document No";
                        col.Width = 100;
                        break;
                    case "RefNo":
                        col.Header.Caption = "Ref / Inv No";
                        col.Width = 100;
                        break;
                    case "PartyName":
                        col.Header.Caption = "Customer / Vendor / Item / Ledger";
                        col.Width = 200;
                        break;
                    case "Amount":
                        col.Header.Caption = "Deleted Value";
                        col.Format = "N2";
                        col.Width = 110;
                        col.CellAppearance.TextHAlign = HAlign.Right;
                        col.Header.Appearance.TextHAlign = HAlign.Right;
                        break;
                    case "Qty":
                        col.Header.Caption = "Qty";
                        col.Format = "N2";
                        col.Width = 70;
                        col.CellAppearance.TextHAlign = HAlign.Right;
                        col.Header.Appearance.TextHAlign = HAlign.Right;
                        break;
                    case "DeletedBy":
                        col.Header.Caption = "Deleted By";
                        col.Width = 90;
                        break;
                    case "Counter":
                        col.Header.Caption = "Counter";
                        col.Width = 85;
                        break;
                    case "DeletionReason":
                        col.Header.Caption = "Reason / Narration";
                        col.Width = 200;
                        break;
                    case "SourceDetails":
                        col.Header.Caption = "Item Details / Audit Snapshot";
                        col.Width = 220;
                        break;
                    case "ModuleKey":
                        col.Hidden = true;
                        col.ExcludeFromColumnChooser = ExcludeFromColumnChooser.True;
                        break;
                }

                if (col.DataType == typeof(decimal) || col.DataType == typeof(double) || col.DataType == typeof(int) || col.DataType == typeof(long))
                {
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Header.Appearance.TextHAlign = HAlign.Right;
                }
            }

            ReportGridLayoutHelper.ApplySavedLayout(LayoutStorageKey, gridReport, userHiddenColumnKeys);
            _filterManager?.UpdateHeaderFilterIcons();
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden) continue;

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
                    _columnAggregations[column.Key] = summaryDefaultNumericColumns.Contains(column.Key) ? "Sum" : (column.Key == "DocNo" ? "Count" : "None");

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
                string current = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
                    : (summaryDefaultNumericColumns.Contains(columnKey) ? "Sum" : "None");
                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null)
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
            if (sender is ToolStripMenuItem item && item.Owner is ContextMenuStrip menu && menu.Tag != null && item.Tag != null)
            {
                _columnAggregations[menu.Tag.ToString()] = item.Tag.ToString();
                UpdateFooterValues();
            }
        }

        private void UpdateFooterValues()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0) return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (var pair in _footerLabels)
            {
                string columnKey = pair.Key;
                Label footerLabel = pair.Value;
                string aggregation = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";

                if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, aggregation, visibleRows);
                string formattedText = FormatAggregationResult(columnKey, aggregation, result);
                footerLabel.Text = string.IsNullOrEmpty(formattedText) ? string.Empty : $"{aggregation}: {formattedText}";
                footerLabel.Tag = Tuple.Create(columnKey, footerLabel.Text);
                footerLabel.Invalidate();
            }
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
            if (string.Equals(aggregation, "None", StringComparison.OrdinalIgnoreCase)) return string.Empty;
            if (result == null) return string.Empty;
            if (aggregation == "Count") return Convert.ToString(result);

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

        private static bool HasCellValue(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            return !(value is string s && string.IsNullOrWhiteSpace(s));
        }

        private static decimal? GetNumericValue(object value)
        {
            if (!HasCellValue(value)) return null;
            try { return Convert.ToDecimal(value); } catch { return null; }
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            Type t = column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            if (gridReport.Rows == null) yield break;
            foreach (UltraGridRow row in gridReport.Rows.GetFilteredInNonGroupByRows())
            {
                if (row.IsDataRow && !row.Hidden) yield return row;
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int defaultHeight = Math.Max(ultraPanelGridFooter.Height - 2, 20);

            ultraPanelGridFooter.SuspendLayout();
            try
            {
                foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
                {
                    if (!_footerLabels.TryGetValue(column.Key, out Label footerLabel)) continue;

                    if (column.Hidden)
                    {
                        footerLabel.Visible = false;
                        continue;
                    }

                    UIElement colElement = column.Header.GetUIElement();
                    if (colElement != null)
                    {
                        Rectangle rect = colElement.Rect;
                        footerLabel.SetBounds(rect.X, 1, rect.Width, defaultHeight);
                        footerLabel.Visible = true;
                    }
                    else
                    {
                        footerLabel.Visible = false;
                    }
                }
            }
            finally
            {
                ultraPanelGridFooter.ResumeLayout();
            }
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Label label && label.Tag is Tuple<string, string> tagInfo)
            {
                Graphics g = e.Graphics;
                Rectangle rect = label.ClientRectangle;

                using (LinearGradientBrush brush = new LinearGradientBrush(rect, GridHeaderBlue, GridHeaderBlueDark, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, rect);
                }

                using (Pen borderPen = new Pen(BorderBlue))
                {
                    g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
                }

                string displayText = tagInfo.Item2;
                if (!string.IsNullOrEmpty(displayText))
                {
                    TextRenderer.DrawText(g, displayText, label.Font, rect, Color.White,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try { gridReport.PrintPreview(); }
            catch (Exception ex) { MessageBox.Show("Preview error: " + ex.Message); }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = ReportPdfExporter.CsvAndPdfFilter;
                    dialog.FileName = $"Deletion_Audit_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (ReportPdfExporter.CheckAndExportPdf(dialog, gridReport, "Deletion & Cancellation Audit Report")) return;
                        MessageBox.Show("Export process initialized.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Export error: " + ex.Message); }
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void frmDeletionAuditReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
                {
                    columnChooserForm.Hide();
                    e.Handled = true;
                    return;
                }
                Close();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5) { LoadReport(); e.Handled = true; }
            else if (e.KeyCode == Keys.F6) { btnPreviewGrid_Click(sender, e); e.Handled = true; }
            else if (e.KeyCode == Keys.F7) { btnExportGrid_Click(sender, e); e.Handled = true; }
            else if (e.KeyCode == Keys.F8) { btnPreviewGrid_Click(sender, e); e.Handled = true; }
        }

        #region Column Chooser & Drag to Hide

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.MouseLeave += GridReport_MouseLeave;
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(e.Location);
                HeaderUIElement headerElement = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;
                if (headerElement != null && headerElement.Header?.Column != null)
                {
                    columnBeingDragged = headerElement.Header.Column;
                    headerDragStartPoint = e.Location;
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
            if (columnBeingDragged != null && e.Button == MouseButtons.Left)
            {
                int deltaX = Math.Abs(e.Location.X - headerDragStartPoint.X);
                int deltaY = Math.Abs(e.Location.Y - headerDragStartPoint.Y);
                if (deltaX > 8 || deltaY > 8)
                {
                    isDraggingHeaderToHide = true;
                    if (!gridReport.ClientRectangle.Contains(e.Location))
                    {
                        Cursor.Current = blackXCursor;
                        headerToolTip.SetToolTip(gridReport, $"Drop outside to hide '{columnBeingDragged.Header.Caption}'");
                    }
                    else
                    {
                        Cursor.Current = Cursors.Default;
                        headerToolTip.Hide(gridReport);
                    }
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (columnBeingDragged != null && isDraggingHeaderToHide)
            {
                if (!gridReport.ClientRectangle.Contains(e.Location))
                {
                    HideColumn(columnBeingDragged);
                }
            }
            columnBeingDragged = null;
            isDraggingHeaderToHide = false;
            Cursor.Current = Cursors.Default;
            headerToolTip.Hide(gridReport);
        }

        private void GridReport_MouseLeave(object sender, EventArgs e)
        {
            if (!isDraggingHeaderToHide)
            {
                Cursor.Current = Cursors.Default;
                headerToolTip.Hide(gridReport);
            }
        }

        private void HideColumn(UltraGridColumn column)
        {
            if (column == null) return;
            column.Hidden = true;
            userHiddenColumnKeys.Add(column.Key);
            UpdateFooterCellPositions();
            ReportGridLayoutHelper.SaveLayout(LayoutStorageKey, gridReport, userHiddenColumnKeys);
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                PopulateColumnChooserListBox();
            }
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            columnChooserForm.BringToFront();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Column Chooser",
                Size = new Size(260, 360),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                ShowInTaskbar = false,
                BackColor = FormBackColor
            };

            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(4) };
            txtColumnSearch = new TextBox { Dock = DockStyle.Fill, Font = new Font("Tahoma", 9F) };
            txtColumnSearch.TextChanged += (s, e) => PopulateColumnChooserListBox();
            pnlSearch.Controls.Add(txtColumnSearch);

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 9F),
                IntegralHeight = false,
                SelectionMode = SelectionMode.One
            };
            columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;

            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 36 };
            Button btnShow = new Button { Text = "Show Column", Dock = DockStyle.Fill, FlatStyle = FlatStyle.System };
            btnShow.Click += (s, e) => ShowSelectedColumnFromChooser();
            pnlBottom.Controls.Add(btnShow);

            columnChooserForm.Controls.Add(columnChooserListBox);
            columnChooserForm.Controls.Add(pnlBottom);
            columnChooserForm.Controls.Add(pnlSearch);
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            columnChooserListBox.BeginUpdate();
            columnChooserListBox.Items.Clear();

            string search = txtColumnSearch?.Text?.Trim() ?? string.Empty;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.Caption))
            {
                if (col.ExcludeFromColumnChooser == ExcludeFromColumnChooser.True) continue;
                if (!col.Hidden) continue;

                string text = col.Header.Caption ?? col.Key;
                if (!string.IsNullOrEmpty(search) && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, text));
            }

            columnChooserListBox.EndUpdate();
        }

        private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
        {
            ShowSelectedColumnFromChooser();
        }

        private void ShowSelectedColumnFromChooser()
        {
            if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
            {
                if (gridReport.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
                {
                    UltraGridColumn col = gridReport.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                    col.Hidden = false;
                    userHiddenColumnKeys.Remove(item.ColumnKey);
                    UpdateFooterCellPositions();
                    ReportGridLayoutHelper.SaveLayout(LayoutStorageKey, gridReport, userHiddenColumnKeys);
                    PopulateColumnChooserListBox();
                }
            }
        }

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                Bitmap bmp = new Bitmap(24, 24);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen pen = new Pen(Color.Red, 3))
                    {
                        g.DrawLine(pen, 4, 4, 20, 20);
                        g.DrawLine(pen, 20, 4, 4, 20);
                    }
                }
                return new Cursor(bmp.GetHicon());
            }
            catch
            {
                return Cursors.No;
            }
        }

        #endregion
    }
}
