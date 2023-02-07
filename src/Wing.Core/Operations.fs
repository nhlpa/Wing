namespace Wing

open Validus

type CommandError = CommandInputError of string list
type CommandResult = Result<unit, CommandError>

type QueryError = QueryInputError of string list | QueryNoResult
type QueryResult<'TOutput> = Result<'TOutput, QueryError>

[<AutoOpen>]
module Operations =
    let inline queryNoResult (_ : 'a option) = QueryNoResult
    let inline commandInputValidationError (errors : ValidationErrors) = CommandInputError (ValidationErrors.toList errors)
    let inline queryInputValidationError (errors : ValidationErrors) = QueryInputError (ValidationErrors.toList errors)
