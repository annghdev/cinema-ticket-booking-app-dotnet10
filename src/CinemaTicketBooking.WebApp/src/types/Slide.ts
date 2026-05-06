export interface Slide {
  id: string
  title: string
  description: string
  imageUrl: string
  targetUrl: string
  displayOrder: number
  type: "ShowingMovie" | "UpcomingMovie" | "Event"
  videoUrl: string | null
}
