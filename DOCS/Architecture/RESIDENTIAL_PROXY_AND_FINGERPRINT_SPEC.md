# TÀI LIỆU KỸ THUẬT: TÍCH HỢP RESIDENTIAL PROXY & CHE GIẤU DẤU VÂN TAY (TLS/JA3/JA4)
================================================================================
Dự án: NextAI VPN Platform & Residential Gateway Mesh (Chuẩn V2.1)
Mã tài liệu: ARCH-SPEC-RESIDENTIAL-FINGERPRINT-01
Áp dụng: Toàn bộ hệ thống Gateway Cổng 10000, CMS Cổng 6033, Desktop Client & Antidetect Integration
================================================================================

## 1. TỔNG QUAN VỀ RESIDENTIAL PROXY
Residential Proxy là loại proxy sử dụng địa chỉ IP thực được các Nhà Cung Cấp Dịch Vụ Internet (ISP) cấp phát cho hộ gia đình và văn phòng người dùng thực tế.
- **Khác biệt cốt lõi với Datacenter Proxy**: Lưu lượng từ Datacenter Proxy có ASN thuộc các trung tâm dữ liệu (AWS, Google Cloud, DigitalOcean, OVH...). Hệ thống WAF/Anti-Bot (Cloudflare, Akamai, DataDome, Imperva) dễ dàng gắn cờ hoặc chặn ngay lập tức.
- **Cơ chế hoạt động**:
  1. Client gửi request tới NextAi Universal Gateway (Cổng `10000`).
  2. Gateway/Smart Rotation Engine chọn một IP dân cư sạch từ Pool (theo quốc gia, thành phố, ISP, sticky session).
  3. Yêu cầu được chuyển tiếp ra ngoài qua IP dân cư đó.
  4. Server đích nhìn thấy request từ ISP dân cư (Viettel, VNPT, Comcast, AT&T...) -> Xem như người dùng thật.

---

## 2. NGUYÊN TẮC BẢO TOÀN DẤU VÂN TAY (TLS/JA3/JA4 FINGERPRINT)

### 2.1. Tại sao Residential IP vẫn bị chặn nếu không che giấu vân tay?
Trước khi một byte dữ liệu HTTP/HTTPS được truyền đi, quá trình **TLS Handshake** diễn ra:
- Thứ tự Cipher Suites, TLS Extensions, Supported Elliptic Curves, ALPN protocol.
- Các thư viện thông thường (`requests`, `httpx`, `urllib`) tạo ra TLS Fingerprint đặc trưng của Python/cURL bị WAF nhận diện ngay lập tức.
- **Quy tắc Vàng (Golden Rule)**: Dấu vân tay TLS (JA3/JA4) và IP ASN **bắt buộc phải đồng nhất**:
  - IP Residential + Fingerprint Python = **BỊ CHẶN (BOT DETECTED)**.
  - IP Datacenter + Fingerprint Chrome = **BỊ CAPTCHA / FLAG**.
  - IP Residential + Fingerprint Chrome thật (`curl_cffi` / Antidetect Browser) = **HỢP LỆ 100% (HUMAN PASSED)**.

### 2.2. Vai trò của NextAi Universal Gateway (Cổng 10000)
- **Transparent TCP Byte-Stream Relaying**: NextAi Gateway không giải mã SSL/TLS (No TLS Termination). Toàn bộ gói tin `ClientHello` từ client (sử dụng `curl_cffi` hoặc KikiLogin Antidetect Browser) được chuyển tiếp nguyên vẹn ra Internet, giữ nguyên 100% JA3/JA4 fingerprint.
- **Hỗ trợ `socks5h` Chống Rò Rỉ DNS (DNS Leak Proof)**:
  - Khi sử dụng giao thức `socks5h://` hoặc SOCKS5 với `ATYP = 0x03` (Domain Name), tên miền được phân giải từ xa tại Gateway/Upstream Proxy, triệt tiêu hoàn toàn nguy cơ rò rỉ DNS cục bộ.

---

## 3. CHIẾN LƯỢC QUẢN LÝ PROXY ROTATION TẠI NEXTAI GATEWAY

### 3.1. Sticky Session (IP Cố Định Duy Trì Phiên)
- **Mục đích**: Duy trì phiên đăng nhập, cookie tài khoản mạng xã hội, sàn TMĐT.
- **Cú pháp định danh qua Cổng 10000**:
  ```
  username-country-{cc}-session-{id}-time-{minutes}:{password}
  ```
  *Ví dụ*: `kikilogin-country-vn-session-profile01-time-30:kiki_resident_pass_2026`
  -> NextAi Smart Rotation Engine giữ nguyên node proxy dân cư trong 30 phút.

### 3.2. Rotating Session (Xoay Vòng IP Tự Động)
- **Mục đích**: Thu thập dữ liệu web, cào dữ liệu quy mô lớn, kiểm thử độ chịu tải.
- **Cú pháp định danh qua Cổng 10000**:
  ```
  username-country-{cc}:{password}
  ```
  *Ví dụ*: `kikilogin-country-us:kiki_resident_pass_2026`
  -> Mỗi kết nối mới được phân phối vào Top-3 node có Ping thấp nhất và tải nhẹ nhất.

---

## 4. MẪU TÍCH HỢP PYTHON CHUẨN VỚI NEXTAI GATEWAY (CỔNG 10000)

### 4.1. Tích hợp `curl_cffi` (Giả lập Chrome 110 + SOCKS5h)
```python
"""
Mẫu kết nối Python với NextAi Residential Gateway (Cổng 10000)
Sử dụng curl_cffi để giả lập dấu vân tay Chrome 110 & Chống DNS Leak
"""
from curl_cffi import requests

# Cấu hình proxy qua NextAi Gateway với Sticky Session 30 phút tại Việt Nam
GATEWAY_PROXY = "socks5h://kikilogin-country-vn-session-worker01-time-30:kiki_resident_pass_2026@127.0.0.1:10000"

proxies = {
    "http": GATEWAY_PROXY,
    "https": GATEWAY_PROXY
}

# Gửi yêu cầu với dấu vân tay Chrome 110 thật
response = requests.get(
    "https://tls.browserleaks.com/json",
    impersonate="chrome110",
    proxies=proxies,
    timeout=30
)

data = response.json()
print("=== KẾT QUẢ KIỂM TRA BROWSERLEAKS ===")
print(f"IP Thoát: {data.get('ip_address')}")
print(f"JA3 Hash: {data.get('ja3_hash')}")
print(f"JA3 Text: {data.get('ja3_text')[:60]}...")
```

### 4.2. Tích hợp Bất Đồng Bộ (Async Session) & Xoay Proxy Tự Động
```python
import asyncio
from curl_cffi.requests import AsyncSession

async def fetch_target(session_id: str, url: str):
    # Mỗi worker mang một session ID riêng biệt hoặc xoay ngẫu nhiên
    proxy_url = f"socks5h://kikilogin-country-us-session-{session_id}-time-15:kiki_resident_pass_2026@127.0.0.1:10000"
    
    async with AsyncSession() as session:
        resp = await session.get(
            url,
            impersonate="chrome110",
            proxies={"https": proxy_url, "http": proxy_url},
            timeout=20
        )
        return resp.status_code, len(resp.text)

async def main():
    tasks = [
        fetch_target(f"task_{i}", "https://api.ipify.org")
        for i in range(5)
    ]
    results = await asyncio.gather(*tasks)
    print("Hoàn thành các tác vụ:", results)

if __name__ == "__main__":
    asyncio.run(main())
```

---

## 5. BẢNG KIỂM TRA & XÁC MINH AN TOÀN (VERIFICATION CHECKLIST)
1. **Kiểm tra rò rỉ DNS (DNS Leak)**:
   - Truy cập: `https://ipleak.net`
   - Tiêu chuẩn: Danh sách DNS Server chỉ xuất hiện IP tại quốc gia của proxy, không được xuất hiện IP DNS của nhà mạng máy thật.
2. **Kiểm tra Dấu vân tay TLS (JA3 / JA4)**:
   - Truy cập: `https://tls.browserleaks.com/json`
   - Tiêu chuẩn: Khớp 100% với fingerprint của Chrome/Safari/Edge thật, không có dấu hiệu của cURL/Python.
3. **Kiểm tra ASN của Exit IP**:
   - Truy cập: `https://ipinfo.io` hoặc `https://check.jibaoproxy.com`
   - Tiêu chuẩn: Kiểu kết nối (`type`) phải là `isp` hoặc `residential`, không được là `hosting` / `datacenter`.
4. **Kiểm tra WebRTC Leak**:
   - Vô hiệu hóa hoặc mock WebRTC STUN request trong trình duyệt để IP LAN/Local không bị lộ.
