using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;



namespace CompTech.Ict.Executor
{
    public class SessionManager
    {
        private Dictionary<Guid, ComputationGraph> sessionDictionary;
        private Dictionary<Guid, SessionStatus> sessionStatus;
        private object lockListSession = new object();
        private readonly ILogger _logger;

        private Action<string[]> GetCallBack (Guid idSession, int idOperation)
        {
            return outputs => Notify(idSession, idOperation, outputs);
        }

        public SessionManager(ILogger<SessionManager> logger) 
        {
            sessionDictionary = new Dictionary<Guid, ComputationGraph>();
            sessionStatus = new Dictionary<Guid, SessionStatus>();
            _logger = logger;
            _logger.LogInformation("Session manager a created!");
        }

        public Guid StartSession(ComputationGraph session)
        {
            Guid idSession = Guid.NewGuid();
            lock (lockListSession)
            {
                sessionDictionary.Add(idSession, session);
            }
            _logger.LogInformation($"Create session with id: {idSession}");
            List<OperationStatus> opStatus = new List<OperationStatus>();
            foreach (Operation operation in session.Operations)
            {
                opStatus.Add(new OperationStatus(operation.Id, StatusEnum.Awaits));
            }
            lock (lockListSession)
            {
                sessionStatus.Add(idSession, new SessionStatus(opStatus, session.MnemonicsValues));
            }

            GetLogSession(idSession);

            List<int> idAvailableOperation = SessionUtilities.GetIDAvailableOperation(
                                                        sessionStatus[idSession].operationStatus,
                                                        sessionDictionary[idSession].Dependecies);
            _logger.LogInformation($"ID available operation for session - {idSession}" +
                                    $" - is is: {string.Join(',',idAvailableOperation.ToArray())}");
            OperationsToExecute(idSession, idAvailableOperation);
            
            return idSession;
        }

        private void GetLogSession(Guid idSession)
        {
            string str = $"\r\nSession {idSession}:\r\n";
            foreach(OperationStatus each in sessionStatus[idSession].operationStatus)
            {
                str += $"Operation : {each.idOperation}, status: {each.status}\r\n";
            }
            _logger.LogInformation(str);
        }

        private void GetLogVariable(Guid idSession)
        {
            var table = sessionStatus[idSession].mnemonicsTable;
            string tmp = $"\r\nSession {idSession}:\r\n";
            foreach(var each in table)
            {
                tmp += $"var: {each.Key}, val: {each.Value.Value}\r\n";
            }
            _logger.LogInformation(tmp);
        }

        public void Notify(Guid idSession, int idOperation, string[] outputs)
        {
            OperationStatus operationSt = sessionStatus[idSession].operationStatus[idOperation];
            if (outputs != null)
            {
                SessionUtilities.OperationCompleted(operationSt, outputs);
                ComputationGraph session = sessionDictionary[idSession];                
                SessionUtilities.UpdateMnemonicValues(sessionStatus[idSession].mnemonicsTable,
                                                        session.Operations[idOperation].Output,
                                                        outputs);
                GetLogSession(idSession);

                if (!SessionUtilities.SessionCompleted(sessionStatus[idSession].operationStatus))
                {
                    List<int> idAvailableOperation = SessionUtilities.GetIDAvailableOperation(
                                                                    sessionStatus[idSession].operationStatus,
                                                                    sessionDictionary[idSession].Dependecies);
                    _logger.LogInformation($"ID available operation for session - {idSession} " +
                                            $"- is: {string.Join(',', idAvailableOperation.ToArray())}");
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
                executor.Add(script, inputsValues.ToArray(), callback);
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
