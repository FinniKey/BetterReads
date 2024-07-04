using System.Runtime.Versioning;
using HtmlAgilityPack;

namespace SimpleWebScraper
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
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var web = new HtmlWeb();
            Console.WriteLine("loading webpage");
            var document = web.Load("https://www.goodreads.com/review/list/91520258-jack-edwards?shelf=read"); // loads the webpage
            Console.WriteLine("webpage loaded");

            var bookReviews = new List<BookReview>(); // list of all the book reviews from above

            Console.WriteLine("getting all book reviews");
            var bookReviewHTMLElements = document.DocumentNode.QuerySelectorAll("tr.review");

            Console.WriteLine(bookReviewHTMLElements.Count);

            Console.WriteLine("going through reviews");
            foreach (var bookReviewHTMLElement in bookReviewHTMLElements)
            {
                // if the book actually has a rating then we add it. if it does not, we do not.
                // this is so books neither party has rated arent listed as "similar" or "dissimilar" later on.
                // they simply should not be added.
                if (bookReviewHTMLElement.QuerySelector("td.rating div span").ChildAttributes("title").Any())
                {
                    var title = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").InnerText);
                    var titleFormatted = System.Text.RegularExpressions.Regex.Replace(title, @"\s+", "");
                    var url = "https://www.goodreads.com" + HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").Attributes["href"].Value);

                    var ratingTitle = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.rating div span").Attributes["title"].Value);

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
                    bookReviews.Add(new BookReview() { Url = url, Title = titleFormatted, Rating = rating });
                    //Console.WriteLine(title);
                }
            }

            foreach (var BookReview in bookReviews)
            {
                Console.WriteLine(BookReview.Rating + "/5 - " + BookReview.Title + "\n" + BookReview.Url + "\n");
            }
        }
    }
}

