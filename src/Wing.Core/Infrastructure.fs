namespace Wing

open System
open System.Data
open System.Threading.Tasks

/// Provides ability to create clients which implement IDisposable
type IClientFactory<'a> =
    abstract member Create : unit -> 'a

/// Provides ability to dispatch messages asynchronously
type IMessageClient<'a> =
    abstract member Dispatch : 'a -> Task<unit>

/// Provides ability to search for items and retrieve details
/// regarding a specific item
type ILookupClient<'a, 'b, 'c> =
    abstract member Lookup : string -> Task<'b list>
    abstract member Get : 'a -> Task<'c option>

//
// Database

type IDbConnectionFactory = inherit IClientFactory<IDbConnection>

/// A type to represent available interactions with a database.
type IDbClient =
    abstract member NewUid : unit -> Guid
    abstract member NewConnection : unit -> IDbConnection

type DbClient(dbConnectionFactory : IDbConnectionFactory) =
    interface IDbClient with
        member _.NewUid () = Guid.NewGuid()
        member _.NewConnection () = dbConnectionFactory.Create ()

//
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

type DispatchEmail = EmailMessage -> Task<unit>

type IEmailClient =
    inherit IDisposable
    inherit IMessageClient<EmailMessage>

type IEmailClientFactory = inherit IClientFactory<IEmailClient>

//
// SMS

type SmsMessage =
    { To : E164
      Body : string }

type DispatchSms = SmsMessage -> Task<unit>

type ISmsClient =
    inherit IDisposable
    inherit IMessageClient<SmsMessage>

type ISmsClientFactory = inherit IClientFactory<ISmsClient>
