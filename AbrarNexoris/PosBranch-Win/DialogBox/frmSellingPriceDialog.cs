using System;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSellingPriceDialog : Form
    {
        private decimal _currentValue = 0;
        private bool _firstInput = true;

        public decimal SellingPrice { get; private set; }

        public frmSellingPriceDialog(decimal currentValue)
        {
            InitializeComponent();
            _currentValue = currentValue;
            txtSellingPrice.Text = _currentValue.ToString();

            // Wire up events
            txtSellingPrice.KeyPress += TxtSellingPrice_KeyPress;
            txtSellingPrice.TextChanged += TxtSellingPrice_TextChanged;
            this.Load += FrmSellingPriceDialog_Load;

            // Set up button click handlers
            SetupButtonHandlers();
        }

        private void FrmSellingPriceDialog_Load(object sender, EventArgs e)
        {
            // Set focus to the selling price textbox and select all text
            txtSellingPrice.Focus();
            txtSellingPrice.SelectAll();
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
                        string buttonText = btn.Text; // Capture the button text
                        btn.Click += (s, e) =>
                        {
                            if (_firstInput)
                            {
                                txtSellingPrice.Text = buttonText;
                                _firstInput = false;
                            }
                            else
                            {
                                txtSellingPrice.Text += buttonText;
                            }
                        };
                    }
                }
            }

            // Set up special button handlers
            btnClose.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            btnClear.Click += (s, e) =>
            {
                txtSellingPrice.Text = "";
                _firstInput = false;
            };

            btnBackspace.Click += (s, e) =>
            {
                if (txtSellingPrice.Text.Length > 0)
                {
                    txtSellingPrice.Text = txtSellingPrice.Text.Substring(0, txtSellingPrice.Text.Length - 1);
                    _firstInput = false;
                }
            };

            btnEnter.Click += (s, e) => AcceptSellingPrice();

            // Add handler for calculator button
            btnCalc.Click += BtnCalc_Click;

            // Add handler to ensure textbox keeps focus after button clicks
            foreach (Control control in buttonsPanel.Controls)
            {
                if (control is Button btn)
                {
                    // For numeric and operator buttons, add a handler to refocus the textbox
                    btn.Click += (s, e) =>
                    {
                        // After button click, put focus back on the textbox
                        txtSellingPrice.Focus();
                    };
                }
            }
        }

        private void BtnCalc_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the current selling price value
                decimal currentValue = 0;
                if (!string.IsNullOrEmpty(txtSellingPrice.Text))
                {
                    decimal.TryParse(txtSellingPrice.Text, out currentValue);
                }

                // Open our custom calculator form with the current value
                using (frmCalculator calculator = new frmCalculator(currentValue))
                {
                    // Position the calculator form to appear in the center of the screen
                    calculator.StartPosition = FormStartPosition.CenterScreen;

                    // Show the calculator form as a dialog
                    if (calculator.ShowDialog() == DialogResult.OK)
                    {
                        // Update the selling price with the calculated value
                        txtSellingPrice.Text = calculator.CalculatedValue.ToString();

                        // Set focus back to the selling price textbox
                        txtSellingPrice.Focus();
                        txtSellingPrice.SelectAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening calculator: {ex.Message}",
                    "Calculator Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSellingPrice_TextChanged(object sender, EventArgs e)
        {
            // Validate and format the selling price as needed
            if (!string.IsNullOrWhiteSpace(txtSellingPrice.Text))
            {
                if (decimal.TryParse(txtSellingPrice.Text, out decimal val))
                {
                    // Valid selling price, no need to do anything
                }
                else
                {
                    // Invalid selling price, revert to previous valid value
                    txtSellingPrice.Text = _currentValue.ToString();

                    // Select all text for easier correction
                    txtSellingPrice.SelectAll();
                }
            }
        }

        private void TxtSellingPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, decimal point, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && txtSellingPrice.Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }

            // If Enter key is pressed
            if (e.KeyChar == (char)Keys.Enter)
            {
                AcceptSellingPrice();
            }
            else if (!char.IsControl(e.KeyChar))
            {
                // Any non-control key press means user is typing
                _firstInput = false;
            }
        }

        private void AcceptSellingPrice()
        {
            if (decimal.TryParse(txtSellingPrice.Text, out decimal price))
            {
                if (price > 0)
                {
                    SellingPrice = price;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a price greater than zero.", "Invalid Price", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Set focus back to the selling price textbox
                    txtSellingPrice.Focus();
                    txtSellingPrice.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid price.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Set focus back to the selling price textbox
                txtSellingPrice.Focus();
                txtSellingPrice.SelectAll();
            }
        }

        // Override the OnShown method to ensure the textbox gets focus
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Set focus to the textbox when the form is shown
            txtSellingPrice.Focus();
            txtSellingPrice.SelectAll();
        }
    }
}