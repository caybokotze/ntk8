using System.Transactions;

namespace Ntk8.Tests
{
    public class Transactions
    {
        public static TransactionScope RepeatableRead()
        {
            return new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.RepeatableRead
                }, TransactionScopeAsyncFlowOption.Enabled);
        }

        public static TransactionScope UncommittedRead()
        {
            return new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadUncommitted
                }, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}