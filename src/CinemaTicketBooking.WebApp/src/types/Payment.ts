import { type BookingDetailsDto } from "./Booking"

/** Matches CinemaTicketBooking.Domain.PaymentRedirectBehavior */
export type PaymentRedirectBehavior = "Redirect" | "QrCode" | 0 | 1

export type PaymentGatewayOptionDto = {
  method: string
  displayName: string
  redirectBehavior: PaymentRedirectBehavior
  icon: string
}

export type PaymentGatewayOptionDtoRaw = {
  method: string
  displayName: string
  redirectBehavior: PaymentRedirectBehavior
  icon: string
}

export function normalizeOption(r: PaymentGatewayOptionDtoRaw): PaymentGatewayOptionDto {
  return {
    method: r.method,
    displayName: r.displayName,
    redirectBehavior: r.redirectBehavior,
    icon: r.icon,
  }
}

export function isRedirectBehavior(value: PaymentRedirectBehavior | null | undefined): "Redirect" | "QrCode" {
  if (value === 0 || value === "Redirect") {
    return "Redirect"
  }
  return "QrCode"
}

/**
 * Response from VerifyPayment (fake-callback, real gateways, etc.)
 */
export type VerifyPaymentResponse = {
  bookingId: string
  paymentTransactionId: string
  isSuccess: boolean
  checkinQrCode: string | null
  status: string
  errorMessage: string | null
  canRetry: boolean
}

export type PaymentResultLookupResponse = {
  bookingId: string | null
  isSuccess: boolean
  status: string
  checkinQrCode: string | null
  booking: BookingDetailsDto | null
  errorMessage: string | null
  canRetry: boolean
}

export type PaymentConfirmedRealtimeEventRaw = {
  bookingId: string
  paymentTransactionId: string
  gatewayTransactionId: string
  status: string
  checkinQrCode: string | null
  occurredAtUtc: string
}

export type PaymentConfirmedRealtimeEvent = PaymentConfirmedRealtimeEventRaw
