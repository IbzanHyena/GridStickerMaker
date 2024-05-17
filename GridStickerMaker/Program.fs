// Copyright 2024 Ibzan Hyena
// This project (and this file) is licensed under the MIT License.
// See the LICENSE file in the project root, or distributed with this software,
// for more information.

module Program

open System
open System.IO
open Argu
open FSharpPlus
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing

[<Literal>]
let StickerSize = 512

type Arguments =
    | [<ExactlyOnce; AltCommandLine("-i")>] Input of string
    | [<Unique; CustomCommandLine("-x")>] X of uint
    | [<Unique; CustomCommandLine("-y")>] Y of uint
    | [<Unique; AltCommandLine("-s")>] Size of uint
    | [<Unique; AltCommandLine("-o")>] Output of string
    | [<Unique>] GridSize of uint

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Input _ -> "Filepath to the input image"
            | X _ -> "The horizontal position of the square to extract, measured from the left side"
            | Y _ -> "The vertical position of the square to extract, measured from the top"
            | Size _ -> "The side length of the square to extract"
            | Output _ -> "Output directory"
            | GridSize _ -> "The number of stickers to create (e.g. 5 for 5x5)"

let private errorHandler =
    ProcessExiter (function
        | ErrorCode.HelpText -> None
        | _ -> Some ConsoleColor.Red)

type Settings =
    { Input: string
      X: uint
      Y: uint
      S: uint option
      Output: string
      GridSize: uint }

module Settings =
    let fromArgv argv =
        let results =
            ArgumentParser.Create<Arguments>(errorHandler = errorHandler).Parse(argv)

        { Input = results.GetResult <@ Input @>
          X = results.TryGetResult <@ X @> |> Option.defaultValue 0u
          Y = results.TryGetResult <@ Y @> |> Option.defaultValue 0u
          S = results.TryGetResult <@ Size @>
          Output = results.TryGetResult <@ Output @> |> Option.defaultValue "."
          GridSize = results.TryGetResult <@ GridSize @> |> Option.defaultValue 5u }

    let run
        { Input = input
          X = x
          Y = y
          S = s
          Output = output
          GridSize = gridSize }
        : Result<unit, string> =
        monad.fx' {
            let image = Image.Load(input)
            let w = uint image.Width
            let h = uint image.Height

            printfn $"Image dimensions: %i{w}x%i{h}"
            printfn $"Offsets: x = %i{x}, y = %i{y}"

            Directory.CreateDirectory(output) |> ignore

            if x >= w then
                return! Error $"x = %i{x} is greater than the image width %i{w}"

            if y >= h then
                return! Error $"y = %i{y} is greater than the image height %i{h}"

            let s = s |> Option.defaultWith (fun () -> min (w - x) (h - y))

            printfn $"Square side length: s = %i{s}"

            if s = 0u then
                return! Error "s = 0 is invalid"

            if s > w then
                return!
                    Error
                        $"s = %i{s} is greater than the image width %i{w}\n\
                The largest possible value for s is %i{min w h}\n\
                The largest possible value for s with x = %i{x} and y = %i{y} is %i{min (w - x) (h - y)}"

            if s > h then
                return!
                    Error
                        $"s = %i{s} is greater than the image height %i{h}\n\
                The largest possible value for s is %i{min w h}\n\
                The largest possible value for s with x = %i{x} and y = %i{y} is %i{min (w - x) (h - y)}"

            if x + s > w then
                return!
                    Error
                        $"x + s = %i{x} + %i{s} = %i{x + s} is greater than the image width %i{w}\n\
                The largest possible value for x is %i{w - s}\n\
                The largest possible value for x with s = %i{s} and y = %i{y} is %i{min (w - s) (h - y)}"

            if y + s > h then
                return!
                    Error
                        $"y + s = %i{y} + %i{s} = %i{y + s} is greater than the image height %i{h}\n\
                The largest possible value for y is %i{h - s}\n\
                The largest possible value for y with s = %i{s} and x = %i{x} is %i{min (w - x) (h - s)}"

            printfn $"Grid size: %i{gridSize}x%i{gridSize}"
            let stepSize = s / gridSize
            printfn $"Side length of each sticker: %i{stepSize}"

            if stepSize = 0u then
                return! Error $"s / gridSize = %i{s} / %i{gridSize} = 0 is invalid"

            // Create the sequence of starting points to crop from
            [ for j in 0u .. (gridSize - 1u) do
                  for i in 0u .. (gridSize - 1u) -> (x + i * stepSize, y + j * stepSize) ]
            // Crop, resize, and save each sticker
            |> Seq.iteri (fun n (i, j) ->
                printfn $"%i{n + 1} / %i{gridSize * gridSize}..."

                use cropped =
                    image.Clone(fun ctx ->
                        ctx
                            .Crop(Rectangle(int i, int j, int stepSize, int stepSize))
                            .Resize(StickerSize, StickerSize, KnownResamplers.Lanczos3)
                        |> ignore)

                cropped.Save(Path.Join(output, $"%i{n + 1}.webp")))

            return ()
        }

[<EntryPoint>]
let main argv =
    Settings.fromArgv argv
    |> Settings.run
    |> function
        | Ok() -> 0
        | Error e ->
            let lastColor = Console.ForegroundColor
            Console.ForegroundColor <- ConsoleColor.Red
            Console.Error.WriteLine $"ERROR: %s{e}"
            Console.ForegroundColor <- lastColor
            1
