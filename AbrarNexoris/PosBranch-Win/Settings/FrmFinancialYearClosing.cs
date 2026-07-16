using ModelClass;
using ModelClass.Settings;
using Repository.SettingsRepo;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public partial class FrmFinancialYearClosing : Form
    {
        private FinancialYearRepository _repo;
        private FinancialYearModel _currentYear;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public FrmFinancialYearClosing()
        {
            InitializeComponent();
            _repo = new FinancialYearRepository();
        }

        private void FrmFinancialYearClosing_Load(object sender, EventArgs e)
        {
            try
            {
                // Round buttons and panels like in login
                btnVerify.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnVerify.Width, btnVerify.Height, 10, 10));
                btnRunClosing.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnRunClosing.Width, btnRunClosing.Height, 10, 10));
                btnClose.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 10, 10));

                // Wire up change events on next year setup inputs to reset verification state
                dtpNewFrom.ValueChanged += (s, ev) => ResetVerification();
                dtpNewTo.ValueChanged += (s, ev) => ResetVerification();
                txtNewId.TextChanged += (s, ev) => ResetVerification();

                LoadFinancialYearData();

                // Home applies its global theme to hosted forms after construction.
                // Re-apply action styling once the form is fully hosted so button
                // text never becomes white-on-white.
                BeginInvoke(new Action(ApplyActionButtonStyles));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing screen: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyActionButtonStyles()
        {
            btnVerify.Appearance.BackColor = Color.FromArgb(37, 99, 235);
            btnVerify.Appearance.BackColor2 = Color.FromArgb(29, 78, 216);
            btnVerify.Appearance.ForeColor = Color.White;

            btnRunClosing.Appearance.BackColor = btnRunClosing.Enabled
                ? Color.FromArgb(22, 163, 74)
                : Color.FromArgb(226, 232, 240);
            btnRunClosing.Appearance.BackColor2 = btnRunClosing.Enabled
                ? Color.FromArgb(21, 128, 61)
                : Color.FromArgb(203, 213, 225);
            btnRunClosing.Appearance.ForeColor = btnRunClosing.Enabled
                ? Color.White
                : Color.FromArgb(100, 116, 139);

            btnClose.Appearance.BackColor = Color.White;
            btnClose.Appearance.BackColor2 = Color.White;
            btnClose.Appearance.ForeColor = Color.FromArgb(51, 65, 85);

            btnVerify.Refresh();
            btnRunClosing.Refresh();
            btnClose.Refresh();
        }

        private void ResetVerification()
        {
            btnRunClosing.Enabled = false;
            btnRunClosing.BackColor = Color.Gray;
            ApplyActionButtonStyles();
            if (lstChecks.Items.Count > 0 && !lstChecks.Items[lstChecks.Items.Count - 1].ToString().Contains("Re-run verification"))
            {
                lstChecks.Items.Add("Inputs modified. Please re-run verification before closing.");
            }
        }

        private void LoadFinancialYearData()
        {
            lstChecks.Items.Clear();

            // 1. Session Context Check
            if (SessionContext.CompanyId <= 0 || SessionContext.BranchId <= 0)
            {
                lstChecks.Items.Add("[FAIL] Session context is uninitialized.");
                lstChecks.Items.Add("       Please re-login to configure company and branch details.");
                btnVerify.Enabled = false;
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                return;
            }

            int companyId = SessionContext.CompanyId;

            // 2. Database Active Year Check — exceptions propagate as a hard failure
            try
            {
                _currentYear = _repo.GetCurrentFinancialYear(companyId);
            }
            catch (Exception ex)
            {
                lblCurId.Text = "Year ID: Error";
                lblCurFrom.Text = "Date From: --";
                lblCurTo.Text = "Date To: --";
                txtNewId.Text = "";
                lstChecks.Items.Add($"[FAIL] Database error reading financial year: {ex.Message}");
                lstChecks.Items.Add("       Check your database connection and try again.");
                btnVerify.Enabled = false;
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                return;
            }

            if (_currentYear != null)
            {
                lblCurId.Text = $"Year ID:  {_currentYear.FinYearID}";
                lblCurFrom.Text = $"Date From:  {_currentYear.FinYearFrom:dd-MMM-yyyy}";
                lblCurTo.Text = $"Date To:  {_currentYear.FinYearTo:dd-MMM-yyyy}";

                // Set up next year proposals
                txtNewId.Text = (_currentYear.FinYearID + 1).ToString();
                dtpNewFrom.Value = _currentYear.FinYearTo.AddDays(1);
                dtpNewTo.Value = _currentYear.FinYearTo.AddYears(1);

                btnVerify.Enabled = true;
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                lstChecks.Items.Add("System ready. Click 'Run Verifications' before performing year-end closing.");
            }
            else
            {
                lblCurId.Text = "Year ID: Not Found";
                lblCurFrom.Text = "Date From: --";
                lblCurTo.Text = "Date To: --";

                txtNewId.Text = "";
                lstChecks.Items.Add("[FAIL] Active financial year not found in the database.");
                lstChecks.Items.Add("       You cannot perform a closing without an active financial year.");
                btnVerify.Enabled = false;
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
            }
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            lstChecks.Items.Clear();
            lstChecks.Items.Add("Running pre-closing checks...");

            if (SessionContext.CompanyId <= 0 || SessionContext.BranchId <= 0)
            {
                lstChecks.Items.Add("[FAIL] Invalid Session Context.");
                return;
            }

            if (_currentYear == null)
            {
                lstChecks.Items.Add("[FAIL] Active financial year is required.");
                return;
            }

            // 1. Strict Date Validations
            DateTime proposedFrom = Convert.ToDateTime(dtpNewFrom.Value).Date;
            DateTime proposedTo = Convert.ToDateTime(dtpNewTo.Value).Date;
            DateTime expectedFrom = _currentYear.FinYearTo.AddDays(1).Date;

            if (proposedFrom != expectedFrom)
            {
                lstChecks.Items.Add($"[FAIL] Next Start Date must be exactly {expectedFrom:dd-MMM-yyyy} (current end + 1 day).");
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                return;
            }

            if (proposedTo < proposedFrom)
            {
                lstChecks.Items.Add("[FAIL] Next End Date must be after the start date.");
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                return;
            }

            int companyId = SessionContext.CompanyId;

            // 2. Company-Wide Cashier Sessions Check
            try
            {
                bool hasOpenSessions = _repo.HasOpenSessions(companyId);

                if (hasOpenSessions)
                {
                    lstChecks.Items.Add("[FAIL] Active cashier/counter sessions found in the company.");
                    lstChecks.Items.Add("       Please close all counter shift sessions in all branches before closing.");
                    btnRunClosing.Enabled = false;
                    btnRunClosing.BackColor = Color.Gray;
                    ApplyActionButtonStyles();
                }
                else
                {
                    lstChecks.Items.Add("[SUCCESS] No active counter sessions detected company-wide.");
                    lstChecks.Items.Add("[SUCCESS] Ready for rollover. Please verify new year start and end dates.");
                    btnRunClosing.Enabled = true;
                    btnRunClosing.BackColor = Color.ForestGreen;
                    ApplyActionButtonStyles();
                }
            }
            catch (Exception ex)
            {
                lstChecks.Items.Add($"[ERROR] Database check failed: {ex.Message}");
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                MessageBox.Show($"Verification error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRunClosing_Click(object sender, EventArgs e)
        {
            // Block immediately if any session context values are missing.
            // This is an irreversible operation — the audit trail must be accurate.
            if (SessionContext.CompanyId <= 0 || SessionContext.BranchId <= 0)
            {
                MessageBox.Show("Invalid Session Context: Company or Branch not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (SessionContext.UserId <= 0)
            {
                MessageBox.Show(
                    "Rollover blocked: User ID is not set in the current session.\n" +
                    "Please log out and log in again before performing year-end closing.",
                    "Invalid Session", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (SessionContext.CounterId <= 0)
            {
                MessageBox.Show(
                    "Rollover blocked: Counter ID is not set in the current session.\n" +
                    "Please log out and log in again before performing year-end closing.",
                    "Invalid Session", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_currentYear == null)
            {
                MessageBox.Show("Active financial year is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int companyId = SessionContext.CompanyId;
            int branchId = SessionContext.BranchId;
            int oldYearId = _currentYear.FinYearID;

            // Defensive parse: Convert.ToInt32 would throw on empty/invalid input
            if (!int.TryParse(txtNewId.Text?.Trim(), out int newYearId) || newYearId <= 0)
            {
                MessageBox.Show("New Financial Year ID is invalid. Please enter a positive integer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Validate dates immediately before running rollover
            DateTime proposedFrom = Convert.ToDateTime(dtpNewFrom.Value).Date;
            DateTime proposedTo = Convert.ToDateTime(dtpNewTo.Value).Date;
            DateTime expectedFrom = _currentYear.FinYearTo.AddDays(1).Date;

            if (proposedFrom != expectedFrom || proposedTo < proposedFrom)
            {
                MessageBox.Show("Rollover aborted: Date ranges are invalid. Please run verification again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnRunClosing.Enabled = false;
                btnRunClosing.BackColor = Color.Gray;
                return;
            }

            var confirm = MessageBox.Show(
                $"Are you sure you want to perform the Year-End Closing?\n\n" +
                $"This will transition the system to Financial Year ID {newYearId} ({proposedFrom:dd-MMM-yyyy} to {proposedTo:dd-MMM-yyyy}).\n\n" +
                $"This action resets transaction sequences and is irreversible.",
                "Confirm Financial Year Closing",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            // 2. Final session re-check AFTER the dialog closes — a cashier could have logged in
            //    during the seconds the confirmation was on screen.
            try
            {
                if (_repo.HasOpenSessions(companyId))
                {
                    MessageBox.Show(
                        "Rollover aborted: A counter/cashier session was opened while the confirmation dialog was open.\n" +
                        "Please close all sessions and re-run verification.",
                        "Session Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnRunClosing.Enabled = false;
                    btnRunClosing.BackColor = Color.Gray;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rollover aborted: Failed to verify active sessions: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Show progress UI
                progressBar.Visible = true;
                lblProgressStatus.Visible = true;
                btnVerify.Enabled = false;
                btnRunClosing.Enabled = false;
                btnClose.Enabled = false;

                progressBar.Value = 10;
                lblProgressStatus.Text = "Status: Backup verification and validation checks...";
                Application.DoEvents();

                progressBar.Value = 30;
                lblProgressStatus.Text = "Status: Transferring active ledger balances to opening entries...";
                Application.DoEvents();

                progressBar.Value = 60;
                lblProgressStatus.Text = "Status: Carrying forward inventory opening stocks...";
                Application.DoEvents();

                // Run stored procedure rollover asynchronously to prevent freezing.
                // branchId is NOT passed — the SP iterates all branches for the company.
                string response = await Task.Run(() => _repo.PerformFinancialYearClosing(
                    companyId,
                    oldYearId,
                    newYearId,
                    proposedFrom,
                    proposedTo,
                    SessionContext.UserName ?? "Admin",
                    SessionContext.UserId,    // guaranteed > 0 by guards above
                    SessionContext.CounterId  // guaranteed > 0 by guards above
                ));

                if (response.Equals("Success", StringComparison.OrdinalIgnoreCase))
                {
                    progressBar.Value = 100;
                    lblProgressStatus.Text = "Status: Year-End Closing Completed Successfully!";
                    Application.DoEvents();

                    MessageBox.Show(
                        "Financial Year Closing completed successfully!\n\n" +
                        "The application will now restart to load the settings for the new financial year.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Clean restart to force fresh session context reload
                    Application.Restart();
                }
                else
                {
                    throw new Exception(response);
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblProgressStatus.Visible = false;
                btnVerify.Enabled = true;
                btnRunClosing.Enabled = false; // Lock closing run until successful re-verification
                btnRunClosing.BackColor = Color.Gray;
                btnClose.Enabled = true;

                MessageBox.Show($"Year End Closing Failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
