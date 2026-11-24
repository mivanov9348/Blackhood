import { useCallback, useEffect, useRef, useState } from 'react'

/**
 * Reveals `text` letter by letter. `finish()` jumps to the end (first click on "next"
 * completes the line, the second one advances — like every dialogue game).
 */
export function useTypewriter(text: string, msPerChar = 28) {
  // The count is stored together with the text it belongs to, so a new line never
  // inherits the previous line's progress (which would make it look "done" at once).
  const [progress, setProgress] = useState({ text, count: 0 })
  const count = progress.text === text ? progress.count : 0
  const timer = useRef<number | undefined>(undefined)

  useEffect(() => {
    setProgress({ text, count: 0 })
    if (!text) return
    timer.current = window.setInterval(() => {
      setProgress((p) => {
        if (p.text !== text || p.count >= text.length) {
          window.clearInterval(timer.current)
          return p
        }
        return { text, count: p.count + 1 }
      })
    }, msPerChar)
    return () => window.clearInterval(timer.current)
  }, [text, msPerChar])

  const finish = useCallback(() => {
    window.clearInterval(timer.current)
    setProgress({ text, count: text.length })
  }, [text])

  return { shown: text.slice(0, count), done: count >= text.length, finish }
}
