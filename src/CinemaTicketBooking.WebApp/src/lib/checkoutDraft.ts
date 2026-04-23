const PREFIX = "cinemaTicketBooking.checkoutDraft"

export type CheckoutDraft = {
  selectedTicketIds: string[]
}

/**
 * Prevents losing selected ticket ids on checkout refresh (sessionStorage is same-tab only).
 */
export function saveCheckoutDraft(showtimeId: string, data: CheckoutDraft) {
  try {
    window.sessionStorage.setItem(`${PREFIX}.${showtimeId}`, JSON.stringify({ ...data, at: Date.now() }))
  } catch {
    // ignore
  }
}

export function loadCheckoutDraft(showtimeId: string): CheckoutDraft | null {
  try {
    const raw = window.sessionStorage.getItem(`${PREFIX}.${showtimeId}`)
    if (!raw) {
      return null
    }
    const p = JSON.parse(raw) as { selectedTicketIds?: string[] }
    if (!p.selectedTicketIds || p.selectedTicketIds.length === 0) {
      return null
    }
    return { selectedTicketIds: p.selectedTicketIds }
  } catch {
    return null
  }
}

export function clearCheckoutDraft(showtimeId: string) {
  try {
    window.sessionStorage.removeItem(`${PREFIX}.${showtimeId}`)
  } catch {
    // ignore
  }
}
