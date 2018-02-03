using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompTech.Ict.Executor.Models
{
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
}
