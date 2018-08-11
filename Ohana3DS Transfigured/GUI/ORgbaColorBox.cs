using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ohana3DS_Transfigured.GUI
{
    public partial class ORgbaColorBox : UserControl
    {
        public event EventHandler ColorChanged;

        public ORgbaColorBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
                SeekA.BackColor = value;
                SeekR.BackColor = value;
                SeekG.BackColor = value;
                SeekB.BackColor = value;
            }
        }

        public Color Color
        {
            get
            {
                return CurrentColor();
            }
            set
            {
                SeekA.Value = value.A;
                SeekR.Value = value.R;
                SeekG.Value = value.G;
                SeekB.Value = value.B;
                UpdateColor(false);
            }
        }

        private void SeekR_ValueChanged(object sender, EventArgs e)
        {
            UpdateColor();
        }

        private void SeekG_ValueChanged(object sender, EventArgs e)
        {
            UpdateColor();
        }

        private void SeekB_ValueChanged(object sender, EventArgs e)
        {
            UpdateColor();
        }

        private void SeekA_ValueChanged(object sender, EventArgs e)
        {
            UpdateColor();
        }

        private void UpdateColor(bool colorChanged = true)
        {
            SelectedColor.BackColor = CurrentColor();
            if (ColorChanged != null && colorChanged) ColorChanged(this, EventArgs.Empty);
        }

        private Color CurrentColor()
        {
            int a = Clamp((int)SeekA.Value);
            int r = Clamp((int)SeekR.Value);
            int g = Clamp((int)SeekG.Value);
            int b = Clamp((int)SeekB.Value);
            return Color.FromArgb(a, r, g, b);
        }

        private int Clamp(int value)
        {
            return Math.Max(Math.Min(value, 255), 0);
        }
    }
}
