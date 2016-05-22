import {HttpClient} from "aurelia-http-client"

export class Estimator{
    constructor(){
        this.district="";
        this.area = "";
        this.numberOfRooms = "";
        this.tier = "";
        this.parking = false;
        this.yearOfConstruction = "";
        this.estimatedPrice = undefined;
        this.waitingForResults = false;
        this.districts = ["Bieńczyce", "Bronowice", "Czyżyny", "Dębniki",
        "Grzegórzki", "Krowodrza", "Łagiewniki", "Nowa Huta", "Podgórze", 
        "Prądnik Czerwony", "Prądnik Biały", "Prokocim-Bieżanów", "Mistrzejowice", 
        "Stare Miasto", "Swoszowice", "Wola Duchacka", "Wzgórza Krzesławickie", "Zwierzyniec"];
    }
    
    activate(){
    }

    executeEstimation(){
        let districtTitle = $("#select2-districtSelect-container").attr('title');        
        let selectedDistrictId = this.districts.indexOf(districtTitle);
        let self = this;
        let q = `?districtId=${selectedDistrictId}&area=${this.area}&numberOfRooms=${this.numberOfRooms}&parking=${this.parking}&yearOfConstruction=${this.yearOfConstruction}`;
        
        this.waitingForResults = true;
        let client = new HttpClient();
        client.get("api/estimations" + q).then( res => {
            self.estimatedPrice = parseFloat(res.response).toFixed(2);
            self.waitingForResults = false;
        });
    }

    resetEstimationResult(){
        this.estimatedPrice = undefined;
    }
}
