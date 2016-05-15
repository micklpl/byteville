import {HttpClient} from "aurelia-http-client"
import {ObserverLocator} from 'aurelia-framework';

export class AdvertsList{
    constructor(){        
        this.params = this.defaultParams();
        this.observerLocator = new ObserverLocator();
        this.waitingForResults = false;
    }

    defaultParams(){
        return {
            q: "", 
            district: "",
            timespan: "",
            options: "",
            priceFrom: "",
            priceTo: "",
            areaFrom: "",
            areaTo: ""
        };
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

            if(!!params.areaFrom)
                url = this.tryAppendNumericParam(url, "areaFrom", params.areaFrom);

            if(!!params.areaTo)
                url = this.tryAppendNumericParam(url, "areaTo", params.areaTo);

            if(!!params.priceFrom)
                url = this.tryAppendNumericParam(url, "priceFrom", params.priceFrom);

            if(!!params.priceTo)
                url = this.tryAppendNumericParam(url, "priceTo", params.priceTo);
        }        

        self.waitingForResults = true;
        client.get(url).then( response => {
            self.adverts = response.content;
            self.waitingForResults = false;
        })
    }

    appendQueryParam(url, name, value){
        let pair = name + "=" + value;
        return url.lastIndexOf("?") !== -1 ? url + "&" + pair : url + "?" + pair;
    }

    tryAppendNumericParam(url, name, value){
        let valueInt = parseInt(value.replace(" ", ""));
        if(!isNaN(valueInt)){
            return this.appendQueryParam(url, name, valueInt);
        }
        else{
            return url;
        }
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

    resetFilters(){
        let self = this;
        this.params = this.defaultParams();
        setTimeout(function(){
            self.search(self.params);
        }, 200);
    }
}

