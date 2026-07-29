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
        /// Apply professional styling to UltraGrid - matching frmAuditReport design
        /// </summary>
        private void ApplyGridStyling(UltraGrid targetGrid)
        {
            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.FilterUIType = FilterUIType.HeaderIcons;
            targetGrid.DisplayLayout.Override.FilterOperatorLocation = FilterOperatorLocation.Hidden;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortMulti;
            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 28;
            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(247, 250, 255);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(120, 116, 235);
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(145, 179, 222);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(118, 157, 209);
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.FromArgb(17, 52, 102);
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = Color.FromArgb(103, 142, 196);
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.FilterCellAppearance.BorderColor = Color.FromArgb(180, 198, 220);
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(210, 220, 235);
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
            targetGrid.DisplayLayout.Override.WrapHeaderText = DefaultableBoolean.True;
        }

        private void StyleButtons()
        {
            ConfigureButton(btnSearch, Color.FromArgb(72, 122, 214), Color.FromArgb(95, 145, 230));
            ConfigureButton(btnExport, Color.FromArgb(0, 121, 107), Color.FromArgb(0, 150, 136));
            ConfigureButton(btnPrint, Color.FromArgb(74, 130, 176), Color.FromArgb(104, 155, 196));
            ConfigureButton(btnClose, Color.FromArgb(211, 47, 47), Color.FromArgb(244, 67, 54));
            ConfigureButton(btnHideSelection, Color.FromArgb(84, 120, 190), Color.FromArgb(112, 148, 214));

            // Style summary labels
            StyleSummaryLabels();
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

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnHideSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
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
