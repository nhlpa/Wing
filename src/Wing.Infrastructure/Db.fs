namespace Wing.Infrastructure

open System
open System.Data.Common
open Donald
open Wing

module private DbUnit =
    let toDetailString (dbUnit : DbUnit) =
        let cmd = dbUnit.Command :?> DbCommand
        let param =
            [ for i in 0 .. cmd.Parameters.Count - 1 ->
                let p = cmd.Parameters.[i]
                let pName = p.ParameterName
                let pValue = if isNull p.Value || p.Value = DBNull.Value then "NULL" else string p.Value
                String.Concat [ "@"; pName; " = "; pValue ] ]
            |> String.concat ", "
            |> fun str -> if (String.IsNullOrWhiteSpace str) then "--" else str

        String.Concat [ "\n"; "Parameters:\n"; param; "\n\nCommand Text:\n"; cmd.CommandText ]

module Db =
    let logDbUnit (log : WriteLogMessage) (dbUnit : DbUnit) =
        log (LogVerbose (DbUnit.toDetailString dbUnit))
        dbUnit