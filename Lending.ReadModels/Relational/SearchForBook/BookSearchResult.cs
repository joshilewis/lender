using System;

namespace Lending.ReadModels.Relational.SearchForBook
{
    public class BookSearchResult
    {
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }
        public string LibraryPicture { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public int PublishYear { get; set; }
        public string CoverPicture { get; set; }

        public BookSearchResult(Guid libraryId, string libraryName, string libraryPicture, string title, string author,
            string isbn, int publishYear,string coverPicture)
        {
            LibraryId = libraryId;
            LibraryName = libraryName;
            LibraryPicture = libraryPicture;
            Title = title;
            Author = author;
            Isbn = isbn;
            PublishYear = publishYear;
            CoverPicture = coverPicture;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (BookSearchResult)obj;
            return LibraryId.Equals(other.LibraryId) &&
                   LibraryName.Equals(other.LibraryName) &&
                   LibraryPicture.Equals(other.LibraryPicture) &&
                   Title.Equals(other.Title) &&
                   Author.Equals(other.Author) &&
                   PublishYear.Equals(other.PublishYear) &&
                   Isbn.Equals(other.Isbn) &&
                   CoverPicture.Equals(other.CoverPicture);
        }
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result = (result * 397) ^ LibraryId.GetHashCode();
            result = (result * 397) ^ LibraryName.GetHashCode();
            result = (result * 397) ^ LibraryPicture.GetHashCode();
            result = (result * 397) ^ Title.GetHashCode();
            result = (result * 397) ^ Author.GetHashCode();
            result = (result * 397) ^ Isbn.GetHashCode();
            result = (result * 397) ^ PublishYear.GetHashCode();
            return result;
        }
    }
}