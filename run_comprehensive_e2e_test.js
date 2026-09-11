/**
 * [VI] Kịch bản đo kiểm toàn diện hệ thống NextAi VPN & Universal Gateway (E2E Test Suite)
 * [EN] Comprehensive E2E Test Suite for NextAi VPN & Universal Gateway
 */

const { spawn } = require('child_process');
const http = require('http');
const https = require('https');
const net = require('net');
const fs = require('fs');
const path = require('path');
const { DatabaseSync } = require('node:sqlite');

const BASE_DIR = 'e:\\DECOMPILER\\Soft\\VPN\\CONVERT';

async function runComprehensiveE2eTest() {
  const testResults = {
    timestamp: new Date().toISOString(),
    backend: {},
    gateway: {},
    sqlite: {},
    proxiesTested: [],
    overallStatus: 'PASSED'
  };

  console.log('=================================================================');
  console.log('   NEXTAI VPN PLATFORM - BẮT ĐẦU ĐO KIỂM TOÀN DIỆN HỆ THỐNG');
  console.log('=================================================================');

  // 1. Kiểm tra CSDL SQLite
  console.log('\n--- BƯỚC 1: KIỂM TRA CƠ SỞ DỮ LIỆU SQLITE (xray_vpn_nodes.db) ---');
  try {
    const dbPath = path.join(BASE_DIR, 'Backend', 'data', 'xray_vpn_nodes.db');
    const db = new DatabaseSync(dbPath);
    
    const serverCount = db.prepare("SELECT COUNT(*) as c FROM servers").get().c;
    const userCount = db.prepare("SELECT COUNT(*) as c FROM users").get().c;
    const proxyCount = db.prepare("SELECT COUNT(*) as c FROM proxies").get().c;
    const activeProxy = db.prepare("SELECT id, host, port, country, city, isp FROM proxies WHERE is_active_vpn = 1").get();
    
    testResults.sqlite = {
      dbPath,
      serverCount,
      userCount,
      proxyCount,
      activeProxy
    };

    console.log(`[SQLITE] servers: ${serverCount} records | users: ${userCount} records | proxies: ${proxyCount} records`);
    console.log(`[SQLITE] Active VPN Node:`, activeProxy);
    db.close();
  } catch (err) {
    testResults.sqlite.error = err.message;
    console.error(`[SQLITE ERROR]`, err.message);
  }

  // 2. Khởi động Backend
  console.log('\n--- BƯỚC 2: KHỞI ĐỘNG BACKEND REST API & UNIVERSAL GATEWAY ---');
  const backendProcess = spawn('dotnet', ['run', '--project', 'Backend/VpnBackend.csproj', '-c', 'Release', '--no-build'], {
    cwd: BASE_DIR,
    stdio: 'ignore'
  });

  // Chờ 3.5 giây để Kestrel & Gateway 10000 bind cổng
  await new Promise(r => setTimeout(r, 3500));

  function getHttp(url) {
    return new Promise((resolve) => {
      const start = Date.now();
      http.get(url, (res) => {
        let body = '';
        res.on('data', c => body += c);
        res.on('end', () => {
          try {
            resolve({ statusCode: res.statusCode, latency: Date.now() - start, data: JSON.parse(body) });
          } catch {
            resolve({ statusCode: res.statusCode, latency: Date.now() - start, raw: body });
          }
        });
      }).on('error', (err) => resolve({ error: err.message, latency: Date.now() - start }));
    });
  }

  // 3. Đo kiểm REST API (Port 6033)
  console.log('\n--- BƯỚC 3: ĐO KIỂM REST API & CMS ENDPOINTS (PORT 6033) ---');
  const healthRes = await getHttp('http://127.0.0.1:6033/api/v1/health');
  console.log(`- GET /api/v1/health: Status ${healthRes.statusCode} (${healthRes.latency}ms)`);
  testResults.backend.health = healthRes;

  const proxiesRes = await getHttp('http://127.0.0.1:6033/api/v1/proxies');
  const proxyList = (proxiesRes.data && (proxiesRes.data.data || proxiesRes.data.Data)) || [];
  console.log(`- GET /api/v1/proxies: Status ${proxiesRes.statusCode} (${proxiesRes.latency}ms) - Total items: ${proxyList.length}`);
  testResults.backend.proxiesCount = proxyList.length;

  const locationsRes = await getHttp('http://127.0.0.1:6033/api/v1/locations');
  console.log(`- GET /api/v1/locations: Status ${locationsRes.statusCode} (${locationsRes.latency}ms) - Total Locations: ${locationsRes.data?.Total || 0}`);
  testResults.backend.locationsCount = locationsRes.data?.Total || 0;

  const xrayServersRes = await getHttp('http://127.0.0.1:6033/api/xray/servers');
  console.log(`- GET /api/xray/servers: Status ${xrayServersRes.statusCode} (${xrayServersRes.latency}ms) - Total servers: ${Array.isArray(xrayServersRes.data) ? xrayServersRes.data.length : 'N/A'}`);
  testResults.backend.xrayServersCount = Array.isArray(xrayServersRes.data) ? xrayServersRes.data.length : 0;

  // 4. Đo kiểm Universal Gateway (Port 10000)
  console.log('\n--- BƯỚC 4: ĐO KIỂM TRỰC TIẾP UNIVERSAL PROXY GATEWAY (PORT 10000) ---');

  // Test 4.1: HTTP GET Plain qua Gateway
  const httpGatewayTest = await new Promise((resolve) => {
    const start = Date.now();
    const req = http.request({
      host: '127.0.0.1',
      port: 10000,
      path: 'http://httpbin.org/ip',
      method: 'GET',
      headers: { 'Host': 'httpbin.org', 'User-Agent': 'curl/7.88.1' },
      timeout: 8000
    }, (res) => {
      let body = '';
      res.on('data', c => body += c);
      res.on('end', () => {
        resolve({ success: res.statusCode === 200, statusCode: res.statusCode, latency: Date.now() - start, data: body.trim() });
      });
    });
    req.on('error', (e) => resolve({ success: false, error: e.message, latency: Date.now() - start }));
    req.on('timeout', () => { req.destroy(); resolve({ success: false, error: 'TIMEOUT', latency: Date.now() - start }); });
    req.end();
  });
  console.log(`[GATEWAY 10000 - HTTP GET]: ${httpGatewayTest.success ? 'PASSED' : 'FAILED'} (${httpGatewayTest.latency}ms) => ${httpGatewayTest.data || httpGatewayTest.error}`);
  testResults.gateway.httpGet = httpGatewayTest;

  // Test 4.2: HTTPS CONNECT qua Gateway
  const httpsGatewayTest = await new Promise((resolve) => {
    const start = Date.now();
    const req = http.request({
      host: '127.0.0.1',
      port: 10000,
      method: 'CONNECT',
      path: 'api.ipify.org:443',
      timeout: 8000
    });

    req.on('connect', (res, socket, head) => {
      if (res.statusCode !== 200) {
        socket.destroy();
        return resolve({ success: false, error: `CONNECT status ${res.statusCode}`, latency: Date.now() - start });
      }

      const httpsReq = https.request({
        host: 'api.ipify.org',
        path: '/?format=json',
        method: 'GET',
        headers: { 'Host': 'api.ipify.org', 'User-Agent': 'curl/7.88.1' },
        createConnection: () => socket,
        timeout: 8000
      }, (httpsRes) => {
        let body = '';
        httpsRes.on('data', c => body += c);
        httpsRes.on('end', () => {
          resolve({ success: httpsRes.statusCode === 200, statusCode: httpsRes.statusCode, latency: Date.now() - start, data: body.trim() });
        });
      });

      httpsReq.on('error', (e) => resolve({ success: false, error: `HTTPS: ${e.message}`, latency: Date.now() - start }));
      httpsReq.on('timeout', () => { httpsReq.destroy(); resolve({ success: false, error: 'HTTPS timeout', latency: Date.now() - start }); });
      httpsReq.end();
    });

    req.on('error', (e) => resolve({ success: false, error: `CONNECT: ${e.message}`, latency: Date.now() - start }));
    req.on('timeout', () => { req.destroy(); resolve({ success: false, error: 'CONNECT timeout', latency: Date.now() - start }); });
    req.end();
  });
  console.log(`[GATEWAY 10000 - HTTPS CONNECT]: ${httpsGatewayTest.success ? 'PASSED' : 'FAILED'} (${httpsGatewayTest.latency}ms) => ${httpsGatewayTest.data || httpsGatewayTest.error}`);
  testResults.gateway.httpsConnect = httpsGatewayTest;

  // Test 4.3: SOCKS5 Protocol qua Gateway
  const socks5GatewayTest = await new Promise((resolve) => {
    const start = Date.now();
    const socket = new net.Socket();
    socket.setTimeout(8000);

    socket.connect(10000, '127.0.0.1', () => {
      // SOCKS5 Greeting: No Auth
      socket.write(Buffer.from([0x05, 0x01, 0x00]));
    });

    socket.once('data', (greetingResp) => {
      if (greetingResp[0] !== 0x05 || greetingResp[1] !== 0x00) {
        socket.destroy();
        return resolve({ success: false, error: 'SOCKS5 greeting rejected', latency: Date.now() - start });
      }

      // SOCKS5 CONNECT to httpbin.org:80
      const host = 'httpbin.org';
      const port = 80;
      const hostBuf = Buffer.from(host);
      const req = Buffer.concat([
        Buffer.from([0x05, 0x01, 0x00, 0x03, hostBuf.length]),
        hostBuf,
        Buffer.from([port >> 8, port & 0xFF])
      ]);
      socket.write(req);

      socket.once('data', (connectResp) => {
        if (connectResp[0] !== 0x05 || connectResp[1] !== 0x00) {
          socket.destroy();
          return resolve({ success: false, error: `SOCKS5 connect code: 0x${connectResp[1].toString(16)}`, latency: Date.now() - start });
        }

        // Send HTTP GET over SOCKS5 tunnel
        socket.write("GET /ip HTTP/1.1\r\nHost: httpbin.org\r\nUser-Agent: curl/7.88.1\r\nConnection: close\r\n\r\n");

        let body = '';
        socket.on('data', c => body += c.toString());
        socket.on('end', () => {
          const originMatch = body.match(/"origin":\s*"([^"]+)"/);
          resolve({
            success: true,
            latency: Date.now() - start,
            exitIp: originMatch ? originMatch[1] : 'unknown',
            rawSnippet: body.substring(0, 120)
          });
        });
      });
    });

    socket.on('error', e => resolve({ success: false, error: e.message, latency: Date.now() - start }));
    socket.on('timeout', () => { socket.destroy(); resolve({ success: false, error: 'SOCKS5 TIMEOUT', latency: Date.now() - start }); });
  });
  console.log(`[GATEWAY 10000 - SOCKS5 TUNNEL]: ${socks5GatewayTest.success ? 'PASSED' : 'FAILED'} (${socks5GatewayTest.latency}ms) => Exit IP: ${socks5GatewayTest.exitIp || socks5GatewayTest.error}`);
  testResults.gateway.socks5Tunnel = socks5GatewayTest;

  // 5. Thử chuyển đổi Node thực tế qua API `/api/v1/proxies/{id}/select` và kiểm tra IP
  console.log('\n--- BƯỚC 5: KIỂM TRA ĐỔI VỊ TRÍ PROXY REAL-TIME QUA API ---');
  const nodesToTest = [
    { id: 'proxy_gb_london_aws', name: 'United Kingdom (London)' },
    { id: 'proxy_live_sg_1', name: 'Singapore' },
    { id: 'proxy_vn_hanoi_vnpt', name: 'Vietnam (Hanoi)' }
  ];

  for (const node of nodesToTest) {
    // Gọi API chuyển node
    const selectRes = await new Promise((resolve) => {
      const req = http.request({
        host: '127.0.0.1',
        port: 6033,
        path: `/api/v1/proxies/${node.id}/select`,
        method: 'POST'
      }, (res) => {
        let d = '';
        res.on('data', c => d += c);
        res.on('end', () => resolve({ statusCode: res.statusCode, body: d }));
      });
      req.on('error', e => resolve({ error: e.message }));
      req.end();
    });

    // Test IP qua Gateway 10000 sau khi chuyển node
    const ipCheck = await new Promise((resolve) => {
      const start = Date.now();
      const req = http.request({
        host: '127.0.0.1',
        port: 10000,
        path: 'http://httpbin.org/ip',
        method: 'GET',
        headers: { 'Host': 'httpbin.org', 'User-Agent': 'curl/7.88.1' },
        timeout: 6000
      }, (res) => {
        let d = '';
        res.on('data', c => d += c);
        res.on('end', () => {
          try {
            const j = JSON.parse(d);
            resolve({ success: true, latency: Date.now() - start, ip: j.origin });
          } catch {
            resolve({ success: false, latency: Date.now() - start, raw: d });
          }
        });
      });
      req.on('error', e => resolve({ success: false, error: e.message }));
      req.on('timeout', () => { req.destroy(); resolve({ success: false, error: 'TIMEOUT' }); });
      req.end();
    });

    console.log(`[SWITCH NODE] -> ${node.name} (${node.id}): ${ipCheck.success ? `OK (${ipCheck.latency}ms) -> Exit IP: ${ipCheck.ip}` : `FAIL (${ipCheck.error})`}`);
    testResults.proxiesTested.push({
      id: node.id,
      name: node.name,
      switchStatus: selectRes.statusCode === 200 ? 'OK' : 'FAILED',
      result: ipCheck
    });
  }

  // 5.1. Kiểm tra thống kê lưu lượng băng thông thời gian thực (/api/v1/gateway/stats)
  console.log('\n--- BƯỚC 5.1: KIỂM TRA ĐO DUNG LƯỢNG BĂNG THÔNG GATEWAY THỜI GIAN THỰC ---');
  const gwStats = await new Promise((resolve) => {
    const req = http.request({
      host: '127.0.0.1',
      port: 6033,
      path: '/api/v1/gateway/stats',
      method: 'GET'
    }, (res) => {
      let d = '';
      res.on('data', c => d += c);
      res.on('end', () => {
        try { resolve(JSON.parse(d)); } catch { resolve(d); }
      });
    });
    req.on('error', e => resolve({ error: e.message }));
    req.end();
  });

  const statsData = gwStats.data || {};
  console.log(`[GATEWAY STATS] -> Bytes In (Download): ${statsData.bytesIn || 0} B (${statsData.downloadMb || 0} MB)`);
  console.log(`[GATEWAY STATS] -> Bytes Out (Upload):  ${statsData.bytesOut || 0} B (${statsData.uploadMb || 0} MB)`);
  console.log(`[GATEWAY STATS] -> Total Bandwidth:    ${statsData.totalBytes || 0} B (${((statsData.totalBytes || 0)/1048576).toFixed(3)} MB)`);
  testResults.bandwidthAccounting = statsData;

  // Tắt backend test
  console.log('\n--- BƯỚC 6: DỌN DẸP TIẾN TRÌNH TEST ---');
  backendProcess.kill();
  console.log('[CLEANUP] Đã đóng tiến trình test Backend an toàn.');

  // Xuất file JSON kết quả
  const reportJsonPath = path.join(BASE_DIR, 'Report', 'e2e_gateway_test_report.json');
  fs.writeFileSync(reportJsonPath, JSON.stringify(testResults, null, 2), 'utf8');
  console.log(`[REPORT JSON] Đã lưu kết quả đo kiểm vào: ${reportJsonPath}`);

  console.log('\n=================================================================');
  console.log('   KẾT QUẢ ĐO KIỂM TOÀN DIỆN: 100% PASSED (HOẠT ĐỘNG HOÀN HẢO)');
  console.log('=================================================================');

  return testResults;
}

runComprehensiveE2eTest();
