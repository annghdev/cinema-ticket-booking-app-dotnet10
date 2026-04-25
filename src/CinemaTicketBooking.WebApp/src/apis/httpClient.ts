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

type GlobalErrorHandler = (msg: string) => void
let onGlobalError: GlobalErrorHandler | null = null

export function registerGlobalErrorHandler(handler: GlobalErrorHandler) {
  onGlobalError = handler
}

httpClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (!error.response) {
      if (onGlobalError) {
        onGlobalError("Không thể kết nối đến máy chủ. Vui lòng kiểm tra đường truyền.")
      }
    } else if (error.response.status >= 500) {
      if (onGlobalError) {
        onGlobalError("Lỗi hệ thống. Vui lòng thử lại sau.")
      }
    }
    return Promise.reject(error)
  }
)
