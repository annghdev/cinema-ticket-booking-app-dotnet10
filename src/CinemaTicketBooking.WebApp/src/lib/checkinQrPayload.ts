const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

/**
 * Plain text to encode in the check-in QR. Always use the booking Guid when available.
 * Legacy API values were accidentally stored as `data:image/png;base64,...` (double-encoding); those
 * must not be the QR payload — scanners should read the same id as the booking.
 */
export function getCheckinQrPayload(
  bookingId: string | null | undefined,
  checkinFieldFromApi: string | null | undefined,
): string {
  if (bookingId && UUID_RE.test(bookingId.trim())) {
    return bookingId.trim()
  }
  if (checkinFieldFromApi && /^\s*data:/i.test(checkinFieldFromApi)) {
    return bookingId?.trim() ?? ""
  }
  if (checkinFieldFromApi?.trim()) {
    return checkinFieldFromApi.trim()
  }
  return bookingId?.trim() ?? ""
}
