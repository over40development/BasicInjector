#pragma once

#pragma warning(disable: 4005)
#pragma warning(disable: 4244)
#pragma warning(disable: 4302)
#pragma warning(disable: 4311)

#pragma comment (lib, "Psapi.lib")

#include <stdio.h>
#include <tchar.h>
#include <string>
#include <Windows.h>
#include <winnt.h>
#include <time.h>
#include <cstdlib>
#include <fstream>
#include <TlHelp32.h>
#include <Psapi.h>
#include <stdexcept>
#include <minwindef.h>

#include "InjectorDll.h"
#include "Inject.h"
#include "WinNT.h"