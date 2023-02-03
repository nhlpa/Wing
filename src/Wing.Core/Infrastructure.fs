namespace Wing

open System
open System.Data
open System.Threading.Tasks

type IClientFactory<'a when 'a :> IDisposable> =
    abstract member Create : unit -> 'a

/// Provides ability to dispatch messages asynchronously
type IMessageClient<'a> =
    inherit IDisposable
    abstract member Dispatch : 'a -> Task<unit>

///
type ILookupClient<'a, 'b, 'c> =
    inherit IDisposable
    abstract member Lookup : string -> Task<'b list>
    abstract member Get : 'a -> Task<'c option>

//
// Data

type IDbConnectionFactory = inherit IClientFactory<IDbConnection>

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

type IEmailClient = inherit IMessageClient<EmailMessage>

type IEmailClientFactory = inherit IClientFactory<IEmailClient>

//
// SMS

type SmsMessage =
    { To : E164
      Body : string }

type DispatchSms = SmsMessage -> Task<unit>

type ISmsClient = inherit IMessageClient<SmsMessage>

type ISmsClientFactory = inherit IClientFactory<ISmsClient>
