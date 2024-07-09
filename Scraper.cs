using System.Runtime.Versioning;
using HtmlAgilityPack;



namespace BetterReads
{
    public class BookReview
    {
        public string? Url { get; set; }
        public string? Title { get; set; }

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
        public int? Rating { get; set; }

        public int? NumRatings { get; set; }
    }

    public class Reviewer
    {
        public string? Name { get; set; }
        public string? Link { get; set; }
        public List<BookReview>? Stuff { get; set; }
        public int? Similarity { get; set; }
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
            return null;
        }

        static void Main(string[] args)
        {
            var scraper = new Scraper();

            List<BookReview> bookReviews = scraper.getBookReviewInfoList("https://www.goodreads.com/review/list/91520258-jack-edwards?shelf=read");

            foreach (var BookReview in bookReviews)
            {
                Console.WriteLine(BookReview.Rating + "/5 - " + BookReview.Title + ". Ratings: " + BookReview.NumRatings + "\n" + BookReview.Url + "\n");
            }
        }
    }
}

