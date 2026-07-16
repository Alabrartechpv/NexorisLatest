using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public partial class PurchaseEdit : Form
    {
        private UltraGridRow _currentRow;
        public bool IsUpdated { get; private set; }

        // Flag and timer for tracking double Enter presses
        private DateTime _lastEnterPress = DateTime.MinValue;
        private bool _isDoubleEnter = false;
        private bool _hasEdited = false;
        private Dictionary<Control, string> _originalValues = new Dictionary<Control, string>();

        public PurchaseEdit()
        {
            InitializeComponent();
            IsUpdated = false;

            // Add event handlers for the picture boxes
            ultraPictureBox5.Click += btnSave_Click;
            ultraPictureBox1.Click += btnCancel_Click;

            // Add KeyDown and TextChanged event handlers for all text editors

            ultraTextEditor5.KeyDown += TextEditor_KeyDown;
            ultraTextEditor3.KeyDown += TextEditor_KeyDown;
            ultraTextEditor4.KeyDown += TextEditor_KeyDown;
            ultraTextEditor6.KeyDown += TextEditor_KeyDown;


            ultraTextEditor5.TextChanged += TextEditor_TextChanged;
            ultraTextEditor3.TextChanged += TextEditor_TextChanged;
            ultraTextEditor4.TextChanged += TextEditor_TextChanged;
            ultraTextEditor6.TextChanged += TextEditor_TextChanged;

            ultraTextEditor5.Enter += TextEditor_Enter;
            ultraTextEditor3.Enter += TextEditor_Enter;
            ultraTextEditor4.Enter += TextEditor_Enter;
            ultraTextEditor6.Enter += TextEditor_Enter;

            // Set the tab order
            SetTabOrder();
        }

        private void SetTabOrder()
        {
            // First, reset all tab indices to a high value
            foreach (Control c in this.Controls)
            {
                c.TabIndex = 100;
            }

            // Set specific tab order according to desired sequence
            ultraTextEditor5.TabIndex = 1;
            ultraTextEditor3.TabIndex = 2;
            ultraTextEditor4.TabIndex = 3;
            ultraTextEditor6.TabIndex = 4;
        }

        public void SetGridRow(UltraGridRow row)
        {
            _currentRow = row;
            LoadDataToFields();
        }

        private void LoadDataToFields()
        {
            if (_currentRow == null) return;

            // Load grid data to text fields

            ultraTextEditor5.Text = _currentRow.Cells["RetailPrice"].Value?.ToString() ?? string.Empty;

            // Make sure Cost field is properly loaded
            if (_currentRow.Cells["Cost"].Value != null)
                ultraTextEditor4.Text = _currentRow.Cells["Cost"].Value.ToString();
            else
                ultraTextEditor4.Text = "0";

            ultraTextEditor3.Text = _currentRow.Cells["Free"].Value?.ToString() ?? "0";
            ultraTextEditor6.Text = _currentRow.Cells["Qty"].Value?.ToString() ?? string.Empty;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentRow == null) return;

                // Update grid data with edited values



                float retailPrice = 0;
                float.TryParse(ultraTextEditor5.Text, out retailPrice);
                _currentRow.Cells["RetailPrice"].Value = retailPrice;

                float free = 0;
                float.TryParse(ultraTextEditor3.Text, out free);
                _currentRow.Cells["Free"].Value = free;

                float cost = 0;
                float.TryParse(ultraTextEditor4.Text, out cost);
                _currentRow.Cells["Cost"].Value = cost;

                float qty = 0;
                float.TryParse(ultraTextEditor6.Text, out qty);
                _currentRow.Cells["Qty"].Value = qty;

                // Calculate amount and total amount
                float amount = cost * qty;
                _currentRow.Cells["Amount"].Value = amount;
                _currentRow.Cells["TotalAmount"].Value = amount;

                // Set flag indicating data was updated
                IsUpdated = true;

                // Close the form
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void TextEditor_Enter(object sender, EventArgs e)
        {
            // Store original value when field gets focus
            var control = sender as Control;
            if (control != null && !_originalValues.ContainsKey(control))
            {
                _originalValues[control] = (control as Infragistics.Win.UltraWinEditors.UltraTextEditor)?.Text ?? string.Empty;
            }
            _hasEdited = false;
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            var control = sender as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            if (control != null && _originalValues.ContainsKey(control))
            {
                // Only mark as edited if the value has actually changed from original
                _hasEdited = control.Text != _originalValues[control];
            }
        }

        private void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // If field was edited, save immediately
                if (_hasEdited)
                {
                    btnSave_Click(sender, e);
                    return;
                }
                else
                {
                    // If not edited, just move to next field
                    MoveToNextControl(sender as Control);
                }
            }
            else if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // Get the current control and move to the next
                MoveToNextControl(sender as Control);
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // Move to next control (same as Tab)
                MoveToNextControl(sender as Control);
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // Move to previous control
                MoveToPreviousControl(sender as Control);
            }
        }

        private void MoveToNextControl(Control currentControl)
        {

            if (currentControl == ultraTextEditor5)
                ultraTextEditor3.Focus();
            else if (currentControl == ultraTextEditor3)
                ultraTextEditor4.Focus();
            else if (currentControl == ultraTextEditor4)
                ultraTextEditor6.Focus();
            else if (currentControl == ultraTextEditor6)
                ultraTextEditor5.Focus(); // Loop back to the first control
        }

        private void MoveToPreviousControl(Control currentControl)
        {

            if (currentControl == ultraTextEditor3)
                ultraTextEditor5.Focus();
            else if (currentControl == ultraTextEditor4)
                ultraTextEditor3.Focus();
            else if (currentControl == ultraTextEditor6)
                ultraTextEditor4.Focus();
        }

        private void ultraPanel3_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void PurchaseEdit_Load(object sender, EventArgs e)
        {

        }
    }
}
