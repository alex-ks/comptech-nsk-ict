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
        public List<int> FinishedCommands() //available operation id session and 
        {
            return new List<int>();
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
    public class Executer
    {

    }
    public class SessionManager
    {
        private int countSession = 0;
        private Dictionary<Guid, Session> sessionDictionary;
        private Dictionary<Guid, SessionStatusEnum> sessionStatus;
        //private List<KeyValuePair<string, string[]>> availableOperations;

        private object lockListSession = new object();        


        public SessionManager()
        {
            sessionDictionary = new Dictionary<Guid, Session>();
            sessionStatus = new Dictionary<Guid, SessionStatusEnum>();
            //availableOperations = new List<KeyValuePair<string, string[]>>();
        }
        /*
         получили от Session граф с зависимостями, таблицей значений.
         дали ей некоторый id, добавили в список сессий и в таблицу со статусом
         получили доступные операции
         отправили на выполнение
         вернули пользователю id сессии
         */
        public Guid StartSession(/*ComputationGraph*/)
        {
            Guid idSession = Guid.NewGuid();
            Session session;
            lock (lockListSession)
            {
                session = new Session(/*ComputatingGraph*/);
                sessionDictionary.TryAdd(idSession, session);
                sessionStatus.Add(idSession, SessionStatusEnum.Awaits);
            }
            SendOperation(idSession, session.FinishedCommands());//get available operation from session

            return idSession;
        }

        public void SendOperation(Guid idSession, List<int> availableSessionOperation)
        {
            //execute oper with callback

            //найти сессию по ее id
            sessionDictionary.TryGetValue(idSession, out Session session);
            /*
             вытащить отдельно Operation и MnemonicsTable из данной сессии.

             Operation operationSession = session.ComputationGraph.operatio;
             MnemonicsTable mnemonicsTableSession = session.ComputationGraph.MnemonicsTable;

             найти в операциях сессии каждую доступную операцию
             List<Operation> availableOperationList = new List<Operation>();
             foreach(int each in availableSessionOperation)
             {
                 availableOperationList.Add(x => x.id==each);
             }
             в каждой операции есть массив строк = входам*/
            //List<KeyValuePair<string, string[]>> operationToExecute = new List<KeyValuePair<string, string[]>>();
            /*foreach(var each in availableOperationList)
            {
               List<string> valueInputs = new List<string>();
               найти в таблице MnemonicValues соответствие переменная - ее значение

            }
             */
            //formed a list of available operation to execute
        }
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
