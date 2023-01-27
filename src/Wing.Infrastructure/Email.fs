namespace Wing.Infrastructure

open System
open System.IO
open System.Net.Mail
open System.Threading.Tasks
open Wing

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
    abstract member Dispatch : EmailMessage -> Task<Result<unit, string>>

//
// SMTP

type SmtpEmailClient (
    smtp : SmtpClient,
    from : EmailRecipient,
    logger : IAppLogger) =
    interface IEmailClient with
        member _.Dispatch (email) =
            task {
                try
                    let msg = new MailMessage()
                    msg.From <- MailAddress(string from.Email, from.Name)
                    msg.Subject <- email.Subject

                    msg.Body <- email.Body

                    for recipient in email.Recipients do
                        match recipient with
                        | EmailTo x -> msg.To.Add (MailAddress(string x.Email, x.Name))
                        | EmailCc x -> msg.CC.Add (MailAddress(string x.Email, x.Name))
                        | EmailBcc x -> msg.Bcc.Add (MailAddress(string x.Email, x.Name))

                    for a in email.Attachments do
                        let ms = new MemoryStream(a.Blob)
                        let attachment = new Attachment(contentStream = ms, name = a.Name)
                        attachment.ContentId <- a.ContentId
                        msg.Attachments.Add(attachment)

                    do! smtp.SendMailAsync msg
                    return Ok ()
                with
                | :? SmtpException as ex ->
                    logger.Write (LogError { Error = ex; Message = ex.Message })
                    return Error ex.Message
            }

        member _.Dispose () =
            smtp.Dispose ()
