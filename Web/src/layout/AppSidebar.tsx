import { cilBell, cilGroup, cilSpeedometer } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import {
  CBadge, CCloseButton, CNavGroup, CNavItem, CNavLink, CNavTitle, CSidebar, CSidebarBrand, CSidebarFooter,
  CSidebarHeader, CSidebarNav, CSidebarToggler,
} from '@coreui/react'
import { useQuery } from '@tanstack/react-query'
import { memo } from 'react'
import { useTranslation } from 'react-i18next'
import { NavLink } from 'react-router-dom'
import SimpleBar from 'simplebar-react'
import 'simplebar-react/dist/simplebar.min.css'
import { notificationsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { useAuthStore } from '@/shared/auth/authStore'
import { useUiStore } from '@/shared/stores/uiStore'

const BrandMark = () => (
  <svg width="28" height="28" viewBox="0 0 32 32" aria-hidden="true">
    <rect width="32" height="32" rx="7" fill="#5856d6" />
    <path d="M9 10h14v3h-5.5v10h-3V13H9z" fill="#fff" />
  </svg>
)

function AppSidebarComponent() {
  const { t } = useTranslation()
  const sidebarShow = useUiStore((s) => s.sidebarShow)
  const unfoldable = useUiStore((s) => s.sidebarUnfoldable)
  const setShow = useUiStore((s) => s.setSidebarShow)
  const setUnfoldable = useUiStore((s) => s.setSidebarUnfoldable)
  const teams = useAuthStore((s) => s.teams)
  const unread = useQuery({ queryKey: qk.unreadCount, queryFn: notificationsApi.unreadCount, refetchInterval: 60_000 })

  return (
    <CSidebar
      className="border-end"
      colorScheme="dark"
      position="fixed"
      unfoldable={unfoldable}
      visible={sidebarShow}
      onVisibleChange={(visible) => setShow(visible)}
    >
      <CSidebarHeader className="border-bottom">
        <CSidebarBrand as={NavLink} to="/" className="gap-2">
          <span className="sidebar-brand-full d-flex align-items-center gap-2">
            <BrandMark />
            <span className="fs-5 fw-semibold">TeamHub</span>
          </span>
          <span className="sidebar-brand-narrow">
            <BrandMark />
          </span>
        </CSidebarBrand>
        <CCloseButton className="d-lg-none" dark onClick={() => setShow(false)} />
      </CSidebarHeader>

      <CSidebarNav as={SimpleBar}>
        <CNavItem as="div">
          <CNavLink as={NavLink} to="/dashboard">
            <CIcon customClassName="nav-icon" icon={cilSpeedometer} />
            {t('nav.dashboard')}
          </CNavLink>
        </CNavItem>
        <CNavItem as="div">
          <CNavLink as={NavLink} to="/teams" end>
            <CIcon customClassName="nav-icon" icon={cilGroup} />
            {t('nav.teams')}
          </CNavLink>
        </CNavItem>
        <CNavItem as="div">
          <CNavLink as={NavLink} to="/notifications">
            <CIcon customClassName="nav-icon" icon={cilBell} />
            {t('nav.notifications')}
            {!!unread.data?.count && (
              <CBadge color="danger" className="ms-auto" size="sm">
                {unread.data.count > 99 ? '99+' : unread.data.count}
              </CBadge>
            )}
          </CNavLink>
        </CNavItem>

        {teams.length > 0 && (
          <>
            <CNavTitle>{t('nav.myTeams')}</CNavTitle>
            <CNavGroup as="div" compact toggler={<><CIcon customClassName="nav-icon" icon={cilGroup} />{t('nav.myTeams')}</>} visible>
              {teams.map((team) => (
                <CNavItem as="div" key={team.teamId}>
                  <CNavLink as={NavLink} to={`/teams/${team.teamId}`}>
                    <span className="nav-icon">
                      <span className="nav-icon-bullet" />
                    </span>
                    {team.name}
                  </CNavLink>
                </CNavItem>
              ))}
            </CNavGroup>
          </>
        )}
      </CSidebarNav>

      <CSidebarFooter className="border-top d-none d-lg-flex">
        <CSidebarToggler onClick={() => setUnfoldable(!unfoldable)} />
      </CSidebarFooter>
    </CSidebar>
  )
}

export const AppSidebar = memo(AppSidebarComponent)
