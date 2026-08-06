using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository.MasterRepositry;

namespace PosBranch_Win.Master
{
    public partial class frmItemType : Form
    {
        private readonly Color pageBack = Color.FromArgb(232, 246, 255);
        private readonly Color cardBack = Color.FromArgb(250, 253, 255);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color muted = Color.FromArgb(72, 98, 138);
        private readonly Color accent = Color.FromArgb(42, 121, 232);
        private readonly Color skyBlueOutline = Color.FromArgb(102, 190, 255);
        private readonly Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
        private readonly Color gridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private readonly Color gridSelectedBlue = Color.FromArgb(126, 126, 245);
        private readonly Color gridRowLine = Color.FromArgb(197, 217, 241);
        private readonly Color gridAltRow = Color.FromArgb(246, 250, 255);
        private readonly Color buttonBlueTop = Color.FromArgb(232, 241, 252);
        private readonly Color buttonBlueBottom = Color.FromArgb(145, 181, 224);
        private readonly Color buttonLightOutline = Color.FromArgb(166, 183, 202);

        private ItemTypeRepository _repository;
        private List<ItemType> _itemTypesList;
        private ItemType _currentItemType;
        private int _currentIndex = -1;
        private bool _isEventsWired = false;
        private bool _isInitializing = false;

        public frmItemType()
        {
            InitializeComponent();
            _repository = new ItemTypeRepository();
            _itemTypesList = new List<ItemType>();
            _currentItemType = new ItemType();

            this.Load += FrmItemType_Load;
            this.Shown += FrmItemType_Shown;

            InitForm();
        }

        private void FrmItemType_Load(object sender, EventArgs e)
        {
            InitForm();
        }

        private void FrmItemType_Shown(object sender, EventArgs e)
        {
            InitForm();
        }

        private void InitForm()
        {
            if (_isInitializing) return;
            _isInitializing = true;

            try
            {
                this.KeyPreview = true;
                ApplyRuntimeStyles();
                WireEvents();
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitForm error: {ex.Message}");
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void ApplyRuntimeStyles()
        {
            Text = "Item Type Master";
            BackColor = pageBack;
            Font = new Font("Segoe UI", 9F);

            StyleFilterText(txt_ItemType);

            AttachCardPaint(panelFilters);
            AttachCardPaint(panelGrid);

            StyleButton(btnSave, true);
            StyleButton(btnUpdate, true);
            StyleButton(btnDelete, false);
            StyleButton(btnClear, false);
            StyleButton(btnClose, false);

            StyleClassicButton(btnSetDefault);
            StyleClassicButton(btnSearchF11);

            StyleGrid();
        }

        private void AttachCardPaint(Panel panel)
        {
            if (panel != null)
                panel.Paint += Card_Paint;
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (Pen pen = new Pen(border, 1))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void StyleButton(Button button, bool primary)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.ForeColor = primary ? Color.White : navy;
            button.BackColor = primary ? accent : Color.FromArgb(236, 246, 255);
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = primary ? accent : skyBlueOutline;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.MouseOverBackColor = primary ? accent : Color.FromArgb(225, 244, 255);
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(31, 96, 205) : Color.FromArgb(210, 235, 252);
        }

        private void StyleFilterText(UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = skyBlueOutline;
            editor.Appearance.ForeColor = navy;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void StyleClassicButton(UltraButton button)
        {
            if (button == null) return;

            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = buttonBlueTop;
            button.Appearance.BackColor2 = buttonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = navy;
            button.Appearance.BorderColor = buttonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = buttonLightOutline;
            button.HotTrackAppearance.ForeColor = navy;
            button.PressedAppearance.BackColor = Color.FromArgb(118, 161, 214);
            button.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            button.PressedAppearance.ForeColor = navy;
        }

        private void StyleGrid()
        {
            if (gridReport == null) return;

            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Appearance.BackColor = pageBack;
            layout.Appearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Appearance.BackColor2 = pageBack;
            layout.Appearance.BackGradientStyle = GradientStyle.None;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Override.RowSelectorAppearance.BackColor = gridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = gridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = gridAltRow;
            layout.Override.RowAppearance.BorderColor = gridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = gridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = gridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.BackColor = gridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.CellAppearance.BorderColor = gridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Segoe UI";
            layout.Override.CellAppearance.FontData.SizeInPoints = 9F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 22;
            layout.Override.DefaultRowHeight = 22;
            layout.RowConnectorStyle = RowConnectorStyle.None;
            layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void WireEvents()
        {
            if (_isEventsWired) return;

            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnSetDefault.Click += BtnSetDefault_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;
            btnClose.Click += BtnClose_Click;

            btnSearchF11.Click += BtnSearchF11_Click;

            gridReport.ClickCell += GridReport_ClickCell;
            gridReport.DoubleClickRow += GridReport_DoubleClickRow;

            txt_ItemType.ValueChanged += Txt_ItemType_ValueChanged;
            txt_ItemType.TextChanged += Txt_ItemType_ValueChanged;

            this.KeyDown += FrmItemType_KeyDown;

            _isEventsWired = true;
        }

        private void Txt_ItemType_ValueChanged(object sender, EventArgs e)
        {
            string text = txt_ItemType.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(text))
            {
                _currentItemType = new ItemType();
                _currentIndex = -1;
                SetButtonMode(false);
                return;
            }

            // If an item record is currently loaded for edit (_currentItemType.Id > 0),
            // maintain edit mode so btnUpdate stays visible and btnSave stays hidden when user edits/renames the text!
            if (_currentItemType != null && _currentItemType.Id > 0)
            {
                SetButtonMode(true);
                return;
            }

            var match = _itemTypesList?.FirstOrDefault(x => string.Equals(x.ItemTypeName, text, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                _currentItemType = match;
                _currentIndex = _itemTypesList.IndexOf(match);
                SetButtonMode(true);
            }
            else
            {
                _currentItemType = new ItemType();
                _currentIndex = -1;
                SetButtonMode(false);
            }
        }

        private void FrmItemType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                BtnClose_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F11)
            {
                OpenSearchDialog();
                e.Handled = true;
            }
        }

        private void LoadData()
        {
            try
            {
                var rawList = _repository.GetAllItemTypes() ?? new List<ItemType>();
                _itemTypesList = rawList.OrderBy(x => x.Id).ToList();

                gridReport.DataSource = _itemTypesList.Select(x => new
                {
                    Id = x.Id,
                    ItemType = x.ItemTypeName,
                    IsDefault = x.IsDefault ? "Yes (Default)" : "No"
                }).ToList();

                ConfigureColumns();
                lblShowing.Text = $"Showing {_itemTypesList.Count} record(s)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Item Types: {ex.Message}");
            }
        }

        private void ConfigureColumns()
        {
            try
            {
                if (gridReport.DisplayLayout.Bands.Count > 0)
                {
                    var band = gridReport.DisplayLayout.Bands[0];
                    if (band.Columns.Exists("Id"))
                    {
                        band.Columns["Id"].Header.Caption = "ID";
                        band.Columns["Id"].Width = 60;
                        band.Columns["Id"].CellAppearance.TextHAlign = HAlign.Center;
                    }
                    if (band.Columns.Exists("ItemType"))
                    {
                        band.Columns["ItemType"].Header.Caption = "Item Type";
                        band.Columns["ItemType"].Width = 300;
                        band.Columns["ItemType"].CellAppearance.TextHAlign = HAlign.Left;
                    }
                    if (band.Columns.Exists("IsDefault"))
                    {
                        band.Columns["IsDefault"].Header.Caption = "Default Status";
                        band.Columns["IsDefault"].Width = 150;
                        band.Columns["IsDefault"].CellAppearance.TextHAlign = HAlign.Center;
                    }
                }
            }
            catch { }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txt_ItemType.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Please enter an Item Type name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_ItemType.Focus();
                    return;
                }

                ItemType itemType = new ItemType
                {
                    ItemTypeName = name,
                    IsDefault = false,
                    _Operation = "CREATE"
                };

                string result = _repository.SaveItemType(itemType);
                if (result == "Success")
                {
                    frmSuccesMsg msg = new frmSuccesMsg();
                    msg.ShowDialog();

                    LoadData();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to save Item Type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving Item Type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentItemType == null || _currentItemType.Id <= 0)
                {
                    MessageBox.Show("Please select an Item Type to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string name = txt_ItemType.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Please enter an Item Type name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_ItemType.Focus();
                    return;
                }

                _currentItemType.ItemTypeName = name;
                _currentItemType._Operation = "UPDATE";

                ItemType updated = _repository.UpdateItemType(_currentItemType);
                MessageBox.Show("Item Type updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Item Type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSetDefault_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentItemType == null || _currentItemType.Id <= 0)
                {
                    MessageBox.Show("Please select an Item Type to set as default.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = _repository.SetDefaultItemType(_currentItemType.Id);
                if (success)
                {
                    MessageBox.Show($"'{_currentItemType.ItemTypeName}' has been set as the default Item Type!", "Default Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to set default Item Type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting default Item Type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentItemType == null || _currentItemType.Id <= 0)
                {
                    MessageBox.Show("Please select an Item Type to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string name = txt_ItemType.Text?.Trim();
                DialogResult confirm = MessageBox.Show($"Are you sure you want to delete '{name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    _repository.DeleteItemType(_currentItemType.Id);
                    MessageBox.Show("Item Type deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting Item Type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Parent != null && this.Parent is Infragistics.Win.UltraWinTabControl.UltraTabPageControl tabPage)
                {
                    if (tabPage.Tab != null && tabPage.Tab.TabControl != null)
                    {
                        tabPage.Tab.TabControl.Tabs.Remove(tabPage.Tab);
                        return;
                    }
                }
                this.Close();
            }
            catch
            {
                this.Close();
            }
        }

        private void BtnSearchF11_Click(object sender, EventArgs e)
        {
            OpenSearchDialog();
        }

        private void OpenSearchDialog()
        {
            try
            {
                using (frmItemTypeDialog dlg = new frmItemTypeDialog())
                {
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    var res = dlg.ShowDialog();
                    if (res == DialogResult.OK || !string.IsNullOrWhiteSpace(dlg.SelectedItemType))
                    {
                        LoadData();
                        if (!string.IsNullOrWhiteSpace(dlg.SelectedItemType))
                        {
                            txt_ItemType.Text = dlg.SelectedItemType;
                            var match = _itemTypesList?.FirstOrDefault(x => string.Equals(x.ItemTypeName, dlg.SelectedItemType, StringComparison.OrdinalIgnoreCase));
                            if (match != null)
                            {
                                _currentItemType = match;
                                _currentIndex = _itemTypesList.IndexOf(match);
                                SetButtonMode(true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening ItemTypeDialog: {ex.Message}");
                MessageBox.Show($"Error opening search dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            txt_ItemType.Text = string.Empty;
            _currentItemType = new ItemType();
            _currentIndex = -1;
            SetButtonMode(false);
            txt_ItemType.Focus();
        }

        private void SetButtonMode(bool isEditMode)
        {
            btnSave.Visible = !isEditMode;
            btnUpdate.Visible = isEditMode;
            btnDelete.Enabled = isEditMode;
            btnSetDefault.Enabled = isEditMode;
        }

        private void GridReport_ClickCell(object sender, ClickCellEventArgs e)
        {
            if (e.Cell != null && e.Cell.Row != null && !e.Cell.Row.IsGroupByRow)
            {
                LoadRowToForm(e.Cell.Row);
            }
        }

        private void GridReport_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null && !e.Row.IsGroupByRow)
            {
                LoadRowToForm(e.Row);
            }
        }

        private void LoadRowToForm(UltraGridRow row)
        {
            try
            {
                if (row.Cells.Exists("Id"))
                {
                    int id = Convert.ToInt32(row.Cells["Id"].Value);
                    var match = _itemTypesList.FirstOrDefault(x => x.Id == id);
                    if (match != null)
                    {
                        _currentItemType = match;
                        _currentIndex = _itemTypesList.IndexOf(match);
                        txt_ItemType.Text = match.ItemTypeName;
                        SetButtonMode(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading row: {ex.Message}");
            }
        }
    }
}
