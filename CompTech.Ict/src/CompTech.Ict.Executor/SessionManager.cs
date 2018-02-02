using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;
using Newtonsoft.Json;
using System.IO;


namespace CompTech.Ict.Executor
{
    public enum StatusEnum
    {
        Awaits,
        Running,
        Completed,
        Aborted,
        Failed
    }    
    
    public class OperationStatus
    {
        public int idOperation { get; set; }
        public StatusEnum status { get; set; }
        public string[] result { get; set; }
        public OperationStatus(int id, StatusEnum st, string[] res = null)
        {
            idOperation = id;
            status = st;
            result = res;
        }
    }

    public class SessionStatus
    {
        public List<OperationStatus> operationStatus { get; set; }
        public Dictionary<string, MnemonicsValue> mnemonicsTable { get; set; }
        public SessionStatus(List<OperationStatus> opStatus, Dictionary<string, MnemonicsValue> value)
        {
            operationStatus = opStatus;
            mnemonicsTable = value;
        }
    }

    public class SessionManager
    {
        private Dictionary<Guid, ComputationGraph> sessionDictionary;
        private Dictionary<Guid, StatusEnum> sessionStatus;
        private Dictionary<Guid, SessionStatus> operationStatusDictionary;
        private object lockListSession = new object();
        private Executor executor;

        private Action<string[]> GetCallBack (Guid idSession, int idOperation)
        {
            return (string[] outputs) => Notify(idSession, idOperation, outputs);
        }
        public SessionManager()
        {
            sessionDictionary = new Dictionary<Guid, ComputationGraph>();
            sessionStatus = new Dictionary<Guid, StatusEnum>();
            operationStatusDictionary = new Dictionary<Guid, SessionStatus>();
            executor = new Executor();
        }
        public Guid StartSession(ComputationGraph session)
        {
            Guid idSession = Guid.NewGuid();
            lock (lockListSession)
            {
                sessionDictionary.Add(idSession, session);
            }
            lock(lockListSession)
            {
                sessionStatus.Add(idSession, StatusEnum.Awaits);
            }
            List<OperationStatus> opStatus = new List<OperationStatus>();
            foreach(Operation operation in session.Operations)
            {
                opStatus.Add(new OperationStatus(operation.Id, StatusEnum.Awaits));
            }
            lock (lockListSession)
            {
                operationStatusDictionary.Add(idSession, new SessionStatus(opStatus, session.MnemonicsValues));
            }
            List<int> idAvailableOperation = SessionUtilities.GetIDAvailableOperation(operationStatusDictionary[idSession].operationStatus, sessionDictionary[idSession].Dependecies);
            OperationsToExecute(idSession, idAvailableOperation);
            return idSession;
        }

        public void Notify(Guid idSession, int idOperation, string[] outputs)
        {
            OperationStatus operationSt = operationStatusDictionary[idSession].operationStatus[idOperation];
            if (outputs.Length > 0)
            {
                SessionUtilities.OperationCompleted(operationSt, outputs);

                ComputationGraph session = sessionDictionary[idSession];
                string[] operationOutputs = session.Operations[idOperation].Output;

                int index = 0;
                foreach(string each in operationOutputs)
                {
                    session.MnemonicsValues[each].Value = outputs[index];
                    index++;
                }
                operationStatusDictionary[idSession].mnemonicsTable = session.MnemonicsValues;
                int countComplitedOperation = operationStatusDictionary[idSession].operationStatus.Count(x => x.status == StatusEnum.Completed);
                if (countComplitedOperation < operationStatusDictionary[idSession].operationStatus.Count)
                {
                    sessionStatus[idSession] = StatusEnum.Running;
                    List<int> idAvailableOperation = SessionUtilities.GetIDAvailableOperation(operationStatusDictionary[idSession].operationStatus, sessionDictionary[idSession].Dependecies);
                    OperationsToExecute(idSession, idAvailableOperation);
                }
                else
                {
                    StopSession(idSession);
                }
            }
            else
            {
                SessionUtilities.OperationFaild(operationSt);
                StopSession(idSession);
            }
        }

        public void OperationsToExecute(Guid idSession, List<int> idAvailableOperation)
        {
            ComputationGraph session = sessionDictionary[idSession];
            List<Operation> operationSession = session.Operations;
            Dictionary<string, MnemonicsValue> mnemonicsTableSession = session.MnemonicsValues;

            foreach (Operation operation in GetAvailable(operationSession, idAvailableOperation))
            {
                List<string> inputsValues = SessionUtilities.GetInputsValues(operation.Input,mnemonicsTableSession);                
                operationStatusDictionary[idSession].operationStatus[operation.Id].status = StatusEnum.Running;
                Action<string[]> callback = GetCallBack(idSession, operation.Id);
                string path = new MethodManager().PathForMethod(operation.Name);
                string script = File.ReadAllText(path);
                executor.Add(path, inputsValues.ToArray(), callback);
            }
        }

        public List<Operation> GetAvailable(List<Operation> allOperation, List<int> IdAvailable)
        {
            List<Operation> availableOperation = new List<Operation>();
            foreach (int index in IdAvailable)
            {
                availableOperation.Add(allOperation.Find(x => x.Id == index));
            }
            return availableOperation;
        }

        public void StopSession(Guid id)
        {
            if(operationStatusDictionary[id].operationStatus.Count(x => x.status == StatusEnum.Failed) > 0)
            {
                sessionStatus[id] = StatusEnum.Failed;
            }
            else if(operationStatusDictionary[id].operationStatus.Count(x => x.status == StatusEnum.Completed) == 
                        operationStatusDictionary[id].operationStatus.Count)
            {
                sessionStatus[id] = StatusEnum.Completed;
            }
            else
            {
                sessionStatus[id] = StatusEnum.Aborted;
            }
            ComputationGraph session;
            lock (lockListSession)
            {
                sessionDictionary.Remove(id, out session);
            }                                    
        }

        public SessionStatus GetStatusSession(Guid idSession)
        {
            if (operationStatusDictionary.ContainsKey(idSession))
            {
                return operationStatusDictionary[idSession];
            }
            else
            {
                throw new ArgumentException($"Session {idSession} not found!");
            }
        }
    }
}
