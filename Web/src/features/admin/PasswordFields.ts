import { z } from 'zod'

/**
 * Chính sách mật khẩu do Identity quyết định (appsettings Identity:Password: >=8, có số, chữ hoa, chữ thường).
 * Client chỉ chặn rỗng/quá dài và hiện gợi ý; lỗi chính sách từ server được hiển thị nguyên văn.
 */
export const passwordField = z.string().min(1, 'validation.required').max(128, 'validation.tooLong')
export const PASSWORD_POLICY_HINT_KEY = 'users.passwordHint'

export const passwordsMatch = <T extends { newPassword: string; confirmPassword: string }>(v: T): boolean =>
  v.newPassword === v.confirmPassword
