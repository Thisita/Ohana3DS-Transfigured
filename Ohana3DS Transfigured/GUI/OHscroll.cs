﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Ohana3DS_Transfigured.GUI
{
    public partial class OHScroll : Control
    {
        private int scrollX;
        private int scrollBarX;

        private int scrollBarSize = 32;
        private int scroll;
        private bool mouseDrag;
        private Color foreColor;

        private Color barColor = Color.White;
        private Color barColorHover = Color.LightGray;

        private int max = 100;

        public event EventHandler ScrollChanged;

        public OHScroll()
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
            foreColor = barColor;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        /// <summary>
        ///     Scroll bar color when mouse is outside of the bar.
        /// </summary>
        public Color BarColor
        {
            get
            {
                return barColor;
            }
            set
            {
                barColor = value;
            }
        }

        /// <summary>
        ///     Scroll bar color when mouse is hovering the bar.
        /// </summary>
        public Color BarColorHover
        {
            get
            {
                return barColorHover;
            }
            set
            {
                barColorHover = value;
            }
        }

        /// <summary>
        ///     The maximum Value the scroll can have.
        /// </summary>
        public int MaximumScroll
        {
            get
            {
                return max;
            }
            set
            {
                if (value < 1) throw new Exception("OHScroll: Maximum value MUST be greater than 0!");
                max = value;
                RecalcSize();
                if (scrollX > value)
                {
                    scrollX = value;
                    scrollBarX = (int)Math.Max(Width - scrollBarSize, 0);
                    Refresh();
                }
            }
        }

        /// <summary>
        ///     The current value of the scroll (smaller than or equal to MaximumScroll).
        /// </summary>
        public int Value
        {
            get
            {
                return scrollX;
            }
            set
            {
                if (value > max) throw new Exception("OHScroll: The Value set is greater than the maximum value!");
                if (value < 0) throw new Exception("OHScroll: Value can't be less than 0!");
                scrollX = value;
                scrollBarX = (int)(((float)scrollX / max) * Math.Max(Width - scrollBarSize, 0));
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(foreColor), new Rectangle(scrollBarX, 0, scrollBarSize, Height));
            base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Rectangle scrollRect = new Rectangle(scrollBarX, 0, scrollBarSize, Height);
                if (scrollRect.Contains(e.Location))
                {
                    scroll = e.X - scrollBarX;
                    mouseDrag = true;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Focus();
            if (e.Button == MouseButtons.Left) mouseDrag = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Rectangle scrollRect = new Rectangle(scrollBarX, 0, scrollBarSize, Height);
            if (scrollRect.Contains(e.Location))
            {
                if (foreColor != barColorHover)
                {
                    foreColor = barColorHover;
                    Refresh();
                }
            }
            else if (!mouseDrag)
            {
                if (foreColor != barColor)
                {
                    foreColor = barColor;
                    Refresh();
                }
            }

            if (e.Button == MouseButtons.Left)
            {
                if (mouseDrag)
                {
                    int x = e.X - scroll;
                    if (x < 0) x = 0;
                    else if (x > Width - scrollBarSize) x = Math.Max(Width - scrollBarSize, 0);
                    scrollBarX = x;

                    scrollX = (int)(((float)x / Math.Max(Width - scrollBarSize, 1)) * max);
                    ScrollChanged?.Invoke(this, EventArgs.Empty);
                    Refresh();
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!mouseDrag) foreColor = barColor;
            Refresh();
            base.OnMouseLeave(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            RecalcSize();

            base.OnLayout(levent);
        }

        private void RecalcSize()
        {
            scrollBarSize = Math.Max(Width - max, 32);
            scrollBarX = (int)(((float)scrollX / max) * (Width - scrollBarSize));
            Refresh();
        }
    }
}
