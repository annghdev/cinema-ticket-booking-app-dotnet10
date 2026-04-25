import type { AuthTokenResponse } from "../apis/authApi"

export type StoredAuthSession = {
  accessToken: string
  accessTokenExpiresAtUtc: string
  accountId: string
  customerId: string | null
  refreshToken: string | null
}

export type StoredAuthProfile = {
  displayName: string
  email: string | null
  avatarUrl: string | null
  phoneNumber?: string | null
}

export type StoredAuthState = {
  session: StoredAuthSession
  profile: StoredAuthProfile
}

export type PersistAuthProfileInput = {
  displayName?: string | null
  email?: string | null
  avatarUrl?: string | null
  customerId?: string | null
  phoneNumber?: string | null
}

export const AUTH_SESSION_KEY = "ctb.auth.session"
export const AUTH_PROFILE_KEY = "ctb.auth.profile"
export const AUTH_STATE_CHANGED_EVENT = "ctb:auth-state-changed"

function decodeBase64Url(input: string): string | null {
  try {
    const normalized = input.replace(/-/g, "+").replace(/_/g, "/")
    const pad = normalized.length % 4 === 0 ? "" : "=".repeat(4 - (normalized.length % 4))
    return window.atob(normalized + pad)
  } catch {
    return null
  }
}

function parseJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split(".")
  if (parts.length < 2) {
    return null
  }
  const decoded = decodeBase64Url(parts[1] ?? "")
  if (!decoded) {
    return null
  }
  try {
    return JSON.parse(decoded) as Record<string, unknown>
  } catch {
    return null
  }
}

function getClaim(payload: Record<string, unknown> | null, ...keys: string[]): string | null {
  if (!payload) {
    return null
  }
  for (const key of keys) {
    const raw = payload[key]
    if (typeof raw === "string" && raw.trim().length > 0) {
      return raw.trim()
    }
  }
  return null
}

function deriveProfile(tokens: AuthTokenResponse, input?: PersistAuthProfileInput): StoredAuthProfile {
  const jwtPayload = parseJwtPayload(tokens.accessToken)
  const displayName =
    input?.displayName?.trim() ||
    getClaim(jwtPayload, "name", "given_name", "unique_name") ||
    input?.email?.trim() ||
    getClaim(jwtPayload, "email") ||
    "Khách hàng"

  const email = input?.email?.trim() || getClaim(jwtPayload, "email")
  const avatarUrl = input?.avatarUrl?.trim() || getClaim(jwtPayload, "picture")

  return {
    displayName,
    email: email ?? null,
    avatarUrl: avatarUrl ?? null,
    phoneNumber: input?.phoneNumber ?? null,
  }
}

function deriveCustomerId(accessToken: string): string | null {
  const jwtPayload = parseJwtPayload(accessToken)
  return getClaim(jwtPayload, "customer_id", "customerId")
}

function safeParseJson<T>(raw: string | null): T | null {
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as T
  } catch {
    return null
  }
}

function notifyAuthStateChanged() {
  window.dispatchEvent(new Event(AUTH_STATE_CHANGED_EVENT))
}

export function persistAuthState(tokens: AuthTokenResponse, profileInput?: PersistAuthProfileInput): StoredAuthState {
  const session: StoredAuthSession = {
    accessToken: tokens.accessToken,
    accessTokenExpiresAtUtc: tokens.accessTokenExpiresAtUtc,
    accountId: tokens.accountId,
    customerId: profileInput?.customerId?.trim() || deriveCustomerId(tokens.accessToken),
    refreshToken: tokens.refreshToken,
  }
  const profile = deriveProfile(tokens, profileInput)

  window.localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session))
  window.localStorage.setItem(AUTH_PROFILE_KEY, JSON.stringify(profile))
  notifyAuthStateChanged()

  return { session, profile }
}

export function readAuthState(): StoredAuthState | null {
  const rawSession = safeParseJson<StoredAuthSession>(window.localStorage.getItem(AUTH_SESSION_KEY))
  if (!rawSession) {
    return null
  }
  const normalizedSession: StoredAuthSession = {
    ...rawSession,
    customerId: rawSession.customerId ?? deriveCustomerId(rawSession.accessToken),
  }
  const profile = safeParseJson<StoredAuthProfile>(window.localStorage.getItem(AUTH_PROFILE_KEY))
  window.localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(normalizedSession))
  if (profile) {
    return { session: normalizedSession, profile }
  }
  const reconstructedProfile = deriveProfile(
    {
      accessToken: normalizedSession.accessToken,
      accessTokenExpiresAtUtc: normalizedSession.accessTokenExpiresAtUtc,
      accountId: normalizedSession.accountId,
      refreshToken: normalizedSession.refreshToken,
    },
    undefined,
  )
  window.localStorage.setItem(AUTH_PROFILE_KEY, JSON.stringify(reconstructedProfile))
  return { session: normalizedSession, profile: reconstructedProfile }
}

export function clearAuthState() {
  window.localStorage.removeItem(AUTH_SESSION_KEY)
  window.localStorage.removeItem(AUTH_PROFILE_KEY)
  notifyAuthStateChanged()
}
