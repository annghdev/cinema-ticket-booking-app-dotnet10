import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr"
import {
  type PaymentConfirmedRealtimeEvent,
  type PaymentConfirmedRealtimeEventRaw,
} from "../types/Payment"

const PAYMENT_CONFIRMED_EVENT = "payment-confirmed"

function resolveHubUrl() {
  const configuredBase = import.meta.env.VITE_API_BASE_URL
  const fallbackBase = typeof window !== "undefined" ? window.location.origin : "http://localhost:5000"

  try {
    return new URL("/hubs/payments", configuredBase || fallbackBase).toString()
  } catch {
    return `${fallbackBase}/hubs/payments`
  }
}

export async function connectPaymentHub(
  bookingId: string,
  onConfirmed: (event: PaymentConfirmedRealtimeEvent) => void,
): Promise<HubConnection> {
  const connection = new HubConnectionBuilder()
    .withUrl(resolveHubUrl(), {
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on(PAYMENT_CONFIRMED_EVENT, (payload: PaymentConfirmedRealtimeEventRaw) => {
    onConfirmed(payload)
  })

  await connection.start()
  await connection.invoke("SubscribeBooking", bookingId)

  return connection
}
