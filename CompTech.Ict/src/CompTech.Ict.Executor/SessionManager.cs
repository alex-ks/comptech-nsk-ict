using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace CompTech.Ict.Sample
{
    public enum SessionStatusEnum
    {
        Awaits,
        Running,
        Completed,
        Aborted,
        Failed
    }
    public class Session
    {
        public Dictionary<Guid, Operation> FinishedCommands() //available operation
        {
            return new Dictionary<Guid, Operation>();
        }
        public Dictionary<string, string> mnemonicsValue { get; set; }//<Name, Value> of variable
    };
    public class Operation
    {
        Guid id;
        public string name { get; set; }
        public string[] inputs { get; set; }
        public string[] outputs { get; set; }
    }
    
    public class SessionManager
    {
        private int countSession = 0;
        private Dictionary<Guid, Session> sessionDictionary;
        private Dictionary<Guid, SessionStatusEnum> sessionStatus;
        private List<KeyValuePair<string, string[]>> availableOperations;

        private object lockListSession = new object();



        public Session WorkingSession()
        {
            Guid id = sessionStatus.First(x => (x.Value == SessionStatusEnum.Awaits || x.Value == SessionStatusEnum.Running)).Key;
            Session session = sessionDictionary.First(x => x.Key == id).Value;
            
            return session;
        }

        public SessionManager()
        {
            sessionDictionary = new Dictionary<Guid, Session>();
            sessionStatus = new Dictionary<Guid, SessionStatusEnum>();
        }

        public Guid StartSession(/*ComputationGraph*/)
        {
            Guid idSession = Guid.NewGuid();
            lock (lockListSession)
            {
                sessionDictionary.TryAdd(idSession, new Session(/*graph*/));
                sessionStatus.Add(idSession, SessionStatusEnum.Awaits);
            }
            Session session = sessionDictionary.Last().Value;

            Dictionary<Guid, Operation> tmp = session.FinishedCommands();//get available operation from session
            foreach (var each in tmp)
            {
                string nameOperation = each.Value.name;
                List<string> inputsValue = new List<string>();
                foreach (string inputs in each.Value.inputs)
                {
                    string value;
                    if (session.mnemonicsValue.TryGetValue(inputs, out value))
                    {
                        inputsValue.Add(value);
                    }
                }
                string[] inputsOperation = inputsValue.ToArray();
                availableOperations.Add(new KeyValuePair<string, string[]>(nameOperation, inputsOperation));
            }//formed a list of available operation to execute


            //get available oper
            //execute oper with callback
            return idSession;
        }

        public void SendOperation() { }

        public void StopSession(Guid id)
        {
            lock (lockListSession)
            {
                Session session;
                this.sessionDictionary.Remove(id, out session);
                if (sessionStatus.First(x => x.Key == id).Value != SessionStatusEnum.Completed)
                {
                    sessionStatus[id] = SessionStatusEnum.Aborted;
                }
            }
        }
        
        
    }
    
}
