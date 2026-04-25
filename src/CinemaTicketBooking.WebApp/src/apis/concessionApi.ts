import { httpClient } from "./httpClient"
import { type ConcessionDto, type ConcessionDtoRaw, normalizeConcession } from "../types/Concession"

/**
 * All concessions (client should filter to available when displaying).
 */
export async function getConcessions(): Promise<ConcessionDto[]> {
  const response = await httpClient.get<ConcessionDtoRaw[]>("/api/concessions")
  return response.data.map(normalizeConcession)
}
