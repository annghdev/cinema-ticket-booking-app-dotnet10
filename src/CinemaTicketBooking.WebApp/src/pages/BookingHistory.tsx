import { isAxiosError } from "axios"
import { useEffect, useMemo, useState } from "react"
import { Link } from "react-router-dom"
import { getBookingHistory } from "../apis/bookingApi"
import { type BookingHistoryItemDto } from "../types/Booking"
import { useAuth } from "../contexts/AuthContext"
import { useToast } from "../contexts/ToastContext"

const PAGE_SIZE = 10

function formatCurrency(amount: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0,
  }).format(amount)
}

function formatDateTime(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }
  return new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date)
}

function normalizeBookingStatus(status: number | string): { label: string; className: string } {
  const raw = typeof status === "string" ? status : String(status)
  const key = raw.toLowerCase()

  if (key === "1" || key === "pending") {
    return { label: "Chờ thanh toán", className: "bg-amber-500/10 text-amber-300 border-amber-300/30" }
  }
  if (key === "2" || key === "confirmed") {
    return { label: "Đã xác nhận", className: "bg-emerald-500/10 text-emerald-300 border-emerald-300/30" }
  }
  if (key === "3" || key === "checkedin" || key === "checked_in") {
    return { label: "Đã check-in", className: "bg-sky-500/10 text-sky-300 border-sky-300/30" }
  }
  if (key === "4" || key === "cancelled" || key === "canceled") {
    return { label: "Đã hủy", className: "bg-rose-500/10 text-rose-300 border-rose-300/30" }
  }

  return {
    label: typeof status === "string" ? status : `Trạng thái ${status}`,
    className: "bg-surface-container-high text-on-surface border-outline-variant/30",
  }
}

function BookingHistory() {
  const { isAuthenticated, isResolvingProfile, customerId, displayName } = useAuth()
  const { error: toastError } = useToast()
  const [pageNumber, setPageNumber] = useState(1)
  const [loading, setLoading] = useState(true)
  const [errorText, setErrorText] = useState<string | null>(null)
  const [items, setItems] = useState<BookingHistoryItemDto[]>([])
  const [totalPages, setTotalPages] = useState(1)
  const [totalItems, setTotalItems] = useState(0)

  useEffect(() => {
    if (!isAuthenticated || !customerId) {
      setLoading(false)
      setItems([])
      setErrorText(null)
      return
    }

    let disposed = false
    const resolvedCustomerId = customerId

    async function loadBookingHistory() {
      setLoading(true)
      setErrorText(null)
      try {
        const response = await getBookingHistory(resolvedCustomerId, { pageNumber, pageSize: PAGE_SIZE })
        if (disposed) {
          return
        }
        setItems(response.items)
        setTotalPages(Math.max(1, response.totalPages))
        setTotalItems(response.totalItems)
      } catch (error) {
        if (disposed) {
          return
        }
        if (isAxiosError(error) && error.response?.data) {
          const payload = error.response.data as { detail?: string; title?: string; message?: string }
          const msg = payload.detail ?? payload.title ?? payload.message ?? "Không tải được lịch sử đặt vé."
          setErrorText(msg)
          toastError(msg)
        } else {
          setErrorText("Không tải được lịch sử đặt vé.")
          toastError("Không tải được lịch sử đặt vé.")
        }
      } finally {
        if (!disposed) {
          setLoading(false)
        }
      }
    }

    void loadBookingHistory()
    return () => {
      disposed = true
    }
  }, [customerId, isAuthenticated, pageNumber])

  const pageInfoLabel = useMemo(() => `Trang ${pageNumber}/${Math.max(1, totalPages)}`, [pageNumber, totalPages])

  if (!isAuthenticated) {
    return (
      <main className="mx-auto min-h-[60vh] w-full max-w-screen-xl px-8 pb-12 pt-28">
        <div className="rounded-2xl border border-outline-variant/20 bg-surface-container-low p-8 text-center">
          <h1 className="font-headline text-3xl font-bold">Lịch sử đặt vé</h1>
          <p className="mt-3 text-on-surface-variant">Bạn cần đăng nhập để xem lịch sử đặt vé cá nhân.</p>
          <Link
            to="/"
            className="mt-6 inline-flex items-center justify-center rounded-full bg-gradient-to-r from-primary to-primary-container px-6 py-3 text-sm font-bold text-on-primary"
          >
            Quay lại trang chủ
          </Link>
        </div>
      </main>
    )
  }

  if (!customerId && isResolvingProfile) {
    return (
      <main className="mx-auto min-h-[60vh] w-full max-w-screen-xl px-8 pb-12 pt-28">
        <div className="rounded-2xl border border-outline-variant/20 bg-surface-container-low p-8 text-center">
          <h1 className="font-headline text-3xl font-bold">Lịch sử đặt vé</h1>
          <p className="mt-3 text-on-surface-variant">Đang đồng bộ thông tin tài khoản...</p>
        </div>
      </main>
    )
  }

  if (!customerId) {
    return (
      <main className="mx-auto min-h-[60vh] w-full max-w-screen-xl px-8 pb-12 pt-28">
        <div className="rounded-2xl border border-outline-variant/20 bg-surface-container-low p-8 text-center">
          <h1 className="font-headline text-3xl font-bold">Lịch sử đặt vé</h1>
          <p className="mt-3 text-on-surface-variant">Không tìm thấy hồ sơ khách hàng cho tài khoản hiện tại.</p>
        </div>
      </main>
    )
  }

  return (
    <main className="mx-auto w-full max-w-screen-xl px-8 pb-14 pt-28">
      <section className="mb-8 flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="font-headline text-4xl font-bold tracking-tight">Lịch sử đặt vé</h1>
          <p className="mt-2 text-on-surface-variant">Xin chào {displayName ?? "bạn"}, dưới đây là các giao dịch gần nhất của bạn.</p>
        </div>
        <span className="rounded-full border border-outline-variant/30 px-4 py-2 text-xs uppercase tracking-widest text-on-surface-variant">
          Tổng {totalItems} đơn
        </span>
      </section>

      {errorText && (
        <div className="mb-6 rounded-xl border border-red-400/30 bg-red-500/10 px-4 py-3 text-sm text-red-200">{errorText}</div>
      )}

      {loading ? (
        <div className="flex min-h-[240px] items-center justify-center">
          <span className="inline-block h-10 w-10 animate-spin rounded-full border-2 border-secondary border-t-transparent" aria-hidden />
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-2xl border border-outline-variant/20 bg-surface-container-low p-8 text-center text-on-surface-variant">
          Bạn chưa có đơn đặt vé nào.
        </div>
      ) : (
        <div className="space-y-4">
          {items.map((booking) => {
            const status = normalizeBookingStatus(booking.status)
            return (
              <article
                key={booking.bookingId}
                className="rounded-2xl border border-outline-variant/20 bg-surface-container-low p-5 shadow-[0_8px_30px_rgba(0,0,0,0.2)]"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-xs uppercase tracking-wider text-on-surface-variant">Mã đơn</p>
                    <p className="font-mono text-sm">{booking.bookingId}</p>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${status.className}`}>{status.label}</span>
                </div>
                <div className="mt-4 grid gap-3 text-sm md:grid-cols-2">
                  <div>
                    <p className="text-on-surface-variant">Phim</p>
                    <p className="font-semibold">{booking.showTimeInfo.movie}</p>
                  </div>
                  <div>
                    <p className="text-on-surface-variant">Phòng chiếu</p>
                    <p className="font-semibold">{booking.showTimeInfo.screen}</p>
                  </div>
                  <div>
                    <p className="text-on-surface-variant">Suất chiếu</p>
                    <p className="font-semibold">{formatDateTime(booking.showTimeInfo.startAt)}</p>
                  </div>
                  <div>
                    <p className="text-on-surface-variant">Thành tiền</p>
                    <p className="font-semibold text-secondary">{formatCurrency(booking.finalAmount)}</p>
                  </div>
                </div>
                <p className="mt-4 text-xs text-on-surface-variant">Đặt lúc: {formatDateTime(booking.createdAt)}</p>
              </article>
            )
          })}
        </div>
      )}

      <section className="mt-8 flex items-center justify-end gap-3">
        <button
          type="button"
          onClick={() => setPageNumber((prev) => Math.max(1, prev - 1))}
          disabled={loading || pageNumber <= 1}
          className="rounded-full border border-outline-variant/30 px-4 py-2 text-sm text-on-surface disabled:cursor-not-allowed disabled:opacity-50"
        >
          Trang trước
        </button>
        <span className="text-sm text-on-surface-variant">{pageInfoLabel}</span>
        <button
          type="button"
          onClick={() => setPageNumber((prev) => Math.min(totalPages, prev + 1))}
          disabled={loading || pageNumber >= totalPages}
          className="rounded-full border border-outline-variant/30 px-4 py-2 text-sm text-on-surface disabled:cursor-not-allowed disabled:opacity-50"
        >
          Trang sau
        </button>
      </section>
    </main>
  )
}

export default BookingHistory
