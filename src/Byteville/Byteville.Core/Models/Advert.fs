namespace Byteville.Core.Models
open System
open FSharp.Data.UnitSystems.SI.UnitSymbols

[<Measure>] type PLN

type Advert = {
    Title: String;
    Description: String;
    Md5 : String;
    Url: String;
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
}

exception IncorrectAdvert of string