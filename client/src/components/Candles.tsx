/** The match timer: six candles, one burns out per round. */
export function Candles({ total, round }: { total: number; round: number }) {
  return (
    <div className="candles" aria-label={`Свещ ${round} от ${total}`}>
      {Array.from({ length: total }, (_, i) => {
        const state = i < round - 1 ? 'out' : i === round - 1 ? 'burning' : 'lit'
        return (
          <div key={i} className={`candle candle--${state}`}>
            {state !== 'out' && <span className="candle__flame" style={{ animationDelay: `${i * 0.37}s` }} />}
            <span className="candle__wax" />
          </div>
        )
      })}
    </div>
  )
}
