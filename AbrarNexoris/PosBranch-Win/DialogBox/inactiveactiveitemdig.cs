using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using Repository;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class inactiveactiveitemdig : Form
    {
        private DataTable fullDataTable;

        public inactiveactiveitemdig()
        {
            InitializeComponent();
            this.Load += inactiveactiveitemdig_Load;
            this.Shown += inactiveactiveitemdig_Shown;

            if (textBoxsearch != null)
            {
                textBoxsearch.TextChanged += textBoxsearch_TextChanged;
            }

            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            WireClosePanels();
        }

        private void inactiveactiveitemdig_Load(object sender, EventArgs e)
        {
            InitializeSearchControls();
            LoadInactiveItems();
        }

        private void inactiveactiveitemdig_Shown(object sender, EventArgs e)
        {
            if (textBoxsearch != null)
            {
                textBoxsearch.Focus();
            }
        }

        private void InitializeSearchControls()
        {
            if (comboBox1 != null)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(new object[] { "Select all", "Barcode", "Item Name", "Status" });
                comboBox1.SelectedIndex = 0;
                comboBox1.SelectedIndexChanged += (s, e) => ApplyFilter();
            }

            if (comboBox2 != null)
            {
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(new object[] { "Item Name", "Barcode", "Status" });
                comboBox2.SelectedIndex = 0;
                comboBox2.SelectedIndexChanged += (s, e) => ApplyFilter();
            }
        }

        private void WireClosePanels()
        {
            if (ultraPanel6 != null)
            {
                ultraPanel6.Click += CloseDialog_Click;
                ultraPanel6.ClientArea.Click += CloseDialog_Click;
            }

            if (ultraPictureBox2 != null)
            {
                ultraPictureBox2.Click += CloseDialog_Click;
            }

            if (ultraPanel5 != null)
            {
                ultraPanel5.Click += CloseDialog_Click;
                ultraPanel5.ClientArea.Click += CloseDialog_Click;
            }

            if (ultraPictureBox1 != null)
            {
                ultraPictureBox1.Click += CloseDialog_Click;
            }
        }

        private void CloseDialog_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LoadInactiveItems()
        {
            try
            {
                fullDataTable = GetAllItems();
                ApplyStatusFilter(fullDataTable);
                textBox3.Text = fullDataTable.Rows.Count.ToString();
                ultraGrid1.DataSource = fullDataTable;
                ApplyFilter();
                this.Text = "Inactive Items";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading inactive items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                fullDataTable = new DataTable();
                ultraGrid1.DataSource = fullDataTable;
                textBox3.Text = "0";
            }
        }

        private DataTable GetAllItems()
        {
            using (BaseRepostitory repo = new BaseRepostitory())
            {
                SqlConnection connection = repo.DataConnection as SqlConnection;
                if (connection == null)
                {
                    return new DataTable();
                }

                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemDetalisDDL, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemName", "");
                    cmd.Parameters.AddWithValue("@Operation", "GETALL");

                    DataTable table = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(table);
                    }

                    return table;
                }
            }
        }

        private void ApplyStatusFilter(DataTable table)
        {
            if (table == null)
            {
                return;
            }

            Dropdowns dropdowns = new Dropdowns();
            dropdowns.ApplyItemStatuses(table);

            DataRow[] activeRows = table.AsEnumerable()
                .Where(row => string.Equals(
                    Dropdowns.NormalizeItemStatusName(row["ItemStatus"] == DBNull.Value ? string.Empty : row["ItemStatus"].ToString()),
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (DataRow row in activeRows)
            {
                table.Rows.Remove(row);
            }

            SortInactiveItems(table);
        }

        private void SortInactiveItems(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
            {
                return;
            }

            string sortColumn = table.Columns.Contains("Description") ? "Description ASC" :
                table.Columns.Contains("ItemName") ? "ItemName ASC" :
                table.Columns.Contains("ItemId") ? "ItemId DESC" : string.Empty;

            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return;
            }

            DataTable sortedTable = table.DefaultView.ToTable();
            DataView view = sortedTable.DefaultView;
            view.Sort = sortColumn;
            DataTable ordered = view.ToTable();

            table.Rows.Clear();
            foreach (DataRow row in ordered.Rows)
            {
                table.ImportRow(row);
            }
        }

        private void ApplyFilter()
        {
            if (fullDataTable == null)
            {
                return;
            }

            string searchText = textBoxsearch == null ? string.Empty : textBoxsearch.Text.Trim();
            DataView view = fullDataTable.DefaultView;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                view.RowFilter = string.Empty;
            }
            else
            {
                string escaped = searchText.Replace("'", "''");
                string filterOption = comboBox1 == null ? "Select all" : comboBox1.SelectedItem?.ToString() ?? "Select all";

                switch (filterOption)
                {
                    case "Barcode":
                        view.RowFilter = ColumnLikeFilter("BarCode", escaped);
                        break;
                    case "Item Name":
                        view.RowFilter = ColumnLikeFilter("Description", escaped);
                        break;
                    case "Status":
                        view.RowFilter = ColumnLikeFilter("ItemStatus", escaped);
                        break;
                    default:
                        view.RowFilter = string.Join(" OR ", new[]
                        {
                            ColumnLikeFilter("BarCode", escaped),
                            ColumnLikeFilter("Description", escaped),
                            ColumnLikeFilter("ItemStatus", escaped)
                        }.Where(filter => !string.IsNullOrWhiteSpace(filter)));
                        break;
                }
            }

            ultraGrid1.DataSource = view;
            textBox3.Text = view.Count.ToString();
        }

        private string ColumnLikeFilter(string columnName, string escapedSearchText)
        {
            if (fullDataTable == null || !fullDataTable.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            return $"CONVERT([{columnName}], 'System.String') LIKE '%{escapedSearchText}%'";
        }

        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            e.Layout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            e.Layout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            e.Layout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            e.Layout.Override.RowSelectorWidth = 20;
            e.Layout.Override.CellClickAction = CellClickAction.RowSelect;
            e.Layout.Override.AllowUpdate = DefaultableBoolean.False;
            e.Layout.Override.DefaultRowHeight = 30;
            e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;
            e.Layout.ViewStyleBand = ViewStyleBand.OutlookGroupBy;
            e.Layout.GroupByBox.Hidden = true;

            Color headerBlue = Color.FromArgb(0, 123, 255);
            e.Layout.Override.HeaderAppearance.BackColor = headerBlue;
            e.Layout.Override.HeaderAppearance.BackColor2 = headerBlue;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            if (e.Layout.Bands.Count == 0)
            {
                return;
            }

            foreach (UltraGridColumn column in e.Layout.Bands[0].Columns)
            {
                column.Hidden = true;
            }

            ShowColumn(e.Layout.Bands[0], "BarCode", "Barcode", 0, 120);
            ShowColumn(e.Layout.Bands[0], "Description", "Item Name", 1, 230);
            ShowColumn(e.Layout.Bands[0], "Unit", "Unit", 2, 75);
            ShowColumn(e.Layout.Bands[0], "Stock", "Stock", 3, 80, "N2", HAlign.Right);
            ShowColumn(e.Layout.Bands[0], "ItemStatus", "Status", 4, 130);
            ShowColumn(e.Layout.Bands[0], "StatusReason", "Reason", 5, 180);
            ShowColumn(e.Layout.Bands[0], "StatusDate", "Status Date", 6, 95, "dd/MM/yyyy", HAlign.Center);
        }

        private void ShowColumn(UltraGridBand band, string key, string caption, int position, int width, string format = null, HAlign alignment = HAlign.Left)
        {
            if (!band.Columns.Exists(key))
            {
                return;
            }

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = caption;
            column.Header.VisiblePosition = position;
            column.Width = width;
            column.CellAppearance.TextHAlign = alignment;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }
    }
}