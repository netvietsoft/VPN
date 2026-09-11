# BÁO CÁO 06: BLUEPRINT TÁI DỰNG TOÀN DIỆN BACKEND, SERVER & CMS (100% PARITY)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 11, 13 trong Convertme.txt
================================================================================

Tài liệu này cung cấp bản thiết kế chi tiết (Blueprint) kiến trúc Backend, Server và CMS tương đương 100% bản gốc, đảm bảo tính kế thừa toàn bộ tính năng và nâng cấp thêm năng lực điều phối proxy cư dân độc lập.

---

## 1. SO SÁNH NĂNG LỰC TÁI DỰNG (FUNCTIONAL PARITY MATRIX)

| Chức Năng Cốt Lõi | Bản Gốc (Decompiled) | Bản Tái Dựng (NextAI Platform) | Mức Độ Parity |
| :--- | :--- | :--- | :---: |
| **Xác thực Người Dùng** | WLVPN Cloud API / Spaceship SSO | JWT Bearer Authentication + Local DB + SSO Hooks | **100%** |
| **Danh Sách Vị Trí Server** | Tải từ `api.wlvpn.com/locations` | Quản lý Node Pool động (Datacenter + Cư dân 234 nước) | **100% + Vượt trội** |
| **Giao Thức Mạng Hỗ Trợ** | WireGuard, OpenVPN (TCP/UDP), IKEv2 | WireGuard NT, OpenVPN Scramble, SOCKS5, HTTP CONNECT | **100%** |
| **Bảo Mật Kill Switch** | WFP Driver (`VpnSDK.Private.WFP.dll`) | WFP Native Driver được bảo tồn 100% nguyên bản | **100%** |
| **Phân Luồng Ứng Dụng** | Split Tunneling (theo Domain & EXE) | NetFilter SDK + WFP Application Rules nguyên bản | **100%** |
| **Giao Diện & Themes** | 113 màn hình XAML, Dark / Light mode | Bảo tồn nguyên vẹn 113 file XAML, 234 cờ, 11 fonts, 215 icons | **100%** |
| **Quản Trị Băng Thông (CMS)**| Phụ thuộc hoàn toàn máy chủ WLVPN | Web CMS nội bộ độc lập trên **cổng 6033** quản lý Quota & Tenants | **Độc lập 100%** |

---

## 2. BLUEPRINT HẠ TẦNG BACKEND & SERVER CỔNG 6033

### A. Cấu Trúc Thành Phần Backend (`Backend/`)
1. **API Gateway Layer**: Lắng nghe trên cổng `6033` (Kestrel .NET 10 Engine):
   - Đảm nhận tiếp nhận các kết nối cấu hình, tra cứu danh sách server, đăng nhập, đo đạc tốc độ ping.
   - Xuất tài liệu Swagger tương tác trực tiếp tại `/swagger` hoặc `/api/v1/health`.
2. **Universal Proxy Engine**: Lắng nghe trên cổng `10000`:
   - Phân tích cú pháp header động: Nhận diện tức thời phiên truyền là SOCKS5 handshake (`0x05`) hay HTTP CONNECT request (`CONNECT host:port HTTP/1.1`).
   - Xử lý xác thực người dùng theo định dạng: `username-country-[code]-session-[id]-time-[minutes]:password`.
3. **Dedicated Port Allocation Service**: Quản lý dải cổng `10001 - 10500`:
   - Phục vụ cấp cổng riêng biệt cho từng profile trình duyệt hoặc khách hàng thuê tháng.
4. **Relational & Quota Schema (In-Memory / SQLite)**:
   - Quản lý tài khoản, mật khẩu, thời hạn thuê, tổng GB cấp phát, số GB đã tiêu thụ.

### B. Blueprint Web CMS Admin (`Backend/cms/` & `wwwroot/`)
- **Theme**: Hỗ trợ chuyển đổi Theme Tối (Dark) và Theme Sáng (Light).
- **Trực quan hóa**:
  - Bản đồ phân bổ Node cư dân theo quốc gia.
  - Bảng điều khiển tài khoản khách hàng (Tenants) với thanh tiến trình băng thông màu sắc trực quan (Xanh: Còn nhiều, Vàng: Sắp hết, Đỏ: Đã khóa).
  - Bộ tạo nhanh chuỗi Proxy (Proxy Generator Tool) cho phép sao chép 1-click vào KikiLogin hoặc các phần mềm nuôi profile khác.
  - Thiết kế Responsive hoàn chỉnh: Tự động co giãn dạng Card khi xem trên điện thoại hoặc màn hình nhỏ.
