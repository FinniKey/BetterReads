using System.Collections;
using System.Runtime.Versioning;
using HtmlAgilityPack;

/*
How it works:
1. Goes through all (currently actually just 20) the books in a GoodReads review list.
2. Gathers the URL, the book title, the what the book was rated, and how many times the book was rated across the site.
3. Stores all that in BookReview.
4. Repeats steps 2 and 3 until all books in the list have been collected.
5. Goes through the reviews of the books found in that list.
6. Gets their name and profile URL.
7. Goes through their book review list and does steps 1-4. It adds all these books to the reviewers BookReview list
8. While it does the above step, it adjusts the similarity index of the reviewer, and puts that into the Reviewer object
9. After all the reviewers have completed the above process it returns a list of all the reviewers, their info, and their similarity index
*/

namespace BetterReads
{

    public class Book
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public int NumRatings { get; set; }

        public Book(string title, string url, int numRatings)
        {
            ArgumentException.ThrowIfNullOrEmpty(title);
            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentException.ThrowIfNullOrWhiteSpace(url);
            ArgumentOutOfRangeException.ThrowIfLessThan<int>(numRatings, 0);

            this.Title = title;
            this.Url = url;
            this.NumRatings = numRatings;

        }

        public override string ToString()
        {
            return "" + this.Title + " | " + this.NumRatings + " total ratings | " + this.Url;
        }
    }

    public class BookReview
    {
        public Book ReviewedBook { get; set; }

        /*
        Goodreads has it rating stored in td.rating div.value span.staticstars.
        That span has a title which denotes how many stars the user has rated the book.
        If the book is unrated, it does not have a title
        1/5 = "did not like it"
        2/5 = "it was ok"
        3/5 = "liked it"
        4/5 = "really liked it"
        5/5 = "it was amazing"
        */
        public int Rating { get; set; }

        public BookReview(Book book, int rating)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan<int>(rating, 1); // 1 is minimum rating
            ArgumentOutOfRangeException.ThrowIfGreaterThan<int>(rating, 5); // 5 is maximum rating

            this.ReviewedBook = book;
            this.Rating = rating;
        }

        public static int ConvertRating(string ratingString)
        {
            return ratingString switch
            {
                "did not like it" => 1,
                "it was ok" => 2,
                "liked it" => 3,
                "really liked it" => 4,
                "it was amazing" => 5,
                _ => 0,
            };
        }

        public override string ToString()
        {
            return this.Rating + "/5: " + this.ReviewedBook.ToString();
        }
    }

    public class Reviewer
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int UserID { get; set; }
        public List<BookReview> Reviews { get; }
        public float Similarity { get; set; }

        public Reviewer(string name, string url, int userid)
        {
            this.Name = name;
            this.Url = url;
            this.UserID = userid;
            Reviews = new List<BookReview>();
        }

        public bool addReview(BookReview br)
        {
            this.Reviews.Add(br);
            return true;
        }

        /**
        Calculates the similarity of the reviewer to the user.
        Doesn't do that right now. Just returns the default float value (0) unless Similarity is set to something else.
        */
        public float calculateSimilarity()
        {
            return Similarity;
        }
    }

    public static class Scraper
    {
        static void Main(string[] args)
        {
            //string mainProfileUrl = args[0];
            string mainProfileUrl = "https://www.goodreads.com/author/show/20013214.Jack_Edwards"; // example for ease of use
            int mainID = Scraper.getIDFromProfileURL(mainProfileUrl);
            string mainUsername = Scraper.getUsernameFromProfileID(mainProfileUrl);
            Reviewer mainReviewer = new Reviewer(mainUsername, mainProfileUrl, mainID); // user everyone is compared to
            Scraper.addReviewersReadBooks(mainReviewer);

            Console.WriteLine(mainReviewer.Name + " reviews:");
            foreach (BookReview bookReview in mainReviewer.Reviews)
            {
                Console.WriteLine(bookReview.ToString());
            }
        }

        private static void addReviewersReadBooks(Reviewer reviewer)
        {
            string readBooksURL = "https://www.goodreads.com/review/list/" + reviewer.UserID + "?shelf=read";
            var web = new HtmlWeb();
            var document = web.Load(readBooksURL); // loads the webpage

            var bookReviewHTMLElements = document.DocumentNode.QuerySelectorAll("tr.review");

            foreach (var bookReviewHTMLElement in bookReviewHTMLElements)
            {
                if (bookReviewHTMLElement.QuerySelector("td.rating div span").ChildAttributes("title").Any())
                {
                    string bookTitleUnformatted = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").InnerText);
                    string bookTitle = System.Text.RegularExpressions.Regex.Replace(bookTitleUnformatted, @"\s\s+|\n", "");
                    var bookURL = "https://www.goodreads.com" + HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").Attributes["href"].Value);
                    var bookNumRatingsStr = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.num_ratings div.value").InnerText);
                    var bookNumRatingsStrFormatted = System.Text.RegularExpressions.Regex.Replace(bookNumRatingsStr, @"\s+|\n|,", "");
                    int bookNumRatings = int.Parse(bookNumRatingsStrFormatted);

                    var ratingString = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.rating div span").Attributes["title"].Value);
                    int rating = BookReview.ConvertRating(ratingString);

                    Book book = new Book(bookTitle, bookURL, bookNumRatings);
                    BookReview bookReview = new BookReview(book, rating);
                    reviewer.addReview(bookReview);
                }
            }
        }

        private static int getIDFromProfileURL(string profileURL)
        {
            var web = new HtmlWeb();
            var document = web.Load(profileURL);
            string profileIDString = document.DocumentNode
                .SelectSingleNode("//a[@class='js-ratingDistTooltip']")
                .Attributes["data-user-id"].Value;
            return int.Parse(profileIDString);
        }

        private static string getUsernameFromProfileID(string profileURL)
        {
            var web = new HtmlWeb();
            var document = web.Load(profileURL);
            return document.DocumentNode
                .SelectSingleNode("//span[@itemprop='name']")
                .InnerText;
        }
    }
}

