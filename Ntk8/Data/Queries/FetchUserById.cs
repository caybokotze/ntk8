﻿using Dapper.CQRS;
using Ntk8.Helpers;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserById : Query<BaseUser>
    {
        public int Id { get; }

        public FetchUserById(int id)
        {
            Id = id;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<BaseUser>("SELECT * FROM users WHERE id = @Id", 
                new { Id = Id });
        }
        
        // todo: Eager load refresh tokens
    }
}