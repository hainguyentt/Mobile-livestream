import { cn } from '@/shared/lib/utils'

/** Skeleton loading placeholder — use while data is loading. */
function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div className={cn('animate-pulse rounded-md bg-gray-200', className)} {...props} />
  )
}

export { Skeleton }
