import { httpClient } from "./httpClient"
import { type PaymentRedirectBehavior } from "./paymentApi"

export type CreateBookingRequest = {
  showTimeId: string
  customerSessionId: string
  customerName: string
  customerEmail: string
  customerPhoneNumber: string
  selectedTicketIds: string[]
  concessions: { concessionId: string; quantity: number }[]
  discountAmount: number
  paymentMethod: string
  returnUrl: string
  ipAddress: string
}

export type CreateBookingResponse = {
  bookingId: string
  paymentExpiresAt: string
  originAmount: number
  finalAmount: number
  paymentStatus: string
  paymentUrl: string | null
  redirectBehavior: PaymentRedirectBehavior | null
  paymentTransactionId: string | null
  gatewayTransactionId: string | null
}

type CreateBookingResponseRaw = {
  bookingId: string
  paymentExpiresAt: string
  originAmount: number
  finalAmount: number
  paymentStatus: string
  paymentUrl: string | null
  redirectBehavior: PaymentRedirectBehavior | null
  paymentTransactionId: string | null
  gatewayTransactionId: string | null
}

function normalizeResponse(r: CreateBookingResponseRaw): CreateBookingResponse {
  return {
    bookingId: r.bookingId,
    paymentExpiresAt: r.paymentExpiresAt,
    originAmount: r.originAmount,
    finalAmount: r.finalAmount,
    paymentStatus: r.paymentStatus,
    paymentUrl: r.paymentUrl,
    redirectBehavior: r.redirectBehavior,
    paymentTransactionId: r.paymentTransactionId,
    gatewayTransactionId: r.gatewayTransactionId,
  }
}

export async function createBooking(body: CreateBookingRequest): Promise<CreateBookingResponse> {
  const response = await httpClient.post<CreateBookingResponseRaw>("/api/bookings", {
    showTimeId: body.showTimeId,
    customerSessionId: body.customerSessionId,
    customerName: body.customerName,
    customerEmail: body.customerEmail,
    customerPhoneNumber: body.customerPhoneNumber,
    selectedTicketIds: body.selectedTicketIds,
    concessions: body.concessions,
    discountAmount: body.discountAmount,
    paymentMethod: body.paymentMethod,
    returnUrl: body.returnUrl,
    ipAddress: body.ipAddress,
  })
  return normalizeResponse(response.data)
}

export type BookingDetailsDto = {
  bookingId: string
  showTimeInfo: { screen: string; movie: string; startAt: string; endAt: string }
  originalAmount: number
  discountAmount: number
  finalAmount: number
  checkinQrCode: string
  status: number | string
  createdAt: string
  tickets: { seatCode: string; price: number }[]
  concessions: { name: string; imageUrl: string; price: number; quantity: number; amount: number }[]
}

export async function getBookingById(bookingId: string): Promise<BookingDetailsDto> {
  const response = await httpClient.get<BookingDetailsDto>(`/api/bookings/${bookingId}`)
  return response.data
}
