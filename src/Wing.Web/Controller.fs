namespace Wing.Web

open System.Text.Json
open Falco
open Falco.Markup
open Wing

[<AbstractClass; Sealed>]
type Controller =
    static member index (
        service : Query<unit, 'T, QueryError>,
        response : 'T -> HttpHandler,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        match service () with
        | Ok x ->
            response x

        | Error (QueryInputError e) ->
            match inputErrorResponse with
            | Some handler -> handler e
            | None -> TextErrorResponses.http400 e

        | Error (QueryOperationError e) ->
            match operationErrorResponse with
            | Some handler -> handler e
            | None -> TextErrorResponses.http500 e

        | Error NoResult ->
            match notFound with
            | Some handler -> handler
            | None -> TextErrorResponses.http404

    static member detail (
        input : 'TInput option,
        service : Query<'TInput, 'T, QueryError>,
        response : 'T -> HttpHandler,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        let result =
            match input with
            | Some x -> service x
            | None -> Error NoResult

        match result with
        | Ok x ->
            response x

        | Error (QueryInputError e) ->
            match inputErrorResponse with
            | Some handler -> handler e
            | None -> TextErrorResponses.http400 e

        | Error (QueryOperationError e) ->
            match operationErrorResponse with
            | Some handler -> handler e
            | None -> TextErrorResponses.http500 e

        | Error NoResult ->
            match notFound with
            | Some handler -> handler
            | None -> TextErrorResponses.http404

    static member page (
        input : Pager<'TFilter>,
        service : Query<Pager<'TFilter>, 'T, QueryError>,
        response : 'T -> HttpHandler,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        match service input with
        | Ok x ->
            response x

        | Error (QueryInputError e) ->
            match inputErrorResponse with
            | Some handler -> handler e
            | None -> TextErrorResponses.http400 e

        | Error (QueryOperationError e) ->
            match operationErrorResponse with
            | Some handler -> handler e
            | None -> TextErrorResponses.http500 e

        | Error NoResult ->
            match notFound with
            | Some handler -> handler
            | None -> TextErrorResponses.http404

    // static member input service view : HttpHandler =
    //     match service () with
    //     | Error (QueryOperationError e)
    //     | Error (QueryInputError e) ->
    //         let html = view None e
    //         Response.ofHtml html

    //     | Error NotFound ->
    //         let html = view None []
    //         Response.ofHtml html

    //     | Ok result ->
    //         let html = view (Some result) []
    //         Response.ofHtmlCsrf html

    // static member edit service view : HttpHandler =
    //     inputHandler service view

    // static member remove service view : HttpHandler =
    //     inputHandler service view

    // static member editOrCreate service viewService view  : HttpHandler =
    //     match service () with
    //     | Error (QueryInputError e)
    //     | Error (QueryOperationError e) ->
    //         view None e

    //     | Error NotFound ->
    //         match viewService () with
    //         | Error (QueryOperationError e)
    //         | Error (QueryInputError e) ->
    //             view None e

    //         | Error NotFound ->
    //             view None []

    //         | Ok result ->
    //             let html = view (Some result) []
    //             Response.ofHtmlCsrf html

    //     | Ok result ->
    //         let html = view (Some result) []
    //         Response.ofHtmlCsrf html

    // static member saveOrReview service viewService view successUrl : HttpHandler =
    //     match service () with
    //     | Error (CommandOperationError e) ->
    //         view None e

    //     | Error (CommandInputError inputErrors) ->
    //         match viewService () with
    //         | Error (QueryOperationError e)
    //         | Error (QueryInputError e) ->
    //             view None (inputErrors @ e)

    //         | Error NotFound ->
    //             ErrorPages.notFound

    //         | Ok result ->
    //             let html = view result inputErrors
    //             Response.ofHtmlCsrf html

    //     | Ok () ->
    //         Response.redirectTemporarily successUrl

    // static member save service view successUrl input =
    //     let viewService _ = Ok input
    //     saveOrReviewHandler service viewService view successUrl

    // static member delete = saveHandler

[<AbstractClass; Sealed>]
type HtmlController =
    static member index (
        service : Query<unit, 'T list, QueryError>,
        view : ControllerView<'T list>,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        let viewResponse result =
            let html = view result []
            Response.ofHtml html

        let errorResponse e =
            let html = view [] e
            Response.ofHtml html

        Controller.index (
            service = service,
            response = viewResponse,
            inputErrorResponse = Option.defaultValue errorResponse inputErrorResponse,
            operationErrorResponse = Option.defaultValue HtmlErrorResponses.http500 operationErrorResponse,
            notFound = Option.defaultValue HtmlErrorResponses.http404 notFound)

    static member detail (
        input : 'TInput option,
        service : Query<'TInput, 'T, QueryError>,
        view : ControllerView<'T>,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        let viewResponse result =
            let html = view result []
            Response.ofHtml html

        Controller.detail (
            input = input,
            service = service,
            response = viewResponse,
            inputErrorResponse = Option.defaultValue HtmlErrorResponses.http400 inputErrorResponse,
            operationErrorResponse = Option.defaultValue HtmlErrorResponses.http500 operationErrorResponse,
            notFound = Option.defaultValue HtmlErrorResponses.http404 notFound)

    static member page (
        input : Pager<'TFilter>,
        service : Query<Pager<'TFilter>, Page<'TFilter, 'T>, QueryError>,
        view : ControllerView<Page<'TFilter, 'T>>,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        let viewResponse result =
            let html = view result []
            Response.ofHtml html

        let errorResponse e =
            let html = view (Page<'TFilter, 'T>.Empty input) e
            Response.ofHtml html

        Controller.page (
            input = input,
            service = service,
            response = viewResponse,
            inputErrorResponse = Option.defaultValue errorResponse inputErrorResponse,
            operationErrorResponse = Option.defaultValue HtmlErrorResponses.http500 operationErrorResponse,
            notFound = Option.defaultValue HtmlErrorResponses.http404 notFound)

and ControllerView<'T> = 'T -> string list -> XmlNode

[<AbstractClass; Sealed>]
type JsonController =
    static member detail (
        input : 'TInput option,
        service : Query<'TInput, 'T, QueryError>,
        ?jsonOptions : JsonSerializerOptions,
        ?inputErrorResponse : string list -> HttpHandler,
        ?operationErrorResponse : string list -> HttpHandler,
        ?notFound : HttpHandler) : HttpHandler =
        let response =
            match jsonOptions with
            | Some options -> Response.ofJsonOptions options
            | None -> Response.ofJson

        Controller.detail (
            input = input,
            service = service,
            response = response,
            inputErrorResponse = Option.defaultValue JsonErrorResponses.http400 inputErrorResponse,
            operationErrorResponse = Option.defaultValue JsonErrorResponses.http500 operationErrorResponse,
            notFound = Option.defaultValue JsonErrorResponses.http404 notFound)
//
// JSON
// [<AbstractClass; Sealed>]
// type JsonController =
//     let badRequest msgs : HttpHandler =
//         Response.withContentType "application/json; charset=utf-8"
//         >> Response.withStatusCode 400
//         >> Response.ofJson msgs

//     static member index service jsonMap : HttpHandler =
//         match service () with
//         | Error (QueryOperationError e)
//         | Error (QueryInputError e) ->
//             badRequest e

//         | Error NotFound ->
//             Response.ofJson []

//         | Ok items ->
//             let dtos = Seq.map jsonMap items
//             responseOfJson dtos

//     static member detail service jsonMap : HttpHandler =
//         match service () with
//         | Error (QueryOperationError e)
//         | Error (QueryInputError e) ->
//             badRequest e

//         | Error NotFound ->
//             Response.withStatusCode 404
//             >> Response.ofEmpty

//         | Ok result ->
//             let dto = jsonMap result
//             responseOfJson dto