using System;
using System.Collections.Generic;
using System.Text;

namespace Easysave_v2._0.model

{
    class JsonLoggerData
    {
        
        public string SourceDir { get; set; }
        public string TargetDir { get; set; }
        public string SaveName { get; set; }
        public string BackupDate { get; set; }
        public string ElapsedTime { get; set; }
        public long TotalSize { get; set; }
        public string CryptTime { get; set; }

    }
}
