using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompTech.Ict.Executor.Models
{
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
}
