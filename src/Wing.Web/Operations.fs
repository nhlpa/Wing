namespace Wing.Web

type CommandError =
    | CommandInputError of string list
    | CommandOperationError

type Command<'TInput> = 'TInput -> Result<unit, CommandError>

type QueryError =
    | QueryInputError of string list
    | QueryOperationError
    | NoResult

type Query<'TInput, 'TOutput> = 'TInput -> Result<'TOutput, QueryError>

[<AutoOpen>]
module Operations =
    let inline commandOperationError _ = CommandOperationError
    let inline queryOperationError _ = QueryOperationError