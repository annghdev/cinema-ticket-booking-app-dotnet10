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
export type ApiShowTimeStatus = "Upcoming" | "Showing" | "Completed" | "Cancelled"
export type ApiTicketStatus = "Available" | "Locking" | "PendingPayment" | "Sold"

type MovieStatusValue = ApiMovieStatus | number
type MovieGenreValue = ApiMovieGenre | number
type ShowTimeStatusValue = ApiShowTimeStatus | number
type TicketStatusValue = ApiTicketStatus | number

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

export type ShowTimeDtoRaw = {
  id: string
  movieId: string
  movieName: string
  screenId: string
  screenCode: string
  cinemaId: string
  cinemaName: string
  date: string
  startAt: string
  endAt: string
  status: ShowTimeStatusValue
  ticketCount: number
  availableTicketCount: number
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
}

export type ShowTimeTicketDto = Omit<ShowTimeTicketDtoRaw, "status"> & {
  status: ApiTicketStatus
}

export type ShowTimeDetailDtoRaw = {
  id: string
  movieId: string
  movieName: string
  screenId: string
  screenCode: string
  cinemaId: string
  cinemaName: string
  seatMap: string
  date: string
  startAt: string
  endAt: string
  status: ShowTimeStatusValue
  ticketCount: number
  availableTicketCount: number
  createdAt: string
  tickets: ShowTimeTicketDtoRaw[]
}

export type ShowTimeDetailDto = Omit<ShowTimeDetailDtoRaw, "status" | "tickets"> & {
  status: ApiShowTimeStatus
  tickets: ShowTimeTicketDto[]
}

const movieStatusFromNumber: Record<number, ApiMovieStatus> = {
  0: "Upcoming",
  1: "NowShowing",
  2: "NoShow",
}

const movieGenreFromNumber: Record<number, ApiMovieGenre> = {
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

const showTimeStatusFromNumber: Record<number, ApiShowTimeStatus> = {
  0: "Upcoming",
  1: "Showing",
  2: "Completed",
  3: "Cancelled",
}

const ticketStatusFromNumber: Record<number, ApiTicketStatus> = {
  0: "Available",
  1: "Locking",
  2: "PendingPayment",
  3: "Sold",
}

function normalizeEnumValue<T extends string>(value: T | number, numberMap: Record<number, T>, fallback: T): T {
  if (typeof value === "string") {
    return value
  }

  return numberMap[value] ?? fallback
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

export function normalizeMovieDto(raw: MovieDtoRaw): MovieDto {
  return {
    ...raw,
    genre: normalizeEnumValue(raw.genre, movieGenreFromNumber, "Action"),
    status: normalizeEnumValue(raw.status, movieStatusFromNumber, "Upcoming"),
  }
}

export function normalizeShowTimeDto(raw: ShowTimeDtoRaw): ShowTimeDto {
  return {
    ...raw,
    status: normalizeEnumValue(raw.status, showTimeStatusFromNumber, "Upcoming"),
  }
}

export function normalizeShowTimeTicketDto(raw: ShowTimeTicketDtoRaw): ShowTimeTicketDto {
  return {
    ...raw,
    code: extractSeatCodeFromTicketCode(raw.code),
    status: normalizeTicketStatus(raw.status),
  }
}

export function normalizeShowTimeDetailDto(raw: ShowTimeDetailDtoRaw): ShowTimeDetailDto {
  return {
    ...raw,
    status: normalizeEnumValue(raw.status, showTimeStatusFromNumber, "Upcoming"),
    tickets: raw.tickets.map(normalizeShowTimeTicketDto),
  }
}
