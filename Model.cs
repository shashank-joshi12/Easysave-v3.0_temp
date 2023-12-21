using System;
using System.Collections.Generic;
using System.Text;
using Easysave_v2._0.viewmodel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Xml;

namespace Easysave_v2._0.model
{
    class Model
    {
        public int noOfBackups;
        private string serializeObj;
        public string backupJobsFile = System.Environment.CurrentDirectory + @"\BackupJobs\";
        public string backupStatusFile = System.Environment.CurrentDirectory + @"\BackupJobsStatus\";
        public string LogDir = @"..\..\..\Logs\";
        public StatusData statusData { get; set; }
        public string StatusFile { get; set; }
        public string BackupNameState { get; set; }
        public string SourcePath { get; set; }
        public int nbfilesmax { get; set; }
        public int nbfiles { get; set; }
        public long size { get; set; }
        public float progs { get; set; }
        public string TargetPath { get; set; }
        public string SaveName { get; set; }
        public int Type { get; set; }
        public string SourceFile { get; set; }
        public string TypeString { get; set; }
        public long TotalSize { get; set; }
        public TimeSpan TimeTakenForBackup { get; set; }
        public TimeSpan TimeTakenForEncryption { get; set; }
        public string userMenuInput { get; set; }

        public Model()
        {
            userMenuInput = " ";

            if (!Directory.Exists(backupJobsFile)) //Check if the folder is created
            {
                DirectoryInfo Dir = Directory.CreateDirectory(backupJobsFile); //Function that creates the folder
            }
            backupJobsFile += @"backupList.json"; //Create a JSON file

            if (!Directory.Exists(backupStatusFile))//Check if the folder is created
            {
                DirectoryInfo Dir = Directory.CreateDirectory(backupStatusFile); //Function that creates the folder
            }
            backupStatusFile += @"state.json"; //Create a JSON file

            if (!Directory.Exists(LogDir)) //Check if the folder is created
            {
                DirectoryInfo Dir = Directory.CreateDirectory(LogDir); //Function that creates the folder
            }

        }
        public void CompleteSave(string srcpath, string tgtpath, bool copyDir, bool verif) //Function for full folder backup
        {
            statusData = new StatusData(StatusFile);
            this.statusData.SaveState = true;
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch cryptwatch = new Stopwatch();
            stopwatch.Start(); //Starting the timed for the log file

            DirectoryInfo dir = new DirectoryInfo(srcpath);  // Get the subdirectories for the specified directory.

            if (!dir.Exists) //Check if the file is present
            {
                throw new DirectoryNotFoundException("ERROR 404 : Directory Not Found ! " + srcpath);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(tgtpath); // If the destination directory doesn't exist, create it.  

            FileInfo[] files = dir.GetFiles(); // Get the files in the directory and copy them to the new location.

            if (!verif) //  Check for the status file if it needs to reset the variables
            {
                TotalSize = 0;
                nbfilesmax = 0;
                size = 0;
                nbfiles = 0;
                progs = 0;

                foreach (FileInfo file in files) // Loop to allow calculation of files and folder size
                {
                    TotalSize += file.Length;
                    nbfilesmax++;
                }
                foreach (DirectoryInfo subdir in dirs) // Loop to allow calculation of subfiles and subfolder size
                {
                    FileInfo[] Maxfiles = subdir.GetFiles();
                    foreach (FileInfo file in Maxfiles)
                    {
                        TotalSize += file.Length;
                        nbfilesmax++;
                    }
                }

            }

            //Loop that allows to copy the files to make the backup
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(tgtpath, file.Name);

                if (size > 0)
                {
                    progs = ((float)size / TotalSize) * 100;
                }

                //Systems which allows to insert the values ​​of each file in the report file.
                statusData.SourceFile = Path.Combine(srcpath, file.Name);
                statusData.TargetFile = tempPath;
                statusData.TotalSize = nbfilesmax;
                statusData.TotalFile = TotalSize;
                statusData.Progress = progs;

                UpdateBackupStatusFile(); //Call of the function to start the state file system

                if (UsePriorityExtension(Path.GetExtension(file.Name)))
                {
                    if (CryptExt(Path.GetExtension(file.Name)))
                    {
                        cryptwatch.Start();
                        Encrypt(statusData.SourceFile, tempPath);
                        cryptwatch.Stop();
                    }
                    else
                    {
                        file.CopyTo(tempPath, true); //Function that allows you to copy the file to its new folder.
                    }

                }
                else
                {
                    if (CryptExt(Path.GetExtension(file.Name)))
                    {
                        cryptwatch.Start();
                        Encrypt(statusData.SourceFile, tempPath);
                        cryptwatch.Stop();
                    }
                    else
                    {
                        file.CopyTo(tempPath, true); //Function that allows you to copy the file to its new folder.
                    }
                }

                nbfiles++;
                size += file.Length;

            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copyDir)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(tgtpath, subdir.Name);
                    CompleteSave(subdir.FullName, tempPath, copyDir, true);
                }
            }
            //System which allows the values ​​to be reset to 0 at the end of the backup
            statusData.TotalSize = TotalSize;
            statusData.SourceFile = null;
            statusData.TargetFile = null;
            statusData.TotalFile = 0;
            statusData.TotalSize = 0;
            statusData.Progress = 0;
            statusData.SaveState = false;

            UpdateBackupStatusFile(); //Call of the function to start the state file system

            stopwatch.Stop(); //Stop the stopwatch
            cryptwatch.Stop();
            this.TimeTakenForBackup = stopwatch.Elapsed; // Transfer of the chrono time to the variable
            this.TimeTakenForEncryption = cryptwatch.Elapsed;
        }
        public void DifferentialSave(string srcpath, string tgtpath, string tgtpathM) // Function that allows you to make a differential backup
        {
            statusData = new StatusData(StatusFile); //Instattation of the method
            Stopwatch stopwatch = new Stopwatch(); // Instattation of the method
            Stopwatch cryptwatch = new Stopwatch();
            stopwatch.Start(); //Starting the stopwatch

            statusData.SaveState = true;
            TotalSize = 0;
            nbfilesmax = 0;

            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(srcpath);
            System.IO.DirectoryInfo dir2 = new System.IO.DirectoryInfo(tgtpath);

            // Take a snapshot of the file system.  
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> list2 = dir2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            //A custom file comparer defined below  
            FileComparison _2filecomapre = new FileComparison();

            var queryList1Only = (from file in list1 select file).Except(list2, _2filecomapre);
            size = 0;
            nbfiles = 0;
            progs = 0;

            foreach (var v in queryList1Only)
            {
                TotalSize += v.Length;
                nbfilesmax++;

            }

            //Loop that allows the backup of different files
            foreach (var v in queryList1Only)
            {
                string tempPath = Path.Combine(tgtpathM, v.Name);
                //Systems which allows to insert the values ​​of each file in the report file.
                statusData.SourceFile = Path.Combine(srcpath, v.Name);
                statusData.TargetFile = tempPath;
                statusData.TotalSize = nbfilesmax;
                statusData.TotalFile = TotalSize;
                statusData.Progress = progs;

                UpdateBackupStatusFile();//Call of the function to start the state file system

                if (UsePriorityExtension(Path.GetExtension(v.Name)))
                {
                    if (CryptExt(Path.GetExtension(v.Name)))
                    {
                        cryptwatch.Start();
                        Encrypt(statusData.SourceFile, tempPath);
                        cryptwatch.Stop();
                    }
                    else
                    {
                        v.CopyTo(tempPath, true); //Function that allows you to copy the file to its new folder.
                    }
                }
                else
                {
                    if (CryptExt(Path.GetExtension(v.Name)))
                    {
                        cryptwatch.Start();
                        Encrypt(statusData.SourceFile, tempPath);
                        cryptwatch.Stop();
                    }
                    else
                    {
                        v.CopyTo(tempPath, true); //Function that allows you to copy the file to its new folder.
                    }
                }
                size += v.Length;
                nbfiles++;
            }

            //System which allows the values ​​to be reset to 0 at the end of the backup
            statusData.SourceFile = null;
            statusData.TargetFile = null;
            statusData.TotalFile = 0;
            statusData.TotalSize = 0;
            statusData.Progress = 0;
            statusData.SaveState = false;
            UpdateBackupStatusFile();//Call of the function to start the state file system

            stopwatch.Stop(); //Stop the stopwatch
            this.TimeTakenForBackup = stopwatch.Elapsed; // Transfer of the chrono time to the variable
            this.TimeTakenForEncryption= cryptwatch.Elapsed;
        }
        private void UpdateBackupStatusFile()//Function that updates the status file.
        {
            List<StatusData> stateList = new List<StatusData>();
            this.serializeObj = null;
            if (!File.Exists(StatusFile)) //Checking if the file exists
            {
                File.Create(StatusFile).Close();
            }

            string jsonString = File.ReadAllText(StatusFile);  //Reading the json file

            if (jsonString.Length != 0) //Checking the contents of the json file is empty or not
            {
                StatusData [] list = JsonConvert.DeserializeObject<StatusData[]>(jsonString); //Derialization of the json file

                foreach (var obj in list) // Loop to allow filling of the JSON file
                {
                    if (obj.SaveName == this.backupStatusFile) //Verification so that the name in the json is the same as that of the backup
                    {
                        obj.SourceFile = this.statusData.SourceFile;
                        obj.TargetFile = this.statusData.TargetFile;
                        obj.TotalFile = this.statusData.TotalFile;
                        obj.TotalSize = this.statusData.TotalSize;
                        obj.Progress = this.statusData.Progress;
                        obj.BackupDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        obj.SaveState = this.statusData.SaveState;
                    }

                    stateList.Add(obj); //Allows you to prepare the objects for the json filling

                }

                this.serializeObj = JsonConvert.SerializeObject(stateList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file

                File.WriteAllText(StatusFile, this.serializeObj); //Function to write to JSON file
                
            }


        }
        public void UpdateLogFile(string savename, string sourcedir, string targetdir, bool logType)//Function to allow modification of the log file
        {
            Stopwatch stopwatch = new Stopwatch(); //Declaration of the stopwatch
            string elapsedTimeBackup = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", TimeTakenForBackup.Hours, TimeTakenForBackup.Minutes, TimeTakenForBackup.Seconds, TimeTakenForBackup.Milliseconds / 10);
            string elapsedTimeEncryption = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", TimeTakenForEncryption.Hours, TimeTakenForEncryption.Minutes, TimeTakenForEncryption.Seconds, TimeTakenForEncryption.Milliseconds / 10);

            JsonLoggerData loggerData= new JsonLoggerData //Variable moves retrieved from the variables for placement in the JSON file.
            {
                SaveName = savename,
                SourceDir = sourcedir,
                TargetDir = targetdir,
                BackupDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                TotalSize = TotalSize,
                ElapsedTime = elapsedTimeBackup,
                CryptTime = elapsedTimeEncryption,
            };

            List<JsonLoggerData> loggerDataList = new List<JsonLoggerData>();
            this.serializeObj = null;
            var directory = System.IO.Path.GetDirectoryName(LogDir + @"/Logs/");
            var path = directory + @"DailyLogs_JSON_" + DateTime.Now.ToString("dd-MM-yyyy") + ".json";

            if (!File.Exists(path))
            {
                File.WriteAllText(path, this.serializeObj);
            }
            string jsonString = File.ReadAllText(path);

            if (jsonString.Length != 0)
            {
                JsonLoggerData[] list = JsonConvert.DeserializeObject<JsonLoggerData[]>(jsonString);
                foreach (var obj in list) // Loop to allow filling of the JSON file
                {
                    loggerDataList.Add(obj);
                }
            }
            loggerDataList.Add(loggerData); //Allows you to prepare the objects for the json filling
            this.serializeObj = JsonConvert.SerializeObject(loggerDataList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file
            File.WriteAllText(path, this.serializeObj); //Function to write to JSON file


            if (logType)
            {
                var pathXML = directory + @"DailyLogs_XML_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xml";
                var logEntry = new //define the value for the log
                {
                    JobName = loggerData.SaveName,
                    SourceFolder = loggerData.SourceDir,
                    DestinationFile = loggerData.TargetDir,
                    FolderSize = loggerData.TotalSize,
                    TransferTime = loggerData.ElapsedTime,
                    CryptTime = loggerData.CryptTime
                };

                if (File.Exists(pathXML))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(pathXML);

                    XmlElement logEntryElement = doc.CreateElement("LogEntry");

                    XmlElement timestampElement = doc.CreateElement("Transfer Time");
                    timestampElement.InnerText = logEntry.TransferTime.ToString();
                    logEntryElement.AppendChild(timestampElement);

                    XmlElement cryptTimeElement = doc.CreateElement("Encryption Time");
                    timestampElement.InnerText = logEntry.CryptTime.ToString();
                    logEntryElement.AppendChild(timestampElement);

                    XmlElement jobNameElement = doc.CreateElement("Backup Job Name");
                    jobNameElement.InnerText = logEntry.JobName;
                    logEntryElement.AppendChild(jobNameElement);

                    XmlElement sourceFolderElement = doc.CreateElement("Source Path");
                    sourceFolderElement.InnerText = logEntry.SourceFolder;
                    logEntryElement.AppendChild(sourceFolderElement);

                    XmlElement destinationFileElement = doc.CreateElement("Target Path");
                    destinationFileElement.InnerText = logEntry.DestinationFile;
                    logEntryElement.AppendChild(destinationFileElement);

                    XmlElement folderSizeElement = doc.CreateElement("Total Size ");
                    folderSizeElement.InnerText = logEntry.FolderSize.ToString();
                    logEntryElement.AppendChild(folderSizeElement);

                    
                    doc.DocumentElement?.AppendChild(logEntryElement);


                    doc.Save(pathXML);
                }
                else
                {
                    XmlDocument doc = new XmlDocument();

                    XmlElement root = doc.CreateElement("LogEntries");
                    doc.AppendChild(root);

                    XmlElement logEntryElement = doc.CreateElement("LogEntry");

                    XmlElement timestampElement = doc.CreateElement("Transfer Time");
                    timestampElement.InnerText = logEntry.TransferTime.ToString();
                    logEntryElement.AppendChild(timestampElement);

                    XmlElement cryptTimeElement = doc.CreateElement("Encryption Time");
                    timestampElement.InnerText = logEntry.CryptTime.ToString();
                    logEntryElement.AppendChild(timestampElement);

                    XmlElement jobNameElement = doc.CreateElement("Backup Job Name");
                    jobNameElement.InnerText = logEntry.JobName;
                    logEntryElement.AppendChild(jobNameElement);

                    XmlElement sourceFolderElement = doc.CreateElement("Source Path");
                    sourceFolderElement.InnerText = logEntry.SourceFolder;
                    logEntryElement.AppendChild(sourceFolderElement);

                    XmlElement destinationFileElement = doc.CreateElement("Target Path");
                    destinationFileElement.InnerText = logEntry.DestinationFile;
                    logEntryElement.AppendChild(destinationFileElement);

                    XmlElement folderSizeElement = doc.CreateElement("Total Size ");
                    folderSizeElement.InnerText = logEntry.FolderSize.ToString();
                    logEntryElement.AppendChild(folderSizeElement);

                    root.AppendChild(logEntryElement);

                    doc.Save(pathXML);

                }

            }

            stopwatch.Reset(); // Reset of stopwatch
        }
        public void AddSave(BackupJob backupJob) //Function that creates a backup job
        {
            List<BackupJob> backupList = new List<BackupJob>();
            this.serializeObj = null;

            if (!File.Exists(backupJobsFile)) //Checking if the file exists
            {
                File.WriteAllText(backupJobsFile, this.serializeObj);
            }

            string jsonString = File.ReadAllText(backupJobsFile); //Reading the json file

            if (jsonString.Length != 0) //Checking the contents of the json file is empty or not
            {
                BackupJob[] list = JsonConvert.DeserializeObject<BackupJob[]>(jsonString); //Derialization of the json file
                foreach (var obj in list) //Loop to add the information in the json
                {
                    backupList.Add(obj);
                }
            }
            backupList.Add(backupJob); //Allows you to prepare the objects for the json filling

            this.serializeObj = JsonConvert.SerializeObject(backupList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file
            File.WriteAllText(backupJobsFile, this.serializeObj); // Writing to the json file

            statusData = new StatusData(this.SaveName); //Class initiation

            statusData.BackupDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); //Adding the time in the variable
            AddState(); //Call of the function to add the backup in the report file.
        }
        public void AddState() //Function that allows you to add a backup job to the report file.
        {
            List<StatusData> stateList = new List<StatusData>();
            this.serializeObj = null;

            if (!File.Exists(backupStatusFile)) //Checking if the file exists
            {
                File.Create(backupStatusFile).Close();
            }

            string jsonString = File.ReadAllText(backupStatusFile); //Reading the json file

            if (jsonString.Length != 0)
            {
                StatusData[] list = JsonConvert.DeserializeObject<StatusData[]>(jsonString); //Derialization of the json file
                foreach (var obj in list) //Loop to add the information in the json
                {
                    stateList.Add(obj);
                }
            }
            this.statusData.SaveState = false;
            stateList.Add(this.statusData); //Allows you to prepare the objects for the json filling

            this.serializeObj = JsonConvert.SerializeObject(stateList.ToArray(), Newtonsoft.Json.Formatting.Indented) + Environment.NewLine; //Serialization for writing to json file
            File.WriteAllText(backupStatusFile, this.serializeObj);// Writing to the json file
        }
        public void LoadSave(string backupname) //Function that allows you to load backup jobs
        {
            BackupJob backupJob = null;
            this.TotalSize = 0;
            BackupNameState = backupname;

            string jsonString = File.ReadAllText(backupJobsFile); //Reading the json file


            if (jsonString.Length != 0) //Checking the contents of the json file is empty or not
            {
                BackupJob[] list = JsonConvert.DeserializeObject<BackupJob[]>(jsonString);  //Derialization of the json file
                foreach (var obj in list)
                {
                    if (obj.SaveName == backupname) //Check to have the correct name of the backup
                    {
                        backupJob = new BackupJob(obj.SaveName, obj.SourceDir, obj.TargetDir, obj.Type, obj.LogType);//Function that allows you to retrieve information about the backup

                        break;
                    }
                }
            }

            if (backupJob.Type == 1) //If the type is 1, it means it's a full backup
            {
                StatusFile = backupJob.SaveName;
                CompleteSave(backupJob.SourceDir, backupJob.TargetDir, true, false); //Calling the function to run the full backup
                UpdateLogFile(backupJob.SaveName, backupJob.SourceDir, backupJob.TargetDir, backupJob.LogType); //Call of the function to start the modifications of the log file
                Console.WriteLine("Saved Successfull !"); //Satisfaction message display
            }
            else //If this is the wrong guy then, it means it's a differential backup
            {
                StatusFile = backupJob.SaveName;
                DifferentialSave(backupJob.SourceDir, backupJob.TargetDir, backupJob.TargetDir); //Calling the function to start the differential backup
                UpdateLogFile(backupJob.SaveName, backupJob.SourceDir, backupJob.TargetDir, backupJob.LogType); //Call of the function to start the modifications of the log file
                Console.WriteLine("Saved Successfull !"); //Satisfaction message display
            }

        }
        public void CheckDataFile()  // Function that allows to count the number of backups in the json file of backup jobs
        {
            noOfBackups = 0;

            if (File.Exists(backupJobsFile)) //Check on file exists
            {
                string jsonString = File.ReadAllText(backupJobsFile);//Reading the json file
                if (jsonString.Length != 0)//Checking the contents of the json file is empty or not
                {
                    BackupJob[] list = JsonConvert.DeserializeObject<BackupJob[]>(jsonString); //Derialization of the json file
                    noOfBackups = list.Length; //Allows to count the number of backups
                }
            }
        }
        public void Encrypt(string sourceDir, string targetDir)//This function allows you to encrypt files. 
        {
            using (Process process = new Process())//Declaration of the process
            {
                process.StartInfo.FileName = @"..\..\..\Resources\CryptoSoft\CryptoSoft.exe"; //Calls the process that is CryptoSoft
                process.StartInfo.Arguments = String.Format("\"{0}\"", sourceDir) + " " + String.Format("\"{0}\"", targetDir); //Preparation of variables for the process.
                process.Start(); //Launching the process
                process.Close();

            }

        }
        private static string[] getExtensionCrypt()//Function that allows to recover the extensions that the user wants to encrypt in the json file.
        {
            using (StreamReader reader = new StreamReader(@"..\..\..\Resources\CryptExtension.json"))//Function to read the json file
            {
                CryptFormat[] item_crypt;
                string[] crypt_extensions_array;
                string json = reader.ReadToEnd();
                List<CryptFormat> items = JsonConvert.DeserializeObject<List<CryptFormat>>(json);
                item_crypt = items.ToArray();
                crypt_extensions_array = item_crypt[0].extension_to_crypt.Split(',');

                return crypt_extensions_array; //We return the variables that are stored in an array
            }
        }
        public static bool CryptExt(string extension)//Function that compares the extensions of the json file and the one of the file being backed up.
        {
            foreach (string extensionExt in getExtensionCrypt())
            {
                if (extensionExt == extension)
                {
                    return true;
                }
            }
            return false;
        }
        public List<BackupJob> NameList()//Function that lets you know the names of the backups.
        {
            List<BackupJob> backupList = new List<BackupJob>();

            if (!File.Exists(backupJobsFile)) //Checking if the file exists
            {
                File.WriteAllText(backupJobsFile, this.serializeObj);
            }

            List<BackupJob> names = new List<BackupJob>();
            string jsonString = File.ReadAllText(backupJobsFile); //Function to read json file
            BackupJob[] list = JsonConvert.DeserializeObject<BackupJob[]>(jsonString); // Function to dezerialize the json file

            if (jsonString.Length != 0)
            {
                foreach (var obj in list) //Loop to display the names of the backups
                {
                    names.Add(obj);
                }

            }

            return names;

        }
        public static string[] getBlackList()//Function that allows to recover software that is blacklisted.
        {
            using (StreamReader reader = new StreamReader(@"C:\Users\ratwo\source\repos\Easysave v2.0\Resources\BlackList.json"))//Function to read the json file
            {
                BlackListFormat[] item_blacklist;
                string[] blacklist_array;
                string json = reader.ReadToEnd();
                List<BlackListFormat> items = JsonConvert.DeserializeObject<List<BlackListFormat>>(json);
                item_blacklist = items.ToArray();
                blacklist_array = item_blacklist[0].blacklisted_items.Split(',');

                return blacklist_array;//We return the names of the softwares which are in the list of the json file.
            }
        }

        public static bool checkSoftware(string[] blacklist_app)//Function that allows you to compare a program that is in the list is running.
        {
            foreach (string process in blacklist_app)
            {
                if (Process.GetProcessesByName(process).Length > 0)
                {
                    return true;
                }
            }

            return false;
        }
        public static string[] getPriorityExtensions() //Function that allows to recover the extensions of the files to be prioritized
        {
            using (StreamReader reader = new StreamReader(@"..\..\..\Resources\PriorityExtension.json"))//Function to read the json file
            {
                FilePriorityFormat[] itemsPriorityList;
                string[] priorityFileList;
                string json = reader.ReadToEnd();
                List<FilePriorityFormat> items = JsonConvert.DeserializeObject<List<FilePriorityFormat>>(json);
                itemsPriorityList = items.ToArray();
                priorityFileList = itemsPriorityList[0].filePriorityExtension.Split(',');

                return priorityFileList;
            }
        }
        public static bool UsePriorityExtension(string extension) //Function that compares the extensions of the file to be prioritized json and that of the saved file.
        {
            foreach (string priorityExtension in getPriorityExtensions())
            {
                if (priorityExtension == extension)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
