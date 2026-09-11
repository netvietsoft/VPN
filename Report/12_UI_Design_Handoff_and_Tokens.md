# BÁO CÁO 12: TÀI LIỆU HANDOFF THIẾT KẾ GIAO DIỆN (UI DESIGN HANDOFF & TOKENS)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 22 trong Convertme.txt
Mục đích: Cung cấp tài liệu thông số chính xác để User / AI Designer tái thiết kế giao diện sau này
================================================================================

Tài liệu này chứa thông số kỹ thuật chuẩn về bố cục, kích thước cửa sổ, mã màu Hex, phông chữ, bán kính bo góc (Corner Radius) và liên kết tài nguyên đồ họa để tái thiết kế giao diện Desktop và Web.

---

## 1. THÔNG SỐ KHUNG CỬA SỔ CHÍNH (WINDOW METRICS)

| Thông Số | Giá Trị Chuẩn | Tệp XAML Định Nghĩa | Ghi Chú Kỹ Thuật |
| :--- | :--- | :--- | :--- |
| **Kích Thước Thu Gọn (Collapsed)** | `Width = 370px`, `Height = 575px` | `MainWindow.xaml` | Chế độ mặc định khi VPN hoạt động nền |
| **Kích Thước Mở Rộng (Expanded)** | `Width = 720px`, `Height = 575px` | `VPNWindowExpanded.xaml` | Chế độ khi mở danh sách Server hoặc Settings |
| **Thanh Tiêu Đề (WindowChrome)** | `CaptionHeight = 34px` | `MainWindow.xaml:L4` | Cửa sổ không viền chuẩn Windows 11 Fluent |
| **Vị Trí Khởi Động** | `CenterScreen` | `MainWindow.xaml:L2` | Tự động căn giữa màn hình Desktop |
| **Chế Độ Thu Phóng (ResizeMode)** | `CanMinimize` | `MainWindow.xaml:L2` | Cố định tỷ lệ, chỉ cho phép thu nhỏ xuống Tray |

---

## 2. BẢNG MÃ MÀU TOKENS ĐẶC TẢ (SEMANTIC COLOR TOKENS)
Trích xuất từ `colorbrush.xaml` và `theme.dark.xaml`:

| DynamicResource Key | Mã Màu Hex | Vai Trò & Vị Trí Áp Dụng |
| :--- | :---: | :--- |
| `{DynamicResource Black}` | `#000000` | Nền lớp che Modal bóng mờ (Opacity 0.12 - 0.5) |
| `{DynamicResource MainWindowBackground}` | `#1E1E20` | Nền chính của cửa sổ MainWindow |
| `{DynamicResource CardBackground}` | `#262629` | Nền các khối thẻ chức năng và bảng vị trí |
| `{DynamicResource HoverBackground}` | `#323236` | Hiệu ứng khi rê chuột vào hàng hoặc nút bấm |
| `{DynamicResource ColorBrushB1B1B2}` | `#B1B1B2` | Màu chữ phụ, mô tả, nhãn phân cấp |
| `{DynamicResource ColorBrushWhite}` | `#FFFFFF` | Màu chữ tiêu đề, biểu tượng sáng |
| `{DynamicResource PrimaryBlue}` | `#2563EB` | Màu nút Connect và các tác vụ chính |
| `{DynamicResource ConnectedGreen}` | `#10B981` | Đèn trạng thái VPN đã kết nối an toàn |
| `{DynamicResource ErrorRed}` | `#EF4444` | Đèn báo ngắt kết nối hoặc sự cố mạng |
| `{DynamicResource BorderBrushDark}` | `#59595F` | Viền khung nút bấm và đường phân cách |

---

## 3. QUY CHUẨN TYPOGRAPHY & FONT SIZES
Trích xuất từ `fonts.xaml`:
- **Font Gia Đình Chủ Đạo**: `{DynamicResource MuseoSans500FontFamily}` (`museosans_500.otf`).
- **Font Gia Đình Tiêu Đề**: `{DynamicResource MuseoSans700FontFamily}` (`museosans_700.otf`).
- **Thang Kích Thước Chữ (Type Scale)**:
  - `FontSize = 24px`: Thông số tốc độ tải (Mbps) và IP lớn.
  - `FontSize = 18px`: Tiêu đề màn hình và tên nước trên Header.
  - `FontSize = 16px`: Tên vị trí server trong danh sách, nhãn nút chính.
  - `FontSize = 14px`: Văn bản hướng dẫn trong cài đặt.
  - `FontSize = 12px`: Nhãn phụ, thời gian ping (ms), phiên bản phần mềm.

---

## 4. QUY CHUẨN BO GÓC & NÚT BẤM (CORNER RADIUS & CONTROLS)
- **Nút Hành Động Lớn (Action Buttons)**: `CornerRadius = 30` (ví dụ: Nút Đăng Nhập, Nút Kết Nối bo tròn viên thuốc).
- **Thẻ và Khối Nội Dung (Cards & Panels)**: `CornerRadius = 16`.
- **Hộp Nhập Liệu (TextBox / ComboBox)**: `CornerRadius = 8`.
- **Nút Công Tắc (ToggleSwitch)**: `CornerRadius = 15`.

---

## 5. HƯỚNG DẪN REDESIGN DÀNH CHO AI / DESIGNER (HOW TO REDESIGN)
Khi User hoặc AI tiến hành thiết kế lại giao diện trong tương lai:
1. **Thay đổi Theme tổng thể**: Chỉ cần chỉnh sửa mã màu Hex trong `apps/desktop/resources/styles/theme.dark.xaml` và `theme.light.xaml`. Toàn bộ 113 màn hình sử dụng `DynamicResource` sẽ tự động cập nhật đồng loạt theo phong cách mới!
2. **Thay đổi Layout màn hình**: Mở trực tiếp các file XAML tương ứng trong `apps/desktop/Views_XAML/`.
3. **Thay thế Logo & Icon**: Đặt file ảnh mới cùng tên vào `apps/desktop/assets/` để ghi đè (ví dụ: `vpn_icon.ico`, `logo.png`).
4. **Không làm hỏng Data Binding**: Khi sửa XAML, giữ nguyên các thuộc tính `Binding` (như `{Binding CurrentLocation}`, `{Binding IsConnected}`, `{Binding PingTime}`) để tầng ViewModel C# tiếp tục liên kết dữ liệu mượt mà.
