[![License](https://img.shields.io/github/license/eurotools/PS2_Vag_Tool)](https://www.gnu.org/licenses/gpl-3.0.html)
[![Issues](https://img.shields.io/github/issues/eurotools/PS2_Vag_Tool)](https://github.com/eurotools/PS2_Vag_Tool/issues)
[![GitHub Release](https://img.shields.io/github/v/release/eurotools/PS2_Vag_Tool)](https://github.com/eurotools/PS2_Vag_Tool/releases/latest)

**PlayStation 2 VAG Tool** is a versatile utility designed to seamlessly encode 16-bit PCM WAV files into the PS2 VAG Format, and also perform the reverse operation, converting PS2 VAG files back to WAV format. The tool extends its compatibility to AIFF files as well. Mono and stereo audio files are supported. In addition, the tool intelligently extracts loop information, detecting "smpl" chunks within WAV files and "MARK" chunks within AIFF files.

## Features
- Encode mono or stereo 16-bit PCM WAV files into PS2 VAG Format.
- Decode mono or stereo PS2 VAG files back into WAV format.
- Full compatibility with mono AIFF files for encoding and decoding.
- Automatic detection of loop points using "smpl" (WAV) and "MARK" (AIFF) chunks.

## Download
To get started, you can download the latest version of the tool from the link below:

[![GitHub All Releases](https://img.shields.io/github/v/release/eurotools/PS2_Vag_Tool?style=for-the-badge)](https://github.com/eurotools/PS2_Vag_Tool/releases/latest)

## Usage

### Encoding
To encode a mono or stereo 16-bit PCM WAV/AIFF file to PS2 VAG, use:

```console
AIFF2VAG.exe <InputFile>
```

Encoding Options:
- `-1` : Force non-looping encoding, ignoring embedded loop metadata.
- `-L` : Force looping encoding from the first VAG block through the last when no embedded loop is present.
- `-i <bytes>`, `--interleave <bytes>` : Stereo interleave size. Defaults to 16 bytes; accepts decimal or hexadecimal values such as `128` or `0x80`.
- `-o <file>`, `--output <file>` : Write to a specific output path. By default, the input extension is replaced with `.vag`.
- `-v`, `--verbose` : Print detected audio format, sample count and loop information.

Without `-1` or `-L`, loop points are detected automatically from `smpl` chunks in WAV files and `MARK`/`INST` chunks in AIFF files. The `-1` and `-L` options cannot be used together.

```console
AIFF2VAG.exe "input.wav"
AIFF2VAG.exe "stereo.wav" -i 0x80
```

### Decoding
To decode a mono or stereo PS2 VAG file to WAV, use:

```console
AIFF2VAG.exe Decode <InputFile> <OutputFile>
```

```
AIFF2VAG.exe Decode "input.vag" "output.wav"
AIFF2VAG.exe Decode "stereo.vag" "stereo.wav" -i 0x80
AIFF2VAG.exe Decode "stereo.vag" "stereo.wav" -i 0x80 --samples 5540963
```

The decoder must be given the same stereo interleave used by the encoder because the VAG header does not store this value.
Use `--samples <frames>` when the exact source PCM length is known to remove the final 28-sample ADPCM block padding.

## Exit codes

- `0`: conversion completed successfully.
- `1`: an input, output, format, or conversion error occurred.
- `2`: command-line usage or option error.
