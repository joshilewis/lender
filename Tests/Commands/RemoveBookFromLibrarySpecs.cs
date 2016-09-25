﻿using System;
using Joshilewis.Testing.Helpers;
using Lending.Domain.AddBookToLibrary;
using Lending.Domain.Model;
using Lending.Domain.OpenLibrary;
using Lending.Domain.RemoveBookFromLibrary;
using Lending.ReadModels.Relational.BookAdded;
using Lending.ReadModels.Relational.SearchForBook;
using NUnit.Framework;
using static Tests.TestData;
using static Joshilewis.Testing.Helpers.ApiExtensions;
using static Joshilewis.Testing.Helpers.EventStoreExtensions;
using static Tests.LendingPersistenceExtentions;
using static Tests.AutomationExtensions;

namespace Tests.Commands
{
    [TestFixture]
    public class RemoveBookFromLibrarySpecs : Fixture
    {
        private readonly BookSearchResult[] emptyLibraryBookCollection = { };

        [Test]
        public void RemoveBookInLibraryShouldSucceed()
        {
            var transactionId = Guid.Empty;
            var userId = Guid.NewGuid();
            Given(() => UserRegisters(userId, "user1", "email1", "user1Picture"));
            Given(() => OpenLibrary(transactionId, userId, "library1"));
            Given(() => AddBookToLibrary1(transactionId, userId, userId, "Title", "Author", "isbn", 1982));
            When(() => RemoveBookFromLibrary(transactionId, userId, userId, "Title", "Author", "isbn", 1982));
            Then1(() => BookRemovedSucccessfully());
            AndGETTo($"/libraries/{userId}/books/").Returns(new BookSearchResult[] { });
            AndEventsSavedForAggregate<Library>(userId,
                new LibraryOpened(transactionId, userId, "library1", userId),
                new BookAddedToLibrary(transactionId, userId, "Title", "Author", "isbn", 1982),
                new BookRemovedFromLibrary(transactionId, userId, "Title", "Author", "isbn", 1982)
            );
        }

        [Test]
        public void RemoveBookNotInLibraryShouldFail()
        {
            var transactionId = Guid.Empty;
            var userId = Guid.NewGuid();
            Given(() => UserRegisters(userId, "user1", "email1", "user1Picture"));
            Given(() => OpenLibrary(transactionId, userId, "library1"));
            When(() => RemoveBookFromLibrary(transactionId, userId, userId, "Title", "Author", "isbn", 1982));
            Then1(() => BookNotInLibrary());
            AndGETTo($"/libraries/{userId}/books/").Returns(new BookSearchResult[] { });
            AndEventsSavedForAggregate<Library>(userId,
                new LibraryOpened(transactionId, userId, "library1", userId)
            );
        }

        [Test]
        public void UnauthorizedRemoveBookInLibraryShouldFail()
        {
            var transactionId = Guid.Empty;
            var userId = Guid.NewGuid();
            Given(() => UserRegisters(userId, "user1", "email1", "user1Picture"));
            Given(() => OpenLibrary(transactionId, userId, "library1"));
            When(() => RemoveBookFromLibrary(transactionId, userId, Guid.Empty, "Title", "Author", "isbn", 1982));
            Then1(() => UnauthorisedCommandRejected(Guid.Empty, typeof(Library), userId));
            AndGETTo($"/libraries/{userId}/books/").Returns(new BookSearchResult[] { });
            AndEventsSavedForAggregate<Library>(userId,
                new LibraryOpened(transactionId, userId, "library1", userId)
            );
        }

    }
}
