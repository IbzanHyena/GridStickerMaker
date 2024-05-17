// Copyright 2024 Ibzan Hyena
// This project (and this file) is licensed under the MIT License.
// See the LICENSE file in the project root, or distributed with this software,
// for more information.

module Program

open System
open System.IO
open Argu
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Processing

[<Literal>]
let StickerSize = 512

type Arguments =
    | [<ExactlyOnce; AltCommandLine("-i")>] Input of string
    | [<Unique; CustomCommandLine("-x")>] X of int
    | [<Unique; CustomCommandLine("-y")>] Y of int
    | [<Unique; AltCommandLine("-s")>] Size of int
    | [<Unique; AltCommandLine("-o")>] Output of string
    | [<Unique>] GridSize of int

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
      X: int
      Y: int
      S: int option
      Output: string
      GridSize: int }

module Settings =
    let fromArgv argv =
        let results =
            ArgumentParser.Create<Arguments>(errorHandler = errorHandler).Parse(argv)

        { Input = results.GetResult <@ Input @>
          X = results.TryGetResult <@ X @> |> Option.defaultValue 0
          Y = results.TryGetResult <@ Y @> |> Option.defaultValue 0
          S = results.TryGetResult <@ Size @>
          Output = results.TryGetResult <@ Output @> |> Option.defaultValue "."
          GridSize = results.TryGetResult <@ GridSize @> |> Option.defaultValue 5 }

[<EntryPoint>]
let main argv =
    let { Input = input
          X = x
          Y = y
          S = s
          Output = output
          GridSize = gridSize } =
        Settings.fromArgv argv

    let image = Image.Load(input)
    Directory.CreateDirectory(output) |> ignore

    if x < 0 then
        ArgumentException $"x = %i{x} is less than 0" |> raise

    if y < 0 then
        ArgumentException $"y = %i{y} is less than 0" |> raise

    if x >= image.Width then
        ArgumentException $"x = %i{x} is greater than the image width %i{image.Width}"
        |> raise

    if y >= image.Height then
        ArgumentException $"y = %i{y} is greater than the image height %i{image.Height}"
        |> raise

    let s = s |> Option.defaultWith (fun () -> min (image.Width - x) (image.Height - y))

    if s <= 0 then
        ArgumentException $"s = %i{s} is less than or equal to 0" |> raise

    let stepSize = s / gridSize
    
    if stepSize <= 0 then
        ArgumentException $"s / gridSize = %i{s} / %i{gridSize} = %i{stepSize} is less than or equal to 0"
        |> raise

    if x + s > image.Width then
        $"x + x = %i{x} + %i{s} = %i{x + s} is greater than the image width %i{image.Width}\n\
        The largest possible value for x is %i{image.Width - s}"
        |> ArgumentException
        |> raise

    if y + s > image.Height then
        $"y + s = %i{y} + %i{s} = %i{y + s} is greater than the image height %i{image.Height}\n\
        The largest possible value for y is %i{image.Height - s}"
        |> ArgumentException
        |> raise

    // Create the sequence of starting points to crop from
    [ for j in 0 .. (gridSize - 1) do
          for i in 0 .. (gridSize - 1) -> (x + i * stepSize, y + j * stepSize) ]
    // Crop, resize, and save each sticker
    |> Seq.iteri (fun n (i, j) ->
        printfn $"%i{n + 1} / %i{gridSize * gridSize}..."

        use cropped =
            image.Clone(fun ctx ->
                ctx
                    .Crop(Rectangle(i, j, stepSize, stepSize))
                    .Resize(StickerSize, StickerSize, KnownResamplers.Lanczos3)
                |> ignore)

        cropped.Save(Path.Join(output, $"%i{n + 1}.webp")))

    0
