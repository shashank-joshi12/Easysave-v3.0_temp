using System;
using System.Collections.Generic;
using System.Text;

namespace Easysave_v2._0.model
{
    public class BackupJob
    {
        
        public string SourceDir { get; set; }
        public string TargetDir { get; set; }
        public string SaveName { get; set; }
        public int Type { get; set; }
        public bool LogType { get; set; }
        

        public BackupJob(string saveName, string sourceDir, string targetDir, int type, bool logType)
        {
            SaveName = saveName;
            SourceDir = sourceDir;
            TargetDir = targetDir;
            Type = type;
            LogType = logType;
            
        }
    }
}
