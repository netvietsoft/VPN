# BÁO CÁO 01: DANH SÁCH API ENDPOINTS, EXTERNAL DOMAINS, IP & PORTS
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 3, 4, 4b trong Convertme.txt
================================================================================

## 1. Danh Sách API Endpoints Trích Xuất Từ Bản Gốc (Decompiled Endpoints)
Các API gốc phát hiện trong quá trình phân tích mã nguồn FastVPN / WLVPN:

| Endpoint Gốc | Phương thức | Chức Năng & Nhiệm Vụ | Sử Dụng Ngay Được Không? | Mô Tả & Models Trả Về |
| :--- | :---: | :--- | :---: | :--- |
| `https://api.wlvpn.com/v2/user/authenticate` | `POST` | Xác thực tài khoản người dùng, cấp JWT Bearer token | **Không** (Cần API Key WLVPN cũ) | `AuthResponse { token, refresh_token, user_id, email, status }` |
| `https://api.wlvpn.com/v2/locations` | `GET` | Lấy danh sách toàn bộ server VPN, quốc gia, thành phố, ping | **Không** (Chuyển sang Backend nội bộ) | `LocationList { locations: [{ id, country_code, city, ip, capacity }] }` |
| `https://api.wlvpn.com/v2/user/subscription` | `GET` | Kiểm tra trạng thái gói cước VIP, ngày hết hạn | **Không** (Thay bằng Quota Engine nội bộ) | `SubscriptionInfo { is_active, plan_name, expires_at, devices_limit }` |
| `https://api.spaceship.com/v1/auth/login` | `POST` | Đăng nhập tài khoản Spaceship SSO | **Không** (Khách hàng bên ngoài) | `SsoResponse { access_token, user_profile }` |
| `https://notify.bugsnag.com/` | `POST` | Gửi báo cáo crash và lỗi runtime của client | **Được** (nếu có API Key Bugsnag mới) | `BugsnagPayload { apiKey, events: [...] }` |

---

## 2. Danh Sách API Endpoints Thay Thế Nội Bộ (NextAI Backend Port 6033)
Toàn bộ các chức năng trên đã được tái dựng thành công và sẵn sàng phục vụ trực tiếp trên **cổng 6033**:

| Endpoint Mới | Method | Chức Năng & Nhiệm Vụ | Sẵn Sàng? | Request / Response Model |
| :--- | :---: | :--- | :---: | :--- |
| `/api/v1/health` | `GET` | Kiểm tra tình trạng server, Uptime, tổng số Node, lượng data đã phục vụ | **CÓ** (100%) | `ApiResponse<SystemHealthData>` |
| `/api/v1/residential/nodes` | `GET` | Trả về danh sách Node Cư Dân theo Quốc Gia, Thành Phố, ISP, Fraud Score | **CÓ** (100%) | `ApiResponse<NodeGroupList>` |
| `/api/v1/tenants` | `GET` | Lấy danh sách khách hàng thuê, hạn ngạch (Quota), dung lượng đã dùng | **CÓ** (100%) | `ApiResponse<IReadOnlyList<Tenant>>` |
| `/api/v1/tenants` | `POST` | Tạo mới khách hàng hoặc kích hoạt gói cước | **CÓ** (100%) | `CreateTenantRequest` -> `Tenant` |
| `/api/v1/proxy/generate` | `POST` | Tạo chuỗi kết nối Proxy (Random IP, Country, Sticky Session) | **CÓ** (100%) | `GenerateProxyRequest` -> `ProxyStrings` |
| `/api/v1/dedicated/allocate` | `POST` | Cấp phát cổng SOCKS5 riêng biệt (Dải 10001 - 10500) | **CÓ** (100%) | `AllocatePortRequest` -> `PortInfo` |
| `/api/v1/dedicated/ports` | `GET` | Danh sách các cổng riêng đang mở | **CÓ** (100%) | `ApiResponse<List<DedicatedPortInfo>>` |
| `/api/v1/dedicated/{port}` | `DELETE` | Giải phóng / thu hồi cổng riêng | **CÓ** (100%) | `ApiResponse<object>` |

---

## 3. Danh Sách External Domains, Địa Chỉ IP & Cổng (Domains & External Ports)
Phát hiện trong mã nguồn decompile và cấu hình mạng:

| Tên Miền / Dịch Vụ Bên Ngoài | Địa Chỉ IP Mặc Định | Cổng | Giao Thức | Mục Đích Sử Dụng |
| :--- | :--- | :---: | :---: | :--- |
| `api.wlvpn.com` | `104.16.x.x` (Cloudflare) | `443` | HTTPS | Server API gốc của WLVPN |
| `auth.spaceship.com` | `172.67.x.x` (Cloudflare) | `443` | HTTPS | Xác thực OAuth SSO Spaceship |
| `notify.bugsnag.com` | `34.250.x.x` (AWS) | `443` | HTTPS | Crash reporting telemetry |
| `1.1.1.1` / `1.0.0.1` | Cloudflare DNS Resolver | `53`, `853` | UDP/TCP/DoT | DNS an toàn chống rò rỉ |
| `8.8.8.8` / `8.8.4.4` | Google Public DNS | `53` | UDP/TCP | DNS dự phòng |
| `WireGuard Gateway Relays` | `10.0.0.1/24` (Internal Mesh) | `51820` | UDP | Đường hầm VPN mã hóa tốc độ cao |
| `OpenVPN TAP Relays` | Dynamic Gateway IPs | `1194`, `443` | UDP/TCP | Đường hầm OpenVPN với tính năng Scramble |
