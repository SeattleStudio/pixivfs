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

    //作品评论
    member __.illust_comments (illust_id, ?offset, ?include_total_comments,
                               ?req_auth) =
        let offset = defaultArg offset null
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/illust/comments"
        let mutable query = [ "illust_id", illust_id ]
        if not (String.IsNullOrEmpty offset) then
            query <- query @ [ "offset", offset ]
        if not (include_total_comments.IsNone) then
            query <- query @ [ "include_total_comments",
                               (if include_total_comments.Value then "true"
                                else "false") ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //相关作品
    member __.illust_related (illust_id, ?filter, ?seed_illust_ids, ?req_auth) =
        let filter = defaultArg filter "for_ios"
        let seed_illust_ids = defaultArg seed_illust_ids []
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v2/illust/related"

        let mutable query =
            [ "illust_id", illust_id
              "filter", filter ]
        if not seed_illust_ids.IsEmpty then
            for x in seed_illust_ids do
                query <- query @ [ "seed_illust_ids[]", x ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //首页推荐
    //content_type: [illust, manga]
    member __.illust_recommended (?content_type, ?include_ranking_label, ?filter,
                                  ?max_bookmark_id_for_recommend,
                                  ?min_bookmark_id_for_recent_illust, ?offset,
                                  ?include_ranking_illusts, ?bookmark_illust_ids,
                                  ?include_privacy_policy, ?req_auth) =
        let content_type = defaultArg content_type "illust"
        let include_ranking_label = defaultArg include_ranking_label true
        let filter = defaultArg filter "for_ios"
        let max_bookmark_id_for_recommend =
            defaultArg max_bookmark_id_for_recommend null
        let min_bookmark_id_for_recent_illust =
            defaultArg min_bookmark_id_for_recent_illust null
        let offset = defaultArg offset null
        let bookmark_illust_ids = defaultArg bookmark_illust_ids []
        let include_privacy_policy = defaultArg include_privacy_policy null
        let req_auth = defaultArg req_auth true

        let url =
            if req_auth then "https://app-api.pixiv.net/v1/illust/recommended"
            else "https://app-api.pixiv.net/v1/illust/recommended-nologin"

        let mutable query =
            [ "content_type", content_type
              "include_ranking_label",
              (if include_ranking_label then "true"
               else "false")
              "filter", filter ]

        if not (String.IsNullOrEmpty max_bookmark_id_for_recommend) then
            query <- query
                     @ [ "max_bookmark_id_for_recommend",
                         max_bookmark_id_for_recommend ]
        if not (String.IsNullOrEmpty min_bookmark_id_for_recent_illust) then
            query <- query
                     @ [ "min_bookmark_id_for_recent_illust",
                         min_bookmark_id_for_recent_illust ]
        if not (String.IsNullOrEmpty offset) then
            query <- query @ [ "offset", offset ]
        if not include_ranking_illusts.IsNone then
            query <- query @ [ "include_ranking_illusts",
                               (if include_ranking_illusts.Value then "true"
                                else "false") ]
        if not req_auth then
            let mutable ids = ""
            for x in bookmark_illust_ids do
                ids <- ids + x + ","
            if not (ids = "") then
                ids <- ids.TrimEnd ','
                query <- query @ [ "bookmark_illust_ids", ids ]
        if not (String.IsNullOrEmpty include_privacy_policy) then
            query <- query
                     @ [ "include_privacy_policy", include_privacy_policy ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //作品排行
    //mode: [day, week, month, day_male, day_female, week_original, week_rookie, day_manga]
    //date: yyyy-mm-dd
    member __.illust_ranking (?mode, ?filter, ?date, ?offset, ?req_auth) =
        let mode = defaultArg mode "day"
        let filter = defaultArg filter "for_ios"
        let date = defaultArg date null
        let offset = defaultArg offset null
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/illust/ranking"

        let mutable query =
            [ "mode", mode
              "filter", filter ]
        if not (String.IsNullOrEmpty date) then
            query <- query @ [ "date", date ]
        if not (String.IsNullOrEmpty offset) then
            query <- query @ [ "offset", offset ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //趋势标签
    member __.trending_tags_illust (?filter, ?req_auth) =
        let filter = defaultArg filter "for_ios"
        let req_auth = defaultArg req_auth true
        let url = "https://app-api.pixiv.net/v1/trending-tags/illust"
        let query = [ "filter", filter ]
        __.no_auth_requests_call("GET", url, query = query, req_auth = req_auth)
          .Body.ToString()
        |> __.get_json
        |> JsonValue.Parse
