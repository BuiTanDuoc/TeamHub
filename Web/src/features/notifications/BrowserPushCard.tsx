import { CAlert, CCard, CCardBody, CFormSwitch, CSpinner } from '@coreui/react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  disablePush, enablePush, getNotificationPermission, getPushSupport, isPushEnabled, PushPermissionError, type PushSupport,
} from '@/shared/push/push'
import { toast } from '@/shared/stores/uiStore'

/** Bật/tắt push của trình duyệt này (Firebase Cloud Messaging). Trạng thái lưu theo từng trình duyệt, không theo tài khoản. */
export function BrowserPushCard() {
  const { t } = useTranslation()
  const [support, setSupport] = useState<PushSupport | null>(null)
  const [enabled, setEnabled] = useState(isPushEnabled)
  const [busy, setBusy] = useState(false)
  const [denied, setDenied] = useState(getNotificationPermission() === 'denied')

  useEffect(() => {
    let cancelled = false
    void getPushSupport().then((s) => {
      if (!cancelled) setSupport(s)
    })
    return () => {
      cancelled = true
    }
  }, [])

  const toggle = async (next: boolean) => {
    setBusy(true)
    try {
      if (next) {
        await enablePush()
        setEnabled(true)
        setDenied(false)
        toast.success(t('notifications.browserPush.enabledToast'))
      } else {
        await disablePush()
        setEnabled(false)
        toast.success(t('notifications.browserPush.disabledToast'))
      }
    } catch (error) {
      if (error instanceof PushPermissionError) {
        setDenied(error.permission === 'denied')
        toast.error(t('notifications.browserPush.permissionNotGranted'))
      } else {
        toast.error(t('notifications.browserPush.failed'))
      }
    } finally {
      setBusy(false)
    }
  }

  const blocked = support === 'unsupported' || support === 'not-configured'

  return (
    <CCard className="mb-3">
      <CCardBody>
        <div className="d-flex justify-content-between align-items-start gap-3">
          <div>
            <h5 className="mb-1">{t('notifications.browserPush.title')}</h5>
            <div className="small text-body-secondary">{t('notifications.browserPush.hint')}</div>
          </div>
          {support === null ? (
            <CSpinner size="sm" />
          ) : (
            <div className="d-flex align-items-center">
              {busy && <CSpinner size="sm" className="me-2" />}
              <CFormSwitch
                id="browser-push"
                className="d-inline-block"
                aria-label={t('notifications.browserPush.title')}
                checked={enabled}
                disabled={busy || blocked || (denied && !enabled)}
                onChange={(e) => void toggle(e.target.checked)}
              />
            </div>
          )}
        </div>

        {support === 'unsupported' && (
          <CAlert color="warning" className="mt-3 mb-0">
            {t('notifications.browserPush.unsupported')}
          </CAlert>
        )}
        {support === 'not-configured' && (
          <CAlert color="secondary" className="mt-3 mb-0">
            {t('notifications.browserPush.notConfigured')}
          </CAlert>
        )}
        {support === 'ok' && denied && !enabled && (
          <CAlert color="warning" className="mt-3 mb-0">
            {t('notifications.browserPush.denied')}
          </CAlert>
        )}
      </CCardBody>
    </CCard>
  )
}
