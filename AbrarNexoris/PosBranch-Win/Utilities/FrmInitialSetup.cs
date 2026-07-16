using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Utilities
{
    public partial class FrmInitialSetup : Form
    {
        private TextBox activeTextBox = null;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        public FrmInitialSetup()
        {
            InitializeComponent();
            SetupRoundedDesign();
        }

        private void SetupRoundedDesign()
        {
            // Set form rounded corners
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 25, 25));

            // Setup buttons
            btnInitialize.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnInitialize.Width, btnInitialize.Height, 15, 15));
            btnCancel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnCancel.Width, btnCancel.Height, 15, 15));
        }

        private void FrmInitialSetup_Load(object sender, EventArgs e)
        {
            txtCompanyName.Focus();
        }

        private void btnInitialize_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Please enter the Company Name.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompanyName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBranchName.Text))
            {
                MessageBox.Show("Please enter the Branch Name.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBranchName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter the Admin Password.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match. Please verify and re-enter.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                btnInitialize.Enabled = false;
                btnInitialize.Text = "Initializing...";
                Cursor = Cursors.WaitCursor;

                bool success = InitialSetupHelper.InitializeDatabase(
                    txtCompanyName.Text.Trim(),
                    txtCompanyCaption.Text.Trim(),
                    txtBranchName.Text.Trim(),
                    txtBranchAddress.Text.Trim(),
                    txtBranchPhone.Text.Trim(),
                    txtPassword.Text
                );

                if (success)
                {
                    using (frmSuccesMsg successForm = new frmSuccesMsg())
                    {
                        successForm.ShowDialog();
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Seeding failed without throwing an exception. Please verify database connection.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during database initialization:\n{ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInitialize.Enabled = true;
                btnInitialize.Text = "Initialize System";
                Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel setup and exit the application?", "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // Close app if setup is cancelled/closed without success
        private void FrmInitialSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK && e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }
    }
}
