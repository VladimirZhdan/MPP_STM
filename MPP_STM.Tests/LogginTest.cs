using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MPP_STM;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace MPP_STM.Tests
{
    public enum TransactionAction
    {
        READ,
        WRITE,
        ROLLBACK,
        COMMIT 
    }

    public struct TransactionInfo
    {
        public int number;
        public TransactionAction action;        
    }

    [TestClass]
    public class LogginTest
    {
        private StmRef<int> variable;
        string logFileName;

        [TestInitialize]
        public void Initialization()
        {
            variable = new StmRef<int>(0);
            Stm.UseLoggingStmTransaction = true;
            logFileName = "Log.txt";
            ReCreateFile(logFileName);
            Logger.LogFileName = logFileName;
            Logger.IsNotEndOutputLogs = true;
        }

        [TestMethod]
        public void CheckRightLogginWritingTasks()
        {
            RunAndWaitTasks();
            
            Logger.IsNotEndOutputLogs = false;
            while (!Logger.IsLoggingThreadProgressed) ; 

            bool expectedResult = true;
            bool actualResult = CheckRightLogging();

            Assert.AreEqual(expectedResult, actualResult);
        }

        private void RunAndWaitTasks()
        {
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
        }

        private bool CheckRightLogging()
        {
            bool result = true;

            Dictionary<int, Transaction> transactionDict = new Dictionary<int, Transaction>();            
            TransactionInfo[] transactionInfoArray = GetInfo(logFileName);
            for(int i = 0; i < transactionInfoArray.Length; ++i)
            {
                TransactionInfo temp = transactionInfoArray[i];
                if(!transactionDict.ContainsKey(temp.number))
                {
                    transactionDict.Add(temp.number, new Transaction());
                }
                switch (temp.action)
                {
                    case TransactionAction.WRITE:
                        transactionDict[temp.number].IsOnlyReadingTransaction = false;                        
                        break;

                    case TransactionAction.ROLLBACK:
                        transactionDict[temp.number].NeedRollback = false;
                        break;

                    case TransactionAction.COMMIT:
                        if (!transactionDict[temp.number].NeedRollback)
                        {
                            transactionDict[temp.number].IsCommited = true;
                        }
                        if(!transactionDict[temp.number].IsOnlyReadingTransaction)
                        {
                            foreach (KeyValuePair<int, Transaction> transactionPair in transactionDict)
                            {
                                if (transactionPair.Key != temp.number)
                                {
                                    if (!transactionPair.Value.IsCommited)
                                    {
                                        transactionPair.Value.NeedRollback = true;
                                    }
                                }
                            }
                        }                        
                        break;
                }
            }

            foreach(Transaction transaction in transactionDict.Values)
            {
                if(!transaction.IsCommited)
                {
                    result = false;
                }
            }

            return result;
        }


        private void ReCreateFile(string fileName)
        {
            FileStream fStream = File.Create(fileName);
            fStream.Close();
        }

        private TransactionInfo[] GetInfo(string fileName)
        {
            string fileContent;
            fileContent = File.ReadAllText(fileName);

            Regex reg = new Regex(@"Transaction №([\d]+) - (\w+)");
            MatchCollection mCollection = reg.Matches(fileContent);

            List<TransactionInfo> transactionInfoList  = new List<TransactionInfo>();

            TransactionInfo temp;
            foreach (Match match in mCollection)
            {
                temp.number = Convert.ToInt32(match.Groups[1].Value);
                temp.action = GetActionFromString(match.Groups[2].Value);
                transactionInfoList.Add(temp);
            }

            return transactionInfoList.ToArray();                        
        } 

        private TransactionAction GetActionFromString(string str)
        {
            switch(str)
            {
                case "Read":
                    return TransactionAction.READ;                    
                case "Write":
                    return TransactionAction.WRITE;                    
                case "Rollback":
                    return TransactionAction.ROLLBACK;                    
                default:
                    return TransactionAction.COMMIT;                   
            }
        }

    }
}
