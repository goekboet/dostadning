namespace dostadning.cli

open System
open dostadning.domain.result

module EnvironmentVariables =
    let id = Environment.GetEnvironmentVariable "dostadning_tradera_appid" |> int
    let key = Environment.GetEnvironmentVariable "dostadning_tradera_appkey"
    let pKey = Environment.GetEnvironmentVariable("dostadning_tradera_pkey")
    let appId = new AppIdentity(id, key)