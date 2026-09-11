# BUG-002: SideMenu Tabs (Protocol, Logs, Help, Legal) Không Chuyển Màn Hình & Nền Trắng Lỗi Độ Tương Phản Trên Settings
================================================================================
* **Mã Bug**: BUG-002
* **Mức độ nghiêm trọng**: Medium / UI State & Theme Inconsistency
* **Trạng thái**: **FIXED & VERIFIED**
* **Phát hiện bởi**: Agent 6 (TESTER) qua UI Automation Snapshot QA
* **Khắc phục bởi**: Agent 7 (FIXER)
* **Kiểm định bởi**: Agent 8 (REVIEWER) & Agent 0 (ORCHESTRATOR)
================================================================================

## 1. Mô tả Lỗi (Bug Description)
* **Triệu chứng 1**: Khi người dùng hoặc AutoTest click vào các mục trên SideMenu gồm `Protocol`, `Log Details`, `Get Help`, `Legal Information`, icon và text bên trái chuyển sang trạng thái active (in đậm), nhưng màn hình ở cột giữa không thay đổi mà vẫn giữ nguyên màn hình trước đó (ví dụ: màn hình Settings).
* **Triệu chứng 2**: Tại màn hình `Settings`, phần nội dung bên dưới có nền trắng (`#FFFFFF`) hiển thị đè lên giao diện, khiến các dòng chữ "Auto-Protect", "Trusted Wi-Fi networks" bị chìm (màu xám nhạt trên nền trắng), phá vỡ phong cách Dark Mode tổng thể.

## 2. Phân tích Nguyên nhân Gốc (Root Cause Analysis)
* **Nguyên nhân 1 (`ExpandedSideMenu.cs`)**: Trong hàm `SetMenuOption(SideMenuOption option)`, các case `Protocol`, `LogDetails`, `GetHelp`, `LegalInformation` chỉ cập nhật icon và foreground chữ, nhưng bị thiếu lệnh gọi `SetSettingsOption(option);` (hàm này có nhiệm vụ ẩn màn hình cũ và bật `Visibility = Visibility.Visible` cho màn hình tương ứng).
* **Nguyên nhân 2 (`VPNWindowExpanded.xaml`)**: `Window` chính và `MainGrid` không khai báo `Background="{DynamicResource Color1D1D20}"`, dẫn đến khi các UserControl con (như `SettingsMainControl`) không có nền riêng sẽ bị lộ màu nền mặc định của WPF Window trên Windows (màu trắng).

## 3. Giải pháp Khắc phục (Resolution)
1. Trong `ExpandedSideMenu.cs`: Bổ sung lệnh `SetSettingsOption(option);` vào 4 case:
   - `SideMenuOption.Protocol`
   - `SideMenuOption.LogDetails`
   - `SideMenuOption.GetHelp`
   - `SideMenuOption.LegalInformation`
2. Trong `VPNWindowExpanded.xaml`: Bổ sung `Background="{DynamicResource Color1D1D20}"` trên thẻ `<Window>` và `<Grid Name="MainGrid">` để đồng bộ 100% màu nền Dark Theme toàn diện.

## 4. Kiểm thử Xác minh Nghiệm thu (Verification & QA Proof)
* Build thành công 100% (0 errors).
* Chạy lại bộ kiểm thử tự động `AutoTestAgent`:
  - 📸 `02_menu_settings.png`: Nền đen than `#1D1D20` mịn màng, chữ và toggle switch sắc nét.
  - 📸 `03_menu_protocols.png`: Màn hình cấu hình Protocol hiển thị đúng đắn.
  - 📸 `04_menu_logs.png`: Màn hình Log Details chuyển tab tức thì, nạp và hiển thị toàn bộ log hệ thống thời gian thực.
