from pathlib import Path
import subprocess,zipfile,hashlib,struct,json,os
root=Path(__file__).resolve().parents[1]
apks=sorted((root/'builds/android').glob('12_appearance_*.apk')); assert apks
apk=apks[-1]; evidence=root/os.environ.get('CHUPA_APK_EVIDENCE', 'docs/evidencias/apk_'+apk.stem); evidence.mkdir(parents=True,exist_ok=False)
android=Path('/home/cacawatin/Unity/Hub/Editor/6000.3.22f1/Editor/Data/PlaybackEngines/AndroidPlayer')
bt=android/'SDK/build-tools/36.0.0'; env=dict(os.environ,JAVA_HOME=str(android/'OpenJDK'))
results={}
for name,args in [('signature',['apksigner','verify','--verbose',str(apk)]),('zipalign',['zipalign','-c','-P','16','-v','4',str(apk)]),('badging',['aapt','dump','badging',str(apk)])]:
 result=subprocess.run([str(bt/args[0]),*args[1:]],capture_output=True,text=True,env=env)
 (evidence/(name+'.txt')).write_text(result.stdout+result.stderr)
 assert result.returncode==0,(name,result.stderr); results[name]=True
badging=(evidence/'badging.txt').read_text();assert "name='com.chupacabras.ar.appearance12'" in badging and "versionName='0.0.14'" in badging and "native-code: 'arm64-v8a'" in badging
assert 'android.permission.CAMERA' in badging
libs=[]
with zipfile.ZipFile(apk) as z:
 for n in z.namelist():
  if not n.startswith('lib/') or not n.endswith('.so'): continue
  assert n.startswith('lib/arm64-v8a/'); data=z.read(n); assert data[:6]==b'\x7fELF\x02\x01'
  machine=struct.unpack_from('<H',data,18)[0];assert machine==183
  offset=struct.unpack_from('<Q',data,32)[0]; size,count=struct.unpack_from('<HH',data,54)
  aligns=[]
  for i in range(count):
   t,flags,off,va,pa,fs,ms,align=struct.unpack_from('<IIQQQQQQ',data,offset+i*size)
   if t==1:
    assert align>=16384 and (va-off)%16384==0,(n,align)
    aligns.append(align)
  libs.append({'file':n,'machine':machine,'load_segment_alignments':aligns})
 assert any('AprilTag' in l['file'] for l in libs),libs
report={'apk':str(apk),'sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'bytes':apk.stat().st_size,'tools':str(bt),'package':'com.chupacabras.ar.appearance12','version':'0.0.14','checks':results,'libraries':libs,'physical_device':False,'page_size_runtime_validation':False}
(evidence/'apk_verificacion.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='libraries'},indent=2))
