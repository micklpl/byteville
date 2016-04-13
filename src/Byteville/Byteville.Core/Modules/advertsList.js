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
    }
}

