import { useMemo, type CSSProperties } from 'react'

/** Slowly rising sparks — purely decorative. */
export function Embers({ count = 24 }: { count?: number }) {
  const embers = useMemo(
    () =>
      Array.from({ length: count }, (_, i) => ({
        id: i,
        left: Math.random() * 100,
        delay: Math.random() * 12,
        duration: 9 + Math.random() * 10,
        size: 2 + Math.random() * 3,
        drift: (Math.random() - 0.5) * 120,
      })),
    [count],
  )

  return (
    <div className="embers" aria-hidden="true">
      {embers.map((e) => (
        <span
          key={e.id}
          className="ember"
          style={
            {
              left: `${e.left}%`,
              width: e.size,
              height: e.size,
              animationDelay: `${e.delay}s`,
              animationDuration: `${e.duration}s`,
              '--drift': `${e.drift}px`,
            } as CSSProperties
          }
        />
      ))}
    </div>
  )
}
