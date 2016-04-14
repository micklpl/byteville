import {bindable, bindingMode, inject} from 'aurelia-framework';

@inject(Element)
export class DistrictsFilter{
    @bindable({ defaultBindingMode: bindingMode.twoWay }) selectedDistrict = undefined;
    
    constructor(element){
        this.element = element;
        this.districts = ["Bieńczyce", "Bronowice", "Czyżyny", "Dębniki",
        "Grzegórzki", "Krowodrza", "Łagiewniki", "Nowa Huta", "Podgórze", 
        "Prądnik Czerwony", "Prądnik Biały", "Prokocim-Bieżanów", "Mistrzejowice", 
        "Stare Miasto", "Swoszowice", "Wola Duchacka", "Wzgórza Krzesławickie", "Zwierzyniec"];
    }
    
    activate(){
        
    }

    selectItem(item){
        this.selectedDistrict = item;
        let changeEvent = new CustomEvent('change', {
            bubbles: true
        });
        this.element.dispatchEvent(changeEvent);
    }
}

