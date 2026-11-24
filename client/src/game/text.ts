import type { CardType, Phase, RoleType } from './types'

export const cardInfo: Record<CardType, { name: string; glyph: string; base: string }> = {
  Gold: { name: 'Злато', glyph: '◈', base: 'Стойност за сделки, подкупи и оферти на Домакина' },
  Dagger: { name: 'Кама', glyph: '†', base: 'Убива — само в ръката на Убиеца. За другите е улика' },
  Shield: { name: 'Щит', glyph: '⛨', base: 'Блокира едно враждебно действие срещу теб тази нощ' },
  Eye: { name: 'Око', glyph: '◉', base: 'Виждаш ръката на играч' },
  Hand: { name: 'Ръка', glyph: '✋', base: 'Взимаш случайна карта от играч' },
  Seal: { name: 'Печат', glyph: '✠', base: 'Сделката е задължителна — Домакинът наказва нарушителя' },
  Devil: { name: 'Дяволска карта', glyph: '♆', base: 'Играе се веднага: Домакинът ти прави лично предложение' },
}

/** "Една карта — различно действие според ролята". Missing entry = base action. */
export const roleReading: Partial<Record<RoleType, Partial<Record<CardType, string>>>> = {
  Assassin: { Gold: 'Подкупи', Dagger: 'Убива', Shield: 'Крие се от Око', Eye: 'Вижда дали целта има Щит' },
  Inquisitor: { Dagger: 'Доказателство', Eye: 'Вижда ролята, не картите' },
  Thief: { Gold: 'Твоята цел', Dagger: 'Подхвърляш я в чужда ръка', Eye: 'Вижда колко злато има целта', Hand: 'Избираш картата, анонимно' },
  Merchant: { Gold: '+1 при размяна', Dagger: 'Продаваш я', Seal: 'Брои за 2 сделки' },
  Priest: { Gold: 'Откупва душа от Домакина', Dagger: 'Унищожаваш я', Shield: 'Пазиш друг играч', Devil: 'Отказваш без наказание' },
  Sold: { Gold: 'Подкупва другите да приемат оферти', Dagger: 'Даряваш я на Домакина срещу информация', Seal: 'Подпечатва сделка на Домакина', Devil: 'Тайна задача от Домакина' },
  Fool: { Dagger: 'Показваш я нарочно — печеливша карта' },
}

export const roleInfo: Record<RoleType, { name: string; goal: string; skill: string; fears: string }> = {
  Assassin: { name: 'Убиецът', goal: 'Убий двама играчи и остани неразкрит до края.', skill: 'Само ти можеш да убиваш с Кама.', fears: 'Инквизитора' },
  Inquisitor: { name: 'Инквизиторът', goal: 'Обвини правилно Убиеца.', skill: 'С Око виждаш роля, не само карти.', fears: 'Убиеца' },
  Thief: { name: 'Крадецът', goal: 'Завърши с най-много злато и поне 8.', skill: 'Крадеш избрана карта; жертвата не разбира от кого.', fears: 'Търговеца' },
  Merchant: { name: 'Търговецът', goal: '3 изпълнени сделки и поне 6 злато в края.', skill: '+1 злато от всяка размяна.', fears: 'Крадеца' },
  Priest: { name: 'Свещеникът', goal: 'До края да е умрял най-много 1 играч.', skill: 'Пазиш със Щит другите, не само себе си.', fears: 'Убиеца' },
  Sold: { name: 'Продаденият', goal: 'Домакинът да прибере 3 души.', skill: 'Знаеш ролята на един играч от началото.', fears: 'Всички, ако се разбере' },
  Fool: { name: 'Глупакът', goal: 'Масата да те осъди погрешно за Убиец.', skill: 'Камата в ръката ти е печеливша карта.', fears: 'Инквизитора' },
}

export const phaseName: Record<Phase, string> = {
  Draw: 'Теглене',
  Table: 'Масата',
  Night: 'Нощта',
  Resolution: 'Разрешаване',
  Whisper: 'Шепот',
  Court: 'Съдът',
  Finished: 'Краят',
}
