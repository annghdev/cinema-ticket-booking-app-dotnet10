import { httpClient } from "./httpClient"
import { type CinemaDto, type CinemaDtoRaw, normalizeCinemaDto } from "../types/Cinema"

export async function getCinemas(): Promise<CinemaDto[]> {
  const response = await httpClient.get<CinemaDtoRaw[]>("/api/cinemas")
  return response.data.map(normalizeCinemaDto)
}
