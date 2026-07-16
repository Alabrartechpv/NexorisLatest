using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmItemReport : Form
    {
        private ItemReportRepo itemReportRepo;
        private BaseRepostitory baseRepo;

        public frmItemReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                itemReportRepo = new ItemReportRepo();
                baseRepo = new BaseRepostitory();

                // Load initial data
                LoadBranches();
                LoadItems();

                // Configure Grids
                ConfigureTransactionGrid();
                ConfigurePriceSettingsGrid();
                ConfigureVendorGrid();
                ConfigureStockGrid();
                ConfigurePendingOrdersGrid();

                // Style Buttons
                StyleButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBranches()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)baseRepo.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        DataRow dr = dt.NewRow();
                        dr["Id"] = 0;
                        dr["BranchName"] = "--Select Branch--";
                        dt.Rows.InsertAt(dr, 0);

                        ultraComboBranch.ValueMember = "Id";
                        ultraComboBranch.DisplayMember = "BranchName";
                        ultraComboBranch.DataSource = dt;

                        // Set current branch as default
                        if (!string.IsNullOrEmpty(DataBase.BranchId))
                        {
                            // ultraComboBranch.Value = Convert.ToInt32(DataBase.BranchId);
                            // To avoid issues if BranchId not in list, try safe cast or check
                            int currentBranchId;
                            if (int.TryParse(DataBase.BranchId, out currentBranchId))
                            {
                                ultraComboBranch.Value = currentBranchId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading branches: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadItems()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemDetalisDDL, (SqlConnection)baseRepo.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", DataBase.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", DataBase.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        ultraComboItem.ValueMember = "ItemId";
                        ultraComboItem.DisplayMember = "Description";
                        ultraComboItem.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureTransactionGrid()
        {
            ApplyGridStyling(ultraGridTransactions);
        }

        private void ConfigurePriceSettingsGrid()
        {
            ApplyGridStyling(ultraGridPriceSettings);
        }

        private void ConfigureVendorGrid()
        {
            ApplyGridStyling(ultraGridVendors);
        }

        private void ConfigureStockGrid()
        {
            ApplyGridStyling(ultraGridStock);
        }

        private void ConfigurePendingOrdersGrid()
        {
            ApplyGridStyling(ultraGridPendingOrders);
        }

        /// <summary>
        /// Apply professional styling to UltraGrid - Material Design inspired
        /// </summary>
        private void ApplyGridStyling(UltraGrid grid)
        {
            // Sorting & Filtering
            grid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            grid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            grid.DisplayLayout.Override.FilterUIType = FilterUIType.FilterRow;
            grid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            // Row selectors - Modern look
            grid.DisplayLayout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(69, 90, 100);
            grid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = Infragistics.Win.HAlign.Center;

            // Modern header styling - Deep Blue-Grey gradient
            grid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(55, 71, 79);
            grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(69, 90, 100);
            grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            grid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            grid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

            // Row height
            grid.DisplayLayout.Override.MinRowHeight = 25;
            grid.DisplayLayout.Override.DefaultRowHeight = 25;

            // Alternating row colors - Soft gradient
            grid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(250, 250, 252);

            // Selection colors - Material Design Blue
            grid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(66, 165, 245);
            grid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            grid.DisplayLayout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.True;

            // Active row - Light blue hover
            grid.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(227, 242, 253);
            grid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(33, 33, 33);
            grid.DisplayLayout.Override.ActiveRowAppearance.BorderColor = Color.FromArgb(66, 165, 245);

            // Border styles
            grid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            grid.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
            grid.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
            grid.DisplayLayout.GroupByBox.Hidden = true;

            // Column interactions
            grid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            grid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            grid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
        }

        private void StyleButtons()
        {
            // Style search button - Primary Blue with gradient
            btnSearch.UseAppStyling = false;
            btnSearch.UseOsThemes = DefaultableBoolean.False;
            btnSearch.Appearance.BackColor = Color.FromArgb(25, 118, 210);
            btnSearch.Appearance.BackColor2 = Color.FromArgb(33, 150, 243);
            btnSearch.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnSearch.Appearance.ForeColor = Color.White;
            btnSearch.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnSearch.Appearance.FontData.SizeInPoints = 10;
            btnSearch.Appearance.BorderColor = Color.FromArgb(21, 101, 192);
            btnSearch.HotTrackAppearance.BackColor = Color.FromArgb(66, 165, 245);
            btnSearch.HotTrackAppearance.ForeColor = Color.White;

            // Style export button - Teal with gradient
            btnExport.UseAppStyling = false;
            btnExport.UseOsThemes = DefaultableBoolean.False;
            btnExport.Appearance.BackColor = Color.FromArgb(0, 121, 107);
            btnExport.Appearance.BackColor2 = Color.FromArgb(0, 150, 136);
            btnExport.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnExport.Appearance.ForeColor = Color.White;
            btnExport.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnExport.Appearance.FontData.SizeInPoints = 10;
            btnExport.Appearance.BorderColor = Color.FromArgb(0, 105, 92);
            btnExport.HotTrackAppearance.BackColor = Color.FromArgb(38, 166, 154);
            btnExport.HotTrackAppearance.ForeColor = Color.White;

            // Style print button - Deep Purple with gradient
            btnPrint.UseAppStyling = false;
            btnPrint.UseOsThemes = DefaultableBoolean.False;
            btnPrint.Appearance.BackColor = Color.FromArgb(81, 45, 168);
            btnPrint.Appearance.BackColor2 = Color.FromArgb(103, 58, 183);
            btnPrint.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnPrint.Appearance.ForeColor = Color.White;
            btnPrint.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnPrint.Appearance.FontData.SizeInPoints = 10;
            btnPrint.Appearance.BorderColor = Color.FromArgb(69, 39, 160);
            btnPrint.HotTrackAppearance.BackColor = Color.FromArgb(126, 87, 194);
            btnPrint.HotTrackAppearance.ForeColor = Color.White;

            // Style close button - Red with gradient
            btnClose.UseAppStyling = false;
            btnClose.UseOsThemes = DefaultableBoolean.False;
            btnClose.Appearance.BackColor = Color.FromArgb(211, 47, 47);
            btnClose.Appearance.BackColor2 = Color.FromArgb(244, 67, 54);
            btnClose.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.FontData.Bold = DefaultableBoolean.True;
            btnClose.Appearance.FontData.SizeInPoints = 10;
            btnClose.Appearance.BorderColor = Color.FromArgb(183, 28, 28);
            btnClose.HotTrackAppearance.BackColor = Color.FromArgb(229, 115, 115);
            btnClose.HotTrackAppearance.ForeColor = Color.White;

            // Style summary labels
            StyleSummaryLabels();
        }

        /// <summary>
        /// Style summary labels with colors and bold text
        /// </summary>
        private void StyleSummaryLabels()
        {
            // Caption labels - bold with accent colors
            ultraLabelTotalInCaption.Appearance.ForeColor = Color.FromArgb(56, 142, 60); // Green
            ultraLabelTotalInCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalInCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelTotalOutCaption.Appearance.ForeColor = Color.FromArgb(211, 84, 0); // Orange
            ultraLabelTotalOutCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalOutCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelCurrentStockCaption.Appearance.ForeColor = Color.FromArgb(25, 118, 210); // Blue
            ultraLabelCurrentStockCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelCurrentStockCaption.Appearance.FontData.SizeInPoints = 10;

            ultraLabelStockValueCaption.Appearance.ForeColor = Color.FromArgb(123, 31, 162); // Purple
            ultraLabelStockValueCaption.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelStockValueCaption.Appearance.FontData.SizeInPoints = 10;

            // Value labels - larger, bold
            ultraLabelTotalInValue.Appearance.ForeColor = Color.FromArgb(27, 94, 32);
            ultraLabelTotalInValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalInValue.Appearance.FontData.SizeInPoints = 14;

            ultraLabelTotalOutValue.Appearance.ForeColor = Color.FromArgb(191, 54, 12);
            ultraLabelTotalOutValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelTotalOutValue.Appearance.FontData.SizeInPoints = 14;

            ultraLabelCurrentStockValue.Appearance.ForeColor = Color.FromArgb(13, 71, 161);
            ultraLabelCurrentStockValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelCurrentStockValue.Appearance.FontData.SizeInPoints = 16;

            ultraLabelStockValueValue.Appearance.ForeColor = Color.FromArgb(74, 20, 140);
            ultraLabelStockValueValue.Appearance.FontData.Bold = DefaultableBoolean.True;
            ultraLabelStockValueValue.Appearance.FontData.SizeInPoints = 16;
        }

        private void frmItemReport_Load(object sender, EventArgs e)
        {
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (ultraComboItem.Value == null)
            {
                MessageBox.Show("Please select an item first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraComboItem.Focus();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            try
            {
                int itemId = Convert.ToInt32(ultraComboItem.Value);
                int branchId = ultraComboBranch.Value != null ? Convert.ToInt32(ultraComboBranch.Value) : (int.TryParse(DataBase.BranchId, out int bId) ? bId : 0);

                int finYearId = !string.IsNullOrEmpty(DataBase.FinyearId) ? Convert.ToInt32(DataBase.FinyearId) : 1;
                int companyId = !string.IsNullOrEmpty(DataBase.CompanyId) ? Convert.ToInt32(DataBase.CompanyId) : 1;

                // Fetch Data
                var reportData = itemReportRepo.GetItemReport(finYearId, companyId, branchId, itemId);

                // Bind Data
                ultraGridTransactions.DataSource = reportData.Transactions;
                ultraGridPriceSettings.DataSource = reportData.PriceSettings;
                ultraGridVendors.DataSource = reportData.Vendors;
                ultraGridStock.DataSource = reportData.StockSummary;
                ultraGridPendingOrders.DataSource = reportData.PendingOrders;

                // Update details
                if (reportData.ItemDetails != null)
                {
                    ultraLabelItemNameValue.Text = reportData.ItemDetails.ItemName;
                    ultraLabelBrandValue.Text = reportData.ItemDetails.BrandName;
                    ultraLabelGroupValue.Text = reportData.ItemDetails.GroupName;
                    ultraLabelCategoryValue.Text = reportData.ItemDetails.CategoryName;
                    ultraLabelSubCategoryValue.Text = reportData.ItemDetails.SubCategoryName;
                    ultraLabelLocationValue.Text = $"{reportData.ItemDetails.Row} - {reportData.ItemDetails.RackName}";
                }
                else
                {
                    ultraLabelItemNameValue.Text = "-";
                    ultraLabelBrandValue.Text = "-";
                    ultraLabelGroupValue.Text = "-";
                    ultraLabelCategoryValue.Text = "-";
                    ultraLabelSubCategoryValue.Text = "-";
                    ultraLabelLocationValue.Text = "-";
                }

                // Update Summary
                if (reportData.Transactions != null)
                {
                    decimal totalIn = reportData.Transactions.Where(x => x.Way == "IN").Sum(x => x.Qty);
                    decimal totalOut = reportData.Transactions.Where(x => x.Way == "OUT").Sum(x => x.Qty);

                    ultraLabelTotalInValue.Text = totalIn.ToString("N2");
                    ultraLabelTotalOutValue.Text = totalOut.ToString("N2");
                }
                else
                {
                    ultraLabelTotalInValue.Text = "0.00";
                    ultraLabelTotalOutValue.Text = "0.00";
                }

                if (reportData.StockSummary != null && reportData.StockSummary.Count > 0)
                {
                    ultraLabelCurrentStockValue.Text = reportData.StockSummary.Sum(x => x.Stock).ToString("N2");
                }
                else
                {
                    ultraLabelCurrentStockValue.Text = "0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "Save Report"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Export ultraGridTransactions
                    // Check if there are rows
                    if (ultraGridTransactions.Rows.Count > 0)
                    {
                        ExportToCSV(ultraGridTransactions, saveFileDialog.FileName);
                        MessageBox.Show("Export successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(UltraGrid grid, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            // Header
            foreach (var col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                    sb.Append(col.Header.Caption + ",");
            }
            sb.Length--; // Remove last comma
            sb.AppendLine();

            // Rows
            foreach (var row in grid.Rows)
            {
                foreach (var col in grid.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        string value = row.Cells[col].Value?.ToString() ?? "";
                        if (value.Contains(",")) value = "\"" + value + "\"";
                        sb.Append(value + ",");
                    }
                }
                sb.Length--;
                sb.AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            ultraGridTransactions.PrintPreview();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
