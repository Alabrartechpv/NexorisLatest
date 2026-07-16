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
    public partial class frmCustomerReceiptReport : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(214, 230, 240);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(214, 229, 241);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBlue = Color.FromArgb(187, 214, 243);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonLightOutline = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private readonly CustomerReceiptReportRepository _repository;
        private List<CustomerReceiptReportRow> _reportRows;
        private List<CustomerDDl> _customers;
        private bool _isLoading;
        private bool _isSyncingCustomerControls;
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;

        public frmCustomerReceiptReport()
        {
            _repository = new CustomerReceiptReportRepository();
            _reportRows = new List<CustomerReceiptReportRow>();
            _customers = new List<CustomerDDl>();
            _footerLabels = new Dictionary<string, Label>();
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmCustomerReceiptReport_Load;
            btnSearch.Click += btnSearch_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExport.Click += btnExport_Click;
            comboBox1.ValueChanged += comboBox1_ValueChanged;
            ultraComboPreset.ValueChanged += ultraComboPreset_ValueChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            ultraComboCustomer.ValueChanged += ultraComboCustomer_ValueChanged;
            ultraComboCustomer.KeyDown += ultraComboCustomer_KeyDown;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += gridReport_Resize;
            button1.Click += button1_Click;
            ultraButton1.Click += ultraButton1_Click;
            ultraButton2.Click += ultraButton2_Click;

            KeyPreview = true;
            KeyDown += frmCustomerReceiptReport_KeyDown;
        }

        private void frmCustomerReceiptReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;

            try
            {
                Text = "Customer Receipt Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeDateControls();
                InitializeSearchControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                InitializeGridFooter();
                LoadCustomers();
                ResetReportView();
            }
            finally
            {
                _isLoading = false;
            }
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
        }

        private void InitializeSearchControls()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("ByRange", "By Range");
            comboBox1.Items.Add("Today", "Today");
            comboBox1.Items.Add("Yesterday", "Yesterday");
            comboBox1.Items.Add("ThisWeek", "This Week");
            comboBox1.Items.Add("ThisMonth", "This Month");
            comboBox1.Items.Add("LastMonth", "Last Month");
            comboBox1.Value = "ByRange";

            ultraComboPreset.Items.Clear();
            ultraComboPreset.Items.Add("CustomerReceipt", "Customer Receipt");
            ultraComboPreset.Value = "CustomerReceipt";

            txtSearch.Text = string.Empty;
        }

        private void ApplyQuickDateSelection()
        {
            string preset = Convert.ToString(comboBox1.Value);
            DateTime today = DateTime.Today;
            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date;
            bool isRange = string.IsNullOrWhiteSpace(preset) || string.Equals(preset, "ByRange", StringComparison.OrdinalIgnoreCase);

            switch (preset)
            {
                case "Today":
                    fromDate = today;
                    toDate = today;
                    break;
                case "Yesterday":
                    fromDate = today.AddDays(-1);
                    toDate = today.AddDays(-1);
                    break;
                case "ThisWeek":
                    int daysFromMonday = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    fromDate = today.AddDays(-daysFromMonday);
                    toDate = today;
                    break;
                case "ThisMonth":
                    fromDate = new DateTime(today.Year, today.Month, 1);
                    toDate = today;
                    break;
                case "LastMonth":
                    DateTime lastMonth = today.AddMonths(-1);
                    fromDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                    break;
            }

            dtFrom.Enabled = isRange;
            dtTo.Enabled = isRange;

            if (!isRange)
            {
                dtFrom.Value = fromDate;
                dtTo.Value = toDate;
            }
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;
            ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelMaster.Appearance.BackColor = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabel(lblCustomer);
            StyleLabel(lblSearch);
            StyleLabel(lblPreset);
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);

            UpdateSelectionToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnSearch);
            StyleClassicButton(btnClearFilters);
            StyleClassicButton(btnExport);
            StyleClassicButton(ultraButton1);
            StyleClassicButton(ultraButton2);
            StyleClassicButton(ultraButton3);
            StylePickerButton(button1);
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
            StyleFilterCombo(comboBox1, true);
            StyleFilterCombo(txtSearch, false);
            StyleUltraCombo(ultraComboPreset);
            StyleUltraCombo(ultraComboCustomer);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
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

        private static void StyleUltraCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
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

            layout.Appearance.BackColor = Color.White;
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
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
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

        private void InitializeGridFooter()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void LoadCustomers()
        {
            _customers = _repository.GetCustomers()
                .Where(x => x != null && x.LedgerID > 0)
                .OrderBy(x => x.LedgerName)
                .ToList();

            ultraComboCustomer.Items.Clear();
            ultraComboCustomer.Items.Add(0, "All Customers");
            txtSearch.Items.Clear();

            foreach (CustomerDDl customer in _customers)
            {
                string displayText = GetCustomerDisplayText(customer);

                ultraComboCustomer.Items.Add(customer.LedgerID, displayText);
                txtSearch.Items.Add("display_" + customer.LedgerID, displayText);
                txtSearch.Items.Add("id_" + customer.LedgerID, customer.LedgerID.ToString());
                txtSearch.Items.Add("name_" + customer.LedgerID, customer.LedgerName ?? string.Empty);
            }

            ultraComboCustomer.Value = 0;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                CustomerReceiptReportFilter filter = new CustomerReceiptReportFilter
                {
                    FromDate = Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = Convert.ToDateTime(dtTo.Value).Date,
                    BranchId = SessionContext.BranchId,
                    CustomerLedgerId = GetSelectedLedgerId()
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load customer receipt report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<CustomerReceiptReportRow> filteredRows = _reportRows ?? Enumerable.Empty<CustomerReceiptReportRow>();
            string searchText = GetSearchText();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.CustomerName) && x.CustomerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    GetCustomerDisplayText(x.CustomerLedgerId, x.CustomerName).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.CustomerLedgerId.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.VoucherId.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.BillNo.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.Status.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<CustomerReceiptReportRow> boundRows = filteredRows
                .OrderByDescending(x => x.VoucherDate)
                .ThenByDescending(x => x.VoucherId)
                .ToList();

            gridReport.DataSource = boundRows;
            UpdateFooterValues();
        }

        private void ResetFormState()
        {
            _isLoading = true;

            try
            {
                DateTime today = DateTime.Today;
                comboBox1.Value = "ByRange";
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                ultraComboPreset.Value = "CustomerReceipt";
                ultraComboCustomer.Value = 0;
                txtSearch.Text = string.Empty;
                _reportRows = new List<CustomerReceiptReportRow>();
                ResetReportView();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetReportView()
        {
            gridReport.DataSource = null;
            UpdateFooterValues();
        }

        private bool ValidateDateRange()
        {
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
            if (ultraComboCustomer.Value == null)
                return 0;

            int ledgerId;
            return int.TryParse(ultraComboCustomer.Value.ToString(), out ledgerId) ? ledgerId : 0;
        }

        private string GetSearchText()
        {
            return string.IsNullOrWhiteSpace(txtSearch.Text) ? string.Empty : txtSearch.Text.Trim();
        }

        private void ExportCsv()
        {
            List<CustomerReceiptReportRow> rows = gridReport.DataSource as List<CustomerReceiptReportRow>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = string.Format("CustomerReceipt_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Date,Receipt No,Bill No,Customer ID,Customer,Total Amount,Receipt Amount,Balance,Status");

                foreach (CustomerReceiptReportRow row in rows)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(row.VoucherDate.ToString("yyyy-MM-dd")),
                        row.VoucherId.ToString(),
                        row.BillNo.ToString(),
                        row.CustomerLedgerId.ToString(),
                        EscapeCsv(row.CustomerName),
                        row.TotalAmount.ToString("F2"),
                        row.ReceiptAmount.ToString("F2"),
                        row.Balance.ToString("F2"),
                        EscapeCsv(row.Status)));
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

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void ultraComboPreset_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || ultraComboPreset.Value == null)
                return;

            string preset = ultraComboPreset.Value.ToString();
            DateTime today = DateTime.Today;

            switch (preset)
            {
                case "CustomerReceipt":
                    comboBox1.Value = "ByRange";
                    dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                    dtTo.Value = today;
                    break;
            }
        }

        private void comboBox1_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyQuickDateSelection();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            TrySyncCustomerSelectionFromSearchText();
        }

        private void ultraComboCustomer_ValueChanged(object sender, EventArgs e)
        {
            if (_isLoading || _isSyncingCustomerControls)
                return;

            SyncSearchFromSelectedCustomer();
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

            ConfigureGridColumn(band, "VoucherDate", "Date", 108, "dd-MMM-yyyy", HAlign.Left, 0);
            ConfigureGridColumn(band, "VoucherId", "Receipt No", 92, null, HAlign.Right, 1);
            ConfigureGridColumn(band, "BillNo", "Bill No", 92, null, HAlign.Right, 2);
            ConfigureGridColumn(band, "CustomerLedgerId", "Customer ID", 92, null, HAlign.Right, 3);
            ConfigureGridColumn(band, "CustomerName", "Customer", 210, null, HAlign.Left, 4);
            ConfigureGridColumn(band, "TotalAmount", "Total Amount", 96, "#,##0.00", HAlign.Right, 5);
            ConfigureGridColumn(band, "ReceiptAmount", "Receipt Amount", 104, "#,##0.00", HAlign.Right, 6);
            ConfigureGridColumn(band, "Balance", "Balance", 84, "#,##0.00", HAlign.Right, 7);
            ConfigureGridColumn(band, "Status", "Status", 72, null, HAlign.Left, 8);

            if (band.Columns.Exists("CustomerName"))
            {
                band.Columns["CustomerName"].CellAppearance.FontData.Bold = DefaultableBoolean.False;
                band.Columns["CustomerName"].CellAppearance.FontData.Name = "Microsoft Sans Serif";
                band.Columns["CustomerName"].CellAppearance.FontData.SizeInPoints = 8.25F;
            }

            if (band.Columns.Exists("ReceiptAmount"))
            {
                band.Columns["ReceiptAmount"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
                band.Columns["ReceiptAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
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

            if (balance > 0)
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            }
            else
            {
                e.Row.Cells["Balance"].Appearance.ForeColor = Color.FromArgb(46, 125, 50);
            }

            if (e.Row.Cells.Exists("Status"))
            {
                bool isClosed = string.Equals(Convert.ToString(e.Row.Cells["Status"].Value), "Closed", StringComparison.OrdinalIgnoreCase);
                e.Row.Cells["Status"].Appearance.ForeColor = isClosed
                    ? Color.FromArgb(46, 125, 50)
                    : Color.FromArgb(191, 54, 12);
                e.Row.Cells["Status"].Appearance.FontData.Bold = DefaultableBoolean.True;
            }
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
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

                Label footerLabel = new Label();
                footerLabel.Name = "footer_" + column.Key;
                footerLabel.Text = string.Empty;
                footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                footerLabel.BackColor = GridHeaderBlue;
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.Tag = Tuple.Create(column.Key, string.Empty);
                footerLabel.ForeColor = Color.White;
                footerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                footerLabel.Paint += FooterLabel_Paint;
                footerLabel.ContextMenuStrip = CreateFooterContextMenu(column.Key);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
            itemSum.Tag = "Sum";
            itemSum.Enabled = isNumeric;
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min");
            itemMin.Tag = "Min";
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max");
            itemMax.Tag = "Max";
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count");
            itemCount.Tag = "Count";
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average");
            itemAverage.Tag = "Avg";
            itemAverage.Enabled = isNumeric;
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
            itemNone.Tag = "None";
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
                    : "None";

                foreach (ToolStripItem menuItem in menu.Items)
                {
                    ToolStripMenuItem toolStripMenuItem = menuItem as ToolStripMenuItem;
                    if (toolStripMenuItem != null && toolStripMenuItem.Tag != null)
                    {
                        toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                    }
                }
            };

            return menu;
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null)
                return;

            string columnKey = menu.Tag.ToString();
            string aggregation = item.Tag.ToString();

            _columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
        }

        private void UpdateFooterValues()
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
                return Convert.ToString(result);

            if (gridReport.DisplayLayout != null &&
                gridReport.DisplayLayout.Bands.Count > 0 &&
                gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
                decimal? numericValue = GetNumericValue(result);
                if (numericValue.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(column.Format))
                        return numericValue.Value.ToString(column.Format);

                    return numericValue.Value.ToString("N2");
                }
            }

            return Convert.ToString(result);
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0)
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
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null)
                return false;

            Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return type == typeof(decimal) ||
                   type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(short) ||
                   type == typeof(byte);
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label footerLabel = sender as Label;
            if (footerLabel == null)
                return;

            Tuple<string, string> tagData = footerLabel.Tag as Tuple<string, string>;
            string columnKey = tagData != null ? tagData.Item1 : string.Empty;
            string displayText = tagData != null ? tagData.Item2 : footerLabel.Text;

            if (string.IsNullOrWhiteSpace(displayText))
                return;

            if (_columnAggregations.ContainsKey(columnKey) &&
                string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                return;

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            SizeF textSize = graphics.MeasureString(displayText, footerLabel.Font);
            int padding = 6;
            int cornerRadius = 6;
            int margin = 1;
            int boxWidth = footerLabel.Width - (margin * 2);
            int boxHeight = (int)textSize.Height + padding;
            int x = margin;
            int y = (footerLabel.Height - boxHeight) / 2;

            Rectangle rect = new Rectangle(x, y, boxWidth, boxHeight);
            Color boxColor = Color.FromArgb(0, 80, 160);

            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseAllFigures();

                using (SolidBrush brush = new SolidBrush(boxColor))
                {
                    graphics.FillPath(brush, path);
                }
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                float textX = x + (boxWidth - textSize.Width) / 2;
                float textY = y + (boxHeight - textSize.Height) / 2 - 1;
                graphics.DrawString(displayText, footerLabel.Font, textBrush, textX, textY);
            }

            footerLabel.Text = string.Empty;
        }

        private void frmCustomerReceiptReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                button1.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                btnExport.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnSearch.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F6)
            {
                btnClearFilters.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenCustomerDialog();
        }

        private void ultraButton1_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateSelectionToggleButtonText();
        }

        private void ultraButton2_Click(object sender, EventArgs e)
        {
            ShowReportFormatDialog(
                "CUSTOMER RECEIPT",
                new[]
                {
                    "CUSTOMER RECEIPT DETAILS",
                    "CUSTOMER RECEIPT DETAILS - GROUP BY DOCUMENT",
                    "CUSTOMER RECEIPT SUMMARY"
                });
        }

        private void UpdateSelectionToggleButtonText()
        {
            ultraButton1.Text = ultraPanelControls.Visible ? "Hide Selection" : "View Selection";
        }

        private void ShowReportFormatDialog(string reportCaption, IEnumerable<string> formatDescriptions)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(reportCaption, formatDescriptions))
            {
                dialog.ShowDialog(this);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
                e.Handled = true;
            }
        }

        private void ultraComboCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                OpenCustomerDialog();
                e.Handled = true;
            }
        }

        private void OpenCustomerDialog()
        {
            using (frmCustomerDialog customerDialog = new frmCustomerDialog())
            {
                if (customerDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (customerDialog.SelectedCustomerId <= 0)
                    return;

                SelectCustomer(customerDialog.SelectedCustomerId, customerDialog.SelectedCustomerName);
            }
        }

        private void SelectCustomer(int customerId, string customerName)
        {
            _isSyncingCustomerControls = true;

            try
            {
                ultraComboCustomer.Value = customerId;

                CustomerDDl customer = _customers.FirstOrDefault(x => x.LedgerID == customerId);
                string searchText = customer != null
                    ? (customer.LedgerName ?? string.Empty)
                    : (customerName ?? string.Empty);

                txtSearch.Text = searchText;
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }
        }

        private void SyncSearchFromSelectedCustomer()
        {
            if (_isSyncingCustomerControls)
                return;

            int selectedLedgerId = GetSelectedLedgerId();
            if (selectedLedgerId <= 0)
                return;

            CustomerDDl customer = _customers.FirstOrDefault(x => x.LedgerID == selectedLedgerId);
            if (customer == null)
                return;

            _isSyncingCustomerControls = true;
            try
            {
                txtSearch.Text = customer.LedgerName ?? string.Empty;
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }
        }

        private void TrySyncCustomerSelectionFromSearchText()
        {
            if (_isSyncingCustomerControls)
                return;

            string searchText = GetSearchText();
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

            CustomerDDl customer = _customers.FirstOrDefault(x =>
                x.LedgerID.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(x.LedgerName) && x.LedgerName.Equals(searchText, StringComparison.OrdinalIgnoreCase)) ||
                GetCustomerDisplayText(x).Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (customer == null)
            {
                if (GetSelectedLedgerId() > 0)
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

                return;
            }

            _isSyncingCustomerControls = true;
            try
            {
                ultraComboCustomer.Value = customer.LedgerID;
            }
            finally
            {
                _isSyncingCustomerControls = false;
            }
        }

        private static string GetCustomerDisplayText(CustomerDDl customer)
        {
            if (customer == null)
                return string.Empty;

            return GetCustomerDisplayText(customer.LedgerID, customer.LedgerName);
        }

        private static string GetCustomerDisplayText(int ledgerId, string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return ledgerId.ToString();

            return string.Format("{0} - {1}", ledgerId, customerName);
        }
    }
}

