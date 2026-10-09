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
    public partial class frmVendorOutstandingReport : Form
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
        private readonly VendorOutstandingReportRepository _repository;
        private List<VendorOutstandingReportRow> _reportRows;
        private List<VendorGridList> _vendors;
        private bool _isLoading;
        private bool _isSyncingVendorControls;
        private readonly bool _getUnallocatedReturnsOnly;
        private Panel pnlWarning;
        private Label lblWarning;

        // Dynamic footer panel and cell controls (matching frmStockReport)
        private readonly Dictionary<string, Label> _footerLabels;
        private readonly Dictionary<string, string> _columnAggregations;
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] summaryTypes = new[] { "Sum", "Min", "Max", "Average", "Count", "None" };

        private readonly HashSet<string> summaryDefaultNumericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DocAmt",
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

        #region Constructors
        public frmVendorOutstandingReport()
        {
            _repository = new VendorOutstandingReportRepository();
            _reportRows = new List<VendorOutstandingReportRow>();
            _vendors = new List<VendorGridList>();
            _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
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

            // Grid event registrations for footer cell sync and column drag
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow += gridReport_InitializeRow;
            gridReport.DoubleClickRow += gridReport_DoubleClickRow;
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) =>
            {
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
            };
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowFilterChanged += (s, e) =>
            {
                UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
                UpdateFooterCellPositions();
            };
            gridReport.AfterSortChange += (s, e) => UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Resize += (s, e) => UpdateFooterCellPositions();
            }

            KeyPreview = true;
            KeyDown += frmVendorOutstandingReport_KeyDown;
            FormClosing += FrmVendorOutstandingReport_FormClosing;
        }

        public frmVendorOutstandingReport(bool getUnallocatedReturnsOnly) : this()
        {
            _getUnallocatedReturnsOnly = getUnallocatedReturnsOnly;
        }
        #endregion

        #region Form Lifecycle
        private void frmVendorOutstandingReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void FrmVendorOutstandingReport_FormClosing(object sender, FormClosingEventArgs e)
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
                SetupHeaderDragToHideAndColumnChooser();
                ResetReportView();
                InitializeWarningPanel();
            }
            finally
            {
                _isLoading = false;
            }
        }
        #endregion

        #region Filter Controls Setup
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
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            label.Appearance.FontData.Bold = DefaultableBoolean.False;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9;
            label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
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
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
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
        #endregion

        #region Grid Footer Setup & Alignment (Parity with frmStockReport)
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
            UpdateFooterValues(new List<VendorOutstandingReportRow>());
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
                UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
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

        private void UpdateFooterValues(IList<VendorOutstandingReportRow> rows = null)
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
            return string.Equals(column.Key, "DocAmt", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(column.Key, "Balance", StringComparison.OrdinalIgnoreCase) ||
                   column.DataType == typeof(decimal) ||
                   column.DataType == typeof(double) ||
                   column.DataType == typeof(float) ||
                   column.DataType == typeof(int) ||
                   column.DataType == typeof(long) ||
                   column.DataType == typeof(short);
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
            UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
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
                UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
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
            UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
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
        private void LoadVendors()
        {
            _vendors = _repository.GetVendors();
            ultraComboVendor.Items.Clear();
            ultraComboVendor.Items.Add(0, "All Vendors");
            txtVendorSearch.Items.Clear();

            ultraComboVendorFrom.Items.Clear();
            ultraComboVendorTo.Items.Clear();

            foreach (VendorGridList vendor in _vendors)
            {
                string displayText = GetVendorDisplayText(vendor);
                ultraComboVendor.Items.Add(vendor.LedgerID, displayText);
                txtVendorSearch.Items.Add("display_" + vendor.LedgerID, displayText);
                txtVendorSearch.Items.Add("id_" + vendor.LedgerID, vendor.LedgerID.ToString());
                txtVendorSearch.Items.Add("name_" + vendor.LedgerID, vendor.LedgerName ?? string.Empty);
                ultraComboVendorFrom.Items.Add(vendor.LedgerID, displayText);
                ultraComboVendorTo.Items.Add(vendor.LedgerID, displayText);
            }

            ultraComboVendor.Value = 0;
            ultraComboVendorFrom.Value = 0;
            ultraComboVendorTo.Value = 0;
        }

        private void LoadReport()
        {
            if (!ValidateDateRange())
                return;

            if (chkPaymentDueOnly.Checked && !ValidatePaymentDueDateRange())
                return;

            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                VendorOutstandingReportFilter filter = new VendorOutstandingReportFilter
                {
                    BranchId = SessionContext.BranchId,
                    GetUnallocatedReturnsOnly = _getUnallocatedReturnsOnly,
                    FromDate = dtFrom.Value == null ? DateTime.MinValue : Convert.ToDateTime(dtFrom.Value).Date,
                    ToDate = dtTo.Value == null ? DateTime.MaxValue : Convert.ToDateTime(dtTo.Value).Date,
                    DateFilterMode = chkPaymentDueOnly.Checked ? "DOC_DATE" : Convert.ToString(ultraComboDateMode.Value),
                    UseDateFilter = chkPaymentDueOnly.Checked || IsDateRangeMode(),
                    PaymentDueOnly = chkPaymentDueOnly.Checked,
                    LedgerId = GetSelectedLedgerId(),
                    FromLedgerId = GetLedgerId(ultraComboVendorFrom),
                    ToLedgerId = GetLedgerId(ultraComboVendorTo)
                };

                _reportRows = _repository.GetReport(filter);
                ApplyClientFilters();

                if (!_getUnallocatedReturnsOnly && pnlWarning != null)
                {
                    VendorOutstandingReportFilter unallocatedFilter = new VendorOutstandingReportFilter
                    {
                        BranchId = SessionContext.BranchId,
                        GetUnallocatedReturnsOnly = true,
                        FromDate = DateTime.MinValue,
                        ToDate = DateTime.MaxValue
                    };
                    List<VendorOutstandingReportRow> unallocated = _repository.GetReport(unallocatedFilter);
                    pnlWarning.Visible = unallocated != null && unallocated.Count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor outstanding report.\n" + ex.Message, "Report Error",
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

            if (IsVendorSelectionMode())
            {
                int selectedLedgerId = GetSelectedLedgerId();
                if (selectedLedgerId > 0)
                {
                    filteredRows = filteredRows.Where(x => x.AcctCode == selectedLedgerId);
                }
            }
            else if (IsVendorRangeMode())
            {
                int fromId = GetLedgerId(ultraComboVendorFrom);
                int toId = GetLedgerId(ultraComboVendorTo);
                if (fromId > 0 || toId > 0)
                {
                    int lowerBound = fromId > 0 && toId > 0 ? Math.Min(fromId, toId) : Math.Max(fromId, toId);
                    int upperBound = fromId > 0 && toId > 0 ? Math.Max(fromId, toId) : Math.Max(fromId, toId);
                    filteredRows = filteredRows.Where(x => x.AcctCode >= lowerBound && x.AcctCode <= upperBound);
                }
            }

            string searchText = GetSearchText();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredRows = filteredRows.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.Company) && x.Company.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Name) && x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Phone) && x.Phone.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.DocNo) && x.DocNo.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(x.Reference) && x.Reference.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    x.AcctCode.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!_getUnallocatedReturnsOnly)
            {
                filteredRows = filteredRows.Where(x => x.Balance != 0);
            }

            List<VendorOutstandingReportRow> boundRows = filteredRows
                .OrderBy(x => x.Company)
                .ThenBy(x => x.Date)
                .ThenBy(x => x.DocNo)
                .ToList();

            gridReport.DataSource = boundRows;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(boundRows);
        }

        private bool ValidatePaymentDueDateRange()
        {
            if (!chkPaymentDueOnly.Checked) return true;

            if (ultraDateTimeEditor1.Value == null || ultraDateTimeEditor2.Value == null)
            {
                MessageBox.Show("Please select both Payment Due From and To dates.", "Validation",
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
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(new List<VendorOutstandingReportRow>());
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
                band.Columns["DocAmt"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(gridReport.DataSource as IList<VendorOutstandingReportRow>);
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
        #endregion

        #region Button & Control Handlers
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

        public void RibbonClear() => Clear();

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
        #endregion
    }
}
