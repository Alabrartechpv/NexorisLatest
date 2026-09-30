using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports.FinancialReports;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsToolTip = System.Windows.Forms.ToolTip;

namespace PosBranch_Win.Reports.SalesReports
{
    public partial class frmSalesmanIncentiveReport : Form
    {
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

        private readonly SalesmanIncentiveReportRepository _reportRepository = new SalesmanIncentiveReportRepository();
        private readonly Dropdowns _dropdowns = new Dropdowns();
        private SalesmanIncentiveReportData _currentReport = new SalesmanIncentiveReportData();
        private List<SalesmanIncentiveDetail> _activeDetails = new List<SalesmanIncentiveDetail>();
        private bool _selectionHidden;
        private bool _gridLayoutsLoaded;
        private Form _columnChooserForm;
        private ListBox _columnChooserListBox;
        private UltraGrid _columnChooserTargetGrid;
        private UltraGridColumn _columnToMove;
        private Point _dragStartPoint;
        private bool _isDraggingColumn;
        private readonly Dictionary<string, int> _savedColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly WinFormsToolTip _columnToolTip = new WinFormsToolTip();
        private List<LookupItem> _salesmanLookupItems = new List<LookupItem>();
        private List<LookupItem> _userLookupItems = new List<LookupItem>();
        private List<LookupItem> _groupLookupItems = new List<LookupItem>();
        private List<LookupItem> _categoryLookupItems = new List<LookupItem>();
        private List<LookupItem> _brandLookupItems = new List<LookupItem>();
        private List<LookupItem> _vendorLookupItems = new List<LookupItem>();
        private int _selectedSalesmanId;
        private int _selectedUserId;
        private int _selectedGroupId;
        private int _selectedCategoryId;
        private int _selectedBrandId;
        private int _selectedVendorId;

        private const string SummaryGridLayoutFileName = "SalesmanIncentiveSummaryGridLayout.xml";
        private const string DetailGridLayoutFileName = "SalesmanIncentiveDetailGridLayout.xml";
        private string SummaryGridLayoutPath => Path.Combine(Application.StartupPath, SummaryGridLayoutFileName);
        private string DetailGridLayoutPath => Path.Combine(Application.StartupPath, DetailGridLayoutFileName);

        public frmSalesmanIncentiveReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Salesman Incentive Report";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            KeyDown += frmSalesmanIncentiveReport_KeyDown;
            FormClosing += frmSalesmanIncentiveReport_FormClosing;
            Load += frmSalesmanIncentiveReport_Load;

            InitializePanels();
            StyleButtons();
            StyleFilterControls();
            ConfigurePresetCombo();
            ConfigureGrids();
            LoadLookups();
            ApplyDefaultFilters();
        }

        private void InitializePanels()
        {
            BackColor = FormBackColor;
            pnlMain.Appearance.BackColor = FormBackColor;

            pnlSelection.Appearance.BackColor = FilterPanelBackColor;
            pnlSelection.Appearance.BorderColor = BorderBlue;
            pnlSelection.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;

            pnlToolbar.Appearance.BackColor = ActionPanelBackColor;
            pnlToolbar.Appearance.BorderColor = BorderBlue;
            pnlToolbar.BorderStyle = UIElementBorderStyle.Solid;

            pnlContent.Appearance.BackColor = FormBackColor;

            grpSummary.Appearance.BackColor = FormBackColor;
            grpSummary.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;
            grpSummary.HeaderAppearance.BackColor = GridHeaderBlueDark;
            grpSummary.HeaderAppearance.ForeColor = Color.White;
            grpSummary.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            grpDetails.Appearance.BackColor = FormBackColor;
            grpDetails.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;
            grpDetails.HeaderAppearance.BackColor = GridHeaderBlueDark;
            grpDetails.HeaderAppearance.ForeColor = Color.White;
            grpDetails.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            pnlFooter.Appearance.BackColor = GridHeaderBlue;
            pnlFooter.Appearance.BorderColor = GridFooterBorder;
            pnlFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabels();
        }

        private void StyleLabels()
        {
            StyleLabel(lblFromDate);
            StyleLabel(lblToDate);
            StyleLabel(lblDatePreset);
            StyleLabel(lblSalesman);
            StyleLabel(lblUser);
            StyleLabel(lblGroup);
            StyleLabel(lblCategory);
            StyleLabel(lblBrand);
            StyleLabel(lblVendor);
            StyleLabel(lblIncentivePercent);

            StyleFooterLabel(lblSummaryCount);
            StyleFooterLabel(lblDetailCount);
            StyleFooterLabel(lblNetProfitCaption);
            StyleFooterLabel(lblNetProfitValue);
            StyleFooterLabel(lblIncentiveCaption);
            StyleFooterLabel(lblIncentiveValue);
        }

        private static void StyleLabel(Label label)
        {
            if (label == null) return;
            label.BackColor = Color.Transparent;
            label.ForeColor = Color.FromArgb(18, 47, 95);
            label.Font = new Font("Tahoma", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private static void StyleFooterLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            if (label == null) return;
            label.Appearance.BackColor = Color.Transparent;
            label.Appearance.ForeColor = Color.White;
            label.Appearance.FontData.Bold = DefaultableBoolean.True;
            label.Appearance.FontData.Name = "Tahoma";
            label.Appearance.FontData.SizeInPoints = 9.5F;
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnHideSelection);

            StylePickerButton(btnLookupSalesman);
            StylePickerButton(btnLookupUser);
            StylePickerButton(btnLookupGroup);
            StylePickerButton(btnLookupCategory);
            StylePickerButton(btnLookupBrand);
            StylePickerButton(btnLookupVendor);
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
            StyleFilterCombo(cmbDatePreset);
            StyleDateEditor(dtFromDate);
            StyleDateEditor(dtToDate);
            StyleNumericEditor(numIncentivePercent);
            StyleTextBox(txtSalesman);
            StyleTextBox(txtUser);
            StyleTextBox(txtGroup);
            StyleTextBox(txtCategory);
            StyleTextBox(txtBrand);
            StyleTextBox(txtVendor);
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
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 9.5F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
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
            editor.Appearance.FontData.SizeInPoints = 9.5F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.MaskInput = "{date}";
            editor.FormatString = "dd/MM/yyyy";
        }

        private static void StyleNumericEditor(Infragistics.Win.UltraWinEditors.UltraNumericEditor editor)
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
        }

        private static void StyleTextBox(TextBox textBox)
        {
            if (textBox == null) return;
            textBox.BackColor = ControlBackColor;
            textBox.ForeColor = ControlTextColor;
            textBox.Font = new Font("Tahoma", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private void ConfigurePresetCombo()
        {
            cmbDatePreset.Items.Clear();
            cmbDatePreset.Items.Add("Today", "Today");
            cmbDatePreset.Items.Add("Yesterday", "Yesterday");
            cmbDatePreset.Items.Add("ThisWeek", "This Week");
            cmbDatePreset.Items.Add("LastWeek", "Last Week");
            cmbDatePreset.Items.Add("ThisMonth", "This Month");
            cmbDatePreset.Items.Add("LastMonth", "Last Month");
            cmbDatePreset.Items.Add("Custom", "Custom Range");
            cmbDatePreset.DropDownStyle = DropDownStyle.DropDownList;
            cmbDatePreset.Value = "ThisMonth";
        }

        private void ConfigureGrids()
        {
            ConfigureGrid(gridSummary);
            ConfigureGrid(gridDetails);
            gridSummary.InitializeLayout += gridSummary_InitializeLayout;
            gridDetails.InitializeLayout += gridDetails_InitializeLayout;
            gridSummary.AfterRowActivate += gridSummary_AfterRowActivate;
            SetupColumnChooserMenu(gridSummary);
            SetupColumnChooserMenu(gridDetails);
        }

        private static void ConfigureGrid(UltraGrid grid)
        {
            if (grid == null) return;
            grid.DisplayLayout.Reset();
            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
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
            layout.Override.AllowColMoving = AllowColMoving.WithinBand;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 25;
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
            layout.Override.MinRowHeight = 20;
            layout.Override.DefaultRowHeight = 20;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            grid.BackColor = FormBackColor;
            grid.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void LoadLookups()
        {
            _salesmanLookupItems = BuildLookupItems(_dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName);
            _userLookupItems = BuildLookupItems(_dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName);
            _groupLookupItems = BuildLookupItems(_dropdowns.getGroupDDl().List, x => x.Id, x => x.GroupName);
            _categoryLookupItems = BuildLookupItems(_dropdowns.getCategoryDDl(string.Empty).List, x => x.Id, x => x.CategoryName);
            _brandLookupItems = BuildLookupItems(_dropdowns.getBrandDDL().List, x => x.Id, x => x.BrandName);
            _vendorLookupItems = BuildLookupItems(_dropdowns.VendorDDL().List, x => x.LedgerID, x => x.LedgerName);

            ApplyLookupSelection(txtSalesman, _salesmanLookupItems, ref _selectedSalesmanId);
            ApplyLookupSelection(txtUser, _userLookupItems, ref _selectedUserId);
            ApplyLookupSelection(txtGroup, _groupLookupItems, ref _selectedGroupId);
            ApplyLookupSelection(txtCategory, _categoryLookupItems, ref _selectedCategoryId);
            ApplyLookupSelection(txtBrand, _brandLookupItems, ref _selectedBrandId);
            ApplyLookupSelection(txtVendor, _vendorLookupItems, ref _selectedVendorId);
        }

        private static void ApplyLookupSelection(TextBox textBox, List<LookupItem> items, ref int selectedId)
        {
            if (items == null) items = new List<LookupItem>();

            int currentSelectedId = selectedId;
            if (!items.Any(x => x.Id == currentSelectedId))
            {
                selectedId = 0;
                currentSelectedId = 0;
            }

            LookupItem selected = items.FirstOrDefault(x => x.Id == currentSelectedId) ?? items.FirstOrDefault(x => x.Id == 0);
            if (textBox != null)
            {
                textBox.Text = selected != null ? selected.Name : "All";
            }
        }

        private void btnLookupSalesman_Click(object sender, EventArgs e)
        {
            string selectedSalesmanName = null;
            using (frmSalesPersonDial dialog = new frmSalesPersonDial())
            {
                dialog.OnSalesPersonSelected += name => selectedSalesmanName = name;
                if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(selectedSalesmanName))
                {
                    RefreshLookup(ref _salesmanLookupItems, _dropdowns.getUsersDDl().List, x => x.UserID, x => x.UserName);
                    LookupItem match = _salesmanLookupItems.FirstOrDefault(x => string.Equals(x.Name, selectedSalesmanName, StringComparison.OrdinalIgnoreCase));
                    _selectedSalesmanId = match != null ? match.Id : 0;
                    ApplyLookupSelection(txtSalesman, _salesmanLookupItems, ref _selectedSalesmanId);
                }
            }
        }

        private void btnLookupUser_Click(object sender, EventArgs e)
        {
            SelectLookupValue("Select User", txtUser, _userLookupItems, ref _selectedUserId);
        }

        private void btnLookupGroup_Click(object sender, EventArgs e)
        {
            RefreshLookup(ref _groupLookupItems, _dropdowns.getGroupDDl().List, x => x.Id, x => x.GroupName);
            using (frmGroupDialog dialog = new frmGroupDialog())
            {
                dialog.TargetTextBox = txtGroup;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedGroupId = dialog.SelectedGroupId;
                    ApplyLookupSelection(txtGroup, _groupLookupItems, ref _selectedGroupId);
                }
            }
        }

        private void btnLookupCategory_Click(object sender, EventArgs e)
        {
            RefreshLookup(ref _categoryLookupItems, _dropdowns.getCategoryDDl(string.Empty).List, x => x.Id, x => x.CategoryName);
            using (frmCategoryDialog dialog = new frmCategoryDialog("frmSalesmanIncentiveReport"))
            {
                dialog.TargetTextBox = txtCategory;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedCategoryId = dialog.SelectedCategoryId;
                    ApplyLookupSelection(txtCategory, _categoryLookupItems, ref _selectedCategoryId);
                }
            }
        }

        private void btnLookupBrand_Click(object sender, EventArgs e)
        {
            RefreshLookup(ref _brandLookupItems, _dropdowns.getBrandDDL().List, x => x.Id, x => x.BrandName);
            using (frmBrandDialog dialog = new frmBrandDialog())
            {
                dialog.TargetTextBox = txtBrand;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedBrandId = dialog.SelectedBrandId;
                    ApplyLookupSelection(txtBrand, _brandLookupItems, ref _selectedBrandId);
                }
            }
        }

        private void btnLookupVendor_Click(object sender, EventArgs e)
        {
            using (frmVendorDig dialog = new frmVendorDig())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshLookup(ref _vendorLookupItems, _dropdowns.VendorDDL().List, x => x.LedgerID, x => x.LedgerName);
                    _selectedVendorId = dialog.SelectedVendorId;
                    ApplyLookupSelection(txtVendor, _vendorLookupItems, ref _selectedVendorId);
                }
            }
        }

        private static void RefreshLookup<T>(ref List<LookupItem> target, IEnumerable<T> source, Func<T, int> idSelector, Func<T, string> nameSelector)
        {
            target = BuildLookupItems(source, idSelector, nameSelector);
        }

        private void SelectLookupValue(string title, TextBox textBox, List<LookupItem> items, ref int selectedId)
        {
            using (LookupPickerDialog dialog = new LookupPickerDialog(title, items, selectedId))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedId = dialog.SelectedId;
                    ApplyLookupSelection(textBox, items, ref selectedId);
                }
            }
        }

        private static List<LookupItem> BuildLookupItems<T>(IEnumerable<T> source, Func<T, int> idSelector, Func<T, string> nameSelector)
        {
            List<LookupItem> items = new List<LookupItem> { new LookupItem { Id = 0, Name = "All" } };
            if (source != null)
            {
                items.AddRange(source.Select(item => new LookupItem
                {
                    Id = idSelector(item),
                    Name = string.IsNullOrWhiteSpace(nameSelector(item)) ? "(Blank)" : nameSelector(item)
                }));
            }

            return items.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Id == 0 ? -1 : 0).ThenBy(x => x.Name).ToList();
        }

        public void RibbonClear()
        {
            _selectedSalesmanId = 0;
            _selectedUserId = 0;
            _selectedGroupId = 0;
            _selectedCategoryId = 0;
            _selectedBrandId = 0;
            _selectedVendorId = 0;
            txtSalesman.Text = "All";
            txtUser.Text = "All";
            txtGroup.Text = "All";
            txtCategory.Text = "All";
            txtBrand.Text = "All";
            txtVendor.Text = "All";
            if (cmbDatePreset != null && cmbDatePreset.Items.Count > 0) cmbDatePreset.SelectedIndex = 0;
            gridSummary.DataSource = null;
            gridDetails.DataSource = null;
            UpdateFooter(new List<SalesmanIncentiveSummary>(), new List<SalesmanIncentiveDetail>());
        }

        public void Clear() => RibbonClear();

        private void ApplyDefaultFilters()
        {
            dtFromDate.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtToDate.Value = DateTime.Today;
            numIncentivePercent.Value = 5M;
        }

        private void LoadReport()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                SalesmanIncentiveReportFilter filter = new SalesmanIncentiveReportFilter
                {
                    FromDate = dtFromDate.DateTime.Date,
                    ToDate = dtToDate.DateTime.Date,
                    CompanyId = SessionContext.CompanyId > 0 ? SessionContext.CompanyId : Convert.ToInt32(DataBase.CompanyId),
                    BranchId = SessionContext.BranchId > 0 ? SessionContext.BranchId : Convert.ToInt32(DataBase.BranchId),
                    SalesmanId = _selectedSalesmanId,
                    UserId = _selectedUserId,
                    GroupId = _selectedGroupId,
                    CategoryId = _selectedCategoryId,
                    BrandId = _selectedBrandId,
                    VendorId = _selectedVendorId,
                    IncentivePercent = Convert.ToDecimal(numIncentivePercent.Value ?? 0M),
                    IncludeDetails = true
                };

                _currentReport = _reportRepository.GetSalesmanIncentiveReport(filter) ?? new SalesmanIncentiveReportData();
                gridSummary.DataSource = _currentReport.Summary;
                _activeDetails = _currentReport.Details ?? new List<SalesmanIncentiveDetail>();
                gridDetails.DataSource = _activeDetails;
                LoadGridLayouts();
                UpdateFooter(_currentReport.Summary, _activeDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load salesman incentive report.\n" + ex.Message, "Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateFooter(IEnumerable<SalesmanIncentiveSummary> summary, IEnumerable<SalesmanIncentiveDetail> details)
        {
            List<SalesmanIncentiveSummary> summaryList = summary == null ? new List<SalesmanIncentiveSummary>() : summary.ToList();
            List<SalesmanIncentiveDetail> detailList = details == null ? new List<SalesmanIncentiveDetail>() : details.ToList();
            lblSummaryCount.Text = "Summary Rows: " + summaryList.Count.ToString("N0");
            lblDetailCount.Text = "Detail Rows: " + detailList.Count.ToString("N0");
            lblNetProfitValue.Text = summaryList.Sum(x => x.NetProfit).ToString("N3");
            lblIncentiveValue.Text = summaryList.Sum(x => x.IncentiveAmount).ToString("N3");
        }

        private void FilterDetailsForActiveSummary()
        {
            if (gridSummary.ActiveRow == null || _currentReport == null || _currentReport.Details == null)
            {
                gridDetails.DataSource = _activeDetails;
                return;
            }

            int salesmanId = Convert.ToInt32(gridSummary.ActiveRow.Cells["SalesmanId"].Value);
            int branchId = Convert.ToInt32(gridSummary.ActiveRow.Cells["BranchId"].Value);
            List<SalesmanIncentiveDetail> filtered = _currentReport.Details.Where(x => x.SalesmanId == salesmanId && x.BranchId == branchId).ToList();
            gridDetails.DataSource = filtered;
            UpdateFooter(_currentReport.Summary, filtered);
        }

        private void ApplyDatePreset(string preset)
        {
            DateTime today = DateTime.Today;
            DateTime from;
            DateTime to;

            switch (preset)
            {
                case "Today":
                    from = today;
                    to = today;
                    break;
                case "Yesterday":
                    from = today.AddDays(-1);
                    to = today.AddDays(-1);
                    break;
                case "ThisWeek":
                    int diff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)today.DayOfWeek - 1);
                    from = today.AddDays(-diff);
                    to = today;
                    break;
                case "LastWeek":
                    int currentDiff = today.DayOfWeek == DayOfWeek.Sunday ? 6 : ((int)today.DayOfWeek - 1);
                    to = today.AddDays(-currentDiff - 1);
                    from = to.AddDays(-6);
                    break;
                case "LastMonth":
                    DateTime firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
                    from = firstOfThisMonth.AddMonths(-1);
                    to = firstOfThisMonth.AddDays(-1);
                    break;
                case "Custom":
                    return;
                default:
                    from = new DateTime(today.Year, today.Month, 1);
                    to = today;
                    break;
            }

            dtFromDate.Value = from;
            dtToDate.Value = to;
        }

        private void gridSummary_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            UltraGridBand band = e.Layout.Bands[0];

            if (band.Columns.Exists("SalesmanId")) band.Columns["SalesmanId"].Hidden = true;
            if (band.Columns.Exists("BranchId")) band.Columns["BranchId"].Hidden = true;

            ConfigureNumberColumn(band, "SalesQty", "Sales Qty");
            ConfigureNumberColumn(band, "IncentivePercent", "Incentive %");
            ConfigureNumberColumn(band, "SalesAmount", "Sales Amount");
            ConfigureNumberColumn(band, "GrossProfit", "Gross Profit");
            ConfigureNumberColumn(band, "SalesReturnLoss", "Return Loss");
            ConfigureNumberColumn(band, "NetProfit", "Net Profit");
            ConfigureNumberColumn(band, "IncentiveAmount", "Incentive Amount");

            if (band.Columns.Exists("NetProfit"))
            {
                band.Columns["NetProfit"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["NetProfit"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            if (band.Columns.Exists("IncentiveAmount"))
            {
                band.Columns["IncentiveAmount"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["IncentiveAmount"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void gridDetails_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;
            UltraGridBand band = e.Layout.Bands[0];

            if (band.Columns.Exists("SalesmanId")) band.Columns["SalesmanId"].Hidden = true;
            if (band.Columns.Exists("BranchId")) band.Columns["BranchId"].Hidden = true;
            if (band.Columns.Exists("ItemId")) band.Columns["ItemId"].Hidden = true;

            ConfigureNumberColumn(band, "Qty", "Qty");
            ConfigureNumberColumn(band, "CostPerUnit", "Cost/Unit");
            ConfigureNumberColumn(band, "SalesPricePerUnit", "Price/Unit");
            ConfigureNumberColumn(band, "SalesValue", "Sales Value");
            ConfigureNumberColumn(band, "CostValue", "Cost Value");
            ConfigureNumberColumn(band, "ProfitValue", "Profit");
            if (band.Columns.Exists("TransactionDate"))
            {
                band.Columns["TransactionDate"].Format = "dd-MM-yyyy HH:mm";
                band.Columns["TransactionDate"].Width = 125;
            }

            if (band.Columns.Exists("ProfitValue"))
            {
                band.Columns["ProfitValue"].CellAppearance.FontData.Bold = DefaultableBoolean.True;
                band.Columns["ProfitValue"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private static void ConfigureNumberColumn(UltraGridBand band, string key, string caption)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
            col.Header.Caption = caption;
            col.Format = "#,##0.000";
            col.CellAppearance.TextHAlign = HAlign.Right;
            col.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            col.CellAppearance.FontData.SizeInPoints = 8.25F;
            col.Width = 105;
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
            LoadReport();
            ShowReportFormatDialog(
                "SALESMAN INCENTIVE REPORT",
                new[]
                {
                    "SALESMAN INCENTIVE REPORT - SUMMARY",
                    "SALESMAN INCENTIVE REPORT - DETAILED BY SALESMAN",
                    "SALESMAN INCENTIVE REPORT - BRANCHWISE SUMMARY"
                });
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            _selectionHidden = !_selectionHidden;
            pnlSelection.Visible = !_selectionHidden;
            btnHideSelection.Text = _selectionHidden ? "View Selection" : "Hide Selection";
        }

        private void ExportCsv()
        {
            List<SalesmanIncentiveSummary> rows = gridSummary.DataSource as List<SalesmanIncentiveSummary>;
            if (rows == null || rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = string.Format("SalesmanIncentiveReport_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("BranchName,SalesmanName,SalesQty,SalesAmount,GrossProfit,ReturnLoss,NetProfit,IncentivePercent,IncentiveAmount");

                foreach (SalesmanIncentiveSummary row in rows)
                {
                    builder.AppendLine(string.Join(",",
                        EscapeCsv(row.BranchName),
                        EscapeCsv(row.SalesmanName),
                        row.SalesQty.ToString("F3"),
                        row.SalesAmount.ToString("F3"),
                        row.GrossProfit.ToString("F3"),
                        row.SalesReturnLoss.ToString("F3"),
                        row.NetProfit.ToString("F3"),
                        row.IncentivePercent.ToString("F2"),
                        row.IncentiveAmount.ToString("F3")));
                }

                File.WriteAllText(dialog.FileName, builder.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (!safeValue.Contains(",") && !safeValue.Contains("\"") && !safeValue.Contains("\n"))
                return safeValue;
            return string.Format("\"{0}\"", safeValue.Replace("\"", "\"\""));
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
            List<SalesmanIncentiveSummary> rows = gridSummary.DataSource as List<SalesmanIncentiveSummary>;
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
                preview.Text = "Salesman Incentive Report - Preview";
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
                    Text = "SALESMAN INCENTIVE REPORT",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Period: {0:dd-MMM-yyyy} to {1:dd-MMM-yyyy}    |    Salesman: {2}    |    Incentive %: {3}",
                        dtFromDate.DateTime.Date, dtToDate.DateTime.Date, txtSalesman.Text, numIncentivePercent.Value),
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
                ConfigureGrid(previewGrid);
                previewGrid.DataSource = rows.ToList();

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = GridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                decimal netProfitTotal = rows.Sum(x => x.NetProfit);
                decimal incentiveTotal = rows.Sum(x => x.IncentiveAmount);
                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Salesmen: {0:N0}    |    Net Profit Total: {1:N3}    |    Incentive Total: {2:N3}",
                        rows.Count, netProfitTotal, incentiveTotal),
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

        private void cmbDatePreset_ValueChanged(object sender, EventArgs e)
        {
            if (cmbDatePreset.Value != null)
            {
                ApplyDatePreset(cmbDatePreset.Value.ToString());
            }
        }

        private void gridSummary_AfterRowActivate(object sender, EventArgs e)
        {
            FilterDetailsForActiveSummary();
        }

        private void frmSalesmanIncentiveReport_Load(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void frmSalesmanIncentiveReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                btnViewGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.E)
            {
                btnExportGrid.PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void LoadGridLayouts()
        {
            if (_gridLayoutsLoaded) return;
            TryLoadGridLayout(gridSummary, SummaryGridLayoutPath);
            TryLoadGridLayout(gridDetails, DetailGridLayoutPath);
            _gridLayoutsLoaded = true;
        }

        private void SaveGridLayouts()
        {
            TrySaveGridLayout(gridSummary, SummaryGridLayoutPath);
            TrySaveGridLayout(gridDetails, DetailGridLayoutPath);
        }

        private static void TryLoadGridLayout(UltraGrid grid, string layoutPath)
        {
            try
            {
                if (grid != null && File.Exists(layoutPath))
                {
                    grid.DisplayLayout.LoadFromXml(layoutPath);
                }
            }
            catch
            {
            }
        }

        private static void TrySaveGridLayout(UltraGrid grid, string layoutPath)
        {
            try
            {
                if (grid != null)
                {
                    grid.DisplayLayout.SaveAsXml(layoutPath);
                }
            }
            catch
            {
            }
        }

        private void frmSalesmanIncentiveReport_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayouts();
        }

        private void SetupColumnChooserMenu(UltraGrid grid)
        {
            if (grid == null) return;

            ContextMenuStrip gridContextMenu = new ContextMenuStrip();
            ToolStripMenuItem columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += (s, e) => ShowColumnChooser(grid);
            gridContextMenu.Items.Add(columnChooserMenuItem);
            grid.ContextMenuStrip = gridContextMenu;

            grid.AllowDrop = true;
            grid.MouseDown += Grid_MouseDown;
            grid.MouseMove += Grid_MouseMove;
            grid.MouseUp += Grid_MouseUp;
            grid.DragOver += Grid_DragOver;
            grid.DragDrop += Grid_DragDrop;
        }

        private void CreateColumnChooserForm()
        {
            _columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(240, 240, 240),
                ShowIcon = false,
                ShowInTaskbar = false
            };

            _columnChooserForm.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                _columnChooserForm.Hide();
            };
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

            _columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
            _columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            _columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            _columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;

            _columnChooserForm.Controls.Add(_columnChooserListBox);
        }

        private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || _columnChooserListBox == null) return;

            ColumnItem item = _columnChooserListBox.Items[e.Index] as ColumnItem;
            if (item == null) return;

            Rectangle rect = e.Bounds;
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
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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

        private void ShowColumnChooser(UltraGrid grid)
        {
            if (grid == null) return;

            _columnChooserTargetGrid = grid;
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed || _columnChooserListBox == null)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox(grid);
            _columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void PopulateColumnChooserListBox(UltraGrid grid)
        {
            if (_columnChooserListBox == null || grid == null) return;

            _columnChooserListBox.Items.Clear();
            if (grid.DisplayLayout.Bands.Count <= 0) return;

            foreach (UltraGridColumn col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden)
                {
                    string displayText = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                    _columnChooserListBox.Items.Add(new ColumnItem(col.Key, displayText, grid.Name));
                }
            }
        }

        private sealed class ColumnItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }
            public string GridName { get; }

            public ColumnItem(string key, string text, string gridName)
            {
                ColumnKey = key;
                DisplayText = text;
                GridName = gridName;
            }

            public override string ToString() => DisplayText;
        }

        private void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            _isDraggingColumn = false;
            _columnToMove = null;
            _columnChooserTargetGrid = grid;
            _dragStartPoint = new Point(e.X, e.Y);

            if (grid == null || e.Y >= 40 || grid.DisplayLayout.Bands.Count <= 0) return;

            int xPos = 0;
            if (grid.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True)
            {
                xPos += grid.DisplayLayout.Override.RowSelectorWidth;
            }

            foreach (UltraGridColumn col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                {
                    if (e.X >= xPos && e.X < xPos + col.Width)
                    {
                        _columnToMove = col;
                        _isDraggingColumn = true;
                        break;
                    }
                    xPos += col.Width;
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            if (!_isDraggingColumn || _columnToMove == null || grid == null || e.Button != MouseButtons.Left) return;

            int deltaX = Math.Abs(e.X - _dragStartPoint.X);
            int deltaY = Math.Abs(e.Y - _dragStartPoint.Y);
            if (deltaX <= SystemInformation.DragSize.Width && deltaY <= SystemInformation.DragSize.Height) return;

            bool isDraggingDown = e.Y > _dragStartPoint.Y && deltaY > deltaX;
            if (!isDraggingDown) return;

            grid.Cursor = Cursors.No;
            string columnName = !string.IsNullOrEmpty(_columnToMove.Header.Caption) ? _columnToMove.Header.Caption : _columnToMove.Key;
            _columnToolTip.SetToolTip(grid, $"Drag down to hide '{columnName}' column");

            if (e.Y - _dragStartPoint.Y > 50)
            {
                HideColumn(grid, _columnToMove);
                _columnToMove = null;
                _isDraggingColumn = false;
                grid.Cursor = Cursors.Default;
                _columnToolTip.SetToolTip(grid, string.Empty);
            }
        }

        private void Grid_MouseUp(object sender, MouseEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            if (grid != null)
            {
                grid.Cursor = Cursors.Default;
                _columnToolTip.SetToolTip(grid, string.Empty);
            }

            _isDraggingColumn = false;
            _columnToMove = null;
        }

        private void HideColumn(UltraGrid grid, UltraGridColumn column)
        {
            if (grid == null || column == null || column.Hidden) return;

            if (GetEssentialColumns(grid).Contains(column.Key, StringComparer.OrdinalIgnoreCase))
            {
                MessageBox.Show($"The '{column.Header.Caption}' column is essential and cannot be hidden.", "Cannot Hide Column", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _savedColumnWidths[grid.Name + "|" + column.Key] = column.Width;
            grid.SuspendLayout();
            column.Hidden = true;
            foreach (UltraGridColumn col in grid.DisplayLayout.Bands[0].Columns)
            {
                string widthKey = grid.Name + "|" + col.Key;
                if (!col.Hidden && _savedColumnWidths.ContainsKey(widthKey))
                {
                    col.Width = _savedColumnWidths[widthKey];
                }
            }
            grid.ResumeLayout();

            if (_columnChooserForm == null || _columnChooserForm.IsDisposed || _columnChooserListBox == null)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox(grid);
            if (_columnChooserTargetGrid == grid && _columnChooserForm != null && _columnChooserForm.Visible)
            {
                PositionColumnChooserAtBottomRight();
            }
        }

        private static IEnumerable<string> GetEssentialColumns(UltraGrid grid)
        {
            if (grid == null) return Enumerable.Empty<string>();

            if (grid.Name == "gridSummary")
            {
                return new[] { "BranchName", "SalesmanName", "NetProfit", "IncentiveAmount" };
            }

            return new[] { "TransactionDate", "ItemName", "ProfitValue" };
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (_columnChooserListBox == null) return;

            int index = _columnChooserListBox.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches) return;

            ColumnItem item = _columnChooserListBox.Items[index] as ColumnItem;
            if (item != null)
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
            UltraGridColumn column = e.Data.GetData(typeof(UltraGridColumn)) as UltraGridColumn;
            if (column == null || column.Hidden) return;

            UltraGrid grid = column.Band != null && column.Band.Layout != null ? column.Band.Layout.Grid as UltraGrid : null;
            if (grid == null) return;

            column.Hidden = true;
            PopulateColumnChooserListBox(grid);
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(ColumnItem)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void Grid_DragDrop(object sender, DragEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            ColumnItem item = e.Data.GetData(typeof(ColumnItem)) as ColumnItem;
            if (grid == null || item == null || !string.Equals(item.GridName, grid.Name, StringComparison.OrdinalIgnoreCase)) return;

            if (grid.DisplayLayout.Bands.Count > 0 && grid.DisplayLayout.Bands[0].Columns.Exists(item.ColumnKey))
            {
                UltraGridColumn column = grid.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                column.Hidden = false;
                string widthKey = grid.Name + "|" + item.ColumnKey;
                if (_savedColumnWidths.ContainsKey(widthKey))
                {
                    column.Width = _savedColumnWidths[widthKey];
                }
                PopulateColumnChooserListBox(grid);
                _columnToolTip.Show($"'{item.DisplayText}' restored", grid, grid.PointToClient(MousePosition), 1500);
            }
        }

        private sealed class LookupItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private sealed class LookupPickerDialog : Form
        {
            private readonly List<LookupItem> _allItems;
            private readonly TextBox _txtSearch;
            private readonly ListBox _lstItems;
            private readonly Button _btnOk;
            private readonly Button _btnCancel;

            public int SelectedId { get; private set; }

            public LookupPickerDialog(string title, List<LookupItem> items, int selectedId)
            {
                _allItems = items ?? new List<LookupItem>();
                SelectedId = selectedId;

                Text = title;
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;
                ClientSize = new Size(420, 480);

                Label lblSearch = new Label
                {
                    AutoSize = true,
                    Location = new Point(12, 15),
                    Text = "Search"
                };

                _txtSearch = new TextBox
                {
                    Location = new Point(68, 12),
                    Size = new Size(340, 20)
                };
                _txtSearch.TextChanged += (s, e) => BindItems(_txtSearch.Text);

                _lstItems = new ListBox
                {
                    Location = new Point(12, 42),
                    Size = new Size(396, 386),
                    DisplayMember = "Name"
                };
                _lstItems.DoubleClick += (s, e) => ConfirmSelection();

                _btnOk = new Button
                {
                    Location = new Point(252, 440),
                    Size = new Size(75, 27),
                    Text = "OK"
                };
                _btnOk.Click += (s, e) => ConfirmSelection();

                _btnCancel = new Button
                {
                    Location = new Point(333, 440),
                    Size = new Size(75, 27),
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel
                };

                Controls.Add(lblSearch);
                Controls.Add(_txtSearch);
                Controls.Add(_lstItems);
                Controls.Add(_btnOk);
                Controls.Add(_btnCancel);

                AcceptButton = _btnOk;
                CancelButton = _btnCancel;

                BindItems(string.Empty);
                SelectCurrentItem();
            }

            private void BindItems(string searchText)
            {
                IEnumerable<LookupItem> filtered = _allItems;
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered.Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                                                   x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                _lstItems.DataSource = filtered.ToList();
                _lstItems.DisplayMember = "Name";
                SelectCurrentItem();
            }

            private void SelectCurrentItem()
            {
                if (_lstItems.Items.Count == 0) return;

                for (int i = 0; i < _lstItems.Items.Count; i++)
                {
                    LookupItem item = _lstItems.Items[i] as LookupItem;
                    if (item != null && item.Id == SelectedId)
                    {
                        _lstItems.SelectedIndex = i;
                        return;
                    }
                }

                _lstItems.SelectedIndex = 0;
            }

            private void ConfirmSelection()
            {
                LookupItem selected = _lstItems.SelectedItem as LookupItem;
                if (selected == null) return;

                SelectedId = selected.Id;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
