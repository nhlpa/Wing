namespace Wing

open Validus
open Wing

[<AbstractClass; Sealed>]
type ApiWorkflow =
    static member input (
        effect : ApiEffect<'T, unit, CommandError>,
        validator : ApiValidator<'TInput, 'T>) : Command<'TInput, CommandError> =
        let validate =
            validator
            >> Result.mapError (ValidationErrors.toList >> CommandInputError)

        validate
        >> Result.bind effect

    static member get (
        effect : ApiEffect<'TFilter, 'TDto option, QueryError>,
        inputValidator : ApiValidator<'TInput, 'TFilter>,
        dtoValidator : ApiValidator<'TDto, 'T>) : Query<'TInput, 'T, QueryError> =
        let validate =
            inputValidator
            >> Result.mapError (ValidationErrors.toList >> QueryInputError)

        let map =
            Option.map (
                dtoValidator
                >> Result.mapError (ValidationErrors.toList >> QueryInputError))
            >> Option.defaultValue (Error NoResult)

        validate
        >> Result.bind effect
        >> Result.bind map

    static member search (
        effect : ApiEffect<'TFilter, 'TDto list, QueryError>,
        inputValidator : ApiValidator<'TInput, 'TFilter>,
        dtoValidator : ApiValidator<'TDto, 'T>) : Query<'TInput, 'T list, QueryError> =
        let validate =
            inputValidator
            >> Result.mapError (ValidationErrors.toList >> QueryInputError)

        let map =
            List.map dtoValidator
            >> Result.sequence
            >> Result.mapError (ValidationErrors.toList >> QueryInputError)

        validate
        >> Result.bind effect
        >> Result.bind map

    static member list (
        effect : ApiEffect<unit, 'TDto list, QueryError>,
        dtoValidator : ApiValidator<'TDto, 'T>) : Query<unit, 'T list, QueryError> =
        ApiWorkflow.search (
            effect = effect,
            inputValidator = Ok,
            dtoValidator = dtoValidator)

    static member page (
        effect : ApiEffect<Pager<'TFilter>, 'TDto list, QueryError>,
        inputValidator : ApiValidator<'TInput, 'TFilter>,
        dtoValidator : ApiValidator<'TDto, 'T>) : Query<Pager<'TInput>, Page<'TFilter, 'T>, QueryError> = fun pager ->
        let validate =
            let pageNumber = pager.PageNumber
            let pageSize = pager.PageSize

            Option.map (
                inputValidator
                >> Result.mapError (ValidationErrors.toList >> QueryInputError)
                >> Result.map (fun x -> Pager.Of (Some x, pageNumber, pageSize)))
            >> Option.defaultValue (Ok (Pager.Of (None, pageNumber, pageSize)))

        let select pager =
            effect pager
            |> Result.map (fun items -> pager, items)

        let map (pager, items) =
            items
            |> List.map dtoValidator
            |> Result.sequence
            |> Result.map (fun items -> Page.Of (pager, items))
            |> Result.mapError (ValidationErrors.toList >> QueryInputError)

        validate pager.Filter
        |> Result.bind select
        |> Result.bind map

and internal ApiValidator<'T, 'U> = 'T -> Result<'U, ValidationErrors>

and internal ApiEffect<'T, 'U, 'E> = 'T -> Result<'U, 'E>