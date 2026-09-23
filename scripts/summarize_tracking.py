#!/usr/bin/env python3
"""Summarize an explicitly delimited physical CSV interval; never infer user actions."""
import argparse
import csv
import json
import statistics

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('csv')
parser.add_argument('--start', type=float, required=True)
parser.add_argument('--end', type=float, required=True)
args = parser.parse_args()
with open(args.csv, newline='') as stream:
    rows = [r for r in csv.DictReader(stream)
            if args.start <= float(r['seconds']) <= args.end]
if len(rows) < 2:
    parser.error('At least two samples are required')
visible = [r for r in rows if r['visible'].lower() == 'true']
elapsed = float(rows[-1]['seconds']) - float(rows[0]['seconds'])
result = {'csv': args.csv, 'start': float(rows[0]['seconds']),
          'end': float(rows[-1]['seconds']), 'samples': len(rows),
          'visible_samples': len(visible), 'elapsed_s': elapsed,
          'rt_states': sorted(set(r['rt'] for r in rows)),
          'camera_modes': sorted(set((r['width'], r['height'], r['rotation'], r['sensor_fov']) for r in rows)),
          'limitation': 'CSV samples can repeat poses. Frame time is an EMA, not GPU profiling. User actions and distances require separate evidence.'}
if 'processed_frames' in rows[0] and elapsed > 0:
    result['processed_hz'] = (int(rows[-1]['processed_frames']) - int(rows[0]['processed_frames'])) / elapsed
result['visible_statistics'] = {}
if visible:
    for key in ['frame_ms', 'image_ms', 'detect_ms', 'x', 'y', 'z']:
        values = [float(r[key]) for r in visible]
        result['visible_statistics'][key] = {'median': statistics.median(values),
                'min': min(values), 'max': max(values), 'stdev': statistics.pstdev(values)}
print(json.dumps(result, indent=2, ensure_ascii=False))
