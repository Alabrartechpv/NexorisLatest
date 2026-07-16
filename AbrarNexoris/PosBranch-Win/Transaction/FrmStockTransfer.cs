using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Transaction
{
    public partial class FrmStockTransfer : Form
    {
        private Dropdowns dp = new Dropdowns();
        private StockTransferRepository repo = new StockTransferRepository();
        private DataTable transferTable;
        private bool isViewOnlyMode = false;

        public FrmStockTransfer()
        {
            InitializeComponent();
            this.KeyPreview = true; // Support key capture
        }

        private void FrmStockTransfer_Load(object sender, EventArgs e)
        {
            try
            {
                // Wire up form/controls events
                ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
                ultraGrid1.AfterCellUpdate += ultraGrid1_AfterCellUpdate;
                ultraGrid1.AfterRowsDeleted += ultraGrid1_AfterRowsDeleted;
                ultraGrid1.InitializeRow += ultraGrid1_InitializeRow;

                // Load initial details
                txtb_sourceBranch.Text = SessionContext.BranchName ?? "Main Branch";
                
                // Populate target branch dropdown
                PopulateBranches();

                // Build Table and bind to grid
                InitializeTable();

                // Style the grid
                StyleGrid();

                // Get new Doc Number
                GetNewDocNo();

                txtb_barcode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Stock Transfer form: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateBranches()
        {
            try
            {
                var branchGrid = dp.getBanchDDl();
                if (branchGrid != null && branchGrid.List != null)
                {
                    cmb_targetBranch.Items.Clear();
                    
                    // Exclude the current branch from the target branch dropdown list
                    var targetBranches = branchGrid.List.Where(b => b.Id != SessionContext.BranchId).ToList();
                    
                    foreach (var branch in targetBranches)
                    {
                        cmb_targetBranch.Items.Add(branch.Id, branch.BranchName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error populating branches: " + ex.Message);
            }
        }

        private void InitializeTable()
        {
            transferTable = new DataTable();
            transferTable.Columns.Add("NO", typeof(int));
            transferTable.Columns.Add("BarCode", typeof(string));
            transferTable.Columns.Add("ItemNo", typeof(int));
            transferTable.Columns.Add("Description", typeof(string));
            transferTable.Columns.Add("UOM", typeof(string));
            transferTable.Columns.Add("Stock", typeof(double));
            transferTable.Columns.Add("Qty", typeof(int));
            transferTable.Columns.Add("Rate", typeof(decimal));
            transferTable.Columns.Add("Amt", typeof(decimal));
            transferTable.Columns.Add("ExpiryDate", typeof(DateTime));
            transferTable.Columns.Add("UnitId", typeof(int));

            ultraGrid1.DataSource = transferTable;
        }

        private void StyleGrid()
        {
            ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid1.DisplayLayout.Override.CellPadding = 4;
            ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            ultraGrid1.UseOsThemes = DefaultableBoolean.False;

            // Flat borders style
            ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = Color.FromArgb(226, 232, 240);

            // Alternating row styling
            Infragistics.Win.Appearance altRowAppearance = new Infragistics.Win.Appearance();
            altRowAppearance.BackColor = Color.FromArgb(248, 250, 252);
            ultraGrid1.DisplayLayout.Override.RowAlternateAppearance = altRowAppearance;

            // Row selectors appearance
            ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.None;

            // Formatting headers
            foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                col.CellAppearance.TextHAlign = HAlign.Left;
                col.Header.Appearance.TextHAlign = HAlign.Center;
                col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                col.Header.Appearance.FontData.Name = "Segoe UI";
                col.Header.Appearance.BackColor = Color.FromArgb(31, 58, 86); // Navy blue matching the card headers
                col.Header.Appearance.ForeColor = Color.White;
            }

            // Customize specific columns
            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("NO"))
            {
                var col = ultraGrid1.DisplayLayout.Bands[0].Columns["NO"];
                col.Width = 50;
                col.CellAppearance.TextHAlign = HAlign.Center;
                col.CellActivation = Activation.NoEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("ItemNo"))
            {
                ultraGrid1.DisplayLayout.Bands[0].Columns["ItemNo"].Hidden = true;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("UnitId"))
            {
                ultraGrid1.DisplayLayout.Bands[0].Columns["UnitId"].Hidden = true;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Description"))
            {
                ultraGrid1.DisplayLayout.Bands[0].Columns["Description"].Width = 250;
                ultraGrid1.DisplayLayout.Bands[0].Columns["Description"].CellActivation = Activation.NoEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("BarCode"))
            {
                ultraGrid1.DisplayLayout.Bands[0].Columns["BarCode"].Width = 120;
                ultraGrid1.DisplayLayout.Bands[0].Columns["BarCode"].CellActivation = Activation.NoEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("UOM"))
            {
                ultraGrid1.DisplayLayout.Bands[0].Columns["UOM"].Width = 80;
                ultraGrid1.DisplayLayout.Bands[0].Columns["UOM"].CellAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Bands[0].Columns["UOM"].CellActivation = Activation.NoEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Qty"))
            {
                var col = ultraGrid1.DisplayLayout.Bands[0].Columns["Qty"];
                col.Width = 90;
                col.CellAppearance.TextHAlign = HAlign.Right;
                col.CellAppearance.BackColor = Color.FromArgb(255, 255, 230); // Editable highlight
                col.CellActivation = Activation.AllowEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Rate"))
            {
                var col = ultraGrid1.DisplayLayout.Bands[0].Columns["Rate"];
                col.Width = 100;
                col.CellAppearance.TextHAlign = HAlign.Right;
                col.CellAppearance.BackColor = Color.FromArgb(255, 255, 230); // Editable highlight
                col.Format = "N2";
                col.CellActivation = Activation.AllowEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Amt"))
            {
                var col = ultraGrid1.DisplayLayout.Bands[0].Columns["Amt"];
                col.Width = 110;
                col.CellAppearance.TextHAlign = HAlign.Right;
                col.Format = "N2";
                col.CellActivation = Activation.NoEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("ExpiryDate"))
            {
                var col = ultraGrid1.DisplayLayout.Bands[0].Columns["ExpiryDate"];
                col.Width = 120;
                col.CellAppearance.TextHAlign = HAlign.Center;
                col.CellAppearance.BackColor = Color.FromArgb(255, 255, 230);
                col.Format = "yyyy-MM-dd";
                col.CellActivation = Activation.AllowEdit;
            }

            if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Stock"))
            {
                var col = ultraGrid1.DisplayLayout.Bands[0].Columns["Stock"];
                col.Width = 100;
                col.CellAppearance.TextHAlign = HAlign.Right;
                col.CellActivation = Activation.NoEdit;
                col.Header.Caption = "Current Stock";
            }

            // Enable Grid Summaries for a clean, modern total overview
            try
            {
                ultraGrid1.DisplayLayout.Bands[0].Summaries.Clear();
                ultraGrid1.DisplayLayout.Override.SummaryDisplayArea = SummaryDisplayAreas.BottomFixed;
                ultraGrid1.DisplayLayout.Override.SummaryValueAppearance.BackColor = Color.FromArgb(241, 245, 249);
                ultraGrid1.DisplayLayout.Override.SummaryValueAppearance.BorderColor = Color.FromArgb(218, 224, 233);
                ultraGrid1.DisplayLayout.Override.SummaryValueAppearance.FontData.Name = "Segoe UI";
                ultraGrid1.DisplayLayout.Override.SummaryValueAppearance.FontData.SizeInPoints = 9.5F;

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("BarCode"))
                {
                    var sumItems = ultraGrid1.DisplayLayout.Bands[0].Summaries.Add("TotalItems", SummaryType.Count, ultraGrid1.DisplayLayout.Bands[0].Columns["BarCode"]);
                    sumItems.DisplayFormat = "Items: {0}";
                    sumItems.Appearance.TextHAlign = HAlign.Left;
                    sumItems.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Qty"))
                {
                    var sumQty = ultraGrid1.DisplayLayout.Bands[0].Summaries.Add("TotalQty", SummaryType.Sum, ultraGrid1.DisplayLayout.Bands[0].Columns["Qty"]);
                    sumQty.DisplayFormat = "Qty: {0}";
                    sumQty.Appearance.TextHAlign = HAlign.Right;
                    sumQty.Appearance.FontData.Bold = DefaultableBoolean.True;
                }

                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Amt"))
                {
                    var sumAmt = ultraGrid1.DisplayLayout.Bands[0].Summaries.Add("TotalAmt", SummaryType.Sum, ultraGrid1.DisplayLayout.Bands[0].Columns["Amt"]);
                    sumAmt.DisplayFormat = "Total: {0:N2}";
                    sumAmt.Appearance.TextHAlign = HAlign.Right;
                    sumAmt.Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up summaries: " + ex.Message);
            }
        }

        private void GetNewDocNo()
        {
            try
            {
                int newNo = repo.GenerateTransferNo();
                txt_DocNo.Text = newNo.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error generating transfer doc no: " + ex.Message);
            }
        }

        private void txtb_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string input = txtb_barcode.Text.Trim();
                if (string.IsNullOrEmpty(input)) return;

                // 1. Change quantity of focused row command (e.g. *2)
                if (input.StartsWith("*") && input.Length > 1 && !input.Contains("-"))
                {
                    string quantityStr = input.Substring(1);
                    if (int.TryParse(quantityStr, out int newQty) && newQty > 0)
                    {
                        if (ultraGrid1.Rows.Count > 0 && ultraGrid1.ActiveRow != null)
                        {
                            ultraGrid1.ActiveRow.Cells["Qty"].Value = newQty;
                            txtb_barcode.Text = "";
                            txtb_barcode.Focus();
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Please select or add a row first to change its quantity.", "No Active Row", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtb_barcode.Text = "";
                            txtb_barcode.Focus();
                            return;
                        }
                    }
                }

                // 2. Quantity with barcode command (e.g. 2*BARCODE)
                int scanQty = 1;
                string barcode = input;
                if (input.Contains("*"))
                {
                    string[] parts = input.Split('*');
                    if (parts.Length == 2)
                    {
                        if (int.TryParse(parts[0], out int tempQty) && tempQty > 0 && !string.IsNullOrEmpty(parts[1]))
                        {
                            scanQty = tempQty;
                            barcode = parts[1];
                        }
                    }
                }

                // Check if item already exists in the grid. If so, increment its quantity.
                bool itemExists = false;
                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    if (row.Cells.Exists("BarCode") && row.Cells["BarCode"].Value?.ToString() == barcode)
                    {
                        int currentQty = Convert.ToInt32(row.Cells["Qty"].Value ?? 0);
                        row.Cells["Qty"].Value = currentQty + scanQty;
                        itemExists = true;
                        break;
                    }
                }

                if (itemExists)
                {
                    txtb_barcode.Text = "";
                    txtb_barcode.Focus();
                    return;
                }

                // Query database
                DataBase.Operations = "BARCODEPURCHASE";
                ItemDDlGrid itemDDLG = dp.itemDDlGrid(barcode, null);

                if (itemDDLG == null || itemDDLG.List == null || !itemDDLG.List.Any())
                {
                    MessageBox.Show("Item not found with barcode: " + barcode, "Item Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtb_barcode.Text = "";
                    txtb_barcode.Focus();
                    return;
                }

                var item = itemDDLG.List.First();
                decimal rate = 0;
                if (item.RetailPrice > 0) rate = Convert.ToDecimal(item.RetailPrice);
                else if (item.Cost > 0) rate = Convert.ToDecimal(item.Cost);

                AddItemToGrid(item.ItemId.ToString(), item.BarCode, item.Description, item.Unit, rate.ToString(), item.UnitId, scanQty);
                txtb_barcode.Text = "";
                txtb_barcode.Focus();
            }
        }

        public void AddItemToGrid(string itemId, string barcode, string description, string unit, string rateText, int unitId = 0, int initialQty = 1)
        {
            try
            {
                decimal rate = 0;
                decimal.TryParse(rateText, out rate);

                // Fetch Stock using existing Dropdowns
                double stock = 0;
                try
                {
                    DataBase.Operations = "BARCODEPURCHASE";
                    ItemDDlGrid itemDDLG = dp.itemDDlGrid(barcode, null);
                    if (itemDDLG?.List != null && itemDDLG.List.Any())
                    {
                        var matchingItem = itemDDLG.List.FirstOrDefault(x => x.BarCode == barcode);
                        if (matchingItem != null)
                        {
                            stock = matchingItem.Stock;
                        }
                        else
                        {
                            stock = itemDDLG.List.First().Stock;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error querying item stock: " + ex.Message);
                }

                // Add row to DataTable
                DataRow newRow = transferTable.NewRow();
                newRow["NO"] = transferTable.Rows.Count + 1;
                newRow["BarCode"] = barcode;
                newRow["ItemNo"] = Convert.ToInt32(itemId);
                newRow["Description"] = description;
                newRow["UOM"] = unit;
                newRow["Stock"] = stock;
                newRow["Qty"] = initialQty;
                newRow["Rate"] = rate;
                newRow["Amt"] = rate * initialQty;
                newRow["ExpiryDate"] = DateTime.Today.AddYears(1); // Default expiry 1 year from now
                newRow["UnitId"] = unitId;

                transferTable.Rows.Add(newRow);
                ultraGrid1.DataBind();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item to grid: " + ex.Message);
            }
        }

        private void btn_ItemLoad_Click(object sender, EventArgs e)
        {
            frmdialForItemMaster lookup = new frmdialForItemMaster("FrmStockTransfer");
            lookup.Owner = this;
            lookup.ShowDialog();
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            // Empty layout init to satisfy event signature if needed
        }

        private void ultraGrid1_AfterRowsDeleted(object sender, EventArgs e)
        {
            RenumberRows();
        }

        private void RenumberRows()
        {
            try
            {
                for (int i = 0; i < transferTable.Rows.Count; i++)
                {
                    transferTable.Rows[i]["NO"] = i + 1;
                }
                ultraGrid1.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in RenumberRows: " + ex.Message);
            }
        }

        private void ultraGrid1_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                var row = e.Row;
                if (row.Cells.Exists("Stock") && row.Cells.Exists("Qty"))
                {
                    double stock = ParseDouble(row.Cells["Stock"].Value);
                    double qty = ParseDouble(row.Cells["Qty"].Value);

                    if (qty > stock)
                    {
                        row.Cells["Qty"].Appearance.BackColor = Color.FromArgb(254, 226, 226); // Soft red background
                        row.Cells["Qty"].Appearance.ForeColor = Color.FromArgb(220, 38, 38); // Dark red text
                        row.Cells["Qty"].ToolTipText = "Quantity exceeds available stock!";
                    }
                    else
                    {
                        row.Cells["Qty"].Appearance.BackColor = Color.FromArgb(255, 255, 230); // Soft yellow for editable
                        row.Cells["Qty"].Appearance.ForeColor = Color.Black;
                        row.Cells["Qty"].ToolTipText = "";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in ultraGrid1_InitializeRow: " + ex.Message);
            }
        }

        private void ultraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            try
            {
                var row = e.Cell.Row;
                if (e.Cell.Column.Key == "Qty" || e.Cell.Column.Key == "Rate")
                {
                    int qty = ParseInt(row.Cells["Qty"].Value);
                    decimal rate = ParseDecimal(row.Cells["Rate"].Value);
                    row.Cells["Amt"].Value = qty * rate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in AfterCellUpdate calculations: " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
                if (isViewOnlyMode)
                {
                    MessageBox.Show("This is a saved stock transfer and cannot be modified or re-saved.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Validate target branch
                if (cmb_targetBranch.SelectedItem == null)
                {
                    MessageBox.Show("Please select a Target Branch for this transfer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int targetBranchId = Convert.ToInt32(cmb_targetBranch.Value);

                if (ultraGrid1.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to transfer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ShiftSessionGuard.CanDoTransaction(out string transactionError))
                {
                    MessageBox.Show(transactionError, "Shift Closing Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Do you want to save this stock transfer?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // Prepare Master Data
                StockTransferMaster master = new StockTransferMaster
                {
                    CompanyId = SessionContext.CompanyId,
                    FinYearId = SessionContext.FinYearId,
                    SourceId = SessionContext.BranchId,
                    TargetId = targetBranchId,
                    TransferDate = dateTimePicker1.DateTime,
                    UserId = SessionContext.UserId,
                    TotalAmount = 0,
                    Description = txteditor_remark.Text,
                    TransferType = "Branch",
                    CancelFlag = false,
                    VoucherType = "Stock Transfer"
                };

                // Prepare Details Data
                List<StockTransferDetail> details = new List<StockTransferDetail>();
                decimal totalAmount = 0;

                foreach (UltraGridRow row in ultraGrid1.Rows)
                {
                    int qty = ParseInt(row.Cells["Qty"].Value);
                    decimal rate = ParseDecimal(row.Cells["Rate"].Value);
                    decimal amt = qty * rate;
                    totalAmount += amt;

                    var detail = new StockTransferDetail
                    {
                        ItemId = ParseInt(row.Cells["ItemNo"].Value),
                        BarCode = row.Cells["BarCode"].Value?.ToString(),
                        Qty = qty,
                        Rate = rate,
                        Amt = amt,
                        UnitId = ParseInt(row.Cells["UnitId"].Value),
                        ExpiryDate = row.Cells["ExpiryDate"].Value != DBNull.Value && row.Cells["ExpiryDate"].Value != null 
                                     ? Convert.ToDateTime(row.Cells["ExpiryDate"].Value) 
                                     : (DateTime?)null
                    };
                    details.Add(detail);
                }

                master.TotalAmount = totalAmount;

                Cursor.Current = Cursors.WaitCursor;
                string result = repo.saveStockTransfer(master, details);
                Cursor.Current = Cursors.Default;

                if (result == "success")
                {
                    MessageBox.Show("Stock Transfer saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clear();
                }
                else
                {
                    MessageBox.Show($"Failed to save stock transfer:\n{result}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving stock transfer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear()
        {
            try
            {
                transferTable.Clear();
                cmb_targetBranch.Value = null;
                cmb_targetBranch.SelectedItem = null;
                txteditor_remark.Text = "";
                dateTimePicker1.Value = DateTime.Today;
                GetNewDocNo();
                txtb_barcode.Text = "";

                // Reset view-only mode controls
                isViewOnlyMode = false;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.True;
                cmb_targetBranch.Enabled = true;
                dateTimePicker1.Enabled = true;
                txteditor_remark.ReadOnly = false;
                txtb_barcode.Enabled = true;
                ultraPictureBox1.Enabled = true;

                txtb_barcode.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error clearing form: " + ex.Message);
            }
        }

        private int ParseInt(object val, int defaultVal = 0)
        {
            if (val == null || val == DBNull.Value) return defaultVal;
            if (int.TryParse(val.ToString(), out int result)) return result;
            return defaultVal;
        }

        private double ParseDouble(object val, double defaultVal = 0.0)
        {
            if (val == null || val == DBNull.Value) return defaultVal;
            if (double.TryParse(val.ToString(), out double result)) return result;
            return defaultVal;
        }

        private decimal ParseDecimal(object val, decimal defaultVal = 0m)
        {
            if (val == null || val == DBNull.Value) return defaultVal;
            if (decimal.TryParse(val.ToString(), out decimal result)) return result;
            return defaultVal;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (keyData == Keys.F8)
                {
                    Save();
                    return true;
                }
                else if (keyData == Keys.F1)
                {
                    Clear();
                    return true;
                }
                else if (keyData == Keys.F9)
                {
                    this.Close();
                    return true;
                }
                else if (keyData == Keys.F6)
                {
                    btn_ItemLoad_Click(this, EventArgs.Empty);
                    return true;
                }
                else if (keyData == Keys.F7)
                {
                    OpenHistoryDialog();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in ProcessCmdKey: " + ex.Message);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FrmStockTransfer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F8)
            {
                Save();
            }
            else if (e.KeyCode == Keys.F1)
            {
                Clear();
            }
            else if (e.KeyCode == Keys.F9)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F6)
            {
                btn_ItemLoad_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F7)
            {
                OpenHistoryDialog();
            }
        }

        private double GetItemStock(string barcode)
        {
            double stock = 0;
            try
            {
                DataBase.Operations = "BARCODEPURCHASE";
                ItemDDlGrid itemDDLG = dp.itemDDlGrid(barcode, null);
                if (itemDDLG?.List != null && itemDDLG.List.Any())
                {
                    var matchingItem = itemDDLG.List.FirstOrDefault(x => x.BarCode == barcode);
                    if (matchingItem != null)
                    {
                        stock = matchingItem.Stock;
                    }
                    else
                    {
                        stock = itemDDLG.List.First().Stock;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error querying item stock: " + ex.Message);
            }
            return stock;
        }

        public void LoadTransfer(int id)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var data = repo.GetStockTransferById(id);
                if (data == null || data.Item1 == null)
                {
                    MessageBox.Show("Stock Transfer details not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                StockTransferMaster master = data.Item1;
                List<StockTransferDetail> details = data.Item2;

                // Clear current grid rows
                transferTable.Clear();

                // Populate master details on Form
                txt_DocNo.Text = master.StkTrNo.ToString();
                dateTimePicker1.Value = master.TransferDate;
                txteditor_remark.Text = master.Description;

                // Populate Target Branch dropdown
                cmb_targetBranch.Value = master.TargetId;

                // Populate details rows into transferTable
                int slNo = 1;
                foreach (var detail in details)
                {
                    DataRow row = transferTable.NewRow();
                    row["NO"] = slNo++;
                    row["BarCode"] = detail.BarCode;
                    row["ItemNo"] = detail.ItemId;
                    row["Description"] = detail.ItemName;
                    row["UOM"] = detail.UnitName;
                    row["Qty"] = detail.Qty;
                    row["Rate"] = detail.Rate;
                    row["Amt"] = detail.Amt;
                    row["UnitId"] = detail.UnitId;
                    if (detail.ExpiryDate.HasValue)
                        row["ExpiryDate"] = detail.ExpiryDate.Value;
                    else
                        row["ExpiryDate"] = DBNull.Value;
                    
                    // Fetch stock using helper
                    row["Stock"] = GetItemStock(detail.BarCode);

                    transferTable.Rows.Add(row);
                }

                ultraGrid1.DataBind();

                // Set view-only mode controls
                isViewOnlyMode = true;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                cmb_targetBranch.Enabled = false;
                dateTimePicker1.Enabled = false;
                txteditor_remark.ReadOnly = true;
                txtb_barcode.Enabled = false;
                ultraPictureBox1.Enabled = false;

                txtb_barcode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Stock Transfer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OpenHistoryDialog()
        {
            try
            {
                frmStockTransferListDialog historyDialog = new frmStockTransferListDialog();
                historyDialog.StartPosition = FormStartPosition.CenterParent;
                historyDialog.OnTransferSelected += (id, stkTrNo, totalAmount) =>
                {
                    LoadTransfer(id);
                };
                historyDialog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Stock Transfer history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pic_DocNoSearch_Click(object sender, EventArgs e)
        {
            OpenHistoryDialog();
        }
    }
}
