# Jellyfin Plugin - NguonC (Phim HD)

A Jellyfin channel plugin that integrates with the [phim.nguonc.com](https://phim.nguonc.com) API to stream Vietnamese movies and TV shows.

## Features

- 🔍 **Browse Films**: Browse latest films, categories, genres, countries, and years
- 🎬 **Watch Episodes**: Stream episodes with multiple server options
- 🔎 **Search**: Search for films by keyword
- 📱 **Multi-Server Support**: Choose from Vietsub, Thuyết Minh, or Lồng Tiếng options
- ⚙️ **Configurable**: Customize API settings and preferences

## Installation

### Method 1: Jellyfin Repository (Easiest)

1. Open Jellyfin **Dashboard → Plugins → Repositories**
2. Click **"+"** to add a new repository:
   - **Repository Name:** `NguonC Plugin Repository`
   - **Repository URL:** `https://raw.githubusercontent.com/thanhdtr/jellyfin-plugin-nguonc/main/manifest.json`
3. Save, then go to **Dashboard → Plugins → Available**
4. Install **NguonC - Phim HD** and restart Jellyfin

### Method 2: Docker (Manual)

#### Option A: Download from Release

```bash
# Find your Jellyfin container name
docker ps | grep jellyfin

# Download and install the plugin
docker exec jellyfin bash -c '
  mkdir -p /config/plugins/Jellyfin.Plugin.NguonC &&
  cd /config/plugins/Jellyfin.Plugin.NguonC &&
  curl -L -o plugin.zip https://github.com/thanhdtr/jellyfin-plugin-nguonc/releases/download/v1.0.0/Jellyfin.Plugin.NguonC-1.0.0.zip &&
  unzip -o plugin.zip &&
  rm plugin.zip
'

# Restart the container
docker restart jellyfin
```

#### Option B: Build from Source

```bash
# Clone and build
git clone https://github.com/thanhdtr/jellyfin-plugin-nguonc.git
cd jellyfin-plugin-nguonc
dotnet build -c Release

# Copy the DLL into the container
docker cp bin/Release/net9.0/Jellyfin.Plugin.NguonC.dll \
  jellyfin:/config/plugins/Jellyfin.Plugin.NguonC/Jellyfin.Plugin.NguonC.dll

# Restart the container
docker restart jellyfin
```

#### Option C: Docker Compose Volume Mount

If you have Jellyfin's plugin directory mounted as a volume:

```bash
# Build locally
dotnet build -c Release

# Copy to your mounted plugins directory
cp bin/Release/net9.0/Jellyfin.Plugin.NguonC.dll /path/to/jellyfin/plugins/Jellyfin.Plugin.NguonC/
```

### Method 3: Windows / Linux (Manual)

1. Download `Jellyfin.Plugin.NguonC-1.0.0.zip` from [Releases](https://github.com/thanhdtr/jellyfin-plugin-nguonc/releases)
2. Extract to your Jellyfin plugins directory:
   - **Windows**: `%ProgramData%\Jellyfin\Server\plugins\Jellyfin.Plugin.NguonC\`
   - **Linux**: `/var/lib/jellyfin/plugins/Jellyfin.Plugin.NguonC/`
3. Restart Jellyfin server

## Configuration

1. Go to **Dashboard → Plugins → NguonC - Phim HD → Settings**
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
- .NET 9.0 runtime
- Internet connection to access phim.nguonc.com API

## Troubleshooting

### Plugin not showing up
- Ensure the DLL is in the correct plugins directory (`/config/plugins/Jellyfin.Plugin.NguonC/` for Docker)
- Check Jellyfin logs for any startup errors
- Restart the Jellyfin server

### Videos not playing
- Check if the API is accessible from your server
- Verify network settings and firewall rules
- Check Jellyfin logs for stream proxy errors

### Slow loading
- Try adjusting the Items Per Page setting
- Check your network connection to phim.nguonc.com

### Config page blank
- Update to the latest version (v1.0.0)
- The config page uses Jellyfin's dashboard framework — it must be loaded through the plugin system

## License

MIT License - See [LICENSE](LICENSE) for details

## Credits

- [phim.nguonc.com](https://phim.nguonc.com) - Movie data source
- [Jellyfin](https://jellyfin.org) - Open source media system
