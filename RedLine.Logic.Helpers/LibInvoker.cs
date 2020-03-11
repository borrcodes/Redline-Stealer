using System;
using System.Runtime.InteropServices;

namespace RedLine.Logic.Helpers
{
	public sealed class LibInvoker : IDisposable
	{
		private IntPtr SystemNetMailSmtpNtlmAuthenticationModuleC;

		public LibInvoker(string fileName)
		{
			SystemNetMailSmtpNtlmAuthenticationModuleC = NativeMethods.LoadLibrary(fileName);
			if (SystemNetMailSmtpNtlmAuthenticationModuleC == IntPtr.Zero)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		public TDelegate CastToDelegate<TDelegate>(string MicrosoftWinTimerElapsedEventHandlerKtionName) where TDelegate : class
		{
			IntPtr procAddress = NativeMethods.GetProcAddress(SystemNetMailSmtpNtlmAuthenticationModuleC, MicrosoftWinTimerElapsedEventHandlerKtionName);
			if (procAddress == IntPtr.Zero)
			{
				return null;
			}
			return Marshal.GetDelegateForFunctionPointer(procAddress, typeof(TDelegate)) as TDelegate;
		}

		public void Dispose()
		{
			if (SystemNetMailSmtpNtlmAuthenticationModuleC != IntPtr.Zero)
			{
				NativeMethods.FreeLibrary(SystemNetMailSmtpNtlmAuthenticationModuleC);
			}
		}
	}
}
