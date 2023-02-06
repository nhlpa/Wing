namespace Wing

open System
open System.Data
open System.Threading.Tasks

//
// Logging

/// Log input to represent a program error.
type LogError =
    { Error   : exn
      Message : string }

/// The kind of message to output to the log.
type LogMessage =
    | LogError of LogError
    | LogVerbose of string

//
// Database

/// Provides ability to perform actions against a database.
type IDbEffect =
    inherit IDisposable
    abstract member Connection : IDbConnection
    abstract member Transaction : IDbTransaction option

/// Provides ability to execute & group actions against a database together, to be
/// saved or undone at run time.
type IDbBatch =
    inherit IDbEffect
    abstract member Save : unit -> unit
    abstract member Undo : unit -> unit

/// Provices ability to create new database connections.
type IDbConnectionFactory =
    abstract member NewConnection : unit -> IDbConnection

/// A type to represent available interactions with a database.
type IDbClient =
    abstract member NewUid : unit -> Guid
    abstract member NewEffect : unit -> IDbEffect
    abstract member NewBatch : unit -> IDbBatch

//
// Messaging

/// Provides ability to dispatch messages asynchronously
type IMessageClient<'a> =
    abstract member Dispatch : 'a -> Task<unit>

// Email

type EmailRecipient =
    { Email : EmailAddress
      Name : string }

type EmailRecipientType =
    | EmailTo of EmailRecipient
    | EmailCc of EmailRecipient
    | EmailBcc of EmailRecipient

type EmailAttachment =
    { ContentId : string
      Name : string
      Blob : byte[] }

type EmailMessage =
    { Recipients : EmailRecipientType list
      Subject : string
      Body : string
      Attachments : EmailAttachment list }

type IEmailClient =
    inherit IDisposable
    inherit IMessageClient<EmailMessage>

// SMS

type SmsMessage =
    { To : E164
      Body : string }

type ISmsClient =
    inherit IDisposable
    inherit IMessageClient<SmsMessage>


//
// Lookup

/// Provides ability to search for items and retrieve details
/// regarding a specific item
type ILookupClient<'a, 'b, 'c> =
    abstract member Lookup : string -> Task<'b list>
    abstract member Get : 'a -> Task<'c option>
