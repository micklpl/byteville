import {HttpClient} from "aurelia-http-client"
import {ObserverLocator} from 'aurelia-framework';

export class AdvertsList{
    constructor(){        
        this.params = {
            q: "", 
            district: "",
            timespan: "",
            options: ""
        };
        this.observerLocator = new ObserverLocator(); 
    }
    
    activate(){        
        var self = this;
        var client = new HttpClient();
        this.search();

        client.get("api/trends/").then( payload => {
            self.trends = JSON.parse(payload.response).description.items;
        })        
    }

    bind(){
        var self = this;
        this.observerLocator.getObserver(this.params, 'q').subscribe(function executeSearch(val){
            self.search(self.params);            
        });
    }

    search(params){
        var url = "api/fts";
        var client = new HttpClient();
        var self = this;

        if(typeof params === "object"){
            if(!!params.q)
                url = this.appendQueryParam(url, "q", params.q);

            if(!!params.district)
                url = this.appendQueryParam(url, "district", params.district);

            if(!!params.timespan)
                url = this.appendQueryParam(url, "dateFrom", params.timespan);
            
            if(!!params.options)
                url = this.appendQueryParam(url, "positiveFields", params.options);
        }        

        client.get(url).then( response => {
            self.adverts = response.content;
        })
    }

    appendQueryParam(url, name, value){
        let pair = name + "=" + value;
        return url.lastIndexOf("?") !== -1 ? url + "&" + pair : url + "?" + pair;
    }

    filterChanged(){
        var self = this;
        setTimeout(function(){
            self.search(self.params);  
        }, 500);            
    }

    setInput(value){
        this.params.q = value;
        filterChanged();
    }
}

