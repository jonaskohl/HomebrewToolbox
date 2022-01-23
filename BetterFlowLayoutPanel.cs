using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WiiBrewToolbox
{
    public class BetterFlowLayoutPanel : FlowLayoutPanel
    {
        public BetterFlowLayoutPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Invalidate();
        }
    }
}
