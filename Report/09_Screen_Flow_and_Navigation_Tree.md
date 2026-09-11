# BÁO CÁO 09: CẤU TRÚC LUỒNG MÀN HÌNH & CÂY ĐIỀU HƯỚNG MENU / TABS
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 17, 18 trong Convertme.txt
================================================================================

Tài liệu này vẽ sơ đồ luồng di chuyển giữa các màn hình (Screen Flow) và cây phân cấp Menu, Tab từ cấp cha xuống cấp con trong ứng dụng Client.

---

## 1. SƠ ĐỒ LUỒNG MÀN HÌNH CHÍNH (SCREEN FLOW DIAGRAM)

```
                       [ MÀN HÌNH KHỞI ĐỘNG (Splash / Init) ]
                                         │
                                         ▼
                     [ XÁC THỰC (Authentication Gate) ]
                                         │
                     ┌───────────────────┴───────────────────┐
                     ▼                                       ▼
        [ SignInWindow / SignUpWindow ]         [ SpaceshipSignInWindow (SSO) ]
                     │                                       │
                     └───────────────────┬───────────────────┘
                                         ▼
                 [ MÀN HÌNH CHÍNH (MainWindow / VPNWindowExpanded) ]
                                         │
    ┌──────────────┬─────────────────────┼─────────────────────┬──────────────┐
    ▼              ▼                     ▼                     ▼              ▼
[ Nút Connect ]  [ SideMenu ]     [ Locations Panel ]  [ Header Control ] [ Quick Tools ]
    │              │                     │                     │              │
    ▼              ▼                     ▼                     ▼              ▼
Toggle VPN     Xem chi tiết         Chọn Server           Thu nhỏ / Đóng   Đổi Theme
(WireGuard /   các tính năng       (234 Quốc Gia,         khay hệ thống    (Dark/Light)
 OpenVPN)      nâng cao            Streaming, Fav)
```

---

## 2. CÂY PHÂN CẤP MENU & TABS TỪ CHA XUỐNG CON (NAVIGATION TREE)

### Cấp 1: `MainWindow` / `VPNWindowExpanded` (Cửa sổ mẹ)
- **Top Header (`WindowHeader.xaml`)**:
  - Logo ứng dụng NextAI VPN.
  - Biểu tượng thu nhỏ (Minimize), Đóng (Close to System Tray).
  - Tình trạng tài khoản (Free / VIP badge).
- **Side Navigation Menu (`ExpandedSideMenu.xaml`)**:
  - ├── **Tab 1: Vị Trí & Máy Chủ (`ExpandedLocations.xaml`)**
    - ├── **Tab con 1.1: Tất Cả Vị Trí (`AllLocationsControl.xaml`)**
      - Danh sách 234 quốc gia xếp theo thứ tự A-Z kèm cờ quốc gia (`AllLocationListItem.xaml`).
      - Tìm kiếm tức thì theo tên nước, thành phố.
      - Hiển thị độ trễ Ping (ms) và tải của server (Capacity %).
    - ├── **Tab con 1.2: Vị Trí Yêu Thích (`FavoriteLocationsTab.xaml`)**
      - Danh sách các server người dùng đã ghim ngôi sao (`FavoriteLocationListItem.xaml`).
    - └── **Tab con 1.3: Máy Chủ Tối Ưu Cho Streaming (`StreamingLocations.xaml`)**
      - Các server tối ưu vượt tường lửa Netflix, Disney+, BBC iPlayer (`FavoriteStreamingLocationsControl.xaml`).
  - ├── **Tab 2: Cài Đặt Giao Thức & Mạng (`SettingsMainControl.xaml`)**
    - ├── **Tab con 2.1: Giao Thức Kết Nối (`ExpandedProtocolsControl.xaml` / `ExpandedProtocolSettings.xaml`)**
      - Tùy chọn WireGuard (Mặc định - Nhanh nhất).
      - Tùy chọn OpenVPN TCP (Vượt tường lửa chặt chẽ).
      - Tùy chọn OpenVPN UDP (Tốc độ truyền dữ liệu cao).
      - Tính năng Scramble (Làm mờ chữ ký gói tin chống Deep Packet Inspection - DPI).
    - ├── **Tab con 2.2: Phân Luồng Ứng Dụng (`SplitTunnelingMainControl.xaml`)**
      - Phân luồng theo Phần mềm EXE (`SplitTunnelingAppWindow.xaml`): Chọn app nào đi qua VPN, app nào đi mạng thường.
      - Phân luồng theo Tên miền Web (`SplitTunnelingDomainWindow.xaml`): Danh sách domain bỏ qua VPN.
    - └── **Tab con 2.3: Mạng Tin Cậy & Kill Switch (`TrustedNetworksWindow.xaml`)**
      - Tự động bật VPN khi kết nối Wi-Fi công cộng không an toàn.
      - Kích hoạt Kill Switch mức Kernel qua WFP.
  - ├── **Tab 3: Giao Diện & Trải Nghiệm (`ThemeAppearanceControl.xaml`)**
    - Tùy chọn Chế độ Tối (Dark Theme).
    - Tùy chọn Chế độ Sáng (Light Theme).
    - Tùy chọn Tự động theo giao diện Windows.
  - ├── **Tab 4: Tài Khoản & Gói Cước (`ExpandedAccount.xaml`)**
    - Thông tin email, ngày hết hạn gói cước, số thiết bị đang dùng (`DeviceLimitWindow.xaml`).
    - Nút Nâng cấp / Gia hạn gói cước (`SubscriptionExpiredWindow.xaml`).
  - ├── **Tab 5: Nhật Ký Hoạt Động (`ExpandedLogsView.xaml`)**
    - Hiển thị log thời gian thực, nút xóa log (`DeleteLogsMessageWindow.xaml`).
  - └── **Tab 6: Trợ Giúp & Phản Hồi (`ExpandedGetHelp.xaml` / `FeedbackWindow.xaml`)**
    - Báo cáo lỗi (`ProblemReportWindow.xaml`), khảo sát mức độ hài lòng (`FeedbackControl.xaml`).
