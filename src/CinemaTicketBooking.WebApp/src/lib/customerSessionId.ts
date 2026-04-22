const STORAGE_KEY = "cinemaTicketBooking.customerSessionId"

const PREFIX = "web-"

function newSessionId(): string {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
    return `${PREFIX}${crypto.randomUUID()}`
  }
  return `${PREFIX}${Date.now()}-${Math.random().toString(36).slice(2, 12)}`
}

/**
 * Stable anonymous session id (guest or pre-login) for lock/release and booking.
 * Persists across tab close, refresh, and SPA navigation. Align with server MaxLength 128.
 */
export function getOrCreateCustomerSessionId(): string {
  if (typeof window === "undefined") {
    return newSessionId()
  }

  try {
    const existing = window.localStorage.getItem(STORAGE_KEY)
    if (existing && existing.length > 0 && existing.length <= 128) {
      return existing
    }
    const created = newSessionId()
    window.localStorage.setItem(STORAGE_KEY, created)
    return created
  } catch {
    return newSessionId()
  }
}
