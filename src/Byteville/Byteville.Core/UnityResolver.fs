namespace Byteville.Core

open System
open System.Web.Http.Dependencies
open Microsoft.Practices.Unity
open System.Collections.Generic

type UnityResolver(container: IUnityContainer) =
    member x.unityContainer = container

    interface IDependencyResolver with    

        member x.GetService(serviceType:Type) =
            try
                x.unityContainer.Resolve(serviceType)
            with
                | :? ResolutionFailedException -> null
    
        member x.GetServices(serviceType:Type) =
            try
                x.unityContainer.ResolveAll(serviceType)
            with
                | :? ResolutionFailedException -> new List<obj>() :> IEnumerable<obj>

        member x.BeginScope() =
            let child = x.unityContainer.CreateChildContainer()
            new UnityResolver(child) :> IDependencyScope

        member x.Dispose() =
            x.unityContainer.Dispose()