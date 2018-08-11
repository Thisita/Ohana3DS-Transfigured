﻿using System;
using System.Linq;
using System.Windows.Forms;

using Ohana3DS_Transfigured.Ohana;
using Ohana3DS_Transfigured.Properties;

namespace Ohana3DS_Transfigured.GUI
{
    public partial class OViewportPanel : UserControl, IPanel
    {
        public RenderEngine renderer;

        RenderBase.OVector2 initialRotation;
        RenderBase.OVector2 initialMovement;
        RenderBase.OVector2 finalMovement;
        bool clicked;

        public bool ShowSidebar
        {
            set
            {
                Splitter.Panel1Collapsed = !value;
            }
        }

        public OViewportPanel()
        {
            initialRotation = new RenderBase.OVector2();
            initialMovement = new RenderBase.OVector2();
            finalMovement = new RenderBase.OVector2();

            InitializeComponent();
        }

        private void OViewportPanel_Load(object sender, EventArgs e)
        {
            ShowSidebar = Settings.Default.viewShowSidebar;
        }

        public void Launch(object data)
        {
            renderer = new RenderEngine
            {
                models = (RenderBase.OModelGroup)data
            };
            renderer.Initialize(Screen.Handle, Screen.Width, Screen.Height);

            ModelsPanel.Launch(renderer);
            TexturesPanel.Launch(renderer);
            SkeletalAnimationsPanel.Launch(renderer, FileIO.FileType.skeletalAnimation);
            MaterialAnimationsPanel.Launch(renderer, FileIO.FileType.materialAnimation);
            VisibilityAnimationsPanel.Launch(renderer, FileIO.FileType.visibilityAnimation);

            renderer.Render();
        }

        public void Clear()
        {
            TexturesPanel.Clear();
            renderer.Dispose();
        }

        private void Screen_Resize(object sender, EventArgs e)
        {
            if (renderer != null) renderer.Resize(Screen.Width, Screen.Height);
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicked)
            {
                if (renderer != null)
                {
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            float rY = (float)(((e.X - initialRotation.x) / Screen.Width) * Math.PI);
                            float rX = (float)(((e.Y - initialRotation.y) / Screen.Height) * Math.PI);
                            renderer.SetRotation(rY, rX);
                            initialRotation = new RenderBase.OVector2(e.X, e.Y);
                            break;
                        case MouseButtons.Right:
                            float tX = (initialMovement.x - e.X) + finalMovement.x;
                            float tY = (initialMovement.y - e.Y) + finalMovement.y;
                            renderer.SetTranslation(tX, tY);
                            break;
                    }
                }
            }
        }

        private void Screen_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Screen.Focused) Screen.Select();
            clicked = true;
            switch (e.Button)
            {
                case MouseButtons.Left: initialRotation = new RenderBase.OVector2(e.X, e.Y); break;
                case MouseButtons.Right: initialMovement = new RenderBase.OVector2(e.X, e.Y); break;
            }
        }

        private void Screen_MouseUp(object sender, MouseEventArgs e)
        {
            if (clicked)
            {
                clicked = false;
                float x = finalMovement.x + (initialMovement.x - e.X);
                float y = finalMovement.y + (initialMovement.y - e.Y);
                if (e.Button == MouseButtons.Right) finalMovement = new RenderBase.OVector2(x, y);
            }
        }

        private void Screen_MouseWheel(object sender, MouseEventArgs e)
        {
            float step = 1f;
            if (ModifierKeys == Keys.Shift) step = 0.1f;
            if (renderer != null && e.Delta > 0)
                renderer.SetZoom(renderer.Zoom + step);
            else
                renderer.SetZoom(renderer.Zoom - step);
        }

        private void Splitter_Panel1_Layout(object sender, LayoutEventArgs e)
        {
            RecalcGroupSize();
        }

        private void Group_GroupBoxExpanded(object sender, EventArgs e)
        {
            OGroupBox senderGroup = (OGroupBox)sender;
            foreach (OGroupBox group in Splitter.Panel1.Controls.OfType<OGroupBox>())
            {
                if (group != senderGroup) group.Collapsed = true;
            }

            RecalcGroupSize();
        }

        private void RecalcGroupSize()
        {
            int usedSpace = (Splitter.Panel1.Controls.Count - 1) * OGroupBox.collapsedHeight;
            foreach (OGroupBox group in Splitter.Panel1.Controls.OfType<OGroupBox>())
            {
                if (!group.Collapsed)
                {
                    group.Height = Splitter.Panel1.Height - usedSpace;
                    break;
                }
            }
        }
    }
}
