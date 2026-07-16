using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class ActivityLogSelector : Form
    {
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(176, 224, 255);
        private readonly Color selectedBlue = Color.FromArgb(38, 119, 237);

        public ActivityLogSelector()
        {
            InitializeSelectorUi();
        }

        private void InitializeSelectorUi()
        {
            Text = "Activity Log";
            Name = "ActivityLogSelector";
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(247, 252, 255),
                Padding = new Padding(24)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 122F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var titlePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            titlePanel.Controls.Add(new Label
            {
                Text = "Activity Log",
                AutoSize = true,
                Location = new Point(4, 12),
                Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
                ForeColor = navy
            });
            titlePanel.Controls.Add(new Label
            {
                Text = "Select which activity log you want to open.",
                AutoSize = true,
                Location = new Point(6, 48),
                ForeColor = Color.FromArgb(35, 77, 145)
            });

            var buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };
            buttonRow.Controls.Add(CreateLogButton("ItemLog", "Activity Log - Item Master", () => new ActivityLog()));
            buttonRow.Controls.Add(CreateLogButton("itemstockactivity", "Activity Log - Item Stock", () => new ItemStockActivity()));
            buttonRow.Controls.Add(CreateLogButton("PurchaseLog", "Activity Log - Purchase", () => new PurchaseLog()));
            buttonRow.Controls.Add(CreateLogButton("SalesLog", "Activity Log - Sales", () => new SalesLog()));
            buttonRow.Controls.Add(CreateLogButton("UserLog", "Activity Log - User Logging", () => new UserActivityLog()));

            root.Controls.Add(titlePanel, 0, 0);
            root.Controls.Add(buttonRow, 0, 1);
            root.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 0, 2);
            Controls.Add(root);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
        }

        private Button CreateLogButton(string text, string tabTitle, Func<Form> formFactory)
        {
            var button = new Button
            {
                Text = text,
                Width = 150,
                Height = 44,
                Margin = new Padding(0, 0, 12, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = navy,
                UseVisualStyleBackColor = false,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderColor = border;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 243, 255);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(204, 232, 252);
            button.Click += (s, e) =>
            {
                button.BackColor = selectedBlue;
                button.ForeColor = Color.White;
                OpenLogForm(formFactory(), tabTitle);
            };
            return button;
        }

        private void OpenLogForm(Form form, string tabTitle)
        {
            try
            {
                var homeForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.GetType().Name == "Home");
                if (homeForm != null)
                {
                    MethodInfo openMethod = homeForm.GetType().GetMethod(
                        "OpenFormInTab",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (openMethod != null)
                    {
                        openMethod.Invoke(homeForm, new object[] { form, tabTitle });
                        return;
                    }
                }

                form.Show();
            }
            catch (Exception ex)
            {
                form.Dispose();
                MessageBox.Show("Unable to open activity log: " + ex.Message, "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
