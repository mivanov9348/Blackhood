import { useState } from 'react'

interface Props {
  hue: number
  host?: boolean
  /** Character code — when set and `hasImage` is true, the uploaded portrait is shown. */
  code?: string
  hasImage?: boolean
  /** Bust the browser cache after an upload in the editor. */
  version?: number
  className?: string
}

/** Uploaded portrait if there is one, otherwise a hooded silhouette tinted by the character's hue. */
export function Portrait({ hue, host = false, code, hasImage = false, version, className = '' }: Props) {
  const [broken, setBroken] = useState(false)
  const cls = `${host ? 'portrait portrait--host' : 'portrait'} ${className}`.trim()

  if (code && hasImage && !broken)
    return (
      <img
        className={`${cls} portrait--image`}
        src={`/api/characters/${code}/portrait${version ? `?v=${version}` : ''}`}
        alt=""
        onError={() => setBroken(true)}
      />
    )

  const glow = host ? '#ff5a2a' : `hsl(${hue} 60% 55%)`
  const gid = `bg-${hue}-${host ? 'h' : 'p'}`
  return (
    <svg className={cls} viewBox="0 0 100 120" aria-hidden="true">
      <defs>
        <radialGradient id={gid} cx="50%" cy="40%" r="60%">
          <stop offset="0%" stopColor={`hsl(${hue} 35% 22%)`} />
          <stop offset="100%" stopColor="#0a0604" />
        </radialGradient>
      </defs>
      <rect width="100" height="120" rx="10" fill={`url(#${gid})`} />
      <path d="M8 120 C14 88 30 80 50 80 C70 80 86 88 92 120 Z" fill="#16100c" />
      <path d="M50 14 C30 14 22 34 22 54 C22 72 30 86 50 90 C70 86 78 72 78 54 C78 34 70 14 50 14 Z" fill="#1f1610" stroke={`hsl(${hue} 30% 30%)`} strokeWidth="1.2" />
      <ellipse cx="50" cy="56" rx="17" ry="22" fill="#030201" />
      <circle className="portrait__eye" cx="43" cy="54" r="1.6" fill={glow} opacity={host ? 1 : 0.65} />
      <circle className="portrait__eye" cx="57" cy="54" r="1.6" fill={glow} opacity={host ? 1 : 0.65} />
    </svg>
  )
}
