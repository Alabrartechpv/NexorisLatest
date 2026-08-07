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
    /// Grid ID columns hidden. Only Qty cell editable in _gridItems.
    /// Duplicate items prevented. Pressing 'q' moves focus to the next row's Qty cell.
    /// Search box shortcuts:
    ///   *<number>  => sets Qty of active row
    ///   **<number> => sets Qty of ALL rows
    ///   .<number>  => sets Cost of active row
    /// Fully functional summary grid footers (hidden when empty).
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

        // ── Footer Labels ──────────────────────────────────────────────────────────
        private Label _lblFooterPresets;
        private Label _lblFooterItems;

        // ── Dragging State ─────────────────────────────────────────────────────────
        private bool _isDragging;
        private Point _dragStart;

        private int _initialPresetId;

        // ── Constructors ───────────────────────────────────────────────────────────
        public FrmQuickPurchasePresets() : this(0)
        {
        }

        public FrmQuickPurchasePresets(int initialPresetId)
        {
            _initialPresetId = initialPresetId;
            InitializeComponent();
            WireEvents();

            ApplyUnifiedGridTheme(_gridPresets);
            ApplyUnifiedGridTheme(_gridItems);

            // Default state: footers hidden when starting / empty
            _footerPanelPresets.Visible = false;
            _footerPanelItems.Visible = false;

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
            _gridItems.KeyDown += GridItems_KeyDown;
            _gridItems.KeyPress += GridItems_KeyPress;
            _gridItems.AfterCellUpdate += (s, e) => UpdateGridFooters();

            _txtItemSearch.GotFocus += (s, ev) => { if (_txtItemSearch.ForeColor == Color.Gray) { _txtItemSearch.Text = ""; _txtItemSearch.ForeColor = Color.Black; } };
            _txtItemSearch.LostFocus += (s, ev) => { if (string.IsNullOrEmpty(_txtItemSearch.Text)) { _txtItemSearch.Text = "Search items…"; _txtItemSearch.ForeColor = Color.Gray; } };
            _txtItemSearch.TextChanged += TxtItemSearch_TextChanged;
            _txtItemSearch.KeyDown += TxtItemSearch_KeyDown;

            // Register ultraPanels with the ultraPanel8 button theme and actions
            RegisterActionPanel(ultraPanel1, () => BtnNewPreset_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel3, () => BtnDeletePreset_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel7, () => BtnAddItem_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel6, () => BtnRemoveItem_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel11, () => BtnExportGridFile_Click(null, EventArgs.Empty));
            RegisterActionPanel(ultraPanel4, () => { _cmbVendor.Value = null; SelectedVendorId = 0; SelectedVendorName = string.Empty; });
            RegisterActionPanel(ultraPanel5, () => BtnExport_Click(null, EventArgs.Empty));

            _cmbVendor.ValueChanged += CmbVendor_ValueChanged;

            _leftPanel.Resize += (s, e) =>
            {
                int footerH = _footerPanelPresets.Visible ? 24 : 0;
                int bottomBarH = 50;
                int top = 34;
                int gridH = Math.Max(50, _leftPanel.Height - top - bottomBarH - footerH - 12);
                _gridPresets.Size = new Size(_leftPanel.Width - 12, gridH);
                if (_footerPanelPresets.Visible)
                {
                    _footerPanelPresets.Location = new Point(6, top + gridH);
                    _footerPanelPresets.Size = new Size(_leftPanel.Width - 12, footerH);
                }
            };

            _centerPanel.Resize += (s, e) =>
            {
                int footerH = _footerPanelItems.Visible ? 24 : 0;
                int bottomBarH = 50;
                int top = 68;
                int gridH = Math.Max(50, _centerPanel.Height - top - bottomBarH - footerH - 12);
                _gridItems.Size = new Size(_centerPanel.Width - 12, gridH);
                if (_footerPanelItems.Visible)
                {
                    _footerPanelItems.Location = new Point(6, top + gridH);
                    _footerPanelItems.Size = new Size(_centerPanel.Width - 12, footerH);
                }
            };
        }

        // ── Dynamic Summary Footers ────────────────────────────────────────────────
        private void UpdateGridFooters()
        {
            // Presets Footer
            if (_gridPresets != null && _gridPresets.Rows.Count > 0)
            {
                _footerPanelPresets.Visible = true;
                if (_lblFooterPresets == null)
                {
                    _lblFooterPresets = new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.White,
                        Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Bold),
                        BackColor = Color.Transparent
                    };
                    _footerPanelPresets.ClientArea.Controls.Add(_lblFooterPresets);
                }
                _lblFooterPresets.Text = $"Total Presets: {_gridPresets.Rows.Count}";
            }
            else
            {
                _footerPanelPresets.Visible = false;
            }

            // Items Footer
            if (_gridItems != null && _gridItems.Rows.Count > 0)
            {
                _footerPanelItems.Visible = true;
                if (_lblFooterItems == null)
                {
                    _lblFooterItems = new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        ForeColor = Color.White,
                        Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Bold),
                        BackColor = Color.Transparent
                    };
                    _footerPanelItems.ClientArea.Controls.Add(_lblFooterItems);
                }

                int totalItems = _gridItems.Rows.Count;
                int totalQty = 0;
                double totalAmount = 0;

                foreach (UltraGridRow row in _gridItems.Rows)
                {
                    int qty = 0;
                    double cost = 0;
                    try { qty = Convert.ToInt32(row.Cells["Quantity"].Value ?? 0); } catch { }
                    try { cost = Convert.ToDouble(row.Cells["Cost"].Value ?? 0); } catch { }

                    totalQty += qty;
                    totalAmount += (cost * qty);
                }

                _lblFooterItems.Text = $"Total Items: {totalItems}   |   Total Qty: {totalQty}   |   Total Amount: {totalAmount:N2}";
            }
            else
            {
                _footerPanelItems.Visible = false;
            }

            // Refresh layout bounds after footer visibility update
            _leftPanel.PerformLayout();
            _centerPanel.PerformLayout();
        }

        // ── Search & Shortcut Commands (*number, **number, .number) ─────────────────
        private void TxtItemSearch_TextChanged(object sender, EventArgs e)
        {
            string query = _txtItemSearch.Text.Trim();
            if (query == "Search items…" || string.IsNullOrEmpty(query))
            {
                if (_gridItems.DisplayLayout?.Bands?.Count > 0)
                {
                    _gridItems.DisplayLayout.Bands[0].ColumnFilters.ClearAllFilters();
                }
                return;
            }

            if (query.StartsWith("*") || query.StartsWith("."))
            {
                // Don't filter grid with shortcut commands
                return;
            }

            if (_gridItems.DisplayLayout?.Bands?.Count > 0)
            {
                UltraGridBand band = _gridItems.DisplayLayout.Bands[0];
                band.ColumnFilters.ClearAllFilters();
                if (band.Columns.Exists("ItemName"))
                {
                    band.ColumnFilters["ItemName"].FilterConditions.Clear();
                    band.ColumnFilters["ItemName"].FilterConditions.Add(FilterComparisionOperator.Contains, query);
                }
            }
        }

        private void TxtItemSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string input = _txtItemSearch.Text.Trim();
                if (string.IsNullOrEmpty(input) || input == "Search items…") return;

                // **number => Apply quantity to ALL rows
                if (input.StartsWith("**"))
                {
                    string numStr = input.Substring(2).Trim();
                    if (int.TryParse(numStr, out int qty) && qty > 0)
                    {
                        ApplyQuantityToAllRows(qty);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        ResetSearchBox();
                        return;
                    }
                }
                // *number => Apply quantity to HIGHLIGHTED / ACTIVE row
                else if (input.StartsWith("*"))
                {
                    string numStr = input.Substring(1).Trim();
                    if (int.TryParse(numStr, out int qty) && qty > 0)
                    {
                        ApplyQuantityToActiveRow(qty);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        ResetSearchBox();
                        return;
                    }
                }
                // .number => Apply cost to HIGHLIGHTED / ACTIVE row
                else if (input.StartsWith("."))
                {
                    string numStr = input.Substring(1).Trim();
                    if (double.TryParse(numStr, out double cost) && cost >= 0)
                    {
                        ApplyCostToActiveRow(cost);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        ResetSearchBox();
                        return;
                    }
                }
            }
        }

        private void ApplyQuantityToActiveRow(int qty)
        {
            UltraGridRow targetRow = null;
            if (_gridItems.ActiveRow != null)
            {
                targetRow = _gridItems.ActiveRow;
            }
            else if (_gridItems.Selected.Rows.Count > 0)
            {
                targetRow = _gridItems.Selected.Rows[0];
            }
            else if (_gridItems.Rows.Count > 0)
            {
                targetRow = _gridItems.Rows[0];
            }

            if (targetRow != null && targetRow.Cells.Exists("Quantity"))
            {
                targetRow.Cells["Quantity"].Value = qty;
                int presetItemId = 0;
                try { presetItemId = Convert.ToInt32(targetRow.Cells["PresetItemId"].Value); } catch { }
                if (presetItemId > 0)
                {
                    _repo.UpdateItemQuantity(presetItemId, qty);
                }
                UpdateGridFooters();
            }
            else
            {
                MessageBox.Show("No item selected in grid.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ApplyCostToActiveRow(double cost)
        {
            UltraGridRow targetRow = null;
            if (_gridItems.ActiveRow != null)
            {
                targetRow = _gridItems.ActiveRow;
            }
            else if (_gridItems.Selected.Rows.Count > 0)
            {
                targetRow = _gridItems.Selected.Rows[0];
            }
            else if (_gridItems.Rows.Count > 0)
            {
                targetRow = _gridItems.Rows[0];
            }

            if (targetRow != null && targetRow.Cells.Exists("Cost"))
            {
                targetRow.Cells["Cost"].Value = cost;
                int presetItemId = 0;
                try { presetItemId = Convert.ToInt32(targetRow.Cells["PresetItemId"].Value); } catch { }
                if (presetItemId > 0)
                {
                    _repo.UpdateItemCost(presetItemId, cost);
                }
                UpdateGridFooters();
            }
            else
            {
                MessageBox.Show("No item selected in grid.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ApplyQuantityToAllRows(int qty)
        {
            if (_gridItems.Rows.Count == 0)
            {
                MessageBox.Show("No items in preset to update.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (UltraGridRow row in _gridItems.Rows)
            {
                if (row.Cells.Exists("Quantity"))
                {
                    row.Cells["Quantity"].Value = qty;
                    int presetItemId = 0;
                    try { presetItemId = Convert.ToInt32(row.Cells["PresetItemId"].Value); } catch { }
                    if (presetItemId > 0)
                    {
                        _repo.UpdateItemQuantity(presetItemId, qty);
                    }
                }
            }
            UpdateGridFooters();
        }

        private void ResetSearchBox()
        {
            _txtItemSearch.Text = "";
            if (_gridItems.DisplayLayout?.Bands?.Count > 0)
            {
                _gridItems.DisplayLayout.Bands[0].ColumnFilters.ClearAllFilters();
            }
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
                band.Columns["PresetId"].Hidden = true; // Hide ID cell
            }
            if (band.Columns.Exists("PresetName"))
            {
                var col = band.Columns["PresetName"];
                col.Header.Caption = "Preset Name";
                col.Width = 200;
                col.Header.VisiblePosition = 0;
            }

            e.Layout.Override.SelectTypeRow = SelectType.Single;
            e.Layout.Override.CellClickAction = CellClickAction.RowSelect;
        }

        private void GridItems_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            var band = e.Layout.Bands[0];
            string[] hidden = { "PresetItemId", "PresetId", "ItemId", "UnitId" };
            foreach (string h in hidden)
            {
                if (band.Columns.Exists(h))
                    band.Columns[h].Hidden = true; // Hide ID cells
            }

            var captions = new Dictionary<string, (string Caption, int Width, HAlign Align)>
            {
                ["ItemName"]  = ("Item Name", 200, HAlign.Left),
                ["Barcode"]   = ("Barcode", 110, HAlign.Center),
                ["Unit"]      = ("Unit", 65, HAlign.Center),
                ["UnitPrice"] = ("Price", 75, HAlign.Right),
                ["Cost"]      = ("Cost", 75, HAlign.Right),
                ["Quantity"]  = ("Qty", 60, HAlign.Center)
            };

            int pos = 0;
            foreach (var col in band.Columns)
            {
                if (captions.TryGetValue(col.Key, out var info))
                {
                    col.Hidden = false;
                    col.Header.Caption = info.Caption;
                    col.Header.VisiblePosition = pos++;
                    col.Width = info.Width;
                    col.CellAppearance.TextHAlign = info.Align;
                    if (col.Key == "UnitPrice" || col.Key == "Cost") col.Format = "N2";

                    // Allow editing ONLY for Quantity cell
                    if (col.Key == "Quantity")
                    {
                        col.CellActivation = Activation.AllowEdit;
                    }
                    else
                    {
                        col.CellActivation = Activation.NoEdit;
                    }
                }
                else
                {
                    col.Hidden = true;
                }
            }

            e.Layout.Override.AllowUpdate = DefaultableBoolean.True;
            e.Layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
        }

        // ── 'q' Key Navigation Handler ─────────────────────────────────────────────
        private void GridItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                _gridItems.PerformAction(UltraGridAction.ExitEditMode);
                MoveToNextRowQuantityCell();
            }
        }

        private void GridItems_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'q' || e.KeyChar == 'Q')
            {
                e.Handled = true;
            }
        }

        private void MoveToNextRowQuantityCell()
        {
            if (_gridItems.ActiveRow == null) return;

            int nextIndex = _gridItems.ActiveRow.Index + 1;
            if (nextIndex < _gridItems.Rows.Count)
            {
                UltraGridRow nextRow = _gridItems.Rows[nextIndex];
                _gridItems.ActiveRow = nextRow;
                _gridItems.Selected.Rows.Clear();
                _gridItems.Selected.Rows.Add(nextRow);

                if (nextRow.Cells.Exists("Quantity"))
                {
                    _gridItems.ActiveCell = nextRow.Cells["Quantity"];
                    _gridItems.PerformAction(UltraGridAction.EnterEditMode);
                }
            }
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
                UltraGridRow targetRow = _gridPresets.Rows[0];
                if (_initialPresetId > 0)
                {
                    foreach (UltraGridRow r in _gridPresets.Rows)
                    {
                        if (r.Cells.Exists("PresetId") && Convert.ToInt32(r.Cells["PresetId"].Value) == _initialPresetId)
                        {
                            targetRow = r;
                            break;
                        }
                    }
                }

                _gridPresets.ActiveRow = targetRow;
                _gridPresets.Selected.Rows.Clear();
                _gridPresets.Selected.Rows.Add(targetRow);
                GridPresets_SelectionOrRowChanged(null, null);
            }
            else
            {
                _activePreset = null;
                _gridItems.DataSource = null;
                UpdateGridFooters();
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

                UpdateGridFooters();

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
                UpdateGridFooters();
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
                    UpdateGridFooters();
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

                        if (itemId <= 0)
                        {
                            MessageBox.Show("Could not read item ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Prevent adding duplicate items
                        var currentItems = _repo.GetPresetItems(_activePreset.PresetId);
                        if (currentItems != null && currentItems.Any(i => i.ItemId == itemId))
                        {
                            MessageBox.Show($"Item '{itemName}' is already added to this preset.", "Duplicate Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        string unit = GetDictVal<string>(data, "UnitName", "Unit", "unit") ?? string.Empty;
                        int unitId = GetDictVal<int>(data, "UnitId", "unitId");
                        double unitPrice = GetDictDouble(data, "RetailPrice", "CostPrice", "UnitPrice", "Price");
                        double cost = GetDictDouble(data, "CostPrice", "Cost", "RetailPrice");

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

        private void BtnExportGridFile_Click(object sender, EventArgs e)
        {
            if (_gridItems == null || _gridItems.Rows.Count == 0)
            {
                MessageBox.Show("There are no items in the preset to export.", "Export Grid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                string safePresetName = _activePreset != null ? _activePreset.PresetName.Replace(" ", "_") : "PresetItems";
                dialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
                dialog.FileName = $"{safePresetName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                dialog.Title = "Export Preset Items Grid";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        SaveGridQuantities();
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.AppendLine("Item Name,Barcode,Unit,Price,Cost,Quantity,Total Amount");

                        foreach (UltraGridRow row in _gridItems.Rows)
                        {
                            string itemName = EscapeCsv(Convert.ToString(row.Cells["ItemName"].Value));
                            string barcode  = EscapeCsv(Convert.ToString(row.Cells["Barcode"].Value));
                            string unit     = EscapeCsv(Convert.ToString(row.Cells["Unit"].Value));
                            double price    = Convert.ToDouble(row.Cells["UnitPrice"].Value ?? 0);
                            double cost     = Convert.ToDouble(row.Cells["Cost"].Value ?? 0);
                            int qty         = Convert.ToInt32(row.Cells["Quantity"].Value ?? 0);
                            double total    = cost * qty;

                            sb.AppendLine($"{itemName},{barcode},{unit},{price:F2},{cost:F2},{qty},{total:F2}");
                        }

                        System.IO.File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                        MessageBox.Show($"Preset grid items exported successfully to:\n{dialog.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting grid items:\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static string EscapeCsv(string val)
        {
            string s = val ?? string.Empty;
            if (!s.Contains(",") && !s.Contains("\"") && !s.Contains("\n")) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
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

        public static string ShowInputDialog(string prompt, string title)
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
