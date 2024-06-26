﻿# GridStickerMaker

GridStickerMaker takes an image and slices it up into a grid of smaller images.
These smaller images are designed to be uploaded to Telegram in the order
produced to re-create the original image.

## Installation

Download the latest release from the releases page.

## Configuration

The following configuration options are accepted on the command line:

| Long option  | Short option | Type   | Required | Default                                          | Description                                                                    |
|--------------|--------------|--------|----------|--------------------------------------------------|--------------------------------------------------------------------------------|
| `--input`    | `-i`         | string | yes      |                                                  | Filepath to the input image                                                    |
|              | `-x`         | uint   | no       | 0                                                | The horizontal position of the square to extract, measured from the left side  |
|              | `-y`         | uint   | no       | 0                                                | The vertical position of the square to extract, measured from the top          |
| `--size`     | `-s`         | uint   | no       | shortest distance to horizontal or vertical edge | The side length of the square to extract                                       |
| `--output`   | `-o`         | string | no       | `.`                                              | The directory to save the output images to                                     |
| `--gridsize` |              | uint   | no       | 5                                                | The number of stickers to create (e.g. 5 for 5x5)                              |

## Usage

Run the program from the command line with the desired configuration options.

## License

MIT license. See the [LICENSE](LICENSE) file for details.

## Contributing

Please open an issue if you want to contribute something before submitting a pull request.
This is so that we can discuss the changes before you spend time working on them.
I would hate for you to spend time on something that I don't want to include in the project.
