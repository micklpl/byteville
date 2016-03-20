import {HttpClient} from "aurelia-http-client"

export class Districts{
    constructor(){

    }
    
    activate(){
        var client = new HttpClient();
        var self = this;
        client.get("api/districts").then( response => {
            self.districts = response.content;
        })
    }
}