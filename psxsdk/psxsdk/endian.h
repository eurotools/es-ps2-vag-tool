#pragma once

#include <stdio.h>

unsigned short read_le_word(FILE* f);

unsigned int read_le_dword(FILE* f);

void write_le_word(FILE* f, unsigned short leword);

void write_le_dword(FILE* f, unsigned int ledword);