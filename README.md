# NextAI VPN & Residential Proxy Mesh Platform
================================================================================
Dự án Tái cấu trúc & Phát triển Hệ sinh thái VPN & Proxy Cư dân Độc lập Chuẩn V2.1
Nhà phát triển: **nextaitechnology** | Package: `com.nextaitechnology.vpn`
================================================================================

## 1. Giới thiệu Tổng quan (Overview)
NextAI VPN Platform là giải pháp mạng bảo mật và hạ tầng proxy dân cư phân tán hiệu năng cao. Hệ thống được xây dựng theo chuẩn **Development Workspace Standard V2.1 (Design-Gated)**, phân tách tuyệt đối giữa hạ tầng mạng lõi và các ứng dụng client/antidetect bên ngoài.

## 2. Kiến trúc Hệ sinh thái (Ecosystem Architecture)
Hệ thống gồm 3 phân hệ chủ lực:
- **`Backend/`**: Web API RESTful, Admin CMS Dashboard chạy trên **cổng 6033** và Universal Proxy Gateway trên **cổng 10000** (hỗ trợ cả SOCKS5 và HTTP CONNECT với cú pháp xoay IP cư dân thông minh).
- **`apps/desktop/`**: Ứng dụng Desktop Native .NET 10.0 WPF với giao diện Windows 11 Fluent Design (`Wpf.Ui`), bảo tồn nguyên vẹn 113 màn hình XAML, 234 cờ quốc gia, 11 font chữ Museo Sans/Helvetica và 215 tài nguyên đồ họa vector.
- **`apps/android/`**: Scaffold Native Android Kotlin + Jetpack Compose với kiến trúc Clean Architecture & Coroutines/Flows.
- **`Core_Service/`**: Windows Service chạy mức SYSTEM điều khiển Windows Filtering Platform (WFP), WireGuard NT, và OpenVPN.

## 3. Cổng Dịch vụ (Service Ports)
| Dịch vụ | Cổng | Giao thức | Mô tả |
| :--- | :--- | :--- | :--- |
| **Admin CMS & REST API** | `6033` | HTTP / JSON | Quản lý Node Pool, Tenants, Quota, Swagger API |
| **Universal Gateway** | `10000` | SOCKS5 & HTTP CONNECT | Cổng proxy xoay IP cư dân, hỗ trợ sticky session |
| **Dedicated Port Range** | `10001 - 10500` | SOCKS5 | Cổng riêng cố định dành cho từng profile/khách hàng |

## 4. Hướng dẫn Khởi chạy Nhanh (Quick Start)

### Yêu cầu Tiên quyết (Prerequisites):
- Hệ điều hành: **Windows 10 / Windows 11 (x64)**.
- **.NET 10 SDK** (Khuyến nghị version `10.0.400` trở lên).
- Tùy chọn: **Node.js** (để chạy các script test/seed nâng cao nếu cần).

### Cách 1: Khởi Chạy 1-Click Toàn Hệ Thống (Khuyến nghị cho Dev mới)
Chỉ cần chạy tệp batch tại thư mục gốc:
```bat
Chay_HeThong_NextAiVPN.bat
```
Script sẽ tự động:
1. Kiểm tra và khởi động **Backend REST API & Universal Gateway** trên cổng `6033` và `10000`.
2. Mở trình duyệt Web CMS Quản trị: `http://127.0.0.1:6033/cms_admin.html`.
3. Tự động kiểm tra và biên dịch .NET 10 nếu chưa có binary, sau đó khởi chạy **NextAiVPN Desktop Client**.

### Cách 2: Khởi Chạy Từng Phân Hệ Thủ Công

#### 1. Khởi chạy Backend & Gateway:
```bash
cd Backend
dotnet run --project VpnBackend.csproj -c Release
```
- Web Admin CMS: `http://127.0.0.1:6033/cms_admin.html`
- Universal Gateway: `127.0.0.1:10000` (SOCKS5 / HTTP CONNECT)

#### 2. Khởi chạy Desktop Client:
```bash
cd apps/desktop/NextAiVPN.Desktop
dotnet run -c Release
```

#### 3. Tự động Đóng Gói Bộ Cài Single-File (`NextAiVPN_Setup.exe`):
```powershell
powershell -ExecutionPolicy Bypass -File scripts/package_installer.ps1
```

#### 4. Chạy Bộ Kiểm Thử Tự Động Toàn Diện:
```bash
# Test Desktop WPF 18/18 bước trực quan:
cd apps/desktop/NextAiVPN.Desktop/bin/Release/net10.0-windows
NextAiVPN.Desktop.exe --autotest

# Test End-to-End Gateway & SQLite CSDL:
node run_comprehensive_e2e_test.js
```

---

## 5. Cấu Trúc Thư Mục Bàn Giao (Repository Structure)
```text
├── Backend/                 # Server .NET 10 (Web API port 6033 + Universal Gateway port 10000)
│   ├── data/                # CSDL SQLite (xray_vpn_nodes.db), proxies.json, client_nodes.json
│   ├── services/            # UniversalGateway, ProxyManager, DedicatedPort, RotationEngine
│   └── wwwroot/             # Web CMS Admin Portal + 234 cờ quốc gia tròn 1:1
├── apps/
│   ├── desktop/
│   │   ├── NextAiVPN.Desktop/ # Giao diện chính WPF .NET 10 (113 XAML, 8 tabs, Clean Emerald)
│   │   ├── extracted/       # Thư viện native SDK, drivers WFP, WireGuard, OpenVPN
│   │   └── SDK/             # Mã nguồn C# VpnSDK modules
│   └── installer/
│       └── NextAiVPN.Setup/ # Dự án đóng gói Standalone Setup Wizard tự trích xuất
├── scripts/
│   └── package_installer.ps1 # Script tự động đóng gói Single-File NextAiVPN_Setup.exe
├── DOCS/ & Report/          # 16 báo cáo kỹ thuật, tài liệu kiến trúc, token thiết kế
├── Chay_HeThong_NextAiVPN.bat # Launcher tự động 1-click cho toàn bộ hệ thống
├── AGENTS.md                # Hiến pháp quy chuẩn phát triển (13 vai trò, Decoupling Law)
├── PROJECT_MEMORY.md        # Bộ nhớ vận hành & nhật ký các quyết định kỹ thuật
├── TASK_LOG.md              # Nhật ký chi tiết tiến độ 54 tasks đã hoàn thành
└── UPDATETODOS.md           # Danh sách trạng thái các đầu việc
```

---

## 6. Tài liệu Kỹ thuật Chi tiết (Documentation Index)
- **Hiến pháp Hệ thống**: [AGENTS.md](AGENTS.md)
- **Bộ nhớ Dự án**: [PROJECT_MEMORY.md](PROJECT_MEMORY.md)
- **Quy chuẩn Lập trình**: [Docs/rules.md](Docs/rules.md)
- **Hướng dẫn Cài đặt**: [installer/HUONG_DAN_CAI_DAT.md](installer/HUONG_DAN_CAI_DAT.md)
- **Báo cáo Kiểm thử Tự động E2E**: [Report/UI_AUTOMATION_TEST_REPORT.md](Report/UI_AUTOMATION_TEST_REPORT.md)
- **Báo cáo Kỹ thuật Chuyên sâu**: Xem trong thư mục [Report/](Report/)

