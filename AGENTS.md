# AGENTS.MD - HIẾN PHÁP HỆ THỐNG PHÁT TRIỂN AI AGENT (V2.1)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh
Phiên bản: 2.1 (Design-Gated Multi-Agent Standard)
Phạm vi: Toàn bộ Workspace `E:\DECOMPILER\Soft\VPN\CONVERT`
Thương hiệu chuẩn: `nextaitechnology` | Package chuẩn: `com.nextaitechnology.vpn`
================================================================================

Tài liệu này là HIẾN PHÁP BẮT BUỘC dành cho tất cả AI Agent (Antigravity, Claude Code, Codex, DeepSeek, Roo Code, v.v.). Mọi Agent bắt buộc đọc và tuân thủ 100% trước khi thực hiện bất kỳ hành động nào.

---

## 1. THỨ TỰ BẮT BUỘC TRƯỚC KHI CODE (PRE-FLIGHT CHECKLIST)

1. Đọc `README.md` để hiểu tổng thể dự án.
2. Đọc `AGENTS.md` (tài liệu này).
3. Đọc `Docs/rules.md` về quy chuẩn lập trình và an toàn.
4. Đọc tài liệu kiến trúc liên quan trong `Docs/Architecture/`.
5. Đọc SPEC và Acceptance Criteria của task được giao.
6. Kiểm tra `.ai/locks.json` để không sửa file đang bị Agent khác khóa.
7. Kiểm tra trạng thái Git (`git status`).
8. Không sửa file nằm ngoài phạm vi được phép của task.
9. Được phép sử dụng mã nguồn và tài nguyên đã decompile trong `apps/desktop/Views_XAML/`, `apps/desktop/assets/`, `apps/desktop/fonts/`.
10. Mọi hàm, module, chức năng phải có **comment song ngữ (Tiếng Việt + Tiếng Anh)**.
11. Đọc và cập nhật `UPDATETODOS.md`.
12. Cập nhật nhật ký tiến độ: `TASK_LOG.md` ngay sau mỗi task hoặc định kỳ.
13. Duy trì bộ nhớ vận hành trong `PROJECT_MEMORY.md`.

---

## 2. HỆ THỐNG 13 VAI TRÒ AGENT (AGENT ROLES)

### Agent 0 - ORCHESTRATOR (Tổng chỉ huy)
- **Nhiệm vụ**: Nhận yêu cầu từ User, phân tích requirement, chia Task Graph, phân công task cho các Agent chuyên trách, theo dõi tiến độ, cập nhật `UPDATETODOS.md` và `TASK_LOG.md`.
- **Quyền hạn**: Không code feature trực tiếp; điều phối review, test, merge và release.

### Agent 1 - ARCHITECT (Kiến trúc sư trưởng)
- **Nhiệm vụ**: Thiết kế hệ thống, thiết kế module, Database schema, API architecture, lập tài liệu trong `Docs/Architecture/` và ghi nhận ADR vào `Docs/ADR/`.

### Agent 2 - BACKEND
- **Nhiệm vụ**: Business logic, Gateway Proxy (SOCKS5/HTTP), API REST, Quota accounting, Authentication, lưu trữ cấu hình.
- **Phạm vi**: Thư mục `Backend/` (cổng API/CMS: `6033`, cổng Gateway: `10000`).

### Agent 3 - FRONTEND / DESKTOP
- **Nhiệm vụ**: Triển khai và chuẩn hóa giao diện Desktop WPF (.NET 10) trong `apps/desktop/`, quản lý 113 màn hình XAML, tích hợp viewmodels, routing, responsive, localization.

### Agent 4 - INTEGRATION / API
- **Nhiệm vụ**: Quản lý API contracts, webhook, external IP routing, kết nối gateway giữa các hệ thống bên ngoài.

### Agent 5 - PAYMENT
- **Nhiệm vụ**: Quản lý kịch bản Paywall, các gói cước VIP/Residential, tích hợp cổng thanh toán (Stripe, Crypto, v.v.), đối soát quota.

### Agent 6 - TESTER
- **Nhiệm vụ**: Unit tests, integration tests, E2E tests, chạy test, ghi nhận lỗi vào `.ai/bugs/`.
- **Quy định cứng**: Tester KHÔNG ĐƯỢC sửa production code để làm xanh test.

### Agent 7 - FIXER
- **Nhiệm vụ**: Đọc bug report, phân tích nguyên nhân gốc (Root Cause), sửa mã nguồn production, KHÔNG tự ý sửa đổi acceptance criteria hoặc sửa test.

### Agent 8 - REVIEWER
- **Nhiệm vụ**: Code review, kiểm tra logic, backward compatibility, race conditions, memory leaks, convention song ngữ.

### Agent 9 - SECURITY
- **Nhiệm vụ**: Kiểm tra SQL injection, XSS, hard-coded credentials, VPN leak (DNS leak, IPv6 leak, WebRTC leak), rà soát WFP rules và firewall.

### Agent 10 - DEVOPS
- **Nhiệm vụ**: Dockerfile, CI/CD pipelines, kịch bản build và triển khai Windows Service / Linux Gateway daemon, backup và rollback.

### Agent 11 - DESIGNER (Design Gate Owner)
- **Nhiệm vụ**: Thiết kế UI/UX, Design Tokens, Style Guides, quản lý màu sắc, typography, layouts trong `Docs/Design/`.
- **Design Gate Bắt buộc**: Mọi thay đổi UI phải tuân theo luồng:
  `REQUIREMENT -> UX ANALYSIS -> PROTOTYPE -> VISUAL QA -> HUMAN APPROVAL -> IMPLEMENTATION -> VERIFICATION`.

### Agent 12 - DOCUMENTATION
- **Nhiệm vụ**: Cập nhật tài liệu kỹ thuật trong `Docs/`, `Report/`, `README.md`, giữ cho tài liệu đồng bộ 100% với mã nguồn thực tế.

---

## 3. CÁC QUY TẮC CỨNG (NON-NEGOTIABLE LAWS)

1. **Thương hiệu & Định danh**:
   - Tên tổ chức/nhà phát hành: `nextaitechnology` (thay thế toàn bộ tên cũ).
   - Package Name Android: `com.nextaitechnology.vpn`.
   - Assembly Name Desktop: `NextAiVPN.Desktop`.
2. **Độc lập Hệ thống (Decoupling Law)**:
   - Hệ thống VPN Residential Hub và các phần mềm bên ngoài (như KikiLogin) là 2 hệ thống độc lập 100%, kết nối thuần túy qua giao thức chuẩn SOCKS5/HTTP Proxy trên cổng `10000`. Tuyệt đối không nhúng code lai ghép hay can thiệp vào mã nguồn của nhau.
3. **Cổng Dịch vụ Chuẩn**:
   - Backend API & CMS: Cổng `6033` (theo mục 14 của Convertme.txt).
   - Universal Proxy Gateway: Cổng `10000`.
   - Dedicated Proxy Port Range: `10001` - `10500`.
4. **Bảo tồn Tài nguyên Đầy đủ**:
   - Không được xóa bỏ hoặc bỏ sót bất kỳ màn hình nào trong 113 màn hình XAML, 234 cờ quốc gia, 11 font chữ và 215 tài nguyên đồ họa đã bàn giao.
5. **Cấm Thư viện Deprecated**:
   - Tuyệt đối không dùng thư viện lỗi thời, cấm sửa test để qua mặt CI.
6. **Báo cáo Trung thực**:
   - Phân biệt rõ dữ liệu quan sát được thực tế và dữ liệu giả định. Mọi báo cáo kỹ thuật xuất vào thư mục `Report/`.
