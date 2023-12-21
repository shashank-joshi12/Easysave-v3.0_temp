using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Easysave_v2._0.viewmodel;
using Easysave_v2._0.model;
using System.Windows.Forms;
using System.Diagnostics;
using MessageBox = System.Windows.MessageBox;

namespace Easysave_v2._0.view
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string language;
        public ViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            viewModel = new ViewModel();
            ShowBackupList();
            Process currentProccess = Process.GetCurrentProcess(); 

            Process[] runningProcesses = Process.GetProcesses();

            foreach (Process p in runningProcesses)
                if ((currentProccess.Id != p.Id) && (currentProccess.ProcessName == p.ProcessName)) 
                {                          
                    MessageBox.Show("An instance of the application is already running");
                    this.Close();                    
                }



        }
        
        private void SaveBackupButton_Click(object sender, RoutedEventArgs e)
        {
            string saveName = "";
            string sourceDir = "";
            string targetDir = "";
            

            saveName = BackupNameTextBox.Text;
            sourceDir = SourcePathTextBox.Text;
            targetDir = TargetPathTextBox.Text;
            

            if (FullBackupTypeRadioButton.IsChecked.Value) 
            {
                if (BackupNameTextBox.Text.Length.Equals(0) || SourcePathTextBox.Text.Length.Equals(0) || TargetPathTextBox.Text.Length.Equals(0))
                {
                    if (language == "fr")
                    {
                        ResultTextBox.Text = " Remplissez tous les champs, s'il vous plaît! ";
                    }
                    else
                    {
                        ResultTextBox.Text = " Please complete all the fields! ";
                    }
                }
                else
                {
                    int type = 1;

                    viewModel.AddSaveModel(type, saveName, sourceDir, targetDir); //Function to add the backup

                    if (language == "fr")//Condition for the display of the success message according to the language chosen by the user.
                    {
                        ResultTextBox.Text = "VOUS AVEZ AJOUTÉ UNE SAUVEGARDE \n";
                    }
                    else
                    {
                        ResultTextBox.Text = "YOU HAVE ADDED A BACKUP";
                    }

                    ShowBackupList();//Function to update the list.
                }

            }
            else if (DifferentialBackupTypeRadioButton.IsChecked.Value)//If the button of the full backup is selected
            {
                if (BackupNameTextBox.Text.Length.Equals(0) || SourcePathTextBox.Text.Length.Equals(0) || TargetPathTextBox.Text.Length.Equals(0))
                {
                    if (language == "fr")
                    {
                        ResultTextBox.Text = "Remplissez tous les champs, s'il vous plaît! ";
                    }
                    else
                    {
                        ResultTextBox.Text = " Please complete all the fields! ";
                    }
                }
                else
                {
                    int type = 2;
                    viewModel.AddSaveModel(type, saveName, sourceDir, targetDir);//Function to add the backup

                    if (language == "fr")//Condition for the display of the success message according to the language chosen by the user.
                    {
                        ResultTextBox.Text = "VOUS AVEZ AJOUTÉ UNE SAUVEGARDE DIFFÉRENTIELLE";
                    }
                    else
                    {
                        ResultTextBox.Text = "YOU HAVE ADDED A DIFFERENTIAL BACKUP";
                    }

                    ShowBackupList();//Function to update the list.
                }

            }

        }
    
        private void RunBackupButton_Click(object sender, RoutedEventArgs e)
        {
            string saveName = "";

            if (BackupJobsListView.SelectedItem != null) //Condition that allows to check if the user has selected a backup.
            {
                foreach (NameList item in BackupJobsListView.SelectedItems)//Loop that allows you to select multiple saves
                {
                    saveName = item.Name;
                    viewModel.LoadBackup(saveName);

                    if (viewModel.blacklist_stop == false) //Check for message display if blacklisted software was detected.
                    {
                        if (language == "fr")
                        {
                            ResultTextBox.Text = "ECHEC DE SAUVEGARDE\n" +
                                "ERREUR : LOGICIEL SUR LISTE NOIRE EN EXECUTION";
                        }
                        else
                        {   
                            ResultTextBox.Text = "BACKUP FAILURE\n" +
                                "ERROR: BLACKLISTED SOFTWARE RUNNING";
                        }
                    }
                    else
                    {
                        if (language == "fr")
                        {
                            ResultTextBox.Text = "SAUVEGARDE RÉUSSIE";
                        }
                        else
                        {
                            ResultTextBox.Text = "BACKUP SUCCESSFUL";
                        }
                    }
                }

            }
        }


        private void EnglishButton_Clicked(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = "Results";
            language = "en";
            BackupNameTextBlock.Text = "Backup Name:";
            SourcePathTextBlock.Text = "Source Path:";
            TargetPathTextBlock.Text = "Target Path:";
            BackupTypeTextBlock.Text = "Backup Type:";
            LogFileTextBlock.Text = "Log File Type:";
            LanguagesTextBlock.Text = "Languages:";
            SourcePathTextBox.Text = "Enter the source path here";
            TargetPathTextBox.Text = "Enter the target path here";
            BackupNameTextBox.Text = "BackupJob_";
            SaveBackupButton.Content = "Save Backup";
            SaveBackupButton.FontSize = 15;
            RunBackupButton.Content = "Run Backup";
            RunBackupButton.FontSize = 15;
            ExitButton.Content = "Exit";
            EncryptionExtentionsButton.Content = "Encryption Extentions";
            EncryptionExtentionsButton.FontSize = 10;
            BlacklistSoftwaresButton.Content = "Blacklist Softwares";

        }

        private void FrenchButton_Clicked(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = "Résultats";
            language = "fr";
            BackupNameTextBlock.Text = "Nom de la sauvegarde:";
            SourcePathTextBlock.Text = "Chemin source:";
            TargetPathTextBlock.Text = "Chemin cible:";
            BackupTypeTextBlock.Text = "Type de sauvegarde:";
            LogFileTextBlock.Text = "Type de fichier journal:";
            LanguagesTextBlock.Text = "Langues:";
            SourcePathTextBox.Text = "Entrez le chemin source ici";
            TargetPathTextBox.Text = "Entrez le chemin cible ici";
            BackupNameTextBox.Text = "BackupJob_";
            SaveBackupButton.Content = "Enregistrer la sauvegarde";
            SaveBackupButton.FontSize = 13;
            RunBackupButton.Content = "Exécuter la sauvegarde";
            RunBackupButton.FontSize = 14;
            ExitButton.Content = "Sortie";
            EncryptionExtentionsButton.Content = "Extensions de chiffrement";
            EncryptionExtentionsButton.FontSize = 9;
            BlacklistSoftwaresButton.Content = "Logiciels sur liste noire";
        }
        private void ShowBackupList()
        {
            BackupJobsListView.Items.Clear();

            List<string> names = viewModel.ListBackup();
            foreach (string name in names)//Loop that allows you to manage the names in the list.
            {
                _ = BackupJobsListView.Items.Add(new NameList()
                {
                    Name = name
                }
                ); //Function that allows you to insert the names of the backups in the list.
            }
        }
        public class NameList
        {
            public string Name { get; set; }
        }

        private void EncryptionExtentionsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"..\..\..\Resources\CryptExtension.json");
        }

        private void BlacklistSoftwaresButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"..\..\..\Resources\BlackList.json");
        }

        private void BackupJobsListView_Loaded(object sender, RoutedEventArgs e)
        {
            ShowBackupList();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChooseTargetPathButton_Click(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    
                    string selectedFolderSourcePath = folderBrowserDialog.SelectedPath;
                    TargetPathTextBox.Text = selectedFolderSourcePath;
                }
            }

        }

        private void ChooseSourcePathButton_Click(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {

                    string selectedFolderSourcePath = folderBrowserDialog.SelectedPath;
                    SourcePathTextBox.Text = selectedFolderSourcePath;
                }
            }
        }

        private void ChoosePriorityFilesButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"..\..\..\Resources\PriorityExtension.json");
        }
    }
}
