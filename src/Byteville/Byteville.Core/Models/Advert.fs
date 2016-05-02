namespace Byteville.Core.Models
open System
open FSharp.Data.UnitSystems.SI.UnitSymbols

[<Measure>] type PLN

type Location = {
    lat: float;
    lon: float
    }

type Advert = {
    Title: String;
    Description: String;
    Md5 : String;
    Url: String;
    CreationDate: DateTime;
    TotalPrice : decimal<PLN>;
    PricePerMeter : decimal<PLN/m^2>;
    Area : decimal<m^2>;
    NumberOfRooms : Option<int>;
    Furnished : Option<bool>;
    NewConstruction : Option<bool>;
    BuildingType : Option<string>;
    Tier : Option<string>;
    YearOfConstruction : Option<int>;
    Elevator : Option<bool>;
    Basement: Option<bool>;
    Balcony: Option<bool>;
    Heating: Option<string>;
    Parking: Option<string>;

    mutable Street: Option<string>;
    mutable District: Option<string>;
    mutable Location: Option<Location>
}

exception IncorrectAdvert of string

type advertCsv = {a:decimal<m^2>;n:int;t:int;p:int;y:int;l:int;tp:decimal<PLN>} 
                   override this.ToString() = 
                                sprintf "%f;%i;%i;%i;%i;%i;%f\n" 
                                        this.a this.n this.t this.p this.y this.l this.tp