namespace Wing.Infrastructure

open System
open System.Threading.Tasks
open Wing

type SmsMessage =
    { To : E164
      Body : string }

type SendSms = SmsMessage -> Task<unit>