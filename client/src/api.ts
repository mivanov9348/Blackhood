export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

// Identity error codes → friendly English. Anything unknown falls back to the server text.
const identityErrors: Record<string, string> = {
  DuplicateUserName: 'That name is already taken.',
  DuplicateEmail: 'That e-mail is already registered.',
  InvalidEmail: 'That e-mail is not valid.',
  InvalidUserName: 'Names may contain only Latin letters, digits and - . _ @ +',
  PasswordTooShort: 'The password must be at least 8 characters.',
  PasswordRequiresDigit: 'The password needs at least one digit.',
  PasswordRequiresLower: 'The password needs at least one lowercase letter.',
}

async function readError(res: Response): Promise<string> {
  try {
    const body = await res.json()
    if (Array.isArray(body?.errors)) return body.errors.join('\n')
    if (body?.message) return body.message
    if (body?.errors) {
      return Object.entries(body.errors as Record<string, string[]>)
        .map(([code, msgs]) => identityErrors[code] ?? msgs.join(' '))
        .join('\n')
    }
    if (body?.detail) return body.detail
    if (body?.title) return body.title
  } catch {
    /* no JSON body */
  }
  return res.status === 401 ? 'You are not signed in.' : `Error ${res.status}`
}

/** multipart/form-data upload (the browser sets the boundary header itself). */
export async function upload(path: string, file: File, field = 'file'): Promise<void> {
  const form = new FormData()
  form.append(field, file)
  const res = await fetch(path, { method: 'POST', credentials: 'same-origin', body: form })
  if (!res.ok) throw new ApiError(res.status, await readError(res))
}

export async function api<T>(path: string, options: { method?: string; body?: unknown } = {}): Promise<T> {
  const res = await fetch(path, {
    method: options.method ?? 'GET',
    credentials: 'same-origin',
    headers: options.body !== undefined ? { 'Content-Type': 'application/json' } : undefined,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  })
  if (!res.ok) throw new ApiError(res.status, await readError(res))
  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}
