using Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public partial class FrmLanguageSettings : Form
    {
        public FrmLanguageSettings()
        {
            InitializeComponent();

            this.Load += FrmLanguageSettings_Load;
            this.btnApply.Click += BtnApply_Click;
            this.btnResetDefault.Click += BtnResetDefault_Click;
            this.btnImport.Click += BtnImport_Click;
            this.btnExport.Click += BtnExport_Click;
            this.btnClose.Click += (s, e) => this.Close();

            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
        }

        private void FrmLanguageSettings_Load(object sender, EventArgs e)
        {
            PopulateLanguageList();
            UpdateStatusLabel();
            LanguageManager.ApplyLanguageToForm(this);
        }

        private void LanguageManager_LanguageChanged(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            this.BeginInvoke(new Action(() =>
            {
                PopulateLanguageList();
                UpdateStatusLabel();
                LanguageManager.ApplyLanguageToForm(this);
            }));
        }

        private void PopulateLanguageList()
        {
            lstLanguages.Items.Clear();
            var languages = LanguageManager.GetAvailableLanguages();

            int selectedIndex = 0;
            for (int i = 0; i < languages.Count; i++)
            {
                var lang = languages[i];
                string display = $"{lang.FlagSymbol}  {lang.Name} [Code: {lang.Code.ToUpper()}]" + (lang.IsCustom ? " (Custom)" : "");
                lstLanguages.Items.Add(display);

                if (string.Equals(lang.Code, LanguageManager.CurrentLanguageCode, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndex = i;
                }
            }

            if (lstLanguages.Items.Count > 0)
            {
                lstLanguages.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateStatusLabel()
        {
            lblStatus.Text = $"{LanguageManager.GetString("Ready")}: {LanguageManager.CurrentLanguageName} ({LanguageManager.CurrentLanguageCode.ToUpper()})";
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                int index = lstLanguages.SelectedIndex;
                var languages = LanguageManager.GetAvailableLanguages();
                if (index >= 0 && index < languages.Count)
                {
                    var selectedLang = languages[index];
                    bool success = LanguageManager.SetLanguage(selectedLang.Code);
                    if (success)
                    {
                        MessageBox.Show(
                            $"{LanguageManager.GetString("Language Changed") ?? "Language changed successfully to"}: {selectedLang.Name}",
                            LanguageManager.GetString("App Language"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Please select a language from the list.",
                        LanguageManager.GetString("Warning"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying language: {ex.Message}", LanguageManager.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResetDefault_Click(object sender, EventArgs e)
        {
            try
            {
                bool success = LanguageManager.SetLanguage("en");
                if (success)
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Rolled back to Default Language (English)"),
                        LanguageManager.GetString("App Language"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting language: {ex.Message}", LanguageManager.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Import Custom Language File";
                dialog.Filter = "Language Files (*.json;*.csv)|*.json;*.csv|JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                dialog.Multiselect = false;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        bool success = LanguageManager.ImportLanguageFile(dialog.FileName);
                        if (success)
                        {
                            PopulateLanguageList();
                            MessageBox.Show(
                                "Language file imported successfully!",
                                LanguageManager.GetString("Success"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Could not import language file. Please check file format.",
                                LanguageManager.GetString("Warning"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to import language file: {ex.Message}", LanguageManager.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Language Dictionary Template";
                dialog.Filter = "JSON Files (*.json)|*.json";
                dialog.FileName = "custom_language_template.json";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        LanguageManager.ExportLanguageTemplate(dialog.FileName);
                        MessageBox.Show(
                            $"Language dictionary template exported to:\n{dialog.FileName}",
                            LanguageManager.GetString("Success"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export template: {ex.Message}", LanguageManager.GetString("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
            base.OnFormClosed(e);
        }
    }
}
