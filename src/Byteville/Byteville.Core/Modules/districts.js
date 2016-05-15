import {HttpClient} from "aurelia-http-client";
import {Router} from 'aurelia-router';

export class Districts{

    static inject() { return [Router]; }

    constructor(router){
        this.options = [
            {name: "Liczba ogłoszeń"},
            {field:"TotalPrice", name:"Cena", aggregation: true, unit: "zł"},
            {field:"PricePerMeter", name:"Cena za metr", aggregation: true, unit: "zł/m2"},
            {field:"Area", name:"Powierzchnia", aggregation: true, unit: "m2"},
            {field:"NumberOfRooms", name:"Liczba pokoi", aggregation: true},
            {field:"YearOfConstruction", name:"Rok budowy", aggregation: true},
            {field:"powierzchnia", name:"Obszar [ha]", aggregation: false, unit: "ha"},
            {field:"liczba_mieszkancow", name:"Liczba mieszkańców", aggregation: false},
            {field:"zageszczenie_ludnosci", name:"Zagęszczenie ludności", aggregation: false, unit: "osób/km2"}
        ];

        this.waitingForResults = false;
        this.router = router;
    }
    
    activate(){        
        this.data = {};
        this.details = undefined;
        this.selectedOption = undefined;
        this.aggregationsData = [];
        this.selectedItemInfo = " ";

        var self = this;
        this.waitingForResults = true;
        self.countItems(self);
    }

    countItems(self){
        var client = new HttpClient();
        client.get("api/aggregations/District").then( response => {
            var response = JSON.parse(response.response);
            self.waitingForResults = false;
            setTimeout(function(){
                for(var i = 0; i < response.length; i++){
                    var name = response[i].key;                
                    var element = document.getElementById(name);
                    element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
                    self.data[name] = response[i].docCount;
                    self.aggregationsData = [];                
                }
            }, 100);            
        })
    }

    statsAggregation(self, field){
        var client = new HttpClient();
        self.data = {};
        self.waitingForResults = true;
        client.get("api/aggregations/District?statsField=" + field).then( response => {
            var response = JSON.parse(response.response);
            self.waitingForResults = false;
            response = response.sort((item1, item2) => {
                let avg1 = item1.aggregations.inner_aggregation.average;
                let avg2 = item2.aggregations.inner_aggregation.average;
                if(avg1 === avg2)
                    return 0;
                if(avg1 < avg2)
                    return 1;
                return -1;
            });

            setTimeout(function(){
                for(var i = 0; i < response.length; i++){
                    let name = response[i].key;
                    let element = document.getElementById(name);
                    element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
                }
            }, 100);            

            let aggregationsData = response.map(item => {
                let stats = item.aggregations.inner_aggregation;
                if(stats.count === 0) return undefined;
                
                return{
                    key: item.key,
                    avg: stats.average.toFixed(2),
                    min: stats.min.toFixed(2),
                    max: stats.max.toFixed(2)
                }
            });

            aggregationsData = aggregationsData.filter(item => item !== undefined);

            aggregationsData.forEach(item =>{
                self.data[item.key] = item.avg;
            });

            document.querySelector('#aggs').setAttribute("aggregations", JSON.stringify(aggregationsData));            
        })
    }

    districtStats(self, field){
        var client = new HttpClient();
        self.data = {};
        self.waitingForResults = true;
        client.get("api/districtstats?id=" + field).then( response => {
            var response = JSON.parse(response.response);
            self.data = JSON.parse(response);
            self.waitingForResults = false;

            let items = [];
            for(var key in self.data){
                let value = self.data[key].replace(",",".").replace(" ", "");

                items.push({
                    key: key, 
                    value: parseFloat(value)
                });
            }

            items = items.sort((item1, item2) => {
                let v1 = item1.value;
                let v2 = item2.value;
                if(v1 === v2)
                    return 0;
                if(v1 < v2)
                    return 1;
                return -1;
            });

            setTimeout(function(){
                for(var i = 0; i < items.length; i++){
                    let name = items[i].key;
                    let element = document.getElementById(name);
                    element.setAttribute("fill", `hsl(${8*i}, 100%, 50%)`);
                }
            }, 100);
            

            self.aggregationsData = undefined;            
        })
    }

    fetchData(field, aggregation){
        var self = this;
        this.selectedOption = field;

        if(!field){
            self.countItems(self);
        }
        else if(aggregation){            
            self.statsAggregation(self, field);
        }
        else{
            self.districtStats(self, field);
        }
    }

    selectDistrict($event){
        $event.srcElement.classList.add("pulsar-animation");
        this.selectedItem  = $event.srcElement.id;
        var districtId = $event.srcElement.id;
        this.selectedItemInfo = districtId;
        this.selectedItemInfo += ": " + this.data[districtId];

        if(this.selectedOption){
            this.selectedItemInfo += " " + (this.options.filter(opt => opt.field === this.selectedOption)[0].unit || "");
        }
    }

    leaveDistrict($event){
        $event.srcElement.classList.remove("pulsar-animation");
        this.selectedItem = undefined;
        this.selectedItemInfo = "";
    }

    optionToTitle(selectedOption){
        if(selectedOption === undefined) return "Liczba ogłoszeń";
        let elem = this.options.filter(option => option.field === selectedOption)[0]; 
        return elem.name;
    }

    redirectTo(district){
        this.router.navigate('advertsList/'+ district);
    }
}
