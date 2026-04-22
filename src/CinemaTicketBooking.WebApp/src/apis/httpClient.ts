import axios from "axios"

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ""

export const httpClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10000,
})
