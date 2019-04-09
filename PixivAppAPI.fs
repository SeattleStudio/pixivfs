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
            __.require_auth()
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
    //type: [illust, manga]
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

    //用户收藏
    //restrict: [public, private]
    //tag: 从user_bookmark_tags_illust获取的收藏标签
    member __.user_bookmarks_illust (user_id, ?restrict, ?filter,
                                     ?max_bookmark_id, ?tag, ?req_auth) =
        let restrict = defaultArg restrict "public"
        let filter = defaultArg filter "for_ios"
        let max_bookmark_id = defaultArg max_bookmark_id null
        let tag = defaultArg tag null
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/user/bookmarks/illust"

        let mutable query =
            [ "user_id", user_id
              "restrict", restrict
              "filter", filter ]
        if not (String.IsNullOrEmpty max_bookmark_id) then
            query <- query @ [ "max_bookmark_id", max_bookmark_id ]
        if not (String.IsNullOrEmpty tag) then query <- query @ [ "tag", tag ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //关注者的新作品
    //restrict: [public, private]
    member __.illust_follow (?restrict, ?offset, ?req_auth) =
        let restrict = defaultArg restrict "public"
        let offset = defaultArg offset null
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/illust/detail"
        let mutable query = [ "restrict", restrict ]
        if not (String.IsNullOrEmpty offset) then
            query <- query @ [ "offset", offset ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //作品详情（iOS中未使用）
    member __.illust_detail (illust_id, ?req_auth) =
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/illust/detail"
        let query = [ "illust_id", illust_id ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse
