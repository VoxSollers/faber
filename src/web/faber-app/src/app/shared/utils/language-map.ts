export interface LanguageOption {
  /** BCP-47 locale, e.g. `en-us`. */
  readonly locale: string;
  /** Display name of the language. */
  readonly name: string;
  /** ISO country code of the flag asset under `public/flags/{country}.svg`. */
  readonly country: string;
}

export const LANGUAGES: readonly LanguageOption[] = [
  { locale: 'en-us', name: 'English', country: 'gb' },
  { locale: 'pl-pl', name: 'Polish', country: 'pl' },
];

export const LANGUAGE_MAP = new Map<string, LanguageOption>(
  LANGUAGES.map(language => [language.locale, language]),
);
