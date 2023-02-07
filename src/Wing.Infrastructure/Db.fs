namespace Wing.Infrastructure

open System
open System.Data
open System.Threading.Tasks

/// Provices ability to create new database connections.
type CreateDbConnection = unit -> IDbConnection
