# Launcher
The Bedrock Cosmos Launcher acts as a local proxy on your Windows device to add custom capes and skin packs to Minecraft Bedrock Edition! More documentation will be coming soon!

# Dependencies
Bedrock Cosmos is dependent on several packages for functionality.

**NuGet Packages:**
- [AutoUpdater NET](https://www.nuget.org/packages/Autoupdater.NET.Official) - Used for launcher updates.
- [Discord Rich Presence C#](https://www.nuget.org/packages/DiscordRichPresence) - Used to display a Discord activity for the launcher.
- [Newtonsoft Json](https://www.nuget.org/packages/Newtonsoft.Json) - Used to handle response queries and other Json data.
- [Titanium Web Proxy](https://www.nuget.org/packages/Titanium.Web.Proxy) - Allows for the decryption and modification of web traffic. *Reference is included in the source code to fix an exception thrown while debugging with the latest NuGet Package.*

**References:**
- System.IO.Compression.FileSystem - Used for extracting the contents of Zip files.
