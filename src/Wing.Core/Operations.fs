namespace Wing

open System.Threading.Tasks
open Validus

type CommandError = CommandInputError of string list | CommandOperationError of string list
type CommandResult = Result<unit, CommandError>
type Command<'TInput> = 'TInput -> CommandResult
type CommandAsync<'TInput> = 'TInput -> Task<CommandResult>

type QueryError = QueryInputError of string list | QueryNoResult
type QueryResult<'TOutput> = Result<'TOutput, QueryError>
type Query<'TInput, 'TOutput> = 'TInput -> QueryResult<'TOutput>
type QueryAsync<'TInput, 'TOutput> = 'TInput -> Task<QueryResult<'TOutput>>

[<AutoOpen>]
module Operations =
    let inline queryNoResult (_ : 'a option) = QueryNoResult
    let inline commandInputError (errors : ValidationErrors) = CommandInputError (ValidationErrors.toList errors)
    let inline queryInputError (errors : ValidationErrors) = QueryInputError (ValidationErrors.toList errors)
