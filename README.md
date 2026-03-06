# Launcher
Launcher for the Bedrock Cosmos local proxy

# Dependencies
Bedrock Cosmos is dependent on several packages for functionality.

**NuGet Packages:**
- Discord RPC C# - Used to display a Discord activity for the launcher.
- Newtonsoft Json - Used to handle response queries and other Json data.
- Titanium Web Proxy - Allows for the decryption and modification of web traffic. Reference is included in the source code to fix an exception thrown while debugging with the latest NuGet Package.

**References:**
- System.IO.Compression.FileSystem - Used for extracting the contents of Zip files.
