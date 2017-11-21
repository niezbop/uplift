using System;
using System.IO;
using System.Text;
using UnityEngine;
using FileSystemUtil = Uplift.Common.FileSystemUtil;

namespace Uplift.Common
{
    public class TemporaryGitClone : IDisposable
    {
        private static readonly string stub = "uplift_temp_clone";
        private TemporaryDirectory parent;
        private bool disposed = false;
        public string RepositoryPath
        {
            get { return Path.Combine(parent.Path, stub); }
        }
        
        public TemporaryGitClone(string uri, string branch = "master", bool shallow = true)
        {
            parent = new TemporaryDirectory();
            try
            {
                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.Append(string.Format("/C git clone --single-branch {0} {1} --branch={2}", uri, RepositoryPath, branch));
                if(shallow) argumentsBuilder.Append(" --depth=1");

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = argumentsBuilder.ToString();
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                if(process.ExitCode != 0)
                {
                    Debug.LogError("There was an error while cloning the repo at " + uri);
                    Debug.LogError(process.StandardError.ReadToEnd().Replace("\n", " "));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not retrieve requirement from git at " + uri);
                throw e;
            }
        }

        ~ TemporaryGitClone()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                // Make .git files not readonly
                string[] tempEntries = FileSystemUtil.RecursivelyListFiles(Path.Combine(RepositoryPath, ".git")).ToArray();
                for(int i = 0; i < tempEntries.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(tempEntries[i]);
                    fileInfo.IsReadOnly = false;
                }
                parent.Dispose();
                disposed = true;
            }
        }
    }
}