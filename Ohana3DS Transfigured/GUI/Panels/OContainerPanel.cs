﻿using System.IO;
using System.Windows.Forms;

using Ohana3DS_Transfigured.Ohana.Containers;

namespace Ohana3DS_Transfigured.GUI
{
    public partial class OContainerPanel : UserControl, IPanel
    {
        OContainer container;

        public OContainerPanel()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            if (container.data != null)
            {
                container.data.Close();
                container.data = null;
            }
        }

        public void Launch(object data)
        {
            container = (OContainer)data;
            FileList.AddColumn(new OList.ColumnHeader(384, "Name"));
            FileList.AddColumn(new OList.ColumnHeader(128, "Size"));
            foreach (OContainer.FileEntry file in container.content)
            {
                OList.ListItemGroup item = new OList.ListItemGroup();
                item.columns.Add(new OList.ListItem(file.name));
                uint length = file.loadFromDisk ? file.fileLength : (uint)file.data.Length;
                item.columns.Add(new OList.ListItem(GetLength(length)));
                FileList.AddItem(item);
            }
            FileList.Refresh();
        }

        private void BtnExportAll_Click(object sender, System.EventArgs e)
        {
            using (FolderBrowserDialog browserDlg = new FolderBrowserDialog())
            {
                browserDlg.Description = "Export all files";
                if (browserDlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (OContainer.FileEntry file in container.content)
                    {
                        string fileName = Path.Combine(browserDlg.SelectedPath, file.name);
                        string dir = Path.GetDirectoryName(fileName);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        if (file.loadFromDisk)
                        {
                            byte[] buffer = new byte[file.fileLength];
                            container.data.Seek(file.fileOffset, SeekOrigin.Begin);
                            container.data.Read(buffer, 0, buffer.Length);
                            File.WriteAllBytes(fileName, buffer);
                        }
                        else
                            File.WriteAllBytes(fileName, file.data);
                    }
                }
            }
        }

        private void BtnExport_Click(object sender, System.EventArgs e)
        {
            if (FileList.SelectedIndex == -1) return;
            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Title = "Export file";
                saveDlg.FileName = container.content[FileList.SelectedIndex].name;
                saveDlg.Filter = "All files|*.*";
                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    OContainer.FileEntry file = container.content[FileList.SelectedIndex];

                    if (file.loadFromDisk)
                    {
                        byte[] buffer = new byte[file.fileLength];
                        container.data.Seek(file.fileOffset, SeekOrigin.Begin);
                        container.data.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(saveDlg.FileName, buffer);
                    }
                    else
                        File.WriteAllBytes(saveDlg.FileName, file.data);
                }
            }
        }

        string[] lengthUnits = { "Bytes", "KB", "MB", "GB", "TB" };
        private string GetLength(uint length)
        {
            int i = 0;
            while (length > 0x400)
            {
                length /= 0x400;
                i++;
            }

            return length.ToString() + " " + lengthUnits[i];
        }
    }
}
