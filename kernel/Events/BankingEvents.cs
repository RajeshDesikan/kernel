using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel.Events
{
    public class BankingEvents
    {
        public const string BalanceInquiryStarted = nameof(BalanceInquiryStarted);
        public const string BalanceInquiryCompleted = nameof(BalanceInquiryCompleted);
        public const string StatementRetrievalStarted = nameof(StatementRetrievalStarted);
        public const string StatementRetrievalCompleted = nameof(StatementRetrievalCompleted);

        public const string FanOutStarted = nameof(FanOutStarted);
        public const string FanOutCompleted = nameof(FanOutCompleted);
        public const string FanInStarted = nameof(FanInStarted);
        public const string FanInCompleted = nameof(FanInCompleted);

        public const string MapStarted = nameof(MapStarted);
        public const string MapCompleted = nameof(MapCompleted);
        public const string ReduceStarted = nameof(ReduceStarted);
        public const string ReduceCompleted = nameof(ReduceCompleted);
    }
}
