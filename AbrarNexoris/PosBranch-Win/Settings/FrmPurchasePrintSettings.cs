using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ModelClass;
using Repository.SettingsRepo;

namespace PosBranch_Win.Settings
{
    public sealed class FrmPurchasePrintSettings : Form
    {
        private readonly int companyId = SessionContext.CompanyId;
        private readonly int branchId = SessionContext.BranchId;
        private readonly PurchasePrintBranding settings;
        private readonly TextBox shopName = new TextBox { MaxLength = 150 };
        private readonly TextBox address1 = new TextBox { MaxLength = 200 };
        private readonly TextBox address2 = new TextBox { MaxLength = 200 };
        private readonly TextBox phone = new TextBox { MaxLength = 60 };
        private readonly TextBox email = new TextBox { MaxLength = 160 };
        private readonly ComboBox logoMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly PictureBox previewLogo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom };
        private readonly Label previewText = new Label { TextAlign = ContentAlignment.TopCenter, Dock = DockStyle.Fill };
        private byte[] branchLogo;

        public FrmPurchasePrintSettings()
        {
            using (var repository = new PurchasePrintBrandingRepository())
                settings = repository.Get(companyId, branchId)
                    ?? throw new InvalidOperationException("Select a company and branch before opening Purchase Print.");

            branchLogo = settings.Logo;
            Text = "Purchase Print";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(840, 480);
            MinimumSize = new Size(740, 490);
            Font = new Font("Segoe UI", 10);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(layout);

            var heading = new Label
            {
                AutoSize = true,
                Text = $"Branch  {settings.BranchName}\nBlank fields use the company or branch profile.",
                Margin = new Padding(3, 0, 3, 16)
            };
            layout.Controls.Add(heading, 0, 0);
            layout.SetColumnSpan(heading, 2);

            var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.Controls.Add(fields, 0, 1);
            AddField(fields, "Shop name", shopName, settings.ShopName);
            AddField(fields, "Address line 1", address1, settings.Address1);
            AddField(fields, "Address line 2", address2, settings.Address2);
            AddField(fields, "Phone", phone, settings.Phone);
            AddField(fields, "Email", email, settings.Email);
            logoMode.Items.AddRange(new object[] { "Use company logo", "Use branch logo", "Hide logo" });
            logoMode.SelectedIndex = settings.HideLogo ? 2 : branchLogo != null ? 1 : 0;
            AddField(fields, "Logo", logoMode);
            var upload = new Button { Text = "Choose logo…", AutoSize = true };
            upload.Click += (sender, args) => ChooseLogo();
            AddField(fields, "", upload);
            logoMode.SelectedIndexChanged += (sender, args) => UpdatePreview();

            var preview = new GroupBox { Text = "Header preview", Dock = DockStyle.Fill, Padding = new Padding(14) };
            previewLogo.Dock = DockStyle.Top;
            previewLogo.Height = 100;
            preview.Controls.Add(previewText);
            preview.Controls.Add(previewLogo);
            layout.Controls.Add(preview, 1, 1);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            var cancel = new Button { Text = "Close", AutoSize = true };
            cancel.Click += (sender, args) => Close();
            var save = new Button { Text = "Save", AutoSize = true };
            save.Enabled = SessionContext.IsAdmin || SessionContext.CanEdit("company");
            save.Click += (sender, args) => Save();
            actions.Controls.Add(save);
            actions.Controls.Add(cancel);
            layout.Controls.Add(actions, 0, 2);
            layout.SetColumnSpan(actions, 2);
            AcceptButton = save;
            UpdatePreview();
        }

        private void AddField(TableLayoutPanel layout, string caption, Control field, string value = null)
        {
            int row = layout.RowCount++;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            field.Dock = DockStyle.Fill;
            field.Margin = new Padding(3, 3, 12, 8);
            if (field is TextBox text)
            {
                text.Text = value ?? "";
                text.TextChanged += (sender, args) => UpdatePreview();
            }
            layout.Controls.Add(field, 1, row);
        }

        private void ChooseLogo()
        {
            using (var dialog = new OpenFileDialog { Filter = "Logo images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    using (var stream = File.OpenRead(dialog.FileName))
                    {
                        if (stream.Length > 10 * 1024 * 1024)
                            throw new InvalidDataException("Choose a logo under 10 MB.");
                        using (var image = Image.FromStream(stream, true, true))
                        {
                            if (image.Width > 4096 || image.Height > 4096)
                                throw new InvalidDataException("Choose a logo no larger than 4096 × 4096 pixels.");
                            double scale = Math.Min(1, Math.Min(600d / image.Width, 260d / image.Height));
                            using (var bitmap = new Bitmap(Math.Max(1, (int)Math.Round(image.Width * scale)),
                                Math.Max(1, (int)Math.Round(image.Height * scale)), PixelFormat.Format32bppArgb))
                            using (var graphics = Graphics.FromImage(bitmap))
                            using (var output = new MemoryStream())
                            {
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.DrawImage(image, 0, 0, bitmap.Width, bitmap.Height);
                                bitmap.Save(output, ImageFormat.Png);
                                branchLogo = output.ToArray();
                            }
                        }
                    }
                    logoMode.SelectedIndex = 1;
                    UpdatePreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Could not open logo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void UpdatePreview()
        {
            if (Disposing || IsDisposed || previewLogo.IsDisposed)
                return;

            previewText.Text = string.Join(Environment.NewLine, new[]
            {
                Resolve(shopName.Text, settings.DefaultShopName),
                Resolve(address1.Text, settings.DefaultAddress1),
                Resolve(address2.Text, settings.DefaultAddress2),
                Resolve(phone.Text, settings.DefaultPhone),
                Resolve(email.Text, settings.DefaultEmail)
            });
            ClearPreviewLogo();
            byte[] logo = logoMode.SelectedIndex == 1 ? branchLogo : logoMode.SelectedIndex == 0 ? settings.CompanyLogo : null;
            if (logo == null || logo.Length == 0)
                return;
            try
            {
                using (var stream = new MemoryStream(logo))
                using (var image = Image.FromStream(stream))
                    previewLogo.Image = new Bitmap(image);
            }
            catch (ArgumentException)
            {
                // A legacy company logo can be invalid. Text branding remains usable.
            }
        }

        private void ClearPreviewLogo()
        {
            var image = previewLogo.Image;
            previewLogo.Image = null;
            image?.Dispose();
        }

        private static string Resolve(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback ?? "" : value.Trim();

        private void Save()
        {
            if (!SessionContext.IsAdmin && !SessionContext.CanEdit("company"))
                return;
            if (logoMode.SelectedIndex == 1 && branchLogo == null)
            {
                MessageBox.Show(this, "Choose a branch logo before saving.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var repository = new PurchasePrintBrandingRepository())
                    repository.Save(companyId, branchId, new PurchasePrintBranding
                    {
                        ShopName = Resolve(shopName.Text, null),
                        Address1 = Resolve(address1.Text, null),
                        Address2 = Resolve(address2.Text, null),
                        Phone = Resolve(phone.Text, null),
                        Email = Resolve(email.Text, null),
                        Logo = logoMode.SelectedIndex == 1 ? branchLogo : null,
                        HideLogo = logoMode.SelectedIndex == 2
                    });
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Could not save Purchase Print", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !previewLogo.IsDisposed)
                ClearPreviewLogo();
            base.Dispose(disposing);
        }
    }
}
