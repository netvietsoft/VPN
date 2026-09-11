# BÁO CÁO 07: KIỂM TOÁN TẬP TIN, KHÓA NHẠY CẢM & BÍ MẬT HỆ THỐNG
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 12 trong Convertme.txt
================================================================================

Tài liệu này ghi nhận kết quả rà soát sâu toàn bộ các tập tin cấu hình, manifest, chứng chỉ số và chuỗi ký tự trong mã nguồn đã dịch ngược để xác định mọi khóa nhạy cảm và mức độ rủi ro.

---

## 1. BẢNG THỐNG KÊ KHÓA & THÔNG TIN NHẠY CẢM TÌM THẤY

| STT | Khóa / Dữ Liệu Tìm Thấy | Nơi Lưu Trữ (File Path) | Mức Độ Nhạy Cảm | Mô Tả & Khuyến Nghị Xử Lý |
| :---: | :--- | :--- | :---: | :--- |
| 1 | **Driver Publisher Certificate** | `extracted/.../tapwlvpn/Drivers/driver_publisher.cer` | **CAO (HIGH)** | Chứng chỉ công khai dùng xác thực driver mạng TAP. Không chứa Private Key. Cần ký bằng chứng chỉ EV Code Signing mới của `nextaitechnology` khi phát hành chính thức. |
| 2 | **OpenVPN CA Certificate** (`ca.crt`) | `extracted/.../tapwlvpn/ca.crt` | **TRUNG BÌNH (MEDIUM)** | Chứng thư Root CA để bắt tay TLS trong OpenVPN. Khi dựng backend VPN riêng, cần thay bằng CA mới tự sinh. |
| 3 | **Spaceship OAuth Client ID** | Mã nguồn `SpaceshipSignInWindow` | **TRUNG BÌNH (MEDIUM)** | Client ID xác thực OAuth của Spaceship. Cần chuyển sang cấu hình biến môi trường hoặc thay bằng hệ thống Auth của NextAI. |
| 4 | **Bugsnag Telemetry Key** | Cấu hình App / Serilog Bugsnag Sink | **THẤP (LOW)** | API key nhận crash log. Đã thay thế bằng `mock_bugsnag_api_key` trong `Tasksrequiring.md`. |
| 5 | **Hardcoded Server IP Ranges** | `VpnSDK.Common.dll` / Resource files | **TRUNG BÌNH (MEDIUM)** | Danh sách dải IP máy chủ của bên phát hành cũ. Đã được chuyển hóa sang động thông qua API `/api/v1/residential/nodes`. |
| 6 | **Default Local Admin Credentials** | `Backend/Services/TenantService.cs` | **TRUNG BÌNH (MEDIUM)** | Tài khoản mặc định: `admin` / `NextAI@2026!Secure`. Khuyến nghị User đổi ngay trong môi trường Production. |

---

## 2. KẾ HOẠCH BẢO VỆ & CÔ LẬP KHÓA
- **Không lưu trữ Secret trong Git**: Toàn bộ các khóa nhạy cảm thật được liệt kê trong [Tasksrequiring.md](Tasksrequiring.md) để User bổ sung độc lập.
- **Mã hóa file cấu hình cục bộ**: Khi Desktop Client lưu trữ thông tin đăng nhập của người dùng, sử dụng Windows Data Protection API (`DPAPI` - `ProtectedData.Protect`) để đảm bảo chỉ đúng người dùng và máy tính đó mới có thể giải mã.
