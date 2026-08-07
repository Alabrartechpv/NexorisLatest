using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class FrmReason : Form
    {
        private StockAdjustmentRepository _repository = new StockAdjustmentRepository();
        private List<StockAdjustmentReasonMaster> _reasonList = new List<StockAdjustmentReasonMaster>();
        private int _currentIndex = -1;
        private int _currentReasonId = 0;

        public FrmReason()
        {
            InitializeComponent();
            this.KeyPreview = true;
            WireEvents();
            PopulateReasonTypes();
        }

        private void WireEvents()
        {
            if (btnLookupF7 != null) btnLookupF7.Click += new EventHandler(btnF7List_Click);

            // Location X = 414: First (|<)
            if (ultraPanel3 != null) ultraPanel3.Click += new EventHandler(btnFirst_Click);
            if (ultraPictureBox2 != null) ultraPictureBox2.Click += new EventHandler(btnFirst_Click);

            // Location X = 447: Prev (<)
            if (ultraPanel9 != null) ultraPanel9.Click += new EventHandler(btnPrev_Click);
            if (ultraPictureBox4 != null) ultraPictureBox4.Click += new EventHandler(btnPrev_Click);

            // Location X = 480: Next (>)
            if (ultraPanel8 != null) ultraPanel8.Click += new EventHandler(btnNext_Click);
            if (ultraPictureBox6 != null) ultraPictureBox6.Click += new EventHandler(btnNext_Click);

            // Location X = 513: Last (>|)
            if (ultraPanel10 != null) ultraPanel10.Click += new EventHandler(btnLast_Click);
            if (ultraPictureBox5 != null) ultraPictureBox5.Click += new EventHandler(btnLast_Click);
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

        private void FrmReason_Load(object sender, EventArgs e)
        {
            LoadAllReasons();
        }

        private void LoadAllReasons()
        {
            try
            {
                _reasonList = _repository.GetStockAdjustmentReasons(SessionContext.BranchId);
                if (_reasonList != null && _reasonList.Count > 0)
                {
                    if (_currentIndex < 0 || _currentIndex >= _reasonList.Count)
                        _currentIndex = 0;
                    DisplayReasonRecord(_currentIndex);
                }
                else
                {
                    ClearRecord();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading reasons: " + ex.Message);
            }
        }

        private void DisplayReasonRecord(int index)
        {
            if (_reasonList == null || index < 0 || index >= _reasonList.Count)
                return;

            _currentIndex = index;
            var record = _reasonList[index];
            _currentReasonId = record.Id;
            txtReasonName.Text = record.ReasonName;

            for (int i = 0; i < cmbReasonType.Items.Count; i++)
            {
                if (cmbReasonType.Items[i] is ReasonTypeItem item && item.TypeValue == record.ReasonType)
                {
                    cmbReasonType.SelectedIndex = i;
                    break;
                }
            }
        }

        public void saveMaster()
        {
            SaveRecord();
        }

        public void Save()
        {
            SaveRecord();
        }

        public void SaveRecord()
        {
            btnSave_Click(this, EventArgs.Empty);
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

            string typeValue = "Loss";
            if (cmbReasonType.SelectedItem is ReasonTypeItem selectedType)
            {
                typeValue = selectedType.TypeValue;
            }

            StockAdjustmentReasonMaster model = new StockAdjustmentReasonMaster
            {
                Id = _currentReasonId,
                CompanyId = SessionContext.CompanyId,
                BranchId = SessionContext.BranchId,
                ReasonName = reasonName,
                ReasonType = typeValue
            };

            string result = _repository.SaveStockAdjustmentReason(model);
            if (result == "success")
            {
                MessageBox.Show("Reason saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAllReasons();
            }
            else
            {
                MessageBox.Show("Failed to save reason: " + result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            SaveRecord();
        }

        public void Clear()
        {
            ClearRecord();
        }

        public void ClearRecord()
        {
            _currentReasonId = 0;
            txtReasonName.Text = string.Empty;
            if (cmbReasonType.Items.Count > 0)
                cmbReasonType.SelectedIndex = 0;
            txtReasonName.Focus();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            ClearRecord();
        }

        public void Delete()
        {
            DeleteRecord();
        }

        public void DeleteRecord()
        {
            btn_delete_Click(this, EventArgs.Empty);
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (_currentReasonId <= 0)
            {
                MessageBox.Show("Please select a reason to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this reason?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string result = _repository.DeleteStockAdjustmentReason(_currentReasonId);
                if (result == "success")
                {
                    MessageBox.Show("Reason deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearRecord();
                    LoadAllReasons();
                }
                else
                {
                    MessageBox.Show("Failed to delete reason: " + result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void SelectReasonByName(string reasonName)
        {
            if (string.IsNullOrWhiteSpace(reasonName)) return;
            LoadAllReasons();
            if (_reasonList != null && _reasonList.Count > 0)
            {
                int foundIndex = _reasonList.FindIndex(r => r.ReasonName != null && r.ReasonName.Equals(reasonName, StringComparison.OrdinalIgnoreCase));
                if (foundIndex >= 0)
                {
                    DisplayReasonRecord(foundIndex);
                }
            }
        }

        private void btnF7List_Click(object sender, EventArgs e)
        {
            frmReasonDialog dialog = new frmReasonDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadAllReasons();
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            if (_reasonList == null || _reasonList.Count == 0)
                LoadAllReasons();

            if (_reasonList != null && _reasonList.Count > 0)
                DisplayReasonRecord(0);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_reasonList == null || _reasonList.Count == 0)
                LoadAllReasons();

            if (_reasonList != null && _reasonList.Count > 0)
            {
                int prevIndex = _currentIndex - 1;
                if (prevIndex < 0) prevIndex = 0;
                DisplayReasonRecord(prevIndex);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_reasonList == null || _reasonList.Count == 0)
                LoadAllReasons();

            if (_reasonList != null && _reasonList.Count > 0)
            {
                int nextIndex = _currentIndex + 1;
                if (nextIndex >= _reasonList.Count) nextIndex = _reasonList.Count - 1;
                DisplayReasonRecord(nextIndex);
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            if (_reasonList == null || _reasonList.Count == 0)
                LoadAllReasons();

            if (_reasonList != null && _reasonList.Count > 0)
                DisplayReasonRecord(_reasonList.Count - 1);
        }

        private void FrmReason_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                btnF7List_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F8)
            {
                SaveRecord();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                ClearRecord();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F12)
            {
                DeleteRecord();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
                e.Handled = true;
            }
        }
    }
}
