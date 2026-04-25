import { httpClient } from "./httpClient"
import { type MovieDto, type MovieDtoRaw, normalizeMovieDto } from "../types/Movie"

export async function getUpcomingAndNowShowingMovies(): Promise<MovieDto[]> {
  const response = await httpClient.get<MovieDtoRaw[]>("/api/movies/upcoming-now-showing")
  return response.data.map(normalizeMovieDto)
}
