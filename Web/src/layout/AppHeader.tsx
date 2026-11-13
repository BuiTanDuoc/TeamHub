import { cilAccountLogout, cilBell, cilContrast, cilLanguage, cilMenu, cilMoon, cilSun } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import {
  CBadge, CContainer, CDropdown, CDropdownDivider, CDropdownHeader, CDropdownItem, CDropdownMenu, CDropdownToggle, CHeader,
  CHeaderNav, CHeaderToggler, useColorModes,
} from '@coreui/react'
import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { LANGUAGES } from '@/i18n'
import { useAuthStore } from '@/shared/auth/authStore'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useUiStore } from '@/shared/stores/uiStore'
import { NotificationBell } from './NotificationBell'

/** Khóa localStorage mà script trong index.html cũng đọc để tránh nháy theme khi tải trang. */
export const THEME_STORAGE_KEY = 'teamhub-theme'

type Mode = 'light' | 'dark' | 'auto'
const MODES: { mode: Mode; icon: string[]; labelKey: string }[] = [
  { mode: 'light', icon: cilSun, labelKey: 'theme.light' },
  { mode: 'dark', icon: cilMoon, labelKey: 'theme.dark' },
  { mode: 'auto', icon: cilContrast, labelKey: 'theme.auto' },
]

export function AppHeader() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const headerRef = useRef<HTMLDivElement>(null)
  const { colorMode, setColorMode } = useColorModes(THEME_STORAGE_KEY)
  const sidebarShow = useUiStore((s) => s.sidebarShow)
  const setSidebarShow = useUiStore((s) => s.setSidebarShow)
  const user = useAuthStore((s) => s.user)
  const logout = useAuthStore((s) => s.logout)

  useEffect(() => {
    const onScroll = () => headerRef.current?.classList.toggle('shadow-sm', document.documentElement.scrollTop > 0)
    document.addEventListener('scroll', onScroll)
    return () => document.removeEventListener('scroll', onScroll)
  }, [])

  const currentIcon = MODES.find((m) => m.mode === colorMode)?.icon ?? cilContrast
  const currentLang = i18n.language.startsWith('en') ? 'en' : 'vi'

  return (
    <CHeader position="sticky" className="mb-4 p-0" ref={headerRef}>
      <CContainer className="border-bottom px-4" fluid>
        <CHeaderToggler onClick={() => setSidebarShow(!sidebarShow)} style={{ marginInlineStart: '-14px' }} aria-label="Toggle sidebar">
          <CIcon icon={cilMenu} size="lg" />
        </CHeaderToggler>

        <CHeaderNav className="ms-auto">
          <NotificationBell />
        </CHeaderNav>

        <CHeaderNav>
          <li className="nav-item py-1">
            <div className="vr h-100 mx-2 text-body text-opacity-75" />
          </li>

          {/* Ngôn ngữ */}
          <CDropdown variant="nav-item" placement="bottom-end">
            <CDropdownToggle caret={false} aria-label={t('language.label')}>
              <CIcon icon={cilLanguage} size="lg" />
              <span className="ms-1 text-uppercase small">{currentLang}</span>
            </CDropdownToggle>
            <CDropdownMenu>
              {LANGUAGES.map((lang) => (
                <CDropdownItem key={lang.code} as="button" type="button" active={currentLang === lang.code} onClick={() => void i18n.changeLanguage(lang.code)}>
                  <span className="me-2">{lang.flag}</span>
                  {lang.label}
                </CDropdownItem>
              ))}
            </CDropdownMenu>
          </CDropdown>

          {/* Giao diện sáng/tối */}
          <CDropdown variant="nav-item" placement="bottom-end">
            <CDropdownToggle caret={false} aria-label={t('theme.label')}>
              <CIcon icon={currentIcon} size="lg" />
            </CDropdownToggle>
            <CDropdownMenu>
              {MODES.map((m) => (
                <CDropdownItem key={m.mode} as="button" type="button" active={colorMode === m.mode} className="d-flex align-items-center" onClick={() => setColorMode(m.mode)}>
                  <CIcon className="me-2" icon={m.icon} size="lg" />
                  {t(m.labelKey)}
                </CDropdownItem>
              ))}
            </CDropdownMenu>
          </CDropdown>

          <li className="nav-item py-1">
            <div className="vr h-100 mx-2 text-body text-opacity-75" />
          </li>

          {/* Tài khoản */}
          <CDropdown variant="nav-item" placement="bottom-end">
            <CDropdownToggle className="py-0 pe-0" caret={false}>
              <UserAvatar name={user?.fullName ?? '?'} src={user?.avatarUrl ?? null} size="md" />
            </CDropdownToggle>
            <CDropdownMenu className="pt-0">
              <CDropdownHeader className="bg-body-secondary fw-semibold py-2">
                <div>{user?.fullName}</div>
                <div className="small fw-normal text-body-secondary">{user?.email}</div>
                {user?.isSystemAdmin && (
                  <CBadge color="danger" className="mt-1">
                    {t('roles.systemAdmin')}
                  </CBadge>
                )}
              </CDropdownHeader>
              <CDropdownDivider />
              <CDropdownItem as="button" type="button" onClick={() => navigate('/settings/notifications')}>
                <CIcon icon={cilBell} className="me-2" />
                {t('notifications.settings')}
              </CDropdownItem>
              <CDropdownItem
                as="button"
                type="button"
                onClick={() => {
                  void logout().then(() => navigate('/login', { replace: true }))
                }}
              >
                <CIcon icon={cilAccountLogout} className="me-2" />
                {t('auth.logout')}
              </CDropdownItem>
            </CDropdownMenu>
          </CDropdown>
        </CHeaderNav>
      </CContainer>
    </CHeader>
  )
}
