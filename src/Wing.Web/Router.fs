namespace Wing.Web

open Falco
open Falco.Routing

module Router =
    let mapControllers (controllers : Controller list) : HttpEndpoint list =
        controllers |> List.concat |> List.map (fun (url, actions) -> all url actions)