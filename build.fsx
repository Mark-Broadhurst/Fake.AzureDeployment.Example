#I __SOURCE_DIRECTORY__

#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/AzureRmRest/lib/net461/AzureRmRest.dll"
#r "packages/KuduRest/lib/net461/KuduRest.dll"

open Fake
open AzureRmRest
open KuduRest

let subscriptionId = "bcc28703-7916-4096-bf4e-2741b2a5976d"
let tenantId = "f9b615c1-61a9-42fd-bab0-956c12f0c5a4"
let clientId = "1bd25888-a6cb-4be5-9e4a-44cce875fa07@chambersandpartners.co.uk"
let clientSecret = "W/fxVJrgNEMa17lDkF6aJ4iFCINStuiNswkH1rgdbLQ="
let rm = new ResourceManager(subscriptionId,tenantId,clientId,clientSecret)

Target "Default" <| ignore

Target "CreateResourceGroup" <| fun _ ->
    rm.CreateResourceGroup "Mark-foo" ``North Europe``
    |> Async.RunSynchronously
    |> tracef "%A"

Target "CreateAppServicePlan" <| fun _ ->
    rm.CreateAppServicePlan "Mark-foo" "Mark-foo-plan" ``B1`` ``North Europe`` 1
    |> Async.RunSynchronously
    |> tracef "%A"
    
Target "CreateAppService" <| fun _ ->
    rm.CreateAppService "Mark-foo" "Mark-foo-plan" "Mark-foo-app" ``North Europe``
    |> Async.RunSynchronously
    |> tracef "%A"

Target "CreateSqlServer" <| fun _ ->
    rm.CreateSqlServer "Mark-foo" "mark-foo-server" "Mark.B" "Password1234" ``North Europe``
    |> Async.RunSynchronously
    |> tracef "%A"

Target "CreateSqlDatabase" <| fun _ ->
    rm.CreateSqlDatabase "Mark-foo" "mark-foo-server" "mark-foo-database" ``S1`` ``North Europe``
    |> Async.RunSynchronously
    |> tracef "%A"

Target "UploadDeployment" <| fun _ -> 
    let cred = rm.GetPublishCredentials "Mark-foo" "Mark-foo-app"
    let k = new KuduRestClient("mark-foo-app", cred.Username, cred.Password)
    let f = System.IO.File.ReadAllBytes "index.html"
    k.PutFile @"D:\home\site\wwwroot\index.html" f
    |> ignore

"CreateResourceGroup"
==> "CreateSqlServer"

"CreateResourceGroup"
==> "CreateAppServicePlan"

"CreateSqlServer"
==> "CreateSqlDatabase"

"CreateAppServicePlan"
==> "CreateAppService"

"CreateAppService"
==> "UploadDeployment"

"Default"
<== ["UploadDeployment";"CreateSqlDatabase"]

RunTargetOrDefault "Default"