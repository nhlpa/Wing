namespace Wing

open System.Threading.Tasks

type CommandError = CommandInputError of string list | CommandOperationError of string
type CommandResult = Result<unit, CommandError>
type Command<'TInput> = 'TInput -> CommandResult
type CommandAsync<'TInput> = 'TInput -> Task<CommandResult>

type QueryError = QueryInputError of string list | QueryNoResult | QueryOperationError of string
type QueryResult<'TOutput> = Result<'TOutput, QueryError>
type Query<'TInput, 'TOutput> = 'TInput -> QueryResult<'TOutput>
type QueryAsync<'TInput, 'TOutput> = 'TInput -> Task<QueryResult<'TOutput>>

[<AutoOpen>]
module Operations =
    let inline commandOperationError _ = CommandOperationError
    let inline queryOperationError _ = QueryOperationError
    let inline queryNoResult (_ : 'a option) = QueryNoResult
