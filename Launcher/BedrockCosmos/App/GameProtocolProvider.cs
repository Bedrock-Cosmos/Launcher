using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// =============================================================================
// Bedrock Cosmos - Copyright (c) 2026
//
// This file is part of Bedrock Cosmos, licensed under the MIT License.
// You must read and agree to the terms of the MIT License before using,
// copying, modifying, or distributing this code.
//
// MIT License - Full terms: https://opensource.org/licenses/MIT
// =============================================================================

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
