import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/shared/lib/utils'

const buttonVariants = cva(
  // Base styles — 44px min height for touch targets (UX-05-5)
  'inline-flex items-center justify-center whitespace-nowrap rounded-lg text-sm font-medium transition-all duration-150 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 active:scale-[0.97]',
  {
    variants: {
      variant: {
        /** Primary CTA — pink brand color */
        default:
          'bg-primary-500 text-white shadow hover:bg-primary-600',
        /** Secondary — outlined */
        outline:
          'border border-primary-500 text-primary-600 bg-transparent hover:bg-primary-50',
        /** Ghost — no background */
        ghost:
          'text-gray-600 hover:bg-gray-100 hover:text-gray-900',
        /** LINE login button — exact brand color */
        line:
          'bg-[#06C755] text-white shadow hover:bg-[#05b34c]',
        /** Destructive action */
        destructive:
          'bg-rose-500 text-white shadow hover:bg-rose-600',
        /** Link style */
        link:
          'text-primary-600 underline-offset-4 hover:underline',
      },
      size: {
        default: 'h-11 px-6 py-2',   /* 44px — touch target minimum */
        sm:      'h-9 px-4 text-xs',
        lg:      'h-12 px-8 text-base',
        icon:    'h-11 w-11',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  }
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {}

/**
 * shadcn/ui-style Button component with Japanese market variants.
 * Includes LINE login variant and 44px minimum touch targets.
 */
const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, ...props }, ref) => (
    <button
      className={cn(buttonVariants({ variant, size, className }))}
      ref={ref}
      {...props}
    />
  )
)
Button.displayName = 'Button'

export { Button, buttonVariants }
