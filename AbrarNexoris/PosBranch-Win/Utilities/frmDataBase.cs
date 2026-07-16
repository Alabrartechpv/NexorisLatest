using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;

namespace PosBranch_Win.Utilities
{
    public partial class frmDataBase : Form
    {
        private readonly DatabaseRepository _dbRepo;
        private readonly CompanyRepo _companyRepo;

        public frmDataBase()
        {
            InitializeComponent();
            _dbRepo = new DatabaseRepository();
            _companyRepo = new CompanyRepo();
        }

        private void frmDataBase_Load(object sender, EventArgs e)
        {
            LogMessage("Console initialized. Ready for operations.");

            // Check for administrative permissions
            string userLevel = SessionContext.UserLevel ?? string.Empty;
            bool isAdmin = userLevel.IndexOf("admin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           userLevel.IndexOf("manager", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           userLevel.IndexOf("supervisor", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isAdmin)
            {
                MessageBox.Show("Database Maintenance is restricted to Admin, Manager, or Supervisor users.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Form loading Close requires BeginInvoke or post-load closing
                BeginInvoke(new MethodInvoker(Close));
                return;
            }

            LoadDefaultBackupPath();
        }

        private void LoadDefaultBackupPath()
        {
            try
            {
                string savedPath = string.Empty;

                // Retrieve the previously saved backup path from company settings
                if (AppSession.CompanyID > 0)
                {
                    LogMessage($"Fetching settings for Company ID: {AppSession.CompanyID}...");
                    var company = _companyRepo.GetCompanyById(AppSession.CompanyID);
                    if (company != null && !string.IsNullOrWhiteSpace(company.BackupPath))
                    {
                        savedPath = company.BackupPath.Trim();
                        LogMessage($"Saved backup path loaded: {savedPath}");
                    }
                    else
                    {
                        LogMessage("No backup path saved yet. Please browse and select a folder.");
                    }
                }
                else
                {
                    LogMessage("Session company context missing. Please browse and select a backup folder.");
                }

                txtBackupPath.Text = savedPath;
            }
            catch (Exception ex)
            {
                LogMessage($"Warning loading saved path: {ex.Message}");
                txtBackupPath.Text = string.Empty;
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select Destination Directory for Database Backups";
                dlg.ShowNewFolderButton = true;
                
                if (Directory.Exists(txtBackupPath.Text))
                {
                    dlg.SelectedPath = txtBackupPath.Text;
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtBackupPath.Text = dlg.SelectedPath;
                    LogMessage($"Selected backup directory: {dlg.SelectedPath}");
                }
            }
        }

        private void btnBrowseRestore_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select SQL Server Database Backup File";
                dlg.Filter = "SQL Server Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
                
                if (Directory.Exists(txtBackupPath.Text))
                {
                    dlg.InitialDirectory = txtBackupPath.Text;
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtRestorePath.Text = dlg.FileName;
                    LogMessage($"Selected restore file: {dlg.FileName}");
                }
            }
        }

        private async void btnBackup_Click(object sender, EventArgs e)
        {
            string backupFolder = txtBackupPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(backupFolder))
            {
                MessageBox.Show("Please specify a valid backup directory.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetFormState(false);
            progressBar.Style = ProgressBarStyle.Marquee;
            LogMessage("Starting database backup process...");

            try
            {
                // Run backup asynchronously to keep UI responsive
                var result = await Task.Run(() =>
                {
                    string backupFile;
                    string error;
                    bool success = _dbRepo.BackupDatabase(backupFolder, out backupFile, out error);
                    return new { Success = success, BackupFile = backupFile, Error = error };
                });

                if (result.Success)
                {
                    LogMessage($"Success: Backup file generated successfully at:");
                    LogMessage($" -> {result.BackupFile}");
                    progressBar.Style = ProgressBarStyle.Blocks;
                    progressBar.Value = 100;
                    MessageBox.Show($"Database backup completed successfully!\n\nSaved to:\n{result.BackupFile}", "Backup Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    LogMessage($"Backup Failed: {result.Error}");
                    progressBar.Style = ProgressBarStyle.Blocks;
                    progressBar.Value = 0;
                    MessageBox.Show($"Database backup failed:\n{result.Error}\n\nNote: If this is a remote database or permission error, ensure the SQL Server Service Account has read/write permission to the backup folder.", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Unexpected exception during database backup operation.", ex);
                LogMessage($"Unexpected exception during backup: {ex.Message}");
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
                MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetFormState(true);
            }
        }

        private async void btnRestore_Click(object sender, EventArgs e)
        {
            string restoreFile = txtRestorePath.Text.Trim();

            if (string.IsNullOrWhiteSpace(restoreFile))
            {
                MessageBox.Show("Please select a database backup file (.bak) to restore.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(restoreFile))
            {
                MessageBox.Show("The specified backup file does not exist on this machine. If the database is on a remote server, please ensure the file is placed on a server-accessible drive.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // High-priority confirmations for critical restore operation
            var firstConfirm = MessageBox.Show(
                "WARNING: Restoring the database will overwrite all current master data, products, configuration, and transactions.\n\n" +
                "Active cashier sessions and open forms will be disconnected.\n\n" +
                "Are you sure you want to proceed?",
                "CRITICAL: Confirm Database Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (firstConfirm != DialogResult.Yes)
                return;

            var finalConfirm = MessageBox.Show(
                "FINAL CONFIRMATION:\n" +
                "All existing client connections will be forcefully closed. Data entered since this backup was taken will be permanently lost.\n\n" +
                "Do you wish to proceed with the restoration?",
                "CRITICAL: Final Restoration Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop,
                MessageBoxDefaultButton.Button2);

            if (finalConfirm != DialogResult.Yes)
                return;

            SetFormState(false);
            progressBar.Style = ProgressBarStyle.Marquee;
            LogMessage("Starting database restore process...");
            LogMessage("Terminating database connections and setting SINGLE_USER mode...");

            try
            {
                // Run restore asynchronously to prevent UI freeze
                var result = await Task.Run(() =>
                {
                    string error;
                    bool success = _dbRepo.RestoreDatabase(restoreFile, out error);
                    return new { Success = success, Error = error };
                });

                if (result.Success)
                {
                    LogMessage("Success: Database restoration completed successfully.");
                    LogMessage("Database has been placed back in MULTI_USER mode.");
                    progressBar.Style = ProgressBarStyle.Blocks;
                    progressBar.Value = 100;

                    MessageBox.Show(
                        "Database restored successfully!\n\n" +
                        "The application will now restart to reload session state and master configurations.",
                        "Restoration Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Application.Restart();
                }
                else
                {
                    LogMessage($"Restore Failed: {result.Error}");
                    progressBar.Style = ProgressBarStyle.Blocks;
                    progressBar.Value = 0;
                    MessageBox.Show($"Database restore failed:\n{result.Error}\n\nEnsure SQL Server has read access to the backup file and it is a valid SQL Server backup file.", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Unexpected exception during database restore operation.", ex);
                LogMessage($"Unexpected exception during restore: {ex.Message}");
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
                MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetFormState(true);
            }
        }

        private void SetFormState(bool enabled)
        {
            btnBackup.Enabled = enabled;
            btnRestore.Enabled = enabled;
            btnBrowseBackup.Enabled = enabled;
            btnBrowseRestore.Enabled = enabled;
            txtBackupPath.Enabled = enabled;
            txtRestorePath.Enabled = enabled;
        }

        private void LogMessage(string text)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lstLogs.Items.Add($"[{timeStamp}] {text}");
            
            // Auto-scroll log window to latest entry
            lstLogs.TopIndex = lstLogs.Items.Count - 1;
        }
    }
}
