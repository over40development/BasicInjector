// Inject.cpp


#include "stdafx.h"

#ifdef RECAST
#undef RECAST
#endif
#define RECAST	reinterpret_cast

#ifdef UNICODE
#undef Module32First
#undef Module32Next
#undef MODULEENTRY32
#endif

#define inline	__forceinline
#define stdcall __stdcall

typedef HINSTANCE(stdcall * fLoadLibrary)(const char*);
typedef uintptr_t(stdcall * fGetProcAddress)(HINSTANCE, const char*);
typedef BOOL(WINAPI * fDLL_ENTRY_POINT)(void*, DWORD, void*);
typedef PROCESS_INFO PROCESS_INFO_CLASS;
typedef NTSTATUS(__stdcall * mNtCreateThreadEx)(HANDLE * pHandle, ACCESS_MASK DesiredAccess, void * pAttr, HANDLE hProcess, void * pFunction, void * pArg, ULONG Flags, SIZE_T ZeroBits, SIZE_T StackSize, SIZE_T MaxStackSize, void * pAttrListOut);
typedef NTSTATUS(__stdcall * mNtQueryInformationProcess)(HANDLE hProcess, PROCESS_INFO_CLASS PIC, void * pBuffer, ULONG BufferSize, ULONG * SizeOut);

HANDLE Start(HANDLE hTargetProcess, void * pRoutine, void * pArg, bool bUseExistingThread = false, bool bFastCall = true);
DWORD MapFromArray(unsigned char * bytes, UINT size, HANDLE hProcess, bool bUseExistingThread);
DWORD Hide(unsigned char * source, HANDLE hProcess, DWORD dwFlags);
PEB * GetPEBlock(HANDLE hProcess);

bool FileExistsA(const char * szFile);

struct MANUAL_MAPPING_DATA
{
	fLoadLibrary	pLoadLibrary;
	fGetProcAddress	pGetProcAddress;
};

void stdcall ImportTLSExecute(MANUAL_MAPPING_DATA * pData);
void inline ClearMemory(BYTE * pMemory, UINT size);
UINT inline stringLengthA(const char * szString);

DWORD LastError = ERROR_SUCCESS;

DWORD Inject(unsigned char * source, UINT size, HANDLE hProcess, bool bUseExistingThread, DWORD dwPostInjection, DWORD * dwErrorCode)
{
	DWORD dwResult = MapFromArray(source, size, hProcess, bUseExistingThread);

	if (!dwResult)
		dwResult = Hide(source, hProcess, dwPostInjection);

	if (dwErrorCode)
		*dwErrorCode = LastError;

	return dwResult;
}

DWORD MapFromArray(unsigned char * byteArray, UINT size, HANDLE hProcess, bool bUseExistingThread)
{
	if (!hProcess)
		return ERROR_INVALID_PROC_HANDLE;

	if (!byteArray)
		return ERROR_FILE_DOESNT_EXIST;

	// Randomly add between 1 and 512 bytes to our dynamic, in memory DLL
	srand(time(NULL));
	int randomBytes = rand() % 0x200 + 1;
	auto FileSize = size;// +randomBytes;

	BYTE *					pSourceData;
	IMAGE_NT_HEADERS *		pOriginalNTHeader;
	IMAGE_OPTIONAL_HEADER * pOriginalOptionalHeader;
	IMAGE_FILE_HEADER *		pOriginalFileHeader;
	BYTE *					pLocalBase;
	BYTE *					pTargetBase;

	pSourceData = new BYTE[static_cast<UINT_PTR>(FileSize)];
	memcpy(pSourceData, byteArray, FileSize);

	if (RECAST<IMAGE_DOS_HEADER*>(pSourceData)->e_magic != 0x5A4D /* 23117 */)
	{
		delete[] pSourceData;

		return ERROR_INVALID_FILE;
	}

	pOriginalNTHeader		= RECAST<IMAGE_NT_HEADERS*>(pSourceData + RECAST<IMAGE_DOS_HEADER*>(pSourceData)->e_lfanew);
	pOriginalOptionalHeader = &pOriginalNTHeader->OptionalHeader;
	pOriginalFileHeader		= &pOriginalNTHeader->FileHeader;

	if (pOriginalFileHeader->Machine != IMAGE_FILE_MACHINE_AMD64)
	{
		delete[] pSourceData;

		return ERROR_NO_X64FILE;
	}

	pTargetBase				= RECAST<BYTE*>(VirtualAllocEx(hProcess, RECAST<void*>(pOriginalOptionalHeader->ImageBase), pOriginalOptionalHeader->SizeOfImage, MEMORY_PERMISSIONS, PAGE_EXECUTE_READWRITE));
	if (!pTargetBase)
		pTargetBase = RECAST<BYTE*>(VirtualAllocEx(hProcess, nullptr, pOriginalOptionalHeader->SizeOfImage, MEMORY_PERMISSIONS, PAGE_EXECUTE_READWRITE));

	if (!pTargetBase)
	{
		delete[] pSourceData;
		LastError = GetLastError();

		return ERROR_CANT_ALLOC_MEM;
	}

	pLocalBase				= RECAST<BYTE*>(VirtualAlloc(nullptr, pOriginalOptionalHeader->SizeOfImage, MEMORY_PERMISSIONS, PAGE_EXECUTE_READWRITE));
	if (!pLocalBase)
	{
		delete[] pSourceData;
		LastError = GetLastError();
		VirtualFreeEx(hProcess, pTargetBase, 0, MEM_RELEASE);

		return ERROR_OUT_OF_MEMORY;
	}

	memset(pLocalBase, 0, pOriginalOptionalHeader->SizeOfImage);
	memcpy(pLocalBase, pSourceData, 0x1000 /* 4096 bytes is the size the PE Header */);

	auto * pSectionHeader	= IMAGE_FIRST_SECTION(pOriginalNTHeader);
	UINT i;
	for (i = 0; i < pOriginalFileHeader->NumberOfSections; ++i, ++pSectionHeader)
		if (pSectionHeader->SizeOfRawData)
			memcpy(pLocalBase + pSectionHeader->VirtualAddress, pSourceData + pSectionHeader->PointerToRawData, pSectionHeader->SizeOfRawData);

	BYTE * LocationDelta	= pTargetBase - pOriginalOptionalHeader->ImageBase;
	if (LocationDelta)
	{
		if (!pOriginalOptionalHeader->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].Size)
		{
			VirtualFreeEx(hProcess, pTargetBase, 0, MEM_RELEASE);
			VirtualFree(pLocalBase, 0, MEM_RELEASE);
			delete[] pSourceData;

			return ERROR_IMAGE_CANT_RELOC;
		}

		auto * pRelocationData = RECAST<IMAGE_BASE_RELOCATION*>(pLocalBase + pOriginalOptionalHeader->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].VirtualAddress);
		while (pRelocationData->VirtualAddress)
		{
			WORD * pRelativeInfo = RECAST<WORD*>(pRelocationData + 1);
			for (UINT i = 0; i < ((pRelocationData->SizeOfBlock - sizeof(IMAGE_BASE_RELOCATION)) / 2); ++i, ++pRelativeInfo)
			{
				if ((*pRelativeInfo >> 0x0C /* 12 */) == IMAGE_REL_BASED_HIGHLOW)
				{
					DWORD * pPatch = RECAST<DWORD*>(pLocalBase + pRelocationData->VirtualAddress + ((*pRelativeInfo) & 0xFFF)); /* 4095 bytess + Address of relocated image */
					*pPatch += RECAST<DWORD>(LocationDelta);
				}
			}

			pRelocationData = RECAST<IMAGE_BASE_RELOCATION*>(RECAST<BYTE*>(pRelocationData) + pRelocationData->SizeOfBlock);
		}
	}

	RECAST<MANUAL_MAPPING_DATA*>(pLocalBase)->pLoadLibrary = LoadLibraryA;
	RECAST<MANUAL_MAPPING_DATA*>(pLocalBase)->pGetProcAddress = RECAST<fGetProcAddress>(GetProcAddress);

	BOOL bResult = WriteProcessMemory(hProcess, pTargetBase, pLocalBase, pOriginalOptionalHeader->SizeOfImage, nullptr);
	if (!bResult)
	{
		LastError = GetLastError();
		VirtualFree(pLocalBase, 0, MEM_RELEASE);
		delete[] pSourceData;

		return ERROR_WPM_FAIL;
	}

	VirtualFree(pLocalBase, 0, MEM_RELEASE);
	delete[] pSourceData;

	ULONG_PTR FunctionSize = 0x600; /* 1536 bytes */
	void * pFunction = VirtualAllocEx(hProcess, nullptr, FunctionSize, MEMORY_PERMISSIONS, PAGE_EXECUTE_READWRITE);
	if (!pFunction)
	{
		LastError = GetLastError();

		return ERROR_CANT_ALLOC_MEM;
	}

	if (!WriteProcessMemory(hProcess, pFunction, ImportTLSExecute, FunctionSize, nullptr))
	{
		LastError = GetLastError();
		VirtualFreeEx(hProcess, pFunction, 0, MEM_RELEASE);

		return ERROR_WPM_FAIL;
	}

	HANDLE hThread = Start(hProcess, pFunction, pTargetBase, bUseExistingThread, false);

	if (!hThread)
	{
		VirtualFreeEx(hProcess, pFunction, 0, MEM_RELEASE);
		VirtualFreeEx(hProcess, pTargetBase, 0, MEM_RELEASE);

		return ERROR_CANT_CREATE_THREAD;
	}
	else if (!bUseExistingThread)
	{
		WaitForSingleObject(hThread, INFINITE);
		CloseHandle(hThread);
	}

	VirtualFreeEx(hProcess, pFunction, 0, MEM_RELEASE);

	return ERROR_SUCCESS;
}

DWORD Hide(unsigned char * source, HANDLE hProcess, DWORD dwFlags)
{
	if (!dwFlags)
		return ERROR_SUCCESS;

	if (dwFlags > ALL_FLAGS)
		return ERROR_INVALID_FLAGS;

	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, GetProcessId(hProcess));
	if (!hSnapshot)
	{
		LastError = GetLastError();

		return ERROR_TH32_FAIL;
	}

	MODULEENTRY32 ModuleEntry32{ 0 };
	ModuleEntry32.dwSize = sizeof(MODULEENTRY32);
	HINSTANCE hModule = 0;

	BOOL bResult = Module32First(hSnapshot, &ModuleEntry32);
	while (bResult)
	{
		char Buffer[MAX_PATH]{ 0 };
		GetModuleFileNameExA(hProcess, RECAST<HINSTANCE>(ModuleEntry32.modBaseAddr), Buffer, MAX_PATH);
		if (Buffer[3] == source[3] && !lstrcmpA((const char*)source, Buffer))
		{
			hModule = ModuleEntry32.hModule;
			break;
		}

		bResult = Module32Next(hSnapshot, &ModuleEntry32);
	}

	CloseHandle(hSnapshot);

	if (!bResult || !hModule)
		return ERROR_CANT_FIND_MOD;

	if (dwFlags & FAKE_HEADER)
	{
		void * pKernel32 = RECAST<void*>(GetModuleHandleA("kernel32.dll"));
		DWORD dwOriginal = 0;

		BOOL bResult = VirtualProtectEx(hProcess, hModule, 0x1000 /* 4096 */, PAGE_EXECUTE_READWRITE, &dwOriginal);
		if (!bResult)
		{
			LastError = GetLastError();

			return ERROR_VPE_FAIL;
		}

		bResult = WriteProcessMemory(hProcess, hModule, pKernel32, 0x1000 /* 4096 */, nullptr);
		if (!bResult)
		{
			LastError = GetLastError();

			return ERROR_WPM_FAIL;
		}

		bResult = VirtualProtectEx(hProcess, hModule, 0x1000 /* 4096 */, dwOriginal, &dwOriginal);
		if (!bResult)
		{
			LastError = GetLastError();

			return ERROR_VPE_FAIL;
		}
	}
	else if (dwFlags & ERASE_HEADER)
	{
		BYTE Buffer[0x1000 /* 4096 */]{ 0 };
		DWORD dwOriginal = 0; BOOL bResult = VirtualProtectEx(hProcess, hModule, 0x1000 /* 4096 */, PAGE_EXECUTE_READWRITE, &dwOriginal);
		if (!bResult)
		{
			LastError = GetLastError();

			return ERROR_VPE_FAIL;
		}

		bResult = WriteProcessMemory(hProcess, hModule, Buffer, 0x1000 /* 4096 */, nullptr);
		if (!bResult)
		{
			LastError = GetLastError();

			return ERROR_WPM_FAIL;
		}
	}

	if (dwFlags & UNLINK_FROM_PEB)
	{
		PEB * pPEB = GetPEBlock(hProcess);
		if (!pPEB)
			return ERROR_CANT_GET_PEB;

		PEB	peb;
		if (!ReadProcessMemory(hProcess, pPEB, &peb, sizeof(PEB), nullptr))
		{
			LastError = GetLastError();

			return ERROR_CANT_ACCESS_PEB;
		}

		PEB_LDR_DATA ldrdata;
		if (!ReadProcessMemory(hProcess, peb.Ldr, &ldrdata, sizeof(PEB_LDR_DATA), nullptr))
		{
			LastError = GetLastError();

			return ERROR_CANT_ACCESS_PEB_LDR;
		}

		LOADER_DATA_TABLE_ENTRY * pCurrentEntry = RECAST<LOADER_DATA_TABLE_ENTRY*>(ldrdata.InLoadOrderModuleListHead.Flink);
		LOADER_DATA_TABLE_ENTRY * pLastEntry	= RECAST<LOADER_DATA_TABLE_ENTRY*>(ldrdata.InLoadOrderModuleListHead.Blink);

		while (true)
		{
			LOADER_DATA_TABLE_ENTRY CurrentEntry;
			ReadProcessMemory(hProcess, pCurrentEntry, &CurrentEntry, sizeof(LOADER_DATA_TABLE_ENTRY), nullptr);

			if (CurrentEntry.DllBase == hModule)
			{
				LIST_ENTRY flink;
				LIST_ENTRY blink;

				ReadProcessMemory(hProcess, CurrentEntry.InLoadOrder.Flink, &flink, sizeof(LIST_ENTRY), nullptr);
				ReadProcessMemory(hProcess, CurrentEntry.InLoadOrder.Blink, &blink, sizeof(LIST_ENTRY), nullptr);

				flink.Blink = CurrentEntry.InLoadOrder.Blink;
				blink.Flink = CurrentEntry.InLoadOrder.Flink;

				WriteProcessMemory(hProcess, CurrentEntry.InLoadOrder.Flink, &flink, sizeof(LIST_ENTRY), nullptr);
				WriteProcessMemory(hProcess, CurrentEntry.InLoadOrder.Blink, &blink, sizeof(LIST_ENTRY), nullptr);

				ReadProcessMemory(hProcess, CurrentEntry.InMemoryOrder.Flink, &flink, sizeof(LIST_ENTRY), nullptr);
				ReadProcessMemory(hProcess, CurrentEntry.InMemoryOrder.Blink, &blink, sizeof(LIST_ENTRY), nullptr);

				flink.Blink = CurrentEntry.InMemoryOrder.Blink;
				blink.Flink = CurrentEntry.InMemoryOrder.Flink;

				WriteProcessMemory(hProcess, CurrentEntry.InMemoryOrder.Flink, &flink, sizeof(LIST_ENTRY), nullptr);
				WriteProcessMemory(hProcess, CurrentEntry.InMemoryOrder.Blink, &blink, sizeof(LIST_ENTRY), nullptr);

				ReadProcessMemory(hProcess, CurrentEntry.InInitOrder.Flink, &flink, sizeof(LIST_ENTRY), nullptr);
				ReadProcessMemory(hProcess, CurrentEntry.InInitOrder.Blink, &blink, sizeof(LIST_ENTRY), nullptr);

				flink.Blink = CurrentEntry.InInitOrder.Blink;
				blink.Flink = CurrentEntry.InInitOrder.Flink;

				WriteProcessMemory(hProcess, CurrentEntry.InInitOrder.Flink, &flink, sizeof(LIST_ENTRY), nullptr);
				WriteProcessMemory(hProcess, CurrentEntry.InInitOrder.Blink, &blink, sizeof(LIST_ENTRY), nullptr);

				BYTE Buffer[MAX_PATH * 2]{ 0 };
				WriteProcessMemory(hProcess, CurrentEntry.BaseDllName.szBuffer, Buffer, CurrentEntry.BaseDllName.MaxLength, nullptr);
				WriteProcessMemory(hProcess, CurrentEntry.FullDllName.szBuffer, Buffer, CurrentEntry.FullDllName.MaxLength, nullptr);
				WriteProcessMemory(hProcess, pCurrentEntry, Buffer, sizeof(LOADER_DATA_TABLE_ENTRY), nullptr);

				return ERROR_SUCCESS;
			}

			if (pCurrentEntry == pLastEntry)
			{
				LastError = ERROR_ADV_CANT_FIND_MODULE;

				return ERROR_CANT_FIND_MOD_PEB;
			}

			pCurrentEntry = RECAST<LOADER_DATA_TABLE_ENTRY*>(CurrentEntry.InLoadOrder.Flink);
		}
	}

	return ERROR_SUCCESS;
}

UINT inline stringLengthA(const char * szString)
{
	UINT result = 0;
	for (; *szString++; result++);

	return result;
}

void inline ClearMemory(BYTE * pMemory, UINT size)
{
	for (BYTE * i = pMemory; i < pMemory + size; ++i)
		*i = 0x00;
}

void stdcall ImportTLSExecute(MANUAL_MAPPING_DATA * pData)
{
	BYTE * pBase			= RECAST<BYTE*>(pData);
	auto * pOptional		= &RECAST<IMAGE_NT_HEADERS*>(pBase + RECAST<IMAGE_DOS_HEADER*>(pData)->e_lfanew)->OptionalHeader;
	auto _LoadLibraryA		= pData->pLoadLibrary;
	auto _GetProcAddress	= pData->pGetProcAddress;
	auto _DllMain			= RECAST<fDLL_ENTRY_POINT>(pBase + pOptional->AddressOfEntryPoint);

	if (pOptional->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].Size)
	{
		auto * pImportDescriptor = RECAST<IMAGE_IMPORT_DESCRIPTOR*>(pBase + pOptional->DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);
		while (pImportDescriptor->Name)
		{
			HINSTANCE hDll					= _LoadLibraryA(RECAST<const char*>(pBase + pImportDescriptor->Name));
			ULONG_PTR * pThunkReference		= RECAST<ULONG_PTR*>(pBase + pImportDescriptor->OriginalFirstThunk);
			ULONG_PTR * pFunctionReference	= RECAST<ULONG_PTR*>(pBase + pImportDescriptor->FirstThunk);

			ClearMemory(pBase + pImportDescriptor->Name, stringLengthA(RECAST<char*>(pBase + pImportDescriptor->Name)));

			if (!pImportDescriptor->OriginalFirstThunk)
				pThunkReference = pFunctionReference;

			for (; *pThunkReference; ++pThunkReference, ++pFunctionReference)
			{
				if (IMAGE_SNAP_BY_ORDINAL(*pThunkReference))
				{
					*pFunctionReference = _GetProcAddress(hDll, RECAST<const char*>(*pThunkReference & 0xFFFF /* 65535 */));

					ClearMemory(RECAST<BYTE*>(*pThunkReference & 0xFFFF /* 65535 */), stringLengthA(RECAST<char*>(*pThunkReference & 0xFFFF /* 65535 */)));
				}
				else
				{
					auto * pImport = RECAST<IMAGE_IMPORT_BY_NAME*>(pBase + (*pThunkReference));
					*pFunctionReference = _GetProcAddress(hDll, pImport->Name);

					ClearMemory(RECAST<BYTE*>(pImport->Name), stringLengthA(pImport->Name));
				}
			}

			++pImportDescriptor;
		}
	}

	if (pOptional->DataDirectory[IMAGE_DIRECTORY_ENTRY_TLS].Size)
	{
		auto * pTLS			= RECAST<IMAGE_TLS_DIRECTORY*>(pBase + pOptional->DataDirectory[IMAGE_DIRECTORY_ENTRY_TLS].VirtualAddress);
		auto * pCallback	= RECAST<PIMAGE_TLS_CALLBACK*>(pTLS->AddressOfCallBacks);

		for (; pCallback && *pCallback; ++pCallback)
			(*pCallback)(pBase, DLL_PROCESS_ATTACH, nullptr);
	}

	_DllMain(pBase, DLL_PROCESS_ATTACH, nullptr);

	for (UINT i = 0; i <= IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR; ++i)
	{
		if (i == IMAGE_DIRECTORY_ENTRY_IAT)
			continue;

		DWORD size = pOptional->DataDirectory[i].Size;

		if (size)
			ClearMemory(pBase + pOptional->DataDirectory[i].VirtualAddress, size);
	}

	for (UINT i = 0; i != 0x1000; i += sizeof(ULONG64))
		*RECAST<ULONG64*>(pBase + i) = 0;
}

HANDLE Start(HANDLE hTargetProcess, void * pRoutine, void * pArg, bool bUseExistingThread, bool bUseFastCall)
{
	if (!bUseExistingThread)
	{
		auto NtCreateThreadEx = RECAST<mNtCreateThreadEx>(GetProcAddress(GetModuleHandleA("ntdll.dll"), "NtCreateThreadEx"));
		if (!NtCreateThreadEx)
		{
			HANDLE hThread = CreateRemoteThreadEx(hTargetProcess, nullptr, 0, RECAST<LPTHREAD_START_ROUTINE>(pRoutine), pArg, 0, nullptr, nullptr);
			if (!hThread)
				LastError = GetLastError();

			return hThread;
		}

		HANDLE hThread = nullptr;

		NtCreateThreadEx(&hThread, THREAD_ALL_ACCESS, nullptr, hTargetProcess, pRoutine, pArg, 0, 0, 0, 0, nullptr);
		if (!hThread)
			LastError = GetLastError();

		return hThread;
	}

	DWORD dwProcessId = GetProcessId(hTargetProcess);
	if (!dwProcessId)
	{
		LastError = ERROR_ADV_INV_PROC;
		return nullptr;
	}

	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	if (!hSnapshot)
	{
		LastError = GetLastError();
		return nullptr;
	}

	THREADENTRY32 ThreadEntry32 = { 0 };
	ThreadEntry32.dwSize = sizeof(THREADENTRY32);

	BOOL bResult = Thread32First(hSnapshot, &ThreadEntry32);
	while (bResult)
	{
		if (ThreadEntry32.th32OwnerProcessID == dwProcessId && ThreadEntry32.th32ThreadID != GetCurrentThreadId())
			break;

		bResult = Thread32Next(hSnapshot, &ThreadEntry32);
	}

	CloseHandle(hSnapshot);

	if (!bResult)
	{
		LastError = ERROR_ADV_NO_THREADS;

		return nullptr;
	}

	HANDLE hThread = OpenThread(THREAD_ALL_ACCESS, FALSE, ThreadEntry32.th32ThreadID);
	if (!hThread)
	{
		LastError = ERROR_ADV_CANT_OPEN_THREAD;

		return nullptr;
	}

	if (SuspendThread(hThread) == (DWORD)-1)
	{
		LastError = ERROR_ADV_SUSPEND_FAIL;
		CloseHandle(hThread);

		return nullptr;
	}

	CONTEXT originalContext;
	originalContext.ContextFlags = CONTEXT_CONTROL;
	if (!GetThreadContext(hThread, &originalContext))
	{
		LastError = ERROR_ADV_GET_CONTEXT_FAIL;
		ResumeThread(hThread);
		CloseHandle(hThread);

		return nullptr;
	}

	void * pCave = VirtualAllocEx(hTargetProcess, nullptr, 0x100 /* 256 */, MEMORY_PERMISSIONS, PAGE_EXECUTE_READWRITE);
	if (!pCave)
	{
		LastError = ERROR_ADV_OUT_OF_MEMORY;
		ResumeThread(hThread);
		CloseHandle(hThread);

		return nullptr;
	}

	bUseFastCall = true;

	BYTE shell[] =
	{
		0x48, 0x83, 0xEC, 0x08,
		0xC7, 0x04, 0x24, 0x00, 0x00, 0x00, 0x00,
		0xC7, 0x44, 0x24, 0x04, 0x00, 0x00, 0x00, 0x00,
		0x50, 0x51, 0x52, 0x53, 0x41, 0x50, 0x41, 0x51, 0x41, 0x52, 0x41, 0x53,
		0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x48, 0x83, 0xEC, 0x20,
		0xFF, 0xD3,
		0x48, 0x83, 0xC4, 0x20,
		0x41, 0x5B, 0x41, 0x5A, 0x41, 0x59, 0x41, 0x58, 0x5B, 0x5A, 0x59, 0x58,
		0xC6, 0x05, 0xB0, 0xFF, 0xFF, 0xFF, 0x00,
		0xC3
	};

	DWORD dwLoRIP = (DWORD)(originalContext.Rip & 0xFFFFFFFF /* 4294967295 */);
	DWORD dwHiRIP = (DWORD)((originalContext.Rip >> 0x20) & 0xFFFFFFFF /* 4294967295 */);

	*RECAST<DWORD*>(shell + 0x07)			= dwLoRIP;
	*RECAST<DWORD*>(shell + 0x0F /* 15 */)	= dwHiRIP;
	*RECAST<void**>(shell + 0x21 /* 33 */)	= pRoutine;
	*RECAST<void**>(shell + 0x2B /* 43 */)	= pArg;

	originalContext.Rip = RECAST<DWORD64>(pCave);

	if (!WriteProcessMemory(hTargetProcess, pCave, shell, sizeof(shell), nullptr))
	{
		LastError = ERROR_ADV_WPM_FAIL;

		VirtualFreeEx(hTargetProcess, pCave, MEM_RELEASE, 0);
		ResumeThread(hThread);
		CloseHandle(hThread);

		return nullptr;
	}

	if (!SetThreadContext(hThread, &originalContext))
	{
		LastError = ERROR_ADV_SET_CONTEXT_FAIL;

		VirtualFreeEx(hTargetProcess, pCave, MEM_RELEASE, 0);
		ResumeThread(hThread);
		CloseHandle(hThread);

		return nullptr;
	}

	if (ResumeThread(hThread) == (DWORD)-1)
	{
		LastError = ERROR_ADV_RESUME_FAIL;

		VirtualFreeEx(hTargetProcess, pCave, MEM_RELEASE, 0);
		CloseHandle(hThread);

		return nullptr;
	}

	BYTE CheckByte = 1;
	while (CheckByte)
		ReadProcessMemory(hTargetProcess, pCave, &CheckByte, 1, nullptr);

	CloseHandle(hThread);
	VirtualFreeEx(hTargetProcess, pCave, MEM_RELEASE, 0);

	return (HANDLE)1;
}

PEB * GetPEBlock(HANDLE hProcess)
{
	auto _NtQIP = RECAST<mNtQueryInformationProcess>(GetProcAddress(GetModuleHandleA("ntdll.dll"), "NtQueryInformationProcess"));
	if (!_NtQIP)
	{
		LastError = ERROR_ADV_QIP_MISSING;

		return nullptr;
	}

	PROCESS_BASIC_INFORMATION PBI{ 0 };
	ULONG SizeOut = 0;
	if (_NtQIP(hProcess, PROCESS_INFO_CLASS::ProcessBasicInformation, &PBI, sizeof(PROCESS_BASIC_INFORMATION), &SizeOut) < 0)
	{
		LastError = ERROR_ADV_QIP_FAIL;

		return nullptr;
	}

	return PBI.pPEB;
}

bool FileExistsA(const char * szFile)
{
	return (GetFileAttributesA(szFile) != INVALID_FILE_ATTRIBUTES);
}