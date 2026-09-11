# TỔNG QUAN KIẾN TRÚC HỆ THỐNG (DOCS/ARCHITECTURE/OVERVIEW.MD)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
================================================================================

## 1. MÔ HÌNH KIẾN TRÚC TỔNG THỂ (HIGH-LEVEL ARCHITECTURE)

```
[ Client Applications ]
  ├── apps/desktop (WPF .NET 10 Fluent UI, 113 Screens)
  ├── apps/android (Native Kotlin + Jetpack Compose)
  └── Antidetect Browsers / External Tools (KikiLogin, AdsPower, GoLogin)
           │
           │ SOCKS5 / HTTP CONNECT (Port 10000 / 10001-10500)
           ▼
[ NextAI Universal Gateway Engine (Backend/) ]
  ├── Authentication & Token Verification (username:password)
  ├── Session Sticky Router (Session ID, TTL Timeout)
  ├── Country / City Geo-Targeting Filter
  └── Quota & Traffic Metering Engine
           │
           │ Dynamic Internal Routing
           ▼
[ Residential Node Pool & WireGuard Relays ]
  ├── US Nodes (Comcast, AT&T, Verizon Residential)
  ├── VN Nodes (VNPT, Viettel, FPT Telecom)
  ├── EU Nodes (Deutsche Telekom, Orange, Vodafone)
  └── High-Speed Datacenter WireGuard Relays
```

## 2. NGUYÊN TẮC THIẾT KẾ (DESIGN PRINCIPLES)
1. **Hoàn toàn Phi Tập Trung & Độc Lập**: Backend VPN Hub không phụ thuộc vào bất kỳ phần mềm antidetect nào. Nó cung cấp giao diện chuẩn IETF SOCKS5 (RFC 1928) và HTTP CONNECT proxy.
2. **Hiệu năng Cao Cực Hạn**: Sử dụng socket bất đồng bộ non-blocking của .NET 10, hỗ trợ hàng chục nghìn luồng truyền dữ liệu đồng thời với độ trễ thấp.
3. **Bảo Mật Cấp Hệ Thống (WFP Kill Switch)**: Trên Desktop, Windows Filtering Platform (WFP) can thiệp ở tầng nhân (Kernel Level) để chặn hoàn toàn mọi rò rỉ DNS, WebRTC, IPv6 khi kết nối VPN bị gián đoạn.
