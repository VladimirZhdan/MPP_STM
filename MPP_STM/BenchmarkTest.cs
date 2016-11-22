using System;
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

        [Benchmark(Description = "StmTasksWithManyVariables")]            
        public void StmTasksWithManyVariables()
        {
            int countTask = 5;
            int countVariables = 10;
            StmRef<int>[] variable = new StmRef<int>[countVariables];

            for(int i = 0; i < variable.Length; ++i)
            {
                variable[i] = new StmRef<int>(i);
            }
            int[] randomNumbers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                      

            List<Task> taskList = new List<Task>();

            for (int i = 0; i < countTask; ++i)
            {
                StmRef<int> tempRef = variable[randomNumbers[i]];
                taskList.Add(
                    Task.Run(() =>
                    {
                        Stm.Do<int>(new TransactionBlock<int>(
                            (IStmTransaction<int> stmTransaction) =>
                            {
                                int temp = tempRef.Get(stmTransaction);
                                temp += (10 * i);
                                tempRef.Set(temp, stmTransaction);
                            }
                        ));
                    })
                );
            }

            Task.WaitAll(taskList.ToArray());       
        }

        [Benchmark(Description = "LockTasksWithManyVariables")]
        public void LockTasksWithManyVariables()
        {
            int countTask = 5;
            int countVariables = 10;
            LockRef<int>[] variable = new LockRef<int>[countVariables];

            for (int i = 0; i < variable.Length; ++i)
            {
                variable[i] = new LockRef<int>(i);
            }
            int[] randomNumbers = GenerateRandomNumbers(countTask, countVariables);

            List<Task> taskList = new List<Task>();

            for (int i = 0; i < countTask; ++i)
            {
                LockRef<int> tempRef = variable[randomNumbers[i]];
                taskList.Add(
                    Task.Run(() =>
                    {                        
                        LockManager.Do(() =>
                        {
                            int temp = tempRef.Get();
                            temp += (10 * i);
                            tempRef.Set(temp);
                        });                                               
                    })
                );
            }

            Task.WaitAll(taskList.ToArray());
        }

        private int[] GenerateRandomNumbers(int count, int maxValue)
        {
            Random rnd = new Random();

            int[] result = new int[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = rnd.Next(0, maxValue - 1);
            }

            return result;
        }
       
        //[Benchmark(Description = "SimpleStmTask")]
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

            return variable.Value;
        }

        //[Benchmark(Description = "SimpleLockTasks")]
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

        //[Benchmark(Description = "ReadingStmTasks")]
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

            return variable.Value;
        }

        //[Benchmark(Description = "ReadingTasksWithLock")]
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

