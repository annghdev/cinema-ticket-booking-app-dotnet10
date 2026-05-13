import { normalizeEnumValue } from "./Common"

export type ApiShowTimeStatus = "Upcoming" | "Showing" | "Completed" | "Cancelled"
export type ApiTicketStatus = "Available" | "Locking" | "PendingPayment" | "Sold"

export type ShowTimeStatusValue = ApiShowTimeStatus | number
export type TicketStatusValue = ApiTicketStatus | number

export type ShowTimeDtoRaw = {
  id: string
  movieId: string
  movieName: string
  screenId: string
  screenCode: string
  cinemaId: string
  cinemaName: string
  cinemaAddress: string
  movieThumbnailUrl: string
  movieGenre: string
  movieDuration: number
  date: string
  startAt: string
  endAt: string
  status: ShowTimeStatusValue
  ticketCount: number
  availableTicketCount: number
  format: string | number
  createdAt: string
}

export type ShowTimeDto = Omit<ShowTimeDtoRaw, "status"> & {
  status: ApiShowTimeStatus
}

export type ShowTimeTicketDtoRaw = {
  id: string
  code: string
  price: number
  status: TicketStatusValue
  /** Opaque id when status is Locking (server stores client session id). */
  lockingBy?: string | null
}

export type ShowTimeTicketDto = Omit<ShowTimeTicketDtoRaw, "status"> & {
  status: ApiTicketStatus
  lockingBy: string | null
}

export type ShowTimeDetailDtoRaw = {
  id: string
  movieId: string
  movieName: string
  screenId: string
  screenCode: string
  cinemaId: string
  cinemaName: string
  cinemaAddress: string
  movieThumbnailUrl: string
  movieGenre: string
  movieDuration: number
  seatMap: string
  date: string
  startAt: string
  endAt: string
  status: ShowTimeStatusValue
  ticketCount: number
  availableTicketCount: number
  format: string | number
  createdAt: string
  tickets: ShowTimeTicketDtoRaw[]
}

export type ShowTimeDetailDto = Omit<ShowTimeDetailDtoRaw, "status" | "tickets"> & {
  status: ApiShowTimeStatus
  tickets: ShowTimeTicketDto[]
}

export const showTimeStatusFromNumber: Record<number, ApiShowTimeStatus> = {
  0: "Upcoming",
  1: "Showing",
  2: "Completed",
  3: "Cancelled",
}

export const ticketStatusFromNumber: Record<number, ApiTicketStatus> = {
  0: "Available",
  1: "Locking",
  2: "PendingPayment",
  3: "Sold",
}

export const screenTypeFromNumber: Record<number, string> = {
  0: "2D",
  1: "3D",
  2: "IMAX",
}

function extractSeatCodeFromTicketCode(ticketCode: string): string {
  if (!ticketCode) {
    return ticketCode
  }

  const segments = ticketCode.split("-")
  if (segments.length >= 3) {
    return segments[segments.length - 1]
  }

  return ticketCode
}

export function normalizeTicketStatus(value: TicketStatusValue): ApiTicketStatus {
  return normalizeEnumValue(value, ticketStatusFromNumber, "Available")
}

export function normalizeShowTimeDto(raw: ShowTimeDtoRaw): ShowTimeDto {
  return {
    ...raw,
    status: normalizeEnumValue(raw.status, showTimeStatusFromNumber, "Upcoming"),
    format: typeof raw.format === "number" ? (screenTypeFromNumber[raw.format] || "2D") : raw.format,
  }
}

export function normalizeShowTimeTicketDto(raw: ShowTimeTicketDtoRaw): ShowTimeTicketDto {
  return {
    ...raw,
    code: extractSeatCodeFromTicketCode(raw.code),
    status: normalizeTicketStatus(raw.status),
    lockingBy: raw.lockingBy ?? null,
  }
}

export function normalizeShowTimeDetailDto(raw: ShowTimeDetailDtoRaw): ShowTimeDetailDto {
  return {
    ...raw,
    status: normalizeEnumValue(raw.status, showTimeStatusFromNumber, "Upcoming"),
    format: typeof raw.format === "number" ? (screenTypeFromNumber[raw.format] || "2D") : raw.format,
    tickets: raw.tickets.map(normalizeShowTimeTicketDto),
  }
}

export type ValidateSeatSelectionRequest = {
  selectedTicketIds: string[]
  customerSessionId: string
}

export type TicketStatusChangedRealtimeEventRaw = {
  showTimeId: string
  ticketId: string
  ticketCode: string
  status: TicketStatusValue
  occurredAtUtc: string
  lockingBy?: string | null
}

export type TicketStatusChangedRealtimeEvent = Omit<TicketStatusChangedRealtimeEventRaw, "status"> & {
  status: ApiTicketStatus
  lockingBy: string | null
}
