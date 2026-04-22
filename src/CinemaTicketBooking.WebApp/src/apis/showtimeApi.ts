import { isAxiosError } from "axios"
import { httpClient } from "./httpClient"
import {
  type ShowTimeDetailDto,
  type ShowTimeDetailDtoRaw,
  type ShowTimeDto,
  type ShowTimeDtoRaw,
  normalizeShowTimeDetailDto,
  normalizeShowTimeDto,
} from "../types/contracts"

type GetShowTimesParams = {
  movieId?: string
  cinemaId?: string
  screenId?: string
  status?: "Upcoming" | "Showing" | "Completed" | "Cancelled"
  date?: string
}

export async function getShowTimes(params?: GetShowTimesParams): Promise<ShowTimeDto[]> {
  const response = await httpClient.get<ShowTimeDtoRaw[]>("/api/showtimes", { params })
  return response.data.map(normalizeShowTimeDto)
}

export async function getShowTimeById(showtimeId: string): Promise<ShowTimeDetailDto> {
  const response = await httpClient.get<ShowTimeDetailDtoRaw>(`/api/showtimes/${showtimeId}`)
  return normalizeShowTimeDetailDto(response.data)
}

export async function lockTicket(showtimeId: string, ticketId: string, lockBy: string): Promise<void> {
  await httpClient.post(`/api/showtimes/${showtimeId}/tickets/${ticketId}/lock`, { lockBy })
}

export async function releaseTicket(showtimeId: string, ticketId: string, releaseBy: string): Promise<void> {
  await httpClient.post(`/api/showtimes/${showtimeId}/tickets/${ticketId}/release`, { releaseBy })
}

export type ValidateSeatSelectionRequest = {
  selectedTicketIds: string[]
  customerSessionId: string
}

type SeatSelectionViolation = {
  type?: string
  severity?: string
  message?: string
  affectedSeats?: string[]
  blockCheckout?: boolean
}

type ValidateSeatSelectionErrorBody = {
  canProceed?: boolean
  errors?: SeatSelectionViolation[]
  warnings?: SeatSelectionViolation[]
  hints?: string[]
}

/**
 * Pre-checkout seat policy validation. Throws Error with a user-facing message on failure.
 */
export async function validateSeatSelection(
  showtimeId: string,
  payload: ValidateSeatSelectionRequest,
): Promise<void> {
  try {
    await httpClient.post<unknown>(`/api/showtimes/${showtimeId}/validate-seat-selection`, {
      selectedTicketIds: payload.selectedTicketIds,
      customerSessionId: payload.customerSessionId,
    })
  } catch (e) {
    if (isAxiosError(e) && e.response?.data && typeof e.response.data === "object") {
      const d = e.response.data as ValidateSeatSelectionErrorBody
      const fromViolations = (list: SeatSelectionViolation[] | undefined) =>
        (list ?? [])
          .map((v) => v.message)
          .filter((m): m is string => Boolean(m && m.length > 0))
      const parts = [...fromViolations(d.errors), ...fromViolations(d.warnings)]
      if (parts.length > 0) {
        throw new Error(parts.join(" "))
      }
    }
    throw new Error("Không thể xác nhận lựa chọn ghế. Vui lòng thử lại.")
  }
}
