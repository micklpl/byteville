var createDocumentDefinition = function (data) {  
    var advert = JSON.parse(data).hits.hits[0]._source;
    var createColumnItem = function (label, value, suffix) {
        if (suffix)
            value += suffix;
        return {
            text: [
                {
                    text: label,
                    bold: true
                },
                ""+ value
            ]
        }
    };

    var dd = {
        content: [],
        styles: {
            header: {
                fontSize: 24,
                bold: true
            },
            content: {
                fontSize: 12
            },
            columns: {
                fontSize: 16
            }
        }
    };

    dd.content.push({
        text: advert.Title,
        style: 'header',
        alignment: 'center',
        margin: [0, 0, 0, 30]
    });

    var location = "";
    if (advert.Street)
        location = advert.Street + ", ";

    if (advert.District)
        location += advert.District;

    if (location !== "") {
        dd.content.push({
            text: location,
            margin: [0, 0, 0, 30],
            fontSize: 14,
            bold: true
        });
    }

    var optionalColumnItems = [];

    if (advert.NumberOfRooms)
        optionalColumnItems.push(createColumnItem("Liczba pokoi: ", advert.NumberOfRooms));

    if (advert.Tier)
        optionalColumnItems.push(createColumnItem("Piętro: ", advert.Tier));

    if (advert.YearOfConstruction)
        optionalColumnItems.push(createColumnItem("Rok budowy: ", advert.YearOfConstruction));

    dd.content.push({
        columns: [
            [
                createColumnItem("Powierzchnia: ", advert.TotalPrice, " zł"),
                createColumnItem("Cena: ", advert.Area, " m2"),
                createColumnItem("Cena za metr: ", advert.PricePerMeter),
                createColumnItem("Data utworzenia: ", advert.CreationDate.substr(0, 10))
            ], optionalColumnItems
        ],
        style: 'columns'
    });    

    var booleanKeys = {
        Furnished: "umeblowane",
        NewConstruction: "nowe budownictwo",
        Elevator: "winda",
        Basement: "piwnica",
        Balcony: "balkon",
        Parking: "parking"
    }

    var found = [];
    for (var k in booleanKeys) {        
        if (advert[k] === true) {
            found.push(booleanKeys[k]);
        }
    }

    if (found.length > 0) {
        dd.content.push({
            text: found.join(', '),
            margin: [0, 30, 0, 0],
            fontSize: 14,
            bold: true
        });
    }    

    dd.content.push({
        text: advert.Description,
        style: 'content',
        margin: [0, 30, 0, 0]
    });  

    return dd;
}


var PdfPrinter = require('pdfmake');

var fonts = {
    Roboto: {
        normal: 'fonts/Roboto-Regular.ttf',
        bold: 'fonts/Roboto-Medium.ttf',
        italics: 'fonts/Roboto-Italic.ttf',
        bolditalics: 'fonts/Roboto-Italic.ttf'
    }
};

var printer = new PdfPrinter(fonts);

var zlib = require('zlib');
var gzip = zlib.createGzip();

return function(data, cb){

    var printer = new PdfPrinter(fonts);
    var docDefinition = createDocumentDefinition(data);

    var pdfDoc = printer.createPdfKitDocument(docDefinition);

    var bufs = [];
    pdfDoc.on('data', function (d) { bufs.push(d); });
    pdfDoc.on('end', function(){
        var buf = Buffer.concat(bufs);
        cb(null, buf);
    });

    pdfDoc.pipe(gzip);
    pdfDoc.end();
    gzip.flush();
}