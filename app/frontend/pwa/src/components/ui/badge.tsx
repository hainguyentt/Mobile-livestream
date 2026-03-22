import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const badgeVariants = cva(
  'inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors',
  {
    variants: {
      variant: {
        default:     'border-transparent bg-primary-500 text-white',
        secondary:   'border-transparent bg-gray-100 text-gray-700',
        destructive: 'border-transparent bg-rose-500 text-white',
        outline:     'border-primary-300 text-primary-700',
        success:     'border-transparent bg-green-100 text-green-700',
        live:        'border-transparent bg-red-500 text-white animate-pulse-live',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  }
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

/** Status badge — includes 'live' variant with pulse animation */
function Badge({ className, variant, ...props }: BadgeProps) {
  return (
    <div className={cn(badgeVariants({ variant }), className)} {...props} />
  )
}

export { Badge, badgeVariants }
