"""Apply only the restriction boolean and update-source list; stop on upstream changes."""
import json
from pathlib import Path

APP = Path('Plain Craft Launcher 2')
POLICY = Path('PCL.Core/App/Region/RegionPolicy.cs')
ORIGINAL = b'public bool IsRestricted => Classification is RegionClassification.Mainland or RegionClassification.Unknown;'
PATCHED = b'public bool IsRestricted => false;'

UPDATE_SOURCE = APP / 'Modules/Updates/UpdateManager.cs'
UPSTREAM_SOURCES = b'''    public static UpdatesWrapperModel remoteServer = new(new List<IUpdateSource>
    {
        new UpdatesMirrorChyanModel(),
        new UpdatesRandomModel(new[]
        {
            new UpdatesMinioModel("https://s3.pysio.online/pcl2-ce/", "Pysio"),
            new UpdatesMinioModel("https://staticassets.naids.com/resources/pclce/", "Naids")
        }),
        new UpdatesMinioModel("https://github.com/PCL-Community/PCL2_CE_Server/raw/main/", "GitHub")
    });'''
FORK_SOURCES = b'''    public static UpdatesWrapperModel remoteServer = new(new List<IUpdateSource>
    {
        new UpdatesMinioModel("https://gh-proxy.com/https://raw.githubusercontent.com/idadwind1/PCL-CE/codex/update-feed/", "GitHub via gh-proxy"),
        new UpdatesMinioModel("https://raw.githubusercontent.com/idadwind1/PCL-CE/codex/update-feed/", "GitHub")
    });'''

def apply(root):
    root = Path(root)
    path = root / POLICY
    content = path.read_bytes()
    if content.count(ORIGINAL) == 1:
        path.write_bytes(content.replace(ORIGINAL, PATCHED))
    elif content.count(PATCHED) != 1:
        raise ValueError('Restriction declaration changed upstream. Stopping.')
    path = root / UPDATE_SOURCE
    content = path.read_bytes()
    if content.count(UPSTREAM_SOURCES) == 1:
        path.write_bytes(content.replace(UPSTREAM_SOURCES, FORK_SOURCES))
    elif content.count(FORK_SOURCES) != 1:
        raise ValueError('Update source list changed upstream; stopping.')
    return json.loads((root / APP / 'metadata.json').read_text(encoding='utf-8-sig'))

if __name__ == '__main__':
    apply(Path.cwd())
