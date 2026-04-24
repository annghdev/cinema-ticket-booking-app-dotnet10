import axios from "axios"
import { readAuthState } from "../lib/authSession"

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ""

export const httpClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 100000,
})

httpClient.interceptors.request.use((config) => {
  const accessToken = readAuthState()?.session.accessToken
  if (accessToken) {
    config.headers = config.headers ?? {}
    if (!config.headers.Authorization) {
      config.headers.Authorization = `Bearer ${accessToken}`
    }
  }
  return config
})
