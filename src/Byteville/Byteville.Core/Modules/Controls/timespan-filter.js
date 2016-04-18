import {bindable, bindingMode, inject} from 'aurelia-framework';
import moment from 'moment';

@inject(Element)
export class TimespanFilter{
    @bindable({ defaultBindingMode: bindingMode.twoWay }) selectedSpan = undefined;
    
    constructor(element){
        this.element = element;
        let now = new Date()
        this.timespanOptions = [
            {
                name: "Ostatnia godzina",
                value: moment().subtract(1, 'hours')
            },
            {
                name: "Ostatnie 24 godziny",
                value: moment().subtract(1, 'days')
            },
            {
                name: "Ostatni tydzień",
                value: moment().subtract(7, 'days')
            },
            {
                name: "Ostatni miesiąc",
                value: moment().subtract(1, 'months')
            }
        ];
    }
    
    activate(){
        
    }

    selectItem(item){
        this.selectedSpan = item.value;
        this.selectedSpanName = item.name;
        let changeEvent = new CustomEvent('change', {
            bubbles: true
        });
        this.element.dispatchEvent(changeEvent);
    }
}

