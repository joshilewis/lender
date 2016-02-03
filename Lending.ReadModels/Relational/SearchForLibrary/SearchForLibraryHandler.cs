using System;
using System.Linq;
using Lending.Cqrs;
using Lending.Cqrs.Query;
using Lending.Domain.OpenLibrary;
using Lending.ReadModels.Relational.LibraryOpened;
using NHibernate;

namespace Lending.ReadModels.Relational.SearchForLibrary
{
    public class SearchForLibraryHandler : MessageHandler<SearchForLibrary, Result>, 
        IQueryHandler<SearchForLibrary, Result>
    {
        private readonly Func<ISession> getSession;

        public SearchForLibraryHandler(Func<ISession> sessionFunc)
        {
            this.getSession = sessionFunc;
        }

        public override Result Handle(SearchForLibrary query)
        {
            OpenedLibrary[] libraries = getSession().QueryOver<OpenedLibrary>()
                .WhereRestrictionOn(x => x.Name).IsInsensitiveLike("%" + query.SearchString.ToLower() + "%")
                .List()
                .ToArray();

            return new Result<OpenedLibrary[]>(libraries);
        }
    }
}