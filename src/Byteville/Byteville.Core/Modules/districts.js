import {HttpClient} from "aurelia-http-client"

export class Districts{
    constructor(){
        this.options = [
            {name: "Liczba ogloszen"},
            {field:"TotalPrice", name:"Cena"},
            {field:"PricePerMeter", name:"Cena za metr"},
            {field:"Area", name:"Powierzchnia"},
            {field:"NumberOfRooms", name:"Liczba pokoi"},
            {field:"YearOfConstruction", name:"Rok budowy"}            
        ];
    }
    
    activate(){        
        this.data = {};
        this.details = undefined;
        this.selectedOption = undefined;
        this.aggregationsData = [];

        var self = this;
        self.countItems(self);        
    }

    countItems(self){
        var client = new HttpClient();
        client.get("api/aggregations/District").then( response => {
            var response = JSON.parse(response.response);
            for(var i = 0; i < response.length; i++){
                var name = response[i].key;
                var element = document.getElementById(name);
                element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
                self.data[name] = response[i].docCount;
                self.aggregationsData = [];
            }
        })
    }

    statsAggregation(self, field){
        var client = new HttpClient();
        self.data = {};
        client.get("api/aggregations/District?statsField=" + field).then( response => {
            var response = JSON.parse(response.response);
            response = response.sort((item1, item2) => {
                let avg1 = item1.aggregations.inner_aggregation.average;
                let avg2 = item2.aggregations.inner_aggregation.average;
                if(avg1 === avg2)
                    return 0;
                if(avg1 < avg2)
                    return 1;
                return -1;
            });

            for(var i = 0; i < response.length; i++){
                let name = response[i].key;
                let element = document.getElementById(name);
                element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
            }

            let aggregationsData = response.map(item => {
                let stats = item.aggregations.inner_aggregation;
                return{
                    key: item.key,
                    avg: stats.average.toFixed(2),
                    min: stats.min.toFixed(2),
                    max: stats.max.toFixed(2)
                }
            });

            document.querySelector('#aggs').setAttribute("aggregations", JSON.stringify(aggregationsData));
        })
    }

    executeAggregation(field){
        var self = this;
        this.selectedOption = field;

        if(field){
            self.statsAggregation(self, field);
        }
        else{
            self.countItems(self);
        }
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
