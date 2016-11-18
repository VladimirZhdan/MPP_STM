using Microsoft.VisualStudio.TestTools.UnitTesting;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MPP_STM
{
    [TestClass]
    public class BenchmarkTest
    {
        [TestMethod]
        public void RunTestMainClass()
        {
            BenchmarkRunner.Run<Program>();
        }

        [TestMethod]
        public void RunSimpleTest()
        {
            BenchmarkRunner.Run<BenchmarkTest>();
        }

        [Benchmark(Description = "SimpleStmTask")]
        public int StartStmTasks()
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

            Task.WaitAll(taskList.ToArray());

            return variable.value;
        }

        [Benchmark(Description = "SimpleLockTasks")]
        public int StartTasksWithLock()
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

            Task.WaitAll(taskList.ToArray());

            return variable.Get();
        }

        [Benchmark(Description = "ReadingStmTasks")]
        public int StartReadingStmTasks()
        {
            var variable = new StmRef<int>(0);

            List<Task> taskList = new List<Task>();

            taskList.Add(
                Task.Run(() =>
                {
                    Stm.Do<int>(new TransactionBlock<int>(
                    (IStmTransaction<int> stmTransaction) =>
                    {
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get(stmTransaction);
                        }                            
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
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get(stmTransaction);
                        }
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
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get(stmTransaction);
                        }
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
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get(stmTransaction);
                        }
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
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get(stmTransaction);
                        }
                    }
                    ));
                })
            );

            Task.WaitAll(taskList.ToArray());

            return variable.value;
        }

        [Benchmark(Description = "ReadingTasksWithLock")]
        public int StartReadingTasksWithLock()
        {
            LockRef<int> variable = new LockRef<int>(0);
            List<Task> taskList = new List<Task>();

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        int[] temp = new int[10];
                        for(int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get();
                        }                                                   
                    });
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get();
                        }                            
                    });
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get();
                        }                            
                    });
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get();
                        }                            
                    });
                }
                )
            );

            taskList.Add(
                Task.Run(() =>
                {
                    LockManager.Do(() =>
                    {
                        int[] temp = new int[10];
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = variable.Get();
                        }                            
                    });
                }
                )
            );

            Task.WaitAll(taskList.ToArray());
            return variable.Get();
        }
    }
}

