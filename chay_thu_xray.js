/**
 * [VI] Script chạy thử nghiệm kiểm tra CSDL SQLite và kết nối Xray Reality Servers
 * [EN] Test runner script to query SQLite DB and test Xray Reality Servers
 *
 * Chạy lệnh: node chay_thu_xray.js
 */

const { DatabaseSync } = require('node:sqlite');
const path = require('node:path');
const net = require('node:net');
const tls = require('node:tls');

const dbPath = path.join(__dirname, 'xray_vpn_nodes.db');
console.log('======================================================================');
console.log('   KIỂM TRA CƠ SỞ DỮ LIỆU & CHẠY THỬ XRAY VLESS-REALITY SERVERS');
console.log('======================================================================\n');

if (!require('node:fs').existsSync(dbPath)) {
  console.error(`[LỖI] Không tìm thấy file cơ sở dữ liệu: ${dbPath}`);
  process.exit(1);
}

const db = new DatabaseSync(dbPath);

// 1. Đọc bảng servers
const servers = db.prepare("SELECT * FROM servers").all();
console.log(`📦 BẢNG 'servers' (${servers.length} máy chủ):`);
console.table(servers.map(s => ({
  ID: s.id,
  QuốcGia: s.country_code.toUpperCase(),
  Host: s.host,
  IP: s.ip,
  Port: s.vless_port,
  SNI: s.xsni,
  TrạngThái: s.status,
  Ping: `${s.ping_ms} ms`
})));

// 2. Đọc bảng users
const users = db.prepare("SELECT * FROM users").all();
console.log(`\n👥 BẢNG 'users' (${users.length} tài khoản):`);
console.table(users.map(u => ({
  ID: u.id,
  Email: u.email,
  TrạngThái: u.account_status,
  VIP: u.is_premium ? 'Yes (VIP)' : 'Free',
  XrayUUID: u.xray_uuid,
  Gói: u.subscription_plan,
  HạnDùng: u.paid_until || 'N/A'
})));

// 3. Sinh link VLESS Share Link cho từng User và Server
console.log('\n======================================================================');
console.log('🔗 DANH SÁCH LIÊN KẾT VLESS REALITY (DÙNG ĐỂ IMPORT VÀO CLIENT/KIKILOGIN)');
console.log('======================================================================');

const activeUser = users.find(u => u.account_status === 'active' && u.is_premium) || users[0];

for (const s of servers) {
  // Chuẩn định dạng VLESS Reality:
  // vless://UUID@HOST:PORT?security=reality&encryption=none&pbk=PUBLIC_KEY&headerType=none&fp=chrome&type=tcp&flow=xtls-rprx-vision&sni=SNI&sid=SID#LABEL
  const address = s.id === 3 ? s.ip : s.host; // Server 3 dùng IP vì DNS de-d.bsdup.com chưa trỏ
  const label = encodeURIComponent(`NextAI-${s.country_code.toUpperCase()}-${s.host}`);
  const vlessUrl = `vless://${activeUser.xray_uuid}@${address}:${s.vless_port}?security=reality&encryption=none&pbk=${s.reality_pbkey}&headerType=none&fp=chrome&type=tcp&flow=xtls-rprx-vision&sni=${s.xsni}&sid=${s.reality_sid}#${label}`;
  
  console.log(`\n👉 Server #${s.id} [${s.country_code.toUpperCase()}] (${address}:${s.vless_port})`);
  console.log(`   SNI: ${s.xsni} | SID: ${s.reality_sid}`);
  console.log(`   VLESS URL:`);
  console.log(`   ${vlessUrl}`);
}

// 4. Chạy kiểm tra kết nối mạng thực tế (TCP & TLS Handshake)
console.log('\n======================================================================');
console.log('⚡ ĐANG CHẠY THỬ KIỂM TRA MẠNG THỜI GIAN THỰC...');
console.log('======================================================================');

async function testServer(s) {
  return new Promise((resolve) => {
    const start = Date.now();
    const socket = new net.Socket();
    socket.setTimeout(4000);

    socket.connect(s.vless_port, s.ip, () => {
      const tcpTime = Date.now() - start;
      
      // Tiếp tục kiểm tra TLS Handshake với SNI
      const tlsSocket = tls.connect({
        socket: socket,
        servername: s.xsni,
        rejectUnauthorized: false, // Reality dùng self-reflected cert
        timeout: 4000
      }, () => {
        const cert = tlsSocket.getPeerCertificate();
        const tlsTime = Date.now() - start;
        tlsSocket.end();
        resolve({
          id: s.id,
          ip: s.ip,
          port: s.vless_port,
          tcpOk: true,
          tcpMs: tcpTime,
          tlsOk: true,
          tlsMs: tlsTime,
          certSubject: cert.subject ? cert.subject.CN : 'OK',
          certIssuer: cert.issuer ? cert.issuer.O || cert.issuer.CN : 'OK'
        });
      });

      tlsSocket.on('error', (err) => {
        resolve({
          id: s.id,
          ip: s.ip,
          port: s.vless_port,
          tcpOk: true,
          tcpMs: tcpTime,
          tlsOk: false,
          error: `TLS Error: ${err.message}`
        });
      });
    });

    socket.on('error', (err) => {
      resolve({
        id: s.id,
        ip: s.ip,
        port: s.vless_port,
        tcpOk: false,
        error: `TCP Connect Failed: ${err.message}`
      });
    });

    socket.on('timeout', () => {
      socket.destroy();
      resolve({
        id: s.id,
        ip: s.ip,
        port: s.vless_port,
        tcpOk: false,
        error: 'TCP Connect Timeout'
      });
    });
  });
}

(async () => {
  for (const s of servers) {
    const res = await testServer(s);
    if (res.tcpOk && res.tlsOk) {
      console.log(`[PASS] Server #${res.id} (${res.ip}:${res.port}) -> TCP: ${res.tcpMs}ms | TLS Reality: ${res.tlsMs}ms | Cert: ${res.certSubject} (${res.certIssuer})`);
    } else if (res.tcpOk) {
      console.log(`[WARN] Server #${res.id} (${res.ip}:${res.port}) -> TCP: ${res.tcpMs}ms | ${res.error}`);
    } else {
      console.log(`[FAIL] Server #${res.id} (${res.ip}:${res.port}) -> ${res.error}`);
    }
  }

  console.log('\n======================================================================');
  console.log('✅ HOÀN TẤT CHẠY THỬ! BẠN CÓ THỂ COPY VLESS URL Ở TRÊN ĐỂ SỬ DỤNG.');
  console.log('======================================================================\n');
  db.close();
})();
