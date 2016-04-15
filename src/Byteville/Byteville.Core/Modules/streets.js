import {HttpClient} from "aurelia-http-client"

export class Streets{
    myPlace = "";

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
        this.geocode(self.name, (res, status) => {
            self.coordinates = res[0].geometry.location;
            this.createMap(self.coordinates);
        });
    }

    geocode(name, cb){
        let geocoder = new google.maps.Geocoder();
        geocoder.geocode( {'address': name}, cb);
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

    getRoute(){
        let self = this;

        this.geocode(this.myPlace, (res, status) => {
            let placeCoordinates = res[0].geometry.location;
            
            let directionsService = new google.maps.DirectionsService;
            let directionsDisplay = new google.maps.DirectionsRenderer;
            directionsDisplay.setMap(self.map);
            self.placeCoordinates = placeCoordinates;

            directionsService.route({
                origin: self.coordinates,
                destination: placeCoordinates,
                travelMode: google.maps.TravelMode.DRIVING
            }, 
            (response, status) => {
                if (status === google.maps.DirectionsStatus.OK) {
                    directionsDisplay.setDirections(response);
                    this.calculateDistance();
                }
            });
        });
    }

    calculateDistance(){
        let distanceMatrixService = new google.maps.DistanceMatrixService();
        let self = this;

        distanceMatrixService.getDistanceMatrix(
        {
            origins: [this.coordinates],
            destinations: [this.placeCoordinates],
            travelMode: google.maps.TravelMode.DRIVING,
        }, (response, status) =>{
            let val = response.rows[0].elements[0];
            self.timeToWork = val.duration.text;
            self.kilometers = val.distance.text;
        });        
    }

}