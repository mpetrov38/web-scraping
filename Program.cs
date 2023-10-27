using HtmlAgilityPack;
using System.Net;
using System.Globalization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        string path = "products.html";

        try
        {
            string htmlContent = File.ReadAllText(path);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            HtmlNodeCollection productNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='item']");

            List<object> products = new List<object>();

            foreach (HtmlNode product in productNodes)
            {
                HtmlNode imgNode = product.SelectSingleNode(".//figure/a/img");
                string name = imgNode != null ? WebUtility.HtmlDecode(imgNode.GetAttributeValue("alt", "N/A")) : "N/A";

                HtmlNode priceNode = product.SelectSingleNode(".//span[@class='price-display formatted']");
                string fullPriceText = priceNode != null ? priceNode.InnerText.Trim() : "N/A";

                var priceTextMatch = Regex.Match(fullPriceText, @"[\d,]+\.\d{2}");
                string priceText = priceTextMatch.Value.Replace(",", "");

                string ratingAttribute = product.GetAttributeValue("rating", "N/A").Trim();
                bool isParsed = float.TryParse(ratingAttribute, NumberStyles.Any, CultureInfo.InvariantCulture,
                    out float ratingValue);
                if (isParsed && ratingValue > 5)
                {
                    ratingValue /= 2;
                }
                
                object formattedRating = (ratingValue % 1 == 0) ? (object)(int)ratingValue : ratingValue;

                var productInfo = new
                {
                    productName = name,
                    price = priceText, 
                    rating = formattedRating 
                };

                products.Add(productInfo);
            }

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);
            Console.WriteLine(json);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(e.Message);
        }
    }
}