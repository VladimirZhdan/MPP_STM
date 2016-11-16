using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace MPP_STM
{
    public class Logger
    {
        public static string LogFileName { get; set; }       
        
        public Logger()
        {
            if((LogFileName == null) || (LogFileName == ""))
            {
                LogFileName = "Temp.txt";
            }
        } 
        
        public void ReadLog<T>(MethodBase method, long revision, StmRef<T> stmRef) where T: struct
        {
            using (StreamWriter streamWriter = File.AppendText(LogFileName))
            {
                string outputString = ("Transaction №" + revision + " - " + method.Name + "; value = " + stmRef.value + "; version = " + stmRef.revision);
                streamWriter.WriteLine(outputString);
            }
        }

        public void WriteLog<T>(MethodBase method, long revision, StmRef<T> stmRef, T newValue) where T: struct
        {
            using (StreamWriter streamWriter = File.AppendText(LogFileName))
            {
                string outputString = ("Transaction №" + revision + " - " + method.Name + "; OldValue = " + stmRef.value + "; NewValue = " + newValue + "; version = " + stmRef.revision);
                streamWriter.WriteLine(outputString);
            }
        }

        public void Log(MethodBase method, long revision)
        {
            using (StreamWriter streamWriter = File.AppendText(LogFileName))
            {
                string outputString = ("Transaction №" + revision + " - " + method.Name);
                streamWriter.WriteLine(outputString);
            }
        }
    }
}
