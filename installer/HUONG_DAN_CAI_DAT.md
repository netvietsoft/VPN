# HƯỚNG DẪN CÀI ĐẶT NEXTAI VPN DESKTOP (INSTALLATION GUIDE)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phiên bản: 8.0.0.0 | Nhà phát hành: `nextaitechnology`
================================================================================

Bộ cài đặt độc lập (Standalone Setup Wizard) chuẩn Windows thương mại đã được đóng gói hoàn chỉnh:
- **Tại thư mục installer:** `E:\DECOMPILER\Soft\VPN\CONVERT\installer\NextAiVPN_Setup.exe`
- **Tại thư mục gốc Workspace:** `E:\DECOMPILER\Soft\VPN\CONVERT\NextAiVPN_Setup.exe`

---

## 1. CÁC BƯỚC CÀI ĐẶT NHƯ PHẦN MỀM THƯƠNG MẠI THÔNG THƯỜNG

Khi bạn nhấp đúp vào tệp **`NextAiVPN_Setup.exe`**, cửa sổ giao diện cài đặt (Setup Wizard) hiện đại sẽ mở ra:

1. **Bước 1 - Chào mừng (Welcome Screen):**
   - Hiển thị logo 🛡️ NextAI, thông tin phiên bản v8.0.0 và tóm tắt các tính năng (WireGuard, Universal Gateway cổng 10000, Residential IP Harvester).
   - Bấm **[Tiếp Tục >]**.

2. **Bước 2 - Chọn Vị Trí Cài Đặt & Tùy Chọn (Destination & Options):**
   - **Vị trí mặc định:** 
     - Quản trị viên (Admin): `C:\Program Files\NextAiTechnology\NextAiVPN`
     - Người dùng thông thường: `%LocalAppData%\Programs\NextAiTechnology\NextAiVPN`
   - Bấm nút **[Duyệt...]** nếu muốn đổi ổ đĩa/thư mục cài đặt khác.
   - Tích chọn các tiện ích:
     - ☑️ Tạo biểu tượng ngoài màn hình nền (**Desktop Shortcut**)
     - ☑️ Tạo lối tắt trong Menu Bắt đầu (**Start Menu**)
     - ⬜ Tự động khởi động cùng Windows
   - Bấm **[Cài Đặt >]**.

3. **Bước 3 - Tiến Trình Cài Đặt (Installation Progress):**
   - Thanh tiến trình (Progress Bar) chạy mượt mà từ 0% đến 100%.
   - Trích xuất toàn bộ 75 tệp nhị phân .NET 10, cấu hình driver, tạo shortcuts và đăng ký hệ thống Windows.

4. **Bước 4 - Hoàn Tất (Finish Screen):**
   - Thông báo cài đặt thành công.
   - Tích chọn ☑️ **Khởi chạy NextAI VPN Desktop ngay bây giờ**.
   - Bấm **[Hoàn Tất]** để mở ứng dụng!

---

## 2. GỠ CÀI ĐẶT (UNINSTALLATION)

Phần mềm đã được tích hợp sâu vào hệ thống Windows:
- **Cách 1:** Vào **Windows Settings > Apps > Installed Apps** (hoặc Control Panel > Programs and Features), tìm **"NextAI VPN Desktop & Residential Gateway"** và chọn **Uninstall**.
- **Cách 2:** Chạy lệnh: `NextAiVPN_Setup.exe /uninstall` trong thư mục cài đặt.
- Hệ thống sẽ tự động dọn dẹp sạch sẽ shortcuts, khóa Registry và toàn bộ tệp ứng dụng.

---

## 3. PHƯƠNG ÁN CHẠY NHANH KHÔNG CẦN CÀI ĐẶT (PORTABLE)

Nếu chỉ muốn chạy thử nghiệm trực tiếp từ thư mục mã nguồn:
- Nhấp đúp vào: `Chay_NextAiVPN.bat` ở thư mục gốc.
