/**
 * [VI] Script thêm danh sách proxy kiểm định vào CSDL SQLite và proxies.json
 * [EN] Script to add verified proxy list to SQLite Database and proxies.json
 */

const { DatabaseSync } = require('node:sqlite');
const fs = require('node:fs');
const path = require('node:path');

const baseDir = 'e:\\DECOMPILER\\Soft\\VPN\\CONVERT';

const dbPaths = [
  path.join(baseDir, 'xray_vpn_nodes.db'),
  path.join(baseDir, 'Backend', 'data', 'xray_vpn_nodes.db'),
  path.join(baseDir, 'Backend', 'bin', 'Release', 'net10.0', 'data', 'xray_vpn_nodes.db'),
  path.join(baseDir, 'Backend', 'bin', 'Debug', 'net10.0', 'data', 'xray_vpn_nodes.db')
];

const proxiesJsonPaths = [
  path.join(baseDir, 'Backend', 'data', 'proxies.json'),
  path.join(baseDir, 'Backend', 'bin', 'Release', 'net10.0', 'data', 'proxies.json'),
  path.join(baseDir, 'Backend', 'bin', 'Debug', 'net10.0', 'data', 'proxies.json')
];

const newProxies = [
  {
    Id: "proxy_vn_hanoi_vnpt",
    Type: "http",
    Host: "14.251.13.20",
    Port: 8080,
    Username: null,
    Password: null,
    Country: "VN",
    CountryName: "Vietnam",
    City: "Hanoi",
    Isp: "VNPT Residential",
    PingMs: 158,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_gb_london_aws",
    Type: "socks5",
    Host: "3.10.170.234",
    Port: 3128,
    Username: null,
    Password: null,
    Country: "GB",
    CountryName: "United Kingdom",
    City: "London",
    Isp: "Amazon.com, Inc.",
    PingMs: 456,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_in_gandhinagar_bsnl",
    Type: "http",
    Host: "117.236.124.166",
    Port: 3128,
    Username: null,
    Password: null,
    Country: "IN",
    CountryName: "India",
    City: "Gandhinagar",
    Isp: "BSNL Internet",
    PingMs: 460,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_cn_guangzhou_aliyun",
    Type: "http",
    Host: "8.138.217.152",
    Port: 21001,
    Username: null,
    Password: null,
    Country: "CN",
    CountryName: "China",
    City: "Guangzhou",
    Isp: "Hangzhou Alibaba Advertising Co., Ltd.",
    PingMs: 529,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_us_mountainview_gcp",
    Type: "http",
    Host: "34.43.46.91",
    Port: 443,
    Username: null,
    Password: null,
    Country: "US",
    CountryName: "United States",
    City: "Mountain View",
    Isp: "Google LLC",
    PingMs: 1362,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_vn_hcm_aceville",
    Type: "http",
    Host: "101.32.65.42",
    Port: 8888,
    Username: null,
    Password: null,
    Country: "VN",
    CountryName: "Vietnam",
    City: "Ho Chi Minh City",
    Isp: "Aceville Pte.ltd / Tencent",
    PingMs: 117,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_us_powhatan_backbone",
    Type: "http",
    Host: "64.112.184.210",
    Port: 3128,
    Username: null,
    Password: null,
    Country: "US",
    CountryName: "United States",
    City: "Powhatan",
    Isp: "Hosted Backbone",
    PingMs: 469,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_jp_tokyo_akari",
    Type: "http",
    Host: "45.146.163.31",
    Port: 80,
    Username: null,
    Password: null,
    Country: "JP",
    CountryName: "Japan",
    City: "Tokyo",
    Isp: "Akari Networks Limited",
    PingMs: 301,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  },
  {
    Id: "proxy_us_councilbluffs_gcp",
    Type: "http",
    Host: "34.44.49.215",
    Port: 80,
    Username: null,
    Password: null,
    Country: "US",
    CountryName: "United States",
    City: "Council Bluffs",
    Isp: "Google LLC",
    PingMs: 515,
    Status: "LIVE",
    LastChecked: new Date().toISOString(),
    IsActiveVpn: false,
    CreatedAt: new Date().toISOString()
  }
];

// 1. Cập nhật SQLite databases
for (const dbPath of dbPaths) {
  const dir = path.dirname(dbPath);
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });

  const db = new DatabaseSync(dbPath);

  // Tạo bảng proxies nếu chưa có
  db.exec(`
    CREATE TABLE IF NOT EXISTS proxies (
      id TEXT PRIMARY KEY,
      type TEXT NOT NULL DEFAULT 'http',
      host TEXT NOT NULL,
      port INTEGER NOT NULL,
      username TEXT,
      password TEXT,
      country TEXT NOT NULL,
      country_name TEXT NOT NULL,
      city TEXT NOT NULL,
      isp TEXT NOT NULL,
      ping_ms INTEGER,
      status TEXT DEFAULT 'LIVE',
      last_checked TEXT,
      is_active_vpn BOOLEAN DEFAULT 0,
      created_at TEXT DEFAULT CURRENT_TIMESTAMP
    );
  `);

  const upsert = db.prepare(`
    INSERT INTO proxies (id, type, host, port, username, password, country, country_name, city, isp, ping_ms, status, last_checked, is_active_vpn, created_at)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    ON CONFLICT(id) DO UPDATE SET
      type = excluded.type,
      host = excluded.host,
      port = excluded.port,
      username = excluded.username,
      password = excluded.password,
      country = excluded.country,
      country_name = excluded.country_name,
      city = excluded.city,
      isp = excluded.isp,
      ping_ms = excluded.ping_ms,
      status = excluded.status,
      last_checked = excluded.last_checked,
      is_active_vpn = excluded.is_active_vpn;
  `);

  for (const p of newProxies) {
    upsert.run(
      p.Id,
      p.Type,
      p.Host,
      p.Port,
      p.Username,
      p.Password,
      p.Country,
      p.CountryName,
      p.City,
      p.Isp,
      p.PingMs,
      p.Status,
      p.LastChecked,
      p.IsActiveVpn ? 1 : 0,
      p.CreatedAt
    );
  }

  const count = db.prepare("SELECT COUNT(*) as cnt FROM proxies").get();
  console.log(`[SQLITE] Updated ${dbPath} - Total proxies in table: ${count.cnt}`);
  db.close();
}

// 2. Cập nhật JSON files
for (const jsonPath of proxiesJsonPaths) {
  const dir = path.dirname(jsonPath);
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });

  let existing = [];
  if (fs.existsSync(jsonPath)) {
    try {
      existing = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));
    } catch (e) {
      existing = [];
    }
  }

  // Merge proxies by ID
  const map = new Map();
  for (const item of existing) {
    map.set(item.Id, item);
  }
  for (const item of newProxies) {
    map.set(item.Id, item);
  }

  const merged = Array.from(map.values());
  fs.writeFileSync(jsonPath, JSON.stringify(merged, null, 2), 'utf8');
  console.log(`[JSON] Updated ${jsonPath} - Total items: ${merged.length}`);
}

console.log('=== ĐÃ THÊM THÀNH CÔNG 9 PROXY VÀO CSDL SQLITE & PROXIES.JSON ===');
