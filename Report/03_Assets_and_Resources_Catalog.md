# BÁO CÁO 03: DANH MỤC TOÀN DIỆN TÀI NGUYÊN ĐỒ HỌA, PHÔNG CHỮ & STYLES (ASSETS CATALOG)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 8 trong Convertme.txt
================================================================================

Tài liệu này kiểm kê và phân loại 100% tài nguyên đồ họa, phông chữ và file định kiểu (styles) đã được trích xuất và bảo tồn trọn vẹn tại thư mục `apps/desktop/`.

---

## 1. TỔNG HỢP SỐ LƯỢNG TÀI NGUYÊN (SUMMARY METRICS)
- **Tổng số cờ quốc gia (Flags)**: **234 file** PNG độ nét cao (tương ứng chuẩn ISO 3166-1 alpha-2).
- **Tổng số phông chữ (Fonts)**: **11 file** font chuyên nghiệp (Museo Sans & Helvetica LT).
- **Tổng số tài nguyên hình ảnh & icons (Assets)**: **215 file** raster và vector.
- **Tổng số file XAML giao diện & styles**: **113 file XAML màn hình** + **5 file Resource Dictionaries**.

---

## 2. DANH MỤC CỜ QUỐC GIA (234 FLAGS - `apps/desktop/resources/flags/`)
Được đặt tên theo mã ISO-2 tiêu chuẩn quốc tế:
- **Châu Á & Thái Bình Dương**: `vn.png` (Việt Nam), `jp.png` (Nhật Bản), `kr.png` (Hàn Quốc), `sg.png` (Singapore), `th.png` (Thái Lan), `my.png` (Malaysia), `id.png` (Indonesia), `ph.png` (Philippines), `in.png` (Ấn Độ), `au.png` (Úc), `nz.png` (New Zealand), v.v.
- **Châu Mỹ**: `us.png` (Mỹ), `ca.png` (Canada), `br.png` (Brazil), `mx.png` (Mexico), `ar.png` (Argentina), `cl.png` (Chile), v.v.
- **Châu Âu**: `gb.png` (Vương Quốc Anh), `de.png` (Đức), `fr.png` (Pháp), `nl.png` (Hà Lan), `se.png` (Thụy Điển), `ch.png` (Thụy Sĩ), `it.png` (Ý), `es.png` (Tây Ban Nha), `fi.png` (Phần Lan), `no.png` (Na Uy), `ru.png` (Nga), `ua.png` (Ukraine), v.v.
- **Cờ đặc biệt**: `earth.png` / `global.png` (biểu tượng chọn server ngẫu nhiên/nhanh nhất thế giới).

---

## 3. DANH MỤC PHÔNG CHỮ BẢO TỒN (11 FONTS - `apps/desktop/fonts/`)
Toàn bộ font chữ phục vụ cho thiết kế UI của phần mềm:
1. `museosans-300.otf`: Museo Sans 300 (Light) - Dành cho phụ đề, hướng dẫn tinh tế.
2. `museosans_500.otf`: Museo Sans 500 (Regular) - Dành cho toàn bộ nội dung text, menu, nút bấm.
3. `museosans_700.otf`: Museo Sans 700 (Bold) - Dành cho tiêu đề lớn, thông số IP, trạng thái kết nối.
4. `helvetica-20lt-2025-20ultra-20light.ttf`: Helvetica LT Ultra Light.
5. `helvetica-20lt-2035-20thin.ttf`: Helvetica LT Thin.
6. `helvetica-20lt-2045-20light.ttf`: Helvetica LT Light.
7. `helvetica-20lt-2055-20roman.ttf`: Helvetica LT Roman.
8. `helvetica-20lt-2065-20medium.ttf`: Helvetica LT Medium.
9. `helvetica-20lt-2075-20bold.ttf`: Helvetica LT Bold.
10. `helvetica-20lt-2085-20heavy.ttf`: Helvetica LT Heavy.
11. `segoe_ui.ttf`: Segoe UI - Phông chữ hệ thống tiêu chuẩn Windows.

---

## 4. DANH MỤC HÌNH ẢNH & BIỂU TƯỢNG (215 ASSETS - `apps/desktop/assets/`)
Được tổ chức theo các nhóm chức năng:
- **Nhóm Điều Hướng & Cửa Sổ (Window Controls)**:
  - `btnexpand.png`, `btncollapse.png`: Mở rộng / thu gọn panel bên phải.
  - `btncrossdefault.png`, `btncrosshover.png`: Nút đóng cửa sổ trạng thái bình thường và hover.
  - `carretdown.png`, `carretup.png`: Mũi tên dropdown sổ xuống / thu lên.
  - `arrowbacknormal.png`, `arrowbackclicked.png`: Nút quay lại (Back navigation).
  - `chevron.png`, `chevron-right.png`: Mũi tên chỉ mục menu con.
- **Nhóm Trạng Thái Kết Nối (Connection States)**:
  - `connected.png`, `connectedplain.png`: Biểu tượng khi đã kết nối VPN bảo mật thành công.
  - `backgrounderror.png`, `errortap.png`: Đồ họa cảnh báo khi lỗi mạng hoặc lỗi driver.
  - `wifi-white.png`, `ethernet-white.png`: Biểu tượng loại kết nối mạng LAN / Wi-Fi.
  - `warning.png`, `notification-nonetwork.png`: Cảnh báo mất kết nối Internet.
- **Nhóm Menu & Tabs (Side Menu Icons)**:
  - `locations_normal.png`, `locationssidemenunotactive.png`: Icon danh sách server / vị trí.
  - `settingssidemenu.png`: Icon cài đặt cấu hình.
  - `accountsidemenu.png`, `accountsidemenunotactive.png`: Icon tài khoản người dùng.
  - `logssidemenunotactive.png`: Icon nhật ký hoạt động.
  - `gethelp.png`: Icon trợ giúp kỹ thuật.
  - `sendfeedbackicon.png`: Icon đóng góp ý kiến.
- **Nhóm Yêu Thích & Tối Ưu Mạng (Favorites & Features)**:
  - `addfavorite.png`, `favselected.png`, `favnormal.png`, `favhover.png`: Icon đánh dấu server yêu thích.
  - `st_domaincheck.png`, `st_includesubdomainsinfo.png`: Icon tính năng Split Tunneling theo tên miền.
- **Nhóm Biểu Cảm Phản Hồi (Feedback Emojis)**:
  - `mad.png`, `mah.png`, `1.png`, `2.png`: Bộ icon cảm xúc khảo sát trải nghiệm người dùng.
- **Thư mục con chuyên biệt**:
  - `assets/darkmode/`: 48 assets được tinh chỉnh riêng cho nền đen sâu.
  - `assets/fastvpn/`: 8 assets logo, thương hiệu và biểu đồ đo tốc độ `datadownload.png`.
  - `assets/notifications/`: 12 assets chuông thông báo, menu dấu 3 chấm `dotsmenu.png`.

---

## 5. CÁC TẬP TIN STYLES ĐỊNH KIỂU (XAML RESOURCE DICTIONARIES)
Nằm tại `apps/desktop/resources/styles/`:
1. `theme.dark.xaml` (67,063 bytes): Định nghĩa toàn bộ màu nền, text foreground, border, template cho Button, TextBox, ComboBox, CheckBox, ToggleSwitch, Slider trên nền tối.
2. `theme.light.xaml` (63,863 bytes): Định nghĩa phong cách giao diện sáng tương ứng.
3. `colorbrush.xaml` (4,471 bytes): Bảng biến màu (SolidColorBrush) dạng semantic tokens.
4. `fonts.xaml` (697 bytes): Khai báo FontFamily ánh xạ trực tiếp tới các file trong `fonts/`.
5. `scrollbarstyle.xaml` (3,715 bytes): Thanh cuộn siêu mỏng bo tròn hiện đại theo phong cách Windows 11.
