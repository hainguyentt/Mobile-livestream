import { cn } from '@/lib/utils'

/**
 * Skeleton loading placeholder.
 * Use to reflect the layout of content while data is loading.
 */
function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('animate-pulse rounded-md bg-gray-200', className)}
      {...props}
    />
  )
}

export { Skeleton }
