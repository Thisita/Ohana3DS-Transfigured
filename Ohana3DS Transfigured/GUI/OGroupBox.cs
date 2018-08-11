﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Ohana3DS_Transfigured.GUI
{
    [Designer(typeof(OGroupBoxDesigner))]
    public partial class OGroupBox : UserControl
    {
        private string title;
        private int originalHeight = 256;
        private bool collapsed;
        private bool autoSize;

        public const int collapsedHeight = 24;

        public OGroupBox()
        {
            Init();
            InitializeComponent();
        }

        private void Init()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Panel ContentArea
        {
            get { return ContentPanel; }
        }

        /// <summary>
        ///     Set to true to hide content, or false to show them.
        /// </summary>
        public bool Collapsed
        {
            get
            {
                return collapsed;
            }
            set
            {
                if (value == collapsed) return;
                if (value) Collapse(); else Expand();
            }
        }

        /// <summary>
        ///     Set the expanded Height equal to the size of all controls.
        ///     It will NOT work if a Control uses the Dock Fill property, for obvious reasons.
        /// </summary>
        public bool AutomaticSize
        {
            get
            {
                return autoSize;
            }
            set
            {
                autoSize = value;
                Recalc();
            }
        }

        /// <summary>
        ///     GroupBox Title text.
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                UpdateTitle();
            }
        }

        /// <summary>
        ///     BackColor of the inner Panel.
        /// </summary>
        public Color ContentColor
        {
            get
            {
                return ContentPanel.BackColor;
            }
            set
            {
                ContentPanel.BackColor = value;
            }
        }

        /// <summary>
        ///     The Height of the control content when it is expanded.
        ///     It's not necessary to set this when AutomaticSize is enabled.
        /// </summary>
        public int ExpandedHeight
        {
            get
            {
                return originalHeight - TitleBar.Height;
            }
            set
            {
                originalHeight = value + TitleBar.Height;
            }
        }

        /// <summary>
        ///     Occurs whenever the GroupBox is expanded from a collapsed state.
        /// </summary>
        public event EventHandler GroupBoxExpanded;

        protected override void OnControlAdded(ControlEventArgs e)
        {
            e.Control.Layout += Control_Layout;

            base.OnControlAdded(e);
        }

        private void Control_Layout(object sender, EventArgs e)
        {
            Recalc();
        }

        private void Recalc()
        {
            if (autoSize)
            {
                int maxY = 0;
                foreach (Control child in ContentPanel.Controls)
                {
                    int y = child.Top + child.Height;
                    if (child.Visible && y > maxY) maxY = y;
                }
                originalHeight = maxY + TitleBar.Height;
                if (!collapsed) Height = originalHeight;
            }
        }

        /// <summary>
        ///     Forces the size to be recalculated, if AutomaticSize is enabled.
        /// </summary>
        public void RecalculateSize()
        {
            Recalc();
        }

        private void OGroupBox_Layout(object sender, LayoutEventArgs e)
        {
            ContentPanel.Location = new Point(0, TitleBar.Height);
            ContentPanel.Size = new Size(Width, Height - TitleBar.Height);
            UpdateTitle();
        }

        private void OGroupBox_EnabledChanged(object sender, EventArgs e)
        {
            BtnToggle.Visible = Enabled;
        } 

        private void BtnToggle_MouseEnter(object sender, EventArgs e)
        {
            BtnToggle.BackColor = ColorManager.ui_hoveredDark;
        }

        private void BtnToggle_MouseLeave(object sender, EventArgs e)
        {
            BtnToggle.BackColor = Color.FromArgb(0, 0, 0, 0);
        }

        private void BtnToggle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (collapsed) Expand(); else Collapse();
        }

        private void UpdateTitle()
        {
            using (Graphics g = Graphics.FromHwnd(Handle))
            {
                LblTitle.Text = DrawingUtils.ClampText(g, title, LblTitle.Font, Width - (BtnToggle.Width + 4));
            }
        }

        private void Expand()
        {
            Recalc();
            collapsed = false;
            BtnToggle.Image = Properties.Resources.ui_icon_minus;
            Height = originalHeight;

            GroupBoxExpanded?.Invoke(this, EventArgs.Empty);
        }

        private void Collapse()
        {
            collapsed = true;
            BtnToggle.Image = Properties.Resources.ui_icon_plus;
            Height = TitleBar.Height;
        }

        public class OGroupBoxDesigner : ParentControlDesigner
        {
            public override void Initialize(IComponent component)
            {
                base.Initialize(component);

                if (Control is OGroupBox)
                {
                    EnableDesignMode(((OGroupBox)Control).ContentArea, "ContentArea");
                }
            }
        }
    }
}
