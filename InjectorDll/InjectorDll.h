// InjectDll.h


#pragma once

#include "stdafx.h"

#define DllExport __declspec(dllexport)

// Exported Method(s)
extern "C" DllExport bool Map(unsigned char * source, UINT size, UINT processId);