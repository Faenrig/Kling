using System;
using System.Windows.Forms;

namespace Kling
{
    public partial class Display : Form
    {
        int WM_NCHITTEST = 0x84, HTTRANSPARENT = -1;
        
        public Display()
        {
            InitializeComponent();
            AllowTransparency = true;
            Opacity = 0.6;
            ShowInTaskbar = false;
            TopMost = true;
        }
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WM_NCHITTEST)
                m.Result = (IntPtr)HTTRANSPARENT;
            else
                base.WndProc(ref m);
        }
        
        public void SetText(string Text)
        {
            label1.Text = Text;
        }
    }
}
