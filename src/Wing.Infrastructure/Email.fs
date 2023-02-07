namespace Wing.Infrastructure

open System
open System.Threading.Tasks
open Wing

type EmailRecipient =
    { Email : EmailAddress
      Name : string }

type EmailRecipientType =
    | EmailTo of EmailRecipient
    | EmailCc of EmailRecipient
    | EmailBcc of EmailRecipient

type EmailMessage =
    { Recipients : EmailRecipientType list
      Subject : string
      Body : string }

type SendEmail = EmailMessage -> Task<unit>