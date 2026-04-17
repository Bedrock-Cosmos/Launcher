using DiscordRPC;

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
    internal class DiscordRichPresence
    {
        private static DiscordRpcClient client;
        private static bool _isInitialized = false;

        internal static void InitializeRpc()
        {
            if (!_isInitialized)
            {
                client = new DiscordRpcClient("1477362317006999692");
                client.Initialize();
                CosmosConsole.WriteLine("Started Discord rich presence.");
                _isInitialized = true;
            }
        }

        internal static void DisposeRpc()
        {
            if (_isInitialized)
            {
                client.Dispose();
                CosmosConsole.WriteLine("Stopped Discord rich presence.");
                _isInitialized = false;
            }
        }

        internal static void UpdatePresence()
        {
            client.SetPresence(new RichPresence()
            {
                Details = LanguageHandler.Get("DiscordRpc.Details.Text"),
                State = LanguageHandler.Get("DiscordRpc.State.Text"),
                Buttons = new Button[]
                {
                    new Button() { Label = LanguageHandler.Get("DiscordRpc.Button.Website"), Url = "https://bedrock-cosmos.app/" },
                    new Button() { Label = LanguageHandler.Get("DiscordRpc.Button.Discord"), Url = "https://discord.gg/HRG2NapegP" }
                },
                Assets = new Assets()
                {
                    LargeImageKey = "minecraft-bedrock",
                    LargeImageText = LanguageHandler.Get("DiscordRpc.Assets.LargeImageText"),
                    SmallImageKey = "bedrock-cosmos",
                    SmallImageText = LanguageHandler.Get("DiscordRpc.Assets.SmallImageText")
                }
            });
        }

        internal static bool IsInitialized()
        {
            return _isInitialized;
        }
    }
}
