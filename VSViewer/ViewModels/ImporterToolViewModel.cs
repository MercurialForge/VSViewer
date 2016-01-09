using GameFormatReader.Common;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Input;
using VSViewer.Common;
using VSViewer.FileFormats;
using VSViewer.Loader;
using VSViewer.Models;
using VSViewer.Rendering;

namespace VSViewer.ViewModels
{
    public class ImporterToolViewModel : ViewModelBase
    {
        #region Properties
        public string MainFileName
        {
            get
            {
                if (m_mainFile == null) { return "No File Chosen"; }
                return m_mainFile.Name;
            }
        }
        public string MainFilePath
        {
            get { return m_mainFile.ToString(); }
            set
            {
                m_mainFile = new FileInfo(value);
                OnPropertyChanged("MainFileName");
            }
        }
        #endregion

        #region Commands
        public ICommand OnMainFile
        {
            get { return new RelayCommand(x => PrepMainFile()); }
        }
        #endregion

        #region Private Fields / Properties
        MainWindowViewModel m_mainWindow;
        FileInfo m_mainFile;
        RenderCore core;
        #endregion

        public ImporterToolViewModel(MainWindowViewModel mainWindowViewModel, RenderCore theCore)
        {
            core = theCore;
            m_mainWindow = mainWindowViewModel;
        }

        #region Debug Code
        public ICommand FindNext
        {
            get { return new RelayCommand(x => FindNextCommand()); }
        }
        internal void FindNextCommand()
        {
            string path = @"C:\Users\Oliver\Desktop\VSDump\OBJ\" + t.ToString("X2") + ".WEP";
            //string path = @"E:\CloudServices\GoogleDrive\VSTools\OBJ\" + t.ToString("X2") + ".WEP";
            Console.WriteLine("Viewing WEP:" + t.ToString("X2"));
                core.Actor.SEQ = null;
                LoadShape(path);
                t++;
        }
        int t = 1;
        private void LoadShape(string path)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
            {
                // load type
                Geometry wepGeometry = VSTools.CreateGeometry(WEPLoader.FromStream(reader));
                core.Actor = new Actor(wepGeometry);
                core.TextureRequiresUpdate = true;
                m_mainWindow.TextureTool.ShowTool();
                m_mainWindow.AnimationTool.HideTool();
            }
        } 
        #endregion

        #region Command Methods
        internal void PrepMainFile()
        {
            string path = "";
            if (OpenMainFile(out path))
            {
                MainFilePath = path;
                core.Actor.SEQ = null;
                using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(MainFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.Little))
                {
                    LoadActor(reader);
                }
            }
        }

        #endregion

        #region Local Methods
        private void LoadActor(EndianBinaryReader reader)
        {
            switch (m_mainFile.Extension)
            {
                case ".WEP":
                    Geometry wepGeometry = VSTools.CreateGeometry(WEPLoader.FromStream(reader));
                    core.Actor = new Actor(wepGeometry);
                    core.TextureRequiresUpdate = true;
                    m_mainWindow.TextureTool.ShowTool();
                    m_mainWindow.AnimationTool.HideTool();
                    m_mainWindow.ViewportTool.ShowTool();
                    break;

                case ".SHP":
                    Geometry shpGeometry = VSTools.CreateGeometry(SHPLoader.FromStream(reader));
                    if (core.TextureIndex >= 2) { core.TextureIndex = 0; }
                    m_mainWindow.IsAnimationToolEnabled = true;
                    core.Actor = new Actor(shpGeometry);
                    core.TextureRequiresUpdate = true;
                    m_mainWindow.TextureTool.ShowTool();
                    m_mainWindow.AnimationTool.ShowTool();
                    m_mainWindow.ViewportTool.ShowTool();
                    break;

                case ".ZUD":
                    ZUD zud = ZUDLoader.FromStream(reader);
                    Console.WriteLine(string.Format("Wep:{0} -- Shd:{1} -- Com:{2} -- Bat{3}", zud.HasWeapon, zud.HasShield, zud.HasCommon, zud.HasBattle));
                    if (true)
                    {
                        Geometry zudGeometry = VSTools.CreateGeometry(zud.Character);
                        core.Actor = new Actor(zudGeometry);
                        core.Actor.SEQ = zud.Battle;
                        core.TextureRequiresUpdate = true;
                        m_mainWindow.TextureTool.ShowTool();
                        m_mainWindow.AnimationTool.ShowTool();
                        m_mainWindow.ViewportTool.ShowTool();
                        m_mainWindow.AnimationTool.ForceUpdate();
                    }
                    break;
            }
        }

        private bool OpenMainFile(out string outPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = 
                "Actors (*.WEP,*.SHP,*.ZUD)|*.WEP;*.SHP;*.ZUD|" +
                "Zone Unit Data (*.ZUD)|*.ZUD|" +
                "SHP (*.SHP)|*.SHP|" +
                "WEP (*.WEP)|*.WEP";
            if (openFileDialog.ShowDialog() == true)
            {
                outPath = openFileDialog.FileName;
                return true;
            }
            outPath = "";
            return false;
        }

        #endregion
    }
}
