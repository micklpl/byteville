import {HttpClient} from "aurelia-http-client"

export class Recommendations{
    placeDesc = ""

    constructor(){

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
        let q = `?lat=${this.lat}&lon=${this.lon}&price=${this.price}&area=${this.area}`
        client.get("api/recommendations" + q).then( res => {
            self.results = JSON.parse(res.response);
        })
    }
}

