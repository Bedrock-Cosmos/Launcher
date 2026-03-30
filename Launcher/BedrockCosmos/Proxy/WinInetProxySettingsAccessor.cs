using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BedrockCosmos.Proxy
{
    public sealed class WinInetProxySettingsAccessor : IInternetProxySettingsAccessor
    {
        private const int InternetOptionPerConnectionOption = 75;
        private const int InternetOptionRefresh = 37;
        private const int InternetOptionSettingsChanged = 39;
        private const int InternetOptionProxySettingsChanged = 95;

        private const int InternetPerConnFlags = 1;
        private const int InternetPerConnProxyServer = 2;
        private const int InternetPerConnProxyBypass = 3;
        private const int InternetPerConnAutoConfigUrl = 4;
        private const int InternetPerConnFlagsUi = 10;

        internal const int ProxyTypeDirect = 0x00000001;
        internal const int ProxyTypeProxy = 0x00000002;
        internal const int ProxyTypeAutoProxyUrl = 0x00000004;
        internal const int ProxyTypeAutoDetect = 0x00000008;

        public ProxySettingsSnapshot ReadCurrentSettings()
        {
            try
            {
                return QuerySettings(InternetPerConnFlagsUi);
            }
            catch
            {
                return QuerySettings(InternetPerConnFlags);
            }
        }

        public void ApplySettings(ProxySettingsSnapshot settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            IntPtr optionsPointer = IntPtr.Zero;
            IntPtr listPointer = IntPtr.Zero;
            IntPtr proxyServerPointer = IntPtr.Zero;
            IntPtr proxyBypassPointer = IntPtr.Zero;
            IntPtr autoConfigPointer = IntPtr.Zero;

            try
            {
                var options = new INTERNET_PER_CONN_OPTION[4];
                options[0].dwOption = InternetPerConnFlags;
                options[0].Value.dwValue = settings.Flags;

                options[1].dwOption = InternetPerConnProxyServer;
                proxyServerPointer = StringToPointer(settings.ProxyServer);
                options[1].Value.pszValue = proxyServerPointer;

                options[2].dwOption = InternetPerConnProxyBypass;
                proxyBypassPointer = StringToPointer(settings.ProxyBypass);
                options[2].Value.pszValue = proxyBypassPointer;

                options[3].dwOption = InternetPerConnAutoConfigUrl;
                autoConfigPointer = StringToPointer(settings.AutoConfigUrl);
                options[3].Value.pszValue = autoConfigPointer;

                int optionSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
                optionsPointer = Marshal.AllocCoTaskMem(optionSize * options.Length);

                for (int index = 0; index < options.Length; index++)
                {
                    IntPtr target = IntPtr.Add(optionsPointer, index * optionSize);
                    Marshal.StructureToPtr(options[index], target, false);
                }

                var list = new INTERNET_PER_CONN_OPTION_LIST
                {
                    dwSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST)),
                    pszConnection = IntPtr.Zero,
                    dwOptionCount = options.Length,
                    dwOptionError = 0,
                    pOptions = optionsPointer
                };

                listPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST)));
                Marshal.StructureToPtr(list, listPointer, false);

                int size = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));
                if (!InternetSetOption(IntPtr.Zero, InternetOptionPerConnectionOption, listPointer, size))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, InternetOptionProxySettingsChanged, IntPtr.Zero, 0);
                InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
            }
            finally
            {
                FreePointer(proxyServerPointer);
                FreePointer(proxyBypassPointer);
                FreePointer(autoConfigPointer);

                if (listPointer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(listPointer);

                if (optionsPointer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(optionsPointer);
            }
        }

        private ProxySettingsSnapshot QuerySettings(int flagsOption)
        {
            IntPtr optionsPointer = IntPtr.Zero;
            IntPtr listPointer = IntPtr.Zero;

            try
            {
                var options = new INTERNET_PER_CONN_OPTION[4];
                options[0].dwOption = flagsOption;
                options[1].dwOption = InternetPerConnProxyServer;
                options[2].dwOption = InternetPerConnProxyBypass;
                options[3].dwOption = InternetPerConnAutoConfigUrl;

                int optionSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
                optionsPointer = Marshal.AllocCoTaskMem(optionSize * options.Length);

                for (int index = 0; index < options.Length; index++)
                {
                    IntPtr target = IntPtr.Add(optionsPointer, index * optionSize);
                    Marshal.StructureToPtr(options[index], target, false);
                }

                var list = new INTERNET_PER_CONN_OPTION_LIST
                {
                    dwSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST)),
                    pszConnection = IntPtr.Zero,
                    dwOptionCount = options.Length,
                    dwOptionError = 0,
                    pOptions = optionsPointer
                };

                listPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST)));
                Marshal.StructureToPtr(list, listPointer, false);

                int size = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));
                if (!InternetQueryOption(IntPtr.Zero, InternetOptionPerConnectionOption, listPointer, ref size))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                var snapshot = new ProxySettingsSnapshot
                {
                    Flags = ReadIntOption(optionsPointer, 0),
                    ProxyServer = ReadStringOption(optionsPointer, 1),
                    ProxyBypass = ReadStringOption(optionsPointer, 2),
                    AutoConfigUrl = ReadStringOption(optionsPointer, 3)
                };

                snapshot.RefreshClassification();
                return snapshot;
            }
            finally
            {
                FreeQueryString(optionsPointer, 1);
                FreeQueryString(optionsPointer, 2);
                FreeQueryString(optionsPointer, 3);

                if (listPointer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(listPointer);

                if (optionsPointer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(optionsPointer);
            }
        }

        private static int ReadIntOption(IntPtr optionsPointer, int index)
        {
            var option = ReadOption(optionsPointer, index);
            return option.Value.dwValue;
        }

        private static string ReadStringOption(IntPtr optionsPointer, int index)
        {
            var option = ReadOption(optionsPointer, index);
            if (option.Value.pszValue == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringUni(option.Value.pszValue);
        }

        private static INTERNET_PER_CONN_OPTION ReadOption(IntPtr optionsPointer, int index)
        {
            int optionSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
            IntPtr optionPointer = IntPtr.Add(optionsPointer, index * optionSize);
            return (INTERNET_PER_CONN_OPTION)Marshal.PtrToStructure(optionPointer, typeof(INTERNET_PER_CONN_OPTION));
        }

        private static void FreeQueryString(IntPtr optionsPointer, int index)
        {
            if (optionsPointer == IntPtr.Zero)
                return;

            var option = ReadOption(optionsPointer, index);
            if (option.Value.pszValue != IntPtr.Zero)
                GlobalFree(option.Value.pszValue);
        }

        private static IntPtr StringToPointer(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? IntPtr.Zero
                : Marshal.StringToHGlobalUni(value);
        }

        private static void FreePointer(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
                Marshal.FreeHGlobal(pointer);
        }

        // Force the Unicode entry points because this accessor allocates and reads UTF-16 buffers explicitly.
        [DllImport("wininet.dll", EntryPoint = "InternetQueryOptionW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool InternetQueryOption(
            IntPtr hInternet,
            int dwOption,
            IntPtr lpBuffer,
            ref int lpdwBufferLength);

        [DllImport("wininet.dll", EntryPoint = "InternetSetOptionW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool InternetSetOption(
            IntPtr hInternet,
            int dwOption,
            IntPtr lpBuffer,
            int dwBufferLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        [StructLayout(LayoutKind.Sequential)]
        private struct INTERNET_PER_CONN_OPTION_LIST
        {
            public int dwSize;
            public IntPtr pszConnection;
            public int dwOptionCount;
            public int dwOptionError;
            public IntPtr pOptions;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INTERNET_PER_CONN_OPTION
        {
            public int dwOption;
            public INTERNET_PER_CONN_OPTION_VALUE Value;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INTERNET_PER_CONN_OPTION_VALUE
        {
            [FieldOffset(0)]
            public int dwValue;

            [FieldOffset(0)]
            public IntPtr pszValue;

            [FieldOffset(0)]
            public System.Runtime.InteropServices.ComTypes.FILETIME ftValue;
        }
    }
}
