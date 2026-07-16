using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class Connection : Form
    {
        private const string ConfigPath = @"C:\Connection\Config.txt";

        public Connection()
        {
            InitializeComponent();
        }

        private void Connection_Load(object sender, EventArgs e)
        {
            LoadCurrentConfiguration();
        }

        private void LoadCurrentConfiguration()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string content = File.ReadAllText(ConfigPath).Trim();
                    string[] parts = content.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string part in parts)
                    {
                        string[] keyValue = part.Split(new[] { '=' }, 2);
                        if (keyValue.Length != 2)
                            continue;

                        string key = keyValue[0].Trim().ToLower();
                        string val = keyValue[1].Trim();

                        if (key == "data source")
                            txtServer.Text = val;
                        else if (key == "initial catalog")
                            txtDatabase.Text = val;
                        else if (key == "user id")
                            txtUsername.Text = val;
                        else if (key == "password")
                            txtPassword.Text = val;
                        else if (key == "counterid")
                            txtCounterId.Text = val;
                    }
                }
                else
                {
                    // Default values if no file exists
                    txtServer.Text = "localhost\\SQLEXPRESS";
                    txtDatabase.Text = "NexorisPOS";
                    txtUsername.Text = "sa";
                    txtPassword.Text = "";
                    txtCounterId.Text = "1";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load existing configuration:\n{ex.Message}", "Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string BuildConnectionString()
        {
            return $"Data Source={txtServer.Text.Trim()};Initial Catalog={txtDatabase.Text.Trim()};User ID={txtUsername.Text.Trim()};Password={txtPassword.Text.Trim()};";
        }

        private bool TestConnection(out string errorMessage)
        {
            errorMessage = null;
            string connString = BuildConnectionString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (TestConnection(out string error))
                {
                    MessageBox.Show("Connection test succeeded! Database is reachable.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Connection test failed:\n{error}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Input Validation
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("Please enter the SQL Server instance name.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServer.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show("Please enter the Database Name.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDatabase.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter the Database Username.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter the Database Password.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (!int.TryParse(txtCounterId.Text.Trim(), out int counterId) || counterId <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Counter ID.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCounterId.Focus();
                return;
            }

            Cursor = Cursors.WaitCursor;
            bool isConnWorking = false;
            string testError = null;
            try
            {
                isConnWorking = TestConnection(out testError);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (!isConnWorking)
            {
                DialogResult dr = MessageBox.Show(
                    $"Warning: Connection test failed with the following error:\n\n{testError}\n\nDo you still want to save these settings?",
                    "Connection Test Failed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dr != DialogResult.Yes)
                    return;
            }

            // Save to file
            try
            {
                string configDir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                string content = $"Data Source={txtServer.Text.Trim()};Initial Catalog={txtDatabase.Text.Trim()};User ID={txtUsername.Text.Trim()};Password={txtPassword.Text.Trim()};CounterId={counterId};";
                File.WriteAllText(ConfigPath, content);

                // Update the static Database status
                ModelClass.DataBase.Status = "Local";

                MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration:\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
