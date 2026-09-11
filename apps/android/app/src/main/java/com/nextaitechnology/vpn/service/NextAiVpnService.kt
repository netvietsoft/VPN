package com.nextaitechnology.vpn.service

import android.content.Intent
import android.net.VpnService
import android.os.ParcelFileDescriptor
import android.util.Log

/**
 * [VI] Dịch vụ VPN Native của hệ điều hành Android điều khiển luồng Tun Interface.
 * [EN] Android Native VpnService managing Tun Interface and routing packets.
 */
class NextAiVpnService : VpnService() {

    private var vpnInterface: ParcelFileDescriptor? = null

    companion object {
        const val TAG = "NextAiVpnService"
        const val ACTION_CONNECT = "com.nextaitechnology.vpn.CONNECT"
        const val ACTION_DISCONNECT = "com.nextaitechnology.vpn.DISCONNECT"
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        when (intent?.action) {
            ACTION_CONNECT -> establishVpnTunnel()
            ACTION_DISCONNECT -> tearDownVpnTunnel()
        }
        return START_NOT_STICKY
    }

    /**
     * [VI] Khởi tạo giao diện mạng ảo (TUN Interface) và cấu hình tuyến đường định tuyến IP.
     * [EN] Establishes virtual network TUN interface and configures IP routing rules.
     */
    private fun establishVpnTunnel() {
        try {
            if (vpnInterface != null) return

            val builder = Builder()
                .setSession("NextAI VPN Mesh")
                .addAddress("10.0.0.2", 24)
                .addDnsServer("1.1.1.1")
                .addRoute("0.0.0.0", 0) // Route all traffic through VPN tunnel
                .setMtu(1420)

            vpnInterface = builder.establish()
            Log.i(TAG, "[VI] Đường hầm VPN đã kích hoạt thành công / [EN] VPN Tunnel established successfully")
        } catch (e: Exception) {
            Log.e(TAG, "Failed to establish VPN tunnel", e)
        }
    }

    private fun tearDownVpnTunnel() {
        try {
            vpnInterface?.close()
            vpnInterface = null
            stopSelf()
            Log.i(TAG, "[VI] Đường hầm VPN đã đóng / [EN] VPN Tunnel closed")
        } catch (e: Exception) {
            Log.e(TAG, "Error closing VPN tunnel", e)
        }
    }

    override fun onDestroy() {
        tearDownVpnTunnel()
        super.onDestroy()
    }
}
