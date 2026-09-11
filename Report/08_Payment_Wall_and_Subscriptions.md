# BÁO CÁO 08: PHÂN TÍCH PAYMENT WALL, KỊCH BẢN GÓI CƯỚC & BẢN ĐỒ QUẢNG CÁO
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 16 trong Convertme.txt
================================================================================

Tài liệu này phân tích kịch bản tường thanh toán (Paywall), các gói cước thuê bao, luồng mua hàng trong ứng dụng và bản đồ vị trí quảng cáo (Ads Mapping).

---

## 1. KỊCH BẢN TƯỜNG THANH TOÁN (PAYWALL FLOW)

### A. Luồng Trải Nghiệm Người Dùng (User Journey)
```
[ Người Dùng Mới ] ──> [ Mở App ] ──> [ Màn hình TrialFirstWindow / Free Tier ]
                                               │
               ┌───────────────────────────────┴───────────────────────────────┐
               ▼                                                               ▼
     [ Chọn Gói Cước VIP ]                                           [ Dùng Bản Free Có Giới Hạn ]
               │                                                               │
               ▼                                                               ▼
   [ SubscriptionExpiredWindow ]                                    - Giới hạn 3 vị trí server
   - Gói Tháng: $4.99 / tháng                                       - Tốc độ tối đa 5 Mbps
   - Gói Năm: $39.99 / năm (Tiết kiệm 40%)                         - Quảng cáo banner / Interstitial
               │
               ▼
   [ Thanh Toán Qua WebView2 ]
   - Hỗ trợ Thẻ Quốc Tế (Stripe / PayPal)
   - Hỗ trợ Crypto (USDT / BTC)
               │
               ▼
   [ Kích Hoạt Quyền VIP ] ──> [ Mở Khóa 234 Nước + Tốc Độ Không Giới Hạn + Dedicated Proxy ]
```

### B. Các Màn Hình XAML Quản Lý Thuê Bao Đã Được Bảo Tồn
Nằm tại `apps/desktop/Views_XAML/`:
1. `NamecheapVPN.SubscriptionExpiredWindow.xaml`: Cửa sổ thông báo hết hạn hoặc nâng cấp tài khoản.
2. `NamecheapVPN.SubscriptionExpiredControl.xaml`: Thành phần giao diện hiển thị các tùy chọn gói cước.
3. `NamecheapVPN.SubscriptionExpired_CompletePurchase.xaml`: Màn hình hướng dẫn hoàn tất giao dịch thanh toán.
4. `NamecheapVPN.SubscriptionExpired_MainMessage.xaml`: Thông điệp nhắc nhở gia hạn.
5. `NamecheapVPN.UI.Trial.TrialFirstWindow.xaml`: Màn hình chào mừng và kích hoạt gói dùng thử miễn phí.
6. `NamecheapVPN.UI.Account.PopUp.SubscriptionExpireSoonControl.xaml`: Cảnh báo tài khoản sắp hết hạn trước 3 ngày.

---

## 2. BẢN ĐỒ QUẢNG CÁO (ADS MAPPING) - SẼ THAY SAU
Dành cho phiên bản Mobile Android và bản Free Desktop:

| Vị Trí Quảng Cáo | Định Dạng Ads | Thời Điểm Xuất Hiện | File Cấu Hình / Key Hiện Tại | Vị Trí Thay Sau |
| :--- | :--- | :--- | :--- | :--- |
| **Màn Hình Kết Nối (Connection Screen)** | Banner Ads (320x50) | Hiển thị cố định phía dưới màn hình chính khi đang kết nối | Chưa kích hoạt trên Desktop | `apps/android/res/layout` / Google AdMob Banner |
| **Sau Khi Ngắt Kết Nối (Disconnected)** | Interstitial (Toàn màn hình) | Xuất hiện khi người dùng bấm nút "Ngắt kết nối" (Disconnect) | Mock Ad Unit: `ca-app-pub-3940256099942544/1033173712` | Thay trong `Tasksrequiring.md` mục 6 |
| **Đổi Vị Trí Server (Server Switch)** | Rewarded Video | Người dùng xem video 15s để mở khóa server VIP trong 1 giờ | Mock Ad Unit: `ca-app-pub-3940256099942544/5224354917` | Thay trong `Tasksrequiring.md` |
