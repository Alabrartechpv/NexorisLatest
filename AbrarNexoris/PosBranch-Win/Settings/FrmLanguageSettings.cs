using PosBranch_Win;
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
        private readonly Color pageBack = Color.FromArgb(232, 246, 255);
        private readonly Color cardBack = Color.FromArgb(250, 253, 255);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color muted = Color.FromArgb(72, 98, 138);
        private readonly Color accent = Color.FromArgb(42, 121, 232);
        private readonly Color skyBlueOutline = Color.FromArgb(102, 190, 255);

        private bool suppressSelectionChange = false;

        public FrmLanguageSettings()
        {
            InitializeComponent();

            this.Load += FrmLanguageSettings_Load;
            this.btnApply.Click += BtnApply_Click;
            this.btnResetDefault.Click += BtnResetDefault_Click;
            this.btnImport.Click += BtnImport_Click;
            this.btnExport.Click += BtnExport_Click;
            this.btnClose.Click += (s, e) => this.Close();
            this.lstLanguages.SelectedIndexChanged += LstLanguages_SelectedIndexChanged;
            this.lstLanguages.DoubleClick += LstLanguages_DoubleClick;

            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            ApplyRuntimeStyles();
        }

        private void ApplyRuntimeStyles()
        {
            this.Text = "App Language Settings";
            this.BackColor = pageBack;
            this.Font = new Font("Segoe UI", 9F);

            AttachCardPaint(panelHeaderCard);
            AttachCardPaint(panelCard);

            StyleButton(btnApply, true);
            StyleButton(btnResetDefault, false);
            StyleButton(btnImport, false);
            StyleButton(btnExport, false);
            StyleButton(btnClose, false);
        }

        private void AttachCardPaint(Panel panel)
        {
            if (panel != null)
                panel.Paint += Card_Paint;
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Panel panel)
            {
                using (Pen pen = new Pen(border, 1))
                {
                    Rectangle rect = panel.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private void StyleButton(Button button, bool primary)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.ForeColor = primary ? Color.White : navy;
            button.BackColor = primary ? accent : Color.FromArgb(236, 246, 255);
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = primary ? accent : skyBlueOutline;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.MouseOverBackColor = primary ? accent : Color.FromArgb(225, 244, 255);
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(31, 96, 205) : Color.FromArgb(210, 235, 252);

            if (primary)
            {
                button.Paint -= ApplyButton_Paint;
                button.Paint += ApplyButton_Paint;
            }
        }

        private void ApplyButton_Paint(object sender, PaintEventArgs e)
        {
            if (sender is Button btn)
            {
                using (SolidBrush brush = new SolidBrush(accent))
                    e.Graphics.FillRectangle(brush, btn.ClientRectangle);

                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle,
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
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
            suppressSelectionChange = true;
            try
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
            finally
            {
                suppressSelectionChange = false;
            }
        }

        private void UpdateStatusLabel()
        {
            lblStatus.Text = $"{LanguageManager.GetString("Ready")}: {LanguageManager.CurrentLanguageName} ({LanguageManager.CurrentLanguageCode.ToUpper()})";
        }

        private void LstLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressSelectionChange) return;
            ApplySelectedLanguage();
        }

        private void LstLanguages_DoubleClick(object sender, EventArgs e)
        {
            ApplySelectedLanguage();
        }

        private void ApplySelectedLanguage()
        {
            try
            {
                int index = lstLanguages.SelectedIndex;
                var languages = LanguageManager.GetAvailableLanguages();
                if (index >= 0 && index < languages.Count)
                {
                    var selectedLang = languages[index];
                    if (!string.Equals(selectedLang.Code, LanguageManager.CurrentLanguageCode, StringComparison.OrdinalIgnoreCase))
                    {
                        bool success = LanguageManager.SetLanguage(selectedLang.Code);
                        if (success)
                        {
                            LanguageManager.ApplyLanguageToApplication();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying selected language: {ex.Message}");
            }
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
                        LanguageManager.ApplyLanguageToApplication();
                        MessageBox.Show(
                            $"{LanguageManager.GetString("Language Changed", "Language changed successfully to")}: {selectedLang.Name}",
                            LanguageManager.GetString("App Language", "App Language"),
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
                    LanguageManager.ApplyLanguageToApplication();
                    MessageBox.Show(
                        LanguageManager.GetString("Rolled back to Default Language (English)", "Rolled back to Default Language (English)"),
                        LanguageManager.GetString("App Language", "App Language"),
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
                            LanguageManager.ApplyLanguageToApplication();
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
