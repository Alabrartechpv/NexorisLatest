using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using Repository;
using System;
using System.Collections.Generic;
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
                SortInactiveItems(fullDataTable);
                if (textBox3 != null)
                {
                    textBox3.Text = fullDataTable.Rows.Count.ToString();
                }
                ultraGrid1.DataSource = fullDataTable;
                ApplyFilter();
                this.Text = "Inactive Items";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading inactive items: " + ex.Message);
                fullDataTable = new DataTable();
                StandardizeColumns(fullDataTable);
                ultraGrid1.DataSource = fullDataTable;
                if (textBox3 != null)
                {
                    textBox3.Text = "0";
                }
            }
        }

        private DataTable GetAllItems()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ItemId", typeof(int));
            table.Columns.Add("ItemNo", typeof(string));
            table.Columns.Add("BarCode", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Unit", typeof(string));
            table.Columns.Add("Stock", typeof(decimal));
            table.Columns.Add("ItemStatus", typeof(string));
            table.Columns.Add("StatusReason", typeof(string));
            table.Columns.Add("StatusDate", typeof(DateTime));

            try
            {
                Repository.ReportRepository.InactiveItemsReportRepository repo = new Repository.ReportRepository.InactiveItemsReportRepository();
                var reportData = repo.GetInactiveItemsReport(new ModelClass.Report.InactiveItemsReportFilter());

                if (reportData != null)
                {
                    foreach (var item in reportData)
                    {
                        table.Rows.Add(
                            item.ItemId,
                            item.ItemNo ?? "",
                            item.Barcode ?? "",
                            item.ItemName ?? "",
                            item.Unit ?? "",
                            item.Stock,
                            string.IsNullOrWhiteSpace(item.ItemStatus) ? "Inactive" : item.ItemStatus,
                            item.StatusReason ?? "",
                            item.StatusDate ?? item.CreatedOn ?? DateTime.Now
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllItems error: {ex.Message}");
            }

            StandardizeColumns(table);
            return table;
        }

        private static void StandardizeColumns(DataTable table)
        {
            if (table == null) return;

            MapColumn(table, "Barcode", "BarCode");
            MapColumn(table, "ItemName", "Description");
            MapColumn(table, "Item Name", "Description");
            MapColumn(table, "ItemDescription", "Description");
            MapColumn(table, "BaseUnitName", "Unit");
            MapColumn(table, "UnitName", "Unit");
            MapColumn(table, "ClosingStock", "Stock");
            MapColumn(table, "AvailableStock", "Stock");
            MapColumn(table, "Status", "ItemStatus");
            MapColumn(table, "StatusName", "ItemStatus");

            if (!table.Columns.Contains("BarCode")) table.Columns.Add("BarCode", typeof(string));
            if (!table.Columns.Contains("Description")) table.Columns.Add("Description", typeof(string));
            if (!table.Columns.Contains("Unit")) table.Columns.Add("Unit", typeof(string));
            if (!table.Columns.Contains("Stock")) table.Columns.Add("Stock", typeof(decimal));
            if (!table.Columns.Contains("ItemStatus")) table.Columns.Add("ItemStatus", typeof(string));
            if (!table.Columns.Contains("StatusReason")) table.Columns.Add("StatusReason", typeof(string));
            if (!table.Columns.Contains("StatusDate")) table.Columns.Add("StatusDate", typeof(DateTime));
        }

        private static void MapColumn(DataTable table, string oldName, string newName)
        {
            if (table.Columns.Contains(oldName) && !table.Columns.Contains(newName))
            {
                table.Columns[oldName].ColumnName = newName;
            }
        }

        private void ApplyStatusFilter(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
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
                        view.RowFilter = ColumnLikeFilter(view.Table, "BarCode", escaped);
                        break;
                    case "Item Name":
                        view.RowFilter = ColumnLikeFilter(view.Table, "Description", escaped);
                        break;
                    case "Status":
                        view.RowFilter = ColumnLikeFilter(view.Table, "ItemStatus", escaped);
                        break;
                    default:
                        view.RowFilter = string.Join(" OR ", new[]
                        {
                            ColumnLikeFilter(view.Table, "BarCode", escaped),
                            ColumnLikeFilter(view.Table, "Description", escaped),
                            ColumnLikeFilter(view.Table, "ItemStatus", escaped)
                        }.Where(filter => !string.IsNullOrWhiteSpace(filter)));
                        break;
                }
            }

            ultraGrid1.DataSource = view;
            if (textBox3 != null)
            {
                textBox3.Text = view.Count.ToString();
            }
        }

        private string ColumnLikeFilter(DataTable table, string columnName, string escapedSearchText)
        {
            if (table == null) return string.Empty;
            string actualCol = FindTableColumnName(table, columnName);
            if (string.IsNullOrEmpty(actualCol)) return string.Empty;

            return $"CONVERT([{actualCol}], 'System.String') LIKE '%{escapedSearchText}%'";
        }

        private string FindTableColumnName(DataTable table, string name)
        {
            if (table == null) return null;
            foreach (DataColumn col in table.Columns)
            {
                if (string.Equals(col.ColumnName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return col.ColumnName;
                }
            }
            return null;
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
            e.Layout.Override.RowSelectorWidth = 25;
            e.Layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            e.Layout.Override.CellClickAction = CellClickAction.RowSelect;
            e.Layout.Override.AllowUpdate = DefaultableBoolean.False;
            e.Layout.Override.DefaultRowHeight = 28;
            e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;
            e.Layout.ViewStyleBand = ViewStyleBand.OutlookGroupBy;
            e.Layout.GroupByBox.Hidden = true;

            Color headerBlue = Color.FromArgb(0, 123, 255);
            e.Layout.Override.HeaderAppearance.BackColor = headerBlue;
            e.Layout.Override.HeaderAppearance.BackColor2 = headerBlue;
            e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
            e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;

            if (e.Layout.Bands.Count == 0 || e.Layout.Bands[0].Columns.Count == 0)
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
            UltraGridColumn column = FindColumn(band, key);
            if (column == null)
            {
                return;
            }

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

        private UltraGridColumn FindColumn(UltraGridBand band, string key)
        {
            if (band == null || string.IsNullOrWhiteSpace(key)) return null;
            foreach (UltraGridColumn col in band.Columns)
            {
                if (string.Equals(col.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }
            return null;
        }
    }
}