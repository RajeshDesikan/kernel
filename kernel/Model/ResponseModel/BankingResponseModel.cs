using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace kernel.Model.ResponseModel
{
    public  class BankingResponseModel
    {
        public string AccountHolderName { get; set; }
        public decimal? Balance { get; set; }
        public List<kernel.Entity.Transaction> Transactions { get; set; } = new List<kernel.Entity.Transaction>();
        public string Message { get; set; } 
    }
}
