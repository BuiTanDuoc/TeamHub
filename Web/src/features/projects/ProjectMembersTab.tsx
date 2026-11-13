import { cilPlus, cilTrash } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import {
  CButton, CCard, CForm, CFormSelect, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner, CTable, CTableBody,
  CTableDataCell, CTableHead, CTableHeaderCell, CTableRow,
} from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { projectsApi, teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { ProjectMember } from '@/shared/api/types'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { EmptyState } from '@/shared/components/EmptyState'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { formatDate } from '@/shared/utils/format'

interface Props {
  teamId: string
  projectId: string
  canManage: boolean
}

/** ProjectMember = tập người có thể được giao task của dự án; phải là thành viên team (CLAUDE.md §14). */
export function ProjectMembersTab({ teamId, projectId, canManage }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [adding, setAdding] = useState(false)
  const [selected, setSelected] = useState('')
  const [removing, setRemoving] = useState<ProjectMember | null>(null)

  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId) })
  const teamMembers = useQuery({ queryKey: qk.teamMembers(teamId), queryFn: () => teamsApi.members(teamId), enabled: adding })

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) })
    await queryClient.invalidateQueries({ queryKey: qk.projects(teamId) })
  }

  const add = useMutation({
    mutationFn: (userId: string) => projectsApi.addMember(teamId, projectId, userId),
    onSuccess: async () => {
      await refresh()
      setAdding(false)
      setSelected('')
      toast.success(t('members.added'))
    },
    onError: showError,
  })

  const remove = useMutation({
    mutationFn: (userId: string) => projectsApi.removeMember(teamId, projectId, userId),
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

  const existing = new Set(members.data?.items.map((m) => m.userId))
  const candidates = teamMembers.data?.items.filter((m) => !existing.has(m.userId)) ?? []

  return (
    <>
      {canManage && (
        <div className="d-flex justify-content-end mb-3">
          <CButton color="primary" onClick={() => setAdding(true)}>
            <CIcon icon={cilPlus} className="me-1" />
            {t('members.addToProject')}
          </CButton>
        </div>
      )}

      <QueryBoundary query={members}>
        {(data) =>
          data.items.length === 0 ? (
            <EmptyState title={t('projects.noMembers')} description={t('projects.noMembersHint')} />
          ) : (
            <CCard>
              <CTable align="middle" hover responsive className="mb-0">
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>{t('members.member')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('members.addedAt')}</CTableHeaderCell>
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
                            <div className="fw-semibold">{m.fullName}</div>
                            <div className="small text-body-secondary">{m.email}</div>
                          </div>
                        </div>
                      </CTableDataCell>
                      <CTableDataCell className="text-body-secondary">{formatDate(m.addedAt.slice(0, 10))}</CTableDataCell>
                      {canManage && (
                        <CTableDataCell className="text-end">
                          <CButton color="danger" variant="ghost" size="sm" title={t('members.remove')} onClick={() => setRemoving(m)}>
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

      <CModal visible={adding} onClose={() => setAdding(false)} backdrop="static">
        <CForm
          onSubmit={(e) => {
            e.preventDefault()
            if (selected) add.mutate(selected)
          }}
        >
          <CModalHeader>
            <CModalTitle>{t('members.addToProject')}</CModalTitle>
          </CModalHeader>
          <CModalBody>
            {teamMembers.isPending ? (
              <div className="text-center">
                <CSpinner size="sm" />
              </div>
            ) : candidates.length === 0 ? (
              <p className="text-body-secondary mb-0">{t('members.noCandidates')}</p>
            ) : (
              <CFormSelect value={selected} aria-label={t('members.member')} onChange={(e) => setSelected(e.target.value)}>
                <option value="">{t('members.selectMember')}</option>
                {candidates.map((m) => (
                  <option key={m.userId} value={m.userId}>
                    {m.fullName} ({m.email})
                  </option>
                ))}
              </CFormSelect>
            )}
          </CModalBody>
          <CModalFooter>
            <CButton color="secondary" variant="outline" onClick={() => setAdding(false)} disabled={add.isPending}>
              {t('common.cancel')}
            </CButton>
            <CButton type="submit" color="primary" disabled={!selected || add.isPending}>
              {add.isPending && <CSpinner size="sm" className="me-2" />}
              {t('common.add')}
            </CButton>
          </CModalFooter>
        </CForm>
      </CModal>

      <ConfirmModal
        visible={!!removing}
        title={t('members.remove')}
        message={t('members.removeFromProjectConfirm', { name: removing?.fullName ?? '' })}
        confirmLabel={t('members.remove')}
        busy={remove.isPending}
        onClose={() => setRemoving(null)}
        onConfirm={() => removing && remove.mutate(removing.userId)}
      />
    </>
  )
}
