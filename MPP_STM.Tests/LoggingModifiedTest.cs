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
    public enum TransactionActionModified
    {
        READ,
        WRITE,
        ROLLBACK,
        COMMIT,
        COMMIT_PARENTCONFLICT
    }

    public struct TransactionInfoModified
    {
        public int number;
        public int parentNumber;
        public int variable;
        public TransactionActionModified action;
    }

    [TestClass]
    public class LoggingModifiedTest
    {
        private StmRef<int> variable1;
        private StmRef<int> variable2;
        private string logFileName;

        [TestInitialize]
        public void Initialization()
        {
            variable1 = new StmRef<int>(0);
            variable2 = new StmRef<int>(0);
            StmModified.UseLoggingStmTransaction = true;
            logFileName = "LoggingSubTransactionTest.txt";
            File.WriteAllText(logFileName, "");
            LoggerModified.InitializationForStartLogging(logFileName);

            StartAndWaitTasks();

            LoggerModified.WaitLoggingEnd();
        }

        public void StartAndWaitTasks()
        {
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
        }

        [TestMethod]
        public void CheckRightLoggingModifiedTasks()
        {
            bool expectedResult = true;
            bool actualResult = CheckRightLogging();

            Assert.AreEqual(expectedResult, actualResult);
        }

        private bool CheckRightLogging()
        {
            bool result = true;

            Dictionary<int, TransactionModified> transactionDict = new Dictionary<int, TransactionModified>();
            TransactionInfoModified[] transactionInfoArray = GetInfo(logFileName);
            for (int i = 0; i < transactionInfoArray.Length; ++i)
            {
                TransactionInfoModified temp = transactionInfoArray[i];
                if (!transactionDict.ContainsKey(temp.number))
                {
                    transactionDict.Add(temp.number, new TransactionModified(temp.parentNumber));
                    if(transactionDict.ContainsKey(temp.parentNumber))
                    {
                        if(transactionDict[temp.parentNumber].NeedRollback)
                        {
                            transactionDict[temp.number].ParentConflict = true;
                        }
                    }
                }
                switch (temp.action)
                {
                    case TransactionActionModified.WRITE:
                        transactionDict[temp.number].AddWritingVariable(temp.variable);
                        transactionDict[temp.number].IsOnlyReadingTransaction = false;
                        break;

                    case TransactionActionModified.ROLLBACK:
                        transactionDict[temp.number].NeedRollback = false;
                        break;

                    case TransactionActionModified.READ:
                        transactionDict[temp.number].AddReadingVariable(temp.variable);
                        break;

                    case TransactionActionModified.COMMIT_PARENTCONFLICT:
                        if(transactionDict[temp.number].ParentConflict)
                        {
                            transactionDict[temp.number].IsCommited = true;
                        }
                        break;

                    case TransactionActionModified.COMMIT:
                        if (!transactionDict[temp.number].NeedRollback)
                        {
                            transactionDict[temp.number].IsCommited = true;
                            if (!transactionDict[temp.number].IsOnlyReadingTransaction)
                            {
                                int[] usedVariables = transactionDict[temp.number].GetWritingVariableArray();
                                foreach (KeyValuePair<int, TransactionModified> transactionPair in transactionDict)
                                {
                                    if ((transactionPair.Key != temp.number) && (transactionPair.Key != temp.parentNumber) && (transactionPair.Value.IsTransactionWorkWithVariables(usedVariables)))
                                    {
                                        if (!transactionPair.Value.IsCommited)
                                        {
                                            transactionPair.Value.NeedRollback = true;                                            
                                        }
                                    }
                                }

                                //set ParentConflict to SubTransactions of Transactions, that has NeedRollback
                                foreach (KeyValuePair<int, TransactionModified> transactionPair in transactionDict)
                                {
                                    int parentTransactionNumber = transactionPair.Value.ParentTransactionNumber;
                                    if (parentTransactionNumber != 0)
                                    {
                                        if(transactionDict[parentTransactionNumber].NeedRollback)
                                        {
                                            transactionPair.Value.ParentConflict = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }            

            foreach (TransactionModified transaction in transactionDict.Values)
            {
                if (!transaction.IsCommited)
                {
                    result = false;
                }
            }

            return result;
        }

        private TransactionInfoModified[] GetInfo(string fileName)
        {
            string fileContent;
            fileContent = File.ReadAllText(fileName);

            Regex reg = new Regex(@"Transaction №([\d]+)(\({1}Parent - ([\d]+)\){1}){0,1} - (\w+)(.*\[([\d]+)\])*");
            MatchCollection mCollection = reg.Matches(fileContent);

            List<TransactionInfoModified> transactionInfoList = new List<TransactionInfoModified>();

            TransactionInfoModified temp;
            foreach (Match match in mCollection)
            {
                temp.number = Convert.ToInt32(match.Groups[1].Value);
                if(match.Groups[3].Value.Length != 0)
                {
                    temp.parentNumber = Convert.ToInt32(match.Groups[3].Value);
                }
                else
                {
                    temp.parentNumber = 0;
                }
                temp.action = GetActionFromString(match.Groups[4].Value);
                if((temp.action == TransactionActionModified.READ) || (temp.action == TransactionActionModified.WRITE))
                {
                    temp.variable = Convert.ToInt32(match.Groups[6].Value);
                }
                else
                {
                    temp.variable = 0;
                }
                transactionInfoList.Add(temp);
            }

            return transactionInfoList.ToArray();
        }

        private TransactionActionModified GetActionFromString(string str)
        {
            switch (str)
            {
                case "Read":
                    return TransactionActionModified.READ;
                case "Write":
                    return TransactionActionModified.WRITE;
                case "Rollback":
                    return TransactionActionModified.ROLLBACK;
                case "Commit_ParentConflict":
                    return TransactionActionModified.COMMIT_PARENTCONFLICT;                   
                default:
                    return TransactionActionModified.COMMIT;
            }
        }
    }
}
