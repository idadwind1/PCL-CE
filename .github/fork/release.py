"""Sync stable CE releases and apply the boolean and update-source patch."""
import json
import os
from pathlib import Path
import re
import subprocess
import sys
from apply_fork import POLICY, UPDATE_SOURCE, apply

STATE = Path('.github/fork/upstream.json')
REPO = os.environ.get('FORK_REPOSITORY', 'idadwind1/PCL-CE')
UPSTREAM = os.environ.get('UPSTREAM_REPOSITORY', 'PCL-Community/PCL-CE')
RELEASE_ZIP_PATHS = ('最新正式版.zip', '最新正式版-ARM64.zip')

def run(*args, **kwargs):
    return subprocess.check_output(args, text=True, **kwargs).strip()

def git(*args):
    return run('git', *args)

def api(path, method='GET', data=None, missing_ok=False):
    command = ['gh', 'api', '--method', method, '--include', path]
    if data is not None:
        command += ['--input', '-']
    result = subprocess.run(command, input=None if data is None else json.dumps(data), text=True,
                            stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    response = result.stdout.replace('\r\n', '\n')
    if missing_ok and response.startswith('HTTP/') and response.split('\n', 1)[0].split()[1] == '404':
        return None
    if result.returncode:
        raise RuntimeError(f'GitHub API {method} {path} failed: {result.stderr.strip()}')
    _, body = response.split('\n\n', 1)
    return json.loads(body) if body.strip() else None

def write_json(path, data):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def identity():
    git('config', 'user.name', 'github-actions[bot]')
    git('config', 'user.email', '41898282+github-actions[bot]@users.noreply.github.com')

def outputs(**values):
    with open(os.environ['GITHUB_OUTPUT'], 'a', encoding='utf-8') as stream:
        for key, value in values.items():
            value = str(value)
            if '\n' in value or '\r' in value:
                raise ValueError('Invalid workflow output.')
            stream.write(f'{key}={value}\n')

def assert_no_release_zip_history(ref='HEAD'):
    if git('log', '--format=%H', ref, '--', *RELEASE_ZIP_PATHS):
        raise ValueError('Release ZIP paths remain in history; refusing to push.')

def filter_upstream(sha):
    ref = 'refs/heads/fork-upstream-filtered'
    git('update-ref', ref, sha)
    command = [sys.executable, '-m', 'git_filter_repo', '--force', '--preserve-commit-hashes',
               '--replace-refs', 'delete-no-add', '--invert-paths']
    for path in RELEASE_ZIP_PATHS:
        command += ['--path', path]
    subprocess.run(command + ['--refs', ref], check=True)
    filtered = git('rev-parse', ref)
    assert_no_release_zip_history(ref)
    git('update-ref', '-d', ref, filtered)
    return filtered

def rebase(upstream_sha, old_base):
    result = subprocess.run(['git', 'rebase', '--onto', upstream_sha, old_base, 'main'])
    if result.returncode:
        raise RuntimeError('Rebase failed; resolve source conflicts before retrying. Nothing was pushed.')

def sync():
    identity()
    git('fetch', '--no-tags', 'origin', '+refs/heads/main:refs/remotes/origin/main')
    git('checkout', '-B', 'main', 'origin/main')
    before = git('rev-parse', 'HEAD')
    latest = api(f'repos/{UPSTREAM}/releases/latest')
    tag = latest['tag_name']
    if not re.fullmatch(r'v\d+\.\d+\.\d+(?:\.\d+)?', tag):
        raise ValueError('Unexpected stable CE version tag; review upstream changes.')
    state = json.loads(STATE.read_text(encoding='utf-8'))
    upstream_sha = state['upstream_sha']
    upstream_base = state['upstream_base']
    if state['tag'] != tag:
        git('fetch', '--no-tags', f'https://github.com/{UPSTREAM}.git', f'refs/tags/{tag}')
        upstream_sha = git('rev-parse', 'FETCH_HEAD^{commit}')
        upstream_base = filter_upstream(upstream_sha)
        rebase(upstream_base, state['upstream_base'])
    metadata = apply(Path.cwd())
    version = metadata['version']['base']
    if tag != 'v' + version:
        raise ValueError('Source version differs from upstream release tag.')
    write_json(STATE, {'upstream_sha': upstream_sha, 'upstream_base': upstream_base, 'tag': tag, 'version': version})
    # Amend the fork patch rather than introducing mechanical follow-up commits.
    git('add', '--', str(STATE), str(POLICY), str(UPDATE_SOURCE))
    if subprocess.run(['git', 'diff', '--cached', '--quiet']).returncode:
        git('commit', '--amend', '--no-edit')
    sha = git('rev-parse', 'HEAD')
    assert_no_release_zip_history()
    if sha != before:
        git('push', f'--force-with-lease=refs/heads/main:{before}', 'origin', 'HEAD:refs/heads/main')
    release = api(f'repos/{REPO}/releases/tags/{tag}', missing_ok=True)
    build = release is None or release['draft'] or os.environ.get('REBUILD') == 'true'
    outputs(build=str(build).lower(), sha=sha, tag=tag, version=version,
            upstream_name=latest['name'] or tag, upstream_url=latest['html_url'])
    print(f'{tag}: ' + ('build required' if build else 'already published'))

if __name__ == '__main__':
    sync()
