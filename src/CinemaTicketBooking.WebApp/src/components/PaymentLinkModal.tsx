import * as Dialog from "@radix-ui/react-dialog"
import { useEffect, useState } from "react"
import QRCode from "qrcode"

type PaymentLinkModalProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  /** URL to the payment page (or fake URL). */
  paymentUrl: string | null
  displayMethod: string
  isNoneOrFake: boolean
  onConfirmNonePayment: () => void
  completingCallback: boolean
  errorText: string | null
}

/**
 * Shown when create-booking returns a non-redirect flow: QR to open/gateway, optional fake callback for None.
 */
export function PaymentLinkModal({
  open,
  onOpenChange,
  paymentUrl,
  displayMethod,
  isNoneOrFake,
  onConfirmNonePayment,
  completingCallback,
  errorText,
}: PaymentLinkModalProps) {
  const [qrDataUrl, setQrDataUrl] = useState<string | null>(null)

  useEffect(() => {
    if (!open || !paymentUrl) {
      setQrDataUrl(null)
      return
    }
    let cancelled = false
    void QRCode.toDataURL(paymentUrl, { width: 280, margin: 2, errorCorrectionLevel: "M" })
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

  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className="auth-modal-overlay fixed inset-0 z-[100] bg-black/75 backdrop-blur-sm" />
        <Dialog.Content className="auth-modal-content fixed left-1/2 top-1/2 z-[110] w-[min(100vw-1.5rem,420px)] max-h-[90vh] -translate-x-1/2 -translate-y-1/2 overflow-y-auto border border-outline-variant/20 bg-surface-container-low p-6 text-on-background shadow-2xl focus:outline-none md:p-8">
          <div className="mb-4 flex items-start justify-between gap-2">
            <div>
              <Dialog.Title className="font-headline text-xl font-bold">Quét mã để thanh toán</Dialog.Title>
              <Dialog.Description className="mt-1 text-sm text-on-surface-variant">
                {displayMethod} · mở liên kết trên thiết bị thanh toán, hoặc dùng mã QR bên dưới.
              </Dialog.Description>
            </div>
            <Dialog.Close
              className="shrink-0 rounded p-1 text-on-surface-variant transition-colors hover:bg-surface-container-highest hover:text-secondary"
              aria-label="Đóng"
            >
              <span className="material-symbols-outlined">close</span>
            </Dialog.Close>
          </div>

          {errorText && (
            <div className="mb-4 rounded border border-red-400/30 bg-red-500/10 px-3 py-2 text-sm text-red-200">{errorText}</div>
          )}

          {qrDataUrl && (
            <div className="mb-4 flex flex-col items-center">
              <div className="rounded-2xl border border-outline-variant/30 bg-white p-3">
                <img src={qrDataUrl} width={260} height={260} className="h-[260px] w-[260px] object-contain" alt="QR thanh toán" />
              </div>
            </div>
          )}

          {paymentUrl && (
            <a
              href={paymentUrl}
              target="_blank"
              rel="noreferrer"
              className="mb-4 block break-all rounded-lg border border-outline-variant/20 bg-surface-container-highest px-3 py-2 text-sm text-secondary hover:underline"
            >
              {paymentUrl}
            </a>
          )}

          {isNoneOrFake && (
            <div className="space-y-2 border-t border-outline-variant/20 pt-4">
              <p className="text-xs text-on-surface-variant">
                Cổng <strong>None</strong> (môi trường dev): sau khi mô phỏng, bấm xác nhận để hệ thống gọi callback thanh toán.
              </p>
              <button
                type="button"
                disabled={completingCallback}
                onClick={onConfirmNonePayment}
                className="flex w-full items-center justify-center gap-2 rounded-lg bg-gradient-to-r from-primary to-primary-container py-3 font-headline text-sm font-bold text-on-primary transition-opacity hover:opacity-95 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {completingCallback ? (
                  <>
                    <span
                      className="inline-block h-4 w-4 shrink-0 animate-spin rounded-full border-2 border-on-primary border-t-transparent"
                      aria-hidden
                    />
                    Đang xác nhận…
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

          {!isNoneOrFake && (
            <p className="border-t border-outline-variant/20 pt-3 text-center text-xs text-on-surface-variant">Hoàn tất thanh toán trên trang cổng, rồi quay lại ứng dụng.</p>
          )}
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}
