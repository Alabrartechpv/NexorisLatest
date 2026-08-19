using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    /// <summary>
    /// Dedicated Item Master Reference Popup Form (Ctrl + D).
    /// Displays all items in a sleek Skyblue Pearl Glass theme with UltraGrid styled matching frmvendorpurchasereport.cs.
    /// Supports Filtering by Search Text, Category (txt_Category), Group (txt_Group), Hold Items, and Stock.
    /// </summary>
    public partial class FrmItemReferencePopup : Form
    {
        private static FrmItemReferencePopup _currentInstance = null;

        private readonly Color pageBack = Color.FromArgb(232, 246, 255);
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color accent = Color.FromArgb(42, 121, 232);
        private readonly Color skyBlueOutline = Color.FromArgb(102, 190, 255);
        private readonly Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
        private readonly Color gridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private readonly Color gridSelectedBlue = Color.FromArgb(126, 126, 245);
        private readonly Color gridRowLine = Color.FromArgb(197, 217, 241);
        private readonly Color gridAltRow = Color.FromArgb(246, 250, 255);
        private readonly Color gridFooterBorder = Color.FromArgb(144, 181, 223);

        private DataTable fullItemTable = new DataTable();
        private Dictionary<string, Label> footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);

        public FrmItemReferencePopup()
        {
            InitializeComponent();

            this.KeyPreview = true;

            // Suppress black dashed focus rectangle on selected rows/cells
            gridReport.DrawFilter = new NoFocusRectDrawFilter();

            // Handle resize & layout events for grid footer sync
            gridReport.Resize += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();

            // Wire filtering controls
            txtSearch.TextChanged += (s, e) => ApplyItemFilter();
            cmbCategory.SelectedIndexChanged += (s, e) => ApplyItemFilter();
            cmbGroup.SelectedIndexChanged += (s, e) => ApplyItemFilter();
            cmbHoldItems.SelectedIndexChanged += (s, e) => ApplyItemFilter();
            cmbStockFilter.SelectedIndexChanged += (s, e) => ApplyItemFilter();
            btnRefresh.Click += (s, e) => LoadAllItems();
            btnClose.Click += (s, e) => this.Close();

            ApplyThemeStyles();
        }

        /// <summary>
        /// Global activator: Shows or brings to front the dedicated Item Reference popup
        /// </summary>
        public static void ShowPopup(Form parentForm = null)
        {
            try
            {
                if (_currentInstance == null || _currentInstance.IsDisposed)
                {
                    _currentInstance = new FrmItemReferencePopup();
                    if (parentForm != null)
                    {
                        _currentInstance.Owner = parentForm;
                        _currentInstance.StartPosition = FormStartPosition.CenterParent;
                    }
                    _currentInstance.Show();
                }
                else
                {
                    if (_currentInstance.WindowState == FormWindowState.Minimized)
                        _currentInstance.WindowState = FormWindowState.Normal;
                    _currentInstance.BringToFront();
                    _currentInstance.Focus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing FrmItemReferencePopup: {ex.Message}");
            }
        }

        private void ApplyThemeStyles()
        {
            this.Text = "Items Master Reference (Ctrl + D)";
            this.BackColor = pageBack;
            this.Font = new Font("Segoe UI", 9.5F);

            // Style top buttons
            StyleButton(btnRefresh, false);
            StyleButton(btnClose, true);

            // Populate Hold Items combo
            cmbHoldItems.Items.Clear();
            cmbHoldItems.Items.Add("All Items");
            cmbHoldItems.Items.Add("Active / Normal");
            cmbHoldItems.Items.Add("On Hold");
            cmbHoldItems.SelectedIndex = 0;

            // Populate Stock Filter combo
            cmbStockFilter.Items.Clear();
            cmbStockFilter.Items.Add("All Stock");
            cmbStockFilter.Items.Add("Available (>0)");
            cmbStockFilter.Items.Add("Out of Stock (=0)");
            cmbStockFilter.SelectedIndex = 0;

            // Footer styling matching frmvendorpurchasereport.cs
            ultraPanelGridFooter.Appearance.BackColor = gridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = gridHeaderBlueDark;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.Vertical;
            ultraPanelGridFooter.Appearance.BorderColor = gridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleGrid();
        }

        private void StyleButton(Button button, bool primary)
        {
            if (button == null) return;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            button.ForeColor = primary ? Color.White : navy;
            button.BackColor = primary ? accent : Color.FromArgb(236, 246, 255);
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = primary ? accent : skyBlueOutline;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.MouseOverBackColor = primary ? accent : Color.FromArgb(225, 244, 255);
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(31, 96, 205) : Color.FromArgb(210, 235, 252);
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

            layout.Override.ActiveAppearancesEnabled = DefaultableBoolean.True;

            // GroupByBox styled exactly like frmvendorpurchasereport.cs
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = gridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = gridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = gridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

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
            layout.Override.RowSelectorWidth = 24;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.AllowRowFiltering = DefaultableBoolean.False;

            layout.Override.RowSelectorAppearance.BackColor = gridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = gridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.FontData.Name = "Segoe UI";
            layout.Override.RowSelectorAppearance.FontData.SizeInPoints = 9.5F;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Larger Headers
            layout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5F;

            // Rows & Larger Text
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = gridAltRow;
            layout.Override.RowAppearance.BorderColor = gridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = gridRowLine;

            // Active & Selected Row Appearance (No black dashed border line!)
            layout.Override.ActiveRowAppearance.BackColor = gridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = gridSelectedBlue;
            layout.Override.ActiveRowAppearance.BorderAlpha = Alpha.Opaque;

            layout.Override.SelectedRowAppearance.BackColor = gridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.BorderColor = gridSelectedBlue;
            layout.Override.SelectedRowAppearance.BorderAlpha = Alpha.Opaque;

            layout.Override.ActiveCellAppearance.BackColor = gridSelectedBlue;
            layout.Override.ActiveCellAppearance.ForeColor = Color.White;
            layout.Override.ActiveCellAppearance.BorderColor = gridSelectedBlue;
            layout.Override.ActiveCellAppearance.BorderAlpha = Alpha.Transparent;

            layout.Override.CellAppearance.BorderColor = gridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Segoe UI";
            layout.Override.CellAppearance.FontData.SizeInPoints = 9.5F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;

            // Bigger row height
            layout.Override.MinRowHeight = 28;
            layout.Override.DefaultRowHeight = 28;
            layout.RowConnectorStyle = RowConnectorStyle.None;
            layout.AutoFitStyle = AutoFitStyle.None;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadCategoryAndGroupDropdowns();
            LoadAllItems();
            if (txtSearch != null) txtSearch.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (_currentInstance == this) _currentInstance = null;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D))
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void LoadCategoryAndGroupDropdowns()
        {
            try
            {
                // Populate Category Dropdown (matching txt_Category in frmItemMasterNew.cs)
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add("All Categories");
                try
                {
                    Dropdowns dropdownRepo = new Dropdowns();
                    var categoryGrid = dropdownRepo.getCategoryDDl("");
                    if (categoryGrid != null && categoryGrid.List != null)
                    {
                        foreach (var cat in categoryGrid.List.Where(c => !string.IsNullOrWhiteSpace(c.CategoryName)).OrderBy(c => c.CategoryName))
                        {
                            if (!cmbCategory.Items.Contains(cat.CategoryName.Trim()))
                                cmbCategory.Items.Add(cat.CategoryName.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading categories from DDL: {ex.Message}");
                }
                if (cmbCategory.Items.Count == 1)
                {
                    cmbCategory.Items.Add("General");
                }
                cmbCategory.SelectedIndex = 0;

                // Populate Group Dropdown (matching txt_Group in frmItemMasterNew.cs)
                cmbGroup.Items.Clear();
                cmbGroup.Items.Add("All Groups");
                try
                {
                    Dropdowns dropdownRepo = new Dropdowns();
                    var groupGrid = dropdownRepo.getGroupDDl();
                    if (groupGrid != null && groupGrid.List != null)
                    {
                        foreach (var grp in groupGrid.List.Where(g => !string.IsNullOrWhiteSpace(g.GroupName)).OrderBy(g => g.GroupName))
                        {
                            if (!cmbGroup.Items.Contains(grp.GroupName.Trim()))
                                cmbGroup.Items.Add(grp.GroupName.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading groups from DDL: {ex.Message}");
                }
                if (cmbGroup.Items.Count == 1)
                {
                    cmbGroup.Items.Add("General");
                }
                cmbGroup.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing Category and Group dropdowns: {ex.Message}");
            }
        }

        private void LoadAllItems()
        {
            try
            {
                DataTable dt = new DataTable();
                BaseRepostitory baseRepo = new BaseRepostitory();
                var conn = baseRepo.DataConnection;
                if (conn.State != ConnectionState.Open) conn.Open();

                // 1. Primary Attempt: Query using stored procedure POS_ItemDetalisDDL
                try
                {
                    using (SqlCommand cmd = new SqlCommand("POS_ItemDetalisDDL", (SqlConnection)conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                        cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                        cmd.Parameters.AddWithValue("@Barcode", DBNull.Value);
                        cmd.Parameters.AddWithValue("@ItemName", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Operation", "GETALL");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Primary POS_ItemDetalisDDL failed: {ex.Message}");
                }

                // 2. Fallback Attempt: Direct SQL Query against ItemMaster and ItemMasterPriceSettings with explicit JOINs for Category, Group, and Brand
                if (dt == null || dt.Rows.Count == 0)
                {
                    dt = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.ItemMaster', 'U') IS NOT NULL
BEGIN
    SELECT 
        i.ItemId,
        ISNULL(i.ItemNo, '') AS ItemNo,
        ISNULL(p.BarCode, ISNULL(i.BarCode, '')) AS [Barcode],
        ISNULL(i.Description, ISNULL(i.ItemName, '')) AS [Item Name],
        ISNULL(c.Category, ISNULL(c.CategoryName, 'General')) AS [Category],
        ISNULL(g.Group_Name, ISNULL(g.[Group], ISNULL(g.GroupName, 'General'))) AS [Group],
        ISNULL(b.Brand, ISNULL(b.BrandName, '')) AS [Brand],
        ISNULL(p.Unit, '') AS [Unit],
        ISNULL(p.Cost, 0) AS [Cost],
        ISNULL(p.RetailPrice, 0) AS [Price],
        ISNULL(p.Stock, 0) AS [Stock],
        ISNULL(i.IsHold, 'N') AS [IsHold],
        i.CategoryId,
        i.GroupId,
        i.BrandId
    FROM dbo.ItemMaster i
    LEFT JOIN dbo.Category c ON i.CategoryId = c.CategoryId OR i.CategoryId = c.Id
    LEFT JOIN dbo.[Group] g ON i.GroupId = g.GroupId OR i.GroupId = g.Id
    LEFT JOIN dbo.Brand b ON i.BrandId = b.BrandId OR i.BrandId = b.Id
    LEFT JOIN dbo.ItemMasterPriceSettings p ON i.ItemId = p.ItemId AND (p.Packing = 1 OR p.IsBaseUnit = 'Y')
    ORDER BY i.Description;
END
ELSE IF OBJECT_ID('dbo.Items', 'U') IS NOT NULL
BEGIN
    SELECT 
        i.ItemId,
        ISNULL(i.ItemNo, '') AS ItemNo,
        ISNULL(i.BarCode, '') AS [Barcode],
        ISNULL(i.Description, i.ItemName) AS [Item Name],
        'General' AS [Category],
        'General' AS [Group],
        '' AS [Brand],
        '' AS [Unit],
        ISNULL(i.Cost, 0) AS [Cost],
        ISNULL(i.RetailPrice, 0) AS [Price],
        ISNULL(i.Stock, 0) AS [Stock],
        'N' AS [IsHold]
    FROM dbo.Items i
    ORDER BY i.Description;
END", (SqlConnection)conn))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                // Standardize Table Column Names
                StandardizeItemTableColumns(dt);

                // Enrich missing Category, Group, Brand names using database dictionaries
                EnrichCategoryGroupBrand(dt, (SqlConnection)conn);

                fullItemTable = dt;

                // Sync category and group list with loaded data
                UpdateDropdownsFromLoadedTable();

                ApplyItemFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading items for popup: {ex.Message}");
            }
        }

        private void EnrichCategoryGroupBrand(DataTable dt, SqlConnection conn)
        {
            if (dt == null || dt.Rows.Count == 0) return;

            try
            {
                // 1. Build Category lookup dictionary
                Dictionary<int, string> categoryDict = new Dictionary<int, string>();
                using (SqlCommand cmd = new SqlCommand(@"
                    IF OBJECT_ID('dbo.Category', 'U') IS NOT NULL
                    BEGIN
                        SELECT CategoryId AS Id, Category AS Name FROM dbo.Category WHERE Category IS NOT NULL AND LEN(RTRIM(Category)) > 0
                        UNION
                        SELECT Id, CategoryName AS Name FROM dbo.Category WHERE CategoryName IS NOT NULL AND LEN(RTRIM(CategoryName)) > 0
                    END", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0;
                            string name = reader["Name"]?.ToString()?.Trim();
                            if (id > 0 && !string.IsNullOrEmpty(name)) categoryDict[id] = name;
                        }
                    }
                }

                // 2. Build Group lookup dictionary
                Dictionary<int, string> groupDict = new Dictionary<int, string>();
                using (SqlCommand cmd = new SqlCommand(@"
                    IF OBJECT_ID('dbo.Group', 'U') IS NOT NULL
                    BEGIN
                        SELECT GroupId AS Id, Group_Name AS Name FROM dbo.[Group] WHERE Group_Name IS NOT NULL AND LEN(RTRIM(Group_Name)) > 0
                        UNION
                        SELECT GroupId AS Id, [Group] AS Name FROM dbo.[Group] WHERE [Group] IS NOT NULL AND LEN(RTRIM([Group])) > 0
                        UNION
                        SELECT Id, GroupName AS Name FROM dbo.[Group] WHERE GroupName IS NOT NULL AND LEN(RTRIM(GroupName)) > 0
                    END", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0;
                            string name = reader["Name"]?.ToString()?.Trim();
                            if (id > 0 && !string.IsNullOrEmpty(name)) groupDict[id] = name;
                        }
                    }
                }

                // 3. Build Brand lookup dictionary
                Dictionary<int, string> brandDict = new Dictionary<int, string>();
                using (SqlCommand cmd = new SqlCommand(@"
                    IF OBJECT_ID('dbo.Brand', 'U') IS NOT NULL
                    BEGIN
                        SELECT BrandId AS Id, Brand AS Name FROM dbo.Brand WHERE Brand IS NOT NULL AND LEN(RTRIM(Brand)) > 0
                        UNION
                        SELECT Id, BrandName AS Name FROM dbo.Brand WHERE BrandName IS NOT NULL AND LEN(RTRIM(BrandName)) > 0
                    END", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0;
                            string name = reader["Name"]?.ToString()?.Trim();
                            if (id > 0 && !string.IsNullOrEmpty(name)) brandDict[id] = name;
                        }
                    }
                }

                // 4. Fill in missing Category, Group, Brand strings for every row in dt
                foreach (DataRow row in dt.Rows)
                {
                    // Category
                    string catVal = dt.Columns.Contains("Category") ? row["Category"]?.ToString()?.Trim() : "";
                    if (string.IsNullOrEmpty(catVal) || int.TryParse(catVal, out _))
                    {
                        int catId = 0;
                        if (dt.Columns.Contains("CategoryId") && row["CategoryId"] != DBNull.Value)
                            int.TryParse(row["CategoryId"].ToString(), out catId);

                        if (catId > 0 && categoryDict.TryGetValue(catId, out string catName))
                            row["Category"] = catName;
                    }

                    // Group
                    string grpVal = dt.Columns.Contains("Group") ? row["Group"]?.ToString()?.Trim() : "";
                    if (string.IsNullOrEmpty(grpVal) || int.TryParse(grpVal, out _))
                    {
                        int grpId = 0;
                        if (dt.Columns.Contains("GroupId") && row["GroupId"] != DBNull.Value)
                            int.TryParse(row["GroupId"].ToString(), out grpId);

                        if (grpId > 0 && groupDict.TryGetValue(grpId, out string grpName))
                            row["Group"] = grpName;
                    }

                    // Brand
                    string brdVal = dt.Columns.Contains("Brand") ? row["Brand"]?.ToString()?.Trim() : "";
                    if (string.IsNullOrEmpty(brdVal) || int.TryParse(brdVal, out _))
                    {
                        int brdId = 0;
                        if (dt.Columns.Contains("BrandId") && row["BrandId"] != DBNull.Value)
                            int.TryParse(row["BrandId"].ToString(), out brdId);

                        if (brdId > 0 && brandDict.TryGetValue(brdId, out string brdName))
                            row["Brand"] = brdName;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnrichCategoryGroupBrand warning: {ex.Message}");
            }
        }

        private void StandardizeItemTableColumns(DataTable dt)
        {
            if (dt == null) return;

            // Map common database column aliases to standard UI names
            MapColumnName(dt, "ItemDescription", "Item Name");
            MapColumnName(dt, "Description", "Item Name");
            MapColumnName(dt, "ItemName", "Item Name");
            MapColumnName(dt, "BarCode", "Barcode");
            MapColumnName(dt, "RetailPrice", "Price");
            MapColumnName(dt, "SellingPrice", "Price");
            MapColumnName(dt, "CategoryName", "Category");
            MapColumnName(dt, "Cat_Name", "Category");
            MapColumnName(dt, "ItemCategory", "Category");
            MapColumnName(dt, "GroupName", "Group");
            MapColumnName(dt, "Group_Name", "Group");
            MapColumnName(dt, "Grp_Name", "Group");
            MapColumnName(dt, "ItemGroup", "Group");
            MapColumnName(dt, "BrandName", "Brand");
            MapColumnName(dt, "ItemBrand", "Brand");

            // Ensure required columns exist
            if (!dt.Columns.Contains("ItemNo")) dt.Columns.Add("ItemNo", typeof(string));
            if (!dt.Columns.Contains("Barcode")) dt.Columns.Add("Barcode", typeof(string));
            if (!dt.Columns.Contains("Item Name")) dt.Columns.Add("Item Name", typeof(string));
            if (!dt.Columns.Contains("Category")) dt.Columns.Add("Category", typeof(string));
            if (!dt.Columns.Contains("Group")) dt.Columns.Add("Group", typeof(string));
            if (!dt.Columns.Contains("Brand")) dt.Columns.Add("Brand", typeof(string));
            if (!dt.Columns.Contains("Unit")) dt.Columns.Add("Unit", typeof(string));
            if (!dt.Columns.Contains("Cost")) dt.Columns.Add("Cost", typeof(decimal));
            if (!dt.Columns.Contains("Price")) dt.Columns.Add("Price", typeof(decimal));
            if (!dt.Columns.Contains("Stock")) dt.Columns.Add("Stock", typeof(decimal));
            if (!dt.Columns.Contains("IsHold")) dt.Columns.Add("IsHold", typeof(string));
        }

        private void MapColumnName(DataTable dt, string oldName, string newName)
        {
            if (dt.Columns.Contains(oldName) && !dt.Columns.Contains(newName))
            {
                dt.Columns[oldName].ColumnName = newName;
            }
        }

        private void UpdateDropdownsFromLoadedTable()
        {
            if (fullItemTable == null) return;

            try
            {
                // Ensure distinct categories from data are present in cmbCategory
                if (fullItemTable.Columns.Contains("Category"))
                {
                    var categories = fullItemTable.AsEnumerable()
                        .Select(r => r["Category"]?.ToString()?.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(c => c);

                    foreach (var cat in categories)
                    {
                        if (!cmbCategory.Items.Contains(cat))
                            cmbCategory.Items.Add(cat);
                    }
                }

                // Ensure distinct groups from data are present in cmbGroup
                if (fullItemTable.Columns.Contains("Group"))
                {
                    var groups = fullItemTable.AsEnumerable()
                        .Select(r => r["Group"]?.ToString()?.Trim())
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(g => g);

                    foreach (var grp in groups)
                    {
                        if (!cmbGroup.Items.Contains(grp))
                            cmbGroup.Items.Add(grp);
                    }
                }
            }
            catch { }
        }

        private void ApplyItemFilter()
        {
            if (fullItemTable == null) return;

            try
            {
                string search = txtSearch.Text?.Trim().ToLower() ?? "";
                string selectedCat = cmbCategory.SelectedItem?.ToString() ?? "All Categories";
                string selectedGrp = cmbGroup.SelectedItem?.ToString() ?? "All Groups";
                string holdMode = cmbHoldItems.SelectedItem?.ToString() ?? "All Items";
                string stockMode = cmbStockFilter.SelectedItem?.ToString() ?? "All Stock";

                var rows = fullItemTable.AsEnumerable().Where(r =>
                {
                    string itemNo = r["ItemNo"]?.ToString()?.ToLower() ?? "";
                    string barcode = r["Barcode"]?.ToString()?.ToLower() ?? "";
                    string itemName = r["Item Name"]?.ToString()?.ToLower() ?? "";
                    string category = r["Category"]?.ToString() ?? "";
                    string group = r["Group"]?.ToString() ?? "";
                    string isHoldVal = r["IsHold"]?.ToString()?.Trim().ToUpper() ?? "N";
                    decimal stock = r["Stock"] != DBNull.Value ? Convert.ToDecimal(r["Stock"]) : 0m;

                    bool matchSearch = string.IsNullOrEmpty(search) ||
                                       itemNo.Contains(search) ||
                                       barcode.Contains(search) ||
                                       itemName.Contains(search);

                    bool matchCategory = selectedCat == "All Categories" ||
                                         string.Equals(category.Trim(), selectedCat.Trim(), StringComparison.OrdinalIgnoreCase);

                    bool matchGroup = selectedGrp == "All Groups" ||
                                      string.Equals(group.Trim(), selectedGrp.Trim(), StringComparison.OrdinalIgnoreCase);

                    bool matchHold = true;
                    if (holdMode == "Active / Normal") matchHold = (isHoldVal == "N" || isHoldVal == "0" || isHoldVal == "FALSE" || isHoldVal == "");
                    else if (holdMode == "On Hold") matchHold = (isHoldVal == "Y" || isHoldVal == "1" || isHoldVal == "TRUE" || isHoldVal == "HOLD");

                    bool matchStock = true;
                    if (stockMode == "Available (>0)") matchStock = stock > 0;
                    else if (stockMode == "Out of Stock (=0)") matchStock = stock <= 0;

                    return matchSearch && matchCategory && matchGroup && matchHold && matchStock;
                });

                DataTable filteredTable = fullItemTable.Clone();
                foreach (var row in rows)
                {
                    filteredTable.ImportRow(row);
                }

                gridReport.DataSource = filteredTable;
                ConfigureGridColumns();
                CreateFooterCells();
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error filtering items: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            if (band.Columns.Exists("ItemId")) band.Columns["ItemId"].Hidden = true;
            if (band.Columns.Exists("CategoryId")) band.Columns["CategoryId"].Hidden = true;
            if (band.Columns.Exists("GroupId")) band.Columns["GroupId"].Hidden = true;
            if (band.Columns.Exists("BrandId")) band.Columns["BrandId"].Hidden = true;

            if (band.Columns.Exists("ItemNo"))
            {
                band.Columns["ItemNo"].Header.Caption = "Item No";
                band.Columns["ItemNo"].Width = 95;
                band.Columns["ItemNo"].Header.VisiblePosition = 0;
            }
            if (band.Columns.Exists("Barcode"))
            {
                band.Columns["Barcode"].Header.Caption = "Barcode";
                band.Columns["Barcode"].Width = 135;
                band.Columns["Barcode"].Header.VisiblePosition = 1;
            }
            if (band.Columns.Exists("Item Name"))
            {
                band.Columns["Item Name"].Header.Caption = "Item Description / Name";
                band.Columns["Item Name"].Width = 280;
                band.Columns["Item Name"].Header.VisiblePosition = 2;
            }
            if (band.Columns.Exists("Category"))
            {
                band.Columns["Category"].Header.Caption = "Category";
                band.Columns["Category"].Width = 130;
                band.Columns["Category"].Header.VisiblePosition = 3;
            }
            if (band.Columns.Exists("Group"))
            {
                band.Columns["Group"].Header.Caption = "Group";
                band.Columns["Group"].Width = 120;
                band.Columns["Group"].Header.VisiblePosition = 4;
            }
            if (band.Columns.Exists("Brand"))
            {
                band.Columns["Brand"].Header.Caption = "Brand";
                band.Columns["Brand"].Width = 110;
                band.Columns["Brand"].Header.VisiblePosition = 5;
            }
            if (band.Columns.Exists("Unit"))
            {
                band.Columns["Unit"].Header.Caption = "Unit";
                band.Columns["Unit"].Width = 75;
                band.Columns["Unit"].Header.VisiblePosition = 6;
            }
            if (band.Columns.Exists("Cost"))
            {
                band.Columns["Cost"].Header.Caption = "Cost";
                band.Columns["Cost"].Width = 95;
                band.Columns["Cost"].Format = "N2";
                band.Columns["Cost"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Cost"].Header.VisiblePosition = 7;
            }
            if (band.Columns.Exists("Price"))
            {
                band.Columns["Price"].Header.Caption = "Selling Price";
                band.Columns["Price"].Width = 110;
                band.Columns["Price"].Format = "N2";
                band.Columns["Price"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Price"].Header.VisiblePosition = 8;
            }
            if (band.Columns.Exists("Stock"))
            {
                band.Columns["Stock"].Header.Caption = "Stock Qty";
                band.Columns["Stock"].Width = 95;
                band.Columns["Stock"].Format = "N2";
                band.Columns["Stock"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Stock"].Header.VisiblePosition = 9;
            }
            if (band.Columns.Exists("IsHold"))
            {
                band.Columns["IsHold"].Header.Caption = "Hold";
                band.Columns["IsHold"].Width = 70;
                band.Columns["IsHold"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["IsHold"].Header.VisiblePosition = 10;
            }
        }

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden) continue;

                Label footerLabel = new Label();
                footerLabel.Name = "footer_" + column.Key;
                footerLabel.Text = string.Empty;
                footerLabel.TextAlign = (column.Key == "Cost" || column.Key == "Price" || column.Key == "Stock")
                    ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
                footerLabel.BackColor = gridHeaderBlue;
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.ForeColor = Color.White;
                footerLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                footerLabels[column.Key] = footerLabel;

                xOffset += column.Width;
            }
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraPanelGridFooter == null || gridReport == null || gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden) continue;

                if (footerLabels.TryGetValue(column.Key, out Label lbl))
                {
                    lbl.Left = xOffset;
                    lbl.Width = column.Width;
                }

                xOffset += column.Width;
            }
        }

        private void UpdateFooterValues()
        {
            if (gridReport == null || gridReport.Rows == null) return;

            try
            {
                int totalCount = gridReport.Rows.Count;
                decimal totalStock = 0m;

                foreach (UltraGridRow row in gridReport.Rows)
                {
                    if (row.Cells.Exists("Stock") && row.Cells["Stock"].Value != DBNull.Value)
                    {
                        totalStock += Convert.ToDecimal(row.Cells["Stock"].Value);
                    }
                }

                if (footerLabels.TryGetValue("Item Name", out Label lblName))
                {
                    lblName.Text = $"Total Items: {totalCount}";
                }
                if (footerLabels.TryGetValue("Stock", out Label lblStock))
                {
                    lblStock.Text = totalStock.ToString("N2");
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Custom DrawFilter for Infragistics UltraGrid that completely suppresses the black dashed focus rectangle
    /// </summary>
    public class NoFocusRectDrawFilter : IUIElementDrawFilter
    {
        public bool DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            return true; // Suppress focus rectangle drawing completely
        }

        public DrawPhase GetPhasesToFilter(ref UIElementDrawParams drawParams)
        {
            return DrawPhase.BeforeDrawFocus;
        }
    }
}
