namespace Wing.Web

open System.Threading.Tasks

type CommandError =
    | CommandInputError of string list
    | CommandOperationError

type Command<'TInput> = 'TInput -> Result<unit, CommandError>
type CommandAsync<'TInput> = 'TInput -> Task<Result<unit, CommandError>>

type QueryError =
    | QueryInputError of string list
    | QueryOperationError
    | NoResult

type Query<'TInput, 'TOutput> = 'TInput -> Result<'TOutput, QueryError>
type QueryAsync<'TInput, 'TOutput> = 'TInput -> Task<Result<'TOutput, QueryError>>

[<AutoOpen>]
module Operations =
    let inline commandOperationError _ = CommandOperationError
    let inline queryOperationError _ = QueryOperationError