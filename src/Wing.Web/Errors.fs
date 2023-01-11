namespace Wing.Web

open System
open System.Net
open Falco
open Falco.Markup

module internal HttpStatusCode =
    let toDetailString (statusCode : HttpStatusCode) : string =
        let statusCodeNum = int statusCode
        String.concat "" [ "HTTP "; string statusCodeNum; " - "; string statusCode ]

module internal ErrorResponses =
    let httpStatusCode handler (statusCode : HttpStatusCode) : HttpHandler =
        Response.withStatusCode (int statusCode)
        >> handler statusCode

    let http400 handler : HttpHandler = httpStatusCode handler HttpStatusCode.BadRequest
    let http401 handler : HttpHandler = httpStatusCode handler HttpStatusCode.Unauthorized
    let http403 handler : HttpHandler = httpStatusCode handler HttpStatusCode.Forbidden
    let http404 handler : HttpHandler = httpStatusCode handler HttpStatusCode.NotFound
    let http500 handler : HttpHandler = httpStatusCode handler HttpStatusCode.InternalServerError

module TextErrorResponses =
    let private handler errors statusCode =
        let statusCodeString = HttpStatusCode.toDetailString statusCode
        let errorsString = String.concat "\n- " errors
        let message = String.concat "\n\n- " [ statusCodeString; if not(String.IsNullOrWhiteSpace(errorsString)) then errorsString ]
        Response.ofPlainText message

    let http400 errors : HttpHandler = ErrorResponses.http400 (handler errors)
    let http401 : HttpHandler = ErrorResponses.http401 (handler [])
    let http403 : HttpHandler = ErrorResponses.http403 (handler [])
    let http404 : HttpHandler = ErrorResponses.http404 (handler [])
    let http500 errors : HttpHandler = ErrorResponses.http500 (handler errors)

module HtmlErrorResponses =
    let private handler errors statusCode =
        let statusCodeString = HttpStatusCode.toDetailString statusCode
        let html = Templates.html5 "en" [] [
            Elem.h1 [] [ Text.raw statusCodeString ]
            Elem.br []
            Elem.ul [] [
                for e in errors ->
                    Elem.li [] [ Text.raw e ] ]
        ]
        Response.ofHtml html

    let http400 errors : HttpHandler  = ErrorResponses.http400 (handler errors)
    let http401 : HttpHandler  = ErrorResponses.http401 (handler [])
    let http403 : HttpHandler  = ErrorResponses.http403 (handler [])
    let http404 : HttpHandler  = ErrorResponses.http404 (handler [])
    let http500 errors : HttpHandler  = ErrorResponses.http500 (handler errors)

module JsonErrorResponses =
    let private handler (errors : string seq) (statusCode : HttpStatusCode) =
        Response.withStatusCode (int statusCode)
        >> Response.ofJson errors

    let http400 errors : HttpHandler  = ErrorResponses.http400 (handler errors)
    let http401 : HttpHandler  = ErrorResponses.http401 (handler [])
    let http403 : HttpHandler  = ErrorResponses.http403 (handler [])
    let http404 : HttpHandler  = ErrorResponses.http404 (handler [])
    let http500 errors : HttpHandler  = ErrorResponses.http500 (handler errors)
