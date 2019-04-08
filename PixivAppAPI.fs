namespace PixivFS

open System

type PixivAppAPI() =
    inherit PixivBaseAPI()
    member __.no_auth_requests_call (method, url, ?headers, ?query, ?body,
                                     ?req_auth) =
        let req_auth = defaultArg req_auth true
        let mutable headers = defaultArg headers []
        if not (List.exists (fun elem ->
                    let (a, _) = elem
                    a = "User-Agent" || a = "user-agent") headers)
        then
            headers <- headers
                       @ [ "App-OS", "ios"
                           "App-OS-Version", "10.3.1"
                           "App-Version", "6.7.1"

                           "User-Agent",
                           "PixivIOSApp/6.7.1 (iOS 10.3.1; iPhone8,1)" ]
        if req_auth then
            base.require_auth
            headers <- headers
                       @ [ "Authorization",
                           String.Format("Bearer {0}", base.access_token) ]
        __.requests_call (method, url, headers, ?query = query, ?body = body)
