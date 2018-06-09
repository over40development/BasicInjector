// WinNT.h


#pragma once

struct UNICODE_STRING
{
	WORD			Length;
	WORD			MaxLength;
	wchar_t *		szBuffer;
};

struct LOADER_DATA_TABLE_ENTRY
{
	LIST_ENTRY		InLoadOrder;
	LIST_ENTRY		InMemoryOrder;
	LIST_ENTRY		InInitOrder;
	void *			DllBase;
	void *			EntryPoint;
	ULONG			SizeOfImage;
	UNICODE_STRING	FullDllName;
	UNICODE_STRING	BaseDllName;
};

struct PEB_LDR_DATA
{
	ULONG			Length;
	BYTE			Initialized;
	HANDLE			SsHandle;
	LIST_ENTRY		InLoadOrderModuleListHead;
	LIST_ENTRY		InMemoryOrderModuleListHead;
	LIST_ENTRY		InInitializationOrderModuleListHead;
	void *			EntryInProgress;
	BYTE			ShutdownInProgress;
	HANDLE			ShutdownThreadId;
};

struct PEB
{
	void *			Reserved[3];
	PEB_LDR_DATA *	Ldr;
};

struct PROCESS_BASIC_INFORMATION
{
	NTSTATUS		ExitStatus;
	PEB *			pPEB;
	ULONG_PTR		AffinityMask;
	LONG			BasePriority;
	HANDLE			UniqueProcessId;
	HANDLE			InheritedFromUniqueProcessId;
};

enum PROCESS_INFO
{
	ProcessBasicInformation
};