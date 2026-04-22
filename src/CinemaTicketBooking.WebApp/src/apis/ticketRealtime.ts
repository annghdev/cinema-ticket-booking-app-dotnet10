import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr"
import { normalizeTicketStatus, type ApiTicketStatus } from "../types/contracts"

type TicketStatusChangedRealtimeEventRaw = {
  showTimeId: string
  ticketId: string
  ticketCode: string
  status: ApiTicketStatus | number
  occurredAtUtc: string
  lockingBy?: string | null
}

export type TicketStatusChangedRealtimeEvent = Omit<TicketStatusChangedRealtimeEventRaw, "status"> & {
  status: ApiTicketStatus
  lockingBy: string | null
}

const TICKET_STATUS_CHANGED_EVENT = "ticket-status-changed"

function extractSeatCodeFromTicketCode(ticketCode: string): string {
  if (!ticketCode) {
    return ticketCode
  }

  const segments = ticketCode.split("-")
  if (segments.length >= 3) {
    return segments[segments.length - 1]
  }

  return ticketCode
}

function resolveHubUrl() {
  const configuredBase = import.meta.env.VITE_API_BASE_URL
  const fallbackBase = typeof window !== "undefined" ? window.location.origin : "http://localhost:5000"

  try {
    return new URL("/hubs/tickets", configuredBase || fallbackBase).toString()
  } catch {
    return `${fallbackBase}/hubs/tickets`
  }
}

export async function connectTicketStatusHub(
  showtimeId: string,
  onStatusChanged: (event: TicketStatusChangedRealtimeEvent) => void,
): Promise<HubConnection> {
  const connection = new HubConnectionBuilder()
    .withUrl(resolveHubUrl(), {
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on(TICKET_STATUS_CHANGED_EVENT, (payload: TicketStatusChangedRealtimeEventRaw) => {
    onStatusChanged({
      ...payload,
      ticketCode: extractSeatCodeFromTicketCode(payload.ticketCode),
      status: normalizeTicketStatus(payload.status),
      lockingBy: payload.lockingBy ?? null,
    })
  })

  await connection.start()
  await connection.invoke("SubscribeShowTime", showtimeId)

  return connection
}
