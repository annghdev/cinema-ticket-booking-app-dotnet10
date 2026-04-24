import { httpClient } from "./httpClient"

export type AuthTokenResponse = {
  accessToken: string
  accessTokenExpiresAtUtc: string
  accountId: string
  refreshToken: string | null
}

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = {
  email: string
  password: string
  name: string
  phoneNumber: string
  sessionId?: string | null
}

export type ForgotPasswordRequest = {
  email: string
}

export type AuthProfileResponse = {
  accountId: string
  customerId: string | null
  displayName: string
  email: string | null
  avatarUrl: string | null
}

export async function login(body: LoginRequest): Promise<AuthTokenResponse> {
  const response = await httpClient.post<AuthTokenResponse>("/api/auth/login", body, {
    withCredentials: true,
  })
  return response.data
}

export async function register(body: RegisterRequest): Promise<AuthTokenResponse> {
  const response = await httpClient.post<AuthTokenResponse>("/api/auth/register", body, {
    withCredentials: true,
  })
  return response.data
}

export async function forgotPassword(body: ForgotPasswordRequest): Promise<void> {
  await httpClient.post("/api/auth/forgot-password", body, {
    withCredentials: true,
  })
}

export async function logout(): Promise<void> {
  await httpClient.post("/api/auth/logout", undefined, {
    withCredentials: true,
  })
}

export async function getCurrentAuthProfile(): Promise<AuthProfileResponse> {
  const response = await httpClient.get<AuthProfileResponse>("/api/auth/me", {
    withCredentials: true,
  })
  return response.data
}
