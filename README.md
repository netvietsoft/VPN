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

## 4. Hướng dẫn Khởi chạy (Quick Start)
### Chạy Backend & Gateway Platform:
```bash
cd E:\DECOMPILER\Soft\VPN\CONVERT\Backend
dotnet run
```
Truy cập Web CMS: `http://127.0.0.1:6033`

### Mở mã nguồn Desktop Client:
```bash
cd E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop
# Mở solution bằng Visual Studio 2026 / Rider / VS Code
```

## 5. Tài liệu Chi tiết (Documentation Index)
- **Hiến pháp AI Agent**: [AGENTS.md](AGENTS.md)
- **Quy chuẩn Lập trình**: [Docs/rules.md](Docs/rules.md)
- **Sơ đồ Thư mục**: [Docs/CAU_TRUC_THU_MUC.md](Docs/CAU_TRUC_THU_MUC.md)
- **Báo cáo Kỹ thuật Chuyên sâu (12 Reports)**: Xem trong thư mục [Report/](Report/)
- **Danh sách Thông số cần bổ sung**: [Tasksrequiring.md](Tasksrequiring.md)
- **Nhật ký Tiến độ**: [TASK_LOG.md](TASK_LOG.md)
