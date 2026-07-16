using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmSuccesMsg : Form
    {
        public frmSuccesMsg()
        {
            InitializeComponent();
        }

        public frmSuccesMsg(string purchaseNo)
        {
            InitializeComponent();

            // Adjust the form size to accommodate new labels
            this.Size = new Size(330, 340);

            // Modify existing label
            ultraLabel1.Text = "save this purchase";
            ultraLabel1.Location = new Point(0, 185);
            ultraLabel1.Size = new Size(330, 40);
            ultraLabel1.Appearance.TextHAlign = Infragistics.Win.HAlign.Center;

            // Add new label for GRN
            Label lblGrn = new Label();
            lblGrn.Text = "GRN-" + purchaseNo;
            lblGrn.ForeColor = Color.Black; // Black color font requested by user
            lblGrn.Font = new Font("OCR-A-Seagull", 16F, FontStyle.Bold);
            lblGrn.AutoSize = false;
            lblGrn.Size = new Size(330, 40);
            lblGrn.TextAlign = ContentAlignment.MiddleCenter;
            lblGrn.Location = new Point(0, 225); // Below ultraLabel1
            lblGrn.BackColor = Color.Transparent;

            // Hide the default OK button
            ultraButton1.Visible = false;

            // Create Yes Button
            Infragistics.Win.Misc.UltraButton btnYes = new Infragistics.Win.Misc.UltraButton();
            btnYes.Text = "YES";
            btnYes.Size = new Size(80, 35);
            btnYes.Location = new Point(75, 280);
            btnYes.Appearance.BackColor = Color.Teal;
            btnYes.Appearance.ForeColor = Color.Black;
            btnYes.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnYes.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            };

            // Create No Button
            Infragistics.Win.Misc.UltraButton btnNo = new Infragistics.Win.Misc.UltraButton();
            btnNo.Text = "NO";
            btnNo.Size = new Size(80, 35);
            btnNo.Location = new Point(175, 280);
            btnNo.Appearance.BackColor = Color.IndianRed;
            btnNo.Appearance.ForeColor = Color.Black;
            btnNo.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold);
            btnNo.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.No;
                this.Close();
            };

            ultraPanel1.ClientArea.Controls.Add(lblGrn);
            ultraPanel1.ClientArea.Controls.Add(btnYes);
            ultraPanel1.ClientArea.Controls.Add(btnNo);
        }

        private void ultraButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
