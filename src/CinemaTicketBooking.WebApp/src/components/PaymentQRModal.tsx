import * as Dialog from "@radix-ui/react-dialog"
import { useEffect, useState } from "react"
import QRCode from "qrcode"
import { useToast } from "../contexts/ToastContext"

type SwitchGatewayOption = {
  method: string
  displayName: string
  icon?: string | null
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

type PaymentQRModalProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  paymentUrl: string | null
  displayMethod: string
  paymentExpiresAt: string | null
  isNoneOrFake: boolean
  onConfirmNonePayment: () => void
  completingCallback: boolean
  errorText: string | null
  onCancelPayment: () => void
  cancellingPayment: boolean
  switchableGateways: SwitchGatewayOption[]
  selectedSwitchGateway: string | null
  onSelectSwitchGateway: (method: string) => void
  onSwitchGateway: () => void
  switchingGateway: boolean
}

function formatRemainingLabel(remainingSeconds: number) {
  const safe = Math.max(0, remainingSeconds)
  const mm = Math.floor(safe / 60)
  const ss = safe % 60
  return `${String(mm).padStart(2, "0")}:${String(ss).padStart(2, "0")}`
}

export function PaymentQRModal({
  open,
  onOpenChange,
  paymentUrl,
  displayMethod,
  paymentExpiresAt,
  isNoneOrFake,
  onConfirmNonePayment,
  completingCallback,
  errorText,
  onCancelPayment,
  cancellingPayment,
  switchableGateways,
  selectedSwitchGateway,
  onSelectSwitchGateway,
  onSwitchGateway,
  switchingGateway,
}: PaymentQRModalProps) {
  const { error: toastError } = useToast()
  const [qrDataUrl, setQrDataUrl] = useState<string | null>(null)
  const [nowMs, setNowMs] = useState(() => Date.now())
  const [showCloseConfirm, setShowCloseConfirm] = useState(false)

  useEffect(() => {
    if (!open || !paymentUrl) {
      setQrDataUrl(null)
      return
    }
    let cancelled = false
    void QRCode.toDataURL(paymentUrl, { width: 320, margin: 2, errorCorrectionLevel: "M" })
      .then((u) => {
        if (!cancelled) {
          setQrDataUrl(u)
        }
      })
      .catch(() => {
        if (!cancelled) {
          setQrDataUrl(null)
        }
      })
    return () => {
      cancelled = true
    }
  }, [open, paymentUrl])

  useEffect(() => {
    if (!open) {
      return
    }
    const timer = window.setInterval(() => {
      setNowMs(Date.now())
    }, 1000)
    return () => {
      window.clearInterval(timer)
    }
  }, [open])

  useEffect(() => {
    if (!open) {
      setShowCloseConfirm(false)
    }
  }, [open])

  useEffect(() => {
    if (errorText) {
      toastError(errorText)
    }
  }, [errorText, toastError])

  const expiresAtMs = paymentExpiresAt ? new Date(paymentExpiresAt).getTime() : Number.NaN
  const remainingSeconds = Number.isFinite(expiresAtMs) ? Math.floor((expiresAtMs - nowMs) / 1000) : 0
  const isExpired = !Number.isFinite(expiresAtMs) || remainingSeconds <= 0

  return (
    <Dialog.Root
      open={open}
      onOpenChange={(nextOpen) => {
        if (nextOpen) {
          onOpenChange(true)
          return
        }
        setShowCloseConfirm(true)
      }}
    >
      <Dialog.Portal>
        <Dialog.Overlay className="auth-modal-overlay fixed inset-0 z-[100] bg-black/75 backdrop-blur-sm" />
        <Dialog.Content
          onPointerDownOutside={(event) => event.preventDefault()}
          onInteractOutside={(event) => event.preventDefault()}
          onEscapeKeyDown={(event) => event.preventDefault()}
          className="auth-modal-content fixed left-1/2 top-1/2 z-[110] w-[min(100vw-1.5rem,920px)] max-h-[92vh] -translate-x-1/2 -translate-y-1/2 overflow-y-auto border border-outline-variant/20 bg-surface-container-low p-6 text-on-background shadow-2xl focus:outline-none md:p-8"
        >
          <div className="mb-5 flex items-start justify-between gap-3">
            <div>
              <Dialog.Title className="font-headline text-2xl font-bold">Quét mã để thanh toán</Dialog.Title>
              <Dialog.Description className="mt-1 text-sm text-on-surface-variant">
                {displayMethod} · mở liên kết trên thiết bị thanh toán, hoặc dùng mã QR bên dưới.
              </Dialog.Description>
              <div className={`mt-3 inline-flex items-center rounded-full px-4 py-1.5 text-sm font-bold shadow-sm ${isExpired ? "bg-red-500/20 text-red-200" : "bg-secondary/15 text-secondary border border-secondary/20"}`}>
                <span className="material-symbols-outlined mr-1.5 text-[1.1rem]">{isExpired ? "event_busy" : "schedule"}</span>
                {isExpired ? "Đơn thanh toán đã hết hạn" : `Hết hạn sau ${formatRemainingLabel(remainingSeconds)}`}
              </div>
            </div>
            <button
              type="button"
              className="rounded p-1 text-on-surface-variant transition-colors hover:bg-surface-container-highest hover:text-secondary"
              aria-label="Đóng"
              onClick={() => setShowCloseConfirm(true)}
            >
              <span className="material-symbols-outlined">close</span>
            </button>
          </div>

          {errorText && (
            <div className="mb-4 rounded border border-red-400/30 bg-red-500/10 px-3 py-2 text-sm text-red-200">{errorText}</div>
          )}

          <div className="grid grid-cols-1 gap-6 md:grid-cols-[1.2fr_1fr]">
            <section className="space-y-4">
              {qrDataUrl && (
                <div className="flex flex-col items-center rounded-xl border border-outline-variant/25 bg-surface-container-highest p-4">
                  <div className="rounded-2xl border border-outline-variant/30 bg-white p-3">
                    <img src={qrDataUrl} width={300} height={300} className="h-[300px] w-[300px] object-contain" alt="QR thanh toán" />
                  </div>
                </div>
              )}

              {paymentUrl && (
                <a
                  href={paymentUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="block break-all rounded-lg border border-outline-variant/20 bg-surface-container-highest px-3 py-2 text-sm text-secondary hover:underline"
                >
                  {paymentUrl}
                </a>
              )}

              {!isNoneOrFake && (
                <p className="text-xs text-on-surface-variant">Hoàn tất thanh toán trên trang cổng, rồi quay lại ứng dụng.</p>
              )}
            </section>

            <section className="space-y-3 rounded-xl border border-outline-variant/20 bg-surface-container-highest p-4">
              {isNoneOrFake && (
                <div className="space-y-2">
                  <p className="text-xs text-on-surface-variant">
                    Cổng <strong>None</strong> (môi trường dev): sau khi mô phỏng, bấm xác nhận để hệ thống gọi callback thanh toán.
                  </p>
                  <button
                    type="button"
                    disabled={completingCallback || isExpired}
                    onClick={onConfirmNonePayment}
                    className="flex w-full items-center justify-center gap-2 rounded-lg bg-gradient-to-r from-primary to-primary-container py-3 font-headline text-sm font-bold text-on-primary transition-opacity hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {completingCallback ? (
                      <>
                        <span
                          className="inline-block h-4 w-4 shrink-0 animate-spin rounded-full border-2 border-on-primary border-t-transparent"
                          aria-hidden
                        />
                        Đang xác nhận...
                      </>
                    ) : (
                      <>
                        <span className="material-symbols-outlined text-[1.1rem]">verified</span>
                        Xác nhận thanh toán
                      </>
                    )}
                  </button>
                </div>
              )}

              {switchableGateways.length > 0 && (
                <div className="space-y-2 border-t border-outline-variant/20 pt-3">
                  <p className="text-xs text-on-surface-variant">Đổi phương thức thanh toán cho cùng đơn đặt vé</p>
                  <div className="grid grid-cols-1 gap-2">
                    {switchableGateways.map((gateway) => {
                      const iconUrl = resolvePaymentIcon(gateway.icon)
                      return (
                        <button
                          key={gateway.method}
                          type="button"
                          onClick={() => onSelectSwitchGateway(gateway.method)}
                          className={`flex items-center gap-3 rounded-lg border px-3 py-2.5 text-left text-sm transition-all ${
                            selectedSwitchGateway === gateway.method
                              ? "border-secondary bg-secondary/10 text-secondary shadow-[0_0_10px_rgba(0,244,254,0.1)]"
                              : "border-outline-variant/25 bg-surface-container-low text-on-surface-variant hover:border-outline-variant/50 hover:bg-surface-container-low/80"
                          }`}
                        >
                          <div className="flex h-7 w-7 shrink-0 items-center justify-center overflow-hidden rounded bg-white p-0.5">
                            {iconUrl ? (
                              <img src={iconUrl} alt="" className="h-full w-full object-contain" />
                            ) : (
                              <span className="material-symbols-outlined text-sm">account_balance</span>
                            )}
                          </div>
                          <span className="font-medium">{gateway.displayName}</span>
                        </button>
                      )
                    })}
                  </div>
                  <button
                    type="button"
                    disabled={switchingGateway || !selectedSwitchGateway}
                    onClick={onSwitchGateway}
                    className="flex w-full items-center justify-center gap-2 rounded-lg border border-secondary/40 bg-secondary/10 py-2.5 text-sm font-semibold text-secondary transition-colors hover:bg-secondary/20 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {switchingGateway ? "Đang chuyển cổng..." : "Đổi gateway"}
                  </button>
                </div>
              )}

              <div className="space-y-2 border-t border-outline-variant/20 pt-3">
                <button
                  type="button"
                  disabled={cancellingPayment}
                  onClick={onCancelPayment}
                  className="flex w-full items-center justify-center gap-2 rounded-lg border border-red-400/30 bg-red-500/10 py-2.5 text-sm font-semibold text-red-200 transition-colors hover:bg-red-500/20 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {cancellingPayment ? "Đang hủy thanh toán..." : "Hủy thanh toán"}
                </button>
              </div>
            </section>
          </div>

          {showCloseConfirm && (
            <div className="absolute inset-0 z-[120] grid place-items-center bg-black/70 p-4">
              <div className="w-full max-w-md rounded-xl border border-outline-variant/25 bg-surface-container-highest p-5">
                <p className="font-headline text-lg font-semibold">Đóng màn hình QR?</p>
                <p className="mt-2 text-sm text-on-surface-variant">
                  Nếu đóng, bạn có thể mất phiên theo dõi realtime. Bạn muốn hủy thanh toán hay tiếp tục ở lại?
                </p>
                <div className="mt-4 grid grid-cols-1 gap-2 sm:grid-cols-2">
                  <button
                    type="button"
                    disabled={cancellingPayment}
                    onClick={onCancelPayment}
                    className="rounded-lg border border-red-400/30 bg-red-500/10 px-3 py-2 text-sm font-semibold text-red-200 transition-colors hover:bg-red-500/20 disabled:cursor-not-allowed disabled:opacity-50"
                  >
                    {cancellingPayment ? "Đang hủy..." : "Hủy thanh toán"}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowCloseConfirm(false)}
                    className="rounded-lg border border-outline-variant/30 bg-surface-container-low px-3 py-2 text-sm font-semibold text-on-surface transition-colors hover:bg-surface-container"
                  >
                    Ở lại
                  </button>
                </div>
              </div>
            </div>
          )}
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
