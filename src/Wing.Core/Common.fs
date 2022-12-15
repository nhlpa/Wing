namespace Wing

open System

//
// Common
[<AutoOpen>]
module Conversions =
    let inline stringf format (x : ^a) = (^a : (member ToString : string -> string) (x, format))
    let inline bool (x : ^a) = (^a : (member ToBool : unit -> bool) x)
    let inline datetime (x : ^a) = (^a : (member ToDateTime : unit -> DateTime) x)
    let inline guid (x : ^a) = (^a : (member ToGuid : unit -> Guid) x)

[<RequireQualifiedAccess>]
module Option =
    let mapDefault mapping defaultValue =
        Option.map mapping
        >> Option.defaultValue defaultValue

[<RequireQualifiedAccess>]
module Result =
    let traverse fn list =
        let folder item acc =
            match fn item, acc with
            | Ok i, Ok a -> Ok (i :: a)
            | _, Error e
            | Error e, _ -> Error e

        let seed = Ok []

        List.foldBack folder list seed

    let sequence list =
        traverse id list

[<RequireQualifiedAccess>]
module ResultOption =
    let mapOption fn opt =
        match opt with
        | None -> Ok None
        | Some x -> Result.map Some (fn x)

    let map fn ropt =
        Result.bind (mapOption fn) ropt

//
// Logging
type LogError =
    { Error   : exn
      Message : string }

type LogMessage =
    | LogError of LogError
    | LogVerbose of string

type IAppLogger =
    abstract member Write : LogMessage -> unit

type IAppLoggerFactory =
    abstract member CreateLogger : unit -> IAppLogger

//
// API
type Command<'TInput, 'TError> = 'TInput -> Result<unit, 'TError>

type Query<'TInput, 'TOutput, 'TError> = 'TInput -> Result<'TOutput, 'TError>

type CommandError =
    | CommandInputError of string list
    | CommandOperationError of string list

type QueryError =
    | NoResult
    | QueryInputError of string list
    | QueryOperationError of string list

type Page<'TFilter, 'TItem> =
    { Pager : Pager<'TFilter>
      PageItems : 'TItem list
      HasMore : bool }

    static member Of (pager : Pager<'TFilter>, ?items : 'TItem list) =
        let items' = defaultArg items []
        let hasMore = items'.Length > pager.PageSize
        let pageItems =
            match items' with
            | [] -> []
            | _ when hasMore -> items' |> List.truncate pager.PageSize
            | _ -> items'

        { Pager = pager
          PageItems  = pageItems
          HasMore = hasMore }

    static member Empty pager =
        Page.Of (pager = pager)

and Pager<'TFilter> =
    { Filter     : 'TFilter option
      PageNumber : int
      Offset     : int
      PageSize   : int
      FetchSize  : int }

    static member DefaultPageSize : int = 10

    static member Of (filter : 'TFilter option, ?pageNumber : int, ?pageSize : int) =
        let pageNumber = defaultArg pageNumber 1
        let pageSize = defaultArg pageSize Pager<'TFilter>.DefaultPageSize

        let pageNumber' = if pageNumber < 0 then 1 else pageNumber
        let pageSize' = if pageSize < 1 then Pager<'TFilter>.DefaultPageSize else pageSize // set 10 as default page size
        // include a throwaway to determine if there are more items (i.e.,
        // fetch size = page size + 1)
        { Filter     = filter
          PageNumber = pageNumber'
          Offset     = pageSize' * (pageNumber' - 1)
          PageSize   = pageSize'
          FetchSize  = pageSize' + 1 }

    static member Empty =
        Pager<'TFilter>.Of (filter = None)