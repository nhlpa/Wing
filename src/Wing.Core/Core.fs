namespace Wing

open System

[<RequireQualifiedAccess>]
module ResultOption =
    let bind (fn : 'T -> Result<'TOut option, 'TError>) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> fn inner
        | Ok None -> Ok None
        | Error e -> Error e

    let map (fn : 'T -> 'TOut) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> Ok (Some (fn inner))
        | Ok None -> Ok None
        | Error e -> Error e

//
// Logging

/// Log input to represent a program error.
type LogError =
    { Error   : exn
      Message : string }

/// The kind of message to output to the log.
type LogMessage =
    | LogError of LogError
    | LogVerbose of string

/// A type to perform logging.
type IAppLogger =
    /// Output the contents of the LogMessage
    abstract member Write : LogMessage -> unit

/// Factory for creating new IAppLogger instances.
type IAppLoggerFactory =
    /// Create a new instance of IAppLogger
    abstract member CreateLogger : unit -> IAppLogger

//
// Paging

/// A forward-seeking pager which supports optional filtering.
///
/// Note: Fetches +1 row to determine if more resultsexists.
type Pager<'TFilter> =
    { Filter     : 'TFilter option
      PageNumber : int
      PageSize   : int}

    static member DefaultPageNumber : int = 1
    static member DefaultPageSize : int = 10
    static member Empty =
        { Filter = None
          PageNumber = Pager<'TFilter>.DefaultPageNumber
          PageSize = Pager<'TFilter>.DefaultPageSize }

    member x.Offset = x.PageSize * (x.PageNumber - 1)
    member x.FetchSize = x.PageSize + 1

/// Bi-directionally aware page of items. Includes the optionally applied
/// filter.
type Page<'TFilter, 'TItem> =
    { Pager : Pager<'TFilter>
      FetchedItems: 'TItem list }

    static member Empty =
        { Pager = Pager<'TFilter>.Empty
          FetchedItems = [] }

    member x.HasMore = x.FetchedItems.Length > x.Pager.PageSize
    member x.NextPage = if x.HasMore then Some (x.Pager.PageNumber + 1) else None
    member x.PreviousPage = if x.Pager.PageNumber <= 1 then None else Some (x.Pager.PageNumber - 1)
    member x.PageItems = if x.HasMore then List.truncate x.Pager.PageSize x.FetchedItems else x.FetchedItems
