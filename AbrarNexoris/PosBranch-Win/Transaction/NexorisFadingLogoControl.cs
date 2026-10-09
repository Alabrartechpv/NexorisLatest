using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    /// <summary>
    /// Intro branding logo card shown when opening the Sales Invoice form newly (IRS POS Malaysia style).
    /// Features an attractive rounded rectangle card, elevation shadow, smooth fade-in,
    /// an elegant sheen sweep, prominent hold, and graceful fade-out into the SkyBlue header.
    /// </summary>
    public class NexorisFadingLogoControl : Control
    {
        private enum AnimationPhase
        {
            FadeIn,
            HoldWithShine,
            FadingOut,
            Completed
        }

        private Timer _animationTimer;
        private Bitmap _cleanedLogo;
        private Bitmap _darkThemeLogo;
        private float _currentOpacity = 0.0f;
        private AnimationPhase _currentPhase = AnimationPhase.FadeIn;
        private int _phaseTicks = 0;
        private float _shineProgress = -0.5f;

        // Exact header background color (SkyBlue - RGB: 135, 206, 235)
        public static readonly Color HeaderSkyBlue = Color.FromArgb(135, 206, 235);

        // Timing parameters (in milliseconds)
        private const int TIMER_INTERVAL_MS = 30;   // ~33 FPS, silky smooth without CPU overhead
        private const int FADE_IN_TICKS = 14;        // ~420ms smooth fade in
        private const int HOLD_TICKS = 140;          // ~4.2 seconds hold
        private const int FADE_OUT_TICKS = 25;       // ~750ms graceful fade out

        public NexorisFadingLogoControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            this.BackColor = HeaderSkyBlue;
            this.Size = new Size(190, 71);

            LoadAndCleanLogo();

            _animationTimer = new Timer();
            _animationTimer.Interval = TIMER_INTERVAL_MS;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        /// <summary>
        /// Loads splash_logo.png, removes any near-white/light-blue background remnants,
        /// and crops to the content bounding box so it scales crisply onto the card.
        /// </summary>
        private void LoadAndCleanLogo()
        {
            Bitmap rawBitmap = null;
            try
            {
                rawBitmap = Properties.Resources.splash_logo_png;
            }
            catch { }

            if (rawBitmap == null)
            {
                string[] searchPaths = new string[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "splash_logo.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", "splash_logo.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "splash_logo.png")
                };

                foreach (string p in searchPaths)
                {
                    if (File.Exists(p))
                    {
                        try
                        {
                            rawBitmap = new Bitmap(p);
                            break;
                        }
                        catch { }
                    }
                }
            }

            if (rawBitmap != null)
            {
                try
                {
                    _cleanedLogo = CleanAndCropLogo(rawBitmap, false);
                    _darkThemeLogo = CleanAndCropLogo(rawBitmap, true);
                }
                catch
                {
                    _cleanedLogo = (Bitmap)rawBitmap.Clone();
                    _darkThemeLogo = (Bitmap)rawBitmap.Clone();
                }
            }
        }

        /// <summary>
        /// Filters out light-colored background artifacts and crops tightly to the logo boundaries.
        /// When adaptForDarkBackground is true, converts dark navy lettering to crisp white for contrast against dark headers.
        /// </summary>
        private static Bitmap CleanAndCropLogo(Bitmap src, bool adaptForDarkBackground = false)
        {
            int minX = src.Width;
            int maxX = 0;
            int minY = src.Height;
            int maxY = 0;

            for (int y = 0; y < src.Height; y += 2)
            {
                for (int x = 0; x < src.Width; x += 2)
                {
                    Color p = src.GetPixel(x, y);
                    bool isLightArtifact = (p.R > 180 && p.G > 200 && p.B > 215) || (p.R > 200 && p.G > 200 && p.B > 200);
                    if (p.A > 20 && !isLightArtifact)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX > minX && maxY > minY)
            {
                int cropW = Math.Min(src.Width - minX, maxX - minX + 1);
                int cropH = Math.Min(src.Height - minY, maxY - minY + 1);

                Bitmap cleaned = new Bitmap(cropW, cropH, PixelFormat.Format32bppArgb);
                for (int y = 0; y < cropH; y++)
                {
                    for (int x = 0; x < cropW; x++)
                    {
                        Color p = src.GetPixel(minX + x, minY + y);
                        bool isLightArtifact = (p.R > 180 && p.G > 200 && p.B > 215) || (p.R > 200 && p.G > 200 && p.B > 200);
                        if (p.A > 20 && !isLightArtifact)
                        {
                            if (adaptForDarkBackground && p.R < 60 && p.G < 90 && p.B < 140)
                            {
                                // Dark text converts to crisp white for dark headers
                                cleaned.SetPixel(x, y, Color.FromArgb(p.A, 255, 255, 255));
                            }
                            else
                            {
                                cleaned.SetPixel(x, y, p);
                            }
                        }
                        else
                        {
                            cleaned.SetPixel(x, y, Color.Transparent);
                        }
                    }
                }
                return cleaned;
            }

            return (Bitmap)src.Clone();
        }

        private Bitmap GetLogoForBackground(Color bg)
        {
            bool isDark = (bg.R * 0.299 + bg.G * 0.587 + bg.B * 0.114) < 128;
            if (isDark && _darkThemeLogo != null)
            {
                return _darkThemeLogo;
            }
            return _cleanedLogo;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!this.Visible || this.IsDisposed)
            {
                return;
            }

            _phaseTicks++;

            switch (_currentPhase)
            {
                case AnimationPhase.FadeIn:
                    _currentOpacity = Math.Min(1.0f, (float)_phaseTicks / FADE_IN_TICKS);
                    if (_phaseTicks >= FADE_IN_TICKS)
                    {
                        _currentOpacity = 1.0f;
                        _currentPhase = AnimationPhase.HoldWithShine;
                        _phaseTicks = 0;
                        _shineProgress = -0.3f;
                    }
                    this.Invalidate();
                    break;

                case AnimationPhase.HoldWithShine:
                    // After ~1s of hold, animate an elegant light sheen sweep across the card
                    if (_phaseTicks > 30 && _shineProgress < 1.3f)
                    {
                        _shineProgress += 0.045f;
                    }

                    if (_phaseTicks >= HOLD_TICKS)
                    {
                        _currentPhase = AnimationPhase.FadingOut;
                        _phaseTicks = 0;
                    }
                    this.Invalidate();
                    break;

                case AnimationPhase.FadingOut:
                    _currentOpacity = Math.Max(0.0f, 1.0f - ((float)_phaseTicks / FADE_OUT_TICKS));
                    if (_phaseTicks >= FADE_OUT_TICKS || _currentOpacity <= 0.0f)
                    {
                        _currentOpacity = 0.0f;
                        _currentPhase = AnimationPhase.Completed;
                        if (_animationTimer != null)
                        {
                            _animationTimer.Stop();
                        }
                        this.Visible = false; // Becomes completely invisible after intro
                    }
                    this.Invalidate();
                    break;

                case AnimationPhase.Completed:
                    if (_animationTimer != null)
                    {
                        _animationTimer.Stop();
                    }
                    this.Visible = false;
                    break;
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (rect.Width <= d || rect.Height <= d)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Color bg = this.Parent != null ? this.Parent.BackColor : HeaderSkyBlue;
            g.Clear(bg);

            if (_currentOpacity <= 0.005f)
            {
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Bitmap logoToDraw = GetLogoForBackground(bg);
            if (logoToDraw == null) return;

            // Direct rendering without white card background
            int padX = 6;
            int padY = 4;
            Rectangle innerRect = new Rectangle(
                padX,
                padY,
                Math.Max(1, this.ClientSize.Width - (padX * 2)),
                Math.Max(1, this.ClientSize.Height - (padY * 2)));

            float ratioX = (float)innerRect.Width / logoToDraw.Width;
            float ratioY = (float)innerRect.Height / logoToDraw.Height;
            float ratio = Math.Min(ratioX, ratioY);

            int drawW = (int)(logoToDraw.Width * ratio);
            int drawH = (int)(logoToDraw.Height * ratio);
            int drawX = innerRect.X + (innerRect.Width - drawW) / 2;
            int drawY = innerRect.Y + (innerRect.Height - drawH) / 2;

            Rectangle destRect = new Rectangle(drawX, drawY, drawW, drawH);

            float clampedOpacity = Math.Max(0.0f, Math.Min(1.0f, _currentOpacity));
            ColorMatrix matrix = new ColorMatrix(new float[][]
            {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, clampedOpacity, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });

            using (ImageAttributes attr = new ImageAttributes())
            {
                attr.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(logoToDraw, destRect, 0, 0, logoToDraw.Width, logoToDraw.Height, GraphicsUnit.Pixel, attr);
            }

            // Elegant light sheen sweep directly across the logo during hold
            if (_currentPhase == AnimationPhase.HoldWithShine && _shineProgress >= -0.1f && _shineProgress <= 1.2f)
            {
                int shineW = 32;
                int shineX = destRect.X + (int)(_shineProgress * destRect.Width);
                Rectangle shineRect = new Rectangle(shineX, destRect.Y - 4, shineW, destRect.Height + 8);

                using (LinearGradientBrush shineBrush = new LinearGradientBrush(
                    shineRect,
                    Color.FromArgb(0, 255, 255, 255),
                    Color.FromArgb((int)(110 * _currentOpacity), 255, 255, 255),
                    LinearGradientMode.Horizontal))
                {
                    shineBrush.SetBlendTriangularShape(0.5f);
                    g.FillRectangle(shineBrush, shineRect);
                }
            }
        }

        /// <summary>
        /// Plays intro branding animation when opening the form newly:
        /// Smoothly fades in, holds prominently with an attractive shine sweep,
        /// then gracefully fades out into invisible and stays invisible.
        /// </summary>
        public void StartIntroAnimation()
        {
            if (this.IsDisposed) return;

            _currentOpacity = 0.0f;
            _currentPhase = AnimationPhase.FadeIn;
            _phaseTicks = 0;
            _shineProgress = -0.5f;
            this.Visible = true;

            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Start();
            }
            this.Invalidate();
        }

        public void PauseAndHide()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
            }
            _currentOpacity = 0.0f;
            _currentPhase = AnimationPhase.Completed;
            this.Visible = false;
        }

        public void StopAnimation()
        {
            if (_animationTimer != null && _animationTimer.Enabled)
            {
                _animationTimer.Stop();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_animationTimer != null)
                {
                    _animationTimer.Stop();
                    _animationTimer.Tick -= AnimationTimer_Tick;
                    _animationTimer.Dispose();
                    _animationTimer = null;
                }
                if (_cleanedLogo != null)
                {
                    _cleanedLogo.Dispose();
                    _cleanedLogo = null;
                }
                if (_darkThemeLogo != null)
                {
                    _darkThemeLogo.Dispose();
                    _darkThemeLogo = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
