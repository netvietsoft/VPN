@echo off
chcp 65001 >nul
title NextAI VPN Platform - All-in-One Launcher
color 0A

echo ===============================================================================
echo                NEXTAI VPN PLATFORM & RESIDENTIAL GATEWAY (V2.1)
echo ===============================================================================
echo   [1] Backend REST API & CMS:   http://127.0.0.1:6033
echo   [2] Universal Proxy Gateway:   127.0.0.1:10000 (SOCKS5 / HTTP CONNECT)
echo   [3] Desktop Client (.NET 10):  NextAiVPN.Desktop.exe
echo ===============================================================================
echo.

set ROOT_DIR=%~dp0

:: Kiem tra xem Backend port 6033 da hoat dong binh thuong chua
powershell -Command "$tc = New-Object System.Net.Sockets.TcpClient; try { $tc.Connect('127.0.0.1', 6033); exit 0; } catch { exit 1; } finally { $tc.Dispose(); }" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] Backend Service dang hoat dong tren cong 6033 va 10000.
) else (
    echo [*] Dang giai phong cac tien trinh cu va khoi dong Backend Service & Gateway...
    taskkill /F /IM VpnBackend.exe >nul 2>&1
    start "NextAiVPN Backend & Gateway" /d "%ROOT_DIR%Backend" cmd /c "dotnet run --project VpnBackend.csproj -c Release"
    timeout /t 3 /nobreak >nul
)

echo.
echo [*] Dang mo Web CMS Quan ly Proxy: http://127.0.0.1:6033/cms_admin.html ...
start http://127.0.0.1:6033/cms_admin.html

echo.
echo [*] Dang khoi dong NextAiVPN Desktop Client (.NET 10 + WPF)...
cd /d "%ROOT_DIR%apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows"
start "" "NextAiVPN.Desktop.exe"

echo.
echo ===============================================================================
echo   Khoi dong hoan tat! Ban co the quan ly Proxy qua trinh duyet Web CMS 
echo   va su dung NextAiVPN Desktop tren man hinh.
echo ===============================================================================
timeout /t 5
