import {HttpClient} from "aurelia-http-client"

export class Recommendations{
    placeDesc = ""

    constructor(){
        this.waitingForResults = false;
    }
    
    activate(){
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

    getRecommendations(){
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
}

