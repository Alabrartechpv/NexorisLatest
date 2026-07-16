using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Master;
using Repository.MasterRepositry;
using ModelClass;

namespace PosBranch_Win.Utilities
{
    public partial class FrmPlu : Form
    {
        private PluRepository pluRepo;
        private List<PluModel> allItems;
        private DataTable fullDataTable;

        public FrmPlu()
        {
            InitializeComponent();
            pluRepo = new PluRepository();
        }

        private void FrmPlu_Load(object sender, EventArgs e)
        {
            try
            {
                // Setup UltraGrid styling
                SetupUltraGridStyle();

                // Load data
                LoadWeighingItems();

                // Wire up search functionality
                txtSearch.TextChanged += TxtSearch_TextChanged;

                // Apply button hover effects
                ApplyButtonHoverEffects();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading form: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Load all weighing items from database
        /// </summary>
        private void LoadWeighingItems()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                allItems = pluRepo.GetAllWeighingItems(SessionContext.BranchId);

                // Convert to DataTable for grid binding
                fullDataTable = ConvertToDataTable(allItems);

                // Bind to grid
                ultraGrid1.DataSource = fullDataTable;

                // Update record count
                UpdateRecordCount();

                // Auto-size columns after binding
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        col.PerformAutoResize(PerformAutoSizeType.AllRowsInBand);
                    }
                }

                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("Error loading weighing items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Convert List<PluModel> to DataTable
        /// </summary>
        private DataTable ConvertToDataTable(List<PluModel> items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SlNo", typeof(int));
            dt.Columns.Add("ItemId", typeof(int));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("BarCode", typeof(string));
            dt.Columns.Add("RetailPrice", typeof(decimal));

            for (int i = 0; i < items.Count; i++)
            {
                dt.Rows.Add(i + 1, items[i].ItemId, items[i].Description, items[i].BarCode, items[i].RetailPrice);
            }

            return dt;
        }

        /// <summary>
        /// Setup UltraGrid appearance and styling
        /// </summary>
        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();

                // Behavior
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;

                // Column interactions
                ultraGrid1.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

                // GroupBy box
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;

                // Hide caption
                ultraGrid1.DisplayLayout.CaptionVisible = Infragistics.Win.DefaultableBoolean.False;

                // Borders
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;

                // Row height
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 35;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 35;

                // Colors
                Color headerBlue = Color.FromArgb(0, 123, 255);
                Color lightBlue = Color.FromArgb(173, 216, 230);

                // Header styling
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 10;

                // Row backgrounds
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(240, 248, 255);

                // Selection colors
                Color selectionBlue = Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = selectionBlue;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = selectionBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;

                // Row selector
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;

                // Cell appearance
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;

                // Wire up events
                ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up grid style: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handle grid layout initialization
        /// </summary>
        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                Color headerBlue = Color.FromArgb(0, 123, 255);

                // Style headers
                if (e.Layout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        col.Header.Appearance.BackColor = headerBlue;
                        col.Header.Appearance.BackColor2 = headerBlue;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        col.Header.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;

                        // Configure specific columns
                        if (col.Key == "SlNo")
                        {
                            col.Header.Caption = "Sl No";
                            col.Width = 60;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        }
                        else if (col.Key == "ItemId")
                        {
                            col.Header.Caption = "Item ID";
                            col.Width = 100;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        }
                        else if (col.Key == "Description")
                        {
                            col.Header.Caption = "Description";
                            col.Width = 300;
                            col.CellActivation = Activation.NoEdit;
                        }
                        else if (col.Key == "BarCode")
                        {
                            col.Header.Caption = "Barcode";
                            col.Width = 120;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        }
                        else if (col.Key == "RetailPrice")
                        {
                            col.Header.Caption = "Price";
                            col.Format = "N2";
                            col.Width = 100;
                            col.CellActivation = Activation.NoEdit;
                            col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Right;
                        }
                    }
                }

                // Row selector styling
                e.Layout.Override.RowSelectorAppearance.BackColor = headerBlue;
                e.Layout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            }
            catch { }
        }

        /// <summary>
        /// Apply hover effects to button panels
        /// </summary>
        private void ApplyButtonHoverEffects()
        {
            try
            {
                ApplyPanelHoverEffect(ultraPanelExportCSV, lblExportCSV);
                ApplyPanelHoverEffect(ultraPanelExportTXT, lblExportTXT);
                ApplyPanelHoverEffect(ultraPanelRefresh, lblRefresh);
                ApplyPanelHoverEffect(ultraPanelClose, lblClose);
            }
            catch { }
        }

        /// <summary>
        /// Apply hover effect to a panel
        /// </summary>
        private void ApplyPanelHoverEffect(Infragistics.Win.Misc.UltraPanel panel, Label label)
        {
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);

            // Panel hover
            panel.MouseEnter += (s, e) => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
            };
            panel.MouseLeave += (s, e) => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };

            // Label hover
            label.MouseEnter += (s, e) => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
            };
            label.MouseLeave += (s, e) => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
            };
        }

        /// <summary>
        /// Brighten a color by a specified amount
        /// </summary>
        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(color.R + amount, 255),
                Math.Min(color.G + amount, 255),
                Math.Min(color.B + amount, 255));
        }

        /// <summary>
        /// Handle search text change
        /// </summary>
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Apply filter to the grid based on search text
        /// </summary>
        private void ApplyFilter()
        {
            try
            {
                if (fullDataTable == null) return;

                string searchText = txtSearch.Text.Trim();
                DataView dv = fullDataTable.DefaultView;

                if (!string.IsNullOrEmpty(searchText))
                {
                    string escapedSearchText = searchText.Replace("'", "''");
                    dv.RowFilter = $"Description LIKE '%{escapedSearchText}%' OR BarCode LIKE '%{escapedSearchText}%' OR CONVERT(ItemId, 'System.String') LIKE '%{escapedSearchText}%' OR CONVERT(SlNo, 'System.String') LIKE '%{escapedSearchText}%'";
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }

                UpdateRecordCount();

                // Select first row if available
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying filter: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update record count label
        /// </summary>
        private void UpdateRecordCount()
        {
            try
            {
                int totalRecords = fullDataTable != null ? fullDataTable.Rows.Count : 0;
                int visibleRecords = fullDataTable != null ? fullDataTable.DefaultView.Count : 0;

                if (visibleRecords == totalRecords)
                {
                    lblRecordCount.Text = $"Total Records: {totalRecords}";
                }
                else
                {
                    lblRecordCount.Text = $"Showing {visibleRecords} of {totalRecords} records";
                }
            }
            catch { }
        }

        /// <summary>
        /// Export all items to CSV file
        /// </summary>
        private void lblExportCSV_Click(object sender, EventArgs e)
        {
            try
            {
                if (fullDataTable == null || fullDataTable.DefaultView.Count == 0)
                {
                    MessageBox.Show("No items to export.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Use a fixed filename instead of timestamp-based
                string defaultFileName = "WeighingItemsPLU.csv";

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.Title = "Export to CSV";
                saveFileDialog.FileName = defaultFileName;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int exportedCount = ExportToCSV(saveFileDialog.FileName);
                    MessageBox.Show($"Successfully exported {exportedCount} item(s) to:\n{saveFileDialog.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Cannot export the file because it is currently open in another program (like Excel).\n\n" +
                               "Please close the file and try again.\n\n" +
                               "Error: " + ioEx.Message,
                               "File In Use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Export all items to TXT file
        /// </summary>
        private void lblExportTXT_Click(object sender, EventArgs e)
        {
            try
            {
                if (fullDataTable == null || fullDataTable.DefaultView.Count == 0)
                {
                    MessageBox.Show("No items to export.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text files (*.txt)|*.txt";
                saveFileDialog.Title = "Export to TXT";
                saveFileDialog.FileName = $"WeighingItemsPLU_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    int exportedCount = ExportToTXT(saveFileDialog.FileName);
                    MessageBox.Show($"Successfully exported {exportedCount} item(s) to:\n{saveFileDialog.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to TXT: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Export all items to CSV format (overwrites existing file)
        /// </summary>
        private int ExportToCSV(string filePath)
        {
            try
            {
                int exportedCount = 0;

                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    sw.WriteLine("SlNo,ItemId,Description,BarCode,RetailPrice");

                    // Write data rows (all visible rows)
                    DataView dv = fullDataTable.DefaultView;
                    foreach (DataRowView rowView in dv)
                    {
                        DataRow row = rowView.Row;
                        string line = $"{row["SlNo"]},{row["ItemId"]},\"{EscapeCSV(row["Description"].ToString())}\",\"{EscapeCSV(row["BarCode"].ToString())}\",{row["RetailPrice"]}";
                        sw.WriteLine(line);
                        exportedCount++;
                    }
                }

                return exportedCount;
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing CSV file: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Export all items to TXT format (tab-delimited)
        /// </summary>
        private int ExportToTXT(string filePath)
        {
            try
            {
                int exportedCount = 0;
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    sw.WriteLine("SlNo\tItemId\tDescription\tBarCode\tRetailPrice");

                    // Write data rows (all visible rows)
                    DataView dv = fullDataTable.DefaultView;
                    foreach (DataRowView rowView in dv)
                    {
                        DataRow row = rowView.Row;
                        string line = $"{row["SlNo"]}\t{row["ItemId"]}\t{row["Description"]}\t{row["BarCode"]}\t{row["RetailPrice"]}";
                        sw.WriteLine(line);
                        exportedCount++;
                    }
                }

                return exportedCount;
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing TXT file: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Escape CSV special characters
        /// </summary>
        private string EscapeCSV(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // If the string contains comma, quote, or newline, wrap it in quotes and escape existing quotes
            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n") || input.Contains("\r"))
            {
                return input.Replace("\"", "\"\"");
            }
            return input;
        }

        /// <summary>
        /// Refresh data
        /// </summary>
        private void lblRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Clear();
                LoadWeighingItems();
                MessageBox.Show("Data refreshed successfully!", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Close form
        /// </summary>
        private void lblClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
