@echo off
setlocal enabledelayedexpansion
for /l %%i in (1,1,1300) do (
    if not exist %%i.json echo Thiếu file %%i.json
)
echo.
echo ===========================
echo Đã kiểm tra xong tất cả 1300 file.
pause
