@echo on
c:
cd \Solutions\ohi\minimalgrpc
for /d /r . %%d in (bin,obj,ClientBin,Generated_Code,node_modules,.vs) do @if exist "%%d" rd /s /q "%%d"
pause
