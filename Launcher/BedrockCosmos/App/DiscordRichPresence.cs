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
            CosmosConsole.WriteLine("Started Discord rich presence.");
        }

        internal static void DisposeRpc()
        {
            client.Dispose();
            CosmosConsole.WriteLine("Stopped Discord rich presence.");
        }

        internal static void UpdatePresence()
        {
            client.SetPresence(new RichPresence()
            {
                Details = "Using Custom Capes & Skins",
                State = "On Minecraft Bedrock Edition",
                Buttons = new Button[]
                {
                    new Button() { Label = "Website", Url = "https://bedrock-cosmos.app/" },
                    new Button() { Label = "Discord Server", Url = "https://discord.gg/HRG2NapegP" }
                },
                Assets = new Assets()
                {
                    LargeImageKey = "minecraft-bedrock",
                    LargeImageText = "Minecraft Bedrock Edition",
                    SmallImageKey = "bedrock-cosmos",
                    SmallImageText = "Bedrock Cosmos"
                }
            });
        }
    }
}
