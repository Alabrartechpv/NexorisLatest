using Infragistics.Controls.Barcodes;
using Infragistics.Win.DataVisualization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Utilities
{
    public partial class frmBarcode : Form
    {
        public frmBarcode()
        {
            InitializeComponent();
        }

        private void frmBarcode_Load(object sender, EventArgs e)
        {
            var Barcode = new UltraCode128Barcode();
            Barcode.Data = "8906016300344";
            this.Controls.Add(Barcode);
            pictureBox1.Controls.Add(Barcode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            PrintDocument pdoc = new PrintDocument();
            pdoc.PrintPage += PrintPicture;
            pd.Document = pdoc;
            if (pd.ShowDialog() == DialogResult.OK)
            {
                pdoc.Print();
            }
        }
        private void PrintPicture(object sender, PrintPageEventArgs e)
        {
            Bitmap bmp = new Bitmap(100, 130);
            pictureBox1.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0,100, 130));
            e.Graphics.DrawImage(bmp, 0, 0);
            bmp.Dispose();
        }
    }
}
