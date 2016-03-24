import {HttpClient} from "aurelia-http-client"

export class Map{
    constructor(){
    }
    
    activate(){
        var client = new HttpClient();
        var self = this;
        client.get("api/districts").then( response => {
            var response = JSON.parse(response.response);
            for(var i = 0; i < response.length; i++){
                var element = document.getElementById(response[i].name);
                element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
            }
        })
    }
}
