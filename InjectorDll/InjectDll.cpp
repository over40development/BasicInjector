// InjectDll.cpp


#include "stdafx.h"

DWORD dwError = ERROR_SUCCESS;

bool SetPrivilegeA(const char * szPrivilege, bool bState = true)
{
	HANDLE hToken = nullptr;
	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY | TOKEN_ADJUST_PRIVILEGES, &hToken))
	{
		dwError = GetLastError();

		return false;
	}

	TOKEN_PRIVILEGES TokenPrivileges = { 0 };
	TokenPrivileges.PrivilegeCount = 1;
	TokenPrivileges.Privileges[0].Attributes = bState ? SE_PRIVILEGE_ENABLED : 0;

	if (!LookupPrivilegeValueA(nullptr, szPrivilege, &TokenPrivileges.Privileges[0].Luid))
	{
		dwError = GetLastError();
		CloseHandle(hToken);

		return false;
	}

	if (!AdjustTokenPrivileges(hToken, FALSE, &TokenPrivileges, sizeof(TOKEN_PRIVILEGES), nullptr, nullptr))
	{
		dwError = GetLastError();
		CloseHandle(hToken);

		return false;
	}

	CloseHandle(hToken);

	return true;
}

bool Map(unsigned char * source, UINT size, UINT processId) {
	bool bUseExistingThread = false;
	bool bUnlink = false;

	DWORD dwFlags = 0;
	DWORD dwError = 0;
	DWORD dwResult = 0;
	DWORD dwHeaderOption = 0;

	if (!source)
		return false;

	if (bUnlink)
		dwFlags = UNLINK_FROM_PEB;

	dwFlags |= dwHeaderOption;

	if (!SetPrivilegeA("SeDebugPrivilege", true))
		return false;

	if (!processId)
		return false;

	HANDLE hProcess = OpenProcess(
		PROCESS_CREATE_THREAD | 
		PROCESS_QUERY_INFORMATION | 
		PROCESS_VM_READ | 
		PROCESS_VM_WRITE | 
		PROCESS_VM_OPERATION, FALSE, processId);

	if (!hProcess)
		return false;

	dwResult = Inject(source, size, hProcess, bUseExistingThread, dwFlags, &dwError);

	CloseHandle(hProcess);

	if (dwResult)
		return false;

	return true;
}