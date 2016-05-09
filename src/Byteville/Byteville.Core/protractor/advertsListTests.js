describe('adverts list', function() {

    beforeEach(() => {
        browser.loadAndWaitForAureliaPage('http://localhost:48213/');
    });

    it('should redirect to adverts list', function() {
        browser.getTitle().then(title => expect(title.indexOf("Lista ogłoszeń")).toEqual(0));
    });

    it('should filter adverts by selected district', function(){
        element.all(by.css('.district-filter')).then(function(items) {
            var btn = element(by.id('dropdownMenu1'));            
            var district = items[0];
            district.getInnerHtml().then(districtName => {
                btn.click();
                district.click();

                browser.sleep(1000); //czekaj na AJAX-owe żądanie

                element.all(by.css('.ad-district')).then(districts =>{
                    districts.forEach(district => expect(district.getText()).toBe(districtName));
                });       
            });
        });
    });
});