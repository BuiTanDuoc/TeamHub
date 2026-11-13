import { CButton, CFormInput, CFormSelect, CFormTextarea, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { bugsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { BugDetail, BugStatus } from '@/shared/api/types'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { requiresNote } from './bugWorkflow'

interface Props {
  teamId: string
  projectId: string
  bug: BugDetail
  /** null = đóng. */
  target: BugStatus | null
  onClose: () => void
}

/** Hộp thoại chuyển trạng thái bug: nhập lý do (bắt buộc với Reopened/WontFix/CannotReproduce/Duplicate), bản fix khi Fixed, bug gốc khi Duplicate. */
export function BugStatusDialog({ teamId, projectId, bug, target, onClose }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [note, setNote] = useState('')
  const [version, setVersion] = useState('')
  const [originalId, setOriginalId] = useState('')

  useEffect(() => {
    if (target) {
      setNote('')
      setVersion('')
      setOriginalId('')
    }
  }, [target])

  const isDuplicate = target === 'Duplicate'
  const originals = useQuery({
    queryKey: qk.bugPicker(teamId, projectId),
    queryFn: () => bugsApi.list(teamId, projectId, { pageSize: 100 }),
    enabled: isDuplicate,
    select: (d) => d.items.filter((b) => b.id !== bug.id),
  })

  const mutation = useMutation({
    mutationFn: async () => {
      if (!target) return
      const trimmed = note.trim()
      if (isDuplicate) return bugsApi.markDuplicate(teamId, projectId, bug.id, originalId, trimmed)
      return bugsApi.changeStatus(teamId, projectId, bug.id, target, trimmed || null, version.trim() || null)
    },
    onSuccess: async () => {
      toast.success(t('bugs.statusChanged'))
      await queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) })
      onClose()
    },
    onError: (e) => showError(e),
  })

  if (!target) return null
  const noteRequired = requiresNote(target)
  const valid = (!noteRequired || note.trim().length > 0) && (!isDuplicate || originalId !== '')

  return (
    <CModal visible onClose={onClose} backdrop="static" alignment="center">
      <CModalHeader>
        <CModalTitle>
          {t('bugs.moveTo', { status: t(`enums.bugStatus.${target}`) })}
        </CModalTitle>
      </CModalHeader>
      <CModalBody>
        {isDuplicate && (
          <Field label={t('bugs.originalBug')} required htmlFor="bug-original">
            <CFormSelect id="bug-original" value={originalId} onChange={(e) => setOriginalId(e.target.value)}>
              <option value="">{t('bugs.selectOriginal')}</option>
              {originals.data?.map((b) => (
                <option key={b.id} value={b.id}>{b.code} — {b.title}</option>
              ))}
            </CFormSelect>
          </Field>
        )}
        {target === 'Fixed' && (
          <Field label={t('bugs.fixedInVersion')} htmlFor="bug-fixed-version">
            <CFormInput id="bug-fixed-version" maxLength={50} value={version} onChange={(e) => setVersion(e.target.value)} />
          </Field>
        )}
        <Field label={t('bugs.note')} required={noteRequired} hint={noteRequired ? t('bugs.noteRequiredHint') : undefined} htmlFor="bug-note" className="mb-0">
          <CFormTextarea id="bug-note" rows={3} maxLength={2000} value={note} onChange={(e) => setNote(e.target.value)} />
        </Field>
      </CModalBody>
      <CModalFooter>
        <CButton color="secondary" variant="outline" onClick={onClose} disabled={mutation.isPending}>
          {t('common.cancel')}
        </CButton>
        <CButton color="primary" disabled={!valid || mutation.isPending} onClick={() => mutation.mutate()}>
          {mutation.isPending && <CSpinner size="sm" className="me-2" />}
          {t('common.confirm')}
        </CButton>
      </CModalFooter>
    </CModal>
  )
}
