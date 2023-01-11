namespace Wing

open System
open System.Net.Mail
open System.Text.RegularExpressions
open Validus

type EntityId =
    | EntityId of int

    static member op_Explicit (EntityId entityId) = entityId

    /// Attempt to create an EntityId from an untrusted source.
    static member tryCreate (field : string) (input : int) =
        Check.Int.greaterThan 0 field input
        |> Result.map EntityId

type EmailAddress =
    | EmailAddress of string

    override x.ToString () =
        let (EmailAddress emailAddressStr) = x
        emailAddressStr

    static member Empty = EmailAddress String.Empty

    /// Mask the email address for public presentation.
    member x.Masked =
        let (EmailAddress str) = x
        let atIndex = str.IndexOf "@"

        String.Concat [|
            if str.Length = 0 then "-"
            elif atIndex = 1 then "*" // one-letter user
            elif atIndex < 4 then // three or less-letter user
                str.Substring (0, 1)
                String('*', atIndex - 1)
            else
                str.Substring (0, 1)
                String('*', atIndex - 2)
                str.Substring (atIndex - 1, 1)
            str.Substring atIndex |]

    /// Attempt to create an EmailAddress from an untrusted source.
    static member tryCreate (field : string) (input : string) =
        let msg field = $"{field} mst be a valid email address"
        let rule email =
            let validEmail, _ = MailAddress.TryCreate email
            validEmail
        Validator.create msg rule field input
        |> Result.map EmailAddress

type E164 =
    | E164 of string

    override x.ToString () =
        let (E164 e164Str) = x
        e164Str

    static member Empty = E164 String.Empty

    /// Mask the E164 number for public presentation.
    member x.Masked =
        let (E164 str) = x
        if str.Length = 0 then "-"
        elif str.Length < 5 then str.Substring (0, 1) + String('*', str.Length - 1)
        else str.Substring (0, 2) + str.Substring (str.Length - 3, 2)

    /// Attempt to create an E164 from an untrusted source.
    static member tryCreate (field : string) (input : string) =
        let msg field = $"{field} must be a valid international phone number, starting with a + followed by the country code then number"
        let rule phone = Regex.IsMatch(phone, @"^\+[1-9]\d{3,14}$")
        Validator.create msg rule field input
        |> Result.map E164

type PersonalName =
    { FirstName : string
      LastName : string }

    override x.ToString () = x.FullName

    static member Empty =
        { FirstName = String.Empty
          LastName = String.Empty }

    member x.FullName = String.concat " " [| x.FirstName; x.LastName |]
    member x.LegalName = String.concat ", " [| x.LastName; x.FirstName |]

    static member tryCreate (field : string) (input : PersonalName) =
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
