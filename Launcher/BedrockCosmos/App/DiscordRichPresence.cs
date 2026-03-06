using DiscordRPC;

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
                    new Button() { Label = "Discord Server", Url = "https://discord.com/invite/DSbyeN5T" }
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
