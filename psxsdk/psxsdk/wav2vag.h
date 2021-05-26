#pragma once
#include <stdio.h>

#ifdef __cplusplus
extern "C" {
#endif

extern "C" WAV2VAG_API int get_vag_file(const char* in_file, const char* out_file, int in_sample_freq);
extern "C" WAV2VAG_API void find_predict(short* samples, double* d_samples, int* predict_nr, int* shift_factor);
extern "C" WAV2VAG_API void pack(double* d_samples, short* four_bit, int predict_nr, int shift_factor);
extern "C" WAV2VAG_API void fputi(int d, FILE* fp);

#ifdef __cplusplus
}
#endif