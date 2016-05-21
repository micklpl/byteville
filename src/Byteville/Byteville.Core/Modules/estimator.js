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
    }
    
    activate(){
    }

    executeEstimation(){
        let client = new HttpClient();

        let self = this;
        let q = `?districtId=${this.district}&area=${this.area}&numberOfRooms=${this.numberOfRooms}
                 &parking=${this.parking}&yearOfConstruction=${this.yearOfConstruction}`;
        
        this.waitingForResults = true;
        client.get("api/estimations" + q).then( res => {
            self.estimatedPrice = parseFloat(res.response).toFixed(2);
            self.waitingForResults = false;
        });
    }

    resetEstimationResult(){
        this.estimatedPrice = undefined;
    }
}
