using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Ohana3DS_Transfigured.GUI;
using Ohana3DS_Transfigured.Ohana;
using Ohana3DS_Transfigured.Properties;
using Ohana3DS_Transfigured.Ohana.Models;
using Ohana3DS_Transfigured.Ohana.Models.PocketMonsters;
using Ohana3DS_Transfigured.Ohana.Textures;
using Ohana3DS_Transfigured.Ohana.Textures.PocketMonsters;
using Ohana3DS_Transfigured.Ohana.Animations;

namespace Ohana3DS_Transfigured
{
    public partial class FrmMain : OForm
    {
        bool hasFileToOpen;
        string fileToOpen;
        FileIO.File file;

        public FrmMain()
        {
            InitializeComponent();
            TopMenu.Renderer = new OMenuStrip();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //Viewport menu settings
            switch (Settings.Default.reAntiAlias)
            {
                case 0: MenuViewAANone.Checked = true; break;
                case 2: MenuViewAA2x.Checked = true; break;
                case 4: MenuViewAA4x.Checked = true; break;
                case 8: MenuViewAA8x.Checked = true; break;
                case 16: MenuViewAA16x.Checked = true; break;
            }

            MenuViewShowGuidelines.Checked = Settings.Default.reShowGuidelines;
            MenuViewShowInformation.Checked = Settings.Default.reShowInformation;
            MenuViewShowAllMeshes.Checked = Settings.Default.reShowAllMeshes;

            MenuViewFragmentShader.Checked = Settings.Default.reFragmentShader;
            switch (Settings.Default.reLegacyTexturingMode)
            {
                case 0: MenuViewTexUseFirst.Checked = true; break;
                case 1: MenuViewTexUseLast.Checked = true; break;
            }
            if (MenuViewFragmentShader.Checked)
            {
                MenuViewTexUseFirst.Enabled = false;
                MenuViewTexUseLast.Enabled = false;
            }

            MenuViewShowSidebar.Checked = Settings.Default.viewShowSidebar;
            MenuViewWireframeMode.Checked = Settings.Default.reWireframeMode;

            if (hasFileToOpen)
            {
                RenderBase.OModelGroup group = PC.Load(fileToOpen);
                group.model[0].name = Path.GetFileNameWithoutExtension(fileToOpen);

                object[] arguments = { 0, 0, Path.Combine(Path.GetDirectoryName(fileToOpen), Path.GetFileNameWithoutExtension(fileToOpen)) };

                FileIO.Export(FileIO.FileType.model, group, arguments);

                file.data = null;

                Close();

                Application.Exit();
            }
        }

        public void SetFileToOpen(string fileName)
        {
            hasFileToOpen = true;
            fileToOpen = fileName;
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            if (hasFileToOpen) Open(fileToOpen);
            hasFileToOpen = false;
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (currentPanel != null) currentPanel.Clear();
        }

        delegate void openFile(string fileNmame);
        FileIO.FormatType currentFormat;
        IPanel currentPanel;

        public void Open(string fileName)
        {
            file = FileIO.Load(fileName);
            currentFormat = file.type;

            if (file.type != FileIO.FormatType.unsupported)
            {
                switch (file.type)
                {
                    case FileIO.FormatType.container: currentPanel = new OContainerPanel(); break;
                    case FileIO.FormatType.image: currentPanel = new OImagePanel(); break;
                    case FileIO.FormatType.model: currentPanel = new OViewportPanel(); break;
                    case FileIO.FormatType.texture: currentPanel = new OTexturesPanel(); break;
                    case FileIO.FormatType.animation: currentPanel = new OAnimationsPanel(); break;
                }

                ((Control)currentPanel).Dock = DockStyle.Fill;
                SuspendDrawing();
                ContentContainer.Controls.Add((Control)currentPanel);
                ContentContainer.Controls.SetChildIndex((Control)currentPanel, 0);
                ResumeDrawing();
                currentPanel.Launch(file.data);
            }
            else
                MessageBox.Show("Unsupported file format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void FrmMain_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void FrmMain_DragEnter(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            RenderBase.OModelGroup group;

            if (files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    group = PC.Load(files[i]);
                    group.model[0].name = Path.GetFileNameWithoutExtension(files[i]);

                    object[] arguments = { 0, 0, Path.Combine(Path.GetDirectoryName(files[i]), Path.GetFileNameWithoutExtension(files[i])) };

                    FileIO.Export(FileIO.FileType.model, group, arguments);

                    file.data = null;
                }
            }
        }

        private void DestroyOpenPanels()
        {
            if (currentPanel != null)
            {
                currentPanel.Clear();
                ContentContainer.Controls.Remove((Control)currentPanel);
            }
        }

        #region "Menus"
        /*
         * File
         */

        //Open

        private void MenuOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDlg = new OpenFileDialog())
            {
                openDlg.Filter = "All files|*.*";
                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    DestroyOpenPanels();
                    Open(openDlg.FileName);
                }
            }
        }

        /*
         * Options -> Viewport
         */

        //Anti Aliasing

        private void MenuViewAANone_Click(object sender, EventArgs e)
        {
            SetAACheckBox((ToolStripMenuItem)sender, 0);
        }

        private void MenuViewAA2x_Click(object sender, EventArgs e)
        {
            SetAACheckBox((ToolStripMenuItem)sender, 2);
        }

        private void MenuViewAA4x_Click(object sender, EventArgs e)
        {
            SetAACheckBox((ToolStripMenuItem)sender, 4);
        }

        private void MenuViewAA8x_Click(object sender, EventArgs e)
        {
            SetAACheckBox((ToolStripMenuItem)sender, 8);
        }

        private void MenuViewAA16x_Click(object sender, EventArgs e)
        {
            SetAACheckBox((ToolStripMenuItem)sender, 16);
        }

        private void SetAACheckBox(ToolStripMenuItem control, int value)
        {
            MenuViewAANone.Checked = false;
            MenuViewAA2x.Checked = false;
            MenuViewAA4x.Checked = false;
            MenuViewAA8x.Checked = false;
            MenuViewAA16x.Checked = false;

            control.Checked = true;
            Settings.Default.reAntiAlias = value;
            Settings.Default.Save();
            UpdateViewportSettings();
            ChangesNeedsRestart();
        }

        //Background

        private void MenuViewBgBlack_Click(object sender, EventArgs e)
        {
            SetViewportBgColor(Color.Black.ToArgb());
        }

        private void MenuViewBgGray_Click(object sender, EventArgs e)
        {
            SetViewportBgColor(Color.DimGray.ToArgb());
        }

        private void MenuViewBgWhite_Click(object sender, EventArgs e)
        {
            SetViewportBgColor(Color.White.ToArgb());
        }

        private void MenuViewBgCustom_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDlg = new ColorDialog())
            {
                if (colorDlg.ShowDialog() == DialogResult.OK) SetViewportBgColor(colorDlg.Color.ToArgb());
            }
        }

        private void SetViewportBgColor(int color)
        {
            Settings.Default.reBackgroundColor = color;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        //Show/hide

        private void MenuViewShowGuidelines_Click(object sender, EventArgs e)
        {
            MenuViewShowGuidelines.Checked = !MenuViewShowGuidelines.Checked;
            Settings.Default.reShowGuidelines = MenuViewShowGuidelines.Checked;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        private void MenuViewShowInformation_Click(object sender, EventArgs e)
        {
            MenuViewShowInformation.Checked = !MenuViewShowInformation.Checked;
            Settings.Default.reShowInformation = MenuViewShowInformation.Checked;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        private void MenuViewShowAllMeshes_Click(object sender, EventArgs e)
        {
            MenuViewShowAllMeshes.Checked = !MenuViewShowAllMeshes.Checked;
            Settings.Default.reShowAllMeshes = MenuViewShowAllMeshes.Checked;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        //Texturing

        private void MenuViewFragmentShader_Click(object sender, EventArgs e)
        {
            MenuViewFragmentShader.Checked = !MenuViewFragmentShader.Checked;

            if (MenuViewFragmentShader.Checked)
            {
                MenuViewTexUseFirst.Enabled = false;
                MenuViewTexUseLast.Enabled = false;
            }
            else
            {
                MenuViewTexUseFirst.Enabled = true;
                MenuViewTexUseLast.Enabled = true;
            }

            Settings.Default.reFragmentShader = MenuViewFragmentShader.Checked;
            Settings.Default.Save();
            UpdateViewportSettings();
            ChangesNeedsRestart();
        }

        private void MenuViewTexUseFirst_Click(object sender, EventArgs e)
        {
            MenuViewTexUseFirst.Checked = true;
            MenuViewTexUseLast.Checked = false;
            Settings.Default.reLegacyTexturingMode = 0;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        private void MenuViewTexUseLast_Click(object sender, EventArgs e)
        {
            MenuViewTexUseFirst.Checked = false;
            MenuViewTexUseLast.Checked = true;
            Settings.Default.reLegacyTexturingMode = 1;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        //Misc. UI

        private void MenuViewShowSidebar_Click(object sender, EventArgs e)
        {
            MenuViewShowSidebar.Checked = !MenuViewShowSidebar.Checked;
            Settings.Default.viewShowSidebar = MenuViewShowSidebar.Checked;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        private void MenuViewWireframeMode_Click(object sender, EventArgs e)
        {
            MenuViewWireframeMode.Checked = !MenuViewWireframeMode.Checked;
            Settings.Default.reWireframeMode = MenuViewWireframeMode.Checked;
            Settings.Default.Save();
            UpdateViewportSettings();
        }

        private void UpdateViewportSettings()
        {
            if (currentFormat == FileIO.FormatType.model)
            {
                OViewportPanel viewport = (OViewportPanel)currentPanel;
                viewport.renderer.UpdateSettings();
                viewport.ShowSidebar = MenuViewShowSidebar.Checked;
            }
        }

        private void ChangesNeedsRestart()
        {
            if (currentFormat == FileIO.FormatType.model)
            {
                MessageBox.Show(
                    "Please restart the rendering engine to make those changes take effect!",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /*
         * Help
         */

        //About

        private void MenuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Ohana3DS Rebirth made by gdkchan. Recoded into Transfigured by Quibilia.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}
