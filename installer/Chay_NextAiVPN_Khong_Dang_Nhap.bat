@echo off
title Khoi chay NextAI VPN Desktop (Che Do Khong Can Dang Nhap)
chcp 65001 >nul
echo =================================================================
echo   KÍCH HOẠT NEXTAI VPN DESKTOP - VÀO TRỰC TIẾP KHÔNG CẦN ĐĂNG NHẬP
echo =================================================================
echo.
echo Đang mở ứng dụng NextAI VPN Desktop...
cd /d "%~dp0apps\desktop\NextAiVPN_Bypass"
start "" "FastVPN.exe"
echo.
echo [OK] Đã mở giao diện chính VPN Dashboard thành công!
timeout /t 3 >nul
