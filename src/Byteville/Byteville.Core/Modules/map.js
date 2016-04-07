import {HttpClient} from "aurelia-http-client"

export class Map{
    constructor(){
    }
    
    activate(){
        var client = new HttpClient();
        this.data = {};
        this.details = undefined;

        var self = this;
        client.get("api/aggregations/District").then( response => {
            var response = JSON.parse(response.response);
            for(var i = 0; i < response.length; i++){
                var name = response[i].key;
                var element = document.getElementById(name);
                element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
                this.data[name] = response[i].docCount;
            }
        })


    }

    selectDistrict($event){
        $event.srcElement.classList.add("pulsar-animation");
        this.selectedItem  = $event.srcElement.id;
    }

    leaveDistrict($event){
        $event.srcElement.classList.remove("pulsar-animation");
        this.selectedItem = undefined;
    }

}
