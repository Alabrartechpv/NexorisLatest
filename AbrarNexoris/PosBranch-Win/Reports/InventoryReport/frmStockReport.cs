using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using PosBranch_Win.Reports.FinancialReports;
using Repository;
using Repository.MasterRepositry;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmStockReport : Form
    {
        // ─── Colour Palette (matches frmVendorOutstandingReport theme) ──────────────
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
        private static readonly Color ButtonLightOutline   = Color.FromArgb(166, 183, 202);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        // ─── State ───────────────────────────────────────────────────────────────────
        private readonly StockReportAdvanceRepo _repo;
        private readonly Dropdowns _dropdowns;
        private readonly ItemMasterRepository _itemRepository;
        private List<StockReportItem> _reportRows = new List<StockReportItem>();
        private readonly Dictionary<string, Label>  _footerLabels      = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool _isLoading;

        // ─── Secondary Filter Buttons (controls 8-15 are declared in Designer.cs) ────
        // ─── Constructor ─────────────────────────────────────────────────────────────
        public frmStockReport()
        {
            _repo      = new StockReportAdvanceRepo();
            _dropdowns = new Dropdowns();
            _itemRepository = new ItemMasterRepository();

            InitializeComponent();

            Load                       += frmStockReport_Load;
            btnViewGrid.Click          += btnViewGrid_Click;
            btnPreviewGrid.Click       += btnPreviewGrid_Click;
            btnPreviewReport.Click     += btnPreviewReport_Click;
            btnExportGrid.Click        += btnExportGrid_Click;
            btnToggleSelection.Click   += btnToggleSelection_Click;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.InitializeRow    += gridReport_InitializeRow;
            gridReport.Resize           += gridReport_Resize;

            KeyPreview = true;
            KeyDown    += frmStockReport_KeyDown;
        }

        // ─── Load ─────────────────────────────────────────────────────────────────────
        private void frmStockReport_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            _isLoading = true;
            try
            {
                Text        = "Stock Report";
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;

                InitializeFilterControls();
                InitializePanels();
                StyleButtons();
                StyleFilterControls();
                SetupGrid();
                LoadFilterDropdowns();
                InitializeGridFooter();
                ResetReportView();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private bool _groupsLoaded = false;
        private bool _categoriesLoaded = false;
        private bool _brandsLoaded = false;
        private bool _itemTypesLoaded = false;

        private void InitializeFilterControls()
        {
            // Hide legacy Group and Category and Brand/Model controls from panel
            ultraComboGroup.Visible = false;
            lblGroup.Visible = false;
            ultraComboCategory.Visible = false;
            lblCategory.Visible = false;
            
            ultraComboEditor1.Visible = false; // legacy Model combo
            ultraLabel1.Visible = false; // legacy Model label
            ultraComboEditor2.Visible = false; // legacy Brand combo
            ultraLabel2.Visible = false; // legacy Brand label

            // Configure Mode Combos
            SetupModeCombo(ultraComboEditor5);
            SetupModeCombo(ultraComboEditor6);
            SetupModeCombo(ultraComboEditor4);

            // Create Dynamic Combo & Buttons & Labels
            CreateDynamicControls();

            // Set Up Mode Value Changed Handlers
            ultraComboEditor5.ValueChanged += (s, e) => ToggleGroupFilters();
            ultraComboEditor6.ValueChanged += (s, e) => ToggleCategoryFilters();
            ultraComboEditor4.ValueChanged += (s, e) => ToggleBrandFilters();

            // Set Up BeforeDropDown events for lazy loading
            ultraComboEditor8.BeforeDropDown += (s, e) => LazyLoadGroups();
            ultraComboEditor3.BeforeDropDown += (s, e) => LazyLoadGroups();
            ultraComboEditor13.BeforeDropDown += (s, e) => LazyLoadGroups();

            ultraComboEditor9.BeforeDropDown += (s, e) => LazyLoadCategories();
            ultraComboEditor10.BeforeDropDown += (s, e) => LazyLoadCategories();
            ultraComboEditor11.BeforeDropDown += (s, e) => LazyLoadBrands();

            ultraComboEditor12.BeforeDropDown += (s, e) => LazyLoadBrands();
            ultraComboEditor14.BeforeDropDown += (s, e) => LazyLoadCategories();
            ultraComboEditor15.BeforeDropDown += (s, e) => LazyLoadBrands();

            ultraComboEditor7.BeforeDropDown += (s, e) => LazyLoadItemTypes();

            // Toggle visibility to default ("ALL")
            ToggleGroupFilters();
            ToggleCategoryFilters();
            ToggleBrandFilters();
            
            // Set up Item Type dropdown default value to 0 and pre-populate placeholder "-- All Item Types --"
            ultraComboEditor7.Items.Clear();
            ultraComboEditor7.Items.Add(0, "-- All Item Types --");
            ultraComboEditor7.Value = 0;

        }

        private void SetupModeCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            combo.Items.Clear();
            combo.Items.Add("ALL", "ALL");
            combo.Items.Add("By Range", "By Range");
            combo.Items.Add("Filter by Selection", "Filter by Selection");
            combo.Value = "ALL";
            StyleFilterCombo(combo, true);
        }

        private void CreateDynamicControls()
        {
            // Layout reference:
            //   Mode combo:  X=95, W=150 → ends at 245
            //   Range-start: X=252, W=150 → ends at 402  |  btn at X=404, W=25
            //   Range-end:   X=430, W=150 → ends at 580  |  btn at X=582, W=25
            //   Selection:   X=252, W=310 → ends at 562  |  btn at X=564, W=25

            // ── Group row (Y=17) ───────────────────────────────────────────────
            // [Mode] [ultraComboEditor8(252)] [btn1] [ultraComboEditor3(430)] [btn2]  (By Range)
            // [Mode] [ultraComboEditor13(252,W=310)] [btn7]                           (Selection)
            StyleClassicButton(button1);
            StyleClassicButton(button2);
            StyleClassicButton(button7);

            // ── Category row (Y=47) ──────────────────────────────────────────────
            // [Mode] [ultraComboEditor9(252)] [btn3] [ultraComboEditor10(430)] [btn4]  (By Range)
            // [Mode] [ultraComboEditor14(252,W=310)] [btn8]                            (Selection)
            StyleClassicButton(button3);
            StyleClassicButton(button4);
            StyleClassicButton(button8);

            // ── Brand row (Y=84) ───────────────────────────────────────────────
            // [Mode] [ultraComboEditor12(252)] [btn5] [ultraComboEditor11(430)] [btn6]  (By Range)
            // [Mode] [ultraComboEditor15(252,W=310)] [btn9]                             (Selection)
            StyleClassicButton(button5);
            StyleClassicButton(button6);
            StyleClassicButton(button9);
            StyleClassicButton(btnBarcodeSearch);

            // ── Click handlers ───────────────────────────────────────────────
            button1.Click += (s, e) => OpenGroupDialogForCombo(ultraComboEditor8);
            button2.Click += (s, e) => OpenGroupDialogForCombo(ultraComboEditor3);
            button7.Click += (s, e) => OpenGroupDialogForCombo(ultraComboEditor13);

            button3.Click += (s, e) => OpenCategoryDialogForCombo(ultraComboEditor9);
            button4.Click += (s, e) => OpenItemCategoryDialogForCombo();
            button8.Click += (s, e) => OpenItemCategoryDialogForCombo();

            button5.Click += (s, e) => OpenBrandDialogForCombo(ultraComboEditor12);
            button6.Click += (s, e) => OpenBrandDialogForCombo(ultraComboEditor11);
            button9.Click += (s, e) => OpenBrandDialogForCombo(ultraComboEditor15);
            btnBarcodeSearch.Click += (s, e) => OpenItemDialogForBarcodeSearch();
            txtBarcodeSearch.KeyDown += txtBarcodeSearch_KeyDown;
        }

        private Infragistics.Win.Misc.UltraLabel CreateLabel(string text, Point location, Size size)
        {
            var lbl = new Infragistics.Win.Misc.UltraLabel();
            lbl.Text = text;
            lbl.Location = location;
            lbl.Size = size;
            ultraPanel1.ClientArea.Controls.Add(lbl);
            StyleLabel(lbl);
            return lbl;
        }

        private Infragistics.Win.UltraWinEditors.UltraComboEditor CreateCombo(Point location, Size size)
        {
            var combo = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            combo.Location = location;
            combo.Size = size;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            ultraPanel1.ClientArea.Controls.Add(combo);
            StyleFilterCombo(combo, true);
            return combo;
        }

        private Infragistics.Win.Misc.UltraButton CreateButton(string text, Point location, Size size)
        {
            var btn = new Infragistics.Win.Misc.UltraButton();
            btn.Text = text;
            btn.Location = location;
            btn.Size = size;
            ultraPanel1.ClientArea.Controls.Add(btn);
            StyleClassicButton(btn);
            return btn;
        }

        private void ToggleGroupFilters()
        {
            string mode      = ultraComboEditor5.Value?.ToString() ?? "ALL";
            bool isRange     = mode == "By Range";
            bool isSelection = mode == "Filter by Selection";

            // By Range: [Mode][combo8][btn1][combo3][btn2]
            if (ultraComboEditor8 != null) ultraComboEditor8.Visible = isRange;
            if (button1           != null) button1.Visible           = isRange;
            if (ultraComboEditor3 != null) ultraComboEditor3.Visible = isRange;
            if (button2           != null) button2.Visible           = isRange;

            // Selection: [Mode][combo13][btn7]
            if (ultraComboEditor13 != null) ultraComboEditor13.Visible = isSelection;
            if (button7            != null) button7.Visible            = isSelection;

            if (isRange || isSelection) LazyLoadGroups();
        }

        private void ToggleCategoryFilters()
        {
            string mode      = ultraComboEditor6.Value?.ToString() ?? "ALL";
            bool isRange     = mode == "By Range";
            bool isSelection = mode == "Filter by Selection";

            // By Range: [Mode][combo9][btn3][combo10][btn4]
            if (ultraComboEditor9  != null) ultraComboEditor9.Visible  = isRange;
            if (button3            != null) button3.Visible            = isRange;
            if (ultraComboEditor10 != null) ultraComboEditor10.Visible = isRange;
            if (button4            != null) button4.Visible            = isRange;

            // Selection: [Mode][combo14][btn8]
            if (ultraComboEditor14 != null) ultraComboEditor14.Visible = isSelection;
            if (button8            != null) button8.Visible            = isSelection;

            if (isRange || isSelection) LazyLoadCategories();
        }

        private void ToggleBrandFilters()
        {
            string mode      = ultraComboEditor4.Value?.ToString() ?? "ALL";
            bool isRange     = mode == "By Range";
            bool isSelection = mode == "Filter by Selection";

            // By Range: [Mode][combo12][btn5][combo11][btn6]
            if (ultraComboEditor12 != null) ultraComboEditor12.Visible = isRange;
            if (button5            != null) button5.Visible            = isRange;
            if (ultraComboEditor11 != null) ultraComboEditor11.Visible = isRange;
            if (button6            != null) button6.Visible            = isRange;

            // Selection: [Mode][combo15][btn9]
            if (ultraComboEditor15 != null) ultraComboEditor15.Visible = isSelection;
            if (button9            != null) button9.Visible            = isSelection;

            if (isRange || isSelection) LazyLoadBrands();
        }

        private void LazyLoadGroups()
        {
            if (_groupsLoaded) return;
            try
            {
                var groups = _dropdowns.getGroupDDl();
                if (groups?.List != null)
                {
                    ultraComboEditor8.Items.Clear();
                    ultraComboEditor3.Items.Clear();
                    ultraComboEditor13.Items.Clear();
                    
                    foreach (var g in groups.List)
                    {
                        ultraComboEditor8.Items.Add(g.Id, g.GroupName);
                        ultraComboEditor3.Items.Add(g.Id, g.GroupName);
                        ultraComboEditor13.Items.Add(g.Id, g.GroupName);
                    }
                    _groupsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error lazy loading groups: {ex.Message}");
            }
        }

        private void LazyLoadCategories()
        {
            if (_categoriesLoaded) return;
            try
            {
                var cats = _dropdowns.getCategoryDDl("");
                if (cats?.List != null)
                {
                    ultraComboEditor9.Items.Clear();
                    ultraComboEditor10.Items.Clear();
                    ultraComboEditor14.Items.Clear();
                    
                    foreach (var c in cats.List)
                    {
                        ultraComboEditor9.Items.Add(c.Id, c.CategoryName);
                        ultraComboEditor10.Items.Add(c.Id, c.CategoryName);
                        ultraComboEditor14.Items.Add(c.Id, c.CategoryName);
                    }
                    _categoriesLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error lazy loading categories: {ex.Message}");
            }
        }

        private void LazyLoadBrands()
        {
            if (_brandsLoaded) return;
            try
            {
                var brands = _dropdowns.getBrandDDl();
                if (brands?.List != null)
                {
                    ultraComboEditor12.Items.Clear();
                    ultraComboEditor11.Items.Clear();
                    ultraComboEditor15.Items.Clear();
                    
                    foreach (var b in brands.List)
                    {
                        ultraComboEditor12.Items.Add(b.Id, b.BrandName);
                        ultraComboEditor11.Items.Add(b.Id, b.BrandName);
                        ultraComboEditor15.Items.Add(b.Id, b.BrandName);
                    }
                    _brandsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error lazy loading brands: {ex.Message}");
            }
        }

        private void LazyLoadItemTypes()
        {
            if (_itemTypesLoaded) return;
            try
            {
                var types = _dropdowns.getItemTypeDDl();
                if (types?.List != null)
                {
                    ultraComboEditor7.Items.Clear();
                    ultraComboEditor7.Items.Add(0, "-- All Item Types --");
                    foreach (var t in types.List)
                    {
                        ultraComboEditor7.Items.Add(t.Id, t.ItemType);
                    }
                    ultraComboEditor7.Value = 0;
                    _itemTypesLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error lazy loading item types: {ex.Message}");
            }
        }

        private void OpenGroupDialogForCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmGroupDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    int selectedId = dlg.SelectedGroupId;
                    string selectedName = dlg.SelectedGroupName;
                    
                    LazyLoadGroups();

                    bool found = false;
                    foreach (var item in combo.Items)
                    {
                        if (item.DataValue != null && Convert.ToInt32(item.DataValue) == selectedId)
                        {
                            combo.Value = item.DataValue;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        combo.Items.Add(selectedId, selectedName);
                        combo.Value = selectedId;
                    }
                }
            }
        }

        private void OpenCategoryDialogForCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmCategoryDialog("StockReport"))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    int selectedId = dlg.SelectedCategoryId;
                    string selectedName = dlg.SelectedCategoryName;
                    
                    LazyLoadCategories();

                    bool found = false;
                    foreach (var item in combo.Items)
                    {
                        if (item.DataValue != null && Convert.ToInt32(item.DataValue) == selectedId)
                        {
                            combo.Value = item.DataValue;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        combo.Items.Add(selectedId, selectedName);
                        combo.Value = selectedId;
                    }
                }
            }
        }

        private void OpenBrandDialogForCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmBrandDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    int selectedId = dlg.SelectedBrandId;
                    string selectedName = dlg.SelectedBrandName;
                    
                    LazyLoadBrands();

                    bool found = false;
                    foreach (var item in combo.Items)
                    {
                        if (item.DataValue != null && Convert.ToInt32(item.DataValue) == selectedId)
                        {
                            combo.Value = item.DataValue;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        combo.Items.Add(selectedId, selectedName);
                        combo.Value = selectedId;
                    }
                }
            }
        }

        private void OpenItemDialogForBarcodeSearch()
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmdialForItemMaster("FrmBarcode"))
            {
                dlg.StartPosition = FormStartPosition.CenterParent;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Dictionary<string, object> itemData = dlg.GetSelectedItemData();
                    string barcode = GetItemDataValue(itemData, "BarCode", "Barcode");

                    if (!string.IsNullOrWhiteSpace(barcode))
                    {
                        txtBarcodeSearch.Text = barcode.Trim();
                        txtBarcodeSearch.Focus();
                    }
                }
            }
        }

        private void OpenItemCategoryDialogForCombo()
        {
            using (var dlg = new PosBranch_Win.DialogBox.frmdialForItemMaster("FrmBarcode"))
            {
                dlg.StartPosition = FormStartPosition.CenterParent;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                Dictionary<string, object> itemData = dlg.GetSelectedItemData();
                int categoryId = GetItemDataInt(itemData, "CategoryId", "CategoryID");
                string categoryName = GetItemDataValue(itemData, "CategoryName", "Category", "txt_Category");
                int itemId = GetItemDataInt(itemData, "ItemId", "ItemID", "Id");

                if ((categoryId <= 0 || string.IsNullOrWhiteSpace(categoryName)) && itemId > 0)
                {
                    try
                    {
                        var item = _itemRepository.GetByIdItem(itemId);
                        if (item != null)
                        {
                            categoryId = item.CategoryId;
                            categoryName = item.CategoryName;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Unable to load item category: " + ex.Message);
                    }
                }

                if (categoryId <= 0 && string.IsNullOrWhiteSpace(categoryName))
                {
                    MessageBox.Show("The selected item does not have a category.", "Category", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                ultraComboEditor6.Value = "Filter by Selection";
                LazyLoadCategories();
                SelectComboValueOrAdd(ultraComboEditor14, categoryId, categoryName);
                ToggleCategoryFilters();
            }
        }

        private static int GetItemDataInt(Dictionary<string, object> itemData, params string[] keys)
        {
            int value;
            return int.TryParse(GetItemDataValue(itemData, keys), out value) ? value : 0;
        }

        private static void SelectComboValueOrAdd(Infragistics.Win.UltraWinEditors.UltraComboEditor combo,
            int value, string text)
        {
            if (combo == null) return;

            foreach (ValueListItem item in combo.Items)
            {
                if (value > 0 && item.DataValue != null && Convert.ToInt32(item.DataValue) == value)
                {
                    combo.Value = item.DataValue;
                    return;
                }

                if (value <= 0 && !string.IsNullOrWhiteSpace(text) &&
                    string.Equals(Convert.ToString(item.DisplayText), text, StringComparison.OrdinalIgnoreCase))
                {
                    combo.Value = item.DataValue;
                    return;
                }
            }

            if (value > 0)
            {
                combo.Items.Add(value, string.IsNullOrWhiteSpace(text) ? value.ToString() : text);
                combo.Value = value;
            }
        }

        private static string GetItemDataValue(Dictionary<string, object> itemData, params string[] keys)
        {
            if (itemData == null)
            {
                return string.Empty;
            }

            foreach (string key in keys)
            {
                if (itemData.TryGetValue(key, out object value) && value != null)
                {
                    return Convert.ToString(value);
                }
            }

            return string.Empty;
        }

        private void txtBarcodeSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadReport();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void UpdateDateControlState()
        {
        }

        private void ultraComboEditor7_ValueChanged(object sender, EventArgs e)
        {
            // Item Type combo changed – no additional UI toggle needed;
            // the selection is read in LoadReport() via GetComboIntValue(ultraComboEditor7).
        }

        // ─── Panel / Button / Control Styling ─────────────────────────────────────────
        private void InitializePanels()
        {
            BackColor = FormBackColor;

            // ─── Fix Layout: re-order controls so Dock stacking works correctly ──────
            // WinForms docking stacks in reverse insertion order: Fill must be added first.
            this.Controls.Clear();
            this.Controls.Add(ultraPanelMaster);    // Fill  (added first = behind)
            this.Controls.Add(ultraPanelAction);    // Top   (docked after Fill)
            this.Controls.Add(ultraPanelControls);  // Top   (docked last = topmost)

            // ─── Dock the three main panels ────────────────────────────────────
            ultraPanelControls.Dock = DockStyle.Top;

            ultraPanelAction.Dock = DockStyle.Top;
            ultraPanelAction.Size = new Size(ultraPanelAction.Width, 47);

            ultraPanelMaster.Dock = DockStyle.Fill;

            // ─── Footer pinned to bottom, grid fills remaining space ──────────────────
            // Must add footer to ClientArea controls BEFORE setting Dock so ordering is respected.
            // Footer = Bottom, then grid = Fill → grid sits above footer automatically.
            ultraPanelGridFooter.Dock   = DockStyle.Bottom;
            ultraPanelGridFooter.Height = 26;

            gridReport.Dock = DockStyle.Fill;

            // ─── Styling ─────────────────────────────────────────────────────────────
            ultraPanelControls.Appearance.BackColor  = FilterPanelBackColor;
            ultraPanelControls.Appearance.BorderColor = BorderBlue;
            ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelAction.Appearance.BackColor  = ActionPanelBackColor;
            ultraPanelAction.Appearance.BorderColor = BorderBlue;
            ultraPanelAction.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelMaster.Appearance.BackColor  = FormBackColor;
            ultraPanelMaster.Appearance.BorderColor = BorderBlue;
            ultraPanelMaster.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelGridFooter.Appearance.BackColor  = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleLabel(lblGroup);
            StyleLabel(lblCategory);
            StyleLabel(lblBarcode);

            UpdateToggleButtonText();
        }

        private void StyleButtons()
        {
            StyleClassicButton(btnViewGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnToggleSelection);
        }

        private static void StyleClassicButton(Infragistics.Win.Misc.UltraButton btn)
        {
            btn.UseAppStyling  = false;
            btn.UseOsThemes    = DefaultableBoolean.False;
            btn.ButtonStyle    = UIElementButtonStyle.Flat;
            btn.UseFlatMode    = DefaultableBoolean.False;
            btn.Appearance.BackColor  = ButtonBlueTop;
            btn.Appearance.BackColor2 = ButtonBlueBottom;
            btn.Appearance.BackGradientStyle = GradientStyle.Vertical;
            btn.Appearance.ForeColor   = ButtonTextBlue;
            btn.Appearance.BorderColor = ButtonLightOutline;
            btn.Appearance.TextHAlign  = HAlign.Center;
            btn.Appearance.TextVAlign  = VAlign.Middle;
            btn.Appearance.FontData.Bold = DefaultableBoolean.False;
            btn.Appearance.FontData.SizeInPoints = 9;
            btn.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btn.HotTrackAppearance.BackColor  = Color.FromArgb(241, 247, 254);
            btn.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            btn.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.HotTrackAppearance.BorderColor = ButtonLightOutline;
            btn.HotTrackAppearance.ForeColor   = ButtonTextBlue;
            btn.PressedAppearance.BackColor  = Color.FromArgb(118, 161, 214);
            btn.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            btn.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            btn.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            btn.PressedAppearance.ForeColor   = ButtonTextBlue;
        }

        private void StyleFilterControls()
        {
            StyleFilterCombo(ultraComboGroup, true);
            StyleFilterCombo(ultraComboCategory, true);
            StyleFilterCombo(ultraComboEditor1, true);
            StyleFilterCombo(ultraComboEditor2, true);
            StyleFilterCombo(ultraComboEditor3, true);
            StyleFilterCombo(ultraComboEditor4, true);
            StyleFilterCombo(ultraComboEditor5, true);
            StyleFilterCombo(ultraComboEditor6, true);
            StyleFilterCombo(ultraComboEditor7, true);
            StyleFilterCombo(ultraComboEditor8, true);
            StyleFilterCombo(ultraComboEditor9, true);
            StyleFilterCombo(ultraComboEditor10, true);
            StyleFilterCombo(ultraComboEditor11, true);
            StyleFilterCombo(ultraComboEditor12, true);
            StyleFilterCombo(ultraComboEditor13, true);
            StyleFilterCombo(ultraComboEditor14, true);
            StyleFilterCombo(ultraComboEditor15, true);
            StyleTextEditor(txtBarcodeSearch);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Tahoma";
            lbl.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, bool isDropDownList)
        {
            combo.UseAppStyling  = false;
            combo.UseOsThemes    = DefaultableBoolean.False;
            combo.DisplayStyle   = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle    = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor  = ControlBackColor;
            combo.Appearance.BorderColor = BorderBlue;
            combo.Appearance.ForeColor  = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10;
            combo.ButtonStyle   = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = isDropDownList
                ? Infragistics.Win.DropDownStyle.DropDownList
                : Infragistics.Win.DropDownStyle.DropDown;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            editor.UseAppStyling  = false;
            editor.UseOsThemes    = DefaultableBoolean.False;
            editor.DisplayStyle   = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle    = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor  = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor  = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            editor.UseAppStyling  = false;
            editor.UseOsThemes    = DefaultableBoolean.False;
            editor.DisplayStyle   = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle    = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor  = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor  = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleCheckEditor(Infragistics.Win.UltraWinEditors.UltraCheckEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9;
        }



        // ─── Dropdown Data Loading ────────────────────────────────────────────────────
        private void LoadFilterDropdowns()
        {
            try
            {
                LoadLedgers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading filter data: " + ex.Message, "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadGroups()
        {
            try
            {
                var groups = _dropdowns.getGroupDDl();
                if (groups?.List != null)
                {
                    ultraComboGroup.Items.Clear();
                    ultraComboGroup.Items.Add(0, "-- All Groups --");
                    foreach (var g in groups.List)
                        ultraComboGroup.Items.Add(g.Id, g.GroupName);
                    ultraComboGroup.Value = 0;
                }
            }
            catch { }
        }

        private void LoadCategories(int groupId = 0)
        {
            try
            {
                var cats = _dropdowns.getCategoryDDl(groupId > 0 ? groupId.ToString() : "");
                if (cats?.List != null)
                {
                    ultraComboCategory.Items.Clear();
                    ultraComboCategory.Items.Add(0, "-- All Categories --");
                    foreach (var c in cats.List)
                        ultraComboCategory.Items.Add(c.Id, c.CategoryName);
                    ultraComboCategory.Value = 0;

                }
            }
            catch { }
        }

        private void LoadLedgers()
        {
            try
            {
                var vendors = _dropdowns.VendorDDL();
                if (vendors?.List != null)
                {

                }
            }
            catch { }
        }

        // ─── Grid Setup ───────────────────────────────────────────────────────────────
        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes   = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle    = UIElementBorderStyle.Solid;

            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor  = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor  = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor     = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2    = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor     = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor  = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew    = AllowAddNew.No;
            layout.Override.AllowDelete    = DefaultableBoolean.False;
            layout.Override.AllowUpdate    = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow  = SelectType.Single;
            layout.Override.RowSelectors   = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor  = FormBackColor;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;

            layout.Override.RowSelectorAppearance.BackColor  = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor   = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign  = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor  = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor  = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor          = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor        = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor    = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor    = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor  = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor  = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor  = Color.White;
            layout.Override.CellAppearance.BorderColor       = GridRowLine;
            layout.Override.CellAppearance.ForeColor         = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name     = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell   = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow    = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight      = 19;
            layout.Override.DefaultRowHeight  = 19;
            layout.RowConnectorStyle  = RowConnectorStyle.Solid;
            layout.RowConnectorColor  = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor  = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor  = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        // ─── Grid Layout / Row Events ─────────────────────────────────────────────────
        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0) return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
                col.Hidden = true;

            // Columns: Category | Group | Barcode | Description | Cost | Selling Price | UOM | Stock | Hold | Available
            ConfigureColumn(band, "CategoryName",  "Category",        110, null,       HAlign.Left,  0);
            ConfigureColumn(band, "GroupName",     "Group",           100, null,       HAlign.Left,  1);
            ConfigureColumn(band, "Barcode",       "Barcode",         120, null,       HAlign.Left,  2);
            ConfigureColumn(band, "ItemName",      "Description",     220, null,       HAlign.Left,  3);
            ConfigureColumn(band, "Cost",          "Cost",             90, "#,##0.00",  HAlign.Right, 4);
            ConfigureColumn(band, "RetailPrice",   "Selling Price",    90, "#,##0.00",  HAlign.Right, 5);
            ConfigureColumn(band, "BaseUnitName",  "UOM",              70, null,       HAlign.Center,6);
            ConfigureColumn(band, "ClosingStock",  "Stock",            80, "#,##0.##",  HAlign.Right, 7);
            ConfigureColumn(band, "HoldQty",       "Hold",             70, "#,##0.##",  HAlign.Right, 8);
            ConfigureColumn(band, "AvailableStock","Available",        85, "#,##0.##",  HAlign.Right, 9);

            // Colour coding
            if (band.Columns.Exists("ClosingStock"))
                band.Columns["ClosingStock"].CellAppearance.ForeColor = Color.FromArgb(27, 94, 32);
            if (band.Columns.Exists("HoldQty"))
                band.Columns["HoldQty"].CellAppearance.ForeColor = Color.FromArgb(191, 54, 12);
            if (band.Columns.Exists("AvailableStock"))
                band.Columns["AvailableStock"].CellAppearance.ForeColor = Color.FromArgb(1, 87, 155);

            e.Layout.AutoFitStyle = AutoFitStyle.None;
        }

        private static void ConfigureColumn(UltraGridBand band, string key, string header,
            int width, string format, HAlign align, int visPos)
        {
            if (!band.Columns.Exists(key)) return;

            UltraGridColumn col = band.Columns[key];
            col.Hidden = false;
            col.Header.Caption = header;
            col.Width = width;
            col.Header.VisiblePosition = visPos;
            col.Header.Appearance.BorderColor  = Color.FromArgb(197, 217, 241);
            col.CellAppearance.BorderColor      = Color.FromArgb(197, 217, 241);
            col.CellAppearance.TextHAlign       = align;
            col.CellAppearance.FontData.Name    = "Microsoft Sans Serif";
            col.CellAppearance.FontData.SizeInPoints = 8.25F;
            if (!string.IsNullOrWhiteSpace(format))
                col.Format = format;
        }

        private void LoadReport()
        {
            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                var filter = BuildFilter();
                var rawRows = _repo.GetStockReport(filter);

                // Fetch Brand / Type attribute mapping logic if needed
                string brandMode = ultraComboEditor4.Value?.ToString() ?? "ALL";
                int selectedItemTypeId = GetComboIntValue(ultraComboEditor7) ?? 0;
                
                Dictionary<int, StockReportAdvanceRepo.ItemAttributeMapping> mappings = null;
                if (brandMode != "ALL" || selectedItemTypeId > 0)
                {
                    mappings = _repo.GetItemAttributeMapping();
                }

                List<StockReportItem> filtered = new List<StockReportItem>();

                // Get dynamic comparison ranges/values
                string groupMode = ultraComboEditor5.Value?.ToString() ?? "ALL";
                string startGroup = groupMode == "By Range" ? ultraComboEditor8.Text?.Trim() : null;
                string endGroup = groupMode == "By Range" ? ultraComboEditor3.Text?.Trim() : null;

                string categoryMode = ultraComboEditor6.Value?.ToString() ?? "ALL";
                string startCategory = categoryMode == "By Range" ? ultraComboEditor9.Text?.Trim() : null;
                string endCategory = categoryMode == "By Range" ? ultraComboEditor10.Text?.Trim() : null;

                string startBrand = brandMode == "By Range" ? ultraComboEditor12.Text?.Trim() : null;
                string endBrand   = brandMode == "By Range" ? ultraComboEditor11.Text?.Trim() : null;  // ultraComboEditor11 = Brand To
                int selectedBrandId = brandMode == "Filter by Selection" ? (GetComboIntValue(ultraComboEditor15) ?? 0) : 0;

                // Load all brands into memory if brand filtering is active
                Dictionary<int, string> brandNames = null;
                if (brandMode == "By Range" || brandMode == "Filter by Selection")
                {
                    brandNames = new Dictionary<int, string>();
                    var brandListResult = _dropdowns.getBrandDDl();
                    if (brandListResult?.List != null)
                    {
                        foreach (var b in brandListResult.List)
                        {
                            brandNames[b.Id] = b.BrandName;
                        }
                    }
                }

                foreach (var row in rawRows)
                {
                    // 1. Group Range Filter
                    if (groupMode == "By Range")
                    {
                        if (!MatchRange(row.GroupName, startGroup, endGroup))
                            continue;
                    }

                    // 2. Category Range Filter
                    if (categoryMode == "By Range")
                    {
                        if (!MatchRange(row.CategoryName, startCategory, endCategory))
                            continue;
                    }

                    // 3. Brand Filter
                    if (brandMode != "ALL")
                    {
                        int brandId = 0;
                        if (mappings != null && mappings.TryGetValue(row.ItemId, out var map))
                        {
                            brandId = map.BrandId;
                        }

                        if (brandMode == "Filter by Selection")
                        {
                            if (brandId != selectedBrandId)
                                continue;
                        }
                        else if (brandMode == "By Range")
                        {
                            string brandName = "";
                            if (brandId > 0 && brandNames != null && brandNames.TryGetValue(brandId, out var name))
                            {
                                brandName = name;
                            }
                            if (!MatchRange(brandName, startBrand, endBrand))
                                continue;
                        }
                    }

                    // 4. Item Type Filter
                    if (selectedItemTypeId > 0)
                    {
                        int itemTypeId = 0;
                        if (mappings != null && mappings.TryGetValue(row.ItemId, out var map2))
                        {
                            itemTypeId = map2.ItemTypeId;
                        }
                        if (itemTypeId != selectedItemTypeId)
                            continue;
                    }

                    filtered.Add(row);
                }

                _reportRows = filtered;
                BindGrid(_reportRows);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load stock report.\n" + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = previousCursor;
            }
        }

        private bool MatchRange(string value, string start, string end)
        {
            value = value?.Trim() ?? "";
            
            if (!string.IsNullOrEmpty(start) && string.Compare(value, start, StringComparison.OrdinalIgnoreCase) < 0)
                return false;
                
            if (!string.IsNullOrEmpty(end) && string.Compare(value, end, StringComparison.OrdinalIgnoreCase) > 0)
                return false;
                
            return true;
        }

        private StockReportFilter BuildFilter()
        {
            int? groupId = null;
            string groupMode = ultraComboEditor5.Value?.ToString() ?? "ALL";
            if (groupMode == "Filter by Selection")
            {
                groupId = GetComboIntValue(ultraComboEditor13);
            }

            int? categoryId = null;
            string categoryMode = ultraComboEditor6.Value?.ToString() ?? "ALL";
            if (categoryMode == "Filter by Selection")
            {
                categoryId = GetComboIntValue(ultraComboEditor14); // ultraComboEditor14 = Category selection combo
            }

            DateTime fromDate = new DateTime(2000, 1, 1);
            DateTime toDate = DateTime.Today;

            return new StockReportFilter
            {
                FromDate    = fromDate,
                ToDate      = toDate,
                CompanyId   = SessionContext.CompanyId,
                BranchId    = SessionContext.BranchId,
                FinYearId   = SessionContext.FinYearId,
                GroupId     = groupId,
                CategoryId  = categoryId,
                BarcodeContains = string.IsNullOrWhiteSpace(txtBarcodeSearch.Text)
                                    ? null : txtBarcodeSearch.Text.Trim()
            };
        }



        private static int? GetComboIntValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo?.Value == null) return null;
            int val;
            return int.TryParse(combo.Value.ToString(), out val) && val > 0 ? val : (int?)null;
        }

      

        private void BindGrid(List<StockReportItem> rows)
        {
            gridReport.DataSource = rows;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(rows);
        }

        private void gridReport_InitializeRow(object sender, Infragistics.Win.UltraWinGrid.InitializeRowEventArgs e)
        {
            try
            {
                if (e.Row.Cells.Exists("ClosingStock"))
                {
                    decimal stock = Convert.ToDecimal(e.Row.Cells["ClosingStock"].Value ?? 0);
                    if (stock < 0)
                    {
                        e.Row.Appearance.BackColor = Color.FromArgb(254, 226, 226);
                        e.Row.Appearance.ForeColor = Color.FromArgb(153, 27, 27);
                    }
                }
            }
            catch { }
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
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
                ToggleGroupFilters();
                ToggleCategoryFilters();
                ToggleBrandFilters();
                _reportRows = new List<StockReportItem>();
                ResetReportView();
                txtBarcodeSearch.Focus();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ResetFilterControls(Control parent)
        {
            if (parent == null)
            {
                return;
            }

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
                if (value != null && string.Equals(Convert.ToString(value), "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    SetComboValue(combo, value);
                    return;
                }
            }

            foreach (ValueListItem item in combo.Items)
            {
                object value = item.DataValue;
                if (value != null && string.Equals(Convert.ToString(value), "0", StringComparison.OrdinalIgnoreCase))
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

        private void ResetReportView()
        {
            gridReport.DataSource = null;
            UpdateFooterValues(new List<StockReportItem>());
        }

        // ─── Grid Footer ──────────────────────────────────────────────────────────────
        private void InitializeGridFooter()
        {
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues(new List<StockReportItem>());
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band    = gridReport.DisplayLayout.Bands[0];
            int           xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>()
                                                        .OrderBy(c => c.Header.VisiblePosition))
            {
                if (col.Hidden) continue;

                Label footerLabel = new Label
                {
                    Name       = "footer_" + col.Key,
                    Text       = string.Empty,
                    TextAlign  = ContentAlignment.MiddleCenter,
                    BackColor  = GridHeaderBlue,
                    BorderStyle = BorderStyle.None,
                    AutoSize   = false,
                    Width      = col.Width,
                    Height     = Math.Max(ultraPanelGridFooter.Height - 2, 20),
                    Left       = xOffset,
                    Top        = 1,
                    Tag        = Tuple.Create(col.Key, string.Empty),
                    ForeColor  = Color.White,
                    Font       = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(col.Key)
                };
                footerLabel.Paint += FooterLabel_Paint;
                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[col.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(col.Key))
                    _columnAggregations[col.Key] = "None";

                xOffset += col.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip { Tag = columnKey };
            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0
                             && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey)
                             && IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

            AddFooterMenuItem(menu, "Sum",     "Sum",   isNumeric);
            AddFooterMenuItem(menu, "Min",     "Min",   true);
            AddFooterMenuItem(menu, "Max",     "Max",   true);
            AddFooterMenuItem(menu, "Count",   "Count", true);
            AddFooterMenuItem(menu, "Average", "Avg",   isNumeric);
            menu.Items.Add(new ToolStripSeparator());
            AddFooterMenuItem(menu, "None",    "None",  true);

            menu.Opening += (s, e) =>
            {
                string current = _columnAggregations.ContainsKey(columnKey)
                                    ? _columnAggregations[columnKey] : "None";
                foreach (ToolStripItem item in menu.Items)
                {
                    ToolStripMenuItem mi = item as ToolStripMenuItem;
                    if (mi?.Tag != null)
                        mi.Checked = string.Equals(mi.Tag.ToString(), current, StringComparison.OrdinalIgnoreCase);
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
            ContextMenuStrip  menu = item?.Owner as ContextMenuStrip;
            if (menu?.Tag == null || item?.Tag == null) return;

            _columnAggregations[menu.Tag.ToString()] = item.Tag.ToString();
            UpdateFooterValues(_reportRows);
        }

        private void UpdateFooterValues(IList<StockReportItem> rows)
        {
            if (_footerLabels.Count == 0) return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> entry in _footerLabels)
            {
                string columnKey   = entry.Key;
                Label  footerLabel = entry.Value;

                if (!_columnAggregations.ContainsKey(columnKey)
                    || string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag  = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result       = CalculateAggregation(columnKey, _columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, _columnAggregations[columnKey], result);
                footerLabel.Text     = displayValue;
                footerLabel.Tag      = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0) return;

            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn col in gridReport.DisplayLayout.Bands[0].Columns
                                                      .Cast<UltraGridColumn>()
                                                      .OrderBy(c => c.Header.VisiblePosition))
            {
                if (col.Hidden || !_footerLabels.ContainsKey(col.Key)) continue;

                Label footerLabel = _footerLabels[col.Key];
                footerLabel.Left   = xOffset;
                footerLabel.Width  = col.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                xOffset += col.Width;
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in gridReport.Rows)
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
                return aggregation == "Count" ? (object)0 : null;

            switch (aggregation)
            {
                case "Sum":
                    return visibleRows.Where(r => r.Cells.Exists(columnKey))
                        .Select(r => GetNumericValue(r.Cells[columnKey].Value))
                        .Where(v => v.HasValue).Sum(v => v.Value);
                case "Min":
                    return visibleRows.Where(r => r.Cells.Exists(columnKey))
                        .Select(r => r.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderBy(v => v).FirstOrDefault();
                case "Max":
                    return visibleRows.Where(r => r.Cells.Exists(columnKey))
                        .Select(r => r.Cells[columnKey].Value).Where(HasCellValue)
                        .Cast<IComparable>().OrderByDescending(v => v).FirstOrDefault();
                case "Count":
                    return visibleRows.Count(r => r.Cells.Exists(columnKey) && HasCellValue(r.Cells[columnKey].Value));
                case "Avg":
                    var vals = visibleRows.Where(r => r.Cells.Exists(columnKey))
                        .Select(r => GetNumericValue(r.Cells[columnKey].Value))
                        .Where(v => v.HasValue).Select(v => v.Value).ToList();
                    return vals.Count == 0 ? 0m : vals.Average();
                default:
                    return null;
            }
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null) return string.Empty;
            if (aggregation == "Count") return Convert.ToString(result);

            UltraGridColumn col = gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey)
                ? gridReport.DisplayLayout.Bands[0].Columns[columnKey] : null;
            decimal? numericValue = GetNumericValue(result);
            if (numericValue.HasValue)
                return col != null && !string.IsNullOrWhiteSpace(col.Format)
                    ? numericValue.Value.ToString(col.Format)
                    : numericValue.Value.ToString("N2");
            return Convert.ToString(result);
        }

        private static bool HasCellValue(object value)
            => value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(value));

        private static decimal? GetNumericValue(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column?.DataType == null) return false;
            Type t = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float)
                || t == typeof(int)    || t == typeof(long)   || t == typeof(short);
        }

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label footerLabel = sender as Label;
            if (footerLabel?.Tag == null) return;
            var val = footerLabel.Tag as Tuple<string, string>;
            if (val == null || string.IsNullOrEmpty(val.Item2)) return;
            using (Pen pen = new Pen(GridFooterBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, footerLabel.Width - 1, footerLabel.Height - 1);
        }

        // ─── Export ───────────────────────────────────────────────────────────────────
        private void ExportCsv()
        {
            if (_reportRows == null || _reportRows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter   = "CSV files (*.csv)|*.csv";
                dlg.FileName = string.Format("StockReport_{0:yyyyMMdd_HHmmss}.csv", DateTime.Now);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Category,Group,Barcode,Description,Cost,Selling Price,UOM,Stock,Hold,Available");
                foreach (StockReportItem row in _reportRows)
                {
                    sb.AppendLine(string.Join(",",
                        Escape(row.CategoryName),
                        Escape(row.GroupName),
                        Escape(row.Barcode),
                        Escape(row.ItemName),
                        row.Cost.ToString("F2"),
                        row.RetailPrice.ToString("F2"),
                        Escape(row.BaseUnitName),
                        row.ClosingStock.ToString("F2"),
                        row.HoldQty.ToString("F2"),
                        row.AvailableStock.ToString("F2")));
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string Escape(string value)
        {
            string s = value ?? string.Empty;
            if (!s.Contains(",") && !s.Contains("\"") && !s.Contains("\n")) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        // ─── Preview (grid print preview) ─────────────────────────────────────────────
        private void ShowReportPreview()
        {
            List<StockReportItem> rows = gridReport.DataSource as List<StockReportItem>;
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
                preview.Text = "Stock Listing - Report Preview";
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
                    Text = "STOCK LISTING",
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

                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = string.Format("Items: {0:N0}    |    Stock: {1:N2}    |    Available: {2:N2}",
                        rows.Count, rows.Sum(x => x.ClosingStock), rows.Sum(x => x.AvailableStock)),
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
            string groupMode = Convert.ToString(ultraComboEditor5.Value);
            string categoryMode = Convert.ToString(ultraComboEditor6.Value);
            string brandMode = Convert.ToString(ultraComboEditor4.Value);
            string dateText = "All dates";

            return string.Format("Group: {0}    |    Category: {1}    |    Brand: {2}    |    Date: {3}",
                string.IsNullOrWhiteSpace(groupMode) ? "All" : groupMode,
                string.IsNullOrWhiteSpace(categoryMode) ? "All" : categoryMode,
                string.IsNullOrWhiteSpace(brandMode) ? "All" : brandMode,
                dateText);
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
        }

        // ─── UI Helpers ───────────────────────────────────────────────────────────────
        private void UpdateToggleButtonText()
        {
            btnToggleSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
        }

        // ─── Event Handlers ───────────────────────────────────────────────────────────
        private void btnViewGrid_Click(object sender, EventArgs e)      => LoadReport();
        private void btnPreviewGrid_Click(object sender, EventArgs e)   => ShowReportPreview();
        private void btnPreviewReport_Click(object sender, EventArgs e) => ShowStockReportFormatDialog();
        private void btnExportGrid_Click(object sender, EventArgs e)    => ExportCsv();

        private void ShowStockReportFormatDialog()
        {
            ShowReportFormatDialog(
                "STOCK LISTING",
                new[]
                {
                    "STOCK LISTING",
                    "STOCK LISTING - GROUP BY CATEGORY",
                    "STOCK LISTING - SUMMARY"
                });
        }

        private void ShowReportFormatDialog(string reportCaption, IEnumerable<string> formatDescriptions)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(reportCaption, formatDescriptions))
            {
                dialog.ShowDialog(this);
            }
        }

        private void btnToggleSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            UpdateToggleButtonText();
        }

        private void frmStockReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)          { LoadReport();     e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.E) { ExportCsv(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close();           e.Handled = true; }
        }

        private void ultraPanelControls_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void frmStockReport_Load_1(object sender, EventArgs e)
        {

        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void ultraComboEditor15_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ultraComboEditor14_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
