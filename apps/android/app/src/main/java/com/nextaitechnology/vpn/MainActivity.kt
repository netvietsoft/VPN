package com.nextaitechnology.vpn

import android.content.Intent
import android.net.VpnService
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.nextaitechnology.vpn.service.NextAiVpnService

/**
 * [VI] Màn hình chính ứng dụng NextAI VPN Native Android (sử dụng Jetpack Compose).
 * [EN] Main Activity of NextAI VPN Native Android application (using Jetpack Compose).
 */
class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            NextAiVpnApp(
                onConnectClick = { startVpnConnection() },
                onDisconnectClick = { stopVpnConnection() }
            )
        }
    }

    private fun startVpnConnection() {
        val intent = VpnService.prepare(this)
        if (intent != null) {
            startActivityForResult(intent, 1001)
        } else {
            onActivityResult(1001, RESULT_OK, null)
        }
    }

    private fun stopVpnConnection() {
        val intent = Intent(this, NextAiVpnService::class.java).apply {
            action = NextAiVpnService.ACTION_DISCONNECT
        }
        startService(intent)
    }

    @Deprecated("Deprecated in Java")
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        super.onActivityResult(requestCode, resultCode, data)
        if (requestCode == 1001 && resultCode == RESULT_OK) {
            val intent = Intent(this, NextAiVpnService::class.java).apply {
                action = NextAiVpnService.ACTION_CONNECT
            }
            startService(intent)
        }
    }
}

@Composable
fun NextAiVpnApp(onConnectClick: () -> Unit, onDisconnectClick: () -> Unit) {
    var isConnected by remember { mutableStateOf(false) }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFF0A0E17))
            .padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.SpaceBetween
    ) {
        // Header
        Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.padding(top = 32.dp)) {
            Text(
                text = "NextAI VPN",
                color = Color.White,
                fontSize = 24.sp,
                fontWeight = FontWeight.Bold
            )
            Text(
                text = "Residential Mesh & Secure Tunnel",
                color = Color(0xFF94A3B8),
                fontSize = 14.sp
            )
        }

        // Status Card
        Card(
            shape = RoundedCornerShape(20.dp),
            colors = CardDefaults.cardColors(containerColor = Color(0xFF111827)),
            modifier = Modifier.fillMaxWidth().padding(horizontal = 8.dp)
        ) {
            Column(
                modifier = Modifier.padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Text(
                    text = if (isConnected) "ĐÃ KẾT NỐI BẢO MẬT" else "CHƯA KẾT NỐI",
                    color = if (isConnected) Color(0xFF10B981) else Color(0xFFEF4444),
                    fontWeight = FontWeight.Bold,
                    fontSize = 16.sp
                )
                Spacer(modifier = Modifier.height(8.dp))
                Text(
                    text = if (isConnected) "IP Cư Dân: 142.250.190.46 (United States)" else "IP Thật: Đang lộ",
                    color = Color.White,
                    fontSize = 13.sp
                )
            }
        }

        // Action Button
        Button(
            onClick = {
                if (isConnected) {
                    onDisconnectClick()
                    isConnected = false
                } else {
                    onConnectClick()
                    isConnected = true
                }
            },
            shape = RoundedCornerShape(30.dp),
            colors = ButtonDefaults.buttonColors(
                containerColor = if (isConnected) Color(0xFFEF4444) else Color(0xFF2563EB)
            ),
            modifier = Modifier
                .fillMaxWidth()
                .height(60.dp)
                .padding(bottom = 16.dp)
        ) {
            Text(
                text = if (isConnected) "NGẮT KẾT NỐI (DISCONNECT)" else "KẾT NỐI NGAY (CONNECT)",
                fontSize = 16.sp,
                fontWeight = FontWeight.Bold
            )
        }
    }
}
