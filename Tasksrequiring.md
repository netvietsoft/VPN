# DANH SÁCH THÔNG SỐ CHỜ USER CUNG CẤP (TASKSREQUIRING.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

Bảng dưới đây thống kê các thông số cấu hình, API keys, dịch vụ bên thứ ba mà User cần chuẩn bị và điền giá trị thật khi đưa hệ thống vào sản xuất (Production). Hiện tại hệ thống đang sử dụng giá trị mẫu (Demo/Mock):

| STT | Tên Tham Số / Khóa | Mô Tả Chức Năng | Giá Trị Mẫu Đang Dùng | Giá Trị Thực Của User | Trạng Thái |
| :---: | :--- | :--- | :--- | :--- | :---: |
| 1 | **Publisher Name** | Tên tổ chức/nhà phát triển hiển thị trên App | `nextaitechnology` | _(User xác nhận hoặc đổi)_ | `APPROVED` |
| 2 | **Application Name** | Tên ứng dụng VPN Desktop & Mobile | `NextAI VPN` | _(Chờ User duyệt tên thương mại)_ | `PENDING` |
| 3 | **Package Name (Android)** | Định danh bundle ứng dụng Android | `com.nextaitechnology.vpn` | _(User xác nhận)_ | `APPROVED` |
| 4 | **Firebase Project ID** | Dự án Firebase Push Notification & Analytics | `nextai-vpn-demo` | _(Cần cung cấp google-services.json)_ | `PENDING` |
| 5 | **AdMob App ID** | Quảng cáo trên bản Free Android/Desktop | `ca-app-pub-3940256099942544~3347511713` (Test) | _(User cấp App ID thật)_ | `PENDING` |
| 6 | **AdMob Interstitial Unit** | Vị trí quảng cáo toàn màn hình | `ca-app-pub-3940256099942544/1033173712` (Test) | _(User cấp Unit ID thật)_ | `PENDING` |
| 7 | **Stripe Secret Key** | Cổng thanh toán quốc tế thẻ Visa/Mastercard | `sk_test_mock_nextai_vpn_secret` | _(User cấp Stripe API Key)_ | `PENDING` |
| 8 | **Crypto Payment Gateway** | Cổng nhận USDT / Bitcoin (NowPayments/Coinbase) | `mock_nowpayments_api_key` | _(User cấp API Key Crypto)_ | `PENDING` |
| 9 | **Cloudflare R2 / S3 Keys** | Lưu trữ tệp tin media, logs, file cập nhật | `mock_cf_access_key` / `mock_cf_secret` | _(User cấp CF R2 Credentials)_ | `PENDING` |
| 10 | **WireGuard Server Endpoint** | Địa chỉ IP/Domain máy chủ VPN WireGuard | `vpn.nextaitechnology.com:51820` | _(User cấp IP server WireGuard)_ | `PENDING` |
| 11 | **Bảng Giá Thuê Bao (Pricing)** | Mức giá các gói cước VIP / Cư Dân | Gói Tháng: $4.99, Gói Năm: $39.99 | _(User điều chỉnh giá cước)_ | `PENDING` |
| 12 | **Bugsnag / Sentry API Key** | Báo cáo sự cố và crash logs từ client | `mock_bugsnag_api_key` | _(User cấp API Key Sentry/Bugsnag)_ | `PENDING` |
