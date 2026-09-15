import { InjectionToken } from '@angular/core';
import type * as PdfJs from 'pdfjs-dist';

export type PdfJsLoader = () => Promise<typeof PdfJs>;

/** Browser loader kept behind DI so preview tests never execute PDF.js browser globals. */
export const PDFJS_LOADER = new InjectionToken<PdfJsLoader>('PDF.js loader', {
  providedIn: 'root',
  factory: () => () => import('pdfjs-dist'),
});
