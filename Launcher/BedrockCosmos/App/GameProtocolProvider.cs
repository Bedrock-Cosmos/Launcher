using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BedrockCosmos.App
{
    [ComImport, Guid("F58E3884-1F75-4C66-9127-A66161818693"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IGameProtocolProvider
    {
        int NotifyGameProtocolActivation(uint titleId, [MarshalAs(UnmanagedType.HString)] string uri, out int wasHandled);

        int HandleStateShare(uint titleId, [MarshalAs(UnmanagedType.HString)] string _1, [MarshalAs(UnmanagedType.HString)] string _2, out int _3);
    }

    [ComImport, Guid("DEA688F3-0625-45AB-AF1A-EFCF9BB440F6")]
    public class GameProtocolService : IGameProtocolProvider
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern int NotifyGameProtocolActivation(uint titleId, [MarshalAs(UnmanagedType.HString)] string uri, out int wasHandled);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern int HandleStateShare(uint titleId, [MarshalAs(UnmanagedType.HString)] string _1, [MarshalAs(UnmanagedType.HString)] string _2, out int _3);

    }
}
