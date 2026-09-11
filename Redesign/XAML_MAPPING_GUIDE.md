# NEXTAIVPN CLEAN LIGHT & EMERALD GREEN UI REDESIGN & XAML MAPPING GUIDE
================================================================================
Dự án / Project: NextAI VPN Platform & Residential Gateway Mesh
Thư mục thiết kế / Design Folder: `Redesign/`
Phiên bản / Version: 2.2 (Clean Modern Light & Deep Emerald Edition)
Ngôn ngữ / Language: Song ngữ (Tiếng Việt + English)
================================================================================

## 1. TỔNG QUAN THIẾT KẾ MỚI / DESIGN OVERVIEW

Bản thiết kế mới giải quyết triệt để yêu cầu về thẩm mỹ: **Nền Trắng & Xám Nhạt Sạch Sẽ Hiện Đại (`#ffffff`, `#f8fafc`)** kết hợp với **Điểm Nhấn Nút Bấm, Chữ & Icon Tông Xanh Ngọc Lục Bảo (`#059669`, `#047857`, `#064e3b`)**. 

Bổ sung đầy đủ 2 phân hệ trọng tâm:
1. **Phân hệ Đăng Nhập (Authentication Modal / Drawer)**: Hỗ trợ đăng nhập tài khoản Email/Password, Spaceship Single Sign-On và chế độ **1-Click Auto-Bypass Login** (vào thẳng không cần tài khoản).
2. **Trung Tâm Thông Báo (Notifications Center & Toast Alerts)**: Biểu tượng chuông với số lượng chưa đọc, danh sách sự kiện kết nối/cập nhật proxy và thông báo nổi góc dưới bên phải.

---

## 2. DANH MỤC FILE NGUYÊN MẪU / PROTOTYPE FILE INVENTORY

| Tên File / File Name | Mục Đích / Purpose | Kích Thước / Resolution | Trạng Thái / State |
| :--- | :--- | :--- | :--- |
| `Redesign/index.html` | **Master Showcase Hub** (Bảng điều hướng tổng hợp, chuyển đổi nhanh giữa các bản demo) | Responsive Full HD | Sẵn sàng mở trên trình duyệt |
| `Redesign/desktop_app.html` | **Desktop Client Prototype** (.NET 10 WPF Simulation: Nền Sáng, Nút Power Emerald, Live Bandwidth MB, Server Picker, Login Modal, Notification Bell) | 960 x 640 WPF Fixed Frame | Clickable & Interactive |
| `Redesign/cms_admin.html` | **Web Admin CMS Portal** (Port 6033: Nền Trắng, Bento Stats, Proxy Table, 1-Click Ping, 1-Click Delete Offline, KikiLogin Syntax) | Responsive Web Portal | Clickable & Interactive |
| `Redesign/tokens.html` | **Design Tokens & UI Kit** (Bảng mã màu, WPF SolidColorBrush, Badges, Typography) | Responsive Documentation | Sẵn sàng sao chép XAML |

---

## 3. BẢNG ÁNH XẠ MÀU SẮC SANG WPF XAML / COLOR TOKEN TO WPF XAML MAPPING

Toàn bộ màu sắc trong file `Redesign/desktop_app.html` được ánh xạ trực tiếp sang file XAML `apps/desktop/resources/styles/theme.light.xaml`:

```xml
<!-- File: apps/desktop/resources/styles/theme.light.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- 1. Backgrounds / Nền sáng sạch sẽ -->
    <Color x:Key="ColorBgWindow">#FFFFFF</Color>
    <Color x:Key="ColorBgSubtle">#F8FAFC</Color>
    <Color x:Key="ColorBorderSubtle">#E2E8F0</Color>
    <Color x:Key="ColorBorderEmerald">#BBF7D0</Color>

    <!-- 2. Emerald Accents / Màu xanh ngọc lục bảo cho Nút & Điểm nhấn -->
    <Color x:Key="ColorEmerald600">#059669</Color>
    <Color x:Key="ColorEmerald700">#047857</Color>
    <Color x:Key="ColorEmerald900">#064E3B</Color>
    <Color x:Key="ColorEmerald100">#DCFCE7</Color>
    <Color x:Key="ColorEmerald50">#F0FDF4</Color>

    <!-- 3. Typography Brushes / Màu chữ -->
    <Color x:Key="ColorTextPrimary">#0F172A</Color>
    <Color x:Key="ColorTextSecondary">#334155</Color>
    <Color x:Key="ColorTextMuted">#64748B</Color>

    <!-- 4. Functional Accents / Màu trạng thái chức năng -->
    <Color x:Key="ColorAccentGold">#D97706</Color>     <!-- Favorite / VIP -->
    <Color x:Key="ColorAccentCyan">#0284C7</Color>     <!-- KikiLogin / Mesh Gateway -->
    <Color x:Key="ColorAccentRed">#DC2626</Color>      <!-- Disconnected / Offline -->

    <!-- SolidColorBrush Declarations -->
    <SolidColorBrush x:Key="BrushBgWindow" Color="{StaticResource ColorBgWindow}"/>
    <SolidColorBrush x:Key="BrushBgSubtle" Color="{StaticResource ColorBgSubtle}"/>
    <SolidColorBrush x:Key="BrushBorderSubtle" Color="{StaticResource ColorBorderSubtle}"/>
    <SolidColorBrush x:Key="BrushBorderEmerald" Color="{StaticResource ColorBorderEmerald}"/>
    <SolidColorBrush x:Key="BrushEmerald600" Color="{StaticResource ColorEmerald600}"/>
    <SolidColorBrush x:Key="BrushEmerald700" Color="{StaticResource ColorEmerald700}"/>
    <SolidColorBrush x:Key="BrushEmerald900" Color="{StaticResource ColorEmerald900}"/>
    <SolidColorBrush x:Key="BrushEmerald100" Color="{StaticResource ColorEmerald100}"/>
    <SolidColorBrush x:Key="BrushTextPrimary" Color="{StaticResource ColorTextPrimary}"/>
    <SolidColorBrush x:Key="BrushTextSecondary" Color="{StaticResource ColorTextSecondary}"/>
    <SolidColorBrush x:Key="BrushTextMuted" Color="{StaticResource ColorTextMuted}"/>
    <SolidColorBrush x:Key="BrushAccentGold" Color="{StaticResource ColorAccentGold}"/>
    <SolidColorBrush x:Key="BrushAccentCyan" Color="{StaticResource ColorAccentCyan}"/>
    <SolidColorBrush x:Key="BrushAccentRed" Color="{StaticResource ColorAccentRed}"/>

</ResourceDictionary>
```

---

## 4. ÁNH XẠ CÁC PHÂN HỆ MỚI / NEW MODULES MAPPING

### 4.1. Phân Hệ Đăng Nhập & Quản Lý Tài Khoản (Login & Account Modal)
- **HTML Element**: `#loginModal`, `.login-card-modal`, `.btn-submit-emerald`, `.bypass-login-btn` trong `Redesign/desktop_app.html`.
- **WPF XAML Target**: `apps/desktop/Views_XAML/MainWindow.xaml` và `apps/desktop/Views_XAML/ExpandedAccount.xaml`.
- **Logic C#**:
  - `AccountTypeHelper.SetAccountType(Namecheap)` hoặc `BypassLoginAndEnter()` khi chọn chế độ Auto-Bypass.
  - Lưu trữ token xác thực tại `AccountSessionManager.cs`.

### 4.2. Trung Tâm Thông Báo (Notifications Center Dropdown)
- **HTML Element**: `#notiBtn`, `#notiDropdown`, `.noti-item-box` trong `Redesign/desktop_app.html`.
- **WPF Control**: `Popup` hoặc `Flyout` gắn với biểu tượng Bell trên TitleBar:
  ```xml
  <Button x:Name="NotificationBellButton" Style="{StaticResource TitleBarIconButtonStyle}" Click="NotificationBell_Click">
      <Grid>
          <ui:SymbolIcon Symbol="Alert24"/>
          <Border Style="{StaticResource UnreadBadgeBorderStyle}" Visibility="{Binding HasUnreadNotifications, Converter={StaticResource BooleanToVisibilityConverter}}">
              <TextBlock Text="{Binding UnreadCount}" Style="{StaticResource BadgeText}"/>
          </Border>
      </Grid>
  </Button>
  ```
- **Sự kiện ghi nhận**: Kết nối VPN thành công, thay đổi IP, cảnh báo độ trễ cao, tự động đồng bộ proxy mới từ CMS (20s).

---

## 5. HƯỚNG DẪN MỞ & KIỂM TRA TRỰC TIẾP TRÊN TRÌNH DUYỆT

1. Mở Master Showcase Hub:
   ```text
   file:///E:/DECOMPILER/Soft/VPN/CONVERT/Redesign/index.html
   ```
2. Mở trực tiếp Desktop App Prototype:
   ```text
   file:///E:/DECOMPILER/Soft/VPN/CONVERT/Redesign/desktop_app.html
   ```
3. Mở trực tiếp Web CMS Admin Prototype:
   ```text
   file:///E:/DECOMPILER/Soft/VPN/CONVERT/Redesign/cms_admin.html
   ```
