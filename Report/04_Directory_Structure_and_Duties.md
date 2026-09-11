# BÁO CÁO 04: CẤU TRÚC THƯ MỤC MÃ NGUỒN VÀ NHIỆM VỤ TỪNG PHẦN
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 9 trong Convertme.txt
================================================================================

Tài liệu này mô tả chi tiết chức năng, nhiệm vụ và công nghệ phụ trách của từng thư mục trong dự án sau khi đã được chuẩn hóa theo `Development_Workspace_Standard_V2.1_Design_Gated.txt`.

---

## 1. SƠ ĐỒ PHÂN BỔ NHIỆM VỤ THƯ MỤC

| Thư Mục | Chức Năng & Nhiệm Vụ Cụ Thể | Công Nghệ / Ngôn Ngữ | Phụ Trách Chính |
| :--- | :--- | :--- | :--- |
| **`Backend/`** | Toàn bộ hạ tầng máy chủ, API endpoints, Web Admin CMS trên cổng `6033` và Universal Proxy Gateway trên cổng `10000`. | C# .NET 10 / Kestrel, Sockets | Agent 2 (Backend) |
| `Backend/api/` | Bộ điều khiển tiếp nhận request RESTful API, định tuyến và trả về JSON. | ASP.NET Core Minimal APIs / Controllers | Agent 2 (Backend) |
| `Backend/services/` | Động cơ Proxy SOCKS5, HTTP CONNECT, tính toán hạn ngạch băng thông (Quota), định tuyến theo Session. | .NET Sockets, Task Async | Agent 2 & Agent 4 |
| `Backend/feature/` | Các module nghiệp vụ: Node Pool, Tenant Management, Dedicated Ports. | C# Domain Models | Agent 2 (Backend) |
| `Backend/cms/` & `wwwroot/` | Giao diện quản trị Web Admin trực quan, theo dõi lưu lượng mạng, bản đồ vị trí node. | HTML5, CSS3 Tokens, Vanilla JS | Agent 2 & Agent 11 |
| **`apps/desktop/`** | Ứng dụng VPN Client trên Windows, quản lý kết nối, giao thức, Split Tunneling. | C# WPF (.NET 10), XAML, Wpf.Ui | Agent 3 (Desktop) |
| `apps/desktop/Views_XAML/` | 113 màn hình và User Controls XAML hoàn chỉnh phục vụ cho việc thiết kế lại sau. | XAML, Fluent Styles | Agent 3 & Agent 11 |
| `apps/desktop/resources/` | 234 cờ quốc gia PNG, các từ điển tài nguyên màu sắc và styles Dark/Light. | ResourceDictionaries, PNG | Agent 11 (Designer) |
| `apps/desktop/fonts/` | 11 font chữ bản quyền phục vụ hiển thị chuẩn mực trên client. | OTF, TTF | Agent 11 (Designer) |
| `apps/desktop/assets/` | 215 tài nguyên đồ họa: icon, nút bấm, hình minh họa trạng thái. | PNG, ICO | Agent 11 (Designer) |
| `apps/desktop/extracted/` | Thư viện liên kết động (.dll), drivers WFP, WireGuard và OpenVPN nguyên bản. | C++, Win32 Native, .NET DLLs | Agent 10 (DevOps) |
| **`apps/android/`** | Dự án ứng dụng Android Native tái dựng bằng Kotlin và Jetpack Compose (`com.nextaitechnology.vpn`). | Kotlin, Jetpack Compose, Coroutines | Agent 3 (Mobile) |
| **`packages/`** | Chứa các thư viện dùng chung giữa client và backend (Contracts, DTOs, Domain logic). | C# Class Libraries (.NET 10) | Agent 1 (Architect) |
| **`Core_Service/`** | Dịch vụ Windows Service (`VpnHostService.exe`) chạy ngầm quyền SYSTEM để thao tác driver. | C# / C++ Win32 Service | Agent 10 (DevOps) |
| **`Docs/`** | Toàn bộ tài liệu kiến trúc, quy chuẩn lập trình, hợp đồng API, tài liệu an ninh mạng. | Markdown, OpenAPI YAML | Agent 12 (Documentation) |
| **`.ai/`** | Trạng thái dự án tự động (Machine-readable state), Task backlog, Bug tracker, Locks. | JSON, YAML | Agent 0 (Orchestrator) |
| **`Report/`** | 12 Báo cáo kỹ thuật phân tích chuyên sâu phục vụ tái phát triển và bàn giao. | Markdown Tables & Specs | Agent 12 (Documentation) |
