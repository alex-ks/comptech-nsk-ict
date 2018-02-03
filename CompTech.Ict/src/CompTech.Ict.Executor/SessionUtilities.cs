using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;


namespace CompTech.Ict.Executor
{
    public static class SessionUtilities
    {       
        public static void UpdateMnemonicValues(Dictionary<string, MnemonicsValue> values, string[] operationOut, string[] newOut)
        {
            int index = 0;
            foreach (string each in operationOut)
            {

                values.Add(each, new MnemonicsValue { Value = newOut[index], Type = null});
                index++;
            }
        }

        public static bool SessionCompleted(List<OperationStatus> operations)
        {
            return operations.All(id => id.status == StatusEnum.Completed);
        }

        public static bool SessionFaild(List<OperationStatus> operations)
        {
            return operations.Exists(id => id.status == StatusEnum.Failed);
        }

        public static void OperationRunning(OperationStatus operation)
        {
            operation.status = StatusEnum.Running;
            operation.result = null;
        }

        public static void OperationCompleted(OperationStatus operation, string[] outputs)
        {
            operation.status = StatusEnum.Completed;
            operation.result = outputs;
        }

        public static void OperationFaild(OperationStatus operation)
        {
            operation.status = StatusEnum.Failed;
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
