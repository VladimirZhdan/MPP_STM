using System;
using System.Collections.Generic;
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
            //int resultValue = StartAndWaitTasksWithLoggingTransactions();
            int resultValue = StartAndWaitSubStmTasksWithLoggingTransactions();
            Console.WriteLine("Result: " + resultValue);
            Console.ReadLine();
        }
       
        private static int StartAndWaitTasksWithLoggingTransactions()
        {
            Stm.UseLoggingStmTransaction = true;
            string logFileName = "Log.txt";
            ReCreateFile(logFileName);
            Logger.LogFileName = logFileName;
            Logger.IsNotEndOutputLogs = true;

            int resultValue = StartStmTasks();
            Logger.IsNotEndOutputLogs = false;
            while (!Logger.IsLoggingThreadProgressed) ;

            return resultValue;            
        }

        private static void ReCreateFile(string fileName)
        {
            FileStream fStream = File.Create(fileName);
            fStream.Close();
        }

        private static int StartAndWaitSubStmTasksWithLoggingTransactions()
        {
            StmModified.UseLoggingStmTransaction = true;
            string logFileName = "LoggingSubTransactions.txt";            
            LoggerModified.InitializationForStartLogging(logFileName);

            int resultValue = StartSubStmTasks();
            LoggerModified.WaitLoggingEnd();            

            return resultValue;
        }

        private static int StartSubStmTasks()
        {
            var variable1 = new StmRef<int>(0);
            var variable2 = new StmRef<int>(0);

            List<Task> taskList = new List<Task>();
            taskList.Add(
                Task.Run(() =>
                {
                    StmModified.Do<int>(new TransactionBlockModified<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        variable1.Set(2, stmTransaction);
                        int temp = variable1.Get(stmTransaction);                        
                        temp += 3;
                        variable2.Set(temp, stmTransaction);
                        List<Task> subTaskList = new List<Task>();
                        subTaskList.Add(
                            Task.Run(() =>
                            {
                                StmModified.Do<int>(new TransactionBlockModified<int>(
                                (IStmTransaction<int> subStmTransaction) =>
                                {
                                    subStmTransaction.SetParentTransaction(stmTransaction);               
                                    int subTemp = variable1.Get(subStmTransaction);
                                    variable2.Set(subTemp, subStmTransaction);
                                }
                                ));
                                //TODO subTask

                            })                            
                        );

                        subTaskList.Add(
                            Task.Run(() =>
                            {
                                StmModified.Do<int>(new TransactionBlockModified<int>(
                                (IStmTransaction<int> subStmTransaction) =>
                                {
                                    subStmTransaction.SetParentTransaction(stmTransaction);
                                    variable1.Set(3, subStmTransaction);
                                }
                                ));
                            })
                        );

                        Task.WaitAll(subTaskList.ToArray());
                        //TODO task

                    }
                    ));
                })
            );

            taskList.Add(
                Task.Run(() =>
                {
                    StmModified.Do<int>(new TransactionBlockModified<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        variable1.Set(2, stmTransaction);                        
                        int temp = variable1.Get(stmTransaction);
                        temp += 3;
                        variable2.Set(temp, stmTransaction);
                        List<Task> subTaskList = new List<Task>();
                        subTaskList.Add(
                            Task.Run(() =>
                            {
                                StmModified.Do<int>(new TransactionBlockModified<int>(
                                (IStmTransaction<int> subStmTransaction) =>
                                {
                                    subStmTransaction.SetParentTransaction(stmTransaction);
                                    int subTemp = variable1.Get(subStmTransaction);
                                    variable2.Set(subTemp, subStmTransaction);
                                }
                                ));
                                //TODO subTask

                            })
                        );

                        subTaskList.Add(
                            Task.Run(() =>
                            {
                                StmModified.Do<int>(new TransactionBlockModified<int>(
                                (IStmTransaction<int> subStmTransaction) =>
                                {
                                    subStmTransaction.SetParentTransaction(stmTransaction);
                                    variable1.Set(3, subStmTransaction);
                                }
                                ));
                            })
                        );

                        Task.WaitAll(subTaskList.ToArray());
                        //TODO task

                    }
                    ));
                })
            );

            Task.WaitAll(taskList.ToArray());

            return variable1.Value;
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
                        Thread.Sleep(500);
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
                        Thread.Sleep(1000);
                        temp += 9;
                        variable.Set(temp, stmTransaction);
                    }
                    ));
                })
            );                        

            Task.WaitAll(taskList.ToArray());            

            return variable.Value;
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
