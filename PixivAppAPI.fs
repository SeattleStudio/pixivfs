namespace PixivFS

open System
open FSharp.Data

type PixivAppAPI() =
    inherit PixivBaseAPI()

    //可通过req_auth来决定是否使用登录后的数据
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
                           String.Format("Bearer {0}", __.access_token) ]
        __.requests_call (method, url, headers, ?query = query, ?body = body)

    //用户详情
    member __.user_detail (user_id, ?filter, ?req_auth) =
        let filter = defaultArg filter "for_ios"
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/user/detail"

        let query =
            [ "user_id", user_id
              "filter", filter ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //用户作品集
    member __.user_illusts (user_id, ?illust_type, ?filter, ?offset, ?req_auth) =
        let illust_type = defaultArg illust_type "illust"
        let filter = defaultArg filter "for_ios"
        let offset = defaultArg offset null
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/user/illusts"

        let mutable query =
            [ "user_id", user_id
              "filter", filter ]
        if not (String.IsNullOrEmpty illust_type) then
            query <- query @ [ "type", illust_type ]
        if not (String.IsNullOrEmpty offset) then
            query <- query @ [ "offset", offset ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse
