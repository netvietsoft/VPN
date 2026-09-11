# BÁO CÁO 02: DANH SÁCH SDK & THƯ VIỆN BÊN NGOÀI ĐƯỢC SỬ DỤNG
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 5 trong Convertme.txt
================================================================================

Tài liệu này liệt kê toàn bộ các thư viện và SDK bên ngoài được phát hiện trong mã nguồn đã decompile, phân loại theo vai trò và chức năng:

| STT | Tên Thư Viện / SDK | Phiên Bản | Nhà Cung Cấp | Chức Năng & Nhiệm Vụ Trong Ứng Dụng |
| :---: | :--- | :---: | :--- | :--- |
| 1 | **Wpf.Ui** & **Wpf.Ui.Abstractions** | 3.0.x | Lepoco (Open Source) | Cung cấp toàn bộ hệ thống giao diện Windows 11 Fluent Design: bo góc tự động, hiệu ứng bóng mờ, điều hướng NavigationView, các controls ToggleSwitch, Button Fluent, v.v. |
| 2 | **Microsoft.Web.WebView2.Wpf** | 1.0.x | Microsoft | Nhúng trình duyệt Edge Chromium vào ứng dụng để thực hiện đăng nhập SSO (Spaceship, Namecheap) và thanh toán bảo mật. |
| 3 | **H.NotifyIcon.Wpf** | 2.0.x | Haven (Open Source) | Quản lý biểu tượng khay hệ thống (System Tray Icon), hiển thị menu chuột phải khi VPN chạy ngầm, quản lý bong bóng thông báo (Notify Balloons). |
| 4 | **Serilog** | 3.1.x | Serilog Contributors | Framework ghi log cấu trúc (Structured Logging) ra console và file xoay vòng (Rolling File), ghi lại tiến trình kết nối và lỗi mạng. |
| 5 | **System.Data.SQLite** & **Dapper** | 1.0.x | SQLite Org & StackOverflow | Lưu trữ cơ sở dữ liệu cục bộ trên máy người dùng: danh sách server yêu thích, thông số cấu hình giao thức, lịch sử kết nối. |
| 6 | **Bugsnag** | 2.2.x | Bugsnag Inc. | Thu thập và gửi crash report, stack trace khi ứng dụng gặp lỗi ngoại lệ nghiêm trọng về máy chủ quản trị. |
| 7 | **Microsoft.Win32.TaskScheduler** | 2.9.x | David Hall | Tự động tạo tác vụ Task Scheduler trong Windows để khởi chạy ứng dụng cùng hệ điều hành hoặc cập nhật định kỳ không cần UAC prompt. |
| 8 | **UACHelper** | 1.3.x | Open Source | Hỗ trợ nâng quyền Administrator (Elevated Privileges) an toàn khi cần cài đặt hoặc khởi động driver mạng WFP / TAP. |
| 9 | **HtmlAgilityPack** | 1.11.x | ZZZ Projects | Phân tích cú pháp HTML khi xử lý các phản hồi từ cổng đăng nhập web hoặc trang điều khoản dịch vụ. |
| 10 | **RestSharp** & **RestSharp.Serializers.NewtonsoftJson** | 108.x | RestSharp Org | Thư viện HTTP Client gọi REST API bất đồng bộ tới máy chủ backend. |
| 11 | **System.Reactive** & **DynamicData** | 6.0.x | .NET Foundation | Xử lý luồng dữ liệu phản ứng (Reactive Extensions), tự động lọc, tìm kiếm và sắp xếp danh sách vị trí server VPN theo thời gian thực. |
| 12 | **VpnSDK.Core** & **VpnSDK.Private.WFP** | 1.0.0 | WLVPN / NextAI | Driver và thư viện lõi can thiệp Windows Filtering Platform (WFP), kích hoạt tính năng Kill Switch chặn rò rỉ dữ liệu. |
| 13 | **VpnSDK.Private.OpenVpn** | 1.0.0 | WLVPN / NextAI | Giao tiếp điều khiển OpenVPN daemon (`openvpn.exe`, `tapwlvpn.sys`), hỗ trợ thuật toán xáo trộn gói tin Scramble. |
| 14 | **WireGuard Driver** (`wireguard.dll`, `tunnel.dll`) | 0.5.x | WireGuard LLC | Driver mạng WireGuard NT kernel-mode tốc độ cao, tiêu thụ ít CPU và độ trễ tối thiểu. |
| 15 | **NetFilter SDK** (`netfilter.sys`, `nfapi.dll`) | 1.7.x | Vitaly Rozhkov | Lọc gói tin TCP/UDP ở tầng driver, hỗ trợ tính năng Phân luồng ứng dụng (Split Tunneling). |
