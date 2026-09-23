#!/usr/bin/env python3
"""Inspect the step-3 APK without claiming execution on Android."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess
import zipfile


def run(*args, env=None):
    return subprocess.run(args, check=True, text=True, capture_output=True, env=env).stdout


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('apk', type=Path)
    parser.add_argument('report', type=Path)
    args = parser.parse_args()
    if args.report.exists():
        raise SystemExit('El informe ya existe; elegir una ruta nueva.')
    android = Path('/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Data/PlaybackEngines/AndroidPlayer')
    build_tools = android / 'SDK/build-tools/36.0.0'
    env = dict(os.environ, JAVA_HOME=str(android / 'OpenJDK'))
    signature = run(str(build_tools / 'apksigner'), 'verify', '--verbose', str(args.apk), env=env)
    alignment = run(str(build_tools / 'zipalign'), '-c', '-P', '16', '4', str(args.apk))
    badging = run(str(build_tools / 'aapt'), 'dump', 'badging', str(args.apk))
    for required in ("name='com.chupacabras.ar.trackingprobe'", "sdkVersion:'26'", "targetSdkVersion:'35'", "android.permission.CAMERA", "native-code: 'arm64-v8a'"):
        assert required in badging, required
    assert 'arcore' not in badging.lower()
    libraries = []
    with zipfile.ZipFile(args.apk) as apk:
        assert not any('arcore' in name.lower() for name in apk.namelist())
        for name in apk.namelist():
            if not (name.startswith('lib/') and name.endswith('.so')):
                continue
            data = apk.read(name)
            assert data[:6] == b'\x7fELF\x02\x01', name
            assert struct.unpack_from('<H', data, 18)[0] == 183, name
            offset = struct.unpack_from('<Q', data, 32)[0]
            size, count = struct.unpack_from('<HH', data, 54)
            load_alignments = []
            for index in range(count):
                at = offset + size * index
                if struct.unpack_from('<I', data, at)[0] == 1:
                    load_alignments.append(struct.unpack_from('<Q', data, at + 48)[0])
            assert load_alignments and min(load_alignments) >= 16384, name
            libraries.append({'name': name, 'load_alignment': load_alignments})
    assert any(item['name'].endswith('/libAprilTag.so') for item in libraries)
    result = {'apk': str(args.apk), 'bytes': args.apk.stat().st_size,
              'sha256': hashlib.sha256(args.apk.read_bytes()).hexdigest(),
              'signature': signature, 'zipalign_passed': True, 'zipalign_output': alignment,
              'manifest': [line for line in badging.splitlines() if line.startswith(('package:', 'sdkVersion:', 'targetSdkVersion:', 'uses-permission:', 'native-code:'))],
              'libraries': libraries, 'physical_device_tested': False,
              'limitation': 'Verificación estática de esta APK; instalación, carga nativa y seguimiento requieren evidencia física separada.'}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    with args.report.open('x') as output:
        json.dump(result, output, indent=2, ensure_ascii=False)
        output.write('\n')
    print(json.dumps({'passed': True, 'bytes': result['bytes'], 'sha256': result['sha256'], 'libraries': len(libraries)}))


if __name__ == '__main__':
    main()
