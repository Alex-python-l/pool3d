from pathlib import Path
import argparse
import json
import os
import subprocess

HERE = Path(__file__).resolve().parent
DELIVERY = HERE.parent


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--method', default='TableReplacementChecks.Run')
    parser.add_argument('--label', default='baseline')
    parser.add_argument('--graphics', action='store_true')
    args = parser.parse_args()
    if not args.label.replace('_', '').isalnum() or args.method not in (
        'TableReplacementBuilder.Prepare', 'TableReplacementChecks.Run', 'TableReplacementChecks.ValidateOnly', 'TableReplacementChecks.ExportPackage', 'FeltLiteEditor.PrepareAndCheck'):
        raise ValueError('Only replacement preparation and edit-mode checks are allowed')
    project = DELIVERY / 'UnityProject'
    # An Editor exiting on a failed check can leave a stale lockfile; check live processes.
    project_literal = str(project).replace("'", "''")
    query = "Get-CimInstance Win32_Process -Filter \"Name='Unity.exe'\" -ErrorAction Stop | Where-Object { $_.CommandLine -and $_.CommandLine.Contains('" + project_literal + "') } | ForEach-Object { $_.ProcessId }"
    active = subprocess.run(['powershell', '-NoProfile', '-NonInteractive', '-Command', query], capture_output=True, text=True,
                            creationflags=subprocess.CREATE_NO_WINDOW)
    if active.returncode != 0: raise RuntimeError('Cannot verify active Editors; not launching a second instance')
    if active.stdout.strip(): raise RuntimeError('Target project is already open; no second Editor will be launched')
    pipe_names = [p for p in os.listdir('\\\\.\\pipe\\') if p.startswith('Unity-LicenseClient-') and not p.endswith('-notifications')]
    if len(pipe_names) != 1:
        raise RuntimeError('No unambiguous current licensing IPC; no credential handling attempted')
    ipc = pipe_names[0].removeprefix('Unity-')
    log = HERE / ('unity_' + args.label + '.log')
    command = [r'D:\UnityTools\Editor\2022.3.62f2\Editor\Unity.exe', '-batchmode', '-nographics', '-quit', '-job-worker-count', '2',
               '-projectPath', str(project), '-licensingIpc', ipc, '-executeMethod', 'Pool3D.Offline.Editor.' + args.method, '-logFile', str(log)]
    if args.graphics: command.remove('-nographics')
    if args.method == 'IntegratedLightingBuilder.BeginBake': command.remove('-quit')
    process = subprocess.Popen(command, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, creationflags=subprocess.CREATE_NO_WINDOW)
    print(json.dumps({'pid': process.pid, 'purpose': args.method, 'playMode': False, 'player': False}), flush=True)
    code = process.wait()
    (HERE / ('exit_' + args.label + '.json')).write_text(json.dumps({'exitCode': code, 'method': args.method}))
    (DELIVERY / 'Docs/editor_exit.json').write_text(json.dumps({'exitCode': code, 'method': args.method}))
    print(json.dumps({'exitCode': code}), flush=True)
    raise SystemExit(code)


if __name__ == '__main__':
    main()
