namespace Wing

open System
open System.Net.Mail
open Validus

type EntityId =
    | EntityId of int

    static member tryCreate (field : string) (input : int) =
        Check.Int.greaterThan 0 field input
        |> Result.map EntityId

type EmailAddress =
    | EmailAddress of string

    static member tryCreate (field : string) (input : string) =
        let msg field = $"{field} mst be a valid email address"
        let rule email =
            let validEmail, _ = MailAddress.TryCreate email
            validEmail
        Validator.create msg rule field input
        |> Result.map EmailAddress

    override x.ToString () =
        let (EmailAddress emailAddressStr) = x
        emailAddressStr

    member x.Mask () =
        let (EmailAddress str) = x
        let atIndex = str.IndexOf "@"
        String.Concat [|
            str.Substring (0, 1)
            String('*', atIndex - 2)
            str.Substring (atIndex - 1, 1)
            str.Substring atIndex |]

type PersonalName =
    { FirstName : string
      LastName : string }

    static member tryCreate (field : string) (input : PersonalName) =
        validate {
            let! firstName = Check.String.betweenLen 2 32 $"{field} First name" input.FirstName
            and! lastName = Check.String.betweenLen 2 32 $"{field} Last name" input.LastName
            return {
                FirstName = firstName
                LastName = lastName }
        }

    member x.FullName () = String.concat " " [| x.FirstName; x.LastName |]
    member x.LegalName () = String.concat ", " [| x.LastName; x.FirstName |]