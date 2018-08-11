using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Ohana3DS_Transfigured.Ohana;

namespace Ohana3DS_Transfigured.GUI
{
    public partial class OTexturesPanel : UserControl, IPanel
    {
        RenderEngine renderer;

        public OTexturesPanel()
        {
            InitializeComponent();
        }

        public void Launch(object data)
        {
            UpdateList((List<RenderBase.OTexture>)data);
        }

        public void Launch(RenderEngine renderer)
        {
            this.renderer = renderer;
            UpdateList(renderer.models.texture);
        }

        public void Clear()
        {
            TextureList.Flush();
        }

        private void UpdateList(List<RenderBase.OTexture> textures)
        {
            TextureList.Flush();
            TextureList.AddColumn(new OList.ColumnHeader(128, "#"));
            TextureList.AddColumn(new OList.ColumnHeader(128, "Name"));
            foreach (RenderBase.OTexture texture in textures)
            {
                OList.ListItemGroup item = new OList.ListItemGroup();
                item.columns.Add(new OList.ListItem(null, texture.texture));
                item.columns.Add(new OList.ListItem(texture.name));
                TextureList.AddItem(item);
            }
            TextureList.Refresh();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            FileIO.Export(FileIO.FileType.texture, renderer.models, TextureList.SelectedIndex);
        }

        private void BtnImport_Click(object sender, System.EventArgs e)
        {
            object importedData = FileIO.Import(FileIO.FileType.texture);
            if (importedData != null)
            {
                if (renderer != null) renderer.AddTextureRange((List<RenderBase.OTexture>)importedData);

                foreach (RenderBase.OTexture texture in (List<RenderBase.OTexture>)importedData)
                {
                    OList.ListItemGroup item = new OList.ListItemGroup();
                    item.columns.Add(new OList.ListItem(null, texture.texture));
                    item.columns.Add(new OList.ListItem(texture.name));
                    TextureList.AddItem(item);
                }

                TextureList.Refresh();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (TextureList.SelectedIndex == -1) return;
            if (renderer != null) renderer.RemoveTexture(TextureList.SelectedIndex);

            //Note: The SelectedIndex will change after this is called, so don't add it before the removeTexture!
            TextureList.RemoveItem(TextureList.SelectedIndex);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            TextureList.Flush(true);
            if (renderer != null) renderer.RemoveAllTextures();
        }
    }
}
