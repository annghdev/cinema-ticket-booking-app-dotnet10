import { httpClient } from "./httpClient"

export type ConcessionDto = {
  id: string
  name: string
  price: number
  imageUrl: string
  isAvailable: boolean
  createdAt: string
}

type ConcessionDtoRaw = {
  id: string
  name: string
  price: number
  imageUrl: string
  isAvailable: boolean
  createdAt: string
}

function normalizeConcession(r: ConcessionDtoRaw): ConcessionDto {
  return {
    id: r.id,
    name: r.name,
    price: r.price,
    imageUrl: r.imageUrl,
    isAvailable: r.isAvailable,
    createdAt: r.createdAt,
  }
}

/**
 * All concessions (client should filter to available when displaying).
 */
export async function getConcessions(): Promise<ConcessionDto[]> {
  const response = await httpClient.get<ConcessionDtoRaw[]>("/api/concessions")
  return response.data.map(normalizeConcession)
}
