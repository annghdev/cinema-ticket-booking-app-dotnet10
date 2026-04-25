export type ConcessionDtoRaw = {
  id: string
  name: string
  description: string
  price: number
  imageUrl: string | null
  isAvailable: boolean
  createdAt: string
}

export type ConcessionDto = {
  id: string
  name: string
  description: string
  price: number
  imageUrl: string | null
  isAvailable: boolean
  createdAt: string
}

export function normalizeConcession(r: ConcessionDtoRaw): ConcessionDto {
  return {
    id: r.id,
    name: r.name,
    description: r.description,
    price: r.price,
    imageUrl: r.imageUrl,
    isAvailable: r.isAvailable,
    createdAt: r.createdAt,
  }
}
