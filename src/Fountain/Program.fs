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
    // let header =

    //     //home
    //     //about
    //     //posts
    let _role = HtmlElements.attr "role"
    let _dataTarget = HtmlElements.attr "data-target"

    let navbar =
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
                    a [ _class "navbar-item"; _href "/about" ] [
                        str "About"
                    ]

                    div [ _class "navbar-item has-dropdown is-hoverable" ] [
                        a [ _class "navbar-link" ] [
                            str "More"
                        ]
                        div [ _class "navbar-dropdown" ] [
                            a [ _class "navbar-item" ] [
                                str "Documentation"
                            ]
                            a [ _class "navbar-item" ] [
                                str "Jobs"
                            ]
                            a [ _class "navbar-item" ] [
                                str "Contact"
                            ]
                            hr [ _class "navbar-divider "]
                            a [ _class "navbar-item" ] [
                                str "Report an issue"
                            ]
                        ]
                    ]
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
                navbar
                div [ _class "container" ] content
            ]
        ]

    let partial () =
        h1 [] [ encodedText "Fountain" ]

    let markdown (content: string) =
        [
            partial()
            div [] [ rawText content ]
        ] |> layout

    let NotFound () =
        [
            partial()
            div [] [ str "Could not find"]
        ] |> layout

    let index (model : Message) =
        [
            partial()
            p [] [ encodedText model.Text ]
        ] |> layout

    let about () =
        [
            partial()
            p [] [ encodedText "About page" ]
        ] |> layout

    let newPage () =
        [
            form [ _action "/newpage"; _method "post" ] [
                div [] [
                    input [ _class "input"; _name "route"; _placeholder "Url Route" ] 
                ]
                div [] [
                    textarea [ _class "textarea"; _name "page"; _placeholder "Markdown Content" ] []
                ]
                div [] [
                    input [_type "submit"]
                ]
            ]   
        ] |> layout

// ---------------------------------
// Web app
// ---------------------------------

type BasePageType =
| About

let indexHandler =
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

let basePageHandler basePageType =
    match basePageType with 
    | About -> aboutHandler


[<CLIMutable>]
type NewPage =
    {
        route   : string
        page   : string
    }

let newPageHandler () =
    htmlView (Views.newPage())

let submitNewPageHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            // Binds a form payload to a Car object
            let! np = ctx.BindFormAsync<NewPage>()

            np.page
            |> Markdown.ToHtml
            |> Db.newPage np.route

            return! Successful.OK np next ctx
        }



let webApp =
    choose [
        GET >=>
            choose [
                route "/newpage" >=> newPageHandler()
                routef "/%s" (fun s -> warbler (fun _ -> markdownHandler s))
                route "/" >=> warbler (fun _ -> indexHandler)
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
            "http://localhost:5000",
            "https://localhost:5001")
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