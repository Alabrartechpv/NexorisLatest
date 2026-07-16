using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class FrmNexorisAISettings : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public FrmNexorisAISettings()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            cmbLanguage.SelectedItem = AIChatbotSettings.Language;
            cmbTheme.SelectedItem = AIChatbotSettings.Theme;
            cmbHumor.SelectedItem = AIChatbotSettings.HumourLevel;
            chkTypewriter.Checked = AIChatbotSettings.EnableTypewriterEffect;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            AIChatbotSettings.Language = cmbLanguage.SelectedItem?.ToString() ?? "English";
            AIChatbotSettings.Theme = cmbTheme.SelectedItem?.ToString() ?? "Dark";
            AIChatbotSettings.HumourLevel = cmbHumor.SelectedItem?.ToString() ?? "Professional";
            AIChatbotSettings.EnableTypewriterEffect = chkTypewriter.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void lblTitle_MouseDown(object sender, MouseEventArgs e)
        {
            pnlHeader_MouseDown(sender, e);
        }

        private void FrmNexorisAISettings_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(0, 255, 200), 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, this.Width - 2, this.Height - 2);
            }
        }
    }
}
