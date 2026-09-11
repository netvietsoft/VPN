# SƠ ĐỒ CẤU TRÚC THƯ MỤC VÀ TRÁCH NHIỆM (DOCS/CAU_TRUC_THU_MUC.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

Tài liệu này trình bày chi tiết cây thư mục toàn diện của dự án và nhiệm vụ của từng thư mục theo đúng chuẩn `Development_Workspace_Standard_V2.1_Design_Gated.txt`.

```
E:\DECOMPILER\Soft\VPN\CONVERT/
├── Backend/                                # Backend API + Server + CMS trên cổng 6033
│   ├── api/                                # REST API Controllers, Endpoints, Swagger
│   ├── feature/                            # Feature Modules (Proxy Node Pool, Traffic, Subscriptions)
│   ├── services/                           # Gateway Engine, Quota Engine, Security Filter
│   ├── cms/                                # Admin CMS Dashboard (Theme Sáng/Tối, Card-based, Responsive)
│   ├── docs/                               # Backend Specs & OpenAPI Schemas
│   ├── wwwroot/                            # Static assets cho Web CMS (HTML5/CSS3/Vanilla JS)
│   ├── Program.cs                          # Entry point Kestrel Web Server & Gateway
│   └── VpnBackend.csproj                   # Project configuration (.NET 10.0)
│
├── apps/
│   ├── desktop/                            # WPF Client .NET 10 (Full 113 XAML screens + assets)
│   │   ├── Views_XAML/                     # 113 màn hình và user controls XAML nguyên vẹn
│   │   ├── resources/                      # 234 cờ quốc gia, styles theme.dark/light.xaml, colorbrush.xaml
│   │   ├── assets/                         # 215 icons, graphics, illustrations (darkmode, fastvpn)
│   │   ├── fonts/                          # 11 font files Museo Sans, Helvetica LT, Segoe UI
│   │   ├── extracted/                      # Runtimes, WFP / WireGuard / OpenVPN drivers
│   │   ├── SDK/                            # Native VPN SDKs (Core, WFP, OpenVpn, Ras, DnsMonitor)
│   │   └── NextAiVPN.Desktop.csproj        # Solution/csproj độc lập
│   │
│   ├── android/                            # Scaffold Native Android Kotlin + Jetpack Compose
│   │   ├── app/src/main/java/com/nextaitechnology/vpn/
│   │   │   ├── ui/                         # Compose screens, themes, components
│   │   │   ├── data/                       # Repositories, API client, Local DB
│   │   │   ├── domain/                     # UseCases, Models
│   │   │   └── vpn/                        # VpnService Android native wrapper
│   │   └── build.gradle.kts                # Gradle script với package com.nextaitechnology.vpn
│   │
│   └── cms/                                # Web CMS quản trị độc lập (giao diện web chuẩn responsive)
│
├── packages/
│   ├── contracts/                          # API Contracts, Models, DTOs chung
│   ├── domain/                             # Core Domain Entities & Rules
│   └── shared/                             # Utility helpers, encryption, networking
│
├── Core_Service/                           # Windows Service daemon chạy SYSTEM (WFP / WireGuard NT)
│
├── Docs/                                   # Hệ thống tài liệu hiến pháp & kỹ thuật
│   ├── rules.md                            # Luật code chung, coding standards (VI + EN), cấm deprecated
│   ├── CAU_TRUC_THU_MUC.md                 # Tài liệu này (sơ đồ và chức năng từng thư mục)
│   ├── Architecture/                       # overview, frontend, backend, api, database, security
│   ├── ADR/                                # Quyết định kiến trúc (ADR-001, ADR-002)
│   ├── Contracts/                          # openapi.yaml, database-schema.md, error-codes.md
│   ├── Design/                             # DESIGN_SYSTEM.md, ui-principles.md, component-guidelines.md
│   ├── Security/                           # threat-model.md, secrets.md, rules.md
│   ├── Testing/                            # testing-strategy.md, unit-test.md, e2e-test.md
│   ├── Reconstruction/                     # overview.md, evidence-model.md, api-recovery.md, parity.md
│   └── Decisions/                          # decision-log.md
│
├── .ai/                                    # Machine-readable State & Agent Orchestration
│   ├── project.yaml                        # Project metadata
│   ├── agents.yaml                         # Định nghĩa 13 agents
│   ├── state.json                          # Machine-readable project state
│   ├── locks.json                          # File locks
│   ├── tasks/                              # Task tracker (backlog, ready, running, review, done)
│   ├── bugs/                               # Bug tracker
│   └── reconstruction/                     # Ledger, inventory, evidence
│
├── Report/                                 # 12 Báo cáo kỹ thuật chi tiết theo Convertme.txt
│   ├── 01_API_Endpoints_Report.md         # Mục 3, 4: Danh sách API, models, external domains, IP & ports
│   ├── 02_SDK_and_Libraries_Report.md     # Mục 5: Danh sách SDK, thư viện bên ngoài và vai trò
│   ├── 03_Assets_and_Resources_Catalog.md # Mục 8: Danh mục 234 flags, 11 fonts, 215 graphics
│   ├── 04_Directory_Structure_and_Duties.md # Mục 9: Sơ đồ thư mục và nhiệm vụ
│   ├── 05_Security_and_Vulnerability_Audit.md # Mục 10: Phân tích lỗ hổng, vector tấn công và giải pháp
│   ├── 06_Reconstruction_and_Parity_Blueprint.md # Mục 11, 13: Đánh giá tái dựng và blueprint backend
│   ├── 07_Sensitive_Keys_and_Secrets_Audit.md # Mục 12: Đào sâu phân tích key & độ nhạy cảm
│   ├── 08_Payment_Wall_and_Subscriptions.md # Mục 16: Phân tích paywall, gói sub, ads mapping
│   ├── 09_Screen_Flow_and_Navigation_Tree.md # Mục 17, 18: Luồng màn hình, menu tree, tabs
│   ├── 10_Screen_Inventory_and_Structure.md # Mục 19: Chi tiết 113 màn hình, controls, cấu trúc
│   ├── 11_Required_Credentials_and_Keys.md # Mục 21: Bảng thông số key user cần chuẩn bị
│   └── 12_UI_Design_Handoff_and_Tokens.md # Mục 22: Báo cáo handoff, tokens màu, layout cho AI redesign
│
├── AGENTS.md                               # Hiến pháp AI Agent bắt buộc
├── README.md                               # Giới thiệu tổng quan dự án
├── CLAUDE.md                               # Chỉ dẫn Claude Code
├── .clinerules                             # Chỉ dẫn Cline / Roo Code
├── UPDATETODOS.md                          # Trạng thái tiến độ công việc trực tiếp
├── TASK_LOG.md                             # Nhật ký tiến độ chi tiết của các Agent
├── PROJECT_MEMORY.md                       # Bộ nhớ vận hành dự án
├── Tasksrequiring.md                       # Danh sách các thông số chờ user cung cấp
└── LIBRARY.MD                              # Danh mục thư viện và SDK được phép sử dụng
```
