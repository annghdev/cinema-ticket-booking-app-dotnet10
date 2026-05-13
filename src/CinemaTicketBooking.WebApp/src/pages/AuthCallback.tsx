import { useEffect, useRef } from "react"
import { useNavigate, useSearchParams } from "react-router-dom"
import { useAuth } from "../contexts/AuthContext"
import { useToast } from "../contexts/ToastContext"

/**
 * Handles the OAuth2 callback from the backend.
 * Extracts tokens from query parameters and initializes the user session.
 */
export default function AuthCallback() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { setAuthFromTokens } = useAuth()
  const { success, error } = useToast()
  const processedRef = useRef(false)

  useEffect(() => {
    if (processedRef.current) return
    processedRef.current = true

    const accessToken = searchParams.get("accessToken")
    const accountId = searchParams.get("accountId")
    const expiresAt = searchParams.get("expiresAt")
    const returnUrl = searchParams.get("returnUrl") || "/"
    const errorCode = searchParams.get("error")

    if (errorCode) {
      error(getErrorMessage(errorCode))
      navigate("/", { replace: true })
      return
    }

    if (accessToken && accountId && expiresAt) {
      void setAuthFromTokens({
        accessToken,
        accountId,
        accessTokenExpiresAtUtc: new Date(parseInt(expiresAt) * 1000).toISOString(),
        refreshToken: null, // Refresh token is handled via HttpOnly cookie
      }).then(() => {
        success("Đăng nhập thành công!")
        navigate(returnUrl, { replace: true })
      })
    } else {
      error("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.")
      navigate("/", { replace: true })
    }
  }, [searchParams, setAuthFromTokens, navigate, success, error])

  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center space-y-6">
      <div className="relative">
        <div className="h-16 w-16 animate-spin rounded-full border-4 border-primary/20 border-t-primary" />
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="material-symbols-outlined animate-pulse text-primary">key</span>
        </div>
      </div>
      <div className="text-center">
        <p className="font-headline text-xl font-black uppercase tracking-[0.2em] text-primary">
          Đang xác thực
        </p>
        <p className="mt-2 font-body text-sm text-on-surface-variant">
          Vui lòng đợi trong giây lát...
        </p>
      </div>
    </div>
  )
}

function getErrorMessage(code: string): string {
  switch (code) {
    case "external_login_failed":
      return "Đăng nhập bên thứ ba thất bại."
    case "account_not_found":
      return "Không tìm thấy tài khoản liên kết."
    case "email_not_provided":
      return "Provider không cung cấp địa chỉ email."
    case "account_creation_failed":
      return "Không thể tạo tài khoản mới."
    default:
      return "Có lỗi xảy ra trong quá trình đăng nhập."
  }
}
