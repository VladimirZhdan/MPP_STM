using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace MPP_STM
{
    public class Program
    {        
        
        public static void Main(string[] args)
        {
            Stm.UseLoggingStmTransaction = true;
            int resultValue = StartTasks();
            Console.WriteLine("Result: " + resultValue);
            Console.ReadLine();            
        }


        private static void ReCreateFile(string fileName)
        {
            FileStream fStream = File.Create(fileName);
            fStream.Close();
        }

        [Benchmark(Description = "StartTasks")]
        public static int StartTasks()
        {
            string logFileName = "Log.txt";
            ReCreateFile(logFileName);
            Logger.LogFileName = logFileName;

            var variable = new StmRef<int>(0);

            List<Task> task = new List<Task>();            

            task.Add(
                Task.Run(() =>
                {
                    Stm.Do<int>(new TransactionBlock<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        variable.Set(2, stmTransaction);
                        int temp = variable.Get(stmTransaction);
                        temp += 3;
                        variable.Set(temp, stmTransaction);
                    }
                    ));
                })
            );

            task.Add(
                Task.Run(() =>
                {
                    Stm.Do<int>(new TransactionBlock<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        variable.Set(3, stmTransaction);
                    }
                    ));
                })
            );

            task.Add(
                Task.Run(() =>
                {
                    Stm.Do<int>(new TransactionBlock<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        variable.Set(4, stmTransaction);
                    }
                    ));
                })
            );

            task.Add(
                Task.Run(() =>
                {
                    Stm.Do<int>(new TransactionBlock<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        int temp = variable.Get(stmTransaction);
                        temp += 9;
                        variable.Set(temp, stmTransaction);
                    }
                    ));
                })
            );

            Task.WaitAll(task.ToArray());            

            return variable.value;
        }

        

    }
}
