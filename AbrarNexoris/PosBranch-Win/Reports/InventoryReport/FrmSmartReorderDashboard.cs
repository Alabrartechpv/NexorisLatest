using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Transaction;
using Repository;
using Repository.MasterRepositry;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsToolTip = System.Windows.Forms.ToolTip;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class FrmSmartReorderDashboard : Form
    {
        // ─── Theme Palette (matches frmVendorOutstandingReport / Stock Report theme) ────────
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);

        // ─── Action Panel Theme (Exact match with ultraPanel6 of frmReportFormatDialog) ───────
        private static readonly Color ButtonTopColor       = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor    = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor    = Color.FromArgb(73, 119, 184);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private sealed class ComboItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public string ParentValue { get; set; }
        }

        private readonly SmartReorderRepository _repository;
        private readonly Dropdowns _dropdowns;
        private readonly WinFormsToolTip _toolTip;
        private readonly List<ComboItem> _groupOptions;
        private readonly List<ComboItem> _categoryOptions;
        private List<SmartReorderItemModel> _allRows;
        private Form _columnChooserForm;
        private CheckedListBox _columnChooserListBox;
        private ContextMenuStrip _gridMenu;
        private bool _layoutLoaded;
        private bool _suppressGridCellUpdate;

        // ─── Attached Cell Footer Synchronization State ─────────────────────────────
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private const string GridLayoutFileName = "SmartReorderGridLayout.xml";
        private string GridLayoutPath => Path.Combine(Application.StartupPath, GridLayoutFileName);

        public FrmSmartReorderDashboard()
        {
            _repository = new SmartReorderRepository();
            _dropdowns = new Dropdowns();
            _toolTip = new WinFormsToolTip();
            _groupOptions = new List<ComboItem>();
            _categoryOptions = new List<ComboItem>();
            _allRows = new List<SmartReorderItemModel>();

            InitializeComponent();

            Load += FrmSmartReorderDashboard_Load;
            FormClosing += FrmSmartReorderDashboard_FormClosing;

            // Wire grid events for attached cell footer scrolling
            ultraGridMaster.Resize += (s, e) => UpdateFooterCellPositions();
            ultraGridMaster.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            ultraGridMaster.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            ultraGridMaster.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            ultraGridMaster.Paint += (s, e) => UpdateFooterCellPositions();
        }

        private void FrmSmartReorderDashboard_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
            {
                return;
            }

            InitializeRuntimeAppearance();
            BindStaticCombos();
            LoadLookupData();
            SetupGridMenu();
            LoadData();
        }

        private bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;
        }

        private int CurrentCompanyId
        {
            get
            {
                if (SessionContext.CompanyId > 0)
                {
                    return SessionContext.CompanyId;
                }

                int companyId;
                return int.TryParse(DataBase.CompanyId, out companyId) ? companyId : 0;
            }
        }

        private int CurrentBranchId
        {
            get
            {
                if (SessionContext.BranchId > 0)
                {
                    return SessionContext.BranchId;
                }

                int branchId;
                return int.TryParse(DataBase.BranchId, out branchId) ? branchId : 0;
            }
        }

        private void InitializeRuntimeAppearance()
        {
            BackColor = FormBackColor;

            // ── Panels ─────────────────────────────────────────────────────────────
            ultraPanelSelection.Appearance.BackColor  = FilterPanelBackColor;
            ultraPanelSelection.Appearance.BorderColor = BorderBlue;
            ultraPanelSelection.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelActionBar.Appearance.BackColor  = ActionPanelBackColor;
            ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
            ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;
            ultraPanelActionBar.Size = new Size(ultraPanelActionBar.Width, 38);

            ultraPanelGrid.Appearance.BackColor  = FormBackColor;
            ultraPanelGrid.Appearance.BorderColor = BorderBlue;
            ultraPanelGrid.BorderStyle = UIElementBorderStyle.Solid;

            gridFooterPanel.Appearance.BackColor  = GridHeaderBlue;
            gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
            gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
            gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
            gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
            gridFooterPanel.Height = 28;

            // ── Labels ─────────────────────────────────────────────────────────────
            StyleLabel(lblItemNo);
            StyleLabel(lblFromBarcode);
            StyleLabel(lblCategory);
            StyleLabel(lblGroup);
            StyleLabel(lblAlert);
            StyleLabel(lblMoreOptions);

            StyleStatusCountLabel(lblCount);
            StyleStatusCountLabel(lblExceptionCount);

            // ── Combos & Text Editors (SkyBlue outline) ─────────────────────────────
            StyleFilterCombo(cmbItemNoMode);
            StyleFilterCombo(cmbCategory);
            StyleFilterCombo(cmbGroup);
            StyleFilterCombo(cmbAlert);
            StyleFilterCombo(cmbMoreOptions);
            StyleTextEditor(txtFromBarcode);

            // ── Style Action Panels (Exact ultraPanel6 theme of frmReportFormatDialog) ───
            RegisterPanelButton(ultraPanel19, OpenItemMasterSearch);
            RegisterPanelButton(ultraPanel2, () => BtnViewGrid_Click(null, EventArgs.Empty));
            RegisterPanelButton(ultraPanel3, () => BtnGeneratePO_Click(null, EventArgs.Empty));
            RegisterPanelButton(ultraPanel4, () => BtnGenBranchPO_Click(null, EventArgs.Empty));
            RegisterPanelButton(ultraPanel5, () => BtnRefreshStats_Click(null, EventArgs.Empty));
            RegisterPanelButton(ultraPanel1, OpenPresetDialog);
            RegisterPanelButton(ultraPanel6, () => BtnHideSelection_Click(null, EventArgs.Empty));

            ConfigureGridAppearance();
            if (ultraPanel5 != null)
            {
                _toolTip.SetToolTip(ultraPanel5, "Refresh ADS snapshot from the database.");
            }
        }

        private void OpenItemMasterSearch()
        {
            try
            {
                using (frmdialForItemMaster dialog = new frmdialForItemMaster("SmartReorder"))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        string barcode = !string.IsNullOrWhiteSpace(dialog.SelectedBarcode)
                            ? dialog.SelectedBarcode
                            : (!string.IsNullOrWhiteSpace(dialog.SelectedItemNo) ? dialog.SelectedItemNo : Convert.ToString(dialog.Tag));

                        if (!string.IsNullOrWhiteSpace(barcode))
                        {
                            txtFromBarcode.Text = barcode;
                            LoadData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Item Master Dialog.\n\n" + ex.Message, "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPresetDialog()
        {
            try
            {
                ultraGridMaster.UpdateData();

                // 1. Collect selected items from grid (checkbox column or selected rows)
                List<SmartReorderItemModel> selectedItems = new List<SmartReorderItemModel>();

                foreach (UltraGridRow row in ultraGridMaster.Rows.GetFilteredInNonGroupByRows())
                {
                    if (row.ListObject is SmartReorderItemModel item && item.IsSelected)
                    {
                        selectedItems.Add(item);
                    }
                }

                if (selectedItems.Count == 0 && ultraGridMaster.Selected.Rows.Count > 0)
                {
                    foreach (UltraGridRow row in ultraGridMaster.Selected.Rows)
                    {
                        if (row.ListObject is SmartReorderItemModel item)
                        {
                            selectedItems.Add(item);
                        }
                    }
                }

                // Remove duplicates if any
                selectedItems = selectedItems.GroupBy(x => x.ItemId).Select(g => g.First()).ToList();

                // 2. If items are selected, prompt for preset name and save preset with items
                if (selectedItems.Count > 0)
                {
                    string presetName = FrmQuickPurchasePresets.ShowInputDialog(
                        $"Enter Preset Name for {selectedItems.Count} selected item(s):", 
                        "New Preset");

                    if (string.IsNullOrWhiteSpace(presetName))
                    {
                        return; // Canceled by user
                    }

                    QuickPurchasePresetRepository presetRepo = new QuickPurchasePresetRepository();
                    int presetId = presetRepo.SavePreset(new QuickPurchasePreset
                    {
                        PresetName = presetName.Trim(),
                        VendorId = 0
                    });

                    if (presetId > 0)
                    {
                        foreach (SmartReorderItemModel item in selectedItems)
                        {
                            double qty = item.FinalQuantity > 0 
                                ? (double)item.FinalQuantity 
                                : (item.SuggestedQuantity > 0 ? (double)item.SuggestedQuantity : 1.0);

                            QuickPurchasePresetItem presetItem = new QuickPurchasePresetItem
                            {
                                PresetId = presetId,
                                ItemId = (int)item.ItemId,
                                ItemName = item.ItemName,
                                Barcode = item.Barcode,
                                Unit = item.Unit,
                                UnitId = item.UnitId,
                                UnitPrice = 0,
                                Cost = 0,
                                Quantity = (int)Math.Max(1, Math.Round(qty))
                            };
                            presetRepo.AddItemToPreset(presetItem);
                        }

                        using (FrmQuickPurchasePresets presetsDialog = new FrmQuickPurchasePresets(presetId))
                        {
                            presetsDialog.ShowDialog(this);
                        }
                    }
                }
                else
                {
                    // No items selected -> simply open Presets Dialog
                    using (FrmQuickPurchasePresets presetsDialog = new FrmQuickPurchasePresets())
                    {
                        presetsDialog.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Quick Purchase Presets.\n\n" + ex.Message, "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Register Panel Buttons (Matches ultraPanel6 of frmReportFormatDialog) ────────
        public void RegisterPanelButton(UltraPanel panel, Action clickAction = null)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            ApplyPanelButtonStyle(panel, false, false);

            EventHandler clickHandler = (s, e) => clickAction?.Invoke();
            EventHandler mouseEnterHandler = (s, e) => ApplyPanelButtonStyle(panel, true, false);
            EventHandler mouseLeaveHandler = (s, e) =>
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            };
            MouseEventHandler mouseDownHandler = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ApplyPanelButtonStyle(panel, true, true);
                }
            };
            MouseEventHandler mouseUpHandler = (s, e) =>
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            };

            if (clickAction != null)
            {
                panel.Click += clickHandler;
                panel.ClientArea.Click += clickHandler;
            }
            panel.MouseEnter += mouseEnterHandler;
            panel.MouseLeave += mouseLeaveHandler;
            panel.MouseDown += mouseDownHandler;
            panel.MouseUp += mouseUpHandler;

            panel.ClientArea.MouseEnter += mouseEnterHandler;
            panel.ClientArea.MouseLeave += mouseLeaveHandler;
            panel.ClientArea.MouseDown += mouseDownHandler;
            panel.ClientArea.MouseUp += mouseUpHandler;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Cursor = Cursors.Hand;
                if (clickAction != null)
                    child.Click += clickHandler;
                child.MouseEnter += mouseEnterHandler;
                child.MouseLeave += mouseLeaveHandler;
                child.MouseDown += mouseDownHandler;
                child.MouseUp += mouseUpHandler;

                if (child is Label label)
                {
                    label.ForeColor = ButtonTextBlue;
                    label.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    label.BackColor = Color.Transparent;
                }
                else if (child is UltraLabel uLbl)
                {
                    uLbl.Appearance.ForeColor = ButtonTextBlue;
                    uLbl.Appearance.FontData.Name = "Microsoft Sans Serif";
                    uLbl.Appearance.FontData.SizeInPoints = 9F;
                    uLbl.Appearance.BackColor = Color.Transparent;
                }
            }
        }

        private static void ApplyPanelButtonStyle(UltraPanel panel, bool isHover, bool isPressed)
        {
            if (panel == null) return;
            Infragistics.Win.Appearance appearance = (Infragistics.Win.Appearance)panel.Appearance;
            appearance.BackGradientStyle = GradientStyle.Vertical;
            appearance.BorderColor = ButtonBorderColor;
            appearance.ForeColor = ButtonTextBlue;

            if (isPressed)
            {
                appearance.BackColor = PanelPressedTopColor;
                appearance.BackColor2 = PanelPressedBottomColor;
            }
            else if (isHover)
            {
                appearance.BackColor = PanelHoverTopColor;
                appearance.BackColor2 = PanelHoverBottomColor;
            }
            else
            {
                appearance.BackColor = ButtonTopColor;
                appearance.BackColor2 = ButtonBottomColor;
            }
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleStatusCountLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = ControlTextColor; // #123166 Dark Navy
            lbl.Appearance.FontData.Bold = DefaultableBoolean.True;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling  = false;
            combo.UseOsThemes    = DefaultableBoolean.False;
            combo.DisplayStyle   = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle    = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor  = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor  = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle   = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling  = false;
            editor.UseOsThemes    = DefaultableBoolean.False;
            editor.DisplayStyle   = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle    = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor  = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor  = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void ConfigureGridAppearance()
        {
            ultraGridMaster.UseAppStyling = false;
            ultraGridMaster.UseOsThemes = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.Appearance.BackColor = FormBackColor;
            ultraGridMaster.DisplayLayout.CaptionVisible = DefaultableBoolean.False;

            // Free cell positions (No AutoFit stretching / squishing)
            ultraGridMaster.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
            ultraGridMaster.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            ultraGridMaster.DisplayLayout.Scrollbars = Scrollbars.Both;

            ultraGridMaster.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.GroupByBox.Hidden = true;

            ultraGridMaster.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridMaster.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;

            // ── Slim & Clean Header Appearance (Matches Image 2 without any funnel icons or pins) ─
            ultraGridMaster.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.Override.FilterUIType = FilterUIType.Default;
            ultraGridMaster.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            ultraGridMaster.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGridMaster.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;

            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // ── Row Selectors with Numbers (1, 2, 3...) ────────────────────────────
            ultraGridMaster.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.RowSelectorWidth = 25;
            ultraGridMaster.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // ── Row & Cell Colors ──────────────────────────────────────────────────
            ultraGridMaster.DisplayLayout.Override.MinRowHeight = 24;
            ultraGridMaster.DisplayLayout.Override.DefaultRowHeight = 24;
            ultraGridMaster.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            ultraGridMaster.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridMaster.DisplayLayout.Override.RowAppearance.ForeColor = ControlTextColor;
            ultraGridMaster.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            ultraGridMaster.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            ultraGridMaster.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;

            ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;
            ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;

            ultraGridMaster.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            ultraGridMaster.DisplayLayout.Override.CellAppearance.ForeColor = ControlTextColor;
            ultraGridMaster.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            ultraGridMaster.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;

            ultraGridMaster.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
        }

        private void BindStaticCombos()
        {
            BindCombo(cmbItemNoMode, new List<ComboItem>
            {
                new ComboItem { Text = "By Range", Value = "RANGE" },
                new ComboItem { Text = "ALL", Value = "ALL" }
            }, "RANGE");

            BindCombo(cmbMoreOptions, new List<ComboItem>
            {
                new ComboItem { Text = "Include PO", Value = "INCLUDE_PO" },
                new ComboItem { Text = "Exclude PO", Value = "EXCLUDE_PO" }
            }, "INCLUDE_PO");

            BindCombo(cmbAlert, new List<ComboItem>
            {
                new ComboItem { Text = "ALL", Value = "ALL" },
                new ComboItem { Text = "URGENT", Value = "URGENT" },
                new ComboItem { Text = "Reorder Level Reached", Value = "Reorder Level Reached" },
                new ComboItem { Text = "Below Target Stock", Value = "Below Target Stock" },
                new ComboItem { Text = "Near Expiry", Value = "Near Expiry" },
                new ComboItem { Text = "Dead Stock", Value = "Dead Stock" },
                new ComboItem { Text = "INACTIVE ITEM", Value = "INACTIVE ITEM" },
                new ComboItem { Text = "Normal", Value = "Normal" }
            }, "ALL");
        }

        private void LoadLookupData()
        {
            BindGroupCombo();
            BindCategoryCombo();
        }

        private void BindGroupCombo()
        {
            _groupOptions.Clear();
            _groupOptions.Add(new ComboItem { Text = "ALL", Value = "ALL" });

            GroupDDlGrid groups = _dropdowns.getGroupDDl();
            if (groups != null && groups.List != null)
            {
                foreach (GroupDDL item in groups.List)
                {
                    _groupOptions.Add(new ComboItem
                    {
                        Text = item.GroupName ?? string.Empty,
                        Value = item.Id.ToString()
                    });
                }
            }

            BindCombo(cmbGroup, _groupOptions, "ALL");
        }

        private void BindCategoryCombo()
        {
            _categoryOptions.Clear();
            _categoryOptions.Add(new ComboItem { Text = "ALL", Value = "ALL", ParentValue = "ALL" });

            CategoryDDlGrid categories = _dropdowns.getCategoryDDl(string.Empty);
            if (categories != null && categories.List != null)
            {
                foreach (CategoryDDL item in categories.List)
                {
                    _categoryOptions.Add(new ComboItem
                    {
                        Text = item.CategoryName ?? string.Empty,
                        Value = item.Id.ToString(),
                        ParentValue = item.GroupId.ToString()
                    });
                }
            }

            ApplyCategoryOptions();
        }

        private void ApplyCategoryOptions()
        {
            string selectedGroup = GetSelectedValue(cmbGroup);

            List<ComboItem> items = _categoryOptions
                .Where(x => x.Value == "ALL" || selectedGroup == "ALL" || string.Equals(x.ParentValue, selectedGroup, StringComparison.OrdinalIgnoreCase))
                .ToList();

            string existingValue = GetSelectedValue(cmbCategory);
            string selectedValue = items.Any(x => x.Value == existingValue) ? existingValue : "ALL";
            BindCombo(cmbCategory, items, selectedValue);
        }

        private void BindCombo(UltraComboEditor combo, List<ComboItem> items, string selectedValue)
        {
            combo.Items.Clear();

            foreach (ComboItem item in items)
            {
                combo.Items.Add(item.Value, item.Text);
            }

            combo.Value = selectedValue;
        }

        private string GetSelectedValue(UltraComboEditor combo)
        {
            return combo.Value == null ? string.Empty : combo.Value.ToString();
        }

        private int? ToNullableInt(string value)
        {
            int parsed;
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return int.TryParse(value, out parsed) ? parsed : (int?)null;
        }

        private void LoadData()
        {
            try
            {
                int companyId = CurrentCompanyId;
                int branchId = CurrentBranchId;
                string barcodeFilter = txtFromBarcode.Text.Trim();
                string toBarcodeFilter = string.IsNullOrWhiteSpace(barcodeFilter) ? null : barcodeFilter;

                IEnumerable<SmartReorderItemModel> data = _repository.GetSmartReorderSuggestions(
                    companyId > 0 ? (int?)companyId : null,
                    branchId > 0 ? (int?)branchId : null,
                    ToNullableInt(GetSelectedValue(cmbCategory)),
                    ToNullableInt(GetSelectedValue(cmbGroup)),
                    string.IsNullOrWhiteSpace(barcodeFilter) ? null : barcodeFilter,
                    toBarcodeFilter);

                _allRows = data.ToList();
                ApplyClientFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load smart reorder data.\n\n" + ex.Message, "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyClientFilters()
        {
            IEnumerable<SmartReorderItemModel> filtered = _allRows;

            string alertFilter = GetSelectedValue(cmbAlert);
            if (!string.IsNullOrWhiteSpace(alertFilter) && !string.Equals(alertFilter, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(alertFilter, "URGENT", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(x => (x.Alert ?? string.Empty).StartsWith("URGENT", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    filtered = filtered.Where(x => string.Equals(x.Alert ?? string.Empty, alertFilter, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (chkShowOnlyExceptions.Checked)
            {
                filtered = filtered.Where(IsExceptionItem);
            }

            ultraGridMaster.DataSource = new BindingList<SmartReorderItemModel>(filtered.ToList());

            if (!_layoutLoaded)
            {
                LoadGridLayout();
            }

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            UpdateSummary();
            RefreshColumnChooser();
        }

        private bool IsExceptionItem(SmartReorderItemModel item)
        {
            string alert = (item.Alert ?? string.Empty).Trim();
            return !string.Equals(alert, "Normal", StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateSummary()
        {
            lblCount.Text = "Rows: " + ultraGridMaster.Rows.Count;
            lblExceptionCount.Text = "Exceptions: " + _allRows.Count(IsExceptionItem);
        }

        // ─── Attached Cell Footer Implementation ────────────────────────────────────
        private void CreateFooterCells()
        {
            gridFooterPanel.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int xOffset = ultraGridMaster.DisplayLayout.Override.RowSelectorWidth;

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
                    Height = Math.Max(gridFooterPanel.Height - 2, 22),
                    Left = xOffset,
                    Top = 1,
                    Tag = Tuple.Create(column.Key, string.Empty),
                    ForeColor = Color.White,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                    ContextMenuStrip = CreateFooterContextMenu(column.Key)
                };

                gridFooterPanel.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                // Default aggregation for ALL columns on form load is "None" (No hardcoding)
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

            bool isNumeric = ultraGridMaster.DisplayLayout.Bands.Count > 0 &&
                             ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey]);

            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum") { Tag = "Sum", Enabled = isNumeric };
            itemSum.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min") { Tag = "Min" };
            itemMin.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max") { Tag = "Max" };
            itemMax.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count") { Tag = "Count" };
            itemCount.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average") { Tag = "Avg", Enabled = isNumeric };
            itemAverage.Click += FooterContextMenu_Click;

            ToolStripMenuItem itemNone = new ToolStripMenuItem("None") { Tag = "None" };
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
                string currentAggregation = _columnAggregations.ContainsKey(columnKey) ? _columnAggregations[columnKey] : "None";
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
            if (item == null) return;
            ContextMenuStrip menu = item.Owner as ContextMenuStrip;
            if (menu == null || menu.Tag == null || item.Tag == null) return;

            string columnKey = menu.Tag.ToString();
            string aggregation = item.Tag.ToString();

            _columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraGridMaster.DisplayLayout == null || ultraGridMaster.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0)
                return;

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            int rowSelectorWidth = ultraGridMaster.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = 0;
            if (ultraGridMaster.ActiveColScrollRegion != null)
            {
                scrollOffset = ultraGridMaster.ActiveColScrollRegion.Position;
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
                footerLabel.Height = Math.Max(gridFooterPanel.Height - 2, 22);
                footerLabel.Visible = (left + width > 0 && left < gridFooterPanel.Width);
                footerLabel.Invalidate();
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0) return;

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
                return aggregation == "Count" ? (object)0 : null;

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
            if (result == null) return string.Empty;
            if (aggregation == "Count") return Convert.ToString(result);

            if (ultraGridMaster.DisplayLayout != null &&
                ultraGridMaster.DisplayLayout.Bands.Count > 0 &&
                ultraGridMaster.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = ultraGridMaster.DisplayLayout.Bands[0].Columns[columnKey];
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

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in ultraGridMaster.Rows)
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
            if (value == null || value == DBNull.Value) return null;
            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
        }

        private static bool IsSummableColumn(UltraGridColumn column)
        {
            if (column == null || column.DataType == null) return false;
            Type t = column.DataType;
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnGeneratePO_Click(object sender, EventArgs e)
        {
            ultraGridMaster.UpdateData();

            List<SmartReorderItemModel> selectedItems = new List<SmartReorderItemModel>();

            foreach (UltraGridRow row in ultraGridMaster.Rows.GetFilteredInNonGroupByRows())
            {
                SmartReorderItemModel item = row.ListObject as SmartReorderItemModel;
                if (item != null && item.IsSelected && item.FinalQuantity > 0)
                {
                    selectedItems.Add(item);
                }
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one row and keep Final Qty greater than zero.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Home homeForm = Application.OpenForms.OfType<Home>().FirstOrDefault();
            if (homeForm != null)
            {
                homeForm.OpenSmartReorderPurchaseOrder(selectedItems);
                return;
            }

            frmPurchaseOrder purchaseOrder = new frmPurchaseOrder();
            purchaseOrder.LoadSmartReorderItems(selectedItems);
            purchaseOrder.Show();
            purchaseOrder.BringToFront();
        }

        private void BtnGenBranchPO_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Branch PO generation is not implemented yet.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRefreshStats_Click(object sender, EventArgs e)
        {
            try
            {
                _repository.RefreshReorderStats(30);
                LoadData();
                MessageBox.Show("Reorder stats refreshed successfully.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to refresh reorder stats.\n\n" + ex.Message, "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnColumnChooser_Click(object sender, EventArgs e)
        {
            ShowColumnChooser();
        }

        private void BtnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelSelection.Visible = !ultraPanelSelection.Visible;
            if (lblHideSelection != null)
            {
                lblHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
            }
        }

        private void ChkShowOnlyExceptions_CheckedChanged(object sender, EventArgs e)
        {
            ApplyClientFilters();
        }

        private void CmbGroup_ValueChanged(object sender, EventArgs e)
        {
            ApplyCategoryOptions();
        }

        private void CmbAlert_ValueChanged(object sender, EventArgs e)
        {
            ApplyClientFilters();
        }

        private void UltraGridMaster_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            // ── Enforce Slim & Clean Headers (No filter icons, no pins, clean single line) ─
            e.Layout.Override.AllowRowFiltering = DefaultableBoolean.False;
            e.Layout.Override.FilterUIType = FilterUIType.Default;
            e.Layout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            e.Layout.Override.WrapHeaderText = DefaultableBoolean.False;
            e.Layout.Override.HeaderStyle = HeaderStyle.Standard;

            ConfigureColumn(band, "IsSelected", "Sel...", 50, true, "0");
            if (band.Columns.Exists("IsSelected"))
            {
                band.Columns["IsSelected"].Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                band.Columns["IsSelected"].CellActivation = Activation.AllowEdit;
            }

            ConfigureColumn(band, "Barcode", "Item No.", 120, false, null);
            ConfigureColumn(band, "ItemName", "Description", 220, false, null);
            ConfigureColumn(band, "Unit", "Unit", 80, false, null);
            ConfigureColumn(band, "Order_Cycle_Days", "Cycle Days", 80, false, "0");
            ConfigureColumn(band, "Box_Quantity", "Box Qty", 70, false, "0");
            ConfigureColumn(band, "Category", "Category", 100, false, null);
            ConfigureColumn(band, "Group", "Group", 100, false, null);
            ConfigureColumn(band, "CurrentStock", "Current Stock", 95, false, "0.####");
            ConfigureColumn(band, "AverageDailySales", "ADS", 80, false, "0.####");
            ConfigureColumn(band, "TargetStock", "Target Stock", 90, false, "0.####");
            ConfigureColumn(band, "ReorderLevel", "Reorder Level", 90, false, "0.####");
            ConfigureColumn(band, "SuggestedQuantity", "Suggested Qty", 90, false, "0.####");
            ConfigureColumn(band, "FinalQuantity", "Final Qty", 80, true, "0.####");
            ConfigureColumn(band, "DaysOfStockLeft", "Days Left", 80, false, "0.##");
            ConfigureColumn(band, "Alert", "Alert", 150, false, null);
            ConfigureColumn(band, "Reason", "Reason", 260, false, null);

            HideColumn(band, "ItemId");
            HideColumn(band, "UnitId");
            HideColumn(band, "Is_Perishable");
            HideColumn(band, "NearestExpiryDate");
            HideColumn(band, "LastSaleDate");
            HideColumn(band, "RequiredQuantity");

            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ConfigureColumn(UltraGridBand band, string key, string caption, int width, bool editable, string format)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            UltraGridColumn column = band.Columns[key];
            column.Header.Caption = caption;
            column.Width = width;
            column.CellActivation = editable ? Activation.AllowEdit : Activation.NoEdit;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void HideColumn(UltraGridBand band, string key)
        {
            if (band.Columns.Exists(key))
            {
                band.Columns[key].Hidden = true;
            }
        }

        private void UltraGridMaster_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            SmartReorderItemModel item = e.Row.ListObject as SmartReorderItemModel;
            if (item == null)
            {
                return;
            }

            string alert = (item.Alert ?? string.Empty).Trim();
            e.Row.Appearance.BackColor = Color.Empty;
            e.Row.Appearance.ForeColor = ControlTextColor;

            UltraGridCell alertCell = e.Row.Cells["Alert"];
            if (alertCell == null)
            {
                return;
            }

            alertCell.Appearance.BackColor = Color.Empty;
            alertCell.Appearance.ForeColor = ControlTextColor;

            if (alert.StartsWith("URGENT", StringComparison.OrdinalIgnoreCase))
            {
                alertCell.Appearance.BackColor = Color.DarkRed;
                alertCell.Appearance.ForeColor = Color.White;
            }
            else if (string.Equals(alert, "Reorder Level Reached", StringComparison.OrdinalIgnoreCase))
            {
                alertCell.Appearance.BackColor = Color.FromArgb(210, 51, 0);
                alertCell.Appearance.ForeColor = Color.White;
            }
            else if (string.Equals(alert, "Below Target Stock", StringComparison.OrdinalIgnoreCase))
            {
                alertCell.Appearance.BackColor = Color.FromArgb(255, 240, 188);
                alertCell.Appearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Near Expiry", StringComparison.OrdinalIgnoreCase))
            {
                alertCell.Appearance.BackColor = Color.LightSalmon;
                alertCell.Appearance.ForeColor = Color.Black;
            }
            else if (string.Equals(alert, "Dead Stock", StringComparison.OrdinalIgnoreCase))
            {
                alertCell.Appearance.BackColor = Color.DimGray;
                alertCell.Appearance.ForeColor = Color.Yellow;
            }
            else if (string.Equals(alert, "INACTIVE ITEM", StringComparison.OrdinalIgnoreCase))
            {
                alertCell.Appearance.BackColor = Color.FromArgb(110, 110, 110);
                alertCell.Appearance.ForeColor = Color.White;
            }
        }

        private void UltraGridMaster_AfterCellUpdate(object sender, CellEventArgs e)
        {
            if (_suppressGridCellUpdate || e.Cell == null)
            {
                return;
            }

            if (string.Equals(e.Cell.Column.Key, "FinalQuantity", StringComparison.OrdinalIgnoreCase))
            {
                decimal value;
                if (!decimal.TryParse(Convert.ToString(e.Cell.Value), out value) || value < 0)
                {
                    value = 0;
                }

                decimal existingValue;
                if (decimal.TryParse(Convert.ToString(e.Cell.Value), out existingValue) && existingValue == value)
                {
                    return;
                }

                try
                {
                    _suppressGridCellUpdate = true;
                    e.Cell.Value = value;
                }
                finally
                {
                    _suppressGridCellUpdate = false;
                }
                UpdateFooterValues();
            }
        }

        private void SetupGridMenu()
        {
            _gridMenu = new ContextMenuStrip();
            _gridMenu.Items.Add("Field/Column Chooser", null, (s, e) => ShowColumnChooser());
            ultraGridMaster.ContextMenuStrip = _gridMenu;
        }

        private void ShowColumnChooser()
        {
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            RefreshColumnChooser();
            PositionColumnChooser();
            _columnChooserForm.Show(this);
            _columnChooserForm.BringToFront();
        }

        private void CreateColumnChooserForm()
        {
            _columnChooserForm = new Form();
            _columnChooserForm.Text = "Column Chooser";
            _columnChooserForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            _columnChooserForm.StartPosition = FormStartPosition.Manual;
            _columnChooserForm.Size = new Size(260, 360);
            _columnChooserForm.ShowInTaskbar = false;

            _columnChooserListBox = new CheckedListBox();
            _columnChooserListBox.Dock = DockStyle.Fill;
            _columnChooserListBox.CheckOnClick = true;
            _columnChooserListBox.ItemCheck += ColumnChooserListBox_ItemCheck;

            _columnChooserForm.Controls.Add(_columnChooserListBox);
        }

        private void RefreshColumnChooser()
        {
            if (_columnChooserListBox == null || ultraGridMaster.DisplayLayout.Bands.Count == 0)
            {
                return;
            }

            UltraGridBand band = ultraGridMaster.DisplayLayout.Bands[0];
            _columnChooserListBox.ItemCheck -= ColumnChooserListBox_ItemCheck;
            _columnChooserListBox.Items.Clear();

            foreach (UltraGridColumn column in band.Columns)
            {
                if (string.Equals(column.Key, "ItemId", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int index = _columnChooserListBox.Items.Add(column);
                _columnChooserListBox.SetItemChecked(index, !column.Hidden);
            }

            _columnChooserListBox.DisplayMember = "Header.Caption";
            _columnChooserListBox.ItemCheck += ColumnChooserListBox_ItemCheck;
        }

        private void ColumnChooserListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                UltraGridColumn column = _columnChooserListBox.Items[e.Index] as UltraGridColumn;
                if (column == null)
                {
                    return;
                }

                column.Hidden = e.NewValue != CheckState.Checked;
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }));
        }

        private void PositionColumnChooser()
        {
            if (_columnChooserForm == null || _columnChooserForm.IsDisposed)
            {
                return;
            }

            Point screenPoint = PointToScreen(new Point(ClientSize.Width - _columnChooserForm.Width - 20, ClientSize.Height - _columnChooserForm.Height - 40));
            _columnChooserForm.Location = screenPoint;
        }

        private void LoadGridLayout()
        {
            if (_layoutLoaded)
            {
                return;
            }

            try
            {
                if (File.Exists(GridLayoutPath))
                {
                    ultraGridMaster.DisplayLayout.LoadFromXml(GridLayoutPath);
                }
            }
            catch
            {
            }

            _layoutLoaded = true;
        }

        private void SaveGridLayout()
        {
            try
            {
                ultraGridMaster.DisplayLayout.SaveAsXml(GridLayoutPath);
            }
            catch
            {
            }
        }

        private void FrmSmartReorderDashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveGridLayout();
        }
    }
}
