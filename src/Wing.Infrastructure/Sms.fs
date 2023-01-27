namespace Wing.Infrastructure

open System
open System.Threading.Tasks

type SmsMessage =
    { MobilePhone : string
      Body : string }

type ISmsClient =
    inherit IDisposable
    abstract member Dispatch : SmsMessage -> Task<Result<unit, string>>

type ISmsClientFactory =
    abstract member CreateClient : unit -> ISmsClient
