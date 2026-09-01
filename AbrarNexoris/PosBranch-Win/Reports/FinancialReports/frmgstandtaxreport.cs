using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmgstandtaxreport : Form
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
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private readonly GSTAndTaxReportRepository _repository;
        private List<GSTAndTaxReportRow> _reportRows;
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private bool _isLoading;

        public frmgstandtaxreport()
        {
            _repository = new GSTAndTaxReportRepository();
            _reportRows = new List<GSTAndTaxReportRow>();
            _footerLabels = new Dictionary<string, Label>();
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmgstandtaxreport_Load;
            btnViewGrid.Click += btnViewGrid_Click;
            btnPreviewGrid.Click += btnPreviewGrid_Click;
            btnPreviewReport.Click += btnPreviewReport_Click;
            btnExportGrid.Click += btnExportGrid_Click;
            btnToggleSelection.Click += btnToggleSelection_Click;

            ultraComboTrnsType.ValueChanged += filter_ValueChanged;
            ultraComboTaxType.ValueChanged += filter_ValueChanged;
            ultraComboTaxPer.ValueChanged += filter_ValueChanged;
            ultraComboViewMode.ValueChanged += filter_ValueChanged;
            ultraComboDateMode.ValueChanged += filter_ValueChanged;
            txtSearch.TextChanged += filter_ValueChanged;
            btnSearchItem.Click += btnSearchItem_Click;
            dtFrom.ValueChanged += filter_ValueChanged;
            dtTo.ValueChanged += filter_ValueChanged;

            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.Resize += gridReport_Resize;

            KeyPreview = true;
            KeyDown += frmgstandtaxreport_KeyDown;
        }

        private void frmgstandtaxreport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "GST & Tax Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                ResetReportView();

                LoadReport();
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

            ultraComboTrnsType.Items.Clear();
            ultraComboTrnsType.Items.Add("ALL", "ALL Transactions");
            ultraComboTrnsType.Items.Add("Sales Invoice", "Sales Invoice");
            ultraComboTrnsType.Items.Add("Purchase Invoice", "Purchase Invoice");
            ultraComboTrnsType.Items.Add("Sales Return", "Sales Return");
            ultraComboTrnsType.Items.Add("Purchase Return", "Purchase Return");
            ultraComboTrnsType.Value = "ALL";

            ultraComboTaxType.Items.Clear();
            ultraComboTaxType.Items.Add("ALL", "ALL Tax Types");
            ultraComboTaxType.Items.Add("Inclusive", "Inclusive");
            ultraComboTaxType.Items.Add("Exclusive", "Exclusive");
            ultraComboTaxType.Value = "ALL";

            ultraComboTaxPer.Items.Clear();
            ultraComboTaxPer.Items.Add("ALL", "ALL Tax %");
            ultraComboTaxPer.Items.Add("0", "0 %");
            ultraComboTaxPer.Items.Add("5", "5 %");
            ultraComboTaxPer.Items.Add("12", "12 %");
            ultraComboTaxPer.Items.Add("18", "18 %");
            ultraComboTaxPer.Items.Add("28", "28 %");
            ultraComboTaxPer.Value = "ALL";

            ultraComboDateMode.Items.Clear();
            ultraComboDateMode.Items.Add("ALL", "ALL Dates");
            ultraComboDateMode.Items.Add("RANGE", "Date Range");
            ultraComboDateMode.Value = "RANGE";

            ultraComboViewMode.Items.Clear();
            ultraComboViewMode.Items.Add("Item", "Item Wise");
            ultraComboViewMode.Items.Add("Invoice", "Invoice Wise");
            ultraComboViewMode.Value = "Item";

            txtSearch.Text = string.Empty;
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

            StyleLabel(lblTrnsType);
            StyleLabel(lblTaxType);
            StyleLabel(lblSearch);
            StyleLabel(lblDate);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
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

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboTrnsType, true);
            StyleFilterCombo(ultraComboTaxType, true);
            StyleFilterCombo(ultraComboTaxPer, true);
            StyleFilterCombo(ultraComboViewMode, true);
            StyleFilterCombo(ultraComboDateMode, true);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
            StyleClassicButton(btnSearchItem);
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
            layout.GroupByBox.Prompt = "Drag a column header here to group by transaction type, party, or HSN";
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

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void LoadReport()
        {
            if (_isLoading) return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                GSTAndTaxReportFilter filter = new GSTAndTaxReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    CompanyId = SessionContext.CompanyId,
                    BranchId = SessionContext.BranchId,
                    FinYearId = SessionContext.FinYearId,
                    TrnsType = Convert.ToString(ultraComboTrnsType.Value),
                    TaxType = Convert.ToString(ultraComboTaxType.Value),
                    SearchText = txtSearch.Text.Trim()
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load GST & Tax Report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<GSTAndTaxReportRow> filteredRows = _reportRows ?? Enumerable.Empty<GSTAndTaxReportRow>();
            string searchText = txtSearch.Text.Trim();
            string selectedTrns = Convert.ToString(ultraComboTrnsType.Value);
            string selectedTax = Convert.ToString(ultraComboTaxType.Value);
            string selectedTaxPer = Convert.ToString(ultraComboTaxPer.Value);

            if (!string.IsNullOrWhiteSpace(selectedTrns) && !string.Equals(selectedTrns, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                filteredRows = filteredRows.Where(x => string.Equals(x.TrnsType, selectedTrns, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(selectedTax) && !string.Equals(selectedTax, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                filteredRows = filteredRows.Where(x => string.Equals(x.TaxType, selectedTax, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(selectedTaxPer) && !string.Equals(selectedTaxPer, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(selectedTaxPer, out double targetTaxPer))
                {
                    filteredRows = filteredRows.Where(x => Math.Abs(x.TaxPer - targetTaxPer) < 0.01);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.InvoiceNo) && x.InvoiceNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.PartyName) && x.PartyName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.PartyGSTIN) && x.PartyGSTIN.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.ItemName) && x.ItemName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.HSNCode) && x.HSNCode.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.TaxCategory) && x.TaxCategory.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    x.TaxPer.ToString("0.##").IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<GSTAndTaxReportRow> boundRows;
            string selectedViewMode = Convert.ToString(ultraComboViewMode.Value);

            if (string.Equals(selectedViewMode, "Invoice", StringComparison.OrdinalIgnoreCase))
            {
                boundRows = filteredRows
                    .GroupBy(x => new { x.TrnsType, x.InvoiceNo, x.PartyName, x.PartyGSTIN, x.TaxType, x.TaxCategory, x.DocDate })
                    .Select(g => new GSTAndTaxReportRow
                    {
                        TrnsType = g.Key.TrnsType,
                        InvoiceNo = g.Key.InvoiceNo,
                        DocDate = g.Key.DocDate,
                        PartyName = g.Key.PartyName,
                        PartyGSTIN = g.Key.PartyGSTIN,
                        ItemName = $"{g.Count()} Items Summary",
                        HSNCode = string.Join(", ", g.Select(x => x.HSNCode).Where(h => !string.IsNullOrWhiteSpace(h)).Distinct()),
                        Qty = g.Sum(x => x.Qty),
                        Unit = "LOT",
                        TaxableAmt = g.Sum(x => x.TaxableAmt),
                        TaxPer = g.Max(x => x.TaxPer),
                        CGSTPer = g.Max(x => x.CGSTPer),
                        CGSTAmt = g.Sum(x => x.CGSTAmt),
                        SGSTPer = g.Max(x => x.SGSTPer),
                        SGSTAmt = g.Sum(x => x.SGSTAmt),
                        IGSTPer = g.Max(x => x.IGSTPer),
                        IGSTAmt = g.Sum(x => x.IGSTAmt),
                        CessPer = g.Max(x => x.CessPer),
                        CessAmt = g.Sum(x => x.CessAmt),
                        TotalTaxAmt = g.Sum(x => x.TotalTaxAmt),
                        GrandTotal = g.Sum(x => x.GrandTotal),
                        TaxType = g.Key.TaxType,
                        TaxCategory = g.Key.TaxCategory,
                        OutputTaxAmt = g.Sum(x => x.OutputTaxAmt),
                        InputTaxAmt = g.Sum(x => x.InputTaxAmt)
                    })
                    .OrderBy(x => x.DocDate)
                    .ThenBy(x => x.InvoiceNo)
                    .ToList();
            }
            else
            {
                boundRows = filteredRows
                    .OrderBy(x => x.DocDate)
                    .ThenBy(x => x.InvoiceNo)
                    .ToList();
            }

            gridReport.DataSource = boundRows;
            ConfigureGridForViewMode(string.Equals(selectedViewMode, "Invoice", StringComparison.OrdinalIgnoreCase));
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(boundRows);
        }

        private void ResetReportView()
        {
            gridReport.DataSource = null;
            UpdateFooterValues(new List<GSTAndTaxReportRow>());
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            SetColumnHeader(band, "TrnsType", "Transaction Type", 120, HAlign.Left);
            SetColumnHeader(band, "TaxCategory", "Tax Category (Input/Output)", 140, HAlign.Left);
            SetColumnHeader(band, "InvoiceNo", "Invoice No", 110, HAlign.Left);
            SetColumnHeader(band, "DocDate", "Date", 90, HAlign.Center, "dd/MM/yyyy");
            SetColumnHeader(band, "PartyName", "Customer / Vendor", 160, HAlign.Left);
            SetColumnHeader(band, "PartyGSTIN", "Party GST No", 120, HAlign.Left);
            SetColumnHeader(band, "ItemName", "Item Description", 160, HAlign.Left);
            SetColumnHeader(band, "HSNCode", "HSN Code", 85, HAlign.Center);
            SetColumnHeader(band, "Qty", "Qty", 65, HAlign.Right, "0.00");
            SetColumnHeader(band, "Unit", "Unit", 60, HAlign.Center);
            SetColumnHeader(band, "TaxableAmt", "Taxable Value", 110, HAlign.Right, "N2");
            SetColumnHeader(band, "TaxPer", "Total Tax %", 75, HAlign.Right, "0.00");
            SetColumnHeader(band, "CGSTPer", "CGST %", 65, HAlign.Right, "0.00");
            SetColumnHeader(band, "CGSTAmt", "CGST Amt", 90, HAlign.Right, "N2");
            SetColumnHeader(band, "SGSTPer", "SGST %", 65, HAlign.Right, "0.00");
            SetColumnHeader(band, "SGSTAmt", "SGST Amt", 90, HAlign.Right, "N2");
            SetColumnHeader(band, "IGSTPer", "IGST %", 65, HAlign.Right, "0.00");
            SetColumnHeader(band, "IGSTAmt", "IGST Amt", 90, HAlign.Right, "N2");
            SetColumnHeader(band, "CessPer", "Cess %", 65, HAlign.Right, "0.00");
            SetColumnHeader(band, "CessAmt", "Cess Amt", 90, HAlign.Right, "N2");
            SetColumnHeader(band, "OutputTaxAmt", "Output Tax", 100, HAlign.Right, "N2");
            SetColumnHeader(band, "InputTaxAmt", "Input Tax (ITC)", 100, HAlign.Right, "N2");
            SetColumnHeader(band, "TotalTaxAmt", "Total Tax", 110, HAlign.Right, "N2");
            SetColumnHeader(band, "GrandTotal", "Gross Amount", 120, HAlign.Right, "N2");
            SetColumnHeader(band, "TaxType", "Tax Mode", 85, HAlign.Center);

            // Default sum aggregations in footer
            _columnAggregations["TaxableAmt"] = "Sum";
            _columnAggregations["CGSTAmt"] = "Sum";
            _columnAggregations["SGSTAmt"] = "Sum";
            _columnAggregations["IGSTAmt"] = "Sum";
            _columnAggregations["CessAmt"] = "Sum";
            _columnAggregations["OutputTaxAmt"] = "Sum";
            _columnAggregations["InputTaxAmt"] = "Sum";
            _columnAggregations["TotalTaxAmt"] = "Sum";
            _columnAggregations["GrandTotal"] = "Sum";

            ConfigureGridForViewMode(ultraComboViewMode != null && string.Equals(Convert.ToString(ultraComboViewMode.Value), "Invoice", StringComparison.OrdinalIgnoreCase));
        }

        private void ConfigureGridForViewMode(bool isInvoiceWise)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            if (isInvoiceWise)
            {
                gridReport.DisplayLayout.GroupByBox.Prompt = "Invoice Wise Tax Summary — Drag a column header here to group by Transaction Type, Party, or Tax Mode";
                
                SetColumnHeader(band, "ItemName", "Items Summary", 140, HAlign.Left);
                SetColumnHeader(band, "HSNCode", "HSN Codes", 100, HAlign.Left);
                SetColumnHeader(band, "Qty", "Total Qty", 70, HAlign.Right, "0.00");
                
                if (band.Columns.Exists("Unit")) band.Columns["Unit"].Hidden = true;
                if (band.Columns.Exists("CGSTPer")) band.Columns["CGSTPer"].Hidden = true;
                if (band.Columns.Exists("SGSTPer")) band.Columns["SGSTPer"].Hidden = true;
                if (band.Columns.Exists("IGSTPer")) band.Columns["IGSTPer"].Hidden = true;
                if (band.Columns.Exists("CessPer")) band.Columns["CessPer"].Hidden = true;
            }
            else
            {
                gridReport.DisplayLayout.GroupByBox.Prompt = "Item Wise Detailed Tax Report — Drag a column header here to group by Item, Transaction Type, Party, or HSN";

                SetColumnHeader(band, "ItemName", "Item Description", 160, HAlign.Left);
                SetColumnHeader(band, "HSNCode", "HSN Code", 85, HAlign.Center);
                SetColumnHeader(band, "Qty", "Qty", 65, HAlign.Right, "0.00");

                if (band.Columns.Exists("Unit")) { band.Columns["Unit"].Hidden = false; SetColumnHeader(band, "Unit", "Unit", 60, HAlign.Center); }
                if (band.Columns.Exists("CGSTPer")) { band.Columns["CGSTPer"].Hidden = false; SetColumnHeader(band, "CGSTPer", "CGST %", 65, HAlign.Right, "0.00"); }
                if (band.Columns.Exists("SGSTPer")) { band.Columns["SGSTPer"].Hidden = false; SetColumnHeader(band, "SGSTPer", "SGST %", 65, HAlign.Right, "0.00"); }
                if (band.Columns.Exists("IGSTPer")) { band.Columns["IGSTPer"].Hidden = false; SetColumnHeader(band, "IGSTPer", "IGST %", 65, HAlign.Right, "0.00"); }
                if (band.Columns.Exists("CessPer")) { band.Columns["CessPer"].Hidden = false; SetColumnHeader(band, "CessPer", "Cess %", 65, HAlign.Right, "0.00"); }
            }
        }

        private static void SetColumnHeader(UltraGridBand band, string key, string caption, int width, HAlign align, string format = null)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
            col.Header.Caption = caption;
            col.Width = width;
            col.CellAppearance.TextHAlign = align;
            if (!string.IsNullOrWhiteSpace(format))
            {
                col.Format = format;
            }
        }

        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(new List<GSTAndTaxReportRow>());
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
            UpdateFooterValues(gridReport.DataSource as IList<GSTAndTaxReportRow>);
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) || t == typeof(int) || t == typeof(long);
        }

        private void UpdateFooterValues(IList<GSTAndTaxReportRow> rows)
        {
            if (_footerLabels.Count == 0)
                return;

            List<UltraGridRow> visibleRows = gridReport.Rows.GetFilteredInNonGroupByRows().Cast<UltraGridRow>().ToList();
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
                return aggregation == "Count" ? (object)0 : null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows.Where(row => row.Cells.Exists(columnKey))
                        .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                        .Where(value => value.HasValue).Sum(value => value.Value);
                case "Count":
                    return visibleRows.Count(row => row.Cells.Exists(columnKey) && row.Cells[columnKey].Value != null && row.Cells[columnKey].Value != DBNull.Value);
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

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            try { return Convert.ToDecimal(value); } catch { return null; }
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
                xOffset += column.Width;
            }
        }

        private void filter_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            ApplyClientFilters();
        }

        private void btnViewGrid_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to preview grid.\n" + ex.Message, "Print Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            try
            {
                gridReport.PrintPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to preview report.\n" + ex.Message, "Print Report Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV File (*.csv)|*.csv";
                    saveFileDialog.FileName = $"GST_Tax_Report_{DateTime.Now:yyyyMMdd_HHmmss}";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCsv(saveFileDialog.FileName);
                        MessageBox.Show("Report exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to export grid.\n" + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string filePath)
        {
            List<GSTAndTaxReportRow> rows = gridReport.DataSource as List<GSTAndTaxReportRow>;
            if (rows == null || rows.Count == 0) return;

            using (StreamWriter sw = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("Transaction Type,Invoice No,Date,Customer/Vendor,GSTIN,Item Description,HSN Code,Qty,Unit,Taxable Value,CGST %,CGST Amt,SGST %,SGST Amt,IGST %,IGST Amt,Cess %,Cess Amt,Total Tax,Gross Amount,Tax Mode");
                foreach (GSTAndTaxReportRow r in rows)
                {
                    sw.WriteLine($"\"{r.TrnsType}\",\"{r.InvoiceNo}\",\"{r.DocDate:dd/MM/yyyy}\",\"{r.PartyName}\",\"{r.PartyGSTIN}\",\"{r.ItemName}\",\"{r.HSNCode}\",{r.Qty},\"{r.Unit}\",{r.TaxableAmt},{r.CGSTPer},{r.CGSTAmt},{r.SGSTPer},{r.SGSTAmt},{r.IGSTPer},{r.IGSTAmt},{r.CessPer},{r.CessAmt},{r.TotalTaxAmt},{r.GrandTotal},\"{r.TaxType}\"");
                }
            }
        }

        private void btnSearchItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (PosBranch_Win.DialogBox.frmdialForItemMaster dialog = new PosBranch_Win.DialogBox.frmdialForItemMaster("frmgstandtaxreport"))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        var data = dialog.GetSelectedItemData();
                        string selectedItemName = GetDictionaryString(data, "Description");
                        if (string.IsNullOrWhiteSpace(selectedItemName))
                            selectedItemName = GetDictionaryString(data, "ItemName");
                        if (!string.IsNullOrWhiteSpace(selectedItemName))
                        {
                            txtSearch.Text = selectedItemName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Item Master lookup: " + ex.Message, "Item Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string GetDictionaryString(System.Collections.IDictionary dict, string key)
        {
            if (dict != null && dict.Contains(key) && dict[key] != null)
                return Convert.ToString(dict[key]);
            return string.Empty;
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        private void frmgstandtaxreport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                LoadReport();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnPreviewGrid_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F7)
            {
                btnExportGrid_Click(sender, e);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F8)
            {
                btnPreviewReport_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ultraComboTrnsType_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
