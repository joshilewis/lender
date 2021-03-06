﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Joshilewis.Cqrs.Query;

namespace Lending.ReadModels.Relational.ListLibrayLinks
{
    public class ListLibraryLinks : AuthenticatedQuery
    {
        public Guid AggregateId { get; set; }

        public ListLibraryLinks(string userId, Guid aggregateId)
            : base(userId)
        {
            AggregateId = aggregateId;
        }

        protected ListLibraryLinks()
        {
        }
    }
}
