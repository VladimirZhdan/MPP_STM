using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace MPP_STM
{
    class Program
    {
        static void Main(string[] args)
        {
            string logFileName = "Log.txt";
            ReCreateFile(logFileName);
            Logger.LogFileName = logFileName;

            var variable = new StmRef<int>(0);            

            Task[] task = new Task[4];

            task[0] = Task.Run(() =>
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
            });

            task[1] = Task.Run(() =>
            {
                Stm.Do<int>(new TransactionBlock<int>(
                (IStmTransaction<int> stmTransaction) =>
                {
                    variable.Set(3, stmTransaction);
                }
                ));
            });


            task[2] = Task.Run(() =>
            {
                Stm.Do<int>(new TransactionBlock<int>(
                (IStmTransaction<int> stmTransaction) =>
                {
                    variable.Set(4, stmTransaction);
                }
                ));
            });

            task[3] = Task.Run(() =>
            {
                Stm.Do<int>(new TransactionBlock<int>(
                (IStmTransaction<int> stmTransaction) =>
                {
                    int temp = variable.Get(stmTransaction);
                    temp += 9;
                    variable.Set(temp, stmTransaction);
                }
                ));
            });

            Task.WaitAll(task);

            Task outputTask = Task.Run(() =>
            {
                Stm.Do<int>(new TransactionBlock<int>(
                (IStmTransaction<int> stmTransaction) =>
                {
                    int temp = variable.Get(stmTransaction);
                    Console.WriteLine(temp);
                }
                ));
            });

            outputTask.Wait();            
            Console.ReadLine();            
        }


        private static void ReCreateFile(string fileName)
        {
            FileStream fStream = File.Create(fileName);
            fStream.Close();
        }

    }
}
