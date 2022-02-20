module Db 
    type RoutePage = { route: string; page: string} 

    let mutable pages = List.empty<RoutePage>

    let pageSearch route routePage =
        route = routePage.route
    
    let searchPage (route: string) =
        pages 
            |> List.tryFind (pageSearch route)
    
    let newPage route page =
        let newRoutePage = { route = route; page = page }
        pages <- newRoutePage :: pages


