using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;


namespace CompTech.Ict.Executor
{
    public static class SessionUtilities
    {
        
        public static List<string> GetInputsValues (string[] operationInputs, Dictionary<string, MnemonicsValue> mnemonicsTable)
        {
            List<string> inputsValues = new List<string>();
            foreach(string variable in operationInputs)
            {
                inputsValues.Add(mnemonicsTable[variable].Value);
            }
            return inputsValues;
        }

        public static void OperationCompleted(OperationStatus operation, string[] outputs)
        {
            operation.status = StatusEnum.Completed;
            operation.result = outputs;
        }

        public static void OperationFaild(OperationStatus operation)
        {
            operation.status = StatusEnum.Completed;
            operation.result = null;
        }

        public static List<int> GetIDAvailableOperation(List<OperationStatus> operation, int[][] dependencies)
        {
            List<int> availableOperation = new List<int>();
            int countOperation = dependencies.Length;
            for(int i = 0; i < countOperation; i++)
            {
                if(operation[i].status != StatusEnum.Completed)
                {
                    if (dependencies[i].All(id => operation[id].status == StatusEnum.Completed))
                    {
                        availableOperation.Add(i);
                    }                    
                }
            }
            return availableOperation;
        }
    }
}
