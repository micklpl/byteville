import {HttpClient} from "aurelia-http-client"

export class DistrictDetails{
    constructor(){

    }
    
    activate(params){
        this.name = params.name;
        var client = new HttpClient();
        var self = this;
        client.get("api/districts/" + params.name).then( response => {
            self.streets = response.content;
        })
    }
}