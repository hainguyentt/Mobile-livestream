import { z } from 'zod'

export const loginAdminSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
})

export type LoginAdminFormValues = z.infer<typeof loginAdminSchema>
