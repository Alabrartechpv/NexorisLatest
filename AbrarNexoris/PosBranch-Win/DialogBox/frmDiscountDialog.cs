using System;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmDiscountDialog : Form
    {
        // Properties to hold the values and return results
        public decimal TotalAmount { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal NewAmount { get; private set; }
        public decimal DiscountPercent { get; private set; }
        public bool IsPercentageDiscount { get; private set; }

        // Mode flags
        private bool isPercentageMode = false;
        private bool isNegativeDiscount = false;

        // Constructor with total amount parameter
        public frmDiscountDialog(decimal totalAmount)
        {
            InitializeComponent();
            TotalAmount = totalAmount;
            IsPercentageDiscount = false;
        }

        // Default constructor
        public frmDiscountDialog()
        {
            InitializeComponent();
            TotalAmount = 0;
            IsPercentageDiscount = false;
        }

        private void frmDiscountDialog_Load(object sender, EventArgs e)
        {
            // Set the form size to match the screenshot
            this.Size = new Size(420, 445);
            this.MaximumSize = new Size(420, 445);
            this.MinimumSize = new Size(420, 445);

            // Initialize the form with the current amount
            txtAmount.Text = TotalAmount.ToString("0.00");
            txtNewAmt.Text = TotalAmount.ToString("0.00");

            // Set initial mode to amount discount
            SetAmountDiscountMode();

            // Ensure focus is set to the discount textbox
            this.ActiveControl = txtDisc;
            txtDisc.Focus();
            txtDisc.SelectAll();
        }

        private void SetAmountDiscountMode()
        {
            // Get current value before changing mode
            string currentValue = txtDisc.Text;

            // Change to amount discount mode
            isPercentageMode = false;
            lblDisc.Text = "Disc$";

            // Show amount discount picturebox, hide percentage picturebox
            ultraPictureBoxamountdiscount.Visible = true;
            ultraPictureBoxpercentage.Visible = false;

            // Recalculate with the current value
            CalculateDiscountByAmount();

            // Ensure focus remains on the discount textbox
            this.ActiveControl = txtDisc;
            txtDisc.Focus();
            txtDisc.SelectAll();
        }

        private void SetPercentDiscountMode()
        {
            // Get current value before changing mode
            string currentValue = txtDisc.Text;

            // Change to percentage discount mode
            isPercentageMode = true;
            lblDisc.Text = "Disc%";

            // Hide amount discount picturebox, show percentage picturebox
            ultraPictureBoxamountdiscount.Visible = false;
            ultraPictureBoxpercentage.Visible = true;

            // Recalculate with the current value
            CalculateDiscountByPercentage();

            // Ensure focus remains on the discount textbox
            this.ActiveControl = txtDisc;
            txtDisc.Focus();
            txtDisc.SelectAll();
        }

        private void txtDisc_TextChanged(object sender, EventArgs e)
        {
            if (isPercentageMode)
                CalculateDiscountByPercentage();
            else
                CalculateDiscountByAmount();
        }

        private void txtDisc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyDiscount();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void CalculateDiscountByAmount()
        {
            try
            {
                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(txtDisc.Text))
                {
                    decimal.TryParse(txtDisc.Text, out discountAmount);
                }

                // Apply negative sign if needed
                if (isNegativeDiscount)
                    discountAmount = -discountAmount;

                // Calculate new amount
                decimal newAmount = TotalAmount - discountAmount;

                // Update display
                txtNewAmt.Text = newAmount.ToString("0.00");

                // Store values for return
                DiscountAmount = discountAmount;
                NewAmount = newAmount;

                // Calculate equivalent percentage for reference
                if (TotalAmount != 0)
                    DiscountPercent = (discountAmount / TotalAmount) * 100;
                else
                    DiscountPercent = 0;

                IsPercentageDiscount = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating discount: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateDiscountByPercentage()
        {
            try
            {
                decimal discountPercent = 0;
                if (!string.IsNullOrEmpty(txtDisc.Text))
                {
                    decimal.TryParse(txtDisc.Text, out discountPercent);
                }

                // Apply negative sign if needed
                if (isNegativeDiscount)
                    discountPercent = -discountPercent;

                // Calculate discount amount and new amount
                decimal discountAmount = TotalAmount * (discountPercent / 100);
                decimal newAmount = TotalAmount - discountAmount;

                // Update display
                txtNewAmt.Text = newAmount.ToString("0.00");

                // Store values for return
                DiscountAmount = discountAmount;
                DiscountPercent = discountPercent;
                NewAmount = newAmount;
                IsPercentageDiscount = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating discount: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyDiscount()
        {
            if (isPercentageMode)
                CalculateDiscountByPercentage();
            else
                CalculateDiscountByAmount();
        }

        private void btnNumber_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (txtDisc.Text == "0")
                txtDisc.Text = btn.Text;
            else
                txtDisc.Text += btn.Text;

            txtDisc.Focus();
            txtDisc.SelectionStart = txtDisc.Text.Length;
        }

        private void btnDecimal_Click(object sender, EventArgs e)
        {
            if (!txtDisc.Text.Contains("."))
            {
                txtDisc.Text += ".";
            }
            txtDisc.Focus();
            txtDisc.SelectionStart = txtDisc.Text.Length;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtDisc.Text = "0";
            isNegativeDiscount = false;
            txtDisc.Focus();
            txtDisc.SelectAll();
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            isNegativeDiscount = !isNegativeDiscount;

            // Recalculate based on current mode
            if (isPercentageMode)
                CalculateDiscountByPercentage();
            else
                CalculateDiscountByAmount();

            txtDisc.Focus();
            txtDisc.SelectionStart = txtDisc.Text.Length;
        }

        private void btnBackspace_Click(object sender, EventArgs e)
        {
            if (txtDisc.Text.Length > 0)
            {
                txtDisc.Text = txtDisc.Text.Substring(0, txtDisc.Text.Length - 1);
                if (string.IsNullOrEmpty(txtDisc.Text))
                    txtDisc.Text = "0";
            }
            txtDisc.Focus();
            txtDisc.SelectionStart = txtDisc.Text.Length;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            ApplyDiscount();
            DialogResult = DialogResult.OK;
            Close();
        }

        // Add event handlers for the pictureboxes
        private void ultraPictureBoxamountdiscount_Click(object sender, EventArgs e)
        {
            // Switch to percentage discount mode when amount discount picturebox is clicked
            SetPercentDiscountMode();

            // Focus on the discount textbox
            txtDisc.Focus();
            txtDisc.SelectAll();
        }

        private void ultraPictureBoxpercentage_Click(object sender, EventArgs e)
        {
            // Switch to amount discount mode when percentage discount picturebox is clicked
            SetAmountDiscountMode();

            // Focus on the discount textbox
            txtDisc.Focus();
            txtDisc.SelectAll();
        }
    }
}
