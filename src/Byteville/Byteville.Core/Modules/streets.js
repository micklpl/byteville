import {HttpClient} from "aurelia-http-client"

export class Streets{
    constructor(){
        this.coordinates = {};
    }
    
    activate(params){
        this.name = params.name + ",Kraków";
        let self = this;
        let geocoder = new google.maps.Geocoder();

        geocoder.geocode( {'address': this.name}, (res, status) => {
            self.coordinates = res[0].geometry.location;
            this.findPlaces(self.coordinates);
        });
    }

    findPlaces(coords){
        let self = this;
        let geocoder = new google.maps.Geocoder();

        geocoder.geocode( {'latLng': self.coordinates}, (res, status) => {
            console.log(res);
        });
    }
}