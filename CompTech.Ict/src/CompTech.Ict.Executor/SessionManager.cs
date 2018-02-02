using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;
using Newtonsoft.Json;

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
    
    public class Session
    {
        [JsonProperty("depend")]
        public int[][] Dependecies { get; set; }
        [JsonProperty("operations")]
        public List<Operation> Operations { get; set; }
        [JsonProperty("mnemonics_values")]
        public Dictionary<string, MnemonicsValue> MnemonicsValues { get; set; }
        public List<int> FinishedCommands() //available operation id session
        {
            return new List<int>();
        }
        
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
        private Dictionary<Guid, Session> sessionDictionary;
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
            sessionDictionary = new Dictionary<Guid, Session>();
            sessionStatus = new Dictionary<Guid, StatusEnum>();
            operationStatusDictionary = new Dictionary<Guid, SessionStatus>();
            executor = new Executor();
        }
        public Guid StartSession(Session session)
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
            OperationToExecute(idSession, session.FinishedCommands());
            return idSession;
        }        
        public void Notify(Guid idSession, int idOperation, string[] outputs)
        {
            OperationStatus operationSt = operationStatusDictionary[idSession].operationStatus.Find(x => x.idOperation == idOperation);
            if (outputs.Length > 0)
            {
                operationSt.status = StatusEnum.Completed;
                operationSt.result = outputs;
                Session session;
                sessionDictionary.TryGetValue(idSession, out session);
                Operation tmp = session.Operations.Find(x => x.Id == idOperation);
                int index = 0;
                foreach(string each in tmp.Output)
                {
                    session.MnemonicsValues[each].Value = outputs[index];
                    index++;
                }
                operationStatusDictionary[idSession].mnemonicsTable = session.MnemonicsValues;
                int countComplitedOperation = operationStatusDictionary[idSession].operationStatus.Count(x => x.status == StatusEnum.Completed);
                if (countComplitedOperation < operationStatusDictionary[idSession].operationStatus.Count)
                {
                    sessionStatus[idSession] = StatusEnum.Running;
                    OperationToExecute(idSession, session.FinishedCommands());
                }
                else
                {
                    StopSession(idSession);
                }                
            }
            else
            {
                operationSt.status = StatusEnum.Failed;
                StopSession(idSession);
            }
            
        }
        public void OperationToExecute(Guid idSession, List<int> indexAvailableOperation)
        {
            sessionDictionary.TryGetValue(idSession, out Session session);            
            List<Operation> operationSession = session.Operations;
            Dictionary<string, MnemonicsValue> mnemonicsTableSession = session.MnemonicsValues;
            List<Operation> availableOperation = new List<Operation>();
            foreach(int index in indexAvailableOperation)
            {
                availableOperation.Add(operationSession.Find(x => x.Id == index));
            }
            foreach(Operation operation in availableOperation)
            {
                List<string> inputsValues = new List<string>();
                foreach(string variable in operation.Input)
                {
                    MnemonicsValue tmp;
                    mnemonicsTableSession.TryGetValue(variable, out tmp);
                    inputsValues.Add(tmp.Value);
                }
                operationStatusDictionary[idSession].operationStatus[operation.Id].status = StatusEnum.Running;
                Action<string[]> callback = GetCallBack(idSession, operation.Id);
                executor.Add(operation.Name, inputsValues.ToArray(), callback);
            }
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
            Session session;
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
