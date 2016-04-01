using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Tasks;

namespace Tasks
{
    public class AsyncExec : Exec
    {
        protected override int ExecuteTool(string pathToTool,
                                           string responseFileCommands,
                                           string commandLineCommands)
        {
            Process process = new Process();
            process.StartInfo = GetProcessStartInfo(pathToTool, commandLineCommands);
            process.Start();
            return 0;
        }

        protected virtual ProcessStartInfo GetProcessStartInfo(string executable,
                                                               string arguments)
        {
            if (arguments.Length > 0x7d00)
            {
                this.Log.LogWarningWithCodeFromResources("ToolTask.CommandTooLong", new object[] { base.GetType().Name });
            }
            ProcessStartInfo startInfo = new ProcessStartInfo(executable, arguments);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = true;
            string workingDirectory = this.GetWorkingDirectory();
            if (workingDirectory != null)
            {
                startInfo.WorkingDirectory = workingDirectory;
            }
            StringDictionary environmentOverride = this.EnvironmentOverride;
            if (environmentOverride != null)
            {
                foreach (DictionaryEntry entry in environmentOverride)
                {
                    startInfo.EnvironmentVariables.Remove(entry.Key.ToString());
                    startInfo.EnvironmentVariables.Add(entry.Key.ToString(), entry.Value.ToString());
                }
            }
            return startInfo;
        }
    }
}
