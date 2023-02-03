namespace Wing

open System
open System.Net.Mail
open System.Text.RegularExpressions
open Validus

[<Struct>]
type EntityId =
    | EntityId of int

    override x.ToString () = match x with EntityId int -> string int

    static member op_Explicit (EntityId entityId) = entityId

    /// Attempt to create an EntityId from an untrusted source.
    static member TryCreate (field : string) (input : int) =
        let msg field = $"{field} must have a valid identifier"
        Check.WithMessage.Int.greaterThan 0 msg field input
        |> Result.map EntityId

[<Struct>]
type EmailAddress =
    | EmailAddress of string

    /// Mask the email address for public presentation.
    member x.Masked =
        let str = string x
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

    override x.ToString () = match x with EmailAddress str -> str

    static member Empty = EmailAddress String.Empty

    /// Attempt to create an EmailAddress from an untrusted source.
    static member TryCreate (field : string) (input : string) =
        let msg field = $"{field} must be a valid email address"
        let rule email =
            let validEmail, _ = MailAddress.TryCreate email
            validEmail
        Validator.create msg rule field input
        |> Result.map EmailAddress

[<Struct>]
type E164 =
    | E164 of string

    /// Mask the E164 number for public presentation.
    member x.Masked =
        let str = string x
        if str.Length = 0 then "-"
        elif str.Length < 5 then str.Substring (0, 1) + String('*', str.Length - 1)
        else str.Substring (0, 2) + String('*', str.Length - 4) + str.Substring (str.Length - 3, 2)

    override x.ToString () = match x with E164 str -> str

    static member Empty = E164 String.Empty

    /// The en-us requirements message for E164
    static member Requirements (field : string) =
        $"{field} must be a valid international phone number, starting with a + followed by the country code then number"

    /// Attempt to create an E164 from an untrusted source.
    static member TryCreate (field : string) (input : string) =
        let rule phone = Regex.IsMatch(phone, @"^\+[1-9]\d{3,14}$")
        Validator.create E164.Requirements rule field input
        |> Result.map E164