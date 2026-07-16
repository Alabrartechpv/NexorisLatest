using System;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmCalculator : Form
    {
        private decimal _result = 0;
        private string _operation = "";
        private bool _newInput = true;
        private decimal _firstValue = 0;

        public decimal CalculatedValue { get; private set; }

        public frmCalculator(decimal initialValue = 0)
        {
            InitializeComponent();
            txtDisplay.Text = initialValue.ToString();
            _firstValue = initialValue;
            _result = initialValue;
        }

        private void frmCalculator_Load(object sender, EventArgs e)
        {
            SetupEventHandlers();

            // Focus on the display textbox when form loads
            txtDisplay.Focus();
            txtDisplay.SelectAll();
        }

        private void SetupEventHandlers()
        {
            // Digit buttons
            btn0.Click += DigitButton_Click;
            btn1.Click += DigitButton_Click;
            btn2.Click += DigitButton_Click;
            btn3.Click += DigitButton_Click;
            btn4.Click += DigitButton_Click;
            btn5.Click += DigitButton_Click;
            btn6.Click += DigitButton_Click;
            btn7.Click += DigitButton_Click;
            btn8.Click += DigitButton_Click;
            btn9.Click += DigitButton_Click;
            btnDecimal.Click += DigitButton_Click;

            // Operation buttons
            btnPlus.Click += OperationButton_Click;
            btnMinus.Click += OperationButton_Click;
            btnMultiply.Click += OperationButton_Click;
            btnDivide.Click += OperationButton_Click;
            btnEquals.Click += OperationButton_Click;

            // Special buttons
            btnClear.Click += BtnClear_Click;
            btnClose.Click += BtnClose_Click;
            btnBackspace.Click += BtnBackspace_Click;
            btnEnter.Click += BtnEnter_Click;
        }

        private void DigitButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string digit = button.Text;

            if (_newInput)
            {
                txtDisplay.Text = "";
                _newInput = false;
            }

            // Handle decimal point - only allow one
            if (digit == "." && txtDisplay.Text.Contains("."))
            {
                return;
            }

            txtDisplay.Text += digit;
        }

        private void OperationButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            // First, complete any pending operation
            if (!_newInput)
            {
                Calculate();
                _newInput = true;
            }

            // Set current operation (except for equals)
            if (button.Text != "=")
            {
                _operation = button.Text;
            }
            else
            {
                _operation = "";
            }

            _firstValue = decimal.Parse(txtDisplay.Text);
        }

        private void Calculate()
        {
            if (string.IsNullOrEmpty(_operation))
            {
                return;
            }

            decimal secondValue = decimal.Parse(txtDisplay.Text);

            switch (_operation)
            {
                case "+":
                    _result = _firstValue + secondValue;
                    break;
                case "-":
                    _result = _firstValue - secondValue;
                    break;
                case "*":
                    _result = _firstValue * secondValue;
                    break;
                case "/":
                    if (secondValue != 0)
                    {
                        _result = _firstValue / secondValue;
                    }
                    else
                    {
                        MessageBox.Show("Cannot divide by zero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    break;
            }

            txtDisplay.Text = _result.ToString();
            _firstValue = _result;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtDisplay.Text = "0";
            _operation = "";
            _newInput = true;
            _result = 0;
            _firstValue = 0;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnBackspace_Click(object sender, EventArgs e)
        {
            if (txtDisplay.Text.Length > 0)
            {
                txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
            }

            if (string.IsNullOrEmpty(txtDisplay.Text))
            {
                txtDisplay.Text = "0";
                _newInput = true;
            }
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            // If there's a pending calculation, complete it
            if (!_newInput && !string.IsNullOrEmpty(_operation))
            {
                Calculate();
            }

            // Return the current value
            CalculatedValue = decimal.Parse(txtDisplay.Text);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
