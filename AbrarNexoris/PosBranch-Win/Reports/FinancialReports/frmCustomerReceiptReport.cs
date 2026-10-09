using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.FinancialReports
{
    public partial class frmCustomerReceiptReport : Form
    {
        #region Colour Palette
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
        private readonly CustomerReceiptReportRepository _repository;
        private List<CustomerReceiptReportRow> _reportRows;
        private List<CustomerDDl> _customers;
        private bool _isLoading;
        private bool _isSyncingCustomerControls;

        // Dynamic footer panel and cell controls (matching frmVendorOutstandingReport / frmStockReport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TotalAmount",
            "ReceiptAmount",
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
        #endregion

        #region Helper Classes
        private sealed class ColumnChooserItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }

            public ColumnChooserItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }

            public override string ToString() => DisplayText;
        }
        #endregion

        #region Constructor
        public frmCustomerReceiptReport()
        {
            _repository = new CustomerReceiptReportRepository();
            _reportRows = new List<CustomerReceiptReportRow>();
            _customers = new List<CustomerDDl>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
            _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            InitializeComponent();

            Load += frmCustomerReceiptReport_Load;
            btnSearch.Click += btnSearch_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            btnExport.Click += btnExport_Click;
            comboBox1.ValueChanged += comboBox1_ValueChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.KeyDown += txtSearch_KeyDown;
            ultraComboCustomer.ValueChanged += ultraComboCustomer_ValueChanged;
            ultraComboCustomer.KeyDown += ultraComboCustomer_KeyDown;
            button1.Click += button1_Click;
            ultraButton1.Click += ultraButton1_Click;
            ultraButton2.Click += ultraButton2_Click;
            ultraButton3.Click += ultraButton3_Click;

            // Grid event registrations for footer cell sync and column drag
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues();
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) => UpdateFooterValues();

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Resize += (s, e) => UpdateFooterCellPositions();
            }

            KeyPreview = true;
            KeyDown += frmCustomerReceiptReport_KeyDown;
            FormClosing += FrmCustomerReceiptReport_FormClosing;
        }
        #endregion

        #region Form Lifecycle
        private void frmCustomerReceiptReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void FrmCustomerReceiptReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Dispose();
                columnChooserForm = null;
            }
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
                SetupHeaderDragToHideAndColumnChooser();
                LoadCustomers();
                ResetReportView();
                UpdateDateControlState();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Filter Controls Setup
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
            comboBox1.Items.Add("ALL", "ALL");
            comboBox1.Items.Add("ByRange", "By Range");
            comboBox1.Items.Add("Today", "Today");
            comboBox1.Items.Add("Yesterday", "Yesterday");
            comboBox1.Items.Add("ThisWeek", "This Week");
            comboBox1.Items.Add("ThisMonth", "This Month");
            comboBox1.Items.Add("LastMonth", "Last Month");
            comboBox1.Value = "ALL";

            txtSearch.Text = string.Empty;
        }

        private void ApplyQuickDateSelection()
        {
            string preset = Convert.ToString(comboBox1.Value);
            bool isAll = string.IsNullOrWhiteSpace(preset) || string.Equals(preset, "ALL", StringComparison.OrdinalIgnoreCase);
            bool isRange = string.Equals(preset, "ByRange", StringComparison.OrdinalIgnoreCase);
            DateTime today = DateTime.Today;
            DateTime fromDate = Convert.ToDateTime(dtFrom.Value).Date;
            DateTime toDate = Convert.ToDateTime(dtTo.Value).Date;

            switch (preset)
            {
                case "ALL":
                    break;
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

            if (!isRange && !isAll)
            {
                dtFrom.Value = fromDate;
                dtTo.Value = toDate;
            }

            UpdateDateControlState();
        }

        private void UpdateDateControlState()
        {
            string preset = Convert.ToString(comboBox1.Value);
            bool isAll = string.IsNullOrWhiteSpace(preset) || string.Equals(preset, "ALL", StringComparison.OrdinalIgnoreCase);
            bool isRange = string.Equals(preset, "ByRange", StringComparison.OrdinalIgnoreCase);

            lblFromDate.Visible = !isAll;
            dtFrom.Visible = !isAll;
            dtFrom.Enabled = isRange;

            lblToDate.Visible = !isAll;
            dtTo.Visible = !isAll;
            dtTo.Enabled = isRange;
        }
        #endregion

        #region Styling & Layout
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelControls.Dock = DockStyle.Top;
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
                ultraPanelGridFooter.Height = 26;
            }

            StyleLabel(lblCustomer);
            StyleLabel(lblSearch);
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
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(comboBox1, true);
            StyleFilterCombo(txtSearch, false);
            StyleFilterCombo(ultraComboCustomer, false);
            StyleDateEditor(dtFrom);
            StyleDateEditor(dtTo);
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool readOnly)
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
            combo.Appearance.FontData.SizeInPoints = 8.25F;
            combo.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.ReadOnly = readOnly;
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
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 8.25F;
            editor.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Segoe UI";
            label.Appearance.FontData.SizeInPoints = 9;
            label.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void SetupGrid()
        {
            UltraGridOverride ovr = gridReport.DisplayLayout.Override;
            UltraGridLayout layout = gridReport.DisplayLayout;

            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.Appearance.BorderColor = BorderBlue;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.AutoFitStyle = AutoFitStyle.None;
            layout.Scrollbars = Scrollbars.Both;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;

            ovr.RowSelectors = DefaultableBoolean.True;
            ovr.RowSelectorWidth = 35;
            ovr.RowSelectorHeaderStyle = RowSelectorHeaderStyle.SeparateElement;
            ovr.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            ovr.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            ovr.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            ovr.RowSelectorAppearance.BorderColor = BorderBlue;
            ovr.RowSelectorAppearance.ForeColor = Color.White;
            ovr.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            ovr.RowSelectorAppearance.TextHAlign = HAlign.Center;
            ovr.RowSelectorAppearance.TextVAlign = VAlign.Middle;

            ovr.HeaderClickAction = HeaderClickAction.SortMulti;
            ovr.SelectTypeRow = SelectType.Single;
            ovr.SelectTypeCol = SelectType.None;
            ovr.CellClickAction = CellClickAction.RowSelect;
            ovr.AllowColMoving = AllowColMoving.WithinBand;
            ovr.AllowColSizing = AllowColSizing.Free;
            ovr.AllowRowFiltering = DefaultableBoolean.True;
            ovr.FilterUIType = FilterUIType.HeaderIcons;
            ovr.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            ovr.FilterOperandStyle = FilterOperandStyle.Combo;
            ovr.FilterClearButtonLocation = FilterClearButtonLocation.Hidden;
            ovr.FilterRowPrompt = "Click here to filter data...";
            ovr.FilterRowAppearance.BackColor = Color.FromArgb(245, 249, 255);
            ovr.FilterRowAppearance.BorderColor = BorderBlue;
            ovr.FilterCellAppearance.BackColor = Color.White;
            ovr.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);

            ovr.HeaderAppearance.BackColor = GridHeaderBlue;
            ovr.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ovr.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ovr.HeaderAppearance.ForeColor = Color.White;
            ovr.HeaderAppearance.BorderColor = BorderBlue;
            ovr.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            ovr.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            ovr.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            ovr.HeaderAppearance.TextHAlign = HAlign.Center;
            ovr.HeaderAppearance.TextVAlign = VAlign.Middle;

            ovr.RowAppearance.BackColor = Color.White;
            ovr.RowAlternateAppearance.BackColor = GridAltRow;
            ovr.RowAppearance.BorderColor = GridRowLine;
            ovr.RowAlternateAppearance.BorderColor = GridRowLine;
            ovr.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ovr.SelectedRowAppearance.ForeColor = Color.White;
            ovr.SelectedRowAppearance.BorderColor = BorderBlue;
            ovr.ActiveRowAppearance.BackColor = GridSelectedBlue;
            ovr.ActiveRowAppearance.ForeColor = Color.White;
            ovr.ActiveRowAppearance.BorderColor = BorderBlue;

            ovr.CellAppearance.BorderColor = GridRowLine;
            ovr.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            ovr.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            ovr.CellAppearance.FontData.SizeInPoints = 8.25F;
            ovr.BorderStyleHeader = UIElementBorderStyle.Solid;
            ovr.BorderStyleCell = UIElementBorderStyle.Solid;
            ovr.BorderStyleRow = UIElementBorderStyle.Solid;
            ovr.MinRowHeight = 20;
            ovr.DefaultRowHeight = 22;
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
        #endregion

        #region Grid Footer Setup & Alignment (Parity with frmVendorOutstandingReport / frmStockReport)
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
            if (ultraPanelGridFooter == null) return;
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

            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

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
            string clean = Convert.ToString(value).Replace("₹", "").Trim();
            return decimal.TryParse(clean, out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null) return false;
            return string.Equals(column.Key, "TotalAmount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "ReceiptAmount", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Balance", StringComparison.OrdinalIgnoreCase) ||
                   column.DataType == typeof(decimal) ||
                   column.DataType == typeof(double) ||
                   column.DataType == typeof(float) ||
                   column.DataType == typeof(int) ||
                   column.DataType == typeof(long);
        }
        #endregion

        #region Header Drag-to-Hide & Column Chooser (Parity with frmVendorOutstandingReport / frmStockReport)
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
            gridReport.AllowDrop = true;
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

                if (headerUI != null && headerUI.Header?.Column != null)
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

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
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
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
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

        private void HideColumn(UltraGridColumn col)
        {
            if (col == null) return;
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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

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

            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(4, 4, 4, 4),
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
            string filter = txtColumnSearch?.Text?.Trim() ?? string.Empty;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.Caption))
            {
                if (col.Hidden)
                {
                    string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    if (string.IsNullOrEmpty(filter) || caption.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
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

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                return;

            if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                return;

            Rectangle rect = e.Bounds;
            rect.Inflate(-4, -3);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color badgeColor = Color.FromArgb(0, 121, 211);

            using (SolidBrush bgBrush = new SolidBrush(badgeColor))
            using (GraphicsPath path = RoundedRect(rect, 4))
            {
                e.Graphics.FillPath(bgBrush, path);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(item.DisplayText, columnChooserListBox.Font, textBrush, rect, sf);
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

        private void UnhideColumn(string columnKey, int targetPosition)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            if (band.Columns.Exists(columnKey))
            {
                userHiddenColumnKeys.Remove(columnKey);
                UltraGridColumn col = band.Columns[columnKey];
                col.Hidden = false;
                col.Header.VisiblePosition = targetPosition;
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }
        }

        private void UnhideAllColumns()
        {
            userHiddenColumnKeys.Clear();
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = false;
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
        #endregion

        #region Data Loading & Grid Binding
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
                bool useDateFilter = !string.Equals(Convert.ToString(comboBox1.Value), "ALL", StringComparison.OrdinalIgnoreCase);

                CustomerReceiptReportFilter filter = new CustomerReceiptReportFilter
                {
                    FromDate = useDateFilter ? (DateTime?)Convert.ToDateTime(dtFrom.Value).Date : null,
                    ToDate = useDateFilter ? (DateTime?)Convert.ToDateTime(dtTo.Value).Date : null,
                    BranchId = SessionContext.BranchId,
                    CustomerLedgerId = GetSelectedLedgerId(),
                    UseDateFilter = useDateFilter
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
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ResetFormState()
        {
            _isLoading = true;

            try
            {
                DateTime today = DateTime.Today;
                comboBox1.Value = "ALL";
                dtFrom.Value = new DateTime(today.Year, today.Month, 1);
                dtTo.Value = today;
                ultraComboCustomer.Value = 0;
                txtSearch.Text = string.Empty;
                _reportRows = new List<CustomerReceiptReportRow>();
                ResetReportView();
                UpdateDateControlState();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetReportView()
        {
            gridReport.DataSource = null;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private bool ValidateDateRange()
        {
            if (string.Equals(Convert.ToString(comboBox1.Value), "ALL", StringComparison.OrdinalIgnoreCase))
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
            column.Hidden = userHiddenColumnKeys.Contains(key);
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
        #endregion

        #region Control Event Handlers
        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        public void RibbonClear() => ResetFormState();
        public void Clear() => ResetFormState();

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCsv();
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

        private void ultraButton3_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to print.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor previousCursor = Cursor;
                Cursor = Cursors.WaitCursor;

                try
                {
                    UltraGridPrintDocument printDocument = new UltraGridPrintDocument();
                    printDocument.Grid = gridReport;
                    printDocument.DefaultPageSettings.Landscape = true;
                    printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 40, 60, 60);

                    string customerName = ultraComboCustomer.Value == null || Convert.ToInt32(ultraComboCustomer.Value) <= 0
                        ? "All Customers"
                        : ultraComboCustomer.Text;

                    printDocument.Header.TextCenter = string.Format("Customer Receipt Report{0}Customer: {1}", Environment.NewLine, customerName);
                    printDocument.Header.TextRight = string.Format("Print Date: {0:dd-MMM-yyyy hh:mm tt}", DateTime.Now);
                    printDocument.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                    printDocument.Header.Appearance.FontData.SizeInPoints = 12;
                    printDocument.Header.Appearance.TextHAlign = HAlign.Center;
                    printDocument.Header.Appearance.TextVAlign = VAlign.Middle;
                    printDocument.Footer.TextCenter = "Page [Page #]";
                    printDocument.FitWidthToPages = 1;

                    using (PrintPreviewDialog previewDialog = new PrintPreviewDialog())
                    {
                        previewDialog.Document = printDocument;
                        previewDialog.WindowState = FormWindowState.Maximized;
                        previewDialog.ShowDialog(this);
                    }
                }
                finally
                {
                    Cursor = previousCursor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error generating print preview: {0}", ex.Message), "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        #endregion
    }
}
