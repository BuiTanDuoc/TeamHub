import { useAuthStore } from './authStore'
import type { TeamRole } from '../api/types'

const RANK: Record<TeamRole, number> = { Viewer: 0, Tester: 1, Developer: 2, TeamLead: 3 }

export const roleAtLeast = (role: TeamRole | undefined, min: TeamRole): boolean =>
  role !== undefined && RANK[role] >= RANK[min]

export interface TeamPermissions {
  role: TeamRole | undefined
  isSystemAdmin: boolean
  /** Tạo/sửa/đổi trạng thái/giao task, bình luận — từ Tester trở lên (CLAUDE.md §14). */
  canContribute: boolean
  /** Quản lý team/dự án, xóa task — TeamLead (SystemAdmin luôn qua được kiểm tra của backend). */
  canManage: boolean
}

/** Quyền của user hiện tại trong một team. Chỉ để ẩn/hiện UI; backend (AuthorizationBehavior) mới là nơi quyết định. */
export const useTeamPermissions = (teamId: string | undefined): TeamPermissions => {
  const user = useAuthStore((s) => s.user)
  const role = useAuthStore((s) => s.teams.find((t) => t.teamId === teamId)?.role)
  const isSystemAdmin = user?.isSystemAdmin ?? false
  return {
    role,
    isSystemAdmin,
    canContribute: isSystemAdmin || roleAtLeast(role, 'Tester'),
    canManage: isSystemAdmin || roleAtLeast(role, 'TeamLead'),
  }
}
