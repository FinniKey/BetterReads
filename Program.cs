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
            var document = web.Load("https://www.goodreads.com/review/list/91520258-jack-edwards?shelf=read"); // loads the webpage

            var bookReviews = new List<BookReview>(); // list of all the book reviews from above

            var bookReviewHTMLElements = document.DocumentNode.QuerySelectorAll("tr.review");

            Console.WriteLine(bookReviewHTMLElements.Count);

            foreach (var bookReviewHTMLElement in bookReviewHTMLElements)
            {
                // if the book actually has a rating then we add it. if it does not, we do not.
                // this is so books neither party has rated arent listed as "similar" or "dissimilar" later on.
                // they simply should not be added.
                if (bookReviewHTMLElement.QuerySelector("td.rating div span").ChildAttributes("title").Any())
                {
                    var title = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").InnerText);
                    var url = "https://www.goodreads.com" + HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.title div a").Attributes["href"].Value);

                    var ratingTitle = HtmlEntity.DeEntitize(bookReviewHTMLElement.QuerySelector("td.rating div span").Attributes["title"].Value);


                    Console.WriteLine(title + ratingTitle + " and " + url);
                }
            }
        }
    }
}

