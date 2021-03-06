module Fable.Repl.Mouse

open Fable.Import.Browser
open Thoth.Json

type Position =
    { X : float
      Y : float }

[<RequireQualifiedAccess>]
module Cmd =

    let ups messageCtor =
        let handler dispatch =
            window.addEventListener_mouseup(fun _ ->
                dispatch messageCtor)
        [ handler ]

    let move messageCtor =
        let handler dispatch =
            window.addEventListener_mousemove(fun ev ->
                { X = ev.pageX
                  Y = ev.pageY }
                |> messageCtor
                |> dispatch)
        [ handler ]

    let iframeMessage moveCtor upCtor =
        let handler dispatch =
            window.addEventListener_message(fun ev ->
                let iframeMessageDecoder =
                    Decode.field "type" Decode.string
                    |> Decode.option
                    |> Decode.andThen
                        (function
                        | Some "mousemove" ->
                            Decode.object (fun get ->
                                { X = get.Required.Field "x" Decode.float
                                  Y = get.Required.Field "y" Decode.float })
                            |> Decode.map moveCtor
                        | Some "mouseup" ->
                            Decode.succeed upCtor
                        | _ ->
                            // Discard messages we don't know how to handle it
                            Decode.fail "Invalid message from iframe"
                    )
                iframeMessageDecoder "$" ev.data
                |> function
                    | Ok msg -> dispatch msg
                    | Error _error -> () // console.warn error
            )
        [ handler ]
