module Db 
    type RoutePage = { route: string; title: string; page: string; onNav: bool } 

    let mutable pages = List.empty<RoutePage>

    let pageSearch route routePage =
        route = routePage.route
    
    let searchPage (route: string) =
        pages 
            |> List.tryFind (pageSearch route)

    let getOnNavRoutes () =
        pages
        |> List.filter (fun r -> r.onNav)
    
    let newPage np =
        pages <- np :: pages


