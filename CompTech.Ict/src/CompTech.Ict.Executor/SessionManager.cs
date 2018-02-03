using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;
using Newtonsoft.Json;
using System.IO;


namespace CompTech.Ict.Executor
{
    public class SessionManager
    {
        private Dictionary<Guid, ComputationGraph> sessionDictionary;
        private Dictionary<Guid, SessionStatus> sessionStatus;
        private object lockListSession = new object();

        private Action<string[]> GetCallBack (Guid idSession, int idOperation)
        {
            return outputs => Notify(idSession, idOperation, outputs);
        }

        public SessionManager()
        {
            sessionDictionary = new Dictionary<Guid, ComputationGraph>();
            sessionStatus = new Dictionary<Guid, SessionStatus>();
        }

        public Guid StartSession(ComputationGraph session)
        {
            Guid idSession = Guid.NewGuid();
            lock (lockListSession)
            {
                sessionDictionary.Add(idSession, session);
            }
            List<OperationStatus> opStatus = new List<OperationStatus>();
            foreach (Operation operation in session.Operations)
            {
                opStatus.Add(new OperationStatus(operation.Id, StatusEnum.Awaits));
            }
            lock (lockListSession)
            {
                sessionStatus.Add(idSession, new SessionStatus(opStatus, session.MnemonicsValues));
            }
            List<int> idAvailableOperation = SessionUtilities.GetIDAvailableOperation(sessionStatus[idSession].operationStatus, sessionDictionary[idSession].Dependecies);
            OperationsToExecute(idSession, idAvailableOperation);
            return idSession;
        }

        public void Notify(Guid idSession, int idOperation, string[] outputs)
        {
            OperationStatus operationSt = sessionStatus[idSession].operationStatus[idOperation];
            if (outputs != null)
            {
                SessionUtilities.OperationCompleted(operationSt, outputs);
                ComputationGraph session = sessionDictionary[idSession];
                SessionUtilities.UpdateMnemonicValues(session.MnemonicsValues,
                                                        session.Operations[idOperation].Output,
                                                        outputs);
                SessionUtilities.UpdateMnemonicValues(sessionStatus[idSession].mnemonicsTable,
                                                        session.Operations[idOperation].Output,
                                                        outputs);

                if (!SessionUtilities.SessionCompleted(sessionStatus[idSession].operationStatus))
                {
                    List<int> idAvailableOperation = SessionUtilities.GetIDAvailableOperation(
                                                                    sessionStatus[idSession].operationStatus,
                                                                    sessionDictionary[idSession].Dependecies);
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
            var session = sessionDictionary[idSession];
            var operationSession = session.Operations;
            var mnemonicsTableSession = session.MnemonicsValues;

            foreach (Operation operation in GetAvailable(operationSession, idAvailableOperation))
            {
                List<string> inputsValues = GetInputsValues(operation.Input,mnemonicsTableSession);
                SessionUtilities.OperationRunning(sessionStatus[idSession].operationStatus[operation.Id]);
                Action<string[]> callback = GetCallBack(idSession, operation.Id);
                string path = new MethodManager().PathForMethod(operation.Name);
                string script = File.ReadAllText(path);
                Executor executor = new Executor(4);
                executor.Add(path, inputsValues.ToArray(), callback);
            }
        }

        public static List<string> GetInputsValues(string[] operationInputs, Dictionary<string, MnemonicsValue> mnemonicsTable)
        {
            List<string> inputsValues = new List<string>();
            foreach (string variable in operationInputs)
            {
                inputsValues.Add(mnemonicsTable[variable].Value);
            }
            return inputsValues;
        }

        public List<Operation> GetAvailable(List<Operation> allOperation, List<int> IdAvailable)
        {
            var availableOperation = IdAvailable.Select(id => allOperation.Find(x => x.Id == id));
            
            return availableOperation.ToList();
        }

        public void StopSession(Guid id)
        {
            ComputationGraph session = sessionDictionary[id];
            SessionStatus status = sessionStatus[id];

            if (SessionUtilities.SessionCompleted(status.operationStatus))
            {
                return;
                //сессия закончена - всё ОК
            }
            else
                if (SessionUtilities.SessionFaild(status.operationStatus))
            {
                foreach(OperationStatus operation in status.operationStatus.Where(op => op.status == StatusEnum.Awaits))
                {
                    operation.status = StatusEnum.Aborted;
                }                
                //одна операция сломалась значит другие отменяем
                //в целом сессия Faild
            }
            else
            {
                foreach (OperationStatus operation in status.operationStatus.Where(op => op.status == StatusEnum.Awaits))
                {
                    operation.status = StatusEnum.Aborted;
                }
                //сессия была отменена пользователем
                //в целом сессия Aborted
            }            
        }

        public SessionStatus GetStatusSession(Guid idSession)
        {
            if (sessionStatus.ContainsKey(idSession))
            {
                return sessionStatus[idSession];
            }
            else
            {
                throw new ArgumentException($"Session {idSession} not found!");
            }
        }
    }
}
