using System.Collections.Generic;
using Dapper.CQRS;

namespace Ntk8.Tests.TestHelpers
{
    public class GenericQuery<T> : Query<List<T>>
    {
        private readonly string _sql;

        public GenericQuery(string sql)
        {
            _sql = sql;
        }
        
        public override void Execute()
        {
            Result = QueryList<T>(_sql);
        }
    }
}