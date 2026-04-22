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
