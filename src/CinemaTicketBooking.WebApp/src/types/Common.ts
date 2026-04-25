export type PagedResult<T> = {
  items: T[]
  pageNumber: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export function normalizeEnumValue<T extends string>(value: T | number, numberMap: Record<number, T>, fallback: T): T {
  if (typeof value === "string") {
    return value
  }

  return numberMap[value] ?? fallback
}
