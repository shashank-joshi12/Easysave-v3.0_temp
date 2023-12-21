using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Easysave_v2._0.model;
using System.Collections.Generic;
using System.Windows;

namespace Easysave_v2._0.viewmodel
{
    public class ViewModel
    {
        public bool blacklist_stop { get; set; }
        private Model model;
        string[] blacklisted_app = Model.getBlackList();
        public string[] blacklitapp { get => blacklisted_app; set => blacklisted_app = value; }


        public ViewModel()
        {
            model = new Model();
        }
        public void AddSaveModel(int type, string saveName, string sourceDir, string targetDir, bool logType)//Function that allows you to add a backup
        {
            model.SaveName = saveName;
            BackupJob backup = new BackupJob(saveName, sourceDir, targetDir, type, logType);
            model.AddSave(backup); // Calling the function to add a backup job
        }
        public List<string> ListBackup()//Function that lets you know the lists of the names of the backups.
        {

            List<string> nameslist = new List<string>();
            foreach (var obj in model.NameList())
            {
                nameslist.Add(obj.SaveName);
            }
            return nameslist;
        }
        public void LoadBackup(string backupname)//Function that allows you to load the backups that were selected by the user.
        {
            blacklist_stop = true;

            if (Model.checkSoftware(blacklitapp))//If a program is in the blacklist we do not start the backup.
            {
                blacklist_stop = false;
            }
            else
            {
                model.LoadSave(backupname);//Function that launches backups
            }
        }
        public void DontSave() //Function that prevent EasySave from saving while a third party app is running
        {
            List<string> BL = new List<string>();

            foreach (string bl in blacklisted_app)
            {
                BL.Add(bl);

                Process[] i = Process.GetProcessesByName(bl);

                if (i.Length > 0 == true)
                {
                    foreach (Process process in i)
                    {
                        process.WaitForExit();

                        if (process.HasExited)
                        {
                            process.CloseMainWindow();

                            process.Close();
                        }
                    }
                }
            }
        }
    }
}
