# DANH MỤC THƯ VIỆN VÀ SDK ĐƯỢC PHÉP SỬ DỤNG (LIBRARY.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

Tài liệu này quy định rõ các thư viện, SDK được phép sử dụng trong dự án và danh sách các thư viện bị cấm tuyệt đối (theo mục 12 và mục rules của chuẩn V2.1).

---

## 1. THƯ VIỆN ĐƯỢC PHÉP (ALLOWED LIBRARIES & SDKS)

### A. Phân hệ Desktop Client (.NET 10 WPF - `apps/desktop`)
| Thư Viện / Package | Phiên Bản | Mục Đích Sử Dụng | Ghi Chú |
| :--- | :--- | :--- | :--- |
| **Wpf.Ui** | 3.0.0+ | Giao diện chuẩn Windows 11 Fluent Design | Bo góc, hiệu ứng Mica/Acrylic, dark/light theme |
| **Microsoft.Web.WebView2** | Latest | Nhúng trình duyệt Edge Chromium cho web login / payment | An toàn, hiệu năng cao |
| **Serilog** | Latest | Ghi log bất đồng bộ ra console và file | Cấu hình rolling file log |
| **Newtonsoft.Json** / **System.Text.Json** | Latest | Tuần tự hóa JSON models | Tối ưu hóa memory |
| **Dapper** | Latest | ORM siêu nhẹ cho SQLite local cache | Lưu danh sách servers, profile settings |
| **System.Data.SQLite** | Latest | Cơ sở dữ liệu SQLite cục bộ trên máy người dùng | Lưu lịch sử kết nối, cấu hình người dùng |
| **H.NotifyIcon.Wpf** | Latest | Biểu tượng khay hệ thống (System Tray Icon) | Thu nhỏ VPN xuống tray, menu ngữ cảnh |
| **VpnSDK (Native)** | 1.0.0 | Điều khiển WFP Kill Switch, WireGuard NT, OpenVPN | Driver gốc được bảo tồn nguyên vẹn |

### B. Phân hệ Backend & Gateway (`Backend/`)
| Thư Viện / Package | Phiên Bản | Mục Đích Sử Dụng | Ghi Chú |
| :--- | :--- | :--- | :--- |
| **Microsoft.AspNetCore.App** | 10.0 | Framework nền tảng Web API & Kestrel | Xử lý hàng chục nghìn request đồng thời |
| **Swashbuckle.AspNetCore** | Latest | Sinh tài liệu tương tác Swagger / OpenAPI | Xem và test API trên trình duyệt |
| **System.Net.Sockets** | Native .NET 10 | Động cơ SOCKS5 Proxy & HTTP CONNECT Gateway | Bất đồng bộ Task-based Socket Server |

### C. Phân hệ Android Client (`apps/android`)
| Thư Viện / Package | Phiên Bản | Mục Đích Sử Dụng | Ghi Chú |
| :--- | :--- | :--- | :--- |
| **Jetpack Compose** | Latest BOM | Xây dựng giao diện UI Declarative hiện đại | Thay thế toàn bộ XML Views cũ |
| **Kotlin Coroutines & Flow** | Latest | Xử lý bất đồng bộ đa luồng | Quản lý trạng thái kết nối VPN |
| **Hilt / Koin** | Latest | Dependency Injection | Quản lý phụ thuộc module |
| **Retrofit + OkHttp** | Latest | Gọi REST API lên Backend cổng 6033 | Tự động phân tích JSON |

---

## 2. DANH MỤC THƯ VIỆN BỊ CẤM TUYỆT ĐỐI (PROHIBITED & DEPRECATED)

| Thư Viện Bị Cấm | Lý Do Cấm | Thư Viện Thay Thế Bắt Buộc |
| :--- | :--- | :--- |
| **System.Net.HttpWebRequest** | Đã deprecated trong .NET hiện đại, gây blocking luồng | Dùng `System.Net.Http.HttpClient` |
| **log4net** (cũ) | Nguy cơ bảo mật và cấu hình cồng kềnh | Dùng `Serilog` |
| **moment.js** | Không còn phát triển, dung lượng lớn | Dùng `dayjs` hoặc Native DateTime |
| **Newtonsoft.Json** (với các hàm Unsafe TypeNameHandling) | Nguy cơ Remote Code Execution (RCE) | Dùng `System.Text.Json` hoặc cấu hình an toàn |
| **Thư viện bên thứ ba không rõ nguồn gốc** | Nguy cơ mã độc đánh cắp lưu lượng mạng VPN | Bắt buộc kiểm tra qua Security Agent |
