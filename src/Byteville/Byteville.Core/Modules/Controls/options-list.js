import {bindable, bindingMode, inject} from 'aurelia-framework';

@inject(Element)
export class OptionsList{
    options = [
	        { key: "Furnished", value: "Umeblowane", checked: false },
	        { key: "NewConstruction", value: "Nowe budownictwo", checked: false },
	        { key: "Elevator", value: "Winda", checked: false },
	        { key: "Basement", value: "Piwnica", checked: false },
	        { key: "Balcony", value: "Balkon", checked: false }
    ];

    @bindable({ defaultBindingMode: bindingMode.twoWay }) selectedOptions = "";

    selectedOptionsArray = [];

    toggleSelection(key){        
        let index = this.selectedOptionsArray.indexOf(key);
        
        if(index !== -1){            
            this.selectedOptionsArray.splice(index, 1);
        }
        else{
            this.selectedOptionsArray.push(key);
        }

        this.selectedOptions = this.selectedOptionsArray.join();

        return true;
    }

    selectionText(){
        let cnt = this.selectedOptionsArray.length;
        let text = "Wybrano " + cnt + " ";

        if(cnt === 1)
            text += "opcję";
        else if(cnt === 5)
            text += "opcji";
        else
            text += "opcje";

        return text;
    }
}
