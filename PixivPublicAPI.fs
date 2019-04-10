namespace PixivFS

open System
open FSharp.Data

type PixivPublicAPI(access_token, refresh_token, user_id) =
    inherit PixivBaseAPI(access_token, refresh_token, user_id)
    new() = PixivPublicAPI(null, null, null)
    new(baseapi : PixivBaseAPI) =
        PixivPublicAPI
            (baseapi.access_token, baseapi.refresh_token, baseapi.user_id)

    //无论如何总需要登录
    member __.auth_requests_call (method, url, ?headers, ?query, ?body) =
        let mutable headers = defaultArg headers []
        __.require_auth()
        headers <- headers
                   @ [ "Referer", "http://spapi.pixiv.net/"
                       "User-Agent", "PixivIOSApp/5.8.7"

                       "Authorization",
                       String.Format("Bearer {0}", __.access_token) ]
        __.requests_call (method, url, headers, ?query = query, ?body = body)

    member __.bad_words() =
        let url = "https://public-api.secure.pixiv.net/v1.1/bad_words.json"
        __.auth_requests_call("GET", url).Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //作品详情
    member __.works (illust_id : string, ?include_sanity_level) =
        let include_sanity_level = defaultArg include_sanity_level false
        let url =
            String.Format
                ("https://public-api.secure.pixiv.net/v1/works/{0}.json",
                 illust_id)

        let query =
            [ "image_sizes", "px_128x128,small,medium,large,px_480mw"
              "include_stats", "true"
              "include_sanity_level",
              (if include_sanity_level then "true"
               else "false") ]
        __.auth_requests_call("GET", url, query = query).Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //用户资料
    member __.users (author_id) =
        let url =
            String.Format
                ("https://public-api.secure.pixiv.net/v1/users/{0}.json",
                 author_id)

        let query =
            [ "profile_image_sizes", "px_170x170,px_50x50"
              "image_sizes", "px_128x128,small,medium,large,px_480mw"
              "include_stats", "1"
              "include_profile", "1"
              "include_workspace", "1"
              "include_contacts", "1" ]
        __.auth_requests_call("GET", url, query = query).Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //我的订阅
    member __.me_feeds (?show_r18, ?max_id) =
        let show_r18 = defaultArg show_r18 true
        let max_id = defaultArg max_id null
        let url = "https://public-api.secure.pixiv.net/v1/me/feeds.json"

        let mutable query =
            [ "relation", "all"
              "type", "touch_nottext"
              "show_r18",
              (if show_r18 then "1"
               else "0") ]
        if not (String.IsNullOrEmpty max_id) then
            query <- query @ [ "max_id", max_id ]
        __.auth_requests_call("GET", url, query = query).Body.ToString()
        |> __.get_json
        |> JsonValue.Parse

    //收藏夹
    //publicity: public, private
    member __.me_favorite_works (?page, ?per_page, ?publicity, ?image_sizes) =
        let page = defaultArg page "1"
        let per_page = defaultArg per_page "50"
        let publicity = defaultArg publicity "public"
        let image_sizes =
            defaultArg image_sizes [ "px_128x128"; "px_480mw"; "large" ]
        let url =
            "https://public-api.secure.pixiv.net/v1/me/favorite_works.json"

        let query =
            [ "page", page
              "per_page", per_page
              "publicity", publicity
              "image_sizes",
              (let mutable sizesstr = ""
               for x in image_sizes do
                   sizesstr <- sizesstr + x + ","
               sizesstr.TrimEnd(',')) ]
        __.auth_requests_call("GET", url, query = query).Body.ToString()
        |> __.get_json
        |> JsonValue.Parse
