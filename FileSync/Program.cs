using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FileSync
{
    class Program
    {
        static string SourcePath = "";
        static string TargetPath = "";
        static string LogPath = "";
        static int Period;
        public static List<RuleClass> rules = new List<RuleClass>();

        static void Main(string[] args)
        {       //Get information from the user
                Console.WriteLine("Please enter a source path: ");
                SourcePath = Console.ReadLine();
                Console.WriteLine("Please enter a target path: ");
                TargetPath = Console.ReadLine();
                Console.WriteLine("Please enter a Log path: ");
                LogPath = Console.ReadLine();
                Console.WriteLine("Please enter a time period for the program to execute in miliseconds: ");
                Period = Convert.ToInt32(Console.ReadLine());
                
            //Start the timer
            var timer = new Timer(timer_Elapsed, null, 0, Period);
            Console.ReadKey();

            void timer_Elapsed(object o)
            { 

                GetRulesFromUser();

                //Mapping information from the user to the RuleClass
                void GetRulesFromUser()
                {
                    RuleClass rule = new RuleClass();

                    rule.SourcePath = SourcePath;
                    rule.TargetPath = TargetPath;
                    rule.SourcePath = SourcePath;
                    rule.NumNewFolders = 0;
                    rule.NumNewFiles = 0;
                    rule.NumOverwrittenFiles = 0;

                    rules.Add(rule);
                }

                foreach (var rule in rules)
                {
                    var srcFolder = new DirectoryInfo(rule.SourcePath);
                    var trgFolder = new DirectoryInfo(rule.TargetPath);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Restart();
                    //Deleting from the target folder before copying
                    DeleteAll(srcFolder, trgFolder);

                    UpdateTargetDirs(rule, srcFolder);
                    UpdateTargetFiles(rule, srcFolder);

                    stopwatch.Stop();
                    rule.ElapsedTime = stopwatch.Elapsed;

                    SaveLogToFile(rule);
                    PrintLogToConsole(rule);
                }

                //If the directory doesnt exists in the target path, create a new directory and increment NumNewFolders by one
                void UpdateTargetDirs(RuleClass rule, DirectoryInfo folder)
                {
                    var srcFolderList = folder.EnumerateDirectories();
                    rule.NumNewFolders = 0;
                    if (srcFolderList.Count() > 0)
                    {
                        foreach (var fld in srcFolderList)
                        {
                            string targPath = GetTargetFolderPath(rule, fld);

                            if (Directory.Exists(targPath) == false)
                            {
                                Directory.CreateDirectory(targPath);
                                rule.NumNewFolders++;

                                UpdateTargetFiles(rule, fld);
                            }

                            UpdateTargetDirs(rule, fld);
                        }
                    }
                }
                //If a file doesnt exists in the target path, create the file and increment NumNewFiles by one.
                //If the LastWriteTime is newer in the source path than in target path, create a new file ad increment NumOverwrittenFiles by one.
                void UpdateTargetFiles(RuleClass rule, DirectoryInfo folder)
                {
                    var srcFileList = folder.EnumerateFiles();
                    rule.NumNewFiles = 0;
                    rule.NumOverwrittenFiles = 0;

                    if (srcFileList.Count() > 0)
                    {
                        foreach (var file in srcFileList)
                        {
                            string targPath = GetTargetFilePath(rule, folder, file);

                            if (File.Exists(targPath) == false)
                            {
                                file.CopyTo(targPath);
                                rule.NumNewFiles++;
                            }
                            else if (file.LastWriteTime > File.GetLastWriteTime(targPath))
                            {
                                file.CopyTo(targPath, true);
                                rule.NumOverwrittenFiles++;
                            }
                        }
                    }
                }

                string GetTargetFolderPath(RuleClass rule, DirectoryInfo folder)
                {
                    string targPath = rule.TargetPath + folder.FullName.Remove(0, rule.SourcePath.Length);
                    return targPath;

                }

                string GetTargetFilePath(RuleClass rule, DirectoryInfo folder, FileInfo file)
                {
                    string targPath = rule.TargetPath +
                        folder.FullName.Remove(0, rule.SourcePath.Length) + "\\" + file.Name;
                    return targPath;
                }

                //Saving log information to a log file desired by user.
                void SaveLogToFile(RuleClass rule)
                {
                    using (StreamWriter sw = new StreamWriter(LogPath, true))
                    {
                        sw.WriteLine(string.Format("Time: {0}", DateTime.Now.ToString("MM/dd/yyyy HH:mm")));
                        sw.WriteLine("Source Path: {0}", rule.SourcePath);
                        sw.WriteLine("Target Path: {0}", rule.TargetPath);
                        sw.WriteLine("Numbers of new folders: {0} Numbers of new files: {1} Numbers of overwritten files: {2}",
                            rule.NumNewFolders, rule.NumNewFiles, rule.NumOverwrittenFiles);
                        sw.WriteLine("Elapsed Time: {0:N2} Sec", rule.ElapsedTime.TotalSeconds);
                        sw.WriteLine("");
                    }
                }

                //Printing out log information to the console
                void PrintLogToConsole(RuleClass rule)
                {
                    Console.WriteLine("-----------------------------------------------------------");
                    Console.WriteLine(string.Format("Time: {0}", DateTime.Now.ToString("MM/dd/yyyy HH:mm")));
                    Console.WriteLine("Source Path: {0}", rule.SourcePath);
                    Console.WriteLine("Target Path: {0}", rule.TargetPath);
                    Console.WriteLine("Numbers of new folders: {0} Numbers of new files: {1} Numbers of overwritten files: {2}",
                            rule.NumNewFolders, rule.NumNewFiles, rule.NumOverwrittenFiles);
                    Console.WriteLine("Elapsed Time: {0:N2} Sec", rule.ElapsedTime.TotalSeconds);
                    Console.ReadLine();
                }

                //Deleting all files in the target directory
                void DeleteAll (DirectoryInfo folder, DirectoryInfo target)
                {
                    var srcFld = folder.EnumerateDirectories();

                    if (srcFld.Count() > 0)
                    {
                        Directory.Delete(TargetPath, true);
                    }
                }

            }
        }
    }
}
