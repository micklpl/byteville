import {HttpClient} from "aurelia-http-client"

export class AdvertsList{
    constructor(){

    }
    
    activate(){
        var client = new HttpClient();
        var self = this;
        client.get("api/fts/").then( response => {
            self.adverts = response.content;
        })

        client.get("api/trends/").then( payload => {
            self.trends = JSON.parse(payload.response).description.items;
        })
    }
}

