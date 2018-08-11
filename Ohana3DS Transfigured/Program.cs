﻿using System;
using System.IO;
using System.Windows.Forms;

namespace Ohana3DS_Transfigured
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FrmMain form = new FrmMain();
            if (args.Length > 0 && File.Exists(args[0])) form.SetFileToOpen(args[0]);
            Application.Run(form);
        }
    }
}
