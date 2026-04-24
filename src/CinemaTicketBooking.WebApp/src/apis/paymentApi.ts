import { httpClient } from "./httpClient"
import { type BookingDetailsDto } from "./bookingApi"

/** Matches CinemaTicketBooking.Domain.PaymentRedirectBehavior */
export type PaymentRedirectBehavior = "Redirect" | "QrCode" | 0 | 1

export type PaymentGatewayOptionDto = {
  method: string
  displayName: string
  redirectBehavior: PaymentRedirectBehavior
  icon: string
}

type PaymentGatewayOptionDtoRaw = {
  method: string
  displayName: string
  redirectBehavior: PaymentRedirectBehavior
  icon: string
}

function normalizeOption(r: PaymentGatewayOptionDtoRaw): PaymentGatewayOptionDto {
  return {
    method: r.method,
    displayName: r.displayName,
    redirectBehavior: r.redirectBehavior,
    icon: r.icon,
  }
}

export async function getAvailableGateways(): Promise<PaymentGatewayOptionDto[]> {
  const response = await httpClient.get<PaymentGatewayOptionDtoRaw[]>("/api/payments/gateways")
  return response.data.map(normalizeOption)
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

/**
 * Dev / fake gateway callback (see BookingCoreFlowRequests.http). Uses gateway transaction id.
 */
export async function getFakePaymentSuccess(
  bookingId: string,
  gatewayTransactionId: string,
): Promise<VerifyPaymentResponse> {
  const res = await httpClient.get<VerifyPaymentResponse>("/api/payments/fake-callback", {
    params: {
      bookingId,
      transactionId: gatewayTransactionId,
      vnp_ResponseCode: "00",
    },
  })
  return res.data
}

/**
 * Reads backend payment result for real gateway redirects (VNPay, etc).
 */
export async function getPaymentResultByTxn(
  bookingId: string | null,
  txnRef: string,
): Promise<PaymentResultLookupResponse> {
  const res = await httpClient.get<PaymentResultLookupResponse>("/api/payments/result", {
    params: bookingId ? { bookingId, txnRef } : { txnRef },
  })
  return res.data
}
