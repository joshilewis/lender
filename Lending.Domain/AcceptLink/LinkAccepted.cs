﻿using System;
using Joshilewis.Cqrs;

namespace Lending.Domain.AcceptLink
{
    public class LinkAccepted : Event
    {
        public Guid RequestingLibraryId { get; set; }

        public LinkAccepted(Guid processId, Guid aggregateId, Guid requestingLibraryId)
            : base(processId, aggregateId)
        {
            RequestingLibraryId = requestingLibraryId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (!base.Equals(obj)) return false;
            var other = (LinkAccepted)obj;
            return RequestingLibraryId.Equals(other.RequestingLibraryId);
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() * 397) ^ RequestingLibraryId.GetHashCode();
        }
    }
}