const fs = require('fs');
const cheerio = require('cheerio');

function convertToCSV(objArray) {
  const array = typeof objArray !== 'object' ? JSON.parse(objArray) : objArray;
  let str = `${Object.keys(array[0]).map(value => `"${value}"`).join(",")}` + '\r\n';

  return array.reduce((str, next) => {
    str += `${Object.values(next).map(value => `"${value}"`).join(",")}` + '\r\n';
    return str;
  }, str);
}

function writeToCSVFile(filename, content) {
  fs.writeFile(filename, content, err => {
    if (err) {
      console.error('Error writing to CSV file', err);
    } else {
      console.log(`Saved as ${filename}`);
    }
  });
}

function main() {
  fs.readFile('products.html', 'utf8', (err, htmlContent) => {
    if (err) {
      console.error("Error reading file:", err);
      return;
    }

    const $ = cheerio.load(htmlContent);

    const productNodes = $('div.item');
    
    const products = [];

    productNodes.each((_, element) => {
      const productElem = $(element);

      const name = productElem.find('figure a img').attr('alt') || "N/A";

      const fullPriceText = productElem.find("span.price-display.formatted").text().trim();
      const priceMatch = fullPriceText.match(/[\d,]+\.\d{2}/);
      const priceText = priceMatch ? priceMatch[0].replace(/,/g, '') : "N/A";

      const ratingAttribute = productElem.attr('rating') || "N/A";
      let ratingValue = parseFloat(ratingAttribute);
      if (ratingValue > 5) {
        ratingValue /= 2; 
      }
      const formattedRating = (ratingValue % 1 === 0) ? parseInt(ratingValue, 10) : ratingValue;

      const productInfo = {
        productName: name,
        price: priceText, 
        rating: formattedRating,
      };

      products.push(productInfo);
    });

    const csvContent = convertToCSV(products);

    writeToCSVFile('products.csv', csvContent);
  });
}

main();
