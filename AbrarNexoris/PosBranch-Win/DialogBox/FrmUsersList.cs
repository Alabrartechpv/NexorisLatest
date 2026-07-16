using ModelClass.Master;
using Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmUsersList : Form
    {
        private readonly Dropdowns dropdowns = new Dropdowns();
        private List<UsersDDl> usersCache = new List<UsersDDl>();

        public int SelectedUserId { get; private set; }

        public FrmUsersList()
        {
            InitializeComponent();
            WireLegacyControls();
            Resize += FrmUsersList_Resize;
        }

        private void FrmUsersList_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            InitializeListControls();
            ApplyGridAppearance();
            LayoutUsersList();
            BindUsers();
            textBoxsearch.Focus();
        }

        private void FrmUsersList_Resize(object sender, EventArgs e)
        {
            LayoutUsersList();
            FormatGridColumns();
        }

        private void InitializeListControls()
        {
            Text = "Select User";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(760, 500);

            ultPanelPurchaseDisplay.Dock = DockStyle.Fill;

            lblSearch.Text = "Search";
            label2.Text = "Sort By";
            label5.Text = "OK";
            label3.Text = "Close";
            label4.Text = "Edit Selected";
            label1.Text = "Enter / double-click to select a user. Esc closes this list.";

            textBox3.ReadOnly = true;
            textBox3.TextAlign = HorizontalAlignment.Center;
            textBox3.TabStop = false;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "User Name", "User ID", "All Fields" });
            comboBox1.SelectedIndex = 0;

            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "Name A-Z", "Name Z-A", "User ID Asc", "User ID Desc" });
            comboBox2.SelectedIndex = 0;

            ultraPanel5.Cursor = Cursors.Hand;
            ultraPanel6.Cursor = Cursors.Hand;
            ultraPanel4.Cursor = Cursors.Hand;
            ultraPanel3.Cursor = Cursors.Hand;
            ultraPanel7.Cursor = Cursors.Hand;
            ultraPanel9.Cursor = Cursors.Hand;
        }

        private void LayoutUsersList()
        {
            if (ultPanelPurchaseDisplay == null)
                return;

            int width = ultPanelPurchaseDisplay.ClientArea.Width;
            int height = ultPanelPurchaseDisplay.ClientArea.Height;
            if (width <= 0 || height <= 0)
                return;

            int margin = 12;
            int actionWidth = 62;
            int topHeight = 42;
            int searchTop = 52;
            int searchHeight = 26;
            int gridTop = 86;
            int bottomHeight = 82;
            int gridWidth = width - (margin * 3) - actionWidth;
            int gridHeight = Math.Max(230, height - gridTop - bottomHeight - 40);
            int actionLeft = margin + gridWidth + margin;

            ultraPanel2.SetBounds(0, 0, width, topHeight);
            lblSearch.SetBounds(12, 12, 58, 20);
            comboBox1.SetBounds(72, 4, Math.Min(220, Math.Max(160, width / 4)), 32);
            label2.SetBounds(comboBox1.Right + 18, 12, 62, 20);
            comboBox2.SetBounds(label2.Right + 8, 4, Math.Min(230, Math.Max(170, width / 4)), 32);
            ultraPanel9.SetBounds(width - 76, 5, 64, 30);

            textBoxsearch.SetBounds(margin, searchTop, gridWidth, searchHeight);
            textBox3.SetBounds(actionLeft, searchTop, actionWidth, searchHeight);

            ultraGrid1.SetBounds(margin, gridTop, gridWidth, gridHeight);
            ultraPanel3.SetBounds(actionLeft, gridTop, actionWidth, 54);
            ultraPanel7.SetBounds(actionLeft, gridTop + 64, actionWidth, 54);

            ultraPanel8.SetBounds(margin, ultraGrid1.Bottom + 4, gridWidth, 24);
            label1.SetBounds(margin, ultraPanel8.Bottom + 8, gridWidth, 20);

            int buttonTop = height - 58;
            ultraPanel5.SetBounds(margin, buttonTop, 116, 51);
            ultraPanel6.SetBounds(ultraPanel5.Right + 12, buttonTop, 116, 51);
            ultraPanel4.SetBounds(ultraPanel6.Right + 12, buttonTop, 184, 51);
        }

        private void WireLegacyControls()
        {
            comboBox1.SelectedIndexChanged += FilterControl_Changed;
            comboBox2.SelectedIndexChanged += FilterControl_Changed;

            ultraPanel5.Click += SelectUser_Click;
            label5.Click += SelectUser_Click;
            ultraPictureBox1.Click += SelectUser_Click;

            ultraPanel4.Click += SelectUser_Click;
            label4.Click += SelectUser_Click;
            ultraPictureBox3.Click += SelectUser_Click;

            ultraPanel6.Click += Close_Click;
            label3.Click += Close_Click;
            ultraPictureBox2.Click += Close_Click;

            ultraPanel3.Click += Refresh_Click;
            ultraPictureBox5.Click += Refresh_Click;

            ultraPanel7.Click += ClearSearch_Click;
            ultraPictureBox6.Click += ClearSearch_Click;

            ultraPanel9.Click += SearchNow_Click;
            ultraPictureBox4.Click += SearchNow_Click;
        }

        private void BindUsers()
        {
            UserDDlGrid users = dropdowns.getUsersDDl();
            usersCache = (users.List ?? Enumerable.Empty<UsersDDl>()).ToList();
            ApplyFiltersAndSort();
        }

        private void SearchUsers()
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            string searchTerm = textBoxsearch.Text.Trim();
            string searchBy = comboBox1.SelectedItem as string ?? "User Name";
            string sortBy = comboBox2.SelectedItem as string ?? "Name A-Z";

            IEnumerable<UsersDDl> filteredUsers = usersCache;

            if (searchTerm.Length > 0)
            {
                filteredUsers = filteredUsers.Where(user =>
                    searchBy == "User ID"
                        ? user.UserID.ToString().Contains(searchTerm)
                        : searchBy == "All Fields"
                            ? user.UserID.ToString().Contains(searchTerm)
                                || (user.UserName ?? string.Empty).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                            : (user.UserName ?? string.Empty).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            switch (sortBy)
            {
                case "Name Z-A":
                    filteredUsers = filteredUsers.OrderByDescending(user => user.UserName ?? string.Empty);
                    break;
                case "User ID Asc":
                    filteredUsers = filteredUsers.OrderBy(user => user.UserID);
                    break;
                case "User ID Desc":
                    filteredUsers = filteredUsers.OrderByDescending(user => user.UserID);
                    break;
                default:
                    filteredUsers = filteredUsers.OrderBy(user => user.UserName ?? string.Empty);
                    break;
            }

            BindGrid(filteredUsers.ToList());
        }

        private void BindGrid(List<UsersDDl> users)
        {
            ultraGrid1.DataSource = users;
            textBox3.Text = users.Count.ToString();
            SelectFirstRow();
            FormatGridColumns();
        }

        private void SelectFirstRow()
        {
            if (ultraGrid1.Rows.Count == 0)
                return;

            ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
            ultraGrid1.Rows[0].Selected = true;
        }

        private void ApplyGridAppearance()
        {
            ultraGrid1.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
            ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
            ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
            ultraGrid1.DisplayLayout.Override.CellPadding = 5;
            ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortMulti;
            ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.Fixed;
            ultraGrid1.DisplayLayout.Override.RowSizingArea = Infragistics.Win.UltraWinGrid.RowSizingArea.EntireRow;
            ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
            ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
            ultraGrid1.DisplayLayout.ViewStyleBand = Infragistics.Win.UltraWinGrid.ViewStyleBand.OutlookGroupBy;

            Infragistics.Win.Appearance headerAppearance = new Infragistics.Win.Appearance();
            headerAppearance.BackColor = Color.FromArgb(0, 102, 184);
            headerAppearance.BackColor2 = Color.FromArgb(0, 122, 204);
            headerAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            headerAppearance.FontData.BoldAsString = "True";
            headerAppearance.FontData.Name = "Segoe UI";
            headerAppearance.FontData.SizeInPoints = 9F;
            headerAppearance.ForeColor = Color.White;
            headerAppearance.TextHAlignAsString = "Center";
            headerAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
            ultraGrid1.DisplayLayout.Override.HeaderAppearance = headerAppearance;

            Infragistics.Win.Appearance rowAppearance = new Infragistics.Win.Appearance();
            rowAppearance.BackColor = Color.White;
            rowAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            rowAppearance.FontData.Name = "Segoe UI";
            rowAppearance.FontData.SizeInPoints = 9F;
            ultraGrid1.DisplayLayout.Override.RowAppearance = rowAppearance;

            Infragistics.Win.Appearance alternateRowAppearance = new Infragistics.Win.Appearance();
            alternateRowAppearance.BackColor = Color.FromArgb(248, 250, 252);
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance = alternateRowAppearance;

            Infragistics.Win.Appearance selectedRowAppearance = new Infragistics.Win.Appearance();
            selectedRowAppearance.BackColor = Color.FromArgb(0, 122, 204);
            selectedRowAppearance.ForeColor = Color.White;
            selectedRowAppearance.FontData.BoldAsString = "True";
            ultraGrid1.DisplayLayout.Override.SelectedRowAppearance = selectedRowAppearance;
            ultraGrid1.DisplayLayout.Override.ActiveRowAppearance = selectedRowAppearance;
        }

        private void FormatGridColumns()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                return;

            Infragistics.Win.UltraWinGrid.UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            band.ColHeadersVisible = true;
            if (band.Columns.Exists("UserID"))
            {
                band.Columns["UserID"].Header.Caption = "ID";
                band.Columns["UserID"].Width = 90;
                band.Columns["UserID"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
            }

            if (band.Columns.Exists("UserName"))
            {
                band.Columns["UserName"].Header.Caption = "User Name";
                band.Columns["UserName"].Width = Math.Max(420, ultraGrid1.Width - 145);
            }
        }

        private void SelectActiveUser()
        {
            if (ultraGrid1.ActiveRow == null || !ultraGrid1.ActiveRow.Cells.Exists("UserID"))
                return;

            object value = ultraGrid1.ActiveRow.Cells["UserID"].Value;
            if (value == null || value == DBNull.Value)
                return;

            SelectedUserId = Convert.ToInt32(value);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            SearchUsers();
        }

        private void textBoxsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Rows[0].Selected = true;
                ultraGrid1.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                SelectActiveUser();
                e.Handled = true;
            }
        }

        private void FilterControl_Changed(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void SelectUser_Click(object sender, EventArgs e)
        {
            SelectActiveUser();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            BindUsers();
            textBoxsearch.Focus();
        }

        private void ClearSearch_Click(object sender, EventArgs e)
        {
            textBoxsearch.Clear();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            ApplyFiltersAndSort();
            textBoxsearch.Focus();
        }

        private void SearchNow_Click(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
            textBoxsearch.Focus();
        }

        private void ultraGrid1_DoubleClickRow(object sender, Infragistics.Win.UltraWinGrid.DoubleClickRowEventArgs e)
        {
            SelectActiveUser();
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectActiveUser();
                e.Handled = true;
            }
        }

        private void FrmUsersList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
