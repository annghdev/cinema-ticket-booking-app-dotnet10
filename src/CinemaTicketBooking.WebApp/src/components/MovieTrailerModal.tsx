import { useEffect, useState } from "react"

interface MovieTrailerModalProps {
  isOpen: boolean
  onClose: () => void
  trailerUrl: string | null
  movieName: string
}

export function MovieTrailerModal({ isOpen, onClose, trailerUrl, movieName }: MovieTrailerModalProps) {
  const [embedUrl, setEmbedUrl] = useState<string | null>(null)

  useEffect(() => {
    if (trailerUrl) {
      // Handle YouTube URLs (standard and shortened)
      let videoId = ""
      if (trailerUrl.includes("youtube.com/watch?v=")) {
        videoId = trailerUrl.split("v=")[1]?.split("&")[0]
      } else if (trailerUrl.includes("youtu.be/")) {
        videoId = trailerUrl.split("youtu.be/")[1]?.split("?")[0]
      } else if (trailerUrl.includes("youtube.com/embed/")) {
        videoId = trailerUrl.split("embed/")[1]?.split("?")[0]
      }

      if (videoId) {
        setEmbedUrl(`https://www.youtube.com/embed/${videoId}?autoplay=1`)
      } else {
        setEmbedUrl(trailerUrl) // Fallback
      }
    } else {
      setEmbedUrl(null)
    }
  }, [trailerUrl])

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/90 p-4 backdrop-blur-sm transition-opacity duration-300">
      <div className="relative w-full max-w-5xl overflow-hidden rounded-2xl border border-outline-variant/20 bg-surface-container shadow-2xl">
        <div className="flex items-center justify-between border-b border-outline-variant/10 px-6 py-4">
          <h3 className="font-headline text-xl font-bold text-on-surface truncate pr-8">
            Trailer: {movieName}
          </h3>
          <button
            type="button"
            onClick={onClose}
            className="flex h-10 w-10 items-center justify-center rounded-full bg-surface-variant/40 text-on-surface-variant transition-colors hover:bg-surface-variant hover:text-on-surface"
          >
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>
        <div className="relative aspect-video w-full bg-black">
          {embedUrl ? (
            <iframe
              src={embedUrl}
              title={`Trailer: ${movieName}`}
              className="absolute inset-0 h-full w-full border-0"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
            />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-on-surface-variant">
              Không tìm thấy video trailer hợp lệ.
            </div>
          )}
        </div>
      </div>
      <div className="absolute inset-0 -z-10" onClick={onClose}></div>
    </div>
  )
}
