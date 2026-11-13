import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { API_URL, getValidAccessToken } from '../api/client'
import { qk } from '../api/queryKeys'
import type { AppNotification } from '../api/types'
import { toast } from '../stores/uiStore'

/** Tên sự kiện server đẩy (SignalRNotificationPusher.EventName). */
const NOTIFICATION_EVENT = 'notification'

/**
 * Kết nối /hubs/notifications: server tự cho client vào group của user và các team của user khi OnConnectedAsync.
 * Token gửi qua query `access_token` (accessTokenFactory) — backend chỉ chấp nhận cách này cho đường dẫn /hubs.
 *
 * Backend chưa có sự kiện nào đẩy riêng cho BoardHub, nên khi có thông báo liên quan task thì ta làm mới
 * nhánh query của dự án đó (board/danh sách/chi tiết/bình luận) để người dùng thấy thay đổi gần realtime.
 */
export const useNotificationHub = (enabled: boolean): void => {
  const queryClient = useQueryClient()
  const { t } = useTranslation()

  useEffect(() => {
    if (!enabled) return

    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/notifications`, { accessTokenFactory: getValidAccessToken })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on(NOTIFICATION_EVENT, (n: AppNotification) => {
      void queryClient.invalidateQueries({ queryKey: qk.notifications })
      if (n.teamId && n.projectId) {
        void queryClient.invalidateQueries({ queryKey: qk.project(n.teamId, n.projectId) })
      }
      toast.info(`${t(`enums.notificationType.${n.type}`)}: ${n.message}`)
    })

    // Sau khi nối lại (mất mạng/server restart) có thể đã lỡ tin: tải lại danh sách
    connection.onreconnected(() => {
      void queryClient.invalidateQueries({ queryKey: qk.notifications })
    })

    let stopped = false
    connection.start().catch(() => {
      /* withAutomaticReconnect không áp dụng cho lần start đầu: thử lại một lần sau 5s */
      setTimeout(() => {
        if (!stopped && connection.state === HubConnectionState.Disconnected) connection.start().catch(() => {})
      }, 5000)
    })

    return () => {
      stopped = true
      void connection.stop()
    }
  }, [enabled, queryClient, t])
}
