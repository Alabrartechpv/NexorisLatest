using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win
{
    class GradiantColor : Panel
    {
        public Color  colorTop {get;set;}
        public Color colorBottom { get; set; }
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = this.ClientRectangle;
            if (rect.Width > 0 && rect.Height > 0)
            {
                using (LinearGradientBrush lgb = new LinearGradientBrush(rect, this.colorTop, this.colorBottom, 90F))
                {
                    e.Graphics.FillRectangle(lgb, rect);
                }
            }
            base.OnPaint(e);
        }
    }
}
