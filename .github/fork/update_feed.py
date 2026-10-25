"""Package release-only update ZIPs and generate the upstream apiv2 format."""
import hashlib
import json
import os
from pathlib import Path
import zipfile

metadata = json.loads(Path('Plain Craft Launcher 2/metadata.json').read_text(encoding='utf-8-sig'))
version = metadata['version']['base']
tag = os.environ['RELEASE_TAG']
if tag != 'v' + version:
    raise ValueError('Release tag differs from the source version.')
base = f"https://github.com/{os.environ['GITHUB_REPOSITORY']}/releases/download/{tag}/"
notes = ('**Release由GitHub Actions自动生成，且与 PCL 官方及 CE 团队无关！**\n'
         f"上游Release：[{os.environ['UPSTREAM_NAME']}]({os.environ['UPSTREAM_URL']})，可以在上游检查changelog和其他说明")
feed = Path('update-feed/apiv2')
cache = {}

def write(name, data, folder=''):
    path = feed / folder / (name + '.json')
    path.parent.mkdir(parents=True, exist_ok=True)
    content = (json.dumps(data, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    path.write_bytes(content)
    cache[name] = hashlib.md5(content).hexdigest()

for arch in ('x64', 'ARM64'):
    stem = f'PCL2_CE_Release_{arch}'
    executable = Path(stem + '.exe').read_bytes()
    if not executable.startswith(b'MZ'):
        raise ValueError(f'{stem}.exe is not a Windows executable.')
    with zipfile.ZipFile(stem + '.zip', 'w', zipfile.ZIP_DEFLATED) as archive:
        archive.writestr('Plain Craft Launcher Community Edition.exe', executable)
    url = base + stem + '.zip'
    data = {'assets': [{
        'file_name': stem + '.zip',
        'version': {'channel': 'stable', 'name': version, 'code': metadata['version']['code']},
        'downloads': ['https://gh-proxy.com/' + url, url],
        'patches': [], 'sha256': hashlib.sha256(executable).hexdigest(),
        'changelog': '<summary>\n' + notes + '\n</summary>'
    }]}
    # This fork publishes stable builds; both updater channels point to that build.
    for channel in ('sr', 'fr'):
        write(f'updates-{channel}{arch.lower()}', data, 'updates')
write('announcement', {'content': []})
(feed / 'cache.json').write_text(json.dumps(cache, indent=2) + '\n', encoding='utf-8')
