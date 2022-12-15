namespace Wing

open System
open System.Net.Mail
open Validus

type Email = private { Email : string } with
    override x.ToString () = x.Email

    static member Of field input =
        let rule (x : string) =
            if x = "" then false
            else
                try
                    let addr = MailAddress(x)
                    if addr.Address = x then true
                    else false
                with
                | :? FormatException -> false

        let message = sprintf "%s must be a valid email address"

        match Validator.create message rule field input with
        | Ok v -> Ok { Email = v }
        | Error e -> Error e

type E164 = private { E164 : string } with
    override x.ToString() = x.E164

    static member Of field input =
        let rule (x : string) =
            let e164Regex = @"^\+[1-9]{1,3}[0-9]{4,14}$"
            if x.Length > 16 then false
            elif not(Text.RegularExpressions.Regex.IsMatch(x, e164Regex)) then false
            else true

        let message = sprintf "%s must conform to the E164 international standard"

        match Validator.create message rule field input with
        | Ok v -> Ok { E164 = v }
        | Error e -> Error e

type Date = private { Date : DateTime } with
    member x.ToDateTime () = x.Date.Date

    override x.ToString () = x.Date.ToString("yyyy\/MM\/dd")

    static member op_Explicit x = x.Date.Date

    static member Of input = { Date = input }

type StrNotEmpty = private { Str : string } with
    override x.ToString () = x.Str

    static member Of (field : string) (input : string) =
        match Check.String.notEmpty field input with
        | Ok x -> Ok { Str = x }
        | Error e -> Error e

type StrMaxLen internal (str : string) =
    member private _.Str = str

    override x.ToString () = x.Str

    static member internal Create retn (len : int) (field : string) (input : string) =
        match Check.String.betweenLen 1 len field input with
        | Ok x -> Ok (retn x)
        | Error e -> Error e

// 2-scale geometric sequence
type Str1 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str1 1 field input

type Str2 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str2 2 field input

type Str4 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str4 4 field input

type Str8 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str8 8 field input

type Str16 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str16 16 field input

type Str32 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str32 32 field input

type Str64 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str64 64 field input

type Str128 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str128 128 field input

type Str256 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str256 256 field input

type Str512 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str512 512 field input

type Str1024 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str1024 1024 field input

type Str2048 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str2048 2048 field input

// 50-scale
type Str50 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str50 50 field input

type Str100 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str100 100 field input

type Str150 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str150 150 field input

type Str200 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str200 200 field input

type Str250 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str250 250 field input

type Str300 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str300 300 field input

type Str350 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str350 350 field input

type Str400 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str400 400 field input

type Str450 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str450 450 field input

type Str500 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str500 500 field input

type Str550 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str550 550 field input

type Str600 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str600 600 field input

type Str650 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str650 650 field input

type Str700 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str700 700 field input

type Str750 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str750 750 field input

type Str800 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str800 800 field input

type Str850 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str850 850 field input

type Str900 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str900 900 field input

type Str950 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str950 950 field input

// 1000-scale
type Str1000 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str1000 1000 field input

type Str2000 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str2000 2000 field input

type Str3000 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str3000 3000 field input

type Str4000 private (str : string) =
    inherit StrMaxLen(str)
    static member Of field input = StrMaxLen.Create Str4000 4000 field input