# QUY CHUẨN LẬP TRÌNH & NGUYÊN TẮC PHÁT TRIỂN (DOCS/RULES.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

Tài liệu này chứa các quy tắc và luật code chung áp dụng cho toàn bộ dự án. Mọi Agent và Developer đều bắt buộc tuân theo.

---

## 1. NGUYÊN TẮC ĐẶT TÊN & THƯƠNG HIỆU (BRANDING & NAMING)
- **Tên tổ chức / Nhà phát triển**: Bắt buộc dùng `nextaitechnology`. Không sử dụng bất kỳ tên tổ chức hay nhà phát hành cũ nào từ bản decompile (như Namecheap, WLVPN, v.v.).
- **Package Name (Android)**: `com.nextaitechnology.vpn`.
- **Root Namespace (.NET)**: `nextaitechnology.vpn` hoặc `NextAiVPN`.
- **Tên biến, hàm**: `camelCase` (trong TypeScript, Kotlin, Dart), `PascalCase` cho method và property trong C#.
- **Tên lớp (Class, Interface, Struct)**: `PascalCase`. Interface bắt đầu bằng tiền tố `I` (ví dụ: `IUniversalGatewayService`).
- **Tên file**: Giữ tính nhất quán với ngôn ngữ (C# file: `PascalCase.cs`, Kotlin: `PascalCase.kt`, TypeScript/JS: `camelCase.ts` hoặc `kebab-case.ts`).

---

## 2. QUY ĐỊNH CHÚ THÍCH SONG NGỮ (BILINGUAL COMMENTS)
Theo mục 1a của `Convertme.txt`, mọi hàm, interface, khối logic phức tạp đều bắt buộc phải có chú thích bằng **cả Tiếng Việt và Tiếng Anh**:
```csharp
/// <summary>
/// [VI] Khởi động động cơ Universal Proxy Gateway trên cổng chỉ định (hỗ trợ cả SOCKS5 và HTTP CONNECT).
/// [EN] Starts the Universal Proxy Gateway engine on the specified port (supports both SOCKS5 and HTTP CONNECT).
/// </summary>
/// <param name="port">[VI] Số hiệu cổng lắng nghe / [EN] The port number to listen on</param>
/// <param name="cancellationToken">[VI] Token hủy luồng / [EN] Cancellation token</param>
public void Start(int port, CancellationToken cancellationToken);
```

---

## 3. NGUYÊN TẮC BẢO TỒN VÀ BẢO MẬT (SECURITY & ASSET INTEGRITY)
- **Bảo tồn toàn vẹn UI**: Không xóa, sửa sai lệch hoặc làm mất bất kỳ thành phần nào trong 113 màn hình XAML, 234 cờ quốc gia, 11 font chữ Museo Sans/Helvetica và 215 tài nguyên đồ họa vector/raster.
- **Không Hardcode Bí Mật**: Không để lộ API Key, Secret Token thật trong mã nguồn. Sử dụng biến môi trường hoặc file `appsettings.json` / `.env`.
- **Không Sửa Test Để "Làm Xanh"**: Tester Agent tuyệt đối không sửa đổi mã nguồn production; Fixer Agent tuyệt đối không sửa đổi kiểm thử để vượt qua CI một cách giả tạo.
- **Báo cáo Trung Thực**: Tuyệt đối không được báo cáo sai sự thật. Mọi phát hiện phải được ghi nhận rõ ràng: quan sát thực tế (observed) vs suy luận (inferred).

---

## 4. QUY CHUẨN THIẾT KẾ GIAO DIỆN (DESIGN GATE)
Mọi thay đổi giao diện phải đi qua Design Gate:
1. **Thiết kế**: Tạo Prototype / Wireframe dựa trên Design System tokens (`Docs/Design/DESIGN_SYSTEM.md`).
2. **Theme**: Bắt buộc hỗ trợ cả Giao diện Tối (Dark Theme) và Giao diện Sáng (Light Theme).
3. **Bố cục**: Responsive trên cả Desktop và Mobile. Các bảng có nhiều cột dữ liệu phải được chuyển đổi hoặc hiển thị dưới dạng **Thẻ (Card-based layout)** trên màn hình nhỏ.
4. **Cấu trúc URL**: `domain.com/menu-cha/menu-con`.
