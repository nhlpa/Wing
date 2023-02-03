namespace Wing

open System
open Validus

/// An aggregate to represent a personal name
type PersonalName =
    { FirstName : string
      LastName : string }

    member x.FullName = $"{x.FirstName} {x.LastName}"
    member x.LegalName = $"{x.LastName}, {x.FirstName}"

    override x.ToString () = x.FullName

    static member Empty =
        { FirstName = String.Empty
          LastName = String.Empty }

    static member TryCreate (field : string) (input : PersonalName) =
        validate {
            let! firstName = Check.String.betweenLen 2 32 $"{field} First name" input.FirstName
            and! lastName = Check.String.betweenLen 2 32 $"{field} Last name" input.LastName
            return {
                FirstName = firstName
                LastName = lastName }
        }

/// A forward-seeking pager which supports optional filtering.
///
/// Note: Fetches +1 row to determine if more resultsexists.
type Pager<'TFilter> =
    { Filter : 'TFilter option
      PageNumber : int
      PageSize : int}

    member x.Offset = x.PageSize * (x.PageNumber - 1)
    member x.FetchSize = x.PageSize + 1

    static member DefaultPageNumber : int = 1
    static member DefaultPageSize : int = 10
    static member Empty =
        { Filter = None
          PageNumber = Pager<'TFilter>.DefaultPageNumber
          PageSize = Pager<'TFilter>.DefaultPageSize }

/// Bi-directionally aware page of items. Includes the optionally applied
/// filter.
type Page<'TFilter, 'TItem> =
    { Pager : Pager<'TFilter>
      FetchedItems: 'TItem list }

    member x.HasMore = x.FetchedItems.Length > x.Pager.PageSize
    member x.NextPage = if x.HasMore then Some (x.Pager.PageNumber + 1) else None
    member x.PreviousPage = if x.Pager.PageNumber <= 1 then None else Some (x.Pager.PageNumber - 1)
    member x.PageItems = if x.HasMore then List.truncate x.Pager.PageSize x.FetchedItems else x.FetchedItems

    static member Empty =
        { Pager = Pager<'TFilter>.Empty
          FetchedItems = [] }
