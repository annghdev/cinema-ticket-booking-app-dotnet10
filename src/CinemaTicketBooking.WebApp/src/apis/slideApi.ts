import { httpClient } from "./httpClient"
import { type Slide } from "../types/Slide"

export const getActiveSlides = async (): Promise<Slide[]> => {
  const response = await httpClient.get<Slide[]>("/api/slides")
  return response.data
}
