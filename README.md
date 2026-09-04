# Jellyfin Plugin - NguonC (Phim HD)

A Jellyfin channel plugin that integrates with the [phim.nguonc.com](https://phim.nguonc.com) API to stream Vietnamese movies and TV shows.

## Features

- 🔍 **Browse Films**: Browse latest films, categories, genres, countries, and years
- 🎬 **Watch Episodes**: Stream episodes with multiple server options
- 🔎 **Search**: Search for films by keyword
- 📱 **Multi-Server Support**: Choose from Vietsub, Thuyết Minh, or Lồng Tiếng options
- ⚙️ **Configurable**: Customize API settings and preferences

## Installation

### Method 1: Manual Installation (Recommended)

1. Download the latest release from [Releases](https://github.com/yourusername/Jellyfin.Plugin.NguonC/releases)
2. Copy `Jellyfin.Plugin.NguonC.dll` to your Jellyfin plugins directory:
   - **Windows**: `%ProgramData%\Jellyfin\Server\plugins\`
   - **Linux**: `/var/lib/jellyfin/plugins/`
   - **Docker**: `/config/plugins/`
3. Restart Jellyfin server
4. Enable the plugin in Dashboard → Plugins

### Method 2: Build from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/Jellyfin.Plugin.NguonC.git
cd Jellyfin.Plugin.NguonC

# Build the plugin
dotnet build -c Release

# Copy the built DLL to Jellyfin plugins directory
cp bin/Release/net8.0/Jellyfin.Plugin.NguonC.dll /path/to/jellyfin/plugins/
```

## Configuration

1. Go to **Dashboard → Plugins → NguonC**
2. Configure the following settings:
   - **API Base URL**: Default is `https://phim.nguonc.com/api`
   - **Items Per Page**: Number of items to display (default: 24)
   - **Preferred Language**: Choose your preferred subtitle/dubbing option
3. Click **Save** to apply settings

## Usage

1. In Jellyfin, go to **Libraries → Channels**
2. Click on **NguonC - Phim HD**
3. Browse by categories:
   - **Phim Mới Cập Nhật**: Latest updated films
   - **Đang Chiếu**: Now showing
   - **Hoàn Tất**: Completed series
   - **Genres**: Action, Romance, Comedy, Horror, etc.
   - **Countries**: China, Korea, Japan, USA, Thailand
   - **Years**: 2024, 2025, 2026
4. Click on a film to see episode list
5. Select a server (Vietsub, Thuyết Minh, etc.)
6. Click an episode to start streaming

## API Endpoints

The plugin exposes these API endpoints through the Jellyfin server:

- `GET /NguonC/films?page={page}` - Get film list
- `GET /NguonC/film/{slug}` - Get film detail with episodes
- `GET /NguonC/search?keyword={keyword}` - Search films
- `GET /NguonC/embed?url={embedUrl}` - Proxy embed page (extracts stream data)
- `GET /NguonC/stream?url={streamUrl}` - Proxy stream for playback
- `GET /NguonC/resolve?url={embedUrl}` - Resolve embed URL to stream URL

## Requirements

- Jellyfin Server 10.10.x or later
- .NET 8.0 runtime
- Internet connection to access phim.nguonc.com API

## Troubleshooting

### Plugin not showing up
- Ensure the DLL is in the correct plugins directory
- Check Jellyfin logs for any startup errors
- Restart the Jellyfin server

### Videos not playing
- Check if the API is accessible from your server
- Verify network settings and firewall rules
- Check Jellyfin logs for stream proxy errors

### Slow loading
- Try adjusting the Items Per Page setting
- Check your network connection to phim.nguonc.com

## License

MIT License - See [LICENSE](LICENSE) for details

## Credits

- [phim.nguonc.com](https://phim.nguonc.com) - Movie data source
- [Jellyfin](https://jellyfin.org) - Open source media system
