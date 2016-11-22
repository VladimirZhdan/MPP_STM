using System.Reflection;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace MPP_STM
{
    public class LoggerModified
    {
        public static object logObject = new object();
        public static string LogFileName { get; set; }
        public static ConcurrentQueue<string> logsQueue = new ConcurrentQueue<string>();
        private Thread logThread = new Thread(new ThreadStart(OutputLogs));
        public static bool IsNotEndOutputLogs { get; set; }
        public static bool IsLoggingThreadProgressed { get; private set; }

        public LoggerModified()
        {
            if ((LogFileName == null) || (LogFileName == ""))
            {
                LogFileName = "Temp.txt";
            }
            logThread.Start();
        }

        public void ReadLog<T>(MethodBase method, long revision, long parentRevision, StmRef<T> stmRef) where T : struct
        {
            string outputString = ("Transaction №" + revision);
            if (parentRevision != -1)
            {
                outputString = ("\t" + outputString);
                outputString += ("(Parent - " + parentRevision + ")");
            }
            outputString += (" - " + method.Name + "; variable: " + stmRef.ToString());
            logsQueue.Enqueue(outputString);
            //OutputLog(outputString);            
        }

        public void WriteLog<T>(MethodBase method, long revision, long parentRevision, StmRef<T> stmRef, T newValue) where T : struct
        {
            string outputString = ("Transaction №" + revision);
            if (parentRevision != -1)
            {
                outputString = ("\t" + outputString);
                outputString += ("(Parent - " + parentRevision + ")");
            }
            outputString += (" - " + method.Name + "; variable: " + stmRef.ToString() +  "; NewValue = " + newValue );            
            logsQueue.Enqueue(outputString);
            //OutputLog(outputString);            
        }

        public void Log(MethodBase method, long revision, long parentRevision, string message = null)
        {
            string outputString = ("Transaction №" + revision);            
            if(parentRevision != -1)
            {
                outputString = ("\t" + outputString);
                outputString += ("(Parent - " + parentRevision + ")");
            }
            outputString += (" - " + method.Name);            
            if(message != null)
            {
                outputString += (message);
            }
            logsQueue.Enqueue(outputString);
            //OutputLog(outputString);
        }

        private void OutputLog(string value)
        {
            lock(logObject)
            {
                using (StreamWriter streamWriter = File.AppendText(LogFileName))
                {
                    streamWriter.WriteLine(value);
                }
            }            
        }

        public static void OutputLogs()
        {
            IsLoggingThreadProgressed = false;
            
            string nextLog;
            bool isLogsInQueue = true;
            while (IsNotEndOutputLogs || (logsQueue.Count != 0))
            {
                isLogsInQueue = logsQueue.TryDequeue(out nextLog);
                if (isLogsInQueue)
                {
                    using (StreamWriter streamWriter = File.AppendText(LogFileName))
                    {
                        streamWriter.WriteLine(nextLog);
                    }
                }
            }            
            IsLoggingThreadProgressed = true;
        }

        public static void InitializationForStartLogging(string logFileName)
        {
            LogFileName = logFileName;
            IsNotEndOutputLogs = true;
            File.WriteAllText(LogFileName, "");
        }

        public static void WaitLoggingEnd()
        {
            IsNotEndOutputLogs = false;
            while (!IsLoggingThreadProgressed) ;
        }
    }
}
