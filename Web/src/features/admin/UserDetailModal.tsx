import { CBadge, CButton, CFormSelect, CListGroup, CListGroupItem, CModal, CModalBody, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { teamsApi, usersApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { TEAM_ROLES, type TeamRole, type UserDetail } from '@/shared/api/types'
import { useAuthStore } from '@/shared/auth/authStore'
import { RoleBadge } from '@/shared/components/Badges'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { formatDateTime } from '@/shared/utils/format'
import { ResetPasswordModal } from './ResetPasswordModal'

type Confirm = 'deactivate' | 'revokeAdmin' | null

function AddToTeam({ user }: { user: UserDetail }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [teamId, setTeamId] = useState('')
  const [role, setRole] = useState<TeamRole>('Developer')
  const teams = useQuery({ queryKey: qk.teams, queryFn: () => teamsApi.list(1, 100) })
  const available = teams.data?.items.filter((x) => x.isActive && !user.teams.some((ut) => ut.teamId === x.id)) ?? []

  const add = useMutation({
    // Thêm vào team qua email (API hiện có); SystemAdmin vượt qua kiểm tra vai trò TeamLead
    mutationFn: () => teamsApi.addMember(teamId, { email: user.email, role, weeklyCapacityHours: null }),
    onSuccess: async () => {
      toast.success(t('members.added'))
      setTeamId('')
      await queryClient.invalidateQueries({ queryKey: qk.user(user.id) })
      await queryClient.invalidateQueries({ queryKey: qk.teams })
    },
    onError: showError,
  })

  if (!user.isActive) return <p className="small text-body-secondary mb-0">{t('users.inactiveCannotJoin')}</p>
  if (teams.isPending) return <CSpinner size="sm" />
  if (available.length === 0) return <p className="small text-body-secondary mb-0">{t('users.noTeamsToAdd')}</p>

  return (
    <div className="d-flex flex-wrap gap-2">
      <CFormSelect style={{ flex: '1 1 180px' }} value={teamId} aria-label={t('users.addToTeam')} onChange={(e) => setTeamId(e.target.value)}>
        <option value="">{t('users.selectTeam')}</option>
        {available.map((x) => (
          <option key={x.id} value={x.id}>{x.name}</option>
        ))}
      </CFormSelect>
      <CFormSelect style={{ width: 150 }} value={role} aria-label={t('members.role')} onChange={(e) => setRole(e.target.value as TeamRole)}>
        {TEAM_ROLES.map((r) => (
          <option key={r} value={r}>{t(`enums.teamRole.${r}`)}</option>
        ))}
      </CFormSelect>
      <CButton color="primary" disabled={!teamId || add.isPending} onClick={() => add.mutate()}>
        {add.isPending && <CSpinner size="sm" className="me-1" />}
        {t('common.add')}
      </CButton>
    </div>
  )
}

interface Props {
  userId: string
  onClose: () => void
}

export function UserDetailModal({ userId, onClose }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const me = useAuthStore((s) => s.user)
  const [confirm, setConfirm] = useState<Confirm>(null)
  const [resetting, setResetting] = useState(false)

  const query = useQuery({ queryKey: qk.user(userId), queryFn: () => usersApi.get(userId) })
  const refresh = () => queryClient.invalidateQueries({ queryKey: ['users'] })

  const setActive = useMutation({
    mutationFn: (isActive: boolean) => usersApi.setActive(userId, isActive),
    onSuccess: async (_d, isActive) => {
      toast.success(isActive ? t('users.activated') : t('users.deactivated'))
      setConfirm(null)
      await refresh()
    },
    onError: (e) => {
      setConfirm(null)
      showError(e)
    },
  })

  const setAdmin = useMutation({
    mutationFn: (isSystemAdmin: boolean) => usersApi.setSystemAdmin(userId, isSystemAdmin),
    onSuccess: async (_d, granted) => {
      toast.success(granted ? t('users.adminGranted') : t('users.adminRevoked'))
      setConfirm(null)
      await refresh()
    },
    onError: (e) => {
      setConfirm(null)
      showError(e)
    },
  })

  return (
    <CModal visible onClose={onClose} size="lg" scrollable alignment="top">
      <CModalHeader>
        <CModalTitle>{t('users.detail')}</CModalTitle>
      </CModalHeader>
      <CModalBody>
        <QueryBoundary query={query}>
          {(user) => {
            const isMe = user.id === me?.id
            return (
              <>
                <div className="d-flex align-items-center gap-3 mb-4">
                  <UserAvatar name={user.fullName} src={user.avatarUrl} size="xl" />
                  <div>
                    <h3 className="h4 mb-0">{user.fullName} {isMe && <span className="small text-body-secondary">({t('common.you')})</span>}</h3>
                    <div className="text-body-secondary">{user.email}</div>
                    <div className="d-flex flex-wrap gap-1 mt-1">
                      <CBadge color={user.isActive ? 'success' : 'secondary'}>{user.isActive ? t('users.active') : t('users.inactive')}</CBadge>
                      {user.isSystemAdmin && <CBadge color="danger">{t('roles.systemAdmin')}</CBadge>}
                    </div>
                    <div className="small text-body-secondary mt-1">{t('users.createdAt')}: {formatDateTime(user.createdAt)}</div>
                  </div>
                </div>

                <h4 className="h6">{t('users.teamsAndRoles')}</h4>
                {user.teams.length === 0 ? (
                  <p className="small text-body-secondary">{t('users.noTeams')}</p>
                ) : (
                  <CListGroup className="mb-3">
                    {user.teams.map((x) => (
                      <CListGroupItem key={x.teamId} className="d-flex justify-content-between align-items-center">
                        <Link to={`/teams/${x.teamId}?tab=members`} onClick={onClose}>{x.teamName}</Link>
                        <RoleBadge role={x.role} />
                      </CListGroupItem>
                    ))}
                  </CListGroup>
                )}
                <div className="mb-4">
                  <AddToTeam user={user} />
                </div>

                <h4 className="h6">{t('users.accountActions')}</h4>
                <div className="d-flex flex-wrap gap-2">
                  {user.isActive ? (
                    <CButton color="danger" variant="outline" disabled={isMe || setActive.isPending} title={isMe ? t('users.cannotSelf') : undefined} onClick={() => setConfirm('deactivate')}>
                      {t('users.deactivate')}
                    </CButton>
                  ) : (
                    <CButton color="success" variant="outline" disabled={setActive.isPending} onClick={() => setActive.mutate(true)}>
                      {setActive.isPending && <CSpinner size="sm" className="me-1" />}
                      {t('users.activate')}
                    </CButton>
                  )}
                  {user.isSystemAdmin ? (
                    <CButton color="warning" variant="outline" disabled={isMe || setAdmin.isPending} title={isMe ? t('users.cannotSelf') : undefined} onClick={() => setConfirm('revokeAdmin')}>
                      {t('users.revokeAdmin')}
                    </CButton>
                  ) : (
                    <CButton color="warning" variant="outline" disabled={!user.isActive || setAdmin.isPending} title={!user.isActive ? t('users.activateFirst') : undefined} onClick={() => setAdmin.mutate(true)}>
                      {t('users.grantAdmin')}
                    </CButton>
                  )}
                  <CButton color="secondary" variant="outline" onClick={() => setResetting(true)}>
                    {t('users.resetPassword')}
                  </CButton>
                </div>
                <p className="small text-body-secondary mt-3 mb-0">{t('users.deactivateNote')}</p>

                <ConfirmModal
                  visible={confirm === 'deactivate'} title={t('users.deactivate')} message={t('users.deactivateConfirm', { name: user.fullName })}
                  confirmLabel={t('users.deactivate')} busy={setActive.isPending} onClose={() => setConfirm(null)} onConfirm={() => setActive.mutate(false)}
                />
                <ConfirmModal
                  visible={confirm === 'revokeAdmin'} title={t('users.revokeAdmin')} message={t('users.revokeAdminConfirm', { name: user.fullName })}
                  confirmLabel={t('users.revokeAdmin')} busy={setAdmin.isPending} onClose={() => setConfirm(null)} onConfirm={() => setAdmin.mutate(false)}
                />
                <ResetPasswordModal userId={user.id} userName={user.fullName} visible={resetting} onClose={() => setResetting(false)} />
              </>
            )
          }}
        </QueryBoundary>
      </CModalBody>
    </CModal>
  )
}
