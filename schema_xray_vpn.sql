-- ================================================================================
-- CƠ SỞ DỮ LIỆU XRAY VLESS-REALITY & QUẢN LÝ NGƯỜI DÙNG (SCHEMA & SEED DATA)
-- Hệ thống: NextAI VPN Platform & Gateway Mesh (Chuẩn V2.1)
-- ================================================================================

-- 1. BẢNG MÁY CHỦ XRAY VLESS-REALITY & IPSEC (servers)
CREATE TABLE IF NOT EXISTS servers (
    id INTEGER PRIMARY KEY,
    country_code VARCHAR(10) NOT NULL,
    host VARCHAR(255) NOT NULL,
    ip VARCHAR(64) NOT NULL,
    xsni VARCHAR(255) NOT NULL,
    source_ip_lrc INT DEFAULT 0,
    availability_xray INT DEFAULT 100,
    availability_ipsec INT DEFAULT 100,
    vless_port INT NOT NULL,
    reality_sid VARCHAR(64) NOT NULL,
    reality_pbkey VARCHAR(255) NOT NULL,
    status VARCHAR(32) DEFAULT 'ONLINE',
    ping_ms INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. BẢNG NGƯỜI DÙNG & TÀI KHOẢN XRAY (users)
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    account_status VARCHAR(32) NOT NULL,
    is_premium BOOLEAN NOT NULL DEFAULT 0,
    xray_uuid VARCHAR(64) NOT NULL,
    access_token TEXT NOT NULL,
    subscription_plan VARCHAR(64) DEFAULT 'none',
    paid_until TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. NẠP DỮ LIỆU MÁY CHỦ XRAY REALITY (SEED DATA)
INSERT INTO servers (id, country_code, host, ip, xsni, source_ip_lrc, availability_xray, availability_ipsec, vless_port, reality_sid, reality_pbkey, status, ping_ms) VALUES
(1, 'bg', 'bg.bsdup.com', '195.123.225.110', 'www.microsoft.com', 55, 100, 100, 63821, 'bbb43890f4d92fd5', 'koKaMgyyxUFwSa28okVxBsicQqXgkSi_sa-KblM2MUM', 'ONLINE', 231),
(2, 'bg', 'bg.bsdup.com', '195.123.225.109', 'www.microsoft.com', 52, 100, 100, 63821, 'bbb43890f4d92fd5', 'koKaMgyyxUFwSa28okVxBsicQqXgkSi_sa-KblM2MUM', 'ONLINE', 235),
(3, 'de', 'de-d.bsdup.com', '77.90.188.26', 'www.tiktok.com', 180, 100, 100, 63821, 'bbb43890f4d92fd5', 'koKaMgyyxUFwSa28okVxBsicQqXgkSi_sa-KblM2MUM', 'ONLINE', 209);

-- 4. NẠP DỮ LIỆU TÀI KHOẢN & XRAY UUID (SEED DATA)
INSERT INTO users (id, email, password_hash, account_status, is_premium, xray_uuid, access_token, subscription_plan, paid_until, created_at) VALUES
(1001, 'user.premium@example.com', '$2a$12$K8M...sampleHash1', 'active', 1, '4a2b9f10-7e3c-4b51-9e20-1a2b3c4d5e6f', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOjEwMDEsImlzcyI6ImJyb3dzZWMifQ.sample_sig_1', 'annual', '2027-10-15 00:00:00', '2024-01-10 12:00:00'),
(1002, 'business.vpn@corp.net', '$2a$12$L9N...sampleHash2', 'active', 1, '8c3d1e20-9f4a-4a62-8f31-2b3c4d5e6f7a', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOjEwMDIsImlzcyI6ImJyb3dzZWMifQ.sample_sig_2', 'biennial', '2028-05-20 00:00:00', '2024-03-01 09:30:00'),
(1003, 'free.trial@gmail.com', '$2a$12$M0O...sampleHash3', 'active', 0, '1f2e3d4c-5b6a-7890-abcd-ef1234567890', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOjEwMDMsImlzcyI6ImJyb3dzZWMifQ.sample_sig_3', 'none', NULL, '2026-02-14 15:20:00'),
(1004, 'banned.abuser@tempmail.org', '$2a$12$N1P...sampleHash4', 'banned', 0, '99999999-8888-7777-6666-555544443333', 'revoked_token_banned', 'none', NULL, '2025-11-05 18:40:00');
