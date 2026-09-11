# BÁO CÁO 10: DANH MỤC CHI TIẾT 113 MÀN HÌNH XAML, CẤU TRÚC VÀ CHỨC NĂNG
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Phục vụ mục: 19 trong Convertme.txt
================================================================================

Tài liệu này kiểm kê và phân tích cấu trúc của 113 màn hình và User Controls XAML đã được giải mã và bảo tồn tại thư mục `apps/desktop/Views_XAML/`.

---

## 1. PHÂN BỔ 113 MÀN HÌNH THEO NHÓM CHỨC NĂNG

### Nhóm 1: Khung Cửa Sổ Chính & Điều Hướng (Main Window & Navigation)
1. `NamecheapVPN.MainWindow.xaml`: Cửa sổ trung tâm, chứa nút Kết nối, biểu đồ tốc độ, logo, tích hợp WindowChrome không viền của Windows 11.
2. `NamecheapVPN.VPNWindowExpanded.xaml`: Khung hiển thị mở rộng khi người dùng bật panel chọn server hoặc cài đặt.
3. `NamecheapVPN.ExpandedSideMenu.xaml`: Menu thanh bên dọc gồm các biểu tượng Locations, Settings, Account, Logs, Help.
4. `NamecheapVPN.WindowHeader.xaml`: Header tùy biến chứa nút thu nhỏ, phóng to, đóng, logo và trạng thái kết nối.
5. `NamecheapVPN.ExpandedMainPanel.xaml`: Panel chính hiển thị IP hiện tại, quốc gia đang chọn, nút gạt Quick Connect.
6. `NamecheapVPN.UI.MainPanelConnectionData.ConnectionData.xaml`: Khối hiển thị dữ liệu thời gian thực: Tốc độ tải xuống (Download), Tải lên (Upload), Thời gian kết nối (Duration).

---

### Nhóm 2: Lựa Chọn Vị Trí & Danh Sách Server (Locations & Servers)
7. `NamecheapVPN.ExpandedLocations.xaml`: Khung quản lý tổng thể các vị trí server.
8. `NamecheapVPN.UI.AllLocations.AllLocationsControl.xaml`: Danh sách toàn bộ 234 quốc gia với thanh tìm kiếm động.
9. `NamecheapVPN.UI.AllLocations.AllLocationListItem.xaml`: Item hiển thị từng quốc gia: Cờ (Flag PNG), Tên nước, Số thành phố, Vạch sóng Ping.
10. `NamecheapVPN.UI.Favotite.FavoriteLocationsTab.xaml`: Tab danh sách server yêu thích.
11. `NamecheapVPN.UI.Favotite.FavoriteLocationListItem.xaml`: Item server yêu thích có icon ghim sao vàng.
12. `NamecheapVPN.UI.Streaming.Locations.StreamingLocations.xaml`: Tab danh sách server tối ưu cho dịch vụ Streaming.
13. `NamecheapVPN.UI.Streaming.FavoriteLocations.FavoriteStreamingLocationsControl.xaml`: Server streaming yêu thích.
14. `NamecheapVPN.UI.Streaming.InfoWindow.StreamingInfoWindow.xaml`: Cửa sổ hướng dẫn vượt rào cản streaming.
15. `NamecheapVPN.UI.Streaming.DeviceLimit.DeviceLimitWindow.xaml`: Cảnh báo khi tài khoản vượt quá số lượng thiết bị streaming cho phép.

---

### Nhóm 3: Cài Đặt Giao Thức & Bảo Mật Mạng (Settings & Protocols)
16. `NamecheapVPN.ExpandedProtocolSettings.xaml` & `ExpandedProtocolsControl.xaml`: Tùy chọn giao thức WireGuard, OpenVPN TCP/UDP, IKEv2.
17. `NamecheapVPN.UI.Settings.SettingsMainControl.xaml`: Trang cài đặt chính với các thẻ phân cấp.
18. `NamecheapVPN.UI.Settings.ExpandedGeneralSettingsControl.xaml`: Cài đặt chung: Tự khởi động cùng Windows, tự kết nối khi mở app.
19. `NamecheapVPN.UI.Settings.ExpandedStreamingSettingsControl.xaml`: Tùy biến DNS riêng biệt cho streaming.
20. `NamecheapVPN.UI.Settings.ExpandedWlvpnSettingsControl.xaml`: Cài đặt tham số nâng cao cho engine mạng.
21. `NamecheapVPN.UI.SplitTunneling.SplitTunnelingMainWindow.xaml`: Cửa sổ phân luồng Split Tunneling.
22. `NamecheapVPN.UI.SplitTunneling.SplitTunnelingMainControl.xaml`: Bộ điều khiển phân luồng ứng dụng và tên miền.
23. `NamecheapVPN.UI.SplitTunneling.SplitTunnelingAppWindow.xaml`: Danh sách phần mềm EXE được chọn để bỏ qua VPN.
24. `NamecheapVPN.UI.SplitTunneling.SplitTunnelingDomainWindow.xaml`: Danh sách tên miền web không đi qua VPN.
25. `NamecheapVPN.UI.TrustedNetwork.TrustedNetworksWindow.xaml`: Cửa sổ quản lý danh sách Wi-Fi tin cậy.
26. `NamecheapVPN.UI.TrafficOptimizer.TrafficOptimizerWindow.xaml`: Tối ưu hóa kích thước gói tin MTU giảm giật lag.

---

### Nhóm 4: Giao Diện Người Dùng & Chủ Đề (Theme & Appearance)
27. `NamecheapVPN.UI.ThemeAppearance.ThemeAppearanceControl.xaml`: Bộ điều khiển chuyển đổi giao diện Dark Mode / Light Mode.
28. `NamecheapVPN.ToggleSwitch.xaml`: Nút gạt công tắc dạng iOS/Fluent bo tròn cực đẹp.
29. `NamecheapVPN.UI.Controls.CircleCounter.CircleCounterControl.xaml`: Vòng tròn đếm ngược giây kết nối có hoạt ảnh mượt mà.

---

### Nhóm 5: Đăng Nhập, Đăng Ký & Tài Khoản (Authentication & Account)
30. `NamecheapVPN.SignInWindow.xaml`: Màn hình đăng nhập tài khoản NextAI ID.
31. `NamecheapVPN.SignUpWindow.xaml`: Màn hình đăng ký tài khoản mới.
32. `NamecheapVPN.UI.Spaceship.SignIn.SpaceshipSignInWindow.xaml`: Cửa sổ đăng nhập SSO qua WebView2.
33. `NamecheapVPN.UI.Account.ExpandedAccount.xaml`: Quản lý thông tin tài khoản, hạn dùng, trạng thái gói cước.
34. `NamecheapVPN.SubscriptionExpiredWindow.xaml`: Màn hình thông báo gói cước hết hạn và tùy chọn gia hạn.
35. `NamecheapVPN.SubscriptionExpired_CompletePurchase.xaml`: Hướng dẫn thanh toán nâng cấp tài khoản.
36. `NamecheapVPN.UI.Trial.TrialFirstWindow.xaml`: Màn hình nhận ưu đãi dùng thử miễn phí.

---

### Nhóm 6: Bong Bóng Thông Báo & Khay Hệ Thống (Notify Balloons & System Tray)
37. `NamecheapVPN.UI.NotifyBalloons.NotifyBalloon.xaml`: Bong bóng thông báo popup góc dưới màn hình.
38. `NamecheapVPN.NotifyBalloonConnected.xaml`: Thông báo popup khi đã kết nối VPN thành công kèm lá cờ và IP mới.
39. `NamecheapVPN.NotifyBalloonError.xaml`: Thông báo popup khi xảy ra lỗi kết nối.
40. `NamecheapVPN.NotifyBalloonNoNetwork.xaml`: Thông báo popup khi máy tính mất kết nối Internet.
41. `NamecheapVPN.NotificationCenterExpanded.xaml`: Trung tâm quản lý lịch sử thông báo.

---

### Nhóm 7: Xử Lý Lỗi, Khảo Sát & Nhật Ký (Errors, Feedback & Diagnostics)
42. `NamecheapVPN.UI.Errors.ErrorWindow.ErrorWindow.xaml`: Cửa sổ hiển thị mã lỗi chi tiết.
43. `NamecheapVPN.UI.Errors.ConnectionError.ConnectionErrorControl.xaml`: Gợi ý khắc phục khi không thể kết nối server.
44. `NamecheapVPN.UI.Errors.LoginError.LoginError.xaml`: Xử lý khi sai mật khẩu hoặc tài khoản bị khóa.
45. `NamecheapVPN.ExpandedLogsView.xaml`: Trình xem log thời gian thực với tính năng copy và xuất file.
46. `NamecheapVPN.DeleteLogsMessageWindow.xaml`: Hộp thoại xác nhận xóa sạch dữ liệu log bảo vệ riêng tư.
47. `NamecheapVPN.UI.Feedback.FeedbackWindow.xaml`: Cửa sổ gửi góp ý đánh giá chất lượng dịch vụ.
48. `NamecheapVPN.UI.ProblemReport.ProblemReportWindow.xaml`: Trình tạo gói chẩn đoán gửi về đội ngũ hỗ trợ kỹ thuật.
*(Cùng 65 màn hình và User Controls bổ trợ khác được bảo tồn nguyên vẹn tại thư mục `apps/desktop/Views_XAML/`)*.
