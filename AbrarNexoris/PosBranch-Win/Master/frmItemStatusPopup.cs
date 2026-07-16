using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class frmItemStatusPopup : Form
    {
        private string[] statusOptions = Array.Empty<string>();

        public string SelectedStatus
        {
            get
            {
                if (cmbItemStatus == null)
                {
                    return string.Empty;
                }

                return (cmbItemStatus.SelectedItem?.ToString() ?? cmbItemStatus.Text ?? string.Empty).Trim();
            }
        }

        public string StatusReason => txtItemStatusReason?.Text?.Trim() ?? string.Empty;

        public DateTime StatusDate => dtpItemStatusDate != null ? dtpItemStatusDate.Value.Date : DateTime.Today;

        public frmItemStatusPopup()
        {
            InitializeComponent();
        }

        public void SetStatusOptions(string[] statuses)
        {
            statusOptions = statuses?.Where(status => !string.IsNullOrWhiteSpace(status)).Distinct().ToArray()
                ?? Array.Empty<string>();

            cmbItemStatus.Items.Clear();
            cmbItemStatus.Items.AddRange(statusOptions);
        }

        public void SetStatusValues(string statusName, string reason, DateTime statusDate)
        {
            string normalizedStatus = statusOptions.FirstOrDefault(status =>
                string.Equals(status, statusName?.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? statusOptions.FirstOrDefault()
                ?? "Active";

            if (cmbItemStatus.Items.Count == 0)
            {
                cmbItemStatus.Items.Add(normalizedStatus);
            }

            cmbItemStatus.SelectedItem = normalizedStatus;
            if (cmbItemStatus.SelectedIndex < 0)
            {
                cmbItemStatus.Text = normalizedStatus;
            }

            txtItemStatusReason.Text = reason ?? string.Empty;
            dtpItemStatusDate.Value = statusDate == DateTime.MinValue ? DateTime.Today : statusDate.Date;
            ApplyUiState();
        }

        private static bool DoesStatusBlockSale(string statusName)
        {
            return string.Equals(statusName, "Inactive", StringComparison.OrdinalIgnoreCase)
                || string.Equals(statusName, "Blocked for Sale", StringComparison.OrdinalIgnoreCase)
                || string.Equals(statusName, "Discontinued", StringComparison.OrdinalIgnoreCase);
        }

        private static bool DoesStatusBlockPurchase(string statusName)
        {
            return string.Equals(statusName, "Inactive", StringComparison.OrdinalIgnoreCase)
                || string.Equals(statusName, "Blocked for Purchase", StringComparison.OrdinalIgnoreCase)
                || string.Equals(statusName, "Discontinued", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReasonRequired(string statusName)
        {
            return !string.Equals(statusName, "Active", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyUiState()
        {
            string statusName = SelectedStatus;
            bool blockSale = DoesStatusBlockSale(statusName);
            bool blockPurchase = DoesStatusBlockPurchase(statusName);
            bool reasonRequired = IsReasonRequired(statusName);

            lblItemStatusReason.Text = reasonRequired ? "Reason *" : "Reason";
            txtItemStatusReason.BackColor = reasonRequired ? Color.FromArgb(255, 224, 192) : Color.White;
            lblItemStatusSaleRule.Text = $"Sale: {(blockSale ? "Blocked" : "Allowed")}";
            lblItemStatusSaleRule.ForeColor = blockSale ? Color.Firebrick : Color.ForestGreen;
            lblItemStatusPurchaseRule.Text = $"Purchase: {(blockPurchase ? "Blocked" : "Allowed")}";
            lblItemStatusPurchaseRule.ForeColor = blockPurchase ? Color.Firebrick : Color.ForestGreen;
        }

        private void cmbItemStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyUiState();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (IsReasonRequired(SelectedStatus) && string.IsNullOrWhiteSpace(StatusReason))
            {
                MessageBox.Show($"Please enter a reason for '{SelectedStatus}'.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemStatusReason.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
