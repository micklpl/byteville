import {HttpClient} from "aurelia-http-client"

export class Recommendations{
    placeDesc = ""

    constructor(){
        this.waitingForResults = false;
        this.remember = false;
        let preferences = localStorage.getItem('userPreferences');
        preferences = !!preferences ? JSON.parse(preferences) : {};

        this.area = preferences.area;
        this.price = preferences.price;
        this.address = preferences.address;
        this.lat = preferences.lat;
        this.lon = preferences.lon;
    }

    tryGetFromLocalStorage(key){
        return localStorage.getItem(key) || "";
    }

    geocode(name, cb){
        let self = this;
        let geocoder = new google.maps.Geocoder();
        geocoder.geocode( {'address': this.placeDesc}, (res, status) => {
            self.address = res[0].formatted_address;
            let location = res[0].geometry.location;
            self.lat = location.lat();
            self.lon = location.lng();
        });
    }

    storeIfSelected(){
        if(this.remember){
            let obj = {
                area : this.area,
                address: this.address,
                price: this.price,
                lat: this.lat,
                lon: this.lon
            };

            localStorage.setItem('userPreferences', JSON.stringify(obj));
        }
    }

    getRecommendations(){
        this.storeIfSelected();

        var client = new HttpClient();
        let self = this;
        let q = `?lat=${this.lat}&lon=${this.lon}&price=${this.price}&area=${this.area}`;
        this.waitingForResults = true;
        client.get("api/recommendations" + q).then( res => {}, err => {
            if(err.statusCode === 412){
                let message = err.response;
                let i = 0;
                let candidate = "";

                while(true){
                    candidate = message + "#" + i++;;
                    let hashBits = window.sjcl.hash.sha256.hash(candidate);
                    let hashStr = sjcl.codec.hex.fromBits(hashBits);
                    if(hashStr.indexOf("0000") === 0){                        
                        break;
                    }
                }

                var client2 = new HttpClient().configure(x => {
                                  x.withHeader('X-Proof-Of-Work', candidate);
                              });

                client2.get("api/recommendations" + q).then( resp => {
                    self.results = JSON.parse(resp.response);
                    self.waitingForResults = false;
                });
            }
        });
    }

    reset(){
        this.address = undefined;
        this.lat = undefined;
        this.lon = undefined;
    }
}

