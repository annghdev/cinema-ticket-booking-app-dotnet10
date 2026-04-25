import type { HubConnection } from "@microsoft/signalr"
import { type ReactElement, useEffect, useMemo, useRef, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import { connectTicketStatusHub } from "../apis/ticketRealtime"
import { getShowTimeById, lockTicket, releaseTicket, validateSeatSelection } from "../apis/showtimeApi"
import { saveCheckoutDraft } from "../lib/checkoutDraft"
import { getOrCreateCustomerSessionId } from "../lib/customerSessionId"
import type { ShowTimeDetailDto } from "../types/contracts"

/**
 * SeatMap cell encoding — mirrors SeatMapCellValue.cs in the Domain layer.
 *   0 = Walking aisle / stair (segment boundary)
 *   1 = Regular seat
 *   2 = VIP seat
 *   3 = SweetBox (couple) seat
 *   4 = SweetBox couple gap spacer (visual only; NOT a seat, NOT an aisle boundary)
 */
type SeatCellType = 0 | 1 | 2 | 3 | 4

type ParsedSeat = {
  code: string
  row: number
  column: number
  type: Exclude<SeatCellType, 0 | 4>
}

function parseSeatMap(seatMap: string): SeatCellType[][] {
  if (!seatMap.trim()) {
    return []
  }

  const trimmed = seatMap.trim()
  if (trimmed.startsWith("[")) {
    const parsedJson = JSON.parse(trimmed) as number[][]
    if (!Array.isArray(parsedJson) || parsedJson.length === 0) {
      return []
    }

    const expectedCols = parsedJson[0]?.length ?? 0
    if (expectedCols === 0) {
      return []
    }

    parsedJson.forEach((row, rowIndex) => {
      if (row.length !== expectedCols) {
        throw new Error(`Row ${rowIndex + 1} does not have the same number of columns.`)
      }
    })

    return parsedJson.map((row) => row.map((cell) => cell as SeatCellType))
  }

  const rows = trimmed
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)
    .map((line) => line.split(/[,\s]+/).map((cell) => Number(cell)))

  if (rows.length === 0) {
    return []
  }

  const expectedCols = rows[0].length
  rows.forEach((row, rowIndex) => {
    if (row.length !== expectedCols) {
      throw new Error(`Row ${rowIndex + 1} does not have the same number of columns.`)
    }
  })

  return rows as SeatCellType[][]
}

function validateSeatMap(seatGrid: SeatCellType[][]) {
  seatGrid.forEach((row, rowIndex) => {
    row.forEach((value, colIndex) => {
      // Valid range: 0 (aisle), 1-3 (seat types), 4 (SweetBox couple gap spacer).
      if (value < 0 || value > 4) {
        throw new Error(`Invalid seat map value ${value} at row ${rowIndex + 1}, column ${colIndex + 1}`)
      }
    })
  })

  const firstRow = seatGrid[0]
  if (!firstRow) {
    return
  }

  // True aisle columns (0 in the first row) must be 0 in every row.
  // SweetBox gap spacers (4) are row-local and are exempt from this constraint.
  firstRow.forEach((value, colIndex) => {
    if (value === 0) {
      for (let rowIndex = 0; rowIndex < seatGrid.length; rowIndex += 1) {
        if (seatGrid[rowIndex][colIndex] !== 0) {
          throw new Error(`Aisle column ${colIndex + 1} must be 0 in all rows, but row ${rowIndex + 1} has value ${seatGrid[rowIndex][colIndex]}`)
        }
      }
    }
  })
}

function generateSeatsFromSeatMap(seatGrid: SeatCellType[][]): ParsedSeat[] {
  const seats: ParsedSeat[] = []
  let sweetBoxCounter = 1

  for (let row = 0; row < seatGrid.length; row += 1) {
    let seatNumber = 1

    // Scan right-to-left, matching the backend Screen.GenerateSeats() convention.
    for (let col = seatGrid[row].length - 1; col >= 0; col -= 1) {
      const seatType = seatGrid[row][col]

      // Skip true aisles (0) and SweetBox couple gap spacers (4) — neither produces a Seat entity.
      if (seatType === 0 || seatType === 4) {
        continue
      }

      const code = seatType === 3 ? `Sweet${sweetBoxCounter++}` : `${String.fromCharCode(65 + row)}${seatNumber}`
      seats.push({
        code,
        row: row + 1,
        column: col + 1,
        type: seatType as Exclude<SeatCellType, 0 | 4>,
      })
      seatNumber += 1
    }
  }

  return seats
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(value)
}

function formatDateTimeLabel(dateInput: string) {
  const date = new Date(dateInput)
  if (Number.isNaN(date.getTime())) {
    return dateInput
  }
  return new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date)
}

function ShowtimeSeatSelection() {
  const { showtimeId } = useParams<{ showtimeId: string }>()
  const [showtime, setShowtime] = useState<ShowTimeDetailDto | null>(null)
  const [tickets, setTickets] = useState<ShowTimeDetailDto["tickets"]>([])
  const [selectedSeatCodes, setSelectedSeatCodes] = useState<string[]>([])
  const [pendingSeatCodes, setPendingSeatCodes] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [checkoutNavigating, setCheckoutNavigating] = useState(false)
  const [clientSessionId] = useState(() => getOrCreateCustomerSessionId())
  const navigate = useNavigate()
  /** Synchronous lock so rapid double-clicks cannot queue two requests before React re-renders. */
  const seatRequestLockRef = useRef<string | null>(null)

  useEffect(() => {
    async function loadShowtimeDetail(targetShowtimeId: string) {
      try {
        setLoading(true)
        const data = await getShowTimeById(targetShowtimeId)
        const session = getOrCreateCustomerSessionId()
        setShowtime(data)
        setTickets(data.tickets)
        setSelectedSeatCodes(
          data.tickets
            .filter((t) => t.status === "Locking" && t.lockingBy != null && t.lockingBy === session)
            .map((t) => t.code),
        )
        setActionError(null)
        setError(null)
      } catch {
        setError("Không tải được chi tiết suất chiếu.")
      } finally {
        setLoading(false)
      }
    }

    if (!showtimeId) {
      setError("Thiếu mã suất chiếu.")
      setLoading(false)
      return
    }

    void loadShowtimeDetail(showtimeId)
  }, [showtimeId])

  useEffect(() => {
    if (!showtimeId) {
      return
    }
    const targetShowtimeId = showtimeId

    let connection: HubConnection | null = null
    let disposed = false

    async function connect() {
      try {
        connection = await connectTicketStatusHub(targetShowtimeId, (event) => {
          setTickets((currentTickets) =>
            currentTickets.map((ticket) => {
              if (ticket.id !== event.ticketId && ticket.code !== event.ticketCode) {
                return ticket
              }
              const nextLockingBy = event.status === "Locking" ? event.lockingBy : null
              return { ...ticket, status: event.status, lockingBy: nextLockingBy }
            }),
          )

          const session = getOrCreateCustomerSessionId()
          if (
            event.status === "Sold" ||
            event.status === "PendingPayment" ||
            event.status === "Available" ||
            (event.status === "Locking" && event.lockingBy != null && event.lockingBy !== session)
          ) {
            setSelectedSeatCodes((current) => current.filter((code) => code !== event.ticketCode))
          }

          setPendingSeatCodes((current) => current.filter((code) => code !== event.ticketCode))
        })
      } catch {
        if (!disposed) {
          setActionError("Không kết nối được realtime trạng thái ghế.")
        }
      }
    }

    void connect()

    return () => {
      disposed = true
      if (!connection) {
        return
      }

      void connection.invoke("UnsubscribeShowTime", targetShowtimeId).catch(() => undefined)
      void connection.stop().catch(() => undefined)
    }
  }, [showtimeId])

  const seatGrid = useMemo(() => {
    if (!showtime) {
      return []
    }
    const parsed = parseSeatMap(showtime.seatMap)
    validateSeatMap(parsed)
    return parsed
  }, [showtime])

  const generatedSeats = useMemo(() => {
    return generateSeatsFromSeatMap(seatGrid)
  }, [seatGrid])

  const seatsByPosition = useMemo(() => {
    return generatedSeats.reduce<Record<string, ParsedSeat>>((acc, seat) => {
      acc[`${seat.row}-${seat.column}`] = seat
      return acc
    }, {})
  }, [generatedSeats])

  const ticketsByCode = useMemo(() => {
    if (!showtime) {
      return {}
    }
    return tickets.reduce<Record<string, ShowTimeDetailDto["tickets"][number]>>((acc, ticket) => {
      acc[ticket.code] = ticket
      return acc
    }, {})
  }, [tickets, showtime])

  const selectedTickets = useMemo(() => {
    return selectedSeatCodes
      .map((code) => ticketsByCode[code])
      .filter((ticket): ticket is NonNullable<typeof ticket> => Boolean(ticket))
      .sort((a, b) => a.code.localeCompare(b.code))
  }, [selectedSeatCodes, ticketsByCode])

  const totalAmount = useMemo(() => {
    return selectedTickets.reduce((sum, ticket) => sum + ticket.price, 0)
  }, [selectedTickets])

  const isSeatRequestInFlight = pendingSeatCodes.length > 0

  const toggleSeat = async (seatCode: string) => {
    if (!showtime) {
      return
    }

    const ticket = ticketsByCode[seatCode]
    if (!ticket) {
      return
    }

    const isSelected = selectedSeatCodes.includes(seatCode)
    const isPending = pendingSeatCodes.includes(seatCode)
    const lockHeldBySomeoneElse =
      ticket.status === "Locking" && (ticket.lockingBy == null || ticket.lockingBy !== clientSessionId)
    const isUnavailable = ticket.status === "Sold" || ticket.status === "PendingPayment" || lockHeldBySomeoneElse

    if (isPending || (!isSelected && isUnavailable)) {
      return
    }

    if (pendingSeatCodes.length > 0 && !pendingSeatCodes.includes(seatCode)) {
      return
    }

    if (seatRequestLockRef.current !== null) {
      return
    }
    seatRequestLockRef.current = seatCode

    try {
      setPendingSeatCodes((current) => [...current, seatCode])
      setActionError(null)

      if (isSelected) {
        await releaseTicket(showtime.id, ticket.id, clientSessionId)
        setSelectedSeatCodes((current) => current.filter((code) => code !== seatCode))
      } else {
        await lockTicket(showtime.id, ticket.id, clientSessionId)
        setSelectedSeatCodes((current) => [...current, seatCode])
      }
    } catch {
      setActionError("Không thể cập nhật trạng thái ghế. Vui lòng thử lại.")
    } finally {
      seatRequestLockRef.current = null
      setPendingSeatCodes((current) => current.filter((code) => code !== seatCode))
    }
  }

  if (loading) {
    return (
      <main className="min-h-screen bg-background px-8 pb-12 pt-24 text-on-background md:pt-28">
        <div className="mx-auto w-full max-w-screen-2xl rounded-xl border border-outline-variant/20 bg-surface-container-low p-8 text-center text-on-surface-variant">
          Đang tải sơ đồ ghế...
        </div>
      </main>
    )
  }

  if (error || !showtime) {
    return (
      <main className="min-h-screen bg-background px-8 pb-12 pt-24 text-on-background md:pt-28">
        <div className="mx-auto w-full max-w-screen-2xl rounded-xl border border-red-400/30 bg-red-500/10 p-8 text-center text-red-200">
          {error ?? "Không tìm thấy suất chiếu."}
        </div>
      </main>
    )
  }

  const canContinueCheckout = selectedTickets.length > 0 && !isSeatRequestInFlight && !checkoutNavigating

  return (
    <main className="min-h-screen bg-background text-on-background">
      <div className="mx-auto flex w-full max-w-screen-2xl flex-col gap-0 px-8 pb-40 pt-20 md:flex-row md:items-start md:gap-8 md:pb-8 md:pt-24">
        <section className="relative min-w-0 flex-1 bg-background">
          <div className="flex w-full flex-col items-center">
            <div className="w-full rounded-xl border border-outline-variant/20 bg-surface-container-low/60 p-4 text-sm text-on-surface-variant">
              <p className="font-semibold text-on-background">{showtime.movieName}</p>
              <p className="mt-1">
                {showtime.cinemaName} • {showtime.screenCode} • {formatDateTimeLabel(showtime.startAt)}
              </p>
            </div>
          </div>

          <div className="mx-auto mt-8 flex w-max max-w-full flex-col-reverse items-stretch gap-8">
            <div
              className="grid w-max max-w-full gap-2"
              style={{
                gridTemplateColumns: `repeat(${seatGrid[0]?.length ?? 0}, minmax(2.25rem, 2.75rem))`,
              }}
            >
              {seatGrid.map((row, rowIndex) => {
                const rowCells: ReactElement[] = []

                // 1. Analyze the row above to identify seat blocks
                type Block = { start: number; end: number; length: number }
                const blocks: Block[] = []
                if (rowIndex > 0) {
                  const rowAbove = seatGrid[rowIndex - 1]
                  let blockStart = -1
                  for (let i = 0; i < rowAbove.length; i++) {
                    if (rowAbove[i] !== 0) {
                      if (blockStart === -1) blockStart = i
                    } else {
                      if (blockStart !== -1) {
                        blocks.push({ start: blockStart, end: i - 1, length: i - blockStart })
                        blockStart = -1
                      }
                    }
                  }
                  if (blockStart !== -1) {
                    blocks.push({ start: blockStart, end: rowAbove.length - 1, length: rowAbove.length - blockStart })
                  }
                }

                // 2. Build rendering commands for the current row
                type RenderCommand = { type: "wide-seat" | "single-seat"; seatIdx: number; span: number }
                const commands: Record<number, RenderCommand> = {}

                for (const block of blocks) {
                  if (block.length % 2 !== 0 && block.start > 0 && seatGrid[rowIndex][block.start - 1] === 0) {
                    // ODD block: overwrite the aisle on the left!
                    const startIdx = block.start - 1
                    const endIdx = block.end
                    const numPairs = Math.floor((endIdx - startIdx + 1) / 2)

                    for (let k = 0; k < numPairs; k++) {
                      const pairStart = startIdx + 2 * k
                      // Search for a '3' in the pair area
                      let seatIdx = -1
                      if (seatGrid[rowIndex][pairStart] === 3) seatIdx = pairStart
                      else if (seatGrid[rowIndex][pairStart + 1] === 3) seatIdx = pairStart + 1

                      if (seatIdx !== -1) {
                        commands[pairStart] = { type: "wide-seat", seatIdx, span: 2 }
                      }
                    }
                  } else {
                    // EVEN block or no aisle on left: map normally
                    for (let i = block.start; i <= block.end; i++) {
                      if (seatGrid[rowIndex][i] === 4 && seatGrid[rowIndex][i + 1] === 3) {
                        commands[i] = { type: "wide-seat", seatIdx: i + 1, span: 2 }
                        i++
                      } else if (seatGrid[rowIndex][i] === 3 && seatGrid[rowIndex][i + 1] === 4) {
                        commands[i] = { type: "wide-seat", seatIdx: i, span: 2 }
                        i++
                      }
                    }
                  }
                }

                // If no blocks were analyzed (e.g., rowIndex 0), fall back to basic standalone mapping
                if (blocks.length === 0) {
                  for (let i = 0; i < row.length; i++) {
                    if (row[i] === 4 && row[i + 1] === 3) {
                      commands[i] = { type: "wide-seat", seatIdx: i + 1, span: 2 }
                      i++
                    } else if (row[i] === 3 && row[i + 1] === 4) {
                      commands[i] = { type: "wide-seat", seatIdx: i, span: 2 }
                      i++
                    }
                  }
                }

                // 3. Execute rendering commands left-to-right
                for (let colIndex = 0; colIndex < row.length; colIndex += 1) {
                  const cell = row[colIndex]

                  if (commands[colIndex]) {
                    const cmd = commands[colIndex]
                    const seat = seatsByPosition[`${rowIndex + 1}-${cmd.seatIdx + 1}`]

                    if (seat) {
                      const ticket = ticketsByCode[seat.code]
                      const status = ticket?.status ?? "Available"
                      const isPending = pendingSeatCodes.includes(seat.code)
                      const isSelected = selectedSeatCodes.includes(seat.code)
                      const lockOwner = ticket?.lockingBy ?? null
                      const lockIsOurs = status === "Locking" && lockOwner === clientSessionId
                      const isUnavailable =
                        status === "Sold" ||
                        status === "PendingPayment" ||
                        (status === "Locking" && !lockIsOurs && !isSelected) ||
                        isPending

                      const baseClass = "border-fuchsia-300/40 bg-fuchsia-500/10"
                      const stateClass = isSelected
                        ? "border-secondary bg-secondary text-on-primary shadow-[0_0_8px_rgba(0,244,254,0.5)]"
                        : isUnavailable
                          ? "cursor-not-allowed border-outline-variant/20 bg-surface-dim text-on-surface-variant opacity-40"
                          : `${baseClass} text-on-background hover:border-primary/50 hover:text-primary`

                      rowCells.push(
                        <button
                          key={seat.code}
                          type="button"
                          onClick={() => void toggleSeat(seat.code)}
                          disabled={isUnavailable}
                          title={`${seat.code} - ${status}`}
                          className={`relative h-11 rounded-md border text-[11px] font-semibold transition-colors col-span-2 w-full ${stateClass}`}
                        >
                          {isPending ? (
                            <span className="absolute inset-0 flex items-center justify-center">
                              <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                            </span>
                          ) : (
                            seat.code
                          )}
                        </button>
                      )
                    } else {
                      rowCells.push(<div key={`missing-${rowIndex}-${colIndex}`} className="h-11 w-11 col-span-2" />)
                    }
                    colIndex += cmd.span - 1
                    continue
                  }

                  // Standard fallback rendering
                  if (cell === 0) {
                    rowCells.push(<div key={`aisle-${rowIndex}-${colIndex}`} className="h-11 w-11" />)
                    continue
                  }

                  const seat = seatsByPosition[`${rowIndex + 1}-${colIndex + 1}`]
                  if (!seat) {
                    rowCells.push(<div key={`missing-${rowIndex}-${colIndex}`} className="h-11 w-11" />)
                    continue
                  }

                  const ticket = ticketsByCode[seat.code]
                  const status = ticket?.status ?? "Available"
                  const isPending = pendingSeatCodes.includes(seat.code)
                  const isSelected = selectedSeatCodes.includes(seat.code)
                  const lockOwner = ticket?.lockingBy ?? null
                  const lockIsOurs = status === "Locking" && lockOwner === clientSessionId
                  const isUnavailable =
                    status === "Sold" ||
                    status === "PendingPayment" ||
                    (status === "Locking" && !lockIsOurs && !isSelected) ||
                    isPending

                  const baseClass =
                    seat.type === 1
                      ? "border-outline-variant/20 bg-surface-container-highest"
                      : seat.type === 2
                        ? "border-amber-300/40 bg-amber-500/10"
                        : "border-fuchsia-300/40 bg-fuchsia-500/10"

                  const stateClass = isSelected
                    ? "border-secondary bg-secondary text-on-primary shadow-[0_0_8px_rgba(0,244,254,0.5)]"
                    : isUnavailable
                      ? "cursor-not-allowed border-outline-variant/20 bg-surface-dim text-on-surface-variant opacity-40"
                      : `${baseClass} text-on-background hover:border-primary/50 hover:text-primary`

                  rowCells.push(
                    <button
                      key={seat.code}
                      type="button"
                      onClick={() => void toggleSeat(seat.code)}
                      disabled={isUnavailable}
                      title={`${seat.code} - ${status}`}
                      className={`relative h-11 rounded-md border text-[11px] font-semibold transition-colors w-11 ${stateClass}`}
                    >
                      {isPending ? (
                        <span className="absolute inset-0 flex items-center justify-center">
                          <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                        </span>
                      ) : (
                        seat.code
                      )}
                    </button>
                  )
                }

                return rowCells
              })}
          </div>
          <div className="relative flex h-12 w-full min-w-0 shrink-0 items-center justify-center overflow-hidden rounded-t-[50%] border-t-4 border-secondary bg-gradient-to-t from-transparent to-surface-container-high/50 shadow-[0_-15px_40px_rgba(0,244,254,0.3)] sm:h-16 sm:rounded-t-full">
            <span className="mt-2 font-headline text-sm uppercase tracking-widest text-on-surface-variant">MÀN HÌNH</span>
          </div>
          </div>

          <div className="mx-auto mt-6 flex w-max max-w-full flex-wrap justify-center gap-3 text-xs text-on-surface-variant">
            <span className="rounded border border-outline-variant/30 bg-surface-container px-2 py-1">Regular</span>
            <span className="rounded border border-amber-300/40 bg-amber-500/10 px-2 py-1">VIP</span>
            <span className="rounded border border-fuchsia-300/40 bg-fuchsia-500/10 px-2 py-1">Sweet/Couple</span>
            <span className="rounded border border-secondary bg-secondary/20 px-2 py-1">Đã chọn</span>
            <span className="rounded border border-outline-variant/30 bg-surface-dim px-2 py-1">Không khả dụng</span>
          </div>
          <p className="mx-auto mt-3 w-full max-w-2xl pb-6 text-center text-xs leading-relaxed sm:text-sm">
            <span className="block rounded-lg border border-amber-400/20 bg-amber-500/5 px-3 py-2.5 text-on-surface-variant">
              <span className="font-semibold text-amber-200/90">Lưu ý:</span> Sau 5 phút kể từ lúc chọn ghế, hệ thống sẽ tự động hủy nếu không tiến hành thanh toán.
            </span>
          </p>
          {actionError && (
            <div className="mx-auto mt-2 w-full pb-4">
              <div className="rounded border border-red-400/30 bg-red-500/10 px-3 py-2 text-sm text-red-200">{actionError}</div>
            </div>
          )}
        </section>

        <aside className="fixed bottom-0 left-0 right-0 z-40 w-full border-t border-outline-variant/20 bg-surface-container-low p-6 shadow-[0_-20px_40px_rgba(0,0,0,0.5)] md:sticky md:top-[72px] md:bottom-auto md:left-auto md:right-auto md:z-auto md:h-[calc(100vh-72px)] md:w-80 md:shrink-0 md:self-start md:overflow-y-auto md:border-l md:border-t-0 md:shadow-none lg:w-96">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="font-headline text-xl font-bold">Đã chọn</h2>
            <span className="border border-outline-variant/20 bg-surface-container-highest px-2 py-1 text-xs text-primary">
              {selectedTickets.length} Vé
            </span>
          </div>
          <div className="mb-6 max-h-[45vh] space-y-3 overflow-auto pr-1">
            {selectedTickets.length > 0 ? (
              selectedTickets.map((ticket) => (
                <div key={ticket.id} className="rounded-xl border border-outline-variant/20 bg-surface-variant/40 p-4">
                  <div className="flex items-center justify-between">
                    <span>{ticket.code}</span>
                    <span className="font-headline text-primary">{formatCurrency(ticket.price)}</span>
                  </div>
                </div>
              ))
            ) : (
              <div className="rounded-xl border border-outline-variant/20 bg-surface-variant/20 p-4 text-sm text-on-surface-variant">Chưa chọn ghế nào.</div>
            )}
          </div>
          <div className="flex items-end justify-between pb-3">
            <div>
              <p className="text-sm text-on-surface-variant">Tổng tiền</p>
              <p className="font-headline text-3xl font-bold">{formatCurrency(totalAmount)}</p>
            </div>
          </div>
          {actionError && (
            <div className="mb-3 rounded border border-red-400/30 bg-red-500/10 p-2 text-sm text-red-200 md:hidden" role="alert">
              {actionError}
            </div>
          )}
          <button
            type="button"
            disabled={!canContinueCheckout}
            onClick={async () => {
              if (!canContinueCheckout) {
                return
              }
              setActionError(null)
              setCheckoutNavigating(true)
              try {
                await validateSeatSelection(showtime.id, {
                  selectedTicketIds: selectedTickets.map((t) => t.id),
                  customerSessionId: getOrCreateCustomerSessionId(),
                })
                const selectedTicketIds = selectedTickets.map((t) => t.id)
                saveCheckoutDraft(showtime.id, { selectedTicketIds })
                navigate(`/checkout?showtimeId=${showtime.id}`, {
                  state: { showtimeId: showtime.id, selectedTicketIds },
                })
              } catch (err) {
                setCheckoutNavigating(false)
                setActionError(err instanceof Error ? err.message : "Không thể xác nhận lựa chọn ghế.")
              }
            }}
            className="flex w-full items-center justify-center gap-2 bg-gradient-to-br from-primary to-primary-container py-4 font-headline font-bold text-on-primary transition-all hover:shadow-[0_0_20px_rgba(0,244,254,0.4)] enabled:hover:brightness-105 enabled:active:scale-[0.99] disabled:cursor-not-allowed disabled:opacity-50"
          >
            {checkoutNavigating ? (
              <>
                <span
                  className="inline-block h-5 w-5 shrink-0 animate-spin rounded-full border-2 border-on-primary border-t-transparent"
                  aria-hidden
                />
                Đang xác nhận…
              </>
            ) : (
              <>
                Tiếp tục thanh toán
                <span className="material-symbols-outlined text-[1.25rem]">arrow_forward</span>
              </>
            )}
          </button>
        </aside>
      </div>
    </main>
  )
}

export default ShowtimeSeatSelection
