using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Master;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    /// <summary>
    /// Quick Purchase Presets popup.
    /// Uses FrmQuickPurchasePresets.Designer.cs for Visual Studio Form Designer compatibility.
    /// Labels styled with Microsoft Sans Serif Regular font.
    /// Preset row selection immediately refreshes _gridItems.
    /// </summary>
    public partial class FrmQuickPurchasePresets : Form
    {
        // ── Theme Colors ───────────────────────────────────────────────────────────
        private static readonly Color BG = Color.FromArgb(196, 232, 255);
        private static readonly Color HeaderBg1 = Color.FromArgb(93, 151, 214);
        private static readonly Color HeaderBg2 = Color.FromArgb(67, 118, 184);
        private static readonly Color BorderColor = Color.FromArgb(118, 154, 198);
        private static readonly Color TextDark = Color.FromArgb(10, 31, 79);
        private static readonly Color AltRow = Color.FromArgb(245, 250, 255);
        private static readonly Color CellBorder = Color.FromArgb(197, 217, 241);

        // ── Action Panel Button Colors (Matching ultraPanel8 in FrmPurchase.cs) ────
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor = Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        // ── Public Results ─────────────────────────────────────────────────────────
        public int SelectedVendorId { get; private set; }
        public string SelectedVendorName { get; private set; }
        public List<QuickPurchasePresetItem> ExportedItems { get; private set; } = new List<QuickPurchasePresetItem>();

        // ── Repo & State ───────────────────────────────────────────────────────────
        private readonly QuickPurchasePresetRepository _repo = new QuickPurchasePresetRepository();
        private List<QuickPurchasePreset> _presets = new List<QuickPurchasePreset>();
        private QuickPurchasePreset _activePreset;

        // ── Dragging State ─────────────────────────────────────────────────────────
        private bool _isDragging;
        private Point _dragStart;

        // ── Constructor ────────────────────────────────────────────────────────────
        public FrmQuickPurchasePresets()
        {
            InitializeComponent();
            WireEvents();

            ApplyUnifiedGridTheme(_gridPresets);
            ApplyUnifiedGridTheme(_gridItems);

            LoadVendors();
            LoadPresets();
        }

        private void WireEvents()
        {
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); } };

            _btnClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _titleBar.MouseDown += TitleBar_MouseDown;
            _lblTitle.MouseDown += TitleBar_MouseDown;

            _leftPanel.Paint += PanelPaint;
            _centerPanel.Paint += PanelPaint;
            _rightPanel.Paint += PanelPaint;

            _gridPresets.InitializeLayout += GridPresets_InitializeLayout;
            _gridPresets.AfterSelectChange += GridPresets_SelectionOrRowChanged;
            _gridPresets.AfterRowActivate += GridPresets_SelectionOrRowChanged;
            _gridPresets.ClickCell += (s, e) => GridPresets_SelectionOrRowChanged(s, null);

            _gridItems.InitializeLayout += GridItems_InitializeLayout;

            _txtItemSearch.GotFocus += (s, ev) => { if (_txtItemSearch.ForeColor == Color.Gray) { _txtItemSearch.Text = ""; _txtItemSearch.ForeColor = Color.Black; } };
            _txtItemSearch.LostFocus += (s, ev) => { if (string.IsNullOrEmpty(_txtItemSearch.Text)) { _txtItemSearch.Text = "Search items…"; _txtItemSearch.ForeColor = Color.Gray; } };

            // Register ultraPanels with the ultraPanel8 button theme and actions
            RegisterActionPanel(ultraPanel1, () => BtnNewPreset_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel3, () => BtnDeletePreset_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel7, () => BtnAddItem_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel6, () => BtnRemoveItem_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel4, () => { _cmbVendor.Value = null; SelectedVendorId = 0; SelectedVendorName = string.Empty; });
            RegisterActionPanel(ultraPanel5, () => BtnExport_Click(null, EventArgs.Empty));

            _cmbVendor.ValueChanged += CmbVendor_ValueChanged;

            _leftPanel.Resize += (s, e) =>
            {
                int footerH = 24;
                int bottomBarH = 50;
                int top = 34;
                int gridH = Math.Max(50, _leftPanel.Height - top - bottomBarH - footerH - 12);
                _gridPresets.Size = new Size(_leftPanel.Width - 12, gridH);
                _footerPanelPresets.Location = new Point(6, top + gridH);
                _footerPanelPresets.Size = new Size(_leftPanel.Width - 12, footerH);
            };

            _centerPanel.Resize += (s, e) =>
            {
                int footerH = 24;
                int bottomBarH = 50;
                int top = 68;
                int gridH = Math.Max(50, _centerPanel.Height - top - bottomBarH - footerH - 12);
                _gridItems.Size = new Size(_centerPanel.Width - 12, gridH);
                _footerPanelItems.Location = new Point(6, top + gridH);
                _footerPanelItems.Size = new Size(_centerPanel.Width - 12, footerH);
            };
        }

        // ── Action Panel Button Theme (Matches ultraPanel8 in FrmPurchase.cs) ─────
        public void RegisterActionPanel(Infragistics.Win.Misc.UltraPanel panel, Action clickAction = null)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            ApplyActionPanelStyle(panel, false, false);

            EventHandler clickHandler = (s, e) => clickAction?.Invoke();
            EventHandler mouseEnterHandler = (s, e) => ApplyActionPanelStyle(panel, true, false);
            EventHandler mouseLeaveHandler = (s, e) =>
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyActionPanelStyle(panel, isInside, false);
            };
            MouseEventHandler mouseDownHandler = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ApplyActionPanelStyle(panel, true, true);
                }
            };
            MouseEventHandler mouseUpHandler = (s, e) =>
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyActionPanelStyle(panel, isInside, false);
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
            }
        }

        private static void ApplyActionPanelStyle(Infragistics.Win.Misc.UltraPanel panel, bool isHover, bool isPressed)
        {
            if (panel == null)
                return;

            AppearanceBase appearance = panel.Appearance;
            appearance.BackGradientStyle = GradientStyle.GlassBottom50;
            appearance.BorderColor = ButtonBlueBorder;
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
                appearance.BackColor = ButtonBlueTop;
                appearance.BackColor2 = ButtonBlueBottom;
            }
        }

        // ── Apply Unified Grid Theme ──────────────────────────────────────────────
        private static void ApplyUnifiedGridTheme(UltraGrid grid)
        {
            if (grid == null) return;

            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;

            layout.Appearance.BackColor = Color.FromArgb(232, 246, 255);
            layout.Appearance.BackColor2 = Color.FromArgb(232, 246, 255);
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Appearance.BorderColor = CellBorder;
            layout.BorderStyle = UIElementBorderStyle.Solid;

            layout.Override.HeaderStyle = HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor = HeaderBg1;
            layout.Override.HeaderAppearance.BackColor2 = HeaderBg2;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderColor;
            layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 25;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.BackColor = HeaderBg2;
            layout.Override.RowSelectorAppearance.BackColor2 = HeaderBg1;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderColor;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.ForeColor = TextDark;
            layout.Override.RowAppearance.BorderColor = CellBorder;
            layout.Override.RowAlternateAppearance.BackColor = AltRow;
            layout.Override.RowAlternateAppearance.BorderColor = CellBorder;

            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            layout.Override.SelectedRowAppearance.ForeColor = TextDark;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            layout.Override.ActiveRowAppearance.ForeColor = TextDark;
            layout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = CellBorder;
            layout.Override.CellAppearance.ForeColor = TextDark;
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            layout.Override.DefaultRowHeight = 24;
            layout.Override.RowSpacingBefore = 0;
            layout.Override.RowSpacingAfter = 0;
            layout.Override.CellPadding = 2;
            layout.Override.CellSpacing = 0;

            layout.GroupByBox.Hidden = true;
            layout.AutoFitStyle = AutoFitStyle.None;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.Scrollbars = Scrollbars.Both;
        }

        // ── Grid Layout Handlers ──────────────────────────────────────────────────
        private void GridPresets_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            if (band.Columns.Exists("PresetId"))
            {
                var col = band.Columns["PresetId"];
                col.Header.Caption = "ID";
                col.Width = 45;
                col.Header.VisiblePosition = 0;
                col.CellAppearance.TextHAlign = HAlign.Center;
            }
            if (band.Columns.Exists("PresetName"))
            {
                var col = band.Columns["PresetName"];
                col.Header.Caption = "Preset Name";
                col.Width = 145;
                col.Header.VisiblePosition = 1;
            }

            e.Layout.Override.SelectTypeRow = SelectType.Single;
            e.Layout.Override.CellClickAction = CellClickAction.RowSelect;
        }

        private void GridItems_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            string[] hidden = { "PresetItemId", "PresetId", "UnitId" };
            foreach (string h in hidden)
                if (band.Columns.Exists(h)) band.Columns[h].Hidden = true;

            var captions = new Dictionary<string, (string Caption, int Width, HAlign Align)>
            {
                ["ItemId"]    = ("ID", 48, HAlign.Center),
                ["ItemName"]  = ("Item Name", 180, HAlign.Left),
                ["Barcode"]   = ("Barcode", 110, HAlign.Center),
                ["Unit"]      = ("Unit", 65, HAlign.Center),
                ["UnitPrice"] = ("Price", 70, HAlign.Right),
                ["Cost"]      = ("Cost", 70, HAlign.Right),
                ["Quantity"]  = ("Qty", 55, HAlign.Center)
            };

            int pos = 0;
            foreach (var kv in captions)
            {
                if (!band.Columns.Exists(kv.Key)) continue;
                var col = band.Columns[kv.Key];
                col.Hidden = false;
                col.Header.Caption = kv.Value.Caption;
                col.Header.VisiblePosition = pos++;
                col.Width = kv.Value.Width;
                col.CellAppearance.TextHAlign = kv.Value.Align;
                if (kv.Key == "UnitPrice" || kv.Key == "Cost") col.Format = "N2";
            }

            if (band.Columns.Exists("Quantity"))
                band.Columns["Quantity"].CellActivation = Activation.AllowEdit;
        }

        // ── Data Loading ───────────────────────────────────────────────────────────
        private void LoadVendors()
        {
            try
            {
                var drop = new Repository.Dropdowns();
                var vendors = drop.VendorDDL();
                if (vendors?.List == null) return;

                _cmbVendor.Items.Clear();
                _cmbVendor.Items.Add(new Infragistics.Win.ValueListItem(0, "(No Vendor)"));
                foreach (var v in vendors.List)
                {
                    if (v == null) continue;
                    _cmbVendor.Items.Add(new Infragistics.Win.ValueListItem(v.LedgerID, v.LedgerName));
                }
                _cmbVendor.Value = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadVendors error: {ex.Message}");
            }
        }

        private void LoadPresets()
        {
            _presets = _repo.GetAllPresets();

            var dt = new DataTable();
            dt.Columns.Add("PresetId", typeof(int));
            dt.Columns.Add("PresetName", typeof(string));

            foreach (var p in _presets)
                dt.Rows.Add(p.PresetId, p.PresetName);

            _gridPresets.DataSource = dt;
            _gridPresets.DataBind();
            _gridPresets.Refresh();

            if (_gridPresets.Rows.Count > 0)
            {
                _gridPresets.ActiveRow = _gridPresets.Rows[0];
                _gridPresets.Selected.Rows.Clear();
                _gridPresets.Selected.Rows.Add(_gridPresets.Rows[0]);
                GridPresets_SelectionOrRowChanged(null, null);
            }
            else
            {
                _activePreset = null;
                _gridItems.DataSource = null;
            }
        }

        private void LoadPresetItems(int presetId)
        {
            try
            {
                var items = _repo.GetPresetItems(presetId);
                var dt = new DataTable();
                dt.Columns.Add("PresetItemId", typeof(int));
                dt.Columns.Add("PresetId", typeof(int));
                dt.Columns.Add("ItemId", typeof(int));
                dt.Columns.Add("ItemName", typeof(string));
                dt.Columns.Add("Barcode", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("UnitId", typeof(int));
                dt.Columns.Add("UnitPrice", typeof(double));
                dt.Columns.Add("Cost", typeof(double));
                dt.Columns.Add("Quantity", typeof(int));

                foreach (var it in items)
                    dt.Rows.Add(it.PresetItemId, it.PresetId, it.ItemId, it.ItemName, it.Barcode, it.Unit, it.UnitId, it.UnitPrice, it.Cost, it.Quantity);

                _gridItems.DataSource = dt;
                _gridItems.DataBind();
                _gridItems.Refresh();

                if (_activePreset != null && _activePreset.VendorId > 0)
                {
                    try { _cmbVendor.Value = _activePreset.VendorId; } catch { }
                }
                else
                {
                    try { _cmbVendor.Value = 0; } catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPresetItems error: {ex.Message}");
            }
        }

        // ── Event Handlers ─────────────────────────────────────────────────────────
        private void GridPresets_SelectionOrRowChanged(object sender, EventArgs e)
        {
            UltraGridRow selectedRow = null;
            if (_gridPresets.Selected.Rows.Count > 0)
            {
                selectedRow = _gridPresets.Selected.Rows[0];
            }
            else if (_gridPresets.ActiveRow != null)
            {
                selectedRow = _gridPresets.ActiveRow;
            }

            if (selectedRow == null || !selectedRow.Cells.Exists("PresetId") || selectedRow.Cells["PresetId"].Value == DBNull.Value || selectedRow.Cells["PresetId"].Value == null)
            {
                _activePreset = null;
                _gridItems.DataSource = null;
                return;
            }

            try
            {
                int presetId = Convert.ToInt32(selectedRow.Cells["PresetId"].Value);
                _activePreset = _presets.FirstOrDefault(p => p.PresetId == presetId);
                if (_activePreset != null)
                {
                    LoadPresetItems(_activePreset.PresetId);
                }
                else
                {
                    _gridItems.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GridPresets_SelectionOrRowChanged error: {ex.Message}");
            }
        }

        private void BtnNewPreset_Click(object sender, EventArgs e)
        {
            string name = ShowInputDialog("New Preset Name:", "New Preset");
            if (string.IsNullOrWhiteSpace(name)) return;

            var preset = new QuickPurchasePreset { PresetName = name.Trim(), VendorId = 0 };
            int id = _repo.SavePreset(preset);
            if (id > 0)
            {
                LoadPresets();
                foreach (UltraGridRow row in _gridPresets.Rows)
                {
                    if (Convert.ToInt32(row.Cells["PresetId"].Value) == id)
                    {
                        _gridPresets.ActiveRow = row;
                        _gridPresets.Selected.Rows.Clear();
                        _gridPresets.Selected.Rows.Add(row);
                        GridPresets_SelectionOrRowChanged(null, null);
                        break;
                    }
                }
            }
        }

        private void BtnDeletePreset_Click(object sender, EventArgs e)
        {
            if (_activePreset == null) { MessageBox.Show("Select a preset first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var dlgRes = MessageBox.Show($"Delete preset '{_activePreset.PresetName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dlgRes == DialogResult.Yes)
            {
                _repo.DeletePreset(_activePreset.PresetId);
                _activePreset = null;
                _gridItems.DataSource = null;
                LoadPresets();
            }
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (_activePreset == null)
            {
                MessageBox.Show("Please select or create a preset first.", "Select Preset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var dlg = new frmdialForItemMaster("FrmPurchase"))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        var data = dlg.GetSelectedItemData();

                        int itemId = (int)dlg.SelectedItemId;
                        string itemName = dlg.SelectedItemName ?? string.Empty;
                        string barcode = dlg.SelectedBarcode ?? string.Empty;

                        string unit = GetDictVal<string>(data, "UnitName", "Unit", "unit") ?? string.Empty;
                        int unitId = GetDictVal<int>(data, "UnitId", "unitId");
                        double unitPrice = GetDictDouble(data, "RetailPrice", "CostPrice", "UnitPrice", "Price");
                        double cost = GetDictDouble(data, "CostPrice", "Cost", "RetailPrice");

                        if (itemId <= 0)
                        {
                            MessageBox.Show("Could not read item ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var item = new QuickPurchasePresetItem
                        {
                            PresetId = _activePreset.PresetId,
                            ItemId = itemId,
                            ItemName = itemName,
                            Barcode = barcode,
                            Unit = unit,
                            UnitId = unitId,
                            UnitPrice = unitPrice,
                            Cost = cost,
                            Quantity = 1
                        };

                        int savedId = _repo.AddItemToPreset(item);
                        if (savedId > 0) LoadPresetItems(_activePreset.PresetId);
                        else MessageBox.Show("Failed to add item to preset.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (_gridItems.ActiveRow == null) { MessageBox.Show("Select an item first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int presetItemId = 0;
            try { presetItemId = Convert.ToInt32(_gridItems.ActiveRow.Cells["PresetItemId"].Value); } catch { }
            if (presetItemId <= 0) return;

            var dlgRes = MessageBox.Show("Remove this item from the preset?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dlgRes == DialogResult.Yes)
            {
                _repo.RemoveItemFromPreset(presetItemId);
                if (_activePreset != null) LoadPresetItems(_activePreset.PresetId);
            }
        }

        private void CmbVendor_ValueChanged(object sender, EventArgs e)
        {
            if (_cmbVendor.Value == null || Convert.ToInt32(_cmbVendor.Value) == 0)
            {
                SelectedVendorId = 0;
                SelectedVendorName = string.Empty;
                if (_activePreset != null) { _activePreset.VendorId = 0; _activePreset.VendorName = string.Empty; _repo.SavePreset(_activePreset); }
                return;
            }

            SelectedVendorId = Convert.ToInt32(_cmbVendor.Value);
            foreach (var item in _cmbVendor.Items)
            {
                var vli = item as Infragistics.Win.ValueListItem;
                if (vli != null && Convert.ToInt32(vli.DataValue) == SelectedVendorId)
                {
                    SelectedVendorName = vli.DisplayText;
                    break;
                }
            }

            if (_activePreset != null)
            {
                _activePreset.VendorId = SelectedVendorId;
                _activePreset.VendorName = SelectedVendorName;
                _repo.SavePreset(_activePreset);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (_activePreset == null)
            {
                MessageBox.Show("Please select a preset to export.", "No Preset Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveGridQuantities();
            ExportedItems = _repo.GetPresetItems(_activePreset.PresetId);

            if (ExportedItems == null || ExportedItems.Count == 0)
            {
                MessageBox.Show("The selected preset has no items.", "Empty Preset", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedVendorId = _activePreset.VendorId;
            SelectedVendorName = _activePreset.VendorName ?? string.Empty;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void SaveGridQuantities()
        {
            try
            {
                if (_gridItems.DataSource == null) return;
                foreach (UltraGridRow row in _gridItems.Rows)
                {
                    int presetItemId = 0;
                    int qty = 1;
                    try
                    {
                        presetItemId = Convert.ToInt32(row.Cells["PresetItemId"].Value);
                        qty = Convert.ToInt32(row.Cells["Quantity"].Value);
                    }
                    catch { continue; }
                    if (presetItemId > 0 && qty > 0)
                        _repo.UpdateItemQuantity(presetItemId, qty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveGridQuantities error: {ex.Message}");
            }
        }

        // ── Paint / Drag / Helpers ─────────────────────────────────────────────────
        private void PanelPaint(object sender, PaintEventArgs e)
        {
            var pnl = sender as Panel;
            if (pnl == null) return;
            using (var pen = new Pen(BorderColor, 1))
                e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _isDragging = true;
            _dragStart = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point pt = PointToScreen(e.Location);
                Location = new Point(pt.X - _dragStart.X, pt.Y - _dragStart.Y);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) { _isDragging = false; base.OnMouseUp(e); }

        private static string ShowInputDialog(string prompt, string title)
        {
            using (var frm = new Form { Width = 350, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, Text = title, StartPosition = FormStartPosition.CenterParent, MaximizeBox = false, MinimizeBox = false })
            {
                var lbl = new Label { Text = prompt, Left = 12, Top = 16, Width = 310, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular) };
                var txt = new TextBox { Left = 12, Top = 38, Width = 310, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular) };
                var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 145, Top = 70, Width = 80, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular) };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 232, Top = 70, Width = 80, Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular) };
                frm.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
                frm.AcceptButton = ok;
                frm.CancelButton = cancel;
                return frm.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : string.Empty;
            }
        }

        private static T GetDictVal<T>(Dictionary<string, object> dict, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (dict.TryGetValue(key, out object val) && val != null)
                {
                    try { return (T)Convert.ChangeType(val, typeof(T)); } catch { }
                }
            }
            return default(T);
        }

        private static double GetDictDouble(Dictionary<string, object> dict, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (dict.TryGetValue(key, out object val) && val != null)
                {
                    if (double.TryParse(val.ToString(), out double d)) return d;
                }
            }
            return 0;
        }
    }
}
