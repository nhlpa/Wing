namespace Wing.Email

open System
open System.IO
open System.Net.Mail

type EmailAddress =
    { Email : string
      Name : string }

type EmailAttachment =
    { ContentId : string
      Name : string
      Blob : byte[] }

type EmailMessage =
    { From : EmailAddress
      Subject : string
      Body : string
      Attachments : EmailAttachment list
      Recipients : EmailAddress list }

type EmailMessageFailure =
    { Error : exn
      Message : string }

type IEmailClient =
    inherit IDisposable
    abstract member Send : EmailMessage -> Result<unit, EmailMessageFailure>

type IEmailClientFactory =
    abstract member CreateClient : unit -> IEmailClient

module EmailMessage =
    let toMailMessage (email : EmailMessage) =
        let msg = new MailMessage()
        msg.From <- MailAddress(email.From.Email, email.From.Name)
        msg.Subject <- email.Subject

        msg.Body <- email.Body

        for r in email.Recipients do
            msg.To.Add (MailAddress(r.Email, r.Name))

        for a in email.Attachments do
            let ms = new MemoryStream(a.Blob)
            let attachment = new Attachment(contentStream = ms, name = a.Name)
            attachment.ContentId <- a.ContentId
            msg.Attachments.Add(attachment)

        msg

type SmtpEmailClient (smtp : SmtpClient) =
    interface IEmailClient with
        member _.Send (email) =
            try
                smtp.Send(EmailMessage.toMailMessage email)
                Ok ()
            with
            | :? SmtpException as ex ->
                Error {
                    Error = ex
                    Message = "Failed to send email via SMTP" }

        member _.Dispose () =
            smtp.Dispose ()

type SmtpEmailClientFactory (host, port) =
    interface IEmailClientFactory with
        member _.CreateClient () =
            let smtp = new SmtpClient(host = host, port = port)
            new SmtpEmailClient(smtp)