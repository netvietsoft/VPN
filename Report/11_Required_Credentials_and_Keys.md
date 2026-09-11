# BÁO CÁO 11: BẢNG THỐNG KÊ CÁC TÀI NGUYÊN, KEYS & CHỨNG CHỈ CẦN CHUẨN BỊ
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 21 trong Convertme.txt
================================================================================

Tài liệu này tổng hợp toàn bộ các khóa API, chứng chỉ số và tài khoản dịch vụ bên thứ ba mà User cần chuẩn bị để cấu hình cho hệ thống sẵn sàng hoạt động trong môi trường thật (Production):

| STT | Tên Dịch Vụ / Khóa Cần Cung Cấp | Mục Đích Sử Dụng | Mức Độ Ưu Tiên | Giá Trị Mẫu Đang Chạy | Nơi Cần Điền Khi Có Key |
| :---: | :--- | :--- | :---: | :--- | :--- |
| 1 | **Tên Thương Hiệu Chính Thức** | Hiển thị trên Title bar, About, Website CMS | **BẮT BUỘC** | `NextAI VPN` / `nextaitechnology` | `appsettings.json`, XAML Strings |
| 2 | **EV Code Signing Certificate** | Ký số file EXE và SYS driver tránh cảnh báo SmartScreen của Windows Defender | **QUAN TRỌNG** | Chưa ký (Self-signed test cert) | CI/CD Build Pipeline (`.pfx` file) |
| 3 | **Domain & Chứng chỉ SSL/TLS** | Tên miền chính cho API và Web CMS (Let's Encrypt / Cloudflare) | **QUAN TRỌNG** | `127.0.0.1:6033` (Local Dev) | Nginx / Caddy Reverse Proxy |
| 4 | **Cloudflare R2 / S3 Storage Keys** | Lưu trữ tệp tin media, logs, file cập nhật phần mềm tự động | **TRUNG BÌNH** | `mock_cf_access_key` | `Backend/appsettings.json` |
| 5 | **Firebase Credentials (`google-services.json`)** | Gửi thông báo đẩy Push Notifications xuống Android/Desktop | **TRUNG BÌNH** | `mock_firebase_config` | `apps/android/app/google-services.json` |
| 6 | **Google AdMob App ID & Unit IDs** | Hiển thị quảng cáo trên phiên bản Free | **TÙY CHỌN** | Test Ad IDs của Google | `apps/android/AndroidManifest.xml` |
| 7 | **Stripe API Keys (`pk_live`, `sk_live`)** | Tiếp nhận thanh toán thẻ quốc tế Visa/Mastercard tự động | **QUAN TRỌNG** | `sk_test_mock_secret` | `Backend/appsettings.json` |
| 8 | **Crypto Payment API Key** | Cổng nhận thanh toán USDT qua mạng TRC20/BEP20 (NowPayments/Coinbase Commerce) | **TÙY CHỌN** | `mock_crypto_key` | `Backend/appsettings.json` |
| 9 | **Sentry / Bugsnag DSN Key** | Báo cáo crash logs và lỗi kết nối từ client về trung tâm | **TRUNG BÌNH** | `mock_bugsnag_key` | `apps/desktop/App.xaml.cs` |
| 10 | **Hạ Tầng Máy Chủ WireGuard** | Danh sách IP Public và Public Key của các server VPN trung chuyển | **BẮT BUỘC** | Dữ liệu giả lập 10 Node mẫu | Database / `Backend/Services/NodePoolService.cs` |
