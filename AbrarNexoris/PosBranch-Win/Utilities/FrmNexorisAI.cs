using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class FrmNexorisAI : Form
    {
        // Colors for Dynamic Theming
        private Color ThemeBg;
        private Color ThemeAccent;
        private Color ThemePanelBg;
        private Color ThemeTextUser;
        private Color ThemeTextAI;

        private bool _isTyping = false;

        // Required for dragging the borderless form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public FrmNexorisAI()
        {
            InitializeComponent();
            ApplyTheme();
            LoadLogo();

            // Initial greeting
            _ = AppendMessageAsync("Nexoris AI", "Initializing neural interface...\nHello! I am Nexoris AI. How can I assist you?", ThemeTextAI);
        }

        private void LoadLogo()
        {
            try
            {
                string logoPath = @"e:\antgravity\PosBranch-Win\PosBranch-Win\Resources\splash_logo.png";
                if (System.IO.File.Exists(logoPath))
                {
                    pbLogo.Image = Image.FromFile(logoPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Nexoris AI Logo load failed: " + ex.Message);
            }
        }

        private void ApplyTheme()
        {
            if (AIChatbotSettings.Theme == "Light")
            {
                ThemeBg = Color.FromArgb(245, 245, 245);
                ThemeAccent = Color.FromArgb(0, 120, 255);
                ThemePanelBg = Color.FromArgb(230, 230, 230);
                ThemeTextUser = Color.FromArgb(0, 120, 255);
                ThemeTextAI = Color.FromArgb(30, 30, 30);
            }
            else if (AIChatbotSettings.Theme == "Cyberpunk")
            {
                ThemeBg = Color.FromArgb(10, 10, 15);
                ThemeAccent = Color.FromArgb(255, 0, 150);
                ThemePanelBg = Color.FromArgb(20, 20, 30);
                ThemeTextUser = Color.FromArgb(0, 255, 200);
                ThemeTextAI = Color.FromArgb(255, 0, 150);
            }
            else // Dark
            {
                ThemeBg = Color.FromArgb(18, 18, 18);
                ThemeAccent = Color.FromArgb(0, 255, 200);
                ThemePanelBg = Color.FromArgb(30, 30, 30);
                ThemeTextUser = Color.FromArgb(0, 255, 200);
                ThemeTextAI = Color.FromArgb(220, 220, 220);
            }

            this.BackColor = ThemeBg;
            pnlHeader.BackColor = ThemePanelBg;
            pnlHistoryContainer.BackColor = ThemeBg;
            rtbChatHistory.BackColor = ThemeBg;
            pnlInput.BackColor = ThemePanelBg;
            txtInput.BackColor = ThemePanelBg;

            lblTitle.ForeColor = ThemeAccent;
            txtInput.ForeColor = ThemeTextAI; // use AI text color for general input
            rtbChatHistory.ForeColor = ThemeTextAI;

            btnSend.BackColor = ThemeAccent;
            btnSend.ForeColor = ThemeBg;

            this.Invalidate();
            pnlInput.Invalidate();
        }

        private void FrmNexorisAI_Paint(object sender, PaintEventArgs e)
        {
            // Draw a subtle cyan border around the form
            using (Pen pen = new Pen(ThemeAccent, 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, this.Width - 2, this.Height - 2);
            }
        }

        private void PnlInput_Paint(object sender, PaintEventArgs e)
        {
            // Draw border line above input
            using (Pen pen = new Pen(Color.FromArgb(50, 50, 50), 1))
            {
                e.Graphics.DrawLine(pen, 0, 0, pnlInput.Width, 0);
            }
            // Draw underline for textbox
            using (Pen pen = new Pen(ThemeAccent, 1))
            {
                e.Graphics.DrawLine(pen, txtInput.Left, txtInput.Bottom + 2, txtInput.Right, txtInput.Bottom + 2);
            }
        }

        private void PnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (var frmSettings = new FrmNexorisAISettings())
            {
                if (frmSettings.ShowDialog(this) == DialogResult.OK)
                {
                    ApplyTheme();
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide(); // Hide instead of close to keep state
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnClearChat_Click(object sender, EventArgs e)
        {
            if (_isTyping) return;
            rtbChatHistory.Clear();
            _ = AppendMessageAsync("Nexoris AI", "Chat history cleared. How can I assist you?", ThemeTextAI, false);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.I) || keyData == Keys.Escape)
            {
                this.Hide();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessUserInput();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            ProcessUserInput();
        }

        private void ProcessUserInput()
        {
            if (_isTyping) return; // Prevent input while typing

            string query = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            txtInput.Text = "";
            txtInput.Focus();

            _ = AppendMessageAsync("You", query, ThemeTextUser, false);

            string response = AIChatbotService.GetResponse(query);

            _ = AppendMessageAsync("Nexoris AI", response, ThemeTextAI);
        }

        private async Task AppendMessageAsync(string sender, string message, Color color, bool useTypingEffect = true)
        {
            _isTyping = true;
            btnSend.Enabled = false;

            int start = rtbChatHistory.TextLength;
            rtbChatHistory.AppendText($"[{DateTime.Now:HH:mm}] {sender}:\n");

            rtbChatHistory.SelectionStart = start;
            rtbChatHistory.SelectionLength = rtbChatHistory.TextLength - start;
            rtbChatHistory.SelectionColor = Color.Gray;
            rtbChatHistory.SelectionFont = new Font("Segoe UI", 8F, FontStyle.Italic);

            if (AIChatbotSettings.EnableTypewriterEffect && useTypingEffect)
            {
                rtbChatHistory.SelectionStart = rtbChatHistory.TextLength;
                rtbChatHistory.SelectionColor = color;
                rtbChatHistory.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Regular);

                string[] words = message.Split(' ');
                foreach (string word in words)
                {
                    rtbChatHistory.AppendText(word + " ");
                    rtbChatHistory.ScrollToCaret();
                    await Task.Delay(20); // By appending words instead of characters, UI redrawing flicker is completely eliminated.
                }
                rtbChatHistory.AppendText("\n\n");
            }
            else
            {
                start = rtbChatHistory.TextLength;
                rtbChatHistory.AppendText(message + "\n\n");

                rtbChatHistory.SelectionStart = start;
                rtbChatHistory.SelectionLength = rtbChatHistory.TextLength - start;
                rtbChatHistory.SelectionColor = color;
                rtbChatHistory.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Regular);
            }

            rtbChatHistory.ScrollToCaret();

            _isTyping = false;
            btnSend.Enabled = true;
            txtInput.Focus();
        }
    }
}
