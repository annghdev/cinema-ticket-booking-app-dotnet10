import html2canvas from "html2canvas"
import { jsPDF } from "jspdf"
import QRCode from "qrcode"

export type ConcessionLine = {
  name: string
  quantity: number
  amount: number
}

export type CheckinTicketPdfInfo = {
  bookingId: string
  movie: string
  screen: string
  cinema: string
  startAt: string
  seats: string
  finalAmount?: number
  concessions?: ConcessionLine[]
}

const moneyVi = (n: number) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 }).format(n)

/**
 * Renders the ticket as HTML, rasterizes with html2canvas (keeps UTF-8 / Vietnamese + webfonts),
 * then places the image in A5 PDF. Replaces helvetica + doc.text (broken for VN in jsPDF).
 */
export async function downloadCheckinPassPdf(info: CheckinTicketPdfInfo) {
  const qrDataUrl = await QRCode.toDataURL(info.bookingId, { width: 220, margin: 2, errorCorrectionLevel: "M" })

  const root = document.createElement("div")
  Object.assign(root.style, {
    position: "fixed",
    left: "-12000px",
    top: "0",
    width: "400px",
    boxSizing: "border-box",
    padding: "28px 24px 24px",
    background: "#ffffff",
    color: "#0b0e14",
    fontFamily: "Manrope, 'Segoe UI', 'Helvetica Neue', Arial, 'Noto Sans', sans-serif",
    fontSize: "12px",
    lineHeight: "1.5",
  })

  const h1 = document.createElement("h1")
  h1.textContent = "Vé điện tử - Check-in"
  Object.assign(h1.style, { fontSize: "18px", fontWeight: "700", margin: "0 0 16px", lineHeight: "1.25" })
  root.appendChild(h1)

  const p = (text: string, extra: Partial<CSSStyleDeclaration> = {}) => {
    const el = document.createElement("p")
    el.textContent = text
    Object.assign(el.style, { margin: "0 0 8px" }, extra)
    root.appendChild(el)
  }

  p(`Mã đặt: ${info.bookingId}`, { fontFamily: "ui-monospace, 'Cascadia Code', monospace", fontSize: "11px" })
  p(`Phim: ${info.movie}`)
  p(`Rạp: ${info.cinema}`)
  p(`Phòng: ${info.screen} · ${info.startAt}`)
  p(`Ghế: ${info.seats}`)

  if (info.concessions && info.concessions.length > 0) {
    const sub = document.createElement("h2")
    sub.textContent = "Đồ ăn & kèm theo"
    Object.assign(sub.style, { fontSize: "13px", fontWeight: "600", margin: "12px 0 6px" })
    root.appendChild(sub)
    for (const c of info.concessions) {
      p(`  • ${c.name} ×${c.quantity} — ${moneyVi(c.amount)}`, { fontSize: "11px" })
    }
  }

  if (info.finalAmount != null) {
    p(`Tổng: ${moneyVi(info.finalAmount)}`, { fontWeight: "600", marginTop: "10px" })
  }

  const qrWrap = document.createElement("div")
  Object.assign(qrWrap.style, {
    display: "flex",
    justifyContent: "center",
    margin: "20px 0 12px",
  })
  const img = document.createElement("img")
  img.src = qrDataUrl
  img.alt = "QR"
  Object.assign(img.style, { width: "150px", height: "150px", display: "block" })
  qrWrap.appendChild(img)
  root.appendChild(qrWrap)

  const foot = document.createElement("p")
  foot.textContent = "Trình mã QR tại quầy soát vé."
  Object.assign(foot.style, { fontSize: "10px", color: "#555", textAlign: "center", width: "100%", margin: "0" })
  root.appendChild(foot)

  document.body.appendChild(root)
  try {
    await document.fonts.ready
    const canvas = await html2canvas(root, {
      scale: 2,
      backgroundColor: "#ffffff",
      useCORS: true,
    })
    const dataUrl = canvas.toDataURL("image/png", 1.0)
    const doc = new jsPDF({ unit: "mm", format: "a5" })
    const pageW = doc.internal.pageSize.getWidth()
    const pageH = doc.internal.pageSize.getHeight()
    const margin = 6
    const maxW = pageW - 2 * margin
    const maxH = pageH - 2 * margin
    const props = doc.getImageProperties(dataUrl)
    const ratio = props.width / props.height
    let w = maxW
    let h = w / ratio
    if (h > maxH) {
      h = maxH
      w = h * ratio
    }
    const x = (pageW - w) / 2
    const y = (pageH - h) / 2
    doc.addImage(dataUrl, "PNG", x, y, w, h)
    doc.save(`checkin-${info.bookingId}.pdf`)
  } finally {
    document.body.removeChild(root)
  }
}
