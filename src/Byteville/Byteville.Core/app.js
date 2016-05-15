export class App{

    configureRouter(config, router) {
        config.title = 'Byteville - find your estate';
        config.map([
          { 
              route: ['','advertsList/:district'], 
              name: 'advertsList', 
              moduleId: './Modules/advertsList', 
              nav: true, title:'Lista ogłoszeń' 
          },
          { 
              route: 'estimator', 
              name: 'estimator', 
              moduleId: './Modules/estimator', 
              nav: false 
          },
          { 
              route: 'recommendations', 
              name: 'recommendations', 
              moduleId: './Modules/recommendations', 
              nav: false 
          },
          { 
              route: 'districts', 
              name: 'districts', 
              moduleId: './Modules/districts', 
              nav: false 
          },
          { 
              route: 'streets/:name', 
              name: 'streets', 
              moduleId: './Modules/streets', 
              nav: false 
          }
        ]);

        this.router = router;
    }

    constructor(){
    }
}
