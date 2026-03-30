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

        internal static void InitializeRpc()
        {
            client = new DiscordRpcClient("1477362317006999692");
            client.Initialize();
            CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DiscordPresenceStarted"));
        }

        internal static void DisposeRpc()
        {
            client.Dispose();
            CosmosConsole.WriteLine(LanguageHandler.Get("Logs.DiscordPresenceStopped"));
        }

        internal static void UpdatePresence()
        {
            client.SetPresence(new RichPresence()
            {
                Details = LanguageHandler.Get("Discord.Details"),
                State = LanguageHandler.Get("Discord.State"),
                Buttons = new Button[]
                {
                    new Button() { Label = LanguageHandler.Get("Discord.Button.Website"), Url = "https://bedrock-cosmos.app/" },
                    new Button() { Label = LanguageHandler.Get("Discord.Button.Discord"), Url = "https://discord.com/invite/DSbyeN5T" }
                },
                Assets = new Assets()
                {
                    LargeImageKey = "minecraft-bedrock",
                    LargeImageText = LanguageHandler.Get("Discord.Assets.LargeText"),
                    SmallImageKey = "bedrock-cosmos",
                    SmallImageText = LanguageHandler.Get("Discord.Assets.SmallText")
                }
            });
        }
    }
}
