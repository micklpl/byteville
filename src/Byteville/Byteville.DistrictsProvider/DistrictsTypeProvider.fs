namespace Byteville.DistrictProvider

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open FSharp.Data

#nowarn "0025"

[<TypeProvider>]
type DistrictProvider() as this = 
   inherit TypeProviderForNamespaces()

   let assembly = System.Reflection.Assembly.GetExecutingAssembly()
   let ns = "Byteville.DistrictProvider"
      
   let districtProviderType = 
        ProvidedTypeDefinition(assembly, ns, "DistrictProvider", None)

   let instantiate typeName ([|:? string as city|]: obj array) =
       let ty = ProvidedTypeDefinition(assembly, ns, typeName, None)

       let ctor = ProvidedConstructor(List.Empty,
                    InvokeCode = fun [] -> <@@ city |> getDistricts @@>)

       ctor.AddXmlDocDelayed(fun () -> "Creates an instance of provider by given city")

       ty.AddMember ctor

       city 
       |> getDistricts
       |> Seq.map(fun mapItem -> 
            ProvidedProperty(mapItem.Key, typeof<DistrictInfo>, 
                GetterCode = fun [map] -> 
                    let key = mapItem.Key
                    <@@ ((%%map:obj) :?> Map<string, DistrictInfo>).[key] @@>))
            |> Seq.toList
            |> ty.AddMembers      
       
       ty

   do
       districtProviderType.DefineStaticParameters(
        [ProvidedStaticParameter("city", typeof<string>)], 
            instantiate)

   do
        this.AddNamespace(ns, [districtProviderType])

[<assembly:TypeProviderAssembly>]
    do()