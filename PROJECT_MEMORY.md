# BỘ NHỚ DỰ ÁN & QUYẾT ĐỊNH KIẾN TRÚC (PROJECT_MEMORY.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

## 1. Bối cảnh & Nguyên tắc Tách biệt Hệ thống (Decoupling Law)
- **Quyết định ngày 2026-09-08**: User yêu cầu rõ ràng: Khi có mạng proxy cư dân lớn, hệ thống sẽ phục vụ đa mục đích, kết nối nhiều nền tảng hoặc cho thuê độc lập. Do đó, hệ thống VPN Gateway Platform tại `E:\DECOMPILER\Soft\VPN\CONVERT` và Antidetect Browser KikiLogin tại `D:\Decompiler\Soft\kikilogin` là **HAI HỆ THỐNG RIÊNG BIỆT 100% KHÔNG CHUNG ĐỤNG MÃ NGUỒN**.
- Giao thức kết nối duy nhất: Chuẩn mở SOCKS5 / HTTP Proxy Gateway qua cổng `10000` (Universal) hoặc cổng dải `10001 - 10500` (Dedicated).
- KikiLogin chỉ lưu cấu hình Proxy Host/Port thông thường, không được phép inject hay liên kết code trực tiếp.

## 2. Nền tảng Công nghệ Desktop Client (apps/desktop)
- **Lý do chọn FastVPN (Pack 2)**:
  - Nền tảng nguyên bản là **.NET 10.0 (`net10.0`)**, trùng khớp tuyệt đối với .NET SDK 10.0.400 trên máy của User.
  - Sử dụng Lepoco `Wpf.Ui` (Fluent Design System Windows 11) hiện đại với bo góc mượt mà, hỗ trợ Mica/Acrylic và Dark/Light mode.
  - Bộ tài nguyên đồ họa được bảo tồn 100%: 113 màn hình/controls XAML, 234 cờ quốc gia, 11 font chữ Museo Sans/Helvetica, 215 tài nguyên vector.
  - Có đầy đủ các driver và service WFP (Windows Filtering Platform) Kill Switch, WireGuard NT native, và OpenVPN.

## 3. Kiến trúc Backend & Cổng Mạng (Backend/)
- **Cổng 6033**: Web Admin CMS Dashboard + REST API Controller (chuẩn Convertme.txt mục 14).
- **Cổng 10000**: Universal Residential Proxy Gateway (hỗ trợ SOCKS5 & HTTP CONNECT trên cùng 1 cổng với cú pháp `username-country-xx-session-yy:password`).
- **Cổng 10001 - 10500**: Dải cổng riêng biệt dành cho từng khách hàng hoặc profile cố định.
- **Thương hiệu**: `nextaitechnology` | Package: `com.nextaitechnology.vpn`.

## 4. Quản lý Giao diện & Design Gate
- User sẽ tự thiết kế lại giao diện UI/UX sau.
- Hệ thống đã phân loại và lưu giữ đầy đủ:
  - `apps/desktop/resources/styles/theme.dark.xaml` (67 KB - toàn bộ màu sắc, brush, button styles cho Dark Theme).
  - `apps/desktop/resources/styles/theme.light.xaml` (63 KB - toàn bộ styles cho Light Theme).
  - `apps/desktop/resources/styles/colorbrush.xaml` (bảng mã màu token).
  - `apps/desktop/resources/styles/fonts.xaml` (mapping font chữ).
  - Thư mục `Report/12_UI_Design_Handoff_and_Tokens.md` cung cấp thông số để AI hoặc Designer thiết kế lại mà không bị lệch kết cấu.

## 5. Cơ Chế Vào Thẳng Desktop Không Cần Đăng Nhập (Bypass Login)
- **Vấn đề**: Bản gốc FastVPN yêu cầu xác thực OAuth với Namecheap hoặc Spaceship qua web browser. Do ứng dụng tách độc lập phục vụ mạng VPN NextAI, người dùng cần vào thẳng giao diện điều khiển (Dashboard) mà không bị kẹt ở màn hình đăng nhập.
- **Giải pháp**: Patch phương thức `BypassLoginAndEnter()` trực tiếp vào IL assembly `FastVPN.dll`, ghi đè `CheckPrelogged()` và các sự kiện click nút để tự động mở `VPNWindowExpanded` ngay khi khởi chạy.
- **Thư mục chạy**: `apps/desktop/NextAiVPN_Bypass/` hoặc tệp batch tiện ích `Chay_NextAiVPN_Khong_Dang_Nhap.bat`.

## 6. Kiến Trúc CMS Quản Lý Proxy Thật & Universal Gateway Forwarding
- **Web Admin Portal**: Truy cập tại `http://127.0.0.1:6033/admin/index.html`.
- **Chức năng**:
  + Thêm thủ công từng proxy hoặc nhập hàng loạt (Bulk Import) qua textarea (`ip:port`, `ip:port:user:pass`, `socks5://`, `http://`).
  + Ping test đo độ trễ thực tế qua SOCKS5 / TCP socket handshake.
  + Lưu trữ bền vững tại `Backend/data/proxies.json`.
  + Chọn proxy hoạt động để Gateway cổng `10000` tự động forward toàn bộ lưu lượng của client/KikiLogin qua proxy thật đó.

## 7. Giải Pháp Đồng Bộ Danh Sách Proxy Cho Desktop Client (.NET 10)
- **Vấn đề đã khắc phục**:
  + Modal TAP driver popup gây kẹt vô tận -> Đã loại bỏ và đặt mặc định sang WireGuard.
  + Logo watermark Namecheap -> Đã gỡ bỏ vĩnh viễn (`BrandImage.Visibility = Collapsed`).
  + TUYỆT ĐỐI KHÔNG can thiệp Windows System Proxy (`ProxyEnable = 0` luôn được bảo toàn): Cổng 10000 là cổng Universal Gateway Proxy dành riêng cho các ứng dụng thứ ba (KikiLogin/Antidetect browsers) kết nối theo cấu hình profile riêng biệt. Ứng dụng Desktop VPN hoạt động an toàn mà không cưỡng bức toàn bộ máy tính Windows vào cổng 10000, bảo vệ 100% đường truyền internet của người dùng không bị gián đoạn hay mất mạng.

## 8. Kiến Trúc Bulk Import Hàng Nghìn Bản Ghi & Động Cơ GeoIP Tự Động Phân Tách Quốc Gia
- **Tối ưu hóa hiệu năng nạp proxy quy mô lớn**:
  + Thay vì nạp từng record và gọi API phân giải IP riêng lẻ (gây nghẽn mạng và bị rate-limit), `ProxyManagerService.AddBulkAsync` thực hiện deduplicate danh sách `uniqueIps` trước.
  + `GeoIpService` gom các IP duy nhất này thành các batch tối đa 100 IP gửi đến `http://ip-api.com/batch`.
  + Bộ nhớ đệm GeoIP lưu tại `Backend/data/geoip_cache.json` đảm bảo không bao giờ tra cứu lại IP đã biết.
  + Fallback Subnet Heuristics nhận diện tức thời các IP phổ biến của Việt Nam (`103.`, `14.`, `113.`, `115.`, `117.`, `118.`, `123.`, `125.`, `171.`, `222.`), US (`8.`, `23.`, `64.`, `104.`, `198.`), SG, JP...
- **Phân tách quốc gia trực quan trên CMS**:
  + Tự động trích xuất `Country`, `CountryName`, `City`, `Isp` gắn vào từng node proxy.
  + Thanh **Dynamic Country Filter Pills** hiển thị số lượng theo thời gian thực (ví dụ `🌐 Tất Cả (23)`, `🇻🇳 Vietnam (17)`, `🇺🇸 United States (2)`, v.v.).
  + Click 1 chạm lọc ngay tức thì danh sách VPN của từng quốc gia.

## 9. Động Cơ Xoay Proxy SaaS Thông Minh & Auto Health-Check Định Kỳ 10 Phút
- **Nghiên cứu Decompiler Patterns (`BestAvailableServerHelper.cs`, `RegionLoadChecker.cs`)**:
  + Hàng rào sức khỏe: `where !s.InMaintenance && s.Load < 85` -> Lọc triệt để chỉ node `LIVE` mới được phép đưa vào pool luân chuyển. Node chết bị loại bỏ hoàn toàn.
  + Proximity Geo Routing: Khớp theo `targetCountry` -> `targetCity` -> Fallback cùng quốc gia -> Fallback toàn cầu.
  + Top-3 Load Shuffling: `LoadScore = (ActiveConnections * 100) + PingMs`, sắp xếp tăng dần và xáo trộn ngẫu nhiên Top 3 node tốt nhất để chống dồn tải cục bộ (Thundering Herd) khi hàng nghìn người dùng SaaS truy cập cùng lúc.
- **Smart SaaS Rotation Engine (`SmartProxyRotationEngine.cs`)**:
  + **Sticky Session**: Cố định IP theo Profile trong 10-30 phút (dành cho Antidetect browser / nuôi nick).
  + **Rotating Session**: Xoay IP theo mỗi kết nối TCP/HTTP (dành cho scraping/botting).
  + **Auto-Healing Failover**: Tự động phát hiện Upstream Proxy vật lý đứt kết nối và chuyển tức thời sang Rescue Node khác.
- **Auto Health-Check Định Kỳ 10 Phút (`ProxyHealthCheckBackgroundService.cs`)**:
  + `BackgroundService` chạy ngầm chu kỳ chính xác 10 phút (`TimeSpan.FromMinutes(10)`).
  + Đo ping song song bằng `SemaphoreSlim`, tự động cập nhật trạng thái `LIVE` (xanh) hoặc `🔴 OFFLINE` (đỏ rực, xóa pingMs và gán DIE) vào `Backend/data/proxies.json`.
  + Cung cấp API `GET /api/v1/proxies/health-check/status` và `POST /api/v1/proxies/health-check/run`.
- **Giao Diện Web CMS**:
  + Thêm checkbox ở cột đầu tiên cho header và từng hàng proxy.
  + Thanh Batch Actions Bar nổi lên khi chọn: `☑️ Đã chọn: X proxy`, nút xóa hàng loạt `[🗑️ Xóa Các Proxy Đã Chọn]`, đo ping hàng loạt `[⚡ Kiểm Tra Ping Đã Chọn]`, và nút `[✕ Bỏ Chọn]`.
  + Widget đếm ngược chu kỳ 10 phút: `🩺 TỰ ĐỘNG ĐO PING (10 PHÚT/LẦN) | 🟢 ĐANG CHẠY | Lần kiểm tra tới: mm:ss` kèm nút `⚡ Ping Ngay`.

## 10. Phân Hệ Thu Thập IP Dân Cư (Residential IP Harvester) & Quản Lý Client Nodes
- **Cơ chế hoạt động**:
  + `NextAiNodeCollectorService.cs` trên Desktop Client tự động xác định Public IP thực tế của máy người dùng, sinh mã định danh duy nhất (`win_{hash}` dựa trên MachineName và UserName).
  + Tự động đăng ký máy lên Backend CMS tại `POST /api/v1/client-nodes/register` và gửi nhịp tim (Heartbeat) định kỳ mỗi 2 phút.
  + Backend lưu trữ danh sách tại `Backend/data/client_nodes.json`, tự động chuyển đổi các Client Node đang trực tuyến (`ONLINE`) thành các node proxy trong Residential Mesh Pool.
- **Chuẩn hóa xác thực proxy cư dân (Node ID Routing)**:
  + Bảo vệ máy cá nhân người dùng: Không mở cổng trực tiếp trên máy client (tránh tường lửa NAT/Firewall và nguy cơ bảo mật).
  + Định tuyến tập trung qua Gateway cổng `10000`: Hỗ trợ User/Pass `node-[DeviceId]:nextai_secret` để điều hướng chính xác lưu lượng ra đúng máy cư dân đó.
  + Giao diện CMS cung cấp Banner hướng dẫn chi tiết và nút [📋 Copy] cú pháp cấu hình 1-Click cho KikiLogin / AdsPower / MoreLogin.

## 11. Trình Cài Đặt Độc Lập Chuẩn Windows (NextAi VPN Standalone Setup Wizard)
- **Kiến trúc đóng gói**:
  + Dự án `apps/installer/NextAiVPN.Setup` biên dịch thành tệp đơn lẻ (Single-File Self-Contained) `NextAiVPN_Setup.exe` (~50.6 MB nén / ~190 MB self-contained).
  + Đóng gói toàn bộ 86 tệp nhị phân Release, driver WFP, WireGuard và tài nguyên của `NextAiVPN.Desktop` vào `payload.zip` nhúng làm `EmbeddedResource`.
- **Quy trình Wizard chuyên nghiệp**:
  + 4 bước cài đặt chuẩn thương mại: Welcome -> Chọn thư mục cài đặt (`%LocalAppData%\Programs\NextAiTechnology\NextAiVPN`) & tùy chọn -> Tiến trình giải nén thời gian thực -> Hoàn tất & Khởi chạy.
  + Tự động tạo Desktop và Start Menu Shortcuts qua COM `WScript.Shell`.
  + Đăng ký Windows Installed Apps và hỗ trợ gỡ cài đặt sạch sẽ (`NextAiVPN_Setup.exe /uninstall`).

## 12. Khởi Động Trực Tiếp & Loại Bỏ Triệt Để Cửa Sổ Đen & Kẹt "Loading locations..."
- **Loại bỏ cửa sổ đen (`MainWindow` cũ)**:
  + `App.cs` gán `Current.MainWindow = expandedWindow`, ẩn hoàn toàn `mainWindow` cũ (`Visibility = Collapsed`, `ShowInTaskbar = false`).
  + `NextAiVPN.MainWindow.xaml` và `NextAiVPN.SignInWindow.xaml` cấu hình `Visibility="Hidden" ShowInTaskbar="False"`.
- **Khắc phục kẹt "Loading locations..."**:
  + `AuthenticationFlow.cs` bỏ qua xác thực cloud cũ, trả về `Success` ngay lập tức (0ms).
  + `SDKMonitor.cs` có khối `finally` đóng hoàn toàn vòng xoay chờ tải, nạp sẵn 9 vị trí mặc định đồng bộ, song song kích hoạt tác vụ ngầm đồng bộ với Backend CMS.

## 13. Tối Ưu Độ Nhạy Click Location (< 1ms) & Chạy Ẩn Khay Hệ Thống (System Tray) Khi Bấm [X]
- **Độ nhạy Click Location**:
  + Trong `AllLocationListItem_OnDataContextChanged`, gán tường minh `LocationItemHoverBorder.DataContext = list[0]` để khắc phục lỗi WPF data binding `{Binding Country}`, `{Binding City}`, `{Binding PingMs}` do DataContext là `List<ILocation>`.
  + Xử lý hiển thị động: `"{N} Locations"` cho quốc gia nhiều thành phố, hoặc hiển thị tên thành phố trực tiếp nếu chỉ có 1 vị trí.
  + Chuyển sang sự kiện `PreviewMouseLeftButtonDown` cho cả hàng quốc gia và thành phố con, loại bỏ độ trễ của ListBoxItem container.
  + Hàm `SelectLocationFast` cập nhật trực tiếp `Mainpanel.Location.Text`, `Mainpanel.City.Text` và cờ quốc gia < 1ms, không chặn UI thread.
  + `FavoriteLocationsTab.cs` dùng `FindVisualParent` tách biệt nút xóa và click vị trí yêu thích.
- **Chạy ẩn khay hệ thống (System Tray)**:
  + `WindowHeader.cs` & `VPNWindowExpanded.cs`: Bắt sự kiện bấm nút `[X]` (hoặc `SC_CLOSE` / Alt+F4) để gọi `Hide()`, chuyển `WindowState = WindowState.Minimized`, hiển thị Balloon thông báo "NextAiVPN still running..." và duy trì `TaskbarIcon.Visibility = Visibility.Visible`.
  + `TaskBarIconService.cs`: Bắt sự kiện nhấp chuột trái (`TrayLeftMouseDown`) hoặc nhấp đúp (`TrayMouseDoubleClick`) để khôi phục cửa sổ ngay lập tức (`DisplayWindowsOnSystemTrayIconClick`).
  + Nhấp chuột phải chọn **"Quit"** để thoát hoàn toàn ứng dụng sạch sẽ (`QuitCommonFunction`).
  + `AdminRunner.cs`: Khắc phục triệt để lỗi timer 3 giây tắt ứng dụng, cho phép ứng dụng chạy ngầm liên tục và ổn định ở quyền người dùng thông thường.

## 14. Cơ Chế Xử Lý Sự Kiện Chuột Tunneling (PreviewMouseLeftButtonDown) & Đồng Bộ Favorites
- **Vấn đề click không nhận**:
  + Thao tác nhấp chuột trên `Favorite` star (ngôi sao ⭐) và `RemoveFavoriteLocation` (nút xóa 🗑️) trong WPF ban đầu dùng `MouseDown`. Do nằm trong `ListBoxItem`, sự kiện Bubbling bị ListBox Selection nuốt mất (`e.Handled = true`).
  + Các tab `All` và `Favorites` trên `ExpandedLocations.xaml` cũng bị ảnh hưởng tương tự nếu không dùng Tunneling event.
- **Giải pháp triệt để**:
  + Thay thế bằng `PreviewMouseLeftButtonDown` trên toàn bộ các control tương tác (`Favorite`, `RemoveFavoriteLocation`, `PrivateModeBorder`, `FavoriteLocationsBorder`).
  + Nâng cấp `SimulateMouseClick` phát đồng thời cả `PreviewMouseLeftButtonDownEvent` và `MouseLeftButtonDownEvent`.
  + Khắc phục bug chuỗi trong `FavoritesService.cs` (`f.Contains(location.ToUpper())`) và `FavoriteLocationsServices.cs` (`item.Substring(3, 3)`) giúp lưu và xóa yêu thích chính xác tuyệt đối.
  + Hỗ trợ đa hình DataContext (`IList<ILocation>`, `CollectionViewGroup`) cho Accordion hiển thị đa vị trí.
- **Hệ thống Kiểm thử Tự động Đa chu kỳ (AutoTestAgent)**:
  + Chạy tự động 2 chu kỳ E2E (23/23 bước PASSED 100%).
  + Chụp ảnh trực tiếp cây đồ họa WPF qua `RenderTargetBitmap` lưu vào `Report/screenshots/`.
  + Đối soát nghiêm ngặt Decoupling Law (`ProxyEnable = 0`).

## 15. Khắc Phục Triệt Để Hit-Test Interception, WPF Virtualization & Selection Loop Storm
- **Hit-Test Block**: `<Rectangle Name="DisablerRectangle">` đè lên `LocationsList` với `ZIndex="990"`, đã được bổ sung `IsHitTestVisible="False"` vĩnh viễn để giải phóng tương tác chuột.
- **WPF UI Virtualization**: Áp dụng `ScrollIntoView(targetItem)` và `UpdateLayout()` trước khi gọi `ContainerFromItem()` để buộc WPF dựng visual container cho các node ngoài khung nhìn.
- **SelectionChanged Infinite Storm**: Loại bỏ `lbi.IsSelected` gán lặp trong `SetSelectedVisualState`, đặt cờ `_isUpdatingInnerSelection` chống đệ quy và bỏ `SelectedIndex = -1` gây nghẽn UI Dispatcher thread.
- **Highlight Độc Quyền**: So sánh `childLoc.Id == selectedLocationId` để 16 node Hà Nội chỉ sáng duy nhất 1 node được chọn.

## 16. Phân Phối Bộ Cài Đặt Thương Mại Single-File (NextAiVPN_Setup.exe v8.0.0)
- Tệp cài đặt: `NextAiVPN_Setup.exe` (~115.3 MB) chứa trọn vẹn .NET 10 WPF runtime, 132 tệp nhị phân release và tài nguyên trong `payload.zip`.
- Vị trí: `installer/NextAiVPN_Setup.exe` và thư mục gốc `NextAiVPN_Setup.exe`.
- Cấu hình chạy: `AutoTestAgent` được gác sau cờ `--autotest` hoặc `NEXTAI_AUTOTEST=1`, đảm bảo người dùng cài đặt xong có thể trải nghiệm giao diện thủ công mượt mà, phản hồi tức thì.

## 17. Quy Định Thực Thi Script & Khởi Chạy (Execution Paths & Launcher Rules)
- **Quy tắc cứng (Hard Rule)**: Mọi AI Agent khi hướng dẫn hoặc thực thi khởi chạy hệ thống qua tệp batch `Chay_HeThong_NextAiVPN.bat` **BẮT BUỘC PHẢI GHI ĐẦY ĐỦ ĐƯỜNG DẪN TUYỆT ĐỐI**:
  - Đường dẫn chuẩn: `E:\DECOMPILER\Soft\VPN\CONVERT\Chay_HeThong_NextAiVPN.bat`
  - Hoặc định dạng liên kết Markdown: `[Chay_HeThong_NextAiVPN.bat](file:///e:/DECOMPILER/Soft/VPN/CONVERT/Chay_HeThong_NextAiVPN.bat)`
- Không được viết tắt hoặc chỉ ghi tên tệp ngắn gọn gây nhầm lẫn môi trường thực thi.
## 18. Cơ Chế Đo & Hiển Thị Dung Lượng Băng Thông Thời Gian Thực (Gateway Throughput Live Accounting)
- **Vấn đề**: Khi Desktop Client định tuyến qua Universal Gateway Cổng 10000 (Decoupling Law, không can thiệp TAP driver / WFP adapter cũ), card mạng TAP không nhận gói tin, khiến biến `DownloadedBytes` và `UploadedBytes` luôn bằng 0 và không cập nhật lên giao diện.
- **Giải pháp**:
  + **Universal Gateway (Backend)**: Bổ sung bộ đếm thời gian thực hai chiều `_totalBytesIn` (Download) và `_totalBytesOut` (Upload) với `Interlocked.Add` trong quá trình streaming dữ liệu.
  + **REST API Metric Snapshot**: Cung cấp endpoint `GET /api/v1/gateway/stats` trả về snapshot JSON gồm `BytesIn`, `BytesOut`, `DownloadMb`, `UploadMb`, `ActiveSessions`, `ActiveProxy`.
  + **Desktop Client Polling & UI Dispatcher**: `SessionStatsTracker.cs` kết hợp cùng `NextAiLocationService.cs` định kỳ mỗi 1 giây lấy dữ liệu Gateway, tính toán delta phiên kết nối và cập nhật chuỗi MB vào `ConnectionDataViewModel.NetworkUsageDownloadMb` và `NetworkUsageUploadMb`. Giao diện màn hình chính `ExpandedMainPanel` tự động nhảy số thời gian thực.

## 19. Cơ Chế Khóa Vùng Nghiêm Ngặt (Strict Country-Lock) & Đồng Bộ Tức Thời (Zero-Rebuild Real-Time Sync)
- **Strict Country-Lock & Anti-Leak**:
  + Khi người dùng lựa chọn một quốc gia trên Desktop App (ví dụ US), `UniversalGatewayService` áp dụng cơ chế khóa vùng: Nếu node được chọn bị gián đoạn hoặc timeout, Gateway **CHỈ** failover sang các node US khác hoặc node quốc tế LIVE (SG, GB, DE) và **TUYỆT ĐỐI KHÔNG** bao giờ rớt về IP Việt Nam hoặc Direct, bảo đảm an toàn 100% cho người dùng.
- **Zero-Rebuild Real-Time Location Sync**:
  + `SDKMonitor.cs` chạy vòng lặp nền chu kỳ 20 giây gọi `LoadLocationsAsync()` và cập nhật trực tiếp `AllLocationsControl`. Khi quản trị viên thêm, sửa hoặc xóa proxy trên Web CMS, danh sách vị trí trên Desktop App lập tức phản chiếu dữ liệu mới mà **KHÔNG CẦN BUILD LẠI HOẶC KHỞI ĐỘNG LẠI PHẦN MỀM**.
- **Tiêu chuẩn Dead Proxy & Auto-Purge**:
  + Proxy được đánh giá là chết (OFFLINE) khi thời gian phản hồi Ping > 3500ms, TCP Connection Refused, lỗi xác thực SOCKS5 hoặc HTTP Status 502/503/407. Web CMS cung cấp nút bấm 1-click **"Xóa Proxy Chết"** để dọn sạch Database.

## 20. Hệ Thống Thiết Kế Giao Diện Mới Tông Xanh Lá Cây Đậm (Deep Emerald UI Redesign Suite)
- **Thư mục đầu ra**: `Redesign/`.
- **Hệ thống tệp nguyên mẫu tương tác (Interactive Clickable Prototypes)**:
  1. `Redesign/index.html`: **Master Showcase Hub** chuyển đổi linh hoạt giữa Desktop App, Web CMS và Design Tokens.
  2. `Redesign/desktop_app.html`: **Desktop Client Prototype** (.NET 10 WPF 960x640) - Power Orb đa trạng thái, đồng hồ MB thời gian thực, duyệt vị trí phân loại, Mesh Devices và Cyberpunk Toggles.
  3. `Redesign/cms_admin.html`: **Web Admin CMS Portal** (Port 6033) - Bento Stats, Bộ lọc quốc gia động, 1-Click Ping, 1-Click Xóa Proxy Chết, Quản lý VLESS Reality và Cú pháp KikiLogin.
  4. `Redesign/tokens.html`: **Design Tokens & UI Kit** - Bảng màu HEX/RGB, SolidColorBrush và ResourceDictionary XAML sẵn sàng tích hợp vào `theme.dark.xaml`.
  5. `Redesign/XAML_MAPPING_GUIDE.md`: **Tài liệu Hướng dẫn Ánh xạ Kỹ thuật XAML** - Chi tiết cấu trúc ánh xạ HTML/CSS sang C# và WPF Controls theo chuẩn song ngữ.

## 21. Tích Hợp Kho 234 Cờ Quốc Gia Định Dạng Raster PNG & Kiến Trúc Phân Bản (Free vs Premium)
- **Hệ Thống Cờ Quốc Gia (234 Flag PNGs)**:
  + Toàn bộ 234 tệp cờ chuẩn ISO 2 ký tự (`us.png`, `vn.png`, `gb.png`, `sg.png`, `de.png`, `jp.png`, `fr.png`, `ca.png`, `au.png`, `nl.png`, `kr.png`, `hk.png`, v.v.) được sao chép từ `apps/desktop/resources/flags/` sang `Redesign/flags/`.
  + Giao diện Desktop Client và Web CMS Admin chuyển đổi 100% từ text emoji sang thẻ hình ảnh sắc nét `<img class="country-flag-img">` (34x24px header, 26x18px accordion, 18x13px filter pills) có viền mịn và đổ bóng tinh tế.
  + Hàm tương tác `selectCityNode()` tự động cập nhật cờ quốc gia tương ứng trên widget Header màn hình chính theo thời gian thực.
- **Kiến Trúc Phân Bản Giao Diện**:
  + **Free Edition (`Redesign/desktop_app_free.html`)**: Hạn mức 300MB/ngày, thanh tiến trình đo dung lượng thời gian thực, cảnh báo khi vượt hạn ngạch và Popup Paywall nâng cấp lên Premium.
  + **Premium VIP Edition (`Redesign/desktop_app_premium.html`)**: Không giới hạn băng thông (Unlimited 10Gbps), mở khóa toàn bộ danh sách thành phố tại Mỹ và các cụm proxy cư dân riêng biệt.

## 22. Quy Chuẩn Bố Cục Thẻ Server Locations & Kiểm Thử Thị Giác Bằng Browser Subagent (Layout Stabilization & Visual QA)
- **Chuẩn Bố Cục Thẻ Quốc Gia (Country Accordion Card Sizing)**:
  + Mọi thẻ quốc gia trong danh sách `.locations-list-scroll` phải có thuộc tính `flex-shrink: 0 !important`, `min-height: 48px`, khoảng cách `gap: 6px` và padding `9px 12px` để triệt tiêu hiện tượng co rúm (squished layout) do Flexbox.
  + Khung chứa cờ `.country-flag-wrapper` cố định tỷ lệ 32x22px có bo góc 4px và đổ bóng viền mờ `0 1px 3px rgba(0,0,0,0.08)`.
  + Phân cấp nhãn Typography: Tên quốc gia `.country-name-title` 13px SemiBold, phụ đề trạng thái node `.country-meta-subtitle` 10.5px Medium màu `#64748b` (không bị cắt cụt chiều cao).
  + Mũi tên trạng thái sử dụng vector SVG mượt mà `.accordion-chevron` có hiệu ứng xoay góc 180 độ.
- **Quy Chuẩn Cuộn Flexbox Đa Tầng (Nested Flex Scrollbar Rule)**:
  + Mọi container cha lồng nhau (`.content-viewport`, `.view-panel`, `.locations-container`, `.locations-list-scroll`) phải có thuộc tính `min-height: 0` để vùng cuộn nội bộ vận hành trơn tru mà không làm giãn hoặc biến dạng kích thước khung cửa sổ WPF 960x640.
- **Quy Trình Kiểm Thử Tự Động Hóa (Browser Subagent Visual QA)**:
  + Sử dụng Browser Subagent mở trực tiếp các tệp HTML thiết kế, thực thi kịch bản click mô phỏng chuột, cuộn danh sách và chụp ảnh nghiệm thu (Screenshot artifacts) để đảm bảo 100% không còn lỗi mất cân đối hoặc xô lệch trước khi bàn giao.

## 23. Quy Chuẩn Cờ Chữ Nhật Phẳng, Bản Quyền Khóa Mã Phần Cứng (1 PC + 1 Mobile) & Kiến Trúc SaaS Suite
- **Quy chuẩn hiển thị Cờ Quốc Gia (Rectangular National Flags)**:
  + Toàn bộ cờ quốc gia trên tất cả màn hình (Desktop App, Free, Premium, CMS Admin, Tokens) được chuẩn hóa sang hình chữ nhật phẳng 28x20px hoặc 32x22px (`border-radius: 2px`, `object-fit: cover`, viền mảnh 1px `rgba(0,0,0,0.12)`).
  + Tuyệt đối không sử dụng cờ nửa tròn nửa vuông, cờ hình oval hay squircle border.
- **Kiến trúc Bản Quyền Khóa Mã Phần Cứng (1 PC + 1 Mobile HWID Enforcement)**:
  + **Đặc tả kiến trúc**: `DOCS/Architecture/SAAS_LICENSING_AND_HWID_ENFORCEMENT_SPEC.md`.
  + **CSDL Cấu trúc**: `Backend/data/schema_saas_enterprise.sql`.
  + **Công thức sinh HWID**:
    * PC: `SHA256(Win32_Processor.ProcessorId + Win32_BaseBoard.SerialNumber + Win32_ComputerSystemProduct.UUID + PrimaryMAC)`.
    * Mobile: `IDFV (iOS)` hoặc `SSAID/Widevine Device ID (Android)`.
  + **Chính sách cấp phát thiết bị**: Mỗi License Key chỉ được phép liên kết tối đa 1 Slot PC và 1 Slot Mobile song song.
  + **Chống chia sẻ tài khoản (Anti-Sharing)**: Khi máy PC thứ 2 kích hoạt, API trả về mã lỗi `HTTP 409 Conflict (DEVICE_LIMIT_EXCEEDED)`, hiển thị modal cảnh báo trên client và cung cấp tùy chọn chuyển đổi / đá thiết bị cũ (`Kick / Transfer Device`).
- **Trọn Bộ 7 Phân Hệ Enterprise SaaS CMS (`Redesign/cms_admin.html`)**:
  1. 📊 **Executive Dashboard**: Chỉ số tài chính MRR ($48,250), ARR ($579,000), 6,310 Users (3,420 PC vs 2,890 Mobiles), Live Throughput (4.82 Gbps).
  2. 👥 **Quản Lý Người Dùng & Thuê Bao**: Danh bạ, hạng mức Quota, gán thiết bị HWID, nút Reset HWID.
  3. 🔑 **Quản Lý License Key & Khóa HWID**: Bảng key, trạng thái ràng buộc PC/Mobile, Modal sinh key hàng loạt (Batch Key Generator).
  4. 💳 **Doanh Thu & Cổng Thanh Toán**: Tích hợp Stripe, Crypto USDT TRC20/BEP20, VietQR, PayPal và sổ cái giao dịch.
  5. 🤝 **Đối Tác Đại Lý & Tiếp Thị Liên Kết (Resellers & Affiliates)**: Quản lý đại lý bán sỉ (chiết khấu 30%-55%), hoa hồng liên kết (25%), ví hoa hồng.
  6. 🌐 **Cụm Cổng Proxy Cư Dân (Port 10000)**: 42 node proxy kèm cờ chữ nhật, Ping 1-click, Xóa node chết 1-click, Form Bulk Import.
  7. 🛡️ **Bảo Mật & Nhật Ký Gian Lận Chia Sẻ (Anti-Sharing Audits)**: Nhật ký kiểm toán an ninh thời gian thực, cảnh báo truy cập bất thường.## 24. Chuyển Đổi Huy Hiệu Cờ Tròn (Circular Flag Badges) & Cơ Chế Khóa Vùng Bản Free (5 Nước)
- **Huy Hiệu Cờ Tròn (1:1 Circular Flags)**:
  + Toàn bộ cờ 234 quốc gia được chuẩn hóa sang hình tròn 50% radius (`border-radius: 50%`, `aspect-ratio: 1/1`, `object-fit: cover`, viền mảnh 1.5px và bóng đổ nhẹ).
- **Phân Định Bản Free vs Premium (Country Gating & Paywall Trigger)**:
  + **Bản Free (`desktop_app_free.html`)**: Chỉ cho phép 5 quốc gia mở kết nối (USA, Vietnam, UK, Singapore, Germany). 7 quốc gia còn lại bị khóa mờ xám (`.locked-country-card`, `opacity: 0.55`, `filter: grayscale(85%)`, badge `🔒 VIP Only`). Bấm vào nước bị khóa sẽ kích hoạt Paywall Modal nâng cấp bản quyền.
  + **Bản Premium (`desktop_app_premium.html`)**: Mở khóa 100% toàn bộ 12 quốc gia và tất cả 234 vị trí trên thế giới.

## 25. Dàn VPS Trung Chuyển Phân Tán (Multi-Relay Shield) & Quản Lý Quota 10GB/Tháng (~300MB/Ngày)
- **Dàn VPS Trung Chuyển (Multi-Relay Fan-Out Shield)**:
  + Dịch vụ quản lý: `Backend/services/RelayPoolManagerService.cs`.
  + **3 Chế độ Chuyển Mạch Master Switch**:
    1. `FullRelay (100%)`: Toàn bộ luồng kết nối đi qua 4 VPS trung chuyển (Singapore, Tokyo, Frankfurt, Los Angeles). Chủ proxy bên ngoài chỉ nhìn thấy IP của VPS Relay, bảo vệ tuyệt đối IP máy chủ Core NextAiVPN.
    2. `SemiRelay (Bán phần)`: User VIP ưu tiên qua Relay; User Free cân bằng tải trực tiếp hoặc chỉ qua Relay khi tải của cụm dưới 60%.
    3. `DirectBypass (Tắt Relay khi quá tải)`: Core kết nối trực tiếp Upstream Proxy để tránh nghẽn băng thông dàn Relay.
  + **Cân bằng tải theo Trọng số (Weight Slider 1-100%)** & **Khớp vùng địa lý (Geo-Matching)**.
- **Động Cơ Quản Lý Hạn Mức 10GB/Tháng (~300MB/Ngày) Cho Hàng Nghìn Proxy**:
  + Mỗi proxy được gán định mức `300 MB/ngày` (`DailyQuotaBytes = 314572800`).
  + Hạch toán băng thông thực tế 2 chiều qua cổng Gateway 10000.
  + Khi proxy chạm ngưỡng 300MB hôm nay -> Đánh dấu `IsExhaustedToday = true`, `SmartProxyRotationEngine` tự động loại bỏ proxy này và tự động đảo kết nối sang các proxy còn hạn mức khác.
  + Cơ chế tự động Reset 300MB/ngày vào 00:00 UTC mỗi ngày (`ResetDailyQuotas()`).
- **Phân Cấp Proxy Pool**:
  + **⭐ VIP Pool**: Proxy có xác thực `User:Pass`, tốc độ 1Gbps+, kết nối qua Relay VPS ẩn danh.
  + **🟢 Free Pool**: Proxy không có User/Pass (IP Whitelist / Public), giới hạn tốc độ 10Mbps, chia tải trực tiếp.
- **CMS Admin Tab 6**: Tích hợp Master Switch 3 chế độ, 4 thẻ VPS Relay kèm thanh trượt trọng số, Bento Stats quản lý hàng nghìn proxy, bộ lọc phân cấp (VIP, Free, Hết Quota), thanh tiến trình đo 300MB/ngày và nút Reset 1-Click.

## 26. Ghép Toàn Diện Giao Diện Mới (Deep Emerald Theme) Vào WPF .NET 10 & Single-File Setup
- **Hệ thống màu sắc Emerald chuẩn**:
  + Thống nhất trên `ColorBrush.xaml`, `theme.light.xaml`, `theme.dark.xaml`: `ColorEmerald900` (`#064E3B`), `ColorEmerald700` (`#047857`), `ColorEmerald600` (`#059669`), `ColorEmerald100` (`#DCFCE7`), `ColorBorderEmerald` (`#A7F3D0`).
- **Huy hiệu cờ tròn 1:1 trong XAML**:
  + Sử dụng `Border` với `CornerRadius="12"`, `ClipToBounds="True"` bao bọc `Image` tại `WindowHeader.xaml`, `ExpandedMainPanel.xaml`, `AllLocationListItem.xaml` và `FavoriteLocationListItem.xaml`.
- **Thẻ bản quyền HWID 1 PC + 1 Mobile**:
  + Tích hợp tại `ExpandedAccount.xaml` & `ExpandedAccountViewModel.cs` hiển thị mã `MachineHwidText`, Slot 1 (PC Active) và Slot 2 (Mobile Linked).
- **Thanh đo Quota 300MB/Ngày**:
  + Tích hợp tại `ConnectionData.xaml` hiển thị tiến trình sử dụng quota trong ngày.
## 27. Tái Cấu Trúc Toàn Diện Desktop Client Thành Chuẩn 2 Cột (2-Column Clean Emerald Layout)
- **Chuẩn hóa bố cục 2 Cột (`VPNWindowExpanded.xaml`)**:
  + Thay thế bố cục 3 cột decompiler cũ bằng bố cục 2 vùng hiện đại theo đúng nguyên mẫu `Redesign/desktop_app.html`:
    * Cột 0: Width="220" (`ExpandedSideMenu.xaml`).
    * Cột 1: Width="*" (Content Viewport lồng nhau hiển thị Dashboard, Locations, Settings, Protocols, Account, v.v.).
- **Thanh Tiêu Đề Thông Minh (`WindowHeader.xaml`)**:
  + Logo Clean Emerald sang trọng và Title "NextAi VPN".
  + Status Pill thời gian thực (`🟢 Protected - 192.111.130.5` / `🔴 Disconnected`).
  + Chuông thông báo chuyển nhanh tab Notifications & Avatar pill chuyển nhanh tab Account.
- **Thanh Điều Hướng 8 Tab Phân Cụm & Thẻ Quota 300MB (`ExpandedSideMenu.xaml`)**:
  + 8 tab danh mục chuẩn: Dashboard, Locations, Residential Mesh, Speed Test, Protocol, Security, Account, General Settings.
  + Thẻ Quota Free 300MB/ngày kèm thanh tiến trình mượt mà và nút `[⚡ Upgrade VIP]`.
- **Màn Hình Chính Dashboard Hiện Đại (`ExpandedMainPanel.xaml`)**:
  + Thẻ Server Header với cờ tròn 1:1 và nút "Change Server".
  + Big Power Orb Button 140px phát sáng với hiệu ứng chuyển đổi trạng thái (Connected Emerald, Connecting Amber, Disconnected Gray).
  + Live Bandwidth Metrics Grid (Download MB + MB/s & Upload MB + MB/s).
  + Thẻ Connection Telemetry (IP Address, Universal Port 10000, Kill Switch, DNS Leak, Mesh status).
  + Thẻ Quick Switches (Auto-Connect, WFP Kill Switch, Split Tunneling, Residential Node).
- **Phân phối & Đóng gói**:
  + Biên dịch sạch 100% Release .NET 10 (0 Errors).
  + Tự động cập nhật trực tiếp vào thư mục cài đặt `%LocalAppData%\Programs\NextAiTechnology\NextAiVPN`.
  + Tái xuất bản bộ cài đặt Single-File [NextAiVPN_Setup.exe](file:///E:/DECOMPILER/Soft/VPN/CONVERT/NextAiVPN_Setup.exe).

## 28. Quy Trình Design Gate Khép Kín: Khắc Phục Lệch Thiết Kế & Bổ Sung 100% Chức Năng (TASK-050)
- **Thiết Kế & Tái Cấu Trúc Toàn Diện Theo Chuẩn V2.1 Design-Gated**:
  + **Khoảng trống giao diện trước đây**: Decompiler nguyên bản thiếu các UserControl chuyên biệt cho Tab 3 (Residential Mesh Hub) và Tab 4 (Speed Test); Tab 2 (`ExpandedLocations`) còn giữ header cũ và thiếu dải nút lọc.
  + **Bổ sung `ResidentialMeshControl.xaml/.cs`**:
    * Thanh Banner Gateway tập trung `127.0.0.1:10000` kèm chỉ số bảo vệ Decoupling Law (`System Proxy: OFF`).
    * 3 thẻ Client Node động (`win_c0a8019b`, `win_89fe121a`, `vn_hanoi_vnpt`) kèm nút copy 1 chạm cú pháp KikiLogin (`127.0.0.1:10000:node-[DeviceId]:nextai123`).
    * Nút chuyển trạng thái Client Node On/Off và chia sẻ lưu lượng an toàn.
  + **Bổ sung `SpeedTestControl.xaml/.cs`**:
    * Đồng hồ đo tốc độ hình tròn (Speedometer circular gauge) tích hợp kim xoay động và hiệu ứng phát sáng Emerald.
    * 4 chỉ số đo lường: Download (184.5 Mbps), Upload (92.4 Mbps), Latency (38 ms), Packet Loss (0.0%).
    * Nút điều khiển "Run Speed Test" với mô phỏng đo đạc thời gian thực.
  + **Nâng cấp `NextAiVPN.ExpandedLocations.xaml`**:
    * Thanh tìm kiếm Clean Emerald cao cấp kèm nút lọc tức thì (`All Locations (42)`, `⚡ For Streaming`, `⭐ Favorites`).
    * Huy hiệu cờ tròn 1:1 cho toàn bộ danh sách 234 quốc gia.
  + **Định tuyến toàn vẹn 8 Tab (`ExpandedSideMenu.cs` & `VPNWindowExpanded.xaml`)**:
    * Định tuyến chuyển đổi mượt mà không độ trễ giữa: Dashboard, Locations, Residential Mesh, Speed Test, Protocols, Settings, Account (HWID 1 PC + 1 Mobile), và Notifications.
- **Quy Trình Kiểm Thử Tự Động AutoTestAgent (Agent 6 - TESTER)**:
  + Cập nhật `AutoTestAgent.cs` để quét kiểm thử lần lượt 8 Tab giao diện, chụp ảnh snapshot và kiểm toán Decoupling Law (`ProxyEnable = 0`).
  + Toàn bộ 10/10 bước kiểm thử thành công, xuất báo cáo đầy đủ tại `Report/16_Auto_Design_Gated_Parity_Verification_Report.md`.
## 29. Cấu Hình Git & Triển Khai Mã Nguồn Lên GitHub (TASK-054)
- **Kho lưu trữ GitHub**: `https://github.com/netvietsoft/VPN` (nhánh chính `main`).
- **Cấu hình `.gitignore`**:
  + Loại trừ toàn bộ thư mục biên dịch .NET 10 (`**/bin/`, `**/obj/`, `**/publish/`), Visual Studio cache (`.vs/`), nhật ký runtime (`*.log`, `corehost.log`, `app_run.log`, `crash.log`).
  + Loại trừ các tệp đóng gói cài đặt lớn vượt hạn mức 100MB của GitHub: `NextAiVPN_Setup.exe` (144.7 MB) và `installer/NextAiVPN_Installer.msi` (112.6 MB).
  + Bảo tồn tệp nhúng `apps/installer/NextAiVPN.Setup/payload.zip` (81.69 MB < 100MB) và cung cấp script `scripts/package_installer.ps1` để tự động build bộ cài Single-File.
- **Xác thực SSH Deploy Key**:
  + Tạo khóa SSH chuyên biệt `~/.ssh/id_ed25519_vpn` gắn quyền Write trực tiếp vào kho lưu trữ `netvietsoft/VPN`.
  + Thiết lập `core.sshCommand` trỏ vào khóa định danh chuyên biệt `id_ed25519_vpn`.
- **Trạng thái Git**: 100% mã nguồn (4,141 files) đã được commit sạch và đẩy thành công (`main -> origin/main`).

