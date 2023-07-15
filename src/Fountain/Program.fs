module Fountain.App

open System
open System.IO
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Markdig

// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }


// ---------------------------------
// Views
// ---------------------------------

module Views =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Accessibility

    
    
    type NavRoute =
        {
            DisplayName : string
            Href : string
        }

        
    let getRoutes () =
        let navRoutes: NavRoute list = 
            Db.getOnNavRoutes()
            |> List.map (fun (rt: Db.RoutePage) -> { DisplayName = rt.title; Href = rt.route })
        List.toArray navRoutes
    // let header =

    //     //home
    //     //about
    //     //posts
    let _role = HtmlElements.attr "role"
    let _dataTarget = HtmlElements.attr "data-target"

    let navbar (routes: NavRoute[])=
        nav [ _class "navbar"; _role "navigation" ] [
            div [ _class "navbar-brand" ] [
                a [ _class "navbar-item"; _href "/" ] [
                //<img src="https://bulma.io/images/bulma-logo.png" width="112" height="28">
                    img [ _src "https://bulma.io/images/bulma-logo.png"; _width "112"; _height "28" ]
                ]

                a [ _class "navbar-burger"; _role "button"; _dataTarget "navbarBasicExample" ] [
                    span [ _ariaHidden "true" ] [] //change this to loop at some point
                    span [ _ariaHidden "true" ] []
                    span [ _ariaHidden "true" ] []
                ]
            ]

            div [ _class "navbar-menu"; _id "navbarBasicExample" ] [
                div [ _class "navbar-start" ] [
                    a [ _class "navbar-item"; _href "/" ] [
                        str "Home"
                    ]

                    if routes.Length > 0 then
                        let firstRoute = Array.head routes 
                        a [ _class "navbar-item"; _href firstRoute.Href] [
                            str firstRoute.DisplayName
                        ]

                        if routes.Length > 1 then

                            div [ _class "navbar-item has-dropdown is-hoverable" ] [
                                a [ _class "navbar-link" ] [
                                    str "More"
                                ]
                                div [ _class "navbar-dropdown" ] [

                                    let rest = routes.[1..]

                                    for rte in rest do
                                        a [ _class "navbar-item"; _href rte.Href ] [
                                            str rte.DisplayName
                                        ]
                                    hr [ _class "navbar-divider "]
                                    a [ _class "navbar-item" ] [
                                        str "Report an issue"
                                    ]
                                ]
                            ]
                        else
                            div [] []
                    else
                        div [] []
                   
                ]

                div [ _class "navbar-end" ] [
                    div [ _class "navbar-item" ] [
                        div [ _class "buttons" ] [
                            a [ _class "button is-primary" ] [
                                str "Sign up"
                            ]
                            a [ _class "button is-light" ] [
                                str "Log in"
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let layout (content: XmlNode list) =
        let routes = getRoutes()
        html [] [
            head [] [
                title []  [ encodedText "Fountain" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/bulma.css" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
                script [ _src "/menu.js"; _async ] []
            ]
            body [] [
                div [ _class "container" ] [
                    
                    div [] [
                        navbar routes
                    ]
                    div [] content
                ]
            ]
        ]

    // let partial () =
    //     h1 [] [ encodedText "Fountain" ]

    let markdown (content: string) =
        [
            //partial()
            div [] [ rawText content ]
        ] |> layout
    
    let printRoutes (routes : NavRoute list) =
        [
            div [] [
                if routes.Length = 0 then
                    p [] [ str "No routes currently" ]
                else 
                    for r in routes do
                        div [] [
                            p [] [ str r.DisplayName ]
                            p [] [ str r.Href ]
                        ]
            ]

            
        ] |> layout

    let NotFound () =
        [
            //partial()
            div [] [ str "Could not find"]
        ] |> layout

    let index (model : Message) =
        [
            //partial()
            p [] [ encodedText model.Text ]
        ] |> layout

    let about () =
        [
            //partial()
            p [] [ encodedText "About page" ]
        ] |> layout

(*
    
  <input type="radio" id="css" name="fav_language" value="CSS">
  <label for="css">CSS</label><br>
  <input type="radio" id="javascript" name="fav_language" value="JavaScript">
  <label for="javascript">JavaScript</label>
*)
    let newPage () =
        [
            form [ _action "/newpage"; _method "post" ] [
                div [] [
                    input [ _class "input"; _name "route"; _placeholder "Url Route" ] 
                ]
                div [] [
                    input [ _class "input"; _name "title"; _placeholder "Page Title" ] 
                ]
                div [] [
                    textarea [ _class "textarea"; _name "page"; _placeholder "Markdown Content" ] []
                ]
                div [] [
                    input [ _name "onNav"; _type "radio"; _value "true" ]
                    label [ _for "true" ] [ str "Yes" ]
                    input [ _name "onNav"; _type "radio"; _value "false" ]
                    label [ _for "false" ] [ str "No" ]
                ]
                div [] [
                    input [_type "submit"]
                ]
            ]   
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler () =
    let greetings = sprintf "Hello %s, from Giraffe!" "johhny"
    let model     = { Text = greetings }
    let view      = Views.index model
    htmlView view

let aboutHandler () =
    Views.about()
    |> htmlView

let markdownHandler (path: string) =
    let potentialPage = Db.searchPage path

    match potentialPage with
    | Some p -> 
        let view = Views.markdown p.page
        htmlView view
    | None ->
        htmlView (Views.NotFound())

[<CLIMutable>]
type NewPage =
    {
        route   : string
        title   : string
        page   : string
        onNav  : bool
    }


let newPageHandler () =
    htmlView (Views.newPage())

let allRoutesHandler () =
    let routes : Views.NavRoute list = 
        Db.pages
        |> List.map (fun route -> { DisplayName = route.title; Href = route.route })
    
    htmlView (Views.printRoutes routes)

let toRoutePage (np: NewPage) markdown : Db.RoutePage =
    let lowerRoute = np.route.ToLower()
    { route = lowerRoute; title = np.title; page = markdown; onNav = np.onNav }

let submitNewPageHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            // Binds a form payload to a Car object
            let! npResult = ctx.TryBindFormAsync<NewPage>()

            match npResult with 
            | Ok np ->

                np.page
                |> Markdown.ToHtml
                |> toRoutePage np
                |> Db.newPage

                return! Successful.OK np next ctx
            | Error err ->
                return! RequestErrors.BAD_REQUEST err next ctx 
        }



let webApp =
    choose [
        GET >=>
            choose [
                route "/newpage" >=> newPageHandler()
                route "/allroutes" >=> warbler (fun _ -> allRoutesHandler())
                routef "/%s" (fun s -> warbler (fun _ -> markdownHandler s))
                route "/" >=> warbler (fun _ -> indexHandler())
            ]
        POST >=> 
            choose [
                route "/newpage" >=> submitNewPageHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000")
            //"https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0