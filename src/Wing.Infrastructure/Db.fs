namespace Wing.Infrastructure

open System
open System.Data
open Donald
open Wing

/// Provides ability to execute actions against a database.
type DbEffect(connection : IDbConnection) =
    interface IDbEffect with
        member _.Connection = connection
        member _.Transaction = None
        member _.Dispose() =
            connection.Dispose()

/// Provides ability to execute a group of actions against a database together,
/// to be saved or undone at run time.
type DbBatch(connection : IDbConnection, transaction : IDbTransaction) =
    interface IDbBatch with
        member _.Connection = connection
        member _.Transaction = Some transaction

        member _.Save () =
            transaction.TryCommit()

        member _.Undo () =
            transaction.TryRollback()

    interface IDisposable with
        member _.Dispose() =
            transaction.Dispose()
            transaction.Connection.Dispose()

/// Provides ability to create resources for databased-related processes.
type DbClient (connectionFactory : IDbConnectionFactory) =
    interface IDbClient with
        member _.NewUid() = Guid.NewGuid()

        member _.NewEffect() =
            let conn = connectionFactory.NewConnection()
            new DbEffect(conn)

        member _.NewBatch() =
            let conn = connectionFactory.NewConnection()
            let tran = conn.TryBeginTransaction()
            new DbBatch(conn, tran)