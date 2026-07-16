using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class FrmSplashScreen : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        private Timer closeTimer;
        private Timer fadeTimer;
        private Timer animationTimer;
        private int displayDurationMs = 3000; // 3 seconds
        private float angle = 0f; // Current angle for loading circle

        public FrmSplashScreen()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            pnlMain.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMain.Width, pnlMain.Height, 14, 14));

            // Initial opacity 0 for fade-in effect
            this.Opacity = 0;

            // Safe image loading
            try
            {
                string[] possiblePaths = new string[]
                {
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "splash_logo.png"), // Bin folder
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", "splash_logo.png"), // Dev folder 2 levels up
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "splash_logo.png") // Parent folder
                };

                foreach (string path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        this.pictureBoxLogo.Image = Image.FromFile(path);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Could not load splash image: " + ex.Message);
            }

            // Setup timer for auto-close
            closeTimer = new Timer();
            closeTimer.Interval = displayDurationMs;
            closeTimer.Tick += CloseTimer_Tick;

            // Setup timer for fade-in
            fadeTimer = new Timer();
            fadeTimer.Interval = 30; // 30ms step
            fadeTimer.Tick += FadeTimer_Tick;

            // Setup timer for loading animation
            animationTimer = new Timer();
            animationTimer.Interval = 50; // 20 FPS
            animationTimer.Tick += AnimationTimer_Tick;

            // Custom paint for loading circle
            this.picLoader.Paint += PicLoader_Paint;

            // Center the loading text and loader
            CenterControls();
        }

        private void CenterControls()
        {
            // Center text horizontally
            lblLoading.Left = (pnlMain.Width - lblLoading.Width) / 2;

            // Position loader below text, centered
            // The Designer put it at hardcoded X=520, let's recenter it below text
            picLoader.Left = (pnlMain.Width - picLoader.Width) / 2;
            picLoader.Top = lblLoading.Bottom + 10;
        }

        private void FrmSplashScreen_Load(object sender, EventArgs e)
        {
            // Start everything
            fadeTimer.Start();
            animationTimer.Start();
            closeTimer.Start();
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            // Fade in logic
            if (this.Opacity < 1)
            {
                this.Opacity += 0.05; // Increase by 5% each tick
            }
            else
            {
                fadeTimer.Stop(); // Full opacity reached
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Rotate the angle
            angle += 15f;
            if (angle >= 360) angle = 0;
            picLoader.Invalidate(); // Trigger repaint
        }

        private void PicLoader_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw smooth loading circle
            int size = Math.Min(picLoader.Width, picLoader.Height) - 4;
            Rectangle rect = new Rectangle(2, 2, size, size);

            using (Pen pen = new Pen(Color.FromArgb(26, 57, 93), 4)) // Brand color dark blue
            {
                // Draw arc starting from current angle, sweeping 300 degrees (open circle)
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                e.Graphics.DrawArc(pen, rect, angle, 300);
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            // Fade out potentially before closing? Or just close.
            // Let's just close as per standard request, unless fade-out requested.
            // User asked for "fade to visible", not fade out.
            closeTimer.Stop();
            animationTimer.Stop();
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            fadeTimer?.Dispose();
            animationTimer?.Dispose();
            closeTimer?.Dispose();
        }
    }
}
