namespace Wing.Web

open Falco

type Controller = (string *  (HttpVerb * HttpHandler) list) list
