# GridStickerMaker

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
|              | `-x`         | int    | no       | 0                                                | The horizontal position of the square to extract, measured from the left side  |
|              | `-y`         | int    | no       | 0                                                | The vertical position of the square to extract, measured from the top          |
| `--size`     | `-s`         | int    | no       | shortest distance to horizontal or vertical edge | The side length of the square to extract                                       |
| `--output`   | `-o`         | string | no       | `.`                                              | The directory to save the output images to                                     |
| `--gridsize` |              | int    | no       | 5                                                | The number of stickers to create (e.g. 5 for 5x5)                              |

## Usage

Run the program from the command line with the desired configuration options.

## License



## Contributing
