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

            string logFileName = "Log.txt";
            ReCreateFile(logFileName);
            Logger.LogFileName = logFileName;

            int resultValue = StartStmTasks();
            Console.WriteLine("Result: " + resultValue);
            Console.ReadLine();            
        }


        private static void ReCreateFile(string fileName)
        {
            FileStream fStream = File.Create(fileName);
            fStream.Close();
        }

        [Benchmark(Description = "StartStmTasks")]
        public static int StartStmTasks()
        {
            var variable = new StmRef<int>(0);

            List<Task> taskList = new List<Task>();

            taskList.Add(
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

            taskList.Add(
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

            taskList.Add(
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

            taskList.Add(
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

            Task.WaitAll(taskList.ToArray());            

            return variable.value;
        }

        [Benchmark(Description = "StartLockTasks")]
        public static int StartTasksWithLock()
        {
            LockRef<int> variable = new LockRef<int>(0);

            List<Task> taskList = new List<Task>();

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        variable.Set(2);
                        int temp = variable.Get();
                        temp += 3;
                        variable.Set(temp);
                    });                    
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        variable.Set(3);
                    });
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        variable.Set(4);
                    });
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        int temp = variable.Get();
                        temp += 9;
                        variable.Set(temp);
                    });
                }
                )
            );

            Task.WaitAll(taskList.ToArray());

            return variable.Get();
        }        
    }
}
