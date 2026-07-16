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
    public partial class tosters : Form
    {
        public tosters(string message)
        {
            InitializeComponent();
            label1.Text = message;
        }

        private void tosters_Load(object sender, EventArgs e)
        {

        }
        public void ShowToast()
        {
            // Set the location of the toast (for example, bottom right of the screen)
            int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10; // 10 pixels from the right
            int y = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 10; // 10 pixels from the bottom
            this.Location = new Point(x, y);
            this.CenterToParent();
            this.Show();
            // Set a timer to close the toast after a few seconds
            Timer timer = new Timer();
            timer.Interval = 1000; // Display for 3 seconds
            timer.Tick += (s, e) =>
            {
                this.Close();
                timer.Stop();
            };
            timer.Start();
        }
    }
}
