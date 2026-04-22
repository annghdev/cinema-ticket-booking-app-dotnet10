import * as Dialog from "@radix-ui/react-dialog"
import { useState } from "react"

type AuthModalProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
}

function AuthModal({ open, onOpenChange }: AuthModalProps) {
  const [isSignup, setIsSignup] = useState(false)
  const [isForgotPassword, setIsForgotPassword] = useState(false)

  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className="auth-modal-overlay fixed inset-0 z-[90] bg-black/70 backdrop-blur-sm" />
        <Dialog.Content className="auth-modal-content fixed left-1/2 top-1/2 z-[100] w-[95vw] max-w-5xl -translate-x-1/2 -translate-y-1/2 overflow-hidden border border-outline-variant/20 bg-surface-container-low text-on-background shadow-2xl focus:outline-none">
          <Dialog.Close className="absolute right-4 top-4 z-20 text-on-surface-variant transition-colors hover:text-secondary" aria-label="Đóng">
            <span className="material-symbols-outlined">close</span>
          </Dialog.Close>

          <div className="grid grid-cols-1 lg:grid-cols-2">
            <div
              className={`relative hidden flex-col overflow-hidden bg-surface-container-highest p-12 transition-transform duration-500 ease-in-out lg:flex ${
                isSignup ? "lg:translate-x-full" : "lg:translate-x-0"
              }`}
            >
              <img
                src="https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?auto=format&fit=crop&w=1200&q=80"
                alt="Neon cinema ambiance background"
                className="absolute inset-0 h-full w-full object-cover opacity-20"
              />
              <div className="absolute inset-0 bg-gradient-to-br from-[#0b0e14]/85 via-[#10131a]/70 to-[#0b0e14]/90" />
              <div className="absolute -right-24 top-12 h-56 w-56 rounded-full bg-secondary/20 blur-[110px]" />
              <div className="absolute bottom-12 -left-24 h-56 w-56 rounded-full bg-primary/20 blur-[110px]" />

              <div className="relative z-20">
                <h1 className="mb-2 font-headline text-4xl font-black uppercase tracking-widest text-primary drop-shadow-[0_0_8px_rgba(0,244,254,0.5)]">
                  ANDROMEDA CINEMA
                </h1>
                <p className="font-label text-sm uppercase tracking-[0.2em] text-on-surface-variant">Trải nghiệm điện ảnh số</p>
              </div>
              <div className="relative z-20 mt-20">
                <blockquote className="max-w-md">
                  <span className="material-symbols-outlined mb-4 text-4xl text-secondary">format_quote</span>
                  <p className="font-headline text-3xl font-light italic leading-snug text-on-surface">
                    "Điện ảnh là câu chuyện của những gì trong khuôn hình và ngoài khuôn hình."
                  </p>
                  <span className="material-symbols-outlined mb-4 text-4xl text-secondary">format_quote</span>
                  <footer className="mt-4 text-xs uppercase tracking-widest text-outline">Martin Scorsese</footer>
                </blockquote>
              </div>
            </div>

            <div
              className={`relative flex flex-col justify-center overflow-hidden bg-surface-variant/40 p-8 backdrop-blur-xl transition-transform duration-500 ease-in-out md:p-12 lg:p-16 ${
                isSignup ? "lg:-translate-x-full" : "lg:translate-x-0"
              }`}
            >
              <div className="relative min-h-[620px] overflow-hidden">
                <div
                  className={`absolute inset-0 transition-transform duration-500 ease-in-out ${
                    isForgotPassword ? "-translate-x-full" : "translate-x-0"
                  }`}
                >
                  <div className="mb-12">
                    <Dialog.Title className="font-headline text-2xl font-bold text-on-surface">
                      {isSignup ? "Tạo tài khoản" : "Chào mừng trở lại"}
                    </Dialog.Title>
                    <Dialog.Description className="mt-1 text-sm text-on-surface-variant">
                      {isSignup ? "Bắt đầu hành trình điện ảnh của bạn" : "Đăng nhập để tiếp tục trải nghiệm"}
                    </Dialog.Description>
                  </div>

                  <form className="space-y-6">
                    <div className="space-y-4">
                      {isSignup && (
                        <div>
                          <label className="mb-2 block text-[10px] font-bold uppercase tracking-widest text-outline">Họ và tên</label>
                          <input
                            className="w-full border-b-2 border-outline-variant bg-surface-container-low px-0 py-2 text-on-surface placeholder:text-outline/40 transition-all focus:border-secondary focus:outline-none"
                            placeholder="Nguyễn Văn A"
                            type="text"
                          />
                        </div>
                      )}
                      <div>
                        <label className="mb-2 block text-[10px] font-bold uppercase tracking-widest text-outline">Địa chỉ email</label>
                        <input
                          className={`w-full border-b-2 border-outline-variant bg-surface-container-low px-0 text-on-surface placeholder:text-outline/40 transition-all focus:border-secondary focus:outline-none ${isSignup ? "py-2" : "py-3"}`}
                          placeholder="ban@email.com"
                          type="email"
                        />
                      </div>
                      <div>
                        <div className="mb-2 flex items-end justify-between">
                          <label className="text-[10px] font-bold uppercase tracking-widest text-outline">Mật khẩu</label>
                          {!isSignup && (
                            <button
                              type="button"
                              onClick={() => setIsForgotPassword(true)}
                              className="text-[10px] font-bold uppercase tracking-widest text-primary transition-colors hover:text-secondary"
                            >
                              Quên mật khẩu?
                            </button>
                          )}
                        </div>
                        <input
                          className={`w-full border-b-2 border-outline-variant bg-surface-container-low px-0 text-on-surface placeholder:text-outline/40 transition-all focus:border-secondary focus:outline-none ${isSignup ? "py-2" : "py-3"}`}
                          placeholder="••••••••"
                          type="password"
                        />
                      </div>
                    </div>

                    <div className="space-y-4 pt-4">
                      <button
                        type="submit"
                        className={`flex w-full items-center justify-center gap-2 bg-gradient-to-r from-primary to-primary-container font-headline text-sm font-bold uppercase tracking-widest text-on-primary transition-all hover:shadow-[0_0_20px_rgba(97,180,254,0.4)] active:scale-[0.98] ${isSignup ? "py-3" : "py-4"}`}
                      >
                        {isSignup ? "Tạo tài khoản" : "Đăng nhập"}
                        <span className="material-symbols-outlined text-lg">arrow_forward</span>
                      </button>
                      <div className="flex items-center gap-4 py-2">
                        <div className="h-px flex-1 bg-outline-variant/30" />
                        <span className="text-[10px] font-bold uppercase tracking-[0.2em] text-outline">hoặc tiếp tục với</span>
                        <div className="h-px flex-1 bg-outline-variant/30" />
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <button
                          type="button"
                          className="flex items-center justify-center gap-2 border border-outline-variant/30 px-4 py-3 text-[10px] font-bold uppercase tracking-widest transition-colors hover:bg-surface-container-highest"
                        >
                          <svg className="h-4 w-4" viewBox="0 0 24 24" aria-hidden="true">
                            <path
                              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                              fill="#4285F4"
                            />
                            <path
                              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                              fill="#34A853"
                            />
                            <path
                              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                              fill="#FBBC05"
                            />
                            <path
                              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84C6.71 7.31 9.14 5.38 12 5.38z"
                              fill="#EA4335"
                            />
                          </svg>
                          Google
                        </button>
                        <button
                          type="button"
                          className="flex items-center justify-center gap-2 border border-outline-variant/30 px-4 py-3 text-[10px] font-bold uppercase tracking-widest transition-colors hover:bg-surface-container-highest"
                        >
                          <svg className="h-4 w-4 fill-current" viewBox="0 0 24 24" aria-hidden="true">
                            <path d="M13.397 20.997v-8.909h2.994l.448-3.47h-3.442V6.402c0-1.004.279-1.689 1.719-1.689h1.837V1.61A24.787 24.787 0 0 0 14.275 1.5c-2.649 0-4.463 1.617-4.463 4.59v2.528H6.813v3.47h2.999v8.909h3.585z" />
                          </svg>
                          Facebook
                        </button>
                      </div>
                      <p className="pt-2 text-center text-xs text-outline">
                        {isSignup ? "Đã có tài khoản?" : "Chưa có tài khoản?"}{" "}
                        <button
                          type="button"
                          onClick={() => {
                            setIsForgotPassword(false)
                            setIsSignup((prev) => !prev)
                          }}
                          className="font-bold uppercase tracking-wider text-primary transition-colors hover:text-secondary"
                        >
                          {isSignup ? "Đăng nhập" : "Đăng ký"}
                        </button>
                      </p>
                    </div>
                  </form>
                </div>

                <div
                  className={`absolute inset-0 transition-transform duration-500 ease-in-out ${
                    isForgotPassword ? "translate-x-0" : "translate-x-full"
                  }`}
                >
                  <h3 className="font-headline text-2xl font-bold text-on-surface">Quên mật khẩu</h3>
                  <p className="mt-2 text-sm text-on-surface-variant">
                    Nhập email bạn đã đăng ký. Chúng tôi sẽ gửi liên kết để đặt lại mật khẩu.
                  </p>

                  <form className="mt-8 space-y-6">
                    <div>
                      <label className="mb-2 block text-[10px] font-bold uppercase tracking-widest text-outline">Địa chỉ email</label>
                      <input
                        className="w-full border-b-2 border-outline-variant bg-surface-container-low px-0 py-2 text-on-surface placeholder:text-outline/40 transition-all focus:border-secondary focus:outline-none"
                        placeholder="ban@email.com"
                        type="email"
                      />
                    </div>

                    <button
                      type="submit"
                      className="flex w-full items-center justify-center gap-2 bg-gradient-to-r from-primary to-primary-container py-3 font-headline text-sm font-bold uppercase tracking-widest text-on-primary transition-all hover:shadow-[0_0_20px_rgba(97,180,254,0.4)] active:scale-[0.98]"
                    >
                      Gửi liên kết đặt lại
                      <span className="material-symbols-outlined text-lg">mail</span>
                    </button>
                  </form>

                  <button
                    type="button"
                    onClick={() => setIsForgotPassword(false)}
                    className="mt-6 inline-flex items-center gap-2 self-start text-xs font-bold uppercase tracking-widest text-primary transition-colors hover:text-secondary"
                  >
                    <span className="material-symbols-outlined text-base">arrow_back</span>
                    Quay lại đăng nhập
                  </button>
                </div>
              </div>
            </div>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

export default AuthModal
