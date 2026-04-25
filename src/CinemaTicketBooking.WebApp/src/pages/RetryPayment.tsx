import { isAxiosError } from "axios"
import type { HubConnection } from "@microsoft/signalr"
import { useCallback, useEffect, useState } from "react"
import { Link, useNavigate, useSearchParams } from "react-router-dom"
import { retryPayment, getBookingById, cancelBooking } from "../apis/bookingApi"
import { connectPaymentHub } from "../apis/paymentRealtime"
import { PaymentQRModal } from "../components/PaymentQRModal"
import { getAvailableGateways, getFakePaymentSuccess } from "../apis/paymentApi"
import { type BookingDetailsDto, type CreateBookingResponse } from "../types/Booking"
import { isRedirectBehavior, type PaymentConfirmedRealtimeEvent, type PaymentGatewayOptionDto, type VerifyPaymentResponse } from "../types/Payment"
import { getOrCreateCustomerSessionId } from "../lib/customerSessionId"
import { useToast } from "../contexts/ToastContext"

function resolveApiOrigin(): string {
  const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL as string | undefined
  if (!configuredBaseUrl) {
    return window.location.origin
  }
  try {
    return new URL(configuredBaseUrl, window.location.origin).origin
  } catch {
    return window.location.origin
  }
}

function resolvePaymentIcon(icon: string | null | undefined): string | null {
  if (!icon || !icon.trim()) {
    return null
  }
  const trimmed = icon.trim()
  if (trimmed.startsWith("<svg") || trimmed.includes("xmlns=")) {
    return `data:image/svg+xml;utf8,${encodeURIComponent(trimmed)}`
  }
  return trimmed
}

function parseGatewayIdFromPaymentUrl(paymentUrl: string | null | undefined, fallback: string | null | undefined): string | null {
  if (fallback) {
    return fallback
  }
  if (!paymentUrl) return null
  try {
    const url = new URL(paymentUrl)
    return url.searchParams.get("gatewayId") ?? null
  } catch {
    return null
  }
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

export default function RetryPayment() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { success } = useToast()

  const bookingId = searchParams.get("bookingId")

  const [booking, setBooking] = useState<BookingDetailsDto | null>(null)
  const [gateways, setGateways] = useState<PaymentGatewayOptionDto[]>([])
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState<string | null>(null)

  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  
  const [cancellingBooking, setCancellingBooking] = useState(false)

  const [postBooking, setPostBooking] = useState<CreateBookingResponse | null>(null)
  const [paymentModalOpen, setPaymentModalOpen] = useState(false)
  const [completingFake, setCompletingFake] = useState(false)
  const [modalError, setModalError] = useState<string | null>(null)

  const [selectedSwitchGateway, setSelectedSwitchGateway] = useState<string | null>(null)
  const [switchingGateway, setSwitchingGateway] = useState(false)

  useEffect(() => {
    if (!bookingId) {
      setLoadError("Thiếu mã đặt vé.")
      setLoading(false)
      return
    }

    let cancelled = false
    void (async () => {
      try {
        const [bookingData, gatewayData] = await Promise.all([
          getBookingById(bookingId, getOrCreateCustomerSessionId()),
          getAvailableGateways(),
        ])
        
        if (cancelled) return

        if (bookingData.status !== 1 && bookingData.status !== "Pending") {
          setLoadError("Vé này không ở trạng thái chờ thanh toán. Không thể thử lại.")
        } else {
          setBooking(bookingData)
          setGateways(gatewayData)
          if (gatewayData.length > 0) {
            setSelectedPaymentMethod(gatewayData[0].method)
          }
        }
      } catch (err) {
        if (!cancelled) setLoadError("Không thể lấy thông tin đặt vé hoặc các phương thức thanh toán.")
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()

    return () => {
      cancelled = true
    }
  }, [bookingId])

  const navigateToSuccess = useCallback((bid: string, checkinQrCode?: string | null, gatewayTxnRef?: string | null) => {
    setPaymentModalOpen(false)
    navigate({
      pathname: "/payment-result",
      search: `?status=success&bookingId=${encodeURIComponent(bid)}${gatewayTxnRef ? `&txnRef=${encodeURIComponent(gatewayTxnRef)}` : ""}`,
    }, checkinQrCode ? {
      state: {
        checkinQrCode,
        bookingId: bid,
        movieName: booking?.showTimeInfo.movie,
        screenCode: booking?.showTimeInfo.screen,
        startAt: booking?.showTimeInfo.startAt,
        seatsLabel: booking?.tickets.map((t) => t.seatCode).join(", "),
        finalAmount: postBooking?.finalAmount ?? booking?.finalAmount,
        concessions: booking?.concessions.map((l) => ({
          name: l.name,
          quantity: l.quantity,
          amount: l.amount,
        })),
      },
    } : undefined)
  }, [navigate, booking, postBooking])

  const onPaymentConfirmedRealtime = useCallback((event: PaymentConfirmedRealtimeEvent) => {
    setModalError(null)
    navigateToSuccess(event.bookingId, event.checkinQrCode, event.gatewayTransactionId)
  }, [navigateToSuccess])

  const onRetryPaymentClick = async () => {
    if (!bookingId || !selectedPaymentMethod) return

    setSubmitError(null)
    setModalError(null)
    setSubmitting(true)
    setPostBooking(null)
    
    try {
      const selectedGateway = selectedPaymentMethod.toLowerCase()
      const returnUrl = selectedGateway === "momo"
        ? `${resolveApiOrigin()}/api/payments/momo-return`
        : selectedGateway === "vnpay"
          ? `${resolveApiOrigin()}/api/payments/vnpay-return`
          : `${window.location.origin}/payment-result`

      const res = await retryPayment(bookingId, {
        customerSessionId: getOrCreateCustomerSessionId(),
        paymentMethod: selectedPaymentMethod,
        returnUrl,
        ipAddress: "127.0.0.1",
        replacePendingPayment: true,
      })

      const behavior = isRedirectBehavior(res.redirectBehavior)
      if (behavior === "Redirect" && res.paymentUrl) {
        window.location.assign(res.paymentUrl)
        return
      }

      setPostBooking(res)
      setPaymentModalOpen(true)
    } catch (e) {
      if (isAxiosError(e) && e.response?.data) {
        const d = e.response.data as { title?: string; detail?: string; message?: string }
        const msg = d.detail ?? d.title ?? d.message
        if (msg) {
          setSubmitError(String(msg))
        } else {
          setSubmitError("Không thể xử lý thanh toán. Vui lòng thử lại.")
        }
      } else {
        setSubmitError("Không thể xử lý thanh toán. Vui lòng thử lại.")
      }
    } finally {
      setSubmitting(false)
    }
  }

  const onCancelBookingClick = async () => {
    if (!bookingId) return
    
    setSubmitError(null)
    setCancellingBooking(true)
    try {
      await cancelBooking(bookingId)
      success("Hủy đặt vé thành công!")
      setTimeout(() => {
        navigate("/showtimes")
      }, 1000)
    } catch (e) {
      if (isAxiosError(e) && e.response?.data) {
        const d = e.response.data as { title?: string; detail?: string; message?: string }
        setSubmitError(d.detail ?? d.title ?? d.message ?? "Không thể hủy đặt vé.")
      } else {
        setSubmitError("Không thể hủy đặt vé.")
      }
    } finally {
      setCancellingBooking(false)
    }
  }

  const onCompleteFakeCallback = async () => {
    if (!postBooking) return
    const gatewayId = parseGatewayIdFromPaymentUrl(postBooking.paymentUrl, postBooking.gatewayTransactionId)
    if (!gatewayId) {
      setSubmitError("Thiếu mã giao dịch cổng thanh toán — không thể xác nhận.")
      return
    }
    setModalError(null)
    setCompletingFake(true)
    try {
      const data: VerifyPaymentResponse = await getFakePaymentSuccess(postBooking.bookingId, gatewayId)
      if (!data.isSuccess) {
        setModalError(data.errorMessage ?? "Thanh toán chưa được xác nhận.")
        return
      }
      navigateToSuccess(data.bookingId, data.checkinQrCode, gatewayId)
    } catch (e) {
      if (isAxiosError(e) && e.response?.data) {
        const d = e.response.data as VerifyPaymentResponse | { title?: string; detail?: string }
        const maybeV = d as VerifyPaymentResponse
        if (typeof maybeV.errorMessage === "string" && maybeV.errorMessage) {
          setModalError(maybeV.errorMessage)
        } else {
          const t = d as { title?: string; detail?: string }
          setModalError(t.detail ?? t.title ?? "Xác nhận thanh toán thất bại.")
        }
      } else {
        setModalError("Xác nhận thanh toán thất bại.")
      }
    } finally {
      setCompletingFake(false)
    }
  }
  const onSelectSwitchGateway = (method: string) => {
    setSelectedSwitchGateway(method)
  }

  const handleSwitchGateway = () => {
    if (!selectedSwitchGateway) return
    setPaymentModalOpen(false)
    setSelectedPaymentMethod(selectedSwitchGateway)
    setSwitchingGateway(true)
  }

  // Effect to auto-retry when gateway changes via modal switch
  useEffect(() => {
    if (switchingGateway && selectedPaymentMethod) {
      setSwitchingGateway(false)
      onRetryPaymentClick()
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedPaymentMethod, switchingGateway])

  const isPostPayQrFlow = isRedirectBehavior(postBooking?.redirectBehavior) === "QrCode"

  useEffect(() => {
    if (!postBooking || !isPostPayQrFlow) return
    let connection: HubConnection | null = null
    let disposed = false

    void (async () => {
      try {
        connection = await connectPaymentHub(postBooking.bookingId, (payload) => {
          if (!disposed) {
            onPaymentConfirmedRealtime(payload)
          }
        })
      } catch {
        // Keep QR flow usable even when realtime channel is unavailable.
      }
    })()

    return () => {
      disposed = true
      if (connection) {
        void connection.stop()
      }
    }
  }, [postBooking?.bookingId, isPostPayQrFlow, onPaymentConfirmedRealtime])

  if (loadError) {
    return (
      <main className="mx-auto w-full max-w-screen-lg px-8 pb-12 pt-28">
        <div className="rounded-xl border border-red-500/30 bg-red-500/10 p-6 text-red-300">
          <p className="font-semibold">{loadError}</p>
        </div>
        <Link to="/showtimes" className="mt-4 inline-block text-secondary hover:underline">
          Lịch chiếu
        </Link>
      </main>
    )
  }

  if (loading || !booking) {
    return (
      <main className="mx-auto flex min-h-[50vh] w-full max-w-screen-lg items-center justify-center px-8 pt-28">
        <div className="flex flex-col items-center gap-3 text-on-surface-variant">
          <span
            className="inline-block h-10 w-10 animate-spin rounded-full border-2 border-secondary border-t-transparent"
            aria-hidden
          />
          <p>Đang tải thông tin thanh toán…</p>
        </div>
      </main>
    )
  }

  return (
    <>
      <main className="mx-auto grid w-full max-w-screen-lg grid-cols-1 gap-8 px-8 pb-12 pt-24 md:pt-28 lg:grid-cols-2">
        <section className="flex flex-col gap-6">
          <div>
            <h1 className="mb-2 font-headline text-3xl font-bold tracking-tight md:text-4xl">
              Thử lại thanh toán
            </h1>
            <p className="text-on-surface-variant">Thanh toán trước đó không thành công. Hãy chọn phương thức và thử lại.</p>
          </div>

          <div className="rounded-xl border border-outline-variant/10 bg-surface-container-low p-6">
            <h2 className="mb-4 text-xl font-semibold">Tóm tắt vé</h2>
            <div className="flex flex-col gap-3 text-sm">
              <div className="flex justify-between">
                <span className="text-on-surface-variant">Phim:</span>
                <span className="font-medium text-right">{booking.showTimeInfo.movie}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-on-surface-variant">Phòng chiếu:</span>
                <span className="font-medium">{booking.showTimeInfo.screen}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-on-surface-variant">Suất chiếu:</span>
                <span className="font-medium">{formatDateTimeLabel(booking.showTimeInfo.startAt)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-on-surface-variant">Ghế:</span>
                <span className="font-medium text-right max-w-[200px] break-words">
                  {booking.tickets.map(t => t.seatCode).join(", ")}
                </span>
              </div>
              {booking.concessions.length > 0 && (
                <div className="flex justify-between mt-2 pt-2 border-t border-outline-variant/10">
                  <span className="text-on-surface-variant">Bắp nước:</span>
                  <span className="font-medium text-right max-w-[200px] break-words">
                    {booking.concessions.map(c => `${c.quantity}x ${c.name}`).join(", ")}
                  </span>
                </div>
              )}
              <div className="flex justify-between mt-4 border-t border-outline-variant/20 pt-4 text-lg font-bold">
                <span>Tổng tiền:</span>
                <span className="text-secondary">{formatCurrency(booking.finalAmount)}</span>
              </div>
            </div>
          </div>
        </section>

        <section className="flex flex-col gap-6">
          <div className="rounded-xl border border-outline-variant/10 bg-surface-container-low p-6">
            <h2 className="mb-4 flex items-center gap-2 font-headline text-xl font-semibold">
              <span className="material-symbols-outlined text-secondary">payment</span>
              Phương thức thanh toán
            </h2>
            
            {submitError && (
              <div className="mb-4 rounded-lg bg-red-500/10 p-3 text-sm text-red-400">
                {submitError}
              </div>
            )}

            {gateways.length === 0 ? (
              <p className="text-sm text-amber-200/80">Chưa cấu hình phương thức thanh toán.</p>
            ) : (
              <div className="flex flex-col gap-3">
                {gateways.map((g) => {
                  const selected = selectedPaymentMethod === g.method
                  const iconDataUrl = resolvePaymentIcon(g.icon)
                  return (
                    <button
                      key={g.method}
                      type="button"
                      onClick={() => setSelectedPaymentMethod(g.method)}
                      className={`flex h-16 items-center gap-4 rounded-xl border px-4 transition-all ${
                        selected
                          ? "border-secondary bg-secondary/5 shadow-[0_0_15px_rgba(0,244,254,0.15)]"
                          : "border-outline-variant/10 bg-surface-container-highest hover:border-outline-variant/30 hover:bg-surface-container-highest/80"
                      }`}
                    >
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-white p-1">
                        {iconDataUrl ? (
                          <img src={iconDataUrl} alt={g.displayName || g.method} className="max-h-full max-w-full object-contain" />
                        ) : (
                          <span className="material-symbols-outlined text-gray-400">account_balance_wallet</span>
                        )}
                      </div>
                      <span className="font-semibold">{g.displayName || g.method}</span>
                    </button>
                  )
                })}
              </div>
            )}

            <button
              type="button"
              disabled={submitting || !selectedPaymentMethod || gateways.length === 0}
              onClick={onRetryPaymentClick}
              className="mt-6 flex w-full items-center justify-center gap-2 rounded-xl bg-secondary py-4 font-bold text-on-secondary transition-all hover:bg-secondary/90 hover:shadow-[0_0_20px_rgba(0,244,254,0.3)] disabled:opacity-50"
            >
              {submitting ? (
                <>
                  <span className="material-symbols-outlined animate-spin">progress_activity</span>
                  <span>Đang xử lý…</span>
                </>
              ) : (
                <>
                  <span className="material-symbols-outlined">shopping_cart_checkout</span>
                  Thanh toán lại {formatCurrency(booking.finalAmount)}
                </>
              )}
            </button>
            <button
              type="button"
              disabled={submitting || cancellingBooking}
              onClick={onCancelBookingClick}
              className="mt-3 flex w-full items-center justify-center gap-2 rounded-xl border border-outline-variant/30 bg-transparent py-4 font-bold text-on-surface-variant transition-all hover:bg-surface-container-highest hover:text-on-surface disabled:opacity-50"
            >
              {cancellingBooking ? (
                <>
                  <span className="material-symbols-outlined animate-spin">progress_activity</span>
                  <span>Đang hủy…</span>
                </>
              ) : (
                <>
                  <span className="material-symbols-outlined">cancel</span>
                  Hủy đặt vé
                </>
              )}
            </button>
          </div>
        </section>
      </main>

      <PaymentQRModal
        open={paymentModalOpen}
        onOpenChange={setPaymentModalOpen}
        paymentUrl={postBooking?.paymentUrl ?? null}
        displayMethod={gateways.find(g => g.method === selectedPaymentMethod)?.displayName ?? selectedPaymentMethod ?? ""}
        paymentExpiresAt={postBooking?.paymentExpiresAt ?? null}
        isNoneOrFake={selectedPaymentMethod?.toLowerCase() === "none"}
        onConfirmNonePayment={onCompleteFakeCallback}
        completingCallback={completingFake}
        errorText={modalError}
        onCancelPayment={() => navigate("/showtimes")}
        cancellingPayment={false}
        switchableGateways={gateways
          .filter((g) => g.method !== selectedPaymentMethod)
          .map((g) => ({ method: g.method, displayName: g.displayName || g.method, icon: g.icon }))}
        selectedSwitchGateway={selectedSwitchGateway}
        onSelectSwitchGateway={onSelectSwitchGateway}
        onSwitchGateway={handleSwitchGateway}
        switchingGateway={switchingGateway}
      />
    </>
  )
}
