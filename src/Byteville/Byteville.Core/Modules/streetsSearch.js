import {HttpClient} from "aurelia-http-client"
import {ObserverLocator} from 'aurelia-framework';

export class StreetsSearch {
    
    constructor() {
        this.text = "";
        this.results = [];
        this.observerLocator = new ObserverLocator();  
        this.isSearching = false;
    }

    bind(){
        let self = this;
        let client = new HttpClient();
        this.observerLocator.getObserver(this, 'text').subscribe(function executeSearch(val){
            self.isSearching = true;
            self.results = [];
            client.get("api/search?q=" + val).then(response => {                
                setTimeout(function(){ 
                    self.isSearching = false;
                    self.results = response.content;
                }, 1000);                
            })
        });
    }
}