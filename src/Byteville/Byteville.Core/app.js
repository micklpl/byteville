export class App{

    configureRouter(config, router) {
        config.title = 'Byteville - find your estate';
        config.map([
          { 
              route: ['','districts'], 
              name: 'districts', 
              moduleId: './Modules/districts', 
              nav: true, title:'Lista dzielnic' 
          },
          { 
              route: 'districts/:name', 
              name: 'districtDetails', 
              moduleId: './Modules/districtDetails', 
              nav: false 
          }
        ]);

        this.router = router;
    }

    constructor(){
    }    
}
