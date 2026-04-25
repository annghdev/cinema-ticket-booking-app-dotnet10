import { httpClient } from "./httpClient"
import {
  type PaymentGatewayOptionDto,
  type PaymentGatewayOptionDtoRaw,
  type VerifyPaymentResponse,
  type PaymentResultLookupResponse,
  normalizeOption,
} from "../types/Payment"

export async function getAvailableGateways(): Promise<PaymentGatewayOptionDto[]> {
  const response = await httpClient.get<PaymentGatewayOptionDtoRaw[]>("/api/payments/gateways")
  return response.data.map(normalizeOption)
}

export { isRedirectBehavior } from "../types/Payment"

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
