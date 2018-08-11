using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Ohana3DS_Transfigured.Ohana;

namespace Ohana3DS_Transfigured.GUI
{
    public partial class OAnimationsPanel : UserControl, IPanel
    {
        private RenderEngine renderer;
        private RenderEngine.AnimationControl control;
        private RenderBase.OAnimationListBase animations;
        private FileIO.FileType type;

        private bool paused = true;
        private bool isAnimationLoaded;

        public OAnimationsPanel()
        {
            InitializeComponent();
        }

        public void Launch(object data)
        {
            RenderBase.OModelGroup group = (RenderBase.OModelGroup)data;
            animations = new RenderBase.OAnimationListBase();

            foreach (RenderBase.OAnimationBase mAnim in group.materialAnimation.list)
            {
                animations.list.Add(mAnim);
            }

            foreach (RenderBase.OAnimationBase sAnim in group.skeletalAnimation.list)
            {
                animations.list.Add(sAnim);
            }

            foreach (RenderBase.OAnimationBase vAnim in group.visibilityAnimation.list)
            {
                animations.list.Add(vAnim);
            }

            UpdateList();
        }

        public void Clear()
        {
            AnimationsList.Flush();
        }

        public void Launch(RenderEngine renderEngine, FileIO.FileType type)
        {
            renderer = renderEngine;
            this.type = type;
            switch (type)
            {
                case FileIO.FileType.skeletalAnimation:
                    control = renderer.ctrlSA;
                    animations = renderer.models.skeletalAnimation;
                    break;
                case FileIO.FileType.materialAnimation:
                    control = renderer.ctrlMA;
                    animations = renderer.models.materialAnimation;
                    break;
                case FileIO.FileType.visibilityAnimation:
                    control = renderer.ctrlVA;
                    animations = renderer.models.visibilityAnimation;
                    break;
            }
            
            control.FrameChanged += Control_FrameChanged;
            UpdateList();
        }

        private void UpdateList()
        {
            AnimationsList.Flush();
            if (control != null)
            {
                control.Load(-1);

                foreach (RenderBase.OAnimationBase animation in animations.list)
                {
                    AnimationsList.AddItem(animation.Name);
                }
            }
            AnimationsList.Refresh();
        }

        private void Control_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = ColorManager.ui_hoveredDark;
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = Color.FromArgb(0, 0, 0, 0);
        }

        private void BtnPlayPause_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isAnimationLoaded || e.Button != MouseButtons.Left) return;
            if (paused)
            {
                BtnPlayPause.Image = Properties.Resources.ui_icon_pause;
                paused = false;
                control.Play();
            }
            else
            {
                BtnPlayPause.Image = Properties.Resources.ui_icon_play;
                paused = true;
                control.Pause();
            }
        }

        private void BtnStop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            BtnPlayPause.Image = Properties.Resources.ui_icon_play;
            control.Stop();
            paused = true;
        }

        private void BtnPrevious_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (control.CurrentAnimation > 0)
            {
                control.Load(control.CurrentAnimation - 1);
                AnimationsList.SelectedIndex--;
            }
        }

        private void BtnNext_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (control.CurrentAnimation < animations.list.Count - 1)
            {
                control.Load(control.CurrentAnimation + 1);
                AnimationsList.SelectedIndex++;
            }
        }

        private void Control_FrameChanged(object sender, EventArgs e)
        {
            if (!Seeker.ManualSeeking && !paused) Seeker.Value = (int)control.Frame;
        }

        private void AnimationsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AnimationsList.SelectedIndex == -1) return;
            isAnimationLoaded = control.Load(AnimationsList.SelectedIndex);
            Seeker.MaximumSeek = (int)animations.list[AnimationsList.SelectedIndex].FrameSize;
            Seeker.Value = 0;
        }

        private void Seeker_Seek(object sender, EventArgs e)
        {
            control.Frame = Seeker.Value;
        }

        private void Seeker_SeekStart(object sender, EventArgs e)
        {
            control.Pause();
        }

        private void Seeker_SeekEnd(object sender, EventArgs e)
        {
            if (!paused) control.Play();
        }

        private void Speed_Seek(object sender, EventArgs e)
        {
            control.animationStep = (float)Speed.Value / 100;
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            RenderBase.OAnimationListBase animation = (RenderBase.OAnimationListBase)FileIO.Import(type);
            if (animation != null)
            {
                animations.list.AddRange(animation.list);
                foreach (RenderBase.OAnimationBase anim in animation.list) AnimationsList.AddItem(anim.Name);
                AnimationsList.Refresh();
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            switch (type)
            {
                case FileIO.FileType.skeletalAnimation:
                    if (renderer.CurrentModel == -1)
                    {
                        MessageBox.Show(
                            "A skeleton is necessary to export an Skeletal Animation." + Environment.NewLine +
                            "You must select a model before exporting!",
                            "Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (control.CurrentAnimation == -1) return;
                    FileIO.Export(type, renderer.models, renderer.CurrentModel, control.CurrentAnimation);
                    break;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (AnimationsList.SelectedIndex == -1) return;
            animations.list.RemoveAt(AnimationsList.SelectedIndex);
            AnimationsList.RemoveItem(AnimationsList.SelectedIndex);
            if (animations.list.Count == 0)
            {
                control.Stop();
                isAnimationLoaded = false;
                BtnPlayPause.Image = Properties.Resources.ui_icon_play;
                paused = true;
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            control.Stop();
            isAnimationLoaded = false;
            BtnPlayPause.Image = Properties.Resources.ui_icon_play;
            paused = true;

            animations.list.Clear();
            UpdateList();
        }
    }
}
