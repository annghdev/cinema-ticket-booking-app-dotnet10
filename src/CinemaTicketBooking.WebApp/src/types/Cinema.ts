export type CinemaDtoRaw = {
  id: string
  name: string
  thumbnailUrl: string
  geo?: string
  address: string
  isActive: boolean
  createdAt: string
}

export type CinemaDto = CinemaDtoRaw

export function normalizeCinemaDto(raw: CinemaDtoRaw): CinemaDto {
  return raw
}
