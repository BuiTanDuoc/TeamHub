import { cilPlus, cilTrash } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CCard, CFormInput, CFormSelect, CTable, CTableBody, CTableDataCell, CTableHead, CTableHeaderCell, CTableRow } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { TEAM_ROLES, type TeamMember, type TeamRole } from '@/shared/api/types'
import { useAuthStore } from '@/shared/auth/authStore'
import { RoleBadge } from '@/shared/components/Badges'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { EmptyState } from '@/shared/components/EmptyState'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { formatDate } from '@/shared/utils/format'
import { AddMemberModal } from './AddMemberModal'

/** Ô sửa số giờ/tuần: lưu khi rời ô hoặc nhấn Enter, chỉ gọi API nếu giá trị hợp lệ và có thay đổi. */
function CapacityCell({ member, teamId, editable }: { member: TeamMember; teamId: string; editable: boolean }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [value, setValue] = useState(String(member.weeklyCapacityHours))
  const mutation = useMutation({
    mutationFn: (hours: number) => teamsApi.changeCapacity(teamId, member.userId, hours),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: qk.teamMembers(teamId) }),
    onError: (e) => {
      setValue(String(member.weeklyCapacityHours))
      showError(e)
    },
  })

  if (!editable) return <>{member.weeklyCapacityHours}h</>

  const commit = () => {
    const n = Number(value)
    if (value.trim() === '' || Number.isNaN(n) || n < 0 || n > 168) {
      setValue(String(member.weeklyCapacityHours))
      toast.error(t('validation.capacity'))
      return
    }
    if (n !== member.weeklyCapacityHours) mutation.mutate(n)
  }

  return (
    <CFormInput
      size="sm" type="number" min={0} max={168} step="0.5" style={{ width: 90 }} value={value} disabled={mutation.isPending}
      aria-label={t('members.capacity')}
      onChange={(e) => setValue(e.target.value)}
      onBlur={commit}
      onKeyDown={(e) => e.key === 'Enter' && e.currentTarget.blur()}
    />
  )
}

export function TeamMembersTab({ teamId, canManage }: { teamId: string; canManage: boolean }) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const me = useAuthStore((s) => s.user)
  const reloadMe = useAuthStore((s) => s.reloadMe)
  const [adding, setAdding] = useState(false)
  const [removing, setRemoving] = useState<TeamMember | null>(null)

  const query = useQuery({ queryKey: qk.teamMembers(teamId), queryFn: () => teamsApi.members(teamId) })

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: qk.team(teamId) })
    await queryClient.invalidateQueries({ queryKey: qk.teams })
  }

  const changeRole = useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: TeamRole }) => teamsApi.changeRole(teamId, userId, role),
    onSuccess: async (_d, vars) => {
      await refresh()
      if (vars.userId === me?.id) await reloadMe() // đổi vai trò của chính mình → quyền UI thay đổi
      toast.success(t('members.roleChanged'))
    },
    onError: showError,
  })

  const remove = useMutation({
    mutationFn: (userId: string) => teamsApi.removeMember(teamId, userId),
    onSuccess: async () => {
      await refresh()
      setRemoving(null)
      toast.success(t('members.removed'))
    },
    onError: (e) => {
      setRemoving(null)
      showError(e)
    },
  })

  return (
    <>
      {canManage && (
        <div className="d-flex justify-content-end mb-3">
          <CButton color="primary" onClick={() => setAdding(true)}>
            <CIcon icon={cilPlus} className="me-1" />
            {t('members.add')}
          </CButton>
        </div>
      )}

      <QueryBoundary query={query}>
        {(data) =>
          data.items.length === 0 ? (
            <EmptyState title={t('members.empty')} />
          ) : (
            <CCard>
              <CTable align="middle" hover responsive className="mb-0">
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>{t('members.member')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('members.role')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('members.capacity')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('members.joined')}</CTableHeaderCell>
                    {canManage && <CTableHeaderCell />}
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {data.items.map((m) => (
                    <CTableRow key={m.userId}>
                      <CTableDataCell>
                        <div className="d-flex align-items-center gap-2">
                          <UserAvatar name={m.fullName} src={m.avatarUrl} size="sm" />
                          <div>
                            <div className="fw-semibold">
                              {m.fullName} {m.userId === me?.id && <span className="text-body-secondary small">({t('common.you')})</span>}
                            </div>
                            <div className="small text-body-secondary">{m.email}</div>
                          </div>
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        {canManage ? (
                          <CFormSelect
                            size="sm" style={{ width: 150 }} value={m.role} disabled={changeRole.isPending}
                            aria-label={t('members.role')}
                            onChange={(e) => changeRole.mutate({ userId: m.userId, role: e.target.value as TeamRole })}
                          >
                            {TEAM_ROLES.map((r) => (
                              <option key={r} value={r}>
                                {t(`enums.teamRole.${r}`)}
                              </option>
                            ))}
                          </CFormSelect>
                        ) : (
                          <RoleBadge role={m.role} />
                        )}
                      </CTableDataCell>
                      <CTableDataCell>
                        <CapacityCell key={m.weeklyCapacityHours} member={m} teamId={teamId} editable={canManage} />
                      </CTableDataCell>
                      <CTableDataCell className="text-body-secondary">{formatDate(m.joinedAt.slice(0, 10))}</CTableDataCell>
                      {canManage && (
                        <CTableDataCell className="text-end">
                          <CButton color="danger" variant="ghost" size="sm" disabled={m.userId === me?.id} title={m.userId === me?.id ? t('members.useLeave') : t('members.remove')} onClick={() => setRemoving(m)}>
                            <CIcon icon={cilTrash} />
                          </CButton>
                        </CTableDataCell>
                      )}
                    </CTableRow>
                  ))}
                </CTableBody>
              </CTable>
            </CCard>
          )
        }
      </QueryBoundary>

      <AddMemberModal teamId={teamId} visible={adding} onClose={() => setAdding(false)} />
      <ConfirmModal
        visible={!!removing}
        title={t('members.remove')}
        message={t('members.removeConfirm', { name: removing?.fullName ?? '' })}
        confirmLabel={t('members.remove')}
        busy={remove.isPending}
        onClose={() => setRemoving(null)}
        onConfirm={() => removing && remove.mutate(removing.userId)}
      />
    </>
  )
}
