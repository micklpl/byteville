import {HttpClient} from "aurelia-http-client"

export class Streets{
    constructor(){
        this.coordinates = {};
    }
    
    activate(params){
        this.name = params.name + ",Kraków";        
    }

    findPlaces(coords){
        let self = this;
        let geocoder = new google.maps.Geocoder();

        geocoder.geocode( {'latLng': self.coordinates}, (res, status) => {
            console.log(res);
        });
    }

    attached(){
        let self = this;
        let geocoder = new google.maps.Geocoder();

        geocoder.geocode( {'address': this.name}, (res, status) => {
            self.coordinates = res[0].geometry.location;
            this.createMap(self.coordinates);
        });


    }

    createMap(coordinates){
        let mapProp = {
            center:coordinates,
            zoom:17,
            mapTypeId:google.maps.MapTypeId.ROADMAP
        };
        this.map = new google.maps.Map(document.getElementById("googleMap"),mapProp);

        let marker = new google.maps.Marker({
            position: coordinates,
            map: this.map,
            title: this.name
        });

        marker.setMap(this.map);
    }
}