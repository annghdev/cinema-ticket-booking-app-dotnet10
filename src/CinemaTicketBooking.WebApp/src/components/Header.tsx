import { useEffect, useMemo, useRef, useState } from "react"
import { NavLink, useNavigate } from "react-router-dom"
import { logout } from "../apis/authApi"
import { useAuth } from "../contexts/AuthContext"
import AuthModal from "./AuthModal"

const navItems = [
  { label: "Trang chủ", to: "/" },
  { label: "Phim", to: "/movies" },
  { label: "Lịch chiếu", to: "/showtimes" },
  { label: "Ưu đãi", to: "/promos" },
  { label: "Hội viên", to: "/member" },
]

function Header() {
  const navigate = useNavigate()
  const { authState, displayName, avatarUrl, clearAuth } = useAuth()
  const [isAuthModalOpen, setIsAuthModalOpen] = useState(false)
  const [isAccountMenuOpen, setIsAccountMenuOpen] = useState(false)
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const accountMenuRef = useRef<HTMLDivElement | null>(null)

  const avatarFallback = useMemo(() => {
    const first = (displayName ?? "").trim().charAt(0)
    return first ? first.toUpperCase() : "U"
  }, [displayName])

  useEffect(() => {
    const onPointerDown = (event: PointerEvent) => {
      if (!accountMenuRef.current?.contains(event.target as Node)) {
        setIsAccountMenuOpen(false)
      }
    }

    window.addEventListener("pointerdown", onPointerDown)
    return () => {
      window.removeEventListener("pointerdown", onPointerDown)
    }
  }, [])

  useEffect(() => {
    if (!authState) {
      setIsAccountMenuOpen(false)
    }
  }, [authState])

  const navigateFromMenu = (path: string) => {
    setIsAccountMenuOpen(false)
    navigate(path)
  }

  const onLogout = async () => {
    if (isLoggingOut) {
      return
    }
    setIsLoggingOut(true)
    try {
      await logout()
    } catch {
      // Keep local logout flow regardless of API response.
    } finally {
      clearAuth()
      setIsAccountMenuOpen(false)
      setIsLoggingOut(false)
      navigate("/")
    }
  }

  return (
    <>
      <header className="fixed top-0 z-50 w-full border-b border-outline-variant/20 bg-background/95 shadow-lg shadow-black/20 backdrop-blur-md">
        <div className="mx-auto flex w-full max-w-screen-2xl items-center justify-between px-8 py-4">
          <div className="font-headline text-2xl font-black tracking-tighter text-[#61b4fe]">
            ABSOLUTE CINEMA
          </div>

          <nav className="hidden items-center space-x-8 font-headline font-bold tracking-tight md:flex">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  isActive
                    ? "active:scale-95 border-b-2 border-[#00f4fe] pb-1 text-[#00f4fe] drop-shadow-[0_0_8px_rgba(0,244,254,0.6)] transition-all duration-300 ease-out"
                    : "px-2 py-1 text-slate-400 transition-all hover:text-[#00f4fe] hover:drop-shadow-[0_0_5px_rgba(0,244,254,0.4)] active:scale-95"
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          <div className="flex items-center gap-4">
            <button
              type="button"
              aria-label="Tìm kiếm"
              className="material-symbols-outlined text-slate-400 transition-all hover:text-[#00f4fe]"
            >
              search
            </button>
            {authState ? (
              <div className="relative" ref={accountMenuRef}>
                <button
                  type="button"
                  aria-label="Mở menu tài khoản"
                  onClick={() => setIsAccountMenuOpen((prev) => !prev)}
                  className="flex items-center gap-2 rounded-full border border-outline-variant/30 bg-surface-container-highest px-2 py-1 transition-colors hover:border-secondary/50"
                >
                  {avatarUrl ? (
                    <img
                      src={avatarUrl}
                      alt={displayName ?? "Avatar"}
                      className="h-8 w-8 rounded-full object-cover"
                    />
                  ) : (
                    <span className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-secondary/20 font-semibold text-secondary">
                      {avatarFallback}
                    </span>
                  )}
                  <span className="hidden max-w-[160px] truncate text-sm text-on-surface md:block">{displayName}</span>
                  <span className="material-symbols-outlined text-lg text-on-surface-variant">keyboard_arrow_down</span>
                </button>
                {isAccountMenuOpen && (
                  <div className="absolute right-0 top-12 z-50 w-56 overflow-hidden rounded-lg border border-outline-variant/20 bg-surface-container-highest shadow-xl">
                    <button
                      type="button"
                      onClick={() => navigateFromMenu("/booking-history")}
                      className="flex w-full items-center gap-2 px-4 py-3 text-left text-sm text-on-surface transition-colors hover:bg-surface-container-high"
                    >
                      <span className="material-symbols-outlined text-base">confirmation_number</span>
                      Xem lịch sử đặt vé
                    </button>
                    <button
                      type="button"
                      onClick={() => navigateFromMenu("/member?section=profile")}
                      className="flex w-full items-center gap-2 px-4 py-3 text-left text-sm text-on-surface transition-colors hover:bg-surface-container-high"
                    >
                      <span className="material-symbols-outlined text-base">badge</span>
                      Thông tin cá nhân
                    </button>
                    <button
                      type="button"
                      onClick={() => void onLogout()}
                      disabled={isLoggingOut}
                      className="flex w-full items-center gap-2 border-t border-outline-variant/20 px-4 py-3 text-left text-sm text-red-300 transition-colors hover:bg-red-500/10 disabled:opacity-60"
                    >
                      <span className="material-symbols-outlined text-base">logout</span>
                      {isLoggingOut ? "Đang đăng xuất..." : "Đăng xuất"}
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <button
                type="button"
                aria-label="Mở đăng nhập"
                onClick={() => setIsAuthModalOpen(true)}
                className="material-symbols-outlined text-[#61b4fe] transition-all hover:text-[#00f4fe]"
              >
                account_circle
              </button>
            )}
          </div>
        </div>
      </header>

      <AuthModal open={isAuthModalOpen} onOpenChange={setIsAuthModalOpen} />
    </>
  )
}

export default Header
