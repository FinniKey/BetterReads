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

        public Reviewer RVer { get; set; }

        public BookReview(Book book, int rating, Reviewer rver)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan<int>(rating, 1); // 1 is minimum rating
            ArgumentOutOfRangeException.ThrowIfGreaterThan<int>(rating, 5); // 5 is maximum rating

            this.ReviewedBook = book;
            this.Rating = rating;
            this.RVer = rver;
        }
    }

    public class Reviewer
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string UserID { get; set; }
        public List<BookReview> Reviews { get; }
        public float Similarity { get; set; }

        public Reviewer(string name, string url, String userid)
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

    class Scraper
    {
        private List<BookReview> getBookReviewInfoList(string reviewURL)
        {

            var web = new HtmlWeb();
            var document = web.Load(reviewURL); // loads the webpage

            var bookReviews = new List<BookReview>(); // list of all the book reviews from above

            var bookReviewHTMLElements = document.DocumentNode.QuerySelectorAll("tr.review");

            foreach (var bookReviewHTMLElement in bookReviewHTMLElements)
            {
                // if the book actually has a rating then we add it. if it does not, we do not.
                // this is so books neither party has rated arent listed as "similar" or "dissimilar" later on.
                // they simply should not be added.
                if (bookReviewHTMLElement.QuerySelector("td.rating div span").ChildAttributes("title").Any())
                {
                    var title = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").InnerText);
                    var titleFormatted = System.Text.RegularExpressions.Regex.Replace(title, @"\s\s+|\n", "");
                    var url = "https://www.goodreads.com" + HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").Attributes["href"].Value);
                    var numRatingsStr = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.num_ratings div.value").InnerText);
                    var numRatingsStrFormatted = System.Text.RegularExpressions.Regex.Replace(numRatingsStr, @"\s+|\n|,", "");
                    int numRatings = int.Parse(numRatingsStrFormatted);

                    var ratingTitle = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.rating div span").Attributes["title"].Value);

                    // for reasoning behind the following, see Rating entry of BookReview class
                    int rating = 0;
                    switch (ratingTitle)
                    {
                        case "did not like it":
                            rating = 1;
                            break;

                        case "it was ok":
                            rating = 2;
                            break;

                        case "liked it":
                            rating = 3;
                            break;

                        case "really liked it":
                            rating = 4;
                            break;

                        case "it was amazing":
                            rating = 5;
                            break;
                    }
                    bookReviews.Add(new BookReview() { Url = url, Title = titleFormatted, Rating = rating, NumRatings = numRatings });
                }
            }

            return bookReviews;
        }

        private List<Reviewer> getReviews(string bookURL)
        {
            var web = new HtmlWeb();
            var document = web.Load(bookURL); // loads the webpage

            var reviewers = new List<Reviewer>(); // list of all the reviewers

            var reviewerHTMLElements = document.DocumentNode.QuerySelectorAll("div.ReviewerProfile");

            Console.WriteLine(reviewerHTMLElements.Count);

            foreach (var reviewerHTMLElement in reviewerHTMLElements)
            {
                string name = HtmlEntity.DeEntitize(reviewerHTMLElement.QuerySelector("section.ReviewerProfile__info span.Text__title4 div a").InnerText);
                string url = HtmlEntity.DeEntitize(reviewerHTMLElement.QuerySelector("section.ReviewerProfile__info span.Text__title4 div a").Attributes["href"].Value);
                var userID = System.Text.RegularExpressions.Regex.Replace(url, @"https://www\.goodreads\.com/user/show/|-.+", "");
                Console.WriteLine("UserID: " + userID);
                reviewers.Add(new Reviewer() { Url = url, Name = name, UserID = userID });
            }
            return reviewers;
        }

        static void Main(string[] args)
        {
            var scraper = new Scraper();

            //List<BookReview> bookReviews = scraper.getBookReviewInfoList("https://www.goodreads.com/review/list/91520258-jack-edwards?shelf=read");

            // foreach (var BookReview in bookReviews)
            // {
            //     Console.WriteLine(BookReview.Rating + "/5 - " + BookReview.Title + ". Ratings: " + BookReview.NumRatings + "\n" + BookReview.Url + "\n");
            // }

            var reviewers = scraper.getReviews("https://www.goodreads.com/book/show/13623848-the-song-of-achilles");

            foreach (Reviewer reviewer in reviewers)
            {
                Console.WriteLine(reviewer.Name + ": " + reviewer.UserID);
            }
        }
    }
}

