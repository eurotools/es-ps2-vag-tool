[![License](https://img.shields.io/github/license/eurotools/PS2_Vag_Tool)](https://www.gnu.org/licenses/gpl-3.0.html)
[![Issues](https://img.shields.io/github/issues/eurotools/PS2_Vag_Tool)](https://github.com/eurotools/PS2_Vag_Tool/issues)
[![GitHub Release](https://img.shields.io/github/v/release/eurotools/PS2_Vag_Tool)](https://github.com/eurotools/PS2_Vag_Tool/releases/latest)

**PlayStation 2 VAG Tool** is a versatile utility designed to seamlessly encode 16-bit PCM WAV files into the PS2 VAG Format, and also perform the reverse operation, converting PS2 VAG files back to WAV format. The tool extends its compatibility to AIFF files as well. Please note that the tool currently supports only mono audio files. In addition, the tool intelligently extracts loop information, detecting "smpl" chunks within WAV files and "MARK" chunks within AIFF files.

## Features

- Encode mono 16-bit PCM WAV files into PS2 VAG Format.
- Decode PS2 VAG files back into mono WAV format.
- Full compatibility with mono AIFF files for encoding and decoding.
- Automatic detection of loop points using "smpl" (WAV) and "MARK" (AIFF) chunks.

## Download
To get started, you can download the latest version of the tool from the link below:

[![GitHub All Releases](https://img.shields.io/github/v/release/eurotools/PS2_Vag_Tool?style=for-the-badge)](https://github.com/eurotools/PS2_Vag_Tool/releases/latest)

## Usage

### Encoding
For encoding mono WAV files to PS2 VAG Format, use the following command:

```console
PS2VagTool <InputFile>
```

Encoding Options:
- `-1` : Force non-looping encoding.
- `-L` : Force looping encoding.

```console
PS2VagTool "input.wav"
```

### Decoding

To decode PS2 VAG files to mono WAV format, utilize the following command:

```console
PS2VagTool Decode <InputFile> <OutputFile>
```

```
PS2VagTool Decode "input.vag" "output.wav"
```