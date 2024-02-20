#xcopy /E /Y c:\pws\pws-web-api Z:\pws\pws-web-api\
Start-Process -FilePath (Resolve-Path -Path "${Env:WinDir}\system32\inetsrv\appcmd.exe") -NoNewWindow -ArgumentList "stop site /site.name:PwsWebApi" -Wait

Remove-Item Z:\pws\pws-web-api -Recurse -Force -Verbose
Copy-Item  -Recurse -Force -Verbose C:\pws\pws-web-api Z:\pws\

Start-Process -FilePath (Resolve-Path -Path "${Env:WinDir}\system32\inetsrv\appcmd.exe") -NoNewWindow -ArgumentList "start site /site.name:PwsWebApi" -Wait
Write-Output "Ok you got to the end!"
