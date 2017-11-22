using System;
using System.IO;
using System.Text;
using UnityEngine;
using FileSystemUtil = Uplift.Common.FileSystemUtil;

namespace Uplift.Common
{
    public class TemporaryGitClone : TemporaryDirectory
    {
        private static readonly string stub = "uplift_temp_clone_";
        
        public TemporaryGitClone(string uri, string branch = "master", bool shallow = true)
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                stub + System.IO.Path.GetFileNameWithoutExtension(uri)
            );
            try
            {
                StringBuilder argumentsBuilder = new StringBuilder();
                argumentsBuilder.Append(string.Format("/C git clone --single-branch {0} {1} --branch={2}", uri, Path, branch));
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

        public override void Dispose()
        {
            if (!disposed)
            {
                // Make .git files not readonly
                string[] tempEntries = FileSystemUtil.RecursivelyListFiles(System.IO.Path.Combine(Path, ".git")).ToArray();
                for(int i = 0; i < tempEntries.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(tempEntries[i]);
                    fileInfo.IsReadOnly = false;
                }
                base.Dispose();
                disposed = true;
            }
        }
    }
}