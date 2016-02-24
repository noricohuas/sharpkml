set MSBUILDEMITSOLUTION=1
pushd C:\_projs\sharpkml
rem Source Builds
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" SharpKml.proj /m:8 /v:d /target:BuildRelease /p:nowarn="1607,1591,1573" /fl /flp:logfile=Build.log;verbosity=diagnostic
start notepad.exe build.log
popd


