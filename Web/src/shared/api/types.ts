// Kiểu dữ liệu khớp DTO của backend (enum được serialize dạng chuỗi bởi JsonStringEnumConverter).
// CLAUDE.md §8 dự kiến sinh từ OpenAPI (`pnpm gen:api`); khi có src/Shared thì thay file này bằng bản generated.

export type TeamRole = 'Viewer' | 'Tester' | 'Developer' | 'TeamLead'
export type ProjectStatus = 'Planning' | 'Active' | 'OnHold' | 'Done'
export type WorkItemStatus = 'Todo' | 'InProgress' | 'InReview' | 'Testing' | 'Done'
export type WorkItemType = 'Epic' | 'Story' | 'Task' | 'SubTask'
export type Priority = 'Low' | 'Medium' | 'High' | 'Urgent'
export type WorkItemField = 'Status' | 'Assignee' | 'Estimate'
export type NotificationType = 'TaskAssigned' | 'TaskCommented' | 'TaskStatusChanged' | 'BugAssigned' | 'BugStatusChanged' | 'Mentioned'
export type BugStatus =
  | 'New' | 'Confirmed' | 'Assigned' | 'InProgress' | 'Fixed' | 'ReadyToRetest' | 'Verified' | 'Closed' | 'Reopened'
  | 'WontFix' | 'Duplicate' | 'CannotReproduce'
export type BugSeverity = 'Trivial' | 'Minor' | 'Major' | 'Critical'
export type BugEnvironment = 'Dev' | 'Staging' | 'Production'

export const TEAM_ROLES: readonly TeamRole[] = ['Viewer', 'Tester', 'Developer', 'TeamLead']
export const PROJECT_STATUSES: readonly ProjectStatus[] = ['Planning', 'Active', 'OnHold', 'Done']
export const WORK_ITEM_STATUSES: readonly WorkItemStatus[] = ['Todo', 'InProgress', 'InReview', 'Testing', 'Done']
export const WORK_ITEM_TYPES: readonly WorkItemType[] = ['Epic', 'Story', 'Task', 'SubTask']
export const NOTIFICATION_TYPES: readonly NotificationType[] = [
  'TaskAssigned', 'TaskCommented', 'TaskStatusChanged', 'BugAssigned', 'BugStatusChanged', 'Mentioned',
]
export const BUG_STATUSES: readonly BugStatus[] = [
  'New', 'Confirmed', 'Assigned', 'InProgress', 'Fixed', 'ReadyToRetest', 'Verified', 'Closed', 'Reopened',
  'WontFix', 'Duplicate', 'CannotReproduce',
]
export const BUG_SEVERITIES: readonly BugSeverity[] = ['Trivial', 'Minor', 'Major', 'Critical']
export const BUG_ENVIRONMENTS: readonly BugEnvironment[] = ['Dev', 'Staging', 'Production']
export const PRIORITIES: readonly Priority[] = ['Low', 'Medium', 'High', 'Urgent']

/** DateOnly của .NET → 'yyyy-MM-dd'. DateTimeOffset → chuỗi ISO 8601. */
export type DateOnly = string
export type IsoDateTime = string

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface CursorPagedResult<T> {
  items: T[]
  nextCursor: number | null
}

/** Keyset pagination với cursor mờ (chuỗi base64url) — dùng cho thông báo. */
export interface KeysetPagedResult<T> {
  items: T[]
  nextCursor: string | null
}

export interface IdResponse {
  id: string
}

// ---------- Auth ----------
export interface AuthUser {
  id: string
  email: string
  fullName: string
  avatarUrl: string | null
  isSystemAdmin: boolean
}

export interface AuthResult {
  accessToken: string
  accessTokenExpiresAt: IsoDateTime
  refreshToken: string
  refreshTokenExpiresAt: IsoDateTime
  user: AuthUser
}

export interface MyTeam {
  teamId: string
  name: string
  code: string
  role: TeamRole
}

export interface MeResponse {
  user: AuthUser
  teams: MyTeam[]
}

// ---------- Users / Teams ----------
export interface UserSummary {
  id: string
  email: string
  fullName: string
  avatarUrl: string | null
}

export interface Team {
  id: string
  name: string
  code: string
  description: string | null
  isActive: boolean
  memberCount: number
  createdAt: IsoDateTime
}

export interface TeamMember {
  userId: string
  email: string
  fullName: string
  avatarUrl: string | null
  role: TeamRole
  weeklyCapacityHours: number
  joinedAt: IsoDateTime
}

// ---------- Projects ----------
export interface Project {
  id: string
  teamId: string
  name: string
  key: string
  description: string | null
  status: ProjectStatus
  startDate: DateOnly | null
  endDate: DateOnly | null
  useSprints: boolean
  memberCount: number
  createdAt: IsoDateTime
}

export interface ProjectMember {
  userId: string
  email: string
  fullName: string
  avatarUrl: string | null
  addedAt: IsoDateTime
}

// ---------- Tasks ----------
export interface Task {
  id: string
  projectId: string
  number: number
  code: string
  type: WorkItemType
  parentId: string | null
  title: string
  status: WorkItemStatus
  priority: Priority
  assigneeId: string | null
  assigneeName: string | null
  estimateHours: number | null
  dueDate: DateOnly | null
  createdAt: IsoDateTime
}

export interface TaskDetail {
  id: string
  projectId: string
  number: number
  code: string
  type: WorkItemType
  parentId: string | null
  title: string
  description: string | null
  status: WorkItemStatus
  priority: Priority
  reporterId: string
  reporterName: string
  assigneeId: string | null
  assigneeName: string | null
  reviewerId: string | null
  estimateHours: number | null
  dueDate: DateOnly | null
  completedAt: IsoDateTime | null
  createdAt: IsoDateTime
  allowedTransitions: WorkItemStatus[]
}

export interface CreatedTask {
  id: string
  number: number
  code: string
}

export interface TaskHistoryEntry {
  field: WorkItemField
  oldValue: string | null
  newValue: string | null
  changedById: string
  changedByName: string
  changedAt: IsoDateTime
}

export interface KanbanColumn {
  status: WorkItemStatus
  totalCount: number
  cards: Task[]
}

export interface KanbanBoard {
  projectId: string
  columns: KanbanColumn[]
}

export interface TaskComment {
  id: string
  authorId: string
  authorName: string
  authorAvatarUrl: string | null
  body: string
  createdAt: IsoDateTime
  editedAt: IsoDateTime | null
}

// ---------- Notifications ----------
export interface AppNotification {
  id: string
  type: NotificationType
  title: string
  message: string
  teamId: string | null
  projectId: string | null
  workItemId: string | null
  bugId: string | null
  isRead: boolean
  createdAt: IsoDateTime
}

// ---------- Requests ----------
export interface CreateTeamRequest {
  name: string
  code: string
  description: string | null
  leadUserId: string
}
export interface UpdateTeamRequest {
  name: string
  description: string | null
}
export interface AddTeamMemberRequest {
  email: string
  role: TeamRole
  weeklyCapacityHours: number | null
}
export interface CreateProjectRequest {
  name: string
  key: string
  description: string | null
  startDate: DateOnly | null
  endDate: DateOnly | null
  useSprints: boolean
}
export interface UpdateProjectRequest {
  name: string
  description: string | null
  startDate: DateOnly | null
  endDate: DateOnly | null
  useSprints: boolean
}
export interface CreateTaskRequest {
  type: WorkItemType
  title: string
  description: string | null
  priority: Priority
  assigneeId: string | null
  parentId: string | null
  estimateHours: number | null
  dueDate: DateOnly | null
}
export interface UpdateTaskRequest {
  title: string
  description: string | null
  priority: Priority
  estimateHours: number | null
  dueDate: DateOnly | null
}

// ---------- Bugs ----------
export interface Bug {
  id: string
  projectId: string
  number: number
  code: string
  title: string
  severity: BugSeverity
  priority: Priority
  status: BugStatus
  environment: BugEnvironment
  assigneeId: string | null
  assigneeName: string | null
  slaDueAt: IsoDateTime | null
  createdAt: IsoDateTime
}

export interface BugDetail {
  id: string
  projectId: string
  number: number
  code: string
  title: string
  severity: BugSeverity
  priority: Priority
  status: BugStatus
  environment: BugEnvironment
  affectedVersion: string | null
  fixedInVersion: string | null
  stepsToReproduce: string
  expectedResult: string
  actualResult: string
  deviceInfo: string | null
  reporterId: string
  reporterName: string
  assigneeId: string | null
  assigneeName: string | null
  linkedWorkItemId: string | null
  duplicateOfBugId: string | null
  slaDueAt: IsoDateTime | null
  resolvedAt: IsoDateTime | null
  closedAt: IsoDateTime | null
  reopenCount: number
  createdAt: IsoDateTime
  /** Các trạng thái người xem được phép chuyển sang (đã tính theo workflow và vai trò của họ). */
  allowedTransitions: BugStatus[]
}

export interface CreatedBug {
  id: string
  number: number
  code: string
}

export interface BugStatusHistoryEntry {
  fromStatus: BugStatus | null
  toStatus: BugStatus
  changedById: string
  changedByName: string
  changedAt: IsoDateTime
  note: string | null
}

/** Body dùng chung cho tạo và sửa bug. */
export interface BugRequest {
  title: string
  severity: BugSeverity
  priority: Priority
  environment: BugEnvironment
  affectedVersion: string | null
  stepsToReproduce: string
  expectedResult: string
  actualResult: string
  deviceInfo: string | null
}

export interface NotificationPreference {
  type: NotificationType
  inAppEnabled: boolean
  pushEnabled: boolean
}
