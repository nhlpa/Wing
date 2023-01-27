namespace Wing

open System
open System.Net.Mail
open System.Text.RegularExpressions
open Validus

type EntityId =
    | EntityId of int

    override x.ToString () =
        let (EntityId int) = x
        string int

    static member op_Explicit (EntityId entityId) = entityId

    /// Attempt to create an EntityId from an untrusted source.
    static member tryCreate (field : string) (input : int) =
        let msg field = $"{field} must have a valid identifier"
        Check.WithMessage.Int.greaterThan 0 msg field input
        |> Result.map EntityId

type EmailAddress =
    | EmailAddress of string

    /// Mask the email address for public presentation.
    member x.Masked =
        let (EmailAddress str) = x
        let atIndex = str.IndexOf "@"
        let finalDot = str.LastIndexOf "."

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
            "@"
            String('*', finalDot - atIndex - 2)
            str.Substring (finalDot - 1) |]

    override x.ToString () =
        let (EmailAddress str) = x
        str

    static member Empty = EmailAddress String.Empty

    /// Attempt to create an EmailAddress from an untrusted source.
    static member tryCreate (field : string) (input : string) =
        let msg field = $"{field} must be a valid email address"
        let rule email =
            let validEmail, _ = MailAddress.TryCreate email
            validEmail
        Validator.create msg rule field input
        |> Result.map EmailAddress

type E164 =
    | E164 of string

    /// Mask the E164 number for public presentation.
    member x.Masked =
        let (E164 str) = x
        if str.Length = 0 then "-"
        elif str.Length < 5 then str.Substring (0, 1) + String('*', str.Length - 1)
        else str.Substring (0, 2) + String('*', str.Length - 4) + str.Substring (str.Length - 3, 2)

    override x.ToString () =
        let (E164 str) = x
        str

    static member Empty = E164 String.Empty

    /// The en-us requirements message for E164
    static member requirements (field : string) =
        $"{field} must be a valid international phone number, starting with a + followed by the country code then number"

    /// Attempt to create an E164 from an untrusted source.
    static member tryCreate (field : string) (input : string) =
        let rule phone = Regex.IsMatch(phone, @"^\+[1-9]\d{3,14}$")
        Validator.create E164.requirements rule field input
        |> Result.map E164

type PersonalName =
    { FirstName : string
      LastName : string }

    member x.FullName = String.concat " " [| x.FirstName; x.LastName |]
    member x.LegalName = String.concat ", " [| x.LastName; x.FirstName |]

    override x.ToString () = x.FullName

    static member Empty =
        { FirstName = String.Empty
          LastName = String.Empty }

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
