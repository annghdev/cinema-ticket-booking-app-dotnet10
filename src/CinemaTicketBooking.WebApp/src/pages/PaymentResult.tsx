import { isAxiosError } from "axios"
import { useCallback, useEffect, useMemo, useState } from "react"
import { Link, useLocation, useSearchParams } from "react-router-dom"
import QRCode from "qrcode"
import { getBookingById, type BookingDetailsDto } from "../apis/bookingApi"
import { getPaymentResultByTxn } from "../apis/paymentApi"
import { downloadCheckinPassPdf, type CheckinTicketPdfInfo, type ConcessionLine } from "../lib/checkinPdf"
import { getCheckinQrPayload } from "../lib/checkinQrPayload"

export type PaymentResultConcession = {
  name: string
  quantity: number
  amount: number
}

export type PaymentResultLocationState = {
  checkinQrCode?: string
  bookingId?: string
  movieName?: string
  screenCode?: string
  startAt?: string
  cinemaName?: string
  seatsLabel?: string
  finalAmount?: number
  concessions?: PaymentResultConcession[]
}

const neonText = { textShadow: "0 0 8px rgba(0, 244, 254, 0.5)" } as const

function formatVnd(n: number | undefined) {
  if (n == null || Number.isNaN(n)) {
    return "—"
  }
  return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(n)
}

function splitDateTimeFromIso(iso: string) {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) {
    return { dateLabel: iso, timeLabel: "—" }
  }
  return {
    dateLabel: new Intl.DateTimeFormat("vi-VN", { day: "2-digit", month: "short", year: "numeric" }).format(d),
    timeLabel: new Intl.DateTimeFormat("vi-VN", { hour: "2-digit", minute: "2-digit" }).format(d),
  }
}

/**
 * Bố cục theo `assets/screens/payment-result.html` (2 cột: chi tiết + QR, glass panel, neon).
 */
function PaymentResult() {
  const [searchParams] = useSearchParams()
  const location = useLocation()
  const navState = location.state as PaymentResultLocationState | null

  const status = searchParams.get("status")
  const txnRef = searchParams.get("txnRef") ?? searchParams.get("orderId")
  const bookingIdParam = searchParams.get("bookingId")
  const [resolvedBookingId, setResolvedBookingId] = useState<string | null>(bookingIdParam ?? navState?.bookingId ?? null)

  const [checkinQrDataUrl, setCheckinQrDataUrl] = useState<string | null>(null)
  const [fetched, setFetched] = useState<BookingDetailsDto | null>(null)
  const [loadDetailError, setLoadDetailError] = useState(false)
  const [downloading, setDownloading] = useState(false)
  const [gatewayCheckinCode, setGatewayCheckinCode] = useState<string | null>(null)

  const checkinText = navState?.checkinQrCode ?? fetched?.checkinQrCode ?? gatewayCheckinCode ?? ""
  const qrPayload = useMemo(() => getCheckinQrPayload(resolvedBookingId, checkinText), [resolvedBookingId, checkinText])
  const hasQrPayload = Boolean(qrPayload)
  const ok = status === "success" || hasQrPayload

  const movie = navState?.movieName ?? fetched?.showTimeInfo?.movie
  const screen = navState?.screenCode ?? fetched?.showTimeInfo?.screen
  const startAtRaw = navState?.startAt ?? fetched?.showTimeInfo?.startAt
  const startAt = startAtRaw ? (typeof startAtRaw === "string" ? startAtRaw : String(startAtRaw)) : null
  const { dateLabel, timeLabel } = useMemo(
    () => (startAt ? splitDateTimeFromIso(startAt) : { dateLabel: "—", timeLabel: "—" }),
    [startAt],
  )
  const cinema = navState?.cinemaName
  const seats = navState?.seatsLabel ?? fetched?.tickets?.map((t) => t.seatCode).join(", ")
  const finalAmount = navState?.finalAmount ?? fetched?.finalAmount

  const concessions: ConcessionLine[] = useMemo(() => {
    if (navState?.concessions && navState.concessions.length > 0) {
      return navState.concessions.map((c) => ({ name: c.name, quantity: c.quantity, amount: c.amount }))
    }
    if (fetched?.concessions && fetched.concessions.length > 0) {
      return fetched.concessions.map((c) => ({ name: c.name, quantity: c.quantity, amount: c.amount }))
    }
    return []
  }, [navState?.concessions, fetched?.concessions])

  const ticketRefDisplay = useMemo(() => {
    if (!resolvedBookingId) {
      return "—"
    }
    const u = resolvedBookingId.replace(/-/g, "").toUpperCase()
    return `TKT-${u.slice(0, 4)}-${u.slice(4, 8)}`
  }, [resolvedBookingId])

  useEffect(() => {
    if (!qrPayload) {
      setCheckinQrDataUrl(null)
      return
    }
    let cancelled = false
    void QRCode.toDataURL(qrPayload, { width: 400, margin: 2, errorCorrectionLevel: "M" })
      .then((u) => {
        if (!cancelled) {
          setCheckinQrDataUrl(u)
        }
      })
      .catch(() => {
        if (!cancelled) {
          setCheckinQrDataUrl(null)
        }
      })
    return () => {
      cancelled = true
    }
  }, [qrPayload])

  useEffect(() => {
    if (!resolvedBookingId && !txnRef) {
      return
    }
    if (navState?.checkinQrCode) {
      return
    }
    let cancelled = false
    void (async () => {
      try {
        if (!cancelled) {
          setLoadDetailError(false)
        }

        // Real gateway flow: use bookingId + txnRef to read payment result without auth context.
        if (txnRef) {
          let hasResolvedResult = false
          for (let attempt = 0; attempt < 4; attempt++) {
            const result = await getPaymentResultByTxn(resolvedBookingId, txnRef)
            if (cancelled) {
              return
            }
            if (result.bookingId) {
              setResolvedBookingId(result.bookingId)
            }

            if (result.booking) {
              setFetched(result.booking)
            }
            if (result.checkinQrCode) {
              setGatewayCheckinCode(result.checkinQrCode)
            }

            // Booking may still be pending when return URL hits before IPN processing.
            if (result.isSuccess || result.status !== "pending_payment") {
              hasResolvedResult = true
              return
            }

            await new Promise((resolve) => setTimeout(resolve, 1500))
          }

          if (!cancelled && !hasResolvedResult) {
            setLoadDetailError(true)
          }
          return
        }

        // Fake payment / legacy flow.
        if (!resolvedBookingId) {
          if (!cancelled) {
            setLoadDetailError(true)
          }
          return
        }
        const booking = await getBookingById(resolvedBookingId)
        if (!cancelled) {
          setFetched(booking)
        }
      } catch (e) {
        if (isAxiosError(e) && (e.response?.status === 401 || e.response?.status === 403)) {
          if (!cancelled) {
            setLoadDetailError(true)
          }
        } else if (!cancelled) {
          setLoadDetailError(true)
        }
      }
    })()
    return () => {
      cancelled = true
    }
  }, [resolvedBookingId, navState?.checkinQrCode, txnRef])

  const onDownloadPdf = useCallback(async () => {
    if (!resolvedBookingId || !qrPayload) {
      return
    }
    setDownloading(true)
    try {
      const when = startAt ? splitDateTimeFromIso(startAt) : null
      const info: CheckinTicketPdfInfo = {
        bookingId: resolvedBookingId,
        movie: movie ?? "—",
        screen: screen ?? "—",
        cinema: cinema ?? "—",
        startAt: when ? `${when.dateLabel} ${when.timeLabel}` : "—",
        seats: seats ?? "—",
        finalAmount,
        concessions: concessions.length > 0 ? concessions : undefined,
      }
      await downloadCheckinPassPdf(info)
    } finally {
      setDownloading(false)
    }
  }, [resolvedBookingId, qrPayload, movie, screen, cinema, startAt, seats, finalAmount, concessions])

  const canDownload = Boolean(resolvedBookingId && qrPayload)
  const showSuccessGrid = ok && (hasQrPayload || movie || startAt)

  return (
    <main className="relative flex min-h-screen flex-grow flex-col items-center overflow-x-hidden bg-background pb-28 pt-24 text-on-background sm:px-8 sm:pt-28 md:pb-12">
      <div className="mx-auto flex w-full max-w-4xl flex-col items-center p-4 sm:p-6">
        <div className="mb-4 flex w-full justify-start md:mb-6 md:hidden">
          <Link
            to="/"
            className="font-label flex items-center gap-2 text-on-surface-variant transition-colors hover:text-on-background"
          >
            <span className="material-symbols-outlined">arrow_back</span>
            <span>Quay lại</span>
          </Link>
        </div>

        <div className="mb-6 w-full text-center md:mb-8">
          <h1
            className={`font-headline mb-2 text-4xl font-bold tracking-tight sm:text-5xl ${ok ? "text-secondary" : "text-on-background"
              }`}
          >
            {ok ? "Thanh toán thành công" : "Kết quả thanh toán"}
          </h1>
          <p className="font-body text-lg text-on-surface-variant">
            {ok ? "Vé của bạn đã sẵn sàng." : "Kiểm tra lại thông tin hoặc tài khoản của bạn."}
          </p>
        </div>

        {!ok && (
          <div className="w-full max-w-md rounded-xl border border-outline-variant/20 bg-surface-container-highest/40 p-6 text-center backdrop-blur-xl">
            <p className="text-on-surface-variant">
              Bạn có thể xem lại trạng thái từ email hoặc mục vé trong tài khoản.
            </p>
            <Link
              to="/"
              className="mt-6 inline-flex items-center justify-center gap-2 rounded-sm bg-primary px-8 py-3 font-headline font-bold text-on-primary"
            >
              Về trang chủ
            </Link>
          </div>
        )}

        {ok && !showSuccessGrid && !loadDetailError && (
          <p className="text-sm text-amber-200/80">Đang tải thông tin vé…</p>
        )}

        {ok && !hasQrPayload && loadDetailError && !navState?.checkinQrCode && (
          <p className="max-w-md text-center text-sm text-on-surface-variant">
            Mã check-in hiển thị khi bạn vừa xác nhận thanh toán từ cùng thiết bị, hoặc sau khi đăng nhập mở lại đơn.
          </p>
        )}

        {ok && showSuccessGrid && (
          <div className="relative grid w-full grid-cols-1 items-start gap-8 md:grid-cols-2">
            {/* Chi tiết — order-2 trên md là cột trái */}
            <div className="order-2 flex min-h-0 w-full min-w-0 flex-col gap-6 md:order-1">
              <div className="relative overflow-hidden rounded-xl border border-white/5 bg-surface-container-highest/40 p-6 shadow-[0_0_0_1px_rgba(69,72,79,0.2)] backdrop-blur-xl sm:p-6">
                <div className="absolute right-0 top-0 p-4 opacity-10">
                  <span className="material-symbols-outlined text-9xl text-secondary">movie</span>
                </div>
                <h2 className="font-headline relative z-10 mb-1 text-2xl font-bold sm:text-3xl">{movie ?? "—"}</h2>
                {cinema && (
                  <p className="font-body relative z-10 mb-6 flex items-center gap-2 text-sm text-primary-dim">
                    <span className="material-symbols-outlined text-sm">location_on</span>
                    {cinema}
                  </p>
                )}
                <div className="relative z-10 mb-6 grid grid-cols-2 gap-4">
                  <div>
                    <p className="font-label mb-1 text-xs uppercase tracking-widest text-on-surface-variant">Ngày</p>
                    <p className="font-body font-semibold text-on-surface">{dateLabel}</p>
                  </div>
                  <div>
                    <p className="font-label mb-1 text-xs uppercase tracking-widest text-on-surface-variant">Giờ</p>
                    <p className="font-body font-semibold text-on-surface">{timeLabel}</p>
                  </div>
                  <div>
                    <p className="font-label mb-1 text-xs uppercase tracking-widest text-on-surface-variant">Phòng</p>
                    <p className="font-body font-semibold text-on-surface">{screen ?? "—"}</p>
                  </div>
                  <div>
                    <p className="font-label mb-1 text-xs uppercase tracking-widest text-on-surface-variant">Ghế</p>
                    <p className="font-body font-semibold text-secondary" style={neonText}>
                      {seats ?? "—"}
                    </p>
                  </div>
                </div>
                {concessions.length > 0 && (
                  <div className="relative z-10 mt-2 border-t border-outline-variant/20 pt-4">
                    <p className="font-label mb-2 text-xs uppercase tracking-widest text-on-surface-variant">Đồ ăn &amp; nước kèm</p>
                    <ul className="space-y-1.5 text-sm text-on-surface">
                      {concessions.map((c) => (
                        <li key={`${c.name}-${c.quantity}`} className="flex justify-between gap-3">
                          <span>
                            {c.name} <span className="text-on-surface-variant">×{c.quantity}</span>
                          </span>
                          <span className="shrink-0 font-medium text-on-surface/90">{formatVnd(c.amount)}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
                <div className="relative z-10 mt-4 flex items-end justify-between border-t border-outline-variant/30 pt-4">
                  <div>
                    <p className="font-label mb-1 text-xs uppercase tracking-widest text-on-surface-variant">Tổng thanh toán</p>
                    <p className="font-headline text-3xl font-bold text-secondary" style={neonText}>
                      {formatVnd(finalAmount)}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 rounded-sm border border-outline-variant/20 bg-surface-container-low px-3 py-1">
                    <span className="material-symbols-outlined text-sm text-primary">check_circle</span>
                    <span className="font-label text-xs">Đã xác nhận</span>
                  </div>
                </div>
              </div>
              <button
                type="button"
                disabled={!canDownload || downloading}
                onClick={() => {
                  void onDownloadPdf()
                }}
                className="font-body flex w-full items-center justify-center gap-2 rounded-xl border border-outline-variant/40 bg-transparent py-4 font-bold text-primary transition-colors hover:bg-primary/10 disabled:cursor-not-allowed disabled:opacity-40"
              >
                {downloading ? (
                  <span
                    className="inline-block h-5 w-5 animate-spin rounded-lg border-2 border-primary border-t-transparent"
                    aria-hidden
                  />
                ) : (
                  <span className="material-symbols-outlined">download</span>
                )}
                Tải PDF vé
              </button>

              {ok && (
                <Link
                  to="/"
                  className="inline-flex h-14 items-center justify-center rounded-xl border border-outline-variant/30 px-8 py-3 font-headline text-sm font-semibold text-on-surface-variant transition-colors hover:border-secondary/40 hover:text-secondary"
                >
                  Về trang chủ
                </Link>
              )}
            </div>

            {/* QR — cùng mép trên với cột trái; không dùng justify-center theo chiều dọc */}
            <div className="order-1 w-full self-start md:order-2">
              <div className="group relative mx-auto w-full max-w-sm cursor-default md:ml-auto">
                <div
                  className="absolute inset-0 rounded-full bg-secondary opacity-20 blur-[60px] transition-opacity duration-500 group-hover:opacity-40"
                  aria-hidden
                />
                <div
                  className="relative z-10 flex w-full flex-col overflow-hidden rounded-xl border-2 border-outline-variant/30 bg-surface-container-highest/40 backdrop-blur-xl transition-colors duration-300 group-hover:border-secondary/50 sm:p-4"
                >
                  {checkinQrDataUrl && hasQrPayload ? (
                    <>
                      <div className="mx-auto my-8 aspect-square w-full max-w-[15.5rem] bg-white sm:max-w-[16rem]">
                        <div className="relative grid h-full min-h-0 w-full place-items-center">
                          <img
                            src={checkinQrDataUrl}
                            alt="Mã QR check-in"
                            className="max-h-full max-w-full object-contain"
                          />
                          <div
                            className="pointer-events-none absolute inset-0 bg-gradient-to-b from-transparent via-secondary/30 to-transparent opacity-50"
                            style={{ height: "25%", top: "50%", transform: "translateY(-50%)" }}
                            aria-hidden
                          />
                        </div>
                      </div>
                      <div className="pt-1 pb-1 text-center sm:pt-2">
                        <p className="font-label mb-2 text-xs uppercase tracking-widest text-on-surface-variant">Quét tại cổng vào</p>
                        <p className="font-headline text-lg font-bold tracking-widest text-on-surface sm:text-xl">{ticketRefDisplay}</p>
                        {resolvedBookingId && (
                          <p className="mt-2 break-all font-mono text-xs leading-snug text-on-surface/90">{qrPayload}</p>
                        )}
                      </div>
                    </>
                  ) : (
                    <div className="flex min-h-[200px] flex-col items-center justify-center gap-2 p-8 text-center text-sm text-on-surface-variant">
                      <span
                        className="inline-block h-10 w-10 animate-spin rounded-sm border-2 border-secondary border-t-transparent"
                        aria-hidden
                      />
                      Đang tải mã check-in…
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

      </div>
    </main>
  )
}

export default PaymentResult
