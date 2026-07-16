using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;

namespace PosBranch_Win.Master
{
    public partial class frmTaxManagement : Form
    {
        public frmTaxManagement()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            // Load current tax setting
            chkTaxEnabled.Checked = DataBase.IsTaxEnabled;
            UpdateTaxStatusMessage();
        }

        private void UpdateTaxStatusMessage()
        {
            if (DataBase.IsTaxEnabled)
            {
                lblTaxStatus.Text = "Tax calculations are ENABLED";
                lblTaxStatus.ForeColor = Color.Green;
                lblTaxStatus.Font = new Font(lblTaxStatus.Font, FontStyle.Bold);
            }
            else
            {
                lblTaxStatus.Text = "Tax calculations are DISABLED (Malaysia Mode)";
                lblTaxStatus.ForeColor = Color.Red;
                lblTaxStatus.Font = new Font(lblTaxStatus.Font, FontStyle.Bold);
            }
        }

        private void chkTaxEnabled_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTaxStatusMessage();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Update global tax setting
                DataBase.IsTaxEnabled = chkTaxEnabled.Checked;

                if (DataBase.IsTaxEnabled)
                {
                    DataBase.TaxToggleMessage = "Tax calculations are enabled";
                }
                else
                {
                    DataBase.TaxToggleMessage = "Tax calculations are disabled (Malaysia Mode)";
                }

                MessageBox.Show($"Tax settings saved successfully!\n\n{DataBase.TaxToggleMessage}",
                    "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tax settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            // Reset to default (enabled)
            chkTaxEnabled.Checked = true;
            UpdateTaxStatusMessage();
        }
    }
}
