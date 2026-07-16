using ModelClass;
using PosBranch_Win.DialogBox;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Utilities
{
    public partial class frmChangeItemNo : Form
    {
        private int _selectedItemId = 0;
        private string _selectedItemNo = string.Empty;

        public frmChangeItemNo()
        {
            InitializeComponent();
            this.Load += FrmChangeItemNo_Load;
        }

        private void FrmChangeItemNo_Load(object sender, EventArgs e)
        {
            var frmMaster = Application.OpenForms.OfType<Master.frmItemMasterNew>().FirstOrDefault();
            if (frmMaster != null && frmMaster.CurrentItemId > 0)
            {
                SearchItemById(frmMaster.CurrentItemId);
            }
        }

        private void btnF7_Click(object sender, EventArgs e)
        {
            OpenItemSearch();
        }

        private void txtOldItemNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                OpenItemSearch();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                // Optionally handle barcode scanning directly into this form
                SearchByBarcode(txtOldItemNo.Text.Trim());
                e.Handled = true;
            }
        }

        private void frmChangeItemNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                OpenItemSearch();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void OpenItemSearch()
        {
            frmdialForItemMaster itemDialog = new frmdialForItemMaster("frmChangeItemNo");
            itemDialog.StartPosition = FormStartPosition.CenterScreen;

            if (itemDialog.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, object> selectedItemData = itemDialog.GetSelectedItemData();

                if (selectedItemData != null && selectedItemData.Count > 0)
                {
                    string barcode = selectedItemData.ContainsKey("BarCode") ? selectedItemData["BarCode"]?.ToString() ?? "" : "";
                    string description = selectedItemData.ContainsKey("Description") ? selectedItemData["Description"]?.ToString() ?? "" : "";
                    _selectedItemNo = selectedItemData.ContainsKey("ItemNo") ? selectedItemData["ItemNo"]?.ToString() ?? "" : string.Empty;

                    if (selectedItemData.ContainsKey("ItemId"))
                    {
                        int.TryParse(selectedItemData["ItemId"].ToString(), out _selectedItemId);
                    }

                    txtOldItemNo.Text = barcode;
                    txtDescription.Text = description;
                    txtNewItemNo.Focus();
                }
            }
        }

        private void SearchByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return;

            Repository.BaseRepostitory db = new Repository.BaseRepostitory();
            db.DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ItemId, ItemNo, Barcode, Description FROM ItemMaster WHERE Barcode = @Barcode", (SqlConnection)db.DataConnection))
                {
                    cmd.Parameters.AddWithValue("@Barcode", barcode);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _selectedItemId = reader.GetInt32(0);
                            _selectedItemNo = reader["ItemNo"] == DBNull.Value ? string.Empty : Convert.ToString(reader["ItemNo"]);
                            txtOldItemNo.Text = reader["Barcode"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Barcode"]);
                            txtDescription.Text = reader["Description"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Description"]);
                            txtNewItemNo.Focus();
                        }
                        else
                        {
                            MessageBox.Show("Item not found.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (db.DataConnection.State == ConnectionState.Open)
                    db.DataConnection.Close();
            }
        }

        private void SearchItemById(int itemId)
        {
            if (itemId <= 0) return;

            Repository.BaseRepostitory db = new Repository.BaseRepostitory();
            db.DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ItemId, ItemNo, Barcode, Description FROM ItemMaster WHERE ItemId = @ItemId", (SqlConnection)db.DataConnection))
                {
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _selectedItemId = reader.GetInt32(0);
                            _selectedItemNo = reader["ItemNo"] == DBNull.Value ? string.Empty : Convert.ToString(reader["ItemNo"]);
                            txtOldItemNo.Text = reader["Barcode"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Barcode"]);
                            txtDescription.Text = reader["Description"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Description"]);
                            txtNewItemNo.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error preloading item: " + ex.Message);
            }
            finally
            {
                if (db.DataConnection.State == ConnectionState.Open)
                    db.DataConnection.Close();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string oldItemNo = txtOldItemNo.Text.Trim();
            string newItemNo = txtNewItemNo.Text.Trim();

            if (string.IsNullOrEmpty(oldItemNo))
            {
                MessageBox.Show("Please select an item first (Press F7 or scan).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldItemNo.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newItemNo))
            {
                MessageBox.Show("Please enter the new Item No.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewItemNo.Focus();
                return;
            }

            if (oldItemNo == newItemNo)
            {
                MessageBox.Show("New Item No cannot be the same as Old Item No.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewItemNo.Focus();
                return;
            }

            if (_selectedItemId <= 0)
            {
                MessageBox.Show("Invalid Item ID. Please search for the item again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm change
            DialogResult dr = MessageBox.Show($"Are you sure you want to change Item No '{oldItemNo}' to '{newItemNo}'?", "Confirm Change", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            // Execute update query
            Repository.BaseRepostitory db = new Repository.BaseRepostitory();
            db.DataConnection.Open();
            var trans = db.DataConnection.BeginTransaction();
            try
            {
                // Update ItemMaster
                using (SqlCommand cmd = new SqlCommand("UPDATE ItemMaster SET Barcode = @NewBarcode WHERE ItemId = @ItemId", (SqlConnection)db.DataConnection, (SqlTransaction)trans))
                {
                    cmd.Parameters.AddWithValue("@NewBarcode", newItemNo);
                    cmd.Parameters.AddWithValue("@ItemId", _selectedItemId);
                    cmd.ExecuteNonQuery();
                }

                // Update PriceSettings (the Price and Barcode tables)
                using (SqlCommand cmd = new SqlCommand("UPDATE PriceSettings SET Barcode = @NewBarcode WHERE ItemId = @ItemId AND Barcode = @OldBarcode", (SqlConnection)db.DataConnection, (SqlTransaction)trans))
                {
                    cmd.Parameters.AddWithValue("@NewBarcode", newItemNo);
                    cmd.Parameters.AddWithValue("@OldBarcode", oldItemNo);
                    cmd.Parameters.AddWithValue("@ItemId", _selectedItemId);
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
                LogItemBarcodeChange(oldItemNo, newItemNo);
                MessageBox.Show("Item No changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (SqlException sqlEx)
            {
                trans.Rollback();
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Unique constraint violation
                {
                    MessageBox.Show("This Item No already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show("Error updating Item No: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (db.DataConnection.State == ConnectionState.Open)
                    db.DataConnection.Close();
            }
        }

        private void LogItemBarcodeChange(string oldBarcode, string newBarcode)
        {
            try
            {
                using (var activityLog = new ItemActivityLogRepository())
                {
                    activityLog.SaveItemActivity(
                        _selectedItemId,
                        _selectedItemNo,
                        txtDescription.Text.Trim(),
                        newBarcode,
                        "UPDATE",
                        $"Item '{txtDescription.Text.Trim()}': Barcode changed from {FormatLogValue(oldBarcode)} to {FormatLogValue(newBarcode)}.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Item barcode activity log failed: " + ex.Message);
            }
        }

        private static string FormatLogValue(string value)
        {
            value = (value ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(value) ? "(blank)" : value;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
