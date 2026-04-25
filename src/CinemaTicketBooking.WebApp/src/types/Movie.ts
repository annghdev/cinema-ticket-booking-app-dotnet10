import { normalizeEnumValue } from "./Common"

export type ApiMovieStatus = "Upcoming" | "NowShowing" | "NoShow"
export type ApiMovieGenre =
  | "Action"
  | "Comedy"
  | "Drama"
  | "Horror"
  | "SciFi"
  | "Romance"
  | "Thriller"
  | "Animation"
  | "Documentary"

export type MovieStatusValue = ApiMovieStatus | number
export type MovieGenreValue = ApiMovieGenre | number

export type MovieDtoRaw = {
  id: string
  name: string
  description: string
  thumbnailUrl: string
  studio: string
  director: string
  officialTrailerUrl: string | null
  duration: number
  genre: MovieGenreValue
  status: MovieStatusValue
  createdAt: string
}

export type MovieDto = Omit<MovieDtoRaw, "genre" | "status"> & {
  genre: ApiMovieGenre
  status: ApiMovieStatus
}

export const movieStatusFromNumber: Record<number, ApiMovieStatus> = {
  0: "Upcoming",
  1: "NowShowing",
  2: "NoShow",
}

export const movieGenreFromNumber: Record<number, ApiMovieGenre> = {
  0: "Action",
  1: "Comedy",
  2: "Drama",
  3: "Horror",
  4: "SciFi",
  5: "Romance",
  6: "Thriller",
  7: "Animation",
  8: "Documentary",
}

export function normalizeMovieDto(raw: MovieDtoRaw): MovieDto {
  return {
    ...raw,
    genre: normalizeEnumValue(raw.genre, movieGenreFromNumber, "Action"),
    status: normalizeEnumValue(raw.status, movieStatusFromNumber, "Upcoming"),
  }
}
