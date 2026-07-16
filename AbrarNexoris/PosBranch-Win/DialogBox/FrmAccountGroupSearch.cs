using System;
using System.Data;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using Repository.Accounts;

namespace PosBranch_Win.DialogBox
{
    public partial class FrmAccountGroupSearch : Form
    {
        private AccountGroupRepository repo;
        private DataTable dtData;
        public int SelectedGroupId { get; private set; } = -1;

        public FrmAccountGroupSearch()
        {
            InitializeComponent();
            repo = new AccountGroupRepository();
            
            this.Load += FrmAccountGroupSearch_Load;
            this.ultraPanel6.Click += BtnClose_Click;
            this.ultraPictureBox2.Click += BtnClose_Click;
            this.label3.Click += BtnClose_Click;
            this.ultraPanel5.Click += BtnSelect_Click;
            this.ultraPictureBox1.Click += BtnSelect_Click;
            this.label5.Click += BtnSelect_Click;
            this.ultraGrid1.DoubleClickRow += UltraGridAccount_DoubleClickRow;
            this.ultraGrid1.KeyDown += UltraGridAccount_KeyDown;
            this.textBoxsearch.TextChanged += TxtSearch_TextChanged;
        }

        private void FrmAccountGroupSearch_Load(object sender, EventArgs e)
        {
            PopulateComboBoxes();
            LoadData();
            UpdateRecordCount();
            textBoxsearch.Focus();
            ApplyButtonHoverEffects();

            this.ultraPanel4.Click += BtnNewEdit_Click;
            this.ultraPictureBox3.Click += BtnNewEdit_Click;
            this.label4.Click += BtnNewEdit_Click;

            this.ultraPanel9.Click += BtnClearSearch_Click;
            this.ultraPictureBox4.Click += BtnClearSearch_Click;
        }

        private void PopulateComboBoxes()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            
            string[] columns = { "All", "ID", "Name", "Type", "Category" };
            comboBox1.Items.AddRange(columns);
            comboBox1.SelectedIndex = 0;

            string[] sortColumns = { "None", "ID", "Name", "Type", "Category" };
            comboBox2.Items.AddRange(sortColumns);
            comboBox2.SelectedIndex = 0;

            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TxtSearch_TextChanged(sender, e);
            textBoxsearch.Focus();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dtData == null || dtData.DefaultView == null) return;
            string sortBy = comboBox2.SelectedItem?.ToString();
            
            if (sortBy == "ID") dtData.DefaultView.Sort = "GroupID ASC";
            else if (sortBy == "Name") dtData.DefaultView.Sort = "GroupName ASC";
            else if (sortBy == "Type") dtData.DefaultView.Sort = "GroupType ASC";
            else if (sortBy == "Category") dtData.DefaultView.Sort = "AccountCategory ASC";
            else dtData.DefaultView.Sort = "";
        }

        private void UpdateRecordCount()
        {
            if (ultraGrid1.Rows != null)
            {
                int count = ultraGrid1.Rows.GetFilteredInNonGroupByRows().Length;
                label1.Text = $"Total Records : {count}";
            }
        }

        private void BtnNewEdit_Click(object sender, EventArgs e)
        {
            SelectedGroupId = 0;
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void BtnClearSearch_Click(object sender, EventArgs e)
        {
            textBoxsearch.Text = "";
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            textBoxsearch.Focus();
        }

        private void LoadData()
        {
            try
            {
                dtData = repo.GetAllAccountGroups();
                ultraGrid1.DataSource = dtData;
                FormatGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void FormatGrid()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            
            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            
            if (band.Columns.Exists("ParentGroupID")) band.Columns["ParentGroupID"].Hidden = true;
            if (band.Columns.Exists("BranchName")) band.Columns["BranchName"].Hidden = true;
            if (band.Columns.Exists("AccountGroupID")) band.Columns["AccountGroupID"].Hidden = true;
            
            if (band.Columns.Exists("GroupID")) band.Columns["GroupID"].Header.Caption = "ID";
            if (band.Columns.Exists("GroupName")) band.Columns["GroupName"].Header.Caption = "Name";
            if (band.Columns.Exists("GroupType")) band.Columns["GroupType"].Header.Caption = "Type";
            if (band.Columns.Exists("AccountCategory")) band.Columns["AccountCategory"].Header.Caption = "Category";
            if (band.Columns.Exists("Description")) band.Columns["Description"].Header.Caption = "Description";
            if (band.Columns.Exists("GroupUnder")) band.Columns["GroupUnder"].Header.Caption = "Group Under";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dtData == null) return;
            
            string filter = textBoxsearch.Text.Trim().Replace("'", "''");
            string searchBy = comboBox1.SelectedItem?.ToString() ?? "All";
            
            if (string.IsNullOrEmpty(filter))
            {
                dtData.DefaultView.RowFilter = "";
            }
            else
            {
                string filterExpression = "";
                if (searchBy == "ID")
                    filterExpression = $"Convert(GroupID, 'System.String') LIKE '%{filter}%'";
                else if (searchBy == "Name")
                    filterExpression = $"GroupName LIKE '%{filter}%'";
                else if (searchBy == "Type")
                    filterExpression = $"GroupType LIKE '%{filter}%'";
                else if (searchBy == "Category")
                    filterExpression = $"AccountCategory LIKE '%{filter}%'";
                else
                    filterExpression = $"GroupName LIKE '%{filter}%' OR Convert(GroupID, 'System.String') LIKE '%{filter}%' OR AccountCategory LIKE '%{filter}%' OR GroupType LIKE '%{filter}%'";
                    
                dtData.DefaultView.RowFilter = filterExpression;
            }
            UpdateRecordCount();
        }

        private void SelectCurrentRow()
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow)
            {
                if (ultraGrid1.ActiveRow.Cells.Exists("GroupID") && ultraGrid1.ActiveRow.Cells["GroupID"].Value != DBNull.Value)
                {
                    SelectedGroupId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["GroupID"].Value);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            SelectCurrentRow();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void UltraGridAccount_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCurrentRow();
        }

        private void UltraGridAccount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrentRow();
                e.Handled = true;
            }
        }

        private void ApplyButtonHoverEffects()
        {
            AttachHover(ultraPanel5, label5, ultraPictureBox1);
            AttachHover(ultraPanel6, label3, ultraPictureBox2);
            AttachHover(ultraPanel4, label4, ultraPictureBox3);
            AttachHover(ultraPanel3, null, ultraPictureBox5);
            AttachHover(ultraPanel7, null, ultraPictureBox6);
            AttachHover(ultraPanel9, null, ultraPictureBox4);
        }

        private void AttachHover(Infragistics.Win.Misc.UltraPanel panel, Label lbl, Infragistics.Win.UltraWinEditors.UltraPictureBox pic)
        {
            var defaultColor1 = System.Drawing.Color.FromArgb(0, 174, 239);
            var defaultColor2 = System.Drawing.Color.FromArgb(0, 116, 217);
            var hoverColor1 = System.Drawing.Color.FromArgb(0, 150, 220);
            var hoverColor2 = System.Drawing.Color.FromArgb(0, 100, 200);
            var clickColor1 = System.Drawing.Color.FromArgb(0, 116, 217);
            var clickColor2 = System.Drawing.Color.FromArgb(0, 174, 239);

            EventHandler mouseEnter = (s, e) => { 
                panel.Appearance.BackColor = hoverColor1;
                panel.Appearance.BackColor2 = hoverColor2;
                if (lbl != null) lbl.Cursor = Cursors.Hand;
                if (pic != null) pic.Cursor = Cursors.Hand;
                panel.Cursor = Cursors.Hand;
            };
            EventHandler mouseLeave = (s, e) => { 
                panel.Appearance.BackColor = defaultColor1;
                panel.Appearance.BackColor2 = defaultColor2;
                if (lbl != null) lbl.Cursor = Cursors.Default;
                if (pic != null) pic.Cursor = Cursors.Default;
                panel.Cursor = Cursors.Default;
            };
            MouseEventHandler mouseDown = (s, e) => {
                panel.Appearance.BackColor = clickColor1;
                panel.Appearance.BackColor2 = clickColor2;
            };
            MouseEventHandler mouseUp = (s, e) => {
                panel.Appearance.BackColor = hoverColor1;
                panel.Appearance.BackColor2 = hoverColor2;
            };

            Control[] controls = { panel, lbl, pic };
            foreach (var ctrl in controls)
            {
                if (ctrl != null)
                {
                    ctrl.MouseEnter += mouseEnter;
                    ctrl.MouseLeave += mouseLeave;
                    ctrl.MouseDown += mouseDown;
                    ctrl.MouseUp += mouseUp;
                }
            }
        }
    }
}
