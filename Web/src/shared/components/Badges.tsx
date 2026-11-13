import { CBadge } from '@coreui/react'
import { useTranslation } from 'react-i18next'
import type { BugSeverity, BugStatus, NotificationType, Priority, ProjectStatus, TeamRole, WorkItemStatus, WorkItemType } from '../api/types'

type Color = 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'dark' | 'light'

const ROLE_COLOR: Record<TeamRole, Color> = { Viewer: 'secondary', Tester: 'info', Developer: 'primary', TeamLead: 'warning' }
const PROJECT_COLOR: Record<ProjectStatus, Color> = { Planning: 'secondary', Active: 'success', OnHold: 'warning', Done: 'primary' }
export const TASK_STATUS_COLOR: Record<WorkItemStatus, Color> = {
  Todo: 'secondary', InProgress: 'primary', InReview: 'info', Testing: 'warning', Done: 'success',
}
export const PRIORITY_COLOR: Record<Priority, Color> = { Low: 'secondary', Medium: 'info', High: 'warning', Urgent: 'danger' }
const TYPE_COLOR: Record<WorkItemType, Color> = { Epic: 'dark', Story: 'success', Task: 'primary', SubTask: 'secondary' }

export const RoleBadge = ({ role }: { role: TeamRole }) => {
  const { t } = useTranslation()
  return <CBadge color={ROLE_COLOR[role]}>{t(`enums.teamRole.${role}`)}</CBadge>
}

export const ProjectStatusBadge = ({ status }: { status: ProjectStatus }) => {
  const { t } = useTranslation()
  return <CBadge color={PROJECT_COLOR[status]}>{t(`enums.projectStatus.${status}`)}</CBadge>
}

export const TaskStatusBadge = ({ status }: { status: WorkItemStatus }) => {
  const { t } = useTranslation()
  return <CBadge color={TASK_STATUS_COLOR[status]}>{t(`enums.taskStatus.${status}`)}</CBadge>
}

export const PriorityBadge = ({ priority }: { priority: Priority }) => {
  const { t } = useTranslation()
  return <CBadge color={PRIORITY_COLOR[priority]}>{t(`enums.priority.${priority}`)}</CBadge>
}

export const TypeBadge = ({ type }: { type: WorkItemType }) => {
  const { t } = useTranslation()
  return (
    <CBadge color={TYPE_COLOR[type]} shape="rounded-pill">
      {t(`enums.taskType.${type}`)}
    </CBadge>
  )
}

export const NOTIFICATION_ICON_COLOR: Record<NotificationType, Color> = {
  TaskAssigned: 'primary', TaskCommented: 'info', TaskStatusChanged: 'warning',
  BugAssigned: 'danger', BugStatusChanged: 'danger', Mentioned: 'success',
}

export const BUG_STATUS_COLOR: Record<BugStatus, Color> = {
  New: 'secondary', Confirmed: 'info', Assigned: 'primary', InProgress: 'primary', Fixed: 'success', ReadyToRetest: 'warning',
  Verified: 'success', Closed: 'dark', Reopened: 'danger', WontFix: 'secondary', Duplicate: 'secondary', CannotReproduce: 'secondary',
}
export const BUG_SEVERITY_COLOR: Record<BugSeverity, Color> = { Trivial: 'secondary', Minor: 'info', Major: 'warning', Critical: 'danger' }

export const BugStatusBadge = ({ status }: { status: BugStatus }) => {
  const { t } = useTranslation()
  return <CBadge color={BUG_STATUS_COLOR[status]}>{t(`enums.bugStatus.${status}`)}</CBadge>
}

export const BugSeverityBadge = ({ severity }: { severity: BugSeverity }) => {
  const { t } = useTranslation()
  return <CBadge color={BUG_SEVERITY_COLOR[severity]}>{t(`enums.bugSeverity.${severity}`)}</CBadge>
}
