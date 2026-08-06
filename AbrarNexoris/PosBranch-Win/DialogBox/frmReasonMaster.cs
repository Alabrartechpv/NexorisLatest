using ModelClass;
using ModelClass.TransactionModels;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmReasonMaster : Form
    {
        private StockAdjustmentRepository _repository = new StockAdjustmentRepository();
        public StockAdjustmentReasonMaster ReasonModel { get; private set; }

        public frmReasonMaster()
        {
            InitializeComponent();
            PopulateReasonTypes();
            ReasonModel = new StockAdjustmentReasonMaster();
        }

        public frmReasonMaster(StockAdjustmentReasonMaster existingReason) : this()
        {
            if (existingReason != null)
            {
                ReasonModel = existingReason;
                txtReasonName.Text = existingReason.ReasonName;
                SelectReasonType(existingReason.ReasonType);
            }
        }

        private class ReasonTypeItem
        {
            public string DisplayName { get; set; }
            public string TypeValue { get; set; }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        private void PopulateReasonTypes()
        {
            cmbReasonType.Items.Clear();
            cmbReasonType.Items.Add(new ReasonTypeItem { DisplayName = "Stock Loss / Damage / Expiry (Indirect Expense - Group 12)", TypeValue = "Loss" });
            cmbReasonType.Items.Add(new ReasonTypeItem { DisplayName = "Stock Gain / Excess / Found (Indirect Income - Group 13)", TypeValue = "Gain" });
            cmbReasonType.Items.Add(new ReasonTypeItem { DisplayName = "Direct Production / Operation Loss (Direct Expense - Group 10)", TypeValue = "DirectLoss" });

            if (cmbReasonType.Items.Count > 0)
                cmbReasonType.SelectedIndex = 0;
        }

        private void SelectReasonType(string typeValue)
        {
            for (int i = 0; i < cmbReasonType.Items.Count; i++)
            {
                if (cmbReasonType.Items[i] is ReasonTypeItem item && item.TypeValue == typeValue)
                {
                    cmbReasonType.SelectedIndex = i;
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string reasonName = txtReasonName.Text != null ? txtReasonName.Text.Trim() : "";
            if (string.IsNullOrWhiteSpace(reasonName))
            {
                MessageBox.Show("Please enter a reason name.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReasonName.Focus();
                return;
            }

            if (cmbReasonType.SelectedItem is ReasonTypeItem selectedType)
            {
                ReasonModel.ReasonName = reasonName;
                ReasonModel.ReasonType = selectedType.TypeValue;
                ReasonModel.CompanyId = SessionContext.CompanyId;
                ReasonModel.BranchId = SessionContext.BranchId;

                string result = _repository.SaveStockAdjustmentReason(ReasonModel);
                if (result == "success")
                {
                    MessageBox.Show("Stock adjustment reason saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save reason: " + result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
