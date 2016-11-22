using System.Collections.Generic;

namespace MPP_STM.Tests
{
    public class TransactionModified
    {
        public int ParentTransactionNumber { get; private set; }
        public bool IsOnlyReadingTransaction { get; set; }
        public bool NeedRollback { get; set; }
        public bool IsCommited { get; set; }
        public bool ParentConflict { get; set; }
        private List<int> readingVariableList = new List<int>();
        private List<int> writingVariableList = new List<int>();

        public TransactionModified(int parentTransactionNumber = 0)
        {
            IsOnlyReadingTransaction = true;
            NeedRollback = false;
            IsCommited = false;
            ParentConflict = false;
            ParentTransactionNumber = parentTransactionNumber;
        }

        public void AddReadingVariable(int number)
        {
            if(number != 0)
            {
                readingVariableList.Add(number);
            }            
        }

        public void AddWritingVariable(int number)
        {
            if(number != 0)
            {
                writingVariableList.Add(number);
            }            
        }

        public int[] GetWritingVariableArray()
        {
            return writingVariableList.ToArray();
        }

        public bool IsTransactionWorkWithVariables(int[] variables)
        {
            bool result = false;
            for(int i = 0; (i < variables.Length) && (!result); ++i)
            {
                if(readingVariableList.Contains(variables[i]))
                {
                    result = true;
                }
                if(writingVariableList.Contains(variables[i]))
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
