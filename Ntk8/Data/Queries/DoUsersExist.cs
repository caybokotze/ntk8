﻿using System.Collections.Generic;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class DoUsersExist : Query<bool>
    {
        public override void Execute()
        {
            var count = QueryFirst<int>("SELECT count(*) as count FROM users;");
            if (count > 0)
            {
                Result = true;
                return;
            }

            Result = false;
        }
    }
}