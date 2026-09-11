# HỆ THỐNG THIẾT KẾ & QUY CHUẨN DESIGN GATE (DOCS/DESIGN/DESIGN_SYSTEM.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

Tài liệu này định nghĩa Design System chuẩn, bộ Design Tokens (màu sắc, typography, spacing) được trích xuất từ các file XAML gốc (`theme.dark.xaml`, `theme.light.xaml`, `colorbrush.xaml`, `fonts.xaml`) và các quy tắc để User / AI Designer thực hiện tái thiết kế giao diện sau này.

---

## 1. DESIGN TOKENS - BẢNG MÃ MÀU CHUẨN (COLOR TOKENS)

### A. Dark Theme Palette (Chủ đạo)
| Token Name | Hex Code | Ứng Dụng |
| :--- | :--- | :--- |
| `--bg-base` | `#0A0E17` / `#1E1E20` | Nền chính của ứng dụng và các cửa sổ |
| `--bg-surface` | `#111827` / `#262629` | Nền thẻ (card), sidebar, headers |
| `--bg-surface-elevated` | `#1E293B` / `#323236` | Nền modal, popup dropdown, input fields |
| `--border-color` | `rgba(255, 255, 255, 0.08)` / `#59595F` | Đường viền phân cách, viền nút bấm |
| `--primary` | `#2563EB` / `#3B82F6` | Màu nhấn hành động (Connect Button, Active Tab) |
| `--primary-glow` | `rgba(59, 130, 246, 0.25)` | Hiệu ứng phát sáng đèn trạng thái kết nối |
| `--success` | `#10B981` | Đèn trạng thái Connected, IP sạch (Clean Score) |
| `--warning` | `#F59E0B` | Cảnh báo hạn ngạch băng thông (Bandwidth Quota Warning) |
| `--danger` | `#EF4444` | Trạng thái ngắt kết nối, lỗi mạng, Disconnected |
| `--text-main` | `#F8FAFC` / `#FFFFFF` | Chữ tiêu đề chính, thông số nổi bật |
| `--text-muted` | `#94A3B8` / `#B1B1B2` | Nhãn phụ, mô tả, chỉ dẫn |

### B. Light Theme Palette (Giao diện Sáng)
| Token Name | Hex Code | Ứng Dụng |
| :--- | :--- | :--- |
| `--bg-base-light` | `#F8FAFC` | Nền sáng tổng thể |
| `--bg-surface-light` | `#FFFFFF` | Nền khối thẻ, danh sách vị trí |
| `--border-color-light` | `#E2E8F0` | Đường viền giao diện sáng |
| `--text-main-light` | `#0F172A` | Chữ chính trên nền sáng |
| `--text-muted-light` | `#64748B` | Chữ phụ trên nền sáng |

---

## 2. QUY CHUẨN TYPOGRAPHY & PHÔNG CHỮ (FONTS)
Toàn bộ phông chữ bản quyền đã được bảo tồn tại `apps/desktop/fonts/`:
- **`MuseoSans-300.otf`**: Tiêu đề mảnh, số liệu thống kê lớn.
- **`MuseoSans_500.otf`**: Văn bản nội dung, nhãn nút bấm, navigation tabs (`FontFamily="{DynamicResource MuseoSans500FontFamily}"`).
- **`MuseoSans_700.otf`**: Tiêu đề in đậm, trạng thái kết nối, tên gói cước.
- **`Helvetica LT` (Ultra Light, Thin, Light, Roman, Medium, Bold, Heavy)**: Sử dụng cho các màn hình phụ và bảng thông số kỹ thuật.
- **`Segoe UI` (`segoe_ui.ttf`)**: Phông hệ thống dự phòng chuẩn Windows 11.

---

## 3. NGUYÊN TẮC BỐ CỤC RESPONSIVE & CARD-BASED (ITEM 15 CONVERTME)
1. **Cấu trúc URL chuẩn**: `domain.com/Menu cha/menu con` (ví dụ: `vpn.nextai.com/nodes/residential`, `vpn.nextai.com/settings/protocols`).
2. **Hiển thị Card-Based trên màn hình nhỏ**: Khi thu nhỏ màn hình hoặc truy cập trên thiết bị di động, bảng dữ liệu nhiều cột (như danh sách Node hoặc danh sách Khách hàng) tự động chuyển đổi hiển thị thành dạng **Thẻ độc lập (Card)** với thông tin xếp dọc, đảm bảo không bị tràn ngang hay xô lệch giao diện.
3. **Bo góc & Hiệu ứng Windows 11**: Mọi thẻ và nút bấm áp dụng bán kính cong `border-radius: 12px` - `16px`, kết hợp đổ bóng mềm và backdrop filter blur `12px`.
