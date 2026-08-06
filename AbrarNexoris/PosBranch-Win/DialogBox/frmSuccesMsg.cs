using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSuccesMsg : Form
    {
        private string _title = "Saved Successfully.";
        private string _subtitle = "Operation completed successfully.";
        private Dictionary<string, string> _details = new Dictionary<string, string>();
        private bool _isConfirmPrompt = false;
        private string _confirmPurchaseNo = string.Empty;

        public frmSuccesMsg()
        {
            InitializeComponent();
            _title = "Item saved successfully.";
            _subtitle = "The operation completed successfully.";
            BuildModernSuccessUI();
        }

        public frmSuccesMsg(string title, string subtitle, Dictionary<string, string> details)
        {
            InitializeComponent();
            _title = !string.IsNullOrWhiteSpace(title) ? title : "Saved successfully.";
            _subtitle = subtitle;
            _details = details ?? new Dictionary<string, string>();
            BuildModernSuccessUI();
        }

        public frmSuccesMsg(string purchaseNo)
        {
            InitializeComponent();
            _isConfirmPrompt = true;
            _confirmPurchaseNo = purchaseNo;
            _title = "Save this purchase?";
            _subtitle = null;
            _details = new Dictionary<string, string>
            {
                { "GRN No", "GRN-" + purchaseNo }
            };
            BuildModernSuccessUI();
        }

        private void BuildModernSuccessUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            int detailCount = _details != null ? _details.Count : 0;
            int cardHeight = detailCount > 0 ? (detailCount * 26 + 18) : 0;
            int mainContentHeight = 65 + (string.IsNullOrEmpty(_subtitle) ? 0 : 22) + (cardHeight > 0 ? (cardHeight + 15) : 10);
            int totalFormHeight = 45 + mainContentHeight + 60;
            if (totalFormHeight < 210) totalFormHeight = 210;

            this.Size = new Size(500, totalFormHeight);
            this.BackColor = Color.White;

            if (pictureBox1 != null) pictureBox1.Visible = false;
            if (ultraPanel1 != null) ultraPanel1.Visible = false;

            this.Controls.Clear();

            // 1. Header Bar
            Panel pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(500, 42),
                BackColor = Color.White
            };

            // Success Icon + Title
            Label lblHeaderTitle = new Label
            {
                Text = _isConfirmPrompt ? "Confirm Action" : "✓  Success",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 50, 60),
                Location = new Point(15, 10),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHeaderTitle);

            // Close button (X)
            Label btnClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 130, 140),
                Location = new Point(465, 8),
                Size = new Size(25, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnClose.MouseEnter += (s, e) => btnClose.ForeColor = Color.FromArgb(220, 50, 50);
            btnClose.MouseLeave += (s, e) => btnClose.ForeColor = Color.FromArgb(120, 130, 140);
            btnClose.Click += (s, e) =>
            {
                if (_isConfirmPrompt) this.DialogResult = DialogResult.No;
                else this.DialogResult = DialogResult.OK;
                this.Close();
            };
            pnlHeader.Controls.Add(btnClose);

            // Top Sky Blue Border & Accent Lines
            pnlHeader.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen skyBluePen = new Pen(Color.FromArgb(102, 190, 255), 2f))
                {
                    // Top, Left, Right outer border
                    g.DrawLine(skyBluePen, 0, 0, pnlHeader.Width - 1, 0);
                    g.DrawLine(skyBluePen, 0, 0, 0, pnlHeader.Height);
                    g.DrawLine(skyBluePen, pnlHeader.Width - 1, 0, pnlHeader.Width - 1, pnlHeader.Height);
                    // Bottom separator line
                    g.DrawLine(skyBluePen, 0, pnlHeader.Height - 1, pnlHeader.Width - 1, pnlHeader.Height - 1);
                }
            };
            this.Controls.Add(pnlHeader);

            // 2. Left Circular Green Checkmark Badge
            Panel pnlBadge = new Panel
            {
                Location = new Point(25, 60),
                Size = new Size(80, 80),
                BackColor = Color.Transparent
            };
            pnlBadge.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Soft mint green circle
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(232, 246, 237)))
                {
                    g.FillEllipse(bgBrush, 5, 5, 70, 70);
                }

                // Checkmark circle border
                using (Pen circlePen = new Pen(Color.FromArgb(34, 160, 75), 2.5f))
                {
                    g.DrawEllipse(circlePen, 18, 18, 44, 44);
                }

                // Checkmark path
                using (Pen checkPen = new Pen(Color.FromArgb(34, 160, 75), 3.5f))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    Point[] checkPoints = { new Point(31, 40), new Point(38, 47), new Point(50, 33) };
                    g.DrawLines(checkPen, checkPoints);
                }
            };
            this.Controls.Add(pnlBadge);

            // 3. Right Title & Subtitle Labels
            Label lblMainTitle = new Label
            {
                Text = _title,
                Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 150, 70),
                Location = new Point(125, 58),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblMainTitle);

            int currentY = 86;
            if (!string.IsNullOrWhiteSpace(_subtitle))
            {
                Label lblSub = new Label
                {
                    Text = _subtitle,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(100, 110, 120),
                    Location = new Point(125, 86),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                this.Controls.Add(lblSub);
                currentY = 112;
            }
            else
            {
                currentY = 92;
            }

            // 4. Details Container Card
            if (detailCount > 0)
            {
                Panel pnlCard = new Panel
                {
                    Location = new Point(125, currentY),
                    Size = new Size(345, cardHeight),
                    BackColor = Color.White
                };

                pnlCard.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    Rectangle cardRect = new Rectangle(0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
                    using (Pen borderPen = new Pen(Color.FromArgb(180, 220, 250), 1f))
                    {
                        GraphicsPath path = GetRoundedRectPath(cardRect, 6);
                        g.DrawPath(borderPen, path);
                    }
                };

                int rowY = 10;
                int index = 0;
                foreach (var kvp in _details)
                {
                    Label lblKey = new Label
                    {
                        Text = kvp.Key,
                        Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(110, 120, 130),
                        Location = new Point(12, rowY),
                        Size = new Size(110, 22),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent
                    };

                    Label lblColon = new Label
                    {
                        Text = ":",
                        Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(160, 170, 180),
                        Location = new Point(122, rowY),
                        Size = new Size(12, 22),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent
                    };

                    Label lblVal = new Label
                    {
                        Text = kvp.Value,
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = (index == 0) ? Color.FromArgb(30, 160, 75) : Color.FromArgb(30, 40, 50),
                        Location = new Point(136, rowY),
                        Size = new Size(195, 22),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = Color.Transparent
                    };

                    pnlCard.Controls.Add(lblKey);
                    pnlCard.Controls.Add(lblColon);
                    pnlCard.Controls.Add(lblVal);

                    rowY += 25;
                    index++;
                }

                this.Controls.Add(pnlCard);
            }

            // 5. Footer Action Bar
            int footerY = totalFormHeight - 55;
            Panel pnlFooter = new Panel
            {
                Location = new Point(0, footerY),
                Size = new Size(500, 55),
                BackColor = Color.FromArgb(250, 252, 254)
            };

            pnlFooter.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen skyBluePen = new Pen(Color.FromArgb(102, 190, 255), 2f))
                {
                    // Top, Bottom, Left, Right outer & separator border
                    g.DrawLine(skyBluePen, 0, 0, pnlFooter.Width - 1, 0);
                    g.DrawLine(skyBluePen, 0, 0, 0, pnlFooter.Height);
                    g.DrawLine(skyBluePen, pnlFooter.Width - 1, 0, pnlFooter.Width - 1, pnlFooter.Height);
                    g.DrawLine(skyBluePen, 0, pnlFooter.Height - 1, pnlFooter.Width - 1, pnlFooter.Height - 1);
                }
            };

            if (!_isConfirmPrompt)
            {
                // Single OK Button
                Button btnOk = new Button
                {
                    Text = "OK",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(27, 102, 222),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(105, 36),
                    Location = new Point((500 - 105) / 2, 9),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                btnOk.Click += (s, e) =>
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                pnlFooter.Controls.Add(btnOk);
                this.AcceptButton = btnOk;
            }
            else
            {
                // YES and NO Buttons for Purchase confirmation
                Button btnYes = new Button
                {
                    Text = "YES",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(34, 160, 75),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(95, 36),
                    Location = new Point(145, 9),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.Yes
                };
                btnYes.FlatAppearance.BorderSize = 0;
                btnYes.Click += (s, e) =>
                {
                    this.DialogResult = DialogResult.Yes;
                    this.Close();
                };

                Button btnNo = new Button
                {
                    Text = "NO",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(220, 60, 60),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(95, 36),
                    Location = new Point(260, 9),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.No
                };
                btnNo.FlatAppearance.BorderSize = 0;
                btnNo.Click += (s, e) =>
                {
                    this.DialogResult = DialogResult.No;
                    this.Close();
                };

                pnlFooter.Controls.Add(btnYes);
                pnlFooter.Controls.Add(btnNo);
                this.AcceptButton = btnYes;
            }

            this.Controls.Add(pnlFooter);

            // Draw 2px sky blue outer border line around entire form window
            this.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen skyBluePen = new Pen(Color.FromArgb(102, 190, 255), 2f))
                {
                    g.DrawRectangle(skyBluePen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ultraButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
