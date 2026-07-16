using PosBranch_Win.DialogBox;
using System;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmQuantityDialog : Form
    {
        private decimal _currentValue = 0;

        public decimal Quantity { get; private set; }

        public frmQuantityDialog(decimal currentValue)
        {
            InitializeComponent();
            _currentValue = currentValue;
            txtQuantity.Text = _currentValue.ToString();

            // Wire up events
            txtQuantity.KeyPress += TxtQuantity_KeyPress;
            txtQuantity.TextChanged += TxtQuantity_TextChanged;

            // Add Load event to focus on textbox when form opens
            this.Load += FrmQuantityDialog_Load;

            // Set up button click handlers
            SetupButtonHandlers();
        }

        private void SetupButtonHandlers()
        {
            // Set up numeric button handlers (0-9 and decimal)
            foreach (Control control in buttonsPanel.Controls)
            {
                if (control is Button btn)
                {
                    if (int.TryParse(btn.Text, out _) || btn.Text == ".")
                    {
                        btn.Click += (s, e) => txtQuantity.Text += btn.Text;
                    }
                }
            }

            // Set up special button handlers
            btnClose.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            btnClear.Click += (s, e) => txtQuantity.Text = "";

            btnBackspace.Click += (s, e) =>
            {
                if (txtQuantity.Text.Length > 0)
                {
                    txtQuantity.Text = txtQuantity.Text.Substring(0, txtQuantity.Text.Length - 1);
                }
            };

            btnEnter.Click += (s, e) => AcceptQuantity();

            // Add handler for calculator button
            btnCalc.Click += BtnCalc_Click;

            // Add handler to ensure textbox keeps focus after button clicks
            foreach (Control control in buttonsPanel.Controls)
            {
                if (control is Button btn)
                {
                    // For all buttons, add a handler to refocus the textbox
                    btn.Click += (s, e) =>
                    {
                        // After button click, put focus back on the textbox
                        txtQuantity.Focus();
                        txtQuantity.SelectionStart = txtQuantity.Text.Length;
                    };
                }
            }
        }

        private void BtnCalc_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the current quantity value
                decimal currentValue = 0;
                if (!string.IsNullOrEmpty(txtQuantity.Text))
                {
                    decimal.TryParse(txtQuantity.Text, out currentValue);
                }

                // Open our custom calculator form with the current value
                using (frmCalculator calculator = new frmCalculator(currentValue))
                {
                    // Position the calculator form to appear in the center of the screen
                    calculator.StartPosition = FormStartPosition.CenterScreen;

                    // Show the calculator form as a dialog
                    if (calculator.ShowDialog() == DialogResult.OK)
                    {
                        // Update the quantity with the calculated value
                        txtQuantity.Text = calculator.CalculatedValue.ToString();

                        // Set focus back to the quantity textbox
                        txtQuantity.Focus();
                        txtQuantity.SelectAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening calculator: {ex.Message}",
                    "Calculator Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtQuantity_TextChanged(object sender, EventArgs e)
        {
            // Validate and format the quantity as needed
            if (!string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                if (decimal.TryParse(txtQuantity.Text, out decimal val))
                {
                    // Valid quantity, no need to do anything
                }
                else
                {
                    // Invalid quantity, revert to previous valid value
                    txtQuantity.Text = _currentValue.ToString();
                }
            }
        }

        private void TxtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, decimal point, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && txtQuantity.Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            // If Enter key is pressed
            if (e.KeyChar == (char)Keys.Enter)
            {
                AcceptQuantity();
            }
        }

        private void AcceptQuantity()
        {
            if (decimal.TryParse(txtQuantity.Text, out decimal quantity))
            {
                if (quantity > 0)
                {
                    Quantity = quantity;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a quantity greater than zero.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FrmQuantityDialog_Load(object sender, EventArgs e)
        {
            // Focus on quantity textbox and select all text when form loads
            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }

        // Override the OnShown method to ensure the textbox gets focus
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Set focus to the textbox when the form is shown
            txtQuantity.Focus();
            txtQuantity.SelectAll();
        }
    }
}