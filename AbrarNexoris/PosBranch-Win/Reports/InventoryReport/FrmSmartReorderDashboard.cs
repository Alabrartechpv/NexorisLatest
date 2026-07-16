using Infragistics.Win;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using PosBranch_Win.Transaction;
using Repository;
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
            ConfigureButton(btnViewGrid, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
         
            ConfigureButton(btnGeneratePO, Color.FromArgb(86, 118, 208), Color.FromArgb(117, 146, 225));
            ConfigureButton(btnGenBranchPO, Color.FromArgb(86, 118, 208), Color.FromArgb(117, 146, 225));
            ConfigureButton(btnRefreshStats, Color.FromArgb(67, 160, 71), Color.FromArgb(102, 187, 106));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            ConfigureGridAppearance();
            _toolTip.SetToolTip(btnRefreshStats, "Refresh ADS snapshot from the database.");
        }

        private void ConfigureButton(Infragistics.Win.Misc.UltraButton button, Color startColor, Color endColor)
        {
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.Appearance.BackColor = startColor;
            button.Appearance.BackColor2 = endColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.FontData.Bold = DefaultableBoolean.True;
            button.Appearance.BorderColor = startColor;
            button.HotTrackAppearance.BackColor = endColor;
            button.HotTrackAppearance.ForeColor = Color.White;
        }

        private void ConfigureGridAppearance()
        {
            ultraGridMaster.UseAppStyling = false;
            ultraGridMaster.UseOsThemes = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ultraGridMaster.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.GroupByBox.Hidden = true;
            ultraGridMaster.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGridMaster.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGridMaster.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            ultraGridMaster.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            ultraGridMaster.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            ultraGridMaster.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            ultraGridMaster.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            ultraGridMaster.DisplayLayout.Override.RowSelectorWidth = 28;
            ultraGridMaster.DisplayLayout.Override.MinRowHeight = 24;
            ultraGridMaster.DisplayLayout.Override.DefaultRowHeight = 24;
            ultraGridMaster.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGridMaster.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGridMaster.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            ultraGridMaster.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            ultraGridMaster.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGridMaster.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            ultraGridMaster.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGridMaster.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            ultraGridMaster.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            ultraGridMaster.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
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

        private void BtnViewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnPreviewGrid_Click(object sender, EventArgs e)
        {
            LoadData();
            MessageBox.Show("Grid preview is not implemented yet. The latest data has been refreshed.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPreviewReport_Click(object sender, EventArgs e)
        {
            LoadData();
            MessageBox.Show("Report preview is not implemented yet. The latest data has been refreshed.", "Smart Reorder", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGeneratePO_Click(object sender, EventArgs e)
        {
            // Commit any pending edits in the grid
            ultraGridMaster.UpdateData();

            List<SmartReorderItemModel> selectedItems = new List<SmartReorderItemModel>();

            // Only grab rows that are currently visible/filtered
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
            btnHideSelection.Text = ultraPanelSelection.Visible ? "Hide Selection" : "Show Selection";
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
            e.Row.Appearance.ForeColor = Color.Black;

            UltraGridCell alertCell = e.Row.Cells["Alert"];
            if (alertCell == null)
            {
                return;
            }

            alertCell.Appearance.BackColor = Color.Empty;
            alertCell.Appearance.ForeColor = Color.Black;

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
