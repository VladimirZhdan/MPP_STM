using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace MPP_STM
{
    public class Logger
    {
        public static object logObject = new object();
        public static string LogFileName { get; set; }
        public static ConcurrentQueue<string> logsQueue = new ConcurrentQueue<string>();
        //public static BlockingCollection<string> logsQueue = new BlockingCollection<string>();
        private Thread logThread = new Thread(new ThreadStart(OutputLogs));
        public static bool IsNotEndOutputLogs { get; set; }
        public static bool IsLoggingThreadProgressed { get; private set; }
        
        public Logger()
        {
            if((LogFileName == null) || (LogFileName == ""))
            {
                LogFileName = "Temp.txt";
            }
            logThread.Start();         
        }

        public void ReadLog<T>(MethodBase method, long revision, StmRef<T> stmRef) where T : struct
        {
            string outputString = ("Transaction №" + revision + " - " + method.Name + "; value = " + stmRef.value + "; version = " + stmRef.revision);            
            logsQueue.Enqueue(outputString);      
        }

        public void WriteLog<T>(MethodBase method, long revision, StmRef<T> stmRef, T newValue) where T: struct
        {
            string outputString = ("Transaction №" + revision + " - " + method.Name + "; OldValue = " + stmRef.value + "; NewValue = " + newValue + "; version = " + stmRef.revision);
            logsQueue.Enqueue(outputString);           
        }

        public void Log(MethodBase method, long revision)
        {
            string outputString = ("Transaction №" + revision + " - " + method.Name);            
            logsQueue.Enqueue(outputString); 
        }

        public static void OutputLogs()
        {
            IsLoggingThreadProgressed = false;
            using (StreamWriter streamWriter = File.AppendText(LogFileName))
            {
                string nextLog;
                bool isLogsInQueue = true;                
                while (IsNotEndOutputLogs || (logsQueue.Count != 0))
                {
                    isLogsInQueue = logsQueue.TryDequeue(out nextLog);
                    if (isLogsInQueue)
                    {
                        streamWriter.WriteLine(nextLog);
                    }                                
                }                                
            }
            IsLoggingThreadProgressed = true;      
        }
    }
}
