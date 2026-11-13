import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import en from './locales/en.json'
import vi from './locales/vi.json'

export const LANGUAGES = [
  { code: 'vi', label: 'Tiếng Việt', flag: '🇻🇳' },
  { code: 'en', label: 'English', flag: '🇬🇧' },
] as const
export type LanguageCode = (typeof LANGUAGES)[number]['code']

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: { vi: { translation: vi }, en: { translation: en } },
    fallbackLng: 'vi',
    supportedLngs: ['vi', 'en'],
    nonExplicitSupportedLngs: true, // en-US → en
    interpolation: { escapeValue: false }, // React đã escape
    detection: { order: ['localStorage', 'navigator'], lookupLocalStorage: 'teamhub-lang', caches: ['localStorage'] },
  })

const syncHtmlLang = (lng: string): void => {
  document.documentElement.lang = lng.startsWith('en') ? 'en' : 'vi'
}
syncHtmlLang(i18n.language)
i18n.on('languageChanged', syncHtmlLang)

export default i18n
