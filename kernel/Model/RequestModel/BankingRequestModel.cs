using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel.Model.RequestModel
{
    internal class BankingRequestModel
    {
        public string AccountId { get; set; }
        public string QueryType { get; set; } // E.g., "Balance", "Statement", etc.
        public int? StatementMonths { get; set; }
    }
}
